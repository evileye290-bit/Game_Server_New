using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZGate;
using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public MSG_ZR_CHAT_LIST ChatList = new MSG_ZR_CHAT_LIST();

        private DateTime LastSendChatTime = ZoneServerApi.now;
        
        public void OnResponse_ChatList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHAT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHAT_LIST>(stream);
            Dictionary<int, PlayerChar> pcList = Api.PCManager.PcList;
            foreach (var item in msg.List)
            {
                MSG_ZGC_CHAT_INFO chat = GetZGateChatMsg(item);
                switch ((ChatChannel)item.ChatChannel)
                {
                    case ChatChannel.Person:
                        if (item.SensitiveWord)
                        {
                            break;
                        }
                        PlayerChar pc = Api.PCManager.FindPc(item.Param);
                        if (pc != null)
                        {
                            if (pc.CheckBlackExist(item.SpeakerInfo.Uid))
                            {
                                PlayerChar player = Api.PCManager.FindPc(item.SpeakerInfo.Uid);
                                player.SendErrorCodeMsg(ErrorCode.InTargetBlack);//
                            }
                            else
                            {
                                pc.AddChat(chat);
                            }
                        }
                        break;
                    case ChatChannel.Camp:
                       
                        foreach (var player in pcList)
                        {
                            if (!item.SensitiveWord)
                            {
                                if ((int)player.Value.Camp == item.Param)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                            else
                            {
                                if (player.Value.SpeakSensitiveWord)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                        }
                        break;
                    case ChatChannel.Family:
                        foreach (var player in pcList)
                        {
                            if (!item.SensitiveWord)
                            {
                                if (player.Value.FamilyId == item.Param)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                            else
                            {
                                if (player.Value.SpeakSensitiveWord)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                        }
                        break;
                    case ChatChannel.Team:
                        foreach (var player in pcList)
                        {
                            if (!item.SensitiveWord)
                            {
                                if (player.Value.Team != null && player.Value.Team.TeamId == item.Param)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                            else
                            {
                                if (player.Value.SpeakSensitiveWord)
                                {
                                    player.Value.AddChat(chat);
                                }
                            }
                        }
                        break;                
                    default:
                        Log.Warn("player {0} chat channel {1} unsupported", item.SpeakerInfo.Uid, item.ChatChannel);
                        break;
                }
            }
        }

        private MSG_ZGC_CHAT_INFO GetZGateChatMsg(MSG_ZR_CHAT msg)
        {
            MSG_ZGC_CHAT_INFO chat = new MSG_ZGC_CHAT_INFO();
            chat.ChatChannel = msg.ChatChannel;
            chat.EmojiId = msg.EmojiId;
            chat.Content = msg.Content;
            chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            chat.ChatFrameId = msg.ChatFrameId;
            if (msg.SpeakerInfo != null)
            {
                chat.PcInfo = PlayerInfo.GetChatPlayerInfo(msg);
            }
            return chat;
        }

        public MSG_ZR_CHAT GetZRChatMsg(PlayerChar player, MSG_GateZ_CHAT info)
        {
            MSG_ZR_CHAT chat = new MSG_ZR_CHAT();
            chat.SpeakerInfo = PlayerInfo.GetChatSpeakerInfo(player);
            chat.ChatChannel = info.ChatChannel;
            chat.Content = info.Content;
            chat.Param = info.Param;
            chat.EmojiId = info.EmojiId;
            chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            chat.ChatFrameId = player.GetChatFrame();
            chat.SensitiveWord = player.SpeakSensitiveWord;
            return chat;
        }


        public void AddChat(int uid, MSG_ZR_CHAT chat)
        {
            ChatList.List.Add(chat);
        }

        public void AddChat(PlayerChar player, MSG_GateZ_CHAT chat)
        {
            MSG_ZR_CHAT chatInfo = GetZRChatMsg(player,chat);
            ChatList.List.Add(chatInfo);
        }

        private void UpdateChat()
        {
            if (ChatList.List.Count >= 100 || (ZoneServerApi.now - LastSendChatTime).TotalMilliseconds > 100)
            {
                SendChatList();
            }
        }

        public void SendChatList()
        {
            if (ChatList.List.Count != 0)
            {
                Write(ChatList);
                ChatList.List.Clear();
            }
            LastSendChatTime = ZoneServerApi.now;
        }

        public void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHAT_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHAT_TRUMPET>(stream);
            SendChatTrumpet(msg.MainId, msg.ItemId, msg.Words, msg.PcInfo);
        }

        private void SendChatTrumpet(int mainId, int itemId, string words, RZ_SPEAKER_INFO pcInfo)
        {
            MSG_ZGC_CHAT_TRUMPET msg = new MSG_ZGC_CHAT_TRUMPET();
            msg.PcInfo = GetSpeakerInfo(pcInfo);
            msg.ItemId = itemId;
            msg.Words = words;
            msg.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            msg.ChatFrameId = pcInfo.ChatFrameId;
            msg.MainId = mainId;
            
            Api.ChatMng.AddBroadcastTrumpet(msg);
        }

        private PLAYER_INFO GetSpeakerInfo(RZ_SPEAKER_INFO msg)
        {
            PLAYER_INFO pcInfo = new PLAYER_INFO();
            pcInfo.PcUid = msg.Uid;
            pcInfo.Name = msg.Name;
            pcInfo.Camp = msg.Camp;
            pcInfo.Level = msg.Level;
            pcInfo.FaceIcon = msg.FaceIcon;
            pcInfo.ShowFaceJpg = msg.ShowFaceJpg;
            pcInfo.FaceFrame = msg.FaceFrame;
            pcInfo.Sex = msg.Sex;
            pcInfo.Title = msg.Title;
            pcInfo.TeamId = msg.TeamId;
            pcInfo.HeroId = msg.HeroId;
            pcInfo.GodType = msg.GodType;
            pcInfo.ArenaLevel = msg.ArenaLevel;
            return pcInfo;
        }
    }
}

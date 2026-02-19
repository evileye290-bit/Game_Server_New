using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RC;
using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public MSG_RZ_CHAT_LIST ChatList = new MSG_RZ_CHAT_LIST();

        private DateTime LastSendChatTime = RelationServerApi.now;

        public void OnResponse_ChatList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHAT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHAT_LIST>(stream);
            foreach (var item in msg.List)
            {
                Client client = ZoneManager.GetClient(item.SpeakerInfo.Uid);
                if (client == null)
                {
                    continue;
                }
                switch ((ChatChannel)item.ChatChannel)
                {
                    case ChatChannel.Camp:
                        // 直接塞进去就行
                        if (item.SpeakerInfo.Camp == (int)CampType.None)
                        {
                            break;
                        }
                        item.Param = item.SpeakerInfo.Camp;
                        foreach (var zone in ZoneManager.ServerList)
                        {
                            ((ZoneServer)zone.Value).AddChat(item);
                        }
                        break;
                    case ChatChannel.Family:
                        // 找到家族id 塞进去
                        if (client.Family == null)
                        {
                            break;
                        }
                        item.Param = client.Family.Uid;
                        foreach (var zone in ZoneManager.ServerList)
                        {
                            ((ZoneServer)zone.Value).AddChat(item);
                        }
                        break;
                    case ChatChannel.Team:
                        // 找到team id 塞进去
                        if (client.Team == null)
                        {
                            break;
                        }
                        item.Param = client.Team.TeamId;
                        foreach (var zone in ZoneManager.ServerList)
                        {
                            ((ZoneServer)zone.Value).AddChat(item);
                        }
                        break;
                    case ChatChannel.Person:
                        // 找到私聊对象及所在zone
                        Client person = ZoneManager.GetClient(item.Param);
                        if (person == null || person.CurZone == null)
                        {
                            break;
                        }
                        person.CurZone.AddChat(item);
                        break;
                    default:
                        Log.Warn("got client {0} unsupport chat channel {1}", item.SpeakerInfo.Uid, item.ChatChannel);
                        break;
                }
            }
        }

        public void AddChat(MSG_ZR_CHAT chat)
        {
            ChatList.List.Add(chat);
        }
     
        public void UpdateChat()
        {
            if (ChatList.List.Count >= 100 || (RelationServerApi.now - LastSendChatTime).TotalMilliseconds > 100)
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
            LastSendChatTime = RelationServerApi.now;
        }

        public void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHAT_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHAT_TRUMPET>(stream);
            MSG_RC_CHAT_TRUMPET request = new MSG_RC_CHAT_TRUMPET();
            request.MainId = msg.MainId;
            request.ItemId = msg.ItemId;
            request.Words = msg.Words;
            request.PcInfo = GetRCSpeakerInfo(msg.PcInfo, msg.ChatFrameId);
            Api.CrossServer.Write(request, uid);
        }

        private RC_SPEAKER_INFO GetRCSpeakerInfo(SPEAKER_INFO msg, int chatFrameId)
        {
            RC_SPEAKER_INFO pcInfo = new RC_SPEAKER_INFO();
            pcInfo.Uid = msg.Uid;
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
            pcInfo.ChatFrameId = chatFrameId;
            return pcInfo;
        }
    }
}

using EnumerateUtility;
using EnumerateUtility.Chat;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using System;
using System.Collections.Generic;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_ChatList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAT_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} chat words not find client", pcUid);
            }
        }
      
        //private void OnResponse_ChatTrumpet(MemoryStream stream, int pcUid)
        //{
        //    foreach (var client in Api.ClientMng.ClientList)
        //    {
        //        if (client != null)
        //        {
        //            client.Write(Id<MSG_ZGC_CHAT_TRUMPET>.Value, stream);
        //        }
        //        else
        //        {
        //            Log.WarnLine("player {0} chat trumpet not find client", client.Uid);
        //        }
        //    }
        //}

        private void OnResponse_ChatTrumpetResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAT_TRUMPET_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} chat trumpet result not find client", pcUid);
            }
        }

        public void OnResponse_BroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_ZGate_BROADCAST_ANNOUNCEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_BROADCAST_ANNOUNCEMENT>(stream);
            MSG_GC_ANNOUNCEMENT notify = new MSG_GC_ANNOUNCEMENT();
            notify.Type = msg.Type;
            // 只有系统公告是否需要在聊天框显示 其他公告为跑马灯+系统聊天框
            notify.Bottom = true;
            foreach (var item in msg.List)
            {
                notify.List.Add(item);
            }
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            Client.BroadCastMsgMemoryMaker(notify, out head, out body);
            Api.ClientMng.Broadcast(head, body);
        }

        private void OnResponse_NearbyEmoji(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEARBY_EMOJI>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} nearby emoji not find client", pcUid);
            }
        }

        private void OnResponse_TipOff(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TIP_OFF>.Value, stream);
            }
        }

        private void OnResponse_BroadcastChat(MemoryStream stream, int pcUid)
        {
            MSG_ZGC_CHAT_BROADCAST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_CHAT_BROADCAST>(stream);
            MSG_ZGC_CHAT_BROADCAST darkRoomPks = new MSG_ZGC_CHAT_BROADCAST();
            MSG_ZGC_CHAT_BROADCAST normalPks = new MSG_ZGC_CHAT_BROADCAST();
            MSG_ZGC_CHAT_BROADCAST trumpetPks = new MSG_ZGC_CHAT_BROADCAST();

            if (Api == null)
            {
                Log.Warn($"Api is null");
                return;
            }
            if (Api.ClientMng == null)
            {
                Log.Warn($"ClientMng is null");
                return;
            }
            
            if (pks.Trumpet.Count > 0)
            {
                foreach (var word in pks.Trumpet)
                {
                    if (word.PcInfo == null)
                    {
                        Log.Warn($"word pcInfo is null");
                        continue;
                    }
                    if (word.SensitiveWord)
                    {
                        Client client = Api.ClientMng.FindClientByUid(word.PcInfo.PcUid);
                        if (client != null)
                        {
                            client.SpeakSensitiveWord = true;
                        }

                       darkRoomPks.Trumpet.Add(word);
                    }
                    else
                    {
                        trumpetPks.Trumpet.Add(word);
                    }              
                }
                if (trumpetPks.Trumpet.Count > 0)
                {
                    Api.ClientMng.Broadcast(trumpetPks, false);
                }           
            }
            if (pks.Words.Count > 0)
            {
                foreach (var word in pks.Words)
                {           
                    if (word.PcInfo == null)
                    {
                        Log.Warn($"word pcInfo is null");
                        continue;
                    }

                    if (word.SensitiveWord)
                    {
                        Client client = Api.ClientMng.FindClientByUid(word.PcInfo.PcUid);
                        if (client != null)
                        {
                            client.SpeakSensitiveWord = true;
                        }

                        darkRoomPks.Words.Add(word);
                    }
                    else
                    {
                        normalPks.Words.Add(word);
                    }
                }
                if (normalPks.Words.Count > 0)
                {
                    Api.ClientMng.Broadcast(normalPks, false);
                }               
            }
            if (darkRoomPks.Words.Count > 0 || darkRoomPks.Trumpet.Count > 0)
            {
                Api.ClientMng.Broadcast(darkRoomPks, true);
            }
        }

        private void OnResponse_ChatSilence(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SILENCE>.Value, stream);
            }
        }

        private void OnResponse_ActivityChatBubble(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_CHAT_BUBBLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} activityChatBubble not find client", uid);
            }
        }

        private void OnResponse_SetGm(MemoryStream stream, int uid)
        {
            MSG_ZGate_GM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_GM>(stream);
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.IsGm = pks.IsGm;
            }
        }

        private void OnResponse_CheckChatLimit(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHECK_CHATLIMIT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} check chatLimit not find client", uid);
            }
        }

        private void OnResponse_BuyChatTrumpet(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_TRUMPET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy chatTrumpet not find client", uid);
            }
        }


        private void OnResponse_Chat(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} chat not find client", uid);
            }
        }

        private void OnResponse_NewBubbleList(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_BUBBLE_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} notify new bubble list not find client", uid);
            }
        }


        private void OnResponse_SensitiveWord(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SENSITIVE_WORD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} notify sensitive word not find client", uid);
            }
        }
    }
}

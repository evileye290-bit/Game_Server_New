using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using ServerShared;
using SocketShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ChatManager
    {
        private ZoneServerApi server = null;
        public ZoneServerApi Server
        { get { return server; } }

        public ChatManager(ZoneServerApi server)
        {
            this.server = server;
        }

        MSG_ZGC_CHAT_BROADCAST BroadcastChatList = new MSG_ZGC_CHAT_BROADCAST();

        public void Update(double deltaTime)
        {
            if (CheckUpdateTip(deltaTime))
            {
                if (BroadcastChatList.Words.Count > 0 || BroadcastChatList.Trumpet.Count > 0)
                {
                    //SendBroadcastChatList(BroadcastChatList);
                    SendBroadCastChat(BroadcastChatList);
                }
            }

            //if (CheckUpdatePersonTip(deltaTime))
            //{
            //    if (PersonChatList.Count > 0)
            //    {
            //        MSG_ZGC_CHAT_INFO msg = PersonChatList[0];
            //        PlayerChar player = server.PCManager.FindPc(msg.ToUid);
            //        if (player == null)
            //        {
            //            Log.Warn("player {0} person chat words not find", msg.ToUid);
            //        }
            //        else
            //        {
            //            if (!player.CheckBlackExist(msg.PcInfo.PcUid))
            //            {
            //                player.Write(msg);
            //            }
            //        }
            //        ClearPersonList();
            //    }
            //}
        }

        private void SendBroadcastChatList(MSG_ZGC_CHAT_BROADCAST chat)
        {
            if (chat.Words.Count + chat.Trumpet.Count > CONST.CHAT_BROADCAST_MAX_COUNT)
            {
                int total = 0;
                bool isEnd = false;
                MSG_ZGC_CHAT_BROADCAST broadCast = new MSG_ZGC_CHAT_BROADCAST();
                foreach (var word in chat.Words)
                {
                    broadCast.Words.Add(word);
                    total++;
                   
                    if (total == CONST.CHAT_BROADCAST_MAX_COUNT)
                    {
                        server.GateManager.Broadcast(broadCast);
                        isEnd = true;
                        break;
                    }
                }
                for (int i = 0; i < total; i++)
                {
                    chat.Words.RemoveAt(0);
                }
                if (isEnd)
                {
                    return;
                }

                int temp = 0;
                foreach (var trumpet in chat.Trumpet)
                {
                    broadCast.Trumpet.Add(trumpet);
                    total++;
                    temp++;

                    if (total == CONST.CHAT_BROADCAST_MAX_COUNT)
                    {
                        server.GateManager.Broadcast(broadCast);
                        //isEnd = true;
                        break;
                    }
                }
                for (int i = 0; i < temp; i++)
                {
                    chat.Trumpet.RemoveAt(0);
                }
                //if (!isEnd)
                //{
                //    server.GateManager.Broadcast(broadCast);
                //}
            }
            else
            {
                server.GateManager.Broadcast(chat);
                chat.Words.Clear();
                chat.Trumpet.Clear();
            }
        }

        private void SendBroadCastChat(MSG_ZGC_CHAT_BROADCAST chat)
        {
            MSG_ZGC_CHAT_BROADCAST broadCast = new MSG_ZGC_CHAT_BROADCAST();

            if (chat.Words.Count > 0)
            {
                MSG_ZGC_CHAT_INFO word = chat.Words.FirstOrDefault();
                broadCast.Words.Add(word);
                chat.Words.Remove(word);
            }
            if (chat.Trumpet.Count > 0)
            {
                MSG_ZGC_CHAT_TRUMPET trumpet = chat.Trumpet.FirstOrDefault();
                broadCast.Trumpet.Add(trumpet);
                chat.Trumpet.Remove(trumpet);
            }
            if (broadCast.Words.Count > 0 || broadCast.Trumpet.Count > 0)
            {
                server.GateManager.Broadcast(broadCast);
            }
        }

        private void SendChatListMsg()
        {
            server.GateManager.Broadcast(BroadcastChatList);
            ClearBroadcastList();
        }

        public void GateBroadcast<T>(T msg, int uid) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(SocketHeader.ZGateSize);
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);

            server.GateManager.Broadcast(msg);
        }

        double elapsedUpdateTime = 1000;
        double updatedTime = 0;

        private bool CheckUpdateTip(double deltaTime)
        {
            updatedTime += (float)deltaTime;
            if (updatedTime < elapsedUpdateTime)
            {
                return false;
            }
            else
            {
                updatedTime = 0;
                return true;
            }
        }

        double elapsedUpdatePersonTime = 100;
        double updatedPersonTime = 0;
        private bool CheckUpdatePersonTip(double deltaTime)
        {
            updatedPersonTime += (float)deltaTime;
            if (updatedPersonTime < elapsedUpdatePersonTime)
            {
                return false;
            }
            else
            {
                updatedPersonTime = 0;
                return true;
            }
        }

        public void AddBroadcastChat(MSG_ZGC_CHAT_INFO word)
        {
            BroadcastChatList.Words.Add(word);

            //if (BroadcastChatList.Words.Count > 20)
            //{
            //    SendChatListMsg();
            //}
        }

        public void AddBroadcastTrumpet(MSG_ZGC_CHAT_TRUMPET trumpet)
        {
            BroadcastChatList.Trumpet.Add(trumpet);

            //if (BroadcastChatList.Trumpet.Count > 20)
            //{
            //    SendChatListMsg();
            //}
        }

        private void ClearBroadcastList()
        {
            BroadcastChatList.Words.Clear();
            BroadcastChatList.Trumpet.Clear();
        }

        //public void AddPersonChat(MSG_ZGC_PERSON_CHAT_WORDS word)
        //{
        //    PersonChatList.Add(word);
        //}

        //private void ClearPersonList()
        //{
        //    PersonChatList.RemoveAt(0);
        //}

    }
}

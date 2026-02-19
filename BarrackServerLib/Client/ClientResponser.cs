using Message.IdGenerator;
using Logger;
using Message.Client.Protocol.CBarrack;
using System;
using System.Collections.Generic;
using System.IO;

namespace BarrackServerLib
{
    partial class Client
    {
        public delegate void Responser(MemoryStream stream);
        private Dictionary<uint, Responser> responsers = new Dictionary<uint, Responser>();

        public void AddResponser(uint id, Responser responser)
        {
            responsers.Add(id, responser);
        }

        public void BindResponser()
        {
            responsers.Add(Id<MSG_CB_HEARTBEAT>.Value, OnResponse_Heartbeat);
            responsers.Add(Id<MSG_CB_USER_LOGIN>.Value, OnResponse_Login);
            responsers.Add(Id<MSG_CB_LOGIN_SERVERS>.Value, OnResponse_LoginServers);
            responsers.Add(Id<MSG_CB_SERVER_STATE>.Value, OnResponse_ServerState);
            responsers.Add(Id<MSG_CB_GAME_LOAD>.Value, OnResponse_GameLoad);
        }

        public void OnResponse(uint id, MemoryStream stream)
        {
            Responser responser = null;
            LastRecvTime = BarrackServerApi.now;
            if (LastHeartbeatTime != DateTime.MaxValue)
            {
                LastHeartbeatTime = DateTime.MaxValue;
            }
            if (responsers.TryGetValue(id, out responser))
            {
                try
                {
                    responser(stream);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            else
            {
                Log.Warn("got client account {0} unsupported package id {1}", AccountRealName, id);
            }
        }

        private void OnResponse_Heartbeat(MemoryStream stream)
        {
            LastHeartbeatTime = DateTime.MaxValue;
        }
    }

}

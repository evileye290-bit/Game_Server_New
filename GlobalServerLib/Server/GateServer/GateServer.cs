using System;
using System.Collections.Generic;
using ServerShared;
using Logger;
using System.IO;
using Message.IdGenerator;
using Message.Gate.Protocol.GateG;
using DBUtility;
using ServerFrame;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class GateServer : FrontendServer
    {
        GlobalServerApi Api
        {get {return (GlobalServerApi)api;}}

        public GateServer(BaseApi api):base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GateG_COMMAND_RESULT>.Value, OnResponse_CommandResult);
        }


        public void OnResponse_GateGRegist(MemoryStream stream, int uid = 0)
        {
            //MSG_GateG_REGISTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateG_REGISTER>(stream);
            //Log.Info("gate {0} id {1} regist to global", msg.MainId, msg.GateId);
            //InitBaseInfo(msg.MainId, msg.GateId);
            //registInfo.Regist(msg.MainId, msg.GateId, msg.Ip, msg.ClientIp, msg.clientPort);
            //serverManager.AddServer(this);
            //Api.OnGateRegist(this);
        }


        public void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_GateG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateG_COMMAND_RESULT>(stream);
            if (msg.Success)
            {
                Log.Warn("====================================================");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("====================================================");
            }
            else
            {
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
        }
    }
}
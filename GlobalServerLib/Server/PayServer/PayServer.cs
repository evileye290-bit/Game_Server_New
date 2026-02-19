using System.Collections.Generic;
using DBUtility;
using Logger;
using Message.Barrack.Protocol.BG;
using Message.IdGenerator;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.IO;
using System.Web.Script.Serialization;
using Message.Pay.Protocol.PG;


namespace GlobalServerLib
{
    public partial class PayServer : FrontendServer
    {
        private GlobalServerApi Api => (GlobalServerApi)api;

        public PayServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_PG_HEARTBEAT>.Value, OnResponse_Heartbeat);
            AddResponser(Id<MSG_PG_COMMAND_RESULT>.Value, OnResponse_CommandResult);
            //ResponseEnd
        }

        private void OnResponse_Heartbeat(MemoryStream stream, int uid = 0)
        {
            MSG_PG_HEARTBEAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_PG_HEARTBEAT>(stream);
            Log.Info("pay heart beat");
        }

        private void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_PG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_PG_COMMAND_RESULT>(stream);
            if (msg.Success == true)
            {
                Log.Warn("==============================================================================");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("==============================================================================");
            }
            else
            {
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
        }
    }
}
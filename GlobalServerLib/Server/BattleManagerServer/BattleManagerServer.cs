using Logger;
using System.IO;
using Message.IdGenerator;
using Message.Global.Protocol.GBM;
using Message.BattleManager.Protocol.BMG;
using ServerFrame;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class BattleManagerServer : FrontendServer
    {
        GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }

        public BattleManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BMG_COMMAND_RESULT>.Value, OnResponse_CommandResult);

        }

        public void OnResponse_BMRegist(MemoryStream stream, int uid = 0)
        {
            //MSG_BMG_REGISTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BMG_REGISTER>(stream);
            //Log.Info("battle manager id {0} regist to global", msg.MainId);
            //InitBaseInfo(msg.MainId);
            //serverManager.AddServer(this);
            //registInfo.Regist(msg.MainId, msg.Ip, msg.zonePort, msg.battlePort);
            //Api.OnBattleManagerRegist(this);
        }

      
        public void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_BMG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BMG_COMMAND_RESULT>(stream);
            if (msg.Success)
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
using System.IO;
using ServerShared;
using Logger;
using Message.IdGenerator;
using Message.Battle.Protocol.BattleG;
using Message.Global.Protocol.GBattle;
using ServerFrame;
using MessagePacker;

namespace BattleServerLib
{
    public class GlobalServer:BaseGlobalServer
    {
        private BattleServerApi Api
        { get { return (BattleServerApi)api; } }

        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GBattle_SHUTDOWN_Battle>.Value, OnResponse_ShutDown);
            AddResponser(Id<MSG_GBattle_STOP_FIGHTING>.Value, OnResponse_StopFighting);
            //ResponserEnd
        }


        private void OnResponse_ShutDown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown battle");
            CONST.ALARM_OPEN = false;
            Api.State = ServerState.Stopping;
            Api.StoppingTime = BattleServerApi.now.AddMinutes(1);
        }

        private void OnResponse_StopFighting(MemoryStream stream, int uid = 0)
        {
            MSG_GBattle_STOP_FIGHTING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GBattle_STOP_FIGHTING>(stream);
            Log.Warn("global request stop fighting result {0}", msg.Result);
        }

    }
}
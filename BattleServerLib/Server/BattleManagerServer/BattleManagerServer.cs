using System.IO;
using Logger;
using Message.IdGenerator;
using Message.BattleManager.Protocol.BMBattle;
using ServerFrame;
using MessagePacker;

namespace BattleServerLib
{
    public partial class BattleManagerServer : BackendServer
    {
        private BattleServerApi Api
        { get { return (BattleServerApi)api; } }

        public BattleManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BMBattle_SHUTDOWN_BATTLE>.Value, OnResponse_ShutDownBattle);
            AddResponser(Id<MSG_BMBattle_SET_FPS>.Value, OnResponse_SetFps);
            AddResponser(Id<MSG_BMBattle_UPDATE_XML>.Value, OnResponse_UpdateXml);
            //ResponserEnd
        }

        private void OnResponse_ShutDownBattle(MemoryStream stream, int uid = 0)
        {
            Log.Warn("battle manager request main {0} sub {1} shutdown", Api.MainId, Api.SubId);
            Api.StopServer(1);
        }

        private void OnResponse_SetFps(MemoryStream stream, int uid = 0)
        {
            MSG_BMBattle_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BMBattle_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);
        }


        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            //MSG_BMBattle_UPDATE_XML msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BMBattle_UPDATE_XML>(stream);
            Api.UpdateXml();
            Log.Write("GM update xml");
        }

    }
}
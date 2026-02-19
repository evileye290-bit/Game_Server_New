using DataProperty;
using CommonUtility;
using ServerShared;
using ScriptFunctions;
using ServerFrame;

namespace BattleManagerServerLib
{
    public partial class BattleManagerServerApi
    {
        public GlobalServer GlobalServer
        { get { return (GlobalServer)(serverManagerProxy.GetSinglePointBackendServer(ServerFrame.ServerType.GlobalServer, ClusterId)); } }

        public FrontendServerManager ZoneManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.ZoneServer); } }

        public BattleServerManager BattleServerManager
        { get { return (BattleServerManager)(serverManagerProxy.GetFrontendServerManager(ServerFrame.ServerType.BattleServer)); } }

        public override void InitData()
        {
            base.InitData();
            RobotLibrary.Init();
            ScriptManager.Init(PathExt.FullPathFromServer("Script"));
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.BattleManager.Protocol.BMBattle.BMBattleIdGenerator.GenerateId();
            Message.BattleManager.Protocol.BMG.BMGIdGenerator.GenerateId();
            Message.BattleManager.Protocol.BMZ.BMZIdGenerator.GenerateId();

            Message.Battle.Protocol.BattleBM.BattleBMIdGenerator.GenerateId();
            Message.Global.Protocol.GBM.GBMIdGenerator.GenerateId();
            Message.Zone.Protocol.ZBM.ZBMIdGenerator.GenerateId();
        }

    }
}

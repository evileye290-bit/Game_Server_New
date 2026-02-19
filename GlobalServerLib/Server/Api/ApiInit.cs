using Logger;
using DataProperty;
using DBUtility;

namespace GlobalServerLib
{
    partial class GlobalServerApi
    {
        private ClientManager clientManager;
        public ClientManager ClientMng
        { get { return clientManager; } }

        private WelfareManager wlfareMng;
        public WelfareManager WelfareMng
        { get { return wlfareMng; } }

        void InitBasic()
        {
            clientManager = new ClientManager(this);

            InitWelfareManager();
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Analysis.Protocol.AG.AGIdGenerator.GenerateId();
            Message.Global.Protocol.GA.GAIdGenerator.GenerateId();
            Message.Global.Protocol.GGate.GGateIdGenerator.GenerateId();
            Message.Global.Protocol.GB.GBIdGenerator.GenerateId();
            Message.Global.Protocol.GM.GMIdGenerator.GenerateId();
            Message.Global.Protocol.GR.GRIdGenerator.GenerateId();
            Message.Global.Protocol.GZ.GZIdGenerator.GenerateId();
            Message.Global.Protocol.GBM.GBMIdGenerator.GenerateId();
            Message.Global.Protocol.GBattle.GBattleIdGenerator.GenerateId();
            Message.Global.Protocol.GCM.GCMIdGenerator.GenerateId();
            Message.Global.Protocol.GCross.GCrossIdGenerator.GenerateId();
            Message.Global.Protocol.GP.GPIdGenerator.GenerateId();

            Message.Gate.Protocol.GateG.GateGIdGenerator.GenerateId();
            Message.Barrack.Protocol.BG.BGIdGenerator.GenerateId();
            Message.Manager.Protocol.MG.MGIdGenerator.GenerateId();
            Message.Relation.Protocol.RG.RGIdGenerator.GenerateId();
            Message.Zone.Protocol.ZG.ZGIdGenerator.GenerateId();
            Message.BattleManager.Protocol.BMG.BMGIdGenerator.GenerateId();
            Message.Battle.Protocol.BattleG.BattleGIdGenerator.GenerateId();;
            Message.ChatManager.Protocol.CMG.CMGIdGenerator.GenerateId();
            Message.Pay.Protocol.PG.PGIdGenerator.GenerateId();
        }

        void InitSocket()
        {
            customPort = (ushort)globalServerData.GetInt("customPort");
            Engine.System.Listen(customPort, (ushort listen_port) =>
            {
                Client client = new Client(this);
                client.Listen(listen_port);
            });
        }

        public override void InitDone()
        {
            base.InitDone();
            if (ChannelServer != null)
            {
                ChannelServer.NotifyInitDone();
            }
            if (HttpGmServer != null)
            {
                HttpGmServer.NotifyInitDone();
            }
        }


        public void InitWelfareManager()
        {
            wlfareMng = new WelfareManager(this);
            //wlfareMng.Init();
        }
    }
}

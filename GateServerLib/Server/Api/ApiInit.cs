using System;
using System.IO;
using DataProperty;
using Logger;
using CommonUtility;
using ServerShared;
using ServerFrame;
using EnumerateUtility;
using System.Threading;
#if DEBUG
using ProtocolCatchMsgLogLib;
using ProtocolObjectParserLib;
#endif
namespace GateServerLib
{

    public partial class GateServerApi
    {
        //public ushort ClientPort;
        //public string ClientIp = string.Empty;
        public AuthManager AuthMng;

        //public ChatManagerServer ChatManagerServer
        //{ get { return (ChatManagerServer)(serverManagerProxy.GetSinglePointBackendServer(ServerType.ChatManagerServer, ClusterId)); } }

        //public BarrackServer BarrackServer
        //{ get { return (BarrackServer)(serverManagerProxy.GetSinglePointBackendServer(ServerType.BarrackServer, ClusterId)); } }

        public BackendServerManager BarrackServerManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.BarrackServer); } }

        public BarrackServer BarrackServerWatchDog
        { get { return (BarrackServer)BarrackServerManager.GetWatchDogServer(); } }

        public BackendServerManager ZoneServerManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.ZoneServer); } }

        public BackendServerManager ManagerServerManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.ManagerServer); } }

        private ClientManager clientManager;
        public ClientManager ClientMng
        { get { return clientManager; } }

        //private ChatManager chatMng;
        //public ChatManager ChatMng
        //{ get { return chatMng; } }

        public UidManager UID = new UidManager();

        public override void InitData()
        {
            base.InitData();
            InitInputLimitConfig();
            EmailLibrary.BindEmailDatas();
            TaskLibrary.BindTaskInfo();
            ChapterLibrary.Init();
            CharacterInitLibrary.Init();
            BagLibrary.Init();
            HeroLibrary.Init();
            CurrenciesLibrary.Init();
            CounterLibrary.Init();
            PushFigureLibrary.Init();
            WuhunResonanceConfig.Init();
            WarehouseLibrary.Init();
            SchoolLibrary.Init();
        }

        public override void InitTrackingLog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", MainId, SubId);
            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }

            trackingLoggerManager = new TrackingLoggerManager(logServerKey, statLogPrefix);
            //TrackingLoggerMng.CreateLogger(TrackingLogType.CREATEACCOUNT);// 创建账号
            TrackingLoggerMng.CreateLogger(TrackingLogType.CREATECHAR);//
            //TrackingLoggerMng.CreateLogger(TrackingLogType.GAMECOMMENT);//评价
        }

        public override void InitBILog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", mainId, subId);
            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }

            biLoggerManager = new BILoggerManager(logServerKey,ServerType.ToString(), statLogPrefix, this);
            //BILoggerMng.CreateLogger(BILoggerType.CREATECHAR);//
            BIXmlUpdate();
        }


#if DEBUG
        ///// <summary>
        ///// 消息解析相关功能
        ///// </summary>
        AProtocolMethods methodHandler;

        private void InitMsgCatchLog()
        {
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            string statLogPrefix = string.Format("{0}_{1}", MainId, SubId);
            Data data = gameConfig.GetByIndex(0);
            var parserLogger = new MsgCatchLogger(this);
            parserLogger.Init(statLogPrefix, true, true);
            MsgCatchLog.SetGlobalLogger(parserLogger);
            MsgCatchLog.Open();

            var streamLogger = new StreamCatchLogger(this);
            streamLogger.Init(statLogPrefix, false, true);
            StreamCatchLog.SetGlobalLogger(streamLogger);
            StreamCatchLog.Open();

            UtilityLibrary.PathExt.InitPath();
            if (AssemblyParser.AssemblyParse(UtilityLibrary.PathExt.codePath))
            {
                methodHandler = new AssemblyProtocolHandler();
                //methodHandler = new ProtoMsg();
                methodHandler.BindMsgId();
                methodHandler.BindParser();
            }
            else
            {
                Log.Error("InitMsgCatchLog got an error");
            }

        }
#endif
        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
            Message.Gate.Protocol.GateB.GateBIdGenerator.GenerateId();
            Message.Gate.Protocol.GateG.GateGIdGenerator.GenerateId();
            Message.Gate.Protocol.GateM.GateMIdGenerator.GenerateId();
            Message.Gate.Protocol.GateZ.GateZIdGenerator.GenerateId();
            Message.Gate.Protocol.GateCM.GateCMIdGenerator.GenerateId();

            Message.Client.Protocol.CGate.CGateIdGenerator.GenerateId();
            Message.Barrack.Protocol.BGate.BGateIdGenerator.GenerateId();
            Message.Global.Protocol.GGate.GGateIdGenerator.GenerateId();
            Message.Manager.Protocol.MGate.MGateIdGenerator.GenerateId();
            Message.Zone.Protocol.ZGate.ZGateIdGenerator.GenerateId();
            Message.ChatManager.Protocol.CMGate.CMGateIdGenerator.GenerateId();
        }

        public override void InitConfig()
        {
            base.InitConfig();
            //if (ServerData != null)
            //{
            //    ClientIp = serverData.GetString("clientIp");
            //    ClientPort = (ushort)serverData.GetInt("port");
            //}
            //else
            //{
            //    Log.Error("init config failed: can not find gate config id {0}", SubId);
            //}

            Data gameConfig = DataListManager.inst.GetData("ConstConfig", 1);
            Client.cryptoOpen = gameConfig.GetBoolean("BlowFishEncrypt");
        }

        private static void InitInputLimitConfig()
        {
            try
            {
                WordLengthLimit.CharNameLenLimit = DataListManager.inst.GetData("InputLimit", "CharNameLenLimit").GetInt("value");
                WordLengthLimit.BadGameJudgeInputLimitLow = DataListManager.inst.GetData("InputLimit", "BadGameJudgeInputLimitLow").GetInt("value");
                WordLengthLimit.BadGameJudgeInputLimitHigh = DataListManager.inst.GetData("InputLimit", "BadGameJudgeInputLimitHigh").GetInt("value");
                WordLengthLimit.ApplyFriendInputLimit = DataListManager.inst.GetData("InputLimit", "ApplyFriendInputLimit").GetInt("value");
                WordLengthLimit.CommentMinLen = DataListManager.inst.GetData("InputLimit", "HandBookLimitLow").GetInt("value");
                WordLengthLimit.CommentMaxLen = DataListManager.inst.GetData("InputLimit", "HandBookLimitHigh").GetInt("value");
                WordLengthLimit.SignatureSize = DataListManager.inst.GetData("InputLimit", "ZoneSingLimit").GetInt("value");
                WordLengthLimit.QNum_SIZE = DataListManager.inst.GetData("InputLimit", "QNumLimit").GetInt("value");
                WordLengthLimit.WNum_SIZE = DataListManager.inst.GetData("InputLimit", "WNumLimit").GetInt("value");
                WordLengthLimit.HeroQueueNameLimit = DataListManager.inst.GetData("InputLimit", "HeroQueueNameLimit").GetInt("value");
                WordLengthLimit.QuestionnaireInput = DataListManager.inst.GetData("InputLimit", "QuestionnaireInput").GetInt("value");
                WordLengthLimit.QuestionOptionInput = DataListManager.inst.GetData("InputLimit", "QuestionOptionInput").GetInt("value");
                WordLengthLimit.QuestionnaireTelephoneInput = DataListManager.inst.GetData("InputLimit", "QuestionnaireTelephoneInput").GetInt("value");
                WordLengthLimit.ReportLimit = DataListManager.inst.GetData("InputLimit", "ReportLimit").GetInt("value");
                WordLengthLimit.GameComment = DataListManager.inst.GetData("InputLimit", "GameComment").GetInt("value");
            }
            catch (Exception e)
            {
                Log.Error("InitInputLimitConfig has error data :{0}",e.Message);
                return;
            }
        }

        public WordChecker WordChecker;
        public WordChecker NameChecker;
        private void InitWordChecker()
        {
            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "WordCheck.txt", SearchOption.AllDirectories);
            if (files.Length != 1)
            {
                Log.Warn("gate main {0} sub {1} Init Word Check failed, Check it!", MainId, SubId);
                return;
            }
            FileStream fsRead = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            fsRead.Close();
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] badWords = myStr.Split('、');
            WordChecker = new WordChecker(badWords);

            files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "NameCheck.txt", SearchOption.AllDirectories);
            if (files.Length != 1)
            {
                Log.Warn("Init Name Check failed, Check it!");
                return;
            }
            fsRead = new FileStream(files[0], FileMode.Open,FileAccess.Read, FileShare.Read);
            fsLen = (int)fsRead.Length;
            heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            fsRead.Close();
            myStr = System.Text.Encoding.UTF8.GetString(heByte);
            badWords = myStr.Split('、');
            NameChecker = new WordChecker(badWords);

        }

        public void InitAuthManager()
        {
            AuthMng = new AuthManager();
            AuthMng.Init();
        }

        //public void InitChatMng()
        //{
        //    chatMng = new ChatManager();
        //}

        public void InitClient()
        {
            clientManager = new ClientManager();
            clientManager.Init(this);
            //DataList constconfigDataList = DataListManager.inst.GetDataList("ConstConfig");

            Engine.System.Listen((ushort)base.Port, (ushort listen_port) =>
            {
                Client client = new Client(this);
                client.Listen(listen_port);
            });
        }

        public override void UpdateXml()
        {
            DoTaskStart(DoUpdateXml);
            //base.UpdateXml();
            //InitData();
            //BIXmlUpdate();
            //AuthMng.Init();
        }

        private void DoUpdateXml()
        {
            Log.WarnLine("gate main {0} sub {1}  XML update START", mainId, subId);

            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.Chnage(file);
                Thread.Sleep(10);
            }
            InitData();
            BIXmlUpdate();
            AuthMng.Init();

            Log.WarnLine("gate main {0} sub {1} XML update END", mainId, subId);
        }

        public static void DoTaskStart(Action action)
        {
            //base.UpdateXml();
            var task = new System.Threading.Tasks.Task(() => action());
            task.Start();
        }

        private void BIXmlUpdate()
        {
            var biConfigData = DataListManager.inst.GetData("BILogConfig", 1);
            int fileSize = biConfigData.GetInt("FileSize");
            int refreshTime = biConfigData.GetInt("RefreshTime");
            BILoggerMng.UpdateXml(refreshTime, fileSize);
        }
    }
}

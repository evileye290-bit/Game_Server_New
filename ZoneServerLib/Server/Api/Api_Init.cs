using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Logger;
using DataProperty;
using DBUtility;
using CommonUtility;
using ServerShared;
using System.Windows.Forms;
using Message.Relation.Protocol.RZ;
using MySql.Data.MySqlClient;
using ServerModels;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerFrame;
using Message.Zone.Protocol.ZGate;
using System.Web.Script.Serialization;
using ServerLogger;

namespace ZoneServerLib
{
	partial class ZoneServerApi
	{
        public ManagerServer ManagerServer
        { get { return (ManagerServer)(serverManagerProxy.GetSinglePointBackendServer(ServerType.ManagerServer, mainId)); } }

        public RelationServer RelationServer
        { get { return (RelationServer)(serverManagerProxy.GetSinglePointBackendServer(ServerType.RelationServer, mainId)); } }

        //public BackendServer BattleManagerServer
        //{ get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.BattleManagerServer, ClusterId); } }

        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public FrontendServerManager GateManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.GateServer); } }

        public FrontendServerManager BattleServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.BattleServer); } }

        public AnalysisServer AnalysisServer
        { get { return (AnalysisServer)serverManagerProxy.GetSinglePointBackendServer(ServerType.AnalysisServer, mainId); } }

        private ChatManager chatMng;
        public ChatManager ChatMng
        {
            get { return chatMng; }
        }

        private CrossBattleManager crossBattleMng;
        public CrossBattleManager CrossBattleMng
        {
            get { return crossBattleMng; }
        }

        public CrossChallengeManager CrossChallengeMng { get; set; }

        public IntegralBossManager IntegralBossManager { get; private set; }

        public ThemeBossManager ThemeBossMng { get; private set; }

        public CarnivalBossManager CarnivalBossMng { get; private set; }

        // key map_id value channel count
        private Dictionary<int, int> mapChannelCount = new Dictionary<int,int>();
        /// <summary>
        /// 地图分线数量
        /// </summary>
        public Dictionary<int, int> MapChannelCount
        { get { return mapChannelCount; } }

        //public Data CreatePlayerConfig { get; set; }

        //private Dictionary<int, int> turntableLimit = new Dictionary<int, int>();
        //public Dictionary<int, int> TurntableLimit
        //{
        //    get { return turntableLimit; }
        //    set { turntableLimit = value; }
        //}

        //private Dictionary<int, int> activationCodePlayerList = new Dictionary<int, int>();
        //public Dictionary<int, int> ActivationCodePlayerList
        //{ get { return activationCodePlayerList; } }

        private Dictionary<int, Data> oldPlayerRebateList = new Dictionary<int, Data>();
        public Dictionary<int, Data> OldPlayerRebateList
        { get { return oldPlayerRebateList; } }

        private List<ChangeNameResult> changeNamePlayerList = new List<ChangeNameResult>();
        public List<ChangeNameResult> ChangeNamePlayerList
        { get { return changeNamePlayerList; } }

        //public Dictionary<string, int> createPlayerCurrencies = new Dictionary<string, int>();
        //public Dictionary<string, int> CreatePlayerCurrencies
        //{ get { return createPlayerCurrencies; } }

        //public List<string[]> CreatePlayerItems
        //{ get; set; }

        private ContributionManager contributionMng;
        public ContributionManager ContributionMng
        { get { return contributionMng; } }

        private SpaceTimeTowerManager spaceTimeTowerManager;
        public  SpaceTimeTowerManager SpaceTimeTowerManager
        {
            get { return spaceTimeTowerManager; }
        }
        /// <summary>
        /// 屏蔽字
        /// </summary>
        public WordChecker wordChecker { get; set; }
        public WordChecker sensitiveWordChecker { get; set; }
        public WordChecker NameChecker { get; set; }
        //public EmojiChecker emojiChecker { get; set; }


        //List<int[]> expLvPunish = new List<int[]>();

        //public TeamBossState TeamBossState { get; set; }

        //public static float LastFreedomBattleRatio = 1.0f;
        //public static float LastRoyalBattleRatio = 1.0f;
        //public static int LastCampBattleWinner = CampBattle.None;

        // mapId_camp
        //public HashSet<string> TeamBossFullList = new HashSet<string>();
        //public Dictionary<int, float> TeamBossRankFactor = new Dictionary<int, float>();


        /// <summary>
        /// 开服时间
        /// </summary>
        public DateTime OpenServerTime { get; set; }
        /// <summary>
        /// 开服日期
        /// </summary>
        public DateTime OpenServerDate { get; set; }


        // key Red Packet Type, value current red packet index and param
        //public Dictionary<RedPacketType, KeyValuePair<int, int>> CurrentRedPacketList = new Dictionary<RedPacketType, KeyValuePair<int, int>>();

        public JavaScriptSerializer JsonSerialize = new JavaScriptSerializer();

        public override void InitConfig()
        {
            base.InitConfig();
            PushServer.Init();
        }

        private void InitWordChecker()
        {
            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "WordCheck.txt", SearchOption.AllDirectories);
            if (files.Length != 1)
            {
                Log.Warn("zone main {0} sub {1} Init Word Check failed, Check it!", mainId, subId);
                return;
            }
            FileStream fsRead = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            fsRead.Close();
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] badWords = myStr.Split('、');
            wordChecker = new WordChecker(badWords);
            //emojiChecker = new EmojiChecker(); 
        }

        private void InitNameChecker()
        {
            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "NameCheck.txt", SearchOption.AllDirectories);
            if (files.Length != 1)
            {
                Log.Warn("zone main {0} sub {1} Init Name Check failed, Check it!", mainId, subId);
                return;
            }
            FileStream fsRead = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            fsRead.Read(heByte, 0, heByte.Length);
            fsRead.Close();
            string myStr = System.Text.Encoding.UTF8.GetString(heByte);
            string[] badWords = myStr.Split('、');
            NameChecker = new WordChecker(badWords);
        }

        public void InitSensitiveWord()
        {
            sensitiveWordChecker = new WordChecker();
            MySqlConnection conn = accountDBPool.DBManagerList[0].GetOneConnection();
            MySqlDataReader reader = null;

            try
            {
                MySqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = "SELECT `content` FROM `sensitive_word`;";
                cmd.CommandType = System.Data.CommandType.Text;
                reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    string badWord = reader.GetString(0);
                    sensitiveWordChecker.AddOneBadWord(badWord);
                }
            }
            catch (Exception e)
            {
                Log.ErrorLine(e.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }
        }

        public void InitRechargeRebate()
        {
            RechargeRebateLibrary.Init(mainId);
        }

        public override void InitTrackingLog()
        {
            //TeamBossState = ServerShared.TeamBossState.Stop;
            string statLogPrefix = string.Format("_{0}_{1}", mainId, subId);

            //FR20161123 
            trackingLoggerManager = new TrackingLoggerManager(GameConfig.LogServerKey, statLogPrefix);
            TrackingLoggerMng.CreateLogger(TrackingLogType.CREATECHAR);//创建角色
            TrackingLoggerMng.CreateLogger(TrackingLogType.LOGIN);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.LOGOUT);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.ONLINE);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.CONSUME);//消耗
            TrackingLoggerMng.CreateLogger(TrackingLogType.OBTAIN);//获得
            TrackingLoggerMng.CreateLogger(TrackingLogType.CONSUMECURRENCY);//消耗
            TrackingLoggerMng.CreateLogger(TrackingLogType.OBTAINCURRENCY);//获得
            TrackingLoggerMng.CreateLogger(TrackingLogType.SHOP);//商店
            TrackingLoggerMng.CreateLogger(TrackingLogType.RECHARGE);//充值
            TrackingLoggerMng.CreateLogger(TrackingLogType.TASK);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.LISTENCHAT);//言论
            TrackingLoggerMng.CreateLogger(TrackingLogType.TIPOFF);// 举报
            TrackingLoggerMng.CreateLogger(TrackingLogType.GAMECOMMENT);//评价
            TrackingLoggerMng.CreateLogger(TrackingLogType.REFRESH);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.RELATIONRANK);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.TIMER);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.RECHARGETIMER);//充值活动刷新

            TrackingLoggerMng.CreateLogger(TrackingLogType.SendEmail);//充值活动刷新
            TrackingLoggerMng.CreateLogger(TrackingLogType.GetEmail);//充值活动刷新
            TrackingLoggerMng.CreateLogger(TrackingLogType.SoulBoneQuenching);//魂骨淬炼
            TrackingLoggerMng.CreateLogger(TrackingLogType.DungeonQueue);//副本阵容
            TrackingLoggerMng.CreateLogger(TrackingLogType.SUGGEST);// 吐槽

            //TrackingLoggerMng.CreateLogger(TrackingLogType.ENTERMAP);
            //TrackingLoggerMng.CreateLogger(TrackingLogType.QUITMAP);////
            //TrackingLoggerMng.CreateLogger(TrackingLogType.COMMENT);// 图鉴评论
            //TrackingLoggerMng.CreateLogger(TrackingLogType.BATTLE);// 战场
            //TrackingLoggerMng.CreateLogger(TrackingLogType.BATTLESTAT1V1);
            //TrackingLoggerMng.CreateLogger(TrackingLogType.BATTLESTAT2V2);
            //TrackingLoggerMng.CreateLogger(TrackingLogType.QUESTION);
        }

        public override void InitBILog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", mainId, subId);
            biLoggerManager = new BILoggerManager(GameConfig.LogServerKey, ServerType.ToString(), statLogPrefix, this);
            //biLoggerManager = new BILoggerManager(GameConfig.LogServerKey, statLogPrefix,this);
            BILoggerMng.CreateLogger(BILoggerType.LOGIN);//
            BILoggerMng.CreateLogger(BILoggerType.LOGOUT);//
            BILoggerMng.CreateLogger(BILoggerType.RECHARGE);//
            BILoggerMng.CreateLogger(BILoggerType.CONSUMECURRENCY);//
            BILoggerMng.CreateLogger(BILoggerType.OBTAINCURRENCY);//
            //BILoggerMng.CreateLogger(BILoggerType.REFRESH);//
            //BILoggerMng.CreateLogger(BILoggerType.ACTIVITY);//
            //BILoggerMng.CreateLogger(BILoggerType.TASK);//
            //BILoggerMng.CreateLogger(BILoggerType.CHECKPOINT);//
            //BILoggerMng.CreateLogger(BILoggerType.DEVELOP);//

            BILoggerMng.CreateLogger(BILoggerType.ITEMPRODUCE);//
            BILoggerMng.CreateLogger(BILoggerType.ITEMCONSUME);//
            BILoggerMng.CreateLogger(BILoggerType.SHOP);//
            //BILoggerMng.CreateLogger(BILoggerType.RECRUIT);//
            //BILoggerMng.CreateLogger(BILoggerType.RINGREPLACE);//
            //BILoggerMng.CreateLogger(BILoggerType.BONEREPLACE);//
            //BILoggerMng.CreateLogger(BILoggerType.EQUIPMENTREPLACE);//
            //BILoggerMng.CreateLogger(BILoggerType.EQUIPMENT);//

            //BILoggerMng.CreateLogger(BILoggerType.LIMITPACK);//
            //BILoggerMng.CreateLogger(BILoggerType.GODREPLACE);//
            //BILoggerMng.CreateLogger(BILoggerType.GOD);//
            //BILoggerMng.CreateLogger(BILoggerType.WISHINGWELL);//
            //BILoggerMng.CreateLogger(BILoggerType.TREASUREMAP);//

            //Nlog 初始化
            KomoeLogManager.SetServerId(mainId);

            BIXmlUpdate();
        }


        /// <summary>
        /// 初始化开服时间
        /// </summary>
        private void InitOpenServerTime()
        {
            Data serverListData = DataListManager.inst.GetData("ServerList", MainId);
            if (serverListData == null)
            {
                OpenServerTime = DateTime.MaxValue;
            }
            else
            {
                string time = serverListData.GetString("openTime");
                if (time == string.Empty)
                {
                    OpenServerTime = DateTime.MaxValue;
                }
                else
                {
                    OpenServerTime = DateTime.Parse(time);
                }
            }
            OpenServerDate = OpenServerTime.Date;

            PassCardLibrary.CheckPeriod(OpenServerDate);
        }

        //public void InitCreatePlayerConfig()
        //{
        //    CreatePlayerCurrencies.Clear();

        //    CreatePlayerConfig = DataListManager.inst.GetData("CreatePlayer", 1);
        //    DataList currenciesDataList = DataListManager.inst.GetDataList("Currencies");
        //    foreach (var item in currenciesDataList)
        //    {
        //        CreatePlayerCurrencies.Add(item.Value.Name, 0);
        //    }


        //    string taskIdString = CreatePlayerConfig.GetString("ActivityTaskIds");
        //    taskIdString = string.Format("{0}|{1}", CreatePlayerConfig.GetString("FirstTaskId"), taskIdString);
        //    TaskIds = taskIdString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

        //    int Vip = CreatePlayerConfig.GetInt("Vip");
        //    if (Vip < 0)
        //    {
        //        Vip = 0;
        //    }
        //    Data vipData = DataListManager.inst.GetData("VIP", Vip);
        //    if (vipData != null)
        //    {
        //        CreatePlayerCurrencies["CoinBuyFreq"] = vipData.GetInt("CoinBuyFreq");
        //        CreatePlayerCurrencies["PhysicalBuyFreq"] = vipData.GetInt("PhysicalBuyFreq");
        //        CreatePlayerCurrencies["OutstandingBuyFreq"] = vipData.GetInt("OutstandingBuyFreq");
        //        CreatePlayerCurrencies["JJCBuyFreq"] = vipData.GetInt("JJCBuyFreq");
        //        CreatePlayerCurrencies["UnrealBuyFreq"] = vipData.GetInt("UnrealBuyFreq");
        //        CreatePlayerCurrencies["PrayBuyFreq"] = vipData.GetInt("PrayBuyFreq");
        //        CreatePlayerCurrencies["VirtueValueBuyFreq"] = vipData.GetInt("VirtueValueBuyFreq");
        //        CreatePlayerCurrencies["WisdomValueBuyFreq"] = vipData.GetInt("WisdomValueBuyFreq");
        //        CreatePlayerCurrencies["WipeoutObtainFreq"] = vipData.GetInt("WipeoutObtainFreq");
        //        CreatePlayerCurrencies["HunterBuyFreq"] = vipData.GetInt("HunterBuyFreq");
        //        CreatePlayerCurrencies["CoinDungeonBuyFreq"] = vipData.GetInt("CoinDungeonBuyFreq");
        //        CreatePlayerCurrencies["ExpDungeonBuyFreq"] = vipData.GetInt("ExpDungeonBuyFreq");
        //        CreatePlayerCurrencies["TreasureDungeonBuyFreq"] = vipData.GetInt("TreasureDungeonBuyFreq");
        //    }
        //    else
        //    {
        //        CreatePlayerCurrencies["CoinBuyFreq"] = 2;
        //        CreatePlayerCurrencies["PhysicalBuyFreq"] = 1;
        //        CreatePlayerCurrencies["OutstandingBuyFreq"] = 0;
        //        CreatePlayerCurrencies["JJCBuyFreq"] = 1;
        //        CreatePlayerCurrencies["UnrealBuyFreq"] = 5;
        //        CreatePlayerCurrencies["PrayBuyFreq"] = 1;
        //        CreatePlayerCurrencies["VirtueValueBuyFreq"] = 0;
        //        CreatePlayerCurrencies["WisdomValueBuyFreq"] = 1;
        //        CreatePlayerCurrencies["WipeoutObtainFreq"] = 0;
        //        CreatePlayerCurrencies["HunterBuyFreq"] = 0;
        //        CreatePlayerCurrencies["CoinDungeonBuyFreq"] = 2;
        //        CreatePlayerCurrencies["ExpDungeonBuyFreq"] = 2;
        //        CreatePlayerCurrencies["TreasureDungeonBuyFreq"] = 2;
        //    }

        //    string currenciesString = CreatePlayerConfig.GetString("Currencies");
        //    List<string[]> currencyList = SplitString(currenciesString);
        //    foreach (var currency in currencyList)
        //    {
        //        CreatePlayerCurrencies[currency[0]] = int.Parse(currency[1]);
        //    }

        //    string itemsString = CreatePlayerConfig.GetString("Items");
        //    CreatePlayerItems = SplitString(itemsString);
        //}

        /// <summary>
        /// 初始化XML数据
        /// </summary>
        public override void InitData()
		{
            base.InitData();

            //初始化开服时间
            InitOpenServerTime();

            InitLibrarys();

            Log.Warn("zone main {0} sub {1} Init data loading done", mainId, subId);
        }

        /// <summary>
        /// 初始化地图和玩家管理
        /// </summary>
		public void InitBasicManager()
		{
            //初始化地图
			mapManager = new MapManager();
			mapManager.Init(this);
     
            //初始化玩家
            pcManager = new PcManager();
            pcManager.Init(this);

            //服务器等级
            //worldLevelManager = new WorldLevelManager();
            //worldLevelManager.Init(this);

            chatMng = new ChatManager(this);

            IntegralBossManager = new IntegralBossManager(this);

            crossBattleMng = new CrossBattleManager();

            contributionMng = new ContributionManager(this);

            ThemeBossMng = new ThemeBossManager(this);

            CarnivalBossMng = new CarnivalBossManager(this);

            CrossChallengeMng = new CrossChallengeManager();

            http163Helper.InitHttpClient(this);
            httpSensitiveHelper.InitHttpClient(this);

            spaceTimeTowerManager = new SpaceTimeTowerManager(this);
        }



        //public void InitCampCoin()
        //{
        //    QueryLoadCampCoins query = new QueryLoadCampCoins();
        //    GameDBPool.Call(query, ret =>
        //    {
        //        foreach(var kv in query.CampCoins)
        //        {
        //            RelationServer.InitGrain(kv.Key, kv.Value);
        //        }
        //    });
        //}


        public override void InitProtocol()
		{
            base.InitProtocol();
            Message.Zone.Protocol.ZR.ZRIdGenerator.GenerateId();
            Message.Zone.Protocol.ZM.ZMIdGenerator.GenerateId();
            Message.Zone.Protocol.ZGate.ZGateIdGenerator.GenerateId();
            Message.Zone.Protocol.ZG.ZGIdGenerator.GenerateId();
            Message.Zone.Protocol.ZBM.ZBMIdGenerator.GenerateId();
            Message.Global.Protocol.GBattle.GBattleIdGenerator.GenerateId();

            Message.Global.Protocol.GZ.GZIdGenerator.GenerateId();
            Message.Manager.Protocol.MZ.MZIdGenerator.GenerateId();
            Message.Relation.Protocol.RZ.RZIdGenerator.GenerateId();
            Message.Gate.Protocol.GateZ.GateZIdGenerator.GenerateId();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
             Message.BattleManager.Protocol.BMZ.BMZIdGenerator.GenerateId();
            Message.Battle.Protocol.BattleZ.BattleZIdGenerator.GenerateId();
		}


        public override void UpdateXml()
        {
            DoTaskStart(DoUpdateXml);
        }

        private void DoUpdateXml()
        {
            Log.WarnLine("zone main {0} sub {1}  XML update START", mainId, subId);

            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.Chnage(file);
                Thread.Sleep(10);
            }

            //初始化开服时间
            InitOpenServerTime();

            InitLibrarys();

            BIXmlUpdate();
            ////初始化礼包码文本
            //InitGiftCode();

            Log.WarnLine("zone main {0} sub {1} XML update END", mainId, subId);
        }

        public static void DoTaskStart(Action action)
        {
            //base.UpdateXml();
            var task = new System.Threading.Tasks.Task(() => action());
            task.Start();
        }

        public void UpdateServerXml()
        {
            string[] files = System.IO.Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "Server\\ServerList.xml", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.Chnage(file);
            }
            InitOpenServerTime();

            //抽卡
            DrawLibrary.Init(OpenServerTime);
            //活动
            ActivityLibrary.Init(OpenServerTime);
            //充值
            RechargeLibrary.Init(OpenServerTime);
        }

        public override void ProcessDBPostUpdate()
        {
            base.ProcessDBPostUpdate();
            http163Helper.Update();
            httpSensitiveHelper.Update();
        }

        public override void ProcessDBExceptionLog()
        {
            try
            {
                base.ProcessDBExceptionLog();

                Queue<string> queue1 = http163Helper.ResponseQueue.GetExceptionLogQueue();
                if (queue1 != null && queue1.Count != 0)
                {
                    Log.Error(queue1.Dequeue());
                }

                Queue<string> queue2 = httpSensitiveHelper.ResponseQueue.GetExceptionLogQueue();
                if (queue2 != null && queue2.Count != 0)
                {
                    Log.Error(queue2.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        private void BIXmlUpdate()
        {
            var biConfigData = DataListManager.inst.GetData("BILogConfig", 1);
            int fileSize = biConfigData.GetInt("FileSize");
            int refreshTime = biConfigData.GetInt("RefreshTime");
            BILoggerMng.UpdateXml(refreshTime, fileSize);
        }


        //public string GetCoinInfoSql(int uid)
        //{
        //    string insertSql = string.Empty;

        //    if (CreatePlayerCurrencies.Count > 0)
        //    {
        //        string resourceParameter0 = string.Empty;
        //        string resourceParameter1 = string.Empty;
        //        string resourceParameter2 = string.Empty;

        //        string counterParameter0 = string.Empty;
        //        string counterParameter1 = string.Empty;
        //        string counterParameter2 = string.Empty;

        //        foreach (var item in CreatePlayerCurrencies)
        //        {
        //            switch (item.Key)
        //            {
        //                case "Exp":
        //                case "Physical":
        //                case "GOLD":
        //                case "Diamond":
        //                case "Prestige":
        //                case "VirtueValue":
        //                case "WisdomValue":
        //                case "FashionCoin":
        //                case "WonderlandCoin":
        //                case "ChallangeCoin":
        //                case "CampCoin":
        //                case "FamilyContribution":
        //                case "FamilyFreezeContribution":
        //                case "UsedDiamond":
        //                case "DailyUsedDiamond":
        //                case "CurrentUsedDiamond":
        //                //case "ShareRedPacketPoints":
        //                    resourceParameter0 += string.Format(", `{0}`", item.Key);
        //                    resourceParameter1 += string.Format(", {0}", item.Value);
        //                    resourceParameter2 += string.Format(", `{0}` = {1}", item.Key, item.Value);
        //                    break;

        //                case "CoinBuyFreq":
        //                case "PhysicalBuyFreq":
        //                case "OutstandingBuyFreq":
        //                case "JJCBuyFreq":
        //                case "UnrealBuyFreq":
        //                case "PrayBuyFreq":
        //                case "VirtueValueBuyFreq":
        //                case "WisdomValueBuyFreq":
        //                case "FashionCoinBuyFreq":
        //                case "WipeoutObtainFreq":
        //                case "HunterBuyFreq":
        //                case "CoinDungeonBuyFreq":
        //                case "ExpDungeonBuyFreq":
        //                case "TreasureDungeonBuyFreq":
        //                case "CoinDungeonCount":
        //                case "ExpDungeonCount":
        //                case "TreasureDungeonCount":
        //                case "CampDailyQuestRefreshFreq":
        //                case "TeamNormalDungeonBuyFreq_1":
        //                case "TeamNormalDungeonBuyFreq_2":
        //                case "TeamNormalDungeonBuyFreq_3":
        //                case "TeamGuardDungeonBuyFreq":
        //                case "TeamTreasureDungeonBuyFreq":
        //                case "TeamNormalDungeonCount_1":
        //                case "TeamNormalDungeonCount_2":
        //                case "TeamNormalDungeonCount_3":
        //                case "TreasureTeamDungeonCount":
        //                case "TeamGuardDungeonCount":
        //                case "TeamDefendDungeonCount":
        //                case "TeamTreasureDungeonCount":
        //                case "TeamBossDungeonCount":
        //                case "PVPRobCount":
        //                case "PVPLoseCount":
        //                case "CampVoteCount":
        //                case "CampRunforCount":
        //                case "CampWorship":
        //                case "CampDiamondWorship":
        //                case "CampCelebrity":
        //                case "FamilyBlessDiamond":
        //                case "FamilyBlessGold":
        //                case "ActivityTreasureCount":
        //                case "PVPExp":
        //                case "FamilyBossCount":
        //                case "FamilyBossBuyCount":
        //                case "PVPExpTime":
        //                case "FamilyWarObserverReward":
        //                case "FamilyWarReward":
        //                case "LoverGiftBagCount":
        //                case "LoverWeddingInvitationCount":
        //                    counterParameter0 += string.Format(", `{0}`", item.Key);
        //                    counterParameter1 += string.Format(", {0}", item.Value);
        //                    counterParameter2 += string.Format(", `{0}` = {1}", item.Key, item.Value);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        string resourceSql = SetSqlString(uid, "character_resource", resourceParameter0, resourceParameter1, resourceParameter2);
        //        string counterSql = SetSqlString(uid, "game_counter", counterParameter0, counterParameter1, counterParameter2);

        //        insertSql = resourceSql + counterSql;
        //    }
        //    return insertSql;
        //}

        //private string SetSqlString(int uid, string tableName, string parameter0, string parameter1, string parameter2)
        //{
        //    string sqlBase = @"	INSERT INTO `{0}` (`pc_uid` {1}) VALUES ({2} {3}) ON DUPLICATE KEY UPDATE {4};";

        //    string resourceSql = string.Empty;
        //    if (!string.IsNullOrEmpty(parameter0))
        //    {
        //        parameter2 = parameter2.Substring(1);
        //        resourceSql = string.Format(sqlBase, tableName, parameter0, uid, parameter1, parameter2);
        //    }
        //    return resourceSql;
        //}

        //public int GetExpPunish(int level, int standardLv)
        //{
        //    foreach (var punishInfo in expLvPunish)
        //    {
        //        if (level <= standardLv + punishInfo[0])
        //        {
        //            return punishInfo[1];
        //        }
        //    }
        //    return 0;
        //}

        //public void InitRevivePolicy()
        //{
        //    DataList dataList = DataListManager.inst.GetDataList("Revive");
        //    foreach (var dataInfo in dataList)
        //    {
        //        ReviveModel model = new ReviveModel();
        //        Data data = dataInfo.Value;
        //        model.ID = data.ID;
        //        string[] timeArrList = data.GetString("Time").Split('|');
        //        foreach (var time in timeArrList)
        //        {
        //            model.Time.Add(int.Parse(time));
        //        }
        //        model.Limit = data.GetInt("Limit");
        //        model.Cost = data.GetInt("Cost");
        //        model.CanHome = data.GetBoolean("IsHome");
        //        model.CanEntrance = data.GetBoolean("IsEntrance");
        //        model.CanImmediately = data.GetBoolean("IsImmediately");
        //        ReviveModelPolicy.Add(data.ID, model);
        //    }
        //}

	}
}

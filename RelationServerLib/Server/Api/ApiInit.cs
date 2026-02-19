using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using ServerFrame;
using ServerLogger;
using ServerLogger.KomoeLog;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RelationServerLib
{

    partial class RelationServerApi
    {
        public UidManager UID = new UidManager();

        private int maxCuildUid = 0;
        public int MaxGuildUid
        {
            get { return maxCuildUid; }
            set { maxCuildUid = value; }
        }

        public DateTime OpenServerDate { get; set; }
        public DateTime OpenServerTime { get; set; }

        //private Dictionary<int, List<MSG_RZ_SEND_EMAILS>> sendEmailMsgs = new Dictionary<int, List<MSG_RZ_SEND_EMAILS>>();
        //public Dictionary<int, List<MSG_RZ_SEND_EMAILS>> SendEmailMsgs
        //{
        //    get { return sendEmailMsgs; }
        //    set { sendEmailMsgs = value; }
        //}

        public ZoneServerManager ZoneManager
        { get { return (ZoneServerManager)serverManagerProxy.GetFrontendServerManager(ServerType.ZoneServer); } }

        public BackendServer ManagerServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.ManagerServer, MainId); } }

        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public BackendServer CrossServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.CrossServer, ClusterId); } }

        private ConfigReloadManager configReloadMng;
        public ConfigReloadManager ConfigReloadMng
        { get { return configReloadMng; } }

        private EmailManager emailMng;
        public EmailManager EmailMng
        { get { return emailMng; } }

        private CampRankManager campRankMng;
        public CampRankManager CampRankMng
        { get { return campRankMng; } }

        private ArenaManager arenaMng;
        public ArenaManager ArenaMng
        { get { return arenaMng; } }

        private CrossBattleManager crossBattleMng;
        public CrossBattleManager CrossBattleMng
        { get { return crossBattleMng; } }

        private CrossGuessingManager crossGuessingMng;
        public CrossGuessingManager CrossGuessingMng
        { get { return crossGuessingMng; } }

        public CampBattleReward campRewardMng;
        public CampBattleReward CampRewardMng
        { get { return campRewardMng; } }

        private CampActivityManager campActivityMng;
        public CampActivityManager CampActivityMng
        { get { return campActivityMng; } }

        private RedisPlayerInfoManager rPlayerInfoMng;
        public RedisPlayerInfoManager RPlayerInfoMng
        { get { return rPlayerInfoMng; } }

        private ContributionManager contributionMng;
        public ContributionManager ContributionMng
        { get { return contributionMng; } }

        private ThemeBossManager themeBossMng;
        public ThemeBossManager ThemeBossMng
        {
            get { return themeBossMng; }
        }

        private CrossChallengeManager crossChallengeMng;
        public CrossChallengeManager CrossChallengeMng => crossChallengeMng;

        private CrossChallengeGuessingManager crossChallengeGuessingMng;
        public CrossChallengeGuessingManager CrossChallengeGuessingMng => crossChallengeGuessingMng;

        private WarehouseManager warehouseMng;
        public WarehouseManager WarehouseMng { get { return warehouseMng; } }
        
        private SpaceTimeTowerManager spaceTimeTowerManager;
        public  SpaceTimeTowerManager SpaceTimeTowerManager
        {
            get { return spaceTimeTowerManager; }
        }

        /// <summary>
        /// 所有礼包码
        /// </summary>
        private static Dictionary<string, int> giftCodeList = new Dictionary<string, int>();
        public static Dictionary<string, int> GiftCodeList { get { return giftCodeList; } }

        public override void InitData()
        {
            base.InitData();
            //初始化开服时间
            InitOpenServerTime();
            InitLibrarys();
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Relation.Protocol.RG.RGIdGenerator.GenerateId();
            Message.Relation.Protocol.RM.RMIdGenerator.GenerateId();
            Message.Relation.Protocol.RZ.RZIdGenerator.GenerateId();
            Message.Zone.Protocol.ZR.ZRIdGenerator.GenerateId();
            Message.Manager.Protocol.MR.MRIdGenerator.GenerateId();
            Message.Global.Protocol.GR.GRIdGenerator.GenerateId();
            Message.Gate.Protocol.GateZ.GateZIdGenerator.GenerateId();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
            Message.Relation.Protocol.RR.RRIdGenerator.GenerateId();
            Message.Relation.Protocol.RC.RCIdGenerator.GenerateId();
            Message.Corss.Protocol.CorssR.CorssRIdGenerator.GenerateId();
        }

        public override void InitDB()
        {
            base.InitDB();
            //if (WatchDog)
            //{
            //    //TODO:获取最大公会id
            //    GameDBPool.Call(new QueryMaxGuildId(), ret =>
            //    {
            //        int max = (int)ret;
            //        MaxFid = MaxFid > max ? MaxFid : max;
            //        Log.Write($"faimly max  fid {max}");
            //    });
            //}
        }


        public void InitConfigLoadManager()
        {
            configReloadMng = new ConfigReloadManager();
        }

        public void InitEmailManager()
        {
            emailMng = new EmailManager(this);
        }

        public void InitCampManager()
        {
            campRankMng = new CampRankManager(this);
        }

        private void InitCampActivityManager()
        {
            campActivityMng = new CampActivityManager(this, mainId);
            campRewardMng = new CampBattleReward(this);
        }

        public void InitArenaManager()
        {
            arenaMng = new ArenaManager(this);
        }

        public void InitCrossBattleManager()
        {
            crossBattleMng = new CrossBattleManager(this);
            crossGuessingMng = new CrossGuessingManager(this);
        }

        public void InitCrossChallengeManager()
        {
            crossChallengeMng = new CrossChallengeManager(this);
            crossChallengeGuessingMng = new CrossChallengeGuessingManager(this);
        }

        public void InitRedisPlayerInfoManager()
        {
            rPlayerInfoMng = new RedisPlayerInfoManager(this);
        }

        public void InitContributionManager()
        {
            contributionMng = new ContributionManager(this);
        }

        public void InitThemeBossManager()
        {
            themeBossMng = new ThemeBossManager(this);
        }

        public void InitWarehouseManager()
        {
            warehouseMng = new WarehouseManager(this);
        }

        public void InitSpaceTimeTowerManager()
        {
            spaceTimeTowerManager = new SpaceTimeTowerManager(this);
        }
        
        public override void UpdateXml()
        {
            //base.UpdateXml();
            InitData();
            campRankMng.Init();
            configReloadMng.LoadConfig();
            RankMng.RankReward.Init();
            BIXmlUpdate();

            //初始化礼包码文本
            DoTaskStart(InitGiftCode);

            TaskTimerMng.Stop();
            ZoneManager.InitTimerManager(RelationServerApi.now);
            ZoneManager.InitRechargeTimerManager(RelationServerApi.now, 0);
        }

        public static void DoTaskStart(Action action)
        {
            //base.UpdateXml();
            var task = new System.Threading.Tasks.Task(() => action());
            task.Start();
        }


        public int GetMaxFid()
        {
            ++MaxFid;
            return MaxFid;
        }

        public void InitOpenServerTime(bool isUpdate = false)
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
            if (isUpdate)
            {
                CampActivityMng.InitPhase(true);
            }

        }

        public RankManager RankMng;

        public void InitRankManager()
        {
            RankMng = new RankManager(this);
            RankMng.InitRanks();
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

            //biLoggerManager = new BILoggerManager(logServerKey, statLogPrefix, this);
            biLoggerManager = new BILoggerManager(logServerKey, ServerType.ToString(), statLogPrefix, this);
            //BILoggerMng.CreateLogger(BILoggerType.ONLINE);//

            BIXmlUpdate();
        }

        private void BIXmlUpdate()
        {
            var biConfigData = DataListManager.inst.GetData("BILogConfig", 1);
            int fileSize = biConfigData.GetInt("FileSize");
            int refreshTime = biConfigData.GetInt("RefreshTime");
            BILoggerMng.UpdateXml(refreshTime, fileSize);
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
            TrackingLoggerMng.CreateLogger(TrackingLogType.RANK);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.RELATIONRANK);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.TIMER);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.RECHARGETIMER);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.SendEmail);//充值活动刷新
            TrackingLoggerMng.CreateLogger(TrackingLogType.Warehouse);
        }

        ////#mainid|ranktype|rank|score|uid
        //public void TrackRankLog(RankType rankType,int rank ,int score,int uid)
        //{
        //    string log_new = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", mainId, (int)rankType, rank, score,uid, now.ToString("yyyy-MM-dd HH:mm:ss"));
        //    TrackingLoggerMng.Write(log_new, TrackingLogType.RANK);
        //}

        /*
       * 排行榜	rank_flow	成就完成时触发	
           achieve_id	string	排行榜对应ID	
           achieve_type	int	排行榜类型	0-战力，1-猎杀魂兽，2-斗魂之路，3-秘境，4-大斗魂场，5-荣耀魂师大赛6-神祗贡献度
           rank_before	int	变化前排名	例如: 1，2，未上榜
           rank_after	int	变化后排名	例如: 1，2，未上榜
           award	array	如有，奖励道具ID及数量	获得的道具ID，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
           achieve_level	string	对应的进度内容	例如: 战力6509k;难度系数 539; 【五怪登阶】19-23; 贡献值: 10270等
           cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
       */
        public void KomoeEventLogRankFlow(int uid, RankType achieve_type, int rank_before, int rank_after,
            int achieve_level, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();
            //infDic.Add("b_udid", client.DeviceId);
            //infDic.Add("b_sdk_udid", client.SDKUuid);
            //infDic.Add("b_sdk_uid", client.AccountName);
            //infDic.Add("b_account_id", client.AccountName);
            //infDic.Add("b_tour_indicator", client.tour);
            //infDic.Add("b_channel_id", client.channelId);
            //infDic.Add("b_version", AuthMng.Version);
            //infDic.Add("level", 1);
            //infDic.Add("role_name", "");

            infDic.Add("b_game_base_id", KomoeLogConfig.GameBaseId);
            infDic.Add("b_game_id", KomoeLogConfig.GameId);
            infDic.Add("b_platform", KomoeLogConfig.Platform);
            infDic.Add("b_zone_id", MainId);
            infDic.Add("b_role_id", uid);

            string logId = $"{KomoeLogConfig.GameBaseId}-{KomoeLogEventType.rank_flow}-{uid}-{Timestamp.GetUnixTimeStampSeconds(Now())}-{rank_after}";
            infDic.Add("b_log_id", logId);
            infDic.Add("b_eventname", KomoeLogEventType.rank_flow.ToString());
            infDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(Now()));
            infDic.Add("b_datetime", Now().ToString("yyyy-MM-dd HH:mm:ss"));

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("achieve_id", (int)achieve_type);
            properties.Add("achieve_type", achieve_type.ToString());
            properties.Add("rank_before", rank_before);
            properties.Add("rank_after", rank_after);
            properties.Add("achieve_level", achieve_level.ToString());
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * 排行榜快照	每4小时上报一条，22点额外上报一条	
         * 
         *  snapshot_name	快照名字	记：rank_list_snapshot	必填	string
            snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
            snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
            b_game_id	游戏id	一款游戏的ID	必填	int
            b_platform	平台名称	统一：ios|android|windows	必填	string
            b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
            b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
            b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
            b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
            b_sdk_udid	用户硬件设备号	b服sdk udid，客户端sdk登陆事件接口获取，32位通用唯一识别码	必填	string
            b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
            b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
            role_name	角色名称	例如：黄昏蔷薇行者	必填	string

            rank_type	排行榜类型	0-战力，    1-猎杀魂兽，    2-斗魂之路，    3-秘境，    4-大斗魂场，    5-荣耀魂师大赛    6-神祗贡献度    7-阵营建设贡献榜	必填	string
            rank_id	排行榜ID	排行榜类ID (对应榜单下的明细分类，根据配置显示)	必填	string
            rank_num	玩家的排行榜排名	排名	必填	int
            rank_value	排行榜分值	排行榜分值	必填	int
            rank_sub_id	次级排行榜ID	排行榜类ID (对应榜单下的明细分类，根据配置显示)	必填	string
            rank_sub_num	玩家的排行榜排名	排名	必填	int
            rank_subvalue	排行榜次级分值		必填	int
            user_param	自定义用户属性	自定义参数	选填	string
        */
        /// <summary>
        /// 排行榜快照
        /// </summary>
        public void KomoeUserLogUserListSnapshot(int uid, RankType type, int rank, int score)
        {
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "rank_list_snapshot");
            infDic.Add("snapshot_date", Now().ToString("yyyyMMdd"));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(Now()));
            infDic.Add("b_game_id", KomoeLogConfig.GameId);
            infDic.Add("b_platform", KomoeLogConfig.Platform);
            infDic.Add("b_zone_id", MainId);
            infDic.Add("b_role_id", uid);
            //infDic.Add("b_channel_id", ChannelId);
            //infDic.Add("b_udid", DeviceId);
            //infDic.Add("b_sdk_udid", SDKUuid);
            //infDic.Add("b_sdk_uid", AccountName);
            //infDic.Add("b_account_id", AccountName);
            //infDic.Add("role_name", Name);

            infDic.Add("rank_type", type.ToString());
            infDic.Add("rank_id", (int)type);
            infDic.Add("rank_num", rank);
            infDic.Add("rank_value", score);
            //infDic.Add("rank_sub_id", item.PileNum);
            //infDic.Add("rank_sub_num", item.PileNum);
            //infDic.Add("rank_subvalue", item.PileNum);
            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }


        public void WriteToCross<T>(T msg, int uid = 0) where T : Google.Protobuf.IMessage
        {
            if (CrossServer != null)
            {
                CrossServer.Write(msg, uid);
            }
        }

        Dictionary<string, int> GiftTxts = new Dictionary<string, int>();
        public void InitGiftCode()
        {
            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML/Gift/GiftTxt"), "*.txt", SearchOption.AllDirectories);
            if (files.Length < 1)
            {
                Log.Warn("zone main {0} sub {1} Init Gift Code failed, Check it!", mainId, subId);
                return;
            }
            else
            {
                Log.WarnLine("zone main {0} sub {1}  Init Gift Code START", mainId, subId);
            }
            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!GiftTxts.ContainsKey(fileInfo.Name))
                {
                    GiftTxts.Add(fileInfo.Name, 0);
                }
                else
                {
                    continue;
                }
                ReadLineFile(file);
                //FileStream fsRead = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                //int fsLen = (int)fsRead.Length;
                //byte[] heByte = new byte[fsLen];
                //fsRead.Read(heByte, 0, heByte.Length);
                //fsRead.Close();
                //string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                //string[] giftCodes = StringSplit.GetArray("|", myStr);
                //foreach (var item in giftCodes)
                //{
                //    string code = item.Trim();
                //    //统一转换成大写
                //    code = code.ToUpper();
                //    string key = code.Substring(0, 4);
                //    List<string> list;
                //    if (giftCodeList.TryGetValue(key, out list))
                //    {
                //        if (!list.Contains(code))
                //        {
                //            list.Add(code);
                //        }
                //    }
                //    else
                //    {
                //        list = new List<string>();
                //        list.Add(code);
                //        giftCodeList.Add(key, list);
                //    }
                //}
            }
            Log.Warn("zone main {0} sub {1} Init Gift Code loading done", mainId, subId);
        }

        public Dictionary<string, string> ReadLineFile(string filePath)
        {
            Dictionary<string, string> contentDictionary = new Dictionary<string, string>();

            FileStream fileStream = null;
            StreamReader streamReader = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                streamReader = new StreamReader(fileStream, Encoding.Default);
                fileStream.Seek(0, SeekOrigin.Begin);
                string content = streamReader.ReadLine();
                while (content != null)
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        giftCodeList[content.Trim()] = 0;
                    }
                    content = streamReader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("zone main {0} sub {1} Init Gift Code Error: {3}", mainId, subId, ex);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                }
            }
            return contentDictionary;
        }
    }
}

using System;
using System.Collections.Generic;
using DataProperty;
using Logger;
using ServerShared;
using ServerFrame;
using EnumerateUtility;
using ServerLogger;
using ServerLogger.KomoeLog;
using CommonUtility;

namespace CrossServerLib
{
    partial class CrossServerApi
    {
        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public RelationServerManager RelationManager
        { get { return (RelationServerManager)serverManagerProxy.GetFrontendServerManager(ServerType.RelationServer); } }

        public RankManager RankMng { get; set; }
        public PlayerInfoManager PlayerInfoMng { get; set; }
        public HidderWeaponManager HidderWeaponMng { get; set; }
        public SeaTreasureManager SeaTreasureMng { get; set; }
        public GardenManager GardenMng { get; set; }
        public CrossBossManager CrossBossMng { get; set; }
        public NotesManager NotesMng { get; set; }
        public DivineLoveManager DivineLoveMng { get; set; }
        public IslandHighManager IslandHighMng { get; set; }
        public StoneWallManager StoneWallMng { get; set; }
        public CarnivalBossManager CarnivalBossMng { get; set; }
        public RouletteManager RouletteManager { get; set; }
        public CanoeManager CanoeMng { get; set; }
        public MidAutumnManager MidAutumnMng { get; set; }
        public ThemeFireworkManager ThemeFireworkMng { get; set; }
        public NineTestManager NineTestMng { get; set; }

        public override void InitConfig()
        {
            base.InitConfig();
        }

        public override void InitData()
        {
            base.InitData();
            InitLibrarys();
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Global.Protocol.GB.GBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BG.BGIdGenerator.GenerateId();
            Message.Barrack.Protocol.BarrackC.BarrackCIdGenerator.GenerateId();
            Message.Client.Protocol.CBarrack.CBarrackIdGenerator.GenerateId();
            Message.Manager.Protocol.MB.MBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BM.BMIdGenerator.GenerateId();
            Message.Gate.Protocol.GateB.GateBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BGate.BGateIdGenerator.GenerateId();
            Message.Corss.Protocol.CorssG.CorssGIdGenerator.GenerateId();
            Message.Corss.Protocol.CorssR.CorssRIdGenerator.GenerateId();
            Message.Relation.Protocol.RC.RCIdGenerator.GenerateId();
            Message.Relation.Protocol.RZ.RZIdGenerator.GenerateId();
            Message.Zone.Protocol.ZR.ZRIdGenerator.GenerateId();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
            Message.Global.Protocol.GCross.GCrossIdGenerator.GenerateId();
        }

        public override void UpdateXml()
        {
            InitData();

            TaskTimerMng.Stop();
            RelationManager.CrossBattleMng.Init();
            RelationManager.CrossChallengeMng.Init();
            RelationManager.InitTimerManager(Now(), 0);
            InitManagers();

        }

        private void InitManagers()
        {
            InitPlayerInfoManager();
            InitRankManager();
            InitActivityManager();
            InitCrossBossManager();
            InitNotesManager();
        }

        public void InitRankManager()
        {
            RankMng = new RankManager(this);
            RankMng.Init();
        }

        public void InitPlayerInfoManager()
        {
            PlayerInfoMng = new PlayerInfoManager(this);
        }

        public void InitActivityManager()
        {
            HidderWeaponMng = new HidderWeaponManager(this);

            SeaTreasureMng = new SeaTreasureManager(this);

            GardenMng = new GardenManager(this);

            DivineLoveMng = new DivineLoveManager(this);

            IslandHighMng = new IslandHighManager(this);

            StoneWallMng = new StoneWallManager(this);

            CarnivalBossMng = new CarnivalBossManager(this);

            RouletteManager = new RouletteManager(this);

            CanoeMng = new CanoeManager(this);

            MidAutumnMng = new MidAutumnManager(this);

            ThemeFireworkMng = new ThemeFireworkManager(this);

            NineTestMng = new NineTestManager(this);
        }

        public void InitCrossBossManager()
        {
            CrossBossMng = new CrossBossManager(this);
        }

        public void InitNotesManager()
        {
            NotesMng = new NotesManager(this);
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
            trackingLoggerManager = new TrackingLoggerManager(logServerKey, statLogPrefix);

            TrackingLoggerMng.CreateLogger(TrackingLogType.CROSSRANK);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.TIMER);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.RANKEMAIL);//
            TrackingLoggerMng.CreateLogger(TrackingLogType.SendEmail);//
        }

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
        public void KomoeEventLogRankFlow(int uid, int mainId, RankType achieve_type, int rank_before, int rank_after,
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
            infDic.Add("b_zone_id", mainId);
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
        public void KomoeUserLogUserListSnapshot(int uid, int mainId, RankType type, int rank, int score, int groupId)
        {
            // LOG 记录开关 
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "rank_list_snapshot");
            infDic.Add("snapshot_date", Now().ToString("yyyyMMdd"));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(Now()));
            infDic.Add("b_game_id", KomoeLogConfig.GameId);
            infDic.Add("b_platform", KomoeLogConfig.Platform);
            infDic.Add("b_zone_id", mainId);
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
            infDic.Add("user_param", groupId.ToString());
            KomoeLogManager.UserWrite(infDic);
        }
    }
}

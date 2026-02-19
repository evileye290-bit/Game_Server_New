using System;
using System.Collections.Generic;

using ServerShared;
using Logger;
using DBUtility;
using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZR;
using Message.Zone.Protocol.ZM;
using ScriptFunctions;
using RedisUtility;
using ServerFrame;
using System.Threading;

namespace ZoneServerLib
{
    public partial class ZoneServerApi : BaseApi
    {
        private MapManager mapManager;
        internal MapManager MapManager
        { get { return mapManager; } }

        private PcManager pcManager;
        internal PcManager PCManager
        { get { return pcManager; } }

        //public PopRankManager PopRankMng;

        private WorldLevelManager worldLevelManager;
        public WorldLevelManager WorldLevelManager 
        { get { return worldLevelManager; } }

        /// <summary>
        /// 生产UID
        /// </summary>
        public UidManager UID = new UidManager();

        //private Dictionary<int, ReviveModel> reviveModelPolicy = new Dictionary<int, ReviveModel>();
        //public Dictionary<int, ReviveModel> ReviveModelPolicy
        //{
        //    get { return reviveModelPolicy; }
        //}

        //private string[] taskIds;

        //public string[] TaskIds
        //{
        //    get { return taskIds; }
        //    set { taskIds = value; }
        //}

        public Dictionary<string, int> RecvMsgCount = new Dictionary<string, int>();


        public Http163CheckerHelper http163Helper = new Http163CheckerHelper();
        public HttpSensitiveCheckerHelper httpSensitiveHelper = new HttpSensitiveCheckerHelper();

        public override void Init(string[] args)
        {
            base.Init(args);

            //初始化Server Config
            InitConfig();


            //初始化屏蔽字
            InitWordChecker();

            //InitCampBattle();
            //InitCampCoin();
            InitSensitiveWord();

            InitNameChecker();

            // 初始化地图和玩家管理
            InitBasicManager();

            ////初始化礼包码文本
            //DoTaskStart(InitGiftCode);
            //BindEmailDatas();

            //Test();

            //初始化充值返利
            DoTaskStart(InitRechargeRebate);

            InitDone();
        }

        public void Test()
        {
            //int time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100101, 1, 1, time, 101));
            //GameRedis.Call(new OperateRemoveCrossRankInfos(100102, 1, 1, 1558985928));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100104, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100104, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100105, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100105, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100108, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100108, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100107, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100107, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100106, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100106, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100103, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100103, 1, 1, time));
            //Thread.Sleep(1000);
            //time = CrossBattleLibrary.GetCrossTimeKey(DateTime.Now);
            //Redis.Call(new OperateUpdateCrossRankInfos(100102, 1, 1, time, 101));
            //Redis.Call(new OperateRemoveCrossRankInfos(100102, 1, 1, time));



            //for (int i = 100101; i < 1001030; i++)
            //{
            //    int rand = NewRAND.Next(100, 300);
            //    Redis.Call(new OperateUpdateCrossRankInfos(i, 1, 101));
            //}

            //获取赛季排行榜
            OperateGetCrossRankInfosByRank operate = new OperateGetCrossRankInfosByRank(1, 1, 0, 10);
            GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Characters == null)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Log.Error("LoadRankInfoFromRedis execute OperateGetCrossRankInfos fail: redis data error!");
                    return;
                }
            });

        }
        public void InitLibrarys()
        {
            ScriptManager.Init(PathExt.FullPathFromServer("Script"));

            GameConfig.InitGameCongfig();
            LiftConfig.InitLiftCongfig();
            TriggerInMapLibrary.Init();
            MonsterGenLibrary.Init();
            MapLibrary.Init();
            //时间触发器
            TimingLibrary.BindTimingData();
            //任务
            TaskLibrary.BindTaskInfo();
        
            QuestionnaireLibrary.BindQuestionnaireInfo();
            TitleLibrary.Init();
            //充值
            //RechargeLibrary.BindDatas();
            ZoneLibrary.BindZonePoint();
            FamilyLibrary.Init();
            //邮件
            EmailLibrary.BindEmailDatas();
            ShopLibrary.Init();
            CommonShopLibrary.Init();
            LimitLibrary.BindDatas();
            FriendLib.LoadDatas();
            BrothersLib.LoadDatas();
            //ShowLibrary.BindData();
            ChatLibrary.BindData();

            SkillLibrary.Init();
            SkillEffectEnhancePolicyLibrary.Init();
            BuffLibrary.Init();
            TriggerCreatedBySkillLibrary.Init();
            TriggerInMonsterLibrary.Init();
            TriggerCreatedBySoulRingLibrary.Init();
            TriggerCreatedBySoulBoneLibrary.Init();
            TriggerInHeroLibrary.Init();
            TriggerCreatedByTowerLibrary.Init();
            TriggerCreatedByGodLibrary.Init();
            TriggerInPetLibrary.Init();
            TriggerCreatedByGuideSoulItemLibrary.Init();
            MarkLibrary.Init();

            NatureLibrary.Init();
            SoulBoneLibrary.Init();
            BagLibrary.Init();
            EquipLibrary.Init();
            SoulRingLibrary.Init();
            SoulSkillLibrary.Init();
            //伙伴
            HeroLibrary.Init();
            MonsterLibrary.Init();
            PetLibrary.Init();

            CharacterLibrary.Init();
            //CharacterInitLibrary.Init();

            CampStarsLibrary.Init();
            CampLibrary.Init();
            CampGatherLibrary.Init();
            DungeonLibrary.Init();
            DelegationLibrary.Init();
            HuntingLibrary.Init();
            TeamLibrary.Init();
            IntegralBossLibrary.Init();
            HelpRewardLibrary.Init();
            //npc要在map后初始化
            NPCLibrary.Init();
            CounterLibrary.Init();
            CurrenciesLibrary.Init();
            MixSkillLibrary.Init();

            //竞技场
            ArenaLibrary.Init();

            //机器人
            RobotLibrary.Init();

            //秘境
            SecretAreaLibrary.Init();

            //通行证
            PassCardLibrary.Init();

            //抽卡
            DrawLibrary.Init(OpenServerTime);
            //活动
            ActivityLibrary.Init(OpenServerTime);
            //章节
            WorldLevelLibrary.Init();
            ChapterLibrary.Init();

            //成神之路
            GodPathLibrary.Init();

            //福利
            WelfareLibrary.Init();

            //许愿池
            WishPoolLibrary.Init();
            //跨服
            CrossBattleLibrary.Init();

            //充值
            RechargeLibrary.Init(OpenServerTime);
            //阵营建设
            CampBuildLibrary.Init();
            //爬塔
            TowerLibrary.Init();
            //阵营战
            CampActivityLibrary.Init();
            CampBattleLibrary.Init();
            RankLibrary.Init();
            //传送
            TransferMapLibrary.Init();
            //挂机
            OnhookLibrary.Init();
            //推图
            PushFigureLibrary.Init();

            //角色相关
            CharacterInitLibrary.Init();

            //武魂共共鳴    
            WuhunResonanceConfig.Init();
            //掉落
            RewardDropLibrary.Init();

            //礼包
            GiftLibrary.Init();

            //领域
            DomainLibrary.Init();

            //成神
            GodHeroLibrary.Init();

            //玩家行为检测
            ActionLibrary.Init(Now());

            //挖宝
            ShovelTreasureLibrary.Init();
            //贡献
            ContributionLibrary.Init();
            //战斗力计算
            BattlePowerLibrary.Init();
            //主题通行证
            ThemePassLibrary.Init();
            //暗器
            HidderWeaponLibrary.Init();

            VideoLibrary.Init();
            //主题Boss
            ThemeBossLibrary.Init();

            //163checkerconfig
            http163Helper.InitConfigData();
            //Sensitivecheckerconfig
            httpSensitiveHelper.InitConfigData();
            //跨服Boss
            CrossBossLibrary.Init();
            //幽香花园
            GardenLibrary.Init();
            //乾坤问情
            DivineLoveLibrary.Init();
            //海岛登高
            IslandHighLibrary.Init();
            //三叉戟
            TridentLibrary.Init();
            //端午活动
            DragonBoatLibrary.Init();
            //昊天石壁
            StoneWallLibrary.Init();
            //海岛挑战
            IslandChallengeLibrary.Init();
            //嘉年华Boss
            CarnivalBossLibrary.Init();
            //嘉年华
            CarnivalLibrary.Init();
            //漫游记
            TravelLibrary.Init();
            //暗器
            HiddenWeaponLibrary.Init();
            //史莱克邀约
            ShrekInvitationLibrary.Init();
            //转盘
            RouletteLibrary.Init();
            //皮划艇
            CanoeLibrary.Init();
            //魂师挑战
            CrossChallengeLibrary.Init();
            //中秋
            MidAutumnLibrary.Init();
            //主题烟花
            ThemeFireworkLibrary.Init();
            //九考试炼
            NineTestLibrary.Init();
            //仓库
            WarehouseLibrary.Init();
            //学院
            SchoolLibrary.Init();
            //玄天宝箱
            XuanBoxLibrary.Init();
            //九笼祈愿
            WishLanternLibrary.Init();

            DaysRechargeLibrary.Init();
            //史莱克乐园
            ShreklandLibrary.Init();
            
            //百兽时空爬塔玩法
            SpaceTimeTowerLibrary.Init();
            //魔鬼训练
            DevilTrainingLibrary.Init();
            
            //神域赐福
            DomainBenedictionLibrary.Init();
            
            //漂流探宝
            DriftExploreLibrary.Init();

        }

        public override void InitDone()
        {
            base.InitDone();
        }

        public override void SpecUpdate(double dt)
        {
            TimerManager.Instance.Update();
            BILoggerMng.CheckNewLogFile(now);
            mapManager.Update(dt);
            pcManager.Update(dt);
            chatMng.Update(dt);
            IntegralBossManager.Update();
            ThemeBossMng.Update();
            CarnivalBossMng.Update();
            //worldLevelManager.Update();
            UpdateOldPlayerRebateList();
            UpdateChangeNamePlayerList();

            RecordFrameInfo(dt);
            UID.ConvertTimestamp();
        }

        private void UpdateOldPlayerRebateList()
        {
            lock (oldPlayerRebateList)
            {
                foreach (var data in oldPlayerRebateList)
                {
                    try
                    {
                        PlayerChar pc = PCManager.FindPc(data.Key);
                        if (pc == null)
                        {
                            pc = PCManager.FindOfflinePc(data.Key);
                            if (pc == null)
                            {
                                // 正在登陆过程中离线，不能得到奖励
                                Log.Warn("player {0} old player rebate not find pc", data.Key);
                                continue;
                            }
                        }

                        List<int> emails = new List<int>();
                        string emailidString = data.Value.GetString("EmailID");
                        string[] emialIds = StringSplit.GetArray("|", emailidString);
                        foreach (var emailId in emialIds)
                        {
                            emails.Add(int.Parse(emailId));
                        }
                        int money = data.Value.GetInt("Money");
                        //pc.OldPlayerRebate(money, emails);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                oldPlayerRebateList.Clear();
            }
        }

        private void UpdateChangeNamePlayerList()
        {
            lock (ChangeNamePlayerList)
            {
                foreach (var item in ChangeNamePlayerList)
                {
                    try
                    {
                        PlayerChar player = pcManager.FindPc(item.Uid);
                        if (player == null)
                        {
                            pcManager.PcOfflineList.TryGetValue(item.Uid, out player);
                        }
                        if (player != null)
                        {
                            if (item.Result == 1)
                            {
                                // 防止数据库卡 导致钻石变负数 秒变超级大R
                                int diamond = player.Currencies[CurrenciesType.diamond];
                                player.Currencies[CurrenciesType.diamond] = Math.Max(diamond - item.Diamond, 0);
                                //player.Packet_Info.name = item.NewName;
                            }

                            //PKS_ZC_CHANGE_NAME notify = new PKS_ZC_CHANGE_NAME();
                            //notify.Result = item.Result;
                            //notify.name = item.NewName;
                            //notify.costDiamond = item.Diamond;
                            //player.Write(notify);
                        }

                        if (item.Result == 1)
                        {
                            //Rename FR20161124
                            //player.RecordConsumeLog_New(ConsumeWay_New.Rename, item.OldName, item.NewName, (int)CurrenciesType.Diamond, item.Diamond);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                changeNamePlayerList.Clear();
            }
        }

        double recordFrameInfoDeltaTime = 0;
        public void RecordFrameInfo(double dt)
        {
            if (recordFrameInfoDeltaTime < 10000)
            {
                recordFrameInfoDeltaTime += dt;
                return;
            }
            recordFrameInfoDeltaTime = 0;
            FpsAndCpuInfo info = Fps.GetFPSAndCpuInfo();
            if (info == null)
            {

            }
            else
            {
                MSG_ZM_CPU_INFO msg = new MSG_ZM_CPU_INFO();
                msg.FrameCount = (int)info.fps;
                msg.SleepTime = (int)info.sleepTime;
                msg.Memory = info.memorySize;
                //Log.Error("frameCount{0},sleepTime{1},memory{2}", info.fps, info.sleepTime, info.memorySize);

                if (ManagerServer != null)
                {
                    ManagerServer.Write(msg);
                }
                //Fps.ClearFPSAndCpuInfo();
            }
        }

        public void SendToRelation<T>(T msg, int uid = 0) where T : Google.Protobuf.IMessage
        {
            if (RelationServer != null)
            {
                RelationServer.Write(msg, uid);
            }
        }

    }
}
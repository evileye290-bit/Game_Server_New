using DataProperty;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {

        public int MainId { get; set; }
        public int SourceMain { get; set; }
        /// <summary>
        /// 人物类型
        /// </summary>
        public override TYPE FieldObjectType
        {
            get { return TYPE.PC; }
        }

        public override FieldObject GetOwner()
        {
            return this;
        }

        public int DBIndex = 0;


        public PlayerChar(ZoneServerApi server, int uid) : base(server)
        {
            this.DBIndex = server.GameDBPool.GetDBIndex();

            SetUid(uid);

            InitSoulRingManager();
            InitEquipmentManager();
            //暗器
            InitWeaponManager();

            //背包
            InitBagManager();

            //任务
            InitTaskManager();

            //通行证
            InitPassCardTaskManager();

            //英雄
            InitHeroManager();

            InitSoulBoneManager();

            // 宠物
            InitPetManager();

            //委派事件
            InitDelegationManager();

            //猎杀魂兽
            InitHuntingManager();

            //竞技场
            InitArenaManager();

            //秘境
            InitSecretAreaManager();

            //商店
            InitShopManager();

            //抽奖
            InitDrawManager();

            //章节
            //InitChapterManager();

            //成神之路
            InitGodPathManager();

            //跨服战
            InitCrossBattleManager();

            //阵营建设
            InitCampBuildManager();

            //阵营战
            InitCampBattleManager();

            //爬塔
            InitTower();

            //挂机
            InitOnhook();

            //推图
            InitPushFigureManager();

            //武魂共鳴
            InitWuhunResonanceManager();

            //成神
            InitHeroGodManager();

            //玩家行为触发器
            InitActionAanager();

            //礼包
            InitGiftManager();

            //挖宝
            InitShovelTreasureManager();

            //主题通行证
            InitThemePassManager();

            //主题Boss
            InitThemeBossManager();

            //称号
            InitTitleManager();

            //跨服boss
            InitCrossBossManager();

            //幽香花园
            InitGardenManager();

            //乾坤问情
            InitDivineLoveManager();

            //海岛登高
            InitIslandHighManager();

            //三叉戟
            InitTridentManager();

            //端午活动
            InitDragonBoatManager();

            //昊天石壁
            InitStoneWallManager();

            //海岛挑战
            InitIslandChallenge();

            //嘉年华Boss
            InitCarnivalBossManager();

            //嘉年华
            InitCarnivalManager();

            //漫游记
            InitTravelManager();

            //史莱克邀约
            InitShrekInvitationManager();

            //轮盘
            InitRouletteManager();

            //皮划艇
            InitCanoeManager();

            //中秋
            InitMidAutumnManager();

            //主题烟花
            InitThemeFireworkManager();

            // 魂师挑战
            InitCrossChallengeManager();

            //九考试炼
            InitNineTestManager();

            //学院
            InitSchoolManager();

            //玄天宝箱
            InitXuanBoxManager();

            //九笼祈愿
            InitWishLanternManager();

            InitDaysRechargeManager();
            
            //史莱克乐园
            InitShreklandManager();

            //时空塔
            InitSpaceTimeTower();

            InitDevilTrainingManager();
            
            //神域赐福
            InitDomainBenediction();
            
            //漂流探宝
            InitDriftExplore();
        }

        protected override void OnUpdate(float deltaTime)
        {
            //同步延迟货币
            SyncDbDelayCurrencies();
            //同步计数器
            SyncDbDelayCounters(false);

            UpdatePassTask();

            UpdateChat();
            UpdateTaskFly();
            UpdateCamp();
            UpdateCounterAddNum();
            UpdateTower();
            UpdateTreasureFly();
            UpdateIslandChallenge();
            RefreshHuntingIntrudeOutOfTime();
            UpdateSpaceTimeTower();

            //延迟同步资源仓库
            DelaySyncDbWarehouseCurrencies();
            UpdateWarehouseItems();
        }

        ///// <summary>
        ///// 初始化地理经纬度
        ///// </summary>
        ///// <param name="longitude"></param>
        ///// <param name="latitude"></param>
        //internal void Geography(double longitude, double latitude)
        //{
        //    //Log.Warn("player {0} set geography-longitude: {1}, latitude {2}", Uid, longitude, latitude);
        //    geography.Latitude = latitude;
        //    geography.Longitude = longitude;
        //    string strGeohash = Geohash.Encode(latitude, longitude);

        //    if (strGeohash.Equals(geography.GeoHashStr))
        //    {
        //        //不需要更新geohash区域
        //        server.Redis.Call(new OperateSetGeography(Uid, longitude, latitude));
        //    }
        //    else
        //    {
        //        //区域变化
        //        server.Redis.Call(new OperateUpdateGeography(Uid, longitude, latitude, strGeohash, geography.GeoHashStr));
        //        geography.GeoHashStr = strGeohash;
        //    }
        //}



        internal bool CheckSex(int typeId)
        {
            Data data = DataListManager.inst.GetData("Card", typeId);
            if (data == null)
            {
                return false;
            }
            else
            {
                int sexLimit = data.GetInt("sexLimit");
                return sexLimit == 2 || sexLimit == Sex;
            }
        }

        /// <summary>
        /// 向前段发送错误码
        /// </summary>
        /// <param name="errorType"></param>
        public void SendErrorCodeMsg(ErrorCode errorType)
        {
            SendErrorCodeMsg((int)errorType);
        }
        /// <summary>
        /// 向前段发送错误码
        /// </summary>
        /// <param name="errorType"></param>
        public void SendErrorCodeMsg(int errorType)
        {
            MSG_ZGC_ERROR_CODE msg = new MSG_ZGC_ERROR_CODE();
            msg.ErrorCode = errorType;
            Write(msg);
        }

        public override bool OnMove(float deltaTime)
        {
            if (CanMove())
            {
                if (!IsMapLoadingDone || IsTransforming)
                {
                    return false;
                }
                Move(deltaTime);
                return CheckPathEnd();
            }
            else
            {
                return true;

            }
        }

        public void SyncMainId()
        {
            // 同步db 当前main id
            if (MainId != server.MainId)
            {
                MainId = server.MainId;
                server.GameDBPool.Call(new QuerySetMainId(uid, server.MainId), DBIndex);
            }
        }

        private bool OnLine = false;

        public bool IsOnline()
        {
            return OnLine;
        }

        public void SetOnline(bool isOnline)
        {
            OnLine = isOnline;
            if (isOnline)
            {
                RecordLoginLog(ClientIp, DeviceId);

                KomoeEventLogPlayerLogin();
                BIRecordLoginLog();
                //server.BILoggerMng.RecordLoginLog(uid.ToString(), AccountName, DeviceId, ChannelName, server.MainId.ToString(), ClientIp, Level);
            }
            else
            {
                RecordLogoutLog(ClientIp, DeviceId);
                //server.BILoggerMng.RecordLogoutLog(uid.ToString(), AccountName, DeviceId, ChannelName, server.MainId.ToString(), ClientIp,Level,(int)(server.Now()- LastLoginTime).TotalSeconds);
                BIRecordLogoutLog();
                KomoeEventLogPlayerLogout();
                KomoeEventLogUserSnapshot();
            }
            server.GameRedis.Call(new OperateSetOnlineState(uid, Level, isOnline, ZoneServerApi.now, MainId, RegisterId));
        }

        public void GetSimpleInfo(MSG_GC_CHAR_SIMPLE_INFO info)
        {
            info.InstanceId = InstanceId;
            info.Name = Name;
            info.Level = Level;
            info.HeroId = HeroId;
            info.PosX = Position.x;
            info.PosY = Position.y;
            info.Angle = GenAngle;
            info.Sex = Sex;

            info.IsVisable = IsVisable;
            info.Uid = Uid;
            info.Title = TitleMng.CurTitleId;
            info.Hp = GetHp();
            info.MaxHp = GetMaxHp();
            info.Model = BagManager.FashionBag.GetModel();
            info.InRealBody = InRealBody;
            HeroInfo temp = HeroMng.GetHeroInfo(HeroId);
            info.AwakenLevel = temp.AwakenLevel;
            info.MainTaskId = MainTaskId;
            info.GodType = GodType;

            if (IsMoving)
            {
                // 移动中 需要添加移动相关信息
                info.DestX = MoveHandler.MoveToPosition.x;
                info.DestY = MoveHandler.MoveToPosition.y;
                info.Speed = MoveHandler.MoveSpeed;
            }
        }

        public override void BroadcastSimpleInfo()
        {
            MSG_GC_CHAR_SIMPLE_INFO info = new MSG_GC_CHAR_SIMPLE_INFO();
            GetSimpleInfo(info);
            BroadCast(info);
        }
    }
}
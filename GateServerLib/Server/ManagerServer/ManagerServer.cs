using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateM;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using Message.Manager.Protocol.MGate;
using ServerFrame;
using ServerLogger;
using ServerLogger.KomoeLog;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace GateServerLib
{
    public partial class ManagerServer : BackendServer
    {
        private GateServerApi Api
        { get { return (GateServerApi)api; } }

        public ManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MGate_MaxUid>.Value, OnResponse_MaxUid);
            AddResponser(Id<MSG_MGate_CharacterGetZone>.Value, OnResponse_CharacterGetZone);
            //AddResponser(Id<MSG_MGate_RouteToOtherManager>.Value, OnResponse_RouteToOtherManager);
            AddResponser(Id<MSG_MGate_FORCE_LOGIN>.Value, OnResponse_ForceLogin);
            AddResponser(Id<MSG_MGate_ZONE_TRANSFORM>.Value, OnResponse_ZoneTransform);
            AddResponser(Id<MSG_MGate_REGIST_CHARACTER_COUNT>.Value, OnResponse_UpdateRegistCharacterCount);

            //ResponserEnd
        }

        //private void OnResponse_MaxUid(MemoryStream stream, int uid = 0)
        //{

        //    MSG_MGate_MaxUid msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_MaxUid>(stream);
        //    Client client = Api.ClientMng.FindClient(msg.AccountName, msg.ChannelName);
        //    if (client == null)
        //    {
        //        Log.Debug("OnResponse_MaxUid  create char with client null " + msg.AccountName + " " + msg.ChannelName);
        //        return;
        //    }
        //    MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
        //    if (client.ReqCreateMsg == null)
        //    {
        //        Log.Warn("account {0} create character failed: ReqCreateMsg is null", msg.AccountName);
        //        response.Result = (int)ErrorCode.NotCreating;
        //        client.Write(response);
        //        return;
        //    }
        //    if (msg.Result != (int)ErrorCode.Success)
        //    {
        //        Log.Warn("account {0} create character failed: no watch dog manager", msg.AccountName);
        //        response.Result = (int)ErrorCode.NotOpen;
        //        client.Write(response);
        //        return;
        //    }
        //    response.Character = new MSG_GC_ENTER_CHARACTER_INFO();
        //    response.Character.Uid = msg.MaxUid;
        //    // 尝试创建新角色
        //    // step 1 检查角色名是否重复
        //    // TODO FIX
        //    Api.GameDBPool.Call(new QueryCheckCharacterName(client.ReqCreateMsg.Name, msg.MaxUid), ret =>
        //   {
        //        // 尝试创建角色名
        //        if ((int)ret != 0)
        //       {
        //            //角色名存在
        //            Log.Write("account {0} try to create char name {1} failed: name exited", client.AccountName, client.ReqCreateMsg.Name);
        //           client.ReqCreateMsg = null;
        //           response.Result = (int)ErrorCode.NameExisted;
        //           client.Write(response);
        //           return;
        //       }
        //       else
        //       {
        //           // 开始尝试创建角色
        //           Log.Debug("try createplayer create char with client " + msg.AccountName + " " + msg.ChannelName);
        //           CreatePlayerWithTransaction(client, CreatePlayerStep.Character, response);
        //       }
        //   });
        //}

        private void OnResponse_MaxUid(MemoryStream stream, int uid = 0)
        {

            MSG_MGate_MaxUid msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_MaxUid>(stream);
            Client client = Api.ClientMng.FindClient(msg.AccountName, msg.ChannelName);
            if (client == null)
            {
                Log.Debug("OnResponse_MaxUid  create char with client null " + msg.AccountName + " " + msg.ChannelName);
                return;
            }
            MSG_GC_CREATE_CHARACTER response = new MSG_GC_CREATE_CHARACTER();
            if (client.ReqCreateMsg == null)
            {
                Log.Warn("account {0} create character failed: ReqCreateMsg is null", msg.AccountName);
                response.Result = (int)ErrorCode.NotCreating;
                client.Write(response);
                return;
            }
            if (msg.Result != (int)ErrorCode.Success)
            {
                Log.Warn("account {0} create character failed: no watch dog manager", msg.AccountName);
                response.Result = (int)ErrorCode.NotOpen;
                client.Write(response);
                return;
            }
            response.Character = new MSG_GC_ENTER_CHARACTER_INFO();
            response.Character.Uid = msg.MaxUid;
            // 尝试创建新角色
            // step 1 检查角色名是否重复
            // TODO FIX
            Api.GameDBPool.Call(new QueryCheckCharacterName(client.ReqCreateMsg.Name, msg.MaxUid), ret =>
            {
                //// 尝试创建角色名
                //if ((int)ret != 0)
                //{
                //    //角色名存在
                //    Log.Write("account {0} try to create char name {1} failed: name exited", client.AccountName, client.ReqCreateMsg.Name);
                //    client.ReqCreateMsg = null;
                //    response.Result = (int)ErrorCode.NameExisted;
                //    client.Write(response);
                //    return;
                //}
                //else
                {
                    // 开始尝试创建角色
                    Log.Debug("try createplayer create char with client " + msg.AccountName + " " + msg.ChannelName);
                    CreatePlayerWithTransaction(client, response);
                }
            });
        }


        private void CreatePlayerWithTransaction(Client client, MSG_GC_CREATE_CHARACTER response)
        {
            int uid = response.Character.Uid;

            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();
            //初始化创建人物信息
            CreateCharacterModel createInfo = GetCreateCharacterInfo(uid, client);
            if (createInfo == null)
            {
                client.ReqCreateMsg = null;
                response.Result = (int)ErrorCode.Unknown;
                client.Write(response);
                return;
            }
            //Character
            QueryCreateCharacter queryCreateCharacter = new QueryCreateCharacter(createInfo);
            querys.Add(queryCreateCharacter);

            //tableName = "character_resource";
            QueryCreateResource queryResource = new QueryCreateResource(uid);
            querys.Add(queryResource);

            //task
            TaskInfo taskInfo = TaskLibrary.GetTaskInfoById(CharacterInitLibrary.FirstTask);
            if (taskInfo != null)
            {
                TaskItem task = new TaskItem();
                task.Id = CharacterInitLibrary.FirstTask;
                task.ParamType = taskInfo.ParamType;
                if (taskInfo.CheckParamKey(TaskParamType.NUM))
                {
                    task.ParamNum = taskInfo.GetParamIntValue(TaskParamType.NUM);
                }
                task.Time = Timestamp.GetUnixTimeStampSeconds(GateServerApi.now);

                QueryInsertTaskInfo queryInsertTaskInfo = new QueryInsertTaskInfo(uid, task);
                querys.Add(queryInsertTaskInfo);
            }

            //tableName = "email";
            QueryInsertEmail queryInsertEmail = new QueryInsertEmail(uid);
            querys.Add(queryInsertEmail);


            ////时装
            //List<FashionInfo> itemsFashion = GetFashions(client.ReqCreateMsg, uid);
            //if (itemsFashion == null)
            //{
            //    // 通知失败
            //    Log.Warn("account {0} try to create items failed in step {1}", client.AccountName, step);
            //    client.ReqCreateMsg = null;
            //    response.Result = (int)ErrorCode.Unknown;
            //    client.Write(response);
            //    return;
            //}
            //QueryInsertFashions queryInsertFashion = new QueryInsertFashions(itemsFashion);
            //querys.Add(queryInsertFashion);

            //头像框
            FaceFrameInfo itemFaceFrame = GetFaceFrame(uid);
            QueryInsertFaceFrame queryInsertFaceFrame = new QueryInsertFaceFrame(itemFaceFrame);
            querys.Add(queryInsertFaceFrame);

            //聊天气泡
            ChatFrameInfo itemChatFrame = GetChatFrames(uid);
            QueryInsertChatFrame queryInsertChatFrame = new QueryInsertChatFrame(itemChatFrame);
            querys.Add(queryInsertChatFrame);

            //tableName = "recharge";
            QueryInsertDefaultRecharge queryInsertDefaultRecharge = new QueryInsertDefaultRecharge(uid);
            querys.Add(queryInsertDefaultRecharge);

            //初始伙伴和主角
            if (createInfo.Heros.Count > 0)
            {
                //tableName = "hero";
                QueryInsertDefaultHeros queryInsertDefaultHeros = new QueryInsertDefaultHeros(uid, createInfo.Heros);
                querys.Add(queryInsertDefaultHeros);
                QueryInsertDefaultHeroSlots queryInsertDefaultHeroSlots = new QueryInsertDefaultHeroSlots(uid, createInfo.Heros);
                querys.Add(queryInsertDefaultHeroSlots);
                QueryInsertDefaultHeroPos queryInsertDefaultHeroPos = new QueryInsertDefaultHeroPos(uid, createInfo.HeroId);
                querys.Add(queryInsertDefaultHeroPos);
            }

            if (createInfo.Items.Count > 0)
            {
                //tableName = "item";
                QueryInsertDefaultItems queryInsertItem = new QueryInsertDefaultItems(uid, createInfo.Items);
                querys.Add(queryInsertItem);
            }

            //阵营养成 tableName = "camp_stars";
            QueryInsertCampStars queryInsertCampStars = new QueryInsertCampStars(uid);
            querys.Add(queryInsertCampStars);

            //计数 tableName = "game_counter";
            QueryInsertDefaultCounter queryInsertDefaultCounter = new QueryInsertDefaultCounter(uid, createInfo.CountList);
            querys.Add(queryInsertDefaultCounter);

            //委派事件 tableName = "delegation";         
            QueryInsertDelegations queryInsertDelegations = new QueryInsertDelegations(uid);
            querys.Add(queryInsertDelegations);

            //竞技场
            QueryInsertArena queryInsertArena = new QueryInsertArena(uid);
            querys.Add(queryInsertArena);

            //跨服战
            QueryInsertCrossBattle queryInsertCrossBattle = new QueryInsertCrossBattle(uid);
            querys.Add(queryInsertCrossBattle);

            QueryInsertSecretArea queryInsertSecretArea = new QueryInsertSecretArea(uid);
            querys.Add(queryInsertSecretArea);

            //通行证
            QueryInsertPassCardRewardBase queryInsertPassCardRewardBase = new QueryInsertPassCardRewardBase(uid, PassCardLibrary.GetPeriod());
            querys.Add(queryInsertPassCardRewardBase);

            //抽卡
            QueryInsertDraw queryInsertDraw = new QueryInsertDraw(uid);
            querys.Add(queryInsertDraw);

            //章节
            QueryInsertChapter queryInsertChapter = new QueryInsertChapter(uid, 1);
            querys.Add(queryInsertChapter);

            //许愿池
            QueryInsertWishPoolInfo queryInsertWishPool = new QueryInsertWishPoolInfo(uid);
            querys.Add(queryInsertWishPool);

            //阵营建设
            QueryInsertCampBuildInfo queryInsertCampBuild = new QueryInsertCampBuildInfo(uid);
            querys.Add(queryInsertCampBuild);

            //爬塔
            QueryInsertTower queryInsertTower = new QueryInsertTower(uid);
            querys.Add(queryInsertTower);

            //挂机
            QueryInsertOnhook queryInsertOnhook = new QueryInsertOnhook(uid);
            querys.Add(queryInsertOnhook);

            //推图
            QueryInsertPushFigure queryInsertPushFigure = new QueryInsertPushFigure(uid);
            querys.Add(queryInsertPushFigure);

            //推图
            QueryInsertDefaultWuhunResonanceInfos queryInsertDefaultWuhunResonanceInfos = new QueryInsertDefaultWuhunResonanceInfos(uid, WuhunResonanceConfig.ResonanceGridDefaultCount);
            querys.Add(queryInsertDefaultWuhunResonanceInfos);

            //猎杀魂兽
            QueryInsertHunting queryInsertHunting = new QueryInsertHunting(uid);
            querys.Add(queryInsertHunting);

            //任务完成状态
            QueryInsertActivityFinishState queryInsertActivityFinish = new QueryInsertActivityFinishState(uid);
            querys.Add(queryInsertActivityFinish);

            //幽香花园
            QueryInsertGarden queryInsertGarden = new QueryInsertGarden(uid);
            querys.Add(queryInsertGarden);

            //海岛登高
            QueryInsertIslandHigh queryInsertIslandHigh = new QueryInsertIslandHigh(uid);
            querys.Add(queryInsertIslandHigh);

            //海岛登高礼包
            //QueryInsertIslandHighGift queryInsertIslandGift = new QueryInsertIslandHighGift(uid);
            //querys.Add(queryInsertIslandGift);

            QueryInsertTrident queryInsertTrident = new QueryInsertTrident(uid);
            querys.Add(queryInsertTrident);

            QueryInsertDragoonBoat queryInsertDragonBoat = new QueryInsertDragoonBoat(uid);
            querys.Add(queryInsertDragonBoat);

            //海岛挑战
            QueryInsertIslandChallenge queryInsertIslandChallenge = new QueryInsertIslandChallenge(uid);
            querys.Add(queryInsertIslandChallenge);

            QueryInsertCarnivalBoss queryInsertCarnivalBoss = new QueryInsertCarnivalBoss(uid);
            querys.Add(queryInsertCarnivalBoss);

            QueryInsertCarnivalRecharge queryInsertCarnivalRecharge = new QueryInsertCarnivalRecharge(uid);
            querys.Add(queryInsertCarnivalRecharge);

            QueryInsertRoulette queryInsertRoulette = new QueryInsertRoulette(uid);
            querys.Add(queryInsertRoulette);

            QueryInsertCanoe queryInsertCanoe = new QueryInsertCanoe(uid);
            querys.Add(queryInsertCanoe);

            QueryInsertMainBattleQueue queryInsertMainQueue = new QueryInsertMainBattleQueue(uid, CharacterInitLibrary.FirstMainQueueNum, createInfo.FirstMainQueueName);
            querys.Add(queryInsertMainQueue);

            QueryInsertMidAutumn queryInsertMidAutumn = new QueryInsertMidAutumn(uid);
            querys.Add(queryInsertMidAutumn);

            QueryInsertThemeFirework queryInsertThemeFirework = new QueryInsertThemeFirework(uid);
            querys.Add(queryInsertThemeFirework);

            QueryInsertCrossChallenge queryInsertCrossChallenge = new QueryInsertCrossChallenge(uid);
            querys.Add(queryInsertCrossChallenge);

            QueryInsertNineTest queryInsertNineTest = new QueryInsertNineTest(uid);
            querys.Add(queryInsertNineTest);

            QueryInsertWarehouseResource queryInsertWhResource = new QueryInsertWarehouseResource(uid);
            querys.Add(queryInsertWhResource);

            QueryInsertWebPayRechargeRebate queryInsertWebPayRebate = new QueryInsertWebPayRechargeRebate(uid);
            querys.Add(queryInsertWebPayRebate);

            QueryInsertDiamondRebate queryInsertDiamondRebate = new QueryInsertDiamondRebate(uid);
            querys.Add(queryInsertDiamondRebate);

            QueryInsertXuanBox queryInsertXuanBox = new QueryInsertXuanBox(uid);
            querys.Add(queryInsertXuanBox);

            QueryInsertWishLantern queryInsertWishLantern = new QueryInsertWishLantern(uid);
            querys.Add(queryInsertWishLantern);

            QueryInsertSchoolInfo queryInsertSchool = new QueryInsertSchoolInfo(uid);
            querys.Add(queryInsertSchool);

            QueryInsertSchoolTaskFinish queryInsertSchoolTaskFin = new QueryInsertSchoolTaskFinish(uid);
            querys.Add(queryInsertSchoolTaskFin);

            int sTPeriod = SchoolLibrary.GetSchoolTaskPeriodByTime(Api.Now());
            Dictionary<int, CommonTaskModel> tasksModels = SchoolLibrary.GetPeriodSchoolTasks(sTPeriod);
            QueryInsertDefaultSchoolTasks queryInsertSchoolTasks = new QueryInsertDefaultSchoolTasks(uid, tasksModels);
            querys.Add(queryInsertSchoolTasks);

            QueryInsertAnswerQuestion queryInsertAnswerQ = new QueryInsertAnswerQuestion(uid);
            querys.Add(queryInsertAnswerQ);

            QueryInsertDaysRecharge queryInsertDaysRecharge = new QueryInsertDaysRecharge(uid);
            querys.Add(queryInsertDaysRecharge);

            QueryInsertShrekland queryInsertShrekland = new QueryInsertShrekland(uid);
            querys.Add(queryInsertShrekland);

            QueryInsertSpaceTimeTower quertInsertSpaceTime = new QueryInsertSpaceTimeTower(uid);
            querys.Add(quertInsertSpaceTime);

            QueryInsertDomainBenediction queryInsertDomainBenediction = new QueryInsertDomainBenediction(uid);
            querys.Add(queryInsertDomainBenediction);
            
            QueryInsertDriftExplore queryInsertDriftExplore = new QueryInsertDriftExplore(uid);
            querys.Add(queryInsertDriftExplore);
            
            CharacterEnterInfo enterInfo = new CharacterEnterInfo();
            enterInfo.Uid = uid;
            enterInfo.MainId = api.MainId;
            enterInfo.Channel = 1;
            enterInfo.MapId = CharacterInitLibrary.MapId;

            response.Character.Name = client.ReqCreateMsg.Name;
            response.Character.Sex = client.ReqCreateMsg.Sex;
            response.Character.Job = client.ReqCreateMsg.Job;
            response.Character.HeroId = createInfo.HeroId;

            //1 把其他表的访问合并为一条transaction
            api.GameDBPool.Call(new DBQueryTransaction(querys), ret =>
             {
                 if ((int)ret == 0)
                 {
                     if (client.CharacterList == null)
                     {
                         client.CharacterList = new List<CharacterEnterInfo>();
                     }
                     client.CharacterList.Add(enterInfo);

                     LoadCreateInfo2Redis(createInfo);
                     response.Result = (int)ErrorCode.Success;
                     client.Write(response);

                     //Api.BILoggerMng.RecordCreateCharLog(uid.ToString(),client.AccountName,client.DeviceId,client.ChannelName,Api.MainId.ToString(),client.Ip,client.SDKUuid);
                     Api.BILoggerMng.RegisterTaLog(uid, client.AccountName, client.DeviceId, client.channelId, Api.MainId, client.Ip, client.idfa, client.caid, client.idfv, client.imei, client.oaid, client.imsi, client.anid, client.packageName, client.SDKUuid, client.extendId);
                     Api.BILoggerMng.AccountFirstRegisterTaLog(uid, client.AccountName, client.DeviceId, client.channelId, Api.MainId, client.Ip, client.idfa, client.caid, client.idfv, client.imei, client.oaid, client.imsi, client.anid, client.packageName, client.SDKUuid, client.extendId);
                     Api.BILoggerMng.DeviceFirstRegisterTaLog(uid, client.AccountName, client.DeviceId, client.channelId, Api.MainId, client.Ip, client.idfa, client.caid, client.idfv, client.imei, client.oaid, client.imsi, client.anid, client.packageName, client.SDKUuid, client.extendId);

                     KomoeEventLogCreateRole(client, enterInfo);
                     //server.UnLockCreate(client.ReqCreateMsg.Name);
                     client.ReqCreateMsg = null;
                     client.LoginToZone();

                     NotifyManagerNewCharacterCheated(client.Uid);

                     UpdateAccountLoginServers(createInfo);
                 }
                 else
                 {
                     // 通知失败
                     Log.Warn("account {0} try to create char name {1} failed in step {2}", client.AccountName, client.ReqCreateMsg.Name, "transaction");
                     client.ReqCreateMsg = null;
                     response.Result = (int)ErrorCode.Unknown;
                     client.Write(response);
                     return;
                 }
             });
        }


        //更新account db中login信息
        private void UpdateAccountLoginServers(CreateCharacterModel createInfo)
        {
            BackendServer manager = Api.ManagerServerManager.GetSinglePointServer(createInfo.MainId);
            if (manager != null)
            {
                //通知下线
                MSG_GateM_LOGOUT msg = new MSG_GateM_LOGOUT()
                {
                    AccountId = createInfo.AccountName,
                    Channel = createInfo.ChannelName,
                    Uid = createInfo.Uid,
                    Level = createInfo.Level,
                    HeroId = createInfo.HeroId,
                    GodType = createInfo.GodType,
                    Name = createInfo.CharName,
                    SourceMain = createInfo.MainId,
                };
                manager.Write(msg);
            }
        }

        /*
         * 玩家创角表	create_role	调输入名字接口则触发事件	
            role_name	string	玩家角色名称	例如：黄昏蔷薇行者
            gender	int	性别	举例：0-男|1-女
            model	string	设备机型	设备的机型，例如Samsung GT-I9208
            os_version	string	操作系统版本	操作系统版本，例如13.0.2
            network	string	网络信息	4G/3G/WIFI/2G
            mac	string	mac 地址	局域网地址
            ip	string	玩家登录IP	玩家登录IP
            cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogCreateRole(Client client, CharacterEnterInfo enterInfo)
        {
            // LOG 记录开关 
            //if (!GameConfig.TrackingLogSwitch)
            //{
            //    return;
            //}
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();
            infDic.Add("b_udid", client.DeviceId);
            infDic.Add("b_sdk_udid", client.SDKUuid);
            infDic.Add("b_sdk_uid", client.AccountName);
            infDic.Add("b_account_id", client.AccountName);
            infDic.Add("b_tour_indicator", client.tour);
            infDic.Add("b_role_id", enterInfo.Uid);

            infDic.Add("b_game_base_id", KomoeLogConfig.GameBaseId);
            //if (client.platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", client.gameId);
            infDic.Add("b_platform", client.platform);
            infDic.Add("b_zone_id", Api.MainId);
            infDic.Add("b_channel_id", client.channelId);
            infDic.Add("b_version", client.clientVersion);
            infDic.Add("level", 1);
            infDic.Add("role_name", "");

            string logId = $"{KomoeLogConfig.GameBaseId}-{KomoeLogEventType.create_role}-{ client.Uid}-{Timestamp.GetUnixTimeStampSeconds(Api.Now())}-{1}";
            infDic.Add("b_log_id", logId);
            infDic.Add("b_eventname", KomoeLogEventType.create_role.ToString());

            infDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(Api.Now()));
            infDic.Add("b_datetime", Api.Now().ToString("yyyy-MM-dd HH:mm:ss"));

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("role_name", client.ReqCreateMsg.Name);
            properties.Add("gender", client.ReqCreateMsg.Sex);
            properties.Add("model", client.deviceModel);
            properties.Add("os_version", client.osVersion);
            properties.Add("network", client.network);
            properties.Add("mac", client.mac);
            properties.Add("ip", client.Ip);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }


        private void NotifyManagerNewCharacterCheated(int uid)
        {
            Write(new MSG_GateM_CREATE_NEW_CHARACTER() { Uid = uid });
        }

        public CreateCharacterModel GetCreateCharacterInfo(int uid, Client client)
        {
            CreateCharacterModel createInfo = new CreateCharacterModel();
            createInfo.Uid = uid;
            createInfo.AccountName = client.AccountName;
            createInfo.ChannelName = client.ChannelName;
            createInfo.CharName = client.ReqCreateMsg.Name;
            createInfo.Sex = client.ReqCreateMsg.Sex;
            createInfo.Job = client.ReqCreateMsg.Job;
            createInfo.MainId = api.MainId;
            createInfo.SourceMain = client.MainId;
            createInfo.Channel = 1;
            createInfo.SDKUuid = client.SDKUuid;

            //int level = data.GetInt("Level");
            createInfo.Level = System.Math.Max(1, CharacterInitLibrary.Level);
            //int mapId = data.GetInt("MapId");
            createInfo.MapId = CharacterInitLibrary.MapId;
            //float posX = data.GetFloat("BeginPosX");
            createInfo.PosX = CharacterInitLibrary.BeginPosX;
            //float posY = data.GetFloat("BeginPosY");
            createInfo.PosY = CharacterInitLibrary.BeginPosY;

            createInfo.AcrossOceanDiff = (int)DungeonDifficulty.Easy;

            //背包
            Data bagData = DataListManager.inst.GetData("BagConfig", 1);
            if (bagData != null)
            {
                createInfo.BagSpace = bagData.GetInt("BagInitSpace");
            }
            DataList counterDataList = DataListManager.inst.GetDataList("Counter");
            if (counterDataList != null)
            {
                foreach (var item in counterDataList)
                {
                    if (item.Value.GetInt("InitMax") == 1)
                    {
                        createInfo.CountList[item.Value.Name] = item.Value.GetInt("MaxCount");
                    }
                }
            }

            createInfo.PowerLimit = ChapterLibrary.MinPower;

            //主角和伙伴
            HeroJobModel heroJobInfo = HeroLibrary.GetHeroJobInfo(createInfo.Job, createInfo.Sex);
            if (heroJobInfo != null)
            {
                createInfo.HeroId = CharacterInitLibrary.InitHeroId;
                createInfo.HeroTitileLevel = 0;

                HeroModel heroJobModel = HeroLibrary.GetHeroModel(heroJobInfo.HeroId);
                if (heroJobModel != null)
                {
                    createInfo.HeroLevel = heroJobModel.InitLevel;
                    createInfo.HeroAwakenLevel = heroJobModel.AwakenLevel;
                    createInfo.HeroAwakenLevel = -1;
                }
                else
                {
                    Log.Warn("player {0} get create character info get hero {1} init level failed", uid, heroJobInfo.HeroId);
                    return null;
                }
                createInfo.FaceIconId = heroJobInfo.FaceIconId;
                createInfo.FashionId = 0;

                HeroCreateModel heroInfo = new HeroCreateModel();
                heroInfo.id = createInfo.HeroId;
                heroInfo.level = createInfo.HeroLevel;
                heroInfo.equipIndex = CONST.MAIN_HERO_EQUIP_INDEX;
                heroInfo.state = (int)WuhunState.WaitAwaken;
                heroInfo.titleLevel = createInfo.HeroTitileLevel;
                heroInfo.awakenLevel = createInfo.HeroAwakenLevel;
                createInfo.Heros.Add(heroInfo);
            }
            else
            {
                Log.Warn("player {0} get create character info get hero job {1} sex {2} failed", uid, client.ReqCreateMsg.Job, client.ReqCreateMsg.Sex);
                return null;
            }
            foreach (var hero in CharacterInitLibrary.Heros)
            {
                createInfo.Heros.Add(hero);
            }

            foreach (var item in CharacterInitLibrary.Items)
            {
                ItemCreateModel itemInfo = new ItemCreateModel();
                itemInfo.typeId = item.typeId;
                itemInfo.num = item.num;
                itemInfo.time = Timestamp.GetUnixTimeStampSeconds(GateServerApi.now);
                itemInfo.uid = Api.UID.NewIuid(Api.MainId, Api.SubId);
                createInfo.Items.Add(itemInfo);
            }

            MainBattleQueueModel model = HeroLibrary.GetHeroMainQueueModel(CharacterInitLibrary.FirstMainQueueNum);
            if (model != null)
            {
                createInfo.FirstMainQueueName = model.DefaultName;
            }

            return createInfo;
        }

        private void OnResponse_CharacterGetZone(MemoryStream stream, int uid = 0)
        {
            MSG_MGate_CharacterGetZone msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_CharacterGetZone>(stream);
            Client client = Api.ClientMng.FindClient(msg.AccountName, msg.ChannelName, msg.Token);
            if (client == null)
            {
                return;
            }
            MSG_GC_TO_ZONE response = new MSG_GC_TO_ZONE();
            if (msg.Result != (int)ErrorCode.Success)
            {
                Log.Warn("player {0} to zone main {1} failed {2}", msg.Uid, msg.MainId, ((ErrorCode)msg.Result).ToString());
                response.Result = msg.Result;
                client.Write(response);
                if (msg.Result == (int)ErrorCode.AlreadyLogin)
                {
                    client.EnableCatch(false);
                    Api.ClientMng.RemoveClient(client);
                }
                return;
            }
            BackendServer zone = Api.ZoneServerManager.GetServer(msg.MainId, msg.SubId);
            if (zone == null)
            {
                Log.Warn("player {0} to zone main {1} failed: no such zone {2}", msg.Uid, msg.MainId, msg.SubId);
                response.Result = (int)ErrorCode.NotOpen;
                client.Write(response);
                return;
            }
            // 成功 正式登录 进入世界
            client.EnterWorld(msg.Uid, zone);
            MSG_GateZ_EnterWorld request = new MSG_GateZ_EnterWorld();
            request.AccountName = client.AccountName;
            request.MapId = msg.MapId;
            request.Channel = msg.Channel;
            request.CharacterUid = client.Uid;
            request.ClientIp = client.Ip;
            request.DeviceId = client.DeviceId;
            request.SdkUuid = client.SDKUuid;
            request.SyncData = true;
            request.RegisterId = client.RegisterId;
            request.ChannelName = msg.ChannelName;
            request.IsRebate = client.IsRebated;


            request.ChannelId = client.channelId;
            request.Idfa = client.idfa;       //苹果设备创建角色时使用
            request.Idfv = client.idfv;       //苹果设备创建角色时使用
            request.Imei = client.imei;       //安卓设备创建角色时使用
            request.Imsi = client.imsi;       //安卓设备创建角色时使用
            request.Anid = client.anid;       //安卓设备创建角色时使用
            request.Oaid = client.oaid;       //安卓设备创建角色时使用
            request.PackageName = client.packageName;//包名
            request.ExtendId = client.extendId;   //广告Id，暂时不使用
            request.Caid = client.caid;     //暂时不使用

            request.Tour = client.tour;              //是否是游客账号（0:非游客，1：游客）
            request.Platform = client.platform;           //平台名称	统一：ios|android|windows
            request.ClientVersion = client.clientVersion; //游戏的迭代版本，例如1.0.3
            request.DeviceModel = client.deviceModel;     //设备的机型，例如Samsung GT-I9208
            request.OsVersion = client.osVersion;         //操作系统版本，例如13.0.2
            request.Network = client.network;             //网络信息	4G/3G/WIFI/2G
            request.Mac = client.mac;                     //局域网地址         
            request.GameId = client.gameId;

            zone.Write(request);

        }

        private void OnResponse_ForceLogin(MemoryStream stream, int uid = 0)
        {
            MSG_MGate_FORCE_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_FORCE_LOGIN>(stream);
            Log.Write("player {0} force login to main {1} sub {2} map {3} channel {4}", msg.Uid, msg.MainId, msg.SubId, msg.MapId, msg.Channel);
            Client client = Api.ClientMng.FindClientByUid(msg.Uid);
            if (client == null)
            {
                return;
            }
            BackendServer zone = Api.ZoneServerManager.GetServer(msg.MainId, msg.SubId);
            if (zone == null)
            {
                Log.Write("player {0} force login to main {1} sub {2} map {3} channel {4} failed: zone not exist", msg.Uid, msg.MainId, msg.SubId, msg.MapId, msg.Channel);
                Api.ClientMng.RemoveClient(client);
                return;
            }
            client.ChangeZone(zone, msg.MapId, msg.Channel, msg.SyncData);
        }

        private void OnResponse_UpdateRegistCharacterCount(MemoryStream stream, int uid = 0)
        {
            MSG_MGate_REGIST_CHARACTER_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_REGIST_CHARACTER_COUNT>(stream);
            Api.ClientMng.RegistedCharacterCount = msg.RegistCharacterCount;
        }

        //private void OnResponse_RouteToOtherManager(MemoryStream stream, int uid = 0)
        //{
        //    MSG_MGate_RouteToOtherManager msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MGate_RouteToOtherManager>(stream);
        //    Client client = Api.ClientMng.FindClient(msg.AccountName, msg.ChannelName);
        //    if (client == null)
        //    {
        //        return;
        //    }
        //    Log.Write("player {0} will route to main {1} map {2} channel {3}", msg.Uid, msg.MainId, msg.MapId, msg.Channel);
        //    BackendServer manager = Api.ManagerServerManager.GetSinglePointServer(msg.MainId);
        //    if (manager == null)
        //    {
        //        Log.Warn("player {0} request to route to other main {1} failed: server not open", msg.Uid, msg.MainId);
        //        MSG_GC_TO_ZONE response = new MSG_GC_TO_ZONE();
        //        response.Result = (int)ErrorCode.NotOpen;
        //        client.Write(response);
        //        return;
        //    }

        //    MSG_GateM_CharacterGetZone request = new MSG_GateM_CharacterGetZone();
        //    request.Uid = msg.Uid;
        //    request.MapId = msg.MapId;
        //    request.Channel = msg.Channel;
        //    request.MainId = msg.MainId;
        //    request.CreateTimeStamp = Timestamp.GetUnixTimeStamp(client.AccountCreatedTime);
        //    request.AccountName = msg.AccountName;

        //    // 渠道
        //    request.ChannelName = msg.ChannelName;
        //    request.Routed = true;
        //    manager.Write(request);
        //}
    }
}
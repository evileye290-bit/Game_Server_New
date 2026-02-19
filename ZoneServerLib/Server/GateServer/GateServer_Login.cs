using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using DBUtility.Sql;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_EnterWorld(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EnterWorld msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EnterWorld>(stream);
            Log.WriteLine("account {0} uid {1} request enter world", msg.AccountName, msg.CharacterUid);
            Log.Info($"account {msg.AccountName}  enter world ChannelId {msg.ChannelId} Idfa {msg.Idfa} Idfv { msg.Idfv} Imei { msg.Imei} Imsi { msg.Imsi} Anid{msg.Anid} Oaid {msg.Oaid} PackageName {msg.PackageName} ExtendId {msg.ExtendId} Caid{msg.Caid}");
            PlayerChar offlinePlayer = null;
            PlayerEnter playerEnter = null;
            if (GameConfig.CatchOfflinePlayer)
            {
                if (Api.PCManager.PcOfflineList.TryGetValue(msg.CharacterUid, out offlinePlayer))
                {
                    Api.PCManager.RemoveOfflinePc(msg.CharacterUid);
                }
            }
            PlayerChar pc = null;
            playerEnter = Api.PCManager.GetPlayerEnter(msg.CharacterUid);
            bool fromTransform = false;
            if (playerEnter != null && playerEnter.Player != null)
            {
                fromTransform = true;
            }
            if (offlinePlayer != null && !fromTransform)
            {
                Log.Write("offline player {0} enter world", offlinePlayer.Uid);
                offlinePlayer.NeedSyncEnterWorld = msg.SyncData;
                //playerEnter = Api.PCManager.GetPlayerEnter(msg.CharacterUid);
                //if (playerEnter!=null && playerEnter.Player != null)
                //{
                //    offlinePlayer = playerEnter.Player;
                //}
                offlinePlayer.DeviceId = msg.DeviceId;
                offlinePlayer.IsRebated = msg.IsRebate;

                offlinePlayer.ChannelId = msg.ChannelId;
                offlinePlayer.Idfa = msg.Idfa;       //苹果设备创建角色时使用
                offlinePlayer.Idfv = msg.Idfv;       //苹果设备创建角色时使用
                offlinePlayer.Imei = msg.Imei;       //安卓设备创建角色时使用
                offlinePlayer.Imsi = msg.Imsi;       //安卓设备创建角色时使用
                offlinePlayer.Anid = msg.Anid;       //安卓设备创建角色时使用
                offlinePlayer.Oaid = msg.Oaid;       //安卓设备创建角色时使用
                offlinePlayer.PackageName = msg.PackageName;//包名
                offlinePlayer.ExtendId = msg.ExtendId;   //广告Id，暂时不使用
                offlinePlayer.Caid = msg.Caid;      //暂时不使用

                offlinePlayer.Tour = msg.Tour;                   //是否是游客账号（0:非游客，1：游客）
                offlinePlayer.Platform = msg.Platform;           //平台名称	统一：ios|android|windows
                offlinePlayer.ClientVersion = msg.ClientVersion; //游戏的迭代版本，例如1.0.3
                offlinePlayer.DeviceModel = msg.DeviceModel;     //设备的机型，例如Samsung GT-I9208
                offlinePlayer.OsVersion = msg.OsVersion;         //操作系统版本，例如13.0.2
                offlinePlayer.Network = msg.Network;             //网络信息	4G/3G/WIFI/2G
                offlinePlayer.Mac = msg.Mac;                     //局域网地址             
                offlinePlayer.GameId = msg.GameId;

                OfflinePlayerEnterWorld(offlinePlayer, msg.ClientIp, playerEnter);
            }
            else
            {
                //playerEnter = Api.PCManager.GetPlayerEnter(msg.CharacterUid);
                //if (playerEnter != null && playerEnter.Player != null)
                if (fromTransform)
                {
                    CreateTransformPlayer(playerEnter);
                }
                else
                {
                    pc = new PlayerChar(Api, msg.CharacterUid);
                    pc.AccountName = msg.AccountName;
                    pc.BindGate(this);
                    pc.NeedSyncEnterWorld = msg.SyncData;
                    pc.RegisterId = msg.RegisterId;
                    pc.ClientIp = msg.ClientIp;
                    pc.DeviceId = msg.DeviceId;
                    pc.SDKUuid = msg.SdkUuid;
                    pc.ChannelName = msg.ChannelName;
                    pc.IsRebated = msg.IsRebate;

                    pc.ChannelId = msg.ChannelId;
                    pc.Idfa = msg.Idfa;       //苹果设备创建角色时使用
                    pc.Idfv = msg.Idfv;       //苹果设备创建角色时使用
                    pc.Imei = msg.Imei;       //安卓设备创建角色时使用
                    pc.Imsi = msg.Imsi;       //安卓设备创建角色时使用
                    pc.Anid = msg.Anid;       //安卓设备创建角色时使用
                    pc.Oaid = msg.Oaid;       //安卓设备创建角色时使用
                    pc.PackageName = msg.PackageName;//包名
                    pc.ExtendId = msg.ExtendId;   //广告Id，暂时不使用
                    pc.Caid = msg.Caid;     //暂时不使用

                    pc.Tour = msg.Tour;                   //是否是游客账号（0:非游客，1：游客）
                    pc.Platform = msg.Platform;           //平台名称	统一：ios|android|windows
                    pc.ClientVersion = msg.ClientVersion; //游戏的迭代版本，例如1.0.3
                    pc.DeviceModel = msg.DeviceModel;     //设备的机型，例如Samsung GT-I9208
                    pc.OsVersion = msg.OsVersion;         //操作系统版本，例如13.0.2
                    pc.Network = msg.Network;             //网络信息	4G/3G/WIFI/2G
                    pc.Mac = msg.Mac;                     //局域网地址
                    pc.GameId = msg.GameId;

                    if (playerEnter != null && playerEnter.DestMapInfo.NeedAnim)
                    {
                        pc.EnterMapInfo.SetNeedAnim();
                    }
                    pc.EnterMapInfo.SetMapInfo(msg.MapId, msg.Channel);
                    //LoadPlayer(pc, LoadPlayerStep.BaseInfo);
                    LoadPlayerWithQuerys(pc, LoadPlayerStep.BaseInfo);
                }
            }
        }

        //需要判断是否是跨zone
        private void OfflinePlayerEnterWorld(PlayerChar pc, string client_ip, PlayerEnter enter = null)
        {
            if (pc == null) return;
            pc.BindGate(this);
            //Log.Warn("offline pc online token {0}", pc.OnlineToken);
            Api.PCManager.RemoveOfflinePc(pc.Uid);
            Log.Debug($"player {pc.Uid} instance {pc.InstanceId} remove from offline");
            //pc.CheckContinuousLogin();
            pc.LastLoginTime = ZoneServerApi.now;
            pc.OnlineRewardTime = ZoneServerApi.now;

            pc.ClientIp = client_ip;

            //// 成功登陆 记录
            FieldMap map = Api.MapManager.GetFieldMap(pc.LastMapInfo.MapId, pc.LastMapInfo.Channel);

            if (map != null)
            {
                if (map.CanEnter(pc.InstanceId))
                {
                    if (map.IsDungeon)
                    {
                        DungeonMap dMap = map as DungeonMap;
                        if (dMap.State != DungeonState.Closed && dMap.State != DungeonState.Stopped)
                        {
                            Log.Debug($"player {pc.Uid} instance {pc.InstanceId} enter lastMap {pc.LastMapInfo.MapId} from offLine");
                            pc.RecordEnterMapInfo(map.MapId, map.Channel, pc.Position);
                            if (enter != null && enter.DestMapInfo != null)
                            {
                                if (enter != null && enter.DestMapInfo.NeedAnim)
                                {
                                    pc.EnterMapInfo.SetNeedAnim();
                                }
                                pc.RecordEnterMapInfo(enter.DestMapInfo.MapId, enter.DestMapInfo.Channel, enter.DestMapInfo.Position);
                            }
                            pc.LoadingDone(true);
                            pc.OnMoveMap(true);
                        }
                        else
                        {
                            Log.Debug($"player {pc.Uid} try enter last dungeon {pc.LastMapInfo.MapId} when map stopped or closed");
                            pc.LoadingDone(true);
                            pc.BackToOriginMap();
                        }
                    }
                    else
                    {
                        Log.Debug($"player {pc.Uid} instance {pc.InstanceId} enter lastMap {pc.LastMapInfo.MapId} from offLine");
                        pc.RecordEnterMapInfo(map.MapId, map.Channel, pc.Position);
                        if (enter != null && enter.DestMapInfo != null)
                        {
                            if (enter != null && enter.DestMapInfo.NeedAnim)
                            {
                                pc.EnterMapInfo.SetNeedAnim();
                            }
                            pc.RecordEnterMapInfo(enter.DestMapInfo.MapId, enter.DestMapInfo.Channel, enter.DestMapInfo.Position);
                        }
                        pc.LoadingDone(true);
                        pc.OnMoveMap(true);
                    }
                }
                else
                {
                    //坐标错误，踢回原先地图
                    Log.Warn("player {0} OfflinePlayer CanEnterMap false ", pc.Uid);
                    pc.LoadingDone(true);
                    pc.BackToOriginMap();
                }
            }
            else
            {
                //未找到之前地图，踢回原先地图
                Log.Warn("player {0} OfflinePlayer not find old map", pc.Uid);
                pc.LoadingDone(true);
                pc.BackToOriginMap();
            }
        }


        private void LoadPlayerWithQuerys(PlayerChar player, LoadPlayerStep step)
        {
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            Api.PCManager.AddLoadingPlayer(player.Uid, player);
            player.LoadingStartTime = ZoneServerApi.now;
            //string baseTableName = "character";1
            QueryLoadPlayerBasic queryBasic = new QueryLoadPlayerBasic(player.Uid);
            querys.Add(queryBasic);

            //string natureTableName = "character_nature";2
            //QueryLoadPlayerNature queryNature = new QueryLoadPlayerNature(player.Uid);
            //querys.Add(queryNature);

            //string resourceTableName = "character_resource";3
            QueryLoadPlayerResource queryResource = new QueryLoadPlayerResource(player.Uid);
            querys.Add(queryResource);

            //string taskCurTableName = "task_current";4
            QueryLoadTask taskCurInfo = new QueryLoadTask(player.Uid);
            querys.Add(taskCurInfo);

            QueryLoadTaskFinishState taskFinishState = new QueryLoadTaskFinishState(player.Uid);
            querys.Add(taskFinishState);

            //string emailItemTableName = "email_items";5
            QueryLoadEmailItem emailItem = new QueryLoadEmailItem(player.Uid);
            querys.Add(emailItem);

            //string emailTableName = "email";6
            QueryLoadEmail emailInfo = new QueryLoadEmail(player.Uid);
            querys.Add(emailInfo);

            //string shopTableName = "shop";8
            QueryLoadShopList shopInfo = new QueryLoadShopList(player.Uid);
            querys.Add(shopInfo);

            QueryLoadCommonShopList commonShopInfo = new QueryLoadCommonShopList(player.Uid);
            querys.Add(commonShopInfo);

            //string activityTableName = "activity_current";9
            QueryLoadActivity activityInfo = new QueryLoadActivity(player.Uid);
            querys.Add(activityInfo);

            QueryLoadSpecialActivity specialActivityInfo = new QueryLoadSpecialActivity(player.Uid);
            querys.Add(specialActivityInfo);
            QueryLoadRunAwayActivity runawayActivityInfo = new QueryLoadRunAwayActivity(player.Uid);
            querys.Add(runawayActivityInfo);

            QueryLoadWebPayRechargeRebate webPayRebateInfo = new QueryLoadWebPayRechargeRebate(player.Uid);
            querys.Add(webPayRebateInfo);

            //string rechargeTableName = "recharge";10
            //string historyTableName = "recharge_history";
            QueryGetRechargeManager rechargeInfo = new QueryGetRechargeManager(player.Uid);
            querys.Add(rechargeInfo);

            // pet ：11
            QueryLoadPet queryPet = new QueryLoadPet(player.Uid);
            querys.Add(queryPet);

            QueryLoadPetEggs queryPetEggs = new QueryLoadPetEggs(player.Uid);
            querys.Add(queryPetEggs);

            QueryLoadPetDungeonQueue queryPetQueues = new QueryLoadPetDungeonQueue(player.Uid);
            querys.Add(queryPetQueues);

            //string heroTableName = "hero";
            QueryLoadHero queryHero = new QueryLoadHero(player.Uid);
            querys.Add(queryHero);

            QueryLoadHeroPos queryHeroPos = new QueryLoadHeroPos(player.Uid);
            querys.Add(queryHeroPos);

            QueryLoadTravelHero queryTravelHero = new QueryLoadTravelHero(player.Uid);
            querys.Add(queryTravelHero);

            //string itemTableName = "items";7
            QueryLoadItem itemInfo = new QueryLoadItem(player.Uid);
            querys.Add(itemInfo);

            //string itemTableName = "fashions";12
            QueryLoadFashion fashionsInfo = new QueryLoadFashion(player.Uid);
            querys.Add(fashionsInfo);

            ////string itemTableName = "faceframes";13
            QueryLoadFaceFrame faceFrameInfo = new QueryLoadFaceFrame(player.Uid);
            querys.Add(faceFrameInfo);

            ////string itemTableName = "chatframes";14
            QueryLoadChatFrame chatFrameInfo = new QueryLoadChatFrame(player.Uid);
            querys.Add(chatFrameInfo);

            QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(player.Uid);
            querys.Add(querySoulBone);

            QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(player.Uid);
            querys.Add(querySoulRing);

            QueryLoadEquipment queryEquip = new QueryLoadEquipment(player.Uid);
            querys.Add(queryEquip);

            QueryLoadHeroFragment queryHeroFragment = new QueryLoadHeroFragment(player.Uid);
            querys.Add(queryHeroFragment);

            QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(player.Uid);
            querys.Add(queryEquipSlot);

            //string itemTableName = "faceframes";
            QueryLoadCurFaceFrameId queryCurFaceFrameId = new QueryLoadCurFaceFrameId(player.Uid);
            querys.Add(queryCurFaceFrameId);

            //string itemTableName = "chatframes";
            QueryLoadCurChatFrameId queryCurChatFrameId = new QueryLoadCurChatFrameId(player.Uid);
            querys.Add(queryCurChatFrameId);

            //string campTableName = "campstars";
            QueryLoadCampStars queryCampStars = new QueryLoadCampStars(player.Uid);
            querys.Add(queryCampStars);

            //string titleTableName = "title";
            //QeuryGetTitles queryTitle = new QeuryGetTitles(player.Uid, titleTableName);
            //querys.Add(queryTitle);

            //string counterTableName = "game_counter";
            QueryLoadPlayerCounter queryCounter = new QueryLoadPlayerCounter(player.Uid);
            querys.Add(queryCounter);

            QueryLoadPlayerCounterTime queryCounterTime = new QueryLoadPlayerCounterTime(player.Uid);
            querys.Add(queryCounterTime);

            //string delegationTableName = "delegation";
            QueryLoadDelegation queryDelegation = new QueryLoadDelegation(player.Uid);
            querys.Add(queryDelegation);

            QueryLoadHunting queryHunting = new QueryLoadHunting(player.Uid);
            querys.Add(queryHunting);

            QueryLoadArena queryArena = new QueryLoadArena(player.Uid);
            querys.Add(queryArena);

            QueryLoadCrossBattle queryCrossBattle = new QueryLoadCrossBattle(player.Uid);
            querys.Add(queryCrossBattle);

            QueryLoadSecretArea querySecretArea = new QueryLoadSecretArea(player.Uid);
            querys.Add(querySecretArea);

            QueryLoadPassCardReward queryPasscardRewards = new QueryLoadPassCardReward(player.Uid);
            querys.Add(queryPasscardRewards);

            QueryLoadPassCardTasks queryPasscardTasks = new QueryLoadPassCardTasks(player.Uid);
            querys.Add(queryPasscardTasks);

            QueryLoadDraw queryDraw = new QueryLoadDraw(player.Uid);
            querys.Add(queryDraw);

            QueryLoadChapter queryChapter = new QueryLoadChapter(player.Uid);
            querys.Add(queryChapter);

            QueryLoadGodPath queryGodPath = new QueryLoadGodPath(player.Uid);
            querys.Add(queryGodPath);


            //string activityTableName = "activity_current";9
            QueryLoadWelfareTrigger queryWelfareTriggerInfo = new QueryLoadWelfareTrigger(player.Uid);
            querys.Add(queryWelfareTriggerInfo);

            QueryLoadWishPoolInfo queryLoadWishPoolInfo = new QueryLoadWishPoolInfo(player.Uid);
            querys.Add(queryLoadWishPoolInfo);

            QueryLoadCampInfo queryLoadCampInfo = new QueryLoadCampInfo(player.Uid);
            querys.Add(queryLoadCampInfo);

            QueryLoadTower queryTower = new QueryLoadTower(player.Uid);
            querys.Add(queryTower);

            QueryLoadOnhook queryOnhook = new QueryLoadOnhook(player.Uid);
            querys.Add(queryOnhook);

            QueryPushFigure queryPushFigure = new QueryPushFigure(player.Uid);
            querys.Add(queryPushFigure);

            QueryLoadResonanceInfo queryResonanceInfo = new QueryLoadResonanceInfo(player.Uid);
            querys.Add(queryResonanceInfo);

            QueryLoadRankReward queryRankReward = new QueryLoadRankReward(player.Uid);
            querys.Add(queryRankReward);

            QueryLoadGiftCodeUse queryGiftCodeUse = new QueryLoadGiftCodeUse(player.Uid);
            querys.Add(queryGiftCodeUse);

            QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(player.Uid);
            querys.Add(queryLoadHeroGod);

            QueryLoadAction queryLoadAction = new QueryLoadAction(player.Uid);
            querys.Add(queryLoadAction);

            QueryLoadGiftList queryLoadGiftList = new QueryLoadGiftList(player.Uid);
            querys.Add(queryLoadGiftList);

            QueryLoadTimingRecommendGift queryTimingRecommendGift = new QueryLoadTimingRecommendGift(player.Uid, api.Now());
            querys.Add(queryTimingRecommendGift);

            QueryLoadLimitTimeGiftList queryLimitTimeGifts = new QueryLoadLimitTimeGiftList(player.Uid);
            querys.Add(queryLimitTimeGifts);

            QueryLoadTreasurePuzzle queryTreasurePuzzle = new QueryLoadTreasurePuzzle(player.Uid);
            querys.Add(queryTreasurePuzzle);

            QueryLoadThemePass queryThemePass = new QueryLoadThemePass(player.Uid);
            querys.Add(queryThemePass);

            QueryLoadThemeBoss queryThemeBoss = new QueryLoadThemeBoss(player.Uid);
            querys.Add(queryThemeBoss);

            QeuryGetTitles queryTitle = new QeuryGetTitles(player.Uid);
            querys.Add(queryTitle);

            QueryLoadCultivateGiftList queryCultivateGifts = new QueryLoadCultivateGiftList(player.Uid);
            querys.Add(queryCultivateGifts);

            QueryLoadpettyGiftItems queryPettyGiftItems = new QueryLoadpettyGiftItems(player.Uid);
            querys.Add(queryPettyGiftItems);

            QueryLoadCrossBossCounter queryCrossBossCounter = new QueryLoadCrossBossCounter(player.Uid);
            querys.Add(queryCrossBossCounter);

            QueryLoadDailyRecharge queryDailyRecharge = new QueryLoadDailyRecharge(player.Uid);
            querys.Add(queryDailyRecharge);

            QueryLoadHeroDaysRewards queryHeroDaysRewards = new QueryLoadHeroDaysRewards(player.Uid);
            querys.Add(queryHeroDaysRewards);

            QueryLoadNewServerPromotion queryNewServerPromotion = new QueryLoadNewServerPromotion(player.Uid);
            querys.Add(queryNewServerPromotion);
            QueryLoadGarden queryLoadGarden = new QueryLoadGarden(player.Uid);
            querys.Add(queryLoadGarden);

            QueryLoadDivineLoveInfo queryDivineLove = new QueryLoadDivineLoveInfo(player.Uid);
            querys.Add(queryDivineLove);

            QueryLoadLuckyFlipCard queryLuckyFlipCard = new QueryLoadLuckyFlipCard(player.Uid);
            querys.Add(queryLuckyFlipCard);

            QueryLoadIslandHigh queryLoadIslandHigh = new QueryLoadIslandHigh(player.Uid);
            querys.Add(queryLoadIslandHigh);

            QueryLoadIslandHighGiftInfo queryIslandHighGift = new QueryLoadIslandHighGiftInfo(player.Uid);
            querys.Add(queryIslandHighGift);

            QueryLoadTrident queryLoadTrident = new QueryLoadTrident(player.Uid);
            querys.Add(queryLoadTrident);

            QueryLoadDragonBoat queryDragonBoat = new QueryLoadDragonBoat(player.Uid);
            querys.Add(queryDragonBoat);

            QueryLoadStoneWallInfo queryStoneWall = new QueryLoadStoneWallInfo(player.Uid);
            querys.Add(queryStoneWall);

            QueryLoadIslandChallenge queryLoadIslandChallenge = new QueryLoadIslandChallenge(player.Uid);
            querys.Add(queryLoadIslandChallenge);


            QueryLoadCarnivalBoss queryCarnivalBoss = new QueryLoadCarnivalBoss(player.Uid);
            querys.Add(queryCarnivalBoss);

            QueryLoadCarnivalRecharge queryCarnivalRecharge = new QueryLoadCarnivalRecharge(player.Uid);
            querys.Add(queryCarnivalRecharge);

            QueryLoadCarnivalMall queryCarnivalMall = new QueryLoadCarnivalMall(player.Uid);
            querys.Add(queryCarnivalMall);

            QueryLoadHiddenWeapon queryHiddenWeapon = new QueryLoadHiddenWeapon(player.Uid);
            querys.Add(queryHiddenWeapon);

            QueryLoadShrekInvitation queryShrekInvitation = new QueryLoadShrekInvitation(player.Uid);
            querys.Add(queryShrekInvitation);

            QueryLoadRoulette queryLoadRoulette = new QueryLoadRoulette(player.Uid);
            querys.Add(queryLoadRoulette);

            QueryLoadCanoeInfo queryCanoe = new QueryLoadCanoeInfo(player.Uid);
            querys.Add(queryCanoe);

            QueryLoadMainBattleQueue queryMainBattleQueue = new QueryLoadMainBattleQueue(player.Uid);
            querys.Add(queryMainBattleQueue);

            QueryLoadFirstOrderInfo queryFisrtOrder = new QueryLoadFirstOrderInfo(player.Uid);
            querys.Add(queryFisrtOrder);
            QueryLoadMidAutumnInfo queryMidAutumn = new QueryLoadMidAutumnInfo(player.Uid);
            querys.Add(queryMidAutumn);

            QueryLoadThemeFirework queryThemeFirework = new QueryLoadThemeFirework(player.Uid);
            querys.Add(queryThemeFirework);

            QueryLoadCrossChallenge queryCrossChallenge= new QueryLoadCrossChallenge(player.Uid);
            querys.Add(queryCrossChallenge);

            QueryLoadNineTest queryNineTest = new QueryLoadNineTest(player.Uid);
            querys.Add(queryNineTest);

            QueryLoadPlayerWarehouseResource queryWarehouseResource = new QueryLoadPlayerWarehouseResource(player.Uid);
            querys.Add(queryWarehouseResource);
            
            WareHouseModel whModel = WarehouseLibrary.GetConfig((int)ItemWarehouseType.SoulRing);          
            QueryLoadWarehouseSoulRing queryWarehouseSoulRing = new QueryLoadWarehouseSoulRing(player.Uid, whModel);
            querys.Add(queryWarehouseSoulRing);

            QueryLoadDiamondRebate queryDiamondRebate = new QueryLoadDiamondRebate(player.Uid);
            querys.Add(queryDiamondRebate);

            QueryLoadXuanBox queryQueryLoadXuanBox = new QueryLoadXuanBox(player.Uid);
            querys.Add(queryQueryLoadXuanBox);

            QueryLoadWishLantern queryLoadWishLantern = new QueryLoadWishLantern(player.Uid);
            querys.Add(queryLoadWishLantern);

            QueryLoadHuntingIntrude queryHuntingIntrude = new QueryLoadHuntingIntrude(player.Uid);
            querys.Add(queryHuntingIntrude);

            QueryLoadSchoolInfo queryLoadSchoolInfo = new QueryLoadSchoolInfo(player.Uid);
            querys.Add(queryLoadSchoolInfo);

            QueryLoadSchoolTaskFinishInfo querySchoolTaskFinish = new QueryLoadSchoolTaskFinishInfo(player.Uid);
            querys.Add(querySchoolTaskFinish);

            QueryLoadSchoolTasksInfo querySchoolTasks = new QueryLoadSchoolTasksInfo(player.Uid);
            querys.Add(querySchoolTasks);

            QueryLoadAnswerQuestionInfo queryAnswerQuestion = new QueryLoadAnswerQuestionInfo(player.Uid);
            querys.Add(queryAnswerQuestion);

            QueryLoadTreasureFlipCard queryTreasureFlipCard = new QueryLoadTreasureFlipCard(player.Uid);
            querys.Add(queryTreasureFlipCard);

            QueryLoadDaysRecharge queryLoadDaysRecharge = new QueryLoadDaysRecharge(player.Uid);
            querys.Add(queryLoadDaysRecharge);

            QueryLoadShrekland queryLoadShrekland = new QueryLoadShrekland(player.Uid);
            querys.Add(queryLoadShrekland);

            QueryLoadSpaceTimeTower queryLoadSpaceTimeTower = new QueryLoadSpaceTimeTower(player.Uid);
            querys.Add(queryLoadSpaceTimeTower);

            QueryLoadSpaceTimeHero queryLoadSpaceTimeHero = new QueryLoadSpaceTimeHero(player.Uid);
            querys.Add(queryLoadSpaceTimeHero);

            QueryLoadDevilTraining queryLoadDevilTraining = new QueryLoadDevilTraining(player.Uid);
            querys.Add(queryLoadDevilTraining);

            /*\ 神域赐福 /*/
            QueryLoadDomainBenedictionInfo queryDomainBenedictionInfo = new QueryLoadDomainBenedictionInfo(player.Uid);
            querys.Add(queryDomainBenedictionInfo);

            QueryLoadDriftExploreInfo queryDriftExplore = new QueryLoadDriftExploreInfo(player.Uid);
            querys.Add(queryDriftExplore);

            QueryLoadDriftExploreTaskInfo queryDriftExploreTasks = new QueryLoadDriftExploreTaskInfo(player.Uid);
            querys.Add(queryDriftExploreTasks);
            
            //string skillQueueTableName = "skill_queue";
            //QueryLoadSkillQueue querySkillQueue = new QueryLoadSkillQueue(player.Uid, skillQueueTableName);
            //querys.Add(querySkillQueue);

            //string heroQueueTableName = "hero_queue";
            //QueryLoadSkillQueue queryHeroQueue = new QueryLoadSkillQueue(player.Uid, heroQueueTableName);
            //querys.Add(queryHeroQueue);

            //string gameLevelTableName = "game_level";
            //QueryLoadGameLevel gameLevelInfo = new QueryLoadGameLevel(player.Uid, gameLevelTableName);
            //querys.Add(gameLevelInfo);


            DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);

            Log.Write("player {0} enter call db querys start", player.Uid);
            api.GameDBPool.Call(dBQuerysWithoutTransaction, ret =>
            {
                if ((int)ret == 0)
                {
                    if (IsInFreezeState(queryBasic.FreezeState, queryBasic.FreezeTime))
                    {
                        MSG_ZGC_LOGIN_FREEZE msg = new MSG_ZGC_LOGIN_FREEZE();
                        msg.State = (int)queryBasic.FreezeState;
                        msg.Time = queryBasic.FreezeTime.ToString(CONST.DATETIME_TO_STRING_1);
                        msg.Reason = queryBasic.FreezeReason;
                        player.Write(msg);
                        return;
                    }

                    player.Name = queryBasic.CharName;
                    if (!player.SDKUuid.Equals(queryBasic.SDKUuid))
                    {
                        //更新db
                    }
                    player.Sex = queryBasic.Sex;
                    player.Level = queryBasic.Level;
                    player.FollowerId = queryBasic.Follower;
                    player.HeroId = queryBasic.HeroId;
                    player.GodType = queryBasic.GodType;
                    player.TimeCreated = queryBasic.TimeCreated;
                    player.GuideId = queryBasic.GuideId;
                    player.MainTaskId = queryBasic.MainTaskId;
                    player.SetMainTaskId(queryBasic.MainTaskId);
                    player.ResonanceLevel = queryBasic.ResonanceLevel;
                    player.BranchTaskIds.AddRange(queryBasic.BranchTaskIds);

                    player.AccountName = queryBasic.AccountName;
                    player.MainId = queryBasic.MainId;
                    player.SourceMain = queryBasic.SourceMain;
                    player.BattlePower = queryBasic.BattlePower;
                    MapModel mapModel = MapLibrary.GetMap(player.EnterMapInfo.MapId);
                    if (mapModel != null)
                    {
                        if (queryBasic.MapId != player.EnterMapInfo.MapId)
                        {
                            // db中的map id与要进入的map id不同 （可能被manager拉回主城）,需要校正初始位置
                            player.SetPosition(mapModel.BeginPos);
                            Log.Warn("player {0} LoadPlayerWithQuerys enter map {1} error, Channel {2}", player.Uid, queryBasic.MapId, queryBasic.Channel);
                        }
                        else
                        {
                            if (mapModel.MapType != MapType.Map)
                            {
                                player.EnterMapInfo.SetMapInfo(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL);
                                mapModel = MapLibrary.GetMap(player.EnterMapInfo.MapId);
                                player.SetPosition(mapModel.BeginPos);
                                Log.Warn("player {0} LoadPlayerWithQuerys enter map {1} error, Channel {2} map type is {3}", player.Uid, queryBasic.MapId, queryBasic.Channel, mapModel.MapType);
                            }
                            else
                            {
                                player.SetPosition(new Vec2(queryBasic.PosX, queryBasic.PosY));
                            }
                        }
                    }
                    else
                    {
                        player.EnterMapInfo.SetMapInfo(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL);
                        mapModel = MapLibrary.GetMap(player.EnterMapInfo.MapId);
                        player.SetPosition(mapModel.BeginPos);
                        Log.Warn("player {0} LoadPlayerWithQuerys enter map {1} error, Channel {2} not find model", player.Uid, queryBasic.MapId, queryBasic.Channel);
                    }

                    //进入地图坐标
                    player.EnterMapInfo.SetPosition(player.Position);
                    player.Icon = player.HeroId;
                    //player.Icon = queryBasic.FaceIcon;
                    player.ShowDIYIcon = queryBasic.ShowFaceJpg;

                    //玩家当前头像框
                    player.BagManager.FaceFrameBag.CurFaceFrameId = queryCurFaceFrameId.TypeId;
                    //玩家当前气泡框
                    player.BagManager.ChatFrameBag.CurChatFrameId = queryCurChatFrameId.TypeId;

                    //player.PetId = queryBasic.PetId;

                    player.LastRefreshTime = queryBasic.LastRefreshTime;
                    player.LastOfflineTime = queryBasic.LastOfflineTime;
                    player.LastLevelUpTime = queryBasic.LastLevelUpTime;
                    player.LastLoginTime = queryBasic.LastLoginTime;

                    player.SilenceTime = queryBasic.SilenceTime;
                    player.SilenceReason = queryBasic.SilenceReason;
                    player.IsGm = queryBasic.Gm;
                    player.MainLineId = queryBasic.MainLineId;
                    //player.LastPhyRecoveryTime = queryBasic.LastPhyRecoveryTime;

                    player.Job = (JobType)queryBasic.Job;
                    player.BagSpace = queryBasic.BagSpace;
                    player.Camp = (CampType)queryBasic.Camp;
                    player.HisPrestige = queryBasic.HisPrestige;
                    //player.HuntingCount = queryBasic.HunrtingCount ;
                    player.IntegralBossLastTime = queryBasic.IntegralBossLastTime;

                    player.Passcard = queryPasscardRewards.Passcard;
                    player.PasscardLevel = queryPasscardRewards.PasscardLevel;
                    player.PasscardExp = queryResource.Currencies[CurrenciesType.PasscardExp];

                    player.AcroessOceanDiff = queryBasic.AcroessOceanDiff;

                    player.CumulateDays = queryBasic.CumulateDays;
                    if (queryBasic.LastLoginTime.Date != Api.Now().Date)
                    {
                        player.CumulateDays++;
                    }
                    player.CumulateOnlineTime = queryBasic.CumulateOnlineTime;

                    // 初始化伙伴列表
                    player.InitHero(queryHero.HeroList);
                    //上阵信息
                    player.HeroMng.InitHeroPos(queryHeroPos.List);

                    //player.InitBasicNature(queryNature.Model);
                    // 临时设置移动速度代码
                    //player.SetNatureValue(NatureType.PRO_SPD,(int)(3.125f * 1000));

                    //step2
                    player.BindCurrencies(queryResource.Currencies);
                    //player.Currencies[CurrenciesType.exp] = queryResource.Exp;
                    //player.Currencies[CurrenciesType.diamond] = queryResource.Diamond;
                    //player.Currencies[CurrenciesType.gold] = queryResource.Gold;
                    //player.Currencies[CurrenciesType.soulPower] = queryResource.SoulPower;
                    //player.Currencies[CurrenciesType.soulCrystal] = queryResource.SoulCrystal;
                    //player.Currencies[CurrenciesType.soulDust] = queryResource.SoulDust;
                    //player.Currencies[CurrenciesType.soulBreath] = queryResource.SoulBreath;
                    //player.Currencies[CurrenciesType.friendlyHeart] = queryResource.FriendlyHeart;

                    //绑定counter
                    player.BindCounterList(queryCounter.CounterList, queryCounterTime.TimeList);

                    //step3
                    player.InitTaskItemList(taskCurInfo.List, taskFinishState.TaskFinishState);

                    //step4
                    player.BindEmailItems(emailItem.List);

                    //step5
                    player.BindSystemEmails(emailInfo.Ids);

                    //step6 各种背包的数据需要风别加载
                    player.BagManager.NormalBag.LoadItems(itemInfo.List);
                    player.BagManager.FashionBag.LoadItems(fashionsInfo.List);
                    player.BagManager.FaceFrameBag.LoadItems(faceFrameInfo.List);
                    player.BagManager.ChatFrameBag.LoadItems(chatFrameInfo.List);
                    player.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
                    player.BagManager.EquipBag.LoadItems(queryEquip.List);
                    player.BagManager.SoulBoneBag.LoadItems(querySoulBone.List, true);
                    player.BagManager.HeroFragmentBag.LoadItems(queryHeroFragment.List);
                    player.BagManager.HiddenWeaponBag.LoadItems(queryHiddenWeapon.WeaponList);
                    //player.BagManager

                    player.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);

                    //step7
                    player.ShopManager.BindShopList(shopInfo.ShopList);



                    //step8
                    player.LoadActivityList(activityInfo.List);
                    player.LoadSpecialActivityList(specialActivityInfo.List);
                    player.LoadRunawayActivityList(runawayActivityInfo.RunawayType, runawayActivityInfo.RunawayTime, runawayActivityInfo.DataBox, runawayActivityInfo.List);
                    player.LoadWebPayRebateActivityLit(webPayRebateInfo.List, webPayRebateInfo.MoneyDic, webPayRebateInfo.LoginMarkDic);
                    //step9
                    //if (rechargeInfo.Products != null && rechargeInfo.Rewards != null)
                    //{
                        player.BindRechargeManager(rechargeInfo.First, rechargeInfo.Total, rechargeInfo.Current, rechargeInfo.Daily, rechargeInfo.Pice, rechargeInfo.Money, rechargeInfo.historys, rechargeInfo.AccumulateOnceMaxMoney, rechargeInfo.LastCommonRechargeTime, rechargeInfo.PayCount);
                        player.BindOperationalActivity(rechargeInfo.MonthCardTime, rechargeInfo.SeasonCardTime, rechargeInfo.WeekCardStart, rechargeInfo.WeekCardEnd, rechargeInfo.MonthCardState, rechargeInfo.SuperMonthCardTime,
                            rechargeInfo.SuperMonthCardState, rechargeInfo.SeasonCardState, rechargeInfo.AccumulateRechargeRewards, rechargeInfo.NewRechargeGiftScore, rechargeInfo.NewRechargeGiftRewards, rechargeInfo.GrowthFund);
                    //}
                    //else
                    //{
                    //    Log.Warn("load player {0} step {1} failed", player.Uid, "recharge");
                    //    return;
                    //}
                    // step 10
                    player.InitPets(queryPet.PetList);
                    player.InitPetEggs(queryPetEggs.PetEggList);
                    player.InitPetDungeonQueues(queryPetQueues.DungeonQueues);

                    //step 11 加载玩家阵营养成信息
                    player.DragonLevel = queryCampStars.DragonLevel;
                    player.TigerLevel = queryCampStars.TigerLevel;
                    player.PhoenixLevel = queryCampStars.PhoenixLevel;
                    player.TortoiseLevel = queryCampStars.TortoiseLevel;

                    //漫游记
                    player.InitTravelManager(queryTravelHero.heroList);

                    //羁绊
                    player.HeroMng.InitCombo(queryDraw.HeroCombo);

                    //step 13
                    player.BindDelegationItem(queryDelegation.Model);

                    //step 14 猎杀魂兽
                    player.HuntingManager.BindHuntingInfo(queryHunting.Research, queryHunting.HuntingInfo, queryHunting.ActivityUnlock, queryHunting.ActivityPassed);
                    //凶兽入侵
                    player.HuntingManager.InitHuntingIntrudeInfo(queryHunting.IntrudeHeroPos, queryHuntingIntrude.huntingIntrudeInfos);

                    //竞技场
                    player.ArenaMng.Init(queryArena.Info);
                    player.DailyRankReward = queryArena.Info.DailyRankReward;

                    //跨服战
                    player.CrossInfoMng.Init(queryCrossBattle.Info);
                    //player.GetCrossSeasonRank();

                    //秘境
                    player.SecretAreaManager.BindSecretAreaInfo(querySecretArea.SecretAreaId, querySecretArea.state, querySecretArea.passTime);

                    //通行证
                    player.LoadDBPassCardInfo(queryPasscardRewards);
                    player.LoadDBTaskItemList(queryPasscardTasks.List);

                    //抽奖
                    player.DrawMng.Init(queryDraw.HeroDraw, queryDraw.Constellation);
                    player.InitDrawManagerInfo();


                    //player.CurTitleId = queryTitle.CurTitle;
                    //player.LoadTitles(queryTitle.Title);
                    //player.LoadFishCount(queryTitle.FishCount);

                    //step3



                    //章节
                    //player.ChapterManager.Init(queryChapter.ChapterList, queryBasic.PowerRecoryLastTime, queryBasic.PowerLimit);

                    //成神之路
                    player.GodPathManager.InitDBInfo(queryGodPath.GodPathHero);

                    player.InitBenefitPassedDungeon(queryBasic.BenefitDungeon);

                    //福利
                    player.LoadWelfareInfoList(queryWelfareTriggerInfo.List);

                    //许愿池
                    player.LoadWishPool(queryLoadWishPoolInfo);

                    //通用商城
                    player.ShopManager.BindCommonShop(commonShopInfo.ShopList);

                    //阵营建设
                    player.LoadCampBuildInfo(queryLoadCampInfo);

                    //阵营战
                    player.LoadCampBattleInfo(queryLoadCampInfo);

                    //爬塔
                    player.TowerManager.Init(queryTower.TowerDBInfo);

                    //挂机
                    player.OnhookManager.Init(queryOnhook.TireId, queryOnhook.LastRewardTime, queryOnhook.LastLookTime, queryOnhook.LastRandomTime, queryOnhook.RandomReward);

                    //推图
                    player.LoadPushFigureInfo(queryPushFigure.Id, queryPushFigure.Status);

                    //武魂共鳴
                    player.LoadResonanceInfo(queryResonanceInfo.ResonanceInfoDic);

                    player.InitRankReward(queryRankReward.idList);

                    //礼包码
                    player.InitGiftCodeUseInfo(queryGiftCodeUse.List);

                    //成神
                    player.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);

                    //玩家行为触发
                    player.InitActionInfo(queryLoadAction.ActionInfos, queryTimingRecommendGift.TimingGiftInfos);

                    //礼包
                    player.InitGiftInfo(queryLoadGiftList.GiftList, queryLimitTimeGifts.Gift, queryCultivateGifts.GiftList, queryPettyGiftItems.Gift, queryDailyRecharge.List, queryHeroDaysRewards.List, queryNewServerPromotion.List);

                    //藏宝拼图
                    player.InitTreasurePuzzle(queryTreasurePuzzle.PuzzleItem);

                    //主题通行证
                    player.InitThemePassInfo(queryThemePass.List);

                    //主题Boss
                    player.InitThemeBossInfo(queryThemeBoss);

                    //称号
                    player.InitTitleInfo(queryTitle.TitleList);

                    //跨服boss
                    player.CrossBossInfoMng.Init(queryCrossBossCounter.Info);

                    //幽香花园
                    player.GardenManager.Init(queryLoadGarden.GardenInfo);

                    //乾坤问情
                    player.DivineLoveMng.Init(queryDivineLove.InfoList);

                    //幸运翻翻乐
                    player.GiftManager.BindLuckyFlipCardInfo(queryLuckyFlipCard.List);

                    //海岛登高
                    player.IslandHighManager.Init(queryLoadIslandHigh.IslandHighDbInfo);
                    //海岛登高礼包
                    player.GiftManager.BindIslandHighGiftInfo(queryIslandHighGift.InfoList);

                    //三叉戟
                    player.TridentManager.Init(queryLoadTrident.TridentDbInfo);

                    //端午活动
                    player.DragonBoatManager.Init(queryDragonBoat.Info);

                    //昊天石壁
                    player.StoneWallMng.Init(queryStoneWall.InfoList);

                    //海岛挑战
                    player.IslandChallengeManager.Init(queryLoadIslandChallenge.IslandChallengeDBInfo);

                    //嘉年华Boss
                    player.CarnivalBossMng.Init(queryCarnivalBoss.Info);

                    //嘉年华
                    player.CarnivalManager.InitRechargeInfo(queryCarnivalRecharge.Info);
                    player.CarnivalManager.InitMallInfo(queryCarnivalMall.List);

                    //史莱克邀约
                    player.ShrekInvitationMng.Init(queryShrekInvitation.List);

                    //轮盘
                    player.RouletteManager.Init(queryLoadRoulette.Info);

                    //皮划艇
                    player.CanoeManager.Init(queryCanoe.Info);

                    //多阵容
                    player.HeroMng.InitMainBattleQueue(queryMainBattleQueue.InfoList);
                    
                    //魂师挑战
                    player.CrossChallengeInfoMng.Init(queryCrossChallenge.Info);

                    //首充订单
                    player.BindFirstOrderInfo(queryFisrtOrder.Info);

                    //中秋
                    player.MidAutumnMng.Init(queryMidAutumn.Info);

                    //主题烟花
                    player.ThemeFireworkMng.Init(queryThemeFirework.Info);

                    //九考试炼
                    player.NineTestMng.Init(queryNineTest.Info);

                    //资源仓库
                    player.BindWarehouseCurrencies(queryWarehouseResource.Currencies);

                    //魂环仓库
                    player.BindWarehouseSoulRings(queryWarehouseSoulRing.List);

                    //钻石返利
                    player.InitDiamondRebateInfo(queryDiamondRebate.Info);

                    //玄天宝箱
                    player.XuanBoxManager.Init(queryQueryLoadXuanBox.Info);

                    //九笼祈愿
                    player.WishLanternManager.Init(queryLoadWishLantern.Info);

                    //学院
                    player.SchoolManager.Init(queryLoadSchoolInfo.SchoolInfo);

                    //学院任务信息
                    player.SchoolManager.InitSchoolTaskInfo(querySchoolTaskFinish.FinishInfo, querySchoolTasks.List);

                    //学院答题
                    player.SchoolManager.InitAnswerQuestionInfo(queryAnswerQuestion.List);

                    player.DaysRechargeManager.Init(queryLoadDaysRecharge.Info);

                    //夺宝翻翻乐
                    player.GiftManager.BindTreasureFlipCardInfo(queryTreasureFlipCard.List);

                    //史莱克乐园
                    player.ShreklandMng.Init(queryLoadShrekland.Info);

                    //时空塔
                    player.SpaceTimeTowerMng.Init(queryLoadSpaceTimeTower.Info, queryLoadSpaceTimeHero.List);

                    //魔鬼训练
                    player.DevilTrainingMng.Init(queryLoadDevilTraining.List);

                    //神域赐福
                    player.ODomainBenedictionMng.Init(queryDomainBenedictionInfo.oDbDomainInfo);

                    //漂流探宝
                    player.DriftExploreMng.Init(queryDriftExplore.Info, queryDriftExploreTasks.List);
                    
                    //一开始初始化FSM会报错
                    player.InitFSMAfterHero();

                    //初始化伙伴属性
                    player.BindHerosNature();
                    //初始化宠物属性
                    player.BindPetsNature();

                    // 数据加载完毕 进入世界
                    Log.Write("load player {0} success", player.Uid);
                    Api.PCManager.AddLoadingDonePlayer(player);


                    //player.InitCounter(CounterType.GiveHeartCountBuy, queryCounter.HeartGiveCount);
                    //player.InitCounter(CounterType.TakeHeartCountBuy, queryCounter.HeartTakeCount);
                    //player.InitCounter(CounterType.CampBlessingCount, queryCounter.CampBlessingCount);
                    //player.InitCounter(CounterType.BenefitsGold, queryCounter.BenefitsGold);
                    //player.InitCounter(CounterType.BenefitsGoldBuy, queryCounter.BenefitsGoldBuy);
                    //player.InitCounter(CounterType.BenefitsExp, queryCounter.BenefitsExp);
                    //player.InitCounter(CounterType.BenefitsExpBuy, queryCounter.BenefitsExpBuy);
                    //player.InitCounter(CounterType.BenefitsSoulPower, queryCounter.BenefitsSoulPower);
                    //player.InitCounter(CounterType.BenefitsSoulPowerBuy, queryCounter.BenefitsSoulPowerBuy);
                    //player.InitCounter(CounterType.BenefitsSoulBreath, queryCounter.BenefitsSoulBreath);
                    //player.InitCounter(CounterType.BenefitsSoulBreathBuy, queryCounter.BenefitsSoulBreathBuy);
                    //player.InitCounter(CounterType.HuntingBuy, queryCounter.HuntingBuy);
                    //player.InitCounter(CounterType.HuntingRes, queryCounter.HuntingRes);
                    //player.InitCounter(CounterType.IntegralBoss, queryCounter.IntegralBoss);
                    //player.InitCounter(CounterType.IntegralBossBuy, queryCounter.IntegralBossBuy);
                    //player.InitCounter(CounterType.TeamHelpCount, queryCounter.TeamHelpCount);

                    //player.InitCounter(CounterType.TipOff, queryCounter.TipOffCount);
                    //player.InitCounter(CounterType.LikeSpace, queryCounter.LikeSpace);
                    //player.InitCounter(CounterType.Suggest, queryCounter.SuggestCount);
                    //player.InitCounter(CounterType.DailyQuestion, queryCounter.DailyQuestion);
                    //player.InitCounter(CounterType.LadderTotalWinNum, queryCounter.LadderTotalWinNum);
                    //player.InitCounter(CounterType.BattleGold, queryCounter.BattleGold);
                    //player.InitCounter(CounterType.BattleDiamond, queryCounter.BattleDiamond);
                    //player.InitCounter(CounterType.BattleReward, queryCounter.BattleReward);
                    //player.InitCounter(CounterType.ItemGameLevelInvited, queryCounter.ItemGameLevelInvited);
                    //player.InitCounter(CounterType.BossTeamReward, queryCounter.BossTeamReward);
                    //player.InitCounter(CounterType.OnePieceChallenge, queryCounter.OnePieceChallenge);
                    //player.InitCounter(CounterType.OnePieceSweep, queryCounter.OnePieceSweep);
                    //player.InitCounter(CounterType.GetBait, queryCounter.GetBait);

                    ////step4
                    //foreach (var item in querySkill.SkillList)
                    //{
                    //    Skill skill = new Skill(item.OwnerUid, item.Id, item.Position0, item.Position1, item.Position2, item.Position3, item.Position4, item.Level, item.Exp, item.Count);
                    //    player.SkillMng.LoadSkill(skill);
                    //}

                    ////step6
                    //player.SkillMng.SetQueueName(0, querySkillQueue.Model.QueueName_0);
                    //player.SkillMng.SetQueueName(1, querySkillQueue.Model.QueueName_1);
                    //player.SkillMng.SetQueueName(2, querySkillQueue.Model.QueueName_2);
                    //player.SkillMng.SetQueueName(3, querySkillQueue.Model.QueueName_3);
                    //player.SkillMng.SetQueueName(4, querySkillQueue.Model.QueueName_4);
                    //player.SkillMng.SetCurQueue(querySkillQueue.Model.CurQueue);

                    ////step7
                    //player.HeroMng.SetQueueName(0, queryHeroQueue.Model.QueueName_0);
                    //player.HeroMng.SetQueueName(1, queryHeroQueue.Model.QueueName_1);
                    //player.HeroMng.SetQueueName(2, queryHeroQueue.Model.QueueName_2);
                    //player.HeroMng.SetQueueName(3, queryHeroQueue.Model.QueueName_3);
                    //player.HeroMng.SetQueueName(4, queryHeroQueue.Model.QueueName_4);
                    //player.HeroMng.SetCurQueue(queryHeroQueue.Model.CurQueue);

                    ////step8
                    //player.ChestMng.LoadChestInfo(chestInfo.Model);

                    //player.GameLevelMng.Bind(gameLevelInfo.FinishNormal, gameLevelInfo.DuringNormal, gameLevelInfo.FinishDifficulty, gameLevelInfo.DuringDifficulty, gameLevelInfo.GetRewardStory, gameLevelInfo.OnePiece);
                    //player.BindRadioGotRadioRewards(gameLevelInfo.GetContributionReward);

                }
                else
                {
                    // 未找到该角色
                    Log.Warn("load player {0} step {1} failed", player.Uid, step);
                    return;
                }
            });

        }

        private bool IsInFreezeState(FreezeState freeze_state, DateTime freeze_time)
        {
            switch (freeze_state)
            {
                case FreezeState.Normal:
                    return false;
                case FreezeState.Freeze:
                    return freeze_time > ZoneServerApi.now;
                case FreezeState.ForeverFreeze:
                    return true;
            }
            return true;
        }

        private void CreateTransformPlayer(PlayerEnter playerEnter)
        {
            if (playerEnter == null || playerEnter.Player == null)
            {
                return;
            }

            // 移除跨zone暂留缓存
            Api.PCManager.RemovePlayerEnter(playerEnter.Uid);

            PlayerChar player = playerEnter.Player;
            if (playerEnter != null && playerEnter.DestMapInfo.NeedAnim)
            {
                player.EnterMapInfo.SetNeedAnim();
            }
            player.BindGate(this);
            player.InitFSMAfterHero();
            Api.PCManager.AddPc(playerEnter.Uid, player, false);


            // 移除
            FieldMap map = null;
            // 进入普通地图前判断
            map = Api.MapManager.GetFieldMap(playerEnter.DestMapInfo.MapId, playerEnter.DestMapInfo.Channel);
            bool turnBack = false;
            if (map != null && playerEnter.TransformDone)
            {
                if (map.IsWalkableAt((int)Math.Round(playerEnter.DestMapInfo.Position.x), (int)Math.Round(playerEnter.DestMapInfo.Position.y), true))
                {
                    //clientEnter已经分配好指定的map和channel 直接进入即可
                    player.SetPosition(new Vec2(playerEnter.DestMapInfo.Position));
                }
                else
                {
                    turnBack = true;
                }
            }
            else
            {
                turnBack = true;
            }
            if (turnBack == true)
            {
                // 主城1线可能不在当前地图
                player.BackToMainCity();
            }
            else
            {
                player.RecordEnterMapInfo(playerEnter.DestMapInfo.MapId, playerEnter.DestMapInfo.Channel, player.Position);
                // 进入地图或副本
                player.OnMoveMap();
            }
        }

        public void OnResponse_LeaveWorld(MemoryStream stream, int uid = 0)
        {
            // 客户端断开连接 gate通知zone处理player相关
            MSG_GateZ_LeaveWorld msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LeaveWorld>(stream);
            Log.Write("player {0} request leave world", msg.Uid);
            PlayerChar player = null;
            if (Api.PCManager.PcList.TryGetValue(msg.Uid, out player))
            {

                player.LeaveWorld(player.InDungeon);
            }
            else
            {
                // 不在pclist中，可能是正在从数据库loading，没有完成加载，未add到pclist
                Api.PCManager.RemoveLoadingPlayer(msg.Uid);
            }
        }

        public void OnResponse_MapLoadingDone(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_MAP_LOADING_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_MAP_LOADING_DONE>(stream);
            Log.Write("player {0} map {1} channel {2} loading done", msg.Uid, msg.MapId, msg.Channel);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                Log.Write("player {0} map {1} channel {2} loading done failed: player is null", msg.Uid, msg.MapId, msg.Channel);
                return;
            }

            if (player.NotStableInMap())
            {
                Log.Warn("player {0} loading map {1} channel {2} done failed: not stable in map", player.Uid, msg.MapId, msg.Channel);
                return;
            }
            if (!(player.CurrentMap.MapId == msg.MapId && player.CurrentMap.Channel == msg.Channel))
            {
                Log.Warn("player {0} loading map {1} channel {2} done: current map {3} channel {4}",
                    player.Uid, msg.MapId, msg.Channel, player.CurrentMap.MapId, player.CurrentMap.Channel);
                return;
            }
            player.SetIsMapLoadingDone(true);
            player.LoadingDoneCreateDungeonWaiting = ZoneServerApi.now.AddSeconds(0.5);

            ////假如从缓存重进的 也要获取自己的aoi，但是不能通知别人
            //if (player.GetReEnterDungeon())
            //{
            //    Log.Debug($"player {player.Uid} maploading done reEnterDungeon");
            //    player.GetDungeonAoi();
            //}
            //else
            //{ 
            //    player.AddToAoi();
            //    player.HeroMng.CallHero2Map();
            //}

            player.CurrentMap.OnPlayerMapLoadingDone(player);

            //处理队友离队
            player.MemberRealQuitTeam();

            if (!player.CurrentMap.IsDungeon)
            {
                //过期物品状态变更
                player.CheckCurTitle();
                player.CheckCurChatFrame(false);
            }
        }

        private void OnResponse_ChangeChannel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHANGE_CHANNEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_CHANNEL>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.NotStableInMap())
            {
                return;
            }
            Log.Write("player {0} request change map {1} channel {2} to channel {3}", player.Uid, player.CurrentMap.MapId, player.CurrentMap.Channel, msg.Channel);
            if (player.CurrentMap.Channel == msg.Channel)
            {
                MSG_ZGC_CHANGE_CHANNEL response = new MSG_ZGC_CHANGE_CHANNEL();
                response.Channel = msg.Channel;
                response.Result = (int)ErrorCode.Already;
                player.Write(response);
                Log.Warn("player {0} request change map {1} channel {2} to channel {3} failed: already in this channel"
                    , player.Uid, player.CurrentMap.MapId, player.CurrentMap.Channel, msg.Channel);
                return;
            }
            //string tableName = "character";
            //Api.GameDBPool.Call(new QuerySetMap(player.Uid, player.CurrentMap.MapId, player.CurrentMap.Channel, player.Position.x, player.Position.y),
            //    player.DBIndex);


#if DEBUG
            //仅允许同进程间的切换(所有模块功能完成后放开此限制)
            FieldMap aimMap = this.Api.MapManager.GetFieldMap(player.CurrentMap.MapId, msg.Channel);
            if (aimMap == null)
            {
                Log.Warn($"DEBUG limit: Only allow to chang channel in the same map, curr map {player.CurrentMap.MapId} aim channel {msg.Channel}");

                MSG_ZGC_CHANGE_CHANNEL response = new MSG_ZGC_CHANGE_CHANNEL();
                response.Channel = msg.Channel;
                response.Result = (int)ErrorCode.Fail;
                player.Write(response);
                return;
            }
#endif

            MSG_ZM_CHANGE_CHANNEL request = new MSG_ZM_CHANGE_CHANNEL();
            request.MapId = player.CurrentMap.MapId;
            request.FromChannel = player.CurrentMap.Channel;
            request.ToChannel = msg.Channel;
            request.Uid = player.Uid;
            Api.ManagerServer.Write(request);

            player.SetIsTransforming(true);
        }

        private void OnResponse_ReconnectLogin(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RECONNECT_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECONNECT_LOGIN>(stream);
            PlayerChar player = Api.PCManager.FindOfflinePc(msg.Uid);
            Log.Write("player {0} request reconnect login", msg.Uid);
            MSG_GC_RECONNECT_LOGIN response = new MSG_GC_RECONNECT_LOGIN();
            if (player == null)
            {
                response.Result = (int)ErrorCode.CharNotExist;
                Write(response, msg.Uid);
                Log.Warn("player {0} request reconnect login failed: not find offline player", msg.Uid);
                return;
            }
            if (player.OfflineToken != msg.Token)
            {
                response.Result = (int)ErrorCode.BadWord;
                Write(response, msg.Uid);
                Log.Warn("player {0} request reconnect login failed: offlineToken {1} not equals msgToken {2}", msg.Uid, player.OfflineToken, msg.Token);
                return;
            }
            if (!player.LastMapInfo.Valid())
            {
                response.Result = (int)ErrorCode.NotInMap;
                Write(response, msg.Uid);
                Log.Warn("player {0} request reconnect login failed: not int map", msg.Uid);
                return;
            }
            // 校验成功 进入 
            response.Result = (int)ErrorCode.Success;
            Write(response, msg.Uid);
            Log.Write("reconnect login player {0} enter world", player.Uid);
            player.NeedSyncEnterWorld = false;
            OfflinePlayerEnterWorld(player, msg.Ip);
        }

        private void OnResponse_LoginGetSoftwares(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LOGIN_GET_SOFTWARES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LOGIN_GET_SOFTWARES>(stream);
            Log.Write("player {0} request LoginGetSoftwares", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                return;
            }
            player.localSoftwares.Clear();
            player.localSoftwares.AddRange(msg.List);
            player.KomoeEventLogGetAppList();
        }

        private void OnResponse_LogOut(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LOGOUT>(stream);
            Log.Write("player {0} request log out", msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }
            player.CanCatchOffline = false;
            Api.PCManager.DestroyPlayer(player, true);
        }
    }
}

using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Zone.Protocol.ZM;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        #region 跨服务器传递信息

        public virtual void Init(MSG_ZMZ_CHARACTER_INFO info)
        {
            RegisterId = info.RegisterId;

            SetUid(info.Uid);
            SetPosition(new Vec2(info.PosX, info.PosY));

            RecordEnterMapInfo(info.MapId, info.Channel, Position);

            Sex = info.Sex;
            Name = info.Name;
            AccountName = info.AccountName;
            Level = info.Level;
            MainId = server.MainId;
            SourceMain = info.SourceMain;

            instanceId = info.InstanceId;
            Icon = info.Icon;
            ShowDIYIcon = info.ShowDIYIcon;
            //PetId = info.PetId;
            //LadderLevel = info.LadderLevel;
            PopScore = info.PopScore;
            HighestPopScore = info.HighestPopScore;
            //geography.GeoHashStr = info.GeoHashStr;
            //geography.Latitude = info.Latitude;
            //geography.Longitude = info.Longitude;

            LastRefreshTime = Timestamp.TimeStampToDateTime(info.LastRefreshTime);
            LastOfflineTime = Timestamp.TimeStampToDateTime(info.LastOfflineTime);
            LastLevelUpTime = Timestamp.TimeStampToDateTime(info.LastLevelUpTime);
            //LastPhyRecoveryTime = Timestamp.TimeStampToDateTime(info.LastPhyRecoveryTime);
            LastLoginTime = Timestamp.TimeStampToDateTime(info.LastLoginTime);
            OnlineRewardTime = Timestamp.TimeStampToDateTime(info.OnlineRewardTime);

            RegisterId = info.RegisterId;
            OfflineToken = info.OfflineToken;
            ClientIp = info.ClientIp;
            ChannelName = info.ChannelName;

            SilenceReason = info.SilenceReason;
            SilenceTime = Timestamp.TimeStampToDateTime(info.SilenceTime);

            FamilyId = info.FamilyId;
            MainTaskId = info.MainTaskId;
            ChapterId = TaskLibrary.GetTaskChapter(info.MainTaskId);
            BranchTaskIds.AddRange(info.BranchTaskIds);
            IsGm = info.IsGm;
            MainLineId = info.MainLineId;

            TimeCreated = Timestamp.TimeStampToDateTime(info.TimeCreated);

            Camp = (CampType)info.CampType;
            HisPrestige = info.HisPrestige;

            FollowerId = info.FollowerId;
            Job = (JobType)info.Job;
            HeroId = info.HeroId;
            GodType = info.GodType;
            GuideId = info.GuidedId;
            BagManager.ChatFrameBag.CurChatFrameId = info.CurChatFrameId;
            BagManager.FaceFrameBag.CurFaceFrameId = info.CurFaceFrameId;
            WillLeaveList.AddRange(info.WillLeaveList);

            OnLine = true;
            IsRebated = info.IsRebated;

            ChannelId = info.ChannelId;
            Idfa = info.Idfa;       //苹果设备创建角色时使用
            Idfv = info.Idfv;       //苹果设备创建角色时使用
            Imei = info.Imei;       //安卓设备创建角色时使用
            Imsi = info.Imsi;       //安卓设备创建角色时使用
            Anid = info.Anid;       //安卓设备创建角色时使用
            Oaid = info.Oaid;       //安卓设备创建角色时使用
            PackageName = info.PackageName;//包名
            ExtendId = info.ExtendId;   //广告Id，暂时不使用
            Caid = info.Caid;		//暂时不使用

            DeviceId = info.DeviceId;		//暂时不使用
            SDKUuid = info.SdkUuid;		//暂时不使用
         
            Tour = info.Tour;                   //是否是游客账号（0:非游客，1：游客）
            Platform = info.Platform;           //平台名称	统一：ios|android|windows
            ClientVersion = info.ClientVersion; //游戏的迭代版本，例如1.0.3
            DeviceModel = info.DeviceModel;     //设备的机型，例如Samsung GT-I9208
            OsVersion = info.OsVersion;         //操作系统版本，例如13.0.2
            Network = info.Network;             //网络信息	4G/3G/WIFI/2G
            Mac = info.Mac;                     //局域网地址
            GameId = info.GameId;
            CumulateDays = info.CumulateDays;
            CumulateOnlineTime = info.CumulateOnlineTime;
        }

        public TransformStep GetNextNeedTag(TransformStep step)
        {
            switch (step)
            {
                case TransformStep.CHARACTER_INFO:
                    return TransformStep.ACTIVITY;
                case TransformStep.ACTIVITY:
                    return TransformStep.ITEMS;
                case TransformStep.ITEMS:
                    return TransformStep.EMAIL;
                case TransformStep.EMAIL:
                    return TransformStep.HERO;
                case TransformStep.HERO:
                    return TransformStep.REDISDATA;
                case TransformStep.REDISDATA:
                    return TransformStep.SHOP;
                case TransformStep.SHOP:
                    return TransformStep.DungeonInfo;
                case TransformStep.DungeonInfo:
                    return TransformStep.Draw;
                case TransformStep.Draw:
                    return TransformStep.GodPath;
                case TransformStep.GodPath:
                    return TransformStep.WuhunResonance;
                case TransformStep.WuhunResonance:
                    return TransformStep.HeroGod;
                case TransformStep.HeroGod:
                    return TransformStep.Gift;
                case TransformStep.Gift:
                    return TransformStep.Action;
                case TransformStep.Action:
                    return TransformStep.ShovelTreasure;
                case TransformStep.ShovelTreasure:
                    return TransformStep.Theme;
                case TransformStep.Theme:
                    return TransformStep.CultivateGift;
                case TransformStep.CultivateGift:
                    return TransformStep.PettyGift;
                case TransformStep.PettyGift:
                    return TransformStep.DaysRewardHero;
                case TransformStep.DaysRewardHero:
                    return TransformStep.Garden;
                case TransformStep.Garden:
                    return TransformStep.DivineLove;
                case TransformStep.DivineLove:
                    return TransformStep.FlipCard;
                case TransformStep.FlipCard:
                    return TransformStep.IslandHigh;
                case TransformStep.IslandHigh:
                    return TransformStep.IslandHighGift;
                case TransformStep.IslandHighGift:
                    return TransformStep.Trident;
                case TransformStep.Trident:
                    return TransformStep.DragonBoat;
                case TransformStep.DragonBoat:
                    return TransformStep.StoneWall;
                case TransformStep.StoneWall:
                    return TransformStep.IslandChallenge;
                case TransformStep.IslandChallenge:
                    return TransformStep.Carnival;
                case TransformStep.Carnival:
                    return TransformStep.HeroTravel;
                case TransformStep.HeroTravel:
                    return TransformStep.ShrekInvitation;
                case TransformStep.ShrekInvitation:
                    return TransformStep.Roulette;
                case TransformStep.Roulette:
                    return TransformStep.Canoe;
                case TransformStep.Canoe:
                    return TransformStep.MidAutumn;
                case TransformStep.MidAutumn:
                    return TransformStep.ThemeFirework;
                case TransformStep.ThemeFirework:
                    return TransformStep.NineTest;
                case TransformStep.NineTest:
                    return TransformStep.Warehouse;
                case TransformStep.Warehouse:
                    return TransformStep.School;
                case TransformStep.School:
                    return TransformStep.SchoolTask;
                case TransformStep.SchoolTask:
                    return TransformStep.AnswerQuestion;
                case TransformStep.AnswerQuestion:
                    return TransformStep.DiamondRebate;
                case TransformStep.DiamondRebate:
                    return TransformStep.XuanBox;
                case TransformStep.XuanBox:
                    return TransformStep.WishLantern;
                case TransformStep.WishLantern:
                    return TransformStep.Pet;
                case TransformStep.Pet:
                    return TransformStep.PetEgg;
                case TransformStep.PetEgg:
                    return TransformStep.PetDungeonQueue;
                case TransformStep.PetDungeonQueue:
                    return TransformStep.DaysRecharge;
                case TransformStep.DaysRecharge:
                    return TransformStep.TreasureFlipCard;
                case TransformStep.TreasureFlipCard:
                    return TransformStep.Shrekland;
                case TransformStep.Shrekland:
                    return TransformStep.SpaceTimeTower;
                case TransformStep.SpaceTimeTower:
                    return TransformStep.DevilTraining;
                case TransformStep.DevilTraining:
                    return TransformStep.DomainBenedition;
                    case TransformStep.DomainBenedition:
                    return TransformStep.DriftExplore;
                case TransformStep.DriftExplore:
                    return TransformStep.DONE;
                default:
                    return TransformStep.INVALID;
            }
        }

        public void SendTransformData(TransformStep step)
        {
            switch (step)
            {
                case TransformStep.CHARACTER_INFO:
                {
                    MSG_ZMZ_CHARACTER_INFO pcInfo = GetCharacterInfo();
                    server.ManagerServer.Write(pcInfo, uid);
                }
                    //    break;
                    //case TransformStep.ACTIVITY:
                    {
                        server.ManagerServer.Write(GetActivityTransform(), uid);
                    }
                    //    break;
                    //case TransformStep.ITEMS:
                {
                    List<MSG_ZMZ_BAG_INFO> items = BagManager.GetBagTransform();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (i == items.Count - 1)
                        {
                            items[i].IsEnd = true;
                            items[i].BagSpace = BagSpace;
                        }

                        server.ManagerServer.Write(items[i], uid);
                    }
                }
                    //    break;
                    //case TransformStep.SHOP:
                {
                    MSG_ZMZ_SHOP_RECHARGE_INFO shopInfo = new MSG_ZMZ_SHOP_RECHARGE_INFO();
                    shopInfo.Uid = uid;
                    shopInfo.Shop = GetShopTransform();
                    shopInfo.CommonShop = GetCommonShopTransform();
                    shopInfo.Recharge = GetRechargeManagerTransfrom();
                    //shopInfo.GameLevel = GetZMZGameLevelMsg();
                    //MSG_ZMZ_SHOP_INFO shopInfo =  GetShopTransform();
                    server.ManagerServer.Write(shopInfo, uid);
                }
                    //    break;
                    //case TransformStep.EMAIL:
                {
                    SendEmailTransfrom();
                }
                    //    break;
                    //case TransformStep.HERO:
                {
                    SendHeroListTransform();
                }
                    //    break;
                    //case TransformStep.REDISDATA:
                {
                    MSG_ZMZ_REDIS_DATA redisInfo = new MSG_ZMZ_REDIS_DATA();
                    redisInfo.Uid = Uid;
                    server.ManagerServer.Write(redisInfo, uid);
                }
                    //    break;
                    //case TransformStep.DungeonInfo:
                {
                    MSG_ZMZ_DUNGEON_INFO msg = new MSG_ZMZ_DUNGEON_INFO();
                    msg.HuntingInfo = HuntingManager.GenerateTransformMsg(); //猎杀魂兽
                    msg.IntegralBoss = GenerateIntegralBossTransformMsg(); //整点boss
                    msg.Arena = GetArenaTransformMsg(); //竞技场
                    msg.BenefitInfo = GenerateBenefitTransformMsg(); //魂师试炼
                    msg.SecretAreaInfo = SecretAreaManager.GenerateTransformMsg(); //秘境
                    //msg.ChapterInfo = ChapterManager.GenerateTransformMsg();//章节
                    msg.TowerInfo = TowerManager.GenerateTransformMsg(); //爬塔
                    msg.CrossBattleInfo = CrossInfoMng.GenerateCrossBattleInfo(); //跨服战
                    msg.OnHookInfo = OnhookManager.GenerateTransformMsg(); //挂机
                    msg.PushFigure = GeneratePushFigureTransformMsg(); //推图
                    msg.CrossChallengeInfo = CrossChallengeInfoMng.GenerateCrossChallengeInfo(); //跨服挑战
                    server.ManagerServer.Write(msg, uid);
                }
                    //    break;
                    //case TransformStep.Draw:
                {
                    MSG_ZMZ_DRAW_MANAGER drawMsg = new MSG_ZMZ_DRAW_MANAGER();
                    drawMsg.HeroCombo = HeroMng.GetComboList();
                    drawMsg.HeroDraw = DrawMng.GetHeroDraw();
                    drawMsg.Constellation = DrawMng.GetDrawConstellation();
                    drawMsg.RankReward.AddRange(RankRewardList);
                    server.ManagerServer.Write(drawMsg, uid);
                }
                    //    break;
                    //case TransformStep.GodPath:
                {
                    MSG_ZMZ_GOD_PATH_INFO godPathMsg = GodPathManager.GetGodPathTransform();
                    server.ManagerServer.Write(godPathMsg, uid);
                }
                    //    break;
                    //case TransformStep.WuhunResonance:
                {
                    SendWuhunResonanceGridListTransform();
                }
                    //    break;
                    //case TransformStep.HeroGod:
                {
                    MSG_ZMZ_HERO_GOD_INFO_LIST heroGodMsg = HeroGodManager.GenerateTransformMsg();
                    server.ManagerServer.Write(heroGodMsg, uid);
                }
                    //    break;
                    //case TransformStep.Gift:
                {
                    List<MSG_ZMZ_GIFT_INFO_LIST> giftMsg = GenerateGiftInfoTransformMsg();
                    giftMsg.ForEach(msg => server.ManagerServer.Write(msg, uid));
                }
                    //    break;
                    //case TransformStep.Action:
                {
                    MSG_ZMZ_GET_TIMING_GIFT actionMsg = ActionManager.GenerateTransformMsg();
                    server.ManagerServer.Write(actionMsg, uid);
                }
                    //    break;
                    //case TransformStep.ShovelTreasure:
                {
                    ZMZ_SHOVEL_TREASURE_INFO treasureMsg = ShovelTreasureMng.GenerateShovelTreasureInfoTransformMsg();
                    server.ManagerServer.Write(treasureMsg, uid);
                }
                    //    break;
                    //case TransformStep.Theme:
                {
                    ZMZ_THEME_INFO themeMsg = GenerateThemeInfoTransformMsg();
                    server.ManagerServer.Write(themeMsg, uid);
                }
                    //    break;
                    //case TransformStep.CultivateGift:
                {
                    MSG_ZMZ_CULTIVATE_GIFT_LIST culGiftMsg = GenerateCultivateGiftTransformMsg();
                    server.ManagerServer.Write(culGiftMsg, uid);
                }
                    //    break;
                    //case TransformStep.PettyGift:
                {
                    MSG_ZMZ_PETTY_GIFT pettyGiftMsg = GeneratePettyGiftTransformMsg();
                    server.ManagerServer.Write(pettyGiftMsg, uid);
                }
                    //    break;
                    //case TransformStep.DaysRewardHero:
                {
                    MSG_ZMZ_DAYS_REWARD_HERO daysRewardHeroMsg = new MSG_ZMZ_DAYS_REWARD_HERO();
                    daysRewardHeroMsg.DailyRecharge.AddRange(GenerateDailyRechargeTransformMsg());
                    daysRewardHeroMsg.HeroDaysRewards.AddRange(GenerateHeroDaysRewardsTransformMsg());
                    daysRewardHeroMsg.NewServerPromotion.AddRange(GenerateNewServerPromotionTransformMsg());
                    server.ManagerServer.Write(daysRewardHeroMsg, uid);
                }
                    //    break;
                    //case TransformStep.Garden:
                {
                    MSG_ZMZ_GARDEN_INFO gardenInfo = GardenManager.GenerateTransformMsg();
                    server.ManagerServer.Write(gardenInfo, uid);
                }
                    //    break;
                    //case TransformStep.DivineLove:
                {
                    MSG_ZMZ_DIVINE_LOVE divineLoveInfo = GenerateDivineLoveTransformMsg();
                    server.ManagerServer.Write(divineLoveInfo, uid);
                }
                    //    break;
                    //case TransformStep.FlipCard:
                {
                    MSG_ZMZ_FLIP_CARD_INFO flipCardInfo = GenerateFlipCardTransformMsg();
                    server.ManagerServer.Write(flipCardInfo, uid);
                }
                    //    break;
                    //case TransformStep.IslandHigh:
                {
                    MSG_ZMZ_ISLAND_HIGH_INFO highInfo = IslandHighManager.GenerateTransformInfo();
                    server.ManagerServer.Write(highInfo, uid);
                }
                    //    break;
                    //case TransformStep.IslandHighGift:
                {
                    MSG_ZMZ_ISLAND_HIGH_GIFT_INFO islandGiftInfo = GenerateIslandHighGiftTransformMsg();
                    server.ManagerServer.Write(islandGiftInfo, uid);
                }
                    //    break;
                    //case TransformStep.Trident:
                {
                    MSG_ZMZ_TRIDENT_INFO tridentInfo = tridentManager.GenerateTransformInfo();
                    server.ManagerServer.Write(tridentInfo, uid);
                }
                    //    break;
                    //case TransformStep.DragonBoat:
                {
                    MSG_ZMZ_DRAGON_BOAT_INFO dragonBoatInfo = GenerateDragonBoatTransformMsg();
                    server.ManagerServer.Write(dragonBoatInfo, uid);
                }
                    //    break;
                    //case TransformStep.StoneWall:
                {
                    MSG_ZMZ_STONE_WALL_INFO stoneWallInfo = GenerateStoneWallTransformMsg();
                    server.ManagerServer.Write(stoneWallInfo, uid);
                }
                    //    break;
                    //case TransformStep.IslandChallenge:
                {
                    MSG_ZMZ_ISLAND_CHALLENGE_INFO islandChallengeInfo = IslandChallengeManager.GenerateTransformMsg();
                    server.ManagerServer.Write(islandChallengeInfo, uid);
                }
                    //    break;
                    //case TransformStep.Carnival:
                {
                    MSG_ZMZ_CARNIVAL_INFO carnivalInfo = GenerateCarnivalTransformMsg();
                    server.ManagerServer.Write(carnivalInfo, uid);
                }
                    //    break;
                    //case TransformStep.HeroTravel:
                {
                    MSG_ZMZ_TRAVEL_MANAGER travelHerosInfo = GenerateTravelManagerTransformMsg();
                    server.ManagerServer.Write(travelHerosInfo, uid);
                }
                    //case TransformStep.ShrekInvitation:
                {
                    MSG_ZMZ_SHREK_INVITATION_INFO shrekInvitationInfo = GenerateShrekInvitationTransformMsg();
                    server.ManagerServer.Write(shrekInvitationInfo, uid);
                }
                {
                    MSG_ZMZ_ROULETTE_INFO rouletteInfo = RouletteManager.GenerateTransformMsg();
                    server.ManagerServer.Write(rouletteInfo, uid);
                }
                    //case TransformStep.Canoe:
                {
                    MSG_ZMZ_CANOE_INFO canoeInfo = GenerateCanoeTransformMsg();
                    server.ManagerServer.Write(canoeInfo, uid);
                }
                    //case TransformStep.MidAutumn:
                {
                    MSG_ZMZ_MIDAUTUMN_INFO midAutumnInfo = MidAutumnMng.GenerateTransformMsg();
                    server.ManagerServer.Write(midAutumnInfo, uid);
                }
                    //case TransformStep.ThemeFirework:
                {
                    MSG_ZMZ_THEME_FIREWORK fireworkInfo = ThemeFireworkMng.GenerateTransformMsg();
                    server.ManagerServer.Write(fireworkInfo, uid);
                }
                    //case TransformStep.NineTest:
                {
                    MSG_ZMZ_NINE_TEST nineTestInfo = NineTestMng.GenerateTransformMsg();
                    server.ManagerServer.Write(nineTestInfo, uid);
                }
                    //case TransformStep.Warehouse:
                    {
                        MSG_ZMZ_WAREHOUSE_ITEMS warehouseInfo = GenerateWarehouseItemsTransformMsg();
                        server.ManagerServer.Write(warehouseInfo, uid);

                        MSG_ZMZ_SCHOOL_INFO schoolInfo = schoolManager.GenerateTransformInfo();
                        server.ManagerServer.Write(schoolInfo, uid);

                        MSG_ZMZ_SCHOOL_TASK_INFO schoolTaskInfo = schoolManager.GenerateSchoolTaskTransformMsg();
                        server.ManagerServer.Write(schoolTaskInfo, uid);

                        MSG_ZMZ_ANSWER_QUESTION_INFO answerQuestionInfo = schoolManager.GenerateAnswerQuestionTransformMsg();
                        server.ManagerServer.Write(answerQuestionInfo, uid);
                    }
                {
                    MSG_ZMZ_WAREHOUSE_ITEMS warehouseInfo = GenerateWarehouseItemsTransformMsg();
                    server.ManagerServer.Write(warehouseInfo, uid);

                        MSG_ZMZ_DIAMOND_REBATE_INFO diamondRebateInfo = GenerateDiamondRebateTransformMsg();
                    server.ManagerServer.Write(diamondRebateInfo, uid);

					SendPetTransformMsg();
                }
                {
                    MSG_ZMZ_XUANBOX_INFO xuanboxInfo = XuanBoxManager.GenerateTransformMsg();
                    server.ManagerServer.Write(xuanboxInfo, uid);
                }
                {
                    MSG_ZMZ_WISH_LANTERN wishLanternInfo = WishLanternManager.GenerateTransformMsg();
                    server.ManagerServer.Write(wishLanternInfo, uid);

                    MSG_ZMZ_DAYS_RECHARGE_INFO daysRechargeInfo = daysRechargeManager.GenerateTransformInfo();
                    server.ManagerServer.Write(daysRechargeInfo, uid);
                }
                    //case TransformStep.TreasureFlipCard:
                {
                    MSG_ZMZ_TREASURE_FLIP_CARD_INFO treasureFlipCardInfo = GenerateTreasureFlipCardTransformMsg();
                    server.ManagerServer.Write(treasureFlipCardInfo, uid);
                }
                {
                    MSG_ZMZ_SHREKLAND_INFO shreklandInfo = ShreklandMng.GenerateTransformMsg();
                    server.ManagerServer.Write(shreklandInfo, uid);

                    MSG_ZMZ_SPACETIME_TOWER_INFO spacetimeTowerInfo = SpaceTimeTowerMng.GenerateTransformMsg();
                    server.ManagerServer.Write(spacetimeTowerInfo, uid);
                }
                {
                    MSG_ZMZ_DEVIL_TRAINING_INFO devilTrainingInfo = DevilTrainingMng.GenerateTransformMsg();
                    server.ManagerServer.Write(devilTrainingInfo, uid);

                    MSG_ZMZ_DOMAIN_BENEDICTION_INFO domainBenedictionInfo = GenerateDomainBenedictionTransformMsg();
                    server.ManagerServer.Write(domainBenedictionInfo, uid);
                }
                {
                    MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO driftExploreTaskInfo = driftExploreMng.GenerateDriftExploreTaskTransformMsg();
                    server.ManagerServer.Write(driftExploreTaskInfo, uid);
                }
                    break;
                case TransformStep.PetDungeonQueue:
                case TransformStep.PetEgg:
                case TransformStep.Pet:
                case TransformStep.Warehouse:
                case TransformStep.NineTest:
                case TransformStep.ThemeFirework:
                case TransformStep.MidAutumn:
                case TransformStep.Canoe:
                case TransformStep.ShrekInvitation:
                case TransformStep.HeroTravel:
                case TransformStep.Carnival:
                case TransformStep.IslandChallenge:
                case TransformStep.StoneWall:
                case TransformStep.DragonBoat:
                case TransformStep.Trident:
                case TransformStep.IslandHighGift:
                case TransformStep.IslandHigh:
                case TransformStep.FlipCard:
                case TransformStep.DivineLove:
                case TransformStep.Garden:
                case TransformStep.DaysRewardHero:
                case TransformStep.PettyGift:
                case TransformStep.CultivateGift:
                case TransformStep.Theme:
                case TransformStep.ShovelTreasure:
                case TransformStep.Action:
                case TransformStep.Gift:
                case TransformStep.HeroGod:
                case TransformStep.WuhunResonance:
                case TransformStep.GodPath:
                case TransformStep.Draw:
                case TransformStep.DungeonInfo:
                case TransformStep.REDISDATA:
                case TransformStep.HERO:
                case TransformStep.EMAIL:
                case TransformStep.SHOP:
                case TransformStep.ITEMS:
                case TransformStep.Roulette:
                case TransformStep.School:
                case TransformStep.SchoolTask:
                case TransformStep.AnswerQuestion:
                case TransformStep.DiamondRebate:
                case TransformStep.XuanBox:
                case TransformStep.WishLantern:
                case TransformStep.DaysRecharge:
                case TransformStep.TreasureFlipCard:
                case TransformStep.Shrekland:
                case TransformStep.SpaceTimeTower:
                case TransformStep.DevilTraining:
                case TransformStep.DomainBenedition:
                case TransformStep.DriftExplore:
                    break;
                case TransformStep.DONE:
                    break;
                default:
                    Log.Warn("send player {0} transform data failed: unsupport step {1}", uid, step);
                    break;
            }
        }

        private MSG_ZMZ_CHARACTER_INFO GetCharacterInfo()
        {
            MSG_ZMZ_CHARACTER_INFO pcInfo = new MSG_ZMZ_CHARACTER_INFO();
            pcInfo.Uid = uid;
            pcInfo.Name = Name;
            pcInfo.Sex = Sex;
            pcInfo.Level = Level;
            pcInfo.AccountName = AccountName;
            pcInfo.MainId = server.MainId;
            pcInfo.SourceMain = SourceMain;
            pcInfo.MapId = EnterMapInfo.MapId;
            pcInfo.Channel = EnterMapInfo.Channel;
            pcInfo.InstanceId = 0;
            pcInfo.PosX = Position.X;
            pcInfo.PosY = Position.Y;
            pcInfo.Icon = Icon;
            pcInfo.ShowDIYIcon = ShowDIYIcon;
            pcInfo.Job = (int)Job;
            pcInfo.HeroId = HeroId;
            pcInfo.GodType = GodType;

            pcInfo.PopScore = PopScore;
            pcInfo.HighestPopScore = HighestPopScore;

            //pcInfo.PetId = PetId;
            pcInfo.LastRefreshTime = Timestamp.GetUnixTimeStampSeconds(LastRefreshTime);
            pcInfo.LastOfflineTime = Timestamp.GetUnixTimeStampSeconds(LastOfflineTime);
            pcInfo.LastLevelUpTime = Timestamp.GetUnixTimeStampSeconds(LastLevelUpTime);
            //pcInfo.LastPhyRecoveryTime = Timestamp.GetUnixTimeStampSeconds(LastPhyRecoveryTime);
            pcInfo.LastLoginTime = Timestamp.GetUnixTimeStampSeconds(LastLoginTime);
            pcInfo.OnlineRewardTime = Timestamp.GetUnixTimeStampSeconds(OnlineRewardTime);

            pcInfo.RegisterId = RegisterId;
            pcInfo.OfflineToken = OfflineToken;
            pcInfo.ClientIp = ClientIp;
            pcInfo.ChannelName = ChannelName;

            pcInfo.SilenceReason = SilenceReason;
            pcInfo.SilenceTime = Timestamp.GetUnixTimeStampSeconds(SilenceTime);

            pcInfo.TimeCreated = Timestamp.GetUnixTimeStampSeconds(TimeCreated);
            pcInfo.FamilyId = FamilyId;
            pcInfo.MainTaskId = MainTaskId;
            pcInfo.BranchTaskIds.AddRange(BranchTaskIds);
            pcInfo.IsGm = IsGm;
            pcInfo.MainLineId = MainLineId;

            pcInfo.Currencies = GetCurrenciesTransform();
            pcInfo.Counter = GetCounterTransform();
            pcInfo.Task = GetTaskTransform();

            pcInfo.TitleInfo = GetTitlesTransform();
            pcInfo.CampStarInfo = GetCampStarTransform();
            pcInfo.HisPrestige = HisPrestige;
            pcInfo.CampType = (int)Camp;
            pcInfo.FollowerId = FollowerId;
            pcInfo.DelegationInfo = GetDelegationTransform();
            pcInfo.GuidedId = GuideId;
            pcInfo.PasscardInfo = GetPassCardTransform();
            pcInfo.WishPoolInfo = GetWishPoolTransform();
            pcInfo.CampBuildInfo = GetCampBuildTransform();
            pcInfo.TaskFlyInfo = GetTaskFlyTransfrom();
            pcInfo.CampBattleInfo = GetCampBattleTransform();
            pcInfo.CurChatFrameId = BagManager.ChatFrameBag.CurChatFrameId;
            pcInfo.CurFaceFrameId = BagManager.FaceFrameBag.CurFaceFrameId;
            pcInfo.WillLeaveList.AddRange(WillLeaveList);
            pcInfo.TreasureFlyInfo = GetTreasureFlyTransfrom();
            pcInfo.IsRebated = IsRebated;

            pcInfo.ChannelId = ChannelId;
            pcInfo.Idfa = Idfa;       //苹果设备创建角色时使用
            pcInfo.Idfv = Idfv;       //苹果设备创建角色时使用
            pcInfo.Imei = Imei;       //安卓设备创建角色时使用
            pcInfo.Imsi = Imsi;       //安卓设备创建角色时使用
            pcInfo.Anid = Anid;       //安卓设备创建角色时使用
            pcInfo.Oaid = Oaid;       //安卓设备创建角色时使用
            pcInfo.PackageName = PackageName;//包名
            pcInfo.ExtendId = ExtendId;   //广告Id，暂时不使用
            pcInfo.Caid = Caid;        //暂时不使用

            pcInfo.DeviceId = DeviceId;        //暂时不使用
            pcInfo.SdkUuid = SDKUuid;       //暂时不使用

            pcInfo.WarehouseCurrencies = GetWarehouseCurrenciesTransform();//仓库货币
            pcInfo.Tour = Tour;                   //是否是游客账号（0:非游客，1：游客）
            pcInfo.Platform = Platform;           //平台名称	统一：ios|android|windows
            pcInfo.ClientVersion = ClientVersion; //游戏的迭代版本，例如1.0.3
            pcInfo.DeviceModel = DeviceModel;     //设备的机型，例如Samsung GT-I9208
            pcInfo.OsVersion = OsVersion;         //操作系统版本，例如13.0.2
            pcInfo.Network = Network;             //网络信息	4G/3G/WIFI/2G
            pcInfo.Mac = Mac;                     //局域网地址
            pcInfo.GameId = GameId;

            pcInfo.CumulateDays = CumulateDays;
            pcInfo.CumulateOnlineTime = CumulateOnlineTime;

            return pcInfo;
        }
        private MSG_ZMZ_RECHARGE_MANAGER GetRechargeManagerTransfrom()
        {
            MSG_ZMZ_RECHARGE_MANAGER rechargeInfo = new MSG_ZMZ_RECHARGE_MANAGER();
            rechargeInfo.Uid = Uid;
            rechargeInfo.First = RechargeMng.First;
            rechargeInfo.AccumulateTotal = RechargeMng.AccumulateTotal;
            rechargeInfo.AccumulatePrice = RechargeMng.AccumulatePrice;
            rechargeInfo.AccumulateCurrent = RechargeMng.AccumulateCurrent;
            rechargeInfo.AccumulateDaily = RechargeMng.AccumulateDaily;
            rechargeInfo.AccumulateMoney = RechargeMng.AccumulateMoney;
            //rechargeInfo.Products = RechargeMng.GetProductsValue();
            //rechargeInfo.Rewards = RechargeMng.GetRewardsValue();
            rechargeInfo.AccumulateOnceMaxMoney = RechargeMng.AccumulateOnceMaxMoney;
            rechargeInfo.LastCommonRechargeTime = RechargeMng.LastCommonRechargeTime;

            rechargeInfo.MonthCardTime = RechargeMng.MonthCardTime;
            rechargeInfo.WeekCardEnd = RechargeMng.WeekCardEnd;
            rechargeInfo.WeekCardStart = RechargeMng.WeekCardStart;
            rechargeInfo.SeasonCardTime = RechargeMng.SeasonCardTime;
            rechargeInfo.GrowthFund = RechargeMng.GrowthFund;
            rechargeInfo.MonthCardState = RechargeManager.MonthCardState;
            rechargeInfo.SuperMonthCardTime = RechargeManager.SuperMonthCardTime;
            rechargeInfo.SuperMonthCardState = RechargeManager.SuperMonthCardState;
            rechargeInfo.SeasonCardState = RechargeManager.SeasonCardState;
            rechargeInfo.AccumulateRechargeRewards = RechargeManager.AccumulateRechargeRewards;
            rechargeInfo.NewRechargeGiftScore = RechargeManager.NewRechargeGiftScore;
            rechargeInfo.NewRechargeGiftRewards = RechargeManager.NewRechargeGiftRewards;

           
            rechargeInfo.PayCount = RechargeMng.PayCount;
            rechargeInfo.FirstOrder = GenerateFirstOrderInfo();

            return rechargeInfo;
        }



        #endregion
    }
}

using CommonUtility;
using DBUtility.Sql;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using Message.Manager.Protocol.MZ;
using Message.Zone.Protocol.ZGate;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;
using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class ManagerServer : BackendServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public Dictionary<int, MapChannelInfo> MapChannelInfoList = new Dictionary<int, MapChannelInfo>();

        public int TotalCount { get; set; }
        public int UseCount { get; set; }

        public ManagerServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MZ_KICK_CLIENT>.Value, OnResponse_KickClient);
            AddResponser(Id<MSG_MZ_MAP_CHANNEL_COUNT>.Value, OnResponse_MapChannelCount);
            AddResponser(Id<MSG_MZ_REPEAT_LOGIN>.Value, OnResponse_RepeatLogin);

            //Transform
            AddResponser(Id<MSG_MZ_CHANGE_CHANNEL>.Value, OnResponse_ChangeChannel);
            AddResponser(Id<MSG_MZ_ANSWER_FOR_MAP>.Value, OnResponse_AnswerForMap);
            AddResponser(Id<MSG_MZ_CLIENT_ENTER>.Value, OnResponse_ClientEnter);
            AddResponser(Id<MSG_MZ_CLIENT_ENTER_TRANSFORM>.Value, OnResponse_ClientEnterTransform);
            AddResponser(Id<MSG_ZMZ_CHARACTER_INFO>.Value, OnResponse_TransformCharacterInfo);
            AddResponser(Id<MSG_ZMZ_ACTIVITY_MANAGER>.Value, OnResponse_TransformActivity);
            AddResponser(Id<MSG_ZMZ_REDIS_DATA>.Value, OnResponse_TransformRedisData);
            AddResponser(Id<MSG_ZMZ_BAG_INFO>.Value, OnResponse_TransformBagInfo);
            //AddResponser(Id<MSG_ZMZ_ITEMS_EMAIL_INFO>.Value, OnResponse_TransformBagAndEmailInfo);
            //AddResponser(Id<MSG_ZMZ_CURRENCIES_INFO>.Value, OnResponse_TransformCurrenciesInfo);
            AddResponser(Id<MSG_ZMZ_COUNTER_INFO>.Value, OnResponse_TransformCounterInfo);
            AddResponser(Id<MSG_ZMZ_SHOP_RECHARGE_INFO>.Value, OnResponse_TransformShopRechargeInfo);
            //AddResponser(Id<MSG_ZMZ_SHOP_INFO>.Value, OnResponse_TransformShopInfo);
            //AddResponser(Id<MSG_ZMZ_TASK_INFO>.Value, OnResponse_TransformTaskInfo);
            AddResponser(Id<MSG_ZMZ_EMAIL_INFO>.Value, OnResponse_TransformEmailInfo);
            AddResponser(Id<MSG_ZMZ_HERO_LIST>.Value, OnResponse_TransformHeroList);
            AddResponser(Id<MSG_ZMZ_HERO_POSES>.Value, OnResponse_TransformHeroPosList);
            AddResponser(Id<MSG_ZMZ_RECHARGE_MANAGER>.Value, OnResponse_TransformRechargeInfo);
            AddResponser(Id<MSG_MZ_NEED_TRANSFORM_DATA_TAG>.Value, OnResponse_NeedTransformDataTag);
            AddResponser(Id<MSG_ZMZ_DUNGEON_INFO>.Value, OnResponse_TransformDungeonInfo);
            AddResponser(Id<MSG_ZMZ_DRAW_MANAGER>.Value, OnResponse_TransformDrawInfo);
            AddResponser(Id<MSG_ZMZ_GOD_PATH_INFO>.Value, OnResponse_TransformGodPathInfo);
            AddResponser(Id<MSG_MZ_TRANSFORM_DONE>.Value, OnResponse_TransformDone);
            AddResponser(Id<MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST>.Value, OnResponse_TransformWuhunResonance);
            AddResponser(Id<MSG_ZMZ_HERO_GOD_INFO_LIST>.Value, OnResponse_TransformHeoGod);
            AddResponser(Id<MSG_ZMZ_GIFT_INFO_LIST>.Value, OnResponse_TransformGiftInfoList);
            AddResponser(Id<MSG_ZMZ_GET_TIMING_GIFT>.Value, OnResponse_TransformActionInfoList);
            AddResponser(Id<ZMZ_SHOVEL_TREASURE_INFO>.Value, OnResponse_TransformShovelTreasureInfo);
            AddResponser(Id<ZMZ_THEME_INFO>.Value, OnResponse_TransformThemeInfo);
            AddResponser(Id<MSG_ZMZ_CULTIVATE_GIFT_LIST>.Value, OnResponse_TransformCultivateGift);
            AddResponser(Id<MSG_ZMZ_PETTY_GIFT>.Value, OnResponse_TransformPettyGift);
            AddResponser(Id<MSG_ZMZ_DAYS_REWARD_HERO>.Value, OnResponse_TransformDaysRewardHero);
            AddResponser(Id<MSG_ZMZ_GARDEN_INFO>.Value, OnResponse_TransformGarden);
            AddResponser(Id<MSG_ZMZ_DIVINE_LOVE>.Value, OnResponse_TransformDivineLove);
            AddResponser(Id<MSG_ZMZ_FLIP_CARD_INFO>.Value, OnResponse_TransformFlipCardInfo);
            AddResponser(Id<MSG_ZMZ_ISLAND_HIGH_INFO>.Value, OnResponse_TransformIslandHighInfo);
            AddResponser(Id<MSG_ZMZ_ISLAND_HIGH_GIFT_INFO>.Value, OnResponse_TransformIslandHighGiftInfo);
            AddResponser(Id<MSG_ZMZ_TRIDENT_INFO>.Value, OnResponse_TransformTridentInfo);
            AddResponser(Id<MSG_ZMZ_DRAGON_BOAT_INFO>.Value, OnResponse_TransformDragonBoatInfo);
            AddResponser(Id<MSG_ZMZ_STONE_WALL_INFO>.Value, OnResponse_TransformStoneWallInfo);
            AddResponser(Id<MSG_ZMZ_ISLAND_CHALLENGE_INFO>.Value, OnResponse_TransformIslandChallengeInfo);
            AddResponser(Id<MSG_ZMZ_CARNIVAL_INFO>.Value, OnResponse_TransformCarnivalInfo);
            AddResponser(Id<MSG_ZMZ_TRAVEL_MANAGER>.Value, OnResponse_TransformTravelHeroInfos);
            AddResponser(Id<MSG_ZMZ_SHREK_INVITATION_INFO>.Value, OnResponse_TransformShrekInvitationInfo);
            AddResponser(Id<MSG_ZMZ_ROULETTE_INFO>.Value, OnResponse_TransformRouletteInfo);
            AddResponser(Id<MSG_ZMZ_CANOE_INFO>.Value, OnResponse_TransformCanoeInfo);
            AddResponser(Id<MSG_ZMZ_MAINQUEUE_INFO>.Value, OnResponse_TransformMainQueueInfo);
            AddResponser(Id<MSG_ZMZ_MIDAUTUMN_INFO>.Value, OnResponse_TransformMidAutumnInfo);
            AddResponser(Id<MSG_ZMZ_THEME_FIREWORK>.Value, OnResponse_TransformThemeFireworkInfo);
            AddResponser(Id<MSG_ZMZ_NINE_TEST>.Value, OnResponse_TransformNineTestInfo);
            AddResponser(Id<MSG_ZMZ_WAREHOUSE_ITEMS>.Value, OnResponse_TransformWarehouseItemsInfo);
            AddResponser(Id<MSG_ZMZ_DIAMOND_REBATE_INFO>.Value, OnResponse_TransformDiamondRebateInfo);
            AddResponser(Id<MSG_ZMZ_XUANBOX_INFO>.Value, OnResponse_TransformXuanBoxInfo);
            AddResponser(Id<MSG_ZMZ_WISH_LANTERN>.Value, OnResponse_TransformWishLanternInfo);
            AddResponser(Id<MSG_ZMZ_SCHOOL_INFO>.Value, OnResponse_TransformSchoolInfo);
            AddResponser(Id<MSG_ZMZ_SCHOOL_TASK_INFO>.Value, OnResponse_TransformSchoolTaskInfo);
            AddResponser(Id<MSG_ZMZ_ANSWER_QUESTION_INFO>.Value, OnResponse_TransformAnswerQuestionInfo);
            AddResponser(Id<MSG_ZMZ_PET_LIST>.Value, OnResponse_TransformPetList);
            AddResponser(Id<MSG_ZMZ_PET_EGG_ITEMS>.Value, OnResponse_TransformPetEggItems);
            AddResponser(Id<MSG_ZMZ_PET_DUNGEON_QUEUES>.Value, OnResponse_TransformPetDungeonQueues);
            AddResponser(Id<MSG_ZMZ_DAYS_RECHARGE_INFO>.Value, OnResponse_TransformDaysRecharge);
            AddResponser(Id<MSG_ZMZ_TREASURE_FLIP_CARD_INFO>.Value, OnResponse_TransformTreasureFlipCardInfo);
            AddResponser(Id<MSG_ZMZ_SHREKLAND_INFO>.Value, OnResponse_TransformShreklandInfo);
            AddResponser(Id<MSG_ZMZ_SPACETIME_TOWER_INFO>.Value, OnResponse_TransformSpaceTimeTowerInfo);
            AddResponser(Id<MSG_ZMZ_DEVIL_TRAINING_INFO>.Value, OnResponse_TransformDevilTrainingInfo);
            AddResponser(Id<MSG_ZMZ_DOMAIN_BENEDICTION_INFO>.Value, OnResponse_TransformDomainBenedictionInfo);
            AddResponser(Id<MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO>.Value, OnResponse_TransformDriftExploreTaskInfo);


            AddResponser(Id<MSG_ZMZ_CHECK_TITLE>.Value, OnResponse_CheckTitle);
            //AddResponser(Id<PKS_ZC_ENTER_WORLD>.Value, OnResponse_ZC_EnterWorld);
            //AddResponser(Id<MSG_RECHARGE_CURRENCIES>.Value, OnResponse_TransformRecharge);
            //AddResponser(Id<MSG_MZ_ADD_MAP>.Value, OnResponse_AddMap);
            //AddResponser(Id<MSG_MZ_CLOSE_MAP>.Value, OnResponse_CloseMap);
            AddResponser(Id<MSG_MZ_ADDICTION_KICK_PLAYER>.Value, OnResponse_AdditionKickPlayer);
            AddResponser(Id<MSG_MZ_KICK_PLAYER>.Value, OnResponse_KickPlayer);
            AddResponser(Id<MSG_MZ_PULL_PLAYER>.Value, OnResponse_PullPlayer);
            AddResponser(Id<MSG_MZ_SHUTDOWN>.Value, OnResponse_Shutdown);
            AddResponser(Id<MSG_MZ_UPDATE_RECHARGE>.Value, OnResponse_UpdatePcRechargeManager);
            AddResponser(Id<MSG_MZ_GET_RECHARGE_ID>.Value, OnResponse_GetRechargeHistoryId);
            AddResponser(Id<MSG_MZ_UNVOICE>.Value, OnResponse_UnVoice);
            AddResponser(Id<MSG_MZ_VOICE>.Value, OnResponse_Voice);
            AddResponser(Id<MSG_MZ_MOVE_PLAYER>.Value, OnResponse_MovePlayer);
            AddResponser(Id<MSG_MZ_VIRTUAL_RECHARGE>.Value, OnResponse_VirtualRecharge);
            AddResponser(Id<MSG_MZ_BAD_WORDS>.Value, OnResponse_BadWords);
            AddResponser(Id<MSG_MZ_KICK_ALL_PLAYER>.Value, OnResponse_KickAllPlayer);
            AddResponser(Id<MSG_MZ_CLOSE_DOOR>.Value, OnResponse_CloseDoor);
            AddResponser(Id<MSG_MZ_SET_FPS>.Value, OnResponse_SetFps);
            AddResponser(Id<MSG_MZ_PLAYER_LEVEL>.Value, OnResponse_PlayerLevel);
            AddResponser(Id<MSG_MZ_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_MZ_MAP_CHANNEL_INFO>.Value, OnResponse_MapChannelInfo);
            AddResponser(Id<MSG_MZ_RECHARGE_GET_REWARD>.Value, OnResponse_RechargeGetReward);
            
            //GM
            AddResponser(Id<MSG_MZ_ADD_REWARD>.Value, OnResponse_AddReward);
            AddResponser(Id<MSG_MZ_EQUIP_HERO>.Value, OnResponse_EquipHero);
            AddResponser(Id<MSG_MZ_ALL_CHAT>.Value, OnResponse_AllChat);
            AddResponser(Id<MSG_MZ_ABSORB_SOULRING>.Value, OnResponse_AbsorbSoulRing);
            AddResponser(Id<MSG_MZ_ABSORB_FINISH>.Value, OnResponse_AbsorbFinish);
            AddResponser(Id<MSG_MZ_ADD_HEROEXP>.Value, OnResponse_AddHeroExp);
            AddResponser(Id<MSG_MZ_HERO_AWAKEN>.Value, OnResponse_HeroAwaken);
            AddResponser(Id<MSG_MZ_HERO_LEVELUP>.Value, OnResponse_HeroLevelUp);
            AddResponser(Id<MSG_MZ_UPDATE_HERO_POS>.Value, OnResponse_UpdateHeroPos);
            AddResponser(Id<MSG_MZ_GIFT_OPEN>.Value, OnResponse_GiftOpen);
            AddResponser(Id<MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM>.Value, OnResponse_GetSpecialActivityItems);

            // Dungeon
            AddResponser(Id<MSG_MZ_NEED_DUNGEON_FAILED>.Value, OnResponse_NeedDungeonFailed);
            AddResponser(Id<MSG_MZ_CREATE_DUNGEON>.Value, OnResponse_CreateDungeon2);
            AddResponser(Id<MSG_ZM_NEW_MAP_RESPONSE>.Value, OnResponse_CreatedMap2Manager);
            AddResponser(Id<MSG_MZ_ZONE_TRANSFORM>.Value, OnResponse_ZoneTransform);
            AddResponser(Id<MSG_MZ_HUNTING_CHANGE>.Value, OnResponse_HuntingChange);

            //所有服在线人数，开服数量
            AddResponser(Id<MSG_MZ_NOTIFY_SERVER_STATE_INFO>.Value, OnResponse_AllServerInfo);
            AddResponser(Id<MSG_MZ_CREATE_MAP>.Value, OnResponse_CreateMap);

            //流失干预
            AddResponser(Id<MSG_MZ_GET_RUNAWA_TYPE>.Value, OnResponse_GetRunAwayType);
            AddResponser(Id<MSG_MZ_GET_SDK_GIFT>.Value, OnResponse_GetSdkGift);
            //ResponserEnd 
            AddResponser(Id<MSG_MZ_GET_SCHOOL_ID>.Value, OnResponse_GetSchoolId);
            //ResponserEnd
        }

        public MapChannelInfo GetMapChannelInfo(int map_id)
        {
            MapChannelInfo info = null;
            MapChannelInfoList.TryGetValue(map_id, out info);
            return info;
        }

        protected override void SendRegistSpecInfo()
        {
            MSG_ZM_REGISTER msg = new MSG_ZM_REGISTER();
            foreach (var item in Api.MapManager.FieldMapList)
            {
                Dictionary<int, FieldMap> channelList = item.Value;
                foreach (var map in channelList)
                {
                    //if (map.Value.campBattle.InitNeedStop)
                    //{
                    //    MSG_ZM_REGISTER.CloseMap closeMap = new MSG_ZM_REGISTER.CloseMap();
                    //    closeMap.mapId = map.Value.MapID;
                    //    closeMap.channel = map.Value.Channel;
                    //    msg.closeMapList.Add(closeMap);
                    //}
                    //通知manager挂载的地图信息
                    MSG_ZM_REGISTER.Types.MapInfo mapInfo = new MSG_ZM_REGISTER.Types.MapInfo();
                    mapInfo.MapId = map.Value.MapId;
                    mapInfo.Channel = map.Value.Channel;
                    msg.MapList.Add(mapInfo);
                }
            }
            Write(msg);

            RegistClientList();
        }

        private void RegistClientList()
        { 
            //MSG_MZ_REGIST_CLIENT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_REGIST_CLIENT_LIST>(stream);
            Log.Info("manager {0} reuqest online client list", MainId);
            List<PlayerChar> onlineList = new List<PlayerChar>();
            List<PlayerChar> offlineList = new List<PlayerChar>();
            foreach (var pc in Api.PCManager.PcList)
            {
                onlineList.Add(pc.Value);
            }
            MSG_ZM_REGIST_CLIENT_LIST onlineNotify = new MSG_ZM_REGIST_CLIENT_LIST();
            for (int i = 0; i < onlineList.Count; i++)
            {
                PlayerChar player = onlineList[i];
                MSG_ZM_REGIST_CLIENT_INFO clientInfo = new MSG_ZM_REGIST_CLIENT_INFO();
                if (player.CurrentMap != null)
                {
                    clientInfo.CharacterUid = player.Uid;
                    clientInfo.MapId = player.CurrentMap.MapId;
                    clientInfo.Channel = player.CurrentMap.Channel;
                    clientInfo.AccountId = player.AccountName + "$" + player.ChannelName;
                }
                onlineNotify.ClientList.Add(clientInfo);
                if (i % 200 == 0 || i == onlineList.Count - 1)
                {
                    Write(onlineNotify);
                    onlineNotify = new MSG_ZM_REGIST_CLIENT_LIST();
                }
            }

            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                offlineList.Add(pc.Value);
            }
            MSG_ZM_REGIST_OFFLINE_CLIENT_LIST offlineNotify = new MSG_ZM_REGIST_OFFLINE_CLIENT_LIST();
            for (int i = 0; i < offlineList.Count; i++)
            {
                PlayerChar player = offlineList[i];
                MSG_ZM_REGIST_OFFLINE_CLIENT clientInfo = new MSG_ZM_REGIST_OFFLINE_CLIENT();
                clientInfo.Uid = player.Uid;
                clientInfo.Token = player.OfflineToken;
                clientInfo.MapId = player.OfflineMapId;
                clientInfo.Channel = player.OfflineChannel;
                offlineNotify.OfflineList.Add(clientInfo);
                if (i % 200 == 0 || i == offlineList.Count - 1)
                {
                    Write(offlineNotify);
                    offlineNotify = new MSG_ZM_REGIST_OFFLINE_CLIENT_LIST();
                }
            }
        }

        // Manager要求Zone踢掉指定client
        private void OnResponse_KickClient(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_KICK_CLIENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_KICK_CLIENT>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.CharacterUid);
            if (pc == null)
            {
                return;
            }
            pc.CanCatchOffline = false;
            Api.PCManager.DestroyPlayer(pc, true);
        }
        private void OnResponse_RepeatLogin(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_REPEAT_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_REPEAT_LOGIN>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.CharacterUid);
            if (pc == null)
            {
                return;
            }
            pc.CanCatchOffline = false;
            Api.PCManager.DestroyPlayer(pc, true);
        }

        private void OnResponse_MapChannelCount(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_MAP_CHANNEL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_MAP_CHANNEL_COUNT>(stream);
            if (msg.MapChannelCount != null && msg.MapChannelCount.Count != 0)
            {
                Api.MapChannelCount.Clear();
                foreach (var item in msg.MapChannelCount)
                {
                    if (Api.MapChannelCount.ContainsKey(item.MapId) == false)
                    {
                        //Log.Write("map {0} channel count {1}", item.mapId, item.channelCount);
                        Api.MapChannelCount.Add(item.MapId, item.ChannelCount);
                    }
                }
            }
        }

        //Transform step 1
        private void OnResponse_ChangeChannel(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_CHANGE_CHANNEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_CHANGE_CHANNEL>(stream);
            Log.Write("player {0} change channel map {1} from channel {2} to {3} result {4}", msg.Uid, msg.MapId, msg.FromChannel, msg.ToChannel, msg.Result);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.IsTransforming)
            {
                return;
            }

            player.SetIsTransforming(false);

            MSG_ZGC_CHANGE_CHANNEL response = new MSG_ZGC_CHANGE_CHANNEL();
            if (msg.Result != (int)ErrorCode.Success)
            {
                response.Result = msg.Result;
                player.Write(response);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Write("player {0} change channel map {1} from channel {2} to {3} failed: not in map", msg.Uid, msg.MapId, msg.FromChannel, msg.ToChannel, msg.Result);
                response.Result = (int)ErrorCode.FullPC;
                player.Write(response);
                return;
            }
            if (!(player.CurrentMap.MapId == msg.MapId && player.CurrentMap.Channel == msg.FromChannel))
            {
                Log.Write("player {0} change channel map {1} from channel {2} to {3} failed: changed", msg.Uid, msg.MapId, msg.FromChannel, msg.ToChannel, msg.Result);
                response.Result = (int)ErrorCode.FullPC;
                player.Write(response);
                return;
            }
            // 验证通过 准备切线
            player.AskForEnterMap(msg.MapId, msg.ToChannel, player.Position, true);
        }

        //Transform step 2
        private void OnResponse_AnswerForMap(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ANSWER_FOR_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ANSWER_FOR_MAP>(stream);
            Log.Write("player {0} request enter map {1} channel {2} result {3}", msg.Uid,  msg.DestMapId, msg.DestChannel, msg.Result);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;

            if (msg.Result == (int)ErrorCode.Success)
            {
                if (msg.SubId == Api.SubId)
                {
                    player.SetIsTransforming(false);

                    player.RecordEnterMapInfo(msg.DestMapId, msg.DestChannel, new Vec2(msg.DestPosX, msg.DestPosY));

                    player.OnMoveMap();
                }
                else
                {
                    // 等待zone2发送transform data请求即可
                    Log.Write("player {0} will enter main {1} sub {2} map {3} channel {4}", msg.Uid, MainId, msg.SubId, msg.DestMapId, msg.DestChannel);
                }
            }
            else
            {
                player.SetIsTransforming(false);

                MSG_GC_ENTER_ZONE result = new MSG_GC_ENTER_ZONE();
                result.MainId = player.MainId;
                result.MapId = player.CurrentMapId;
                result.Angle = player.GenAngle;
                MapChannelInfo channelInfo = Api.ManagerServer.GetMapChannelInfo(player.CurrentMapId);
                if (channelInfo != null)
                {
                    result.MinChannel = channelInfo.MinChannel;
                    result.MaxChannel = channelInfo.MaxChannel;
                }
                result.IsVisable = player.IsVisable;
                result.Channel = player.CurrentChannel;
                result.InstanceId = player.InstanceId;
                result.PosX = player.Position.x;
                result.PosY = player.Position.y;
                result.Result = msg.Result;
                result.NeedAnim = player.EnterMapInfo.NeedAnim;
                player.EnterMapInfo.ClearAnimInfo();

                player.Write(result);
                
            }
        }

        //Transform step 3 记录即将进入游戏世界的client
        private void OnResponse_ClientEnter(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_CLIENT_ENTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_CLIENT_ENTER>(stream);
            Log.Write("add new client enter uid {0} map {1} channel {2}", msg.Uid, msg.DestMapId, msg.DestChannel);
            MSG_ZM_CLIENT_ENTER_TRANSFORM response = new MSG_ZM_CLIENT_ENTER_TRANSFORM();
            response.Uid= msg.Uid;
            response.OriginSubId = msg.OriginSubId;
            response.Result = (int)ErrorCode.Success;

            if (Api.MapManager.GetFieldMap(msg.DestMapId, msg.DestChannel) == null)
            {
                Log.Warn("client enter failed: no such map {0} channel {1}", msg.DestMapId, msg.DestChannel);
                response.Result = (int)ErrorCode.MapNotMounted;
                Write(response);
                return;
            }

            EnterMapInfo origin = new EnterMapInfo();
            origin.SetInfo(msg.OriginMapId, msg.OriginChannel, new Vec2(msg.OriginPosX, msg.OriginPosY));
            EnterMapInfo dest = new EnterMapInfo();
            if (msg.NeedAnim)
            {
                dest.SetNeedAnim();
            }
            dest.SetInfo(msg.DestMapId, msg.DestChannel, new Vec2(msg.DestPosX, msg.DestPosY));

            PlayerEnter playerEnter = new PlayerEnter(Api, msg.Uid, msg.OriginSubId, origin, dest);
            Api.PCManager.AddPlayerEnter(playerEnter);

            Write(response);
        }

        //Transform step 4
        private void OnResponse_ClientEnterTransform(MemoryStream stream, int uid = 0)
        {
            // 向zone2发送character_info
            MSG_MZ_CLIENT_ENTER_TRANSFORM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_CLIENT_ENTER_TRANSFORM>(stream);
            Log.Write("player {0} request enter client transform result {1}", msg.Uid, msg.Result);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                Log.Warn("player {0} request enter client transform failed: player not exist", msg.Uid);
                return;
            }
            if (msg.Result != (int)ErrorCode.Success)
            {
                Log.Warn("player {0} client enter transmform failed: result {1}", msg.Uid, msg.Result);
                player.SetIsTransforming(false);

                // 通知客户端 切图失败
                MSG_GC_ENTER_ZONE result = new MSG_GC_ENTER_ZONE();
                result.MainId = player.MainId;
                result.MapId = player.CurrentMapId;
                result.Angle = player.GenAngle;
                MapChannelInfo channelInfo = Api.ManagerServer.GetMapChannelInfo(player.CurrentMapId);
                if (channelInfo != null)
                {
                    result.MinChannel = channelInfo.MinChannel;
                    result.MaxChannel = channelInfo.MaxChannel;
                }
                result.Channel = player.CurrentChannel;
                result.InstanceId = player.InstanceId;
                result.PosX = player.Position.x;
                result.PosY = player.Position.y;
                result.Result = msg.Result;
                result.NeedAnim = player.EnterMapInfo.NeedAnim;
                player.EnterMapInfo.ClearAnimInfo();

                //???????为啥不发送
                return;
            }
   
            // 成功 开始跨zone
            player.SendTransformData(TransformStep.CHARACTER_INFO);
        }

        //Transform step 5
        private void OnResponse_TransformCharacterInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 character_info信息
            MSG_ZMZ_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CHARACTER_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform character info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform character info failed: pc is null", msg.Uid);
                return;
            }
            //Client client = new Client(server);
            //playerEnter.Player.BindClient(client);
            playerEnter.Player.Init(msg);

            playerEnter.Player.LoadCurrenciesTransform(msg.Currencies.Currencies, msg.Currencies.CurrenciesChanged);
            playerEnter.Player.LoadCounterTransform(msg.Counter.Counters);
            playerEnter.Player.LoadTaskTransform(msg.Task);
            //playerEnter.Player.LoadActivityTransform(msg.Task.ActivityList);
            playerEnter.Player.LoadWelfareTriggerTransform(msg.Task.WelfareTriggerList);
            playerEnter.Player.LoadTtilesTransform(msg.TitleInfo);
            playerEnter.Player.LoadCampStarTransform(msg.CampStarInfo);
            playerEnter.Player.LoadDelegationTransform(msg.DelegationInfo);
            playerEnter.Player.LoadPasscardTransform(msg.PasscardInfo);
            playerEnter.Player.LoadWishPoolTransform(msg.WishPoolInfo);
            playerEnter.Player.LoadCampBuildTransform(msg.CampBuildInfo);
            playerEnter.Player.LoadTaskFlyTransform(msg.TaskFlyInfo);
            playerEnter.Player.LoadCampBattleTransform(msg.CampBattleInfo);
            playerEnter.Player.LoadTreasureFlyTransform(msg.TreasureFlyInfo);
            playerEnter.Player.LoadWarehouseCurrenciesTransform(msg.WarehouseCurrencies);
            //playerEnter.Player.InitVip(msg.accumulateCount, false);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.CHARACTER_INFO);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformActivity(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 character_info信息
            MSG_ZMZ_ACTIVITY_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ACTIVITY_MANAGER>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform activity info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform activity info failed: pc is null", uid);
                return;
            }
            //Client client = new Client(server);
            //playerEnter.Player.BindClient(client);
            playerEnter.Player.LoadActivityTransform(msg.ActivityList);
            playerEnter.Player.LoadSpecialActivityTransform(msg.SpecialList);
            playerEnter.Player.LoadRunawayActivityTransform(msg.RunawayList);
            playerEnter.Player.ActivityMng.RunawayType = msg.OpenType;
            playerEnter.Player.ActivityMng.RunawayTime = msg.OpenTime;
            playerEnter.Player.ActivityMng.DataBox = msg.DataBox;
            playerEnter.Player.LoadWebpayRechargeRebateTransform(msg.WebPayRebateMoney, msg.WebPayRebateLoginMark, msg.WebPayRebateList);


            //playerEnter.Player.InitVip(msg.accumulateCount, false);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ACTIVITY);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformRedisData(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_REDIS_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_REDIS_DATA>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform redis info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform redis info failed: pc is null", msg.Uid);
                return;
            }
            playerEnter.Player.LoadRedisData();
            playerEnter.Player.LoadHeartFlag();

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.REDISDATA);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformBagInfo(MemoryStream stream, int uid = 0)
        {
            //收到zone1 career_info信息
            MSG_ZMZ_BAG_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_BAG_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform bag info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform bag info failed: pc is null", msg.Uid);
                return;
            }

            playerEnter.Player.BagManager.LoadTransform(msg);
            if (msg.BagSpace > 0)
            {
                playerEnter.Player.BagSpace = msg.BagSpace;
            }
            if (msg.IsEnd)
            {
                TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ITEMS);
                playerEnter.SendNeedTransformDataTag(nextTag);
            }
        }

        //private void OnResponse_TransformBagAndEmailInfo(MemoryStream stream, int uid = 0)
        //{
        //    // 收到zone1 career_info信息
        //    MSG_ZMZ_ITEMS_EMAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ITEMS_EMAIL_INFO>(stream);
        //    PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
        //    if (playerEnter == null)
        //    {
        //        Log.Warn("player {0} got transform knapsack info failed: player not exist", msg.Uid);
        //        return;
        //    }
        //    if (playerEnter.Player == null)
        //    {
        //        Log.Warn("player {0} got transform knapsack info failed: pc is null", msg.Uid);
        //        return;
        //    }

        //    playerEnter.Player.BagManager.LoadTransform(msg.Items);
        //    //playerEnter.Player.LoadEmailTransform(msg.EmailList);
        //    //if (msg.IsStart)
        //    {
        //        TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ITEMS);
        //        playerEnter.SendNeedTransformDataTag(nextTag);
        //    }
        //}

        //private void OnResponse_TransformCurrenciesInfo(MemoryStream stream, int uid = 0)
        //{
        //    // 收到zone1 career_info信息
        //    MSG_ZMZ_CURRENCIES_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CURRENCIES_INFO>(stream);
        //    PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
        //    if (playerEnter == null)
        //    {
        //        Log.Warn("player {0} got transform currencies info failed: player not exist", msg.Uid);
        //        return;
        //    }
        //    if (playerEnter.Player == null)
        //    {
        //        Log.Warn("player {0} got transform currencies info failed: pc is null", msg.Uid);
        //        return;
        //    }

        //    playerEnter.Player.LoadCurrenciesTransform(msg.Currencies);
        //    TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.CURRENCIES);
        //    playerEnter.SendNeedTransformDataTag(nextTag);
        //}

        private void OnResponse_TransformCounterInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_COUNTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_COUNTER_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform currencies info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform currencies info failed: pc is null", msg.Uid);
                return;
            }

            playerEnter.Player.LoadCounterTransform(msg.Counters);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.COUNTER);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_CheckTitle(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_CHECK_TITLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CHECK_TITLE>(stream);
            int pcUid = msg.PcUid;
            int highestScore = msg.HighestScore;
            PlayerChar player = Api.PCManager.FindPcAnyway(pcUid);
            if (player == null)
            {
                Log.Write("checkTitle with popScore not find player {0}", pcUid);
            }
            else
            {
                player.CheckTitles(highestScore);
            }
        }

        private void OnResponse_TransformShopRechargeInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_SHOP_RECHARGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHOP_RECHARGE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform shop info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform shop info failed: pc is null", msg.Uid);
                return;
            }

            playerEnter.Player.LoadShopTransform(msg.Shop);
            playerEnter.Player.LoadCommonShopTransform(msg.CommonShop);
            playerEnter.Player.BindRechargeManager(msg.Recharge.First, msg.Recharge.AccumulateTotal, msg.Recharge.AccumulateCurrent, msg.Recharge.AccumulateDaily, msg.Recharge.AccumulatePrice, msg.Recharge.AccumulateMoney, new List<ServerModels.Recharge.RechargeHistoryItem>(), msg.Recharge.AccumulateOnceMaxMoney, msg.Recharge.LastCommonRechargeTime, msg.Recharge.PayCount);
            playerEnter.Player.BindOperationalActivity(msg.Recharge.MonthCardTime, msg.Recharge.SeasonCardTime, msg.Recharge.WeekCardStart, msg.Recharge.WeekCardEnd,msg.Recharge.MonthCardState,
                msg.Recharge.SuperMonthCardTime,msg.Recharge.SuperMonthCardState,msg.Recharge.SeasonCardState, msg.Recharge.AccumulateRechargeRewards, msg.Recharge.NewRechargeGiftScore, msg.Recharge.NewRechargeGiftRewards, msg.Recharge.GrowthFund);
            //playerEnter.Player.BindRadioGotRadioRewards(msg.GameLevel.GetContributionReward);
            playerEnter.Player.LoadFirstOrderInfo(msg.Recharge.FirstOrder);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.SHOP);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        //private void OnResponse_TransformShopInfo(MemoryStream stream, int uid = 0)
        //{
        //    // 收到zone1 career_info信息
        //    MSG_ZMZ_SHOP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHOP_INFO>(stream);
        //    PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
        //    if (playerEnter == null)
        //    {
        //        Log.Warn("player {0} got transform shop info failed: player not exist", msg.Uid);
        //        return;
        //    }
        //    if (playerEnter.Player == null)
        //    {
        //        Log.Warn("player {0} got transform shop info failed: pc is null", msg.Uid);
        //        return;
        //    }

        //    playerEnter.Player.LoadShopTransform(msg);
        //    TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.SHOP);
        //    playerEnter.SendNeedTransformDataTag(nextTag);
        //}

        //private void OnResponse_TransformTaskInfo(MemoryStream stream, int uid = 0)
        //{
        //    // 收到zone1 career_info信息
        //    MSG_ZMZ_TASK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TASK_INFO>(stream);
        //    PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
        //    if (playerEnter == null)
        //    {
        //        Log.Warn("player {0} got transform task info failed: player not exist", msg.Uid);
        //        return;
        //    }
        //    if (playerEnter.Player == null)
        //    {
        //        Log.Warn("player {0} got transform task info failed: pc is null", msg.Uid);
        //        return;
        //    }

        //    playerEnter.Player.LoadTaskTransform(msg);
        //    playerEnter.Player.LoadActivityTransform(msg.ActivityList);

        //    TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.TASK);
        //    playerEnter.SendNeedTransformDataTag(nextTag);
        //}

        private void OnResponse_TransformEmailInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_EMAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_EMAIL_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform email info failed: player not exist", msg.Uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform email info failed: pc is null", msg.Uid);
                return;
            }
            playerEnter.Player.LoadEmailTransform(msg.EmailList);
            if (msg.IsEnd)
            {
                TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.EMAIL);
                playerEnter.SendNeedTransformDataTag(nextTag);
            }
        }

        private void OnResponse_TransformHeroList(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_HERO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_HERO_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform hero info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform hero info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadHeroTransform(msg);
            if (msg.IsEnd)
            {
                TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.HERO);
                playerEnter.SendNeedTransformDataTag(nextTag);
            }
        }

        private void OnResponse_TransformHeroPosList(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_HERO_POSES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_HERO_POSES>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform hero pos info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform hero pos info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadHeroPosTransform(msg);
        }


        private void OnResponse_TransformWuhunResonance(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST
            MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform wuhun resonance info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform wuhun resonance info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadWuhunResonanceTransform(msg);
            if (msg.IsEnd)
            {
                TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.WuhunResonance);
                playerEnter.SendNeedTransformDataTag(nextTag);
            }
        }

        private void OnResponse_TransformHeoGod(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 MSG_ZMZ_HERO_GOD_INFO_LIST
            MSG_ZMZ_HERO_GOD_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_HERO_GOD_INFO_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform hero god info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform hero god  info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadTransform(msg);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.HeroGod);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformRechargeInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_RECHARGE_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_RECHARGE_MANAGER>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform recharge info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform recharge info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.BindRechargeManager(msg.First, msg.AccumulateTotal, msg.AccumulateCurrent, msg.AccumulateDaily, msg.AccumulatePrice, msg.AccumulateMoney, new List<ServerModels.Recharge.RechargeHistoryItem>(), msg.AccumulateOnceMaxMoney, msg.LastCommonRechargeTime, msg.PayCount);
            playerEnter.Player.BindOperationalActivity(msg.MonthCardTime, msg.SeasonCardTime, msg.WeekCardStart, msg.WeekCardEnd, msg.MonthCardState, 
                msg.SuperMonthCardTime, msg.SuperMonthCardState, msg.SeasonCardState, msg.AccumulateRechargeRewards, msg.NewRechargeGiftScore, msg.NewRechargeGiftRewards, msg.GrowthFund);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.RECHARGE);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_NeedTransformDataTag(MemoryStream stream, int uid = 0)
        {
            //  zone2 请求传输data
            MSG_MZ_NEED_TRANSFORM_DATA_TAG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_NEED_TRANSFORM_DATA_TAG>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.CharacterUid);
            if (pc == null)
            {
                Log.Warn("player {0} need transform data tag {1} failed: player not exist", msg.CharacterUid, msg.Tag);
                return;
            }
            pc.SendTransformData((TransformStep)msg.Tag);
        }

        private void OnResponse_TransformDungeonInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DUNGEON_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DUNGEON_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform dungeon info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform dungeon info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadIntegralBossFromTransform(msg.IntegralBoss);
            playerEnter.Player.HuntingManager.LoadFromTransform(msg.HuntingInfo);
            playerEnter.Player.LoadArenaFromTransform(msg.Arena);
            playerEnter.Player.LoadBenefitTransform(msg.BenefitInfo);
            playerEnter.Player.SecretAreaManager.LoadTransform(msg.SecretAreaInfo);
            //playerEnter.Player.ChapterManager.LoadTransform(msg.ChapterInfo);
            playerEnter.Player.TowerManager.LoadTransform(msg.TowerInfo);
            playerEnter.Player.CrossInfoMng.LoadTransform(msg.CrossBattleInfo);
            playerEnter.Player.OnhookManager.LoadTransform(msg.OnHookInfo);
            playerEnter.Player.LoadPushFigureTransform(msg.PushFigure);
            playerEnter.Player.CrossChallengeInfoMng.LoadTransform(msg.CrossChallengeInfo);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DungeonInfo);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDrawInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DRAW_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DRAW_MANAGER>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform draw info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform draw info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.DrawMng.Init(msg.HeroDraw, msg.Constellation);
            playerEnter.Player.HeroMng.InitCombo(msg.HeroCombo);
            playerEnter.Player.RankRewardList.AddRange(msg.RankReward);
            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Draw);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformGodPathInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_GOD_PATH_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GOD_PATH_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform god path info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform god path info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.GodPathManager.LoadTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.GodPath);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformGiftInfoList(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_GIFT_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GIFT_INFO_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform gift list info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform gift list info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadGiftInfoTransform(msg);

            if (msg.IsEnd)
            {
                TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Gift);
                playerEnter.SendNeedTransformDataTag(nextTag);
            }
        }

        private void OnResponse_TransformActionInfoList(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_GET_TIMING_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GET_TIMING_GIFT>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform action list info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform action list info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.ActionManager.LoadFromTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Action);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformShovelTreasureInfo(MemoryStream stream, int uid = 0)
        {
            ZMZ_SHOVEL_TREASURE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<ZMZ_SHOVEL_TREASURE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform shovel treasure info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform shovel treasure info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.ShovelTreasureMng.LoadShovelTreasureInfoTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ShovelTreasure);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformThemeInfo(MemoryStream stream, int uid = 0)
        {
            ZMZ_THEME_INFO msg = MessagePacker.ProtobufHelper.Deserialize<ZMZ_THEME_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform theme info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform theme info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadThemeInfoTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Theme);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformCultivateGift(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_CULTIVATE_GIFT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CULTIVATE_GIFT_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform cultivate gift info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform cultivate gift info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadCultivateGiftTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.CultivateGift);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformPettyGift(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_PETTY_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_PETTY_GIFT>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform petty gift info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform petty gift info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadPettyGiftTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.PettyGift);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDaysRewardHero(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DAYS_REWARD_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DAYS_REWARD_HERO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform days reward hero info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform days reward hero info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadDaysRewardHeroTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DaysRewardHero);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformGarden(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_GARDEN_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GARDEN_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform garden info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform garden info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.GardenManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Garden);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDivineLove(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DIVINE_LOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DIVINE_LOVE>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform divine love info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform divine love info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadDivineLoveTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DivineLove);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformFlipCardInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_FLIP_CARD_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_FLIP_CARD_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform flip card info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform flip card info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadFlipCardTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.FlipCard);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_ISLAND_HIGH_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ISLAND_HIGH_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got TransformIslandHighInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got TransformIslandHighInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.IslandHighManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.IslandHigh);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformIslandHighGiftInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_ISLAND_HIGH_GIFT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ISLAND_HIGH_GIFT_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform island high gift info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform island high gift info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadIslandHighGiftTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.IslandHighGift);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformTridentInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_TRIDENT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TRIDENT_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform trident info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform trident info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.TridentManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Trident);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDragonBoatInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DRAGON_BOAT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DRAGON_BOAT_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform dragon boat info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform dragon boat info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadDragonBoatTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DragonBoat);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_STONE_WALL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_STONE_WALL_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform stone wall info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform stone wall info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadStoneWallTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.StoneWall);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformIslandChallengeInfo(MemoryStream stream, int uid = 0)
        { 
            MSG_ZMZ_ISLAND_CHALLENGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ISLAND_CHALLENGE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform IslandChallengeInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform IslandChallengeInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.IslandChallengeManager.LoadTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.IslandChallenge);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformCarnivalInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_CARNIVAL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CARNIVAL_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform carnival info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform carnival info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadCarnivalTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Carnival);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformTravelHeroInfos(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_TRAVEL_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TRAVEL_MANAGER>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform travel hero infos failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform travel hero infos failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadTravelHeroInfoTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.HeroTravel);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformShrekInvitationInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_SHREK_INVITATION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHREK_INVITATION_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform shrek invitation info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform shrek invitation info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadShrekInvitationTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ShrekInvitation);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformRouletteInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_ROULETTE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ROULETTE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got TransformRouletteInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got TransformRouletteInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.RouletteManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Roulette);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformCanoeInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_CANOE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CANOE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform canoe info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform canoe info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadCanoeTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Canoe);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformMainQueueInfo(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 career_info信息
            MSG_ZMZ_MAINQUEUE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_MAINQUEUE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform main queue info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform main queue info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.HeroMng.LoadMainQueueInfoTransform(msg);
        }

        private void OnResponse_TransformMidAutumnInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_MIDAUTUMN_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_MIDAUTUMN_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform mid autumn info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform mid autumn info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.MidAutumnMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.MidAutumn);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformThemeFireworkInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_THEME_FIREWORK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_THEME_FIREWORK>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform theme firework info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform theme firework info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.ThemeFireworkMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.ThemeFirework);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformNineTestInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_NINE_TEST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_NINE_TEST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform nine test info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform nine test info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.NineTestMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.NineTest);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformWarehouseItemsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_WAREHOUSE_ITEMS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_WAREHOUSE_ITEMS>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform warehouse items info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform warehouse items info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadWarehouseItemsTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Warehouse);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDiamondRebateInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DIAMOND_REBATE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DIAMOND_REBATE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform diamond rebate info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform diamond rebate info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadDiamondRebateTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DiamondRebate);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformXuanBoxInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_XUANBOX_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_XUANBOX_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform XuanBoxInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform XuanBoxInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.XuanBoxManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.XuanBox);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformWishLanternInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_WISH_LANTERN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_WISH_LANTERN>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform WishLanternInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform WishLanternInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.WishLanternManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.WishLantern);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformSchoolInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_SCHOOL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SCHOOL_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform school info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform school info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.SchoolManager.LoadSchoolTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.School);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformSchoolTaskInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_SCHOOL_TASK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SCHOOL_TASK_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform school task info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform school task info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.SchoolManager.LoadSchoolTaskTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.SchoolTask);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformAnswerQuestionInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_ANSWER_QUESTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ANSWER_QUESTION_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform answer question info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform answer question info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.SchoolManager.LoadAnswerQuestionTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.AnswerQuestion);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformPetList(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_PET_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_PET_LIST>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform pet list failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform pet list failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadPetListTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Pet);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformPetEggItems(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_PET_EGG_ITEMS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_PET_EGG_ITEMS>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform pet egg items failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform pet egg items failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadPetEggsTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.PetEgg);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformPetDungeonQueues(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_PET_DUNGEON_QUEUES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_PET_DUNGEON_QUEUES>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform pet dungeon queues failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform pet dungeon queues failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadPetDungeonQueuesTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.PetDungeonQueue);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDaysRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DAYS_RECHARGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DAYS_RECHARGE_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform days recharge failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform days recharge failed: pc is null", uid);
                return;
            }

            playerEnter.Player.DaysRechargeManager.LoadFromTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DaysRecharge);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformTreasureFlipCardInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_TREASURE_FLIP_CARD_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TREASURE_FLIP_CARD_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform treasure flip card info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform treasure flip card info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadTreasureFlipCardTransform(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.TreasureFlipCard);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformShreklandInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_SHREKLAND_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHREKLAND_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform ShreklandInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform ShreklandInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.ShreklandMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.Shrekland);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformSpaceTimeTowerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_SPACETIME_TOWER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SPACETIME_TOWER_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform SpaceTimeTowerInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform SpaceTimeTowerInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.SpaceTimeTowerMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.SpaceTimeTower);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }
        
        private void OnResponse_TransformDevilTrainingInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DEVIL_TRAINING_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DEVIL_TRAINING_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform deviltrainingInfo failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform deviltrainingInfo failed: pc is null", uid);
                return;
            }

            playerEnter.Player.DevilTrainingMng.LoadTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DevilTraining);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }
        
        private void OnResponse_TransformDomainBenedictionInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DOMAIN_BENEDICTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DOMAIN_BENEDICTION_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform domain benediction failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform domain benediction failed: pc is null", uid);
                return;
            }

            playerEnter.Player.LoadDomainBenedictionTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DomainBenedition);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }

        private void OnResponse_TransformDriftExploreTaskInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO>(stream);
            PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(uid);
            if (playerEnter == null)
            {
                Log.Warn("player {0} got transform drift explore task info failed: player not exist", uid);
                return;
            }
            if (playerEnter.Player == null)
            {
                Log.Warn("player {0} got transform drift explore task info failed: pc is null", uid);
                return;
            }

            playerEnter.Player.DriftExploreMng.LoadDriftExploreTaskTransformMsg(msg);

            TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.DriftExplore);
            playerEnter.SendNeedTransformDataTag(nextTag);
        }
        
        private void OnResponse_ZC_EnterWorld(MemoryStream stream, int uid = 0)
        {
            // 收到 PKS_ZC_ENTER_WORLD 用来初始化itemlist skill pet 三个tag
            //PKS_ZC_ENTER_WORLD msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_ENTER_WORLD>(stream);
            //PlayerEnter playerEnter = server.PCManager.GetPlayerEnter(msg.character_uid);
            //if (playerEnter == null)
            //{
            //    Log.Warn("player {0} got transform item skill pet failed: player not exist", msg.character_uid);
            //    return;
            //}
            //if (playerEnter.Player == null)
            //{
            //    Log.Warn("player {0} got transform item skill pet failed: pc is null", msg.character_uid);
            //    return;
            //}

            //// item list 初始化
            //playerEnter.Player.BindItemList(msg.items_list);
            //// skill 初始化
            //playerEnter.Player.Inven.SkillCDList.Clear();
            //playerEnter.Player.Inven.SkillList.Clear();
            //playerEnter.Player.SkillLevel.Clear();
            //playerEnter.Player.BindSkill(msg.Skill);

            // 请求下一个数据包 当前处理为Item SKill
            //TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.SKILL);
            //playerEnter.SendNeedTransformDataTag(nextTag);
            //playerEnter.Player.PrintItemList();
            //playerEnter.Player.PrintSkill();
        }

        private void OnResponse_TransformRecharge(MemoryStream stream, int uid = 0)
        {
            // TODO 新协议
            //MSG_RECHARGE_CURRENCIES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RECHARGE_CURRENCIES>(stream);
            //PlayerEnter playerEnter = server.PCManager.GetPlayerEnter(msg.Uid);
            //if (playerEnter == null)
            //{
            //    Log.Warn("player {0} got transform dungeon data: player not exist", msg.Uid);
            //    return;
            //}
            //if (playerEnter.Player == null)
            //{
            //    Log.Warn("player {0} got transform dungeon data failed: pc is null", msg.Uid);
            //    return;
            //}
            //playerEnter.Player.RechargeManager = msg.Recharge;
            //// 初始化 currenies
            //foreach (var coin in msg.Currencies.Currencies)
            //{
            //   playerEnter.Player.Currencies[coin.CoinNum] = coin.Count; 
            //}
            //TransformStep nextTag = playerEnter.Player.GetNextNeedTag(TransformStep.RECHARGE);
            //playerEnter.SendNeedTransformDataTag(nextTag);
        }

        //Transform step 6 传输完成
        private void OnResponse_TransformDone(MemoryStream stream, int uid = 0)
        {
            //  zone2 数据接收完毕
            MSG_MZ_TRANSFORM_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_TRANSFORM_DONE>(stream);
            //Log.Write("player {0} transform done", msg.CharacterUid);
            PlayerChar pc = Api.PCManager.FindPc(msg.CharacterUid);
            if (pc == null)
            {
                Log.Warn("player {0} need transform done failed: player not exist", msg.CharacterUid);
                return;
            }

            // 通知gate，player协议发到新的zone 
            MSG_ZGate_ENTER_OTHER_ZONE notify = new MSG_ZGate_ENTER_OTHER_ZONE();
            notify.MainId = msg.MainId;
            notify.SubId = msg.SubId;

            //!!!TODO 添加map channel
            //notify.MapId = msg.MapId;
            //notify.subId = msg.Channel;
            pc.Write(notify);

            //如果再队伍中，则需要通知relation 向新zone发送队伍信息

            if (pc.Team != null)
            {
                MSG_ZR_TRANSFORM_DONE notifyR = new MSG_ZR_TRANSFORM_DONE() { Uid= pc.Uid};
                pc.server.SendToRelation(notifyR);
            }

            // 离开世界
            pc.LeaveWorld();
        }

        private void OnResponse_AdditionKickPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ADDICTION_KICK_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ADDICTION_KICK_PLAYER>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.Uid);
            if (pc != null)
            {
                pc.Gate?.Write(new MSG_ZGC_KICK() { Result = (int)ErrorCode.AddictionKick }, msg.Uid);

                pc.CanCatchOffline = false;
                Api.PCManager.DestroyPlayer(pc, true);

                Write(new MSG_ZM_KICK_PLAYER() { Uid = msg.Uid });
            }
        }

        private void OnResponse_KickPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_KICK_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_KICK_PLAYER>(stream);
            PlayerChar pc = Api.PCManager.FindPc(msg.Uid);
            //Todo: 需要考虑player所在的个人副本的情况，不会释放cpu和内存
            if (pc != null)
            {
                pc.CanCatchOffline = false;
                Api.PCManager.DestroyPlayer(pc, true);
                MSG_ZM_KICK_PLAYER response = new MSG_ZM_KICK_PLAYER();
                response.Uid = msg.Uid;
                Write(response);
            }
            else
            {
                pc = Api.PCManager.FindOfflinePc(msg.Uid);
                if (pc != null)
                {
                    Api.PCManager.RemoveOfflinePc(msg.Uid);
                    MSG_ZM_REMOVE_OFFINE_CLIENT notify = new MSG_ZM_REMOVE_OFFINE_CLIENT();
                    notify.UidList.Add(msg.Uid);
                    Write(notify);
                }
            }
        }

        private void OnResponse_PullPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_PULL_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_PULL_PLAYER>(stream);
            Log.Write("manager try pull player {0} to map {1} channel {2}", msg.Uid, msg.MapId, msg.Channel);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.NotStableInMap())
            {
                return;
            }
            if (msg.TeamLimit == true && player.Team == null)
            {
                return;
            }
            Vec2 beginPos = new Vec2(msg.BeginPosX, msg.BeginPosY);
            player.AskForEnterMap(msg.MapId, msg.Channel, beginPos, true);
        }

        private void OnResponse_Shutdown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("manager request main {0} sub {1} shutdown", Api.MainId, Api.SubId);
            Api.StopServer(2);
        }

        private void OnResponse_GetRechargeHistoryId(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_RECHARGE_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GET_RECHARGE_ID>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            {
                if (player == null)
                {
                    player = Api.PCManager.FindOfflinePc(msg.Uid);
                }
            }
            if (player == null)
            {
                Log.Warn($"player {msg.Uid} get recharge history id not find player");
                return;
            }
            player.SendRechargeHistoryId(msg.OrderId, msg.GiftId);
        }
        
        private void OnResponse_UpdatePcRechargeManager(MemoryStream stream, int uid = 0)
        {
            // 收到zone1 character_info信息
            MSG_MZ_UPDATE_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_UPDATE_RECHARGE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player == null)
                {
                    // 判断是否恰好处理跨zone完成状态
                    // 如player在zone1 充值后跨zone到zone2
                    // 此时zone2数据全部接收完毕，player从zone1断开连接，在完全进入zone2之前订单到账情况，需要在zone2完成订单发放
                    PlayerEnter playerEnter = Api.PCManager.GetPlayerEnter(msg.Uid);
                    if (playerEnter != null && playerEnter.TransformDone == true && playerEnter.Player != null)
                    {
                        player = playerEnter.Player;
                        Log.Write("player {0} recharge in transform done state", player.Uid);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            Log.Write($"player {msg.Uid} recharge: {msg.OrderId} info:{msg.OrderId} id:{msg.RechargeId}  money:{msg.Money}  payCurrency:{msg.PayCurrency} way:{msg.Way}");
            player.GetRechargeRewardNew(msg.OrderId, msg.OrderInfo, msg.RechargeId, (RechargeWay)msg.Way, msg.Money, msg.PayCurrency, api.NowString(), msg.Num, int.Parse(msg.IsSandbox), int.Parse(msg.PayMode));
        }

        private void OnResponse_UnVoice(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_UNVOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_UNVOICE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            {
                if (player == null)
                {
                    player = Api.PCManager.FindOfflinePc(msg.Uid);
                }
            }
            if (player == null)
            {
                return;
            }
            player.SilenceReason = msg.Reason;
            player.SilenceTime = DateTime.Parse(msg.Time);
        }

        private void OnResponse_Voice(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_VOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_VOICE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            {
                if (player == null)
                {
                    player = Api.PCManager.FindOfflinePc(msg.Uid);
                }
            }
            if (player == null)
            {
                return;
            }
            player.SilenceReason = "";
            player.SilenceTime = DateTime.MinValue;
        }

        private void OnResponse_MovePlayer(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_MOVE_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_MOVE_PLAYER>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.AskForEnterMap(msg.MapId, 1, new Vec2(msg.PosX, msg.PosY));
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    // 清楚缓存
                    Api.PCManager.RemoveOfflinePc(msg.Uid);
                    MSG_ZM_REMOVE_OFFINE_CLIENT notify = new MSG_ZM_REMOVE_OFFINE_CLIENT();
                    notify.UidList.Add(msg.Uid);
                    Write(notify);
                }
            }
        }

        private void OnResponse_VirtualRecharge(MemoryStream stream, int uid = 0)
        {
            //MSG_MZ_VIRTUAL_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_VIRTUAL_RECHARGE>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.UpdateRechargeManager(msg.money);
            //}
            //else
            //{
            //    player = server.PCManager.FindOfflinePc(msg.Uid);
            //    if (player != null)
            //    {
            //        player.UpdateRechargeManager(msg.money);
            //    }
            //}
        }

        private void OnResponse_BadWords(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_BAD_WORDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_BAD_WORDS>(stream);
            Api.sensitiveWordChecker.AddOneBadWord(msg.Content);
        }

        private void OnResponse_KickAllPlayer(MemoryStream stream, int uid = 0)
        {
            //Log.Warn("open door now");
            GameConfig.CatchOfflinePlayer = false;
            //server.ClientMng.DestroyAllClients();
        }

        private void OnResponse_CloseDoor(MemoryStream stream, int uid = 0)
        {
            //Log.Warn("close door now");
            GameConfig.CatchOfflinePlayer = true;
        }

        private void OnResponse_SetFps(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);
        }

        private void OnResponse_PlayerLevel(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_PLAYER_LEVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_PLAYER_LEVEL>(stream);
            Log.Write("manager request set player {0} level {1}", msg.Uid, msg.Level);
            PlayerChar player = Api.PCManager.FindPcAnyway(msg.Uid);
            if (player == null)
            {
                return;
            }
            player.Level = msg.Level;
            player.LastLevelUpTime = ZoneServerApi.now;
            Api.GameDBPool.Call(new QueryUpdatePlayerLevel(msg.Uid, msg.Level, ZoneServerApi.now));
            // TODO notify client
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_UPDATE_XML msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_UPDATE_XML>(stream);
            if (msg.Type == 1)
            {
                Api.UpdateServerXml();
            }
            else
            {
                Api.UpdateXml();

                //停止时间触发器
                Api.TaskTimerMng.Stop();
                //重新添加时间触发器
                Api.PCManager.InitTimerManager(ZoneServerApi.now);
                //server.InitActivityReward();
                Api.PCManager.InitRechargeTimerManager(ZoneServerApi.now, 0);
            }
            Log.Write("GM update xml");

        }

        private void OnResponse_MapChannelInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_MAP_CHANNEL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_MAP_CHANNEL_INFO>(stream);
            MapChannelInfoList.Clear();
            foreach (var item in msg.InfoList)
            {
                MapChannelInfo info = new MapChannelInfo(item.MapId, item.MinChannel, item.MaxChannel);
                MapChannelInfoList.Add(item.MapId, info);
            }
        }

        private void OnResponse_RechargeGetReward(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_RECHARGE_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_RECHARGE_GET_REWARD>(stream);
            //Log.Write("manager request player {0} get recharge {1} orderId {2}", msg.Uid, msg.RechargeType, msg.OrderId);
            //PlayerChar player = Api.PCManager.FindPcAnyway(msg.Uid);
            //if (player == null)
            //{
            //    return;
            //}
            //player.GetRechargeReward(msg.RechargeType, msg.OrderId, msg.Money, msg.Time);
            //player.GetRechargeRewardNew(msg.RechargeType, msg.OrderId, msg.Money, msg.Time);
            string[] args = new string[] { " 1000" };
            Api.Init(args);
        }

        private void OnResponse_AddReward(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ADD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ADD_REWARD>(stream);
            Log.Write("manager request player add reward {0}", msg.Reward);
                 
            foreach (var player in Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")))
            {
                RewardManager rewards = new RewardManager();
                rewards.InitSimpleReward(msg.Reward);
                player.AddRewards(rewards, ObtainWay.GM);
            }
        }

        private void OnResponse_EquipHero(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_EQUIP_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_EQUIP_HERO>(stream);
            Log.Write("manager request player equip hero {0}", msg.HeroId);

            HeroModel model = HeroLibrary.GetHeroModel(msg.HeroId);
            if (model == null)
            {
                return;
            }
            //Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.EquipHero(msg.HeroId, msg.Equip));
        }

        private void OnResponse_AllChat(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ALL_CHAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ALL_CHAT>(stream);
            Log.Write("manager request player all chat, channel {0} words {1} param {2}", msg.Channel, msg.Words, msg.Param);
            foreach (var player in Api.PCManager.PcList)
            {
                MSG_GateZ_CHAT chat = new MSG_GateZ_CHAT();
                chat.ChatChannel = msg.Channel;
                chat.PcUid = player.Value.Uid;
                chat.Content = msg.Words;
                chat.Param = msg.Param;

                switch ((ChatChannel)chat.ChatChannel)
                {
                    //case ChatChannel.Camp:
                    //case ChatChannel.Team:
                    //    Api.RelationServer.AddChat(player.Value, chat);
                    //    break;
                    case ChatChannel.World:
                        player.Value.SendWorldChat(chat);
                        break;
                    case ChatChannel.Person:
                        if (chat.Param == player.Value.Uid)
                        {
                            continue;
                        }
                        player.Value.SendPersonChat(chat);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnResponse_AbsorbSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ABSORB_SOULRING>(stream);
            Log.Write("manager request player absorb soul ring, hero {0} slot {1}", msg.HeroId, msg.Slot);
            HeroModel model = HeroLibrary.GetHeroModel(msg.HeroId);
            if (model == null)
            {
                return;
            }

            foreach (var player in Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")))
            {
                HeroInfo hero = player.HeroMng.GetHeroInfo(model.Id);
                if (hero == null)
                {
                    continue;
                }
                ulong itemUid = player.BagManager.SoulRingBag.GetItemUid(hero.Id);
                if (itemUid != 0 && player.SoulRingManager.GetEquipedSoulRing(hero.Id, msg.Slot) == null)
                {
                    player.AbsorbSoulRing(msg.HeroId, itemUid, msg.Slot);
                }
            }
        }

        private void OnResponse_AbsorbFinish(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ABSORB_FINISH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ABSORB_FINISH>(stream);
            Log.Write("manager request player absorb finish, hero {0}", msg.HeroId);
            HeroModel model = HeroLibrary.GetHeroModel(msg.HeroId);
            if (model == null)
            {
                return;
            }
            //Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.SoulRingAbsorbFinish(msg.HeroId));
            foreach (var player in Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")))
            {
                HeroInfo hero = player.HeroMng.GetHeroInfo(model.Id);
                if (hero == null)
                {
                    continue;
                }
                if (player.BagManager.SoulRingBag.CheckIsAbsorbed(hero.Id))
                {
                    player.SoulRingAbsorbFinish(hero.Id);
                }
            }
        }

        private void OnResponse_AddHeroExp(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ADD_HEROEXP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ADD_HEROEXP>(stream);
            Log.Write("manager request player add hero exp {0}", msg.Exp);
            Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.AddHeroExp(msg.Exp));
        }
        
        private void OnResponse_HeroAwaken(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_HERO_AWAKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_HERO_AWAKEN>(stream);
            Log.Write("manager request player hero awaken {0}", msg.HeroId);
            HeroModel model = HeroLibrary.GetHeroModel(msg.HeroId);
            if (model == null)
            {
                return;
            }

            Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.HeroAwaken(msg.HeroId));
        }

        private void OnResponse_HeroLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_HERO_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_HERO_LEVELUP>(stream);
            Log.Write("manager request player hero {0} level up", msg.HeroId);
            HeroModel model = HeroLibrary.GetHeroModel(msg.HeroId);
            if (model == null)
            {
                return;
            }
            Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.HeroLevelUp(msg.HeroId));
        }

        private void OnResponse_UpdateHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_UPDATE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_UPDATE_HERO_POS>(stream);
            Log.Write("manager request player update hero pos {0}", msg.HeroPos);
            MSG_GateZ_UPDATE_HERO_POS request = new MSG_GateZ_UPDATE_HERO_POS();
            string[] heroPos = msg.HeroPos.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in heroPos)
            {
                string[] heroStr = item.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int heroId = 0;
                int pos = 0;
                bool delete = false;
                if (heroStr.Length > 1)
                {
                    heroId = heroStr[0].ToInt();
                    pos = heroStr[1].ToInt();
                }
                if (heroStr.Length > 2)
                {
                    delete = heroStr[2].ToBool();
                }
                request.HeroPos.Add(new MSG_GateZ_HERO_POS()
                {
                    HeroId = heroId,
                    Delete = delete,
                    PosId = pos
                });
            }
            Api.PCManager.PcList.Values.Where(pc => pc.AccountName.Contains("FR")).ForEach(pc => pc.UpdateHeroPos(request));
        }

        public void OnResponse_AllServerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_NOTIFY_SERVER_STATE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_NOTIFY_SERVER_STATE_INFO>(stream);
            
            Api.PCManager.PcList.ForEach(x =>
            {
                x.Value.RecordAction(ActionType.OpenNewServer, msg.ServerCount);
                x.Value.RecordAction(ActionType.OnlineHighest, msg.InGameCount);
            });
        }

        public void OnResponse_GiftOpen(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GIFT_OPEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GIFT_OPEN>(stream);
            Log.Write("manager request player gift {0} open", msg.GiftItemId);
            PlayerChar player = Api.PCManager.PcList.Values.FirstOrDefault();
            if (player == null)
            {
                return;
            }
            //player.RecordActionTriggerGiftTime(msg.GiftItemId);
        }


        public void OnResponse_GetSpecialActivityItems(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM>(stream);
            //Log.Write("manager request player gift {0} open", msg.GiftItemId);
            TotalCount = msg.TotalCount;
            UseCount = msg.UseCount;

            if (msg.Uid > 0)
            {
                PlayerChar player = Api.PCManager.FindPc(msg.Uid);
                {
                    if (player == null)
                    {
                        player = Api.PCManager.FindOfflinePc(msg.Uid);
                    }
                }
                if (player == null)
                {
                    return;
                }
                player.SpecialActivityCompleteCallBack(msg.Id, msg.Items.ToDictionary(k => k.Key, v => v.Value));
            }

        }


        
        public void OnResponse_CreateMap(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_CREATE_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_CREATE_MAP>(stream);
            Api.MapManager.AddMap(msg.MapId, msg.Channel);
        }

        private void OnResponse_GetRunAwayType(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_RUNAWA_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GET_RUNAWA_TYPE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.GetRunawayTypeReturn(msg.RunAwayType, msg.InterveneId, msg.DataBox);
            }
        }

        private void OnResponse_GetSdkGift(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_SDK_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GET_SDK_GIFT>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.ReturnSdkGift(msg);
            }
        }

        private void OnResponse_GetSchoolId(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_SCHOOL_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_GET_SCHOOL_ID>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null || msg.SchoolId == 0) return;

            player.EnterSchool(msg.SchoolId, true);
        }
    }
}
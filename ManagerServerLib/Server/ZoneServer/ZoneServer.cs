using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Barrack.Protocol.BM;
using Message.IdGenerator;
using Message.Manager.Protocol.MB;
using Message.Manager.Protocol.MG;
using Message.Manager.Protocol.MZ;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels;
using ServerShared;
using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;

namespace ManagerServerLib
{
    public partial class ZoneServer : FrontendServer
    {
        // key character_uid value Client 该zone下的所有角色
        private Dictionary<int, Client> clientListZone = new Dictionary<int, Client>();
        public Dictionary<int, Client> ClientListZone
        { get { return clientListZone; } }

        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        public ZoneServerManager ZoneServerManager
        {
            get { return (ZoneServerManager)serverManager; }
        }

        // <mapId_channel, Map>
        private Dictionary<string, Map> allMap = new Dictionary<string, Map>();
        public Dictionary<string, Map> AllMap
        { get { return allMap; } }


        private int sleepTime = 0;
        public int SleepTime
        { get { return sleepTime; } }

        private int frameCount = 0;
        public int FrameCount
        { get { return frameCount; } }

        private long memory = 0;
        public long Memory
        { get { return memory; } }

        public ZoneServer(BaseApi api)
            : base(api)
        {
        }

        public void Regist(MSG_ZM_REGISTER msg)
        {
            allMap.Clear();
            foreach (var item in msg.MapList)
            {
                AddMap(item.MapId, item.Channel);
            }
        }

        public Map AddMap(int mapId, int channel)
        {
            Map newMap = new Map(MainId, SubId, mapId, channel);
            string key = string.Format("{0}_{1}", mapId, channel);
            try
            {
                allMap.Add(key, newMap);
            }
            catch (Exception e)
            {
                Log.ErrorLine("add zone main id {0} sub id {1} server map {2} channel {3} failed: {4}",
                    MainId, SubId, mapId, channel, e.ToString());
            }
            return newMap;
        }

        public void RemoveMap(int mapId, int channel)
        {
            string key = string.Format("{0}_{1}", mapId, channel);
            allMap.Remove(key);
        }


        protected override void OnDisconnect()
        {
            ZoneServerManager.OnlineCount -= clientListZone.Count;
            ZoneServerManager.CheckOnlinePlayerState();
            base.OnDisconnect();
        }

        public override void Update(double dt)
        {
            base.Update(dt);

            foreach (var item in allMap)
            {
                try
                {
                    item.Value.UpdateWillEnterList();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_ZM_REGISTER>.Value, OnResponse_Regist);
            AddResponser(Id<MSG_ZM_REGIST_CLIENT_LIST>.Value, OnResponse_RegistClientList);
            AddResponser(Id<MSG_ZM_REGIST_OFFLINE_CLIENT_LIST>.Value, OnResponse_RegistOfflineClientList);

            AddResponser(Id<MSG_ZM_CLIENT_ENTER>.Value, OnResponse_ClientEnter);
            AddResponser(Id<MSG_ZM_CLIENT_ENTER_MAP>.Value, OnResponse_ClientEnterMap);
            AddResponser(Id<MSG_ZM_CLIENT_LEAVE_MAP>.Value, OnResponse_ClientLeaveMap);
            AddResponser(Id<MSG_ZM_CPU_INFO>.Value, OnResponse_CpuInfo);
            AddResponser(Id<MSG_ZM_ASK_FOR_MAP>.Value, OnResponse_AskForMap);
            AddResponser(Id<MSG_ZM_CLIENT_LEAVE_ZONE>.Value, OnResponse_ClientLeaveZone);
            AddResponser(Id<MSG_ZM_MAP_CHANNEL_COUNT>.Value, OnResponse_MapChannelCount);
            AddResponser(Id<MSG_ZM_CHANGE_CHANNEL>.Value, OnResponse_ChangeChannel);
            AddResponser(Id<MSG_ZM_CLIENT_ENTER_TRANSFORM>.Value, OnResponse_ClientEnterTransform);
            //
            AddResponser(Id<MSG_ZMZ_CHARACTER_INFO>.Value, OnResponse_TransformCharacterInfo);
            AddResponser(Id<MSG_ZMZ_ACTIVITY_MANAGER>.Value, OnResponse_TransformActivity);
            AddResponser(Id<MSG_ZMZ_REDIS_DATA>.Value, OnResponse_TransformRedisData);
            AddResponser(Id<MSG_ZMZ_BAG_INFO>.Value, OnResponse_TransformBagInfo);
            AddResponser(Id<MSG_ZMZ_CURRENCIES_INFO>.Value, OnResponse_TransformCurrenciesInfo);
            AddResponser(Id<MSG_ZMZ_COUNTER_INFO>.Value, OnResponse_TransformCounterInfo);
            AddResponser(Id<MSG_ZMZ_SHOP_INFO>.Value, OnResponse_TransformShopInfo);
            AddResponser(Id<MSG_ZMZ_HERO_LIST>.Value, OnResponse_TransformHeroInfo);
            AddResponser(Id<MSG_ZMZ_HERO_POSES>.Value, OnResponse_TransformHeroPosInfo);
            AddResponser(Id<MSG_ZMZ_EMAIL_INFO>.Value, OnResponse_TransformEmailInfo);
            AddResponser(Id<MSG_ZMZ_RECHARGE_MANAGER>.Value, OnResponse_TransformRechargeInfo);
            AddResponser(Id<MSG_ZMZ_ITEMS_EMAIL_INFO>.Value, OnResponse_TransformKnapsackAndEmailInfo);
            AddResponser(Id<MSG_ZMZ_SHOP_RECHARGE_INFO>.Value, OnResponse_TransformShopAndRechargeInfo);
            AddResponser(Id<MSG_ZMZ_DUNGEON_INFO>.Value, OnResponse_TransformDungeonInfo);
            AddResponser(Id<MSG_ZMZ_DRAW_MANAGER>.Value, OnResponse_TransformDrawInfo);
            AddResponser(Id<MSG_ZMZ_GOD_PATH_INFO>.Value, OnResponse_TransformGodPathInfo);
            AddResponser(Id<MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST>.Value, OnResponse_TransformWuhunResonance);
            AddResponser(Id<MSG_ZMZ_HERO_GOD_INFO_LIST>.Value, OnResponse_TransformHeroGodListInfo);
            AddResponser(Id<MSG_ZMZ_GIFT_INFO_LIST>.Value, OnResponse_TransformGiftListInfo);
            AddResponser(Id<MSG_ZMZ_GET_TIMING_GIFT>.Value, OnResponse_TransformActionInfo);
            AddResponser(Id<ZMZ_SHOVEL_TREASURE_INFO>.Value, OnResponse_TransformShovelTreasureInfo);
            AddResponser(Id<ZMZ_THEME_INFO>.Value, OnResponse_TransformThemeInfo);
            AddResponser(Id<MSG_ZMZ_CULTIVATE_GIFT_LIST>.Value, OnResponse_TransformCultivateGift);
            AddResponser(Id<MSG_ZMZ_PETTY_GIFT>.Value, OnResponse_TransformPettyGift);
            AddResponser(Id<MSG_ZMZ_DAYS_REWARD_HERO>.Value, OnResponse_TransformDaysRewardHero);
            AddResponser(Id<MSG_ZMZ_GARDEN_INFO>.Value, OnResponse_TransformGardenInfo);
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
            AddResponser(Id<MSG_ZMZ_XUANBOX_INFO>.Value, OnResponse_TransformXuanBoxInfo);
            AddResponser(Id<MSG_ZMZ_WISH_LANTERN>.Value, OnResponse_TransformWishLanternInfo);
            AddResponser(Id<MSG_ZMZ_SCHOOL_INFO>.Value, OnResponse_TransformSchoolInfo);
            AddResponser(Id<MSG_ZMZ_SCHOOL_TASK_INFO>.Value, OnResponse_TransformSchoolTaskInfo);
            AddResponser(Id<MSG_ZMZ_ANSWER_QUESTION_INFO>.Value, OnResponse_TransformAnswerQuestionInfo);
            AddResponser(Id<MSG_ZMZ_DIAMOND_REBATE_INFO>.Value, OnResponse_TransformDiamondRebateInfo);
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

            AddResponser(Id<MSG_ZMZ_CHECK_TITLE>.Value, OnResponse_BroadCastTitleCheck);

            AddResponser(Id<MSG_ZM_NEED_TRANSFORM_DATA_TAG>.Value, OnResponse_NeedTransfomDataTag);

            //AddResponser(Id<PKS_ZC_ENTER_WORLD>.Value, OnResponse_ZC_EnterWorld);
            //AddResponser(Id<MSG_RECHARGE_CURRENCIES>.Value, OnResponse_RechargeCurrencies);
            AddResponser(Id<MSG_ZM_NEW_MAP>.Value, OnResponse_NewMap);
            AddResponser(Id<MSG_ZM_DELETE_MAP>.Value, OnResponse_DeleteMap);
            AddResponser(Id<MSG_ZM_CLOSE_MAP>.Value, OnResponse_CloseMap);
            AddResponser(Id<MSG_ZM_OPEN_MAP>.Value, OnResponse_OpenMap);
            AddResponser(Id<MSG_ZM_KICK_PLAYER>.Value, OnResponse_KickPlayer);
            AddResponser(Id<MSG_ZM_PULL_PLAYER>.Value, OnResponse_PullPlayer);
            AddResponser(Id<MSG_ZM_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_BroadcastAnnouncement);
            AddResponser(Id<MSG_ZM_ENTER_ZONE>.Value, OnResponse_EnterZone);
            AddResponser(Id<MSG_ZM_CATCH_OFFLINE_CLIENT>.Value, OnResponse_CatchOfflineClient);
            AddResponser(Id<MSG_ZM_REMOVE_OFFINE_CLIENT>.Value, OnResponse_RemoveOfflineClient);
            AddResponser(Id<MSG_ZM_DB_EXCEPTION>.Value, OnResponse_DBException);
            AddResponser(Id<MSG_ZM_BLACK_IP>.Value, OnResponse_BlackIp);

            AddResponser(Id<MSG_ZM_LOGOUT>.Value, OnResponse_ClientLogout);

            AddResponser(Id<MSG_ZM_DEBUG_RECHARGE>.Value, OnResponse_DebugRecharge);
            AddResponser(Id<MSG_ZM_BUY_RECHARGE_GIFT>.Value, OnResponse_BuyRechargeGift);
            AddResponser(Id<MSG_ZM_GET_RECHARGE_ID>.Value, OnResponse_GetRechargeId);
            AddResponser(Id<MSG_ZM_RECHARGE_TOKEN>.Value, OnResponse_DoRechargeToken);
            AddResponser(Id<MSG_ZM_ACTIVATE_ITEM>.Value, OnResponse_ActivateItem);
            AddResponser(Id<MSG_ZM_GET_SPECIAL_ACTIVITY_ITEM>.Value, OnResponse_GetSpecialActivityItems);

            //Dungeon
            AddResponser(Id<MSG_ZM_NEED_DUNGEON>.Value, OnResponse_NeedDungeon);
            AddResponser(Id<MSG_ZM_CREATE_DUNGEON_FAILED>.Value, OnResponse_CreateDungeonFailed);
            AddResponser(Id<MSG_ZMZ_HUNTING_CHANGE>.Value, OnResponse_HuntingChange);

            AddResponser(Id<MSG_ZM_UPDATE_RECHARGE>.Value, OnResponse_UpdateRechargeState);

            //学院
            AddResponser(Id<MSG_ZM_GET_SCHOOL_ID>.Value, OnResponse_GetSchoolId);

            //流失干预
            AddResponser(Id<MSG_ZM_GET_RUNAWA_TYPE>.Value, OnResponse_GetRunAwayType);
            AddResponser(Id<MSG_ZM_GET_SDK_GIFT>.Value, OnResponse_GetGiftType);

            //ResponserEnd 
        }


        public bool IsSameZone(int main_id, int sub_id)
        {
            return (MainId == main_id && SubId == sub_id);
        }

        public bool IsSameMap(Map map_1, Map map_2)
        {
            if (map_1 == null) return false;
            if (map_2 == null) return false;
            return (map_1.MapId == map_2.MapId && map_2.Channel == map_2.Channel);
        }

        public Map GetMap(int map_id, int channel)
        {
            string key = string.Format("{0}_{1}", map_id, channel);
            return GetMap(key);
        }

        public Map GetMap(string key)
        {
            Map map = null;
            if (!allMap.TryGetValue(key, out map))
            {
                return null;
            }
            return map;
        }

        // 尝试进入该地图 能进入返回该Map 否则返回null
        public Map CanEnterMap(int map_id, int channel, bool forceEnter = false)
        {
            Map map = GetMap(map_id, channel);
            if (map == null)
            {
                return null;
            }
            if (map.State == MapState.CLOSE)
            {
                return null;
            }

            // 判断map是否已满
            Data mapData = Api.MapDataList.Get(map_id);
            if (forceEnter)
            {
                // 跟随队长或者切线 条件放宽
                if (map.ClientCount < mapData.GetInt("MaxNum"))
                {
                    return map;
                }
            }
            else
            {
                if (map.ClientCount < mapData.GetInt("UniformNum"))
                {
                    return map;
                }
            }
            return null;
        }

        public Map FindOneMap(int map_id, bool insist = false)
        {
            if (state != ServerState.Started)
            {
                return null;
            }
            if (insist == true)
            {
                // 如果坚持要在当前Zone找到能承载的map
                // 则尝试找到第一个没到MaxPC的map 
                foreach (var item in allMap)
                {
                    string key = item.Key;
                    if (int.Parse(key.Split('_')[0]) == map_id)
                    {
                        if (item.Value.ClientCount < item.Value.MaxNum && item.Value.State == MapState.OPEN)
                        {
                            return item.Value;
                        }
                    }
                }
            }
            else
            {
                // 找到第一个没达到UniformNum的map 
                foreach (var item in allMap)
                {
                    //string key = item.Key;
                    if (item.Value.MapId == map_id)
                    {
                        if (item.Value.ClientCount < item.Value.UniformNum && item.Value.State == MapState.OPEN)
                        {
                            return item.Value;
                        }
                    }
                }
            }
            return null;
        }

        public bool ContainMap(int mapId)
        {
            return allMap.Values.Where(x => x.MapId == mapId).FirstOrDefault() != null;
        }

        public void AddClient(Client newClient)
        {
            Log.Write("zone main {0} sub {1} add client uid {2}", MainId, SubId, newClient.CharacterUid);
            if (newClient == null)
            {
                return;
            }
            try
            {
                clientListZone.Remove(newClient.CharacterUid);
                clientListZone.Add(newClient.CharacterUid, newClient);
                ZoneServerManager.OnlineCount++;
                Api.AddictionMng.SetUnderAgeOnline(newClient, true);
                ZoneServerManager.CheckOnlinePlayerState();
            }
            catch (Exception e)
            {
                Log.Alert("mainId {0} subId {1} add client {2} failed: {3}",
                    MainId, SubId, newClient.CharacterUid, e.ToString());
            }
        }

        public void RemoveClient(int character_uid)
        {
            Client client = null;
            if (clientListZone.TryGetValue(character_uid, out client))
            {
                Log.Write("zone main {0} sub {1} remove client uid {2}", MainId, SubId, client.CharacterUid);
                Map map = client.CurrentMap;
                if (map != null)
                {
                    try
                    {
                        map.ClientListMap.Remove(client.CharacterUid);
                    }
                    catch (Exception e)
                    {
                        Log.Alert("map {0} channel {1} remove client {2} falied {3}",
                            map.MapId, map.Channel, client.CharacterUid, e.ToString());
                    }
                }
                //lock (clientListZone)
                {
                    try
                    {
                        ClientListZone.Remove(client.CharacterUid);
                        ZoneServerManager.OnlineCount--;
                        Api.AddictionMng.SetUnderAgeOnline(client, false);
                        ZoneServerManager.CheckOnlinePlayerState();
                    }
                    catch (Exception e)
                    {
                        Log.Alert("zone main {0} sub {1} remove client {2} falied {3}",
                            MainId, SubId, client.CharacterUid, e.ToString());
                    }
                }
                //if (client.CurrentMap != null)
                //{
                //    Log.Write("player c_uid {0} leave zone mainId {1} subId {2} map {3} channel {4}",
                //        client.CharacterUid, mainId, subId, client.CurrentMap.MapId, client.CurrentMap.Channel);
                //}
                client.LeaveMap();
            }
        }

        public Client GetClient(int character_uid)
        {
            Client client = null;
            clientListZone.TryGetValue(character_uid, out client);
            return client;
        }

        public void OnResponse_ClientEnter(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLIENT_ENTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLIENT_ENTER>(stream);
            if (msg.IsExpried == true)
            {
                RemoveClient(msg.CharacterUid);
            }
        }

        public void OnResponse_ClientEnterMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLIENT_ENTER_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLIENT_ENTER_MAP>(stream);
            Client client = null;
            if (clientListZone.TryGetValue(msg.CharacterUid, out client))
            {
                Map map = GetMap(msg.MapId, msg.Channel);
                if (map == null)
                {
                    Log.Warn("player {0} enter map mainId {1} subId {2} map {3} channel {4} failed: map not exists",
                        client.CharacterUid, MainId, SubId, msg.MapId, msg.Channel);
                    return;
                }
                client.EnterMap(map);
                client.CurrentMap.AddClient(client);
                map.ClientEnter(client.CharacterUid);
                Log.Write("player {0} enter map mainId {1} subId {2} map {3} channel {4}",
                   client.CharacterUid, MainId, SubId, client.CurrentMap.MapId, client.CurrentMap.Channel);
            }
        }

        public void OnResponse_ClientLeaveMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLIENT_LEAVE_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLIENT_LEAVE_MAP>(stream);
            Client client = null;
            Map map = GetMap(msg.MapId, msg.Channel);
            if (map == null)
            {
                Log.Warn("client {0} leave map {1} channel {2} failed: map not in list", msg.CharacterUid, msg.MapId, msg.Channel);
                return;
            }
            if (map.ClientListMap.TryGetValue(msg.CharacterUid, out client))
            {
                if (IsSameMap(client.CurrentMap, map))
                {
                    // 防止服务器卡顿情况 client已经切换到其他图，原始图才通知manager离开地图
                    Log.Write("player {0} leave map mainId {1} subId {2} map {3} channel {4}",
                         client.CharacterUid, client.CurrentMap.MainId, client.CurrentMap.SubId, client.CurrentMap.MapId, client.CurrentMap.Channel);
                    client.LeaveMap();
                }
            }
            map.RemoveClient(msg.CharacterUid);
        }

        public void OnResponse_ClientLeaveZone(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLIENT_LEAVE_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLIENT_LEAVE_ZONE>(stream);
            Client client = null;
            if (clientListZone.TryGetValue(msg.CharacterUid, out client))
            {
                if (client.Zone != null && IsSameZone(client.Zone.MainId, client.Zone.SubId))
                {
                    // 当前map属于当前源zone1 
                    // 防止跨zone切图 目标zone2协议先到manager client的map已更新到新map 如果不做验证逻辑错误
                    Log.Write("player {0} leave zone mainId {1} subId {2}",
                        client.CharacterUid, client.Zone.MainId, client.Zone.SubId);
                    client.LeaveZone();
                }
                client.DestZone = null;
                clientListZone.Remove(client.CharacterUid);
                ZoneServerManager.OnlineCount--;
                Api.AddictionMng.SetUnderAgeOnline(client, false);
                ZoneServerManager.CheckOnlinePlayerState();
            }
        }

        public void OnResponse_CpuInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CPU_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CPU_INFO>(stream);
            sleepTime = msg.SleepTime;
            frameCount = msg.FrameCount;
            memory = msg.Memory;
            //Log.WriteLine("mainId {0} subId {1} frame count {2} sleep time {3} memory {4}MB", mainId, subId, frameCount, sleepTime, memory);
        }

        public void OnResponse_AskForMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_ASK_FOR_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_ASK_FOR_MAP>(stream);
            Log.Write("player {0} request enter from map {1} channel {2} to map {3} channel {4}",
                msg.Uid, msg.OriginMapId, msg.OriginChannel, msg.DestMapId, msg.DestChannel);
            MSG_MZ_ANSWER_FOR_MAP response = new MSG_MZ_ANSWER_FOR_MAP();
            response.Uid = msg.Uid;
            response.DestPosX = msg.DestPosX;
            response.DestPosY = msg.DestPosY;

            Client client = null;
            if (!clientListZone.TryGetValue(msg.Uid, out client))
            {
                response.Result = (int)ErrorCode.NotExist;
                Log.Warn("player {0} request enter map {1} channel {2} failed: not in zone list", msg.Uid, msg.DestMapId, msg.DestChannel);
                Write(response);
                return;
            }

            ZoneServer zone = null;
            Map map = null;
            int mapId = msg.DestMapId;
            int channel = msg.DestChannel;

            ZoneServerManager.GetZone(mapId, channel, out zone, out map, msg.ForceEnter);
            if (zone != null && map != null)
            {
                //Log.Warn("player {0} ask for map {1} channel {2} real map {3} channel {4}", msg.CharacterUid, msg.MapId, msg.Channel, map.MapId, map.Channel);
                response.SubId = zone.SubId;
                response.DestMapId = map.MapId;
                response.DestChannel = map.Channel;
                response.Result = (int)ErrorCode.Success;
                if (!IsSameZone(zone.MainId, zone.SubId))
                {
                    // 需要跨进程
                    MSG_MZ_CLIENT_ENTER notify = new MSG_MZ_CLIENT_ENTER();
                    notify.Uid = msg.Uid;
                    notify.DestMapId = map.MapId;
                    notify.DestChannel = map.Channel;
                    notify.DestPosX = msg.DestPosX;
                    notify.DestPosY = msg.DestPosY;

                    notify.OriginSubId = SubId;
                    notify.OriginMapId = msg.OriginMapId;
                    notify.OriginChannel = msg.OriginChannel;
                    notify.OriginPosX = msg.OriginPosX;
                    notify.OriginPosY = msg.OriginPosY;

                    notify.NeedAnim = msg.NeedAnim;

                    client.DestZone = zone;
                    zone.Write(notify);
                    Log.Write("assign player {0} zone mainId {1} subId {2} map {3} channel {4}",
                        client.CharacterUid, zone.MainId, zone.SubId, map.MapId, map.Channel);
                }
                map.WillEnter(msg.Uid);
            }
            else
            {
                //
                Log.Warn("player {0} request enter map {1} channel {2} failed with zone {3} map {4}", msg.Uid, msg.DestMapId, msg.DestChannel, zone, map);
                response.SubId = 0;
                response.DestMapId = msg.DestMapId;
                response.DestChannel = msg.DestChannel;
                response.Result = (int)ErrorCode.Fail;
            }
            Write(response);
        }

        public void OnResponse_MapChannelCount(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_MAP_CHANNEL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_MAP_CHANNEL_COUNT>(stream);
            if (msg.MapChannelCount != null && msg.MapChannelCount.Count != 0)
            {
                // TODO Manager重启维护
            }
            else
            {
                MSG_MZ_MAP_CHANNEL_COUNT response = new MSG_MZ_MAP_CHANNEL_COUNT();
                // key map id value channel count
                Dictionary<int, int> mapChannelCount = new Dictionary<int, int>();
                foreach (var zoneItem in ZoneServerManager.ServerList)
                {
                    ZoneServer zone = zoneItem.Value as ZoneServer;
                    foreach (var mapItem in zone.allMap)
                    {
                        int mapId = int.Parse(mapItem.Key.Split('_')[0]);
                        if (mapChannelCount.ContainsKey(mapId) == false)
                        {
                            mapChannelCount.Add(mapId, 1);
                        }
                        else
                        {
                            mapChannelCount[mapId]++;
                        }
                    }
                }

                foreach (var item in mapChannelCount)
                {
                    MAP_CHANNEL_COUNT info = new MAP_CHANNEL_COUNT();
                    info.MapId = item.Key;
                    info.ChannelCount = item.Value;
                    response.MapChannelCount.Add(info);
                }
                Write(response);
            }
        }

        public void OnResponse_ChangeChannel(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CHANGE_CHANNEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CHANGE_CHANNEL>(stream);
            Log.Write("player {0} request change channel map {1} from channel {2} to channel {3}", msg.Uid, msg.MapId, msg.FromChannel, msg.ToChannel);
            MSG_MZ_CHANGE_CHANNEL response = new MSG_MZ_CHANGE_CHANNEL();
            response.Uid = msg.Uid;
            response.MapId = msg.MapId;
            response.FromChannel = msg.FromChannel;
            response.ToChannel = msg.ToChannel;
            ManagerMapInfo destManagerInfo = Api.BallenceProxy.FindTheManager(msg.MapId, msg.ToChannel);
            if (destManagerInfo == null)
            {
                // 无挂载该地图的manager
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            if (destManagerInfo.IsFull(msg.MapId, msg.ToChannel))
            {
                // 已满
                response.Result = (int)ErrorCode.FullPC;
                Write(response);
                return;
            }
            response.ToMainId = destManagerInfo.MainId;
            // 验证通过 可以切线
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void OnResponse_ClientEnterTransform(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLIENT_ENTER_TRANSFORM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLIENT_ENTER_TRANSFORM>(stream);
            ZoneServer zone = (ZoneServer)ZoneServerManager.GetServer(MainId, msg.OriginSubId);
            if (zone == null)
            {
                Log.Warn("player {0} client enter transform failed: no such zone main {1} sub {2}", msg.Uid, MainId, msg.OriginSubId);
                return;
            }

            MSG_MZ_CLIENT_ENTER_TRANSFORM notify = new MSG_MZ_CLIENT_ENTER_TRANSFORM();
            notify.Uid = msg.Uid;
            notify.Result = msg.Result;
            zone.Write(notify);
        }

        public void OnResponse_TransformCharacterInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CHARACTER_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform character info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform character info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_CHARACTER_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformActivity(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_ACTIVITY_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ACTIVITY_MANAGER>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform Activity info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform Activity info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ACTIVITY_MANAGER>.Value, uid, stream);
        }

        public void OnResponse_TransformRedisData(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_REDIS_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_REDIS_DATA>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform redis info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform redis info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_REDIS_DATA>.Value, uid, stream);
        }


        public void OnResponse_TransformBagInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform knapsack info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform knapsack info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_BAG_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformCurrenciesInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_CURRENCIES_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CURRENCIES_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform currencies info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform currencies info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_CURRENCIES_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformCounterInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_COUNTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_COUNTER_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform counters info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform counters info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_COUNTER_INFO>.Value, uid, stream);
        }





        public void OnResponse_TransformShopInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_SHOP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHOP_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform shop info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform shop info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SHOP_INFO>.Value, uid, stream);
        }


        public void OnResponse_TransformHeroInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_TASK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TASK_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform hero info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform hero info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_HERO_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformHeroPosInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_TASK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_TASK_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform hero pos info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform hero pos info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_HERO_POSES>.Value, uid, stream);
        }


        public void OnResponse_TransformWuhunResonance(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform wuhun resonance info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform wuhun resonance info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformHeroGodListInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform HeroGodListInfo failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform HeroGodListInfo failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_HERO_GOD_INFO_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformEmailInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_EMAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_EMAIL_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform email info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform email info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_EMAIL_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformRechargeInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_RECHARGE_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_RECHARGE_MANAGER>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform recharge info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform recharge info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_RECHARGE_MANAGER>.Value, uid, stream);
        }

        public void OnResponse_NeedTransfomDataTag(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_NEED_TRANSFORM_DATA_TAG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_NEED_TRANSFORM_DATA_TAG>(stream);
            ZoneServer zone = (ZoneServer)ZoneServerManager.GetServer(MainId, msg.OringinSubId);
            if (zone == null)
            {
                Log.Warn("player {0} client transform need tag failed: no such zone main {1} sub {2}", msg.CharacterUid, MainId, msg.OringinSubId);
                return;
            }

            Client client = zone.GetClient(msg.CharacterUid);
            if (client == null)
            {
                Log.Warn("player {0} client transform need tag failed: no such player", msg.CharacterUid);
                return;
            }
            if (msg.Tag != (int)TransformStep.DONE)
            {
                // 发送给zone1
                var notify = new MSG_MZ_NEED_TRANSFORM_DATA_TAG();
                notify.CharacterUid = msg.CharacterUid;
                notify.Tag = msg.Tag;
                zone.Write(notify);

            }
            else
            {
                // zone2 已经准备好
                Log.Write("player {0} transfrom data from main {1} sub {2} to sub {3} done", msg.CharacterUid, MainId, msg.OringinSubId, SubId);
                client.DestZone = null;
                MSG_MZ_TRANSFORM_DONE response = new MSG_MZ_TRANSFORM_DONE();
                response.CharacterUid = msg.CharacterUid;
                response.MainId = MainId;
                response.SubId = SubId;
                zone.Write(response);
            }
        }

        public void OnResponse_ZC_EnterWorld(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_ENTER_WORLD msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_ENTER_WORLD>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform item skill pet by PKS_ZC_ENTER_WORLD failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform item skill pet bt PKS_ZC_ENTER_WORLD failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            //client.DestZone.Write(Id<PKS_ZC_ENTER_WORLD>.Value, stream);
        }

        public void OnResponse_RechargeCurrencies(MemoryStream stream, int uid = 0)
        {
            //MSG_RECHARGE_CURRENCIES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RECHARGE_CURRENCIES>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} transform recharge currencies failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform recharge currencies failed: dest zone null", uid);
                return;
            }
            //client.DestZone.Write(Id<MSG_RECHARGE_CURRENCIES>.Value, stream);
        }

        public void OnResponse_TransformKnapsackAndEmailInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_ITEMS_EMAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_ITEMS_EMAIL_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform knapsack info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform knapsack info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ITEMS_EMAIL_INFO>.Value, uid, stream);
        }
        public void OnResponse_TransformShopAndRechargeInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_SHOP_RECHARGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_SHOP_RECHARGE_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform shop info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform shop info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SHOP_RECHARGE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformDungeonInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_DUNGEON_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DUNGEON_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform dungeon info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform dungeon info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DUNGEON_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformDrawInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_DRAW_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DRAW_MANAGER>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform draw info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform draw info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DRAW_MANAGER>.Value, uid, stream);
        }

        public void OnResponse_TransformGodPathInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_GOD_PATH_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GOD_PATH_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform god path info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform god path info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_GOD_PATH_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformGiftListInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_GIFT_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GIFT_INFO_LIST>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform gift list info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform gift list info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_GIFT_INFO_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformActionInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_GET_TIMING_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_GET_TIMING_GIFT>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform action list info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform action list info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_GET_TIMING_GIFT>.Value, uid, stream);
        }

        public void OnResponse_TransformShovelTreasureInfo(MemoryStream stream, int uid = 0)
        {
            //ZMZ_SHOVEL_TREASURE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<ZMZ_SHOVEL_TREASURE_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform shovel treasure info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform shovel treasure info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<ZMZ_SHOVEL_TREASURE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformThemeInfo(MemoryStream stream, int uid = 0)
        {
            //ZMZ_THEME_INFO msg = MessagePacker.ProtobufHelper.Deserialize<ZMZ_THEME_INFO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform theme info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform theme info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<ZMZ_THEME_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformCultivateGift(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_CULTIVATE_GIFT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_CULTIVATE_GIFT_LIST>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform cultivate gift info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform cultivate gift info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_CULTIVATE_GIFT_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformPettyGift(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_PETTY_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_PETTY_GIFT>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform petty gift info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform petty gift info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_PETTY_GIFT>.Value, uid, stream);
        }

        public void OnResponse_TransformDaysRewardHero(MemoryStream stream, int uid = 0)
        {
            //MSG_ZMZ_DAYS_REWARD_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_DAYS_REWARD_HERO>(stream);
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform days reward hero info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform days reward hero info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DAYS_REWARD_HERO>.Value, uid, stream);
        }

        public void OnResponse_TransformGardenInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform GardenInfo failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform GardenInfo info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_GARDEN_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformDivineLove(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform divine love info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform divine love info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DIVINE_LOVE>.Value, uid, stream);
        }

        public void OnResponse_TransformFlipCardInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform flip card info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform flip card info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_FLIP_CARD_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform island high info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform island high info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ISLAND_HIGH_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformIslandHighGiftInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform island high gift info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform island high gift info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ISLAND_HIGH_GIFT_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformTridentInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform trident info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform trident info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_TRIDENT_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformDragonBoatInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform dragon boat info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform dragon boat info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DRAGON_BOAT_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform stone wall info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform stone wall info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_STONE_WALL_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformIslandChallengeInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform IslandChallengeInfo failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform IslandChallengeInfo failed: dest zone null", uid);
                return;
            }

            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ISLAND_CHALLENGE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformCarnivalInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform carnival info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform carnival info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_CARNIVAL_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformTravelHeroInfos(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform carnival info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform carnival info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_TRAVEL_MANAGER>.Value, uid, stream);
        }

        public void OnResponse_TransformShrekInvitationInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform shrek invitation info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform shrek invitation info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SHREK_INVITATION_INFO>.Value, uid, stream);
        }

        private void OnResponse_TransformRouletteInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client TransformRouletteInfo info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client TransformRouletteInfo info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ROULETTE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformCanoeInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform canoe info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform canoe info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_CANOE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformMainQueueInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform main queue info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform main queue info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_MAINQUEUE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformMidAutumnInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform mid autumn info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform mid autumn info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_MIDAUTUMN_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformThemeFireworkInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform theme firework info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform theme firework info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_THEME_FIREWORK>.Value, uid, stream);
        }

        public void OnResponse_TransformNineTestInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform nine test info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform nine test info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_NINE_TEST>.Value, uid, stream);
        }

        public void OnResponse_TransformWarehouseItemsInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform warehouse items info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform warehouse items info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_WAREHOUSE_ITEMS>.Value, uid, stream);
        }

        public void OnResponse_TransformDiamondRebateInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform diamond rebate info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform diamond rebate info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DIAMOND_REBATE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformXuanBoxInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform XuanBoxInfo failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform XuanBoxInfo failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_XUANBOX_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformWishLanternInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform WishLanternInfo failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform WishLanternInfo failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_WISH_LANTERN>.Value, uid, stream);
        }

        public void OnResponse_TransformSchoolInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform school info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform school info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SCHOOL_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformPetList(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform petlist failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform petlist failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_PET_LIST>.Value, uid, stream);
        }

        public void OnResponse_TransformPetEggItems(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform pet egg items failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform pet egg items failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_PET_EGG_ITEMS>.Value, uid, stream);
        }

        public void OnResponse_TransformSchoolTaskInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform school task info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform school task info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SCHOOL_TASK_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformAnswerQuestionInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform answer question info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform answer question info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_ANSWER_QUESTION_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformPetDungeonQueues(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform PetDungeonQueues failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform PetDungeonQueues failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_PET_DUNGEON_QUEUES>.Value, uid, stream);
        }

        public void OnResponse_TransformDaysRecharge(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform DaysRecharge failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform DaysRecharge failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DAYS_RECHARGE_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformTreasureFlipCardInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform treasure flip card info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform treasure flip card info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_TREASURE_FLIP_CARD_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformShreklandInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform shrekland info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform shrekland info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SHREKLAND_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformSpaceTimeTowerInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform space time tower info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform space time tower info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_SPACETIME_TOWER_INFO>.Value, uid, stream);
        }
        
        public void OnResponse_TransformDevilTrainingInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform deviltraining info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform deviltraining info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DEVIL_TRAINING_INFO>.Value, uid, stream);
        }

        public void OnResponse_TransformDomainBenedictionInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform domain benediction info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform domain benediction info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DOMAIN_BENEDICTION_INFO>.Value, uid, stream);
        }
        
        public void OnResponse_TransformDriftExploreTaskInfo(MemoryStream stream, int uid = 0)
        {
            Client client = GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} client transform drift explore task info failed: no such player", uid);
                return;
            }
            if (client.DestZone == null)
            {
                Log.Warn("player {0} client transform drift explore task info failed: dest zone null", uid);
                return;
            }
            // 转发给zone2
            client.DestZone.Write(Id<MSG_ZMZ_DRIFT_EXPLORE_TASK_INFO>.Value, uid, stream);
        }
        
        public void OnResponse_BroadCastTitleCheck(MemoryStream stream, int uid)
        {
            Client client = ZoneServerManager.GetClient(uid);
            OfflineClient offLineClient = null;
            ZoneServer targetZone = null;

            if (client == null)
            {
                offLineClient = ZoneServerManager.GetOfflineClient(uid);
                if (offLineClient != null)
                {
                    targetZone = ZoneServerManager.GetZone(offLineClient.MapId, offLineClient.Channel);
                }
            }
            else
            {
                targetZone = ZoneServerManager.GetZone(client.CurrentMap.MapId, client.CurrentMap.Channel);
            }

            if (targetZone != null)
            {
                targetZone.Write(Id<MSG_ZMZ_CHECK_TITLE>.Value, uid, stream);
            }
        }

        public void OnResponse_NewMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_NEW_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_NEW_MAP>(stream);
            Log.Write("zone main {0} sub {1} create new map {2} channel {3}", MainId, SubId, msg.MapId, msg.Channel);
            AddMap(msg.MapId, msg.Channel);

            if (msg.Owner > 0)
            {
                MSG_ZM_NEW_MAP_RESPONSE res = new MSG_ZM_NEW_MAP_RESPONSE();
                res.DungeonId = msg.MapId;
                res.Channel = msg.Channel;
                res.Uid = msg.Owner;
                Write(res);
            }
        }

        public void OnResponse_DeleteMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_DELETE_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_DELETE_MAP>(stream);
            Log.Write("main {0} sub {1} delete map {2} channel {3}", MainId, SubId, msg.MapId, msg.Channel);
            RemoveMap(msg.MapId, msg.Channel);
        }

        public void OnResponse_CloseMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CLOSE_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CLOSE_MAP>(stream);
            Log.Write("main {0} sub {1} close map {2} channel {3}", MainId, SubId, msg.MapId, msg.Channel);
            string key = msg.MapId + "_" + msg.Channel;
            Map map = null;
            allMap.TryGetValue(key, out map);
            if (map == null)
            {
                Log.Warn("main {0} sub {1} close map {2} channel {3} failed: no such map", MainId, SubId, msg.MapId, msg.Channel);
                return;
            }
            map.State = MapState.CLOSE;
        }

        public void OnResponse_OpenMap(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_OPEN_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_OPEN_MAP>(stream);
            Log.Write("main {0} sub {1} open map {2} channel {3}", MainId, SubId, msg.MapId, msg.Channel);
            string key = msg.MapId + "_" + msg.Channel;
            Map map = null;
            allMap.TryGetValue(key, out map);
            if (map == null)
            {
                Log.Warn("main {0} sub {1} open map {2} channel {3} failed: no such map", MainId, SubId, msg.MapId, msg.Channel);
                return;
            }
            if (map.State == MapState.OPEN)
            {
                //Log.Warn("main {0} sub {1} open map {2} channel {3}: already opened", mainId, subId, msg.MapId, msg.Channel);
                return;
            }
            map.State = MapState.OPEN;
            map.ClearClientList();
        }

        public void OnResponse_Regist(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_REGISTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_REGISTER>(stream);
            Log.Write("zone main {0} sub {1} regist to manager {2}", MainId, SubId, Api.MainId);
            // 先init 挂载map
            Regist(msg);

            foreach (var closeMapInfo in msg.CloseMapList)
            {
                Map map = GetMap(closeMapInfo.MapId, closeMapInfo.Channel);
                if (map != null)
                {
                    map.State = MapState.CLOSE;
                }
            }
        }

        public void OnResponse_RegistClientList(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_REGIST_CLIENT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_REGIST_CLIENT_LIST>(stream);
            Log.Info("got zone main {0} sub {1} regist client list count {2}", MainId, SubId, msg.ClientList.Count);
            foreach (var clientInfo in msg.ClientList)
            {
                Map map = GetMap(clientInfo.MapId, clientInfo.Channel);
                if (map != null)
                {
                    Client client = new Client(clientInfo.CharacterUid, this, clientInfo.AccountId);
                    AddClient(client);
                    client.EnterMap(map);
                    map.AddClient(client);
                }
                else
                {
                    Log.Warn("zone main {0} sub {1} add client pc uid {2} falied: map {3} channel {4} not exist ",
                        MainId, SubId, clientInfo.CharacterUid, clientInfo.MapId, clientInfo.Channel);
                }
            }
        }
        public void OnResponse_RegistOfflineClientList(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_REGIST_OFFLINE_CLIENT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_REGIST_OFFLINE_CLIENT_LIST>(stream);
            Log.Info("got zone main {0} sub {1} regist offline client list count {2}", MainId, SubId, msg.OfflineList.Count);
            foreach (var clientInfo in msg.OfflineList)
            {
                OfflineClient client = new OfflineClient(clientInfo.Uid, MainId, SubId, clientInfo.Token, clientInfo.MapId, clientInfo.Channel);
                ZoneServerManager.AddOfflineClient(client);
            }
        }

        public void OnResponse_KickPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_KICK_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_KICK_PLAYER>(stream);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("kick player main {0} uid {1} success", MainId, msg.Uid));
            if (Api.GlobalServer != null)
            {
                Api.GlobalServer.Write(response);
            }
            return;
        }

        public void OnResponse_PullPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_PULL_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_PULL_PLAYER>(stream);
            Log.Write("try pull player {0} sub {1} to map {2} channel {3} sub {4}", msg.Uid, msg.SubId, msg.MapId, msg.Channel, SubId);
            ZoneServer zone = (ZoneServer)ZoneServerManager.GetServer(MainId, msg.SubId);
            if (zone == null)
            {
                Log.Warn("try pull player {0} sub {1} to map {2} channel {3} sub {4}: find player zone failed", msg.Uid, msg.SubId, msg.MapId, msg.Channel, SubId);
                return;
            }
            MSG_MZ_PULL_PLAYER notify = new MSG_MZ_PULL_PLAYER();
            notify.Uid = msg.Uid;
            notify.BeginPosX = msg.BeginPosX;
            notify.BeginPosY = msg.BeginPosY;
            notify.TeamLimit = msg.TeamLimit;
            notify.MapId = msg.MapId;
            notify.Channel = msg.Channel;
            zone.Write(notify);
        }

        public void OnResponse_BroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_BROADCAST_ANNOUNCEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_BROADCAST_ANNOUNCEMENT>(stream);
            MSG_MZ_BROADCAST_ANNOUNCEMENT notify = new MSG_MZ_BROADCAST_ANNOUNCEMENT();
            notify.Type = msg.Type;
            foreach (var item in msg.List)
            {
                notify.List.Add(item);
            }
            ZoneServerManager.Broadcast(notify);
            // 如果抽到橙宠 记录
            //switch ((ANNOUNCEMENT_TYPE)msg.Type)
            //{
            //    case ANNOUNCEMENT_TYPE.OBTAIN_PET_ORANGE:
            //        zoneManager.NextOrangePetAnnouncementTime = Api.now.AddSeconds(zoneManager.GetNextOrangePetAnnouncementSecond());
            //        break;
            //    case ANNOUNCEMENT_TYPE.OPEN_REWARDBOX:
            //        zoneManager.NextRewardBoxAnnouncementTime = Api.now.AddSeconds(zoneManager.GetNextRewardBoxAnnouncementSecond());
            //        break;
            //    case ANNOUNCEMENT_TYPE.TURNTABLE1_REWARD:
            //    case ANNOUNCEMENT_TYPE.TURNTABLE2_REWARD:
            //    case ANNOUNCEMENT_TYPE.TURNTABLE3_REWARD:
            //    case ANNOUNCEMENT_TYPE.TURNTABLE4_REWARD:
            //        zoneManager.NextTurnTableAnnouncementTime = Api.now.AddSeconds(zoneManager.GetNextTurnTableAnnouncementSecond());
            //        break;
            //    default:
            //        break;
            //}
        }

        public void OnResponse_EnterZone(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_ENTER_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_ENTER_ZONE>(stream);
            Client client = new Client(msg.CharUid, this, msg.AccountId);
            AddClient(client);
            ZoneServerManager.RemoveOfflineClient(msg.CharUid);

        }

        public void OnResponse_ClientLogout(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_LOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_LOGOUT>(stream);
            Api.AddictionMng.Logout(msg);

            //下线时更新账号服务器玩家角色信息
            MSG_MB_UPDATE_CHARACTER_INFO notify = new MSG_MB_UPDATE_CHARACTER_INFO
            {
                AccountId = msg.AccountId,
                Channel = msg.Channel,
                Uid = msg.Uid,
                Level = msg.Level,
                HeroId = msg.HeroId,
                GodType = msg.GodType,
                Name = msg.Name,
                SourceMain = msg.SourceMain
            };
            Api.BarrackServerManager.Broadcast(notify);
        }

        public void OnResponse_CatchOfflineClient(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CATCH_OFFLINE_CLIENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CATCH_OFFLINE_CLIENT>(stream);
            Log.Write("zone main {0} sub {1} add offline client {2}", MainId, SubId, msg.Uid);
            OfflineClient offlineClient = new OfflineClient(msg.Uid, msg.MainId, msg.SubId, msg.Token, msg.MapId, msg.Channel);
            ZoneServerManager.AddOfflineClient(offlineClient);
        }

        public void OnResponse_RemoveOfflineClient(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_REMOVE_OFFINE_CLIENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_REMOVE_OFFINE_CLIENT>(stream);
            foreach (var item in msg.UidList)
            {
                // 离线缓存超时 清除
                ZoneServerManager.RemoveOfflineClient(item);
            }
        }

        public void OnResponse_DBException(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_DB_EXCEPTION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_DB_EXCEPTION>(stream);
            Api.GameDBPool.Call(new QueryAlarm((int)AlarmType.DB, MainId, SubId, ManagerServerApi.now.ToString(), msg.Content));
        }

        public void OnResponse_BlackIp(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_BLACK_IP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_BLACK_IP>(stream);
            MSG_MB_BLACK_IP notify = new MSG_MB_BLACK_IP();
            notify.Ip = msg.Ip;
            if (Api.BarrackServerManager != null)
            {
                Api.BarrackServerManager.Broadcast(notify);
            }
        }

        public void OnResponse_DebugRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_DEBUG_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_DEBUG_RECHARGE>(stream);
            //todo debug充值
            //string orderId = "test";
            //string uuid = Guid.NewGuid().ToString("N");
            //RechargeRewardModel recharge = RechargeLibrary.GetNormalRechargeModel(msg.RechargeId);
            //Api.RechargeMng.UpdateRechargeManager(msg.Uid, MainId, msg.RechargeId, orderId + uuid, ManagerServerApi.now, recharge.Money, EnumerateUtility.Recharge.RechargeWay.Server);
            MSG_MZ_RECHARGE_GET_REWARD pks = new MSG_MZ_RECHARGE_GET_REWARD();
            pks.Uid = msg.RechargeId;
            Api.ZoneServerManager.Broadcast(pks);
        }

        public void OnResponse_BuyRechargeGift(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_BUY_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_BUY_RECHARGE_GIFT>(stream);

            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItemOrSdkItem(msg.GiftId);//rechargeitemid
            if (rechargeItem != null)
            {
                RechargePriceModel price = RechargeLibrary.GetRechargePrice(rechargeItem.RechargeId);
                if (price != null)
                {
                    float amount = 0;
                    if (!msg.Discount)
                    {
                        amount = price.Money;
                    }
                    else
                    {
                        RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(rechargeItem.DiscountRechargeId);
                        amount = discountPrice.Money;
                    }
                    int historyId = Api.RechargeMng.GetNewHistoryId();
                    Api.RechargeMng.SaveHistoryId(uid, msg.GiftId, historyId, 0);
                    string orderInfo = $"test_{uid}_{msg.GiftId}";
                    Api.RechargeMng.UpdateRechargeManager(historyId, orderInfo, ManagerServerApi.now, amount, "Zone", RechargeWay.Zone, "1", "0");
                }
            }
        }

        public void OnResponse_GetRechargeId(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_GET_RECHARGE_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_GET_RECHARGE_ID>(stream);
            MSG_MZ_GET_RECHARGE_ID msg = new MSG_MZ_GET_RECHARGE_ID();
            msg.Uid = uid;
            int orderId = Api.RechargeMng.GetNewHistoryId();
            msg.OrderId = orderId;
            msg.GiftId = pks.GiftId;
            Api.RechargeMng.SaveHistoryId(uid, pks.GiftId, orderId, 0);
            Write(msg);
        }

        public void OnResponse_DoRechargeToken(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_RECHARGE_TOKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_RECHARGE_TOKEN>(stream);

            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItemOrSdkItem(msg.GiftId);//rechargeitemid
            if (rechargeItem != null)
            {
                RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(rechargeItem.DiscountRechargeId);
                RechargePriceModel price = RechargeLibrary.GetRechargePrice(rechargeItem.RechargeId);
                float amount = 0;
                if (msg.HasDicount && discountPrice != null)
                {
                    amount = discountPrice.Money;
                }
                else if (price != null)
                {
                    amount = price.Money;
                }
                if (amount > 0)
                {
                    int historyId = Api.RechargeMng.GetNewHistoryId();
                    Api.RechargeMng.SaveHistoryId(uid, msg.GiftId, historyId, 0);
                    string orderInfo = $"rechargeToken_{uid}_{msg.GiftId}";
                    Api.RechargeMng.UpdateRechargeManager(historyId, orderInfo, ManagerServerApi.now, amount, "Token", RechargeWay.Token, "1", "0");
                }            
            }
        }

        public void OnResponse_ActivateItem(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_ACTIVATE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_ACTIVATE_ITEM>(stream);

            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItem(msg.GiftId);//rechargeitemid
            if (rechargeItem != null)
            {
                RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(rechargeItem.DiscountRechargeId);
                RechargePriceModel price = RechargeLibrary.GetRechargePrice(rechargeItem.RechargeId);
                float amount = 0;
                if (msg.HasDicount && discountPrice != null)
                {
                    amount = discountPrice.Money;
                }
                else if (price != null)
                {
                    amount = price.Money;
                }
                if (amount > 0)
                {
                    int historyId = Api.RechargeMng.GetNewHistoryId();
                    Api.RechargeMng.SaveHistoryId(uid, msg.GiftId, historyId, 0);
                    string orderInfo = $"activateItem_{uid}_{msg.GiftId}";
                    Api.RechargeMng.UpdateRechargeManager(historyId, orderInfo, ManagerServerApi.now, amount, "ActivateItem", RechargeWay.ActivateItem, "1", "0");
                }
            }
        }

        public void OnResponse_GetSpecialActivityItems(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_GET_SPECIAL_ACTIVITY_ITEM psk = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_GET_SPECIAL_ACTIVITY_ITEM>(stream);
            Log.Info($"player {uid} GetSpecialActivityItems id {psk.Id} num {psk.Num} time {Api.Now()}");
            QueryGetMyCardItems query = new QueryGetMyCardItems(uid, psk.Id, psk.Num, Timestamp.GetUnixTimeStampSeconds(Api.Now()));
            Api.AccountDBPool.Call(query, ret =>
            {
                MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM msg = new MSG_MZ_GET_SPECIAL_ACTIVITY_ITEM();
                msg.TotalCount = query.TotalCount;
                msg.UseCount = query.UseCount;
                msg.Id = psk.Id;
                msg.Uid = uid;

                foreach (var item in query.Items)
                {
                    msg.Items.Add(item.Key, item.Value);
                }

                ZoneServerManager.Broadcast(msg);
                Log.Info($"player {uid} GetSpecialActivityItems id {psk.Id} num {psk.Num} time {Api.Now()} get item {string.Join("|", query.Items.Keys)}");
            });
        }

        public void OnResponse_UpdateRechargeState(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_UPDATE_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_UPDATE_RECHARGE>(stream);
            if (msg.Way == (int)RechargeWay.VMall)
            {
                //修改状态
                Api.GameDBPool.Call(new QueryUpdateVMallOrder(msg.OrderId, ManagerServerApi.now));
            }
            else
            {
                Api.GameDBPool.Call(new QueryChangeRechargeGetRewardState(msg.OrderId, ManagerServerApi.now));
            }
        }

        public void OnResponse_GetRunAwayType(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_GET_RUNAWA_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_GET_RUNAWA_TYPE>(stream);
            Api.BarrackServerManager.GetOneServer()?.Write(new MSG_MB_GET_RUNAWA_TYPE()
            {
                Uid = msg.Uid, Account = msg.Account, ServerId = msg.ServerId, GameId = msg.GameId
            });
        }

        public void OnResponse_GetGiftType(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_GET_SDK_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_GET_SDK_GIFT>(stream);
            MSG_MB_GET_SDK_GIFT request = new MSG_MB_GET_SDK_GIFT()
            {
                Uid = msg.Uid,
                ActionId = msg.ActionId,
                SdkActionType = msg.SdkActionType,
                Param = msg.Param,
                Account = msg.Account,
                ServerId = MainId
            };
            Api.BarrackServerManager.GetOneServer()?.Write(request);
        }
        


        private void OnResponse_GetSchoolId(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_GET_SCHOOL_ID msg = new MSG_MZ_GET_SCHOOL_ID();
            msg.SchoolId = Api.SchoolManager.GetSchoolId();
            Write(msg, uid);
        }
    }
}

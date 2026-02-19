using Logger;
using ServerShared;
using System;
using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using Message.Gate.Protocol.GateC;
using ServerShared.Map;
using Message.Zone.Protocol.ZGate;
using DBUtility;
using ServerModels.PlayerChar;
using RedisUtility;
using System.Collections.Generic;
using DataProperty;
using ServerModels;
using System.Linq;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {

        #region 属性

        public EnterMapInfo EnterMapInfo = new EnterMapInfo();
        public EnterMapInfo OriginMapInfo = new EnterMapInfo();
        public EnterMapInfo LastMapInfo = new EnterMapInfo();

        public DateTime LoadingStartTime = DateTime.MinValue;

        public bool needKickPlayer = false;
        private bool isTransforming = false;
        /// <summary>
        /// 是否是跨服务器
        /// </summary>
        public bool IsTransforming
        {
            get { return isTransforming; }
        }

        private bool isMapLoadingDone = true;
        /// <summary>
        /// 是否读条完成
        /// </summary>
        public bool IsMapLoadingDone
        {
            get { return isMapLoadingDone; }
        }

        //public DateTime LoadingDonePathFindWaiting = ZoneServerApi.now.AddDays(7);
        public DateTime OnlineRewardTime = ZoneServerApi.now;
        private DateTime lastLoginTime = ZoneServerApi.now;
        public DateTime LastLoginTime
        {
            get { return lastLoginTime; }
            set { lastLoginTime = value; }
        }
       
        private bool isLeavingMap = false;

        public DateTime OfflineTime { get; set; }
        public int OfflineToken { get; set; }
        public int OfflineMapId { get; set; }
        public int OfflineChannel { get; set; }

        public string OfflineIp = String.Empty;

        public bool CanCatchOffline = true;

        #endregion

        /// <summary>
        /// 进入世界
        /// </summary>
        public void LoadingDone(bool offlinePlayer = false)
        {
            DateTime lastOnlineTime = LastOfflineTime;
            DateTime realLastLoginTime = LastLoginTime;

            CheckSignInSpecialActivity();
            // 进入世界
            LastLoginTime = ZoneServerApi.now;
            OnlineRewardTime = ZoneServerApi.now;
            //CalcOfflineReward();
            // 同步服务器时间
            SyncServerTime();

            PassCardMng.InitTasks(true);

            if (!offlinePlayer)
            {
                //加载redis信息
                LoadRedisData();
                // 加载heroMng单次计算内容
                HeroMng.PlayerHeroOnceCalcs();
            }

            LoadHeartFlag();
            LoadCampBattleInfo();

            //检查过期（物品等）
            CheckPastData();
            ////检查宝箱开启条件
            //CheckChestLimitOpen();

            //// 检查赛季结算
            //DelLadderScoreBySeasonEnd();

            //获得离线消息
            GetSaveChatInfo();

            //获取充值成功但是没有获得对应的奖励
            CheckNotReceivedRecharge();

            //检查有没有红点
            CheckNewEmail();

            // 刷新服务器数据
            CheckRefresh();

            //检查气泡框信息
            CheckCurChatFrame();

            // 发送进入世界人物信息
            SendEnterWorldInfo();

            if (!needKickPlayer)
            {
                server.PCManager.AddPc(uid, this, true);

                //检查是否连续登录
                //CheckIsContinuousLogin(lastLoginTime);
                SetOnline(true);
            }

            //获取竞技场每日结算奖励
            GetArenaDailyReward();

            //检查章节红点
            //CheckChapterReward();

            //检查是否竞技场第一名上线
            CheckBroadCastArenaFirstLogin();

            //检查排行榜奖励红点
            CheckNewRankRewards();

            //排行榜奖励页码定位
            NotifyRankRewardPage();

            //周期固活
            GetContributionInfo();

            //重置特殊缓存数据
            ResetSpecialCatchData();

            if (TimeCreated.Date < server.Now().Date)
            { 
                //玩家行为 => 回归
                RecordAction(ActionType.BackMoreThanDayes, lastOnlineTime);
            }

            GetCrossBossInfo();

            GetHidderWeaponInfoByLoading();

            GetGardenByLoading();

            GetDivineLoveInfoByLoading();

            GetIslandHighByLoading();

            SendTridentInfo();

            GetStoneWallInfoByLoading();

            GetRouletteByLoading();

            GetCanoeInfoByLoading();

            GetMidAutumnInfoByLoading();

            GetThemeFireworkByLoading();

            GetNineTestInfoByLoading();

            //发送物品仓库当前页码信息
            SendWarehouseItemsMsg();

            GetXuanBoxInfoByLoading();

            GetWishLanternInfo();

            //网页支付返利
            CheckSendWebPayRechargeRebateInfo(realLastLoginTime);

            SendDaysRechargeInfo();

            SendShreklandInfoByLoading();

            SendDevilTrainingInfoByLoading();

            LoadOrUpdateInfo();
            
            if (needKickPlayer)
            {
                CanCatchOffline = false;
                server.PCManager.DestroyPlayer(this, true);
            }
        }

        /// <summary>
        /// 离开世界
        /// </summary>
        public void LeaveWorld(bool cache=false)
        {
            Log.Debug($"player {Uid} LeaveWorld cache {cache}");
            if (LeavedWorld)
            {
                return;
            }
            // 通知manager client离开zone
            MSG_ZM_CLIENT_LEAVE_ZONE notify = new MSG_ZM_CLIENT_LEAVE_ZONE();
            notify.CharacterUid = Uid;
            server.ManagerServer.Write(notify);

            //三叉戟状态检测，未结算下线直接判失败
            GodPathManager.CheckTridentUse();

            // 如果是下线 而不是跨zone 
            // 通知relation client离开zone
            FieldMap map = CurrentMap;
         
            Team = null;
            //if (IsDead)
            //{
            //    int base_zone_id = 1;
            //    map = server.MapManager.GetFieldMap(base_zone_id, 1);
            //    if (map != null)
            //    {
            //        //Packet_Info.zoneID = base_zone_id;
            //        //Packet_Info.posX = map.Begin_Position.x;
            //        //Packet_Info.posY = map.Begin_Position.y;
            //        //Packet_Info.channel = map.Channel;
            //    }
            //}

            //pc.SaveLastRefreshTime();

            if (!IsTransforming)
            {
                try
                {
                    MSG_ZR_CLIENT_LEAVE_ZONE notifyRelation = new MSG_ZR_CLIENT_LEAVE_ZONE();
                    notifyRelation.CharacterUid = Uid;
                    server.SendToRelation(notifyRelation);
                    //更改在线状态为下线
                    SetOnline(false);
                    //AddTotalOnlineTime();

                    //LastOfflineTime = Api.now;

                    PlayerCharInfo pcInfo = new PlayerCharInfo();
                    if (currentMap != null)
                    {
                        if (currentMap.IsDungeon)
                        {
                            MapModel originMapModel = MapLibrary.GetMap(OriginMapInfo.MapId);
                            if (originMapModel != null && originMapModel.MapType == MapType.Map)
                            {
                                pcInfo.MapId = OriginMapInfo.MapId;
                                pcInfo.Channel = OriginMapInfo.Channel;
                                pcInfo.PosX = OriginMapInfo.Position.x;
                                pcInfo.PosY = OriginMapInfo.Position.y;
                            }
                            else
                            {
                                pcInfo.MapId = CONST.MAIN_MAP_ID;
                                pcInfo.Channel = CONST.MAIN_MAP_CHANNEL;
                                MapModel mapModel = MapLibrary.GetMap(pcInfo.MapId);
                                pcInfo.PosX = mapModel.BeginPosX;
                                pcInfo.PosY = mapModel.BeginPosY;
                            }
                        }
                        else
                        {
                            pcInfo.MapId = currentMap.MapId;
                            pcInfo.Channel = currentMap.Channel;
                            pcInfo.PosX = Position.x;
                            pcInfo.PosY = Position.y;
                        }
                    }
                    else
                    {
                        pcInfo.MapId = CONST.MAIN_MAP_ID;
                        pcInfo.Channel = CONST.MAIN_MAP_CHANNEL;
                        MapModel mapModel = MapLibrary.GetMap(pcInfo.MapId);
                        pcInfo.PosX = mapModel.BeginPosX;
                        pcInfo.PosY = mapModel.BeginPosY;
                    }
                    Log.Write($"player {Uid} LeaveWorld OriginMap {OriginMapInfo?.MapId}  CurrentMap  {currentMap?.MapId} save  {pcInfo?.MapId} ");
                    pcInfo.Uid = Uid;
                    pcInfo.LastLoginTime = LastLoginTime;
                    pcInfo.LastLevelUpTime = LastLevelUpTime;
                    //pcInfo.LastPhyRecoveryTime = LastPhyRecoveryTime;
                    CumulateOnlineTime += (int)(server.Now() - lastLoginTime).TotalSeconds;
                    pcInfo.CumulateDays = CumulateDays;
                    pcInfo.CumulateOnlineTime = CumulateOnlineTime;

                    HeroInfo heroInfo = HeroMng.GetHeroInfo(HeroId);
                    if (heroInfo != null)
                    {
                        //pcInfo.HeroInfo = string.Format("{0}|{1}|{2}|{3}", heroInfo.Id, heroInfo.Level, heroInfo.TitleLevel, heroInfo.AwakenLevel);
                        pcInfo.HeroInfo = heroInfo.Id.ToString();
                    }
                    else
                    {
                        int heroId = HeroMng.GetAllHeroPosHeroId().First();
                        heroInfo = HeroMng.GetHeroInfo(heroId);
                        if (heroInfo != null)
                        {
                            //pcInfo.HeroInfo = string.Format("{0}|{1}|{2}|{3}", heroInfo.Id, heroInfo.Level, heroInfo.TitleLevel, heroInfo.AwakenLevel);
                            pcInfo.HeroInfo = heroInfo.Id.ToString();
                        }
                        else
                        {
                            Log.Warn("player {0} LeaveWorld add GetEquipHeroId {1} : not find info", Uid, heroId);
                        }
                    }
                    LastOfflineTime = ZoneServerApi.now;

                    server.GameDBPool.Call(new QuerySetPlayerCharInfo(pcInfo, LastRefreshTime, LastOfflineTime), DBIndex);
                    SyncDbDelayCurrencies(true);
                    SyncDbDelayCounters(true);
                    SyncDbPlayerBattlePower();

                    DelaySyncDbWarehouseCurrencies(true);
                    //ChapterManager.SyncDBPowerRecoryTime();

                    UpdateFortDefensiveQueue();

                    //离开世界的时候自动选择一个buff
                    TowerSelectBuffAuto();

                    //离开世界，跨服挑战后续没打的都算失败
                    CrossChallengePrimarySetUnFightFail();

                    //通知下线
                    UpdateAccountLoginServers();

                    OfflineTime = ZoneServerApi.now;
                    if (OfflineTime > OnlineRewardTime)
                    {
                        int passTime = (int)(OfflineTime - OnlineRewardTime).TotalSeconds;
                        AddActivityNumForType(EnumerateUtility.Activity.ActivityAction.OnlineReward, passTime);
                        AddActivityNumForType(EnumerateUtility.Activity.ActivityAction.OnlineRewardOnce, passTime);
                        Log.Info("AddActivityNumForType add time {0}", passTime);
                  
                    }

                    AddRunawayActivityNumForType(RunawayAction.OnlinTime, (int)(ZoneServerApi.now - LastLoginTime).TotalMinutes);

                    AddRunawayActivityNumForType(RunawayAction.SignIn);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }

            }

            if(currentMap != null)
            {
                currentMap.OnPlayerLeave(this,cache);
            }

            server.PCManager.RemovePc(this);
            if (map != null)
            {
                OfflineMapId = map.MapId;
                OfflineChannel = map.Channel;
            }
            if (GameConfig.CatchOfflinePlayer && !IsTransforming && CanCatchOffline == true && map!=null)
            {
                MSG_ZM_CATCH_OFFLINE_CLIENT offlineNotify = new MSG_ZM_CATCH_OFFLINE_CLIENT();
                offlineNotify.Uid = Uid;
                offlineNotify.SubId = server.SubId;
                offlineNotify.Token = OfflineToken;
                offlineNotify.MapId = map.MapId;
                offlineNotify.Channel = map.Channel;
                offlineNotify.MainId = server.MainId;
                server.ManagerServer.Write(offlineNotify);

                try
                {
                    server.PCManager.PcOfflineList.Add(Uid, this);
                
                    //OfflineIp = ClientTcp.IP;
                }
                catch (Exception e)
                {
                    Log.Warn("player {0} LeaveWorld add offline error : {1}", Uid, e);
                }
            }
        }

        //更新account db中login信息
        private void UpdateAccountLoginServers()
        {
            //通知下线
            MSG_ZM_LOGOUT msg = new MSG_ZM_LOGOUT()
            {
                AccountId = AccountName,
                Channel = ChannelName,
                Uid = uid,
                Level = Level,
                HeroId = HeroId,
                GodType = GodType,
                SourceMain = SourceMain,
                Name =Name,
            };
            server.ManagerServer.Write(msg);
        }

        /// <summary>
        /// 进入地图
        /// </summary>
        public void OnMoveMap(bool fromOffline=false)
        {
            //当前zone被禁止
            if (ServerFrame.ZoneTransformManager.Instance.IsForbided(server.SubId))
            {
                AskManagerForMap(EnterMapInfo.MapId, EnterMapInfo.Channel, EnterMapInfo.Position);
                currentMap?.OnPlayerLeave(this);
                return;
            }

            FieldMap destMap = server.MapManager.GetFieldMap(EnterMapInfo.MapId, EnterMapInfo.Channel);
            if (destMap == null)
            {
                return;
            }
            if (destMap is DungeonMap)
            {
                //FIXME:由于目前没有真正的pvp，所以再这里把玩家都设置为攻击方。真正pvp这里慎用
                IsAttacker = true;
            }

            if (fromOffline && destMap == currentMap && destMap.IsDungeon)
            {
                destMap.OnPlayerEnter(this, true);
                return;
            }

            currentMap?.OnPlayerLeave(this);
            destMap.OnPlayerEnter(this);
        }

        public void LeaveMap(bool cache=false)
        {
            if(currentMap == null)
            {
                return;
            }
            if (!cache)
            {
                isLeavingMap = true;

                // 删除pc
                MoveHandler.MoveStop();

                RemoveFromAoi();

                RecordLastMapInfo(currentMap.MapId, currentMap.Channel, Position);

                SetCurrentMap(null);

                SetIsMapLoadingDone(false);
            }
            else
            {
                RecordLastMapInfo(currentMap.MapId, currentMap.Channel, Position);
            }
        }

        public void EnterMap(FieldMap map)
        {
            SetCurrentMap(map);

            isLeavingMap = false;

            MoveHandler.MoveStop();

            FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true);

            SetPosition(EnterMapInfo.Position);

            GenAngle = NextAngle;
        }

        /// <summary>
        /// 传送门
        /// </summary>
        /// <param name="zoneNpcId"></param>
        public void CrossPortal(int zoneNpcId)
        {
            if (IsDead)
            {
                Log.Warn("player {0} cross portal {1} fail:is dead", Uid, zoneNpcId);
                return;
            }
            Data data = DataListManager.inst.GetData("ZoneNPC", zoneNpcId);
            if (data == null)
            {
                Log.Warn("player {0} cross portal {1} not exist ", Uid, zoneNpcId);
                return;
            }
            Log.Debug("CrossPortal npc name {0} curmap {1}", zoneNpcId, CurrentMap.MapId);
            NPC portal = CurrentMap.GetNpcById(zoneNpcId);
            if (portal != null)
            {
                if (!IsStateIdle)
                {
                    FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                }
                portal.MoveMap(this);
            }
            else
            {
                Log.Debug("CrossPortal portal = null error :npc name {0} curmap {1}", zoneNpcId, CurrentMap.MapId);
            }
        } 

        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="findId"></param>
        /// <param name="type"></param>
        /// <param name="end"></param>
        public void AutoPathFinding(int findId, int type, Vec2 end = null)
        {
            if ( InDungeon && IsDead)
            {
                Log.Warn("player {0} AutoPathFinding get {1} Data name {2} is dead", Uid, type, findId);
                return;
            }
            if (CurrentMap == null)
            {
                Log.Warn("player {0} AutoPathFinding get {1} Data name {2} error not find cur map ", Uid, type, findId);
                return;
            }
            if (!CanMove())
            {
                return;
            }
            FindPathType findType = (FindPathType)type;

            //获取Xml数据
            Data targetData = null;
            switch (findType)
            {
                case FindPathType.TaskNPC:
                case FindPathType.Npc:
                    targetData = DataListManager.inst.GetData("ZoneNPC", findId);
                    break;
                case FindPathType.PropBook:
                    targetData = DataListManager.inst.GetData("ZoneProp", findId);
                    break;
                case FindPathType.Goods:
                    targetData = DataListManager.inst.GetData("ZoneGoods", findId);
                    break;
                case FindPathType.Treasure:
                    targetData = DataListManager.inst.GetData("ZoneShovelTreasure", findId);
                    break;
                default:
                    Log.Debug("player {0} AutoPathFinding type {0} not find", Uid, findType.ToString());
                    break;
            }
            if (targetData == null)
            {
                Log.Warn("player {0} AutoPathFinding get {1} Data error not find name {2}", Uid, findType, findId);
                return;
            }
            //目标所在地图名
            int zoneId = targetData.GetInt("ZoneId");
            //目标坐标
            Vec2 targetPosition = new Vec2();
            targetPosition.x = targetData.GetFloat("PosX");
            targetPosition.y = targetData.GetFloat("PosZ");
            //获取目标地图ID
            MapModel mapModel = MapLibrary.GetMap(zoneId);
            if (mapModel == null)
            {
                Log.Warn("player {0} AutoPathFinding get {1} Data error not find zone id {2}", Uid, findType, zoneId);
                return;
            }
            //判断是否在同一地图
            int targetMapId = mapModel.MapId;
            if (CurrentMapId != targetMapId)
            {
                Dictionary<int, int> alreadyfind = new Dictionary<int, int>();
                //不在同一地图，找到过度地图
                int tempMapId = AutoFindMap(targetMapId, alreadyfind);
                if (tempMapId == -1)
                {
                    Log.Warn("player {0} AutoPathFinding get {1} Data name {2} error not find can rearch map {3} ", Uid, findType, findId, targetMapId);
                    return;
                }
                //找到传送点去相邻地图
                NPC tempMapNpc = CurrentMap.GetNpcByCanReachMapId(tempMapId);
                if (tempMapNpc != null)
                {
                    SetDestination(tempMapNpc.Position);
                    FsmManager.SetNextFsmStateType(FsmStateType.CHASE, true, tempMapNpc);
                }
                else
                {
                    Log.Warn("player {0} AutoPathFinding get npc {1} Data name {2} error not find in map {3} ", Uid, findType, findId, CurrentMap.MapId);
                    return;
                }
            }
            else
            {
                //在同一地图
                switch (findType)
                {
                    case FindPathType.TaskNPC:
                    case FindPathType.Npc:
                        {
                            NPC targetnpc = CurrentMap.GetNpcById(findId);
                            if (targetnpc != null)
                            {
                                if (InFly())
                                {
                                    SetDestination(end);
                                }
                                else
                                {
                                    SetDestination(targetnpc.Position);
                                }
                                FsmManager.SetNextFsmStateType(FsmStateType.CHASE, true, targetnpc);
                            }
                            else
                            {
                                Log.Warn("player {0} AutoPathFinding get npc {1} Data name {2} error not find in map {3} ", Uid, findType, findId, CurrentMap.MapId);
                                return;
                            }
                        }
                        break;
                    case FindPathType.Goods:
                        {
                            Goods targetGoods = CurrentMap.GetGoodsById(findId);
                            if (targetGoods != null)
                            {
                                SetDestination(targetGoods.Position);
                                FsmManager.SetNextFsmStateType(FsmStateType.CHASE, true, targetGoods);
                            }
                            else
                            {
                                Log.Warn("player {0} AutoPathFinding get goods {1} Data name {2} error not find in map {3} ", Uid, findType, findId, CurrentMap.MapId);
                                return;
                            }
                        }
                        break;
                    case FindPathType.Monster:
                        {
                            this.SetDestination(new Vec2(targetPosition.x, targetPosition.y));
                            this.FsmManager.SetNextFsmStateType(FsmStateType.RUN);
                        }
                        break;
                    case FindPathType.PropBook:
                        {
                            PropBook targetProp= CurrentMap.GetPropById(findId);
                            SetDestination(targetPosition);
                            this.FsmManager.SetNextFsmStateType(FsmStateType.CHASE, true, targetProp);
                        }
                        break;
                    case FindPathType.Treasure:
                        {
                            Treasure targetTreasure = CurrentMap.GetTreasureById(findId);
                            if (targetTreasure != null)
                            {
                                if (InTreasureFly())
                                {
                                    SetDestination(end);
                                }
                                else
                                {
                                    SetDestination(targetTreasure.Position);
                                }
                                FsmManager.SetNextFsmStateType(FsmStateType.CHASE, true, targetTreasure);
                            }
                            else
                            {
                                Log.Warn("player {0} AutoPathFinding get npc {1} Data name {2} error not find in map {3} ", Uid, findType, findId, CurrentMap.MapId);
                                return;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        int AutoFindMap(int findMapId, Dictionary<int, int> alreadyfind)
        {
            //添加地图
            alreadyfind.Add(findMapId, findMapId);

            Dictionary<int, int> neighborMapIds = server.MapManager.GetMapNeighbors(findMapId);
            if (neighborMapIds != null)
            {
                //有这个地图，且获取到这个地图相邻关系地图
                if (neighborMapIds.ContainsKey(CurrentMap.MapId))
                {
                    //相邻地图中包含玩家所在地图，直接单返回查找的地图，说明两个地图相邻
                    return findMapId;
                }
                else
                {
                    //地图不相邻，遍历列表用递归查找有相邻地图
                    foreach (var neighborMapId in neighborMapIds)
                    {
                        if (!alreadyfind.ContainsKey(neighborMapId.Key))
                        {
                            int continuefindId = AutoFindMap(neighborMapId.Key, alreadyfind);
                            if (continuefindId > 0)
                            {
                                //找到地图
                                return continuefindId;
                            }
                        }
                    }
                }
            }
            //没找到
            return -1;
        }


        /// <summary>
        /// 到manager请求进入地图
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="channel"></param>
        /// <param name="beginPosition"></param>
        /// <param name="forceEnter"></param>
        public void AskForEnterMap(int mapId, int channel, Vec2 beginPosition, bool forceEnter = false,bool needAnim=false)
        {
            if (IsTransforming == true)
            {
                return;
            }

            MapModel model = MapLibrary.GetMap(mapId);
            if (model == null || mapId == 0 || channel == 0 || beginPosition == null)
            {
                Log.Warn("player {0} request enter map {1} channel {2}", uid, mapId, channel);
                // 回主城
                BackToMainCity();
                return;
            }
            // 请求进入副本，不允许拉入别的线
            if(model.IsDungeon())
            {
                forceEnter = true;
            }
            // 对于非强制进入某个指定channel的map，尝试在当前zone将player拉入一个合适的map
            if(forceEnter)
            {
                if(server.MapManager.CanEnterMap(mapId, channel))
                {
                    RecordEnterMapInfo(mapId, channel, beginPosition);
                    OnMoveMap();
                    return;
                }
            }
            else
            {
                int tempChannel = channel;
                if(server.MapManager.TryAdjustChannel(mapId, out tempChannel))
                {
                    RecordEnterMapInfo(mapId, tempChannel, beginPosition);
                    OnMoveMap();
                    return;
                }
            }

            //if (CurrentMap != null && CurrentMap.MapId == mapId && CurrentMap.Channel == channel)
            //{
            //    Log.Warn("player {0} request enter current map {1} channel {2}", Uid, mapId, channel);
            //    //if (CurrentMap.DungeonData != null)
            //    //{
            //        // 重复进入副本 则拉回主城
            //        // AskForEnterMap(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, pcid, null);
            //    //}
            //    return;
            //}

            // 当前zone无法直接分配合适地图并拉入，需要请求manager做均衡负载
            AskManagerForMap(mapId, channel, beginPosition, forceEnter, needAnim);
        }

        public void AskManagerForMap(int mapId, int channel, Vec2 beginPosition, bool forceEnter = false, bool needAnim = false)
        {
            MSG_ZM_ASK_FOR_MAP msg = new MSG_ZM_ASK_FOR_MAP();
            msg.Uid = uid;
            msg.DestMapId = mapId;
            msg.DestChannel = channel;
            msg.DestPosX = beginPosition.X;
            msg.DestPosY = beginPosition.Y;

            msg.OriginMapId = CurrentMapId;
            msg.OriginChannel = CurrentChannel;
            msg.OriginPosX = Position.X;
            msg.OriginPosY = Position.Y;
            msg.ForceEnter = forceEnter;

            msg.NeedAnim = needAnim;

            server.ManagerServer.Write(msg);

            SetIsTransforming(true);
        }

        public void BackToOriginMap()
        {
            if(OriginMapInfo != null)
            {
                AskForEnterMap(OriginMapInfo.MapId, OriginMapInfo.Channel, OriginMapInfo.Position);
            }
            else
            {
                BackToMainCity();
            }
        }
        public void BackToMainCity()
        {
            Vec2 beginPos = server.MapManager.GetBeginPosition(CONST.MAIN_MAP_ID);
            AskForEnterMap(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, beginPos);
        }

        public void SetIsTransforming(bool value)
        {
            isTransforming = value;
        }

        public void SetIsMapLoadingDone(bool value)
        {
            isMapLoadingDone = value;
        }

        /// <summary>
        /// 部分数据从redis加载
        /// </summary>
        public void LoadRedisData()
        {
            //Log.Warn("play {0} load redis data", Uid);
            //LoadSpaceInfo();
            LoadCampInfos();
            LoadBlackList();
            LoadFriendList();
            LoadAndSendFriendInviteList();
            //LoadFriendlyHeartInfo();
            LoadBrotherInviteList();
            ////初始化数据统计
            //InitStatDataManager();
        }

        public bool NeedSyncEnterWorld = true;
        /// <summary>
        /// 发送进入世界人物信息
        /// </summary>
        public void SendEnterWorldInfo()
        {

            if (!NeedSyncEnterWorld)
            {
                //断线重连时候不需要再发一次进入世界
                return;
            }

            //GM信息
            SendGMMsg();

            //背包信息
            SyncClientBagInfo();

            //进入世界信息
            SendEnterWorldMsg();

            //英雄信息
            SendHeroListMessage();

            //整点boss信息
            SendIntegralBossMsg();

            //充值信息
            SendRechargeManger();

            //竞技场信息
            SenArenaManagerMessage();

            //抽卡羁绊信息
            SenDrawManagerMessage();

            //通行证
            SendPassCardMsg();

            //跨服战
            SendCrossBattleManagerMessage();

            //许愿池
            SendWishPoolInfo();

            ////阵营建设
            SendCampBuildInfo();

            //挂机信息
            SendOnhookInfo();

            //委派事件
            SendDelegationInfo();

            //推图
            SendPushFigurateInfo();

            //
            SendTowerTime();

            //武魂共鳴列表
            SendWuhunResonanceGridInfo();

            //猎杀魂兽
            SendHuntingInfo();

            //膜拜展示
            SendWorshipShow();

            //金兰
            SendBrotherInfo();

            //好友
            SendFriendInfo();

            //成神
            SendHeroGodInfo();

            //礼包
            SendGiftInfo();

            //主题通行证
            CheckUpdateThemePass();

            //主题Boss
            CheckOpenNewThemeBoss();

            //称号
            SendTitleInfoWhenLoading();

            //充值返利
            SendRechargeRebateInfo();

            //日周任务完成信息
            SendTaskFinishState();

            //乾坤问情
            SendDivineLoveInfo();

            //端午活动
            SendDragonBoatInfo();

            //昊天石壁
            SendStoneWallInfo();

            //嘉年华Boss
            SendCarnivalBossInfoByLoading();

            //海岛挑战
            SendIslandChallengeTime();

            //嘉年华
            SendCarnivalManagerInfo();

            //漫游记
            SendTravelManager();

            //史莱克邀约
            SendShrekInvitationInfoByLoading();

            //主战多阵容信息
            SendMainBattleQueueInfoByLoading();

            //跨服挑战
            SendCrossChallengeManagerMessage();

            //Mycard充值返利活动
            CheckSpecialActivity();

            CheckRunawayActivity();

            //钻石返利
            CheckSendDiamondRebateInfo();

            //凶兽入侵
            SendHuntingIntrudeInfo();

            //学院信息
            SendClientSchoolInfo();

            //学院任务信息
            SendSchoolTaskInfo();

            //答题信息
            SendAnswerQuestionInfo();

            //宠物（魂兽）
            SendPetMsg();

            //时空塔
            SendSpaceTimeTowerMsg();

            //漂流探宝信息
            SendDriftExploreInfo();
        }

        private void SendGMMsg()
        {
            if (IsGm > 0)
            {
                MSG_ZGate_GM gmMesg = new MSG_ZGate_GM();
                gmMesg.IsGm = IsGm;
                Write(gmMesg);
            }
        }

        private void SendEnterWorldMsg()
        {
            // 获取人物基本信息
            MSG_GC_ENTER_WORLD msg = new MSG_GC_ENTER_WORLD();
            //角色信息
            msg.MyselfInfo = PlayerInfo.GetCharacterInfoMsg(this);

            if (BattlePower > 0 && msg.MyselfInfo.BattlePower != BattlePower)
            {
                Log.ErrorLine($"player {Uid} SendEnterWorldMsg error: battle power is {msg.MyselfInfo.BattlePower} not {BattlePower}");
                foreach (var id in HeroMng.GetHeroPos())
                {
                    HeroInfo info = HeroMng.GetHeroInfo(id.Key);
                    if (info != null)
                    {
                        HeroMng.InitHeroNatureInfoForLog(info);
                        int battlePower = info.GetBattlePower();
                        Log.ErrorLine($"player {Uid} SendEnterWorldMsg: hero {info.Id} power is {battlePower}");

                    }
                }

                //if (BattlePower - msg.MyselfInfo.BattlePower > 1000)
                //{
                //    //玩家踢掉
                //    needKickPlayer = true;
                //}
            }
            else
            {
                Log.Info($"player {Uid} SendEnterWorldMsg: battle power is {msg.MyselfInfo.BattlePower} and {BattlePower}");
            }
            BattlePower = 0;
            //任务列表
            msg.TaskList.AddRange(GetTaskListMessage());
            //货币
            msg.Currencies = GetCurrenciesMsg();
            //商店
            //msg.ShopList.AddRange(GetShopListMsg());
            //称号
            msg.TitleInfo = GetTitleInfo();
            OfflineToken = ((int)ZoneServerApi.now.Millisecond + RAND.Range(1, 1000)).GetHashCode();
            msg.Token = OfflineToken;
            //活动
            msg.ActivityList.AddRange(GetActivityListMessage());
            msg.ActivityTypeCompleted.AddRange(GetActivityTypeComplete());
            msg.WelfareTriggerList.AddRange(GetWelfareTriggerListMessage());
            //msg.LastPhyRecoveryTime = LastPhyRecoveryTime.ToString();
            //计数器
            msg.Counter = GetCounterMsg();
            //阵营
            msg.CampInfo = GetCampMsg();
            //服务器等级、开启天数
            //msg.ServerLevel = server.WorldLevelManager.ServerLevel;
            //msg.ServerOpenDays = server.WorldLevelManager.CurrLevelDays;
            msg.OpenServerDate = Timestamp.GetUnixTimeStampSeconds(server.OpenServerDate);

            //仓库货币
            msg.WarehouseCurrencies = GetWarehouseCurrenciesMsg();

            Write(msg);
        }

        /// <summary>
        /// 同步服务器时间
        /// </summary>
        public void SyncServerTime()
        {
            MSG_GC_TIME_SYNC msg = new MSG_GC_TIME_SYNC();
            msg.TimeStamp = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            Write(msg);
        }

        public void NotifyGateKickPlayer()
        {
            MSG_ZGate_LeaveWorld notify = new MSG_ZGate_LeaveWorld();
            Write(notify);
        }

        public bool NeedSyncAoiInfo()
        {
            return !isLeavingMap;
        }

        public bool NotStableInMap()
        {
            return (currentMap == null || isTransforming || isLeavingMap);
        }

        private void ResetSpecialCatchData()
        {
            //连续狩猎状态重置
            HuntingManager.ReSetContinueState();
        }

        //private void CheckIsContinuousLogin(DateTime lastLoginTime)
        //{
        //    if ((server.Now().Date - lastLoginTime.Date).Days == 1)
        //    {
        //        TitleMng.UpdateTitleConditionCount(TitleObtainCondition.ContinuousLoginDays);
        //    }
        //}
    }
}

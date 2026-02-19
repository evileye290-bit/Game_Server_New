using System.Collections.Generic;
using Logger;
using DataProperty;
using System;
using Message.Relation.Protocol.RZ;
using Message.Manager.Protocol.MZ;
using ServerShared;
using EnumerateUtility.Timing;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using EnumerateUtility;
using System.Linq;
using CommonUtility;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
	class PcManager
	{
        ZoneServerApi server;

        private Dictionary<int, PlayerChar> pcList = new Dictionary<int, PlayerChar>();
        /// <summary>
        /// 玩家列表
        /// </summary>
        public Dictionary<int, PlayerChar> PcList
        {
            get { return pcList; }
        }

        private Dictionary<int, PlayerChar> pcOfflineList = new Dictionary<int, PlayerChar>();
        /// <summary>
        /// 缓存数据列表
        /// </summary>
        public Dictionary<int, PlayerChar> PcOfflineList
        {
            get { return pcOfflineList; }
        }


        private Dictionary<int, PlayerChar> loadingList = new Dictionary<int, PlayerChar>();
        /// <summary>
        /// 正在从db加载数据的uid 列表
        /// </summary>
        public Dictionary<int, PlayerChar> LoadingList
        {
            get { return loadingList; }
        }
   
        private Dictionary<int, PlayerChar> loadingDoneList = new Dictionary<int, PlayerChar>();
        /// <summary>
        /// 已经从db加载完毕，准备进入世界的player
        /// </summary>
        public Dictionary<int, PlayerChar> LoadingDoneList
        {
            get { return loadingDoneList; }
        }

        // 跨zone中等待进入的player列表
        private Dictionary<int, PlayerEnter> playerEnterList = new Dictionary<int, PlayerEnter>();
        public Dictionary<int, PlayerEnter> PlayerEnterList
        {
            get { return playerEnterList; }
        }
        List<PlayerEnter> removeEnterList = new List<PlayerEnter>();
        List<int> removeOfflineList = new List<int>();

        /// <summary>
        /// 上次刷新时间
        /// </summary>
        //private DateTime LastRefreshTime { get; set; }
        int nLastMin { get; set; }
        //private TimeSpan offlineTime = TimeSpan.Parse("23:59:55");
        //private TimeSpan logindTime = TimeSpan.Parse("00:00:05");
        public void Init(ZoneServerApi server)
		{
            Destroy();

            this.server = server;

            //LastRefreshTime = ZoneServerApi.now;

            InitTimerManager(ZoneServerApi.now);

            InitRechargeTimerManager(ZoneServerApi.now, 0);
        }


        public void Update(double dt)
        {
            // 处理进入世界请求
            UpdateLoadingDone();
            // 处理离线缓存
            UpdatePcOffline();
            // 检查跨zone是否有过期未进入的playerEnter
            UpdatePlayerEnter();
            // 每日活动刷新
            UpdateDalyRefresh(dt);
        }


        public void Destroy()
		{
            pcList.Clear();
            pcOfflineList.Clear();
            loadingList.Clear();
            loadingDoneList.Clear();
            playerEnterList.Clear();
            removeEnterList.Clear();
        }

        /// <summary>
        /// 查找玩家
        /// </summary>
        /// <param name="character_uid"></param>
        /// <returns></returns>
        public PlayerChar FindPc(int character_uid)
        {
            PlayerChar pc = null;
            pcList.TryGetValue(character_uid, out pc);
            return pc;
        }

        /// <summary>
        /// 检查玩家是否在线
        /// </summary>
        /// <param name="character_uid"></param>
        /// <returns></returns>
        public bool CheckPcOnline(int character_uid)
        {
            return pcList.ContainsKey(character_uid);
        }

        /// <summary>
        /// 查找离线缓存玩家
        /// </summary>
        /// <param name="character_uid"></param>
        /// <returns></returns>
        public PlayerChar FindOfflinePc(int character_uid)
        {
            PlayerChar pc = null;
            PcOfflineList.TryGetValue(character_uid, out pc);
            return pc;
        }

        /// <summary>
        /// 查找玩家在线或者离线缓存
        /// </summary>
        /// <param name="character_uid"></param>
        /// <returns></returns>
        public PlayerChar FindPcAnyway(int character_uid)
        {
            PlayerChar pc = null;
            if (pcList.TryGetValue(character_uid, out pc))
            {
                return pc;
            }
            else
            {
                pcOfflineList.TryGetValue(character_uid, out pc);
            }
            return pc;
        }


        public void AddPc(int uid, PlayerChar pc, bool online = false)
        {
            if (pc == null) return;
            pc.LeavedWorld = false;
            PlayerChar player = null;
            if (pcList.TryGetValue(uid, out player) )
            {
                Log.Warn("PcManager add player {0} failed: already in pc list", pc.Uid);
                return;
            }

            //redis 
            //pc.SetFristEnterTime();

            MSG_ZM_ENTER_ZONE enterZone = new MSG_ZM_ENTER_ZONE();
            enterZone.CharUid = pc.Uid;
            //enterZone.camp = (int)pc.Camp;
            enterZone.AccountId = pc.AccountName+"$"+pc.ChannelName;
            pc.server.ManagerServer.Write(enterZone);

            pcList.Add(uid, pc);
            MSG_ZR_CLIENT_ENTER notify = new MSG_ZR_CLIENT_ENTER();
            notify.CharacterUid = pc.Uid;
            notify.Online = online;
            notify.LastLoginTime = pc.LastLoginTime.ToString();
            notify.Level = pc.Level;
            notify.Camp = (int)pc.Camp;
            notify.Research = pc.HuntingManager.Research;
            notify.ChapterId = pc.ChapterId;

            server.SendToRelation(notify);
        }

       
        public void RemovePc(PlayerChar player)
        {
            if (player == null)
            {
                return;
            }
            player.LeavedWorld = true;
            player.SetIsTransforming(false);
            pcList.Remove(player.Uid);
        }

        public void RemoveOfflinePc(int uid)
        {
            pcOfflineList.Remove(uid);
        }

        public void DestroyPlayer(PlayerChar player, bool notify_gate)
        {
            if (player == null)
            {
                return;
            }
            player.LeavedWorld = false;
            player.LeaveWorld();
            if (notify_gate)
            {
                player.NotifyGateKickPlayer();
            }
        }


        private void UpdateDalyRefresh(double dt)
        {
            if (nLastMin == ZoneServerApi.now.Minute)
            {
                //updatePerMin += dt;
            }
            else
            {
                if (ZoneServerApi.now.Minute == 59)
                {
                    if (ZoneServerApi.now.Hour == 23)
                    {
                        foreach (var pc in PcList)
                        {
                            pc.Value.RecordLogoutLog(pc.Value.ClientIp, pc.Value.DeviceId);
                            pc.Value.KomoeEventLogPlayerLogout();
                            pc.Value.KomoeEventLogUserSnapshot();
                            pc.Value.AddRunawayActivityNumForType(RunawayAction.OnlinTime, (int)(ZoneServerApi.now - pc.Value.LastLoginTime).TotalMinutes);
                        }
                    }
                }

                if (ZoneServerApi.now.Minute == 0)
                {
                    if (ZoneServerApi.now.Hour == 0)
                    {
                        foreach (var pc in PcList)
                        {
                            pc.Value.RecordLoginLog(pc.Value.ClientIp, pc.Value.DeviceId);
                            pc.Value.KomoeEventLogPlayerLogin();
                            //pc.Value.KomoeEventLogUserSnapshot();
                            pc.Value.CheckSendWebPayRechargeRebateInfo(pc.Value.LastLoginTime, true);
                            pc.Value.LastLoginTime = ZoneServerApi.now;
                        }
                    }
                }

                nLastMin = ZoneServerApi.now.Minute;
            }
            //    //控制检查刷新帧数
            //    if (CheckNeedRefresh(dt))
            //{
            //    ////获取刷新任务
            //    //List<TimingType> tasks = TimingLibrary.GetTimingListToRefresh(LastRefreshTime, ZoneServerApi.now);
            //    //if (tasks.Count > 0)
            //    //{
            //    //    TimingRefreshByPlayers(tasks);
            //    //}
            //    if (LastRefreshTime.TimeOfDay <= server.Now().TimeOfDay && server.Now().TimeOfDay <= offlineTime)
            //    {
            //        foreach (var pc in PcList)
            //        {
            //            pc.Value.RecordLogoutLog(pc.Value.ClientIp, pc.Value.DeviceId);
            //            pc.Value.BIRecordLogoutLog();
            //        }
            //    }
            //    if (LastRefreshTime.TimeOfDay <= server.Now().TimeOfDay && server.Now().TimeOfDay <= logindTime)
            //    {
            //        foreach (var pc in PcList)
            //        {
            //            pc.Value.RecordLoginLog(pc.Value.ClientIp, pc.Value.DeviceId);
            //            pc.Value.BIRecordLoginLog();
            //        }
            //    }
            //    //刷新最后刷新时间
            //    LastRefreshTime = ZoneServerApi.now;
            //}

            ////低频率刷新检测
            //CheckSlowRefresh(dt);
        }


        // 每日活动刷新
        public void InitTimerManager(DateTime time)
        {
            //获取刷新任务
            Dictionary<DateTime, List<TimingType>> taskDic = TimingLibrary.GetTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                double interval = (kv.Key - DateTime.Now).TotalMilliseconds;
                CounterTimerQuery counterTimer = new CounterTimerQuery(interval, taskDic);
                server.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                //当天没有了，下一天
                InitTimerManager(time.Date.AddDays(1));
            }
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<TimingType>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                double interval = (firstTask.Key - DateTime.Now).TotalMilliseconds;
                CounterTimerQuery counterTimer = new CounterTimerQuery(interval, taskDic);
                server.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                InitTimerManager(time.Date.AddDays(1));
            }
        }
        private void TimingRefreshByPlayers(Dictionary<DateTime, List<TimingType>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);
            DateTime now = DateTime.Now;
            if (firstTask.Value.Contains(TimingType.DailyActivityRefresh))
            {
                //包含刷新活动，先更新当天的活动列表
                ActivityLibrary.RefreshTodayActivityList(now);

                ActionLibrary.RefreshTodayAction(now);
            }

            if (firstTask.Value.Contains(TimingType.RefreshDrawHero))
            {
                //包含刷新活动，先更新当天的活动列表
                DrawLibrary.InitHeroRatioList(server.OpenServerTime, now);
            }
            //有刷新任务
            foreach (var pc in PcList)
            {
                foreach (var timingType in firstTask.Value)
                {
                    pc.Value.TimingRefresh(timingType);
                    pc.Value.BIRecordRefreshLog(firstTask.Key.ToString(), timingType.ToString(), (int)timingType, "online");
                }
                pc.Value.LastRefreshTime = firstTask.Key;
                pc.Value.UpdateLastRefresh();
            }

            foreach (var timingType in firstTask.Value)
            {
                server.TrackingLoggerMng.TrackTimerLog(server.MainId, "zone", timingType.ToString(), server.Now());
            }

            //foreach (var pc in PcOfflineList)
            //{
            //    foreach (var timingType in firstTask.Value)
            //    {
            //        pc.Value.TimingRefresh(timingType);
            //        pc.Value.BIRecordRefreshLog(firstTask.Key.ToString(), timingType.ToString(), (int)timingType, "offline");
            //    }
            //    pc.Value.LastRefreshTime = firstTask.Key;
            //    pc.Value.UpdateLastRefresh();
            //}
        }

        private void UpdatePlayerEnter()
        {
            foreach (var item in playerEnterList)
            {
                try
                {
                    if ((int)(ZoneServerApi.now - item.Value.ReadyTime).TotalSeconds >= ServerShared.CONST.CLIENT_ENTER_EXPIRED_TIME)
                    {
                        // 超时
                        Log.Write("player {0} enter time expired", item.Key);
                        removeEnterList.Add(item.Value);
                        MSG_ZM_CLIENT_ENTER msg = new MSG_ZM_CLIENT_ENTER();
                        msg.CharacterUid = item.Value.Uid;
                        msg.MapId = item.Value.DestMapInfo.MapId;
                        msg.Channel = item.Value.DestMapInfo.Channel;
                        msg.IsExpried = true;
                        this.server.ManagerServer.Write(msg);
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            for (int i = 0; i < removeEnterList.Count; i++)
            {
                try
                {
                    playerEnterList.Remove(removeEnterList[i].Uid);
                }
                catch (Exception e)
                {
                    Log.Alert("remove client enter character {0} map {1} channel {2} err {3}",
                        removeEnterList[i].Uid, removeEnterList[i].DestMapInfo.MapId, removeEnterList[i].DestMapInfo.Channel, e.ToString());
                }
            }
        }

        private void UpdatePcOffline()
        {
            if (GameConfig.CatchOfflinePlayer == true && PcOfflineList.Count > 0)
            {
                foreach (var player in PcOfflineList)
                {
                    try
                    {
                        if ((ZoneServerApi.now - player.Value.OfflineTime).TotalSeconds > GameConfig.CatchOfflinePeriod && !player.Value.InDungeon)
                        {
                            removeOfflineList.Add(player.Key);
                            if (player.Value.CurrentMap != null)//假如在副本中
                            {
                                Log.Debug($"player {player.Value.Uid} removeed from offline cache");
                                player.Value.CurrentMap.OnPlayerLeave(player.Value);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                if (removeOfflineList.Count > 0)
                {
                    MSG_ZM_REMOVE_OFFINE_CLIENT notify = new MSG_ZM_REMOVE_OFFINE_CLIENT();
                    foreach (var uid in removeOfflineList)
                    {
                        try
                        {
                            Log.Write("remove offline pc {0}", uid);
                            notify.UidList.Add(uid);
                            RemoveOfflinePc(uid);
                        }
                        catch (Exception e)
                        {
                            Log.Alert(e.ToString());
                        }
                    }
                    server.ManagerServer.Write(notify);
                    removeOfflineList.Clear();
                }
            }
        }

        private void UpdateLoadingDone()
        {
            foreach (var pc in loadingDoneList)
            {
                try
                {
                    if (loadingList.ContainsKey(pc.Key) == true)
                    {
                        loadingList.Remove(pc.Key);
                        // 进入世界
                        pc.Value.SyncMainId();
                        pc.Value.LoadingDone();
                        if (!pc.Value.needKickPlayer)
                        {
                            pc.Value.OnMoveMap();
                        }
                        Log.Warn("load player {0} time {1}", pc.Key, (ZoneServerApi.now - pc.Value.LoadingStartTime).TotalMilliseconds);
                    }
                    else
                    {
                        // 正在登陆过程中离线，不需要进入地图
                        Log.Warn("player {0} leave world before loading finished", pc.Key);
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            loadingDoneList.Clear();
        }

        double tempNeedRefreshTime = 0;
        double checkTickTime = 1000;
        private bool CheckNeedRefresh(double dt)
        {
            tempNeedRefreshTime += dt;
            if (tempNeedRefreshTime > checkTickTime)
            {
                tempNeedRefreshTime = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        double slowCheckTick = 0;
        double slowRefreshTick = 60 * 1000;//1min 
        DateTime lastSlowRefreshTime = ZoneServerApi.now;

        /// <summary>
        /// 低频率更新检测  每次/1min，用于不需要特别敏感的更新检测
        /// </summary>
        private void CheckSlowRefresh(double dt)
        {
            slowCheckTick += dt;
            if (slowCheckTick >= slowRefreshTick)
            {
                slowCheckTick = 0;

                //1.商店
                List<ShopType> shopList = ShopLibrary.CheckRefreshShopList(lastSlowRefreshTime, ZoneServerApi.now);
                foreach (var kv in pcList)
                {
                    kv.Value.RefreshShop(shopList);
                }

                lastSlowRefreshTime = ZoneServerApi.now;

                if(PassCardLibrary.CheckPeriodUpdate(server.OpenServerTime, lastSlowRefreshTime))
                {
                    foreach (var kv in pcList)
                    {
                        kv.Value.RefreshPassCardPeriod();
                    }
                }
            }
        }


        public void AddLoadingPlayer(int uid, PlayerChar player)
        {
            PlayerChar temp;
            if (!loadingList.TryGetValue(player.Uid, out temp))
            {
                loadingList.Add(uid, player);
            }
        }

        public void RemoveLoadingPlayer(int uid)
        {
            loadingList.Remove(uid);
        }

        public void AddLoadingDonePlayer(PlayerChar player)
        {
            if (player == null) return;
            PlayerChar temp;
            if (!loadingDoneList.TryGetValue(player.Uid, out temp))
            {
                loadingDoneList.Add(player.Uid, player);
            }
        }

        public PlayerEnter GetPlayerEnter(int character_uid)
        {
            PlayerEnter player;
            playerEnterList.TryGetValue(character_uid, out player);
            return player;
        }

        public void AddPlayerEnter(PlayerEnter player_enter)
        {
            if (player_enter == null) return;
            if (playerEnterList.ContainsKey(player_enter.Uid))
            {
                playerEnterList[player_enter.Uid] = player_enter;
            }
            else
            {
                playerEnterList.Add(player_enter.Uid, player_enter);
            }
        }

        public void RemovePlayerEnter(int uid)
        {
            playerEnterList.Remove(uid);
        }

        public void Broadcast<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (msg == null)
            {
                return;
            }
            ArraySegment<byte> body;
            ushort bodyLen = 0;
            PlayerChar.BroadCastMsgBodyMaker(msg, out body, out bodyLen);

            foreach (var player in pcList)
            {
                ArraySegment<byte> header;
                player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                player.Value.Write(header, body);
            }
        }

        public void DeleteAllEmail(int emailId, int send, int delete)
        {
            List<string> stateList = PlayerChar.GetDeleteSystemEmailStateList(emailId, send, delete);

            foreach (var pc in PcList)
            {
                pc.Value.DeleteSystemEmail(stateList);
            }

            foreach (var pc in PcOfflineList)
            {
                pc.Value.DeleteSystemEmail(stateList);
            }
        }

        public List<PlayerChar> GetGatePlayerList(int gate_id)
        {
            List<PlayerChar> list = new List<PlayerChar>();
            foreach (var player in pcList)
            {
                if (player.Value.Gate != null && player.Value.Gate.SubId == gate_id)
                {
                    list.Add(player.Value);
                }
            }
            foreach (var player in pcOfflineList)
            {
                if (player.Value.Gate != null && player.Value.Gate.SubId == gate_id)
                {
                    list.Add(player.Value);
                }
            }
            return list;
        }

        #region 充值活动刷新
        public void InitRechargeTimerManager(DateTime time, int addDay)
        {
            time = time.AddDays(addDay);
            //获取刷新任务
            Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic = RechargeLibrary.GetRechargeTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                AddTaskTimer(taskDic, kv.Key);
            }
            else
            {
                if (addDay > 0)
                {
                    DateTime nextTime = time.Date.AddDays(0.5);
                    taskDic.Add(nextTime, new List<RechargeGiftTimeType>());
                    //说明已经增加过1天
                    AddTaskTimer(taskDic, nextTime);
                }
                else
                {
                    //当天没有了，下一天
                    InitRechargeTimerManager(time.Date, 1);
                }
            }
        }

        private void AddTaskTimer(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic, DateTime time)
        {
            double interval = (time - DateTime.Now).TotalMilliseconds;
            Log.Info($"InitRechargeTimer add task timer {time} ：after {interval}");
            RechargeTimerQuery rechargeTimer = new RechargeTimerQuery(interval, taskDic);
            server.TaskTimerMng.Call(rechargeTimer, (ret) =>
            {
                RechargeTimingRefresh(rechargeTimer.TaskDic);
            });
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                AddTaskTimer(taskDic, firstTask.Key);
            }
            else
            {
                InitRechargeTimerManager(time.AddSeconds(1), 0);
            }
        }

        private void RechargeTimingRefresh(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);
            //有刷新任务
            foreach (var pc in PcList)
            {
                foreach (var giftType in firstTask.Value)
                {
                    pc.Value.RechargeActivityRefresh(giftType);
                    pc.Value.BIRecordRefreshLog(firstTask.Key.ToString(), giftType.ToString(), (int)giftType, "online");                                    
                }

                foreach (var timingType in firstTask.Value)
                {
                    server.TrackingLoggerMng.TrackRechargeTimerLog(server.MainId, "zone", timingType.ToString(), server.Now());
                }
            }
        }       
        #endregion
    }
}
using CommonUtility;
using DBUtility;
using Logger;
using Message.Manager.Protocol.MB;
using Message.Manager.Protocol.MGate;
using Message.Manager.Protocol.MR;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerLogger;
using ServerLogger.KomoeLog;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ManagerServerLib
{
    public partial class ManagerServerApi:BaseApi
    {
        int nLastMin = 61; //FR20161227

        // args [mainId path]
        public override void Init(string[] args)
        {
            base.Init(args);
            // 初始化global
            InitMapBallenceProxy();

            RechargeLibrary.Init();
            GiftLibrary.Init();
            TridentLibrary.Init();

            InitRechargeManager();
            InitAddictionManager();
            InitSchoolManager();

            // init阶段结束，起服完成
            InitDone();
        }

        public override void SpecUpdate(double dt)
        {
            TrackingLoggerMng.CheckNewLogFile(now);   //FR20161205
            mapBallenceProxy.Update();

            AddictionMng.Update();
            //RechargeMng.Update();

            //rechargeMng.UpdateRechargeList();
            //if (updatePerMin.TotalSeconds < 60)

            BroadcastToBarrackCharacterCount();

            if (nLastMin == now.Minute)
            {
                //updatePerMin += dt;
            }
            else
            {
                if (now.Minute == 59)
                {
                    if (now.Hour == 23)
                    {
                        //触发
                        QueryLoadServerResource query = new QueryLoadServerResource();
                        GameDBPool.Call(query, ret =>
                        {
                            BILoggerMng.CurrencyRemainTaLog("00", MainId, query.Gold, query.Diamond, query.FriendlyHeart, query.SotoCoin, query.ResonanceCrystal, query.ShellCoin);
                        });

                    }
                }

                nLastMin = now.Minute;
                // update per min 
                // 每分钟记录一次在线人数
                try
                {
                    Dictionary<string, int> onlineChannelDic = new Dictionary<string, int>();
                    int onlineCount = 0;
                    foreach (var zone in ZoneServerManager.ServerList)
                    {
                        onlineCount += ((ZoneServer)zone.Value).ClientListZone.Count;
                        foreach (var client in ((ZoneServer)zone.Value).ClientListZone)
                        {
                            if (onlineChannelDic.ContainsKey(client.Value.ChannelName))
                            {
                                onlineChannelDic[client.Value.ChannelName] += 1;
                            }
                            else
                            {
                                onlineChannelDic.Add(client.Value.ChannelName, 1);
                            }
                        }
                    }
                    int registCount = 0;
                    //if (GameConfig.TrackingLogSwitch)
                    {
                        // 记录人数 registCount 暂定为0
                        //string log_new =string.Format("{0}|{1}|{2}|{3}", zoneManager.Value.StatLogMainId, registCount , onlineCount, Api.now.ToString("yyyy-MM-dd"));
                        //#mainid|registcount|onlinecount|time
                        string log_new =string.Format("{0}|{1}|{2}|{3}", mainId, registCount , onlineCount, ManagerServerApi.now.ToString("yyyy-MM-dd HH:mm:ss"));
                        TrackingLoggerMng.Write(log_new, TrackingLogType.ONLINE);

                        //BILoggerMng.RecordOnlineLog(onlineCount, onlineCount, onlineCount,mainId);
                        //BILoggerMng.OnlineTaLog(onlineCount, onlineCount, onlineCount,mainId);
                   
                        foreach (var item in onlineChannelDic)
                        {
                            BILoggerMng.OnlineTaLog(item.Value, item.Value, item.Value, mainId, item.Key);

                            KomoeEventLogOnlineNum(item.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }


        /*
      * online_num	分钟级上报单服实时在线人数
          b_utc_timestamp	int	时间戳	例如：1603630292
          b_datetime	string	日志方法名/事件名	例如登录事件上报player_login
          b_log_id	string	每个日志的唯一ID	游戏名+事件名+加uid+加时间戳+随机数，例如 yjzspub999-3216049191039332853
          b_game_id	int	游戏id	一款游戏的ID
          b_platform	string	平台名称	统一：ios|android|windows
          b_zone_id	int	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段
          b_channel_id	int	游戏的渠道ID	游戏的渠道ID
          online_num	int	在线人数(1分钟频率)	
          cp_param		事件自定义参数	没有自定义字段需求则不需要传
      */
        public void KomoeEventLogOnlineNum(int online_num)
        {
            //// LOG 记录开关 
            //if (!GameConfig.TrackingLogSwitch)
            //{
            //    return;
            //}
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();
            string logId = $"{KomoeLogConfig.GameBaseId}-{KomoeLogEventType.online_num}-{0}-{Timestamp.GetUnixTimeStampSeconds(Now())}"; ;
            infDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(Now()));
            infDic.Add("b_datetime", Now().ToString("yyyy-MM-dd HH:mm:ss"));
            infDic.Add("b_log_id", logId);
            infDic.Add("b_eventname", KomoeLogEventType.online_num.ToString());

            infDic.Add("b_game_id", KomoeLogConfig.GameId);
            infDic.Add("b_platform", KomoeLogConfig.Platform);
            infDic.Add("b_zone_id", MainId);
            //infDic.Add("b_channel_id", ChannelId);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("online_num", online_num);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        //private void UpdateMapLimit()
        //{
        //    foreach (var item in mapLimitList)
        //    {
        //        item.Value.CheckTime();
        //    }
        //}

        //public bool IsMapClosed(int map_id)
        //{
        //    MapLimit mapLimit = null;
        //    if (mapLimitList.TryGetValue(map_id, out mapLimit) == true)
        //    {
        //        return mapLimit.IsClosed();
        //    }
        //    return false;
        //}


        public override void ProcessInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo != null)
            {
                if (keyInfo.Key == ConsoleKey.F6)
                {
                    //Log.Write("当前连接客户端数量:" + clientManager.CurCount);
                }

                if (keyInfo.Key == ConsoleKey.F3)
                {
                    GC.Collect();
                }
            }
        }


        public override void StopServer(int min)
        {
            MSG_MR_SHUTDOWN msgRelation = new MSG_MR_SHUTDOWN();
            if (RelationServer != null)
            {
                RelationServer.Write(msgRelation);
            }
            MSG_MZ_SHUTDOWN msgZone = new MSG_MZ_SHUTDOWN();
            // 关闭所有zone
            foreach (var zone in ZoneServerManager.ServerList)
            {
                zone.Value.Write(msgZone);
            }
            base.StopServer(min);
        }

        public void BroadcastToAllManagers<T>(T msg) where T : Google.Protobuf.IMessage
        {
            FrontendServerManager frontendManager = ServerManagerProxy.GetFrontendServerManager(ServerType.ManagerServer);
            if (frontendManager != null)
            {
                frontendManager.Broadcast(msg);
            }
            BackendServerManager backendManager = ServerManagerProxy.GetBackendServerManager(ServerType.ManagerServer);
            if (backendManager != null)
            {
                backendManager.Broadcast(msg);
            }
        }

        public void BroadcastToBarrackCharacterCount()
        {
            if (now > nextSendTime)
            {
                int onlineCount = 0;
                foreach (var zone in ZoneServerManager.ServerList)
                {
                    onlineCount += ((ZoneServer)zone.Value).ClientListZone.Count;
                }

                nextSendTime = nextSendTime.AddSeconds(5);
                MSG_MB_CHARACTER_COUNT msg = new MSG_MB_CHARACTER_COUNT()
                {
                    MainId = mainId,
                    SubId = subId,
                    RegistCount = registCharacterCount,
                    OnlineCount = onlineCount,
                };
                BarrackServerManager?.Broadcast(msg);

                MSG_MGate_REGIST_CHARACTER_COUNT notifyGate = new MSG_MGate_REGIST_CHARACTER_COUNT() { RegistCharacterCount = registCharacterCount };
                GateServerManager?.Broadcast(notifyGate);
            }


        }
    }

}


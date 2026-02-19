using System;
using System.Collections.Generic;
using ServerShared;
using Logger;
using Message.Relation.Protocol.RM;
using EnumerateUtility.Timing;
using ServerFrame;

namespace RelationServerLib
{
    public partial class RelationServerApi:BaseApi
    {
        public int MaxFid = 0;
        public DateTime LastRefreshTime = DateTime.Now;

        public override void SpecUpdate(double dt)
        {
            if (CheckNeedRefresh(lastTime))
            {
                //List<TimingType> tasks = TimingLibrary.GetTimingListToRefresh(LastRefreshTime, RelationServerApi.now);
                //if (tasks.Count > 0)
                //{
                //    //有刷新任务
                //    foreach (var item in tasks)
                //    {
                //        TimingRefresh(item);
                //    }

                //}
                //刷新最后刷新时间
                LastRefreshTime = RelationServerApi.now;
                //relation按秒刷新
                CampActivityMng.Update(dt);
                CampRankMng.Update(dt);
                //SecretAreaMng.Update();
                //crossBattleMng.OnUpdate(dt);
           
            }
            CampRewardMng.OnUpdate();
            ZoneManager.TeamManager.Update();
            ArenaMng.Update();
            ThemeBossMng.Update();
            SpaceTimeTowerManager.Update(dt);

            RecordFrameInfo(dt);
            UID.ConvertTimestamp();
        }

        // 统计最近10秒内的每秒内帧数和CPU睡眠时间，反映当前进程状态
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
                MSG_RM_CPU_INFO msg = new MSG_RM_CPU_INFO();
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


        // args [mainId path]
        public override void Init(string[] args)
        {
            base.Init(args);

            InitManagers();

            //string strFort = "CBEQAhgCIgcIDRUAAMhCKu0JCLFtKucJChkIs6qo3QMSDOWwkeW5tOWUkOS4iRi/ASACEvMBCAIQZBgFIAIwFkim7YRBUmUKBAgBEAEKBAgEEAEKBAgHEAIKBAgKEAEKBggNEJHrCgoHCBAQlPmZAQoGCBMQ/L0BCgUIFhDXGAoFCBkQszEKBQgcEO0wCgUIHxDeSAoHCCIQ4JuYAQoFCCUQjDwQotYVGJHrCloKCAEQARgLINr7EloKCAIQARgLIPXbM1oKCAMQARgLIPfwIloLCAQQARjOCCC4oi1aCwgFEAEYugggnIsRWgsIBhABGIgIIOqNEloKCAcQARgLIK3lIFoKCAgQARgLIN/WGFoLCAkQARj0ByCYhiZaCwgKEAEY/gcg18o3EvMBCAMQZBgFIAEwGUiswoRBUmQKBAgBEAIKBAgEEAEKBAgHEAEKBAgKEAEKBggNEPfhCgoHCBAQ3OKZAQoFCBMQxXoKBQgWEO0YCgUIGRCyMQoFCBwQ8zAKBQgfEMdICgcIIhDFm5gBCgUIJRD/OxDuwxUY9+EKWgoIARABGAsgh88XWgsIAhABGP4HIOfoHloLCAMQARidCCCuqDZaCwgEEAEYugggy8sHWgoIBRABGAsg/OgrWgoIBhABGAsg+eQwWgsIBxABGPQHIOnGFFoKCAgQARgLIPP7D1oLCAkQARixCCCcpSdaCwgKEAEYpggg/sohEvABCAQQZBgFIAgwGkiayoRBUmQKBAgBEAEKBAgEEAIKBAgHEAEKBAgKEAEKBggNEIj0CgoHCBAQ3qWaAQoFCBMQxXsKBQgWEMYYCgUIGRCxMQoFCBwQn1MKBQgfEMtICgcIIhC6m5gBCgUIJRD6OxCQ6BUYiPQKWgoIARABGAsg75gwWgoIAhABGAsg5q0pWgoIAxABGAsg9/wmWgoIBBABGAsg7esaWgoIBRABGAsgk9oIWgoIBhABGAsgztM5WgsIBxABGJEIIKqwMFoLCAgQARjFCCCE5wtaCggJEAEYCyDz4ghaCwgKEAEY8wcgsZ4YEvIBCAUQZBgFIAcwH0js2YRBUmUKBAgBEAEKBAgEEAEKBAgHEAEKBAgKEAIKBggNEKTrCgoHCBAQzI+aAQoGCBMQkL0BCgUIFhDaGAoFCBkQrzEKBQgcEPEwCgUIHxDFSAoHCCIQzZuYAQoFCCUQgDwQyNYVGKTrCloKCAEQARgLIMf9KFoLCAIQARjECCDPnAdaCggDEAEYCyCO6CFaCggEEAEYCyDX8hFaCggFEAEYCyCtkjBaCwgGEAEY9Acgxs0bWgsIBxABGM8IIPHGKVoLCAgQARj0ByD32S1aCggJEAEYCyCDhBdaCggKEAEYCyDylygS9QEIBhBkGAUgAzAgSOfd/UBSZAoECAEQAgoECAQQAQoECAcQAQoECAoQAQoGCA0Qq6cKCgcIEBCQmZkBCgUIExDbdwoFCBYQshcKBQgZEIkwCgUIHBCbLgoFCB8Qh0YKBwgiEK+YmAEKBQglEL85ENbOFBirpwpaCggBEAEYCyDFtChaCwgCEAEY9Acg/80UWgsIAxABGMQIIIOSHloLCAQQARi7CCCnqRxaCwgFEAEY9AcgnJA2WgsIBhABGP8HIKbROVoLCAcQARiSCCCp6ARaCggIEAEYCyD4iShaCwgJEAEYiAgggIAKWgsIChABGJ0IIJCPCDC+p2s4vqdr";
            //campActivityMng.DeserializeFort(17, strFort);
            DoTaskStart(InitGiftCode);
            // init阶段结束，起服完成
            InitDone();
        }

        private void InitManagers()
        {
            InitConfigLoadManager();
            InitEmailManager();
            InitCampManager();
            InitCampActivityManager();
            InitArenaManager();
            InitCrossBattleManager();
            //InitSecretAreaManager();
            InitRedisPlayerInfoManager();
            InitRankManager();
            InitContributionManager();
            InitThemeBossManager();
            InitCrossChallengeManager();
            InitWarehouseManager();
            InitSpaceTimeTowerManager();
        }

        private void InitLibrarys()
        {
            MonsterGenLibrary.Init();
            MonsterLibrary.Init();
            FamilyLibrary.Init();
            EmailLibrary.BindEmailDatas();
            TimingLibrary.BindTimingData();
            RankLibrary.Init();
            CampLibrary.Init();
            CampBattleLibrary.Init();
            DungeonLibrary.Init();
            TeamLibrary.Init();
            ArenaLibrary.Init();
            HeroLibrary.Init();
            RobotLibrary.Init();
            DataInfoLibrary.Init();
            CounterLibrary.Init();
            CommonShopLibrary.InitShopList();

            //跨服
            CrossBattleLibrary.Init();

            //阵营建设
            CampBuildLibrary.Init();

            //阵营活动
            CampActivityLibrary.Init();

            //猎杀魂兽
            HuntingLibrary.Init();
            //贡献
            ContributionLibrary.Init();
            LimitLibrary.BindDatas();
            //主题Boss
            ThemeBossLibrary.Init();
            //充值
            RechargeLibrary.Init(OpenServerTime);
            //称号
            TitleLibrary.Init();
            //石壁
            StoneWallLibrary.Init();
            CrossChallengeLibrary.Init();
            
            SpaceTimeTowerLibrary.InitConfig();
        }

        //private void ProcessSendEmailInfo()
        //{
        //    lock (SendEmailMsgs)
        //    {
        //        foreach (var msgs in SendEmailMsgs)
        //        {
        //            //MSG_RZ_SEND_EMAILS newMsg = new MSG_RZ_SEND_EMAILS();
        //            foreach (var msg in msgs.Value)
        //            {
        //                //newMsg.Emails.AddRange(msg.Emails);
        //                ZoneManagerBroadCast(msg, msgs.Key);
        //            }
        //        }
        //        SendEmailMsgs.Clear();
        //    }
        //}

        //public void AddSendEmailMsg(int mainId, MSG_RZ_SEND_EMAILS msg)
        //{
        //    lock (SendEmailMsgs)
        //    {
        //        List<MSG_RZ_SEND_EMAILS> msgs;
        //        if (SendEmailMsgs.TryGetValue(mainId, out msgs))
        //        {
        //            msgs.Add(msg);
        //        }
        //        else
        //        {
        //            msgs = new List<MSG_RZ_SEND_EMAILS>();
        //            msgs.Add(msg);
        //            SendEmailMsgs.Add(mainId, msgs);
        //        }
        //    }
        //}

        public void BroadcastToAllRelations<T>(T msg) where T : Google.Protobuf.IMessage
        {
            FrontendServerManager frontendManager = ServerManagerProxy.GetFrontendServerManager(ServerType.RelationServer);
            if (frontendManager != null)
            {
                frontendManager.Broadcast(msg);
            }
            BackendServerManager backendManager = ServerManagerProxy.GetBackendServerManager(ServerType.RelationServer);
            if (backendManager != null)
            {
                backendManager.Broadcast(msg);
            }
        }

        public BaseServer GetRelationServer(int mainId)
        {
            BaseServer server = null;
            if (mainId < this.mainId)
            {
                server = serverManagerProxy.GetFrontendServerManager(ServerType.RelationServer).GetSinglePointServer(mainId);
            }
            else
            {
                server = serverManagerProxy.GetBackendServerManager(ServerType.RelationServer).GetSinglePointServer(mainId);
            }
            return server;
        }

        public BaseServer GetRelationWatchServer()
        {
            BaseServer server = null;
            server = serverManagerProxy.GetFrontendServerManager(ServerType.RelationServer).GetWatchDogServer();
            if (server == null)
            {
                server = serverManagerProxy.GetBackendServerManager(ServerType.RelationServer).GetWatchDogServer();
            }
            return server;
        }

    }
}

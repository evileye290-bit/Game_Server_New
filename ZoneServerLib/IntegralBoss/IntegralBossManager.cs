using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZGate;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class IntegralBossManager
    {
        private ZoneServerApi server;
        private DateTime checkTime = BaseApi.now;

        private IntegralBossState state = IntegralBossState.Stop;
        public IntegralBossState State { get { return state; } }

        private DateTime stopTime = DateTime.MaxValue;
        public DateTime OpenTime { get { return openTime; } }

        private DateTime openTime = DateTime.MaxValue;
        public DateTime StopTime { get { return stopTime; } }

        public bool IsOpenning { get { return state == IntegralBossState.Openning; } }

        public IntegralBossManager(ZoneServerApi server)
        {
            this.server = server;
            SetNextOpenTime();
        }

        public void Update()
        {
            if ((BaseApi.now - checkTime).TotalSeconds < 0.2)
            {
                return;
            }
            checkTime = BaseApi.now;
            switch (state)
            { 
                case IntegralBossState.Stop:
                    CheckPreOpen();
                    break;
                case IntegralBossState.PreOpen:
                    CheckStart();
                    break;
                case IntegralBossState.Openning:
                    CheckStop();
                    break;
                default:
                    break;
            }
        }

        private void CheckPreOpen()
        {
            if (state == IntegralBossState.PreOpen)
            {
                return;
            }

            //与开启状态
            IntegralBossRefreshInfo refreshInfo = null;
            if (IntegralBossLibrary.HaveBossPreOpen(BaseApi.now, ref refreshInfo))
            {
                NotifyIntegralBoss();
                PreStart(refreshInfo);
                return;
            }

            //已开启状态
            if (IntegralBossLibrary.HaveBossOpening(BaseApi.now, ref refreshInfo))
            {
                PreStart(refreshInfo);
            }         
        }

        private void CheckStart()
        {
            if (state == IntegralBossState.Openning)
            {
                return;
            }

            if (BaseApi.now >= openTime)
            {
                state = IntegralBossState.Openning;
                SetIntegralBossNPCState(true);
                NotifyState();

                Log.Info($"IntegralBoss openning at {openTime.ToString()}");
            }         
        }

        private void CheckStop()
        {
            // 如果30分钟后则不允许再进入 进入NPC消失
            if (state == IntegralBossState.Stop)
            {
                return;
            }

            if (stopTime < BaseApi.now)
            {
                Stop();
            }
        }

        private void PreStart(IntegralBossRefreshInfo refreshInfo)
        {
            Log.Info($"IntegralBoss pre start at {BaseApi.now.ToString()}");

            state = IntegralBossState.PreOpen;
            openTime = BaseApi.now.Date.Add(refreshInfo.OpenTime);
            stopTime = openTime.AddMinutes(IntegralBossLibrary.Lastime);
            NotifyState();
        }

        private void SetIntegralBossNPCState(bool appear)
        {
            foreach (var kv in server.MapManager.FieldMapList)
            {
                foreach (var map in kv.Value.Values)
                {
                    if (map.IsDungeon)
                    {
                        break;
                    }
                    if (appear)
                    {
                        map.AppearIntegralBossNpc();
                    }
                    else
                    {
                        map.DisappearIntegralBossNpc();
                    }
                }
            }
        }

        private void Stop()
        {
            if (state != IntegralBossState.Openning)
            {
                return;
            }
            state = IntegralBossState.Stop;

            Log.Info($"IntegralBoss stop at {BaseApi.now.ToString()}");

            SetNextOpenTime();

            NotifyState();
            SetIntegralBossNPCState(false);

            NotifyIntegralBossEnd();
        }

        private void SetNextOpenTime()
        {
            IntegralBossRefreshInfo refreshInfo = null;
            bool gotoNextDay = IntegralBossLibrary.GetNextOpenInfo(BaseApi.now, ref refreshInfo);
            openTime = BaseApi.now.Date.Add(refreshInfo.OpenTime);

            if (gotoNextDay)
            {
                openTime = openTime.AddDays(1);
            }
            stopTime = openTime.AddMinutes(IntegralBossLibrary.Lastime);

            Log.Debug($"IntegralBoss next start at {openTime.ToString()} stop at {stopTime.ToString()}");
        }

        private void NotifyState()
        {
            MSG_ZGC_INTERGRAL_BOSS_STATE notify = new MSG_ZGC_INTERGRAL_BOSS_STATE();
            notify.State = (int)state;
            if (state != IntegralBossState.Openning)
            {
                notify.StateTime = Timestamp.GetUnixTimeStampSeconds(server.IntegralBossManager.OpenTime);
            }
            server.PCManager.PcList.Values.ForEach(ply => ply.Write(notify));
        }

        //通知整点boss战
        private void NotifyIntegralBoss()
        {
            MSG_ZR_INTEGRALBOSS_START request = new MSG_ZR_INTEGRALBOSS_START();
            server.SendToRelation(request);
        }
   
        private void NotifyIntegralBossEnd()
        {
            MSG_ZR_INTEGRALBOSS_END request = new MSG_ZR_INTEGRALBOSS_END();
            server.SendToRelation(request);
        }
    }
}

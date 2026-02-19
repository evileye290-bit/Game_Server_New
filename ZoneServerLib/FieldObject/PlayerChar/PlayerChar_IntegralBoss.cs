using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using DataProperty;
using Message.Zone.Protocol.ZM;
using ServerModels;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //整点boss
        private HashSet<int> fightedIntegralBossList = new HashSet<int>();
        public DateTime IntegralBossLastTime { get; set; }

        public void RecordIntergralBoss(int dungeonId)
        {
            CheckAndResetIntegralBossInfo();

            IntegralBossLastTime = BaseApi.now;
            SyncIntegralBossTimeToDB();
            if (!fightedIntegralBossList.Contains(dungeonId))
            {
                fightedIntegralBossList.Add(dungeonId);
            }
        }

        public void IntegralBossReward(RewardManager manager, int dungeonId)
        {
            //当前时间段打过Boss 则再次打给帮杀奖励
            bool helpReward = fightedIntegralBossList.Contains(dungeonId);

            //剩余挑战次数
            int restCount = GetDungeonChallengeRestCount(MapType.IntegralBoss);

            //挑战次数不够给帮杀奖励
            if (!helpReward && restCount <= 0)
            { 
                helpReward = true;
            }

            if (helpReward)
            {
                //帮杀次数用完
                if (CheckCounter(CounterType.TeamHelpCount))
                {
                    NotifyDungeonHelpUeslessRewardMsg(dungeonId);
                    return;
                }

                manager = GetDungeonHelpReward();
                UpdateCounter(CounterType.TeamHelpCount, 1);
            }
            else
            {
                UpdateCounter(CounterType.IntegralBoss, 1);
            }

            //记录已击杀
            RecordIntergralBoss(dungeonId);

            //下发奖励
            AddRewards(manager, ObtainWay.IntegralBoss);

            //玩家还在副本中，通知前端奖励, 避免出现玩家在主城弹出结算面板
            if (!CurrentMap.IsDungeon)
            {
                return;
            }

            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = dungeonId;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);

            AddRunawayActivityNumForType(RunawayAction.IntegralBoss);
        }

        public bool CheckKillIntegralBoss(int dungeonId)
        {
            return fightedIntegralBossList.Contains(dungeonId);
        }

        public ErrorCode CheckIntegralBoss()
        {
            if (!CheckLimitOpen(LimitType.IntegralBoss))
            {
                return ErrorCode.LevelLimit;
            }
            if (!server.IntegralBossManager.IsOpenning)
            {
                return ErrorCode.NotOpen;
            }
            return ErrorCode.Success;
        }

        public void SendIntegralBossMsg()
        {
            MSG_ZGC_INTERGRAL_BOSS_STATE msg = new MSG_ZGC_INTERGRAL_BOSS_STATE();
            IntegralBossState state = server.IntegralBossManager.State;
            msg.State = (int)state;
            if (state != IntegralBossState.Openning)
            {
                msg.StateTime = Timestamp.GetUnixTimeStampSeconds(server.IntegralBossManager.OpenTime);
            }
            Write(msg);
        }

        public void RequestIntegralBossInfo()
        {
            MSG_ZGC_INTERGRAL_BOSS_INFO response = new MSG_ZGC_INTERGRAL_BOSS_INFO();
            ErrorCode code = CheckIntegralBoss();
            response.Result = (int)code;

            if (code != ErrorCode.Success)
            {
                Log.Warn("player {0} request intgeral boss info failed : errorCode {1}", Uid, (int)code);
                Write(response);
                return;
            }

            CheckAndResetIntegralBossInfo();
            response.KilledList.AddRange(fightedIntegralBossList);
            response.StopTime = Timestamp.GetUnixTimeStampSeconds(server.IntegralBossManager.StopTime);
            Write(response);
        }

        public void RequestIntegralBossKillInfo(int dungeonId)
        {
            MSG_ZGC_INTERGRAL_BOSS_KILLINFO response = new MSG_ZGC_INTERGRAL_BOSS_KILLINFO();
            response.Result = (int)ErrorCode.Success;
            response.Killed = fightedIntegralBossList.Contains(dungeonId);
            Write(response);
        }

        public void LoadIntegralBossFromTransform(MSG_ZMZ_INTEGRAL_BOSS_INFO info)
        {
            this.IntegralBossLastTime = Timestamp.TimeStampToDateTime(info.IntegralBossLastTime);
            if (info.BossList.Count > 0)
            {
                info.BossList.ForEach(x => this.fightedIntegralBossList.Add(x));
            }
        }

        public MSG_ZMZ_INTEGRAL_BOSS_INFO GenerateIntegralBossTransformMsg()
        {
            MSG_ZMZ_INTEGRAL_BOSS_INFO msg = new MSG_ZMZ_INTEGRAL_BOSS_INFO();
            msg.IntegralBossLastTime = Timestamp.GetUnixTimeStampSeconds(server.IntegralBossManager.StopTime);
            msg.BossList.AddRange(fightedIntegralBossList);
            return msg;
        }

        private void CheckAndResetIntegralBossInfo()
        {
            if (IntegralBossLastTime < server.IntegralBossManager.OpenTime)
            {
                fightedIntegralBossList.Clear();
            }
        }

        private void SyncIntegralBossTimeToDB()
        {
            server.GameDBPool.Call(new QueryUpdateIntegralBossTime(uid, IntegralBossLastTime));
        }

        private void SyncIntegralBoss2Redis(int dungeonId)
        {
            server.GameRedis.Call(new OperateSetIntegralBossAdd(uid, dungeonId));
        }

        private void IntegralBoss2RedisClear()
        {
            server.GameRedis.Call(new OperateSetIntegralBossClear(uid));
        }
    }
}

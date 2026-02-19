using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public CarnivalBossManager CarnivalBossMng { get; private set; }

        public void InitCarnivalBossManager()
        {
            CarnivalBossMng = new CarnivalBossManager(this);
        }

        public void SendCarnivalBossInfo()
        {
            MSG_ZGC_CARNIVAL_BOSS_INFO msg = new MSG_ZGC_CARNIVAL_BOSS_INFO();
            msg.Level = CarnivalBossMng.Info.Level;
            msg.Degree = CarnivalBossMng.Info.Degree;
            msg.GotRankReward = CarnivalBossMng.Info.GotRankReward;
            msg.RewardDegrees.AddRange(CarnivalBossMng.Info.RewardedDegreeList);
            Write(msg);
        }

        public void SendCarnivalBossInfoByLoading()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.CarnivalBoss, ZoneServerApi.now, out model))
            {
                return;
            }
            MSG_ZGC_CARNIVAL_BOSS_INFO msg = new MSG_ZGC_CARNIVAL_BOSS_INFO();
            msg.Level = CarnivalBossMng.Info.Level;
            msg.Degree = CarnivalBossMng.Info.Degree;
            msg.GotRankReward = CarnivalBossMng.Info.GotRankReward;
            msg.RewardDegrees.AddRange(CarnivalBossMng.Info.RewardedDegreeList);
            Write(msg);
        }

        public void EnterCarnivalBossDungeon(int dungeonId)
        {
            MSG_ZGC_ENTER_CARNIVAL_BOSS_DUNGEON response = new MSG_ZGC_ENTER_CARNIVAL_BOSS_DUNGEON();
            response.DungeonId = dungeonId;
            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.CarnivalBoss, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} enter carnival boss dungeon failed: carnival boss not open");
                Write(response);
                return;
            }
            int realDungeonId = CarnivalBossLibrary.GetDungeonByLevel(CarnivalBossMng.Info.Level);
            if (dungeonId != realDungeonId)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter carnival boss dungeon failed: dungeonId {dungeonId} is not right, realDungeonId {realDungeonId}");
                Write(response);
                return;
            }
            if (CarnivalBossMng.Info.Degree >= CarnivalBossLibrary.MaxDegree)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter carnival boss dungeon failed: already reach max degree");
                Write(response);
                return;
            }

            ErrorCode result = CanCreateDungeon(dungeonId);
            if (result != ErrorCode.Success)
            {
                response.Result = (int)result;
                Log.Warn($"player {Uid} enter carnival boss dungeon failed: can not create dungeon");
                Write(response);
                return;
            }
            // 在当前zone创建副本
            CarnivalBossDungeon dungeon = server.MapManager.CreateDungeon(dungeonId) as CarnivalBossDungeon;
            if (dungeon == null)
            {
                Log.Warn($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }                  
            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        public void AddCarnivalBossDegree(double degree, bool killed = false)
        {
            CarnivalBossMng.AddCarnivalBossDegree(degree, killed);           
            CarnivalBossMng.UpdateRank();     
            SendCarnivalBossInfo();
            //日志
            //BIRecordThemeBossLog(ThemeBossManager.Period, ThemeBossManager.Level, (int)ThemeBossManager.Degree * 100);
        }

        public void GetCarnivalBossReward(int rewardDegree)
        {
            MSG_ZGC_GET_CARNIVAL_BOSS_REWARD response = new MSG_ZGC_GET_CARNIVAL_BOSS_REWARD();
            response.Degree = rewardDegree;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.CarnivalBoss, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get carnival boss reward failed: carnival boss not open");
                Write(response);
                return;
            }
            ErrorCode result = CarnivalBossMng.CheckCanGetReward(rewardDegree);
            if (result != ErrorCode.Success)
            {
                Log.Warn($"player {Uid} carnival boss level {CarnivalBossMng.Info.Level} degree {CarnivalBossMng.Info.Degree} get reward {rewardDegree} failed: errorCode {(int)result}");
                response.Result = (int)result;
                Write(response);
                return;
            }
            string reward = CarnivalBossLibrary.GetLevelDegreeReward(CarnivalBossMng.Info.Level, rewardDegree);          
            if (!string.IsNullOrEmpty(reward))
            {
                CarnivalBossMng.AddRewardedDegree(rewardDegree);
                CarnivalBossMng.CheckUpdateLevel(rewardDegree);
                CarnivalBossMng.SyncDbUpdateCarnivalBossInfo();
                
                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                AddRewards(manager, ObtainWay.CarnivalBoss);
                manager.GenerateRewardItemInfo(response.Rewards);

                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                Log.Warn($"player {Uid} carnival boss level {CarnivalBossMng.Info.Level} degree {CarnivalBossMng.Info.Degree} get reward {rewardDegree} failed: not find reward");
                response.Result = (int)ErrorCode.NoData;
            }
            Write(response);
            SendCarnivalBossInfo();
        }

        internal void UpdateCarnivalBossQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_CARNIVAL_BOSS_QUEUE response = new MSG_ZGC_UPDATE_CARNIVAL_BOSS_QUEUE();
            Dictionary<int, Dictionary<int, HeroInfo>> oldQueue = new Dictionary<int, Dictionary<int, HeroInfo>>(HeroMng.CarnivalBossQueue);
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update carnival boss queue fail,hero {heroId} not exist");
                    continue;
                }
                if (item.QueueNum > CarnivalBossLibrary.QueueCount)
                {
                    Log.Error($"player {uid} update carnival boss queue fail,hero {heroId} not exist queue {item.QueueNum}");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.CarnivalBossQueueNum,item.QueueNum))
                {
                    Log.Error($"player {uid} carnival boss queue fail,hero {heroId} exist in queue {heroInfo.CarnivalBossQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.CarnivalBoss, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update carnival boss queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.CarnivalBoss, heroInfo, item.QueueNum, item.PositionNum, updateList);
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItem(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                TrackDungeonQueueLog(HeroQueueType.CarnivalBoss, updateList);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //komoeLog
            KomoeLogRecordBattleteamFlow("嘉年华BOSS", oldQueue, HeroMng.CarnivalBossQueue);
        }

        public void GetCarnivalBossRankReward(int rank, MSG_ZGC_GET_CROSS_RANK_REWARD response)
        {
            string reward = CarnivalBossLibrary.GetRankReward(rank);
            if (!string.IsNullOrEmpty(reward))
            {
                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                AddRewards(manager, ObtainWay.CarnivalBoss);
                manager.GenerateRewardItemInfo(response.Rewards);
            }
            response.Result = (int)ErrorCode.Success;
            CarnivalBossMng.ChangeRankRewardGetState();
            SendCarnivalBossInfo();
        }

        private void ClearCarnivalBossInfo()
        {
            CarnivalBossMng.Clear();
            SendCarnivalBossInfo();
        }
    }
}

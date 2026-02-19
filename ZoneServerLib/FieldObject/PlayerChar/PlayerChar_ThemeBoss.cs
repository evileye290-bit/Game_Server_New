using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public ThemeBossManager ThemeBossManager { get; set; }

        public void InitThemeBossManager()
        {
            ThemeBossManager = new ThemeBossManager(this);
        }

        public void InitThemeBossInfo(QueryLoadThemeBoss info)
        {
            ThemeBossManager.InitThemeBossInfo(info);
        }

        public void SendThemeBossInfo()
        {
            MSG_ZGC_THEME_BOSS_INFO msg = new MSG_ZGC_THEME_BOSS_INFO();
            msg.Period = ThemeBossManager.Period;
            msg.Level = ThemeBossManager.Level;
            msg.Degree = ThemeBossManager.Degree;
            msg.Rewarded.AddRange(ThemeBossManager.GetRewardedList());
            Write(msg);
        }

        public void EnterThemeBossDungeon(int dungeonId)
        {
            MSG_ZGC_THEMEBOSS_DUNGEON response = new MSG_ZGC_THEMEBOSS_DUNGEON();
            response.DungeonId = dungeonId;
            //检查是否活动开启
            Dictionary<int, RechargeGiftModel> themeBossList = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemeBoss);
            RechargeGiftModel themeBossModel;
            themeBossList.TryGetValue(ThemeBossManager.Period, out themeBossModel);
            if (themeBossModel == null || ZoneServerApi.now < themeBossModel.StartTime || ZoneServerApi.now >= themeBossModel.EndTime)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter theme boss dungeon failed: themeBoss period {ThemeBossManager.Period} not open");
                Write(response);
                return;
            }          
            int realDungeonId = ThemeBossLibrary.GetThemeBossDungeon(ThemeBossManager.Period, ThemeBossManager.Level);
            if (dungeonId != realDungeonId)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter theme boss dungeon failed: dungeonId {dungeonId} is not right, realDungeonId {realDungeonId}");
                Write(response);
                return;
            }
            if (ThemeBossManager.Degree >= ThemeBossLibrary.MaxDegree)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter theme boss dungeon failed: already reach max degree");
                Write(response);
                return;
            }
        
            ErrorCode result = CanCreateDungeon(dungeonId);
            if (result != ErrorCode.Success)
            {
                response.Result = (int)result;
                Log.Warn($"player {Uid} enter theme boss dungeon failed: can not create dungeon");
                Write(response);
                return;
            }
            // 在当前zone创建副本
            ThemeBossDungeon dungeon = server.MapManager.CreateDungeon(dungeonId) as ThemeBossDungeon;
            if (dungeon == null)
            {
                Log.Warn($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }           
            if (CheckInBuffTime(ZoneServerApi.now))
            {
                dungeon.SetBuffState(true);
            }
            //if (ThemeBossManager.Period == 0)
            //{
            //    ThemeBossManager.UpdatePeriod();//
            //    SyncDbInsertThemeBossInfo(ThemeBossManager.Period);
            //}
            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        private bool CheckInBuffTime(DateTime now)
        {
            Dictionary<int, ThemeBossBuffTime> timeList = ThemeBossLibrary.GetThemeBossBuffTimeList();
            if (timeList == null)
            {
                return false;
            }
            foreach (var item in timeList)
            {
                if (now >= item.Value.StartTime && now < item.Value.EndTime)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddThemeBossDegree(double degree, bool killed = false)
        {
            ThemeBossManager.AddThemeBossDegree(degree, killed);
            ThemeBossManager.SyncRedisUpdateThemeBossRank();
            //SerndUpdateRankValue(RankType.ThemeBoss, score);
            SendThemeBossInfo();
            //日志
            BIRecordThemeBossLog(ThemeBossManager.Period, ThemeBossManager.Level, (int)ThemeBossManager.Degree * 100);
        }

        public void GetThemeBossReward(int rewardDegree)
        {
            MSG_ZGC_GET_THEMEBOSS_REWARD response = new MSG_ZGC_GET_THEMEBOSS_REWARD();
            response.RewardId = rewardDegree;
          
            //检查是否活动开启
            Dictionary<int, RechargeGiftModel> themeBossList = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemeBoss);
            RechargeGiftModel themeBossModel;
            themeBossList.TryGetValue(ThemeBossManager.Period, out themeBossModel);
            if (themeBossModel == null || (ZoneServerApi.now < themeBossModel.StartTime && ZoneServerApi.now >= themeBossModel.EndTime))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} enter theme boss dungeon failed: themeBoss period {ThemeBossManager.Period} not open");
                Write(response);
                return;
            }
            ErrorCode result = ThemeBossManager.CheckCanGetReward(rewardDegree);
            if (result != ErrorCode.Success)
            {
                Log.Warn($"player {Uid} themeBoss period {ThemeBossManager.Period} level {ThemeBossManager.Level} degree{ThemeBossManager.Degree} get reward {rewardDegree} failed: errorCode {(int)result}");
                response.Result = (int)result;
                Write(response);
                return;
            }
            Dictionary<int, string> allDegreeRewards = ThemeBossLibrary.GetThemeBossAllDegreeRewards(ThemeBossManager.Period, ThemeBossManager.Level, ThemeBossManager.Degree);//
            if (allDegreeRewards == null)
            {
                Log.Warn($"player {Uid} themeBoss period {ThemeBossManager.Period} level {ThemeBossManager.Level} degree{ThemeBossManager.Degree} get reward {rewardDegree} failed: already got all rewards");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            string reward;
            allDegreeRewards.TryGetValue(rewardDegree, out reward);
            if (!string.IsNullOrEmpty(reward))
            {
                ThemeBossManager.AddThemeBossRewardedDegree(rewardDegree);
                ThemeBossManager.CheckUpdateThemeBossLevel(rewardDegree);
                ThemeBossManager.SyncDbUpdateThemeBossInfo();

                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                AddRewards(manager, ObtainWay.ThemeBoss);
                manager.GenerateRewardItemInfo(response.Rewards);

                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                Log.Warn($"player {Uid} themeBoss period {ThemeBossManager.Period} level {ThemeBossManager.Level} degree{ThemeBossManager.Degree} get reward {rewardDegree} failed: not find reward");
                response.Result = (int)ErrorCode.NoData;
            }
            Write(response);
            SendThemeBossInfo();
        }

        internal void UpdateThemeQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE response = new MSG_ZGC_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE();
            Dictionary<int, Dictionary<int, HeroInfo>> oldQueue = new Dictionary<int, Dictionary<int, HeroInfo>>(HeroMng.ThemeBossQueue);
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update theme queue fail,hero {heroId} not exist");
                    continue;
                }
                if (item.QueueNum > ThemeBossLibrary.ThemeQueueCount)
                {
                    Log.Error($"player {uid} update theme queue fail,hero {heroId} not exist queue {item.QueueNum}");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.ThemeBossQueueNum, item.QueueNum))
                {
                    Log.Error($"player {uid} carnival theme boss queue fail,hero {heroId} exist in queue {heroInfo.ThemeBossQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.ThemeBoss, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update theme boss queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.ThemeBoss, heroInfo, item.QueueNum, item.PositionNum, updateList);
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

                TrackDungeonQueueLog(HeroQueueType.ThemeBoss, updateList);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //komoeLog
            KomoeLogRecordBattleteamFlow("主题BOSS", oldQueue, HeroMng.ThemeBossQueue);
        }

        public void CheckOpenNewThemeBoss()
        {
            Dictionary<int, RechargeGiftModel> themeBossList = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemeBoss);           
            foreach (var item in themeBossList)
            {
                if (ZoneServerApi.now >= item.Value.StartTime && ZoneServerApi.now < item.Value.EndTime)
                {                  
                    if (ThemeBossManager.Period == 0)
                    {
                        ThemeBossManager.UpdateThemeBossInfoToNewPeriod(item.Value.SubType);
                        ThemeBossManager.SyncDbInsertThemeBossInfo();
                    }
                    else if(ThemeBossManager.Period != item.Value.SubType)
                    {
                        ThemeBossManager.UpdateThemeBossInfoToNewPeriod(item.Value.SubType);
                        ThemeBossManager.SyncDbUpdateThemeBossPeriodInfo();
                    }
                    SendThemeBossInfo();
                    break;
                }               
            }
        }
    }
}

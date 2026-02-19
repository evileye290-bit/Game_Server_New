using DBUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CarnivalBossManager
    {
        private ZoneServerApi server;      
        private PlayerChar owner { get; set; }
        private CarnivalBossInfo info = new CarnivalBossInfo();
        public CarnivalBossInfo Info { get { return info; } }
        private bool onShow = false;

        public CarnivalBossManager(ZoneServerApi server)
        {
            this.server = server;                   
        }

        public CarnivalBossManager(PlayerChar owner)
        {
            this.owner = owner;
        }    

        public void Init(CarnivalBossInfo info)
        {
            this.info = info;
        }

        public void Update()
        {
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.CarnivalBoss, ZoneServerApi.now, out activityModel))
            {
                if (onShow)
                {
                    SetCarnivalBossNPCState(false);
                    onShow = false;
                }
                return;
            }
            DateTime startTime = RechargeLibrary.GetActivityStartTime(activityModel);

            if (server.Now() >= startTime && server.Now() < activityModel.EndTime)
            {
                SetCarnivalBossNPCState(true);
                onShow = true;
            }
            else if (server.Now() >= activityModel.EndTime)
            {
                SetCarnivalBossNPCState(false);
            }
        }

        private void SetCarnivalBossNPCState(bool appear)
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
                        map.AppearCarnivalBossNpc();
                    }
                    else
                    {
                        map.DisappearCarnivalBossNpc();
                    }
                }
            }
        }

        public void AddCarnivalBossDegree(double degree, bool killed)
        {
            if (killed)
            {
                info.Degree = 100.00;
            }
            else
            {
                degree = degree * 100;              
                info.Degree += degree;
                //保证缓存中是两位小数
                info.Degree = (double)Math.Round(info.Degree * 100) / 100;               
            }
            SyncDbUpdateCarnivalBossDegree();
        }

        public void UpdateRank()
        {
            int score = info.Level * 100000 + (int)(info.Degree * 100);           
            int maxScore = CarnivalBossLibrary.MaxLevel * 100000 + CarnivalBossLibrary.MaxDegree * 100;
            if (maxScore < score)
            {
                score = maxScore;
            }
            owner.SerndUpdateRankValue(RankType.CarnivalBoss, score);
            //Owner.server.TrackingLoggerMng.RecordRealtionRankLog(Owner.Uid, score, score, 0, RankType.ThemeBoss.ToString(), Owner.server.MainId, 0, Owner.server.Now());
        }

        public ErrorCode CheckCanGetReward(int rewardDegree)
        {
            ErrorCode result = ErrorCode.Success;
            if (info.RewardedDegreeList.Contains(rewardDegree))
            {
                result = ErrorCode.AlreadyGot;
                return result;
            }
            if (info.Degree < rewardDegree)
            {
                result = ErrorCode.NotReach;
                return result;
            }
            return result;
        }

        public void AddRewardedDegree(int rewardDegree)
        {
            info.RewardedDegreeList.Add(rewardDegree);
        }

        public void CheckUpdateLevel(int rewardDegree)
        {
            int rewardedCount = CarnivalBossLibrary.GetBossLevelRewardCount(info.Level);
            if (rewardedCount != 0 && info.RewardedDegreeList.Count == rewardedCount && info.Level < CarnivalBossLibrary.MaxLevel)
            {
                info.Level++;
                info.Degree = 0;
                info.RewardedDegreeList.Clear();
            }
        }

        public int GetRankScore()
        {
            return info.Level * 100000 + (int)(info.Degree * 100);
        }

        public void Clear()
        {
            info.Level = 1;
            info.Degree = 0;
            info.RewardedDegreeList.Clear();
            info.GotRankReward = 0;
        }

        public void ChangeRankRewardGetState()
        {
            info.GotRankReward = 1;
            SyncDbUpdateCarnivalBossRankStateGetInfo();
        }

        private void SyncDbUpdateCarnivalBossDegree()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCarnivalBossDegree(owner.Uid, info.Degree));
        }

        public void SyncDbUpdateCarnivalBossInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCarnivalBossRewardInfo(owner.Uid, info.Level, info.Degree, info.RewardedDegreeList));
        }

        private void SyncDbUpdateCarnivalBossRankStateGetInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateCarnivalBossRankRewardGetInfo(owner.Uid, info.GotRankReward));
        }

        public void GenerateCarnivalBossTransformMsg(ZMZ_THEME_INFO msg)
        {
            msg.CarnivalBossInfo = new ZMZ_THEMEBOSS_INFO() { Level = info.Level, Degree = info.Degree, GotRanReward = info.GotRankReward };
            msg.CarnivalBossInfo.Rewarded.AddRange(info.RewardedDegreeList);
        }

        public void LoadCarnivalBossInfoTransform(ZMZ_THEMEBOSS_INFO msg)
        {
            info.Level = msg.Level;
            info.Degree = msg.Degree;
            info.GotRankReward = msg.GotRanReward;
            info.RewardedDegreeList.AddRange(msg.Rewarded);
        }
    }
}

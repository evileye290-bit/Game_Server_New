using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ThemeBossManager
    {
        private ZoneServerApi server;
        public PlayerChar Owner { get; set; }
        /// <summary>
        /// 期数
        /// </summary>
        private int period = 0;
        public int Period { get { return period; } }

        private int level = 0;
        public int Level { get { return level; } }
        /// <summary>
        /// 关卡进度
        /// </summary>
        private double degree = 0;
        public double Degree { get { return degree; } }
        /// <summary>
        /// 当前Level已领奖的进度值
        /// </summary>
        private List<int> rewardedDegreeList = new List<int>();

        private int npcPeriod = 1;
        public int NpcPeriod { get { return npcPeriod; } }

        public ThemeBossManager(ZoneServerApi server)
        {
            this.server = server;
        }

        public ThemeBossManager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void InitThemeBossInfo(QueryLoadThemeBoss info)
        {
            period = info.Period;
            level = info.Level;
            degree = info.Degree;
            if (!string.IsNullOrEmpty(info.RewardedDegree))
            {
                string[] degreeArray = StringSplit.GetArray("|", info.RewardedDegree);
                foreach (var degree in degreeArray)
                {
                    rewardedDegreeList.Add(degree.ToInt());
                }
            }
        }

        public List<int> GetRewardedList()
        {
            return rewardedDegreeList;
        }
        
        //public void RecordBuffState(bool hasBuff)
        //{
        //    this.hasBuff = hasBuff;
        //}

        public void AddThemeBossDegree(double degree, bool killed)
        {
            if (killed)
            {
                this.degree = 100.00;
            }
            else
            {
                degree = degree * 100;
                //double finalDegree = (double)Math.Round(degree * 100) / 100;
                //if (this.degree + finalDegree > ThemeBossLibrary.MaxDegree)
                //{
                //    this.degree = 100.00;
                //}
                //else
                //{
                    this.degree += degree;
                    //保证缓存中是两位小数
                    this.degree = (double)Math.Round(this.degree * 100) / 100;
                //}
            }
            SyncDbUpdateThemeBossDegree();
        }

        public ErrorCode CheckCanGetReward(int rewardDegree)
        {
            ErrorCode result = ErrorCode.Success;         
            if (rewardedDegreeList.Contains(rewardDegree))
            {
                result = ErrorCode.AlreadyGot;
                return result;
            }
            if (degree < rewardDegree)//
            {
                result = ErrorCode.NotReach;
                return result;
            }          
            return result;
        }

        public void AddThemeBossRewardedDegree(int rewardDegree)
        {
            rewardedDegreeList.Add(rewardDegree);
        }

        public void CheckUpdateThemeBossLevel(int rewardDegree)
        {
            int rewardedCount = ThemeBossLibrary.GetThemeBossRewardRewardCount(Period, Level);
            if (rewardedCount != 0 && rewardedDegreeList.Count == rewardedCount && Level < ThemeBossLibrary.GetThemeBossMaxLevel(Period))
            {
                level++;
                degree = 0;
                rewardedDegreeList.Clear();
            }
        }

        private string GetRewardedDegreeStr()
        {
            string rewarded = "";
            foreach (var item in rewardedDegreeList)
            {
                rewarded += item + "|";
            }
            return rewarded;
        }

        public int GetRankScore()
        {
            return Level * 100000 + (int)(Degree * 100);
        }     

        public void UpdateThemeBossInfoToNewPeriod(int period)
        {
            this.period = period;
            level = 1;
            degree = 0;
            rewardedDegreeList.Clear();
        }

        public void Update()
        {
            Dictionary<int, RechargeGiftModel> themeBossList = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemeBoss);
            if (themeBossList != null)
            {
                RechargeGiftModel themeBoss;
                themeBossList.TryGetValue(NpcPeriod, out themeBoss);
                if (themeBoss == null)
                {
                    return;
                }
                if (ZoneServerApi.now >= themeBoss.StartTime && ZoneServerApi.now < themeBoss.EndTime)
                {
                    SetThemeBossNPCState(true, NpcPeriod);
                }
                else if (ZoneServerApi.now >= themeBoss.EndTime)
                {
                    SetThemeBossNPCState(false, NpcPeriod);
                    npcPeriod++;
                }
            }
        }

        private void SetThemeBossNPCState(bool appear, int period)
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
                        map.AppearThemeBossNpc(period);
                    }
                    else
                    {
                        map.DisappearThemeBossNpc(period);
                    }
                }
            }
        }

        #region db
        public void SyncDbInsertThemeBossInfo()
        {
            Owner.server.GameDBPool.Call(new QueryInsertThemeBossInfo(Owner.Uid, Period));
        }

        public void SyncDbUpdateThemeBossPeriodInfo()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateThemeBossInfo(Owner.Uid, Period, Level, Degree, GetRewardedDegreeStr()));
        }

        private void SyncDbUpdateThemeBossDegree()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateThemeBossDegree(Owner.Uid, Degree));
        }

        public void SyncDbUpdateThemeBossInfo()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateThemeBossRewardInfo(Owner.Uid, Level, Degree, GetRewardedDegreeStr()));
        }

        public void SyncRedisUpdateThemeBossRank()
        {
            int score = Level * 100000 + (int)(Degree * 100);
            int maxScore = ThemeBossLibrary.GetThemeBossMaxLevel(Period) * 100000 + ThemeBossLibrary.MaxDegree * 100;
            if (maxScore < score)
            {
                score = maxScore;
            }
            Owner.server.GameRedis.Call(new OperateUpdateRankScore(RankType.ThemeBoss, Owner.server.MainId, Owner.Uid, score, Owner.server.Now()));

            Owner.server.TrackingLoggerMng.RecordRealtionRankLog(Owner.Uid, score, score, 0, RankType.ThemeBoss.ToString(), Owner.server.MainId, 0, Owner.server.Now());
        }
        #endregion

        public void GenerateThemeBossTransformMsg(ZMZ_THEME_INFO info)
        {
            info.ThemeBossInfo = new ZMZ_THEMEBOSS_INFO() { Period = Period, Level = Level, Degree = Degree};
            info.ThemeBossInfo.Rewarded.AddRange(rewardedDegreeList);
        }

        public void LoadThemeBossInfoTransform(ZMZ_THEMEBOSS_INFO themeBossInfo)
        {
            period = themeBossInfo.Period;
            level = themeBossInfo.Level;
            degree = themeBossInfo.Degree;
            rewardedDegreeList.AddRange(themeBossInfo.Rewarded);
        }
    }
}

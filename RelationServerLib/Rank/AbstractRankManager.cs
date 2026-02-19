using EnumerateUtility;
using Logger;
using RedisUtility;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public abstract class AbstractRankManager : ILoadConfig
    {
        private RankType rankType;

        protected int PeriodCount;
        protected int NowShowPeriod;
        protected DateTime hisBegin = RelationServerApi.now;
        protected DateTime hisEnd = RelationServerApi.now;
        protected DateTime begin = RelationServerApi.now;
        protected DateTime end = RelationServerApi.now;

        protected bool Loaded = false;

        //protected DateTime now=DateTime.Now;

        //public RankStageType RankStage = RankStageType.None;
        public RankType RankType { get { return rankType; } }

        private RankConfigInfo config = null;//配置信息，配置更新策略，每次重新加载xml就重新加载，需要一个总manager替它更新
        public RankConfigInfo Config { get { return config; } }

        protected SortedDictionary<int, int> rankUids = new SortedDictionary<int, int>();
        protected Dictionary<int, int> uidRanks = new Dictionary<int, int>();

        protected SortedDictionary<int, ulong> rankScores = new SortedDictionary<int, ulong>();

        #region history

        protected SortedDictionary<int, int> historyRankUids = new SortedDictionary<int, int>();

        protected SortedDictionary<int, ulong> historyRankScores = new SortedDictionary<int, ulong>();

        protected List<int> reusedUids = new List<int>();
        protected List<int> updateUids = new List<int>();
        protected HashSet<int> reusedUidsSet = new HashSet<int>();
        protected HashSet<int> updateUidsSet = new HashSet<int>();

        #endregion

        protected void Clear()
        {
            rankUids.Clear();
            uidRanks.Clear();
            rankScores.Clear();
            historyRankUids.Clear();
            historyRankScores.Clear();
            reusedUids.Clear();
            updateUids.Clear();
            reusedUidsSet.Clear();
            updateUidsSet.Clear();
        }

        //protected TimeSpan updateSpan = new TimeSpan(600);//默认60秒

        //protected TimeSpan updateClientsSpan = new TimeSpan(36000);//默认3600秒

        protected DateTime nextUpdate = RelationServerApi.now;

        protected DateTime nextUpdateClients = RelationServerApi.now;

        protected RelationServerApi server;
        protected int mainId;


        public AbstractRankManager(RelationServerApi api,int zoneId,RankType type)
        {
            server = api;
            mainId = zoneId;
            rankType = type;
            config = RankLibrary.GetConfig(RankType);
            if (config == null)
            {
                Logger.Log.Warn($"init rank manager rankType {rankType} can not find config,check rank.xml");
            }

            api.ConfigReloadMng.Add(this);
        }

        public void UpdateRank(SortedSetEntry[] entrys)
        {
            historyRankUids = rankUids;
            historyRankScores = rankScores;
            rankUids = new SortedDictionary<int, int>();
            uidRanks = new Dictionary<int, int>();
            rankScores = new SortedDictionary<int, ulong>();
            int count = 0;
            foreach(var item in entrys)
            {
                if (item.Element.IsNullOrEmpty)
                {
                    continue;
                }
                int uid = (int)item.Element;
                ulong score = (ulong)item.Score;
                count++;
                rankUids.Add(count, uid);
                uidRanks.Add(uid, count);
                rankScores.Add(count, score);
            }
            updateUids = rankUids.Values.Except(historyRankUids.Values).ToList();
            reusedUids = historyRankUids.Values.Except(rankUids.Values).ToList();
            reusedUidsSet = new HashSet<int>(reusedUids);
        }

        public abstract void UpdateClients(); //具体更新方式，需要根据当前的rank类型写

        protected abstract void LoadPeriodFromRedis();

        public abstract void Update();

        public abstract void ReStartRank();

        //public void CheckAndUpdateStage()
        //{
        //    DateTime now = DateTime.Now;
        //    if (RankStage == RankStageType.Waited)
        //    {
        //        if (now > begin)
        //        {
        //            RankStage = RankStageType.Started;
        //        }
        //    }
        //    else if (RankStage == RankStageType.Started)
        //    {
        //        if (now > hisEnd && now<begin)
        //        {
        //            RankStage = RankStageType.Waited;
        //        }
        //    }
        //}

        protected bool CheckUpdateRank()
        {
            if (RelationServerApi.now > nextUpdate)
            {
                nextUpdate = RelationServerApi.now + Config.RankUpdateTimeSpan;
                return true;
            }
            return false;
        }
        protected bool CheckUpdateClients()
        {
            if (RelationServerApi.now > nextUpdateClients)
            {
                nextUpdateClients = RelationServerApi.now + Config.InfoUpdateTimeSpan;
                return true;
            }
            return false;
        }

        public void LoadConfig()
        {
            config = RankLibrary.GetConfig(RankType);
        }

    }

    public enum RankStageType
    {
        None=0,
        Waited = 1,//
        Started = 2,//
        Closed = 3,
        Starting = 4,
    }
}

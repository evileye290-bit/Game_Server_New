using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Monster;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{

    public partial class CampActivityManager
    {
        public Dictionary<CampType, Dictionary<RankType, CampRank>> RankDic = new Dictionary<CampType, Dictionary<RankType, CampRank>>();

        public void InitRankList()
        {
            Dictionary<RankType, CampRank> list1 = new Dictionary<RankType, CampRank>();
            list1[RankType.CampBattleScore] = new CampRank(server, CampType.TianDou, RankType.CampBattleScore);
            //list1[RankType.CampBattleFight] = new CampRank(server, CampType.TianDou, RankType.CampBattleFight);
            list1[RankType.CampBattleCollection] = new CampRank(server, CampType.TianDou, RankType.CampBattleCollection);
            list1[RankType.CampBuild] = new CampRank(server, CampType.TianDou, RankType.CampBuild);
            RankDic[CampType.TianDou] = list1;

            Dictionary<RankType, CampRank> list2 = new Dictionary<RankType, CampRank>();
            list2[RankType.CampBattleScore] = new CampRank(server, CampType.XingLuo, RankType.CampBattleScore);
            //list2[RankType.CampBattleFight] = new CampRank(server, CampType.XingLuo, RankType.CampBattleFight);
            list2[RankType.CampBattleCollection] = new CampRank(server, CampType.XingLuo, RankType.CampBattleCollection);
            list2[RankType.CampBuild] = new CampRank(server, CampType.XingLuo, RankType.CampBuild);
            RankDic[CampType.XingLuo] = list2;

            LoadRankListByRedis();
        }

        private void LoadRankListByRedis()
        {
            foreach (var kv in RankDic)
            {
                foreach (var item in kv.Value)
                {
                    item.Value.Init();
                }
            }
        }

        public CampRank GetCampRank(CampType campType, RankType rankType)
        {
            CampRank camp = null;
            Dictionary<RankType, CampRank> list;
            if (RankDic.TryGetValue(campType, out list))
            {
                list.TryGetValue(rankType, out camp);
            }
            return camp;
        }

        #region 清空排行榜
        public void ResetRankList()
        {
            ClearRedis();
            ClearDb();
        }

        private void ClearRedis()
        {
            foreach (var kv in RankDic)
            {
                foreach (var item in kv.Value)
                {
                    if (item.Key == RankType.CampBuild)
                    {

                    }
                    else
                    {
                        item.Value.Clear();
                    }
                }
            }
        }

        private void ClearDb()
        {
            server.GameDBPool.Call(new QueryCampBattleClear(RankType.CampBattleScore));
            server.GameDBPool.Call(new QueryCampBattleClear(RankType.CampBattleCollection));
            //广播
            MSG_RZ_CLEAR_CAMP_BATTLE_SCORE msg = new MSG_RZ_CLEAR_CAMP_BATTLE_SCORE();
            server.ZoneManager.Broadcast(msg);
        }

        public void ResetCampBuildRankList()
        {
            foreach (var kv in RankDic)
            {
                foreach (var item in kv.Value)
                {
                    if (item.Key == RankType.CampBuild)
                    {
                        item.Value.Clear();
                    }
                }
            }
            server.GameDBPool.Call(new QueryCampBattleClear(RankType.CampBuild));
        }


        public void ResetCampBuildCounter()
        {
            CounterModel count = CounterLibrary.GetCounterModel(CounterType.CampBuildRefreshDiceCount);
            server.GameDBPool.Call(new QueryCampBuildCounter(count.MaxCount));

            MSG_RZ_RESET_CAMP_BUILD_COUNTER msg = new MSG_RZ_RESET_CAMP_BUILD_COUNTER();
            msg.Counter = count.MaxCount;
            server.ZoneManager.Broadcast(msg);
        }

        #endregion






    }



}

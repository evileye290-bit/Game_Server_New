using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class BattlePowerRank : BaseRank
    {
        public BattlePowerRank(RelationServerApi server, RankType rankType = RankType.BattlePower) : base(server, rankType)
        {
        }

        public void ChangeBattlePower(int uid, int battlePower)
        {
            if (uidRankInfoDic.ContainsKey(uid))
            {
                //在榜单中，刷新排行榜
                LoadInitBattlePowerRankFromRedis();
                return;
            }
            else
            {
                //不在榜单中判断是否能进榜
                foreach (var kv in uidRankInfoDic)
                {
                    if (battlePower > kv.Value.Score)
                    {
                        //说明可以进榜
                        LoadInitBattlePowerRankFromRedis();
                        return;
                    }
                }
            }
            //Dictionary<int, RankBaseModel> oldRankList = new Dictionary<int, RankBaseModel>();
            //foreach (var kv in uidRankInfoDic)
            //{
            //    oldRankList.Add(kv.Key, kv.Value);
            //}

            ////新
            //RankBaseModel rankItem = new RankBaseModel();
            //rankItem.Uid = uid;
            //rankItem.Rank = 0;
            //rankItem.Score = battlePower;
            //oldRankList[rankItem.Uid] = rankItem;
            ////排序
            //oldRankList = oldRankList.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);

            //Dictionary<int, RankBaseModel> newRankList = new Dictionary<int, RankBaseModel>();
            //RankBaseModel tempRank;
            //int i = 0;
            //foreach (var kv in oldRankList)
            //{
            //    i++;
            //    if (i <= configInfo.ShowCount)
            //    {
            //        kv.Value.Rank = i;
            //        if (uidRankInfoDic.TryGetValue(kv.Value.Uid, out tempRank))
            //        {
            //            //说明当前有
            //            if (tempRank.Rank != i)
            //            {
            //                //排名变更
            //                UpdateCrossRank(kv.Value.Uid, kv.Value.Rank);
            //            }
            //            else
            //            {
            //                if (tempRank.Uid == uid)
            //                {
            //                    //排名变更
            //                    UpdateCrossRank(kv.Value.Uid, kv.Value.Rank);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //没有，直接通知
            //            UpdateCrossRank(kv.Value.Uid, kv.Value.Rank);
            //        }
            //        newRankList.Add(kv.Value.Uid, kv.Value);
            //    }
            //    else
            //    {
            //        //出榜了
            //        kv.Value.Rank = 0;
            //        UpdateCrossRank(kv.Value.Uid, kv.Value.Rank);
            //    }
            //}
            //uidRankInfoDic = newRankList;
        }
    }
}

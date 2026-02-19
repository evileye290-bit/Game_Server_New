using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
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
    public class RankReward
    {
        protected RelationServerApi server;

        private Dictionary<RankType, Dictionary<int, int>> rewardList = new Dictionary<RankType, Dictionary<int, int>>();      
        public RankReward(RelationServerApi server)
        {
            this.server = server;
        }
        public void Init()
        {
            LoadRankRewardFromRedis(RankType.BattlePower);
            LoadRankRewardFromRedis(RankType.Arena);
            LoadRankRewardFromRedis(RankType.SecretArea);
            LoadRankRewardFromRedis(RankType.PushFigure);
            LoadRankRewardFromRedis(RankType.Hunting);
            LoadRankRewardFromRedis(RankType.CrossServer);
            LoadRankRewardFromRedis(RankType.CrossChallenge);
        }

        private void LoadRankRewardFromRedis(RankType rankType)
        {
            OperateGetRankReward op = new OperateGetRankReward(rankType, server.MainId);
            server.GameRedis.Call(op, ret =>
            {
                rewardList[rankType] = op.uidRank;
            });
        }

        public Dictionary<int, int> GetRewardList(RankType rankType)
        {
            Dictionary<int, int> dic;
            rewardList.TryGetValue(rankType, out dic);
            return dic;
        }



        public List<int> CheckNewRankReward(RepeatedField<int> ids)
        {
            List<int> list = new List<int>();
            foreach (var item in rewardList)
            {
                foreach (var kv in item.Value)
                {
                    if (!ids.Contains(kv.Key))
                    {
                        list.Add((int)item.Key);
                        break;
                    }
                }
            }
            return list;
        }

        public Dictionary<int, int> CheckNewRankRewardByPage(RepeatedField<ZR_PAGE_RANK_REWARD> rewards)
        {
            Dictionary<int, ZR_PAGE_RANK_REWARD> gotRewards = new Dictionary<int, ZR_PAGE_RANK_REWARD>();
            foreach (var item in rewards)
            {
                gotRewards.Add(item.RankType, item);
            }
            Dictionary<int, int> list = new Dictionary<int, int>();//需要通知前端的rankType
            foreach (var item in rewardList)
            {
                Dictionary<int, int> dic = rewardList[item.Key].OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
                List<int> tempList = new List<int>();//rankType对应的所有rewardId
                foreach (var kv in dic)
                {
                    tempList.Add(kv.Key);
                }
                int page = 1;
                if (rewards.Count != 0)
                {
                    ZR_PAGE_RANK_REWARD reward;
                    if (gotRewards.TryGetValue((int)item.Key, out reward) && reward.Page > 0)
                    {
                        int count = Math.Min(tempList.Count, RankLibrary.RewardPageCount * reward.Page);
                        for (int i = 0; i < count; i++)
                        {
                            if (!reward.Ids.Contains(tempList[i]))
                            {
                                int NewRewardCount = i + 1;
                                if (NewRewardCount % RankLibrary.RewardPageCount > 0)
                                {
                                    page = NewRewardCount / RankLibrary.RewardPageCount + 1;
                                }
                                else
                                {
                                    page = NewRewardCount / RankLibrary.RewardPageCount;
                                }
                                list.Add(reward.RankType, page);
                                break;
                            }
                        }
                        if (!list.TryGetValue(reward.RankType, out page))
                        {
                            page = (count - 1) / RankLibrary.RewardPageCount + 1;
                            list.Add(reward.RankType, page);
                        }
                    }
                    else
                    {
                        list.Add((int)item.Key, page);
                    }
                }
                else
                {
                    list.Add((int)item.Key, page);
                }
            }
            return list;
        }

        public void UpdatePlayerInfos()
        {
            List<int> list = new List<int>();
            foreach (var item in rewardList)
            {
                foreach (var kv in item.Value)
                {
                    if (!list.Contains(kv.Value))
                    {
                        list.Add(kv.Value);
                    }
                }
            }
            server.RPlayerInfoMng.RefreshPlayerList(list);
        }

        public void CheckAdd(int uid, RankType rankType, int value)
        {
            RankRewardModel info = Check(rankType, value);
            if (info != null)
            {
                server.GameRedis.Call(new OperateUpdateRankReward(rankType, server.MainId, uid, info.Id));

                Dictionary<int, int> dic = GetRewardList(rankType);
                dic.Add(info.Id, uid);

                MSG_RZ_NEW_RANK_REWARD msg = new MSG_RZ_NEW_RANK_REWARD();
                msg.List.Add(new RZ_NEW_RANK_REWARD() { RankType = (int)rankType, ShowRedPoint = true });
                server.ZoneManager.Broadcast(msg);
            }
        }

        public RankRewardModel Check(RankType rankType, int value)
        {
            Dictionary<int, int> dic = GetRewardList(rankType);
            if (dic == null)
            {
                return null;
            }
            List<int> list = RankLibrary.GetRewardList(rankType);
            if (list == null)
            {
                return null;
            }
            foreach (var id in list)
            {
                if (dic.ContainsKey(id))
                {
                    continue;
                }

                RankRewardModel info = RankLibrary.GetReward(id);
                if (info != null)
                {
                    if (value >= info.ConditionValue)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        public int CheckNotifyRankReward(int rankType, RepeatedField<int> ids)
        {
            Dictionary<int, int> rewardDic;
            if (rewardList.TryGetValue((RankType)rankType, out rewardDic))
            {
                foreach (var kv in rewardDic)
                {
                    if (!ids.Contains(kv.Key))
                    {
                        return rankType;
                    }
                }
            }
            return 0;
        }
    }
}

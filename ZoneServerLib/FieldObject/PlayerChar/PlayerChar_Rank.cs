using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
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
    public partial class PlayerChar
    {
        public List<int> RankRewardList = new List<int>();
        private Dictionary<RankType, List<int>> allRewardList = new Dictionary<RankType, List<int>>();

        public void InitRankReward(List<int> list)
        {
            RankRewardList = list;
            InitAllRewardList();
        }

        private void InitAllRewardList()
        {
            Dictionary<RankType, List<int>> rewardList = RankLibrary.GetAllRewardList();
            foreach (var kv in rewardList)
            {
                foreach (var id in RankRewardList)
                {
                    if (kv.Value.Contains(id))
                    {
                        List<int> list;
                        if (allRewardList.TryGetValue(kv.Key, out list))
                        {
                            allRewardList[kv.Key].Add(id);
                        }
                        else
                        {
                            list = new List<int>();
                            list.Add(id);
                            allRewardList.Add(kv.Key, list);
                        }
                    }
                }
            }
        }

        public void CheckNewRankRewards()
        {
            MSG_ZR_CHECK_NEW_RANK_REWARD msg = new MSG_ZR_CHECK_NEW_RANK_REWARD();
            msg.Ids.AddRange(RankRewardList);
            server.RelationServer.Write(msg, uid);
        }

        public void GetRankRewardInfos(int rankType, int page)
        {
            MSG_ZR_RANK_REWARD_LIST msg = new MSG_ZR_RANK_REWARD_LIST();
            msg.RankType = rankType;
            msg.Page = page;
            msg.Count = RankLibrary.RewardPageCount;
            server.RelationServer.Write(msg, uid);
        }

        public void GetRankReward(int rankType, int id)
        {
            RankRewardModel info = RankLibrary.GetReward(id);
            if (info == null)
            {
                Log.Warn("player {0} GetRankReward id {1} error: not find ", Uid, id);
                return;
            }

            if (!RankRewardList.Contains(id))
            {
                MSG_ZR_GET_RANK_REWARD msg = new MSG_ZR_GET_RANK_REWARD();
                msg.RankType = rankType;
                msg.Id = id;
                server.RelationServer.Write(msg, uid);
            }
            else
            {
                MSG_ZGC_GET_RANK_REWARD msg = new MSG_ZGC_GET_RANK_REWARD();
                msg.RankType = rankType;
                msg.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} GetRankReward id {1} error: already got reward", Uid, id);
                Write(msg);
            }
        }

        public bool CheckRankRewardState(int id)
        {
            return RankRewardList.Contains(id);
        }

        public void UpdateRankRewardState(int rankType, int id)
        {
            RankRewardList.Add(id);
            AddRewardIdToAllRewardList(rankType,id);

            //保存DB
            SyncDbInsertRankRewardid(id);
        }

        public void SyncDbInsertRankRewardid(int Id)
        {
            server.GameDBPool.Call(new QueryInsertRankReward(Uid, Id));
        }

        public void SendNewRankRewardMsg(int rankType, bool showRedPoint)
        {
            MSG_ZGC_NEW_RANK_REWARD msg = new MSG_ZGC_NEW_RANK_REWARD();
            msg.RankType = rankType;
            msg.ShowRedPoint = showRedPoint;
            Write(msg);
        }

        public void SerndUpdateRankValue(RankType rankType, int value)
        {
            MSG_ZR_UPDATE_RANK_VALUE msg = new MSG_ZR_UPDATE_RANK_VALUE();
            msg.RankType = (int)rankType;
            msg.Value = value;
            server.RelationServer.Write(msg, uid);
        }

        public void ChangeRankScore(RankType rankType, int score)
        {
            //加到redis
            server.GameRedis.Call(new OperateUpdateRankScore(rankType, server.MainId, Uid, score, server.Now()));
            server.TrackingLoggerMng.RecordRealtionRankLog(Uid, score, score, 0, rankType.ToString(), server.MainId, 0, server.Now());

            MSG_ZR_ADD_RANK_SCORE msg = new MSG_ZR_ADD_RANK_SCORE();
            msg.Camp = (int)Camp;
            msg.RankType = (int)rankType;
            msg.Score = score;
            server.SendToRelation(msg, Uid);
        }

        public int GetRankRewardPageById(int rankType, int rewardId)
        {
            Dictionary<RankType, List<int>> rewardList = RankLibrary.GetAllRewardList();
            List<int> list;
            rewardList.TryGetValue((RankType)rankType, out list);
            for (int i = 0; i < list.Count; i++)
            {
                if (rewardId == list[i])
                {
                    return i / RankLibrary.RewardPageCount + 1;
                }
            }
            return 1;
        }

        public void NotifyRankRewardPage()
        {
            MSG_ZR_RANK_REWARD_PAGE msg = new MSG_ZR_RANK_REWARD_PAGE();
            foreach (var kv in allRewardList)
            {
                ZR_PAGE_RANK_REWARD reward = new ZR_PAGE_RANK_REWARD();
                int page = kv.Value.Count / RankLibrary.RewardPageCount + 1;
                reward.RankType = (int)kv.Key;
                reward.Page = page;
                foreach (var id in kv.Value)
                {
                    reward.Ids.Add(id);
                }
                msg.PageRewards.Add(reward);
            }
            server.RelationServer.Write(msg, uid);
        }

        public void SendRankRewarPagedMsg(int rankType, int page)
        {
            MSG_ZGC_RANK_REWARD_PAGE msg = new MSG_ZGC_RANK_REWARD_PAGE();
            msg.RankType = rankType;
            msg.Page = page;
            Write(msg);
        }

        public void CheckNotifyRankReward(int rankType)
        {
            MSG_ZR_NOTIFY_RANK_REWARD msg = new MSG_ZR_NOTIFY_RANK_REWARD();
            msg.RankType = rankType;
            msg.Ids.AddRange(RankRewardList);
            server.RelationServer.Write(msg, uid);
        }

        private void AddRewardIdToAllRewardList(int rankType, int rewardId)
        {
            List<int> list;
            if (!allRewardList.TryGetValue((RankType)rankType, out list))
            {
                list = new List<int>();
                list.Add(rewardId);
                allRewardList.Add((RankType)rankType, list);
            }
            else
            {
                list.Add(rewardId);
            }
        }

        public void  CheckNotifyRankRewardPage()
        {
            MSG_ZR_RANK_REWARD_PAGE msg = new MSG_ZR_RANK_REWARD_PAGE();
            bool needSync = false;
            foreach (var kv in allRewardList)
            {
                ZR_PAGE_RANK_REWARD reward = new ZR_PAGE_RANK_REWARD();
                int page = kv.Value.Count / RankLibrary.RewardPageCount + 1;
                reward.RankType = (int)kv.Key;
                reward.Page = page;
                foreach (var id in kv.Value)
                {
                    reward.Ids.Add(id);
                }
                msg.PageRewards.Add(reward);
                if (kv.Value.Count % RankLibrary.RewardPageCount == 0)
                {
                    needSync = true;
                }
            }
            if (needSync)
            {
                server.RelationServer.Write(msg, uid);
            }
        }

        public void GetCrossRankReward(int rankType)
        {
            MSG_ZGC_GET_CROSS_RANK_REWARD response = new MSG_ZGC_GET_CROSS_RANK_REWARD();
            if (!CheckCanGetCrossRankReward(rankType, response))
            {
                //response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get cross rank {rankType} reward failed: errorcode {response.Result}");
                Write(response);
                return;
            }
            MSG_ZR_GET_CROSS_RANK_REWARD msg = new MSG_ZR_GET_CROSS_RANK_REWARD();
            msg.RankType = rankType;
            server.SendToRelation(msg, Uid);
        }

        private bool CheckCanGetCrossRankReward(int rankType, MSG_ZGC_GET_CROSS_RANK_REWARD response)
        {
            RechargeGiftModel activityModel;
            switch ((RankType)rankType)
            {        
                case RankType.CarnivalBoss:
                    if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.CarnivalBoss, ZoneServerApi.now, out activityModel))
                    {
                        response.Result = (int)ErrorCode.NotOnTime;
                        return false;
                    }
                    if (CarnivalBossMng.Info.GotRankReward == 1)
                    {
                        response.Result = (int)ErrorCode.AlreadyGot;
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public void GetCrossRankReward(int rankType, int rank)
        {
            MSG_ZGC_GET_CROSS_RANK_REWARD response = new MSG_ZGC_GET_CROSS_RANK_REWARD();
            response.RankType = rankType;

            if (rank <= 0)
            {
                response.Result = (int)ErrorCode.NotReachGetCondition;
                Log.Warn($"player {Uid} get cross rank reward failed: rank param error");
                Write(response);
                return;
            }

            switch ((RankType)rankType)
            {
                case RankType.CarnivalBoss:
                    GetCarnivalBossRankReward(rank, response);
                    break;
                default:
                    break;
            }

            Write(response);
        }

        public void RecordRankActiveInfo(string rankType, int firstUid, int firstValue, int luckyUid)
        {
            int group = CrossBattleLibrary.GetGroupId(server.MainId);
            List<int> groupServers = CrossBattleLibrary.GetGroupServers(group);
            string[] serverIdArr = null;
            if (groupServers != null)
            {
                serverIdArr = new string[groupServers.Count];
                for (int i = 0; i < groupServers.Count; i++)
                {
                    serverIdArr[i] = groupServers[i].ToString();
                }
            }
            BIRecordRankActiveLog(serverIdArr, rankType, firstUid, firstValue, luckyUid);
        }
    }
}

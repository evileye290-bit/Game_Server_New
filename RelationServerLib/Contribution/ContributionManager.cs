using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerModels.Contribution;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class ContributionManager
    {
        private int phaseNum;
        private int currentValue;
        private RelationServerApi server { get; set; }
        public ContributionManager(RelationServerApi server)
        {
            this.server = server;

            LoadContributionFromDB();
        }

        private void LoadContributionFromDB()
        {

            QueryLoadContribution query = new QueryLoadContribution();
            server.GameDBPool.Call(query, ret =>
            {
                if (query.phaseNum > 0)
                {
                    phaseNum = query.phaseNum;
                    currentValue = query.currentValue;
                }
                else
                {
                    phaseNum = 1;
                    currentValue = 0;
                    server.GameDBPool.Call(new QueryInsertContributionInfo(phaseNum));
                }

                SendContributionInfo(false);
            });
        }

        public void AddContribution(int uid, int value)
        {
            ContributionModel model = ContributionLibrary.GetContributionModel(phaseNum);
            if (model != null)
            {
                currentValue += value;

                if (currentValue >= model.MaxValue)
                {
                    //发送排行榜奖励
                    RankConfigInfo config = RankLibrary.GetConfig(RankType.Contribution);
                    if (config != null)
                    {
                        //进阶
                        phaseNum++;
                        currentValue -= model.MaxValue;
                        UpdateContributionToDB();

                        int addValue = currentValue;
                        OperateGetRankScore op = new OperateGetRankScore(RankType.Contribution, server.MainId, 0, config.ShowCount);
                        server.GameRedis.Call(op, ret =>
                        {
                            SendContributionRankReward(op.uidRank);
                            //清空排行榜
                            server.RankMng.ContributionRank.Clear();

                            UpdateContributionToRedis(uid, addValue);

                            SendContributionInfo(false);
                        });
                    }
                }
                else
                {
                    UpdateContributionToRedis(uid, value);
                    UpdateContributionToDB();
                    SendContributionInfo(false);
                }
            }
        }

        public void MergeServerReward()
        {
            ContributionModel model = ContributionLibrary.GetContributionModel(phaseNum);
            if (model == null) return;

            //发送排行榜奖励
            RankConfigInfo config = RankLibrary.GetConfig(RankType.Contribution);
            if (config != null)
            {
                OperateGetRankScore op = new OperateGetRankScore(RankType.Contribution, server.MainId, 0, config.ShowCount);
                server.GameRedis.Call(op, ret =>
                {
                    SendContributionRankReward(op.uidRank);
                    //清空排行榜
                    server.RankMng.ContributionRank.Clear();

                    SendContributionInfo(false);

                    Log.Warn("MergeServerReward Contribution reward success !");
                });
            }
        }

        private void UpdateContributionToRedis(int uid, int value)
        {
            server.GameRedis.Call(new OperateRankAddScore(RankType.Contribution, server.MainId, uid, value, server.Now()));
        }
        private void UpdateContributionToDB()
        {
            server.GameDBPool.Call(new QueryUpdateContribution(phaseNum, currentValue));
        }


        public void SendContributionInfo(bool getReward)
        {
            MSG_RZ_CONTRIBUTION_INFO res = new MSG_RZ_CONTRIBUTION_INFO();
            res.PhaseNum = phaseNum;
            res.CurrentValue = currentValue;
            res.GetReward = getReward;
            server.ZoneManager.Broadcast(res);
        }

        public void SendContributionRankReward(Dictionary<int, RankBaseModel> uidRankInfoDic)
        {
            if (uidRankInfoDic.Count > 0)
            {
                ContributionModel model = ContributionLibrary.GetContributionModel(phaseNum);
                if (model != null)
                {
                    List<string> rankInfoList = new List<string>();

                    foreach (var kv in uidRankInfoDic)
                    {
                        RankBaseModel info = kv.Value;
                        if (info != null)
                        {
                            int rewardId = model.GetRewardId(info.Rank);
                            if (rewardId > 0)
                            {
                                string reward = ContributionLibrary.GetContributionReward(rewardId);
                                if (!string.IsNullOrEmpty(reward))
                                {
                                    //发送邮件
                                    server.EmailMng.SendPersonEmail(info.Uid, EmailLibrary.ContributionEmail, reward);
                                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.Contribution.ToString(), info.Rank, info.Score, info.Uid, server.Now());
                                    //BI
                                    server.KomoeEventLogRankFlow(info.Uid, RankType.Contribution, info.Rank, info.Rank, info.Score, RewardManager.GetRewardDic(reward));
                                }
                            }
                            rankInfoList.Add(info.Rank + "_" + info.Uid + "_" + info.Score);
                        }
                        else
                        {
                            Log.Warn("player {0} SendContributionRankReward {1} failed: not find reward info", info.Uid, kv.Key);
                        }
                    }
                    server.RankMng.BIRecordRankLog("contribution", phaseNum, rankInfoList);
                }
            }
        }    
    }
}

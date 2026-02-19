using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class DivineLoveManager
    {
        private CrossServerApi server { get; set; }
        private DivineLoveConfig config { get; set; }
        private Dictionary<int, int> currentValues = new Dictionary<int, int>();

        public DivineLoveManager(CrossServerApi server)
        {
            this.server = server;

            LoadDivineLoveValueFromRedis();

            config = DivineLoveLibrary.GetDivineLoveConfig(2);
        }

        protected void LoadDivineLoveValueFromRedis()
        {
            OperateGetDivineLoveValue op = new OperateGetDivineLoveValue();
            server.CrossRedis.Call(op, ret =>
            {
                currentValues = op.allInfo;
            });
        }

        public int GetCurrentValue(int groupId)
        {
            int value;
            currentValues.TryGetValue(groupId, out value);
            return value;
        }

        public RankBaseModel GetFirstValue(int groupId)
        {
            DivineLoveRank rank = server.RankMng.GetDivineLoveRank(groupId);
            if (rank != null)
            {
                //获取rank
                return rank.GetFirst();
            }
            else
            {
                return null;
            }
        }

        public RankBaseModel GetPlayerValue(int groupId, int uid)
        {
            DivineLoveRank rank = server.RankMng.GetDivineLoveRank(groupId);
            if (rank != null)
            {
                //获取rank
                return rank.GetRankBaseInfo(uid);
            }
            else
            {
                return null;
            }
        }

        public bool UpdatePlayerValue(int groupId, int uid, int value)
        {
            int currentValue = value;
            DivineLoveRank rank = server.RankMng.GetDivineLoveRank(groupId);
            if (rank == null)
            {
                server.RankMng.AddDivineLoveRank(groupId);
                //获取rank
                rank = server.RankMng.GetDivineLoveRank(groupId);
            }
            else
            {
                RankBaseModel rankItem = rank.GetRankBaseInfo(uid);
                if (rankItem != null)
                {
                    currentValue += rankItem.Score;
                }
            }


            int totalValue = GetCurrentValue(groupId);
            if (totalValue == config.MaxValue)
            {
                //说明达到满值进入下一期
                ChangeTotalValue(groupId, value);
                rank.Clear();

                rank.UpdateScore(uid, value, value);
            }
            else if (totalValue + value >= config.MaxValue)
            {
                //是幸运儿
                ChangeTotalValue(groupId, config.MaxValue);
                rank.UpdateScore(uid, currentValue, value);//
                //发第一名奖励奖励

                MSG_CorssR_BROADCAST_ANNOUNCEMENT announcementMsg = new MSG_CorssR_BROADCAST_ANNOUNCEMENT();
                announcementMsg.Type = (int)ANNOUNCEMENT_TYPE.DIVINE_LOVE_MAX_VALUE;


                OperateGetCrossRankScore op = new OperateGetCrossRankScore(RankType.DivineLove, groupId, 0, 0, 1);
                server.CrossRedis.Call(op, ret =>
                {
                    MSG_CorssR_RECORD_RANK_ACTIVE_INFO rankActiveMsg = new MSG_CorssR_RECORD_RANK_ACTIVE_INFO();
                    rankActiveMsg.RankType = "divineLove";

                    if (op.uidRank.Count > 0)
                    {
                        //发送奖励
                        RankBaseModel firstModel = op.uidRank.First().Value;
                        if (firstModel != null)
                        {
                            JsonPlayerInfo fitstPlayerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, firstModel.Uid);
                            if (fitstPlayerInfo != null)
                            {
                                int mainId = fitstPlayerInfo.MainId;
                                string name = fitstPlayerInfo.Name;
                                if (mainId > 0)
                                {
                                    MSG_CorssR_SEND_FINALS_REWARD msg = new MSG_CorssR_SEND_FINALS_REWARD();
                                    msg.MainId = mainId;
                                    msg.Uid = firstModel.Uid;
                                    msg.Reward = config.FirstReward;
                                    msg.EmailId = config.FirstEmail;
                                    server.RelationManager.WriteToRelation(msg, mainId);

                                    server.TrackingLoggerMng.RecordSendEmailRewardLog(msg.Uid, msg.EmailId, msg.Reward, msg.Param, msg.MainId, server.Now());
                                    server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.DivineLove.ToString(), firstModel.Uid, firstModel.Score, config.FirstEmail, 1, server.Now());
                                    //BI
                                    server.KomoeEventLogRankFlow(firstModel.Uid, mainId, RankType.DivineLove,1, 1, firstModel.Score, RewardManager.GetRewardDic(config.FirstReward));

                                }

                                announcementMsg.List.Add(mainId.ToString());
                                announcementMsg.List.Add(name);

                                rankActiveMsg.FirstUid = firstModel.Uid;
                                rankActiveMsg.FirstValue = firstModel.Score;
                            }
                        }
                    }

                    //发幸运儿奖励
                    RankBaseModel playerModel = GetPlayerValue(groupId, uid);
                    if (playerModel != null)
                    {
                        JsonPlayerInfo playerInfo = server.PlayerInfoMng.GetJsonPlayerInfo(groupId, playerModel.Uid);
                        if (playerInfo != null)
                        {
                            int mainId = playerInfo.MainId;
                            string name = playerInfo.Name;
                            if (mainId > 0)
                            {
                                MSG_CorssR_SEND_FINALS_REWARD msg = new MSG_CorssR_SEND_FINALS_REWARD();
                                msg.MainId = mainId;
                                msg.Uid = playerModel.Uid;
                                msg.Reward = config.luckilyReward;
                                msg.EmailId = config.luckilyEmail;
                                server.RelationManager.WriteToRelation(msg, mainId);
                                server.TrackingLoggerMng.TrackRankEmailLog(groupId, 0, RankType.DivineLove.ToString(), playerModel.Uid, playerModel.Score, config.luckilyEmail, 0, server.Now());
                                server.TrackingLoggerMng.RecordSendEmailRewardLog(msg.Uid, msg.EmailId, msg.Reward, msg.Param, msg.MainId, server.Now());
                                //BI
                                server.KomoeEventLogRankFlow(playerModel.Uid, mainId, RankType.DivineLove, playerModel.Rank, playerModel.Rank, playerModel.Score, RewardManager.GetRewardDic(config.luckilyReward));
                            }
                            announcementMsg.List.Add(mainId.ToString());
                            announcementMsg.List.Add(name);

                            rankActiveMsg.LuckyUid = playerModel.Uid;
                            server.RelationManager.WriteToRelation(rankActiveMsg, mainId, playerModel.Uid);
                        }
                    }

                    server.RelationManager.BroadcastToGroupRelation(announcementMsg, groupId);
                });
              
                return true;
            }
            else
            {
                ChangeTotalValue(groupId, totalValue + value);
                rank.UpdateScore(uid, currentValue, value);
            }
            return false;
        }

        private void ChangeTotalValue(int groupId, int value)
        {
            currentValues[groupId] = value;

            server.CrossRedis.Call(new OperateSetDivineLoveValue(groupId, value));
        }

        public void Clear()
        {
            currentValues.Clear();
            server.CrossRedis.Call(new OperateDeleteDivineLoveValue());
            server.RankMng.ClearDivineLoveRank();
            server.NotesMng.Clear(NotesType.DivineLove);

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.DivineLove;
            server.RelationManager.Broadcast(msg);
        }
    }
}

using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using ServerModels;
using System.Collections.Generic;
using System.Linq;
using RedisUtility;
using System;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GetCampBattleRankList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMPBATTLE_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMPBATTLE_RANK_LIST>(stream);
            RankType rankType = (RankType)msg.RankType;
            CampType camp = (CampType)msg.Camp;
            switch (rankType)
            {
                case RankType.CampBattlePower:
                case RankType.CampLeader:
                    {
                        RankListModel rankListModel = new RankListModel();
                        rankListModel.Camp = camp;
                        rankListModel.Type = rankType;
                        rankListModel.Page = 1;
                        rankListModel.TotalCount = 3;

                        List<PlayerRankBaseInfo> list = Api.CampRankMng.GetLeaderList(camp);
                        foreach (var player in list)
                        {
                            PlayerRankModel rankInfo = new PlayerRankModel();
                            rankInfo.RankInfo = new RankBaseModel();
                            rankInfo.RankInfo.Uid = player.Uid;
                            rankInfo.RankInfo.Rank = player.Rank;
                            rankInfo.RankInfo.Score = player.ShowValue;

                            RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(player.Uid);
                            if (baseInfo != null)
                            {
                                rankInfo.BaseInfo = baseInfo;
                            }
                            else
                            {
                                Log.Warn($"load rank list error: can not find {0} data in server", player.Uid);
                            }
                            rankListModel.RankList.Add(rankInfo);
                        }


                        Client client = Api.ZoneManager.GetClient(uid);
                        if (client == null)
                        {
                            Log.Warn($"player {uid} GetCampBattleRankList failed: not find client ");
                            return;
                        }

                        MSG_RZ_GET_RANK_LIST rankMsg = RankManager.PushRankListMsg(rankListModel, rankListModel.Type);
                        client.Write(rankMsg);
                    }
                    break;
                case RankType.CampBuildValue:
                    CampRank campRank1 = Api.CampActivityMng.GetCampRank(camp, RankType.CampBuild);
                    if (campRank1 == null)
                    {
                        Logger.Log.Warn($"player {uid} get camp battle rank list failed ");
                        return;
                    }
                    campRank1.PushRankList(uid, msg.Page, RankType.CampBuildValue);
                    break;
                default:
                    CampRank campRank = Api.CampActivityMng.GetCampRank(camp, rankType);
                    if (campRank == null)
                    {
                        Logger.Log.Warn($"player {uid} get camp battle rank list failed ");
                        return;
                    }
                    campRank.PushRankList(uid, msg.Page);
                    break;
            }
        }

        public void OnResponse_GetRankList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_RANK_LIST>(stream);
            RankType rankType = (RankType)msg.RankType;
            switch (rankType)
            {
                case RankType.BattlePower:
                    Api.RankMng.BattlePowerRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.SecretArea:
                    Api.RankMng.SecretAreaRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.PushFigure:
                    Api.RankMng.PushFigureRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.Hunting:
                    Api.RankMng.HuntingRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.Contribution:
                    Api.RankMng.ContributionRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.CrossServer:
                    Api.RankMng.CrossBattleRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.ThemeBoss:
                    Api.RankMng.ThemeBossRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.CrossChallenge:
                    Api.RankMng.CrossChallengeRank.PushRankList(uid, msg.Page);
                    break;
                case RankType.Garden:
                case RankType.CrossBoss:
                case RankType.CrossBossSite:
                case RankType.IslandHigh:
                case RankType.IslandHighCurrStage:
                case RankType.IslandHighLastStage:
                case RankType.CarnivalBoss:
                case RankType.Roulette:
                case RankType.Canoe:
                case RankType.MidAutumn:
                case RankType.ThemeFirework:
                case RankType.NineTest:
                    MSG_RC_GET_RANK_LIST request = new MSG_RC_GET_RANK_LIST();
                    request.RankType = msg.RankType;
                    request.Page = msg.Page;
                    request.ParamId = msg.ParamId;
                    Api.WriteToCross(request, uid);
                    break;
                default:
                    return;
            }
        }

        public void OnResponse_AddRankScore(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_RANK_SCORE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_RANK_SCORE>(stream);
            Log.Write("player {0} add  camp {1} rank {2} score {3}.", uid, pks.Camp, pks.RankType, pks.Score);

            CampType campType = (CampType)pks.Camp;
            RankType rankType = (RankType)pks.RankType;

            switch (rankType)
            {
                case RankType.CampBuild:
                    Api.CampActivityMng.AddCampBuildValue(campType, pks.Score);
                    MSG_RZ_CAMPBUILD_INFO msg = Api.CampActivityMng.GetCampBuildPhaseInfo(pks.Camp);
                    Write(msg, uid);
                    break;
                case RankType.CampBattleScore:
                    Api.CampActivityMng.AddCampBattleScore(campType, pks.Score);
                    break;
                case RankType.CrossServer:
                    Api.RankMng.CrossBattleRank.ChangeScore(uid, pks.Score);
                    break;
                case RankType.CrossChallenge:
                    Api.RankMng.CrossChallengeRank.ChangeScore(uid, pks.Score);
                    break;
                default:
                    break;
            }


            CampRank rankInfo = Api.CampActivityMng.GetCampRank(campType, rankType);
            if (rankInfo != null)
            {
                rankInfo.RefreshList(uid);
            }
            else
            {
                Log.Warn("player {0} add rank score not find camp {1} rank {2} score {3} ", uid, pks.Camp, pks.RankType, pks.Score);
                return;
            }
        }

        public void OnResponse_UpdateRankValue(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_RANK_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_RANK_VALUE>(stream);
            Log.Write("player {0} update rank {1} value {2}.", uid, pks.RankType, pks.Value);
            RankType rankType = (RankType)pks.RankType;
            switch (rankType)
            {
                case RankType.Contribution:
                    Api.ContributionMng.AddContribution(uid, pks.Value);
                    break;
                case RankType.BattlePower:
                    Api.RankMng.RankReward.CheckAdd(uid, rankType, pks.Value);

                    //更新缓存
                    RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
                    if (baseInfo != null)
                    {
                        baseInfo.SetValue(HFPlayerInfo.BattlePower, pks.Value);
                    }

                    Api.RankMng.BattlePowerRank.ChangeBattlePower(uid, pks.Value);
                    break;
                case RankType.Garden:
                case RankType.IslandHigh:
                case RankType.CarnivalBoss:
                case RankType.Roulette:
                case RankType.Canoe:
                case RankType.MidAutumn:
                case RankType.ThemeFirework:
                case RankType.NineTest:
                    MSG_RC_UPDATE_RANK_VALUE request = new MSG_RC_UPDATE_RANK_VALUE();
                    request.RankType = pks.RankType;
                    request.Value = pks.Value;
                    request.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(uid));

                    Api.WriteToCross(request, uid);
                    break;
                default:
                    Api.RankMng.RankReward.CheckAdd(uid, rankType, pks.Value);
                    break;
            }
        }

        public void OnResponse_CheckNewRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHECK_NEW_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHECK_NEW_RANK_REWARD>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} CheckNewRankReward failed: not find client ");
                return;
            }

            List<int> list = Api.RankMng.RankReward.CheckNewRankReward(pks.Ids);
            foreach (var rankType in list)
            {
                MSG_RZ_NEW_RANK_REWARD msg = new MSG_RZ_NEW_RANK_REWARD();
                msg.List.Add(new RZ_NEW_RANK_REWARD() { RankType = rankType, ShowRedPoint = true});
                client.Write(msg);
            }


            //检查数据刷新
            Api.RankMng.RankReward.UpdatePlayerInfos();
        }

        public void OnResponse_GetRankRewardList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_RANK_REWARD_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_RANK_REWARD_LIST>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} GetRankRewardList failed: not find client ");
                return;
            }

            OperateGetRankReward op = new OperateGetRankReward((RankType)pks.RankType, Api.MainId);
            Api.GameRedis.Call(op, ret =>
            {
                if (op.uidRank != null)
                {
                    MSG_RZ_RANK_REWARD_LIST msg = new MSG_RZ_RANK_REWARD_LIST();
                    msg.RankType = pks.RankType;
                    int page = pks.Page;
                    int start = 0;
                    int end = pks.Count;
                    msg.Page = page;
                    msg.Count = op.uidRank.Count;
                    if (page > 0 && (page - 1) * pks.Count < op.uidRank.Count)
                    {
                        start = (page - 1) * pks.Count;
                        end = Math.Min(page * pks.Count, op.uidRank.Count);
                    }
                    Dictionary<int, int> dic = op.uidRank.OrderBy(kv=>kv.Key).ToDictionary(kv=>kv.Key, kv=>kv.Value);
                    for (int i = start; i < end; i++)
                    {
                        if (dic.Count <= i)
                        {
                            break;
                        }
                        int rewardId = dic.ElementAt(i).Key;
                        int playerUid = dic.ElementAt(i).Value;
                        RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(playerUid);
                        if (baseInfo != null)
                        {
                            PlayerRankMsg item = new PlayerRankMsg();
                            item.BaseInfo.AddRange(RankManager.GetRankPlayerBaseInfoItem(baseInfo));
                            item.Rank = new RankBaseInfo();
                            item.Rank.Score = rewardId;
                            msg.RankList.Add(item);
                        }
                        else
                        {
                            Log.Warn($"player {0} GetRankRewardList error: can not find {1} data in server", uid, playerUid);
                        }
                    }           
                    client.Write(msg);

                    //检查数据刷新
                    Api.RPlayerInfoMng.RefreshPlayerList(op.uidRank.Values.ToList());
                }
            });
        }

        public void OnResponse_GetRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_RANK_REWARD>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} GetRankRewardList failed: not find client ");
                return;
            }

            MSG_RZ_GET_RANK_REWARD msg = new MSG_RZ_GET_RANK_REWARD();
            msg.RankType = pks.RankType;
            msg.Id = pks.Id;
            Dictionary<int, int> dic = Api.RankMng.RankReward.GetRewardList((RankType)pks.RankType);
            if (dic != null)
            {
                if (dic.ContainsKey(pks.Id))
                {
                    msg.Result = (int)ErrorCode.Success;
                }
                else
                {
                    msg.Result = (int)ErrorCode.Fail;
                }
                client.Write(msg);

                //检查数据刷新
                Api.RPlayerInfoMng.RefreshPlayerList(dic.Values.ToList());
            }
            else
            {
                msg.Result = (int)ErrorCode.Fail;
                client.Write(msg);
            }
        }

        public void OnResponse_GetRankRewardPage(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_RANK_REWARD_PAGE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_RANK_REWARD_PAGE>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} GetRankRewardPage failed: not find client ");
                return;
            }

            Dictionary<int, int> dic = Api.RankMng.RankReward.CheckNewRankRewardByPage(pks.PageRewards);
            if (dic.Count > 0)
            {
                MSG_RZ_RANK_REWARD_PAGE msg = new MSG_RZ_RANK_REWARD_PAGE();
                foreach (var kv in dic)
                {
                    msg.List.Add(new RZ_RANK_PAGE() { RankType = kv.Key, Page = kv.Value });
                }
                client.Write(msg);
            }      
        }

        public void OnResponse_NotifyRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_NOTIFY_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_NOTIFY_RANK_REWARD>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn($"player {uid} NotifyRankReward failed: not find client ");
                return;
            }

            int rankType = Api.RankMng.RankReward.CheckNotifyRankReward(pks.RankType, pks.Ids);
            if (rankType > 0)
            {
                MSG_RZ_NEW_RANK_REWARD msg = new MSG_RZ_NEW_RANK_REWARD();
                msg.List.Add(new RZ_NEW_RANK_REWARD() { RankType = pks.RankType, ShowRedPoint = true});
                client.Write(msg);
            }
            else
            {
                MSG_RZ_NEW_RANK_REWARD msg = new MSG_RZ_NEW_RANK_REWARD();
                msg.List.Add(new RZ_NEW_RANK_REWARD() { RankType = pks.RankType, ShowRedPoint = false });
                client.Write(msg);
            }
        }

        public void OnResponse_GetRankFirstInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_RANK_FIRST_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_RANK_FIRST_INFO>(stream);
            Log.Write("player {0} GetRankFirstInfo.", uid);
            MSG_RC_GET_RANK_FIRST_INFO msg = new MSG_RC_GET_RANK_FIRST_INFO();
            msg.RankType = pks.RankType;
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_GetCrossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CROSS_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_RANK_REWARD>(stream);

            Log.Write("player {0} GetCrossRankReward.", uid);
            MSG_RC_GET_RANK_REWARD msg = new MSG_RC_GET_RANK_REWARD();
            msg.RankType = pks.RankType;          
            Api.WriteToCross(msg, uid);
        }
    }
}

using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class RelationServer
    {

        #region camp

        public int CampRankPeriod = 1; //两个阵营的榜均会来更新一下
        public DateTime CampRankBegin; 
        public DateTime CampRankEnd;

        public int ElectionPeriod = 1;
        public DateTime ElectionRankBegin;
        public DateTime ElectionRankEnd;

        public Dictionary<int, WorshipRedisInfo> TianDouWorship = new Dictionary<int, WorshipRedisInfo>();
        public Dictionary<int, WorshipRedisInfo> XingLuoWorship = new Dictionary<int, WorshipRedisInfo>();

        #endregion

        public void AskForRankPeriodInfos()
        {
            MSG_ZR_ASK_RANK_PERIOD msg = new MSG_ZR_ASK_RANK_PERIOD();
            Write(msg);
        }


        public void LoadCampElectionInfoFromRedisAndSendUpdateMsg()
        {
            OperateGetWorshipInfo tiandou = new OperateGetWorshipInfo(MainId, (int)CampType.TianDou);
            OperateGetWorshipInfo xingluo = new OperateGetWorshipInfo(MainId, (int)CampType.XingLuo);

            //天斗
            Api.GameRedis.Call(tiandou, ret =>
            {
                int count = tiandou.Infos.Count;
                if (count > 0)
                {
                    foreach(var item in tiandou.Infos)
                    {
                        TianDouWorship[item.Rank] = item;
                    }
                }
                if (3 - count > 0)
                {
                    for(int i = count + 1; i <= 3; i++)
                    {
                        WorshipRedisInfo info = new WorshipRedisInfo();
                        WorshipBase temp = CampLibrary.GetWorshipBase(i);
                        if (temp != null)
                        {
                            info.Rank = i;
                            info.Name = temp.Name;
                            info.ModelId = temp.ModelId;
                            info.HeroId = temp.HeroId;
                            info.Uid = 0;
                            info.BattlePower = temp.BattlePower;
                            info.Icon = temp.Icon;
                            info.Level = temp.Level;
                            TianDouWorship[i] = info;
                        }
                    }
                }

                if (TianDouWorship.Count >= 3)
                {
                    foreach (var item in Api.PCManager.PcList)
                    {
                        item.Value.SyncWorshipShowMsg(CampType.TianDou);
                    }
                }
            });

            //星罗
            Api.GameRedis.Call(xingluo, ret =>
            {
                int count = xingluo.Infos.Count;
                if (count > 0)
                {
                    foreach (var item in xingluo.Infos)
                    {
                        XingLuoWorship[item.Rank] = item;
                    }
                }
                if (3 - count > 0)
                {
                    for (int i = count + 1; i <= 3; i++)
                    {
                        WorshipRedisInfo info = new WorshipRedisInfo();
                        WorshipBase temp = CampLibrary.GetWorshipBase(i);
                        if (temp != null)
                        {
                            info.Rank = i;
                            info.Name = temp.Name;
                            info.ModelId = temp.ModelId;
                            info.HeroId = temp.HeroId;
                            info.Uid = 0;
                            XingLuoWorship[i] = info;
                        }
                    }
                }
                if (XingLuoWorship.Count>=3)
                {
                    foreach (var item in Api.PCManager.PcList)
                    {
                        item.Value.SyncWorshipShowMsg(CampType.XingLuo);
                    }
                }
            });
        }

        //第一周期的内容走默认内容，之后起服总是从redis加载，防止服务重启后数据异常
        //public void LoadCampInfoFromRedis()
        //{
        //    //只需要一次即可
        //    OperateGetCampRankPeriodInfo op = new OperateGetCampRankPeriodInfo(MainId, 1);//只需要随意选择一个
        //    api.Redis.Call(op, ret =>
        //    {
        //        if (op.info != null)
        //        {
        //            CampRankBegin = op.info.begin;
        //            CampRankEnd = op.info.end;
        //            CampRankPeriod = op.info.Period;
        //        }
        //    });
        //}

        //public Dictionary<int, RankPeriod> rankPeriodDic = new Dictionary<int, RankPeriod>();

        private void OnResponse_UpdateCampRankPeriod(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_CAMP_RANK_PERIOD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_CAMP_RANK_PERIOD>(stream);
            CampRankPeriod = msg.CurPeriod;
            CampRankBegin = DateTime.Parse(msg.Begin);
            CampRankEnd = DateTime.Parse(msg.End);
            Log.Write($" update camp rank {(RankType)msg.RankType} period with CurPeriod {CampRankPeriod} Begin {CampRankBegin} End {CampRankEnd}");
        }

        //Todo 重构，所有的榜单周期信息维护在一个dic中
        //private void OnResponse_UpdateRankPeriod(MemoryStream stream,int uid = 0)
        //{
        //    MSG_RZ_UPDATE_RANK_PERIOD msg= MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_RANK_PERIOD>(stream);
        //    RankPeriod rp = null;
        //    if(rankPeriodDic.TryGetValue(msg.RankType,out rp))
        //    {
        //        rp.Period = msg.CurPeriod;
        //        rp.Type = (RankType)msg.RankType;
        //        rp.Start = DateTime.Parse(msg.Begin);
        //        rp.End = DateTime.Parse(msg.End);
        //    }
        //    else
        //    {
        //        rp = new RankPeriod();
        //        rp.Period = msg.CurPeriod;
        //        rp.Type = (RankType)msg.RankType;
        //        rp.Start = DateTime.Parse(msg.Begin);
        //        rp.End = DateTime.Parse(msg.End);
        //        rankPeriodDic.Add((int)rp.Type, rp);
        //    }
        //}

        private void OnResponse_UpdateElectionPeriod(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD>(stream);
            //ElectionPeriod = msg.CurPeriod;
            //ElectionRankBegin = DateTime.Parse(msg.Begin);
            //ElectionRankEnd = DateTime.Parse(msg.End);
            LoadCampElectionInfoFromRedisAndSendUpdateMsg();
            //Log.Write($" update camp election rank period with CurPeriod {CampRankPeriod} Begin {CampRankBegin} End {CampRankEnd}");
        }

        private void OnResponse_ChooseCamp(MemoryStream stream,int uid = 0)
        {
            MSG_RZ_WEAK_CAMP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_WEAK_CAMP>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.SendRandomChooseCampResult(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("choose camp info fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("choose camp fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_ChangeCamp(MemoryStream stream,int uid = 0)
        {
            MSG_RZ_CHANGE_CAMP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHANGE_CAMP>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ChooseCampWithRankCheck(msg.Allowed,msg.CampId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("check change camp info fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("check change choose camp panel info fail, can not find player {0} .", msg.PcUid);
                }
            }
        }



        private void OnResponse_CampPanelInfo(MemoryStream stream,int uid = 0)
        {
            MSG_RZ_CAMP_PANEL_LIST msg= MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_PANEL_LIST>(stream);

            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.SendCampPanelInfo(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("send camp panel info fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("send camp panel info fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_CampRankInfo(MemoryStream stream,int uid = 0)
        {
            MSG_RZ_CAMP_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_RANK_LIST>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.SendCampRankInfo(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("send camp rank info fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("send camp rank info fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_CampElectionInfo(MemoryStream stream,int uid = 0)
        {
            MSG_RZ_CAMP_ELECTION_LIST msg= MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_ELECTION_LIST>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.SendCampElectionInfo(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("send camp election info fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("send camp election info fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

 
        //private void OnResponse_PopRankRefresh(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_POP_RANK_REFRESH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_POP_RANK_REFRESH>(stream);
        //    Log.Warn("relation request refresh pop rank");
        //    Api.PopRankMng.LoadPopRank();
        //}

        //private void OnResponse_PopRankClear(MemoryStream stream, int uid = 0)
        //{ 
        //    Log.Warn("relation request clear pop rank");
        //    Api.PopRankMng.ClearPopRank();
        //    foreach (var item in Api.PCManager.PcList)
        //    {
        //        item.Value.PopScore = 0;
        //    }
        //    foreach (var item in Api.PCManager.PcOfflineList)
        //    {
        //        item.Value.PopScore = 0;
        //    }
        //}

   

        public void OnResponse_RankingAllList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RANKING_ALL_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RANKING_ALL_LIST>(stream);
            Log.Info("got relation ranking all list count {0} page {1}", msg.Ids.Count, msg.Page);
            //Api.SeasonMng.UpdateAllRankingList(msg.Page, msg.Ids);
        }

        private void OnResponse_SecretAreaRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SECRET_AREA_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SECRET_AREA_RANK_LIST>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SendSecretAreaRankInfo(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("send secret area rank info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("send secret area rank info fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_CheckNewRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NEW_RANK_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEW_RANK_REWARD>(stream);
            if (uid == 0)
            {
                foreach (var item in msg.List)
                {
                    MSG_ZGC_NEW_RANK_REWARD pks = new MSG_ZGC_NEW_RANK_REWARD();
                    pks.RankType = item.RankType;
                    pks.ShowRedPoint = item.ShowRedPoint;
                    Api.PCManager.Broadcast(pks);
                }
            }
            else
            {
                PlayerChar player = Api.PCManager.FindPc(uid);
                if (player != null)
                {
                    foreach (var item in msg.List)
                    {
                        player.SendNewRankRewardMsg(item.RankType, item.ShowRedPoint);
                    }
                }
                else
                {
                    Log.WarnLine("player {0} check new rank reward fail, can not find player.", uid);
                }
            }
        }

        private void OnResponse_GetRankRewardList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RANK_REWARD_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RANK_REWARD_LIST>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZGC_RANK_REWARD_LIST msg = new MSG_ZGC_RANK_REWARD_LIST();
                msg.RankType = pks.RankType;
                msg.Page = pks.Page;
                msg.Count = pks.Count;
                foreach (var playerInfo in pks.RankList)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in playerInfo.BaseInfo)
                    {
                        dataList[(HFPlayerInfo)item.Key] = item.Value;
                    }
                    RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                    RANK_REWARD_INFO info = new RANK_REWARD_INFO();
                    info.PcUid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                    info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                    info.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
                    info.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                    info.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
                    info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                    info.Id = playerInfo.Rank.Score;

                    if (player.CheckRankRewardState(info.Id))
                    {
                        info.State = (int)ActivityState.Get;
                    }
                    else
                    {
                        info.State = (int)ActivityState.None;
                    }
                    msg.List.Add(info);
                }
                player.Write(msg);

            }
            else
            {
                Log.WarnLine("player {0} get rank reward fail, can not find player.", uid);
            }
        }

        private void OnResponse_GetRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_RANK_REWARD>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZGC_GET_RANK_REWARD msg = new MSG_ZGC_GET_RANK_REWARD();
                msg.RankType = pks.RankType;
                msg.Result = pks.Result;

                if (msg.Result == (int)ErrorCode.Success)
                {
                    if (player.CheckRankRewardState(pks.Id))
                    {
                        msg.Result = (int)ErrorCode.Fail;
                    }
                    else
                    {
                        //领取奖励
                        RankRewardModel info = RankLibrary.GetReward(pks.Id);
                        if (info == null)
                        {
                            Log.Warn("player {0} GetRankReward id {1} error: not find ", uid, pks.Id);
                            return;
                        }
                        RewardManager manager = new RewardManager();
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, info.DropReward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)player.Job);
                        manager.AddReward(items);
                        manager.BreakupRewards(true);
                        // 发放奖励
                        player.AddRewards(manager, ObtainWay.RankReward);
                        //通知前端奖励
                        manager.GenerateRewardMsg(msg.Rewards);

                        player.UpdateRankRewardState(pks.RankType, pks.Id);
                        
                        int page = player.GetRankRewardPageById(pks.RankType, pks.Id);
                        player.GetRankRewardInfos(pks.RankType, page);
                        //红点通知
                        player.CheckNotifyRankReward(pks.RankType);
                        //页码需变更时通知前端
                        player.CheckNotifyRankRewardPage();
                    }
                }
                player.Write(msg);

            }
            else
            {
                Log.WarnLine("player {0} get rank reward fail, can not find player.", uid);
            }
        }

        private void OnResponse_GetRankRewardPage(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RANK_REWARD_PAGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RANK_REWARD_PAGE>(stream);
            if (uid == 0)
            {
                foreach (var item in msg.List)
                {
                    MSG_ZGC_RANK_REWARD_PAGE pks = new MSG_ZGC_RANK_REWARD_PAGE();
                    pks.RankType = item.RankType;
                    pks.Page = item.Page;
                    Api.PCManager.Broadcast(pks);
                }
            }
            else
            {
                PlayerChar player = Api.PCManager.FindPc(uid);
                if (player != null)
                {
                    foreach (var item in msg.List)
                    {
                        player.SendRankRewarPagedMsg(item.RankType, item.Page);
                    }
                }
                else
                {
                    Log.WarnLine("player {0} get rank reward page fail, can not find player.", uid);
                }
            }
        }

        private void OnResponse_NewGetRankList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_RANK_LIST>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                CampType camp = (CampType)msg.Camp;
                switch ((RankType)msg.RankType)
                {
                    case RankType.CampBattleScore:
                    case RankType.CampBattleCollection:
                    //case RankType.CampBattleFight:
                    case RankType.CampLeader:
                    case RankType.CampBuild:
                        {
                            MSG_ZGC_CAMP_RANK_LIST_BY_TYPE response = new MSG_ZGC_CAMP_RANK_LIST_BY_TYPE();
                            response.Page = msg.Page;
                            response.Count = msg.Count;
                            response.Camp = msg.Camp;
                            response.RankType = msg.RankType;

                            foreach (var item in msg.RankList)
                            {
                                response.RankList.Add(GetRankPlayerInfo(item));
                            }

                            if (player.Camp == camp && msg.Info != null)
                            {
                                response.OwnerInfo = GetRankPlayerInfo(player, msg.Info);
                            }

                            player.Write(response);
                        }
                        break;
                    case RankType.CampBuildValue:
                        {
                            MSG_ZGC_CAMPBUILD_RANK_LIST response = new MSG_ZGC_CAMPBUILD_RANK_LIST();
                            response.Page = msg.Page;
                            response.TotalCount = msg.Count;

                    
                            foreach (var item in msg.RankList)
                            {
                                response.RankList.Add(GetCampBuildRankPlayerInfo(item));
                            }

                            if (player.Camp == camp && msg.Info != null)
                            {
                                response.OwnerInfo = GetCampBuildRankPlayerInfo(player, msg.Info);
                            }
                           
                            player.Write(response);
                        }
                        break;
                    case RankType.Arena:
                        {
                            MSG_ZGC_ARENA_RANK_INFO_LIST response = new MSG_ZGC_ARENA_RANK_INFO_LIST();
                            response.Page = msg.Page;
                            response.TotalCount = msg.Count;

                            if (msg.Info != null)
                            {
                                response.OwnerInfo = GetArenaRankPlayerInfo(player, msg.Info);
                            }

                            foreach (var challenger in msg.RankList)
                            {
                                //玩家
                                ARENA_RANK_INFO rankInfo = GetArenaRankInfo(challenger);
                                response.List.Add(rankInfo);
                            }
                            response.Result = (int)ErrorCode.Success;
                            player.Write(response);
                        }
                        break;
                    case RankType.SecretArea:
                    case RankType.PushFigure:
                    case RankType.Hunting:
                    case RankType.BattlePower:
                    case RankType.Contribution:
                    case RankType.CrossServer:
                    case RankType.CrossBossSite:
                    case RankType.CrossBoss:
                    case RankType.ThemeBoss:
                    case RankType.Garden:
                    case RankType.IslandHigh:
                    case RankType.IslandHighCurrStage:
                    case RankType.IslandHighLastStage:
                    case RankType.CarnivalBoss:
                    case RankType.Roulette:
                    case RankType.Canoe:
                    case RankType.MidAutumn:
                    case RankType.ThemeFirework:
                    case RankType.CrossChallenge:
                    case RankType.NineTest:
                    {
                            MSG_ZGC_RANK_LIST_BY_TYPE response = new MSG_ZGC_RANK_LIST_BY_TYPE();
                            response.Page = msg.Page;
                            response.Count = msg.Count;
                            response.RankType = msg.RankType;

                            if (msg.Info != null)
                            {
                                response.OwnerInfo = new RANK_INFO();
                                response.OwnerInfo.Uid = player.Uid;
                                response.OwnerInfo.Name = player.Name;
                                response.OwnerInfo.Sex = player.Sex;
                                response.OwnerInfo.Icon = player.HeroId;
                                response.OwnerInfo.ShowDIYIcon = player.ShowDIYIcon;
                                response.OwnerInfo.IconFrame = player.GetFaceFrame();
                                response.OwnerInfo.Level = player.Level;
                                response.OwnerInfo.Rank = msg.Info.Rank;
                                response.OwnerInfo.GodType = player.GodType;
                                response.OwnerInfo.MainId = Api.MainId;
                                response.OwnerInfo.BattlePower = player.HeroMng.CalcBattlePower();
                                if (msg.Info.Rank == 0)
                                {
                                    switch ((RankType)msg.RankType)
                                    {
                                        case RankType.SecretArea:
                                            response.OwnerInfo.ShowValue = player.SecretAreaManager.Id;
                                            break;
                                        case RankType.PushFigure:
                                            response.OwnerInfo.ShowValue = player.pushFigureManager.Id;
                                            break;
                                        case RankType.Hunting:
                                            response.OwnerInfo.ShowValue = player.HuntingManager.Research;
                                            break;
                                        case RankType.BattlePower:
                                            response.OwnerInfo.ShowValue = player.HeroMng.CalcBattlePower();
                                            break;
                                        case RankType.Contribution:
                                            response.OwnerInfo.ShowValue = player.ContributionValue;
                                            break;
                                        case RankType.CrossServer:
                                            response.OwnerInfo.ShowValue = player.CrossInfoMng.Info.Star;
                                            break;
                                        case RankType.ThemeBoss:
                                            response.OwnerInfo.ShowValue = player.ThemeBossManager.GetRankScore();//
                                            break;
                                        case RankType.Garden:
                                            response.OwnerInfo.ShowValue = player.GardenManager.GardenInfo.Score;//
                                            break;
                                        case RankType.IslandHigh:
                                        case RankType.IslandHighCurrStage:
                                            response.OwnerInfo.ShowValue = player.IslandHighManager.GridIndex;//
                                            break;
                                        case RankType.CarnivalBoss:
                                            response.OwnerInfo.ShowValue = player.CarnivalBossMng.GetRankScore();
                                            break;
                                        case RankType.Roulette:
                                            response.OwnerInfo.ShowValue = player.RouletteManager.Score;
                                            break;
                                        case RankType.Canoe:
                                            response.OwnerInfo.ShowValue = player.CanoeManager.Info.Score;
                                            break;
                                        case RankType.CrossChallenge:
                                            response.OwnerInfo.ShowValue = player.CrossChallengeInfoMng.Info.Star;
                                            break;
                                        case RankType.MidAutumn:
                                            response.OwnerInfo.ShowValue = player.MidAutumnMng.Info.Score;
                                            break;
                                        case RankType.ThemeFirework:
                                            response.OwnerInfo.ShowValue = player.ThemeFireworkMng.Info.Score;
                                            break;
                                        case RankType.NineTest:
                                            response.OwnerInfo.ShowValue = player.NineTestMng.Info.Score;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {                  
                                    response.OwnerInfo.ShowValue = msg.Info.Score;
                                }
                            }

                            foreach (var challenger in msg.RankList)
                            {
                                //玩家
                                RANK_INFO rankInfo = new RANK_INFO();
                                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                                foreach (var item in challenger.BaseInfo)
                                {
                                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                                }
                                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                                rankInfo.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                                rankInfo.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                                rankInfo.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
                                rankInfo.Icon = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                                rankInfo.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                                rankInfo.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
                                rankInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                                rankInfo.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
                                rankInfo.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                                rankInfo.BattlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);

                                rankInfo.ShowValue = challenger.Rank.Score;
                                rankInfo.Rank = challenger.Rank.Rank;
                                response.RankList.Add(rankInfo);
                            }
                            player.Write(response);


                            switch ((RankType)msg.RankType)
                            {
                                case RankType.CrossServer:
                                    if (msg.Info.Rank != player.CrossInfoMng.Info.Rank)
                                    {
                                        player.UpdateCrossSeasonRank(msg.Info.Rank);
                                        player.SendCrossBattleManagerMessage();
                                    }
                                    break;
                                case RankType.CrossChallenge:
                                    if (msg.Info.Rank != player.CrossChallengeInfoMng.Info.Rank)
                                    {
                                        player.UpdateCrossChallengeSeasonRank(msg.Info.Rank);
                                        player.SendCrossChallengeManagerMessage();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Log.Error("player {0} mainId {1} new get rank list fail,can not find player {0}", uid, MainId, uid);
            }
        }


        public void OnResponse_GetRankFirstInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_RANK_FIRST_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_RANK_FIRST_INFO>(stream);
            Log.Write($"player {uid} GetRankFirstInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetRankFirstInfo from main {MainId} not find player ");
                return;
            }

            switch ((RankType)pks.RankType)
            {
                case RankType.Garden:
                    {
                        MSG_ZGC_GARDEN_INFO msg = player.GardenManager.GenerateGardenInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }

                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }
                        player.Write(msg);
                    }
                    break;
                case RankType.Roulette:
                    {
                        MSG_ZGC_ROULETTE_GET_INFO msg = player.RouletteManager.GenerateRouletteInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }

                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }
                        player.Write(msg);
                    }
                    break;
                case RankType.Canoe:
                    {
                        MSG_ZGC_CANOE_INFO msg = player.GenerateCanoeInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }
                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }                     
                        player.Write(msg);
                    }
                    break;
                case RankType.MidAutumn:
                    {
                        MSG_ZGC_MIDAUTUMN_INFO msg = player.GenerateMidAutumnInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }
                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }
                        player.Write(msg);
                    }
                    break;
                case RankType.ThemeFirework:
                    {
                        MSG_ZGC_THEME_FIREWORK_INFO msg = player.GenerateThemeFireworkInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }
                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }
                        player.Write(msg);
                    }
                    break;
                case RankType.NineTest:
                    {
                        MSG_ZGC_GET_NINETEST_INFO msg = player.GenerateNineTestInfo();
                        if (pks.BaseInfo.Count > 0)
                        {
                            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                            foreach (var item in pks.BaseInfo)
                            {
                                dataList[(HFPlayerInfo)item.Key] = item.Value;
                            }
                            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                            PlayerSimpleInfo info = new PlayerSimpleInfo();
                            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                            info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                            info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                            info.Value = pks.FirstValue;
                            msg.RankFirstInfo = info;
                        }
                        player.Write(msg);
                    }
                    break;
            }
        }

        public void OnResponse_GetCrossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_RANK_REWARD>(stream);
            Log.Write($"player {uid} GetCrossRankReward from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetCrossRankReward from main {MainId} not find player ");
                return;
            }

            player.GetCrossRankReward(pks.RankType, pks.Rank); 
        }

        public void OnResponse_RecordRankActiveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RECORD_RANK_ACTIVE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RECORD_RANK_ACTIVE_INFO>(stream);
            Log.Write($"player {uid} RecordRankActiveInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} RecordRankActiveInfo from main {MainId} not find player ");
                return;
            }

            player.RecordRankActiveInfo(pks.RankType, pks.FirstUid, pks.FirstValue, pks.LuckyUid);
        }

        private CAMP_RANK_INFO GetRankPlayerInfo(PlayerChar player, RankBaseInfo rank)
        {
            if (player == null)
            {
                return null;
            }

            CAMP_RANK_INFO info = new CAMP_RANK_INFO();
            info.Uid = player.Uid;
            info.Name = player.Name;
            info.Sex = player.Sex;
            info.Icon = player.Icon;
            info.ShowDIYIcon = player.ShowDIYIcon;
            info.IconFrame = player.GetFaceFrame();
            info.Level = player.Level;

            if (rank != null)
            {
                info.ShowValue = rank.Score;
                info.Rank = rank.Rank;
            }

            //info.TitleLevel = 0;

            return info;
        }

        private CAMP_BUILD_RANK_INFO GetCampBuildRankPlayerInfo(PlayerChar player, RankBaseInfo rank)
        {
            if (player == null)
            {
                return null;
            }

            CAMP_BUILD_RANK_INFO info = new CAMP_BUILD_RANK_INFO();
            info.Uid = player.Uid;
            info.Name = player.Name;
            info.Sex = player.Sex;
            info.Icon = player.Icon;
            info.ShowDIYIcon = player.ShowDIYIcon;
            info.IconFrame = player.GetFaceFrame();
            info.Level = player.Level;

            if (rank != null)
            {
                info.BuildValue = rank.Score;
                info.Rank = rank.Rank;
            }
         
            //info.TitleLevel = 0;

            return info;
        }

        private CAMP_BUILD_RANK_INFO GetCampBuildRankPlayerInfo(PlayerRankMsg playerInfo)
        {
            if (playerInfo == null)
            {
                return null;
            }

            CAMP_BUILD_RANK_INFO info = new CAMP_BUILD_RANK_INFO();
            if (playerInfo.Rank != null)
            {
                info.Rank = playerInfo.Rank.Rank;
                info.BuildValue = playerInfo.Rank.Score;
            }

            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
            foreach (var item in playerInfo.BaseInfo)
            {
                dataList[(HFPlayerInfo)item.Key] = item.Value;
            }
            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
            info.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
            info.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
            info.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
            info.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
            info.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
            //info.TitleLevel = rInfo.GetIntValue(HFPlayerInfo.CurTitle);
            return info;
        }
        private CAMP_RANK_INFO GetRankPlayerInfo(PlayerRankMsg playerInfo)
        {
            if (playerInfo == null)
            {
                return null;
            }

            CAMP_RANK_INFO info = new CAMP_RANK_INFO();
            if (playerInfo.Rank != null)
            {
                info.Rank = playerInfo.Rank.Rank;
                info.ShowValue = playerInfo.Rank.Score;
            }

            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
            foreach (var item in playerInfo.BaseInfo)
            {
                dataList[(HFPlayerInfo)item.Key] = item.Value;
            }
            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
            info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
            info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
            info.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
            info.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
            info.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
            info.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
            info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
            info.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
            //info.TitleLevel = rInfo.GetIntValue(HFPlayerInfo.CurTitle);
            return info;
        }

        private ARENA_RANK_INFO GetArenaRankPlayerInfo(PlayerChar player, RankBaseInfo rank)
        {
            if (player == null)
            {
                return null;
            }

            ARENA_RANK_INFO self = new ARENA_RANK_INFO();
            self.BaseInfo = PlayerInfo.GetPlayerBaseInfo(player);
            self.Rank = rank.Rank;
            foreach (var kv in player.ArenaMng.DefensiveHeros)
            {
                self.Defensive.Add(kv.Key);
            }
            //self.Defensive.AddRange(player.ArenaMng.DefensiveHeros);
            return self;
        }

        private ARENA_RANK_INFO GetArenaRankInfo(PlayerRankMsg playerInfo)
        {
            ARENA_RANK_INFO info = new ARENA_RANK_INFO();

            if (playerInfo.Rank != null)
            {
                info.Rank = playerInfo.Rank.Rank;
            }

            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
            foreach (var item in playerInfo.BaseInfo)
            {
                dataList[(HFPlayerInfo)item.Key] = item.Value;
            }
            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

            info.BaseInfo = new PLAYER_BASE_INFO();
            info.BaseInfo.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
            info.BaseInfo.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
            info.BaseInfo.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
            info.BaseInfo.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
            info.BaseInfo.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
            info.BaseInfo.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
            info.BaseInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
            info.BaseInfo.Level = rInfo.GetIntValue(HFPlayerInfo.Level);

            info.BaseInfo.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
            info.BaseInfo.IsOnline = rInfo.GetBoolValue(HFPlayerInfo.IsOnline); 
            info.BaseInfo.LogOutTime = rInfo.GetIntValue(HFPlayerInfo.LastLogoutTime); 
            info.BaseInfo.Camp = rInfo.GetIntValue(HFPlayerInfo.CampId); 
            info.BaseInfo.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId); 

            info.BaseInfo.LadderLevel = rInfo.GetIntValue(HFPlayerInfo.ArenaLevel); 
            info.BaseInfo.BattlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);

            //info.Defensive.AddRange(rInfo.GetIntList(HFPlayerInfo.ArenaDefensive));
            string arenaDefensive = rInfo.GetStringValue(HFPlayerInfo.ArenaDefensive);
            string[] defensives = StringSplit.GetArray("|", arenaDefensive);
            foreach (var defensive in defensives)
            {
                string[] hero = StringSplit.GetArray(":", defensive);
                info.Defensive.Add(int.Parse(hero[0]));
                //info.CDefPoses.Add(int.Parse(hero[1]));
            }
            return info;
        }

        
    }
}

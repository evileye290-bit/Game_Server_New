using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace CrossServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_GetRankList(MemoryStream stream, int uid = 0)
        {
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetRankList from main {MainId} not find group id ");
                return;
            }

            MSG_RC_GET_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_RANK_LIST>(stream);
            RankType rankType = (RankType)msg.RankType;
            switch (rankType)
            {
                case RankType.CrossBoss:
                    {
                        int chapterId = msg.ParamId;

                        CrossBossRankManager crossMng = Api.RankMng.GetCrossBossRankManager(groupId);
                        if (crossMng != null)
                        {
                            CrossBossChapterRank chapterRank = crossMng.GetChapterRank(chapterId);
                            if (chapterRank == null)
                            {
                                crossMng.AddChapterRank(groupId, chapterId);
                                chapterRank = crossMng.GetChapterRank(chapterId);
                            }
                            chapterRank.PushRankList(uid, MainId, msg.Page);
                        }
                    }
                    break;
                case RankType.CrossBossSite:
                    {
                        int siteId = msg.ParamId;

                        CrossBossRankManager crossMng = Api.RankMng.GetCrossBossRankManager(groupId);
                        if (crossMng != null)
                        {
                            CrossBossSiteRank chapterRank = crossMng.GetSiteRank(siteId);
                            if (chapterRank == null)
                            {
                                crossMng.AddSiteRank(groupId, siteId);
                                chapterRank = crossMng.GetSiteRank(siteId);
                            }
                            chapterRank.PushRankList(uid, MainId, msg.Page);
                        }
                    }
                    break;
                case RankType.Garden:
                    {
                        GardenRank gardenRank = Api.RankMng.GetGardenRank(groupId);
                        if (gardenRank == null)
                        {
                            Api.RankMng.AddGardenRank(groupId);
                            gardenRank = Api.RankMng.GetGardenRank(groupId);
                        }
                        gardenRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.IslandHigh:
                case RankType.IslandHighCurrStage:
                    {
                        IslandHighRank islandHighRank = Api.RankMng.GetIslandHighRank(groupId);
                        if (islandHighRank == null)
                        {
                            Api.RankMng.AddIslandHighRank(groupId);
                            islandHighRank = Api.RankMng.GetIslandHighRank(groupId);
                        }
                        islandHighRank.SetIsGetCurStageRank(rankType == RankType.IslandHighCurrStage);
                        islandHighRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.IslandHighLastStage:
                    {
                        IslandHighLastStageRank islandHighRank = Api.RankMng.GetIslandHighLastStageRank(groupId);
                        if (islandHighRank == null)
                        {
                            Api.RankMng.AddIslandHighLastStageRank(groupId);
                            islandHighRank = Api.RankMng.GetIslandHighLastStageRank(groupId);
                        }
                        islandHighRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.CarnivalBoss:
                    {
                        CarnivalBossRank carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        if (carnivalBossRank == null)
                        {
                            Api.RankMng.AddCarnivalBossRank(groupId);
                            carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        }
                        carnivalBossRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.Roulette:
                    {
                        RouletteRank rouletteRank = Api.RankMng.GetRouletteRank(groupId);
                        if (rouletteRank == null)
                        {
                            Api.RankMng.AddIslandHighRank(groupId);
                            rouletteRank = Api.RankMng.GetRouletteRank(groupId);
                        }
                        rouletteRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.Canoe:
                    {
                        CanoeRank canoeRank = Api.RankMng.GetCanoeRank(groupId);
                        if (canoeRank == null)
                        {
                            Api.RankMng.AddCanoeRank(groupId);
                            canoeRank = Api.RankMng.GetCanoeRank(groupId);
                        }
                        canoeRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.MidAutumn:
                    {
                        MidAutumnRank midAutumnRank = Api.RankMng.GetMidAutumnRank(groupId);
                        if (midAutumnRank == null)
                        {
                            Api.RankMng.AddMidAutumnRank(groupId);
                            midAutumnRank = Api.RankMng.GetMidAutumnRank(groupId);
                        }
                        midAutumnRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.ThemeFirework:
                    {
                        ThemeFireworkRank fireworkRank = Api.RankMng.GetThemeFireworkRank(groupId);
                        if (fireworkRank == null)
                        {
                            Api.RankMng.AddThemeFireworkRank(groupId);
                            fireworkRank = Api.RankMng.GetThemeFireworkRank(groupId);
                        }
                        fireworkRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                case RankType.NineTest:
                    {
                        NineTestRank nineTestRank = Api.RankMng.GetNineTestRank(groupId);
                        if (nineTestRank == null)
                        {
                            Api.RankMng.AddNineTestRank(groupId);
                            nineTestRank = Api.RankMng.GetNineTestRank(groupId);
                        }
                        nineTestRank.PushRankList(uid, MainId, msg.Page);
                    }
                    break;
                default:
                    return;
            }
        }

        public void OnResponse_GetRankFirstInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_RANK_FIRST_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_RANK_FIRST_INFO>(stream);
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetRankFirstInfo from main {MainId} not find group id ");
                return;
            }

            int firstValue = 0;

            //获取第一名暗器值
            RankBaseModel firstModel = Api.RankMng.GetFirst((RankType)pks.RankType, groupId);
            if (firstModel != null)
            {
                firstValue = firstModel.Score;
            }

            MSG_CorssR_GET_RANK_FIRST_INFO msg = new MSG_CorssR_GET_RANK_FIRST_INFO();
            msg.Uid = uid;
            msg.RankType = pks.RankType;
            msg.FirstValue = firstValue;
            if (firstModel != null)
            {
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }
            Write(msg, uid);
        }

        public void OnResponse_UpdateRankValue(MemoryStream stream, int uid = 0)
        {
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} UpdateRankValue from main {MainId} not find group id ");
                return;
            }

            MSG_RC_UPDATE_RANK_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_UPDATE_RANK_VALUE>(stream);
            RankType rankType = (RankType)msg.RankType;
            switch (rankType)
            {
                case RankType.Garden:
                    {
                        //检查结算时间
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Garden, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.Garden} value {msg.Value} ");
                            return;
                        }

                        GardenRank gardenRank = Api.RankMng.GetGardenRank(groupId);
                        if (gardenRank == null)
                        {
                            Api.RankMng.AddGardenRank(groupId);
                            gardenRank = Api.RankMng.GetGardenRank(groupId);
                        }
                        gardenRank.UpdateScore(uid, msg.Value);
                    }
                    break;
                case RankType.IslandHigh:
                    {
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.IslandHigh, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.IslandHigh} value {msg.Value} ");
                            return;
                        }

                        IslandHighRank islandHighRank = Api.RankMng.GetIslandHighRank(groupId);
                        if (islandHighRank == null)
                        {
                            Api.RankMng.AddIslandHighRank(groupId);
                            islandHighRank = Api.RankMng.GetIslandHighRank(groupId);
                        }
                        islandHighRank.UpdateScore(uid, msg.Value);
                    }
                    break;
                case RankType.CarnivalBoss:
                    {
                        //检查结算时间
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.CarnivalBoss, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.CarnivalBoss} value {msg.Value} ");
                            return;
                        }

                        CarnivalBossRank carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        if (carnivalBossRank == null)
                        {
                            Api.RankMng.AddCarnivalBossRank(groupId);
                            carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        }
                        carnivalBossRank.UpdateScore(uid, msg.Value);
                    }
                    break;
                case RankType.Roulette:
                {
                    RechargeGiftModel model;
                    if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Roulette, Api.Now(), out model))
                    {
                        Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.Roulette} value {msg.Value} ");
                        return;
                    }

                    RouletteRank rouletteRank = Api.RankMng.GetRouletteRank(groupId);
                    if (rouletteRank == null)
                    {
                        Api.RankMng.AddRouletteRank(groupId);
                        rouletteRank = Api.RankMng.GetRouletteRank(groupId);
                    }
                    rouletteRank.UpdateScore(uid, msg.Value);
                }
                    break;
                case RankType.Canoe:
                    {                     
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Canoe, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.Canoe} value {msg.Value} ");
                            return;
                        }

                        Api.CanoeMng.UpdatePlayerValue(groupId, uid, msg.Value);
                        SendRankFirstInfo(groupId, RankType.Canoe, uid, msg.Value);
                    }
                    break;
                case RankType.MidAutumn:
                    {
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.MidAutumn, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.MidAutumn} value {msg.Value} ");
                            return;
                        }

                        Api.MidAutumnMng.UpdatePlayerValue(groupId, uid, msg.Value);                     
                    }
                    break;
                case RankType.ThemeFirework:
                    {
                        if (!RechargeLibrary.CheckInSpecialRechargeGiftTime(RechargeGiftType.ThemeFirework, Api.Now()))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.ThemeFirework} value {msg.Value} ");
                            return;
                        }
                        Api.ThemeFireworkMng.UpdatePlayerValue(groupId, uid, msg.Value);
                    }
                    break;
                case RankType.NineTest:
                    {
                        //检查结算时间
                        RechargeGiftModel model;
                        if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.NineTest, Api.Now(), out model))
                        {
                            Log.Warn($"player {uid} UpdateRankValue failed: time is error, type {RechargeGiftType.NineTest} value {msg.Value} ");
                            return;
                        }

                        Api.NineTestMng.UpdatePlayerValue(groupId, uid, msg.Value);
                    }
                    break;
                default:
                    return;
            }

            if (msg.BaseInfo.Count > 0)
            {
                JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
                if (playerInfo == null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in msg.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    //添加信息
                    playerInfo = new JsonPlayerInfo(dataList);
                    Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                    //Api.CrossRedis.Call(new OperateAddCrossPlayerBaseInfo(uid, playerInfo));
                }
                else
                {
                    foreach (var item in msg.BaseInfo)
                    {
                        if (item.Key == (int)HFPlayerInfo.HeroId)
                        {
                            int heroId = int.Parse(item.Value);
                            if (heroId > 0 && playerInfo.HeroId != heroId)
                            {
                                playerInfo.HeroId = heroId;
                                playerInfo.Icon = heroId;
                                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                            }
                            break;
                        }
                    }
                }
            }
        }


        private void OnResponse_GetIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetIslandHighInfo from main {MainId} not find group id ");
                return;
            }

            MSG_RC_GET_ISLAND_HIGH_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_ISLAND_HIGH_INFO>(stream);
            Log.Write($"player {uid} GetIslandHighInfo mainId {MainId}");


            MSG_CrossR_GET_ISLAND_HIGH_INFO msg = new MSG_CrossR_GET_ISLAND_HIGH_INFO();

            IslandHighRank islandHighRank = Api.RankMng.GetIslandHighRank(groupId);
            IslandHighLastStageRank lastStageRank = Api.RankMng.GetIslandHighLastStageRank(groupId);
            if (islandHighRank != null)
            {
                RankBaseModel model = islandHighRank.GetRankBaseInfo(uid);
                if (model != null)
                {
                    msg.RankValue = model.Rank;
                }

                if (lastStageRank != null)
                {
                    model = islandHighRank.GetRankBaseInfo(uid);
                    if (model != null)
                    {
                        msg.LastRankValue = model.Rank;
                    }
                }
            }

            Write(msg, uid);
        }

        public void OnResponse_GetCrossRankReward(MemoryStream stream, int uid = 0)
        {
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetCrossRankReward from main {MainId} not find group id ");
                return;
            }

            MSG_RC_GET_RANK_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_RANK_REWARD>(stream);
            int rank = 0;
            RankType rankType = (RankType)msg.RankType;
            switch (rankType)
            {
                case RankType.CarnivalBoss:
                    {
                        CarnivalBossRank carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        if (carnivalBossRank == null)
                        {
                            Api.RankMng.AddCarnivalBossRank(groupId);
                            carnivalBossRank = Api.RankMng.GetCarnivalBossRank(groupId);
                        }
                        rank = carnivalBossRank.GetNewRankInfo(uid, MainId);
                    }
                    break;
                default:                 
                    break;
            }

            if (rank != -1)
            {
                MSG_CorssR_GET_RANK_REWARD response = new MSG_CorssR_GET_RANK_REWARD();
                response.Uid = uid;
                response.RankType = msg.RankType;
                response.Rank = rank;
                Write(response, uid);
            }           
        }

        public void SendRankFirstInfo(int groupId, RankType rankType, int uid, int value)
        {
            RankBaseModel firstModel = null;
            switch (rankType)
            { 
                case RankType.Canoe:
                    firstModel = Api.CanoeMng.GetFirstValue(groupId);
                    break;            
                default:
                    break;
            }           
            int firstValue = 0;        
            MSG_CorssR_GET_RANK_FIRST_INFO msg = new MSG_CorssR_GET_RANK_FIRST_INFO();
            msg.Uid = uid;
            msg.RankType = (int)rankType;        
            if (firstModel != null)
            {
                firstValue = firstModel.Score;
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }
            msg.FirstValue = firstValue;
            Write(msg, uid);
        }
    }
}

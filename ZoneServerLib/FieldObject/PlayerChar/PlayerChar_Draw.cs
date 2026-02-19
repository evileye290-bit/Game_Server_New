using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //抽奖系统

        private DrawManager drawManager;
        public DrawManager DrawMng
        { get { return drawManager; } }

        private void InitDrawManager()
        {
            drawManager = new DrawManager(this);
        }

        /// <summary>
        /// 抽卡
        /// </summary>
        /// <param name="heroIds"></param>
        public void DrawHeroCard(int drawType, bool isFree, bool isItem, bool isSingle)
        {
            if (isFree)
            {
                isItem = false;
                isSingle = true;
            }

            MSG_ZGC_DRAW_HERO response = new MSG_ZGC_DRAW_HERO();

            //获取抽奖配置
            DrawTimeModel drawTime = DrawLibrary.GetDrawTime(drawType, ZoneServerApi.now, RechargeLibrary.IgnoreNewServerActivity);
            if (drawTime == null)
            {
                //没有找到对应抽奖系统
                Log.Warn("player {0} draw hero card failed: no such time draw tyoe {1}", uid, drawType);
                response.Result = (int)ErrorCode.NoDrawType;
                Write(response);
                return;
            }

            //获取抽奖配置
            DrawQualityModel drawQuality = DrawLibrary.GetQualityModel(drawType);
            if (drawQuality == null)
            {
                //没有找到对应抽奖系统
                Log.Warn("player {0} draw hero card failed: no such tyoe {1}", uid, drawType);
                response.Result = (int)ErrorCode.NoDrawType;
                Write(response);
                return;
            }

            //伙伴权重
            Dictionary<int, DrawHeroQualityModel> heroQualityRatioLsit = DrawLibrary.GetHeroQualityList(drawType, ZoneServerApi.now);
            if (heroQualityRatioLsit == null)
            {
                //没有找到对应抽奖系统
                Log.Warn("player {0} draw hero card failed: no such hero quality list tyoe {1}", uid, drawType);
                response.Result = (int)ErrorCode.NoDrawType;
                Write(response);
                return;
            }

            //抽奖次数
            int drawNum = 1;
            if (!isSingle)
            {
                drawNum = drawQuality.ContinuousNum;
            }
            int costId = 0;
            int costNum = 1;
            bool isDiscount = drawTime.CheckDiscount(ZoneServerApi.now);

            Dictionary<int, int> ratioList = new Dictionary<int, int>();
            int totalRatio = 0;
            if (isFree)
            {
                //免费抽奖， 只是单抽
                ErrorCode errorCode = CheckHeroFreeDraw(drawType, drawQuality);
                if (errorCode == ErrorCode.Success)
                {
                    //可以免费抽奖
                    ratioList = drawQuality.FreeRatio;
                    totalRatio = drawQuality.FreeTotalRatio;
                }
                else
                {
                    response.Result = (int)errorCode;
                    Write(response);
                    return;
                }
            }
            else
            {
                if (isItem)
                {
                    //兑换券抽奖
                    ErrorCode errorCode = CheckHeroItemDraw(drawType, isSingle, drawQuality, drawNum, out costId, out costNum);
                    if (errorCode == ErrorCode.Success)
                    {
                        //可以兑换券抽奖
                        ratioList = drawQuality.ItemRatio;
                        totalRatio = drawQuality.ItemTotalRatio;
                    }
                    else
                    {
                        Log.Warn("player {0} draw hero card check item failed: no item {1} num {2} tyoe {3} error code {4}", uid, costId, costNum, drawType, errorCode);
                        response.Result = (int)errorCode;
                        Write(response);
                        return;
                    }
                }
                else
                {
                    //使用钻石
                    ErrorCode errorCode = CheckHeroCostDraw(drawType, isSingle, drawQuality, isDiscount);
                    if (errorCode == ErrorCode.Success)
                    {
                        //可以兑换券抽奖
                        ratioList = drawQuality.CostRatio;
                        totalRatio = drawQuality.CostTotalRatio;
                    }
                    else
                    {
                        response.Result = (int)errorCode;
                        Write(response);
                        return;
                    }
                }
            }


            //抽卡历史记录
            DrawCounterItem history = DrawMng.Get(drawType);
            if (history == null)
            {
                history = new DrawCounterItem();
                history.Id = drawType;
            }



            //祝福值满
            bool getBlessing = false;
            if (drawQuality.BlessingMax > 0 && history.Blessing + drawNum >= drawQuality.BlessingMax)
            {
                getBlessing = true;
            }

            //抽卡开始
            Dictionary<int, int> qualityList = ScriptManager.Draw.GetDrawQualityResult(drawType, drawNum, getBlessing, history, ratioList, totalRatio, drawQuality);
            if (qualityList.Count == 0)
            {
                //没有找到对应抽奖系统
                Log.Warn("player {0} draw hero card failed: no such quality tyoe {1} isSingle {2} total {3}", uid, drawType, isSingle, totalRatio);
                response.Result = (int)ErrorCode.NoDrawType;
                Write(response);
                return;
            }

            if (getBlessing && drawQuality.BlessingHeroRatio.Count > 0)
            {
                int count = 0;
                //说明有祝福值保底
                if (qualityList.TryGetValue(drawQuality.MaxBlessingQuality, out count))
                {
                    if (count > 1)
                    {
                        qualityList[drawQuality.MaxBlessingQuality] -= 1;
                    }
                    else
                    {
                        qualityList.Remove(drawQuality.MaxBlessingQuality);
                    }
                }
            }

            DrawCountHerosModel countHeros = drawQuality.GetCountHeroIds(isSingle, history);
            //伙伴
            List<int[]> heroList = ScriptManager.Draw.GetDrawHeroResult(uid, countHeros, drawQuality.NoSameQuality, isSingle, history, qualityList, heroQualityRatioLsit);

            if (getBlessing && drawQuality.BlessingHeroRatio.Count > 0)
            {
                int heroId = ScriptManager.Draw.GetHeroIdByRatio(drawQuality.BlessingHeroTotalRatio, drawQuality.BlessingHeroRatio);
                heroList.Add(new int[] { heroId, 1 });

                if (qualityList.ContainsKey(drawQuality.MaxBlessingQuality))
                {
                    qualityList[drawQuality.MaxBlessingQuality] += 1;
                }
                else
                {
                    qualityList[drawQuality.MaxBlessingQuality] = 1;
                }
            }
            else
            {
                if (heroList.Count == 0)
                {
                    //没有找到对应抽奖系统
                    Log.Warn("player {0} draw hero card failed: no such hero tyoe {1} isSingle {2} ratio count {3}", uid, drawType, isSingle, heroQualityRatioLsit.Count);
                    response.Result = (int)ErrorCode.NoDrawType;
                    Write(response);
                    return;
                }
            }
            //星星数
            //bool changeConstellation = false;
            //List<int> normalResult = new List<int>();
            //List<int> specialResult = new List<int>();
            //int rewardHeroId = 0;
            //ConstellationModel drawStarInfoDic = DrawLibrary.GetStarList(drawType);
            //if (drawStarInfoDic != null)
            //{
            //    DrawStarModel drawStarInfo = DrawLibrary.GetDrawStar(drawStarInfoDic, history.Constellation);
            //    if (drawStarInfo != null)
            //    {
            //        Dictionary<int, int> starRatio;
            //        if (isSingle)
            //        {
            //            starRatio = drawStarInfo.SpecialSingleRatio;
            //        }
            //        else
            //        {
            //            starRatio = drawStarInfo.SpecialContinuousRatio;
            //        }
            //        ScriptManager.Draw.GetDrawStarResult(uid, drawNum, history.SpecialStar, drawStarInfo.NormalStar,drawStarInfo.SpecialStar, starRatio, normalResult, specialResult);

            //        if (history.SpecialStar.Count + specialResult.Count >= drawStarInfo.SpecialStarCount)
            //        {
            //            //获得特殊奖励
            //            rewardHeroId = ScriptManager.Draw.GetHeroIdByRatio(drawStarInfo.SpecialHeroTotalRatio, drawStarInfo.SpecialHeroRatio);

            //            //换星图
            //            if (history.SpecialStar.Count > 0)
            //            {
            //                history.SpecialStar.Clear();
            //            }
            //            //赋值新星图
            //            SetNewConstellation(history, drawStarInfoDic);

            //            changeConstellation = true;
            //        }
            //    }
            //    else
            //    {
            //        //没有找到对应星星
            //        Log.Warn("player {0} draw hero card error: no such draw tyoe {1} star info id {2} ", uid, drawType, history.Constellation);
            //        SetNewConstellation(history, drawStarInfoDic);
            //    }
            //}
            //else
            //{
            //    //没有找到对应星星
            //    Log.Warn("player {0} draw hero card error: no such draw tyoe {1} star info list  ", uid, drawType);
            //}

            List<int> heroIds = new List<int>();

            //领取奖励
            List<ItemBasicInfo> rewardLsit = new List<ItemBasicInfo>();
            for (int i = 0; i < heroList.Count; i++)
            {
                int[] hero = heroList[i];
                ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.Hero, hero[0], hero[1]);
                rewardLsit.Add(baseInfo);
                response.HeroIds.Add(hero[0]);

                //if (!changeConstellation)
                //{
                //    if (specialResult.Count > i)
                //    {
                //        history.SpecialStar.Add(specialResult[i], hero[0]);
                //    }
                //}

                heroIds.Add(hero[0]);
            }

            ////额外奖励
            //if (rewardHeroId > 0)
            //{
            //    heroIds.Add(rewardHeroId);
            //    bool isFind = false;
            //    foreach (var hero in rewardLsit)
            //    {
            //        if (hero.Id == rewardHeroId)
            //        {
            //            hero.Num++;
            //            isFind = true;
            //            break;
            //        }
            //    }
            //    if (!isFind)
            //    {
            //        ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.Hero, rewardHeroId, 1);
            //        rewardLsit.Add(baseInfo);
            //    }
            //}
            //如抽到SSR 跑马灯
            CheckBroadcastDrawSSRCard(drawQuality.Announces, heroIds);

            RewardManager rewards = new RewardManager();
            rewards.InitReward(rewardLsit, false);

            foreach (var quality in qualityList)
            {
                string reward = DrawLibrary.GetDrawHeroReward(quality.Key);
                if (!string.IsNullOrEmpty(reward))
                {
                    rewards.AddSimpleReward(reward, quality.Value);
                }
            }
            rewards.BreakupRewards();
            AddRewards(rewards, ObtainWay.DrawHeroCard);

            Dictionary<string, object> info = new Dictionary<string, object>();
            string costType = string.Empty;
            //int costId = 0;
            //int costNum = 0;
            if (isFree)
            {
                //免费抽奖， 只是单抽
                UpdateCounter(CounterType.RareDrawFreeCount, 1);
                costType = "free";
            }
            else
            {
                if (isSingle)
                {
                    costType = "Single";
                }
                else
                {
                    costType = "Continuous";
                }
                if (isItem)
                {
                    //costNum = 1;
                    ////扣除兑换券
                    //if (isSingle)
                    //{
                    //    costId = drawQuality.SingleItem;
                    //}
                    //else
                    //{
                    //    costId = drawQuality.ContinuousItem;
                    //}

                    BaseItem item = BagManager.GetItem(MainType.Consumable, costId);
                    if (item != null)
                    {
                        BaseItem it = DelItem2Bag(item, RewardType.NormalItem, costNum, ConsumeWay.DrawHeroCard);
                        if (it != null)
                        {
                            SyncClientItemInfo(it);
                        }
                    }

                    info = RewardManager.GetRewardInfoDic(costId, costNum, (int)RewardType.NormalItem);
                }
                else
                {
                    //使用钻石
                    CurrenciesType type = (CurrenciesType)drawQuality.CostType;
                    costNum = GetHeroCostNum(isSingle, drawQuality, isDiscount);
                    costId = drawQuality.CostType;
                    DelCoins(type, costNum, ConsumeWay.DrawHeroCard, drawNum.ToString());
                    costType = type.ToString();

                    info = RewardManager.GetRewardInfoDic(costId, costNum, (int)RewardType.Currencies);
                }
            }

            //保存抽奖结果
            if (isSingle)
            {
                history.Single++;
            }
            else
            {
                history.Continuous++;
            }
            if (drawQuality.BlessingMax > 0)
            {
                history.Blessing += drawNum;

                if (getBlessing)
                {
                    history.Blessing -= drawQuality.BlessingMax;
                }
            }
            DrawMng.SetHeroDraw(history);
            //保存DB
            SyncDbUpdateHeroDraw();


            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            //response.NormalResult.AddRange(normalResult);
            //response.SpecialResult.AddRange(specialResult);
            //response.RewardStarHero = rewardHeroId;
            response.DrawInfo = GetDrawItemMsg(history);
            Write(response);


            //抽卡任务
            if (isSingle)
            {
                AddTaskNumForType(TaskType.DrawHeroCardSingle, 1, true, drawType);
                AddPassCardTaskNum(TaskType.DrawHeroCardSingle, drawType, TaskParamType.TYPE);
            }
            else
            {
                AddTaskNumForType(TaskType.DrawHeroCardContinuous, 1, true, drawType);
                AddPassCardTaskNum(TaskType.DrawHeroCardContinuous, drawType, TaskParamType.TYPE);
            }
            AddTaskNumForType(TaskType.DrawHeroCardNum, drawNum, true, drawType);
            AddPassCardTaskNum(TaskType.DrawHeroCardNum, drawType, TaskParamType.TYPE, drawNum);
            AddDriftExploreTaskNum(TaskType.DrawHeroCardNumForAllType, drawNum);

            //抽卡埋点
            BIRecordRecruitHeroLog(drawType, costType, costNum, heroIds);

            if (drawQuality.BlessingMax > 0 && history.Blessing + drawNum >= drawQuality.BlessingMax)
            {
                getBlessing = true;
            }

            //BI 抽卡
            List<Dictionary<string, object>> costDic = new List<Dictionary<string, object>>() { info };
            string drawBlessing = $"{history.Blessing}/{drawQuality.BlessingMax}";
            KomoeEventLogDrawCard(drawType, costDic, drawNum, drawBlessing, rewards.GetRewardDic());
        }

        public void InitDrawManagerInfo()
        {
            bool updateDb = false;
            foreach (var item in DrawLibrary.QualityList)
            {
                DrawCounterItem history = DrawMng.Get(item.Key);
                if (history == null)
                {
                    history = new DrawCounterItem();
                    history.Id = item.Key;

                    ConstellationModel drawStarInfoDic = DrawLibrary.GetStarList(item.Key);
                    if (drawStarInfoDic != null)
                    {
                        SetNewConstellation(history, drawStarInfoDic);
                    }

                    DrawMng.SetHeroDraw(history);
                    updateDb = true;
                }
                else
                {
                    if (history.Constellation == 0)
                    {
                        ConstellationModel drawStarInfoDic = DrawLibrary.GetStarList(item.Key);
                        if (drawStarInfoDic != null)
                        {
                            SetNewConstellation(history, drawStarInfoDic);
                            updateDb = true;
                        }
                    }
                }
            }

            if (updateDb)
            {
                //保存DB
                SyncDbUpdateHeroDraw();
            }
        }

        private void SetNewConstellation(DrawCounterItem history, ConstellationModel drawStarInfoDic)
        {
            int rand = NewRAND.Next(0, drawStarInfoDic.TotalRatio);
            foreach (var item in drawStarInfoDic.StarList)
            {
                if (rand >= item.Value.Ratio)
                {
                    history.Constellation = item.Key;
                }
                else
                {
                    break;
                }
            }
        }

        private ErrorCode CheckHeroCostDraw(int drawType, bool isSingle, DrawQualityModel drawQuality, bool isDiscount)
        {
            if (drawQuality.CheckHasCost())
            {
                CurrenciesType type = (CurrenciesType)drawQuality.CostType;
                int num = GetHeroCostNum(isSingle, drawQuality, isDiscount);

                //可以兑换券抽取
                if (GetCoins(type) < num)
                {
                    //没有兑换券
                    Log.Warn("player {0} draw hero card check cost failed: no coin {1} num {2} tyoe {3}", uid, type, GetCoins(type), drawType);
                    return ErrorCode.NoCostDraw;
                }
                else
                {
                    return ErrorCode.Success;
                }
            }
            else
            {
                //没有兑换券抽奖
                Log.Warn("player {0} draw hero card check cost failed: no item draw by tyoe {1}", uid, drawType);
                return ErrorCode.NoCostDraw;
            }
        }

        private int GetHeroCostNum(bool isSingle, DrawQualityModel drawQuality, bool isDiscount)
        {
            int num = 0;
            if (isSingle)
            {
                if (isDiscount)
                {
                    num = drawQuality.SingleDiscount;
                }
                else
                {
                    num = drawQuality.Single;
                }
            }
            else
            {
                if (isDiscount)
                {
                    num = drawQuality.ContinuousDiscount;
                }
                else
                {
                    num = drawQuality.Continuous;
                }
            }

            return num;
        }

        private ErrorCode CheckHeroItemDraw(int drawType, bool isSingle, DrawQualityModel drawQuality, int drawNum, out int itemId, out int num)
        {
            itemId = 0;
            num = 1;
            if (isSingle)
            {
                if (!drawQuality.CheckHasSingleItem())
                {
                    //没有兑换券抽奖
                    Log.Warn("player {0} draw hero card check single item failed: no item draw by tyoe {1}", uid, drawType);
                    return ErrorCode.NoItemDraw;
                }
                itemId = drawQuality.SingleItem;
                return CheckHeroDraItemw(drawType, itemId, num);
            }
            else
            {
                if (drawQuality.CheckHasSingleItem())
                {
                    itemId = drawQuality.SingleItem;
                    num = drawNum;
                    ErrorCode result = CheckHeroDraItemw(drawType, itemId, num);
                    if (result != ErrorCode.Success)
                    {
                        if (drawQuality.CheckHasContinuousItem())
                        {
                            itemId = drawQuality.ContinuousItem;
                            num = 1;
                            return CheckHeroDraItemw(drawType, itemId, num);
                        }
                        else
                        {
                            //没有兑换券抽奖
                            Log.Warn("player {0} draw hero card check continuous item failed: no item draw by tyoe {1}", uid, drawType);
                            return ErrorCode.NoItemDraw;
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    //没有兑换券抽奖
                    Log.Warn("player {0} draw hero card check continuous item failed: no item draw by tyoe {1}", uid, drawType);
                    return ErrorCode.NoItemDraw;
                }
            }

            //return CheckHeroDraItemw(drawType, itemId, num);
        }

        private ErrorCode CheckHeroDraItemw(int drawType, int itemId, int num)
        {
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item != null)
            {
                //可以兑换券抽取
                if (item.PileNum < num)
                {
                    //没有兑换券
                    //Log.Warn("player {0} draw hero card check item failed: no item {1} num {2} tyoe {3}", uid, itemId, item.PileNum, drawType);
                    return ErrorCode.NotFoundItem;
                }
                else
                {
                    return ErrorCode.Success;
                }
            }
            else
            {
                //没有兑换券
                //Log.Warn("player {0} draw hero card check item failed: no item {1} tyoe {2}", uid, itemId, drawType);
                return ErrorCode.NotFoundItem;
            }
        }

        private ErrorCode CheckHeroFreeDraw(int drawType, DrawQualityModel drawQuality)
        {
            if (drawQuality.CheckHasFree())
            {
                //可以免费抽取
                if (GetDrawFreeCount(drawQuality.SingleFree) <= 0)
                {
                    //没有免费次数
                    Log.Warn("player {0} draw hero card check free failed: no free count tyoe {1}", uid, drawType);
                    return ErrorCode.NoFreeDraw;
                }
                else
                {
                    return ErrorCode.Success;
                }
            }
            else
            {
                //没有免费抽奖
                Log.Warn("player {0} draw hero card check free failed: no free draw by tyoe {1}", uid, drawType);
                return ErrorCode.NoFreeDraw;
            }
        }

        public int GetDrawFreeCount(int freeCounter)
        {
            CounterType type = (CounterType)freeCounter;
            int count = GetCounterValue(type);
            int maxCount = CounterLibrary.GetMaxCount(type);
            return Math.Max(0, maxCount - count);
        }

        /// <summary>
        /// 保存抽卡计数器
        /// </summary>
        public void SyncDbUpdateHeroDraw()
        {
            string heroDraw = DrawMng.GetHeroDraw();
            string constellation = DrawMng.GetDrawConstellation();
            server.GameDBPool.Call(new QueryUpdatHeroDraw(Uid, heroDraw, constellation));
        }

        public void SenDrawManagerMessage()
        {
            MSG_ZGC_DRAW_MANAGER msg = new MSG_ZGC_DRAW_MANAGER();
            msg.ComboList.AddRange(HeroMng.HeroComboList);

            foreach (var id in HeroMng.GetHeroPos())
            {
                HeroInfo info = HeroMng.GetHeroInfo(id.Key);
                if (info != null)
                {
                    msg.ComboPower += HeroMng.GetComboPower(info.Nature);
                }
            }

            Dictionary<int, DrawCounterItem> dic = DrawMng.GetDrawCounterList();
            foreach (var kv in dic)
            {
                MSG_ZGC_DRAW_ITEN item = GetDrawItemMsg(kv.Value);
                msg.List.Add(item);
            }
            Write(msg);
        }

        private static MSG_ZGC_DRAW_ITEN GetDrawItemMsg(DrawCounterItem info)
        {
            MSG_ZGC_DRAW_ITEN item = new MSG_ZGC_DRAW_ITEN();
            item.Type = info.Id;
            item.Blessing = info.Blessing;
            item.CoordinateId = info.Constellation;
            foreach (var star in info.SpecialStar)
            {
                item.Stars.Add(star.Key);
                item.Heros.Add(star.Value);
            }
            return item;
        }

        public void RefreshDrawBlessing(int RefreshId)
        {
            bool isChange = false;
            Dictionary<int, DrawCounterItem> dic = DrawMng.GetDrawCounterList();
            foreach (var kv in dic)
            {
                DrawQualityModel drawQuality = DrawLibrary.GetQualityModel(kv.Key);
                if (drawQuality != null)
                {
                    if (drawQuality.RefreshBlessing == RefreshId)
                    {
                        kv.Value.Blessing = 0;
                        isChange = true;
                    }
                }
            }

            if (isChange)
            {
                SyncDbUpdateHeroDraw();
                SenDrawManagerMessage();
            }
        }
    }
}

using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public GodPathManager GodPathManager;

        public int AcroessOceanDiff { get; set; }


        public void InitGodPathManager()
        {
            GodPathManager = new GodPathManager(this);
        }

        public void RefreshDailyGodPath()
        {
            GodPathManager.RefreshDaily();
        }

        public ErrorCode CheckCreateDungeon(DungeonModel model)
        {
            if (Team != null)
            {
                return ErrorCode.InTeam;
            }

            //TODO 限制条件
            return ErrorCode.Success;
        }

        public void GodPathReward(RewardManager manager, int dungeonId)
        {
            GodPathManager.OnFinishGodPathDungeon(dungeonId);

            AddRewards(manager, ObtainWay.Hunting);

            //先添加魂环奖励  再添加其他奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
            manager.GenerateRewardMsg(rewardMsg.Rewards);

            rewardMsg.DungeonId = dungeonId;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }


        public void GodPathAcrossOceanReward(RewardManager manager, DungeonModel model)
        {
            //通关的新难度副本,同步到db
            if ((int)model.Difficulty > AcroessOceanDiff)
            {
                SetPassedNewDiffcuteDungeon((int)model.Difficulty);
            }

            //减少挑战次数
            UpdateCounter(CounterType.AcrossOceanCount, 1);

            AddRewards(manager, ObtainWay.Hunting);

            //先添加魂环奖励  再添加其他奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
            manager.GenerateRewardMsg(rewardMsg.Rewards);

            rewardMsg.DungeonId = model.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }

        private void SetPassedNewDiffcuteDungeon(int difficulty)
        {
            AcroessOceanDiff = difficulty;

            QueryUpdateGodHeroAcrossOceanDiff query = new QueryUpdateGodHeroAcrossOceanDiff(Uid, difficulty);
            server.GameDBPool.Call(query);

            MSG_ZGC_GOD_PATH_ACROSS_OCEAN_NEW_DUNGEON msg = new MSG_ZGC_GOD_PATH_ACROSS_OCEAN_NEW_DUNGEON();
            msg.AcroessOceanDiff = difficulty;
            Write(msg);
        }

        public void GodPathGetHeroInfo()
        {
            MSG_ZGC_GOD_HERO_INFO msg = new MSG_ZGC_GOD_HERO_INFO();
            if (!CheckLimitOpen(LimitType.GodPath))
            {
                msg.ErrorCode = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }

            msg.AcroessOceanDiff = AcroessOceanDiff;
            foreach (var kv in GodPathManager.HeroList)
            {
                msg.GodHeroList.Add(kv.Value.GenerateGodHeroInfo());
            }

            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathUseItem(int heroId, ulong id, int count)
        {
            MSG_ZGC_GOD_PATH_USE_ITEM msg = new MSG_ZGC_GOD_PATH_USE_ITEM();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                hero = GodPathManager.AddGodPathHero(heroId);
            }

            if (hero.CheckMaxStage() && !hero.CurrStageIsReady())
            {
                Log.Warn($"player {uid} GodPathUseItem {id} max stage");
                msg.ErrorCode = (int)ErrorCode.GodPathMaxStage;
                Write(msg);
                return;
            }

            NormalItem item = bagManager.NormalBag.GetItem(id) as NormalItem;
            if (item == null)
            {
                Log.Warn($"player {uid} GodPathUseItem no item {id}");
                msg.ErrorCode = (int)ErrorCode.NoSuchItem;
                Write(msg);
                return;
            }

            if (item.PileNum < count)
            {
                Log.Warn($"player {uid} GodPathUseItem item not enough {id} num {item.PileNum}");
                msg.ErrorCode = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            if (hero.Affinity >= hero.GetNeedAffinity())
            {
                Log.Warn($"player {uid} GodPathUseItem Affinity enough curr num {hero.Affinity}");
                msg.ErrorCode = (int)ErrorCode.GodPathAffinityFull;
                Write(msg);
                return;
            }

            int addAffinity = GodPathLibrary.GetDrugAddAffinity(item.Id);
            hero.AddAffinity(addAffinity * count);

            DelItem2Bag(item, RewardType.NormalItem, count, ConsumeWay.ItemUse);
            SyncClientItemInfo(item);

            msg.HeroId = hero.HeroId;
            msg.Affinity = hero.Affinity;
            msg.Stage = hero.Stage;
            msg.CurrStageState = (int)hero.CurrStageState;

            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathFinishStageTasks(int heroId)
        {
            MSG_ZGC_GOD_FINISH_STAGE_TASK msg = new MSG_ZGC_GOD_FINISH_STAGE_TASK();
            msg.HeroId = heroId;

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null || heroInfo == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CheckFinished())
            {
                Log.Warn($"player {uid} GodPathFinishStageTasks not all fask finished hero {heroId}");
                msg.ErrorCode = (int)ErrorCode.GodPathTaskNotAllFinished;
                Write(msg);
                return;
            }

            if (hero.CurrStageIsFinished())
            {
                Log.Warn($"player {uid} GodPathFinishStageTasks curr stage is finished, stage {hero.Stage}");
                msg.ErrorCode = (int)ErrorCode.GodPathTaskAllFinished;
                Write(msg);
                return;
            }

            hero.SetStageFinished();

            if (hero.Stage >= GodPathLibrary.MaxStage)
            {
                heroInfo.IsGod = true;
                //同步
                SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
                SyncDbUpdateHeroItem(heroInfo);
            }

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.Affinity = hero.Affinity;
            msg.Stage = hero.Stage;
            msg.CurrStageState = (int)hero.CurrStageState;

            Write(msg);
        }

        public void BuyGodPathPower(int heroId, int count)
        {
            MSG_ZGC_GOD_PATH_BUY_POWER msg = new MSG_ZGC_GOD_PATH_BUY_POWER();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn($"player {uid} BuyGodPathPower curr stage is not openning, hero {heroId} stage {hero.Stage}");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathSevenDouluoFightTask task = hero.GetGodPath<GodPathSevenDouluoFightTask>(GodPathTaskType.SevenDouluoFight);
            if (task == null)
            {
                Log.Warn($"player {uid} BuyGodPathPower hero have not SevenDouluoFight task, hero {heroId} stage {hero.Stage}");
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            Data buyData = DataListManager.inst.GetData("Counter", (int)CounterType.GodPathPowerBuyCount);
            if (buyData == null)
            {
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //最低一次
            if (count <= 0) count = 1;

            int buyedCount = GetCounterValue(CounterType.GodPathPowerBuyCount);
            if (buyedCount + count > buyData.GetInt("MaxCount"))
            {
                Log.Warn($"player {uid} BuyGodPathPower hero had buyed max count, hero {heroId} buyedCount {buyedCount}");
                msg.ErrorCode = (int)ErrorCode.MaxBuyCount;
                Write(msg);
                return;
            }

            string costStr = DataListManager.inst.GetData("GodPathConfig", 1)?.GetString("SevenFightBuyPowerPrice");
            if (string.IsNullOrEmpty(costStr))
            {
                Log.Warn($"player {uid} BuyGodPathPower GodPathConfig.xml line SevenFightBuyPowerPrice error, hero {heroId}");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count; i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Log.Warn($"player {uid} BuyGodPathPower to hero {heroId} error: coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {costCoin}");
                msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, costCoin, ConsumeWay.BuyGodPathPower, count.ToString());

            task.AddHp(GodPathLibrary.SevenFightGivePower * count);

            UpdateCounter(CounterType.GodPathPowerBuyCount, count);

            msg.HeroId = heroId;
            msg.SevenFightHP = task.SevenFightHP;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }


        public void GodPathSevenFightStart(int heroId, int cardType)
        {
            MSG_ZGC_GOD_PATH_SEVEN_FIGHT_START msg = new MSG_ZGC_GOD_PATH_SEVEN_FIGHT_START();
            msg.HeroId = heroId;

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn($"player {uid} GodPathSevenFightStart hero {heroId} stage {hero.Stage} is not openning");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathSevenDouluoFightTask task = hero.GetGodPath<GodPathSevenDouluoFightTask>(GodPathTaskType.SevenDouluoFight);
            if (task == null)
            {
                Log.Warn($"player {uid} GodPathSevenFightStart hero have not SevenDouluoFight task, hero {heroId} stage {hero.Stage}");
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            GodPathStageFightInfo cardList = GodPathLibrary.GetStageCardList(heroId, task.SevenFightStage);
            GodPathCardFightInfo cardFightInfo = cardList?.GetCardFightInfo(task.SevenFightWinCount + 1);
            if (cardList == null || cardFightInfo == null)
            {
                Log.Warn($"player {uid} GodPathStageFightInfo or GodPathCardFightInfo error, hero {heroId} stage {task.SevenFightStage} fightCount {task.SevenFightWinCount + 1}");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.Check(null))
            {
                Log.Warn($"player {uid} GodPathSevenFightStart curr task finished, hero {heroId} stage {hero.Stage} SevenFightStage {task.SevenFightStage}");
                msg.ErrorCode = (int)ErrorCode.GodPathThisTaskFinished;
                Write(msg);
                return;
            }

            GodPathCardType type = (GodPathCardType)cardType;
            if (type == GodPathCardType.Super)
            {
                if (!CheckCoins(CurrenciesType.diamond, GodPathLibrary.CardPrice))
                {
                    Log.Warn($"player {uid} GodPathSevenFightStart failed : coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost { GodPathLibrary.CardPrice}");
                    msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                    Write(msg);
                    return;
                }
                DelCoins(CurrenciesType.diamond, GodPathLibrary.CardPrice, ConsumeWay.GodPathFight, heroId.ToString());
            }
            else
            {
                if (task.SevenFightHP < GodPathLibrary.SevenFightCostPower)
                {
                    Log.Warn($"player {uid} GodPathSevenFightStart hero have not enough HP, hero {heroId} stage {hero.Stage} HP {task.SevenFightHP}");
                    msg.ErrorCode = (int)ErrorCode.GodPathPowerNotEnough;
                    Write(msg);
                    return;
                }
            }

            int randomCard = cardFightInfo.RandomCard();
            bool isWin = GodPathLibrary.IsRestrain(type, (GodPathCardType)randomCard);
            if (isWin)
            {
                if (cardFightInfo.IsReward)
                {
                    RewardManager manager = new RewardManager();
                    cardList.Rewards.ForEach(x => manager.AddSimpleReward(x));
                    manager.BreakupRewards();

                    AddRewards(manager, ObtainWay.GodPathSevenFight);
                    manager.GenerateRewardMsg(msg.Rewards);
                }

                task.SetWinState();
            }
            else
            {
                task.SetLoseState();
                task.AddHp(GodPathLibrary.SevenFightCostPower * -1);
            }

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.IsWin = isWin;
            msg.Stage = task.SevenFightStage;
            msg.WinCount = task.SevenFightWinCount;
            msg.EnemyCard = randomCard;
            msg.State = task.SevenFightState;
            msg.SevenFightHP = task.SevenFightHP;

            Write(msg);
        }

        public void GodPathSevenFightNextStage(int heroId)
        {
            MSG_ZGC_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE msg = new MSG_ZGC_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE();
            msg.HeroId = heroId;

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn($"player {uid} GodPathSevenFightNextStage  hero {heroId} stage {hero.Stage} is not openning");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathSevenDouluoFightTask task = hero.GetGodPath<GodPathSevenDouluoFightTask>(GodPathTaskType.SevenDouluoFight);
            if (task == null)
            {
                Log.Warn($"player {uid} GodPathSevenFightNextStage hero have not SevenDouluoFight task, hero {heroId} stage {hero.Stage}");
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            if (task.SevenFightState != GodPathSevenDouluoFightTask.WinState)
            {
                Log.Warn($"player {uid} GodPathSevenFightNextStage, hero {heroId} stage {hero.Stage} state is not win, can not go to next stage");
                msg.ErrorCode = (int)ErrorCode.GodPathFightIsNotWin;
                Write(msg);
                return;
            }

            if (task.SevenFightHP < GodPathLibrary.SevenFightNextStagePower)
            {
                Log.Warn($"player {uid} GodPathSevenFightNextStage, hero {heroId} stage {hero.Stage} HP {task.SevenFightHP} not enough, can not go to next stage");
                msg.ErrorCode = (int)ErrorCode.GodPathPowerNotEnough;
                Write(msg);
                return;
            }

            task.AddHp(-1 * GodPathLibrary.SevenFightNextStagePower);
            task.AddWinCount();

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.Stage = task.SevenFightStage;
            msg.State = task.SevenFightState;
            msg.WinCount = task.SevenFightWinCount;
            msg.SevenFightHP = task.SevenFightHP;

            Write(msg);
        }


        public void GodPathBuyBodyTrainShield(int heroId)
        {
            MSG_ZGC_GOD_PATH_TRAIN_BODY_BUY msg = new MSG_ZGC_GOD_PATH_TRAIN_BODY_BUY();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: curr stage is not openning");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTarinBodyTask task = hero.GetGodPath<GodPathTarinBodyTask>(GodPathTaskType.TarinBody);
            if (task == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find task type TarinBody");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.TrainBodyBuy)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: can not buy train body");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!CheckCoins(CurrenciesType.diamond, GodPathLibrary.TrainBodyBuyCost))
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {GodPathLibrary.TrainBodyBuyCost}");
                msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            task.SetBuy();

            DelCoins(CurrenciesType.diamond, GodPathLibrary.TrainBodyBuyCost, ConsumeWay.BuyFriendHeartGiveCount, heroId.ToString());

            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathTrainBody(int heroId)
        {
            MSG_ZGC_GOD_PATH_TRAIN_BODY msg = new MSG_ZGC_GOD_PATH_TRAIN_BODY();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find hero info");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: curr stage is not openning");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTarinBodyTask task = hero.GetGodPath<GodPathTarinBodyTask>(GodPathTaskType.TarinBody);
            if (task == null)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: not find task type TarinBody");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.TrainBodyStage >= GodPathLibrary.TrainBodyMaxStage)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: train body had finished");
                msg.ErrorCode = (int)ErrorCode.GodPathTrainFinished;
                Write(msg);
                return;
            }

            int power = GodPathLibrary.GetWaterPower(task.TrainBodyStage + 1);
            int shield = task.CaculateShield();
            if (task.TrainBodyHP + shield <= power)
            {
                Log.Warn($"player {uid} god path hero {heroId} GodPathTrainBody error: train body hp not enough");
                msg.ErrorCode = (int)ErrorCode.GodPathTrainHPNotEnough;
                Write(msg);
                return;
            }

            if (shield < power)
            {
                task.DeleteHP(power - shield);
            }

            task.AddStage();

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.TrainBodyHP = task.TrainBodyHP;
            msg.TrainBodyStage = task.TrainBodyStage;
            msg.TrainBodyBuy = task.TrainBodyBuy;
            Write(msg);
        }


        public void GodPathBuyOceanHeartCount(int heroId)
        {
            MSG_ZGC_GOD_PATH_BUY_OCEAN_HEART msg = new MSG_ZGC_GOD_PATH_BUY_OCEAN_HEART();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathOceanHeartTask task = hero.GetGodPath<GodPathOceanHeartTask>(GodPathTaskType.OceanHeart);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }


            //最低一次
            int count = 1;

            int buyedCount = task.HeartBuyCount;
            if (buyedCount + count > GodPathLibrary.HeartMaxBuyCount)
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: buy {2} and max is {3}", Uid, heroId, buyedCount, GodPathLibrary.HeartMaxBuyCount);
                msg.ErrorCode = (int)ErrorCode.MaxBuyCount;
                Write(msg);
                return;
            }

            string costStr = GodPathLibrary.HeartBuyCountPrice;
            if (string.IsNullOrEmpty(costStr))
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: price is empty", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count; i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Log.Warn("player {0} god path hero {1} buy ocean heart count error: cost diamond is {2} and player have {3}",
                    Uid, heroId, costCoin, GetCoins(CurrenciesType.diamond));
                msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, costCoin, ConsumeWay.BuyGodPathHeart, heroId.ToString());

            task.AddCount(count);

            msg.HeroId = heroId;
            msg.BuyCount = task.HeartBuyCount;
            msg.UseCount = task.HeartUseCount;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathOceanHeartDraw(int heroId, int index)
        {
            MSG_ZGC_GOD_PATH_OCEAN_HEART_DRAW msg = new MSG_ZGC_GOD_PATH_OCEAN_HEART_DRAW();
            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathOceanHeartTask task = hero.GetGodPath<GodPathOceanHeartTask>(GodPathTaskType.OceanHeart);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            if (task.HeartState == (int)GodPathOceanHeartType.Repaint)
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: state is repaint", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查进度
            if (task.HeartCurrentValue >= GodPathLibrary.HeartMaxValue)
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: value {2} is max", Uid, heroId, task.HeartCurrentValue);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.HeartUseCount <= 0)
            {
                Log.Warn("player {0} god path hero {1} ocean heart draw error: has no use count", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            msg.OldReward = task.HeartRewards;


            int addValue = 0;
            if (!string.IsNullOrEmpty(task.HeartRewards))
            {
                //有奖励，领取奖励
                string[] rewards = StringSplit.GetArray("|", task.HeartRewards);
                if (rewards.Length > 0)
                {
                    int rand = NewRAND.Next(0, rewards.Length - 1);
                    int rewardId = int.Parse(rewards[rand]);
                    Data data = DataListManager.inst.GetData("GodPathHeartReward", rewardId);
                    if (data != null)
                    {
                        addValue = data.GetInt("Value");

                        if (task.HeartDailyCount >= GodPathLibrary.HeartRewardTreble)
                        {
                            addValue *= GodPathLibrary.HeartRewardTrebleNum;
                        }
                        else if (task.HeartDailyCount >= GodPathLibrary.HeartRewardDouble)
                        {
                            addValue *= GodPathLibrary.HeartRewardDoubleNum;
                        }

                        task.SetHeartCurrentValue(addValue);

                        msg.RewardId = rewardId;
                        msg.AddValue = addValue;

                    }
                }
            }

            //生成奖励
            task.SetDrawReward();

            msg.HeroId = heroId;
            msg.Index = index;
            msg.CurrentValue = task.HeartCurrentValue;
            msg.NextReward = task.HeartRewards;
            msg.DailyCount = task.HeartDailyCount;
            msg.UseCount = task.HeartUseCount;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathOceanHeartRepaint(int heroId)
        {
            MSG_ZGC_GOD_PATH_REPAINT_OCEAN_HEART msg = new MSG_ZGC_GOD_PATH_REPAINT_OCEAN_HEART();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} repaint ocean heart error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} repaint ocean heart error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathOceanHeartTask task = hero.GetGodPath<GodPathOceanHeartTask>(GodPathTaskType.OceanHeart);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} repaint ocean heart error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            if (task.HeartState == (int)GodPathOceanHeartType.Repaint)
            {
                Log.Warn("player {0} god path hero {1} repaint ocean heart error: state is repaint", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.HeartCurrentValue < GodPathLibrary.HeartMaxValue)
            {
                Log.Warn("player {0} god path hero {1} repaint ocean heart error: value {2} not max", Uid, heroId, task.HeartCurrentValue);
                msg.ErrorCode = (int)ErrorCode.GodPathHaertNotFull;
                Write(msg);
                return;
            }

            task.ChangeState();

            msg.HeroId = heroId;
            msg.State = task.HeartState;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }


        public void GodPathBuyTridentCount(int heroId)
        {
            MSG_ZGC_GOD_PATH_BUY_TRIDENT msg = new MSG_ZGC_GOD_PATH_BUY_TRIDENT();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTridentTask task = hero.GetGodPath<GodPathTridentTask>(GodPathTaskType.Trident);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }


            //最低一次
            int count = 1;

            int buyedCount = task.BuyCount;
            if (buyedCount + count > GodPathLibrary.TridentMaxBuyCount)
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: buy {2} and max is {3}",
                    Uid, heroId, buyedCount, GodPathLibrary.TridentMaxBuyCount);
                msg.ErrorCode = (int)ErrorCode.MaxBuyCount;
                Write(msg);
                return;
            }

            string costStr = GodPathLibrary.TridentBuyCountPrice;
            if (string.IsNullOrEmpty(costStr))
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: price is empty", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count; i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Log.Warn("player {0} god path hero {1} buy trident count error: cost diamond is {2} and player have {3}",
            Uid, heroId, costCoin, GetCoins(CurrenciesType.diamond));
                msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, costCoin, ConsumeWay.BuyGodPathTrident, heroId.ToString());

            task.AddCount(count);

            msg.HeroId = heroId;
            msg.BuyCount = task.BuyCount;
            msg.UseCount = task.UseCount;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathBuyTridentUse(int heroId, bool strategyBuy)
        {
            MSG_ZGC_GOD_PATH_USE_TRIDENT msg = new MSG_ZGC_GOD_PATH_USE_TRIDENT();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} trident use error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} trident use error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTridentTask task = hero.GetGodPath<GodPathTridentTask>(GodPathTaskType.Trident);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} trident use error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            //检查进度
            if (!task.CanAddValue())
            {
                Log.Warn("player {0} god path hero {1} trident use error: value {2} is max", Uid, heroId, task.CurrentValue);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.UseCount <= 0)
            {
                Log.Warn("player {0} god path hero {1} trident use error: has no use count", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (strategyBuy)
            {
                //花钱
                CurrenciesType costType = (CurrenciesType)GodPathLibrary.TridentStrategyType;
                int costCoin = GodPathLibrary.TridentStrategyPrice;

                if (!CheckCoins(costType, costCoin))
                {
                    Log.Warn("player {0} god path hero {1} trident use error: cost diamond is {2} and player have {3}",
                                    Uid, heroId, costCoin, GetCoins(CurrenciesType.diamond));
                    msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                    Write(msg);
                    return;
                }
                DelCoins(costType, costCoin, ConsumeWay.BuyGodPathTridentStrategy, heroId.ToString());
            }

            task.Use(strategyBuy);

            msg.HeroId = heroId;
            msg.StrategyBuy = strategyBuy;
            msg.UseCount = task.UseCount;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathBuyTridentResult(int heroId, bool randomBuy, bool isSuccess)
        {
            MSG_ZGC_GOD_PATH_TRIDENT_RESULT msg = new MSG_ZGC_GOD_PATH_TRIDENT_RESULT();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} trident resule error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} trident resule error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTridentTask task = hero.GetGodPath<GodPathTridentTask>(GodPathTaskType.Trident);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} trident resule error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            //检查进度
            if (!task.CanAddValue())
            {
                Log.Warn("player {0} god path hero {1} trident resule error: value {2} is max", Uid, heroId, task.CurrentValue);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!task.UseStart)
            {
                //没扣次数直接出结果了
                Log.Warn("player {0} god path hero {1} trident resule error: start is false", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            //检查进度
            if (!task.CanAddValue())
            {
                Log.Warn("player {0} god path hero {1} trident resule error: value {2} is max", Uid, heroId, task.CurrentValue);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int addPro = 0;

            if (isSuccess)
            {
                if (task.CheckUstTime())
                {
                    addPro = GodPathLibrary.TridentSuccessAdd;

                    if (randomBuy)
                    {
                        //花钱
                        CurrenciesType costType = (CurrenciesType)GodPathLibrary.TridentRandomType;
                        int costCoin = GodPathLibrary.TridentRandomPrice;

                        if (!CheckCoins(costType, costCoin))
                        {
                            Log.Warn("player {0} god path hero {1} trident resule error: cost diamond is {2} and player have {3}",
                              Uid, heroId, costCoin, GetCoins(CurrenciesType.diamond));
                            msg.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                            Write(msg);
                            return;
                        }
                        DelCoins(costType, costCoin, ConsumeWay.BuyGodPathTridentRandom, heroId.ToString());

                        //购买成功计算翻倍奖励
                        int ratio = GodPathLibrary.GeTridentRandomRatio();
                        addPro += GodPathLibrary.TridentGreatAdd * ratio;

                        switch (ratio)
                        {
                            case 0:
                                msg.RandomResult = (int)GodPathTridentRandomType.Fail;
                                break;
                            case 1:
                                msg.RandomResult = (int)GodPathTridentRandomType.Success;
                                break;
                            default:
                                msg.RandomResult = (int)GodPathTridentRandomType.Great;
                                break;
                        }
                    }
                    else
                    {
                        //不翻倍
                        msg.RandomResult = (int)GodPathTridentRandomType.No;
                    }
                }
                else
                {
                    //失败
                    addPro = GodPathLibrary.TridentFailAdd;
                    //不翻倍
                    msg.RandomResult = (int)GodPathTridentRandomType.No;
                }
            }
            else
            {
                //失败
                addPro = GodPathLibrary.TridentFailAdd;
                //不翻倍
                msg.RandomResult = (int)GodPathTridentRandomType.No;
            }

            task.SetResult(addPro);

            msg.HeroId = heroId;
            msg.RandomBuy = randomBuy;
            msg.IsSuccess = isSuccess;
            msg.AddPro = addPro;
            msg.CurrentValue = task.CurrentValue;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathBuyTridentPush(int heroId)
        {
            MSG_ZGC_GOD_PATH_PUSH_TRIDENT msg = new MSG_ZGC_GOD_PATH_PUSH_TRIDENT();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} trident push error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} trident push error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathTridentTask task = hero.GetGodPath<GodPathTridentTask>(GodPathTaskType.Trident);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} trident push error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            if (task.State == (int)GodPathTridentType.Push)
            {
                Log.Warn("player {0} god path hero {1} trident push error: state is push", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.CanAddValue())
            {
                Log.Warn("player {0} god path hero {1} trident push error: value {2} not max", Uid, heroId, task.CurrentValue);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            task.ChangeState();

            msg.HeroId = heroId;
            msg.State = task.State;
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GodPathAcrossOceanLightPuzzle(int heroId, int index)
        {
            MSG_ZGC_GOD_PATH_LIGHT_PUZZLE msg = new MSG_ZGC_GOD_PATH_LIGHT_PUZZLE();

            GodPathHero hero = GodPathManager.GetGodPathHero(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} god path hero {1} light puzzle error: not find hero info", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!hero.CurrStageIsOpening())
            {
                Log.Warn("player {0} god path hero {1} light puzzle error: stage not open", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathCurrStageNotOpen;
                Write(msg);
                return;
            }

            GodPathAcrossOceanTask task = hero.GetGodPath<GodPathAcrossOceanTask>(GodPathTaskType.AcrossOcean);
            if (task == null)
            {
                Log.Warn("player {0} god path hero {1} light puzzle error: not find hero task", Uid, heroId);
                msg.ErrorCode = (int)ErrorCode.GodPathNoThisTask;
                Write(msg);
                return;
            }

            if (task.Check(null))
            {
                Log.Warn($"player {Uid} god path hero {heroId} light puzzle error: finished");
                msg.ErrorCode = (int)ErrorCode.GodPathThisTaskFinished;
                Write(msg);
                return;
            }

            if (index <= 0 || index > GodPathLibrary.AcrossOceanPuzzleCount)
            {
                Log.Warn($"player {Uid} god path hero {heroId} light puzzle error: index {index} error");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (task.Puzzle.Contains(index))
            {
                Log.Warn($"player {Uid} god path hero {heroId} light puzzle error: puzzle {index} had lighted");
                msg.ErrorCode = (int)ErrorCode.GodPathCurrPuzzleLighted;
                Write(msg);
                return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(GodPathLibrary.AcrossOceanItemId);
            if (item == null || item.PileNum < GodPathLibrary.AcrossOceanPuzzleCost)
            {
                Log.Debug($"player {Uid} god path hero {heroId} light puzzle error: item {GodPathLibrary.AcrossOceanItemId} not enough");
                msg.ErrorCode = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            item = DelItem2Bag(item, RewardType.NormalItem, GodPathLibrary.AcrossOceanPuzzleCost, ConsumeWay.GodPathLightPuzzle);
            if (item != null)
            {
                SyncClientItemInfo(item);
            }

            List<int> lighted = new List<int>() { index };
            task.SetLightedPuzzle(index);

            //判断是否有额外点亮的
            int extraLight = GodPathLibrary.GetExtraLight(heroId, index);
            if (extraLight > 0 && !task.IsLighted(extraLight))
            {
                lighted.Add(extraLight);
                task.SetLightedPuzzle(extraLight);
            }
            msg.CurrLighted = string.Join("|", lighted);

            task.SyncDBAcrossOceanInfo();

            msg.HeroId = heroId;
            msg.Finished = task.Check(null);
            msg.ErrorCode = (int)ErrorCode.Success;
            msg.AcroessOceanPuzzle = task.PuzzleItemToString();
            Write(msg);
        }

        public void AcrossOceanSweep(int id)
        {
            MSG_ZGC_GOD_PATH_ACROSS_OCEAN_SWEEP msg = new MSG_ZGC_GOD_PATH_ACROSS_OCEAN_SWEEP();

            DungeonModel model = DungeonLibrary.GetDungeon(id);
            if (model == null)
            {
                Log.Warn($"player {Uid} across ocean sweep {id} error: not find dungeon");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int restSweepCount = GetCounterRestCount(CounterType.AcrossOceanCount, CounterType.AcrossOceanBuyCount);
            if (restSweepCount <= 0)
            {
                Log.Warn($"player {Uid} across ocean sweep {id} error: rest sweep count {restSweepCount} not enough");
                msg.ErrorCode = (int)ErrorCode.SweepCountNotEnough;
                Write(msg);
                return;
            }

            if ((int)model.Difficulty > AcroessOceanDiff)
            {
                Log.Warn($"player {Uid} god path across ocean sweep {id} error: not passed dungeonId {id}");
                msg.ErrorCode = (int)ErrorCode.CanNotSweepAccrossOcean;
                Write(msg);
                return;
            }

            UpdateCounter(CounterType.AcrossOceanCount, 1);

            RewardManager manager = new RewardManager();
            //manager.AddSimpleRewardWithSoulBoneCheck(model.Data.GetString("GeneralReward"));
            List<ItemBasicInfo> getList = AddRewardDrop(model.Data.GetIntList("GeneralRewardId", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.SecretAreaSweep);

            //扫荡任务完成
            MapModel tempMapModel = MapLibrary.GetMap(model.Id);
            if (tempMapModel != null)
            {
                AddTaskNumForType(TaskType.CompleteDungeons, 1, true, tempMapModel.MapType);
                AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, tempMapModel.MapType);

                AddPassCardTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
                AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);

                AddSchoolTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
                AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
                
                AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, tempMapModel.MapType);
                AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, tempMapModel.MapType);
            }
            else
            {
                Log.Warn("player {0} AcrossOceanSweep id {1} not find DungeonId {2}", Uid, id, model.Id);
            }
            AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, model.Id);
            AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, model.Id);

            //完成通行证任务
            AddPassCardTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);

            //完成学院任务
            AddSchoolTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);
            AddSchoolTaskNum(TaskType.CompleteDungeonList, model.Id, TaskParamType.DUNGEON_LIST);

            manager.GenerateRewardMsg(msg.Rewards);
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
            AddRunawayActivityNumForType(RunawayAction.Fight);
        }
    }
}

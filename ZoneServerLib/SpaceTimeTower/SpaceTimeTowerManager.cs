using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility.SpaceTimeTower;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;

namespace ZoneServerLib
{
    public class SpaceTimeTowerManager
    {
        private PlayerChar owner;
        public PlayerChar Owner { get { return owner; } }
        /// <summary>
        /// 当前层数
        /// </summary>
        public int TowerLevel { get; private set; }
        /// <summary>
        /// 当日通过的最高层数
        /// </summary>
        public int PassedTopLevel { get; private set; }

        /// <summary>
        /// 伙伴池 
        /// </summary>
        private Dictionary<int, int> heroPool = new Dictionary<int, int>(); /*/ <位置 (从0开始), 英雄id> /*/
        public Dictionary<int, int> HeroPool { get { return heroPool; } }

        /// <summary>
        /// 选择闯塔的伙伴
        /// </summary>
        /// key:heroId
        private Dictionary<int, SpaceTimeHeroInfo> heroTeam;
        public Dictionary<int, SpaceTimeHeroInfo> HeroTeam { get { return heroTeam; } }

        /// <summary>
        /// 闯关阵容
        /// </summary>
        /// key:heroId
        private Dictionary<int, SpaceTimeHeroInfo> heroQueue;
        public Dictionary<int, SpaceTimeHeroInfo> HeroQueue { get { return heroQueue; } }

        /// <summary>
        /// 当前卡池刷新次数 默认为0
        /// </summary>
        /// <returns></returns>
        private int iCurrRefreshNum;
        public int ICurrRefreshNum
        {
            get => iCurrRefreshNum;
            set => iCurrRefreshNum = value;
        }

        public SpaceTimeBaseEvent TowerEvent { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        private DateTime nextUpdateTime = DateTime.Now;

        /// <summary>
        /// 魂导器剩余次数
        /// </summary>
        /// key:itemId, value:restCount剩余次数
        private Dictionary<int, int> guideSoulRestCounts;
        public  Dictionary<int, int> GuideSoulRestCounts
        {
            get { return guideSoulRestCounts; }
        }

        #region [阶段奖励相关属性]

        /*\ 领取过得奖励id <表id> /*/
        private List<int> lstGetAwardId;

        public List<int> LstGetAwardId
        {
            get => lstGetAwardId;
            set => lstGetAwardId = value;
        }

        #endregion

        #region [商城]

        /*\ 商城或时空屋 跟时间类型有关系 /*/
        private SpaceTimeTowerShopInfo oShopInfo;

        public SpaceTimeTowerShopInfo OShopInfo
        {
            get => oShopInfo;
            set => oShopInfo = value;
        }

        /*\ 奖励信息不入库（用来同步客户端） /*/
        public RepeatedField<REWARD_ITEM_INFO> lstAwardInfo = new RepeatedField<REWARD_ITEM_INFO>();
        #endregion
      
        /// <summary>
        /// 挑战失败次数
        /// </summary>
        public int FailCount { get; private set; }
        /// <summary>
        /// 消耗的活动货币
        /// </summary>
        public int ConsumeCoins { get; private set; }

        /// <summary>
        /// 凶兽结算时可供玩家选择的魂导器
        /// </summary>
        /// key:guideSoulId, value:spaceTimeCoinNum
        private Dictionary<int, int> optionalGuideSoulItems = new Dictionary<int, int>();
        public Dictionary<int, int> OptionalGuideSoulItems
        {
            get { return optionalGuideSoulItems; }
        }
        
        /// <summary>
        /// 玩家是否点了开始按钮
        /// </summary>
        public bool Started{ get; private set; }
        /// <summary>
        /// 个人难度
        /// </summary>
        public int PersonalPeriod { get; private set; }
        /// <summary>
        /// 个人是否通关
        /// </summary>
        public bool PersonalPassed { get; private set; }
        /// <summary>
        /// 往期通过的难度
        /// </summary>
        public int PeriodPassedBefore{ get; private set; }
        /// <summary>
        /// 阶段奖励期数
        /// </summary>
        public int StageRewardPeriod { get; private set; }
        /// <summary>
        /// 往期奖励领取状态
        /// </summary>
        public bool PastRewardsState { get; private set; }
        /// <summary>
        /// 当前是活动的第几周
        /// </summary>
        public int Week { get; private set; }

        public SpaceTimeTowerManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(SpaceTimeTowerInfo info, List<SpaceTimeHeroInfo> list)
        {
            InitHeroInfo(list);
            InitTowerInfo(info);
            CheckActivityOpen();
        }

        private void InitTowerInfo(SpaceTimeTowerInfo info)
        {
            TowerLevel = info.TowerLevel;
            PassedTopLevel = info.PassedTopLevel;
            heroPool = info.HeroPool;
            FailCount = info.FailCount;
            ConsumeCoins = info.ConsumeCoins;
            StartTime = info.StartTime;
            EndTime = StartTime.AddDays(SpaceTimeTowerLibrary.OpenDays);
            LstGetAwardId = info.lstGetAwardId;
            guideSoulRestCounts = info.GuideSoulRestCounts;
            optionalGuideSoulItems = info.OptionalGuideSoulItems;
            Started = info.Started;
            PersonalPeriod = info.Period;
            PersonalPassed = info.Passed;
            PeriodPassedBefore = info.PeriodPassedBefore;
            StageRewardPeriod = info.StageRewardPeriod;
            PastRewardsState = info.PastRewardsState;
            //初始化事件 只有第一层可能是0
            if (info.EventType == 0)
            {
                RandomTowerEvent();
                SyncDbUpdateSpaceTimeTower();
            }
            else
            {
                TowerEvent = CreateSpaceTimeEvent(info.EventType, info.ParamList);
            }
            if (info.EventType == (int) SpaceTimeEventType.Shop ||
                info.EventType == (int) SpaceTimeEventType.House)
            {
                oShopInfo = new SpaceTimeTowerShopInfo((SpaceTimeEventType) info.EventType)
                {
                    DicBuyNum = info.dicBuyNum,
                    LstProductId = info.lstProductId
                };
            }

            UpdateActivityWeek();
        }

        private SpaceTimeBaseEvent CreateSpaceTimeEvent(int eventType, List<int> paramList) 
        {
            switch ((SpaceTimeEventType)eventType)
            {
                case SpaceTimeEventType.Dungeon:
                    return new SpaceTimeDungeonEvent(eventType, paramList, this, owner.server);
                case SpaceTimeEventType.Shop:
                    return new SpaceTimeShopEvent(eventType, paramList, this, owner.server);
                case SpaceTimeEventType.House:
                    return new SpaceTimeHouseEvent(eventType, paramList, this, owner.server);
                default:
                    Log.Warn($"player {owner.Uid} create space time event {eventType} error");
                    return null;
            }
        }

        public void RandomTowerEvent()
        {
            SpaceTimeTowerLevel towerLevelModel = SpaceTimeTowerLibrary.GetTowerLevelModel(TowerLevel);
            if (towerLevelModel == null)
            {
                Log.Warn($"player {owner.Uid} random tower level {TowerLevel} event error");
                return;
            }
            int eventType = towerLevelModel.RandomEventType();
            List<int> paramList = RandomTowerEventParam((SpaceTimeEventType)eventType);
            TowerEvent = CreateSpaceTimeEvent(eventType, paramList);
            InitShopInfo((SpaceTimeEventType)eventType, paramList);
        }

        public List<int> RandomTowerEventParam(SpaceTimeEventType eventType)
        {
            List<int> paramList = new List<int>();
            SpaceTimeTowerLevel towerLevelModel = SpaceTimeTowerLibrary.GetTowerLevelModel(TowerLevel);
            if (towerLevelModel == null)
            {
                Log.Warn($"player {owner.Uid} random tower level {TowerLevel} event param error");
                return paramList;
            }
            switch (eventType)
            {
                case SpaceTimeEventType.Dungeon:
                    paramList.Add(towerLevelModel.RandomDungeon());
                    break;
                case SpaceTimeEventType.Shop:
                    // case SpaceTimeEventType.House:
                    int iShopId = RetCurrShopId(eventType);
                    /*\ 随机商城数据 /*/
                    List<int> productList = RandomProduct(iShopId);
                    if (productList != null)
                    {
                        paramList.AddRange(productList);
                    }
                    break;
                default:
                    break;
            }
            if (paramList.Count == 0 && eventType == SpaceTimeEventType.Dungeon)
            {
                Log.Warn($"player {owner.Uid} random tower level {TowerLevel} event {eventType} param error");
            }
            return paramList;
        }

        public  bool RandomHouseEventParam()
        {
            SpaceTimeTowerLevel towerLevelModel = SpaceTimeTowerLibrary.GetTowerLevelModel(TowerLevel);
            if (towerLevelModel == null)
            {
                Log.Warn($"player {owner.Uid} random tower level {TowerLevel} house event param error");
                return false;
            }
            int iShopId = RetCurrShopId(SpaceTimeEventType.House);
            /*\ 随机商城数据 /*/
            List<int> productList = RandomProduct(iShopId);
            if (productList != null)
            {
                TowerEvent.ParamList.Clear();
                TowerEvent.ParamList.AddRange(productList);
                InitShopInfo(SpaceTimeEventType.House, productList);
                SyncDbUpdateSpaceTimeTower();
                return true;
            }
            return false;
        }
        
        public void TowerLevelUp()
        {
            //通过指定层数发放称号
            List<int> paramList = new List<int>() { TowerLevel };
            owner.TitleMng.UpdateTitleConditionCount(TitleObtainCondition.PassSpaceTimeTower, 1, paramList);
            
            PassedTopLevel = Math.Max(PassedTopLevel, TowerLevel);
            
            if (TowerLevel < SpaceTimeTowerLibrary.TowerMaxLevel)
            {
                TowerLevel++;
                owner.ResetCardPoolRefreshNum();
                if (oShopInfo != null)
                {
                    oShopInfo.ClearShopInfo();
                }
                //随机下一层的事件
                RandomTowerEvent();
            }
            else if (TowerLevel == SpaceTimeTowerLibrary.TowerMaxLevel)
            {
                TowerLevel++;
                owner.server.SpaceTimeTowerManager.UpdateMonsterPassedState();
                PersonalPassed = true;
                SyncDbUpdatePersonalPeriod();
            }
            
            SyncDbUpdateSpaceTimeTower();
            //BI埋点
            owner.RecordPetBITowerLog(TowerLevel-1, 1);
        }

        private void InitHeroInfo(List<SpaceTimeHeroInfo> list)
        {
            heroTeam = new Dictionary<int, SpaceTimeHeroInfo>();
            heroQueue = new Dictionary<int, SpaceTimeHeroInfo>();

            foreach (var hero in list)
            {
                heroTeam.Add(hero.Id, hero);
                if (hero.PositionNum >= 0)
                {
                    AddHeroToQueue(hero);
                }
            }
        }

        public void HeroJoinTeam(int index, int heroId)
        {
            RemoveHeroFromPool(index);
            AddHeroToTeam(heroId);
        }

        private void RemoveHeroFromPool(int index)
        {
            heroPool.Remove(index);
            SyncDbUpdateSpaceTimeTower();
        }

        private void AddHeroToTeam(int heroId)
        {
            SpaceTimeHeroInfo heroInfo = new SpaceTimeHeroInfo(heroId, SpaceTimeTowerLibrary.StepInitLevel);
            heroTeam.Add(heroId, heroInfo);
            BindHeroNature(heroInfo);
            SyncDbInsertHero(heroInfo);
        }

        public void RemoveHeroFromTeam(int heroId)
        {
            heroTeam.Remove(heroId);
            SyncDbDeleteHero(heroId);
        }

        public ErrorCode HeroStepLevelUp(int heroId, int index, List<SpaceTimeHeroInfo> updateList)
        {
            SpaceTimeHeroInfo heroInfo;
            if (HeroTeam.TryGetValue(heroId, out heroInfo))
            {
                if (heroInfo.StepLevel < SpaceTimeTowerLibrary.StepMaxLevel)
                {
                    heroInfo.StepLevelUp();
                    BindHeroNature(heroInfo);
                    updateList.Add(heroInfo);
                    RemoveHeroFromPool(index);
                    return ErrorCode.Success;
                }
                return ErrorCode.SpaceTimeTowerHeroStarIsMax;
            }
            return ErrorCode.SpaceTimeTowerHeroTroopNotExist;
        }

        public SpaceTimeHeroInfo GetHeroInfo(int heroId)
        {
            SpaceTimeHeroInfo heroInfo;
            HeroTeam.TryGetValue(heroId, out heroInfo);
            return heroInfo;
        }

        public void UpdateHeroQueue(SpaceTimeHeroInfo heroInfo, int positionNum, List<SpaceTimeHeroInfo> updateList, Dictionary<int, SpaceTimeHeroInfo> oldQueue)
        {
            if (oldQueue.ContainsKey(heroInfo.Id))
            {
                oldQueue.Remove(heroInfo.Id);
            }
            heroInfo.SetPostionNum(positionNum);
            AddHeroToQueue(heroInfo);
            updateList.Add(heroInfo);
        }

        // private SpaceTimeHeroInfo RemoveOriginHeroPos(int srcPosNum)
        // {
        //     SpaceTimeHeroInfo heroInfo = null;
        //     if (HeroQueue.TryGetValue(srcPosNum, out heroInfo))
        //     {
        //         heroInfo.SetPostionNum(0);
        //         HeroQueue.Remove(srcPosNum);
        //     }
        //     return heroInfo;
        // }

        private void AddHeroToQueue(SpaceTimeHeroInfo heroInfo)
        {
            heroQueue.Add(heroInfo.Id, heroInfo);
        }

        public void RemoveHeroFromQueue(int heroId)
        {
            heroQueue.Remove(heroId);
        }

        public void BindHerosNature()
        {
            if (heroTeam == null)
            {
                return;
            }
            foreach (var kv in heroTeam)
            {
                BindHeroNature(kv.Value);
            }
        }

        public void BindHeroNature(SpaceTimeHeroInfo heroInfo)
        {
            //基础属性
            NatureDataModel heroBasicNatureModel = SpaceTimeTowerLibrary.GetHeroBasicNatureModel(heroInfo.Id);
            if (heroBasicNatureModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitSpaceTimeHeroNatureInfo GetHeroBasicNatureModel is null, hero id is {1}", owner.Uid, heroInfo.Id);
            }
            heroInfo.Nature.Clear();
            //两个属性相乘
            Dictionary<NatureType, Int64> basicNatures = owner.HeroMng.GetBasicNatureList(heroBasicNatureModel, null, null);
            //9项基础属性
            heroInfo.InitBasicNature(basicNatures);
            //添加进阶加成
            HeroAddStepRatio(heroInfo);
            //成神
            //int godType = owner.GetHeroGod(heroInfo.Id);
            //HeroGodDetailModel detilModel = GodHeroLibrary.GetHeroGodDetailModel(godType);
            //if (heroInfo != null && detilModel != null)
            //{
            //    GodHeroLibrary.NatureTypes.ForEach(x => heroInfo.AddNatureRatio(x, detilModel.NatureRatio));
            //}
            //初始化移动速度
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            heroInfo.InitSpeed(heroModel.PRO_RUN_IN_BATTLE, heroModel.PRO_RUN_OUT_BATTLE);
            //最后设置
            heroInfo.InitHp();
        }

        public void HeroAddStepRatio(SpaceTimeHeroInfo info)
        {
            int sC = 0;
            GroValFactorModel stepsModel = SpaceTimeTowerLibrary.GetHeroStepGrowthModel(info.StepLevel);
            if (stepsModel != null)
            {
                sC = stepsModel.StepsC;
            }
            if (sC > 0)
            {
                foreach (var type in NatureLibrary.Basic9Nature)
                {
                    info.AddNatureRatio(type.Key, sC);
                }
            }

            HeroModel heroModel = HeroLibrary.GetHeroModel(info.Id);
            if (heroModel == null) return;

            HeroStepNatureModel natureModel = SpaceTimeTowerLibrary.GetHeroStepNatureModel(heroModel.Quality, info.StepLevel);
            if (natureModel == null) return;

            natureModel.NatureList?.ForEach(x => info.AddNatureAddedValue(x.Key, (long)x.Value));
        }

        public int GetHeroPos(int heroId)
        {
            int pos = -1;
            SpaceTimeHeroInfo heroInfo;
            if (HeroQueue.TryGetValue(heroId, out heroInfo))
            {
                pos = heroInfo.PositionNum;
            }
            return pos;
        }

        public void CLearHeroQueue()
        {
            heroQueue.Clear();
        }
        
        public void CheckTime()
        {
            if (!Owner.CheckLimitOpen(LimitType.SpaceTimeTower)) return;

            if (EndTime < Owner.server.Now())
            {
                Refresh();
            }
        }
        
        public void CheckActivityOpen()
        {
            if (!Owner.CheckLimitOpen(LimitType.SpaceTimeTower)) return;
            if (EndTime < Owner.server.Now())
            {
                SetOpenTime(false);
                if (IsOpening())
                {
                    Reset();
                }
            }
        }

        public void CheckActivityClose()
        {
            if (!Owner.CheckLimitOpen(LimitType.SpaceTimeTower)) return;

            if (EndTime < Owner.server.Now())
            {
                SetOpenTime();
                SyncDbUpdateSpaceTimeTower();
            }
        }
        
        public void ActivityOpen()
        {
            if (!Owner.CheckLimitOpen(LimitType.SpaceTimeTower)) return;
            Refresh();
        }
        
        private void Refresh()
        {
            SetOpenTime();
            Reset();
        }

        public void Reset(bool ignoreStageReward = false)
        {
            if (!ignoreStageReward)
            {
                UpdateActivityWeek();
            }
            TowerLevel = 1;
            owner.ResetCardPoolRefreshNum();
            FailCount = 0;
            UpdateStartState(false);
            //只在活动开启时重置
            if (!ignoreStageReward)
            {
                LstGetAwardId.Clear();
                UpdatePersonalPeriod();
                //阶段奖励期数
                StageRewardPeriod = PersonalPeriod;
                UpdatePastRewardsState(false);
                PassedTopLevel = 0;
            }
            ResetSpaceTimeCoins();
            RandomTowerEvent();
            ResetGuideSoulItemInfo();
            SyncDbUpdateSpaceTimeTower();
            
            ClearHeros();
            // if (oShopInfo != null)
            // {
            //     oShopInfo.ClearShopInfo();
            // }

            owner.SendSpaceTimeTowerInfo();
        }

        private void ResetSpaceTimeCoins()
        {
            ConsumeCoins = 0;
            int remainCoins = owner.GetCoins(CurrenciesType.spaceTimeCoin);
            if (remainCoins > SpaceTimeTowerLibrary.InitSpaceTimeCoins)
            {
                owner.DelCoins(CurrenciesType.spaceTimeCoin, remainCoins - SpaceTimeTowerLibrary.InitSpaceTimeCoins, ConsumeWay.SpaceTimeReset, "");
            }
            else
            {
                owner.AddCoins(CurrenciesType.spaceTimeCoin, SpaceTimeTowerLibrary.InitSpaceTimeCoins - remainCoins, ObtainWay.SpaceTimeReset, "");
            }
        }

        private void ClearHeros()
        {
            heroTeam.Clear();
            heroQueue.Clear();
            SyncDbDeleteAllHeros();
        }

        private void SetOpenTime(bool notify = true)
        {
            StartTime = GetStartTime();
            EndTime = StartTime.AddDays(SpaceTimeTowerLibrary.OpenDays);
            if (notify)
            {
                Owner.SendSpaceTimeTowerOpenTime();
            }
        }

        public DateTime GetStartTime()
        {
            DateTime nowDate = owner.server.Now().Date;
            int realWeekDay = nowDate.DayOfWeek == 0 ? 7 : (int)nowDate.DayOfWeek;
            int deltaDays = SpaceTimeTowerLibrary.StartWeekDay - realWeekDay;
            DateTime startWeekDay = nowDate.AddDays(deltaDays);
            TimeSpan weekTime = TimeSpan.Parse(SpaceTimeTowerLibrary.StartWeekTime);
            DateTime startTime = startWeekDay.Add(weekTime);
            return startTime;
        }

        public bool IsOpening()
        {
            if (!Owner.CheckLimitOpen(LimitType.SpaceTimeTower)) return false;

            DateTime now = Owner.server.Now();
            return now >= StartTime && now <= EndTime;
        }

        public void Update()
        {
            if (nextUpdateTime < Owner.server.Now())
            {
                CheckActivityClose();
                nextUpdateTime = Owner.server.Now().AddSeconds(10);
            }
        }

        public void ChallengeFail()
        {
            AddFailCount();
            TowerEvent?.SetOngoingState(false);
        }
        
        private void AddFailCount()
        {
            if (FailCount < SpaceTimeTowerLibrary.ChallengeMaxCount)
            {
                FailCount++;
                SyncDbUpdateSpaceTimeTower();
            }
        }

        public ErrorCode ExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            if (TowerEvent == null || !TowerEvent.CheckCanExecuteEvent(type, param, lstParam))
            {
                return ErrorCode.Fail;
            }
            ErrorCode result = TowerEvent.ExecuteEvent(type, param, lstParam);
            return result;
        }

        #region 魂导器
        public void AddGuideSoulItems(Dictionary<int, int> addList)
        {
            foreach (var item in addList)
            {
                if (!guideSoulRestCounts.ContainsKey(item.Key))
                {
                    guideSoulRestCounts.Add(item.Key, item.Value);
                }
                else
                {
                    guideSoulRestCounts[item.Key] = item.Value;
                }
            }
            SyncDbUpdateGuideSoulRestCounts();
        }
        
        public void ConsumeGuideSoulItems()
        {
            List<int> updateList = new List<int>();
            List<int> deleteList = new List<int>();
            foreach (var item in GuideSoulRestCounts)
            {
                RecordGuideSoulRestCountChange(item.Key, updateList, deleteList);
            }
            foreach (int id in updateList)
            {
                SubdGuideSoulRestCount(id);
            }
            foreach (int id in deleteList)
            {
                RemoveGuideSoulItem(id);
            }
            SyncDbUpdateGuideSoulRestCounts();
            owner.SendGuideSoulRestCountsInfo(deleteList, 1);
        }
        
        private void RecordGuideSoulRestCountChange(int id, List<int> updateList, List<int> deletelList)
        {
            int restCount;
            GuideSoulRestCounts.TryGetValue(id, out restCount);
            if (restCount != 0)
            {
                if (restCount == 1)
                {
                    deletelList.Add(id);
                }
                else
                {
                    updateList.Add(id);
                }
            }
        }

        private void SubdGuideSoulRestCount(int id)
        {
            if (guideSoulRestCounts.ContainsKey(id))
            {
                guideSoulRestCounts[id] -= 1;
            }
        }
        
        private void RemoveGuideSoulItem(int id)
        {
            guideSoulRestCounts.Remove(id);
        }
        
        public void DelGuideSoulItem(int id)
        {
            List<int> deleteList = new List<int>(){id};
            RemoveGuideSoulItem(id);
            SyncDbUpdateGuideSoulRestCounts();
            owner.SendGuideSoulRestCountsInfo(deleteList, 2);
        }

        public bool RandomOptionalGuideSoulItems()
        {
            SpaceTimeTowerLevel levelModel = SpaceTimeTowerLibrary.GetTowerLevelModel(TowerLevel);
            if (levelModel == null)
            {
                Log.Warn($"player {owner.Uid} towerlevel {TowerLevel} RandomOptionalGuideSoulItems failed");
                return false;
            }

            SpaceTimeTowerShop shopInfo = SpaceTimeTowerLibrary.GetShopInfo(levelModel.BeastSettlementId);
            if (shopInfo == null || shopInfo.eShopType != SpaceTimeEventType.BeastSettlement)
            {
                Log.Warn($"player {owner.Uid} towerlevel {TowerLevel} RandomOptionalGuideSoulItems failed: shop {levelModel.BeastSettlementId} param error");
                return false;
            }

            List<int> randomGuideSoulItems = RandomProduct(shopInfo.iId);
            optionalGuideSoulItems.Clear();
            foreach (int itemId in randomGuideSoulItems)
            {
                //随机活动货币
                GuideSoulItemModel itemModel = SpaceTimeTowerLibrary.GetGuideSoulItemModel(itemId);
                if (itemModel.SpaceTimeCoinRewards.Length < 2)continue;
                int coinNum = NewRAND.Next(itemModel.SpaceTimeCoinRewards[0], itemModel.SpaceTimeCoinRewards[1]);
                optionalGuideSoulItems.Add(itemId, coinNum);
            }
            SyncDbUpdateSpaceTimeTower();
            return true;
        }

        public void ClearOptionalGuideSoulItems()
        {
            optionalGuideSoulItems.Clear();
            SyncDbUpdateSpaceTimeTower();
        }

        public void ResetGuideSoulItemInfo()
        {
            guideSoulRestCounts.Clear();
            optionalGuideSoulItems.Clear();
            SyncDbUpdateGuideSoulRestCounts();
            owner.SendGuideSoulRestCountsInfo();
        }
        
        #endregion

        public void UpdateStartState(bool started)
        {
            Started = started;
        }
        
        public void SyncDbUpdateSpaceTimeTower()
        {
            owner.server.GameDBPool.Call(new QueryUpdateSpaceTimeTower(owner.Uid, heroPool.ToString("|", ":"),
                TowerLevel, ICurrRefreshNum, 
                TowerEvent == null ? 0 : (int) TowerEvent?.EventType, 
                TowerEvent == null ? string.Empty : TowerEvent?.ParamList.ToString("|"),
                StartTime, LstGetAwardId.ToString("|"), FailCount, ConsumeCoins, PassedTopLevel,
                 oShopInfo == null ? string.Empty : oShopInfo?.LstProductId.ToString("|"), 
                oShopInfo ==null ? string.Empty : oShopInfo?.DicBuyNum.ToString("|", ":"),
                optionalGuideSoulItems.ToString("|", ":"), Started, StageRewardPeriod, PastRewardsState));
        }

        private void SyncDbInsertHero(SpaceTimeHeroInfo heroInfo)
        {
            owner.server.GameDBPool.Call(new QueryInsertSpaceTimeHero(owner.Uid, heroInfo));
        }

        private void SyncDbUpdateHero(SpaceTimeHeroInfo heroInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdateSpaceTimeHero(owner.Uid, heroInfo));
        }

        private void SyncDbDeleteHero(int heroId)
        {
            owner.server.GameDBPool.Call(new QueryDeleteSpaceTimeHero(owner.Uid, heroId));
        }

        private void SyncDbDeleteAllHeros()
        {
            owner.server.GameDBPool.Call(new QueryDeleteSpaceTimeAllHeros(owner.Uid));
        }

        private void SyncDbUpdateGuideSoulRestCounts()
        {
            owner.server.GameDBPool.Call(new QueryUpdateGuideSoulRestCounts(owner.Uid, guideSoulRestCounts.ToString("|", ":")));
        }
        
        public MSG_ZMZ_SPACETIME_TOWER_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_SPACETIME_TOWER_INFO msg = new MSG_ZMZ_SPACETIME_TOWER_INFO();
            msg.TowerLevel = TowerLevel;
            msg.PassedTopLevel = PassedTopLevel;
            HeroPool.ForEach(x=> msg.HeroPool.Add(x.Key, x.Value));
            HeroTeam.ForEach(x=>msg.HeroTeam.Add(x.Key, GenerateSpaceTimeHeroTransInfo(x.Value)));
            if (TowerEvent != null)
            {
                msg.EventType = (int) TowerEvent.EventType;
                msg.ParamList.AddRange(TowerEvent.ParamList);
            }
            msg.GetAwardId.AddRange(LstGetAwardId);
            msg.FailCount = FailCount;
            msg.ConsumeCoins = ConsumeCoins;
            msg.StartTime = Timestamp.GetUnixTimeStampSeconds(StartTime);
            msg.EndTime = Timestamp.GetUnixTimeStampSeconds(EndTime);
            msg.ICurrRefreshNum = ICurrRefreshNum;
            GuideSoulRestCounts.ForEach(x=>msg.GuideSoulRestCounts.Add(x.Key, x.Value));
            OptionalGuideSoulItems.ForEach(x=>msg.OptionalGuideSoulItems.Add(x.Key, x.Value));
            msg.ShopInfo = GenerateSpaceTimeShopInfo();
            msg.Started = Started;
            msg.Period = PersonalPeriod;
            msg.Passed = PersonalPassed;
            msg.PeriodPassedBefore = PeriodPassedBefore;
            msg.StageRewardPeriod = StageRewardPeriod;
            msg.PastRewardsState = PastRewardsState;
            msg.Week = Week;
            return msg;
        }

        private ZMZ_SPACE_TIME_HERO GenerateSpaceTimeHeroTransInfo(SpaceTimeHeroInfo heroInfo)
        {
            ZMZ_SPACE_TIME_HERO msg = new ZMZ_SPACE_TIME_HERO();
            msg.HeroId = heroInfo.Id;
            msg.StepLevel = heroInfo.StepLevel;
            msg.GodType = heroInfo.GodType;
            msg.PositionNum = heroInfo.PositionNum;
            msg.NatureList = owner.GetNaturesTransform(heroInfo.Nature.GetNatureList());
            return msg;
        }

        private ZMZ_SPACE_TIME_SHOP GenerateSpaceTimeShopInfo()
        {
            ZMZ_SPACE_TIME_SHOP msg = new ZMZ_SPACE_TIME_SHOP();
            if (OShopInfo == null) return null;
            msg.EShopType = (int)OShopInfo.EShopType;
            msg.LstProductId.AddRange(OShopInfo.LstProductId);
            foreach (var kv in OShopInfo.DicBuyNum)
            {
                msg.DicBuyNum.Add(kv.Key, kv.Value);
            }
            return msg;
        }
        
        public void LoadTransformMsg(MSG_ZMZ_SPACETIME_TOWER_INFO msg)
        {
            TowerLevel = msg.TowerLevel;
            PassedTopLevel = msg.PassedTopLevel;
            heroPool = new Dictionary<int, int>();
            foreach (var hero in msg.HeroPool)
            {
                heroPool.Add(hero.Key, hero.Value);
            }
            heroTeam = new Dictionary<int, SpaceTimeHeroInfo>();
            heroQueue = new Dictionary<int, SpaceTimeHeroInfo>();
            foreach (var hero in msg.HeroTeam)
            {
                SpaceTimeHeroInfo heroInfo = new SpaceTimeHeroInfo(hero.Value.HeroId, hero.Value.StepLevel, hero.Value.GodType, hero.Value.PositionNum);
                foreach (var natureIt in hero.Value.NatureList.Natures)
                {
                    heroInfo.Nature.SetNewNature((NatureType)natureIt.Type, natureIt.BaseValue, natureIt.AddedValue, natureIt.BaseRatio);
                }
                heroTeam.Add(hero.Key, heroInfo);
                if (heroInfo.PositionNum >= 0)
                {
                    AddHeroToQueue(heroInfo);
                }
            }
            TowerEvent = CreateSpaceTimeEvent(msg.EventType, msg.ParamList.ToList());
            lstGetAwardId = new List<int>();
            lstGetAwardId.AddRange(msg.GetAwardId);
            FailCount = msg.FailCount;
            ConsumeCoins = msg.ConsumeCoins;
            StartTime = Timestamp.TimeStampToDateTime(msg.StartTime);
            EndTime = Timestamp.TimeStampToDateTime(msg.EndTime);
            ICurrRefreshNum = msg.ICurrRefreshNum;
            guideSoulRestCounts = new Dictionary<int, int>();
            msg.GuideSoulRestCounts.ForEach(x=>guideSoulRestCounts.Add(x.Key, x.Value));
            optionalGuideSoulItems = new Dictionary<int, int>();
            msg.OptionalGuideSoulItems.ForEach(x=>optionalGuideSoulItems.Add(x.Key, x.Value));
            Started = msg.Started;
            PersonalPeriod = msg.Period;
            PersonalPassed = msg.Passed;
            PeriodPassedBefore = msg.PeriodPassedBefore;
            StageRewardPeriod = msg.StageRewardPeriod;
            PastRewardsState = msg.PastRewardsState;
            Week = msg.Week;

            if (msg.ShopInfo == null) return;
            OShopInfo = new SpaceTimeTowerShopInfo((SpaceTimeEventType)msg.ShopInfo.EShopType);
            OShopInfo.LstProductId.AddRange(msg.ShopInfo.LstProductId);
            msg.ShopInfo.DicBuyNum.ForEach(x=>OShopInfo.DicBuyNum.Add(x.Key, x.Value));
        }
        
        #region 怪物期数(本服难度)

        private ZoneServerApi server;

        /// <summary>
        /// 怪物期数(本服难度)
        /// </summary>
        public int Period { get; private set; }
        /// <summary>
        /// 当期是否有玩家通关
        /// </summary>
        public bool Passed { get; private set; }

        public SpaceTimeTowerManager(ZoneServerApi server)
        {
            this.server = server;
        }

        public void RecordMonsterInfo(int period, bool passed, bool notifyPc)
        {
            Period = period;
            Passed = passed;
            if (notifyPc)
            {
                server.PCManager.PcList.ForEach(x=>x.Value.SendSpaceTimeTowerInfo());
                server.PCManager.PcOfflineList.ForEach(x=>x.Value.SendSpaceTimeTowerInfo());
            }
        }
        public void UpdateMonsterPassedState()
        {
            if (!Passed)
            {
                Passed = true;
                server.GameDBPool.Call(new QueryUpdateSpaceTimeMonster(Period, Passed));
                NotiftRelationMonsterInfo(Period, Passed);
            }
        }

        private void NotiftRelationMonsterInfo(int period, bool passed)
        {
            MSG_ZR_SPACETIME_MONSTER_INFO msg = new MSG_ZR_SPACETIME_MONSTER_INFO();
            msg.Period = period;
            msg.Passed = passed;
            server.RelationServer.Write(msg);
        }
        #endregion
        
        #region [商城相关]

        /// <summary>
        /// 随机商品信息
        /// </summary>
        /// <param name="iShopId"></param>
        public List<int> RandomProduct(int iShopId)
        {
            var oSpaceShop = SpaceTimeTowerLibrary.GetShopInfo(iShopId);
            if (oSpaceShop == null)
            {
                return null;
            }
            int productRealCount = Math.Min(oSpaceShop.lstProductId.Count, oSpaceShop.lstWeight.Count);
            
            SpaceTimeEventType eShopType = oSpaceShop.eShopType;
            List<int> lstProductId = null;
            switch (eShopType)
            {
                case SpaceTimeEventType.Shop:
                {//商城 直接返回即可不作处理
                    lstProductId = new List<int>(oSpaceShop.lstProductId);
                    break;
                }
                case SpaceTimeEventType.House:
                {//时空屋
                    lstProductId = new List<int>();
                    //必出魂导器一个
                    List<CRandomObj> lstProductWeight = new List<CRandomObj>();
                    List<CRandomObj> lstHunDaoQi = new List<CRandomObj>();
                    for (int i = 0; i < productRealCount; i++)
                    {
                        int iProductId = oSpaceShop.lstProductId[i];
                        var oProductInfo = SpaceTimeTowerLibrary.GetProductInfo(iProductId);
                        if (oProductInfo == null)
                        {
                            continue;
                        }

                        if (!Owner.CheckShopBuyCondition(oProductInfo))
                        {
                            continue;
                        }
                        
                        CRandomObj oRandom = new CRandomObj
                        {
                            iId = iProductId,
                            iWeight = oSpaceShop.lstWeight[i]
                        };
                        
                        if (oProductInfo.eProductType == EnumSpaceTimeProductType.SpaceTimeHunDaoQi)
                        {
                            lstHunDaoQi.Add(oRandom);
                        }
                        else
                        {
                            if (oProductInfo.eProductType == EnumSpaceTimeProductType.SpaceTimeDiscardHunDaoQi && GuideSoulRestCounts.Count == 0)
                            {
                                continue;
                            }
                            lstProductWeight.Add(oRandom);
                        }
                    }
                    //其他事件不满足随不出来商品时需要策划填魂导器事件来补位
                    if (lstProductWeight.Count + 1 < oSpaceShop.iRandomNum)
                    {
                        int fillUpNum = oSpaceShop.iRandomNum - lstProductWeight.Count - 1 ;
                        for (int i = 0; i < fillUpNum; i++)
                        {
                            CRandomObj oRandom = new CRandomObj
                            {
                                iId = oSpaceShop.HouseFilledProducts[i],
                                iWeight = oSpaceShop.HouseFilledProductsWeight[i]
                            };
                            lstProductWeight.Add(oRandom);
                        }
                    }

                    int iMaxRandom = oSpaceShop.iRandomNum;

                    WeightRandom oRandomHundaoqi = new WeightRandom();
                    oRandomHundaoqi.AddSources(lstHunDaoQi);

                    WeightRandom oRandomProduct = new WeightRandom();
                    oRandomProduct.AddSources(lstProductWeight);
                        
                    /*\ 随机魂导器操作 /*/
                    int iId = oRandomHundaoqi.RandomElement();
                    lstProductId.Add(iId);
                    
                    for (int i = 0; i < iMaxRandom - 1; i++)
                    {
                        int iProductId = oRandomProduct.RandomElementDel();
                        if (iProductId != -1)
                        {
                            lstProductId.Add(iProductId);
                        }
                    }
                    break;
                }
                case SpaceTimeEventType.BeastSettlement:
                {
                    lstProductId = new List<int>();
                    
                    List<CRandomObj> lstProductWeight = new List<CRandomObj>();
                    for (int i = 0; i < productRealCount; i++)
                    {
                        int iProductId = oSpaceShop.lstProductId[i];
                        var oProductInfo = SpaceTimeTowerLibrary.GetGuideSoulItemModel(iProductId);
                        if (oProductInfo == null)
                        {
                            continue;
                        }
                        CRandomObj oRandom = new CRandomObj
                        {
                            iId = iProductId,
                            iWeight = oSpaceShop.lstWeight[i]
                        };
                        lstProductWeight.Add(oRandom);
                    }
                    WeightRandom oRandomProduct = new WeightRandom();
                    oRandomProduct.AddSources(lstProductWeight);
                    for (int i = 0; i < oSpaceShop.iRandomNum; i++)
                    {
                        int iProductId = oRandomProduct.RandomElementDel();
                        if (iProductId != -1)
                        {
                            lstProductId.Add(iProductId);
                        }
                    }
                    break;
                }
            }

            return lstProductId;
        }

        /// <summary>
        /// 初始化商城信息
        /// </summary>
        /// <param name="eShopType"></param>
        public void InitShopInfo(SpaceTimeEventType eShopType, List<int> lstProductId)
        {
            if (eShopType != SpaceTimeEventType.Shop && eShopType != SpaceTimeEventType.House)
            {
                return;
            }
            if (oShopInfo == null)
            {
                oShopInfo = new SpaceTimeTowerShopInfo(eShopType)
                {
                    LstProductId = lstProductId
                };
            }
            else
            {
                oShopInfo.EShopType = eShopType;
                oShopInfo.LstProductId = lstProductId;
                oShopInfo.DicBuyNum.Clear();
            }
            /*\ 同步客户端商城信息 /*/
            Owner.SendSpaceTimeShopInfo();
        }

        /// <summary>
        /// 返回当前商城id
        /// </summary>
        /// <returns></returns>
        public int RetCurrShopId(SpaceTimeEventType eventType)
        {
            var oTbTowerLvInfo = SpaceTimeTowerLibrary.GetTowerLevelModel(TowerLevel);
            if (oTbTowerLvInfo == null)
            {
                return 0;
            }

            // if (TowerEvent == null)
            // {
            //     return 0;
            // }
            
            if (eventType == SpaceTimeEventType.House)
            {
                return oTbTowerLvInfo.HouseId;
            }
            else if (eventType == SpaceTimeEventType.Shop)
            {
                return oTbTowerLvInfo.ShopId;
            }

            return 0;
        }

        /// <summary>
        /// 更新消耗活动货币记录
        /// </summary>
        /// <param name="consumeNum"></param>
        public void UpdateConsumeCoins(int consumeNum)
        {
            ConsumeCoins += consumeNum;
        }
        #endregion

        #region 个人难度

        public void UpdatePersonalPeriod()
        {
            if (PersonalPassed)
            {
                PeriodPassedBefore = PersonalPeriod;
                if (PersonalPeriod < SpaceTimeTowerLibrary.MonsterDifficultyLimit)
                {
                    PersonalPeriod++;
                    PersonalPassed = false;
                }
            }
            else
            {
                PeriodPassedBefore = PersonalPeriod - 1;
            }
            SyncDbUpdatePersonalPeriod();
        }

        public void UpdatePastRewardsState(bool state)
        {
            PastRewardsState = state;
        }
        

        public void CheckChangeToNextDifficulty()
        {
            //本服难度
            int serverPeriod = owner.server.SpaceTimeTowerManager.Period;
            if (serverPeriod == 0)
            {
                serverPeriod = PersonalPeriod;
            }
            if (StageRewardPeriod < serverPeriod)
            {
                //阶段奖励升级
                LstGetAwardId.Clear();
                StageRewardPeriod++;
                //最高通关层数重置
                PassedTopLevel = 0;
            }
            if (PersonalPeriod < serverPeriod)
            {
                //判断难度是否升级
                PersonalPeriod++;
                PersonalPassed = false;
                SyncDbUpdatePersonalPeriod();
            }
        }
        
        private void SyncDbUpdatePersonalPeriod()
        {
            owner.server.GameDBPool.Call(new QueryUpdateSpaceTimeTowerPersonalPeriod(owner.Uid, PersonalPeriod, PeriodPassedBefore, PersonalPassed));
        }
        #endregion
        
        /// <summary>
        /// 更新活动周数
        /// </summary>
        private void UpdateActivityWeek()
        {
            if (SpaceTimeTowerLibrary.StartDateTime == DateTime.MinValue)
            {
                Week = Owner.server.SpaceTimeTowerManager.Period;
            }
            else
            {
                Week = (ZoneServerApi.now - SpaceTimeTowerLibrary.StartDateTime).Days / 7 + 1;
            }
        }
    }
}

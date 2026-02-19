using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //委派事件
        public DelegationManager DelegationMng { get; set; }

        public void InitDelegationManager()
        {
            DelegationMng = new DelegationManager(this);
        }

        public void BindDelegationItem(DelegationInfo info)
        {
            DelegationMng.AddDelegationInfo(info);
        }

        //获取委派事件列表
        public void GetDelegationList()
        {
            MSG_ZGC_DELEGATION_LIST response = new MSG_ZGC_DELEGATION_LIST();

            //委派事件开启限制
            if (!CheckLimitOpen(LimitType.DelegationEvent))
            {
                Log.Warn("player {0} get Delegation list fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                response.Result = (int)ErrorCode.LevelLimit;
                Write(response);
                return;
            }

            //获取身上的委派事件
            Dictionary<int, DelegationItem> delegationList = DelegationMng.GetDelegationList();

            int num = DelegationMng.GetEventNumByMainTaskId(MainTaskId);
            if (num == 0)
            {
                Log.Warn("player {0} get Delegation list fail, event num error", Uid);
                return;
            }

            int addCount = DelegationMng.GetAddDelegationCount(this, num, GetCounterValue(CounterType.DelegateCount));
            //需要发放委派事件
            if (addCount > 0)
            {
                //判断玩家任务进度是否有对应的事件和事件名称区间
                DelegationModelList delegations = DelegationLibrary.GetDelegationsByMainTaskId(MainTaskId);
                if (delegations == null)
                {
                    Log.Warn("player {0} GetDelegationList GetDelegationsByMainTaskId fail, player mainTaskId is {1}", Uid, MainTaskId);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                List<int> nameIds = DelegationLibrary.GetNameIdList();
                if (nameIds.Count == 0)
                {
                    Log.Warn("player {0} GetDelegationList GetNameIdList fail, xml param error", Uid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                //根据任务进度刷新委派事件数量
                List<int> newNameList = GetRandomDelegationNameList(delegationList, nameIds, addCount);
                foreach (var newName in newNameList)
                {
                    //直接随机1个新事件
                    DelegationModel newModel = delegations.RandomDelegationModel();
                    if (newModel != null)
                    {
                        DelegationItem delegationItem = CreateNewDelegationItem(newName, newModel);
                        DelegationMng.AddDelegationItem(delegationItem);
                        UpdateCounter(CounterType.DelegateCount, 1);
                    }
                }
                //检查是否需要保底
                DelegationMng.CheckNeedGuarantee(MainTaskId, delegations);

                //添加到数据库
                SyncUpdateDelegations();
            }

            foreach (var delegation in delegationList)
            {
                DELEGATION_ITEM item = GetDelegationItem(delegation.Value);
                response.Item.Add(item);
            }

            //获取刷新所需钻石数
            int refreshCount = GetCounterValue(CounterType.DelegateRefreshCount);
            int refreshCoins = DelegationLibrary.GetRefreshCoins(refreshCount);
            if (refreshCoins == 0)
            {
                Log.Warn("player {0} GetRefreshCoins fail, refershCount is {1}", Uid, refreshCount);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            response.Price = refreshCoins;

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private DelegationItem CreateNewDelegationItem(int newName, DelegationModel newModel)
        {
            DelegationItem delegationItem = new DelegationItem();
            delegationItem.Id = newModel.Id;
            delegationItem.Name = newName;
            delegationItem.State = (int)DelegationType.None;
            delegationItem.HeroList = new List<int>();
            delegationItem.HeroList.Add(0);
            return delegationItem;
        }

        //获取委派事件列表
        //public void GetDelegationList1()
        //{
        //    MSG_ZGC_DELEGATION_LIST response = new MSG_ZGC_DELEGATION_LIST();

        //    List<DelegationItem> delegationList = DelegationMng.GetDelegationList();
        //    //之前没有委派事件
        //    if (delegationList.Count == 0)
        //    {           
        //        List<DelegationModel> levelDelegations = DelegationLibrary.GetDelegationsByLevel(Level);
        //        List<DelegationName> levelNames = DelegationMng.GetDelegationNameList();
        //        if (levelDelegations == null || levelNames == null)
        //        {
        //            Log.Warn("player {0} get DelegationModel or DelegationName fail, player level is {1}", Uid, Level);
        //            response.Result = (int)ErrorCode.Fail;
        //            Write(response);
        //            return;
        //        }
        //        List<DelegationItem> itemList = new List<DelegationItem>();              
        //        //直接随机3个新事件
        //        for (int i = 0; i < 3; i++)
        //        {
        //            DelegationModel delegation = RandomDelegationModel1(levelDelegations);                                    
        //            DelegationName name = RandomDelegationName(levelNames, 1);
        //            if (delegation == null || name == null)
        //            {
        //                Log.Warn("player {0} random delegation or name fail", Uid);
        //                response.Result = (int)ErrorCode.Fail;
        //                Write(response);
        //                return;
        //            }
        //            //绑定随机事件信息
        //            DELEGATION_ITEM item = BindDelegationInfo(delegation, name);
        //            response.Item.Add(item);
        //            DelegationItem delegationItem = new DelegationItem();
        //            delegationItem.Id = delegation.Id;
        //            delegationItem.Name = name.Id;
        //            delegationItem.State = (int)DelegationType.None;                  
        //            delegationList.Add(delegationItem);
        //            itemList.Add(delegationItem);
        //        }
        //        //添加到数据库
        //        SyncUpdateDelegations(itemList);
        //    }
        //    else
        //    {
        //        //之前有委派事件
        //        foreach (var delegation in delegationList)
        //        {
        //            DELEGATION_ITEM item = GetDelegationItem(delegation);
        //            response.Item.Add(item);
        //        }
        //    }         
        //    //获取刷新所需钻石数
        //    int refreshCoins = DelegationLibrary.GetRefreshCoins(DelegationMng.RefreshCount);
        //    if (refreshCoins == 0)
        //    {
        //        Log.Warn("player {0} GetRefreshCoins fail, refershCount is {1}", Uid, DelegationMng.RefreshCount);
        //        response.Result = (int)ErrorCode.Fail;
        //        Write(response);
        //        return;
        //    }
        //    response.Price = refreshCoins;

        //    //获取委派次数购买价格
        //    int buyCountCoins = DelegationLibrary.GetBuyCountCoins(DelegationMng.BuyCount);
        //    if (buyCountCoins == 0)
        //    {
        //        Log.Warn("player {0} GetBuyCountCoins fail, buyCount is {1}", Uid, DelegationMng.BuyCount);
        //        response.Result = (int)ErrorCode.Fail;
        //        Write(response);
        //        return;
        //    }
        //    response.BuyPrice = buyCountCoins;

        //    response.Count = DelegationMng.DelegateCount;
        //    response.RemainCount = DelegationLibrary.MaxBuyCount - DelegationMng.BuyCount;
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //获取委派事件详细信息
        private DELEGATION_ITEM GetDelegationItem(DelegationItem item)
        {
            DELEGATION_ITEM delegationItem = new DELEGATION_ITEM();
            //DelegationModel model = DelegationLibrary.GetDelegationById(item.Id);
            //DelegationName name = DelegationLibrary.GetNameById(item.Name);
            delegationItem.Id = item.Id;
            delegationItem.NameId = item.Name;
            delegationItem.State = item.State;
            //delegationItem.Quality = model.Quality;
            //delegationItem.Color = model.Color;         

            switch ((DelegationType)item.State)
            {
                case DelegationType.OnDelegation:
                    if (item.EndTime > ZoneServerApi.now)
                    {
                        delegationItem.Time = Timestamp.GetUnixTimeStampSeconds(item.EndTime);
                    }
                    else
                    {
                        delegationItem.Time = 0;
                        item.State = (int)DelegationType.Complete;
                    }
                    foreach (var heroId in item.HeroList)
                    {
                        delegationItem.HeroIds.Add(heroId);
                    }
                    break;
                case DelegationType.Complete:
                    delegationItem.Time = 0;
                    foreach (var heroId in item.HeroList)
                    {
                        delegationItem.HeroIds.Add(heroId);
                    }
                    break;
                case DelegationType.None:
                default:
                    break;
            }
            return delegationItem;
        }

        ////随机选出委派事件
        //public Dictionary<int, DelegationModel> RandomDelegationModel(List<DelegationModel> deleglist, int count)
        //{          
        //    DelegationModel model = null;
        //    int sumWeight = 0;
        //    SortedDictionary<int, int> weightList = new SortedDictionary<int, int>();
        //    foreach (var item in deleglist)
        //    {
        //        sumWeight += item.Weight;
        //        weightList.Add(item.Id, sumWeight);
        //    }
        //    Dictionary<int, DelegationModel> randList = new Dictionary<int, DelegationModel>();
        //    int sumCount = 0;
        //    while (sumCount < count)
        //    {
        //        int rate = RAND.Range(1, sumWeight);
        //        Dictionary<int, int> reWeightList = new Dictionary<int, int>();               
        //        foreach (var item in weightList.Reverse())
        //        {
        //            reWeightList.Add(item.Key, item.Value);
        //        }
        //        List<int> lastWeightList = new List<int>();
        //        DelegationModel temp;
        //        foreach (var kv in reWeightList)
        //        {                
        //            if (lastWeightList.Count > 0)
        //            {
        //                if (rate >= kv.Value)
        //                {                          
        //                    model = DelegationLibrary.GetDelegationById(lastWeightList.Last());
        //                    if (randList.Count == 0)
        //                    {
        //                        randList.Add(model.Id, model);
        //                        sumCount++;
        //                    }
        //                    else
        //                    {
        //                        randList.TryGetValue(model.Id, out temp);
        //                        if (temp == null)
        //                        {
        //                            randList.Add(model.Id, model);
        //                            sumCount++;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }
        //            lastWeightList.Add(kv.Key);
        //        }
        //        if (model == null)
        //        {
        //            model = DelegationLibrary.GetDelegationById(lastWeightList.Last());
        //            if (randList.Count == 0)
        //            {
        //                randList.Add(model.Id, model);
        //                sumCount++;
        //            }
        //            else
        //            {
        //                randList.TryGetValue(model.Id, out temp);
        //                if (temp == null)
        //                {
        //                    randList.Add(model.Id, model);
        //                    sumCount++;
        //                }
        //            }                  
        //        }
        //    }          
        //    return randList;
        //}

        //随机选出事件名称
        //public List<DelegationModel> GetRandomDelegationList(DelegationModelList levelDelegations, int number)
        //{                   
        //    List<DelegationModel> randList = new List<DelegationModel>();

        //    if (levelDelegations.List.Count > 0)
        //    {
        //        for (int i = 0; i < number; i++)
        //        {
        //            DelegationModel model = levelDelegations.RandomDelegationModel();
        //            if (model != null)
        //            {
        //                randList.Add(model);
        //            }
        //        }
        //    }          
        //    return randList;
        //}

        public List<int> GetRandomDelegationNameList(Dictionary<int, DelegationItem> delegationList, List<int> nameIds, int number, List<int> removeList = null)
        {
            List<int> randList = new List<int>();

            List<int> randomNames = GetRandomNames(delegationList, nameIds, removeList);
            if (randomNames.Count < number)
            {
                Log.Warn($"player {Uid} get random delegation name list fail, DelegationEvent xml error, events not enough");
            }
            for (int i = 0; i < number; i++)
            {
                //直接随机1个名字   
                int newName = GetRandomDelegationName(randomNames);
                if (newName != 0)
                {
                    randList.Add(newName);

                    for (int j = 0; j < randomNames.Count; j++)
                    {
                        if (randomNames[j] == newName)
                        {
                            randomNames.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
            return randList;
        }

        public int GetRandomDelegationName(List<int> randomNames)
        {
            int nameId = 0;
            if (randomNames.Count > 0)
            {
                if (randomNames.Count == 1)
                {
                    nameId = randomNames[0];
                }
                else
                {
                    Random rand = new Random();
                    int i = rand.Next(randomNames.Count);
                    nameId = randomNames[i];
                    //RandomNames.RemoveAt(i);
                    //Log.Warn("GetRandomDelegationName i {0}", i);
                }
            }
            return nameId;
        }

        private List<int> GetRandomNames(Dictionary<int, DelegationItem> delegationList, List<int> nameIds, List<int> removeList = null)
        {
            List<int> RandomNames = new List<int>();
            List<int> getNames = new List<int>();
            foreach (var item in delegationList)
            {
                getNames.Add(item.Key);
                //Log.Warn("GetRandomDelegationName key ----------------- {0}", item.Key);
            }
            if (removeList != null)
            {
                foreach (var item in removeList)
                {
                    getNames.Add(item);
                }
            }

            foreach (var nameId in nameIds)
            {
                if (!getNames.Contains(nameId))
                {
                    RandomNames.Add(nameId);
                }
            }

            return RandomNames;
        }

        //绑定事件信息
        //private DELEGATION_ITEM BindDelegationInfo(DelegationModel delegation, int nameId)
        //{
        //    DELEGATION_ITEM item = new DELEGATION_ITEM();
        //    item.Id = delegation.Id;
        //    item.NameId = nameId;
        //    item.State = (int)DelegationType.None;
        //    item.Quality = delegation.Quality;
        //    item.Color = delegation.Color;
        //    //item.Time = delegation.MissionDuration;
        //    //item.HeroIds.Add(delegation.DispatchNum);
        //    return item;
        //}

        //private void SyncAddDelegations(Dictionary<int, DelegationModel> delegationDic)
        //{
        //    server.GameDBPool.Call(new QueryAddDelegations(Uid, delegationDic));
        //}

        private void SyncUpdateDelegations()
        {
            string idStr = DelegationMng.BuildDelegationIdStr();
            string nameStr = DelegationMng.BuildDelegationNameStr();
            string stateStr = DelegationMng.BuildDelegationStateStr();
            string herosStr = DelegationMng.BuildDelegationHerosStr();
            string endTimeStr = DelegationMng.BuildDelegationEndTimeStr();
            server.GameDBPool.Call(new QueryUpdateDelegations(Uid, idStr, nameStr, stateStr, herosStr, endTimeStr));
        }
        //委派伙伴
        public void DelegateHeros(int delegationId, int nameId, RepeatedField<int> heroList)
        {
            MSG_ZGC_DELEGATE_HEROS response = new MSG_ZGC_DELEGATE_HEROS();
            //判断有无该事件
            DelegationModel model = DelegationLibrary.GetDelegationById(delegationId);
            if (model == null)
            {
                Log.Warn("player {0} not find delegation event : {1}", Uid, delegationId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            //判断是伙伴列表是否为空
            if (heroList == null || heroList.Count == 0)
            {
                Log.Warn("player {0} event {1} delegate heros fail, heroList is null", Uid, delegationId);
                response.Result = (int)ErrorCode.HeroNotExist;
                Write(response);
                return;
            }

            //验证伙伴是否存在以及是否已上阵
            foreach (var heroId in heroList)
            {
                HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                if (hero == null)
                {
                    Log.Warn("player {0} event {1} delegate heros fail, hero {2} not exists", Uid, delegationId, heroId);
                    response.Result = (int)ErrorCode.HeroNotExist;
                    Write(response);
                    return;
                }

                if (!DelegationMng.CheckCanDelegate(heroId))
                {
                    Log.Warn("player {0} event {1} delegate heros fail, hero {2} already on delegation", Uid, delegationId, heroId);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                DelegationMng.AddDelegatedHero(heroId);
            }

            Dictionary<int, DelegationItem> itemList = DelegationMng.GetDelegationList();
            DELEGATION_ITEM delegationItem = null;
            foreach (var kv in itemList)
            {
                DelegationItem item = kv.Value;
                if (item.Name == nameId && item.State != (int)DelegationType.None)
                {
                    Log.Warn("delegation event : {0} state {1} is wrong", delegationId, item.State);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                else if (item.Name == nameId && item.State == (int)DelegationType.None)
                {
                    if (item.HeroList.Contains(0))
                    {
                        item.HeroList.Remove(0);
                    }
                    item.State = (int)DelegationType.OnDelegation;
                    item.EndTime = ZoneServerApi.now.AddSeconds(model.MissionDuration);
                    foreach (var heroId in heroList)
                    {
                        item.HeroList.Add(heroId);
                    }
                    delegationItem = GetDelegationItem(item);
                }
            }

            response.Item = delegationItem;
            response.Result = (int)ErrorCode.Success;

            //更新数据库
            SyncUpdateDelegations();
            Write(response);

            //komoelog
            DelegationItem dItem;
            if (itemList.TryGetValue(nameId, out dItem))
            {
                List<Dictionary<string, object>> heroPos = ParseHeroPosList(dItem.HeroList);
                KomoeEventLogDelegateTasks(1, model.Id.ToString(), nameId.ToString(), heroPos, 0, 0, model.Quality, model.MissionDuration, 0, null, null);
            }
        }

        //完成委派事件(耗钻)
        public void CompleteDelegation(int delegationId, int nameId)
        {
            MSG_ZGC_COMPLETE_DELEGATION response = new MSG_ZGC_COMPLETE_DELEGATION();
            //判断有无相应的委派事件
            DelegationModel model = DelegationLibrary.GetDelegationById(delegationId);
            if (model == null)
            {
                Log.Warn("player {0} not find delegation event : {1}", Uid, delegationId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            Dictionary<int, DelegationItem> itemList = DelegationMng.GetDelegationList();
            int price = 0;
            foreach (var kv in itemList)
            {
                DelegationItem item = kv.Value;
                //判断事件状态是否正确
                if (item.Name == nameId)
                {
                    if (item.State != (int)DelegationType.OnDelegation)
                    {
                        Log.Warn("delegation event : {0} state {1} is wrong", delegationId, item.State);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    else
                    {
                        //判断钻石是否足够
                        int coins = GetCoins(CurrenciesType.diamond);
                        int minutes = (int)Math.Ceiling((item.EndTime - ZoneServerApi.now).TotalMinutes);
                        if (minutes > 0)
                        {
                            price = DelegationLibrary.AccelerateFee * minutes;
                        }
                        if (coins < price)
                        {
                            Log.Warn("diamond is not enough, curCoin is {0}", coins);
                            response.Result = (int)ErrorCode.DiamondNotEnough;
                            Write(response);
                            return;
                        }

                        //endTime = item.EndTime;
                        item.State = (int)DelegationType.Complete;
                        item.EndTime = ZoneServerApi.now;
                        DELEGATION_ITEM delegationItem = GetDelegationItem(item);
                        response.Item = delegationItem;
                    }
                    break;
                }
            }
            //扣货币
            if (price > 0)
            {
                DelCoins(CurrenciesType.diamond, price, ConsumeWay.CompleteDelegation, delegationId.ToString());
            }
            //更新数据库
            SyncUpdateDelegations();
            response.Result = (int)ErrorCode.Success;
            Write(response);

            BIRecordActivityLog(ActivityAction.CompleteDelegation, delegationId);


            //komoelog            
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)CurrenciesType.diamond, price);
            KomoeEventLogDelegateTasks(2, model.Id.ToString(), nameId.ToString(), null, 0, 0, model.Quality, model.MissionDuration, 0, null, consume);
        }

        //领取奖励
        public void GetDelegationRewards(int id, int nameId)
        {
            MSG_ZGC_DELEGATION_REWARDS response = new MSG_ZGC_DELEGATION_REWARDS();

            DelegationModel model = DelegationLibrary.GetDelegationById(id);
            if (model == null)
            {
                Log.Warn("player {0} get delegation reward not find delegation event: id {1}", Uid, id);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }

            DelegationItem oldDelegationItem = DelegationMng.GetDelegationItem(nameId);
            if (oldDelegationItem == null)
            {
                Log.Warn("player {0} get delegation reward not find delegation event: nameId {1}", Uid, nameId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }

            if (oldDelegationItem.EndTime > ZoneServerApi.now)
            {
                Log.Warn("player {0} get delegation reward delegation event: id {1} nameId {2} not completed", Uid, id, nameId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //移除信息
            DelegationMng.RemoveDelegationItem(nameId);
            //移除上阵伙伴
            DelegationMng.RemoveDelegatedHero(oldDelegationItem.HeroList);

            int num = DelegationMng.GetEventNumByMainTaskId(MainTaskId);
            if (num == 0)
            {
                Log.Warn("player {0} get Delegation list fail, event num error", Uid);
                return;
            }

            int count = DelegationMng.GetAddDelegationCount(this, num, GetCounterValue(CounterType.DelegateCount));
            if (count > 0)
            {
                //判断玩家任务进度是否有对应的事件和事件名称区间
                DelegationModelList delegations = DelegationLibrary.GetDelegationsByMainTaskId(MainTaskId);
                if (delegations == null)
                {
                    Log.Warn("player {0} get delegation Reward GetDelegationsByMainTaskId fail, player mainTaskId is {1}", Uid, MainTaskId);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                List<int> nameIds = DelegationLibrary.GetNameIdList();
                if (nameIds.Count == 0)
                {
                    Log.Warn("player {0} get delegation Reward GetNameIdList fail, xml param error", Uid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                //根据任务进度刷新委派事件数量
                Dictionary<int, DelegationItem> oldDelegationList = DelegationMng.GetDelegationList();
                List<int> newNameList = GetRandomDelegationNameList(oldDelegationList, nameIds, 1);
                var newName = newNameList.FirstOrDefault();
                //直接随机1个新事件                
                DelegationModel newModel = delegations.RandomDelegationModel();
                if (newModel != null)
                {
                    DelegationItem delegationItem = CreateNewDelegationItem(newName, newModel);
                    DelegationMng.AddDelegationItem(delegationItem);

                    UpdateCounter(CounterType.DelegateCount, 1);
                }
            }

            //添加到数据库
            SyncUpdateDelegations();
            //获取身上的委派事件
            Dictionary<int, DelegationItem> newDelegationList = DelegationMng.GetDelegationList();
            foreach (var delegation in newDelegationList)
            {
                DELEGATION_ITEM item = GetDelegationItem(delegation.Value);
                response.Item.Add(item);
            }
            //判断是否成功
            int rand = RAND.Range(1, 100);
            int rate = CalculateHerosRate(model, oldDelegationItem.HeroList);
            Log.Debug("player {0} get delegation rewards success rate : {1}", Uid, rate);

            //基础奖励
            RewardManager rewardMng = new RewardManager();
            rewardMng.AddSimpleReward(model.BasicReward);

            if (rand <= rate)
            {
                //完成奖励
                rewardMng.AddSimpleReward(model.SpecialReward);
                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                response.Result = (int)ErrorCode.BasicReward;
            }
            //同步前端奖励信息
            rewardMng.BreakupRewards();
            rewardMng.GenerateRewardItemInfo(response.Rewards);
            //添加物品
            AddRewards(rewardMng, ObtainWay.DelegationReward);
            Write(response);

            //完成委派事件次数
            AddTaskNumForType(TaskType.DelegationFinish);
            AddPassCardTaskNum(TaskType.DelegationFinish);
            AddSchoolTaskNum(TaskType.DelegationFinish);

            BIRecordActivityLog(ActivityAction.GetDelegationReward, id);

            //komoelog          
            List<Dictionary<string, object>> award = ParseRewardInfoToList(rewardMng.RewardList);
            KomoeEventLogDelegateTasks(3, id.ToString(), nameId.ToString(), null, 0, 0, model.Quality, model.MissionDuration, rand <= rate ? 1 : 2, award, null);

            AddRunawayActivityNumForType(RunawayAction.Delegation);
        }

        //计算上阵伙伴的成功率
        private int CalculateHerosRate(DelegationModel model, List<int> heroIdList)
        {
            int n = 0;
            int m = 0;
            List<int> cloneList = new List<int>();
            foreach (var heroId in heroIdList)
            {
                cloneList.Add(heroId);
            }
            //特定伙伴id加成项数
            if (model.HeroID.Contains("|"))
            {
                string[] heroIds = model.HeroID.Split('|');
                foreach (var heroId in heroIds)
                {
                    if (cloneList.Contains(int.Parse(heroId)))
                    {
                        n++;
                        cloneList.Remove(int.Parse(heroId));
                    }
                    m++;
                }
            }
            else if (model.HeroID != "")
            {
                if (cloneList.Contains(int.Parse(model.HeroID)))
                {
                    n++;
                    cloneList.Remove(int.Parse(model.HeroID));
                }
                m++;
            }
            List<int> conditionList = new List<int>();
            //特定魂师等级加成项数
            if (model.HeroLevel.Contains("|"))
            {
                string[] levels = model.HeroLevel.Split('|');
                foreach (var level in levels)
                {
                    int count = 0;
                    string[] item = level.Split(':');
                    foreach (var heroId in heroIdList)
                    {
                        HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                        if (heroInfo.Level >= int.Parse(item[0]) && count < int.Parse(item[1]))
                        {
                            count++;
                            conditionList.Add(heroId);
                        }
                    }
                    if (count >= int.Parse(item[1]))
                    {
                        n++;
                        RemoveHeroId(cloneList, conditionList);
                    }
                    conditionList.Clear();
                    m++;
                }
            }
            else if (model.HeroLevel != "")
            {
                string[] level = model.HeroLevel.Split(':');
                int count = 0;
                foreach (var heroId in heroIdList)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                    if (heroInfo.Level >= int.Parse(level[0]) && count < int.Parse(level[1]))
                    {
                        count++;
                        conditionList.Add(heroId);
                    }
                }
                if (count >= int.Parse(level[1]))
                {
                    n++;
                    RemoveHeroId(cloneList, conditionList);
                }
                conditionList.Clear();
                m++;
            }
            //特定职业加成项数
            if (model.HeroJob.Contains("|"))
            {
                string[] jobs = model.HeroJob.Split('|');
                foreach (var job in jobs)
                {
                    int jobCount = 0;
                    string[] item = job.Split(':');
                    foreach (var heroId in heroIdList)
                    {
                        HeroModel hero = HeroLibrary.GetHeroModel(heroId);
                        if (hero.Job == int.Parse(item[0]) && jobCount < int.Parse(item[1]))
                        {
                            jobCount++;
                            conditionList.Add(heroId);
                        }
                    }
                    if (jobCount >= int.Parse(item[1]))
                    {
                        n++;
                        RemoveHeroId(cloneList, conditionList);
                    }
                    conditionList.Clear();
                    m++;
                }
            }
            else if (model.HeroJob != "")
            {
                string[] job = model.HeroJob.Split(':');
                int jobCount = 0;
                foreach (var heroId in heroIdList)
                {
                    HeroModel hero = HeroLibrary.GetHeroModel(heroId);
                    if (hero.Job == int.Parse(job[0]) && jobCount < int.Parse(job[1]))
                    {
                        jobCount++;
                        conditionList.Add(heroId);
                    }
                }
                if (jobCount >= int.Parse(job[1]))
                {
                    n++;
                    RemoveHeroId(cloneList, conditionList);
                }
                m++;
            }
            int basicRate = cloneList.Count * model.BasicRatio;
            int rate = (int)Math.Ceiling((double)n * 100 / m) + basicRate;
            if (rate > 100)
            {
                rate = 100;
            }
            return rate;
        }

        private void RemoveHeroId(List<int> cloneList, List<int> conditionList)
        {
            foreach (var heroId in conditionList)
            {
                if (cloneList.Contains(heroId))
                {
                    cloneList.Remove(heroId);
                }
            }
        }

        //刷新委派事件数据
        public void RefreshDelegationData()
        {
            if (CheckLimitOpen(LimitType.DelegationEvent))
            {
                //获取身上的委派事件
                Dictionary<int, DelegationItem> delegationList = DelegationMng.GetDelegationList();
                List<int> removeList = new List<int>();
                int count = 0;
                //刷新未委派的事件
                foreach (var item in delegationList)
                {
                    if (item.Value.State == (int)DelegationType.None)
                    {
                        removeList.Add(item.Key);
                        //移除上阵伙伴
                        DelegationMng.RemoveDelegatedHero(item.Value.HeroList);
                    }
                    if (item.Value.State != (int)DelegationType.None)
                    {
                        count++;
                    }
                }

                foreach (var nameId in removeList)
                {
                    //移除信息
                    DelegationMng.RemoveDelegationItem(nameId);
                }

                //判断玩家任务进度是否有对应的事件和事件名称区间
                DelegationModelList delegations = DelegationLibrary.GetDelegationsByMainTaskId(MainTaskId);
                if (delegations == null)
                {
                    Log.Warn("player {0} refresh GetDelegationsByMainTaskId fail, player mainTaskId is {1}", Uid, MainTaskId);
                    return;
                }

                List<int> nameIds = DelegationLibrary.GetNameIdList();
                if (nameIds.Count == 0)
                {
                    Log.Warn("player {0} refresh GetNameIdList fail, xml param error", Uid);
                    return;
                }

                //根据任务进度刷新委派事件数量
                int num = DelegationMng.GetEventNumByMainTaskId(MainTaskId);
                if (num == 0)
                {
                    Log.Warn("player {0} refresh delegation fail, event num error", Uid);
                    return;
                }

                List<int> newNameList = GetRandomDelegationNameList(delegationList, nameIds, num - count, removeList);
                foreach (var newName in newNameList)
                {
                    //直接随机1个新事件
                    DelegationModel newModel = delegations.RandomDelegationModel();
                    if (newModel != null)
                    {
                        DelegationItem delegationItem = CreateNewDelegationItem(newName, newModel);
                        DelegationMng.AddDelegationItem(delegationItem);
                        //更新委派事件发放次数
                        UpdateCounter(CounterType.DelegateCount, 1);
                    }
                }
                //检查是否需要保底
                DelegationMng.CheckNeedGuarantee(MainTaskId, delegations);

                //添加到数据库
                SyncUpdateDelegations();
                //通知客户端
                SyncDelegationDailyRefreshMsg(delegationList);
            }
        }

        public void SyncDelegationDailyRefreshMsg(Dictionary<int, DelegationItem> itemList)
        {
            MSG_ZGC_DELEGATION_DAILY_REFRESH msg = new MSG_ZGC_DELEGATION_DAILY_REFRESH();
            msg.Price = DelegationLibrary.GetRefreshCoins(GetCounterValue(CounterType.DelegateRefreshCount));
            foreach (var item in itemList)
            {
                DELEGATION_ITEM itemInfo = GetDelegationItem(item.Value);
                msg.Item.Add(itemInfo);
            }
            Write(msg);
        }

        //手动刷新委派事件
        public void RefreshDelegation(int id, int nameId)
        {
            MSG_ZGC_REFRESH_DELEGATION response = new MSG_ZGC_REFRESH_DELEGATION();

            DelegationItem oldDelegationItem = DelegationMng.GetDelegationItem(nameId);
            if (oldDelegationItem == null)
            {
                Log.Warn("player {0} RefreshDelegation not find delegation event: nameId {1}", Uid, nameId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            //判断有无该事件
            DelegationModel model = DelegationLibrary.GetDelegationById(id);
            if (model == null)
            {
                Log.Warn("player {0} RefreshDelegation not find delegation event: {1}", Uid, id);
                //    response.Result = (int)ErrorCode.NotExist;
                //    Write(response);
                return;
            }
            //判断钻石是否足够
            int coins = GetCoins(CurrenciesType.diamond);
            int price = DelegationLibrary.GetRefreshCoins(GetCounterValue(CounterType.DelegateRefreshCount));
            if (coins < price)
            {
                Log.Warn("player {0} RefreshDelegation diamond is not enough, curCoin is {1}", Uid, coins);
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            //判断玩家任务进度是否有对应的事件和事件名称区间
            DelegationModelList delegations = DelegationLibrary.GetDelegationsByMainTaskId(MainTaskId);
            if (delegations == null)
            {
                Log.Warn("player {0} RefreshDelegation GetDelegationsByMainTaskId fail, player mainTaskId is {1}", Uid, MainTaskId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //如果当前已是最高星不允许刷新
            if (CheckDelegationIsMaxQuality(id, delegations))
            {
                Log.Warn("player {0} RefreshDelegation fail, delegation is already max quality {1}", Uid, DelegationLibrary.MaxQuality);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //筛选出比之前星级高的委派事件
            List<DelegationModel> betterDelegations = delegations.SelectBetterQualityDelegation(model.Quality);

            List<int> nameIds = DelegationLibrary.GetNameIdList();
            if (nameIds.Count == 0)
            {
                Log.Warn("player {0} RefreshDelegation GetNameIdList fail, xml param error", Uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //获取身上的委派事件
            Dictionary<int, DelegationItem> delegationList = DelegationMng.GetDelegationList();
            List<int> randomNames = GetRandomNames(delegationList, nameIds);

            //直接随机1个名字   
            int newName = GetRandomDelegationName(randomNames);
            if (newName == 0)
            {
                Log.Warn("player {0} get delegation reward GetRandomDelegationName fail, nameId count {1}", Uid, nameIds.Count);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //直接随机1个新事件                
            DelegationModel newModel = UserRefreshDelegationModel(id, betterDelegations);
            if (newModel == null)
            {
                Log.Warn("player {0} get delegation reward RandomDelegationModel fail, no new delegation {1}", Uid, id);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //移除信息
            DelegationMng.RemoveDelegationItem(nameId);
            //移除上阵伙伴
            DelegationMng.RemoveDelegatedHero(oldDelegationItem.HeroList);

            //创建新数据
            DelegationItem delegationItem = CreateNewDelegationItem(newName, newModel);
            DelegationMng.AddDelegationItem(delegationItem);

            //添加到数据库
            SyncUpdateDelegations();

            //绑定随机事件信息
            DELEGATION_ITEM delegationItemMsg = GetDelegationItem(delegationItem);
            response.Item = delegationItemMsg;

            //扣钻石
            DelCoins(CurrenciesType.diamond, price, ConsumeWay.RefreshDelegation, id.ToString());

            //刷新次数增加
            UpdateCounter(CounterType.DelegateRefreshCount, 1);

            response.Price = DelegationLibrary.GetRefreshCoins(GetCounterValue(CounterType.DelegateRefreshCount));
            response.Result = (int)ErrorCode.Success;
            Write(response);

            //komoelog
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)CurrenciesType.diamond, price);
            KomoeEventLogDelegateTasks(4, id.ToString(), nameId.ToString(), null, model.Quality, newModel.Quality, model.Quality, model.MissionDuration, 0, null, consume);
        }

        //private Tuple<DelegationModel, int> RandomDelegation(List<DelegationModel> levelDelegations, List<int> levelNames)
        //{          
        //    Dictionary<int, DelegationModel> delegationList = RandomDelegationModel(levelDelegations, 1);
        //    List<int> nameList = RandomDelegationName(levelNames, 1);

        //    return Tuple.Create(delegationList.Last().Value, nameList.Last());
        //}

        //购买委派次数
        //public void BuyDelegationCount()
        //{
        //    MSG_ZGC_BUY_DELEGATION_COUNT response = new MSG_ZGC_BUY_DELEGATION_COUNT();

        //    //判断是否已达购买次数上限
        //    int buyCount = GetCounterValue(CounterType.DelegateBuyCount);
        //    if (buyCount >= DelegationLibrary.MaxBuyCount)
        //    {
        //        Log.Warn("player {0} buy delegation count fail, buyCount {1} is already maxBuyCount", Uid, buyCount);
        //        response.Result = (int)ErrorCode.MaxBuyCount;
        //        Write(response);
        //        return;
        //    }
        //    int restCount = DelegationMng.GetRestDelegateCount(GetCounterValue(CounterType.DelegateCount), GetCounterValue(CounterType.DelegateBuyCount));
        //    if (restCount >= DelegationLibrary.RenewDelegateCount + DelegationLibrary.MaxBuyCount)
        //    {
        //        Log.Warn("player {0} buy delegation count fail, delegateCount {1} is already max", Uid, restCount);
        //        response.Result = (int)ErrorCode.MaxDelegteCount;
        //        Write(response);
        //        return;
        //    }
        //    //判断货币是否足够
        //    int coins = GetCoins(CurrenciesType.diamond);
        //    int price = DelegationLibrary.GetBuyCountCoins(GetCounterValue(CounterType.DelegateBuyCount));
        //    if (coins < price)
        //    {
        //        Log.Warn("diamond is not enough, curCoin is {0}", coins);
        //        response.Result = (int)ErrorCode.DiamondNotEnough;
        //        Write(response);
        //        return;
        //    }
        //    //扣货币
        //    DelCoins(CurrenciesType.diamond, price, ConsumeWay.BuyDelegationCount, CurrenciesType.diamond.ToString());
        //    //增加次数
        //    UpdateCounter(CounterType.DelegateBuyCount, 1);
        //    int delegateBuyCount = GetCounterValue(CounterType.DelegateBuyCount);
        //    response.BuyPrice = DelegationLibrary.GetBuyCountCoins(delegateBuyCount);
        //    //response.Count = DelegationMng.GetRestDelegateCount(GetCounterValue(CounterType.DelegateCount), delegateBuyCount);
        //    response.RemainCount = DelegationLibrary.MaxBuyCount - delegateBuyCount;
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //private bool IsContainsAll(List<int> listA, List<int> listB)
        //{
        //    return listB.All(b => listA.Any(a => a == b));
        //}      

        private bool CheckDelegationIsMaxQuality(int id, DelegationModelList delegations)
        {
            DelegationModel delegation = delegations.List.LastOrDefault();
            if (delegation != null && id == delegation.Id && delegation.Quality == DelegationLibrary.MaxQuality)
            {
                return true;
            }
            return false;
        }

        private DelegationModel UserRefreshDelegationModel(int id, List<DelegationModel> betterDelegations)
        {
            DelegationModel info = null;
            int rand = NewRAND.Next(1, 10000);
            int skipGrade = 1;
            int index = 0;
            for (int i = 0; i < betterDelegations.Count; i++)
            {
                if (betterDelegations[i].Id == id)
                {
                    index = i;
                    int lastProb = 0;
                    foreach (var kv in betterDelegations[i].SkipProbList)
                    {
                        if (rand > lastProb && rand <= kv.Key)
                        {
                            skipGrade = kv.Value;
                        }
                        lastProb = kv.Key;
                    }
                    break;
                }
            }
            if (index + skipGrade < betterDelegations.Count)
            {
                info = betterDelegations[index + skipGrade];
            }
            else
            {
                info = betterDelegations.LastOrDefault();
            }
            return info;
        }

        public ZMZ_DELEGATION_INFO GetDelegationTransform()
        {
            ZMZ_DELEGATION_INFO info = new ZMZ_DELEGATION_INFO();
            info.Uid = Uid;
            Dictionary<int, DelegationItem> delegationList = DelegationMng.GetDelegationList();
            foreach (var kv in delegationList)
            {
                info.ItemList.Add(GenerateDelegationItem(kv.Value));
            }
            info.DelegatedHeros.AddRange(DelegationMng.GetDelegatedHeros());
            return info;
        }

        private ZMZ_DELEGATION_ITEM GenerateDelegationItem(DelegationItem delegation)
        {
            ZMZ_DELEGATION_ITEM item = new ZMZ_DELEGATION_ITEM();
            item.Id = delegation.Id;
            item.Name = delegation.Name;
            item.State = delegation.State;
            foreach (var hero in delegation.HeroList)
            {
                item.HeroList.Add(hero);
            }
            item.EndTime = Timestamp.GetUnixTimeStampSeconds(delegation.EndTime);
            return item;
        }

        public void LoadDelegationTransform(ZMZ_DELEGATION_INFO info)
        {
            foreach (var item in info.ItemList)
            {
                DelegationItem dItem = new DelegationItem();
                dItem.Id = item.Id;
                dItem.Name = item.Name;
                dItem.State = item.State;
                dItem.HeroList = new List<int>();
                foreach (var hero in item.HeroList)
                {
                    dItem.HeroList.Add(hero);
                }
                dItem.EndTime = Timestamp.TimeStampToDateTime(item.EndTime);
                DelegationMng.AddDelegationItem(dItem);
            }
            foreach (var hero in info.DelegatedHeros)
            {
                DelegationMng.AddDelegatedHero(hero);
            }
        }

        public void SendDelegationInfo()
        {
            //委派事件开启限制
            if (!CheckLimitOpen(LimitType.DelegationEvent))
            {
                return;
            }
            MSG_ZGC_DELEGATION_LIST msg = new MSG_ZGC_DELEGATION_LIST();

            foreach (var delegation in DelegationMng.GetDelegationList())
            {
                DELEGATION_ITEM item = GetDelegationItem(delegation.Value);
                msg.Item.Add(item);
            }
            int refreshCount = GetCounterValue(CounterType.DelegateRefreshCount);
            msg.Price = DelegationLibrary.GetRefreshCoins(refreshCount);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }   
    }
}

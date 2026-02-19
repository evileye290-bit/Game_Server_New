using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class TimingGiftStepInfo
    {
        public int RechargeItemId { get; set; }
        public int Step { get; set; }
        public TimingGiftType TimingGiftType { get; set; }

        public bool IsPlus()
        {
            //单数为普通挡位，双数为plus
            return Step != 0 && Step % 2 == 0;
        }
    }

    public class ActionLibrary
    {
        private static List<int> initActions = new List<int>();
        private static List<int> todayActions = new List<int>();
        private static Dictionary<int, ActionModel> actionList = new Dictionary<int, ActionModel>();
        //private static Dictionary<ActionMainType, Dictionary<int, ActionModel>> typeAction = new Dictionary<ActionMainType, Dictionary<int, ActionModel>>();
        private static Dictionary<int, int> actionLineNextId = new Dictionary<int, int>();

        private static ListMap<PlayType, TimingGiftType> playType2GiftTypes = new ListMap<PlayType, TimingGiftType>();

        //每一个action的权重
        private static Dictionary<int, Dictionary<TimingGiftType, float>> actionId2GiftTypesWeight = new Dictionary<int, Dictionary<TimingGiftType, float>>();

        //每个挡位的充值，2的倍数都是plus
        private static Dictionary<int, Dictionary<TimingGiftType, int>> giftRechargeId = new Dictionary<int, Dictionary<TimingGiftType, int>>();
        private static Dictionary<int, TimingGiftStepInfo> giftRechargeInfoId = new Dictionary<int, TimingGiftStepInfo>();

        //money 对应的普通挡位
        private static Dictionary<int, float> money2SimpleStep = new Dictionary<int, float>();
        //private static Dictionary<int, int> simpleStep2Plus = new Dictionary<int, int>();

        private static Dictionary<TimingGiftType, LimitType> timingGiftLimit = new Dictionary<TimingGiftType, LimitType>();
        public static Dictionary<TimingGiftType, LimitType> TimingGiftLimit => timingGiftLimit;

        private static Dictionary<TimingGiftType, int> timingGiftDailyLimitCount = new Dictionary<TimingGiftType, int>();

        public static List<int> InitActions => initActions;
        public static List<int> TodayActions => todayActions;

        public static int DailyTimingBagAddWeight { get; set; }
        public static int WeeklyTimingBagAddWeight { get; set; }
        public static int MonthlyTimingBagAddWeight { get; set; }
        public static int ShopBuyAddWeight { get; set; }
        public static float ResourceCurrDivideCostMin { get; set; }
        public static float ResourceCurrDivideCostMax { get; set; }
        public static float ResourceCurrDivideCostRatio { get; set; }
        public static float BuyTimingGiftTypeAddWeightRatio { get; set; }
        public static float NotBuyTimingGiftTypeAddWeightRatio { get; set; }
        public static int PlayTypeCostDiamondDivideParam { get; set; }

        /// <summary>
        /// 最大普通挡位
        /// </summary>
        public static int MaxStep { get; private set; }
        public static float MaxMoney { get; private set; }


        public static void Init(DateTime time)
        {
            //actionList.Clear();
            //typeAction.Clear();
            //todayActions.Clear();
            //giftRechargeId.Clear();
            //actionId2GiftTypesWeight.Clear();
            //playType2GiftTypes.Clear();
            //giftRechargeInfoId.Clear();
            //money2SimpleStep.Clear();
            //timingGiftLimit.Clear();
            //timingGiftDailyLimitCount.Clear();

            InitConfig();
            InitAction();
            InitTimingGiftWeight();
            InitTimingGiftRechargeItem();
            InitPlayType2TimingGiftTypes();
            InitTimingGiftLimit();
            InitTimingGiftDailtLimitCount();

            RefreshTodayAction(time);
        }

        public static void RefreshTodayAction(DateTime time)
        {
            List<int> todayActions = new List<int>();

            List<ActionModel> models = actionList.Values.Where(x => x.ActionFrequence == ActionFrequence.Time).ToList();
            foreach (var kv in models)
            {
                if (kv.StartTime <= time.Date && kv.EndTime > time)
                {
                    todayActions.Add(kv.Id);
                }
            }
            ActionLibrary.todayActions = todayActions;
        }

        #region action

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("ActionConfig", 1);
            DailyTimingBagAddWeight = data.GetInt("DailyTimingBagAddWeight");
            WeeklyTimingBagAddWeight = data.GetInt("WeeklyTimingBagAddWeight");
            MonthlyTimingBagAddWeight = data.GetInt("MonthlyTimingBagAddWeight");
            ShopBuyAddWeight = data.GetInt("ShopBuyAddWeight");
            ResourceCurrDivideCostMin = data.GetFloat("ResourceCurrDivideCostMin");
            ResourceCurrDivideCostMax = data.GetFloat("ResourceCurrDivideCostMax");
            ResourceCurrDivideCostRatio = data.GetFloat("ResourceCurrDivideCostRatio");
            BuyTimingGiftTypeAddWeightRatio = data.GetFloat("BuyTimingGiftTypeAddWeightRatio");
            NotBuyTimingGiftTypeAddWeightRatio = data.GetFloat("NotBuyTimingGiftTypeAddWeightRatio");
            PlayTypeCostDiamondDivideParam = data.GetInt("PlayTypeCostDiamondDivideParam");
        }

        private static void InitAction()
        {
            List<int> initActions = new List<int>();
            Dictionary<int, ActionModel> actionList = new Dictionary<int, ActionModel>();
            //Dictionary<ActionMainType, Dictionary<int, ActionModel>> typeAction = new Dictionary<ActionMainType, Dictionary<int, ActionModel>>();
            Dictionary<int, int> actionLineNextId = new Dictionary<int, int>();

            ActionModel model, lastModel = null;
            //Dictionary<int, ActionModel> actions = new Dictionary<int, ActionModel>();
            //ActionMainType cacheType = ActionMainType.None;

            DataList dataList = DataListManager.inst.GetDataList("Action");
            foreach (var kv in dataList)
            {
                model = new ActionModel(kv.Value);
                actionList.Add(model.Id, model);

                if (lastModel == null)
                {
                    lastModel = model;
                }

                //if (cacheType != model.ActionMainType)
                //{
                //    cacheType = model.ActionMainType;

                //    actions = new Dictionary<int, ActionModel>();
                //    typeAction.Add(model.ActionMainType, actions);
                //}

                //actions.Add(model.Id, model);

                if (model.CurrStep == 1)
                {
                    initActions.Add(model.Id);
                }

                if (lastModel.ActionType == model.ActionType && (lastModel.CurrStep + 1) == model.CurrStep)
                {
                    actionLineNextId.Add(lastModel.Id, model.Id);
                }
            }
            ActionLibrary.initActions = initActions;
            ActionLibrary.actionList = actionList;
            //ActionLibrary.typeAction = typeAction;
            ActionLibrary.actionLineNextId = actionLineNextId;
        }

        public static ActionModel GetActionModel(int id)
        {
            ActionModel model;
            actionList.TryGetValue(id, out model);
            return model;
        }

        public static ActionModel GetNextActionModel(int id)
        {
            if (actionLineNextId.ContainsKey(id))
            {
                ActionModel model;
                actionList.TryGetValue(actionLineNextId[id], out model);
                return model;
            }

            return null;
        }

        //public static List<ActionModel> GetInitActions()
        //{
        //    return actionList.Values.Where(x => initActions.Contains(x.Id)).ToList();
        //}

        //public static Dictionary<ActionMainType, Dictionary<int, ActionModel>> GetAllTypeActions()
        //{
        //    return typeAction;
        //}

        #endregion


        #region weight

        /// <summary>
        /// 每一项action的权重
        /// </summary>
        private static void InitTimingGiftWeight()
        {
            Dictionary<int, Dictionary<TimingGiftType, float>> actionId2GiftTypesWeight = new Dictionary<int, Dictionary<TimingGiftType, float>>();

            int count = Enum.GetValues(typeof(TimingGiftType)).Length;
            DataList dataList = DataListManager.inst.GetDataList("TimingGiftWeight");
            foreach (var kv in dataList)
            {
                Dictionary<TimingGiftType, float> weight = new Dictionary<TimingGiftType, float>();
                foreach (TimingGiftType item in Enum.GetValues(typeof(TimingGiftType)))
                {
                    if (item == TimingGiftType.None) continue;

                    weight.Add(item, kv.Value.GetFloat(item.ToString()));
                }
                actionId2GiftTypesWeight.Add(kv.Value.ID, weight);
            }

            if (actionId2GiftTypesWeight.Count != actionList.Count)
            {
                Logger.Log.Warn($"InitTimingGiftWeight error，error info TimingGiftWeight count {actionId2GiftTypesWeight.Count} not equal to acions count {actionList.Count} ");
            }
            ActionLibrary.actionId2GiftTypesWeight = actionId2GiftTypesWeight;
        }

        public static Dictionary<TimingGiftType, float> GetActionWeight(int actionId)
        {
            Dictionary<TimingGiftType, float> weight = null;
            Dictionary<TimingGiftType, float> result;
            if (actionId2GiftTypesWeight.TryGetValue(actionId, out result))
            {
                weight = new Dictionary<TimingGiftType, float>(result);
            }
            return weight;
        }

        #endregion

        #region TimingGiftRechargeItem

        private static void InitTimingGiftRechargeItem()
        {
            Dictionary<int, Dictionary<TimingGiftType, int>> giftRechargeId = new Dictionary<int, Dictionary<TimingGiftType, int>>();
            Dictionary<int, TimingGiftStepInfo> giftRechargeInfoId = new Dictionary<int, TimingGiftStepInfo>();
            Dictionary<int, float> money2SimpleStep = new Dictionary<int, float>();
            //Dictionary<int, int> simpleStep2Plus = new Dictionary<int, int>();

            int simpleId = 0;
            DataList dataList = DataListManager.inst.GetDataList("TimingGiftRechargeItem");
            foreach (var kv in dataList)
            {
                int id = kv.Value.ID;
                float money = kv.Value.GetFloat("Money");
                foreach (TimingGiftType item in Enum.GetValues(typeof(TimingGiftType)))
                {
                    if (item == TimingGiftType.None) continue;

                    int itemId = kv.Value.GetInt(item.ToString());
                    AddRechargeId(id, item, itemId, giftRechargeId);

                    giftRechargeInfoId[itemId] = new TimingGiftStepInfo() { RechargeItemId = itemId, Step = id, TimingGiftType = item };
                }

                if (id % 2 == 1)
                {
                    simpleId = id;
                    money2SimpleStep.Add(id, money);

                    if (MaxStep < id)
                        MaxStep = id;

                    if (MaxMoney < money)
                    {
                        MaxMoney = money;
                    }
                }
                //else
                //{
                //    simpleStep2Plus[simpleId] = id;
                //}
            }

            //simpleStep2Plus = simpleStep2Plus.OrderBy(k => k.Value).ToDictionary(k=>k.Key, v=>v.Value);
            money2SimpleStep = money2SimpleStep.OrderBy(k => k.Value).ToDictionary(k=>k.Key, v=>v.Value);

            ActionLibrary.giftRechargeId = giftRechargeId;
            ActionLibrary.giftRechargeInfoId = giftRechargeInfoId;
            ActionLibrary.money2SimpleStep = money2SimpleStep;
            //ActionLibrary.simpleStep2Plus = simpleStep2Plus;
        }

        private static void AddRechargeId(int step, TimingGiftType type, int id, Dictionary<int, Dictionary<TimingGiftType, int>> giftRechargeId)
        {         
            Dictionary<TimingGiftType, int> second;
            if (!giftRechargeId.TryGetValue(step, out second))
            {
                second = new Dictionary<TimingGiftType, int>();
                giftRechargeId.Add(step, second);
            }

            second.Add(type, id);
        }

        public static int GetGiftItemId(int step, TimingGiftType type)
        {
            Dictionary<TimingGiftType, int> second;
            if (giftRechargeId.TryGetValue(step, out second) && second.ContainsKey(type))
            {
                return second[type];
            }
            return 0;
        }

        public static TimingGiftStepInfo GetGiftStepInfoByItemId(int id)
        {
            TimingGiftStepInfo info;
            giftRechargeInfoId.TryGetValue(id, out info);
            return info;
        }

        public static int GetStepByMoney(float money)
        {
            if (money >= MaxMoney) return MaxStep;

            int step = 1;
            foreach (var kv in money2SimpleStep)
            {
                if (money >= kv.Value)
                {
                    step = kv.Key;
                }
            }
            return step;
        }

        public static int GetSmallerStep(int step)
        {
            int  level = step;
            foreach (var kv in money2SimpleStep)
            {
                if (kv.Key <= step)
                {
                    level = kv.Key;
                }
            }
            return level;
        }

        //public static float GetStepMoney(int step)
        //{
        //    float money = 0;
        //    foreach (var kv in money2SimpleStep)
        //    {
        //        if (step >= kv.Key)
        //        {
        //            money = kv.Value;
        //        }
        //    }
        //    return money;
        //}

        //public static int GetPlus(int step)
        //{
        //    if (simpleStep2Plus.ContainsKey(step))
        //    {
        //        return simpleStep2Plus[step];
        //    }
        //    return step;
        //}

        //public static int GetNextStep(int step)
        //{
        //    int cache = step;
        //    if (simpleStep2Plus.ContainsKey(step))
        //    {
        //        //当前时普通挡位，到下一档+2
        //        cache = step + 2;
        //    }
        //    else if(simpleStep2Plus.ContainsValue(step))
        //    {
        //        //当前是plus挡位，到下一档+1
        //        cache += 1;
        //    }

        //    //有下一档位
        //    if (simpleStep2Plus.ContainsValue(cache))
        //    {
        //        return cache;
        //    }

        //    //没有下一档，返回当前档
        //    return step;
        //}

        #endregion

        private static void InitPlayType2TimingGiftTypes()
        {
            ListMap<PlayType, TimingGiftType> playType2GiftTypes = new ListMap<PlayType, TimingGiftType>();

            DataList dataList = DataListManager.inst.GetDataList("PlayType2TimingGiftType");
            foreach (var kv in dataList)
            {
                PlayType playType = (PlayType)kv.Value.ID;
                string str = kv.Value.GetString("TimingGiftType");
                if (string.IsNullOrEmpty(str))
                {
                    playType2GiftTypes.Add(playType, new List<TimingGiftType>());
                }
                else
                { 
                    playType2GiftTypes.Add(playType, str.Split('|').ToList().ConvertAll(x=>(TimingGiftType)int.Parse(x)));
                }
            }
            ActionLibrary.playType2GiftTypes = playType2GiftTypes;
        }

        public static List<TimingGiftType> GetPlayType2TimingGiftTypes(PlayType playType)
        {
            List<TimingGiftType> result;
            playType2GiftTypes.TryGetValue(playType, out result);
            return result;
        }

        private static void InitTimingGiftLimit()
        {
            Dictionary<TimingGiftType, LimitType> timingGiftLimit = new Dictionary<TimingGiftType, LimitType>();

            Data data = DataListManager.inst.GetData("TimingGiftLimit", 1);
            foreach (TimingGiftType item in Enum.GetValues(typeof(TimingGiftType)))
            {
                LimitType limitType = (LimitType)data.GetInt(item.ToString());
                if (LimitLibrary.GetLimitData(limitType) == null) continue;

                timingGiftLimit[item] = limitType;
            }
            ActionLibrary.timingGiftLimit = timingGiftLimit;
        }

        private static void InitTimingGiftDailtLimitCount()
        {
            Dictionary<TimingGiftType, int> timingGiftDailyLimitCount = new Dictionary<TimingGiftType, int>();

            Data data = DataListManager.inst.GetData("TimingGiftDailyLimit", 1);
            foreach (TimingGiftType item in Enum.GetValues(typeof(TimingGiftType)))
            {
                timingGiftDailyLimitCount[item] = data.GetInt(item.ToString());
            }
            ActionLibrary.timingGiftDailyLimitCount = timingGiftDailyLimitCount;
        }

        public static int GetTimingGiftDailtLimitCount(TimingGiftType giftType)
        {
            int count = 0;
            timingGiftDailyLimitCount.TryGetValue(giftType, out count);
            return count;
        }
    }
}

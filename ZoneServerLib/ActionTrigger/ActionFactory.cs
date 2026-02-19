using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public static class ActionFactory
    {
        public static BaseAction CreateAction(ActionModel model, ActionManager manager, ActionInfo actionInfo)
        {
            switch (model.ActionType)
            {
                case ActionType.GainQualityHeroNum:
                    return new GainQualityHeroAction(manager, model, actionInfo);
                case ActionType.GainIdLimitHero:
                    return new GainIdLimitHeroAction(manager, model, actionInfo);
                case ActionType.Resonance:
                    return new ResonanceAction(manager, model, actionInfo);
                case ActionType.ResonancePosCount:
                    return new ResonancePosCountAction(manager, model, actionInfo);
                case ActionType.PerHeroAdvance:
                    return new PerHeroAdvanceAction(manager, model, actionInfo);
                case ActionType.HeroSoulRingYear:
                    return new HeroSoulRingTrainAction(manager, model, actionInfo);
                case ActionType.HerosSoulRingLevel:
                    return new HerosSoulRingLevelAction(manager, model, actionInfo);
                case ActionType.GotQualitySoulBone:
                    return new GotQualitySoulBoneAction(manager, model, actionInfo);
                case ActionType.GotQualityEquipment:
                    return new GotQualityEquipmentAction(manager, model, actionInfo);
                case ActionType.EquipmentTrain:
                    return new EquipmentTrainAction(manager, model, actionInfo);
                case ActionType.HeroEquipmentTrain:
                    return new HeroEquipmentTrainAction(manager, model, actionInfo);
                case ActionType.HeroEquipmentInject:
                    return new HeroEquipmentInjectAction(manager, model, actionInfo);
                case ActionType.OneQualityHeroAdvanceAction:
                    return new OneQualityHeroAdvanceAction(manager, model, actionInfo);
                case ActionType.MainTask:
                    return new MainTaskAction(manager, model, actionInfo);
                case ActionType.SecretArea:
                    return new SecretAreaAction(manager, model, actionInfo);
                case ActionType.BuyAllDailyGiftBag:
                    return new BuyAllDailyGiftBagAction(manager, model, actionInfo);
                case ActionType.BuyAllWeeklyGiftBag:
                    return new BuyAllWeeklyGiftBagAction(manager, model, actionInfo);
                case ActionType.BuyAllMonthlyGiftBag:
                    return new BuyAllMonthlyGiftBagAction(manager, model, actionInfo);
                case ActionType.BuyAllFirstPayDiamond:
                    return new BuyAllFirstPayDiamondAction(manager, model, actionInfo);
                case ActionType.PayActive:
                    return new PayActiveAction(manager, model, actionInfo);
                case ActionType.OpenNewServer:
                    return new OpenNewServerAction(manager, model, actionInfo);
                case ActionType.OnlineHighest:
                    return new OnlineHighestAction(manager, model, actionInfo);
                case ActionType.BackMoreThanDayes:
                    return new BackMoreThanDayesAction(manager, model, actionInfo);
                case ActionType.DailyTaskFinishCount:
                    return  new DailyTaskFinishCountAction(manager, model, actionInfo);
                case ActionType.DailyDiamondCostCount:
                    return new DailyDiamondCostCountAction(manager, model, actionInfo);
                case ActionType.BuyTheLastTimingLimitGift:
                    return new BuyTheLastTimingLimitGiftAction(manager, model, actionInfo);
                default:
                    return new BaseAction(manager, model, actionInfo);
            }
        }
    }
}

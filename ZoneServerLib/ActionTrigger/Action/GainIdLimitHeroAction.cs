using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    /// <summary>
    /// 获得某个id的Hero
    /// model param1 heroId
    /// </summary>
    public class GainIdLimitHeroAction : BaseAction
    {
        public GainIdLimitHeroAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        { 
        }

        public override bool Check(object obj)
        {
            if (CheckOutOfTime()) return false;

            int heroId;
            int.TryParse(obj.ToString(), out heroId);

            HeroModel heroModel = HeroLibrary.GetHeroModel(heroId);
            if (heroModel == null) return false;

            if (heroId == model.Param1)
            {
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                SetNum(heroId, true);
                return true;
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            SetNum(model.Param1, true);
        }
    }
}

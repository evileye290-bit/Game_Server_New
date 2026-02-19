using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    /// <summary>
    /// 获得某个品质的Hero
    /// model param1 quality
    /// model param2 num
    /// </summary>
    public class GainQualityHeroAction : BaseAction
    {
        public GainQualityHeroAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        { 
        }

        public override bool Check(object obj)
        {
            int heroId;
            int.TryParse(obj.ToString(), out heroId);

            HeroModel heroModel = HeroLibrary.GetHeroModel(heroId);
            if (heroModel == null) return false;

            //数字越小品质越高
            if (heroModel.Quality <= model.Param1)
            {
                if (CurNum + 1 >= model.Param2)
                {
                    //走sdk推荐礼包
                    if (CheckActionBySdk(model.Param2))
                    {
                        return false;
                    }
                    AddNum(1, true);
                    return true;
                }
                AddNum(1);
            }
            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(1, true);
        }
    }
}

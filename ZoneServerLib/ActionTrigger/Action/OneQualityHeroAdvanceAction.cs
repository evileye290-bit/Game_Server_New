using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    /// <summary>
    /// 某个品质的hero首次进阶。param1品质，param2达到的星级
    /// </summary>
    public class OneQualityHeroAdvanceAction : BaseAction
    {
        private Dictionary<int, int> heroLastReach = new Dictionary<int, int>();

        public OneQualityHeroAdvanceAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        protected override void InitActionInfo()
        {
            heroLastReach = actionInfo.Infos.ToDictionary('|', ':');
        }

        public override string BuildActionInfo()
        {
            return heroLastReach.ToString("|", ":");
        }

        public override bool Check(object obj)
        {
            int heroId;
            int.TryParse(obj.ToString(), out heroId);
            if (heroLastReach.ContainsKey(heroId)) return false;

            HeroInfo heroInfo = Owner.HeroMng.GetHeroInfo(heroId);
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroId);
            if (heroInfo == null || heroModel == null || heroModel.Quality != model.Param1) return false;

            if (heroInfo.StepsLevel >= model.Param2)
            {
                SetHeroLastReach(heroId, 1);
                AddNum(1);
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param2))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private void SetHeroLastReach(int heroId, int level)
        {
            heroLastReach[heroId] = level;
        }
    }
}

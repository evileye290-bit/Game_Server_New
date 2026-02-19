using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{

    /// <summary>
    /// 每个hero进阶param1品质，param2步进参数
    /// </summary>
    public class PerHeroAdvanceAction : BaseAction
    {
        private Dictionary<int, int> heroLastReach = new Dictionary<int, int>();

        public PerHeroAdvanceAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
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

            HeroInfo heroInfo = Owner.HeroMng.GetHeroInfo(heroId);
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroId);
            if (heroInfo == null || heroModel == null || heroModel.Quality > model.Param1) return false;


            int level = GetHeroLastReach(heroId);
            int limit = level + model.Param2;

            if (heroInfo.StepsLevel >= limit)
            {
                //记录当前可以触发的条件即可，避免连续培养每次都触发
                SetHeroLastReach(heroId, limit);
                AddNum(1);
                //走sdk推荐礼包
                if (CheckActionBySdk(limit))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private int GetHeroLastReach(int heroId)
        {
            int level;
            heroLastReach.TryGetValue(heroId, out level);
            return level;
        }

        private void SetHeroLastReach(int heroId, int level)
        {
            heroLastReach[heroId] = level;
        }
    }
}

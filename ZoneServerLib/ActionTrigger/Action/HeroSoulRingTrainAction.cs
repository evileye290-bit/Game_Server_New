using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// 某个Hero所有魂环达到某年限
    /// model param1 year
    /// </summary>
    public class HeroSoulRingTrainAction : BaseAction
    {
        public HeroSoulRingTrainAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int heroId;
            int.TryParse(obj.ToString(), out heroId);

            HeroInfo heroInfo = Owner.HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null) return false;

            Dictionary<int, SoulRingItem> soulRings = Owner.SoulRingManager.GetAllEquipedSoulRings(heroId);
            if (soulRings == null || soulRings.Count < 10) return false;

            SoulRingItem soulRingItem = soulRings.Values.Where(x => x.Year < model.Param1).FirstOrDefault();
            if (soulRingItem == null)
            {
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                //所有魂环都满足年限
                AddNum(1, true);
                return true;
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            //所有魂环都满足年限
            AddNum(1, true);
        }
    }
}

using ServerModels;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// 某几个英雄魂环强化等级
    /// model param1 英雄数量
    /// model param2 强化等级步进
    /// </summary>
    public class HerosSoulRingLevelAction : BaseAction
    {
        public HerosSoulRingLevelAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int levelLimit = CurNum + model.Param2;

            int count = Owner.HeroMng.GetHeroInfoList().Values.Where(x => x.SoulSkillLevel >= levelLimit).Count();
            if (count >= model.Param1)
            {
                SetNum(levelLimit);
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                return true;
            }
            return false;   
        }
    }
}

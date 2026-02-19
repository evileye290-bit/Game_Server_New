using CommonUtility;
using Logger;
using ServerShared;

namespace ZoneServerLib
{
    public class CureByMarkCountTriHdl : BaseTriHdl
    {
        private readonly int markId;
        private float growth, baseValue;

        public CureByMarkCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3 || !int.TryParse(param[0], out markId) || !float.TryParse(param[1], out growth) || !float.TryParse(param[2], out baseValue))
            {
                Log.Warn("init CureByMarkCountTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            Mark mark = Owner.MarkManager.GetMark(markId);
            if (mark == null || mark.CurCount < 1) return;

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int hp = (int)(trigger.CalcParam(growth, baseValue, skillLevelGrowth) * mark.CurCount);

            Owner.AddHp(trigger.Caster, hp);
            SetThisFspHandled();
        }
    }
}
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class BeenAddedTypeBuffTriCon : BaseTriCon
    {
        private int buffType;
        private int count;
        public BeenAddedTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out buffType) || !int.TryParse(info[1], out count))
            {
                Log.Warn($"init BeenAddedTypeBuffTriCon failed: invalid skill param {conditionParam}");
            }
        }

        public override bool Check()
        {
            return trigger.GetParam_BeenAddedTypeBuffCount(buffType) >= count;
        }
    }
}


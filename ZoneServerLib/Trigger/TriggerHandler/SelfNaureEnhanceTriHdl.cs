using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SelfNaureEnhanceTriHdl : BaseTriHdl
    {
        private readonly float growth;
        private readonly int natureType;
        private readonly int value;
        public SelfNaureEnhanceTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 3)
            {
                Log.WarnLine($"SelfNaureEnhanceTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                growth = float.Parse(param[0]);
                natureType = int.Parse(param[1]);
                value = (int)float.Parse(param[2]);
            }
        }

        public override void Handle()
        {
            //魂环技能等级
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            int natureValue = (int)trigger.CalcParam(growth, value, skillLevelGrowth);
            Owner.AddNatureAddedValue((NatureType)natureType, natureValue, true);
        }
    }
}

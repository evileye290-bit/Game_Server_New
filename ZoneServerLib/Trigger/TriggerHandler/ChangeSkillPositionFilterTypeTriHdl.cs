using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class ChangeSkillPositionFilterTypeTriHdl : BaseTriHdl
    {     
        readonly int skillId;
        readonly SkillPositionFilterType skillPositionFilterType;

        public ChangeSkillPositionFilterTypeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2)
            {
                Log.WarnLine($"ChangeSkillPositionFilterTypeTriHdl param error need params leng 2, current param {handlerParam}");
            }
            else
            {
                skillId = int.Parse(param[0]);
                skillPositionFilterType = (SkillPositionFilterType)int.Parse(param[1]);
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            Skill skill = Owner.SkillManager.GetSkill(skillId);
            skill?.SkillModel.SetSkillPositionFilterType(skillPositionFilterType);

            SetThisFspHandled();
        }
    }
}

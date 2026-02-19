using CommonUtility;

namespace ZoneServerLib
{
    public class SkillReadyTriHdl : BaseTriHdl
    {
        readonly int skillId = 0;
        public SkillReadyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            skillId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            // 将skill加入到skillEngine中
            //Log.Warn("in skill ready handler, owner {0} add skill {1} to engine ", Owner.InstanceId, skillId);
            Owner.SkillEngine.AddSkill(skillId, trigger);
        }
    }
}

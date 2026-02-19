using System.Linq;
using CommonUtility;

namespace ZoneServerLib
{
    public class AllyHeroAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;

        public AllyHeroAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            CurMap?.HeroList.Where(x => x.Value.IsAlly(trigger.Owner) && x.Value.InstanceId != Owner.InstanceId)
                .ForEach(x => x.Value.AddBuff(trigger.Caster, buffId, skillLevelGrowth));
        }
    }
    
}
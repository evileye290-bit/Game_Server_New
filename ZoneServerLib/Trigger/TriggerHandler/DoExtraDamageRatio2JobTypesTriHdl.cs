using CommonUtility;
using Logger;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class DoExtraDamageRatio2JobTypesTriHdl : BaseTriHdl
    {
        private Dictionary<JobType, int> jobTypes = new Dictionary<JobType, int>();

        public DoExtraDamageRatio2JobTypesTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            Dictionary<int, int> jobs = handlerParam.ToDictionary('|',':');
            if (jobs.Count == 0)
            {
                Log.Warn($"init DoExtraDamageByPoisonBuffCountTriHdl failed, invalid handler param {handlerParam}");
                return;
            }

            jobTypes = jobs.ToDictionary(x => (JobType)x.Key, v => v.Value);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach(var kv in jobTypes)
            {
                switch (kv.Key)
                {
                    case JobType.SingleAttack:
                        Owner.AddNatureAddedValue(NatureType.PRO_DO_JOB_DAMAGE_SINGLEATTACK, kv.Value);
                        break;
                    case JobType.Tank:
                        Owner.AddNatureAddedValue(NatureType.PRO_DO_JOB_DAMAGE_TANK, kv.Value);
                        break;
                    case JobType.Support:
                        Owner.AddNatureAddedValue(NatureType.PRO_DO_JOB_DAMAGE_SUPPORT, kv.Value);
                        break;
                    case JobType.Control:
                        Owner.AddNatureAddedValue(NatureType.PRO_DO_JOB_DAMAGE_CONTROL, kv.Value);
                        break;
                    case JobType.GroupAttack:
                        Owner.AddNatureAddedValue(NatureType.PRO_DO_JOB_DAMAGE_GROUPATTACK, kv.Value);
                        break;
                }
            }
            SetThisFspHandled();
        }
    }
}

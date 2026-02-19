using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class MonsterGenTriHdl : BaseTriHdl
    {
        private readonly int genId;
        public MonsterGenTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            :base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out genId);
        }

        public override void Handle()
        {
            if(trigger.CurMap == null)
            {
                return;
            }
            BaseMonsterGen gen = trigger.CurMap.GetMonsterGen(genId);
            if(gen != null && gen.GenType == MonsterGenType.Mannual)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();
                gen.GenerateMonstersDelay(gen.Model.GenCount, gen.Model.GenDelay);
            }
        }
    }
}

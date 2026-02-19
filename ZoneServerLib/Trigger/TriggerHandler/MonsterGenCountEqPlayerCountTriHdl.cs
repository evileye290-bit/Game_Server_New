using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class MonsterGenCountEqPlayerCountTriHdl : BaseTriHdl
    {
        private readonly int genId;
        public MonsterGenCountEqPlayerCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
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
                gen.GenerateMonstersDelay(1, gen.Model.GenDelay); //FIXME: BOIL 这个废弃了貌似。这里我随便给了一个1.后续有需求可以再加
            }
        }
    }
}

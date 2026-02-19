using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    class MonsterGenWithOwnerPositionTriHdl : BaseTriHdl
    {
        private readonly int genId;
        public MonsterGenWithOwnerPositionTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            :base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out genId);
        }

        public override void Handle()
        {
            if (trigger.CurMap == null)
            {
                return;
            }
            BaseMonsterGen gen = trigger.CurMap.GetMonsterGen(genId);
            if (gen != null && gen.GenType == MonsterGenType.Mannual)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();
                gen.GenerateMonstersWithPosition(gen.Model.GenCount, trigger.Owner.Position);
            }
        }
    }
}

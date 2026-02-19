using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class RepleaseMonsterTriHdl : BaseTriHdl
    {
        private readonly int genId;
        public RepleaseMonsterTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out genId);
        }

        public override void Handle()
        {
            if (trigger.CurMap == null) return;
            if (Owner.FieldObjectType != TYPE.MONSTER) return;

            BaseMonsterGen gen = trigger.CurMap.GetMonsterGen(genId);
            if (gen != null && gen.GenType == MonsterGenType.Mannual)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();

                Owner.OnChanged();

                long hp = Owner.GetNatureValue(NatureType.PRO_HP);
                if (hp <= 0)
                {
                    hp = 1;
                }

                trigger.CurMap.CreateMonster(gen.Model.MonsterId, Owner.Position, gen, hp);
            }
        }
    }
}

using ServerModels;

namespace ZoneServerLib
{
    public class DrowHateBuff : BaseBuff
    {
        public DrowHateBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (owner.IsDead)
            {
                OnEnd();
                return;
            }

            int hate = (int)c;
            foreach (var kv in owner.CurrentMap.PcList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
            foreach (var kv in owner.CurrentMap.HeroList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
            foreach (var kv in owner.CurrentMap.MonsterList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
        }

        protected override void End()
        {
            int hate = (int)c * -1;
            foreach (var kv in owner.CurrentMap.PcList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
            foreach (var kv in owner.CurrentMap.HeroList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
            foreach (var kv in owner.CurrentMap.MonsterList)
            {
                if (kv.Value.IsEnemy(owner))
                {
                    kv.Value.HateManager?.AddHate(owner, hate);
                }
            }
        }
    }
}

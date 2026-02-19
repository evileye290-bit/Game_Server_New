using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class CureByDamageRatioBuff : BaseBuff
    {
        public CureByDamageRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.DamageOnce, OnDamage);

            base.End();
        }

        private void OnDamage(object obj)
        {
            DamageTriMsg msg = obj as DamageTriMsg;
            if (msg == null) return;

            float ratio = c * 0.0001f;
            caster.AddHp(Caster, (int)(ratio * msg.Damage));
        }
    }
}

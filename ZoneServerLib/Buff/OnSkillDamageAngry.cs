using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    public class OnSkillDamageAngry : BaseBuff
    {
        private long totalDamage;
        private long proDmg;
        public OnSkillDamageAngry(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            totalDamage = 0;
            proDmg = 0;
            AddListener(TriggerMessageType.SkillDamage, OnMessage);
        }

        protected override void Update(float dt)
        {
           
        }

        protected override void End()
        {
            // 还原伤害值加成
            owner.AddNatureAddedValue(NatureType.PRO_DAM, -1 * proDmg);
            RemoveListener(TriggerMessageType.SkillDamage, OnMessage);
        }

        private void OnMessage(object param)
        {
            SkillDamageTriMsg msg = param as SkillDamageTriMsg;
            if (msg != null)
            {
                totalDamage += msg.Damage;
                long newProDmg = BuffParamCalculator.SpecCalc(Name, skillLevel, totalDamage);
                if (newProDmg != proDmg)
                {
                    owner.AddNatureAddedValue(NatureType.PRO_DAM, proDmg * -1, Model.Notify);
                    owner.AddNatureAddedValue(NatureType.PRO_DAM, newProDmg, Model.Notify);
                    proDmg = newProDmg;
                }
            }
        }
    }
}

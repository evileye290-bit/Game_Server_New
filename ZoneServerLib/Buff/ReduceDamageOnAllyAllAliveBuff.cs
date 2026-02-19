using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ReduceDamageOnAllyAllAliveBuff : BaseBuff
    {      
        public ReduceDamageOnAllyAllAliveBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.AllyDead, AllyDead);
            Effect();
        }

        protected override void Update(float dt)
        {
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.AllyDead, AllyDead);          
        }

        private void Effect()
        {         
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, (int)c, Model.Notify);
        }

        private void UnEffect()
        {       
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, (int) c * -1);
        }

        private void AllyDead(object param)
        {          
            // 属性还原
            UnEffect();
        }
    }
}

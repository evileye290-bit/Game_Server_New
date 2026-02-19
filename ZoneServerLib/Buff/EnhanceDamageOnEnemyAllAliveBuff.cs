using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class EnhanceDamageOnEnemyAllAliveBuff : BaseBuff
    {
        public EnhanceDamageOnEnemyAllAliveBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.EnemyDead, EnemyDead);
            Effect();
        }

        protected override void Update(float dt)
        {
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.EnemyDead, EnemyDead);
        }

        private void Effect()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DAM, (int)c, Model.Notify);
        }

        private void UnEffect()
        {
            owner.AddNatureAddedValue(NatureType.PRO_DAM, (int)c * -1);
        }

        private void EnemyDead(object param)
        {
            // 属性还原
            UnEffect();
        }
    }
}

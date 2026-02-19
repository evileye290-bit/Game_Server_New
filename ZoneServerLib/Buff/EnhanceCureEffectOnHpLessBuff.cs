using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class EnhanceCureEffectOnTargetHpLessBuff : BaseBuff
    {
        private bool effected = false;
        public EnhanceCureEffectOnTargetHpLessBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {         

        }

        protected override void Start()
        {
            AddListener(TriggerMessageType.CastCureBuff, OnCastCureBuff);
            if (owner.HpLessThanRate((int)c))
            {
                Effect();
            }
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }         
            elapsedTime = 0;
            if (owner.HpLessThanRate((int)c))
            {
                Effect();
            }
            else
            {
                UnEffect();
            }
        }

        protected override void End()
        {          
            RemoveListener(TriggerMessageType.CastCureBuff, OnCastCureBuff);
            UnEffect();
        }

        private void Effect()
        {
            if (!effected)
            {
                effected = true;
                caster.AddNatureAddedValue(NatureType.PRO_CURE_ENHANCE, n, Model.Notify);
            }
        }

        private void UnEffect()
        {
            if (effected)
            {
                effected = false;
                caster.AddNatureAddedValue(NatureType.PRO_CURE_ENHANCE, n * -1);
            }
        }      
    
        private void OnCastCureBuff(object param)
        {
            BaseBuff buff = param as BaseBuff;
            if (buff == null)
            {
                return;
            }
            if (!effected && owner.HpLessThanRate((int)c))
            {
                Effect();
            }
        }
    }
}

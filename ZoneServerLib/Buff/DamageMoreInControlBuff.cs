using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class DamageMoreInControlBuff : BaseBuff
    {
        private bool effected = false;
        public DamageMoreInControlBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
            AddListener(TriggerMessageType.ControlledBuffEnd, OnControlledBuffEnd);
        }

        protected override void Update(float dt)
        {
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
            RemoveListener(TriggerMessageType.ControlledBuffEnd, OnControlledBuffEnd);
            // 属性还原
            if (effected)
            {
                owner.AddNatureAddedValue(NatureType.PRO_DAM_IN_CTR, (int)(-1 * c));
            }
        }

        private void OnControlledBuffStart(object param)
        {
            if (effected) return;
            owner.AddNatureAddedValue(NatureType.PRO_DAM_IN_CTR, (int)c, Model.Notify);
            effected = true;
        }

        // 受控buff结束 如果没有其他受控buff起效，则应将伤害加成属性还原
        private void OnControlledBuffEnd(object param)
        {
            // 没有加过伤害加成属性
            if (!effected) return;
            // 还在被控中
            if (owner.BuffManager.InControlledBuff())
            {
                return;
            }
            // 不再被控 属性还原
            effected = false;
            owner.AddNatureAddedValue(NatureType.PRO_DAM_IN_CTR, (int)(-1 * c));
        }
    }
}

using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    public class DeControlledReduceDmgBuff : BaseBuff
    {
        private bool effected = false;
        public DeControlledReduceDmgBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            if(effected)
            {
                owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, (int)(-1 * c));
            }
        }

        private void OnControlledBuffStart(object param)
        {
            if (effected) return;
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, (int)c, Model.Notify);
            effected = true;
        }

        // 受控buff结束 如果没有其他受控buff起效，则应将减伤属性还原
        private void OnControlledBuffEnd(object param)
        {
            // 没有加过减伤属性
            if (!effected) return;
            // 还在被控中
            if(owner.BuffManager.InControlledBuff())
            {
                return;
            }
            // 不再被控 属性还原
            effected = false;
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG_RATIO, (int)(-1 * c));
        }
    }
}

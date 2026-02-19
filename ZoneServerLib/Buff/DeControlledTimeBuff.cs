using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    public class DeControlledTimeBuff : BaseBuff
    {
        public DeControlledTimeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
        }

        protected override void Update(float dt)
        {

        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
        }

        private void OnControlledBuffStart(object param)
        {
            BaseBuff buff = param as BaseBuff;
            if(buff != null)
            {
                buff.SetBuffDuringTime(buff.S * c * 0.0001f);
            }
        }
    }
}

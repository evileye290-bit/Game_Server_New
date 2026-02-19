using CommonUtility;
using ServerModels;
using System;

namespace ZoneServerLib
{
    public class RedeuceControlBuffTimeBuff : BaseBuff
    {
        public RedeuceControlBuffTimeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
        }

        private void OnControlledBuffStart(object param)
        {
            BaseBuff buff = param as BaseBuff;
            if (buff == null) return;

            buff.SetBuffDuringTime(Math.Max(0, buff.S - c));
        }

        protected override void Start()
        {
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
        }
    }
}

using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class RefuseControlBuffNatureBuff : BaseBuff
    {
        private Dictionary<BuffType, int> buffCountList;
        private Dictionary<BuffType, long> buffNatureList;
        public RefuseControlBuffNatureBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            buffCountList = new Dictionary<BuffType, int>();
            buffNatureList = new Dictionary<BuffType, long>();
            AddListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
        }

        protected override void Update(float dt)
        {

        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ControlledBuffStart, OnControlledBuffStart);
            // 还原相关属性
            foreach(var kv in buffNatureList)
            {
                UpdateDeCountrolledNature(kv.Key, kv.Value * -1);
            }   
        }

        private void OnControlledBuffStart(object param)
        {
            BaseBuff buff = param as BaseBuff;
            if (buff == null || !buff.ControlledBuff) return;
            // 增加buff次数
            if(buffCountList.ContainsKey(buff.BuffType))
            {
                buffCountList[buff.BuffType] += 1;
            }
            else
            {
                buffCountList.Add(buff.BuffType, 1);
            }

            long value = BuffParamCalculator.SpecCalc(Name, skillLevel, buffCountList[buff.BuffType]);
            long oldValue = 0;
            buffNatureList.TryGetValue(buff.BuffType, out oldValue);
            buffNatureList[buff.BuffType] = value;
            UpdateDeCountrolledNature(buff.BuffType, value - oldValue);
        }

        private void UpdateDeCountrolledNature(BuffType buffType, long value)
        {
            switch (buffType)
            {
                case BuffType.Dizzy:
                    owner.AddNatureAddedValue(NatureType.PRO_REFUSE_DIZZY, value, Model.Notify);
                    break;
                case BuffType.Silent:
                    owner.AddNatureAddedValue(NatureType.PRO_REFUSE_SILENT, value, Model.Notify);
                    break;
                case BuffType.Fixed:
                    owner.AddNatureAddedValue(NatureType.PRO_REFUSE_FIXED, value, Model.Notify);
                    break;
                case BuffType.Disarm:
                    owner.AddNatureAddedValue(NatureType.PRO_REFUSE_DISARM, value, Model.Notify);
                    break;
            }
        }
    }
}

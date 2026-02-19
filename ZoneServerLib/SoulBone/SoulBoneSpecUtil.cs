using System.Collections.Generic;
using CommonUtility;
using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class SoulBoneSpecUtil
    {
        internal static void DoEffect(List<SoulBoneSpecModel> specs, FieldObject obj)
        {
            foreach(var model in specs)
            {
                DoEffect(model, obj);
            }
        }

        private static void DoEffect(SoulBoneSpecModel model, FieldObject owner)
        {
            float growth = model.growth;
            int value = (int)(model.Param2.Key * growth + model.Param2.Value);

            switch (model.Type)
            {
                case SoulBoneSpecType.AddNatureRatio:
                    {
                        owner.AddNatureRatio((NatureType)model.Param1, value);
                        if ((NatureType)model.Param1 == NatureType.PRO_MAX_HP)
                        {
                            owner.SetNatureBaseValue(NatureType.PRO_HP, owner.GetNatureValue(NatureType.PRO_MAX_HP));
                        }
                    }
                    break;
                case SoulBoneSpecType.AddNatureValue:
                    {
                        owner.AddNatureBaseValue((NatureType)model.Param1, value);
                        if ((NatureType)model.Param1 == NatureType.PRO_MAX_HP)
                        {
                            owner.SetNatureBaseValue(NatureType.PRO_HP, owner.GetNatureValue(NatureType.PRO_MAX_HP));
                        }
                    }
                    break;
                // 某个技能能量积累速度加快（也就是上限减少)
                case SoulBoneSpecType.SkillEnergyLimit:
                    owner.SkillManager.ReduceEnergyLimit((SkillType)model.Param1, value);
                    break;
                // 将某个技能能量补满
                case SoulBoneSpecType.EnoughSkillEnergy:
                    owner.SkillManager.SetEnergyEnough((SkillType)model.Param1);
                    break;
                case SoulBoneSpecType.AddBuff:
                    {
                        owner.AddBuff(owner, model.Param1, (int)growth);
                    }
                    break;
                case SoulBoneSpecType.AddTrigger:
                    {
                        TriggerCreatedBySoulBone trigger = new TriggerCreatedBySoulBone(owner, model.Param1);
                        trigger.RecordFixedParam(TriggerParamKey.CreatedBySkillLevel, growth);
                        trigger.RecordFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, growth);
                        owner.AddTrigger(trigger);
                    }
                    break;
                case SoulBoneSpecType.AdddNatureAddeVal:
                    {
                        owner.AddNatureAddedValue((NatureType)model.Param1, value);
                        //if ((NatureType)model.Param1 == NatureType.PRO_MAX_HP)
                        //{
                        //    owner.SetNatureAddedValue(NatureType.PRO_HP, owner.GetNatureValue(NatureType.PRO_MAX_HP));
                        //}
                    }
                    break;
            }
        }
    }
}

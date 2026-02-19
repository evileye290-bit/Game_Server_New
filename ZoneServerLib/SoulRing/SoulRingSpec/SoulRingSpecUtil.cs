using CommonUtility;
using EnumerateUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.ComponentModel;

namespace ZoneServerLib
{
    // 魂环特殊效果起效逻辑
    public class SoulRingSpecUtil
    {

        #region 魂环

        public static void DoEffect(List<BattleSoulRingInfo> specIdList, FieldObject obj)
        {
            foreach (var specId in specIdList)
            {
                // 魂环特殊效果起效
                DoEffect(specId, obj);
            }
        }

        public static void DoEffect(BattleSoulRingInfo info, FieldObject owner)
        {
            if (owner == null) return; ;

            //1.魂环特殊效果
            {
                SoulRingSpecModel model = SoulRingLibrary.GetSoulRingSpecModel(info.SpecId);
                if (model != null)
                {
                    int growth = ScriptManager.SoulRing.GetNatureGrowthFactor(info.Year);
                    int value = (int)(model.Param2.Key * growth + model.Param2.Value);

                    DoSpecialEffect(owner, model.Type, growth, model.Param1, value);
                }
            }

            //1.魂环元素效果
            if (info.ElementId > 0)
            {
                SoulRingElementModel model = SoulRingLibrary.GetSoulRingElementModel(info.ElementId);
                if (model != null)
                {
                    int growth = ScriptManager.SoulRing.GetElementGrowthFactor(info.Year);
                    int value = (int)(model.Param2.Key * growth + model.Param2.Value);

                    DoSpecialEffect(owner, model.Type, growth, model.Param1, value);
                }
            }
        }

        #endregion

        #region 暗器

        public static void DoEffect(BattleHiddenWeaponInfo info, FieldObject obj)
        {
            HiddenWeaponModel model = HiddenWeaponLibrary.GetHiddenWeaponModel(info.Id);
            if(model == null) return;

            foreach (var item in model.SpecList)
            {
                HiddenWeaponSpecialModel specialModel = HiddenWeaponLibrary.GetHiddenWeaponSpecialModel(item);
                if (specialModel != null)
                {
                    DoEffect(specialModel, obj, info.Star);
                }
            }
        }

        private static void DoEffect(HiddenWeaponSpecialModel model, FieldObject owner, int star)
        {
            if (owner == null || model == null) return; ;

            int value = (int)(model.Param2.Key * star + model.Param2.Value);

            DoSpecialEffect(owner, model.Type, star, model.Param1, value);
        }

        #endregion



        public static void DoSpecialEffect(FieldObject owner, SoulRingSpecType type, int growth, int param1, int value)
        {
            switch (type)
            {
                case SoulRingSpecType.AddNatureRatio:
                {
                    owner.AddNatureRatio((NatureType) param1, value);
                    if ((NatureType) param1 == NatureType.PRO_MAX_HP)
                    {
                        owner.SetNatureBaseValue(NatureType.PRO_HP, owner.GetNatureValue(NatureType.PRO_MAX_HP));
                    }
                }
                    break;
                case SoulRingSpecType.AddNatureValue:
                {
                    owner.AddNatureBaseValue((NatureType) param1, value);
                    if ((NatureType) param1 == NatureType.PRO_MAX_HP)
                    {
                        owner.SetNatureBaseValue(NatureType.PRO_HP, owner.GetNatureValue(NatureType.PRO_MAX_HP));
                    }
                }
                    break;
                // 某个技能能量积累速度加快（也就是上限减少)
                case SoulRingSpecType.SkillEnergyLimit:
                    owner.SkillManager.ReduceEnergyLimit((SkillType) param1, value);
                    break;
                // 将某个技能能量补满
                case SoulRingSpecType.EnoughSkillEnergy:
                    owner.SkillManager.SetEnergyEnough((SkillType) param1);
                    break;
                case SoulRingSpecType.AddBuff:
                    owner.AddBuff(owner, param1, growth);
                    break;
                case SoulRingSpecType.AddTrigger:
                    TriggerCreatedBySoulRing trigger = new TriggerCreatedBySoulRing(owner, param1, growth);
                    owner.AddTrigger(trigger);
                    break;
                case  SoulRingSpecType.EnhanceSkillEffect:
                    var skill = owner.SkillManager.GetSkill(param1);
                    if (skill != null)
                    {
                        owner.EnhanceSkillEffect(value,skill.Level);
                    }

                    break;
            }
        }
    }
}

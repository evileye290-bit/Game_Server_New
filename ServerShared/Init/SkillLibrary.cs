using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class SkillLibrary
    {
        private static Dictionary<int, SkillModel> skillList = new Dictionary<int, SkillModel>();
        private static Dictionary<int, int> skillGrowth = new Dictionary<int, int>();
        private static Dictionary<int, float> battlePowerFactor = new Dictionary<int, float>();

        public static void Init()
        {
            SkillEffectLibrary.Init();
            //skillList.Clear();
            Dictionary<int, SkillModel> skillList = new Dictionary<int, SkillModel>();

            DataList dataList = DataListManager.inst.GetDataList("Skill");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!skillList.ContainsKey(item.Key))
                {
                    SkillModel skillModel = new SkillModel(data);
                    string[] skillEffectListStr = data.GetString("EffectList").Split('|');
                    for (int i = 0; i < skillEffectListStr.Length; i++)
                    {
                        SkillEffectModel skillEffectModel = SkillEffectLibrary.GetSkillEffectModel(int.Parse(skillEffectListStr[i]));
                        if (skillEffectModel != null)
                        {
                            skillModel.AddCalcModel(skillEffectModel);
                        }
                    }
                    skillList.Add(item.Key, skillModel);
                }
            }
            SkillLibrary.skillList = skillList;

            InitSkillGrowth();
        }

        private static void InitSkillGrowth()
        {
            //skillGrowth.Clear();
            //battlePowerFactor.Clear();

            Dictionary<int, int> skillGrowth = new Dictionary<int, int>();
            Dictionary<int, float> battlePowerFactor = new Dictionary<int, float>();

            DataList dataList = DataListManager.inst.GetDataList("SkillGrowth");
            foreach (var item in dataList)
            {
                skillGrowth[item.Key] = item.Value.GetInt("Growth");
                battlePowerFactor[item.Key] = item.Value.GetFloat("BattlePowerFactor");
            }
            SkillLibrary.skillGrowth = skillGrowth;
            SkillLibrary.battlePowerFactor = battlePowerFactor;
        }

        public static int GetSkillGrowth(int level)
        {
            int growth = level;
            skillGrowth.TryGetValue(level, out growth);
            return growth;
        }

        public static float GetSkillBattlePowerFactor(int level)
        {
            float factor = level;
            battlePowerFactor.TryGetValue(level, out factor);
            return factor;
        }

        public static SkillModel GetSkillModel(int skillId)
        {
            SkillModel skillModel = null;
            skillList.TryGetValue(skillId, out skillModel);
            return skillModel;
        }
    }
}

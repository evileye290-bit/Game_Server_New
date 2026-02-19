using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class BaseTrigger
    {
        public void RecordParam(string key, object value)
        {
            paramList[key] = value;
        }

        public bool TryGetParam(string key, out object param)
        {
            return paramList.TryGetValue(key, out param);
        }

        public void RecordFixedParam(string key, object value)
        {
            fixedParamList[key] = value;
        }
        public bool TryGetFixedParam(string key, out object param)
        {
            return fixedParamList.TryGetValue(key, out param);
        }

        public void RemoveParam(string key)
        {
            paramList.Remove(key);
        }

        public void ResetParams()
        {
            paramList.Clear();
            triggerCounter.Clear();
        }

        // 关于获取param的方法
        public int GetParam_PlayerDeadCount()
        {
            int count = 0;
            object param;
            if (!TryGetParam(TriggerParamKey.PlayerDeadCount, out param))
            {
                return count;
            }
            int.TryParse(param.ToString(), out count);
            return count;
        }

        public int GetParam_HeroDeadCount()
        {
            int count = 0;
            object param;
            if (!TryGetParam(TriggerParamKey.HeroDeadCount, out param))
            {
                return count;
            }
            int.TryParse(param.ToString(), out count);
            return count;
        }

        /// <summary>
        /// 用于增加能量，创建trigger传入技能等级，增加技能效果
        /// </summary>
        /// <returns></returns>
        public int GetFixedParam_SkillLevel()
        {
            int skillLevel = 1;
            object param;

            if (TryGetFixedParam(TriggerParamKey.CreatedBySkillLevel, out param))
            {
                int.TryParse(param.ToString(), out skillLevel);
            }
            return skillLevel;
        }

        /// <summary>
        /// buff、伤害、治疗计算需要获取到技能成长值
        /// </summary>
        /// <returns></returns>
        public int GetFixedParam_SkillLevelGrowth()
        {
            int skillLevelGrowth = 1;
            object param;

            if (TryGetFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, out param))
            {
                int.TryParse(param.ToString(), out skillLevelGrowth);
            }
            return skillLevelGrowth;
        }

        public int GetFixedParam_HeroId()
        {
            int heroId = 0;
            object param;

            if (TryGetFixedParam(TriggerParamKey.HeroId, out param))
            {
                int.TryParse(param.ToString(), out heroId);
            }
            return heroId;
        }

        public int GetParam_SkillCastCount(int skillId)
        {
            int count = 0;
            object param;
            if (!TryGetParam(TriggerParamKey.BuildSkillCastCount(skillId), out param))
            {
                return count;
            }
            int.TryParse(param.ToString(), out count);
            return count;
        }

        public int GetParam_SkillTypeCastCount(int skillType)
        {
            int count = 0;
            object param;
            if (!TryGetParam(TriggerParamKey.BuildSkillTypeCastCount(skillType), out param))
            {
                return count;
            }
            int.TryParse(param.ToString(), out count);
            return count;
        }

        public int GetParam_BeenAddedTypeBuffCount(int skillType)
        {
            int count = 0;
            object param;
            if (!TryGetParam(TriggerParamKey.BuildBeenAddedTypeBuffCount(skillType), out param))
            {
                return count;
            }
            int.TryParse(param.ToString(), out count);
            return count;
        }


        #region trigger counter

        public void AddCounter(TriggerCounter type)
        {
            int count = 0;
            if (triggerCounter.TryGetValue(type, out count))
            {
                triggerCounter[type] = count + 1;
            }
            else
            { 
                triggerCounter[type] = 1;
            }
        }

        public int GetCounter(TriggerCounter type)
        {
            int count = 0;
            triggerCounter.TryGetValue(type, out count);
            return count;
        }

        public void SetCounter(TriggerCounter type, int counts)
        {
            if (triggerCounter.ContainsKey(type))
            {
                triggerCounter[type] = counts;
            }
        }

        #endregion
    }
}

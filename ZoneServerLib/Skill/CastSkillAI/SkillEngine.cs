using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SkillEngine
    {
        private FieldObject owner;
        private List<SkillReadyInfo> readyList;
        public SkillEngine(FieldObject owner)
        {
            this.owner = owner;
            readyList = new List<SkillReadyInfo>();
        }

        public bool InReadyList(int skillId)
        {
            foreach (var item in readyList)
            {
                if(item.SkillId == skillId)
                {
                    return true;
                }
            }
            return false;
        }

        private void Sort()
        {
            // 优先级从低到高排序，最后一个元素即最先释放的技能
            readyList.Sort((left, right) =>
            {
                if (left.Priority < right.Priority)
                {
                    return -1;
                }
                if (left.Priority > right.Priority)
                {
                    return 1;
                }
                // 优先级相等 先准备好的技能放在后面，优先释放
                if (left.ReadyTime <= right.ReadyTime)
                {
                    return 1;
                }
                return -1;
            });
        }

        public void ClearReadyList()
        {
            readyList.Clear();
        }


        public void AddSkill(int skillId, BaseTrigger trigger)
        {
            if (InReadyList(skillId))
            {
                return;
            }
            Skill skill = owner.SkillManager.GetSkill(skillId);
            if (skill == null)
            {
                return;
            }
            if(skill.IsBodyAttack() && !owner.InRealBody)
            {
                return;
            }
            if (trigger != null)
            {
                trigger.Stop();
            }

            //取副本内的时间，主要用于战斗加速
            DateTime time = owner.CurDungeon == null ? ZoneServerApi.now : owner.CurDungeon.CurrTime;

            SkillReadyInfo newSkill = new SkillReadyInfo(skill, trigger, time);
            // 技能放入列表中，trigger停止检测释放条件
            readyList.Add(newSkill);
            Sort();
        }

        public bool TryFetchOneSkill(out Skill skill, out FieldObject target, out Vec2 targetPos)
        {
            skill = null;
            target = null;
            targetPos = null;
            for (int i = readyList.Count-1; i >= 0; i--)
            {
                SkillReadyInfo info = readyList[i];
                if (!owner.SkillManager.Check(info.Skill))
                {
                    continue;
                }
                // 能够找到释放坐标或目标
                if (!owner.TryGetCastSkillParam(info.Skill.SkillModel, out target, out targetPos))
                {
                    continue;
                }
                skill = info.Skill;
                return true;
            }
            return false;
        }

        public void SkillStarted(int skillId)
        {
            // 释放技能，该技能的trigger需要重新检测
            SkillReadyInfo skill = null;
            int index = 0;
            for(; index < readyList.Count; index++)
            {
                if (readyList[index].SkillId == skillId)
                {
                    skill = readyList[index];
                    break;
                }
            }
            if(skill != null)
            {
                if(skill.Trigger != null)
                {
                    skill.Trigger.Start();
                }
                skill.Skill.ResetEnergy();
                readyList.RemoveAt(index);
            }
        }


        // 武魂真身结束后，删除待释放的真身攻击
        public void RemoveBodyAttacks()
        {
            List<SkillReadyInfo> removeList = new List<SkillReadyInfo>();
            foreach(var ready in readyList)
            {
                if(ready.Skill.IsBodyAttack())
                {
                    removeList.Add(ready);
                }
            }
            foreach(var ready in removeList)
            {
                if(ready.Trigger != null)
                {
                    ready.Trigger.Start();
                }
                readyList.Remove(ready);
            }
        }

        public void RemoveSkill(int skillId)
        {
            int index = 0;
            SkillReadyInfo skill = null;
            for (; index < readyList.Count; index++)
            {
                if (readyList[index].SkillId == skillId)
                {
                    skill = readyList[index];
                    break;
                }
            }
            if (skill != null)
            {
                if (skill.Trigger != null)
                {
                    skill.Trigger.Start();
                }
                readyList.RemoveAt(index);
                skill = null;
            }
        }

    }
}

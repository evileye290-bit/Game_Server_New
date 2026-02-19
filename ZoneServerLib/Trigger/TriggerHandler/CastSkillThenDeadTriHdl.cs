using System;
using CommonUtility;
using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    public class CastSkillThenDeadTriHdl : BaseTriHdl //亡语技能
    {
        readonly int skillId = 0;
        public CastSkillThenDeadTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            skillId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            Skill skill = Owner.SkillManager.GetSkill(skillId);
            if (skill == null)
            {
                //找不到死亡触发的技能 也要直接死，亡语技能说明是死了后才放的
                Owner.CastSkillThenDeadSkillId = -1;
                long hp = Owner.GetNatureValue(NatureType.PRO_HP);
                Owner.AddNatureBaseValue(NatureType.PRO_HP, -hp);
                Owner.CheckDead();
                Owner.BroadCastHp();
                Owner.FsmManager.SetNextFsmStateType(FsmStateType.DEAD);
                return;
            }
            else
            {
                Owner.CastSkillThenDeadSkillId = skillId;
                Owner.SkillEngine.ClearReadyList();
                Owner.SkillEngine.AddSkill(skillId, trigger); //这里是放技能

                // 然后死的操作，在Hero_FSM.cs 里 CastSkillThenDeadSkillId>0时候
            }

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}

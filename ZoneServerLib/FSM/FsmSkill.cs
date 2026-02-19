using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;

namespace ZoneServerLib
{
    public class FsmSkillState : FsmBaseState
    {
        private float skillDelayTime = 0;

        private float skillLeftTime = 0;
        public float LeftTime
        {
            get { return skillLeftTime; }
            set { skillLeftTime = value; }
        }

        private float skillEffectTime = 0;

        private int skillEffectCount = 0;

        private bool started = false;

        private bool isFirstEffect = true;// 是否首次生效

        public int SkillId;

        private Skill skill;
        public Skill Skill { get { return skill; } }

        public FsmSkillState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.SKILL;
        }

        protected override void Start(FsmStateType prevState)
        {
            skill = startParam as Skill;
            if (skill == null)
            {
                isEnd = true;
                return;
            }

            SkillId = skill.Id;

            if (owner.AutoAI)
            {
                owner.SkillEngine.SkillStarted(skill.Id);
            }
            skillDelayTime = skill.SkillModel.DelayTime;
            skillLeftTime = skill.SkillModel.DuringTime;
            skillEffectTime = skill.SkillModel.EffectTime;
            skillEffectCount = skill.SkillModel.EffectCount;

            //设置为首次生效
            isFirstEffect = true;

            started = false;
            CheckStartSkill();
            if (skill.SkillModel.Alarm)
            {
                // 技能预警前端同步
                BroadcastSkillAlarm();
            }
            //设置多重时间
            ChangeEffectTime();
            owner.SkillManager.StartCasting(skill); 
        }

        private void ChangeEffectTime()
        {
            if (skill.SkillModel.EffectCount > 1 && skill.SkillModel.MultipleTime.Count > 0)
            {
                int index = EffectedCount();
                if (skill.SkillModel.MultipleTime.Count > index)
                {
                    skillEffectTime = skill.SkillModel.MultipleTime[index];
                }
            }
        }

        private int EffectedCount()
        {
            return skill.SkillModel.EffectCount - skillEffectCount;
        }

        protected override void Update(float deltaTime)
        {
            skillDelayTime -= deltaTime;
            skillLeftTime -= deltaTime;
            skillEffectTime -= deltaTime;
            CheckStartSkill();
            
            if (skillEffectCount > 0 && skillEffectTime <= 0)
            {
                SkillEffect();
            }

            if (skillLeftTime <= 0)
            {
                isEnd = true;
				return;
            }
        }

        private void SkillEffect()
        {
            if (isFirstEffect)
            {
                // 应当先dispatch消息，接收到消息的逻辑会影响到技能结算最终的结果
                //第一次生效，触发trigger
                owner.DispatchSkillEffMsg(skill.Id);
                owner.DisPatchSkillCastCountMsg(skill);
            }

            bool afterCast = skillEffectCount <= 1;

            owner.SkillEffect(skill, EffectedCount(), afterCast, isFirstEffect);

            skillEffectCount--;
            skillEffectTime = skill.SkillModel.EffectTime;

            //设置为非首次生效
            isFirstEffect = false;

            //设置多重时间
            ChangeEffectTime();
        }

        protected override void End(FsmStateType nextState)
        {
            //if (isEnd) return;

            if (skill != null)
            {
                if (owner.SubcribedMessage(TriggerMessageType.SkillEnd))
                {
                    owner.DispatchMessage(TriggerMessageType.SkillEnd, skill.Id);
                }
                if (owner.SubcribedMessage(TriggerMessageType.SkillTypeEnd))
                {
                    owner.DispatchMessage(TriggerMessageType.SkillTypeEnd, (int)skill.SkillModel.Type);
                }
            }

            base.End(nextState);

            skill = null;
            skillEffectCount = 0;
            skillLeftTime = 0;
            isFirstEffect = true;
        }

        private void BroadcastSkillAlarm()
        {
            MSG_ZGC_SKILL_ALARM response = new MSG_ZGC_SKILL_ALARM();
            response.CasterId = owner.InstanceId;
            response.SkillId = skill.Id;
            response.TargetId = skill.CastTargetId;
            response.SkillPosX = skill.CastDestPos.X;
            response.SkillPosY = skill.CastDestPos.Y;
            response.AngleX = skill.CastLookDir.X;
            response.AngleY = skill.CastLookDir.Y;
            owner.BroadCast(response);
        }

        private void BroadcastSkillStart()
        {
            MSG_ZGC_SKILL_START response = new MSG_ZGC_SKILL_START();
            response.CasterId = owner.InstanceId;
            response.SkillId = skill.Id;
            response.TargetId = skill.CastTargetId;
            response.SkillPosX = skill.CastDestPos.X;
            response.SkillPosY = skill.CastDestPos.Y;
            response.AngleX = skill.CastLookDir.X;
            response.AngleY = skill.CastLookDir.Y;
            response.Success = (int)ErrorCode.Success;
            owner.BroadCast(response);


            owner.DispatchSkillStartMsg(skill.SkillModel, skill.CastTargetId);
            
            Log.Debug("instance {0} skill {1} start in fsm skill state",owner.InstanceId, skill.Id);
        }

        private void CheckStartSkill()
        {
            if(started)
            {
                return;
            }
            if(skillDelayTime <= 0f)
            {
                started = true;
                if(skill.IsBodySkill())
                {
                    owner.EnableRealBody();
                }
                BroadcastSkillStart();
            }
        }
    }
}

using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmMonsterAttackState : FsmBaseState
    {
        FieldObject target;
        Vec2 targetPos;
        private bool chasing = false;


        private Skill skill;
        public Skill Skill { get { return skill; } }

        public Tuple<Vec2, FieldObject, Skill> nextParam;

        public FsmMonsterAttackState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_ATTACK;
        }
        protected override void Start(FsmStateType prevState)
        {
            chasing = false;
            if (!monster.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 没找到合适技能，重新巡逻
                GoToNextState(FsmStateType.MONSTER_SEARCH);
                return;
            }
            if (InSkillRange())
            {
                // 在释放范围内，准备进入skill状态机
                skill.InitCastParam(targetPos - monster.Position, targetPos, target == null ? 0 : target.InstanceId);
                ToSurroundOrSkill();
                //GoToNextState(FsmStateType.SKILL);
                return;
            }
            else
            {
                StartChasing();
            }
        }

        protected override void Update(float deltaTime)
        {
            if (chasing)
            {
                monster.OnMove(deltaTime);
            }
            elapsedTime += deltaTime;
            if (elapsedTime >= 0.1f)
            {
                elapsedTime = 0;
                if (InSkillRange())
                {
                    skill.InitCastParam(targetPos - monster.Position, targetPos, target == null ? 0 : target.InstanceId);
                    ToSurroundOrSkill();
                    //GoToNextState(FsmStateType.SKILL);
                    return;
                }
                if (CheckReturn())
                {
                    GoToNextState(FsmStateType.MONSTER_RETURN);
                    return;
                }

                // 定期尝试从skill engine重新找此刻优先级更高的技能释放
                if (!monster.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
                {
                    GoToNextState(FsmStateType.MONSTER_SEARCH);
                    return;
                }
                StartChasing();
            }
        }


        private bool CheckReturn()
        {
            return !Vec2.InRange(monster.GenCenter, monster.Position, monster.SearchRange);
        }

        private void ToSurroundOrSkill()
        {
            if (target != null)
            {
                Vec2 toPos = monster.Position;
                nextParam = Tuple.Create(toPos, target, skill);
                GoToNextState(FsmStateType.MONSTER_SURROUND);
            }
            else
            {
                GoToNextState(FsmStateType.SKILL);
            }
        }


        private void StartChasing()
        {
            chasing = true;
            Vec2 dest;

            float targetRadius = target == null ? 0f : target.Radius;
            float casterRadius = owner.Radius;
            float length = skill.SkillModel.CastRange + casterRadius + targetRadius;
            Vec2 temp = targetPos - owner.Position;
            float distance = (float)temp.GetLength();
            float needLength = length - distance;
            if (needLength > 0)
            {
                dest = owner.Position + temp * (needLength + 0.1f) / distance;
            }
            else
            {
                dest = new Vec2(targetPos);
            }
            //Vec2 dest = new Vec2(targetPos);
            monster.SetDestination(dest);
            monster.OnMoveStart();
            monster.BroadCastMove();
        }

        private void StopChasing()
        {
            chasing = false;
            monster.OnMoveStop();
            monster.BroadCastStop();
        }

        private bool InSkillRange()
        {
            float targetRadius = target == null ? 0f : target.Radius;
            float casterRadius = owner.Radius;
            return Vec2.GetRangePower(monster.Position, targetPos) <=
                (skill.SkillModel.CastRange + casterRadius + targetRadius) * (skill.SkillModel.CastRange + casterRadius + targetRadius);
        }

        protected override void GoToNextState(FsmStateType state)
        {
            base.GoToNextState(state);
            if (chasing)
            {
                StopChasing();
            }
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
            skill = null;
            target = null;
            nextParam = null;
        }
    }
}

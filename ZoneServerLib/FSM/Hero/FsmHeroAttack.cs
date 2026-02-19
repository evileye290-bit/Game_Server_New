using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    class FsmHeroAttackState : FsmBaseState
    {
        public FsmHeroAttackState(FieldObject owner) : base(owner)
        {
            fsmStateType = FsmStateType.HERO_ATTACK;
        }

        FieldObject target;
        Vec2 targetPos;
        private bool chasing = false;
        private Skill skill;
        public Skill Skill { get { return skill; } }
        public Tuple<Vec2, FieldObject, Skill> nextParam;      

        protected override void Start(FsmStateType prevState)
        {
            //oldTarget = target as Monster;
            chasing = false;
            if (!hero.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 没找到合适技能，重新巡逻
                GoToNextState(FsmStateType.HERO_IDLE);
                return;
            }
            if (InSkillRange())
            {
                skill.InitCastParam(targetPos - hero.Position, targetPos, target == null ? 0 : target.InstanceId);
                //GoToNextState(FsmStateType.SKILL);
                ToSurroundOrSkill();
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
                hero.OnMove(deltaTime);
            }
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.1f)
            {

                elapsedTime = 0;
                if (InSkillRange())
                {
                    //todo 判断是否能放技能且有位置，进入技能前寻路
                    //skill.InitCastParam(targetPos - hero.Position, targetPos, target == null ? 0 : target.InstanceId);
                    //GoToNextState(FsmStateType.SKILL);
                    skill.InitCastParam(targetPos - hero.Position, targetPos, target == null ? 0 : target.InstanceId);
                    ToSurroundOrSkill();
                    return;
                }
                

                // 定期尝试从skill engine重新找此刻优先级更高的技能释放
                if (!hero.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
                {
                    GoToNextState(FsmStateType.HERO_IDLE);
                    return;
                }
                StartChasing();
            }
        }

        private void ToSurroundOrSkill()
        {
            //Hero hero = owner as Hero;
            //if (oldTarget != null&&oldTarget!=target)
            //{
            //    hero.RemoveFromOldTarget(oldTarget);
            //}
            if (target != null)
            {
                
                //Monster monster = target as Monster;
                if (hero != null && target != null)
                {
                    Vec2 toPos = hero.Position;
                    nextParam = Tuple.Create(toPos, target, skill);
                    GoToNextState(FsmStateType.HERO_SURROUND);
                }
                else
                {
                    GoToNextState(FsmStateType.SKILL);
                }
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
            Vec2 temp = targetPos - hero.Position;
            float distance = (float)temp.GetLength();
            float needLength = length - distance;
            if (needLength > 0)
            {
                dest = hero.Position + temp * (needLength+0.1f) / distance;
            }
            else
            {
                dest = new Vec2(targetPos);
            }
            hero.SetDestination(dest);
            hero.OnMoveStart();
            hero.BroadCastMove();
        }

        private void StopChasing()
        {
            chasing = false;
            hero.OnMoveStop();
            hero.BroadCastStop();
        }

        private bool InSkillRange()
        {
            float targetRadius = target == null ? 0f : target.Radius;
            float casterRadius = owner.Radius;
            return Vec2.GetRangePower(hero.Position, targetPos) <= 
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

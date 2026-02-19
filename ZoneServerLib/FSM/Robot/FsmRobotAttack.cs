using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmRobotAttackState : FsmBaseState
    {
        public FsmRobotAttackState(FieldObject owner) : base(owner)
        {
            fsmStateType = FsmStateType.ROBOT_ATTACK;
        }

        FieldObject target;
        Vec2 targetPos;
        private bool chasing = false;
        //Monster oldTarget = null;


        private Skill skill;
        public Skill Skill { get { return skill; } }

        public Tuple<Vec2, FieldObject, Skill> nextParam;

        protected override void Start(FsmStateType prevState)
        {
            //oldTarget = target as Monster;
            chasing = false;
            if (!robot.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 没找到合适技能，重新巡逻
                GoToNextState(FsmStateType.ROBOT_IDLE);
                return;
            }
            if (InSkillRange())
            {
                skill.InitCastParam(targetPos - robot.Position, targetPos, target == null ? 0 : target.InstanceId);
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
                robot.OnMove(deltaTime);
            }
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.1f)
            {

                elapsedTime = 0;
                if (InSkillRange())
                {
                    //判断是否能放技能且有位置，进入技能前寻路
                    skill.InitCastParam(targetPos - robot.Position, targetPos, target == null ? 0 : target.InstanceId);
                    ToSurroundOrSkill();
                    return;
                }


                // 定期尝试从skill engine重新找此刻优先级更高的技能释放
                if (!robot.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
                {
                    GoToNextState(FsmStateType.ROBOT_IDLE);
                    return;
                }
                StartChasing();
            }
        }

        private void ToSurroundOrSkill()
        {
            if (target != null && robot != null)
            {
                Vec2 toPos = robot.Position;
                nextParam = Tuple.Create(toPos, target, skill);
                GoToNextState(FsmStateType.ROBOT_SURROUND);
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
            Vec2 temp = targetPos - robot.Position;
            float distance = (float)temp.GetLength();
            float needLength = length - distance;
            if (needLength > 0)
            {
                dest = robot.Position + temp * (needLength + 0.1f) / distance;
            }
            else
            {
                dest = new Vec2(targetPos);
            }
            robot.SetDestination(dest);
            robot.OnMoveStart();
            robot.BroadCastMove();
        }

        private void StopChasing()
        {
            chasing = false;
            robot.OnMoveStop();
            robot.BroadCastStop();
        }

        private bool InSkillRange()
        {
            float targetRadius = target == null ? 0f : target.Radius;
            float casterRadius = robot.Radius;
            return Vec2.GetRangePower(robot.Position, targetPos) <=
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

        #region

        //private Vec2 dest;

        //public FsmRobotAttackState(FieldObject owner)
        //    : base(owner)
        //{
        //    fsmStateType = FsmStateType.ROBOT_ATTACK;
        //}

        //protected override void Start(FsmStateType prevState)
        //{
        //    StartWalking();
        //}

        //protected override void Update(float deltaTime)
        //{
        //    bool arrived = hero.OnMove(deltaTime);
        //    elapsedTime += deltaTime;

        //    if (elapsedTime >= 0.5)
        //    {
        //        elapsedTime = 0;
        //    }

        //    if (arrived)
        //    {

        //    }

        //}

        //private Vec2 NextWalkPosition()
        //{
        //    Vec2 position;
        //    for (int i = 0; i < 3; i++)
        //    {
        //        position = Vec2.GetRandomPos(hero.Owner.Position, hero.WalkRange);
        //        if (hero.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
        //        {
        //            return position;
        //        }
        //    }
        //    position = new Vec2(hero.Owner.Position);
        //    return position;
        //}

        //private void StartWalking()
        //{
        //    dest = NextWalkPosition();
        //    hero.SetDestination(dest);
        //    hero.OnMoveStart();
        //    hero.BroadCastMove();
        //}

        //private void StopWalking()
        //{
        //    if (!hero.IsMoving || hero.InBattleField())
        //    {
        //        hero.OnMoveStop();
        //        hero.BroadCastStop();
        //    }
        //}

        //protected override void GoToNextState(FsmStateType state)
        //{
        //    StopWalking();
        //    base.GoToNextState(state);

        //}
        #endregion
    }
}

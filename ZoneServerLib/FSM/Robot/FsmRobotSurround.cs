using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmRobotSurroundState : FsmBaseState
    {
        private Vec2 dest;//当前目标点，到达后判断是碰撞还是计算技能
        private FieldObject target;
        private Skill skill;
        Tuple<Vec2, FieldObject, Skill> param;

        public Skill Skill
        { get { return skill; } }

        public FsmRobotSurroundState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.ROBOT_SURROUND;
        }

        protected override void Start(FsmStateType prevState)
        {
            param = startParam as Tuple<Vec2, FieldObject, Skill>;
            if (param == null)
            {
                GoToNextState(FsmStateType.ROBOT_IDLE);
                return;
            }
            else
            {
                dest = param.Item1;
                target = param.Item2;
                skill = param.Item3;
                Robot own = owner as Robot;
                if (own.CheckCollision(target, dest))
                {
                    Tuple<bool, Vec2> ans = own.GetNonCollisionPos(target, dest, skill.SkillModel.SkillPosRange);
                    //dest = ans.Item2;
                    if (ans.Item2 != dest)
                    {
                        dest = ans.Item2;
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - robot.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                        return;
                    }
                }
                else
                {
                    skill.InitCastParam(target.Position - robot.Position, target.Position, target == null ? 0 : target.InstanceId);
                    GoToNextState(FsmStateType.SKILL);
                }
            }
            StartWalking();
        }

        protected override void Update(float deltaTime)
        {
            bool arrived = robot.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;

            }
            if (!target.IsDead)
            {
                if (arrived)
                {
                    Robot own = owner as Robot;

                    if (own.CheckCollision(target, owner.Position))
                    {
                        Tuple<bool, Vec2> ans = own.GetNonCollisionPos(target, dest, skill.SkillModel.SkillPosRange);
                        if (ans.Item2 != dest)
                        {
                            dest = ans.Item2;
                            StartWalking();
                        }
                        else
                        {
                            skill.InitCastParam(target.Position - robot.Position, target.Position, target == null ? 0 : target.InstanceId);
                            GoToNextState(FsmStateType.SKILL);
                        }
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - robot.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                    }
                }
            }
            else
            {
                GoToNextState(FsmStateType.ROBOT_IDLE);
            }
        }

        private void StartWalking()
        {
            robot.SetDestination(dest);
            robot.OnMoveStart();
            robot.BroadCastMove();
        }

        private void StopWalking()
        {
            if (!robot.IsMoving)
            {
                robot.OnMoveStop();
                robot.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);

        }
    }
}

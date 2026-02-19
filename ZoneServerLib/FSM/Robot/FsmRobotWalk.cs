using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmRobotWalkState : FsmBaseState
    {
        float m_syncElapsedTime = 0.0f;
        float m_tick = 0.2f;

        private Vec2 dest;

        public FsmRobotWalkState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.ROBOT_WALK;
        }

        protected override void Start(FsmStateType prevState)
        {
            m_syncElapsedTime = 0.0f;

            if (robot.InBattleField() && robot.FindTarget())
            {
                GoToNextState(FsmStateType.ROBOT_ATTACK);
                return;
            }
            StartWalking();//值得优化的地方，看着像个人
        }

        protected override void Update(float deltaTime)
        {
            bool arrived = robot.OnMove(deltaTime);

            m_syncElapsedTime += deltaTime;

            if (m_syncElapsedTime >= m_tick)
            {
                m_syncElapsedTime = 0.0f;
                owner.BroadCastMove();

                if (robot.FindTarget())
                {
                    GoToNextState(FsmStateType.ROBOT_ATTACK);
                    return;
                }
            }

            if (arrived)
            {
                StartWalking();
            }
        }

        private Vec2 NextWalkPosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(robot.Position, robot.HeroModel.WalkRange);
                if (robot.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = new Vec2(robot.Position);
            return position;
        }

        private void StartWalking()
        {
            dest = NextWalkPosition();
            robot.SetDestination(dest);
            robot.OnMoveStart();
            robot.BroadCastMove();
        }

        private void StopWalking()
        {
            robot.OnMoveStop();
            robot.BroadCastStop();
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);
        }
    }
}

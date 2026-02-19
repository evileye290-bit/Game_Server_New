using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ZoneServerLib
{
    class FsmRobotIdleState : FsmBaseState
    {
        float m_syncElapsedTime = 0.0f;
        float m_tick = 0.2f;

        public FsmRobotIdleState(FieldObject owner) : base(owner)
        {
            fsmStateType = FsmStateType.ROBOT_IDLE;
        }

        protected override void Start(FsmStateType prevState)
        {
            m_syncElapsedTime = 0.0f;
            //if (robot.InBattleField() && robot.FindTarget())
            //{
            //    GoToNextState(FsmStateType.ROBOT_ATTACK);
            //    return;
            //}
        }

        protected override void Update(float deltaTime)
        {
            //m_syncElapsedTime += deltaTime;

            //if (m_syncElapsedTime >= m_tick)
            //{
            //    if (robot.FindTarget())
            //    {
            //        GoToNextState(FsmStateType.ROBOT_ATTACK);
            //        return;
            //    }
            //}

        }
    }
}

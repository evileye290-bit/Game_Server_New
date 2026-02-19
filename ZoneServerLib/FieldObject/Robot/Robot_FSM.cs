using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Robot
    {
        protected override void InitFSM()
        {
            fsmManager = new FsmManager(this);

            fsmManager.AddFsmState(FsmStateType.IDLE);
            fsmManager.AddFsmState(FsmStateType.CHASE);
            fsmManager.AddFsmState(FsmStateType.DEAD);
            fsmManager.AddFsmState(FsmStateType.SKILL);

            //robot
            fsmManager.AddFsmState(FsmStateType.ROBOT_WALK);
            fsmManager.AddFsmState(FsmStateType.ROBOT_ATTACK);
            fsmManager.AddFsmState(FsmStateType.ROBOT_SURROUND);
            fsmManager.AddFsmState(FsmStateType.ROBOT_IDLE);

            fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);
        }

        public override void FsmAIUpdate(float delta)
        {
            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.IDLE:
                case FsmStateType.DEAD:
                case FsmStateType.BASE:
                case FsmStateType.CHASE:
                case FsmStateType.SKILL:
                case FsmStateType.ROBOT_SURROUND:
                case FsmStateType.ROBOT_ATTACK:
                case FsmStateType.ROBOT_IDLE:
                case FsmStateType.ROBOT_WALK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);
                    }
                    break;
                default:
                    Log.Warn("robot fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

        //public override void FsmAIUpdate(float delta)
        //{
        //    switch (fsmManager.CurFsmState.FsmStateType)
        //    {
        //        case FsmStateType.IDLE:
        //        case FsmStateType.DEAD:
        //            if (fsmManager.CurFsmState.IsEnd)
        //            {
        //                fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);
        //            }
        //            break;
        //        case FsmStateType.BASE:
        //        case FsmStateType.CHASE:
        //            fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);
        //            break;
        //        case FsmStateType.SKILL:
        //            if (fsmManager.CurFsmState.IsEnd)
        //            {
        //                fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);//fsmManager.CurFsmState.Next);
        //            }
        //            break;
        //        case FsmStateType.ROBOT_SURROUND:
        //            if (fsmManager.CurFsmState.IsEnd)
        //            {
        //                FsmRobotSurroundState attackState = (FsmRobotSurroundState)fsmManager.CurFsmState;
        //                fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
        //            }
        //            break;
        //        case FsmStateType.ROBOT_ATTACK:
        //            if (fsmManager.CurFsmState.IsEnd)
        //            {
        //                FsmRobotAttackState attackState = (FsmRobotAttackState)fsmManager.CurFsmState;
        //                if (attackState.Next == FsmStateType.ROBOT_IDLE)
        //                {
        //                    fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_IDLE);
        //                }
        //                else if(attackState.Next == FsmStateType.ROBOT_WALK)
        //                {
        //                    fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_WALK);
        //                }
        //                else if (attackState.Next == FsmStateType.ROBOT_SURROUND)
        //                {
        //                    object param = attackState.nextParam;
        //                    fsmManager.SetNextFsmStateType(FsmStateType.ROBOT_SURROUND, false, param);
        //                }
        //                else
        //                {
        //                    // 释放技能后，进入技能cd
        //                    fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
        //                }
        //            }
        //            break;
        //        case FsmStateType.ROBOT_IDLE:
        //        case FsmStateType.ROBOT_WALK:
        //            if (fsmManager.CurFsmState.IsEnd)
        //            {
        //                fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
        //            }
        //            break;
        //        default:
        //            Log.Warn("robot fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
        //            break;
        //    }
        //}
    }
}

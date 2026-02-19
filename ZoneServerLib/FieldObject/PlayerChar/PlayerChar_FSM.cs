using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public void InitFSMAfterHero()
        {
            InitFSM();
        }

        //状态机
        protected override void InitFSM()
        {
            fsmManager = new FsmManager(this);

            fsmManager.AddFsmState(FsmStateType.IDLE);
            fsmManager.AddFsmState(FsmStateType.RUN);
            fsmManager.AddFsmState(FsmStateType.CHASE);
            fsmManager.AddFsmState(FsmStateType.DEAD);
            fsmManager.AddFsmState(FsmStateType.SKILL);

            fsmManager.SetNextFsmStateType(FsmStateType.IDLE);
        }

        public override void FsmAIUpdate(float delta)
        {
            switch (fsmManager.CurFsmState.FsmStateType)
            { 
                case FsmStateType.BASE:
                case FsmStateType.IDLE:
                case FsmStateType.DEAD:
                    break;
                case FsmStateType.RUN:
                case FsmStateType.CHASE:
                case FsmStateType.SKILL:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                     //   Log.Debug("{0} state end, ai update next state {1}", FsmManager.CurFsmState.FsmStateType, FsmStateType.IDLE);
                        fsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                    }
                    break;
                default:
                    Log.Warn("player fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }
    }
}

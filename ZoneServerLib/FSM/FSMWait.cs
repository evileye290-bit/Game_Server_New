using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmWaitState : FsmBaseState
    {

        public FsmWaitState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = EnumerateUtility.FsmStateType.WAIT;
        }
//        protected override void Start(CharacterState prevState)
//        {
//#if DEBUG || Supervise
//            character.SuperLog(prevState, "状态转变   " + prevState + "===>State_Wait-- State_Wait开始");
//#endif
//            PlayerChar pc = (PlayerChar)character;
//            if (pc.IsPlayingSkill == true)
//            {
//                pc.ChangeState(CharacterState.SKILL);
//                pc.BroadCastStop();
//            }
//            else
//            {
//                pc.ChangeState(CharacterState.IDLE);
//                pc.BroadCastStop();
//            }
            
//        }

        protected override void Start(FsmStateType prevState)
        {
            Logger.Log.Write("Start");
        }

        protected override void Update(float deltaTime)
        {
            //Console.WriteLine("Update");
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
            Logger.Log.Write("End");
        }
    }
}

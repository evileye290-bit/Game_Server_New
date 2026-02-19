using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmPetIdleState : FsmBaseState
    {
        private float idleTime;

        public FsmPetIdleState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.PET_IDLE;
            idleTime = 4.5f;
        }

        protected override void Start(FsmStateType prevState)
        {
            if (startParam == null || !float.TryParse(startParam.ToString(), out idleTime))
            {
                idleTime = 60.5f;
            }
        }

        protected override void Update(float deltaTime)
        {
            if (pet.InBattleField())
            {
                if (pet.FindTarget())
                {
                    GoToNextState(FsmStateType.PET_ATTACK);
                    return;
                }
            }
            elapsedTime += deltaTime;
            // 是否超出闪现距离
            if (!pet.InDungeon && !pet.InTransmitDis())
            {
                GoToNextState(FsmStateType.PET_TRANSMIT);
                return;
            }
            if (!pet.InFollowDis() && !pet.InDungeon)
            {
                // 开始追 
                GoToNextState(FsmStateType.PET_FOLLOW);
                return;
            }
            if (!pet.InDungeon && elapsedTime >= idleTime)
            {
                elapsedTime = 0;
                //不让主动走动
                //GoToNextState(FsmStateType.PET_WALK);
                return;
            }
        }
    }
}

using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmPetFollowState : FsmBaseState
    {
        Vec2 dest = null;

        private bool needTransmit;
        public bool NeedTransmit
        { get { return needTransmit; } }

        public FsmPetFollowState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = EnumerateUtility.FsmStateType.PET_FOLLOW;
        }

        protected override void Start(EnumerateUtility.FsmStateType prevState)
        {
            needTransmit = false;
            StartFollowing();
        }

        protected override void Update(float deltaTime)
        {
            pet.OnMove(deltaTime);
            elapsedTime += deltaTime;
            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;
                if (!pet.InBattleField() && !pet.InTransmitDis())
                {
                    needTransmit = true;
                    StopFollowing();
                    isEnd = true;
                    return;
                }
                if (!pet.Owner.IsMoving || pet.InFollowDis())
                {
                    StopFollowing();//!isdungeon walk
                    isEnd = true;
                    return;
                }
                // 主人的位置发生变化，重新追
                if (!Vec2.VeryClose(dest, pet.Owner.Position))
                {
                    StartFollowing();
                }
            }
        }

        private void StartFollowing()
        {
            dest = pet.Owner.Position;
            pet.SetDestination(dest);
            pet.OnMoveStart();
            pet.BroadCastMove();
        }

        private void StopFollowing()
        {
            if (pet.IsMoving)
            {
                pet.OnMoveStop();
                pet.BroadCastStop();
            }
        }

    }
}

using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmHeroFollowState : FsmBaseState
    {
        Vec2 dest = null;

        private bool needTransmit;
        public bool NeedTransmit
        { get { return needTransmit; } }

        public FsmHeroFollowState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = EnumerateUtility.FsmStateType.HERO_FOLLOW;
        }

        protected override void Start(EnumerateUtility.FsmStateType prevState)
        {
            needTransmit = false;
            StartFollowing();
        }

        protected override void Update(float deltaTime)
        {
            hero.OnMove(deltaTime);
            elapsedTime += deltaTime;
            if (hero.InBattleField())
            {
                if (hero.FindTarget())
                {
                    GoToNextState(FsmStateType.HERO_ATTACK);
                    return;
                }
            }
            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;
                if (!hero.InBattleField() && !hero.InTransmitDis())
                {
                    GoToNextState(FsmStateType.HERO_TRANSMIT);
                    return;
                }
                if (!hero.Owner.IsMoving || hero.InFollowDis())
                {
                    if (!hero.InBattleField())
                    {
                        GoToNextState(FsmStateType.HERO_WALK);
                        return;
                    }
                    else
                    {
                        GoToNextState(FsmStateType.HERO_IDLE);
                        return;
                    }
                }
                // 主人的位置发生变化，重新追
                if (!Vec2.VeryClose(dest, hero.Owner.Position))
                {
                    StartFollowing();
                }
            }
        }

        private void StartFollowing()
        {
            dest = hero.Owner.Position;
            hero.SetDestination(dest);
            hero.OnMoveStart();
            hero.BroadCastMove();
        }

        private void StopFollowing()
        {
            if (hero.IsMoving)
            {
                hero.OnMoveStop();
                hero.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopFollowing();
            base.GoToNextState(state);
        }
    }
}

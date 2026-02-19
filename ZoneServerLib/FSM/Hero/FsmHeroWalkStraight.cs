using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using CommonUtility;

namespace ZoneServerLib
{
    public class FsmHeroWalkStraightState : FsmBaseState
    {
        private Vec2 dest;

        Tuple<Vec2, int> param; //哪个种怪逻辑的行走哪个方向多远

        public FsmHeroWalkStraightState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.HERO_WALK_STRAIGHT;
        }

        protected override void Start(FsmStateType prevState)
        {
            param = startParam as Tuple<Vec2, int>;
            StartWalking();
        }

        protected override void Update(float deltaTime)
        {
            bool arrived = hero.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;
            }

            if (arrived)
            {
                GoToNextState(FsmStateType.HERO_ATTACK);
                return;
            }

        }

        private Vec2 NextWalkPosition()
        {
            Vec2 dest = param.Item1 + owner.Position;
            return dest;
        }

        private void StartWalking()
        {
            dest = NextWalkPosition();
            hero.SetDestination(dest);
            hero.OnMoveStart();
            hero.BroadCastMove();
        }

        private void StopWalking()
        {
            if (!hero.IsMoving || hero.InBattleField())
            {
                hero.OnMoveStop();
                //hero.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
            param = null;
        }
    }
}

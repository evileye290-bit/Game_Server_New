using CommonUtility;
using System;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmHeroWalkState : FsmBaseState
    {

        private Vec2 dest;

        public FsmHeroWalkState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.HERO_WALK;
        }

        protected override void Start(EnumerateUtility.FsmStateType prevState)
        {
            StartWalking();
            if (hero.InBattleField())
            {
                if (hero.FindTarget())
                {
                    GoToNextState(FsmStateType.HERO_ATTACK);
                    return;
                }
            }
        }

        protected override void Update(float deltaTime)
        {
            // todo 攻击状态转换逻辑待添加
            bool arrived = hero.OnMove(deltaTime);
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
                // 是否超出闪现距离
                if (!hero.InBattleField() && !hero.InTransmitDis())
                {
                    GoToNextState(FsmStateType.HERO_TRANSMIT);
                    return;
                }
                if (hero.Owner.IsMoving && !hero.InFollowDis() && !hero.InDungeon)
                {
                    // 开始追 
                    GoToNextState(FsmStateType.HERO_FOLLOW);
                    return;
                }
            }

            if (arrived)
            {
                if (hero.InWalkDis())
                {
                    GoToNextState(FsmStateType.HERO_IDLE);
                    return;
                }
                else
                {
                    StartWalking();
                }
            }
            //if (arrived)
            //{
            //    StartWalking();
            //}
        }

        private Vec2 NextWalkPosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(hero.Owner.Position, hero.WalkRange);
                if (hero.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = new Vec2(hero.Owner.Position);
            return position;
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
            if (!hero.IsMoving||hero.InBattleField())
            {
                hero.OnMoveStop();
                hero.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);

        }

    }
}

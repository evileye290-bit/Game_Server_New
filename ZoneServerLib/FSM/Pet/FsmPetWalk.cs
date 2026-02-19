using CommonUtility;
using System;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmPetWalkState:FsmBaseState
    {
        private bool needFollow;
        public bool NeedFollow
        { get { return needFollow; } }

        private bool needTransmit;
        public bool NeedTransmit
        { get { return needTransmit; } }

        private Vec2 dest;

        public FsmPetWalkState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.PET_WALK;
        }

        protected override void Start(EnumerateUtility.FsmStateType prevState)
        {
            needFollow = false;
            needTransmit = false;
            StartWalking();
            if (pet.InBattleField() && pet.FindTarget())
            {
                GoToNextState(FsmStateType.PET_ATTACK);
                return;
            }
        }

        protected override void Update(float deltaTime)
        {
            // todo 攻击状态转换逻辑待添加
            bool arrived = pet.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (pet.InBattleField() && pet.FindTarget())
            {
                GoToNextState(FsmStateType.PET_ATTACK);
                return;
            }

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;
                // 是否超出闪现距离
                if (!pet.InBattleField() && !pet.InTransmitDis())
                {
                    StopWalking();
                    needTransmit = true;
                    isEnd = true;
                    return;
                }
                if (pet.Owner.IsMoving & !pet.InFollowDis() && !pet.InDungeon)
                {
                    // 开始追 
                    needFollow = true;
                    isEnd = true;
                    return;
                }
            }
            if (arrived)
            {
                //到达目标点后静止 不走动
                if (pet.InWalkDis())
                {
                    GoToNextState(FsmStateType.PET_IDLE);
                    return;
                }
                else
                {
                    StartWalking();
                }
            }
        }

        private Vec2 NextWalkPosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(pet.Owner.Position, pet.WalkRange);
                if (pet.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = new Vec2(pet.Owner.Position);
            return position;
        }

        private void StartWalking()
        {
            dest = NextWalkPosition();
            pet.SetDestination(dest);
            pet.OnMoveStart();
            pet.BroadCastMove();
        }

        private void StopWalking()
        {
            if (!pet.IsMoving)
            {
                pet.OnMoveStop();
                pet.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);
        }
    }
}

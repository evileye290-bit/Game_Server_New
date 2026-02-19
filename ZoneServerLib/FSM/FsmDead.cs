using EnumerateUtility;
using System;

namespace ZoneServerLib
{
    public class FsmDeadState : FsmBaseState
    {
        private float delayTime = 0.2f;
        private bool processDeath = false;
        public FsmDeadState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.DEAD;
            //delayTime = Math.Max(owner.DeadDelay, 0.2f);
        }
        protected override void Start(FsmStateType prevState)
        {
            processDeath = false;
        }

        protected override void Update(float deltaTime)
        {
            if(processDeath)
            {
                return;
            }
            elapsedTime += deltaTime;
            if(elapsedTime >= delayTime)
            {
                processDeath = true;
                owner.OnDead();
            }
        }

        protected override void End(FsmStateType nextState)
        {
            //防止玩家单位死亡，被立即复活的时候没有触发死亡事件
            if (!processDeath)
            {
                processDeath = true;
                owner.OnDead();
            }

            // 只有复活跳出死亡状态机
            if(owner.CanBeRevived())
            {
                owner.OnRevived();
            }
            base.End(nextState);
        }
    }
}

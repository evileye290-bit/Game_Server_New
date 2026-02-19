using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    public class FsmRunState: FsmBaseState
    {
        float m_syncElapsedTime = 0.0f;
        float m_tick = 0.2f;

        public FsmRunState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.RUN;
        }

        protected override void Start(FsmStateType prevState)
        {
            owner.OnMoveStart();
            owner.BroadCastMove();

            m_syncElapsedTime = 0.0f;
        }

        protected override void Update(float deltaTime)
        {
            m_syncElapsedTime += deltaTime;

            // NOTE : 移动
            bool isDestPosChanged = owner.CheckDestination();
            if (isDestPosChanged)
            {
                owner.OnMoveStart();

                if (m_syncElapsedTime >= m_tick)
                {
                    m_syncElapsedTime = 0.0f;
                    owner.BroadCastMove();
                }
            }
            // OnMove
            bool isMoveEnd = owner.OnMove(deltaTime);
            if (isMoveEnd)
            {
                isEnd = true;
                //if (owner.FieldObjectType == TYPE.PC)
                //{
                //    PlayerChar pc = (PlayerChar)owner;
                //}
            }
        }

        protected override void End(FsmStateType nextState)
        {
            owner.OnMoveStop();
            base.End(nextState);
        }
    }

}
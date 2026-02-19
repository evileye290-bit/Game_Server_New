using EnumerateUtility;
namespace ZoneServerLib
{
    public class FsmIdleState: FsmBaseState
    {
        private float idleTime;
        private bool forever;
        public FsmIdleState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.IDLE;
            forever = false;
            idleTime = 0f;
        }

        protected override void Start(FsmStateType prevState)
        {
            if (startParam == null || !float.TryParse(startParam.ToString(), out idleTime))
            {
                forever = true;
            }
        }

        protected override void Update(float deltaTime)
        {
            if(forever)
            {
                return;
            }
            elapsedTime += deltaTime;
            if(elapsedTime >= idleTime)
            {
                isEnd = true;
            }
        }
    }

}
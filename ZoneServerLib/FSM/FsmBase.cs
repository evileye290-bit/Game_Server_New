using CommonUtility;
using EnumerateUtility;
namespace ZoneServerLib
{
    public class FsmBaseState
    {
        protected FsmStateType fsmStateType;
        public FsmStateType FsmStateType
        { get { return fsmStateType; } }

        protected FieldObject owner = null;

        protected Monster monster = null;
        protected Pet pet = null;
        protected Hero hero = null;
        protected PlayerChar player = null;
        protected Robot robot = null;

        protected object startParam = null;

        protected bool isEnd = false;
        public bool IsEnd
        { get { return isEnd; } }

        protected FsmStateType next;
        public FsmStateType Next
        { get { return next; } }

        protected float elapsedTime = 0.0f;
        public float ElapsedTime { get { return elapsedTime; } }

        public FsmBaseState(FieldObject owner)
        {
            this.owner = owner;
            fsmStateType = FsmStateType.BASE;
            switch (owner.FieldObjectType)
            { 
                case TYPE.PC:
                    player = (PlayerChar)owner;
                    break;
                case TYPE.ROBOT:
                    robot = (Robot)owner;
                    break;
                case TYPE.MONSTER:
                    monster = (Monster)owner;
                    break;
                case TYPE.PET:
                    pet = (Pet)owner;
                    break;
                case TYPE.HERO:
                    hero = (Hero)owner;
                    break;
                default:
                    break;
            }
        }

        public void OnStart(FsmStateType prevStateType, object startParam)
        {
            isEnd = false;
            elapsedTime = 0.0f;
            this.startParam = startParam;
            Start(prevStateType);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!isEnd)
            {
                Update(deltaTime);
            }
        }

        public void OnEnd(FsmStateType nextStateType)
        {
            isEnd = true;
            startParam = null;
            End(nextStateType);
        }

        protected virtual void Start(FsmStateType prevState) { }
        protected virtual void Update(float deltaTime) { }
        protected virtual void End(FsmStateType nextState) { startParam = null; }

        public bool CanStart(FsmBaseState prevState)
        {
            // 在复活中情况下跳出死亡状态机
            if (prevState.FsmStateType == FsmStateType.DEAD && owner.InDungeon)
            {
                return owner.IsReviving;
            }
            return true;
        }
        
        public int Compare(FsmStateType other)
        {
            int myPriority = GetPriority(FsmStateType);
            int otherPriority = GetPriority(other);
            return (myPriority - otherPriority);
        }

        public int GetPriority(FsmStateType stateType)
        {
            switch (stateType)
            { 
                case FsmStateType.BASE :
                    return -1;
                case FsmStateType.DEAD:
                    return 999;
                default:
                    return 100;
            }
        }

        protected virtual void GoToNextState(FsmStateType state)
        {
            next = state;
            isEnd = true;
        }
    }
}
using EnumerateUtility;
namespace ZoneServerLib
{
    public class FsmMonsterBorn : FsmBaseState
    {
        float bornTime;
        public FsmMonsterBorn(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_BORN;
            bornTime = monster.MonsterModel.BornTime;
        }

        protected override void Start(FsmStateType prevState)
        {
            // mosnter Borning 创建时为true，此处不需要做处理
        }

        protected override void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= bornTime)
            {
                isEnd = true;
            }
        }

        protected override void End(FsmStateType nextState)
        {
            monster.Borning = false;
            base.End(nextState);
        }

        public float GetBornTime()
        {
            float time = bornTime - elapsedTime;
            if (time <= 0)
            {
                Monster monster = owner as Monster;
                Logger.Log.Warn("monster {0} fsm born get born time error : time is {1}", monster.MonsterModel.Id, time);
                isEnd = true;
                return 0;
            }
            else
            {
                return time;
            }
        } 
    }

}
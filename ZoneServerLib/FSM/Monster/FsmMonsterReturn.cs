using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmMonsterReturnState : FsmBaseState
    {
        private Vec2 home;
        private bool arrived = false;
        long hp;
        long maxHp;
        private float recoverRate = 0.3f;
        private float recoverTick = 2;

        public FsmMonsterReturnState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_RETURN;
        }
        protected override void Start(FsmStateType prevState)
        {
            arrived = false;
            hp = 0;
            maxHp = int.MaxValue;

            monster.HateManager.ClearAllHates();
            home = HomePosition();
            monster.OnMoveStart();

            monster.SetDestination(home);
            monster.OnMoveStart();
            monster.BroadCastMove(); 
        }

        protected override void Update(float deltaTime)
        {
            // 回血
            //hp = monster.GetHp();
            //maxHp = monster.GetMaxHp();
            elapsedTime += deltaTime;
            hp = monster.GetNatureValue(NatureType.PRO_HP);
            maxHp = monster.GetNatureValue(NatureType.PRO_MAX_HP);
            if (elapsedTime >= recoverTick)
            {
                elapsedTime = 0.0f;
                if (hp < maxHp)
                {
                    int recoveryHp = (int)(maxHp * recoverRate);
                    monster.UpdateHp(DamageType.Cure, recoveryHp, monster); 
                }
            }
            // 回家
            if (!arrived)
            {
                arrived = monster.OnMove(deltaTime);
            }
            if (arrived && monster.FullHp())
            {
                isEnd = true;
                return;
            }
        }

        private Vec2 HomePosition()
        {
            return monster.GenCenter;
        }

    }
}

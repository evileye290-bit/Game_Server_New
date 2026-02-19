using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmHeroIdleState : FsmBaseState
    {
        private float idleTime;
        private bool forever;
        public FsmHeroIdleState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.HERO_IDLE;
            forever = false;
            idleTime = 4.5f;
        }

        protected override void Start(FsmStateType prevState)
        {
            if (startParam == null || !float.TryParse(startParam.ToString(), out idleTime))
            {
                idleTime = 60.5f;
            }
        }

        protected override void Update(float deltaTime)
        {
            if (hero.InBattleField())
            {
                if (hero.FindTarget())
                {
                    GoToNextState(FsmStateType.HERO_ATTACK);
                    return;
                }
            }
            elapsedTime += deltaTime;
            // 是否超出闪现距离
            if (!hero.InDungeon && !hero.InTransmitDis())
            {
                GoToNextState(FsmStateType.HERO_TRANSMIT);
                return;
            }
            if (!hero.InFollowDis() && !hero.InDungeon)
            {
                // 开始追 
                GoToNextState(FsmStateType.HERO_FOLLOW);
                return;
            }
            if (!hero.InDungeon && elapsedTime >= idleTime)
            {
                elapsedTime = 0;
                //GoToNextState(FsmStateType.HERO_WALK);
                return;
            }
        }
    }
}

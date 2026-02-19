using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ZoneServerLib
{
    
    public class FsmHeroTransmitState : FsmBaseState
    {
        Vec2 tempPos;
        public FsmHeroTransmitState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = EnumerateUtility.FsmStateType.HERO_TRANSMIT;
        }

        protected override void Start(FsmStateType prevState)
        {
            base.Start(prevState);
            tempPos = startParam as Vec2;
        }

        protected override void Update(float deltaTime)
        {
            // 闪现处理
            Vec2 transmitPos = CalcAppearancePosition();
            if (tempPos != null)
            {
                hero.Transmit(tempPos);
            }
            else
            {
                hero.Transmit(transmitPos);
            }
            isEnd = true;
        }

        private Vec2 CalcAppearancePosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(hero.Owner.Position, 2f);
                if (hero.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = Vec2.GetRandomPos(hero.Owner.Position, 1);
            return position;
        }
    }
}

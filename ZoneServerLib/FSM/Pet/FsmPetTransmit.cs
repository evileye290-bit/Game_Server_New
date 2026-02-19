using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmPetTransmitState: FsmBaseState
    {
        public FsmPetTransmitState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = EnumerateUtility.FsmStateType.PET_TRANSMIT;
        }

        protected override void Update(float deltaTime)
        {
            // 闪现处理
            Vec2 transmitPos = CalcAppearancePosition();
            pet.Transmit(transmitPos);
            isEnd = true;
        }

        private Vec2 CalcAppearancePosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(pet.Owner.Position, 2f);
                if (pet.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = Vec2.GetRandomPos(pet.Owner.Position, 1);
            return position;
        }
    }
}

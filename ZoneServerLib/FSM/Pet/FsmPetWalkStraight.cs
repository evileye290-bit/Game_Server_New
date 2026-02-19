using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using CommonUtility;
using ServerShared;

namespace ZoneServerLib
{
    public class FsmPetWalkStraightState : FsmBaseState
    {
        private Vec2 dest;

        public FsmPetWalkStraightState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.PET_WALK_STRAIGHT;
        }

        protected override void Start(FsmStateType prevState)
        {
            StartWalking();
        }

        protected override void Update(float deltaTime)
        {
            bool arrived = pet.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;
            }

            if (arrived)
            {
                GoToNextState(FsmStateType.PET_ATTACK);
                return;
            }

        }

        private Vec2 NextWalkPosition()
        {
            Vec2 dest = owner.Position;
            MapType mapType = owner.CurDungeon.GetMapType();
            switch (mapType)
            {
                case MapType.Arena:
                    dest = GetPVPNextWalkPosition(ArenaLibrary.ChallengerWalkVec, ArenaLibrary.DefenderWalkVec);
                    break;
                //case MapType.CrossBattle:
                //case MapType.CrossFinals:
                ////case MapType.CrossBoss:
                ////    break;
                //case MapType.CrossBossSite:
                //    dest = GetPVPNextWalkPosition(new Vec2(0.0f, CrossBattleLibrary.CrossWalkDistance), new Vec2(0.0f, -CrossBattleLibrary.CrossWalkDistance));
                //    break;
                default:
                    dest = GetPVENextWalkPosition();
                    break;
            }
            return dest;
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
            if (!pet.IsMoving || pet.InBattleField())
            {
                pet.OnMoveStop();
                //hero.BroadCastStop();
            }
        }

        protected override void GoToNextState(FsmStateType state)
        {
            StopWalking();
            base.GoToNextState(state);
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
        }

        private Vec2 GetPVENextWalkPosition()
        {
            float y = Math.Abs(PetLibrary.PetConfig.GetPetPosition().Y);
            Vec2 walkStraight = owner.Position + new Vec2(0, y);
            if (owner.CurDungeon.MonsterList.Count == 0)
            {
                return walkStraight;
            }
            Monster monster = owner.CurDungeon.MonsterList.Values.First();
            Vec2 finalWalkStraight = monster.Generator.Model.WalkStraightVec;
            Vec2 dest = finalWalkStraight + walkStraight;
            return dest;
        }

        private Vec2 GetPVPNextWalkPosition(Vec2 attackerWalkVec, Vec2 defenderWalkVec)
        {
            Vec2 walkStraight;
            Vec2 dest;
            float y = Math.Abs(PetLibrary.PetConfig.GetPetPosition().Y);//defender这么取值好像有问题 todo check
            if (pet.IsAttacker)
            {
                walkStraight = owner.Position + new Vec2(0, y);
                dest = attackerWalkVec + walkStraight;
            }
            else
            {
                walkStraight = owner.Position + new Vec2(0, -y);
                dest = defenderWalkVec + walkStraight;
            }
            return dest;
        }
    }
}

using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class FsmPetSurroundState : FsmBaseState
    {

        private Vec2 dest;//当前目标点，到达后判断是碰撞还是计算技能
        private FieldObject target;
        private Skill skill;
        Tuple<Vec2, FieldObject, Skill> param;

        public Skill Skill
        { get { return skill; } }

        public FsmPetSurroundState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.PET_SURROUND;
        }

        protected override void Start(FsmStateType prevState)
        {
            param = startParam as Tuple<Vec2, FieldObject, Skill>;
            if (param == null)
            {
                GoToNextState(FsmStateType.PET_IDLE);
                return;
            }
            else
            {
                dest = param.Item1;
                target = param.Item2;
                skill = param.Item3;
                Pet own = owner as Pet;
                if (own.CheckCollision(dest))
                {
                    //Tuple<bool, Vec2> ans = own.GetNonCollisionPos(own.Owner.Position, own.Owner.Radius, target, dest, skill.SkillModel.SkillPosRange);
                    Tuple<bool, Vec2> ans = own.GetNonCollisionPos(target, dest, skill.SkillModel.SkillPosRange);
                    if (ans.Item2 != dest)
                    {
                        dest = ans.Item2;
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - pet.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                        return;
                    }
                }
                else
                {
                    skill.InitCastParam(target.Position - pet.Position, target.Position, target == null ? 0 : target.InstanceId);
                    GoToNextState(FsmStateType.SKILL);
                }
            }
            ////判断是否和当前位置比较近，如果近，不走
            //if (Vec2.VeryClose(owner.Position, dest))
            //{
                StartWalking();
            //}
        }

        protected override void Update(float deltaTime)
        {
            bool arrived = pet.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;

            }
            if (!target.IsDead)
            {
                if (arrived)
                {
                    Pet own = owner as Pet;

                    if (own.CheckCollision(dest))
                    {
                        //Tuple<bool, Vec2> ans = own.GetNonCollisionPos(own.Owner.Position,own.Owner.Radius,target, dest, skill.SkillModel.SkillPosRange);
                        Tuple<bool, Vec2> ans = own.GetNonCollisionPos(target, dest, skill.SkillModel.SkillPosRange);
                        if (ans.Item2 != dest)
                        {
                            dest = ans.Item2;
                            //if (Vec2.VeryClose(owner.Position, dest))
                            //{
                                StartWalking();
                            //}
                        }
                        else
                        {
                            skill.InitCastParam(target.Position - pet.Position, target.Position, target == null ? 0 : target.InstanceId);
                            GoToNextState(FsmStateType.SKILL);
                        }
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - pet.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                    }
                }
            }
            else
            {
                GoToNextState(FsmStateType.PET_IDLE);
            }
        }

        private void StartWalking()
        {
            pet.SetDestination(dest);
            pet.OnMoveStart();
            if (!Vec2.VeryClose(owner.Position, dest))
            {
                pet.BroadCastMove();
            }
        }

        private void StopWalking()
        {
            if (!pet.IsMoving || pet.InBattleField())
            {
                pet.OnMoveStop();
                pet.BroadCastStop();
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
            target = null;
            param = null;
            skill = null;
        }
    }
}

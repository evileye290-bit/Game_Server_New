using CommonUtility;
using EnumerateUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmMonsterSurroundState : FsmBaseState
    {
        private Vec2 dest;//当前目标点，到达后判断是碰撞还是计算技能
        private FieldObject target;
        private Skill skill;
        Tuple<Vec2, FieldObject, Skill> param;

        public Skill Skill
        { get { return skill; } }

        public FsmMonsterSurroundState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_SURROUND;
        }

        protected override void Start(FsmStateType prevState)
        {
            param = startParam as Tuple<Vec2, FieldObject, Skill>;
            if (param == null)
            {
                GoToNextState(FsmStateType.MONSTER_RETURN);
                return;
            }
            else
            {
                dest = param.Item1;
                target = param.Item2;
                skill = param.Item3;
                Monster own = owner as Monster;
                if (own.MonsterModel.ModelSizeType != 2 && owner.CurDungeon.MonsterList.Count > 1 && own.CheckCollision(owner.Position))
                {
                    //Tuple<bool, Vec2> ans = own.GetNonCollisionPos(own.Owner.Position, own.Owner.Radius, target, dest, skill.SkillModel.SkillPosRange);
                    Tuple<bool, Vec2> ans = own.GetNonCollisionPos(target, dest, skill.SkillModel.SkillPosRange);
                    if (ans.Item2 != dest)
                    {
                        dest = ans.Item2;
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - monster.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                        return;
                    }
                }
                else
                {
                    skill.InitCastParam(target.Position - monster.Position, target.Position, target == null ? 0 : target.InstanceId);
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
            bool arrived = monster.OnMove(deltaTime);
            elapsedTime += deltaTime;

            if (elapsedTime >= 0.5)
            {
                elapsedTime = 0;

            }
            if (!target.IsDead)
            {
                if (arrived)
                {
                    Monster own = owner as Monster;

                    if (own.MonsterModel.ModelSizeType != 2 && owner.CurDungeon.MonsterList.Count > 1 && own.CheckCollision(owner.Position))
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
                            skill.InitCastParam(target.Position - monster.Position, target.Position, target == null ? 0 : target.InstanceId);
                            GoToNextState(FsmStateType.SKILL);
                        }
                    }
                    else
                    {
                        skill.InitCastParam(target.Position - monster.Position, target.Position, target == null ? 0 : target.InstanceId);
                        GoToNextState(FsmStateType.SKILL);
                    }
                }
            }
            else
            {
                GoToNextState(FsmStateType.MONSTER_RETURN);
            }
        }

        private void StartWalking()
        {
            monster.SetDestination(dest);
            monster.OnMoveStart();
            if (!Vec2.VeryClose(owner.Position, dest))
            {
                monster.BroadCastMove();
            }
        }

        private void StopWalking()
        {
            monster.OnMoveStop();
            monster.BroadCastStop();
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

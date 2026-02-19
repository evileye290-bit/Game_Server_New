using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Monster
    {
        protected override void InitFSM()
        {
            fsmManager = new FsmManager(this);
            fsmManager.AddFsmState(FsmStateType.IDLE);
            fsmManager.AddFsmState(FsmStateType.MONSTER_BORN);
            FsmManager.AddFsmState(FsmStateType.SKILL);
            fsmManager.AddFsmState(FsmStateType.MONSTER_SEARCH);
            fsmManager.AddFsmState(FsmStateType.MONSTER_ATTACK);
            fsmManager.AddFsmState(FsmStateType.MONSTER_RETURN);
            fsmManager.AddFsmState(FsmStateType.MONSTER_FOLLOW);
            fsmManager.AddFsmState(FsmStateType.MONSTER_SURROUND);
            fsmManager.AddFsmState(FsmStateType.DEAD);

            fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_BORN);
        }


        public override void FsmAIUpdate(float delta)
        {
            if (!InBattle)
            {
                return;
            }
            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.BASE:
                    // 空闲状态下应进入巡逻状态
                    fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_SEARCH);
                    break;

                case FsmStateType.IDLE:
                case FsmStateType.SKILL:
                case FsmStateType.MONSTER_BORN:
                case FsmStateType.MONSTER_RETURN:
                    if (fsmManager.CurFsmState.IsEnd)
                    { 
                        fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_SEARCH);
                    }
                    break;

                case FsmStateType.MONSTER_SEARCH:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;
                case FsmStateType.MONSTER_ATTACK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmMonsterAttackState attackState = (FsmMonsterAttackState)fsmManager.CurFsmState;
                        if (attackState.Next == FsmStateType.MONSTER_RETURN)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_RETURN);
                        }
                        else if (attackState.Next == FsmStateType.SKILL)
                        { 
                            // 释放技能后，进入技能cd
                            fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
                        }
                        else if (attackState.Next == FsmStateType.MONSTER_SURROUND)
                        {
                            object param = attackState.nextParam;
                            fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_SURROUND, false, param);
                        }
                        else
                        {
                            // 目标不存在或者死亡，则重新巡逻
                            fsmManager.SetNextFsmStateType(FsmStateType.MONSTER_SEARCH);
                        }
                    }
                    break;
                case FsmStateType.MONSTER_SURROUND:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmMonsterSurroundState attackState = (FsmMonsterSurroundState)fsmManager.CurFsmState;
                        fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
                    }
                    break;
                case FsmStateType.MONSTER_FOLLOW:
                    if(fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;

                case FsmStateType.DEAD:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                    }
                    break;

                default:
                    Log.Warn("monster fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

    }
}

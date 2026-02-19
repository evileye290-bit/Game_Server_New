using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class Hero
    {
        Dictionary<int, bool> usedGenerateMonsterId = new Dictionary<int, bool>();

        int curStraightWalkId = 0;
        Vec2 curStraightWalkVec = null;
        float FSMWaitTime = 0;
        bool beginFSMWalkWait = false;

        public List<int> OneSideWalkVecs = new List<int>();
        int oneSideWalkId = 1;
        Vec2 oneSideWalkVec = null;

        protected override void InitFSM()
        {
            fsmManager = new FsmManager(this);
            fsmManager.AddFsmState(FsmStateType.BASE);
            fsmManager.AddFsmState(FsmStateType.IDLE);//受到buff等走这个
            fsmManager.AddFsmState(FsmStateType.SKILL);
            fsmManager.AddFsmState(FsmStateType.DEAD);
            fsmManager.AddFsmState(FsmStateType.HERO_WALK);
            fsmManager.AddFsmState(FsmStateType.HERO_FOLLOW);
            fsmManager.AddFsmState(FsmStateType.HERO_TRANSMIT);
            fsmManager.AddFsmState(FsmStateType.HERO_ATTACK);
            fsmManager.AddFsmState(FsmStateType.HERO_IDLE);
            fsmManager.AddFsmState(FsmStateType.HERO_SURROUND);
            fsmManager.AddFsmState(FsmStateType.HERO_WALK_STRAIGHT);         
        }

        public override void FsmAIUpdate(float delta)
        {
            if (currentMap == null)
            {
                return;
            }
            if (currentMap.IsDungeon || currentMap.Model.IsArena())
            {
                FightingAIUpdate(delta);
            }
            else
            {
                PeaceAIUpdate();
            }
        }

        // 和平状态下的ai状态机跳转规则
        private void PeaceAIUpdate()
        {
            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.BASE:
                case FsmStateType.IDLE:
                case FsmStateType.DEAD:
                case FsmStateType.SKILL:
                    // 空闲状态下应进入散步状态
                    fsmManager.SetNextFsmStateType(FsmStateType.HERO_WALK);
                    break;
                case FsmStateType.HERO_TRANSMIT:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.HERO_WALK);
                    }
                    break;
                case FsmStateType.HERO_IDLE:
                case FsmStateType.HERO_WALK:
                case FsmStateType.HERO_FOLLOW:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;
                default:
                    Log.Warn("hero fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

        private void FightingAIUpdate(float delta)
        {
            if (!InBattle)
            {
                return;
            }

            if (beginFSMWalkWait)
            {
                FSMWaitTime -= delta;
                if (FSMWaitTime <= 0)
                {
                    beginFSMWalkWait = false;
                    FSMWaitTime = 0f;
                    //假如需要进入直线行走状态，直接进入
                    if (curStraightWalkId > 0 && !usedGenerateMonsterId.ContainsKey(curStraightWalkId) && curStraightWalkVec != null)
                    {
                        usedGenerateMonsterId.Add(curStraightWalkId, true);

                        Tuple<Vec2, int> param = Tuple.Create(curStraightWalkVec, curStraightWalkId);
                        fsmManager.SetNextFsmStateType(FsmStateType.HERO_WALK_STRAIGHT, false, param);
                        return;
                    }                  
                }
            }

            if (!OneSideWalkVecs.Contains(oneSideWalkId) && oneSideWalkVec != null)
            {
                this.OneSideWalkVecs.Add(oneSideWalkId);
                Tuple<Vec2, int> oneSideWalkVecs = Tuple.Create(oneSideWalkVec, 1);
                fsmManager.SetNextFsmStateType(FsmStateType.HERO_WALK_STRAIGHT, false, oneSideWalkVecs);
                return;
            }

            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.BASE:
                    fsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
                    break;
                case FsmStateType.IDLE:
                case FsmStateType.DEAD:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
                    }
                    break;
                case FsmStateType.SKILL:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        //FsmSkillState fsm = fsmManager.CurFsmState as FsmSkillState;
                        //if (CastSkillThenDeadSkillId > 0 && fsm.Skill != null && CastSkillThenDeadSkillId == fsm.Skill.Id)
                        //if (CastSkillThenDeadSkillId > 0 && CastSkillThenDeadSkillId == (fsmManager.CurFsmState as FsmSkillState).SkillId)
                        //{
                        //    CastSkillThenDeadSkillId = -1;
                        //    long hp = GetNatureValue(NatureType.PRO_HP);
                        //    AddNatureBaseValue(NatureType.PRO_HP, -hp);
                        //    isDead = true;
                        //    BroadCastHp();
                        //    fsmManager.SetNextFsmStateType(FsmStateType.DEAD);
                        //}
                        //else
                        //{
                            fsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
                        //}
                    }

                    break;
                // 空闲状态下应进入散步状态
                case FsmStateType.HERO_TRANSMIT:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
                    }
                    break;
                case FsmStateType.HERO_IDLE:
                case FsmStateType.HERO_WALK:
                case FsmStateType.HERO_WALK_STRAIGHT:
                case FsmStateType.HERO_FOLLOW:              
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;
                case FsmStateType.HERO_ATTACK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmHeroAttackState attackState = (FsmHeroAttackState)fsmManager.CurFsmState;
                        if (attackState.Next == FsmStateType.HERO_IDLE)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
                        }
                        else if (attackState.Next == FsmStateType.HERO_TRANSMIT)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.HERO_TRANSMIT);
                        }
                        else if (attackState.Next == FsmStateType.HERO_SURROUND)
                        {
                            object param = attackState.nextParam;
                            fsmManager.SetNextFsmStateType(FsmStateType.HERO_SURROUND, false, param);
                        }
                        else
                        {
                            // 释放技能后，进入技能cd
                            fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
                        }
                    }
                    break;
                case FsmStateType.HERO_SURROUND:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmHeroSurroundState attackState = (FsmHeroSurroundState)fsmManager.CurFsmState;
                        fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
                    }
                    break;
                default:
                    Log.Warn("hero fighting fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

        public void SetWalkStraightVec(int id, Vec2 vec, float wait = 0f)
        {
            curStraightWalkId = id;
            curStraightWalkVec = vec;
            this.FSMWaitTime = wait;
            beginFSMWalkWait = true;
        }

        public void SetHeroOneSideWalkVec( Vec2 vec)
        {
            oneSideWalkVec = vec;
        }
    }
}

using CommonUtility;
using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    partial class Pet
    {
        float fsmWaitTime = 0;
        bool beginFSMWalkWait = false;
        bool startFight = true; 

        protected override void InitFSM()
        {
            fsmManager = new FsmManager(this);
            fsmManager.AddFsmState(FsmStateType.IDLE);
            fsmManager.AddFsmState(FsmStateType.SKILL);
            //fsmManager.AddFsmState(FsmStateType.DEAD);
            fsmManager.AddFsmState(FsmStateType.PET_WALK);
            fsmManager.AddFsmState(FsmStateType.PET_FOLLOW);
            fsmManager.AddFsmState(FsmStateType.PET_TRANSMIT);
            fsmManager.AddFsmState(FsmStateType.PET_IDLE);
            fsmManager.AddFsmState(FsmStateType.PET_ATTACK);
            fsmManager.AddFsmState(FsmStateType.PET_SURROUND);
            fsmManager.AddFsmState(FsmStateType.PET_WALK_STRAIGHT);
        }

        public override void FsmAIUpdate(float delta)
        {
            if (currentMap == null)
            {
                return;
            }
            if (currentMap.IsDungeon)
            {
                FightAIUpdate(delta);
            }
            else
            {
                PeaceAIUpdate();
            }
        }

        private void FightAIUpdate(float delta)
        {
            if (!InBattle)
            {
                return;
            }
            if (beginFSMWalkWait)
            {
                fsmWaitTime -= delta;
                if (fsmWaitTime <= 0)
                {
                    beginFSMWalkWait = false;
                    fsmWaitTime = 0f;
                    //假如需要进入直线行走状态，直接进入
                    fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK_STRAIGHT, false);
                    return;
                }
            }

            if (startFight)
            {
                fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK_STRAIGHT, false);
                startFight = false;
                return;
            }

            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.BASE:
                case FsmStateType.IDLE:
                    // 空闲状态下应进入散步状态
                    fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK);
                    break;
                //case FsmStateType.DEAD:
                //    if (fsmManager.CurFsmState.IsEnd)
                //    {
                //        fsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                //    }
                //    break;
                case FsmStateType.SKILL:
                case FsmStateType.PET_TRANSMIT:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.PET_IDLE);
                    }
                    break;
                case FsmStateType.PET_WALK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetWalkState walkState = (FsmPetWalkState)fsmManager.CurFsmState;
                        if (walkState.NeedTransmit)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_TRANSMIT);
                        }
                        else if (walkState.NeedFollow)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_FOLLOW);
                        }
                        else
                        {
                            fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                        }
                    }
                    break;
                case FsmStateType.PET_FOLLOW:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetFollowState followState = (FsmPetFollowState)fsmManager.CurFsmState;
                        if (followState.NeedTransmit)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_TRANSMIT);
                        }
                        else
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK);
                        }
                    }
                    break;
                case FsmStateType.PET_IDLE:
                case FsmStateType.PET_WALK_STRAIGHT:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;
                case FsmStateType.PET_ATTACK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetAttackState attackState = (FsmPetAttackState)fsmManager.CurFsmState;
                        if (attackState.Next == FsmStateType.PET_IDLE)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_IDLE);
                        }
                        //else if (attackState.Next == FsmStateType.PET_TRANSMIT)
                        //{
                        //    fsmManager.SetNextFsmStateType(FsmStateType.PET_TRANSMIT);
                        //}
                        else if (attackState.Next == FsmStateType.PET_SURROUND)
                        {
                            object param = attackState.nextParam;
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_SURROUND, false, param);
                        }
                        else
                        {
                            // 释放技能后，进入技能cd
                            fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, attackState.Skill);
                        }
                    }
                    break;
                case FsmStateType.PET_SURROUND:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetSurroundState surroundState = (FsmPetSurroundState)fsmManager.CurFsmState;
                        fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, surroundState.Skill);
                    }
                    break;
                default:
                    Log.Warn("pet fight fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

        private void PeaceAIUpdate()
        {
            switch (fsmManager.CurFsmState.FsmStateType)
            {
                case FsmStateType.BASE:
                case FsmStateType.IDLE:
                    // 空闲状态下应进入散步状态
                    fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK);
                    break;
                case FsmStateType.PET_TRANSMIT:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK);
                    }
                    break;
                case FsmStateType.PET_WALK:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetWalkState walkState = (FsmPetWalkState)fsmManager.CurFsmState;
                        if (walkState.NeedTransmit)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_TRANSMIT);
                        }
                        else if (walkState.NeedFollow)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_FOLLOW);
                        }
                        else
                        {
                            fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                        }
                    }
                    break;
                case FsmStateType.PET_FOLLOW:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        FsmPetFollowState followState = (FsmPetFollowState)fsmManager.CurFsmState;
                        if (followState.NeedTransmit)
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_TRANSMIT);
                        }
                        else
                        {
                            fsmManager.SetNextFsmStateType(FsmStateType.PET_WALK);
                        }
                    }
                    break;
                case FsmStateType.PET_IDLE:
                    if (fsmManager.CurFsmState.IsEnd)
                    {
                        fsmManager.SetNextFsmStateType(fsmManager.CurFsmState.Next);
                    }
                    break;
                default:
                    Log.Warn("pet peace fsm ai update failed: cur state {0} not supported", fsmManager.CurFsmState.FsmStateType);
                    break;
            }
        }

        public void RecordFightFsmWaitTime(float waitTime)
        {
            fsmWaitTime = waitTime;
            beginFSMWalkWait = true;
        }
    }
}

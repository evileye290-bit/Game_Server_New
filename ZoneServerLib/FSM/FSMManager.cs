using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmManager
    {
        FieldObject owner;
        protected Dictionary<FsmStateType, FsmBaseState> stateList = new Dictionary<FsmStateType, FsmBaseState>();

        private FsmBaseState curFsmState;
        public FsmBaseState CurFsmState
        { get { return curFsmState; } }


        private FsmStateType nextStateType;
        public FsmStateType NextStateType
        { get { return nextStateType; } }

        private object nextStateParam;

        public FsmManager(FieldObject owner)
        {
            this.owner = owner;
            curFsmState = FsmStateFactory(FsmStateType.BASE);
            nextStateType = FsmStateType.BASE;
        }

        public void AddFsmState(FsmStateType fsmStateType)
        {
            if (stateList.ContainsKey(fsmStateType))
            {
                stateList.Remove(fsmStateType);
            }

            FsmBaseState state = FsmStateFactory(fsmStateType);
            stateList.Add(fsmStateType, state);
        }

        private FsmBaseState FsmStateFactory(FsmStateType fsmStateType)
        {
            switch (fsmStateType)
            { 
                case FsmStateType.BASE:
                    return new FsmBaseState(owner);
                case FsmStateType.CHASE:
                    return new FsmChaseState(owner);
                case FsmStateType.IDLE:
                    return new FsmIdleState(owner);
                case FsmStateType.SKILL:
                    return new FsmSkillState(owner);
                case FsmStateType.RUN:
                    return new FsmRunState(owner);
                case FsmStateType.WAIT:
                    return new FsmWaitState(owner);
                case FsmStateType.DEAD:
                    return new FsmDeadState(owner);

                case FsmStateType.MONSTER_BORN:
                    return new FsmMonsterBorn(owner);
                case FsmStateType.MONSTER_ATTACK:
                    return new FsmMonsterAttackState(owner);
                case FsmStateType.MONSTER_SEARCH:
                    return new FsmMonsterSearchState(owner);
                case FsmStateType.MONSTER_RETURN:
                    return new FsmMonsterReturnState(owner);
                case FsmStateType.MONSTER_FOLLOW:
                    return new FsmMonsterFollow(owner);
                case FsmStateType.MONSTER_SURROUND:
                    return new FsmMonsterSurroundState(owner);

                case FsmStateType.PET_WALK:
                    return new FsmPetWalkState(owner);
                case FsmStateType.PET_FOLLOW:
                    return new FsmPetFollowState(owner);
                case FsmStateType.PET_TRANSMIT:
                    return new FsmPetTransmitState(owner);
                case FsmStateType.PET_IDLE:
                    return new FsmPetIdleState(owner);
                case FsmStateType.PET_ATTACK:
                    return new FsmPetAttackState(owner);
                case FsmStateType.PET_SURROUND:
                    return new FsmPetSurroundState(owner);
                case FsmStateType.PET_WALK_STRAIGHT:
                    return new FsmPetWalkStraightState(owner);

                case FsmStateType.HERO_WALK:
                    return new FsmHeroWalkState(owner);
                case FsmStateType.HERO_FOLLOW:
                    return new FsmHeroFollowState(owner);
                case FsmStateType.HERO_TRANSMIT:
                    return new FsmHeroTransmitState(owner);
                case FsmStateType.HERO_IDLE:
                    return new FsmHeroIdleState(owner);

                case FsmStateType.HERO_ATTACK:
                    return new FsmHeroAttackState(owner);
                case FsmStateType.HERO_SURROUND:
                    return new FsmHeroSurroundState(owner);
                case FsmStateType.HERO_WALK_STRAIGHT:
                    return new FsmHeroWalkStraightState(owner);

                case FsmStateType.ROBOT_WALK:
                    return new FsmRobotWalkState(owner);
                case FsmStateType.ROBOT_ATTACK:
                    return new FsmRobotAttackState(owner);
                case FsmStateType.ROBOT_SURROUND:
                    return new FsmRobotSurroundState(owner);
                case FsmStateType.ROBOT_IDLE:
                    return new FsmRobotIdleState(owner);
                default:
                    Log.Warn("create fsm state failed: not support {0} yet", fsmStateType);
                    return null;
            }
        }

        public bool SetNextFsmStateType(FsmStateType next_state_type, bool restart_same_state = false, object next_state_param = null)
        {
            // restart_same_state == true, 表示即使next state 与 current state 一致，也重新进入该state
            if (!restart_same_state && curFsmState.FsmStateType == next_state_type)
            {
                return false;
            }
            if (nextStateType == next_state_type)
            {
                return true;
            }
            FsmBaseState nextState = null;
            if(!stateList.TryGetValue(next_state_type, out nextState))
            {
                return false;
            }
            if (!nextState.CanStart(curFsmState))
            {
                return false;
            }

            // 由于一帧内可以多次冲刷下一帧要进入的状态机
            // 需要根据优先级，判断能否覆盖掉即将要执行的nextStateType 
            // 如nextStateType为DEAD时，无法再进入RUN
            if (nextState.Compare(nextStateType) < 0)
            {
                return false;
            }

            // 验证通过
            nextStateType = next_state_type;
            nextStateParam = next_state_param;
            //Log.Debug("set next state from {0} to {1}", curFsmState.FsmStateType, next_state_type);
            return true;
        }

        private void CheckNextState()
        {
            if (nextStateType == FsmStateType.BASE)
            {
                return;
            }

            // 旧状态机结束
            FsmBaseState oldState = curFsmState;
            curFsmState.OnEnd(nextStateType);

            // 新状态机开始 
            curFsmState = stateList[nextStateType];
            curFsmState.OnStart(oldState.FsmStateType, nextStateParam);
            nextStateParam = null;
            //if (owner.FieldObjectType == TYPE.PC)
            //{
            //    Log.Debug("change fsmstate from {0} to {1}", curFsmState.FsmStateType, nextStateType);
            //}

            nextStateType = FsmStateType.BASE;
        }

        public void Update(float deltaTime)
        {
            CheckNextState();
            curFsmState.OnUpdate(deltaTime);
        }

    }
}

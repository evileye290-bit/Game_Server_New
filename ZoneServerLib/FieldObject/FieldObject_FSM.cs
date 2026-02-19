using CommonUtility;
using DataProperty;
using EnumerateUtility;
using System.Collections.Generic;


namespace ZoneServerLib
{
	public partial class FieldObject
    {
        // NOTE : State
        public bool IsStateIdle
        { get { return CurFsmStateType == FsmStateType.IDLE; } }

        public bool InRunningState
        { get { return CurFsmStateType == FsmStateType.RUN; } }

        public bool InSkillState
        { get { return CurFsmStateType == FsmStateType.SKILL; } }

        public bool IsDead
        { get { return isDead; } } //return GetHp() <= 0; } }

        public int CastSkillThenDeadSkillId = -1;
        public bool GetDeadlyHurt = false;

        public FsmStateType CurFsmStateType { get { return fsmManager.CurFsmState.FsmStateType; } }
        public FsmStateType NextFsmStateType { get { return fsmManager.NextStateType; } }

        protected FsmManager fsmManager;
        public FsmManager FsmManager
        { get { return fsmManager; } }


        protected virtual void InitFSM()
        {
        }

        // 根据状态机自身的状态和AI策略来切换状态
        public virtual void FsmAIUpdate(float delta)
        { 
        }

        // FSM不同状态之间的跳转有2种途径
        // 1是根据AI规则，如当前状态机是RUN，且到达目标点，则切换到IDLE状态
        // 2是业务层调用SetNextFsmStateType方法强制进行状态跳转，如当前状态机RUN，在到达目标点前被别人击毙,则直接进入DEAD状态
        // 2的优先级比1高, 即通过2方式设置下一个状态后，不应再通过AI进行逻辑跳转
        public void FsmUpdate(float deltaTime)
        {
            // 对于monster 如果战斗结束则不应再走状态机更新
            if(FieldObjectType == TYPE.MONSTER && !InBattle)
            {
                return;
            }
            if (fsmManager == null)
            {
                return;
            }
            if (fsmManager.NextStateType == FsmStateType.BASE)
            {
                FsmAIUpdate(deltaTime);
            }
            fsmManager.Update(deltaTime);
        }

	}
}
using CommonUtility;
using DataProperty;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class BaseTrigger
    {
        private FieldObject owner;
        public FieldObject Owner
        { get { return owner; } }

        private FieldObject caster;
        public FieldObject Caster
        { get { return caster; } }

        public TriggerModel Model
        { get; private set; }

        public virtual FieldMap CurMap { get { return null; } }

        private TriggerState state;

        protected BaseTriLnr listener;
        protected List<BaseTriCon> conditionList = new List<BaseTriCon>();
        protected TriggerConditionCombine conditionCombine;
        protected TriggerHandleFrequence handlerFrequence;
        protected List<BaseTriHdl> handlerList = new List<BaseTriHdl>();

        protected float triggerTime = 0.5f;
        protected float triggerPassTime = 0.5f;
        // 是否已经执行过
        protected bool handled;

        // condition及handler用到的参数列表 记录在此的param 每次检查condition后都会清空
        protected Dictionary<string, object> paramList = new Dictionary<string, object>();

        // condition及handler用到的参数列表 记录在此的param 不会清空 如由skill创建的trigger 记录skillLevel
        protected Dictionary<string, object> fixedParamList = new Dictionary<string, object>();

        protected Dictionary<TriggerCounter, int> triggerCounter = new Dictionary<TriggerCounter, int>();

        public BaseTrigger(FieldObject owner, FieldObject caster)
        {
            this.owner = owner;
            this.caster = caster;
            state = TriggerState.Start;
        }

        public BaseTrigger()
        {
            state = TriggerState.Start;
        }

        public void Init(TriggerModel model)
        {
            Model = model;
            listener = TriggerListenerFactory.CreateTriggerListener(this, model.MessageType);

            if (model.TriggerCondition_1 != TriggerCondition.None)
            {
                BaseTriCon condition_1 = TriggerConditionFactory.CreateTriggerCondition(this, model.TriggerCondition_1, model.ConditionParam_1);
                conditionList.Add(condition_1);
            }
            if (model.TriggerCondition_2 != TriggerCondition.None)
            {
                BaseTriCon condition_2 = TriggerConditionFactory.CreateTriggerCondition(this, model.TriggerCondition_2, model.ConditionParam_2);
                conditionList.Add(condition_2);
            }
            if (model.TriggerCondition_3 != TriggerCondition.None)
            {
                BaseTriCon condition_3 = TriggerConditionFactory.CreateTriggerCondition(this, model.TriggerCondition_3, model.ConditionParam_3);
                conditionList.Add(condition_3);
            }

            conditionCombine = model.ConditionCombine;
            handlerFrequence = model.HandleFrequence;
            if (model.HandlerType_1 != TriggerHandlerType.None)
            {
                BaseTriHdl handler_1 = TriggerHandlerFactory.CreateTriggerHandler(this, model.HandlerType_1, model.HandlerParam_1);
                handlerList.Add(handler_1);
            }
            if (model.HandlerType_2 != TriggerHandlerType.None)
            {
                BaseTriHdl handler_2 = TriggerHandlerFactory.CreateTriggerHandler(this, model.HandlerType_2, model.HandlerParam_2);
                handlerList.Add(handler_2);
            }
            if (model.HandlerType_3 != TriggerHandlerType.None)
            {
                BaseTriHdl handler_3 = TriggerHandlerFactory.CreateTriggerHandler(this, model.HandlerType_3, model.HandlerParam_3);
                handlerList.Add(handler_3);
            }

            if (model.EffectiveTime == 0)
            {
                triggerTime = model.CoolDowns;
                triggerPassTime = model.CoolDowns;
            }
            else
            {
                triggerTime = model.EffectiveTime;
                triggerPassTime = 0;
            }
        }

        public virtual MessageDispatcher GetMessageDispatcher()
        {
            return null;
        }

        public void Start()
        {
            state = TriggerState.Start;
        }
        public void Stop()
        {
            state = TriggerState.Stop;
        }

        // 有listener收到消息，需要检查condition是否满足
        public void TryHandle()
        {
            if(state != TriggerState.Start)
            {
                // 未开始状态下 触发行为不算数
                ResetConditons();
                ResetParams();
                return;
            }
            if (handled && handlerFrequence == TriggerHandleFrequence.Once)
            {
                return;
            }
            //增加触发CD
            if (triggerPassTime < triggerTime)
            {
                return;
            }
            if (!CheckCondition())
            {
                return;
            }
            if(ProbabilityHappened())
            {
                Handle();
            }
            triggerTime = Model.CoolDowns;
            triggerPassTime = 0;

            // 无论是否概率通过 都应该重置监听维护的参数
            ResetConditons();
            ResetParams();

            //单次触发的，触发完成后移除
            if (handled && handlerFrequence == TriggerHandleFrequence.Once)
            {
                owner?.TriggerMng?.RemoveTrigger(this);
            }
        }

        protected bool CheckCondition()
        {
            if(conditionList.Count == 0)
            {
                return true;
            }
            if (conditionCombine == TriggerConditionCombine.And)
            {
                foreach (var triggerCondition in conditionList)
                {
                    if (!triggerCondition.Check())
                    {
                        return false;
                    }
                }
                return true;
            }
            if (conditionCombine == TriggerConditionCombine.Or)
            {
                foreach (var triggerCondition in conditionList)
                {
                    if (triggerCondition.Check())
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        protected virtual bool ProbabilityHappened()
        {
            if (Model.Probability.Value >= 10000)
            {
                return true;
            }
            return RAND.Range(1, 10000) <= (int)Model.Probability.Value;
        }

        public virtual int CalcParam(TriggerHandlerType handlerType, int param)
        {
            return param;
        }

        public virtual int CalcParam(TriggerCondition condition,int param)
        {
            return param;
        }

        public virtual float CalcParam(TriggerHandlerType handlerType, KeyValuePair<float, float> growth, int level)
        {
            return growth.Key * level + growth.Value;
        }

        public virtual float CalcParam(float growth, float baseValue, float level)
        {
            return growth * level + baseValue;
        }

        public void Update(float dt)
        {
            if (triggerPassTime < triggerTime)
            {
                triggerPassTime += dt;
            }
            if (state != TriggerState.Start)
            {
                return;
            }
            foreach (var condition in conditionList)
            {
                condition.Update(dt);
            }
        }

        protected virtual void Handle()
        {
            handled = true;
            foreach (var handler in handlerList)
            {
                handler.Handle();
                Logger.Log.Debug($"caster {caster?.InstanceId} trigger handler {handler.handlerType} owner {handler.Owner?.InstanceId}");
            }
        }

        private void ResetConditons()
        {
            foreach (var conditon in conditionList)
            {
                conditon.Reset();
            }
        }

        public void RemoveListener()
        {
            listener.RemoveListener();
        }

        public void Reset()
        {
            listener?.RemoveListener();
            ResetParams();
            ResetConditons();
        }
    }
}

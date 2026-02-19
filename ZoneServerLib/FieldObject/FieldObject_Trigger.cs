using CommonUtility;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        protected MessageDispatcher messageDispatcher;

        protected TriggerManager triggerManager;
        public TriggerManager TriggerMng
        { get { return triggerManager; } }
        
        public void InitTrigger()
        {
            // 复活时 triggerManager 和 messageDispatch 不为null 
            if(triggerManager == null)
            {
                triggerManager = new TriggerManager(this);
            }

            if(messageDispatcher != null)
            {
                messageDispatcher.Start();
            }
            else
            {
                messageDispatcher = new MessageDispatcher();
            }
        }

        public void AddTrigger(BaseTrigger trigger)
        {
            Logger.Log.Debug($"{instanceId} Hero {GetHeroId()} add trigger {trigger.Model?.Id} ");
            triggerManager?.Add(trigger);
        }

        public void AddPetTrigger(BaseTrigger trigger)
        {
            Logger.Log.Debug($"{instanceId} pet {GetPetId()} add trigger {trigger.Model?.Id} ");
            triggerManager?.Add(trigger);
        }

        public void RemoveTrigger(BaseTrigger trigger)
        {
            if (trigger == null) return;
            triggerManager.RemoveTrigger(trigger);
        }
        
        public bool RemoveTrigger(int trigger)
        {
            return triggerManager.RemoveTrigger(trigger);
        }

        public bool SubcribedMessage(TriggerMessageType message)
        {
            return messageDispatcher?.Subscribed(message) == true;
        }


        public void DispatchMessage(TriggerMessageType message, object param)
        {
            messageDispatcher?.Dispatch(message, param);
        }

        // 注意 调用此接口需要判断是否为null
        public MessageDispatcher GetDispatcher()
        {
            return messageDispatcher;
        }
    }
}

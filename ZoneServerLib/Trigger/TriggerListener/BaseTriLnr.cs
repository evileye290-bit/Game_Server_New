using CommonUtility;

namespace ZoneServerLib
{
    public class BaseTriLnr
    {
        protected BaseTrigger trigger;
        public FieldObject Owner
        { get { return trigger.Owner; } }

        protected TriggerMessageType messageType = TriggerMessageType.None;

        public BaseTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
        {
            this.trigger = trigger;
            this.messageType = messageType;
            if (messageType != TriggerMessageType.None)
            {
                AddListener();
            }
        }

        public void OnMessage(object message)
        {
            ParseMessage(message);
            trigger.TryHandle();
        }

        // 不同listener将message解析到特定对象，并将必要的数据保存到trigger的paramList中
        // 用于后续condition与handler
        protected virtual void ParseMessage(object message)
        {
        }

        protected void AddListener()
        {
            MessageDispatcher dispatcher = trigger.GetMessageDispatcher();
            if(dispatcher != null)
            {
                dispatcher.AddListener(messageType, OnMessage);
            }
        }

        public void RemoveListener()
        {
            MessageDispatcher dispatcher = trigger.GetMessageDispatcher();
            if (dispatcher != null)
            {
                dispatcher.RemoveListener(messageType, OnMessage);
            }
        }
    }
}

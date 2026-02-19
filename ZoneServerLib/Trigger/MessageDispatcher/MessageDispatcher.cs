using CommonUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class MessageListener
    {
        public MessageListener() { }

        public event Action<object> OnEvent;

        public void Excute(object message)
        {
            if (OnEvent != null)
            {
                OnEvent(message);
            }
        }
    }

    public class MessageDispatcher
    {
        private bool started = true;
        private Dictionary<TriggerMessageType, MessageListener> listenerList = new Dictionary<TriggerMessageType, MessageListener>();

        public void AddListener(TriggerMessageType messageType, Action<object> callback)
        {
            if (!listenerList.ContainsKey(messageType))
            {
                listenerList.Add(messageType, new MessageListener());
            }
            listenerList[messageType].OnEvent += callback;
        }

        public void RemoveListener(TriggerMessageType messageType, Action<object> callback)
        {
            if (listenerList.ContainsKey(messageType))
            {
                listenerList[messageType].OnEvent -= callback;
            }
        }

        public void Dispatch(TriggerMessageType messageType, object eventParams = null)
        {
            if (!started) return;
            MessageListener listener;
            if (listenerList.TryGetValue(messageType, out listener))
            {
                listener.Excute(eventParams);
            }
        }

        // 是否已订阅该消息
        public bool Subscribed(TriggerMessageType messageType)
        {
            if (!started) return false;
            return listenerList.ContainsKey(messageType);
        }

        public void ClearListener()
        {
            listenerList.Clear();
        }

        public void Start()
        {
            started = true;
        }

        public void Stop()
        {
            started = false;
            ClearListener();
        }
    }
}

using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap
    {
        protected List<TriggerInMap> triggerList = new List<TriggerInMap>();

        protected void InitTriggers()
        {
            foreach (var item in DungeonModel.Triggers)
            {
                TriggerInMap trigger = new TriggerInMap(this, item);
                triggerList.Add(trigger);
            }
        }

        public void UpdateTriggers(float dt)
        {
            //副本结束清空trigger
            if (State > DungeonState.Started)
            {
                ClearTriggers();
                return;
            }

            foreach (var trigger in triggerList)
            {
                try
                {
                    trigger.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        private void ClearTriggers()
        {
            if (triggerList.Count >= 0)
            {
                messageDispatcher.ClearListener();
                triggerList.Clear();
            }
        }

        #region 桥接 Messagelistener

        private ListMap<TriggerMessageType, KeyValuePair<FieldObject, bool>> bridgeMessageList = new ListMap<TriggerMessageType, KeyValuePair<FieldObject, bool>>();

        //某些trigger不能直接调用fieldobject的triggerlistener
        //譬如：
        //AllySkillTypeStart = 71,   // 友方某个类型技能开始
        //AllyCastTypeBuff = 72,   // 友方释放了某个类型的buff
        //这两个不能直接调用fieldobject的messageListener，GetAllyInSplash方法的调用特别频繁，而大多数都是无效调用，
        //通过把该类型通知注册到map上，来检测当前是否有fieldobject是否订阅改类型消息，从而过滤99%的无效通知
        public void AddBridgeTriggerMessageListener(TriggerMessageType messageType, FieldObject field, bool includeSelf = false)
        {
            List<KeyValuePair<FieldObject, bool>> fieldObjects;
            if (bridgeMessageList.TryGetValue(messageType, out fieldObjects)) 
            {
                FieldObject fieldObject = fieldObjects.FirstOrDefault(x => x.Key == field).Key;
                if (fieldObject != null) return;
            }

            bridgeMessageList.Add(messageType, new KeyValuePair<FieldObject, bool>(field, includeSelf));
        }

        public void DispatchBridgeTriggerMessage(FieldObject caster, TriggerMessageType messageType, object param)
        {
            List<KeyValuePair<FieldObject, bool>> fieldObjects;
            if (!bridgeMessageList.TryGetValue(messageType, out fieldObjects)) return;

            foreach (var kv in fieldObjects)
            {
                //不能由敌方触发
                if (!caster.IsAlly(kv.Key)) continue;

                //是否排除自己
                if (kv.Value  && caster.InstanceId == kv.Key.InstanceId) continue;

                if (kv.Key.SubcribedMessage(messageType))
                {
                    kv.Key.DispatchMessage(messageType, param);
                }
            }
        }

        #endregion
    }
}

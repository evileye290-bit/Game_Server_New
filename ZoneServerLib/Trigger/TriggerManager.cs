using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TriggerManager
    {
        private FieldObject owner;
        private List<BaseTrigger> triggerList;
		// key instanceId
        private Dictionary<int, List<BaseTrigger>> triggersFromOther;
        private List<BaseTrigger> removeTriggerList;
        private List<BaseTrigger> addTriggerList;
        public TriggerManager(FieldObject owner)
        {
            this.owner = owner;
            triggerList = new List<BaseTrigger>();
            removeTriggerList = new List<BaseTrigger>();
            addTriggerList = new List<BaseTrigger>();
            triggersFromOther = new Dictionary<int, List<BaseTrigger>>();
        }

        public void Update(float dt)
        {
            if (addTriggerList.Count > 0)
            {
                foreach (var trigger in addTriggerList)
                {
                    triggerList.Add(trigger);
                }
                addTriggerList.Clear();
            }
            foreach (var trigger in triggerList)
            {
                trigger.Update(dt);
            }
            foreach (var kv in triggersFromOther)
            {
                foreach(var trigger in kv.Value)
                {
                    trigger.Update(dt);
                }
            }
            if (removeTriggerList.Count > 0)
            {
                foreach (var trigger in removeTriggerList)
                {
                    trigger.Reset();
                    triggerList.Remove(trigger);
                }
                removeTriggerList.Clear();
            }
        }

        public void Add(BaseTrigger trigger)
        {
            Log.Debug($"{owner.GetHeroId()} add trigger {trigger.Model?.Id} own by {trigger.Owner?.GetHeroId()} ");
            addTriggerList.Add(trigger);
        }

        public void RemoveTrigger(BaseTrigger trigger)
        {
            //triggerList.Remove(trigger);
            removeTriggerList.Add(trigger);
        }

        public bool RemoveTrigger(int triggerId)
        {
            BaseTrigger trigger = FindTrigger(triggerId);
            
            if(trigger == null) return false;
            
            RemoveTrigger(trigger);

            return true;
        }

        public BaseTrigger FindTrigger(int triggerId)
        {
            BaseTrigger trigger = triggerList.Where(x => x.Model.Id == triggerId).FirstOrDefault();
            if (trigger == null)
            {
                trigger = addTriggerList.Where(x => x.Model.Id == triggerId).FirstOrDefault();
            }
            return trigger;
        }

        public void ClearSelfTriggers()
        {
            foreach (var trigger in triggerList)
            {
                trigger.Reset();
            }
            triggerList.Clear();
        }

        public void AddTriggersFromOther(int instanceId, BaseTrigger trigger)
        {
            List<BaseTrigger> list;
            if(!triggersFromOther.TryGetValue(instanceId, out list))
            {
                list = new List<BaseTrigger>();
            }
            list.Add(trigger);
        }

        public void RemoveTriggersFromOther(int instanceId)
        {
            List<BaseTrigger> list;
            if(triggersFromOther.TryGetValue(instanceId, out list))
            {
                foreach (var trigger in list)
                {
                    trigger.Reset();
                }
                triggersFromOther.Remove(instanceId);
            }
        }

        public void StopTriggersFromOther()
        {
            foreach (var kv in triggersFromOther)
            {
                foreach(var triggers in kv.Value)
                {
                    triggers.Stop();
                }
            }
            triggersFromOther.Clear();
        }


        public int GetTriggerCount() => triggerList.Count + addTriggerList.Count;
    }
}

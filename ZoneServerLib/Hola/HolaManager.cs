using CommonUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class HolaManager
    {
        //光环目前只通过buff来实现，如果由其他需求则
        //光环生效的fieldobject <instanceId,List<holaId>>
        private Dictionary<int, List<int>> fieldHolaList = new Dictionary<int, List<int>>();

        public FieldObject Owner { get; private set; }

        public HolaManager(FieldObject owner)
        {
            Owner = owner;

            //侦听死亡消息
            MessageDispatcher dispatcher = owner.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.AddListener(TriggerMessageType.Dead, OnDead);
            }
        }

        public void HolaEffect(FieldObject target, int buffId, int skillLevel)
        {
            List<int> holaList;
            if (!fieldHolaList.TryGetValue(target.InstanceId, out holaList))
            {
                holaList = new List<int>();
                fieldHolaList.Add(target.InstanceId, holaList);
            }

            if (!holaList.Contains(buffId))
            {
                holaList.Add(buffId);

                AddBuff(target, buffId, skillLevel);
            }
        }

        public bool HadAddedHold(FieldObject target, int holaId)
        {
            return fieldHolaList.ContainsKey(holaId) && fieldHolaList[holaId].Contains(target.InstanceId);
        }

        private void AddBuff(FieldObject target, int buffId, int skillLevel)
        {
            target.AddBuff(Owner, buffId, skillLevel);
        }

        /// <summary>
        /// 死亡清除所有光环
        /// </summary>
        /// <param name="param"></param>
        private void OnDead(Object param)
        {
            FieldObject dead = param as FieldObject;
            DungeonMap dungeon = Owner.CurrentMap as DungeonMap;

            if (dungeon == null || dead == null || dead.InstanceId != Owner.InstanceId) return;

            //移除地图中自己的光环效果
            dungeon.RemoveFieldHola(Owner.InstanceId);

            foreach (var kv in fieldHolaList)
            {
                FieldObject field = Owner.CurrentMap.GetFieldObject(kv.Key);
                if (field == null) continue;
                if (field.BuffManager == null) continue;
                kv.Value.ForEach(x => field.BuffManager.RemoveBuffsById(x));
            }
            fieldHolaList.Clear();
        }
    }
}

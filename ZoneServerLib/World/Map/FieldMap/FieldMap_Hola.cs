using CommonUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class HolaInfo
    {
        public int BuffId{ get; set; }
        public int SkillLevel{ get; set; }
    }

    public partial class FieldMap
    {
        private Dictionary<int, Dictionary<int, HolaInfo>> fieldHolaList = new Dictionary<int, Dictionary<int, HolaInfo>>();//<instanceId, holaId>

        public void RemoveFieldHola(int instanceId)
        {
            fieldHolaList.Remove(instanceId);
        }

        public void AddHola(FieldObject caster, int holaId, int skillLevel)
        {
            Dictionary<int, HolaInfo> holas;
            if (!fieldHolaList.TryGetValue(caster.InstanceId, out holas))
            {
                holas = new Dictionary<int, HolaInfo>();
                fieldHolaList.Add(caster.InstanceId, holas);
            }

            if (!holas.ContainsKey(holaId))
            {
                HolaInfo holaInfo = new HolaInfo() { BuffId = holaId, SkillLevel = skillLevel };
                holas.Add(holaId, holaInfo);

                HolaEffect(caster, holaInfo);
            }
        }

        public void HolaEffect(FieldObject caster, HolaInfo hola)
        {
            foreach (var kv in allObjectList)
            {
               FieldObject field = GetFieldObject(kv.Key);

                if (caster.IsEnemy(field)) continue;

                caster.HolaManager.HolaEffect(field, hola.BuffId, hola.SkillLevel);
            }
        }

        //新进入地图的玩家添加光环buff
        public void HolaEffect(FieldObject target)
        {
            List<int> remove = new List<int>();

            foreach (var field in fieldHolaList)
            {
                FieldObject fieldObject = GetFieldObject(field.Key);
                if (fieldObject == null || fieldObject.IsDead)
                {
                    remove.Add(field.Key);
                    continue;
                }

                if (fieldObject.IsEnemy(target)) continue;

                foreach (var kv in field.Value)
                {
                    fieldObject.HolaManager.HolaEffect(target, kv.Value.BuffId, kv.Value.SkillLevel);
                }
            }

            remove.ForEach(x => fieldHolaList.Remove(x));
        }
    }
}

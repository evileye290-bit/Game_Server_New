using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    //public class EquipmentSuit
    //{
    //    private Dictionary<int, ulong> partAndGuid = new Dictionary<int, ulong>();

    //    private Dictionary<ulong, Equipment> equipments = new Dictionary<ulong, Equipment>();

    //    public int EquipedCount { get; private set; }

    //    public bool Load(Equipment equip)
    //    {
    //        if (partAndGuid.ContainsKey(equip.Part) || equipments.ContainsKey(equip.Uid))
    //        {
    //            return false;
    //        }
    //        partAndGuid.Add(equip.Part, equip.Uid);
    //        equipments.Add(equip.Uid, equip);
    //        EquipedCount++;
    //        return true;
    //    }

    //    public bool Contain(ulong uid)
    //    {
    //        return equipments.ContainsKey(uid);
    //    }

    //    public bool ContainPart(int part)
    //    {
    //        return partAndGuid.ContainsKey(part);
    //    }

    //    public Equipment Unload(int type)
    //    {
    //        ulong uid = 0;
    //        Equipment equip = null;
    //        if (partAndGuid.TryGetValue(type, out uid) && equipments.TryGetValue(uid, out equip))
    //        {
    //            partAndGuid.Remove(type);
    //            equipments.Remove(uid);
    //            EquipedCount--;
    //            equip.EquipedHeroId = -1;
    //        }
    //        return equip;
    //    }

    //    public List<Equipment> GetEquipmentsClone()
    //    {
    //        List<Equipment> equips = new List<Equipment>();
    //        foreach(var kv in equipments)
    //        {
    //            equips.Add(kv.Value.Clone());
    //        }
    //        return equips;
    //    }


    //}
}

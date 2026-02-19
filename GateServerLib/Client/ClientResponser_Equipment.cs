using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_EquipEquipment(MemoryStream stream)
        {
            MSG_CG_EQUIP_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_EQUIPMENT>(stream);
            MSG_GateZ_EQUIP_EQUIPMENT request = new MSG_GateZ_EQUIP_EQUIPMENT();
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        private void OnResponse_UpgradeSlot(MemoryStream stream)
        {
            MSG_CG_UPGRADE_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPGRADE_EQUIPMENT>(stream);
            MSG_GateZ_UPGRADE_EQUIPMENT request = new MSG_GateZ_UPGRADE_EQUIPMENT();
            request.Slot = msg.Slot;
            request.HeroId = msg.HeroId;
            request.CrackChecked = msg.CrackChecked;
            request.IsAdvanced = msg.IsAdvanced;
            foreach (var item in msg.Items)
            {
                request.Items.Add(new MSG_GateZ_ITEM_USE()
                {
                    Uid = ExtendClass.GetUInt64(item.UidHigh, item.UidLow),
                    Num = item.Num,
                });
            }
            WriteToZone(request);
        }
        
        private void OnResponse_UpgradeSlotDirectly(MemoryStream stream)
        {
            MSG_CG_UPGRADE_EQUIPMENT_DIRECTLY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPGRADE_EQUIPMENT_DIRECTLY>(stream);
            MSG_GateZ_UPGRADE_EQUIPMENT_DIRECTLY request = new MSG_GateZ_UPGRADE_EQUIPMENT_DIRECTLY();
            request.Slot = msg.Slot;
            request.HeroId = msg.HeroId;
            request.ItemUid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(request);
        }

        private void OnResponse_InjectEquipment(MemoryStream stream)
        {
            MSG_CG_INJECTION_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_INJECTION_EQUIPMENT>(stream);
            MSG_GateZ_INJECTION_EQUIPMENT request = new MSG_GateZ_INJECTION_EQUIPMENT();
            request.HeroId = msg.HeroId;
            request.Slot = msg.Slot;
            foreach(var item in msg.InjectionSlots)
            {
                request.InjectionSlots.Add(item);
            }
            request.Jewel = new MSG_GateZ_ITEM_USE();
            request.Jewel.Uid = ExtendClass.GetUInt64(msg.Jewel.UidHigh, msg.Jewel.UidLow);
            WriteToZone(request);
        }

        private void OnResponse_EquipBetterEquipment(MemoryStream stream)
        {
            MSG_CG_EQUIP_BETTER_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIP_BETTER_EQUIPMENT>(stream);
            MSG_GateZ_EQUIP_BETTER_EQUIPMENT request = new MSG_GateZ_EQUIP_BETTER_EQUIPMENT();
            request.HeroId = msg.HeroId;
            foreach (var item in msg.Equipments)
            {
                request.Uids.Add(ExtendClass.GetUInt64(item.UidHigh, item.UidLow));
            }
            WriteToZone(request);
        }

        private void OnResponse_ReturnUpgradeEquipCost(MemoryStream stream)
        {
            MSG_CG_RETURN_UPGRADE_EQUIPMENT_COST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RETURN_UPGRADE_EQUIPMENT_COST>(stream);
            MSG_GateZ_RETURN_UPGRADE_EQUIPMENT_COST request = new MSG_GateZ_RETURN_UPGRADE_EQUIPMENT_COST();
            request.Num = msg.Num;
            WriteToZone(request);
        }

        private void OnResponse_EquipmentAdvance(MemoryStream stream)
        {
            MSG_CG_EQUIPMENT_ADVANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIPMENT_ADVANCE>(stream);
            MSG_GateZ_EQUIPMENT_ADVANCE request = new MSG_GateZ_EQUIPMENT_ADVANCE
            {
                HeroId = msg.HeroId, Slot = msg.Slot
            };
            foreach (var item in msg.Equipments)
            {
                request.Uids.Add(ExtendClass.GetUInt64(item.UidHigh, item.UidLow));
            }
            WriteToZone(request);
        }

        private void OnResponse_EquipmentAdvanceOneKey(MemoryStream stream)
        {
            MSG_CG_EQUIPMENT_ADVANCE_ONE_KEY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIPMENT_ADVANCE_ONE_KEY>(stream);
            MSG_GateZ_EQUIPMENT_ADVANCE_ONE_KEY request = new MSG_GateZ_EQUIPMENT_ADVANCE_ONE_KEY();
            request.AdvanceOnce = msg.AdvanceOnce;
            request.CostIdList.Add(msg.CostIdList);
            WriteToZone(request);
        }

        private void OnResponse_JewelAdvance(MemoryStream stream)
        {
            MSG_CG_JEWEL_ADVANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_JEWEL_ADVANCE>(stream);
            MSG_GateZ_JEWEL_ADVANCE request = new MSG_GateZ_JEWEL_ADVANCE(){Id =  msg.Id, Num = msg.Num};
            WriteToZone(request);
        }

        private void OnResponse_EquipmentEnchant(MemoryStream stream)
        {
            MSG_CG_EQUIPMENT_ENCHANT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EQUIPMENT_ENCHANT>(stream);
            MSG_GateZ_EQUIPMENT_ENCHANT request = new MSG_GateZ_EQUIPMENT_ENCHANT() { EquipUid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow), ItemId = msg.ItemId};
            WriteToZone(request);
        }
    }
}

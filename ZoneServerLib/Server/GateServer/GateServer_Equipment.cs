using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_EquipEquipment(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIP_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIP_EQUIPMENT>(stream);
            Log.Write("player {0} equip equipment uid {1} hero {2}", uid, msg.Uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipEquipment(msg.Uid, msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("equip equipment fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("equip equipment fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_UpgradeSlot(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_UPGRADE_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPGRADE_EQUIPMENT>(stream);
            Log.Write("player {0} upgrade slot {1} hero {2}", uid, msg.Slot, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpgradeEquipment(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("upgrade equip slot fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("upgrade equip slot fail, can not find player {0} .", uid);
                }
            }
        }
        
        private void OnResponse_UpgradeSlotDirectly(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_UPGRADE_EQUIPMENT_DIRECTLY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPGRADE_EQUIPMENT_DIRECTLY>(stream);
            Log.Write("player {0} upgrade slot directly {1} hero {2}", uid, msg.Slot, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpgradeEquipmentDirectly(msg.HeroId, msg.Slot, msg.ItemUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("upgrade equip slot fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("upgrade equip slot fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_InjectEquipment(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_INJECTION_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_INJECTION_EQUIPMENT>(stream);
            Log.Write("player {0} inject equipment hero {1} slot {2}", uid, msg.HeroId, msg.Slot);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.InjectEquipment(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("inject equipment fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("inject equipment fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_EquipBetterEquipment(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIP_BETTER_EQUIPMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIP_BETTER_EQUIPMENT>(stream);
            Log.Write("player {0} equip better equipment to hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipBetterEquipments(msg.HeroId, msg.Uids);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("equip better equipment fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("equip better equipment fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_ReturnUpgradeCost(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RETURN_UPGRADE_EQUIPMENT_COST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RETURN_UPGRADE_EQUIPMENT_COST>(stream);
            Log.Write("player {0} return upgrade cost ", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ReturnUpgradeEquipmentCost(msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("return upgrade equip cost fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("return upgrade equip cost fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_EquipmentAdvance(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIPMENT_ADVANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIPMENT_ADVANCE>(stream);
            Log.Write("player {0} EquipmentAdvance to hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipmentAdvance(msg.HeroId, msg.Slot, msg.Uids);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("EquipmentAdvance fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("EquipmentAdvance fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_EquipmentAdvanceOneKey(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIPMENT_ADVANCE_ONE_KEY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIPMENT_ADVANCE_ONE_KEY>(stream);
            Log.Write("player {0} EquipmentAdvanceOneKey", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipmentAdvanceOneKey(msg.CostIdList, msg.AdvanceOnce == 1);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("EquipmentAdvanceOneKey fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("EquipmentAdvanceOneKey fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_JewelAdvance(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_JEWEL_ADVANCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_JEWEL_ADVANCE>(stream);
            Log.Write("player {0} OnResponse_JewelAdvance", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.JewelAdvance(msg.Id, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("OnResponse_JewelAdvance fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("OnResponse_JewelAdvance fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_EquipmentEnchant(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIPMENT_ENCHANT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIPMENT_ENCHANT>(stream);
            Log.Write("player {0} EquipmentEnchant", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipmentEnchant(msg.EquipUid, msg.ItemId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("EquipmentEnchant fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("EquipmentEnchant fail, can not find player {0} .", uid);
                }
            }
        }

    }
}

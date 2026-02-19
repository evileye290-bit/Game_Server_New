using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_EquipEquipmentResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIP_EQUIPMENT_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} equip equipment not find client", pcUid);
            }
        }

        private void OnResponse_UpgradeEquipmentResult(MemoryStream stream,int pcUid)
        {
            //MSG_ZGC_UPGRADE_EQUIPMENT_RESULT
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPGRADE_EQUIPMENT_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} upgrade equipment not find client", pcUid);
            }
        }
        
        private void OnResponse_UpgradeSlotDirectly(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPGRADE_EQUIPMENT_DIRECTLY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} upgrade UpgradeSlotDirectly not find client", pcUid);
            }
        }

        private void OnResponse_InjectEquipment(MemoryStream stream,int pcUid)
        {
            //MSG_ZGC_EQUIPMENT_INJECTION_RESULT
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIPMENT_INJECTION_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} inject equipment not find client", pcUid);
            }
        }

        private void OnResponse_EquipBetterEquipment(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIP_BETTER_EQUIPMENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} equip better equipment not find client", pcUid);
            }
        }

        private void OnResponse_ReturnUpgradeEquipmentCost(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_RETURN_UPGRADE_EQUIPMENT_COST
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RETURN_UPGRADE_EQUIPMENT_COST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} return upgrade equipment cost not find client", pcUid);
            }
        }

        private void OnResponse_EquipmentAdvance(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIPMENT_ADVANCE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} EquipmentAdvance not find client", pcUid);
            }
        }

        private void OnResponse_EquipmentAdvanceOneKey(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIPMENT_ADVANCE_ONE_KEY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} EquipmentAdvanceOneKey not find client", pcUid);
            }
        }

        private void OnResponse_JewelAdvance(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_JEWEL_ADVANCE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} JewelAdvance not find client", pcUid);
            }
        }

        private void OnResponse_EquipEnchant(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIPMENT_ENCHANT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} EquipEnchant not find client", pcUid);
            }
        }
        
    }
}

using System.IO;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;


namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_HiddenWeaponEquip(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_EQUIP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponEquip not find client", pcUid);
            }
        }

        private void OnResponse_HiddenWeaponUpgrade(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_UPGRADE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponUpgrade not find client", pcUid);
            }
        }

        private void OnResponse_HiddenWeaponStar(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_STAR>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponStar not find client", pcUid);
            }
        }

        private void OnResponse_HiddenWeaponWash(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_WASH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponWash not find client", pcUid);
            }
        }

        private void OnResponse_HiddenWeaponWashConfirm(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_WASH_CONFIRM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponWashConfirm not find client", pcUid);
            }
        }

        private void OnResponse_HiddenWeaponSmash(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDENWEAPON_SMASH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HiddenWeaponSmash not find client", pcUid);
            }
        }
    }
}

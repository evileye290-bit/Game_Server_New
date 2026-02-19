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
        private void OnResponse_GetHeroNature(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_NATURE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get hero nature not find client", uid);
            }
        }

        private void OnResponse_SendBattlePower(MemoryStream stream,int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_BATTLEPOWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} send hero battlePower not find client", uid);
            }
        }

        private void OnResponse_GetHeroPower(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HERO_POWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get hero power not find client", uid);
            }
        }
    }
}

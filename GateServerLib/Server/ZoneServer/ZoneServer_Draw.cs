using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_DrawHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAW_HERO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} draw hero not find client", pcUid);
            }
        }

        private void OnResponse_ActivateHeroCombo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVATE_HERO_COMBO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} activate hero combo not find client", pcUid);
            }
        }

        private void OnResponse_DrawManager(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAW_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} draw manager not find client", pcUid);
            }
        }
    }
}

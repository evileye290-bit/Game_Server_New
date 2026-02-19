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
        private void OnResponse_GetThemeFireworkInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_FIREWORK_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemeFireworkInfo not find client", pcUid);
            }
        }

        private void OnResponse_ThemeFireworkReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_FIREWORK_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ThemeFireworkReward not find client", pcUid);
            }
        }

        private void OnResponse_UseThemeFirework(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_USE_THEME_FIREWORK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UseThemeFirework not find client", pcUid);
            }
        }

        private void OnResponse_GetThemeFireworkScoreReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_FIREWORK_SCORE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemeFireworkScoreReward not find client", pcUid);
            }
        }

        private void OnResponse_GetThemeFireworkUseCountReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_FIREWORK_USECOUNT_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemeFireworkUseCountReward not find client", pcUid);
            }
        }
    }
}

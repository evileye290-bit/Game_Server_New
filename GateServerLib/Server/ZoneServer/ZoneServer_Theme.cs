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
        private void OnResponse_GetThemePassList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_PASS_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemePassList not find client", pcUid);
            }
        }

        private void OnResponse_GetThemePassReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_THEMEPASS_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemePassReward not find client", pcUid);
            }
        }

        private void OnResponse_BuyThemePassResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_THEMEPASS_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyThemePassResult not find client", pcUid);
            }
        }

        private void OnResponse_ThemePassExpChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEMEPASS_EXP_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ThemePassExpChange not find client", pcUid);
            }
        }

        private void OnResponse_ThemeBossInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEME_BOSS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ThemeBossInfo not find client", pcUid);
            }
        }

        private void OnResponse_ThemeBossDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEMEBOSS_DUNGEON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ThemeBossDungeon not find client", pcUid);
            }
        }

        private void OnResponse_GetThemeBossReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_THEMEBOSS_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetThemeBossReward not find client", pcUid);
            }
        }

        private void OnResponse_UpdateThemeBossQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UpdateThemeBossQueue not find client", pcUid);
            }
        }
    }
}

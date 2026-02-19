using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_AbsorbSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ABSORB_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} absorb soulring not find client", pcUid);
            }
        }

        private void OnResponse_HelpAbsorbSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HELP_ABSORB_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} help absorb soulring not find client", pcUid);
            }
        }

        private void OnResponse_AbsorbInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_ABSORBINFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get soulring absorb info not find client", pcUid);
            }
        }

        private void OnResponse_CancelAbsorb(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CANCEL_ABSORB>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cancel soulring absorb not find client", pcUid);
            }
        }

        private void OnResponse_FinishAbsorb(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ABSORB_FINISH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0}  finish soulring absorb not find client", pcUid);
            }
        }


        private void OnResponse_GetHelpThanksList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HELP_THANKS_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0}  get soulring absorb help thanks list not find client", pcUid);
            }
        }


        private void OnResponse_ThankFriend(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_THANK_FRIEND>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} thank friend not find client", pcUid);
            }
        }


        private void OnResponse_EnhanceSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ENHANCE_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} enhance soulring not find client", pcUid);
            }
        }

        private void OnResponse_OneKeyEnhanceSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONEKEY_ENHANCE_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} one key enhance soulring not find client", pcUid);
            }
        }
        

        private void OnResponse_GetAllAbsorbInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_All_ABSORBINFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get all soulring absorb infos not find client", pcUid);
            }
        }

        private void OnResponse_GetAbsorbFriendInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_FRIEND_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get soulring absorb helpers info not find client", pcUid);
            }
        }

        private void OnResponse_ShowHeroSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_HERO_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show hero soulring not find client", pcUid);
            }
        }

        private void OnResponse_ReplaceBetterSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REPLACE_BETTER_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} replace better soulring not find client", pcUid);
            }
        }
        
        private void OnResponse_SelectSoulRingElement(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SELECT_SOULRING_ELEMENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SelectSoulRingElement not find client", pcUid);
            }
        }
    }
}

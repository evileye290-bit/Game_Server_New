using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SpaceInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<SHOW_SPACEINFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} space info not find client", pcUid);
            }
        }

        private void OnResponse_ShowPlayer(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_PLAYER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show player info not find client", pcUid);
            }
        }

        private void OnResponse_NotifyPlayerShow(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NOTIFY_PLAYER_SHOW>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} notify player show info not find client", pcUid);
            }
        }

        private void OnResponse_UpdateSomeShow(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_SOME_SHOW>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update some show not find client", pcUid);
            }
        }


        private void OnResponse_ShowFaceIcon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_FACEICON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show face icon not find client", pcUid);
            }
        }

        private void OnResponse_ShowFaceJpg(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_FACEJPG>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show face jpg not find client", pcUid);
            }
        }


        private void OnResponse_ChangeName(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_NAME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} change name not find client", pcUid);
            }
        }

        private void OnResponse_SetSex(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SET_SEX>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} set sex not find client", pcUid);
            }
        }


        private void OnResponse_SetBirthday(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SET_BIRTHDAY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} set birthday not find client", pcUid);
            }
        }

        private void OnResponse_SetSignature(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SET_SIGNATURE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} set signatur not find client", pcUid);
            }
        }


        private void OnResponse_SetWQ(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SET_SOCIAL_NUM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} set WQ not find client", pcUid);
            }
        }

        private void OnResponse_GetWQ(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SOCIAL_NUM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get WQ not find client", pcUid);
            }
        }

        private void OnResponse_ShowVoice(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_VOICE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} set showvoice not find client", pcUid);
            }
        }


        private void OnResponse_PresentGift(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PRESENT_GIFT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} present flower not find client", pcUid);
            }
        }

        private void OnResponse_GetGiftRecord(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_GIFTRECORD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get gift record not find client", pcUid);
            }
        }

        private void OnResponse_ShowCareer(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_CAREER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show career not find client", pcUid);
            }
        }

        private void OnResponse_GetRankingFriendList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RANKING_FRIEND_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ranking friend list not find client", pcUid);
            }
        }

        private void OnResponse_GetRankingAllList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RANKING_ALL_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ranking all list not find client", pcUid);
            }
        }

        private void OnResponse_GetRankingNearbyList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RANKING_NEARBY_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ranking nearby list not find client", pcUid);
            }
        }
    }
}

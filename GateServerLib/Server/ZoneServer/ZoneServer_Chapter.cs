using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_ChapterInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ChapterInfo words not find client", pcUid);
            }
        }

        private void OnResponse_ChapterReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ChapterReward words not find client", pcUid);
            }
        }

        private void OnResponse_ChapterSweep(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_SWEEP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ChapterSweep words not find client", pcUid);
            }
        }

        private void OnResponse_BuyTimeSpacePower(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_BUY_POWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyTimeSpacePower words not find client", pcUid);
            }
        }

        private void OnResponse_ChapterNextPage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_NEXT_PAGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ChapterNextPage words not find client", pcUid);
            }
        }

        private void OnResponse_ChapterRewardReddot(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHAPTER_REWATRD_REDDOT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ChapterRewardReddot words not find client", pcUid);
            }
        }
    }
}

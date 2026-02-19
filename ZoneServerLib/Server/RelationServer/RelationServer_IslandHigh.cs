using Logger;
using Message.Relation.Protocol.RZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_GetIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_ISLAND_HIGH_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_ISLAND_HIGH_INFO>(stream);
            Log.Write($"player {uid} GetIslandHighInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SendIslandHighInfo(pks.RankValue, pks.LastRankValue);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("GetIslandHighInfo, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("GetIslandHighInfo, can not find player {0} .", uid);
                }
            }
        }
    }
}

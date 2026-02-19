using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RZ;
using System.IO;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        private void OnResponse_GetIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CrossR_GET_ISLAND_HIGH_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CrossR_GET_ISLAND_HIGH_INFO>(stream);
            Log.Write($"player {uid} GetIslandHighInfo from main {MainId} ");

            MSG_RZ_GET_ISLAND_HIGH_INFO msg = new MSG_RZ_GET_ISLAND_HIGH_INFO()
            {
                RankValue = pks.RankValue,
                LastRankValue = pks.LastRankValue
            };

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetIslandHighInfo not find client ");
            }
        }
    }
}

using Message.Relation.Protocol.RC;
using Message.Zone.Protocol.ZR;
using System.IO;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GetIslandHighInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_ISLAND_HIGH_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_ISLAND_HIGH_INFO>(stream);

            Api.WriteToCross(new MSG_RC_GET_ISLAND_HIGH_INFO() {Uid = pks.Uid}, uid);
        }
    }
}

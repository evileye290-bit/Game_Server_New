using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_MaxGuildId(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_MAX_GUILDID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_MAX_GUILDID>(stream);
            MSG_RZ_MAX_GUILDID response = new MSG_RZ_MAX_GUILDID();
            response.MaxGuildId = ++Api.MaxGuildUid;
            response.Result = (int)ErrorCode.Success;
            Write(response);

        }


    }
}

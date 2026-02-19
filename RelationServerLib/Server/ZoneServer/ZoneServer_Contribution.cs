using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_ContributionInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CONTRIBUTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CONTRIBUTION_INFO>(stream);
            Api.ContributionMng.SendContributionInfo(false);  
        }
    }
}

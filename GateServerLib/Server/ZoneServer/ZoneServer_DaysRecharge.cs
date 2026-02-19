using System.IO;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    partial class ZoneServer
    {
        private void OnResponse_DaysRechargeInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DAYS_RECHARGE_INFO>.Value, stream);
            }
        }
    }
}

using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using System.IO;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        //public void OnResponse_GetSecretAreaRank(MemoryStream stream, int uid = 0)
        //{
        //    MSG_ZR_SECRET_AREA_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SECRET_AREA_RANK_LIST>(stream);
        //    Client client = ZoneManager.GetClient(uid);
        //    if (client != null)
        //    {
        //        if (msg.RankType > 0)
        //        {
        //            MSG_RZ_SECRET_AREA_RANK_LIST info = Api.SecretAreaMng.GetRankList(msg.RankType, msg.Page);
        //            client.CurZone.Write(info, uid);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn($"player {uid} try get secret area rank  failed with ranktype {msg.RankType}");
        //        }
        //    }
        //    else
        //    {
        //        Logger.Log.Warn($"try get client {uid} failed in secret area rank");
        //    }
        //}
    }
}

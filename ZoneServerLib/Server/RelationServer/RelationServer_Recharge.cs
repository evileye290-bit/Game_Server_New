using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_UpdateRechargeActivityValue(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_RECHARGE_ACTIVITY_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_RECHARGE_ACTIVITY_VALUE>(stream);
            Log.Write($"UpdateRechargeActivityValue from main {MainId} ");

            RechargeGiftTimeType type = (RechargeGiftTimeType)pks.GiftType;

            foreach (var pc in Api.PCManager.PcList)
            {
                pc.Value.UpdateRechargeActivityValue(type);
            }
            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                pc.Value.UpdateRechargeActivityValue(type);
            }
            foreach (var pc in Api.PCManager.PlayerEnterList)
            {
                pc.Value.Player.UpdateRechargeActivityValue(type);
            }
            
        }
    }
}

using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
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
        private void OnResponse_GiftCodeExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GIFT_CODE_REWARD>(stream);

            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} gift code exchange reward failed: not find ", msg.PcUid);
                return;
            }

            MSG_ZGC_GIFT_CODE_REWARD response = new MSG_ZGC_GIFT_CODE_REWARD();
            response.Result = (int)ErrorCode.Success;
            player.Write(response);
        }

        public void OnResponse_CheckGiftCodeExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHECK_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHECK_GIFT_CODE_REWARD>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} check gift code exchange reward failed: not find ", uid);
                return;
            }
            player.GiftCodeExchangeReward(msg.GiftCode, msg.CheckResult);
        }

        public void OnResponse_CheckCodeUnique(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHECK_CODE_UNIQUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHECK_CODE_UNIQUE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} check code unique failed: not find ", uid);
                return;
            }
            player.CheckAndSaveUniqueCodeInfo(msg.GiftCode, msg.CheckResult);
        }
    }
}

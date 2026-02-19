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
        public void OnResponse_GiftCodeExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GIFT_CODE_REWARD>(stream);

            if (msg.PcUid > 0)
            {
                //发送邮件
                Api.EmailMng.SendPersonEmail(msg.PcUid, msg.EmailId, msg.Rewards);
            }

            MSG_RZ_GIFT_CODE_REWARD response = new MSG_RZ_GIFT_CODE_REWARD()
            {
                PcUid = msg.PcUid,
                EmailId = msg.EmailId,
                Rewards = msg.Rewards
            };

            Write(response);
        }

        public void OnResponse_CheckGiftCodeExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHECK_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHECK_GIFT_CODE_REWARD>(stream);
            bool hasCode = false;
            if (RelationServerApi.GiftCodeList.ContainsKey(msg.GiftCode))
            {
                hasCode = true;
            }

            MSG_RZ_CHECK_GIFT_CODE_REWARD response = new MSG_RZ_CHECK_GIFT_CODE_REWARD()
            {
                GiftCode = msg.GiftCode,
                CheckResult = hasCode,
            };
            Write(response, uid);
        }

        public void OnResponse_CheckCodeUnique(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHECK_CODE_UNIQUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHECK_CODE_UNIQUE>(stream);
            bool hasCode = false;
            if (RelationServerApi.GiftCodeList.ContainsKey(msg.GiftCode))
            {
                hasCode = true;
            }

            MSG_RZ_CHECK_CODE_UNIQUE response = new MSG_RZ_CHECK_CODE_UNIQUE()
            {
                GiftCode = msg.GiftCode,
                CheckResult = hasCode,
            };
            Write(response, uid);
        }
    }
}

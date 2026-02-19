using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GiftCodeExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GIFT_CODE_REWARD>(stream);
            Log.Write("player {0} request use gift code {1} exchange reward", uid, msg.GiftCode);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} use gift code exchange reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} use gift code exchange reward not in map ", uid);
                return;
            }
            //player.GiftCodeExchangeReward(msg.GiftCode);
            player.CheckGiftCodeExchangeReward(msg.GiftCode);
        }

        public void OnResponse_CheckCodeUnique(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHECK_CODE_UNIQUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHECK_CODE_UNIQUE>(stream);
            Log.Write("player {0} request check gift code {1} unique", uid, msg.GiftCode);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} check gift code unique not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  check gift code unique not in map ", uid);
                return;
            }
            //player.CheckAndSaveUniqueCodeInfo(msg.GiftCode);
            player.CheckUniqueCodeInfo(msg.GiftCode);
        }
    }
}

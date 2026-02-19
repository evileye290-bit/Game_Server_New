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
        public void OnResponse_GetDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DIVINE_LOVE_VALUE>(stream);
            Log.Write("player {0} GetDivineLoveInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetDivineLoveInfo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetDivineLoveInfo not in map ", uid);
                return;
            }

            player.GetDivineLoveInfo();
        }

        public void OnResponse_GetDivineLoveReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DIVINE_LOVE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DIVINE_LOVE_REWARD>(stream);
            Log.Write("player {0} GetDivineLoveReward id {1} {2}", uid, pks.Id, pks.UseDiamond);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetDivineLoveReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetDivineLoveReward not in map ", uid);
                return;
            }

            player.GetDivineLoveReward(pks.Id, pks.UseDiamond, pks.Index);
        }

        public void OnResponse_GetDivineLoveCumulateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DIVINE_LOVE_CUMULATE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DIVINE_LOVE_CUMULATE_REWARD>(stream);
            Log.Write("player {0} GetDivineLoveCumulateReward id {1} HeartNum {2}", uid, pks.Id, pks.HeartNum);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetDivineLoveCumulateReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetDivineLoveCumulateReward not in map ", uid);
                return;
            }

            player.GetDivineLoveCumulateReward(pks.Id, pks.HeartNum);
        }

        public void OnResponse_BuyDivineLoveItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_DIVINE_LOVE_ITEM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_DIVINE_LOVE_ITEM>(stream);
            Log.Write("player {0} BuyDivineLoveItem id {1} num {2}", uid, pks.Id, pks.Num);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyDivineLoveItem not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} BuyDivineLoveItem not in map ", uid);
                return;
            }

            player.BuyDivineLoveItem(pks.Id, pks.Num);
        }

        public void OnResponse_OpenDivineLoveRound(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPEN_DIVINE_LOVE_ROUND pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_DIVINE_LOVE_ROUND>(stream);
            Log.Write("player {0} OpenDivineLoveRound id {1} ", uid, pks.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OpenDivineLoveRound not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} OpenDivineLoveRound not in map ", uid);
                return;
            }

            player.OpenDivineLoveRound(pks.Id);
        }

        public void OnResponse_CloseDivineLoveRound(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CLOSE_DIVINE_LOVE_ROUND pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CLOSE_DIVINE_LOVE_ROUND>(stream);
            Log.Write("player {0} CloseDivineLoveRound id {1} ", uid, pks.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CloseDivineLoveRound not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} CloseDivineLoveRound not in map ", uid);
                return;
            }

            player.CloseDivineLoveRound(pks.Id);
        }
    }
}

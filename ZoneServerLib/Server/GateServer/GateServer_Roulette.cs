using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    partial class GateServer
    {
        public void OnResponse_GetRouletteInfo(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} GetRouletteInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetRouletteInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetRouletteInfo not in map ", uid);
                return;
            }

            player.GetRouletteInfo();
        }

        public void OnResponse_RouletteRandom(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ROULETTE_RANDOM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ROULETTE_RANDOM>(stream);

            Log.Write("player {0} RouletteRandom", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} RouletteRandom not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.RouletteRandom(msg.Num);
        }

        public void OnResponse_RouletteReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ROULETTE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ROULETTE_REWARD>(stream);

            Log.Write("player {0} RouletteReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} RouletteReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.RouletteReward();
        }

        private void OnResponse_RouletteRefresh(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} RouletteRefresh", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} RouletteReward not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.RouletteRefresh();
        }

        private void OnResponse_RouletteBuyItem(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} RouletteBuyItem", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} RouletteBuyItem not in gate id {1} pc list", uid, SubId);
                return;
            }

            MSG_GateZ_ROULETTE_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ROULETTE_BUY_ITEM>(stream);
            player.RouletteBuyItem(msg.Num);
        }
    }
}

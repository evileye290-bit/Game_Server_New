using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    partial class Client
    {
        private void OnResponse_GetRouletteInfo(MemoryStream stream)
        {
            MSG_GateZ_ROULETTE_GET_INFO request = new MSG_GateZ_ROULETTE_GET_INFO();
            WriteToZone(request);
        }

        private void OnResponse_RouletteRandom(MemoryStream stream)
        {
            MSG_CG_ROULETTE_RANDOM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ROULETTE_RANDOM>(stream);
            MSG_GateZ_ROULETTE_RANDOM request = new MSG_GateZ_ROULETTE_RANDOM() {Num = msg.Num};
            WriteToZone(request);
        }

        private void OnResponse_RouletteReward(MemoryStream stream)
        {
            WriteToZone(new MSG_GateZ_ROULETTE_REWARD());
        }

        private void OnResponse_RouletteRefresh(MemoryStream stream)
        {
            WriteToZone(new MSG_GateZ_ROULETTE_REFRESH());
        }

        private void OnResponse_RouletteBuyItem(MemoryStream stream)
        {
            MSG_CG_ROULETTE_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ROULETTE_BUY_ITEM>(stream);
            WriteToZone(new MSG_GateZ_ROULETTE_BUY_ITEM() {Num = msg.Num});
        }
    }
}

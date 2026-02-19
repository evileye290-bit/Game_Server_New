using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_CounterBuyCount(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_COUNTER_BUY_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_COUNTER_BUY_COUNT>(stream);
            Log.Write($"player {msg.Uid} request counter buy count, counterType {msg.CounterType} count {msg.Count}");

            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.BuyCounterCount(msg.CounterType, msg.Count);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("BuyCounterCount fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("BuyCounterCount fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        public void OnResponse_GetSpecialCount(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SPECIAL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SPECIAL_COUNT>(stream);

            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.GetSpecialCount();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("get special count fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("get special count fail, can not find player {0} .", msg.Uid);
                }
            }
        }     
    }
}

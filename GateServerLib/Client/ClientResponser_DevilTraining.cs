using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_DevilTrainingInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DEVIL_TRAINING_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DEVIL_TRAINING_INFO>(stream);
            MSG_GateZ_GET_DEVIL_TRAINING_INFO request = new MSG_GateZ_GET_DEVIL_TRAINING_INFO();
            WriteToZone(request);
        }

        public void OnResponse_DevilTrainingReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DEVIL_TRAINING_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DEVIL_TRAINING_REWARD>(stream);
            MSG_GateZ_GET_DEVIL_TRAINING_REWARD request = new MSG_GateZ_GET_DEVIL_TRAINING_REWARD();
            request.Id = msg.Id;
            request.IsConsecutive = msg.IsConsecutive;
            request.UseDiamond = msg.UseDiamond;
            WriteToZone(request);
        }

        public void OnResponse_DevilTrainingBuyItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_DEVIL_TRAINING_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_DEVIL_TRAINING_ITEM>(stream);
            MSG_GateZ_BUY_DEVIL_TRAINING_ITEM request = new MSG_GateZ_BUY_DEVIL_TRAINING_ITEM();
            request.Id = msg.Id;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_DevilTrainingPointReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DEVIL_TRAINING_POINT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DEVIL_TRAINING_POINT_REWARD>(stream);
            MSG_GateZ_GET_DEVIL_TRAINING_POINT_REWARD request = new MSG_GateZ_GET_DEVIL_TRAINING_POINT_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_DevilTrainingChangeBuff(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHANGE_DEVIL_TRAINING_BUFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_DEVIL_TRAINING_BUFF>(stream);
            MSG_GateZ_CHANGE_DEVIL_TRAINING_BUFF request = new MSG_GateZ_CHANGE_DEVIL_TRAINING_BUFF();
            request.Id = msg.Id;
            WriteToZone(request);
        }
    }
}

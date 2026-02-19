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
        public void OnResponse_DevilTrainingInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DEVIL_TRAINING_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DEVIL_TRAINING_INFO>(stream);
            Log.Write("player {0} request deviltraining info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} deviltraining info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SendDevilTrainingInfoByLoading();
        }
        public void OnResponse_DevilTrainingReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DEVIL_TRAINING_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DEVIL_TRAINING_REWARD>(stream);
            Log.Write("player {0} request deviltraining get reward type {1} isConsecutive {2} userDiamond {3}", uid, msg.Id, msg.IsConsecutive, msg.UseDiamond);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} deviltraining get reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetDevilTrainingReward(msg.Id, msg.IsConsecutive, msg.UseDiamond);
        }
        public void OnResponse_DevilTrainingBuyItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_DEVIL_TRAINING_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_DEVIL_TRAINING_ITEM>(stream);
            Log.Write("player {0} request deviltraining buy item type {1} num {2} ", uid, msg.Id, msg.Num);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} deviltraining buy item not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.BuyDevilTrainingItem(msg.Id, msg.Num);
        }
        public void OnResponse_DevilTrainingPointReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DEVIL_TRAINING_POINT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DEVIL_TRAINING_POINT_REWARD>(stream);
            Log.Write("player {0} request deviltraining get point reward id {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} deviltraining get point reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetDevilTrainingPointReward(msg.Id);
        }
        public void OnResponse_DevilTrainingChangeBuff(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHANGE_DEVIL_TRAINING_BUFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_DEVIL_TRAINING_BUFF>(stream);
            Log.Write("player {0} request deviltraining change buff id {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} deviltraining change buff not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.ChangeDevilTrainingBuff(msg.Id);
        }

    }
}

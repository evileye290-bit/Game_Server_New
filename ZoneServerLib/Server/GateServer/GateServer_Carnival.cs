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
        public void OnResponse_EnterCarnivalBossDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_CARNIVAL_BOSS_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_CARNIVAL_BOSS_DUNGEON>(stream);
            Log.Write("player {0} request enter carnival boss dungeon {1}", uid, msg.DungeonId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  enter carnival boss dungeon not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.EnterCarnivalBossDungeon(msg.DungeonId);
        }

        public void OnResponse_GetCarnivalBossReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CARNIVAL_BOSS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CARNIVAL_BOSS_REWARD>(stream);
            Log.Write("player {0} request get carnival boss reward {1}", uid, msg.Degree);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get carnival boss reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCarnivalBossReward(msg.Degree);
        }

        public void OnResponse_UpdateCarnivalBossQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_CARNIVAL_BOSS_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_CARNIVAL_BOSS_QUEUE>(stream);
            Log.Write("player {0} request update carnival boss queue{1}", uid, msg.HeroDefInfos);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} update carnival boss queue not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.UpdateCarnivalBossQueue(msg.HeroDefInfos);
        }

        public void OnResponse_GetCarnivalRechargeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CARNIVAL_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CARNIVAL_RECHARGE_REWARD>(stream);
            Log.Write("player {0} request get carnival recharge reward {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get carnival recharge reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCarnivalRechargeReward(msg.Id);
        }

        public void OnResponse_BuyCarnivalMallGiftItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_CARNIVAL_MALL_GIFT_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_CARNIVAL_MALL_GIFT_ITEM>(stream);
            Log.Write("player {0} request buy carnival mall gift item {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} buy carnival mall gift item not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.BuyCarnivalMallGiftItem(msg.Id);
        }
    }
}

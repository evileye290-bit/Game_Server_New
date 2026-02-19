using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_EnterCarnivalBossDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_CARNIVAL_BOSS_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ENTER_CARNIVAL_BOSS_DUNGEON>(stream);
            MSG_GateZ_ENTER_CARNIVAL_BOSS_DUNGEON request = new MSG_GateZ_ENTER_CARNIVAL_BOSS_DUNGEON();
            request.DungeonId = msg.DungeonId;
            WriteToZone(request);
        }

        public void OnResponse_GetCarnivalBossReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CARNIVAL_BOSS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CARNIVAL_BOSS_REWARD>(stream);
            MSG_GateZ_GET_CARNIVAL_BOSS_REWARD request = new MSG_GateZ_GET_CARNIVAL_BOSS_REWARD();
            request.Degree = msg.Degree;
            WriteToZone(request);
        }

        public void OnResponse_UpdateCarnivalBossQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_CARNIVAL_BOSS_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_CARNIVAL_BOSS_QUEUE>(stream);
            MSG_GateZ_UPDATE_CARNIVAL_BOSS_QUEUE request = new MSG_GateZ_UPDATE_CARNIVAL_BOSS_QUEUE();
            msg.HeroDefInfos.ForEach(x =>
            {
                request.HeroDefInfos.Add(new HERO_DEFENSIVE_DATA() { HeroId = x.HeroId, QueueNum = x.QueueNum, PositionNum = x.PositionNum });
            });
            WriteToZone(request);
        }

        public void OnResponse_GetCarnivalRechargeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CARNIVAL_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CARNIVAL_RECHARGE_REWARD>(stream);
            MSG_GateZ_GET_CARNIVAL_RECHARGE_REWARD request = new MSG_GateZ_GET_CARNIVAL_RECHARGE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_BuyCarnivalMallGiftItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_CARNIVAL_MALL_GIFT_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_CARNIVAL_MALL_GIFT_ITEM>(stream);
            MSG_GateZ_BUY_CARNIVAL_MALL_GIFT_ITEM request = new MSG_GateZ_BUY_CARNIVAL_MALL_GIFT_ITEM();
            request.Id = msg.Id;
            WriteToZone(request);
        }
    }
}

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
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HIDDER_WEAPON_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HIDDER_WEAPON_VALUE>(stream);
            MSG_GateZ_GET_HIDDER_WEAPON_VALUE request = new MSG_GateZ_GET_HIDDER_WEAPON_VALUE();

            WriteToZone(request);
        }

        public void OnResponse_GetHidderWeaponReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HIDDER_WEAPON_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HIDDER_WEAPON_REWARD>(stream);
            MSG_GateZ_GET_HIDDER_WEAPON_REWARD request = new MSG_GateZ_GET_HIDDER_WEAPON_REWARD();
            request.Id = msg.Id;
            request.IsConsecutive = msg.IsConsecutive;
            request.UseDiamond = msg.UseDiamond;
            request.RingNum = msg.RingNum;
            WriteToZone(request);
        }

        public void OnResponse_GetHidderWeaponNumReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HIDDER_WEAPON_NUM_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HIDDER_WEAPON_NUM_REWARD>(stream);
            MSG_GateZ_GET_HIDDER_WEAPON_NUM_REWARD request = new MSG_GateZ_GET_HIDDER_WEAPON_NUM_REWARD();
            request.Id = msg.Id;
            request.RewardNum = msg.RewardNum;
            WriteToZone(request);
        }

        public void OnResponse_BuyHidderWeaponItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_HIDDER_WEAPON_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_HIDDER_WEAPON_ITEM>(stream);
            MSG_GateZ_BUY_HIDDER_WEAPON_ITEM request = new MSG_GateZ_BUY_HIDDER_WEAPON_ITEM();
            request.Id = msg.Id;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SEA_TREASURE_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SEA_TREASURE_VALUE>(stream);
            MSG_GateZ_GET_SEA_TREASURE_VALUE request = new MSG_GateZ_GET_SEA_TREASURE_VALUE();

            WriteToZone(request);
        }

        public void OnResponse_GetSeaTreasureReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SEA_TREASURE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SEA_TREASURE_REWARD>(stream);
            MSG_GateZ_GET_SEA_TREASURE_REWARD request = new MSG_GateZ_GET_SEA_TREASURE_REWARD();
            request.Id = msg.Id;
            request.UseDiamond = msg.UseDiamond;
            request.IsConsecutive = msg.IsConsecutive;
            WriteToZone(request);
        }

        public void OnResponse_BuySeaTreasureItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_SEA_TREASURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_SEA_TREASURE_ITEM>(stream);
            MSG_GateZ_BUY_SEA_TREASURE_ITEM request = new MSG_GateZ_BUY_SEA_TREASURE_ITEM();
            request.Id = msg.Id;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_GetNotesListByType(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NOTES_LIST_BY_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NOTES_LIST_BY_TYPE>(stream);
            MSG_GateZ_NOTES_LIST_BY_TYPE request = new MSG_GateZ_NOTES_LIST_BY_TYPE();
            request.Type = msg.Type;
            WriteToZone(request);
        }

        public void OnResponse_GetSeaTreasureBlessing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SEA_TREASURE_BLESSING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SEA_TREASURE_BLESSING>(stream);
            MSG_GateZ_GET_SEA_TREASURE_BLESSING request = new MSG_GateZ_GET_SEA_TREASURE_BLESSING();

            WriteToZone(request);
        }

        public void OnResponse_CloseSeaTreasureBlessing(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CLOSE_SEA_TREASURE_BLESSING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CLOSE_SEA_TREASURE_BLESSING>(stream);
            MSG_GateZ_CLOSE_SEA_TREASURE_BLESSING request = new MSG_GateZ_CLOSE_SEA_TREASURE_BLESSING();

            WriteToZone(request);
        }

        public void OnResponse_GetSeaTreasureNumReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SEA_TREASURE_NUM_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SEA_TREASURE_NUM_REWARD>(stream);
            MSG_GateZ_GET_SEA_TREASURE_NUM_REWARD request = new MSG_GateZ_GET_SEA_TREASURE_NUM_REWARD();
            request.Id = msg.Id;
            request.RewardNum = msg.RewardNum;
            WriteToZone(request);
        }
    }
}

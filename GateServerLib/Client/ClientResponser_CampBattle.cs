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
        private void OnResponse_GetCampBattleInfo(MemoryStream stream)
        {
            MSG_CG_GET_CAMPBATTLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CAMPBATTLE_INFO>(stream);
            MSG_GateZ_GET_CAMPBATTLE_INFO request = new MSG_GateZ_GET_CAMPBATTLE_INFO();
            WriteToZone(request);
        }

        private void OnResponse_GetFortInfo(MemoryStream stream)
        {
            MSG_CG_GET_FORT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_FORT_INFO>(stream);
            MSG_GateZ_FORT_INFO request = new MSG_GateZ_FORT_INFO();
            request.FortId = msg.FortId;
            WriteToZone(request);
        }


        private void OnResponse_GetCampBattleRankList(MemoryStream stream)
        {
            MSG_CG_GET_CAMPBATTLE_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CAMPBATTLE_RANK_LIST>(stream);
            MSG_GateZ_GET_CAMPBATTLE_RANK_LIST request = new MSG_GateZ_GET_CAMPBATTLE_RANK_LIST();
            request.Type = msg.Type;
            request.Page = msg.Page;
            request.Camp = msg.Camp;
            WriteToZone(request);
        }

        private void OnResponse_OpenCampBox(MemoryStream stream)
        {
            MSG_CG_OPEN_CAMP_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_CAMP_BOX>(stream);
            MSG_GateZ_OPEN_CAMP_BOX request = new MSG_GateZ_OPEN_CAMP_BOX();
            WriteToZone(request);
        }

        private void OnResponse_CheckInBattleRank(MemoryStream stream)
        {
            MSG_CG_CHECK_IN_BATTLE_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHECK_IN_BATTLE_RANK>(stream);
            MSG_GateZ_CHECK_IN_BATTLE_RANK request = new MSG_GateZ_CHECK_IN_BATTLE_RANK();
            WriteToZone(request);
        }

        private void OnResponse_UseNatureItem(MemoryStream stream)
        {
            MSG_CG_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_USE_NATURE_ITEM>(stream);
            MSG_Gate_USE_NATURE_ITEM request = new MSG_Gate_USE_NATURE_ITEM();
            request.FortId = msg.FortId;
            request.ItemId = msg.ItemId;
            WriteToZone(request);
        }

        private void OnResponse_UpdateDefensiveQueue(MemoryStream stream)
        {
            MSG_CG_UPDATE_DEFENSIVE_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_DEFENSIVE_QUEUE>(stream);
            MSG_GateZ_UPDATE_DEFENSIVE_QUEUE request = new MSG_GateZ_UPDATE_DEFENSIVE_QUEUE();
            foreach (var item in msg.HeroDefInfos)
            {
                HERO_DEFENSIVE_DATA heroDefDate = new HERO_DEFENSIVE_DATA();
                heroDefDate.HeroId = item.HeroId;
                heroDefDate.QueueNum = item.QueueNum;
                heroDefDate.PositionNum = item.PositionNum;
                request.HeroDefInfos.Add(heroDefDate);
            }
            WriteToZone(request);
        }

        private void OnResponse_GiveUpFort(MemoryStream stream)
        {
            MSG_CG_GIVEUP_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GIVEUP_FORT>(stream);
            MSG_GateZ_GIVEUP_FORT request = new MSG_GateZ_GIVEUP_FORT();
            request.FortId = msg.FortId;
            WriteToZone(request);
        }

        private void OnResponse_HoldFort(MemoryStream stream)
        {
            MSG_CG_HOLD_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HOLD_FORT>(stream);
            MSG_GateZ_HOLD_FORT request = new MSG_GateZ_HOLD_FORT();
            request.FortId = msg.FortId;
            WriteToZone(request);

        }


        private void OnResponse_GetCampBattleAnnouce(MemoryStream stream)
        {
            MSG_CG_GET_CAMPBATTLE_ANNOUNCE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CAMPBATTLE_ANNOUNCE>(stream);
            MSG_GateZ_GET_CAMPBATTLE_ANNOUNCE request = new MSG_GateZ_GET_CAMPBATTLE_ANNOUNCE();
            WriteToZone(request);
        }

    }

}
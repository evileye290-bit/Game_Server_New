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
        private void OnResponse_ChooseCamp(MemoryStream stream)
        {
            MSG_CG_CHOOSE_CAMP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHOOSE_CAMP>(stream);
            MSG_GateZ_CHOOSE_CAMP request = new MSG_GateZ_CHOOSE_CAMP();
            request.Uid = Uid;
            request.Camp = msg.Camp;
            WriteToZone(request);
        }

        private void OnResponse_Salute(MemoryStream stream)
        {
            MSG_CG_WORSHIP msg= MessagePacker.ProtobufHelper.Deserialize<MSG_CG_WORSHIP>(stream);
            MSG_GateZ_WORSHIP req = new MSG_GateZ_WORSHIP();

            req.PcUid = Uid;
            req.ToRank = msg.ToRank;
            WriteToZone(req);
        }

        private void OnResponse_CampElect(MemoryStream stream)
        {
            MSG_CG_VOTE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_VOTE>(stream);
            MSG_GateZ_VOTE req = new MSG_GateZ_VOTE();

            req.PcUid = Uid;
            req.ToPcUid = msg.ToPcUid;
            req.ItemId = msg.ItemId;
            req.Num = msg.Num;
            WriteToZone(req);
        }

        private void OnResponse_RunInElection(MemoryStream stream)
        {
            MSG_GateZ_RUN_IN_ELECTION req = new MSG_GateZ_RUN_IN_ELECTION();

            WriteToZone(req);
        }

        private void OnResponse_ShowCampPanelInfo(MemoryStream stream)
        {
            MSG_CG_SHOW_CAMP_PANEL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_CAMP_PANEL_INFO>(stream);
            MSG_GateZ_SHOW_CAMP_PANEL_INFO req = new MSG_GateZ_SHOW_CAMP_PANEL_INFO();
            req.Uid = Uid;
            WriteToZone(req);
        }

        private void OnResponse_GetCampReward(MemoryStream stream)
        {
            MSG_CG_GET_CAMP_REWARD msg= MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CAMP_REWARD>(stream);
            MSG_GateZ_GET_CAMP_REWARD req = new MSG_GateZ_GET_CAMP_REWARD();
            req.Uid = Uid;
            WriteToZone(req);
        }

        private void OnResponse_ShowCampInfos(MemoryStream stream)
        {
            MSG_CG_SHOW_CAMP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_CAMP_INFO>(stream);
            MSG_GateZ_SHOW_CAMP_INFO request = new MSG_GateZ_SHOW_CAMP_INFO();
            request.Uid = Uid;
            request.CampId = msg.CampId;
            request.Page = msg.Page;
            WriteToZone(request);
        }

        private void OnResponse_ShowCampElectionInfos(MemoryStream stream)
        {
            MSG_CG_SHOW_CAMP_ELECTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOW_CAMP_ELECTION_INFO>(stream);
            MSG_GateZ_SHOW_CAMP_ELECTION_INFO request = new MSG_GateZ_SHOW_CAMP_ELECTION_INFO();
            request.Uid = Uid;
            request.Page = msg.Page;
            WriteToZone(request);
        }
        private void OnResponse_GetStarLevel(MemoryStream stream)
        {
            MSG_CG_GET_STARLEVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_STARLEVEL>(stream);
            MSG_GateZ_GET_STARLEVEL request = new MSG_GateZ_GET_STARLEVEL();
            request.Uid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_CampStarLevelUp(MemoryStream stream)
        {
            MSG_CG_STAR_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_STAR_LEVELUP>(stream);
            MSG_GateZ_STAR_LEVELUP request = new MSG_GateZ_STAR_LEVELUP();
            request.Uid = Uid;
            request.StarId = msg.StarId;
            WriteToZone(request);
        }

        private void OnResponse_CampGather(MemoryStream stream)
        {
            MSG_CG_CAMP_GATHER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CAMP_GATHER>(stream);
            MSG_GateZ_CAMP_GATHER request = new MSG_GateZ_CAMP_GATHER();
            WriteToZone(request);
        }

        private void OnResponse_GatherDialogueComplete(MemoryStream stream)
        {
            MSG_CG_GATHER_DIALOGUE_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GATHER_DIALOGUE_COMPLETE>(stream);
            MSG_GateZ_GATHER_DIALOGUE_COMPLETE request = new MSG_GateZ_GATHER_DIALOGUE_COMPLETE();
            request.Id = msg.Id;
            request.Refuse = msg.Refuse;
            WriteToZone(request);
        }
    }
}

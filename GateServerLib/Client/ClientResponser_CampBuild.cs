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
        private void OnResponse_GetCampBuildInfo(MemoryStream stream)
        {
            MSG_CG_GET_CAMPBUILD_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CAMPBUILD_INFO>(stream);
            MSG_GateZ_GET_CAMPBUILD_INFO request = new MSG_GateZ_GET_CAMPBUILD_INFO();
            WriteToZone(request);
        }

        private void OnResponse_CampBuildGo(MemoryStream stream)
        {
            MSG_CG_CAMPBUILD_GO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CAMPBUILD_GO>(stream);
            MSG_GateZ_CAMPBUILD_GO request = new MSG_GateZ_CAMPBUILD_GO();
            WriteToZone(request);
        }

        private void OnResponse_BuyCampBuildGoCount(MemoryStream stream)
        {
            MSG_CG_BUY_CAMPBUILD_GO_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_CAMPBUILD_GO_COUNT>(stream);
            MSG_GateZ_BUY_CAMPBUILD_GO_COUNT request = new MSG_GateZ_BUY_CAMPBUILD_GO_COUNT();
            request.Count = msg.Count;
            WriteToZone(request);
        }

        public void OnResponse_CampBuildRankList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CAMPBUILD_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CAMPBUILD_RANK_LIST>(stream);
            MSG_GateZ_CAMPBUILD_RANK_LIST request = new MSG_GateZ_CAMPBUILD_RANK_LIST();
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_OpenCampBuildBox(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_OPEN_CAMPBUILD_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_CAMPBUILD_BOX>(stream);
            MSG_GateZ_OPEN_CAMPBUILD_BOX request = new MSG_GateZ_OPEN_CAMPBUILD_BOX();
            request.BoxType = msg.BoxType;
            WriteToZone(request);
        }

        public void OnResponse_CampCreateDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CAMP_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CAMP_CREATE_DUNGEON>(stream);
            MSG_GateZ_CAMP_CREATE_DUNGEON request = new MSG_GateZ_CAMP_CREATE_DUNGEON();
            request.FortId = msg.FortId;
            request.DungeonId = msg.DungeonId;

            WriteToZone(request);
        }
    }
}

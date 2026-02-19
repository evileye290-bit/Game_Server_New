using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
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
        public void OnResponse_GetRankListByType(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_RANK_LIST_BY_TYPE msg = ProtobufHelper.Deserialize<MSG_CG_GET_RANK_LIST_BY_TYPE>(stream);
            MSG_GateZ_GET_RANK_LIST_BY_TYPE request = new MSG_GateZ_GET_RANK_LIST_BY_TYPE();
            request.RankType = msg.RankType;
            request.Page = msg.Page;
            request.ParamId = msg.ParamId;
            WriteToZone(request);
        }

        public void OnResponse_GetRankRewardInfos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RANK_REWARD_LIST msg = ProtobufHelper.Deserialize<MSG_CG_RANK_REWARD_LIST>(stream);
            MSG_GateZ_RANK_REWARD_LIST request = new MSG_GateZ_RANK_REWARD_LIST();
            request.RankType = msg.RankType;
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_GetRankReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_RANK_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_RANK_REWARD>(stream);
            MSG_GateZ_GET_RANK_REWARD request = new MSG_GateZ_GET_RANK_REWARD();
            request.RankType = msg.RankType;
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetCrossRankReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CROSS_RNAK_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CROSS_RNAK_REWARD>(stream);
            MSG_GateZ_GET_CROSS_RANK_REWARD request = new MSG_GateZ_GET_CROSS_RANK_REWARD();
            request.RankType = msg.RankType;
            WriteToZone(request);
        }
    }
}

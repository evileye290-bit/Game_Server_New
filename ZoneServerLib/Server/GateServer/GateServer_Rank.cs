using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
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
        public void OnResponse_GetRankListByType(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_RANK_LIST_BY_TYPE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANK_LIST_BY_TYPE>(stream);
            Log.Write("player {0} request get rank list: type {1} page {2}", uid, pks.RankType, pks.Page);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get rank list by type info not in gateid {1} pc list", uid, SubId);
                return;
            }
            //if (pks.RankType == (int)RankType.BattlePower)
            //{
            // player.GetBattlePowerRank(pks.Page, pks.RankType);
            //}

            MSG_ZR_GET_RANK_LIST request = new MSG_ZR_GET_RANK_LIST();
            request.RankType = pks.RankType;
            request.Page = pks.Page;
            request.ParamId = pks.ParamId;
            player.server.SendToRelation(request, player.Uid);
        }

        public void OnResponse_GetRankRewardInfos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RANK_REWARD_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RANK_REWARD_LIST>(stream);
            Log.Write("player {0} request get rank reward info: type {1} page {2}", uid, pks.RankType, pks.Page);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get rank reward info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetRankRewardInfos(pks.RankType, pks.Page);
        }


        public void OnResponse_GetRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RANK_REWARD>(stream);
            Log.Write("player {0} request get rank reward: type {1} rewardId {2}", uid, pks.RankType, pks.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get rank reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetRankReward(pks.RankType, pks.Id);
        }

        public void OnResponse_GetCrossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CROSS_RANK_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CROSS_RANK_REWARD>(stream);
            Log.Write("player {0} request get cross rank {1} reward", uid, msg.RankType);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}  get cross rank reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetCrossRankReward(msg.RankType);
        }
    }
}

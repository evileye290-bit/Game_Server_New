using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RZ;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace RelationServerLib
{
    public partial class CrossServer
    {

        public void OnResponse_NewGetRankList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_RANK_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_RANK_LIST>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} NewGetRankList failed: not find ", uid);
                return;
            }
            client.Write(pks);
        }

        public void OnResponse_GetRankFirstInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_RANK_FIRST_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_RANK_FIRST_INFO>(stream);
            Log.Write($"player {uid} GetRankFirstInfo from main {MainId} ");

            MSG_RZ_GET_RANK_FIRST_INFO msg = new MSG_RZ_GET_RANK_FIRST_INFO();
            msg.Uid = pks.Uid;
            msg.RankType = pks.RankType;
            msg.FirstValue = pks.FirstValue;

            HFPlayerBaseInfoItem item;
            foreach (var kv in pks.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                msg.BaseInfo.Add(item);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetRankFirstInfo not find client ");
            }
        }

        public void OnResponse_GetRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_RANK_REWARD>(stream);
            Log.Write($"player {uid} GetCrossRankReward from main {MainId} ");

            MSG_RZ_GET_CROSS_RANK_REWARD msg = new MSG_RZ_GET_CROSS_RANK_REWARD();
            msg.Uid = pks.Uid;
            msg.RankType = pks.RankType;
            msg.Rank = pks.Rank;       

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetCrossRankReward not find client ");
            }
        }

        public void OnResponse_RecordRankActiveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_RECORD_RANK_ACTIVE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_RECORD_RANK_ACTIVE_INFO>(stream);
            Log.Write($"player {uid} RecordRankActiveInfo from main {MainId} ");

            MSG_RZ_RECORD_RANK_ACTIVE_INFO msg = new MSG_RZ_RECORD_RANK_ACTIVE_INFO();
            msg.RankType = pks.RankType;
            msg.FirstUid = pks.FirstUid;
            msg.FirstValue = pks.FirstValue;
            msg.LuckyUid = pks.LuckyUid;

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} RecordRankActiveInfo not find client");
            }
        }

        public void OnResponse_RecordRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_RECORD_RANK_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_RECORD_RANK_INFO>(stream);
            Log.Write($"player {uid} RecordRankInfo from main {MainId} ");

            int group = CrossBattleLibrary.GetGroupId(Api.MainId);
            List<int> groupServers = CrossBattleLibrary.GetGroupServers(group);
            string[] serverIdArr = null;
            if (groupServers != null)
            {
                serverIdArr = new string[groupServers.Count];
                for (int i = 0; i < groupServers.Count; i++)
                {
                    serverIdArr[i] = groupServers[i].ToString();
                }
            }

            string[] rankInfoArr = new string[pks.RankInfo.Count];
            for (int i = 0; i < pks.RankInfo.Count; i++)
            {
                rankInfoArr[i] = pks.RankInfo[i];
            }
            BIRecordRankLog(serverIdArr, pks.RankType, pks.Stage, rankInfoArr);
        }

        public void BIRecordRankLog(string[] serverIdArr, string rankType, int stage, string[] rankInfoArr)
        {
            // LOG 记录开关
            //if (!GameConfig.TrackingLogSwitch)
            //{
            //    return;
            //}
            Api.BILoggerMng.RankTaLog(Api.MainId, serverIdArr, rankType, stage, rankInfoArr);
        }
    }
}

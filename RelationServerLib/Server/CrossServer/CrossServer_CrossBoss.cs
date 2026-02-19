using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using CommonUtility;
using DBUtility;
using Google.Protobuf.Collections;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        public void OnResponse_GetCrossBossInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_CROSS_BOSS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_CROSS_BOSS_INFO>(stream);
            Log.Write($"player {uid} GetCrossBossInfo from main {MainId} ");

            MSG_RZ_GET_CROSS_BOSS_INFO msg = new MSG_RZ_GET_CROSS_BOSS_INFO();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.Score = pks.Score;
            msg.Result = pks.Result;
            foreach (var kv in pks.SiteList)
            {
                RZ_CrossBossSiteInfo info = GetCrossBossSiteInfoMsg(kv.Value);
                msg.SiteList.Add(kv.Key, info);
            }

            foreach (var kv in pks.SiteDefenseList)
            {
                msg.SiteDefenseList.Add(kv.Key, kv.Value);
            }

            foreach (var kv in pks.CurrentSiteList)
            {
                msg.CurrentSiteList.Add(kv.Key, kv.Value);
            }

            foreach (var defenser in pks.Defensers)
            {
                RZ_CrossBossDefenser playerMsg = new RZ_CrossBossDefenser();
                HFPlayerBaseInfoItem item;
                foreach (var kv in defenser.Value.BaseInfo)
                {
                    item = new HFPlayerBaseInfoItem();
                    item.Key = kv.Key;
                    item.Value = kv.Value;
                    playerMsg.BaseInfo.Add(item);
                }
                msg.Defensers.Add(defenser.Key, playerMsg);
            }

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(msg);
            }
            else
            {
                Log.Warn($"player {uid} GetCrossBossInfo not find client ");
            }
        }

        private static RZ_CrossBossSiteInfo GetCrossBossSiteInfoMsg(CorssR_CrossBossSiteInfo siteInfo)
        {
            RZ_CrossBossSiteInfo info = new RZ_CrossBossSiteInfo();
            info.Id = siteInfo.Id;
            info.Hp = siteInfo.Hp;
            info.MaxHp = siteInfo.MaxHp;
            return info;
        }

        //返回信息
        public void OnResponse_GetCrossBossPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_BOSS_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossBossPlayerInfo");

            Client client = Api.ZoneManager.GetClient(pks.FindPcUid);
            if (client != null)
            {
                client.Write(pks);
            }
            else
            {
                FrontendServer server = Api.ZoneManager.GetOneServer();
                if (server != null)
                {
                    server.Write(pks, uid);
                }
            }
        }

        public void OnResponse_ReturnCrossBossPlayerInfoFromCross(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossBossPlayerInfoFromCross");

            Client client = Api.ZoneManager.GetClient(pks.PcUid);
            if (client != null)
            {
                client.Write(pks);
            }
            else
            {
                Log.Warn($"player {uid} ReturnCrossBossPlayerInfoFromCross not find client ");
            }
        }

        public void OnResponse_StopCrossBossDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_STOP_CROSS_BOSS_DUNGEON pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_STOP_CROSS_BOSS_DUNGEON>(stream);
            Log.Write($"player {uid} StopCrossBossDungeon from main {MainId} ");

            MSG_RZ_STOP_CROSS_BOSS_DUNGEON msg = new MSG_RZ_STOP_CROSS_BOSS_DUNGEON();
            msg.DungeonId = pks.DungeonId;
            msg.Uid = pks.Uid;
            Api.ZoneManager.Broadcast(msg);
        }

        public void OnResponse_SendCrossBossPassReward(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_BOSS_PASS_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_BOSS_PASS_REWARD>(stream);
            Log.Write($"player {uid} SendCrossBossPassReward from main {MainId} ");

            Api.GameDBPool.Call(new QuerySetCrossBossPassReward(pks.DungeonId));

            MSG_RZ_CROSS_BOSS_PASS_REWARD msg = new MSG_RZ_CROSS_BOSS_PASS_REWARD();
            msg.DungeonId = pks.DungeonId;
            Api.ZoneManager.Broadcast(msg);
        }

        public void OnResponse_SendCrossBossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_BOSS_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_BOSS_RANK_REWARD>(stream);
            Log.Write($"player {uid} SendCrossBossRankReward( from main {MainId} ");

            Api.GameDBPool.Call(new QuerySetCrossBossRankReward(pks.DungeonId));

            MSG_RZ_CROSS_BOSS_RANK_REWARD msg = new MSG_RZ_CROSS_BOSS_RANK_REWARD();
            msg.DungeonId = pks.DungeonId;
            Api.ZoneManager.Broadcast(msg);
        }


        public void OnResponse_ReturnNotesList(MemoryStream stream, int uid = 0)
        {
            MSG_ZGC_CROSS_NOTES_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_CROSS_NOTES_LIST>(stream);
            Log.Write("cross server ReturnNotesList");

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(pks);
            }
            else
            {
                Log.Warn($"player {uid} ReturnNotesList not find client ");
            }
        }
    }
}

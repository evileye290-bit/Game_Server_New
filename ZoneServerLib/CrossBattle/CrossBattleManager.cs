using CommonUtility;
using DBUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CrossBattleManager
    {
        //private ZoneServerApi server { get; set; }
        public int TeamId { get; set; }
        public int FirstStartTime { get; set; }

        public DateTime StartTime { get; set; }

        //private Dictionary<int, PlayerRankBaseInfo> tempBattleList = new Dictionary<int, PlayerRankBaseInfo>();
        ////uid, rank
        //private Dictionary<int, List<PlayerRankBaseInfo>> battleList = new Dictionary<int, List<PlayerRankBaseInfo>>();

        //public CrossBattleManager(ZoneServerApi server)
        //{
        //    this.server = server;
        //}
        //public void OnUpdate(double deltaTime)
        //{
        //    if (battleList.Count > 0)
        //    {
        //        foreach (var item in battleList)
        //        {
        //            //进入战斗

        //            //发送结果
        //            MSG_ZR_GET_CROSS_BATTLE_RESULT msg = new MSG_ZR_GET_CROSS_BATTLE_RESULT();
        //            int rand = NewRAND.Next(1, 2);
        //            if (rand== 1)
        //            {
        //                msg.PcUid = item.Value[0].Uid;
        //            }
        //            else
        //            {
        //                msg.PcUid = item.Value[1].Uid;
        //            }
        //            server.RelationServer.Write(msg, msg.PcUid);
        //        }

        //        battleList.Clear();
        //    }
        //}

        //public void AddPlayerRankInfo(int uid, ServerModels.PlayerRankBaseInfo info1, ServerModels.PlayerRankBaseInfo info2)
        //{
        //    if (info1.Uid == 0 || info2.Uid == 0)
        //    {
        //        Log.Write("AddPlayerRankInfo add uid1 {0} uid2 {1} : out of rank ", info1.Uid, info2.Uid);
        //        return;
        //    }

        //    List<ServerModels.PlayerRankBaseInfo> list;
        //    //排名不重复
        //    if (battleList.TryGetValue(uid, out list))
        //    {
        //        Log.Warn("AddPlayerRankInfo add uid1 {0} uid2 {1} error: uid has add", info1.Uid, info2.Uid);
        //    }
        //    else
        //    {
        //        list = new List<ServerModels.PlayerRankBaseInfo>();
        //        list.Add(info1);
        //        list.Add(info2);
        //        battleList[uid] = list;
        //    }
        //}

        //public void AddTempBattleInfo(int uid, ServerModels.PlayerRankBaseInfo info)
        //{
        //    if (info.Uid == 0)
        //    {
        //        Log.Write("AddTempBattleInfo add uid1 {0} uid2 {1} : out of rank ", uid, info.Uid);
        //        return;
        //    }

        //    //排名不重复
        //    if (tempBattleList.ContainsKey(uid))
        //    {
        //        Log.Warn("AddTempBattleInfo add uid1 {0} uid2 {1} error: uid has add", uid, info.Uid);
        //    }
        //    else
        //    {
        //        tempBattleList[uid] = info;
        //    }
        //}

        //public PlayerRankBaseInfo GetTempBattleInfoByUid(int uid)
        //{
        //    ServerModels.PlayerRankBaseInfo info;
        //    tempBattleList.TryGetValue(uid, out info);
        //    return info;
        //}

        //public void RemoveTempBattleInfo(int uid)
        //{
        //    //排名不重复
        //    tempBattleList.Remove(uid);
        //}


        //double updatedeAllRankTime = 0;
        //private bool CheckUpdateAllRankTip(double deltaTime)
        //{
        //    updatedeAllRankTime += (float)deltaTime;
        //    if (updatedeAllRankTime < CrossBattleLibrary.RankRefreshTime)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        updatedeAllRankTime = 0;
        //        return true;
        //    }
        //}
    }
}

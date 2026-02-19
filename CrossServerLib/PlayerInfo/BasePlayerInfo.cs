//using Logger;
//using RedisUtility;
//using ServerModels;
//using ServerShared;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CrossServerLib.PlayerInfo
//{
//    public class BasePlayerInfo
//    {
//        private CrossServerApi server { get; set; }

//        private Dictionary<int, RedisPlayerInfo> playerList = new Dictionary<int, RedisPlayerInfo>();

//        public BasePlayerInfo(CrossServerApi server)
//        {
//            this.server = server;
//        }

//        public void RefreshPlayerList(List<int> uids)
//        {
//            if (uids != null && uids.Count > 0)
//            {
//                Dictionary<int, int> updateUids = new Dictionary<int, int>();
//                foreach (var uid in uids)
//                {
//                    RedisPlayerInfo info = GetPlayerInfo(uid);
//                    if (info == null)
//                    {
//                        updateUids[uid] = 0;
//                    }
//                    else
//                    {
//                        if (CheckNeedUpdateTime(info))
//                        {
//                            //需要更新,通知服务器获取信息



//                            continue;
//                        }
//                    }
//                }
//                RefreshPlayerListByRedis(updateUids.Keys.ToList());
//            }
//        }

//        private void RefreshPlayerListByRedis(List<int> updateUids)
//        {
//            if (updateUids.Count > 0)
//            {
//                //获取数据
//                OperateGetCrossPlayerBaseInfo operatePlayerInfo = new OperateGetCrossPlayerBaseInfo(updateUids);
//                server.CrossRedis.Call(operatePlayerInfo, ret =>
//                {
//                    if ((int)ret == 1)
//                    {
//                        foreach (var kv in operatePlayerInfo.Players)
//                        {
//                            AddPlayerInfo(kv.Key, kv.Value);
//                        }
//                    }
//                    else
//                    {
//                        Log.Warn("UpdatePlayerListByRedis OperateGetPlayerInfoList get data error");
//                    }
//                });
//            }
//        }

//        public void RefreshPlayerByRedis(int uid)
//        {
//            //获取数据, 通知服务器获取


//            //OperateGetPlayerInfo operatePlayerInfo = new OperateGetPlayerInfo(uid);
//            //server.GameRedis.Call(operatePlayerInfo, ret =>
//            //{
//            //    if ((int)ret == 1)
//            //    {
//            //        AddPlayerInfo(uid, operatePlayerInfo.Info);
//            //    }
//            //    else
//            //    {
//            //        Log.Warn("RefreshPlayerByRedis OperateGetPlayerInfo get data error");
//            //    }
//            //});
//        }

//        public bool CheckNeedUpdateTime(RedisPlayerInfo info)
//        {
//            if ((server.Now() - info.UpdateTime).TotalSeconds > DataInfoLibrary.BaseInfoUpdateTime)
//            {
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public void AddPlayerInfo(int uid, RedisPlayerInfo info)
//        {
//            if (info != null)
//            {
//                info.UpdateTime = server.Now();
//                playerList[uid] = info;
//            }
//        }

//        public RedisPlayerInfo GetPlayerInfo(int uid)
//        {
//            RedisPlayerInfo info;
//            if (playerList.TryGetValue(uid, out info))
//            {
//                if (CheckNeedUpdateTime(info))
//                {
//                    RefreshPlayerByRedis(uid);
//                }
//            }
//            else
//            {
//                RefreshPlayerByRedis(uid);
//            }
//            return info;
//        }

//        public bool CheckUpdatePlayerInfo(int uid, bool update = false)
//        {
//            RedisPlayerInfo info = GetPlayerInfo(uid);
//            if (info == null || update)
//            {
//                RefreshPlayerByRedis(uid);
//                return true;
//            }
//            else
//            {
//                if (CheckNeedUpdateTime(info))
//                {
//                    RefreshPlayerByRedis(uid);
//                }
//            }
//            return false;
//        }

//    }
//}

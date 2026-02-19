using CommonUtility;
using DataProperty;
using DBUtility;
using Engine;
using EnumerateUtility;
using Logger;
using Message.IdGenerator;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ManagerServerLib
{
    public class ZoneServerManager : FrontendServerManager
    {
        public OnlinePlayerState OnlinePlayerState
        { get; set; }
        // ZoneManager 管理某一mainId下的所有Zone，负责该mainId下的负载均衡，切线等业务
        public ZoneServerManager(ManagerServerApi api, ServerType serverType):base(api, serverType)
        {
            OnlinePlayerState = OnlinePlayerState.NORMAL;
            OnlineCount = 0;

            TaskTimerQuery counterTimer = new CrossBattleTimerQuery(10000);
            Log.Info($"ZoneServerManager call timing task ：after 10000");
            api.TaskTimerMng.Call(counterTimer, ret1 =>
            {
                api.UpdateMyCardCount();
            });
        }

        // <mapId_channel, ZServer> 找到指定map id 指定channel 的zone
        private Dictionary<string, ZoneServer> mapChannelZone = new Dictionary<string, ZoneServer>();
        public Dictionary<string, ZoneServer> MapChannelZone
        { get { return mapChannelZone; } }

        public Dictionary<int, OfflineClient> OfflineClientList = new Dictionary<int, OfflineClient>();

        public DateTime ServerOpenTime = DateTime.MinValue;
        public int OnlineCount
        { get; set; }

        public override void AddServer(FrontendServer server)
        {
            base.AddServer(server);

            InitServerOpenTime();
        }

        public void InitServerOpenTime()
        {
            DataList serverList = DataListManager.inst.GetDataList("ServerList");
            Data openTimeData = serverList.Get(api.MainId);
            if (openTimeData != null)
            {
                ServerOpenTime = DateTime.Parse(openTimeData.GetString("openTime"));
            }
        }

        public override void DestroyServer(FrontendServer server)
        {
            lock(allServersLock)
            {
                try
                {
                    ZoneServer zone = (ZoneServer)server;
                    RemoveOfflineClients(zone);
                }
                catch (Exception e)
                {
                    Logger.Log.ErrorLine("remove zone server failed: {0}", e.ToString());
                }
            }
            base.DestroyServer(server);
        }

        // private ListMap<int, int> mapDynamicChannel = new ListMap<int, int>();
        //
        // private bool IsDynamicChannel(int mapId, int channelId)
        // {
        //     List<int> dynamicList;
        //     if (mapDynamicChannel.TryGetValue(mapId, out dynamicList) && dynamicList?.Contains(channelId) == true)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }

        // 找到包含map channel的zone
        public ZoneServer GetZone(int map_id, int channel)
        {
            ZoneServer zone;
            string key = string.Format("{0}_{1}", map_id.ToString(), channel.ToString());
            foreach(var item in serverList)
            {
                if (ZoneTransformManager.Instance.IsForbided(item.Value.SubId))
                {
                    continue;
                }

                zone = item.Value as ZoneServer;
                if (zone.GetMap(key) != null)
                {
                    return zone;
                }
            }
            return null; 
        }


        // 根据map id channel 找到一个最适宜的zone和map
        /*
         1. 优先按照客户端需求分配指定channel的地图
         2. 指定channel的map满员的情况下，在该channel所在的zone试图分配一个<UniformNum的其他channel的map
         3. 指定channel的zone下所有该id的地图均超过UniformNum, 则遍历该map下的所有map 找到一个人数最低且<MaxPC的map
         4. 以上均不满足 分配失败
         */
        public bool GetZone(int map_id, int channel, out ZoneServer zone, out Map map, bool force_enter = false)
        {
            zone = GetZone(map_id, channel);
            
            // if (IsDynamicChannel(map_id, channel))
            // {
            //     zone = null;
            // }
            
            if (zone != null)
            {
                if (zone.State == ServerShared.ServerState.Started)
                {
                    map = zone.CanEnterMap(map_id, channel, force_enter);
                    if (map != null)
                    {
                        // 指定map channel可进入 返回
                        return true;
                    }
                    else
                    {
                        if (force_enter)
                        {
                            // 如果要求强制进入 没找到map则直接返回失败 不再寻找其他线
                            return false;
                        }
                        // 指定map不可入 尝试找到人数<UniformNum的其他map
                        map = zone.FindOneMap(map_id, false);
                        if (map != null)
                        {
                            // 该zone下有其他channel可进入
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (force_enter)
                {
                    // 对于只进该图情况 直接返回失败
                    map = null;
                    return false;
                }
                // 未找到挂在该channel的map 尝试找到一个Uniform的map
                foreach (var zoneItem in ServerList)
                {
                    if (ZoneTransformManager.Instance.IsForbided(zoneItem.Value.SubId))
                    {
                        continue;
                    }

                    map = ((ZoneServer)zoneItem.Value).FindOneMap(map_id, false);
                    if (map != null)
                    {
                        // 该zone下有其他channel可进入
                        zone = ((ZoneServer)zoneItem.Value);
                        return true;
                    }
                }
            }

            // 所有channel均达到Uniform 则进行排序 找到人数最低的channel进入
            Data mapData = ((ManagerServerApi)Api).MapDataList.Get(map_id);
            zone = null;
            map = null;
            bool isFull = false;
            int minCount = 0xFFFFFF;

            // foreach (var zoneItem in ServerList)
            // {
            //     foreach (var item in ((ZoneServer)zoneItem.Value).AllMap)
            //     {
            //         if (item.Value.MapId == map_id && item.Value.State == MapState.OPEN && !IsDynamicChannel(item.Value.MainId, item.Value.Channel))
            //         {
            //             int clientCount = item.Value.ClientCount;
            //             if (clientCount < minCount && clientCount < item.Value.UniformNum)
            //             {
            //                 zone = ((ZoneServer)zoneItem.Value);
            //                 map = item.Value;
            //                 minCount = clientCount;
            //             }
            //         }
            //     }
            // }

            if (zone == null || map == null)
            {
                foreach (var zoneItem in ServerList)
                {
                    if (zoneItem.Value.State != ServerState.Started || ZoneTransformManager.Instance.IsForbided(zoneItem.Value.SubId))
                    {
                        continue;
                    }

                    foreach (var item in ((ZoneServer)zoneItem.Value).AllMap)
                    {
                        if (item.Value.MapId == map_id && item.Value.State == MapState.OPEN)
                        {
                            int clientCount = item.Value.ClientCount;
                            if (clientCount < minCount && clientCount < item.Value.MaxNum)
                            {
                                zone = ((ZoneServer)zoneItem.Value);
                                map = item.Value;
                                minCount = clientCount;
                            }
                            else
                            {
                                //因为人数满了导致无法分配zone、map
                                isFull |= true;
                            }
                        }
                    }
                }
            }

            //因为人数满了导致无法分配zone、map时动态扩展创建map、channel，其他情况不需要动态扩展
            if (isFull && map == null && zone == null)
            {
                map = CreateMapDynamic(map_id, out zone);
            }

            if (map != null && zone != null)
            {
                return true;
            }

            return false;
        }

        public Map CreateMapDynamic(int mapId, out ZoneServer zone)
        {
            zone = null;

            try
            {
                int maxChannel = 1;
                int lastZoneClientCount = 0;
                ZoneServer aimZone = null;

                foreach (var kv in serverList)
                {
                    int clientNum = 0;
                    bool thisZoneContainMap = false;

                    //找到包含该地图的zone
                    ZoneServer zoneServer = kv.Value as ZoneServer;
                    zoneServer.AllMap.ForEach(x =>
                    {
                        if (x.Value.MapId == mapId)
                        {
                            clientNum += x.Value.ClientCount;
                            thisZoneContainMap = true;
                            maxChannel = Math.Max(maxChannel, x.Value.Channel);
                        }
                    });

                    if (aimZone == null && thisZoneContainMap)
                    {
                        aimZone = zoneServer;
                    }

                    //找出人数最少的zone
                    if (clientNum < lastZoneClientCount || lastZoneClientCount == 0)
                    {
                        lastZoneClientCount = clientNum;
                        aimZone = zoneServer;
                    }
                }

                if (aimZone != null)
                {
                    zone = aimZone;

                    //新增channel来构建map
                    int channel = maxChannel + 1;
                    Map map = aimZone.AddMap(mapId, channel);
                    
                    // mapDynamicChannel.Add(mapId, channel);

                    MSG_MZ_CREATE_MAP msg = new MSG_MZ_CREATE_MAP() { MapId = mapId, Channel = channel };
                    aimZone.Write(msg);

                    return map;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            zone = null;
            return null;
        }

        // 找到一个适合创建副本的zone
        public ZoneServer FindOneDungeonServer()
        {
            // 总是试图分配在承载最低的zone
            ZoneServer dungeonServer = null;
            foreach(var item in serverList)
            {
                //跨图拦截，到该zone的全部拦截
                if (ZoneTransformManager.Instance.IsForbided(item.Value.SubId))
                {
                    continue;
                }

                ZoneServer zone = item.Value as ZoneServer;
                if(dungeonServer == null)
                {
                    dungeonServer = zone;
                    continue;
                }
                if(zone.FrameCount > dungeonServer.FrameCount)
                {
                    dungeonServer = zone;
                }
            }
            return dungeonServer;
        }

        public Client GetClient(int character_uid)
        { 
            Client client = null;
            foreach(var item in serverList)
            {
                if(((ZoneServer)item.Value).ClientListZone.TryGetValue(character_uid, out client))
                {
                    return client; 
                }
            }
            return null;
        }

        public Vec2 GetBeginPosition(int mapID, CampType camp_type)
        {
            Vec2 pos = new Vec2(Vec2.zero.X, Vec2.zero.Y);// Vec2.zero;
            Data mapData = DataListManager.inst.GetData("Zone", mapID);
            if (mapData != null)
            {
                float x = 0, y = 0;
                PvpType pvpType = (PvpType) mapData.GetInt("PVPType");
                if (pvpType == PvpType.None)
                {
                    x = mapData.GetFloat("BeginPosX");
                    y = mapData.GetFloat("BeginPosY");
                    pos = new Vec2(x, y);
                }
                else
                {
                    if (camp_type == CampType.TianDou || camp_type == CampType.None)
                    {
                        x = mapData.GetFloat("BeginPosX");
                        y = mapData.GetFloat("BeginPosX");
                        pos = new Vec2(x, y);
                    }
                    else
                    {
                        x = mapData.GetFloat("BeginPosX");
                        y = mapData.GetFloat("BeginPosX");
                        pos = new Vec2(x, y);
                    }
                }
            }
            return pos;
        }

        public void PullPlayer(int uid, int map, int channel, CampType camp, bool team_limit, Vec2 beginPos = null)
        {       
            Client client = GetClient(uid);
            if (client == null || client.Zone == null)
            {
                Log.Write("pull player {0} to map {1} channel {2} failed: zone is null", uid, map, channel);
            }
            if (beginPos == null)
            {
                beginPos = GetBeginPosition(map, camp);
            }
            MSG_MZ_PULL_PLAYER notify = new MSG_MZ_PULL_PLAYER();
            notify.Uid = uid;
            notify.BeginPosX = beginPos.X;
            notify.BeginPosY = beginPos.Y;
            notify.TeamLimit = team_limit;
            notify.MapId = map;
            notify.Channel = channel;
            client.Zone.Write(notify);
        }

       public static void BroadCastMsgMemoryMaker<T>(T msg, out ArraySegment<byte> first, out ArraySegment<byte> second) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            Tcp.MakeArray(header,body,out first,out second);
        }

        public ZoneServer GetClientZone(int uid)
        {
            ZoneServer zServer = null;
            Client client = GetClient(uid);
            if (client != null)
            {
                zServer = client.Zone;
            }
            else
            {
                OfflineClient offlineClient = GetOfflineClient(uid);
                if (offlineClient != null)
                {
                    zServer = (ZoneServer)GetServer(api.MainId, offlineClient.SubId);
                }
            }
            return zServer;
        }

        public int GetOnlineCount()
        {
            return OnlineCount;
            /*
            int totalCount = 0;
            foreach (var item in ZoneList)
            {
                totalCount += item.Value.ClientListZone.Count;
            }
            return totalCount;
            */
        }

        public void CheckOnlinePlayerState()
        { 
            if (OnlineCount < CONST.ONLINE_COUNT_WAIT_COUNT)
            {
                if (OnlinePlayerState != OnlinePlayerState.NORMAL)
                {
                    OnlinePlayerState = OnlinePlayerState.NORMAL;
                    NotifyOnlinePlayerState();
                }
            }
            else if (OnlineCount < CONST.ONLINE_COUNT_FULL_COUNT )
            {
                if(OnlinePlayerState != OnlinePlayerState.WAIT)
                {
                    OnlinePlayerState = OnlinePlayerState.WAIT;
                    NotifyOnlinePlayerState();
                }
            }
            else if (OnlinePlayerState != ServerShared.OnlinePlayerState.FULL)
            { 
                OnlinePlayerState = OnlinePlayerState.FULL;
                NotifyOnlinePlayerState();
            }
        }

        private void NotifyOnlinePlayerState()
        {
            // TODO 在线状态 排队相关需要重新对接 
        }

        public OfflineClient GetOfflineClient(int uid)
        { 
            OfflineClient client = null;
            OfflineClientList.TryGetValue(uid, out client);
            return client;
        }

        public void RemoveOfflineClient(int uid)
        {
            OfflineClientList.Remove(uid);
        }

        public void AddOfflineClient(OfflineClient client)
        {
            if (client == null)
            {
                return;
            }
            OfflineClientList[client.Uid] = client;
        }

        public void RemoveOfflineClients(ZoneServer zone)
        {
            if (zone == null) return;
            List<int> removeList = new List<int>();
            foreach (var client in OfflineClientList)
            {
                if (client.Value.MainId == zone.MainId && client.Value.SubId == zone.SubId)
                {
                    removeList.Add(client.Key);
                }
            }
            for (int i = 0; i < removeList.Count; i++)
            {
                RemoveOfflineClient(removeList[i]);
            }
        }

    }
}

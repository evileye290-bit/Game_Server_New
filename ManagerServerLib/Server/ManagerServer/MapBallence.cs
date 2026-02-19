using ManagerServerLib;
using Message.Manager.Protocol.MM;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared.Map
{
    public class MapBallenceInfo
    {
        public int MapId;
        public int MinChannel;
        public int MaxChannel;
        public int UniformLeft = 0;
        public int MaxLeft = 0;
        // mapId_channel
        public List<string> FullChannelList = new List<string>();
        public bool IsFull(int map_id, int channel)
        {
            string key = string.Format("{0}_{1}", map_id.ToString(), channel.ToString());
            return FullChannelList.Contains(key);
        }
    }

    public class ManagerMapInfo
    {
        public int MainId = 0;
        public int OnlineCount = 0;
        // key map id
        public Dictionary<int, MapBallenceInfo> MapInfoList = new Dictionary<int, MapBallenceInfo>();
        public bool IsFull(int map_id, int channel)
        {
            MapBallenceInfo mapInfo = null;
            if (MapInfoList.TryGetValue(map_id, out mapInfo))
            {
                return mapInfo.IsFull(map_id, channel);
            }
            return true;
        }
        public bool IsHealthy()
        {
            return OnlineCount < MapBallenceProxy.BuzyOnlineCount;
        }
    }

    public class MapBallenceProxy
    {
        ManagerServerApi api;
        private DateTime lastBroadcastTime = ManagerServerApi.now;

        // key mainId
        public static int BuzyOnlineCount = 4000;
        public static int MaxForceLeft = 10;
        public Dictionary<int, ManagerMapInfo> ManagerList = new Dictionary<int, ManagerMapInfo>();
        // key map id, value, max channel info
        public Dictionary<int, MapChannelInfo> MapChannelInfoList = new Dictionary<int, MapChannelInfo>();

        public MapBallenceProxy(ManagerServerApi api)
        {
            this.api = api;
        }
        public void RecordBallenceInfo(int main_id, int online_count, List<MapBallenceInfo> info_list)
        {
            ManagerMapInfo managerInfo = null;
            if (ManagerList.TryGetValue(main_id, out managerInfo))
            {
                managerInfo.OnlineCount = online_count;
                managerInfo.MapInfoList.Clear(); 
                foreach(var item in info_list)
                {
                    managerInfo.MapInfoList.Add(item.MapId, item);
                }
            }
            else
            {
                managerInfo = new ManagerMapInfo();
                managerInfo.MainId = main_id;
                managerInfo.OnlineCount = online_count;
                foreach (var item in info_list)
                {
                    managerInfo.MapInfoList.Add(item.MapId, item);
                }
                ManagerList.Add(main_id, managerInfo);
            }
            foreach (var item in info_list)
            {
                MapChannelInfo channelInfo = null;
                if (MapChannelInfoList.TryGetValue(item.MapId, out channelInfo))
                {
                    if (channelInfo.MaxChannel < item.MaxChannel)
                    {
                        channelInfo.MaxChannel = item.MaxChannel;
                    }
                    if (channelInfo.MinChannel > item.MinChannel)
                    {
                        channelInfo.MinChannel = item.MinChannel;
                    }
                }
                else
                {
                    channelInfo = new MapChannelInfo(item.MapId, item.MinChannel, item.MaxChannel);
                    MapChannelInfoList.Add(item.MapId, channelInfo);
                }
            }
            //foreach (var item in ManagerList)
            //{
            //    managerInfo = item.Value;
            //    foreach (var map in managerInfo.MapInfoList.Values)
            //    {
            //        Logger.Log.Write("map id {0} min challen {1} max channel {2} uniform {3} max {4}",
            //            map.MapId, map.MinChannel, map.MaxChannel, map.UniformLeft, map.MaxLeft);
            //    }
            //}
        }

        public void RemoveManagerInfo(int main_id)
        { 
            ManagerList.Remove(main_id);
            MapChannelInfoList.Clear();
        }

        // 返回一个适合均衡负载的manager
        public ManagerMapInfo FindOneManager(int map_id)
        {
            // 第一遍 在负载不忙的manager里 招uniformLeft最小的manager
            int uniformLeftMin = int.MaxValue;
            ManagerMapInfo destManager = null;
            foreach(var item in ManagerList)
            {
                ManagerMapInfo manager = item.Value;
                if (manager.IsHealthy())
                {
                    foreach (var mapInfo in manager.MapInfoList.Values)
                    {
                        if (mapInfo.MapId == map_id && mapInfo.UniformLeft < uniformLeftMin && mapInfo.MaxLeft > MapBallenceProxy.MaxForceLeft)
                        {
                            uniformLeftMin = mapInfo.UniformLeft;
                            destManager = manager;
                            break;
                        }
                    }
                }
            }
            int minOnlineCount = int.MaxValue;
            if (destManager == null)
            { 
                // 不忙的manager均满或者所有manager均忙， 找负载最轻的manager路由过去
                foreach (var item in ManagerList)
                {
                    ManagerMapInfo manager = item.Value;
                    foreach (var mapInfo in manager.MapInfoList.Values)
                    {
                        if (mapInfo.MapId == map_id &&  manager.OnlineCount < minOnlineCount)
                        {
                            minOnlineCount = manager.OnlineCount;
                            destManager = manager;
                        }
                    }
                }
            }
            // 只有一种情况下返回null 就是所有manager没有挂载该map
            return destManager;
        }

        // 返回挂载制定map channel对应的manager 如果均为挂载该地图 则返回null
        public ManagerMapInfo FindTheManager(int map_id, int channel)
        {
            foreach (var item in ManagerList)
            {
                ManagerMapInfo manager = item.Value;
                MapBallenceInfo map = null;
                if (manager.MapInfoList.TryGetValue(map_id, out map))
                {
                    if (map.MinChannel <= channel && map.MaxChannel >= channel)
                    {
                        return manager;
                    }
                }
            }
            return null;
        }

        public void Update()
        {
            // 每10s向其他同步一次在线信息，用于均衡负载
                if ((ManagerServerApi.now - lastBroadcastTime).TotalSeconds >= 10)
                {
                    MSG_MM_ONLINE_INFO notify = new MSG_MM_ONLINE_INFO();
                    notify.MainId = api.MainId;
                    notify.OnlineCount = api.ZoneServerManager.OnlineCount;
                    Dictionary<int, MSG_MAP_BALLENCE_INFO> mapInfoList = new Dictionary<int,MSG_MAP_BALLENCE_INFO>();
                    foreach (var item in api.ZoneServerManager.ServerList.Values)
                    {
                        foreach (var map in ((ZoneServer)item).AllMap.Values)
                        {
                            MSG_MAP_BALLENCE_INFO mapInfo = null;
                            int maxLeft = map.MaxNum - map.ClientCount;
                            int uniformLeft = map.UniformNum - map.ClientCount;
                            if (uniformLeft < 0)
                            {
                                uniformLeft = 0;
                            }
                            if (mapInfoList.TryGetValue(map.MapId, out mapInfo))
                            {
                                mapInfo.MaxLeft += maxLeft;
                                mapInfo.UniformLeft += uniformLeft;
                                if (mapInfo.MinChannel > map.Channel)
                                {
                                    mapInfo.MinChannel = map.Channel;
                                }
                                if (mapInfo.MaxChannel < map.Channel)
                                {
                                    mapInfo.MaxChannel = map.Channel;
                                }
                            }
                            else
                            {
                                mapInfo = new MSG_MAP_BALLENCE_INFO();
                                mapInfo.MapId = map.MapId;
                                mapInfo.MaxLeft = maxLeft;
                                mapInfo.UniformLeft = uniformLeft;
                                mapInfo.MinChannel = map.Channel;
                                mapInfo.MaxChannel = map.Channel;
                                mapInfoList.Add(map.MapId, mapInfo);
                            }
                            if (maxLeft <= MapBallenceProxy.MaxForceLeft)
                            {
                                mapInfo.FullChannelList.Add(map.GetKey());
                            }
                        }
                    }
                    foreach (var item in mapInfoList)
                    {
                        notify.MapList.Add(item.Value);
                    }
                    List<MapBallenceInfo> mapList = new List<MapBallenceInfo>();
                    foreach (var item in notify.MapList)
                    {
                        MapBallenceInfo info = new MapBallenceInfo();
                        info.MapId = item.MapId;
                        info.UniformLeft = item.UniformLeft;
                        info.MaxLeft = item.MaxLeft;
                        info.MaxChannel = item.MaxChannel;
                        info.MinChannel = item.MinChannel;
                        info.FullChannelList.AddRange(item.FullChannelList);
                        mapList.Add(info);
                    }
                    api.BallenceProxy.RecordBallenceInfo(notify.MainId, notify.OnlineCount, mapList);
                    MSG_MZ_MAP_CHANNEL_INFO notifyZone = new MSG_MZ_MAP_CHANNEL_INFO();
                    foreach (var item in mapList)
                    {
                        MapChannelInfo channelInfo = null;
                        if (api.BallenceProxy.MapChannelInfoList.TryGetValue(item.MapId, out channelInfo))
                        {
                            MSG_MAP_CHANNEL_INFO info = new MSG_MAP_CHANNEL_INFO();
                            info.MapId = channelInfo.MapId;
                            info.MaxChannel = channelInfo.MaxChannel;
                            info.MinChannel = channelInfo.MinChannel;
                            notifyZone.InfoList.Add(info);
                        }
                    }
                    api.ZoneServerManager.Broadcast(notifyZone);
                    api.BroadcastToAllManagers(notify);
                    lastBroadcastTime = ManagerServerApi.now;
                }
        }
    }
}

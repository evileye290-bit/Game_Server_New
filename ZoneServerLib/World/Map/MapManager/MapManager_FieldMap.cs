using System.Collections.Generic;
using System;
using DataProperty;
using CommonUtility;
using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    public partial class MapManager
    {

        private Dictionary<int, Dictionary<int, FieldMap>> fieldMapList = new Dictionary<int, Dictionary<int, FieldMap>>();
        public Dictionary<int, Dictionary<int, FieldMap>> FieldMapList
        {
            get { return fieldMapList; }
        }

        List<FieldMap> removeMapList = new List<FieldMap>();

        /// <summary>
        /// 传送门地图相邻关系
        /// </summary>
        Dictionary<int, Dictionary<int, int>> mapNeighbors = new Dictionary<int, Dictionary<int, int>>();

        private void InitFieldMapList()
        {
            string fileName = "Zone_" + server.SubId;
            DataList zoneManagerList = DataListManager.inst.GetDataList(fileName);

            foreach (var zoneManager in zoneManagerList)
            {
                Data zoneManagerData = zoneManager.Value;
                int mapId = zoneManagerData.ID;
                string[] channelList = zoneManagerData.GetString("channel").Split(';');

                foreach (var channelValue in channelList)
                {
                    int channel = int.Parse(channelValue);
                    FieldMap map = new FieldMap(server, mapId, channel);
                    AddMap(map);
                }
            }
        }

        public void AddMap(int mapId, int channel)
        { 
            Dictionary<int, FieldMap> mapChannelList;
            if (fieldMapList.TryGetValue(mapId, out mapChannelList))
            {
                if (mapChannelList.ContainsKey(channel)) return;
            }

            FieldMap map = new FieldMap(server, mapId, channel);
            AddMap(map);
        }

        public void AddMap(FieldMap map)
        {
            if (map == null) return;
            Dictionary<int, FieldMap> mapChannelList;
            if (!fieldMapList.TryGetValue(map.MapId, out mapChannelList))
            {
                mapChannelList = new Dictionary<int, FieldMap>();
                fieldMapList.Add(map.MapId, mapChannelList);
            }
            mapChannelList.Add(map.Channel, map);
        }

        public void RemoveMap(FieldMap map)
        {
            removeMapList.Add(map);
        }

        private void InitMapNeighbors()
        {
            DataList zoneNpcDataList = DataListManager.inst.GetDataList("ZoneNPC");
            foreach (var zoneNpcData in zoneNpcDataList)
            {
                string paramStrring = zoneNpcData.Value.GetString("param");
                if (!string.IsNullOrEmpty(paramStrring))
                {
                    if (paramStrring.Contains(NpcParamType.FLY_MAP_ID))
                    {
                        string[] param = StringSplit.GetArray("||", paramStrring);
                        foreach (var item in param)
                        {
                            string[] split = StringSplit.GetArray(":", item);
                            if (split.Length > 1)
                            {
                                if (split[0] == NpcParamType.FLY_MAP_ID)
                                {
                                    int mapId = zoneNpcData.Value.GetInt("ZoneId");
                                    int canReachMapId = int.Parse(split[1]);
                                    AddMapNeighbors(mapId, canReachMapId);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddMapNeighbors(int mapId, int canReachMapId)
        {
            Dictionary<int, int> mapIds;
            if (mapNeighbors.TryGetValue(mapId, out mapIds))
            {
                mapIds.Add(canReachMapId, 0);
            }
            else
            {
                mapIds = new Dictionary<int, int>();
                mapIds.Add(canReachMapId, 0);
                mapNeighbors.Add(mapId, mapIds);
            }
        }

        public Dictionary<int, int> GetMapNeighbors(int mapId)
        {
            Dictionary<int, int> neighbors;
            mapNeighbors.TryGetValue(mapId, out neighbors);
            return neighbors;
        }

        /// <summary>
        /// 更新所有地图、与原方法选其一
        /// </summary>
        /// <param name="dt"></param>
        void UpdateFieldMaps(float dt)
        {
            foreach (var fieldmap in fieldMapList)
            {
                foreach (var item in fieldmap.Value)
                {
                    try
                    {
                        var map = item.Value;
                        if (map == null)
                        {
                            Log.Warn("map {0} channel {1} in mapList is null check it!", fieldmap.Key, item.Key);
                            continue;
                        }
                        map.Update(dt);
                    }
                    catch (Exception e)
                    {
                        Log.Alert("field map manager update all map error: {0}", e.ToString());
                    }
                }
            }
            foreach(var map in removeMapList)
            {
                try
                {
                    Dictionary<int, FieldMap> mapList = null;
                    if(fieldMapList.TryGetValue(map.MapId, out mapList))
                    {
                        mapList.Remove(map.Channel);
                    }
                }
                catch(Exception e)
                {
                    Log.Alert("remove map failed {0}", e.ToString());
                }
            }
            removeMapList.Clear();

        }

        public FieldMap GetFieldMap(int mapID, int channel)
        {
            FieldMap map = null;
            Dictionary<int, FieldMap> mapChannelList = GetAllFieldMaps(mapID);
            if (mapChannelList != null)
            {
                mapChannelList.TryGetValue(channel, out map);
            }
            return map;
        }

        public Dictionary<int, FieldMap> GetAllFieldMaps(int mapID)
        {
            Dictionary<int, FieldMap> mapChannelList;
            if (fieldMapList.TryGetValue(mapID, out mapChannelList))
            {
                return mapChannelList;
            }
            return null;
        }

    }
}
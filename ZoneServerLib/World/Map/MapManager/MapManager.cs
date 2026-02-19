using System.Collections.Generic;
using System;
using DataProperty;
using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class MapManager
    {
        private ZoneServerApi server;

        public void Init(ZoneServerApi server)
        {

            this.server = server;

            InitFieldMapList();

            InitMapNeighbors();
        }


        public void Update(double dt)
        {
            float deltaTime = (float)(dt * 0.001f);

            UpdateFieldMaps(deltaTime);
        }


        /// <summary>
        /// 取得地图的初始位置
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public Vec2 GetBeginPosition(int mapId)
        {
            MapModel model = MapLibrary.GetMap(mapId);
            if (model != null)
            {
                return new Vec2(model.BeginPos);
            }

            return new Vec2(0, 0);
        }


        public int GetTotalChannel(int mapId)
        {
            int count = 0;
            server.MapChannelCount.TryGetValue(mapId, out count);
            return count;
        }

        // 根据当前负载和该map负载情况，调整要进入的实际channel
        public bool TryAdjustChannel(int mapId, out int channel)
        {
            channel = 1;
            MapModel model = MapLibrary.GetMap(mapId);
            if(model == null)
            {
                return false;
            }
            if (server.Fps.GetFrame() < GameConfig.ADJUST_CHANNEL_FRAME)
            {
                return false;
            }
            Dictionary<int, FieldMap> mapList = GetAllFieldMaps(mapId);
            if (mapList == null)
            {
                return false;
            }
            foreach(var map in mapList)
            {
                if(map.Value.PcList.Count < model.UniformNum)
                {
                    channel = map.Value.Channel;
                    return true;
                }
            }
            return false;
        }

        public bool CanEnterMap(int mapId, int channel)
        {
            FieldMap map = GetFieldMap(mapId, channel);
            if(map == null)
            {
                return false;
            }
            if(map.CanEnter())
            {
                return true;
            }
            return false;
        }

    }
}
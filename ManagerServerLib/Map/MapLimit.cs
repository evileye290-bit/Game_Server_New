using System;
using System.Collections.Generic;
using ServerShared;
using DataProperty;
using Logger;
using Message.Manager.Protocol.MZ;

namespace ManagerServerLib
{
    public class MapLimit
    {
        private int mapId;
        public int MapId
        {
            get { return mapId; }
            set { mapId = value; }
        }

        private DateTime openTime;
        public DateTime OpenTime
        {
            get { return openTime; }
            set { openTime = value; }
        }

        private DateTime closeTime;
        public DateTime CloseTime
        {
            get { return closeTime; }
            set { closeTime = value; }
        }

        private MapState state;
        public MapState State
        {
            get { return state; }
            set { state = value; }
        }

        private Data config;
        private ManagerServerApi server;

        public MapLimit(Data config, ManagerServerApi server)
        {
            this.config = config;
            this.server = server;
            mapId = config.ID;
            state = MapState.CLOSE;
            openTime = DateTime.Parse(config.GetString("openTime"));
            closeTime = DateTime.Parse(config.GetString("closeTime"));
            if (closeTime <= openTime)
            {
                // close <= open 说明跨天
                closeTime = closeTime.AddDays(1);
            }
            if (openTime <= ManagerServerApi.now && ManagerServerApi.now < closeTime)
            {
                state = MapState.OPEN;
            }
        }

        public void CheckTime()
        {
            if (state == MapState.OPEN && ManagerServerApi.now >= closeTime)
            {
                // 到点关闭地图 明天再开
                state = MapState.CLOSE;
                openTime = openTime.AddDays(1);
                closeTime = closeTime.AddDays(1);
                Log.Write("map {0} will close", mapId);
                // 通知相应zone关闭地图
                MSG_MZ_CLOSE_MAP notify = new MSG_MZ_CLOSE_MAP();
                notify.MapId = mapId;
                server.ZoneServerManager.Broadcast(notify);
            }

            if (state == MapState.CLOSE && ManagerServerApi.now >= openTime && ManagerServerApi.now < closeTime)
            {
                // 到点开启 则开启
                state = MapState.OPEN;
                Log.Write("map {0} will open", mapId);
            }
        }

        public bool IsClosed()
        {
            return state == MapState.CLOSE;
        }
    }
}

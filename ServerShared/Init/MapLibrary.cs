using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class MapLibrary
    {
        private static Dictionary<int, MapModel> mapList = new Dictionary<int, MapModel>();
        private static Dictionary<string, GeoMapModel> geoList = new Dictionary<string, GeoMapModel>(); // 热更时不清空，地理信息无需热更

        public static void Init()
        {
            //mapList.Clear();
            Dictionary<int, MapModel> mapList = new Dictionary<int, MapModel>();
            Dictionary<string, GeoMapModel> geoList = new Dictionary<string, GeoMapModel>();
            DataList dataList = DataListManager.inst.GetDataList("Zone");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                GeoMapModel geoModel = GetMapGeoModel(geoList, data.GetString("fileName"));
                MapModel model = new MapModel(data, geoModel);
                mapList.Add(item.Key, model);
            }
            //TestJps();
            MapLibrary.mapList = mapList;
            MapLibrary.geoList = geoList;
        }

        public static GeoMapModel GetMapGeoModel(Dictionary<string, GeoMapModel> geoList, string geoName)
        {
            GeoMapModel model = null;
            if(geoList.TryGetValue(geoName, out model))
            {
                return model;
            }

            // 不存在
            model = new GeoMapModel(geoName);
            geoList.Add(geoName, model);
            return model;
        }

        public static MapModel GetMap(int mapId)
        {
            MapModel map = null;
            mapList.TryGetValue(mapId, out map);
            return map;
        }

        //public static void TestJps()
        //{
        //    long oldJpsTime = 0, newJpsTime = 0;
        //    foreach(var kv in mapList)
        //    {
        //        long tmpOld, tmpNew;
        //        kv.Value.RandomTestJps(100, out tmpOld, out tmpNew);
        //        Logger.Log.Warn($"map {kv.Value.MapId} old jps {tmpOld}, new jps {tmpNew} times {tmpOld / tmpNew}");
        //        oldJpsTime += tmpOld;
        //        newJpsTime += tmpNew;
        //    }

        //    Logger.Log.Warn($"total jps test old {oldJpsTime} new {newJpsTime} times {oldJpsTime / newJpsTime}");
        //}
    }
}

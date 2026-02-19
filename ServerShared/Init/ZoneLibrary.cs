using CommonUtility;
using DataProperty;
using Logger;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    static public class ZoneLibrary
    {
        private static Dictionary<int, List<Vec2>> zonePointList = new Dictionary<int, List<Vec2>>();

        public static Dictionary<int, List<Vec2>> ZonePointList
        {
            get { return zonePointList; }
        }

        public static void BindZonePoint()
        {
            //zonePointList = new Dictionary<int, List<Vec2>>();
            Dictionary<int, List<Vec2>> zonePointList = new Dictionary<int, List<Vec2>>();

            List<Vec2> pointList = new List<Vec2>();
            DataList dataList = DataListManager.inst.GetDataList("Zone");
            try
            {
                foreach (var zoneData in dataList)
                {
                    Data data = zoneData.Value;
                    string pointsStr = data.GetString("DungeonPos");
                    if (string.IsNullOrEmpty(pointsStr))
                    {
                        continue;
                    }
                    string[] pointsAry = StringSplit.GetArray("_", pointsStr);
                    pointList = new List<Vec2>();
                    foreach (string s in pointsAry)
                    {
                        string[] p_ary = s.Split(',');
                        float x = p_ary[0].ToFloat();
                        float z = p_ary[1].ToFloat();
                        Vec2 point = new Vec2(x, z);
                        pointList.Add(point);
                    }
                    if (pointList.Count > 0)
                    {
                        zonePointList.Add(data.ID, pointList);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Alert("[Error] BindZonePoint : " + e.ToString());
            }

            ZoneLibrary.zonePointList = zonePointList;
        }

        public static List<Vec2> GetPointList(int id)
        {
            List<Vec2> plist = new List<Vec2>();
            zonePointList.TryGetValue(id, out plist);
            return plist;
        }
    }
}

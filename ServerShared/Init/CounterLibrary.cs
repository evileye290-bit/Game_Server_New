using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataProperty;
using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Timing;
using ServerModels;

namespace ServerShared
{
    public class CounterLibrary
    {
        public static List<int> Ids = new List<int>();
        public static List<string> Names = new List<string>();

        private static Dictionary<CounterType, CounterModel> countModelList = new Dictionary<CounterType, CounterModel>();
        private static Dictionary<TimingType, List<CounterType>> refreshTypeList = new Dictionary<TimingType, List<CounterType>>();
        public static List<CounterType> CountParamList = new List<CounterType>();
        //private static Dictionary<CounterType, bool> needSyncList = new Dictionary<CounterType, bool>();
        //private static Dictionary<CounterType, int> maxCountList = new Dictionary<CounterType, int>();

        private static Dictionary<MapType, MapCounterModel> mapTypeToCounterList = new Dictionary<MapType, MapCounterModel>();
        
        public static void Init()
        {
            //needSyncList.Clear();
            //CountParamList.Clear();
            //refreshTypeList.Clear();
            //Ids.Clear();
            //Names.Clear();
            //mapTypeToCounterList.Clear();

            InitCounter();
            InitMapTypeToCounter();
        }

        private static void InitCounter()
        {
            List<int> Ids = new List<int>();
            List<string> Names = new List<string>();
            Dictionary<CounterType, CounterModel> countModelList = new Dictionary<CounterType, CounterModel>();
            Dictionary<TimingType, List<CounterType>> refreshTypeList = new Dictionary<TimingType, List<CounterType>>();
            List<CounterType> CountParamList = new List<CounterType>();

            CounterModel mdoel;
            List<CounterType> list = new List<CounterType>();
            DataList dataList = DataListManager.inst.GetDataList("Counter");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                mdoel = new CounterModel(data);
                countModelList[mdoel.Type] = mdoel;

                //刷新
                if (mdoel.Timing != TimingType.NoRefresh)
                {
                    //固定刷新机制
                    if (refreshTypeList.TryGetValue(mdoel.Timing, out list))
                    {
                        list.Add(mdoel.Type);
                    }
                    else
                    {
                        list = new List<CounterType>();
                        list.Add(mdoel.Type);
                        refreshTypeList[mdoel.Timing] = list;
                    }
                }

                if (mdoel.Params.Count > 0)
                {
                    CountParamList.Add(mdoel.Type);
                }

                //数据库
                if (mdoel.DbColumn)
                {
                    Ids.Add(item.Value.ID);
                    Names.Add(item.Value.Name);
                }
            }
            CounterLibrary.Ids = Ids;
            CounterLibrary.Names = Names;
            CounterLibrary.countModelList = countModelList;
            CounterLibrary.refreshTypeList = refreshTypeList;
            CounterLibrary.CountParamList = CountParamList;
        }

        private static void InitMapTypeToCounter()
        {
            Dictionary<MapType, MapCounterModel> mapTypeToCounterList = new Dictionary<MapType, MapCounterModel>();

            MapCounterModel model;
            DataList dataList = DataListManager.inst.GetDataList("MapTypeCounter");
            foreach (var item in dataList)
            {
                MapType type = (MapType)item.Value.ID;
                model = new MapCounterModel(item.Value);
                mapTypeToCounterList[type] = model;
            }
            CounterLibrary.mapTypeToCounterList = mapTypeToCounterList;
        }

        public static CounterModel GetCounterModel(CounterType type)
        {
            CounterModel model;
            countModelList.TryGetValue(type, out model);
            return model;
        }

        public static int GetMaxCount(CounterType type)
        {
            CounterModel model = GetCounterModel(type);
            if (model != null)
            {
                return model.MaxCount;
            }
            else
            {
                return 0;
            }
        }

        public static List<CounterType> GetRefreshCounter(TimingType type)
        {
            List<CounterType> list;
            refreshTypeList.TryGetValue(type, out list);
            return list;
        }

        /// <summary>
        /// 检查是否需要同步客户端
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckNeedSync(CounterType type)
        {
            CounterModel model = GetCounterModel(type);
            if (model != null)
            {
                return model.NeedSync;
            }
            else
            {
                return false;
            }
        }

        public static int GetBuyCountCost(string costStr, int currCount)
        {
            //1:100|3:200|6:1000，表示，第1、2次购买100钻、第3、4、5次购买200钻，第6次以上1000钻
            Dictionary<int, int> countCost = StringSplit.GetKVPairs(costStr);
            if (countCost.Count == 0)
            {
                return 0;
            }

            Dictionary<int, int> sorted = countCost.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            foreach (var kv in sorted)
            {
                if (currCount >= kv.Key)
                {
                    return kv.Value;
                }
            }

            return 0;
        }

        public static string GetUpdateSql(List<Counter> counters, int pcUid)
        {
            string sqlString = string.Empty;
            if (counters.Count > 0)
            {
                string parameter = string.Empty;
                foreach (var item in counters)
                {
                    parameter += string.Format(", `{0}` = {1}", item.Type.ToString(), item.Count);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);


                if (!string.IsNullOrEmpty(parameter))
                {
                    string sqlBase = @"	UPDATE `game_counter` SET  {0}  WHERE `uid` = {1};";
                    sqlString = string.Format(sqlBase, parameter, pcUid);
                }
            }
            return sqlString;
        }

        public static string GetSelectSql()
        {
            List<string> nameList = Names;
            string parameter = string.Empty;
            if (nameList.Count > 0)
            {
                foreach (var name in nameList)
                {
                    parameter += string.Format(", `{0}`", name);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);
            }
            return parameter;
        }

        public static MapCounterModel GetCounterType(MapType mapType)
        {
            MapCounterModel counterType;
            mapTypeToCounterList.TryGetValue(mapType, out counterType);
            return counterType;
        }

    }
}

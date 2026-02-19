using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class CampActivityTimerData
    {
        public int Id;
        public CampActivityType Type;
        private string BeginStr;
        private string EndTimeStr;

        public DayOfWeek BeginWeekDay;
        public int BeginHour;
        public int BeginMinute;
        public int BeginSecond;

        public DayOfWeek EndWeekDay;
        public int EndHour;
        public int EndMinute;
        public int EndSecond;

        public CampActivityTimerData(Data data)
        {
            Id = data.ID;
            Type = (CampActivityType)data.GetInt("CampActivityType");
            BeginStr = data.GetString("BeginTime");
            string[] beginArr = BeginStr.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            BeginWeekDay = (DayOfWeek)beginArr[0].ToInt();
            BeginHour = beginArr[1].ToInt();
            BeginMinute = beginArr[2].ToInt();
            BeginSecond = beginArr[3].ToInt();

            EndTimeStr = data.GetString("EndTime");
            string[] endArr = EndTimeStr.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            EndWeekDay = (DayOfWeek)endArr[0].ToInt();
            EndHour = endArr[1].ToInt();
            EndMinute = endArr[2].ToInt();
            EndSecond = endArr[3].ToInt();
        }
    }

    public class CampFortData
    {
        public int Id;
        public CampType CampType;
        public List<int> RelationForts;
        public int BossDungeonId;
        public int DefenderDungeonId;
        public List<int> FollowerDungoenIdList;
        public Dictionary<int, int> AddNaturesDic;
        public bool IsStartOpen;
        public int ScorePerMin;
        public int Star;

        public CampFortData(int fortId)
        {
            Id = fortId;
            RelationForts = new List<int>();
        }


        internal void AddRelationFort(int otherFortId)
        {
            if (!RelationForts.Contains(otherFortId))
            {
                RelationForts.Add(otherFortId);
            }
        }
    }


    public static class WeekTime
    {
        public static int ToInt(this DayOfWeek dayOfWeek)
        {
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                return 7;
            }
            return (int)dayOfWeek;
        }
    }


    public class CampActivityLibrary
    {
        public static Dictionary<CampActivityType, CampActivityTimerData> campActivtyTimers = new Dictionary<CampActivityType, CampActivityTimerData>();

        public static Dictionary<int, CampFortData> CampFortLayout = new Dictionary<int, CampFortData>();
        //public static Dictionary<int, int> DugeonFort = new Dictionary<int, int>();

        public static void Init()
        {
            Dictionary<CampActivityType, CampActivityTimerData> campActivtyTimers = new Dictionary<CampActivityType, CampActivityTimerData>();
            Dictionary<int, CampFortData> CampFortLayout = new Dictionary<int, CampFortData>();

            //campActivtyTimers.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CampActivityTimer");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampActivityTimerData itemInfo = new CampActivityTimerData(data);


                campActivtyTimers.Add(itemInfo.Type, itemInfo);
            }
            CampActivityLibrary.campActivtyTimers = campActivtyTimers;

            //CampFortLayout.Clear();
            //DugeonFort.Clear();

            dataList = DataListManager.inst.GetDataList("CampBattleLayout");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampFortData campFortData;
                if (!CampFortLayout.TryGetValue(data.ID, out campFortData))
                {
                     campFortData = new CampFortData(data.ID);
                     CampFortLayout.Add(campFortData.Id, campFortData);
                }

                campFortData.CampType = (CampType)data.GetInt("CampType");
                campFortData.Star = data.GetInt("Star");

                string relationFortString = data.GetString("RelationForts");
                string[] relationFortArr = relationFortString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var it in relationFortArr)
                {
                    int fortId = it.ToInt();
                    CampFortData campFort;
                    if (!CampFortLayout.TryGetValue(fortId, out campFort))
                    {
                        campFort = new CampFortData(fortId);
                        CampFortLayout.Add(campFort.Id, campFort);
                    }
                    campFort.AddRelationFort(campFortData.Id);
                }
                campFortData.BossDungeonId = data.GetInt("BossDungeonId");
                campFortData.DefenderDungeonId = data.GetInt("DefenderDungeonId");
                campFortData.FollowerDungoenIdList = new List<int>();
                string follewersStr = data.GetString("FollowerDungeonId");
                string[] follewerArr = follewersStr.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var it1 in follewerArr)
                {
                    int dungeonId = it1.ToInt();
                    campFortData.FollowerDungoenIdList.Add(dungeonId);
                }

                campFortData.AddNaturesDic = new Dictionary<int, int>();
                string addNatureStr = data.GetString("AddNature");
                string[] addNatureArr = addNatureStr.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var it1 in addNatureArr)
                {
                    var natureArr = it1.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    campFortData.AddNaturesDic.Add(natureArr[0].ToInt(), natureArr[1].ToInt());
                }

                campFortData.IsStartOpen = data.GetBoolean("IsStartOpen");
                campFortData.ScorePerMin = data.GetInt("ScorePerMin");
            }
            CampActivityLibrary.CampFortLayout = CampFortLayout;
        }

        public static bool CheckFortAndDungeon(int fortId, int dungeonId)
        {
            CampFortData fort;
            CampFortLayout.TryGetValue(fortId, out fort);
            if (fort == null)
            {
                return false;
            }

            if (fort.BossDungeonId == dungeonId)
            {
                return true;
            }

            if (fort.DefenderDungeonId == dungeonId)
            {
                return true;
            }

            foreach (var item in fort.FollowerDungoenIdList)
            {
                if (dungeonId == item)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算活动周期
        /// </summary>
        /// <param name="type"></param>
        /// <param name="now"></param>
        /// <param name="serverOpenTime"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        public static Tuple<DateTime, DateTime> CalcWeekPhase(CampActivityType type, CampActivityTimerData config, DateTime now, DateTime serverOpenTime, ref int phase)
        {
            //因为开服当周是第一期
            phase = 1;
  
            ///周期为7天
            int period = 7;

            if (config != null)
            {
                int deltaDay = serverOpenTime.DayOfWeek.ToInt() - DayOfWeek.Monday.ToInt();
                //开服当周 周一的时间
                DateTime serverOpenMonday = serverOpenTime.Date - TimeSpan.FromDays(deltaDay);

                deltaDay = (int)(now.Date - serverOpenMonday).TotalDays;

                int delta = (int)(deltaDay / period);
                phase += delta;

                DateTime curPhaseMonday = serverOpenMonday.AddDays(delta * period);

                DateTime begin = curPhaseMonday.AddDays(config.BeginWeekDay.ToInt() - 1).AddHours(config.BeginHour).AddMinutes(config.BeginMinute).AddSeconds(config.BeginSecond);
                DateTime end = curPhaseMonday.AddDays(config.EndWeekDay.ToInt() - 1).AddHours(config.EndHour).AddMinutes(config.EndMinute).AddSeconds(config.EndSecond);
                return Tuple.Create(begin, end);
            }
            else
            {
                Logger.Log.Warn($"check {type} config in rank.xml");
                return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
            }

        }

        public static Tuple<DateTime, DateTime> CalcWeekPhaseByPhaseNum(CampActivityType type, CampActivityTimerData config, DateTime now, DateTime serverOpenTime, int phase)
        {
            ///周期为7天
            int period = 7;
            int deltaDay = serverOpenTime.DayOfWeek.ToInt() - DayOfWeek.Monday.ToInt();
            //开服当周 周一的时间
            DateTime serverOpenMonday = serverOpenTime.Date - TimeSpan.FromDays(deltaDay);

            if (config != null)
            {
                if (phase>0)
                {
                    //因为开服周算第一期
                    DateTime curPhaseMonday = serverOpenMonday.AddDays((phase - 1) * period);
                    DateTime begin = curPhaseMonday.AddDays(config.BeginWeekDay.ToInt() - 1).AddHours(config.BeginHour).AddMinutes(config.BeginMinute).AddSeconds(config.BeginSecond);
                    DateTime end = curPhaseMonday.AddDays(config.EndWeekDay.ToInt() - 1).AddHours(config.EndHour).AddMinutes(config.EndMinute).AddSeconds(config.EndSecond);
                    return Tuple.Create(begin, end);
                }
                else
                {
                    return Tuple.Create(now.Date, now.Date);
                }
            }
            else
            {
                Logger.Log.Warn($"check {type} config in rank.xml");
                return Tuple.Create(now.Date - TimeSpan.FromDays(now.DayOfWeek.ToInt() - DayOfWeek.Monday.ToInt()), now + TimeSpan.FromDays(7 - (int)now.DayOfWeek));
            }

        }

        public static CampFortData GetCampFortData(int fortId)
        {
            CampFortData fort;
            CampFortLayout.TryGetValue(fortId, out fort);
            return fort;
        }

    }
}

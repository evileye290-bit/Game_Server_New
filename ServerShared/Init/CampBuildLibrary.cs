using CommonUtility;
using DataProperty;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{

    public class PhaseKey
    {
        public int PhaseMin;
        public int PhaseMax;

        public PhaseKey(int phasemin, int phasemax)
        {
            PhaseMin = phasemin;
            PhaseMax = phasemax;
        }

        public override int GetHashCode()
        {
            return PhaseMin << 16 | PhaseMax;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PhaseKey))
                return false;
            PhaseKey key = obj as PhaseKey;
            // Return true if the fields match:
            return (PhaseMin == key.PhaseMin) && (PhaseMax == key.PhaseMax);
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", this.PhaseMin, this.PhaseMax);
        }
    }

    public class CampBuildItemPoolData
    {
        public PhaseKey Key;

        public int TotalWeight = 0;

        public Dictionary<int, CampBuildItemData> campBuildItems = new Dictionary<int, CampBuildItemData>();

        public CampBuildItemPoolData(PhaseKey key)
        {
            Key = key;
        }
    }

    public class CampBuildItemData
    {
        public int Id;
        public int ItemId;
        public int ItemType;
        public int ItemCount;
        public int Weight;
        public int RealWeight;
        public int PhaseMin;
        public int PhaseMax;
    }

    public class CampBuildPhaseData
    {
        public int PhaseMin;
        public int PhaseMax;

        public int DoubleItemCount;
        public int DoubleLeftStep;

        public int FlagCount;
        public string FlagReward;

        public int LeftStep;
        public string StepReward;


        public string EndTime;
        public string[] EndTimeArr;

        public void FormatEndTime()
        {
            EndTimeArr = EndTime.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class CampBuildBoxData
    {
        public int BuildValue;
        public string BuildReward;
    }


    public class CampBuildRankRewardData
    {
        public int PhaseMin;
        public int PhaseMax;
        public int RankMin;
        public int RankMax;
        public string Rewards;
        public int EmailId;
        public int Period;
    }




    public class CampBuildLibrary
    {
        public static Dictionary<PhaseKey, CampBuildItemPoolData> campBuildPhaseMap = new Dictionary<PhaseKey, CampBuildItemPoolData>();
        public static Dictionary<int, CampBuildItemData> campBuildItems = new Dictionary<int, CampBuildItemData>();


        public static List<CampBuildPhaseData> campBuildPhasesData = new List<CampBuildPhaseData>();

        private static List<CampBuildRankRewardData> buildValueRankRewards = new List<CampBuildRankRewardData>();

        private static Dictionary<int, CampBuildBoxData> buildBoxGradeDic = new Dictionary<int, CampBuildBoxData>();


        public static void Init()
        {
            Dictionary<PhaseKey, CampBuildItemPoolData> campBuildPhaseMap = new Dictionary<PhaseKey, CampBuildItemPoolData>();
            Dictionary<int, CampBuildItemData> campBuildItems = new Dictionary<int, CampBuildItemData>();
            List<CampBuildPhaseData> campBuildPhasesData = new List<CampBuildPhaseData>();
            List<CampBuildRankRewardData> buildValueRankRewards = new List<CampBuildRankRewardData>();
            Dictionary<int, CampBuildBoxData> buildBoxGradeDic = new Dictionary<int, CampBuildBoxData>();
            //campBuildPhaseMap.Clear();
            //campBuildItems.Clear();

            DataList dataList = DataListManager.inst.GetDataList("CampBuildItemPool");
            foreach (var item in dataList)
            {
                int phasemin = item.Value.GetInt("Min");
                int phasemax = item.Value.GetInt("Max");
                PhaseKey key = new PhaseKey(phasemin, phasemax);
                CampBuildItemPoolData data;
                if (!campBuildPhaseMap.TryGetValue(key, out data))
                {
                    data = new CampBuildItemPoolData(key);
                    campBuildPhaseMap.Add(key, data);
                }
                CampBuildItemData itemInfo = new CampBuildItemData();
                itemInfo.Id = item.Value.ID;
                itemInfo.Weight = item.Value.GetInt("Weight");
                itemInfo.ItemId = item.Value.GetInt("ItemId");
                itemInfo.ItemType = item.Value.GetInt("ItemType");
                itemInfo.ItemCount = item.Value.GetInt("ItemCount");
                itemInfo.RealWeight = data.TotalWeight;
                data.TotalWeight = data.TotalWeight + itemInfo.Weight;

                data.campBuildItems.Add(itemInfo.Id, itemInfo);

                campBuildItems.Add(itemInfo.Id, itemInfo);
            }

            //campBuildPhasesData.Clear();
            dataList = DataListManager.inst.GetDataList("CampBuild");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                string phasesString = data.GetString("PhaseValue");

                CampBuildPhaseData itemInfo = new CampBuildPhaseData();
                string[] phaseList = phasesString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (phaseList.Length > 1)
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[1].ToInt();
                }
                else
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[0].ToInt();
                }

                itemInfo.FlagCount = data.GetInt("FlagCount");
                itemInfo.FlagReward = data.GetString("FlagReward");
                itemInfo.DoubleLeftStep = data.GetInt("DoubleLeftStep");
                itemInfo.DoubleItemCount = data.GetInt("DoubleItemCount");
                itemInfo.LeftStep = data.GetInt("LeftStep");
                itemInfo.StepReward = data.GetString("StepReward");
                itemInfo.EndTime = data.GetString("EndTime");
                itemInfo.FormatEndTime();
                campBuildPhasesData.Add(itemInfo);
            }

            buildBoxGradeDic.Clear();
            dataList = DataListManager.inst.GetDataList("CampBuildBox");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBuildBoxData itemInfo = new CampBuildBoxData();
                int id = data.ID;
                itemInfo.BuildReward = data.GetString("BuildReward");
                itemInfo.BuildValue = data.GetInt("BuildValue");
                buildBoxGradeDic.Add(id, itemInfo);
            }

            //buildValueRankRewards.Clear();
            dataList = DataListManager.inst.GetDataList("CampBuildRank");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                CampBuildRankRewardData itemInfo = new CampBuildRankRewardData();

                string phasesString = data.GetString("ActivityNumber");
                string[] phaseList = phasesString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (phaseList.Length > 1)
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[1].ToInt();
                }
                else
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[0].ToInt();
                }

                string rankString = data.GetString("Rank");
                string[] rankArr = rankString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (rankArr.Length > 1)
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[1].ToInt();
                }
                else
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[0].ToInt();
                }

                itemInfo.Rewards = data.GetString("Rewards");
                itemInfo.EmailId = data.GetInt("EmailId");

                buildValueRankRewards.Add(itemInfo);
            }
            CampBuildLibrary.campBuildPhaseMap = campBuildPhaseMap;
            CampBuildLibrary.campBuildItems = campBuildItems;
            CampBuildLibrary.campBuildPhasesData = campBuildPhasesData;
            CampBuildLibrary.buildValueRankRewards = buildValueRankRewards;
            CampBuildLibrary.buildBoxGradeDic = buildBoxGradeDic;
        }

        public static CampBuildBoxData GetCampBuildBoxData(int buildBoxCount)
        {
            CampBuildBoxData data;
            int grade = buildBoxCount + 1;
            buildBoxGradeDic.TryGetValue(grade, out data);
            return data;
        }

        public static CampBuildRankRewardData GetCampBuildRankRewardInfo(int phase, int rank)
        {
            foreach (var item in buildValueRankRewards)
            {
                if (item.PhaseMin <= phase && phase <= item.PhaseMax)
                {
                    if (item.RankMin <= rank && rank <= item.RankMax)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private static CampBuildItemPoolData GetCampBuildItemPool(int phaseNum)
        {
            if (campBuildPhaseMap.Count <= 0)
            {
                return null;
            }

            foreach (var item in campBuildPhaseMap)
            {
                if (item.Key.PhaseMin <= phaseNum && phaseNum <= item.Key.PhaseMax)
                {
                    return item.Value;
                }
            }
            return campBuildPhaseMap.ElementAtOrDefault(0).Value;
        }


        public static CampBuildItemData RandomItem(int phaseNum)
        {
            var itemPool = GetCampBuildItemPool(phaseNum);
            CampBuildItemData itemInfo = null;
            int rand = NewRAND.Next(0, itemPool.TotalWeight);
            foreach (var item in itemPool.campBuildItems)
            {
                if (rand >= item.Value.RealWeight)
                {
                    itemInfo = item.Value;
                }
                else
                {
                    break;
                }
            }
            return itemInfo;
        }


        public static CampBuildItemData GetBuildItemData(int xmlId)
        {
            CampBuildItemData data = null;
            campBuildItems.TryGetValue(xmlId, out data);
            return data;
        }


        public static string GetCampBuildBoxRewardsData(int phase, int boxType)
        {
            CampBuildPhaseData data = campBuildPhasesData[0];
            foreach (var item in campBuildPhasesData)
            {
                if (phase >= item.PhaseMin && phase <= item.PhaseMax)
                {
                    data = item;
                    break;
                }
            }
            switch ((CampBuildBoxType)boxType)
            {
                case CampBuildBoxType.StepBox:
                    return data.StepReward;
                case CampBuildBoxType.FlagBox:
                    return data.FlagReward;
                default:
                    break;
            }
            return string.Empty;
        }



        public static CampBuildPhaseData GetCampBuildPhaseData(int phase)
        {
            CampBuildPhaseData data = campBuildPhasesData[0];
            foreach (var item in campBuildPhasesData)
            {
                if (phase >= item.PhaseMin && phase <= item.PhaseMax)
                {
                    data = item;
                    break;
                }
            }
            return data;
        }
    }
}

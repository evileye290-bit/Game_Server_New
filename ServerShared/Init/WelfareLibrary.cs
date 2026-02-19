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
    public class WelfareLibrary
    {
        private static Dictionary<int, WelfareTriggerInfo> triggerInfos = new Dictionary<int, WelfareTriggerInfo>();
        private static Dictionary<WelfareConditionType, List<int>> triggerGroupInfos = new Dictionary<WelfareConditionType, List<int>>();

        public static void Init()
        {
            // triggerInfos.Clear();
            // triggerGroupInfos.Clear();

            DataList triggerDataList = DataListManager.inst.GetDataList("WelfareTrigger");
            InitTriggerInfos(triggerDataList);
        }

        private static void InitTriggerInfos(DataList dataList)
        {
            Dictionary<int, WelfareTriggerInfo> triggerInfos = new Dictionary<int, WelfareTriggerInfo>();
            Dictionary<WelfareConditionType, List<int>> triggerGroupInfos = new Dictionary<WelfareConditionType, List<int>>();
            
            List<int> list;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                WelfareTriggerInfo info = new WelfareTriggerInfo(data);
                triggerInfos.Add(data.ID, info);

                if (triggerGroupInfos.TryGetValue(info.ConditionType, out list))
                {
                    list.Add(data.ID);
                }
                else
                {
                    list = new List<int>();
                    list.Add(data.ID);
                    triggerGroupInfos.Add(info.ConditionType, list);
                }
            }

            WelfareLibrary.triggerInfos = triggerInfos;
            WelfareLibrary.triggerGroupInfos = triggerGroupInfos;
        }

        #region api

        public static Dictionary<int, WelfareTriggerInfo> GetTriggerInfos(WelfareConditionType type, int param)
        {
            Dictionary<int, WelfareTriggerInfo> dic = new Dictionary<int, WelfareTriggerInfo>();
            List<int> list = GetTriggerGroups(type);
            if (list != null)
            {
                foreach (var id in list)
                {
                    WelfareTriggerInfo info = GetTriggerInfo(id);
                    if (info != null)
                    {
                        switch (type)
                        {
                            case WelfareConditionType.Task:
                            case WelfareConditionType.Level:
                            case WelfareConditionType.Recharge:
                                if (param >= info.ConditionParam)
                                {
                                    dic.Add(id, info);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return dic;
        }
        public static List<int> GetTriggerGroups(WelfareConditionType type)
        {
            List<int> list;
            triggerGroupInfos.TryGetValue(type, out list);
            return list;
        }
        public static WelfareTriggerInfo GetTriggerInfo(int id)
        {
            WelfareTriggerInfo info = null;
            triggerInfos.TryGetValue(id, out info);
            return info;
        }

        #endregion
    }
}

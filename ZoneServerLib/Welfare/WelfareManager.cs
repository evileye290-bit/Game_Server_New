using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib.Welfare
{
    public class WelfareManager
    {
        private ZoneServerApi server { get; set; }
        /// <summary>
        /// 所有活动
        /// </summary>
        Dictionary<int, WelfareTriggerItem> currentTriggerDic = new Dictionary<int, WelfareTriggerItem>();
        ///// <summary>
        ///// 触发过活动
        ///// </summary>
        //List<int> oldTriggerList = new List<int>();

        public Dictionary<int, WelfareTriggerItem> GetTriggerList()
        {
            return currentTriggerDic;
        }

        public void AddTriggerItem(WelfareTriggerItem item)
        {
            currentTriggerDic.Add(item.Id, item);
        }

        public bool CheckTriggerItem(int id)
        {
            if (currentTriggerDic.ContainsKey(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public WelfareTriggerItem GetTriggerItemForId(int id)
        {
            WelfareTriggerItem item;
            currentTriggerDic.TryGetValue(id, out item);
            return item;
        }
    }
}

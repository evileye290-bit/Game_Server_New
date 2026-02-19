using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public static class DelegationLibrary
    {
        private static Dictionary<int, DelegationModel> delegationList = new Dictionary<int, DelegationModel>();
        public static DelegConfigModel configModel { get; private set; }

        private static SortedDictionary<int, DelegationModelList> taskIdLimitDic = new SortedDictionary<int, DelegationModelList>();      

        private static List<int> nameIdList = new List<int>();

        private static SortedDictionary<int, int> eventNumDic = new SortedDictionary<int, int>();

        private static SortedDictionary<int, DelegationGuarantee> guaranteeDic = new SortedDictionary<int, DelegationGuarantee>();
        /// <summary>
        /// 每日恢复的委派次数
        /// </summary>
        //public static int RenewDelegateCount;
        /// <summary>
        /// 每日可购买委派次数上限
        /// </summary>
        //public static int MaxBuyCount;
        /// <summary>
        /// 每分钟加速钻石数
        /// </summary>
        public static int AccelerateFee;
        /// <summary>
        /// 最高星级
        /// </summary>
        public static int MaxQuality;     

        public static void Init()
        {
            //delegationList.Clear();
            //taskIdLimitDic.Clear();
            //nameIdList.Clear();
            //eventNumDic.Clear();
            //guaranteeDic.Clear();

            InitDelegation();
            InitDelegationConfig();
            InitDelegationEvent();
            InitEventNum();
            InitDelegationGuarantee();
        }

        private static void InitDelegation()
        {
            Dictionary<int, DelegationModel> delegationList = new Dictionary<int, DelegationModel>();
            SortedDictionary<int, DelegationModelList> taskIdLimitDic = new SortedDictionary<int, DelegationModelList>();

            DataList dataList = DataListManager.inst.GetDataList("Delegation");
            DelegationModel model;
            DelegationModelList taskIdLimitList = null;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                model = new DelegationModel(data);

                if (!delegationList.ContainsKey(item.Key))
                {
                    delegationList.Add(item.Key, model);
                }

                if (!taskIdLimitDic.TryGetValue(model.TaskId, out taskIdLimitList))
                {
                    taskIdLimitList = new DelegationModelList();
                    taskIdLimitList.AddModel(model);
                    taskIdLimitDic.Add(model.TaskId, taskIdLimitList);
                }
                else
                {
                    taskIdLimitList.AddModel(model);
                }
            }
            DelegationLibrary.delegationList = delegationList;
            DelegationLibrary.taskIdLimitDic = taskIdLimitDic;
        }

        private static void InitDelegationConfig()
        {
            Data data = DataListManager.inst.GetData("DelegationConfig", 1);
            configModel = new DelegConfigModel(data);
            //RenewDelegateCount = data.GetInt("TimesPerDay");
            //MaxBuyCount = data.GetInt("BuyTimes");
            AccelerateFee = data.GetInt("AccelerateFee");
            MaxQuality = data.GetInt("MaxQuality");         
        }

        private static void InitDelegationEvent()
        {
            List<int> nameIdList = new List<int>();

            DataList dataList = DataListManager.inst.GetDataList("DelegationEvent");          
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!nameIdList.Contains(data.ID))
                {
                    nameIdList.Add(data.ID);
                }
            }
            DelegationLibrary.nameIdList = nameIdList;
        }

        private static void InitEventNum()
        {
            SortedDictionary<int, int> eventNumDic = new SortedDictionary<int, int>();
            DataList dataList = DataListManager.inst.GetDataList("DelegationEventNum");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int taskId = data.GetInt("TaskId");
                int num = data.GetInt("Num");
                if (!eventNumDic.ContainsKey(taskId))
                {
                    eventNumDic.Add(taskId, num);
                }              
            }
            DelegationLibrary.eventNumDic = eventNumDic;
        }

        private static void InitDelegationGuarantee()
        {
            SortedDictionary<int, DelegationGuarantee> guaranteeDic = new SortedDictionary<int, DelegationGuarantee>();

            DataList dataList = DataListManager.inst.GetDataList("DelegationGuarantee");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DelegationGuarantee model = new DelegationGuarantee(data);
                if (!guaranteeDic.ContainsKey(model.TaskId))
                {
                    guaranteeDic.Add(model.TaskId, model);
                }
            }
            DelegationLibrary.guaranteeDic = guaranteeDic;
        }

        public static DelegationModel GetDelegationById(int id)
        {
            DelegationModel model = null;
            delegationList.TryGetValue(id, out model);
            return model;
        }    

        public static int GetRefreshCoins(int count)
        {
            if (count == 0)
            {
                count = 1;
            }
            SortedDictionary<int, int> refreshCoins = new SortedDictionary<int, int>();
            string[] renewFee =  configModel.RenewFee.Split('|');
            foreach (var item in renewFee)
            {
                string[] perRenewFee = item.Split(':');
                refreshCoins.Add(int.Parse(perRenewFee[0]), int.Parse(perRenewFee[1]));
            }
            int temp;
            int curCount = 0;
            foreach (var key in refreshCoins.Keys)
            {
                if (count >= key)
                {
                    curCount = key;
                }
                else
                {
                    refreshCoins.TryGetValue(curCount, out temp);
                    return temp;
                }
            }
            refreshCoins.TryGetValue(curCount, out temp);
            return temp;
        }

        public static int GetBuyCountCoins(int count)
        {
            if (count == 0)
            {
                count = 1;
            }
            SortedDictionary<int, int> buyCountCoins = new SortedDictionary<int, int>();
            string[] buyPrices = configModel.Price.Split('|');
            foreach (var item in buyPrices)
            {
                string[] buyprice = item.Split(':');
                buyCountCoins.Add(int.Parse(buyprice[0]), int.Parse(buyprice[1]));
            }
            int temp;
            int curCount = 0;
            foreach (var key in buyCountCoins.Keys)
            {
                if (count >= key)
                {
                    curCount = key;
                }
                else
                {
                    buyCountCoins.TryGetValue(curCount, out temp);
                    return temp;
                }
            }
            buyCountCoins.TryGetValue(curCount, out temp);
            return temp;
        }

        public static DelegationModelList GetDelegationsByMainTaskId(int taskId)
        {
            DelegationModelList list = null;
            foreach (var item in taskIdLimitDic)
            {
                if (taskId >= item.Key)
                {
                    list = item.Value;
                }
                else
                {
                    break;
                }
            }
            return list;
        }

        public static List<int> GetNameIdList()
        {
            return nameIdList;
        }

        public static int GetEventNumByMainTaskId(int taskId)
        {
            int num = 0;
            foreach (var item in eventNumDic)
            {
                if (taskId >= item.Key)
                {
                    num = item.Value;
                }
                else
                {
                    break;
                }
            }
            return num;
        }

        public static DelegationGuarantee GetGuaranteedNumByMainTaskId(int taskId)
        {
            DelegationGuarantee model = null;
            foreach (var item in guaranteeDic)
            {
                if (taskId >= item.Key)
                {
                    model = item.Value;
                }
                else
                {
                    break;
                }
            }
            return model;
        }
        
        public static int RandomGuaranteeQuality(int taskId)
        {
            int quality = 0;
            DelegationGuarantee guarantee = GetGuaranteedNumByMainTaskId(taskId);
            if (guarantee == null || guarantee.StarWeightDic.Count == 0)
            {
                return quality;
            }       
            int randTopLimit = guarantee.StarWeightDic.Keys.Max();
            int rand = NewRAND.Next(1, randTopLimit);
            foreach (var kv in guarantee.StarWeightDic)
            {
                if (rand <= kv.Key)
                {
                    quality = kv.Value;                    
                }
                else
                {
                    break;
                }          
            }
            return quality;
        }    
    }
}

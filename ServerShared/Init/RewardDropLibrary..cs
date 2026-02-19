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
    public class RewardDropLibrary
    {
        private static Dictionary<int, RewardDropItemList> dropDic = new Dictionary<int, RewardDropItemList>();
        public static void Init()
        {
            Dictionary<int, RewardDropItemList> dropDic = new Dictionary<int, RewardDropItemList>();
            //dropDic.Clear();
            DataList dataList = DataListManager.inst.GetDataList("RewardDrop");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!dropDic.ContainsKey(item.Key))
                {
                    dropDic.Add(item.Key, new RewardDropItemList(data));
                }
            }
            RewardDropLibrary.dropDic = dropDic;
        }

        public static RewardDropItemList GetRewardDropItems(int id)
        {
            RewardDropItemList model = null;
            dropDic.TryGetValue(id, out model);
            return model;
        }

        #region new itemstr id:maintype:min:max:rate

        public static List<ItemBasicInfo> GetProbability(int action, string resourceString)
        {
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int i = RAND.Range(0, 10000);
            foreach (string resourceItem in resourceList)
            {
                //取出单个设置
                //string[] resource = resourceItem.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                string[] resource = resourceItem.Split(new char[] { ':', '@' }, StringSplitOptions.RemoveEmptyEntries);
                if (action == 1)
                {
                    CheckAddItem(getItems, resource);
                }
                else if (action == 2)
                {
                    //2为所有该类型定义的物品中仅掉落一项
                    if (i > int.Parse(resource[4]))
                    {
                        //当I大于预设值时，不符合这个档次，查看下一档次
                        continue;
                    }
                    else
                    {
                        //当I小于预设值时符合条件，只掉落这个档次物品，之后推出循环
                        string[] attrs = null;
                        if (resource.Length > 5)
                        {
                            attrs = new string[resource.Length - 5];
                            Array.Copy(resource, 5, attrs, 0, attrs.Length);
                        }
                        int count = RAND.Range(int.Parse(resource[2]), int.Parse(resource[3]));
                        getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), int.Parse(resource[0]), count, attrs));
                        break;
                    }
                }
            }
            return getItems;
        }

        private static void CheckAddItem(List<ItemBasicInfo> getItems, string[] resource)
        {
            int i = RAND.Range(0, 10000);
            //1为符合概率都掉落
            if (i <= int.Parse(resource[4]))
            {
                //当I小于预设值时符合条件，掉落物品
                int count = RAND.Range(int.Parse(resource[2]), int.Parse(resource[3]));
                string[] attrs = null;
                if (resource.Length > 5)
                {
                    attrs = new string[resource.Length - 5];
                    Array.Copy(resource, 5, attrs, 0, attrs.Length);
                }
                getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), int.Parse(resource[0]), count, attrs));
            }
        }

        public static List<ItemBasicInfo> GetCalculateReward(int action, string resourceString)
        {
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int tempMin = -1;
            int tempMax = 0;
            int i = NewRAND.Next(0, 10000);
            foreach (string resourceItem in resourceList)
            {
                //取出单个设置
                string[] resource = resourceItem.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                tempMax = int.Parse(resource[4]);
                //2为所有该类型定义的物品中仅掉落一项
                if (tempMin < i && i <= tempMax)
                {
                    //当I小于预设值时符合条件，只掉落这个档次物品，之后推出循环
                    int count = NewRAND.Next(int.Parse(resource[2]), int.Parse(resource[3]));
                    string[] attrs = null;
                    if (resource.Length > 5)
                    {
                        attrs = new string[resource.Length - 5];
                        Array.Copy(resource, 5, attrs, 0, attrs.Length);
                    }
                    getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), int.Parse(resource[0]), count, attrs));
                }
                //else
                //{
                //    //当I大于预设值时，不符合这个档次，查看下一档次
                //    //continue;
                //}
                if (tempMax == 10000)
                {
                    tempMin = -1;
                    i = NewRAND.Next(0, 10000);
                }
                else
                {
                    tempMin = tempMax;
                }


            }
            return getItems;
        }

        //批量
        public static List<ItemBasicInfo> GetBatchReward(int action, string resourceString, int num)
        {
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resources = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> resourceList = new List<string[]>();
            foreach (string resourceItem in resources)
            {
                //取出单个设置
                //string[] resource = resourceItem.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                string[] resource = resourceItem.Split(new char[] { ':', '@' }, StringSplitOptions.RemoveEmptyEntries);
                resourceList.Add(resource);
            }

            for (int j = 0; j < num; j++)
            {
                int i = NewRAND.Next(0, 10000);
                foreach (var resourceItem in resourceList)
                {
                    //取出单个设置
                    string[] resource = resourceItem;
                    if (action == 1)
                    {
                        CheckBatchAddItem(getItems, resource);
                    }
                    else if (action == 2)
                    {
                        if (i > int.Parse(resource[4]))
                        {
                            continue;
                        }
                        else
                        {
                            int count = NewRAND.Next(int.Parse(resource[2]), int.Parse(resource[3]));
                            int key = int.Parse(resource[0]);

                            string[] attrs = null;
                            if (resource.Length > 5)
                            {
                                attrs = new string[resource.Length - 5];
                                Array.Copy(resource, 5, attrs, 0, attrs.Length);
                            }
                            getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), key, count, attrs));
                            break;
                        }
                    }
                }
            }
            return getItems;
        }

        private static void CheckBatchAddItem(List<ItemBasicInfo> getItems, string[] resource)
        {
            int i = RAND.Range(0, 10000);
            //1为符合概率都掉落
            if (i <= int.Parse(resource[4]))
            {
                //当I小于预设值时符合条件，掉落物品
                int count = RAND.Range(int.Parse(resource[2]), int.Parse(resource[3]));
                string[] attrs = null;
                if (resource.Length > 5)
                {
                    attrs = new string[resource.Length - 5];
                    Array.Copy(resource, 5, attrs, 0, attrs.Length);
                }
                getItems.Add(new ItemBasicInfo(int.Parse(resource[1]), int.Parse(resource[0]), count, attrs));
            }
        }

        /// <summary>
        /// typid:rewardType:num@attr1:attr2|typid:rewardType:num@attr1:attr2
        /// </summary>
        public static List<ItemBasicInfo> GetSimpleRewards(string resourceString, int batchCount = 1)
        {
            ItemBasicInfo info = null;
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string resourceItem in resourceList)
            {
                //取出单个设置
                info = ItemBasicInfo.Parse(resourceItem);
                if (info == null) continue;

                info.Num *= batchCount;
                getItems.Add(info);
            }
            return getItems;
        }

        #endregion

    }
}

using EnumerateUtility;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class RewardManager
    {
        private List<ItemBasicInfo> allRewards = new List<ItemBasicInfo>();
        private Dictionary<RewardType, Dictionary<int, int>> rewardList = new Dictionary<RewardType, Dictionary<int, int>>();
        private List<ItemBasicInfo> specialRewards = new List<ItemBasicInfo>();

        public List<ItemBasicInfo> AllRewards
        {
            get
            {
                return allRewards;
            }
        }
        public List<ItemBasicInfo> SpecialRewards
        {
            get
            {
                return specialRewards;
            }
        }
        public Dictionary<RewardType, Dictionary<int, int>> RewardList
        {
            get { return rewardList; }
        }

        public void InitReward(List<ItemBasicInfo> allRewards, bool breakup = true)
        {
            this.allRewards = allRewards;

            if (breakup)
            {
                BreakupRewards();
            }
        }

        public void InitCalculateReward(int action, string rewardString)
        {
            allRewards = RewardDropLibrary.GetProbability(action, rewardString);

            BreakupRewards();
        }

        public void InitSimpleReward(string rewardString, bool breakup = true)
        {
            allRewards = RewardDropLibrary.GetSimpleRewards(rewardString);

            if (breakup)
            {
                BreakupRewards();
            }
        }

        public void InitSimpleReward(string rewardString, int batchCount)
        {
            allRewards = RewardDropLibrary.GetSimpleRewards(rewardString, batchCount);
            BreakupRewards();
        }

        public void InitBatchReward(int action, string rewardString, int num)
        {
            allRewards = RewardDropLibrary.GetBatchReward(action, rewardString, num);

            BreakupRewards();
        }

        public void MergeRewards()
        {
            Dictionary<int, ItemBasicInfo> dic = new Dictionary<int, ItemBasicInfo>();
            List<ItemBasicInfo> list = new List<ItemBasicInfo>();
            foreach (var item in allRewards)
            {
                ItemBasicInfo temp = null;
                if (dic.TryGetValue(item.Id, out temp) && item.RewardType != (int)RewardType.Equip && item.RewardType != (int)RewardType.SoulBone && item.RewardType != (int)RewardType.SoulRing)
                {
                    //符合合并规则
                    temp.Num += item.Num;
                }
                else
                {
                    if (!dic.ContainsKey(item.Id))
                    {
                        dic.Add(item.Id, item);
                    }
                    list.Add(item);
                }
            }
            allRewards = list;
        }

        public void AddSimpleReward(string rewardString, int batchCount = 1)
        {
            allRewards.AddRange(RewardDropLibrary.GetSimpleRewards(rewardString, batchCount));
        }

        public void AddCalculateReward(int action, string rewardString)
        {
            allRewards.AddRange(RewardDropLibrary.GetProbability(action, rewardString));
        }

        public void AddReward(ItemBasicInfo reward)
        {
            allRewards.Add(reward);
        }

        public void AddReward(List<ItemBasicInfo> rewards)
        {
            allRewards.AddRange(rewards);
        }

        public void AddReward(ItemBasicInfo reward, int num)
        {
            for (int i = 0; i < num; ++i)
            {
                allRewards.Add(reward);
            }
        }

        public void AddSpecialReward(ItemBasicInfo reward)
        {
            specialRewards.Add(reward);
        }

        public void RemoveReward(ItemBasicInfo reward)
        {
            for (int i = 0; i < AllRewards.Count; i++)
            {
                if (AllRewards[i].RewardType == reward.RewardType &&
                    AllRewards[i].Id == reward.Id &&
                    AllRewards[i].Num == reward.Num)
                {
                    AllRewards.RemoveAt(i);
                    break;
                }
            }
        }

        public void RemoveReward(RewardType type)
        {
            Dictionary<int, int> list = GetRewardList(type);
            if (list?.Count > 0)
            {
                foreach (var reward in list)
                {
                    for (int i = 0; i < AllRewards.Count; i++)
                    {
                        if (AllRewards[i].RewardType == (int)type &&
                            AllRewards[i].Id == reward.Key)
                        {
                            AllRewards.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            RewardList.Remove(type);


        }

        public bool RemoveReward(RewardType type, int id, int num)
        {
            Dictionary<int, int> heros = GetRewardList(type);
            if (heros != null)
            {
                //移除原奖励
                int oldNum = 0;
                if (heros.TryGetValue(id, out oldNum))
                {
                    int removeType = (int)type;

                    if (oldNum > num)
                    {
                        int newNum = oldNum - num;
                        heros[id] = newNum;

                        foreach (var item in AllRewards)
                        {
                            if (item.RewardType == removeType && item.Id == id)
                            {
                                item.Num = newNum;
                                break;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        heros.Remove(id);

                        int removeIndex = -1;
                        for (int i = 0; i < AllRewards.Count; i++)
                        {
                            var item = AllRewards[i];
                            if (item.RewardType == removeType && item.Id == id)
                            {
                                removeIndex = i;
                                break;
                            }
                        }
                        if (removeIndex >= 0)
                        {
                            AllRewards.RemoveAt(removeIndex);
                        }

                        return heros.Remove(id);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void BreakupRewards(bool needClear = false)
        {
            if (needClear)
            {
                rewardList.Clear();
            }

            foreach (var item in AllRewards)
            {
                AddBreakupReward(item);
            }
        }

        public void AddBreakupReward(string reward, int multiple = 1)
        {
            ItemBasicInfo baseInfo = ItemBasicInfo.Parse(reward);
            baseInfo.Num *= multiple;
            AddBreakupReward(baseInfo);
            allRewards.Add(baseInfo);
        }

        public void AddBreakupReward(int type, int id, int num)
        {
            ItemBasicInfo baseInfo = new ItemBasicInfo(type, id, num);
            AddBreakupReward(baseInfo);
            allRewards.Add(baseInfo);
        }

        private void AddBreakupReward(ItemBasicInfo item)
        {
            Dictionary<int, int> list;
            RewardType type = (RewardType)item.RewardType;
            if (rewardList.TryGetValue(type, out list))
            {
                if (list.ContainsKey(item.Id))
                {
                    list[item.Id] += item.Num;
                }
                else
                {
                    list.Add(item.Id, item.Num);
                }
            }
            else
            {
                list = new Dictionary<int, int>();
                list.Add(item.Id, item.Num);
                rewardList.Add(type, list);
            }
        }

        public Dictionary<int, int> GetRewardList(RewardType type)
        {
            Dictionary<int, int> list;
            rewardList.TryGetValue(type, out list);
            return list;
        }
        public void RemoveRewardList(RewardType type)
        {
            rewardList.Remove(type);
            int removeType = (int)type;
            List<ItemBasicInfo> removeList = new List<ItemBasicInfo>();
            foreach (var item in AllRewards)
            {
                if (item.RewardType == removeType)
                {
                    removeList.Add(item);
                }
            }
            foreach (var item in removeList)
            {
                AllRewards.Remove(item);
            }
        }
        public IEnumerable<ItemBasicInfo> GetRewardItemList(RewardType type)
        {
            return AllRewards.Where(x => x.RewardType == (int)type);
        }
        public bool IsOnlyCurrencies()
        {
            foreach (var kv in allRewards)
            {
                if (kv.RewardType != (int)RewardType.Currencies) return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join("|", allRewards.ConvertAll(x => x.ToString()));
        }

        public static RewardManager operator *(RewardManager manager, float ratio)
        {
            manager.allRewards.ForEach(x => x.Num = (int)(x.Num * ratio));
            return manager;
        }

        public void AddBreakupRewardSpecial(ItemBasicInfo item)
        {
            AddBreakupReward(item);
        }

        public void Clear()
        {
            allRewards.Clear();
            rewardList.Clear();
            specialRewards.Clear();
        }

        public List<Dictionary<string, object>> GetRewardDic()
        {
            //[{ "itemId":5533,"count":10},{ "itemId":1247,"count":100}]
            List<Dictionary<string, object>> dic = new List<Dictionary<string, object>>();
            Dictionary<string, object> info = new Dictionary<string, object>();
            foreach (var reward in rewardList)
            {
                foreach (var item in reward.Value)
                {
                    info = GetRewardInfoDic(item.Key, item.Value, (int)reward.Key);
                    dic.Add(info);
                }
            }
            return dic;
        }

        public static List<Dictionary<string, object>> GetRewardDic(string resourceString)
        {
            //[{ "itemId":5533,"count":10},{ "itemId":1247,"count":100}]
            List<Dictionary<string, object>> dic = new List<Dictionary<string, object>>();

            if (!string.IsNullOrEmpty(resourceString))
            {
                Dictionary<string, object> info = new Dictionary<string, object>();
                List<ItemBasicInfo> rewards = RewardDropLibrary.GetSimpleRewards(resourceString);
                foreach (var reward in rewards)
                {
                    info = GetRewardInfoDic(reward.Id, reward.Num, reward.RewardType);
                    dic.Add(info);
                }
            }

            return dic;
        }

        public static Dictionary<string, object> GetRewardInfoDic(int itemId, int count, int type)
        {
            //[{ "itemId":5533,"count":10},{ "itemId":1247,"count":100}]
            Dictionary<string, object> info = new Dictionary<string, object>();
            info.Add("itemId", itemId);
            info.Add("itemType", type);
            info.Add("count", count);
            return info;
        }
    }
}

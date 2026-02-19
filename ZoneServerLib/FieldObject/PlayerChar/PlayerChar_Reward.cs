using ServerShared;
using System.Collections.Generic;
using System.Linq;
using ServerModels;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using CommonUtility;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public List<ItemBasicInfo> AddRewardDrop(List<int> rwardDropIds)
        {
            List<ItemBasicInfo> getList = new List<ItemBasicInfo>();
            if (rwardDropIds != null && rwardDropIds.Count > 0)
            {
                foreach (var rewardDropId in rwardDropIds)
                {
                    RewardDropItemList rewardDrop = RewardDropLibrary.GetRewardDropItems(rewardDropId);
                    if (rewardDrop != null)
                    {
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        getList.AddRange(items);
                    }
                }
            }
            return getList;
        }

        public void AddRewards(RewardManager rewards, ObtainWay way, string extraParam = "")
        {
            RewardResult resulet = new RewardResult();

            RewardItemChange(rewards);

            AddHeros(rewards, resulet, way, extraParam);

            AddCoins(rewards, resulet, way, extraParam);

            AddItems(rewards, way, extraParam);

            AddHideTasks(rewards, resulet);

            AddCounters(rewards, resulet, way, extraParam);

            AddGrain(rewards, way, extraParam);

            AddPets(rewards, way, extraParam);

            AddGuideSoulItems(rewards, way, extraParam);
        }

        public void RewardItemChange(RewardManager rewards)
        {
            Dictionary<int, int> itemChanges = rewards.GetRewardList(RewardType.ItemChange);
            if (itemChanges != null)
            {
                foreach (var itemChange in itemChanges)
                {
                    string reward = BagLibrary.GetItemChange(itemChange.Key, ZoneServerApi.now);
                    if (!string.IsNullOrEmpty(reward))
                    {
                        ItemBasicInfo rewardItem = ItemBasicInfo.Parse(reward);
                        rewardItem.Num *= itemChange.Value;
                        rewards.AddReward(rewardItem);
                    }
                }
                rewards.RemoveRewardList(RewardType.ItemChange);
                rewards.BreakupRewards();
            }
        }

        public RewardManager GetSimpleReward(string rewardStr, ObtainWay way, int batchCount = 1, string extraParam = "")
        {
            RewardManager rewards = new RewardManager();
            rewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(rewardStr, HeroMng.GetFirstHeroJob());
            //TODO 获取奖励
            if (!string.IsNullOrEmpty(rewardStr))
            {
                rewards.InitSimpleReward(rewardStr, batchCount);

                AddRewards(rewards, way, extraParam);
            }
            //没有奖励
            return rewards;
        }

        public RewardManager GetCalculateReward(int action, string rewardStr, ObtainWay way, string extraParam = "")
        {
            RewardManager rewards = new RewardManager();
            //TODO 获取奖励
            if (!string.IsNullOrEmpty(rewardStr))
            {
                rewards.InitCalculateReward(action, rewardStr);

                AddRewards(rewards, way, extraParam);
            }
            //没有奖励
            return rewards;
        }

        public RewardManager GetItemsBatchUseReward(int action, string rewardStr, int num)
        {
            RewardManager rewards = new RewardManager();

            //TODO 获取奖励
            if (!string.IsNullOrEmpty(rewardStr))
            {
                rewards.InitBatchReward(action, rewardStr, num);

                AddRewards(rewards, ObtainWay.ItemUse);
            }

            //没有奖励
            return rewards;
        }

        public int GetNeedBagCount(RewardManager rewardMng)
        {
            int count = 0;
            foreach (var kv in rewardMng.RewardList)
            {
                switch (kv.Key)
                {
                    case RewardType.NormalItem:
                        {
                            foreach (var item in kv.Value)
                            {
                                ItemModel model = BagLibrary.GetItemModel(item.Key);
                                if (model != null)
                                {
                                    if (model.PileMax == 1)
                                    {
                                        count += item.Value;
                                    }
                                    else
                                    {
                                        NormalItem normalItem = bagManager.NormalBag.GetItemBySubType(item.Key);
                                        if (normalItem == null)
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case RewardType.SoulRing:
                    case RewardType.SoulBone:
                    case RewardType.Equip:
                    case RewardType.HiddenWeapon:
                        foreach (var item in kv.Value)
                        {
                            count += item.Value;
                        }
                        break;
                    default:
                        break;
                }
            }
            return count;
        }

        public int GetNeedBagCountByRewardType(RewardManager manager)
        {
            int needCount = 0;
            List<int> normalItemList = new List<int>();

            foreach (var reward in manager.AllRewards)
            {
                switch ((RewardType)reward.RewardType)
                {
                    case RewardType.SoulRing:
                    case RewardType.SoulBone:
                    case RewardType.Equip:
                    case RewardType.HiddenWeapon:
                        needCount++;
                        break;
                    case RewardType.NormalItem:
                        if (!normalItemList.Contains(reward.Id))
                        {
                            needCount++;
                        }
                        break;
                    default:
                        break;
                }
            }
            return needCount;
        }

        public int GetNeedPetEggBagCount(RewardManager rewardMng)
        {
            int count = 0;
            Dictionary<int, int> petEggList;
            if (rewardMng.RewardList.TryGetValue(RewardType.Pet, out petEggList))
            {
                foreach (var kv in petEggList)
                {
                    count += kv.Value;
                }
                return count;
            }
            return 0;
        }

        public RewardManager GetSoulBoneShopItemReward(string rewardStr, SoulBone soulBone, ObtainWay way, int batchCount = 1, string extraParam = "")
        {
            RewardManager manager = new RewardManager();
            rewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(rewardStr, HeroMng.GetFirstHeroJob());
            //TODO 获取奖励
            if (!string.IsNullOrEmpty(rewardStr))
            {
                manager.InitSimpleReward(rewardStr, batchCount);

                var soulBones = manager.GetRewardItemList(RewardType.SoulBone);
                var soulBoneDic = manager.GetRewardList(RewardType.SoulBone);
                var curr = soulBones.FirstOrDefault();
                 //用于检查是否已经获得过
                 if (curr.Attrs.Count > 0 && curr.Attrs.Count >= ItemBasicInfo.SoulBoneFixAttrCount && soulBoneDic.ContainsKey(curr.Id))
                {
                    var item = this.bagManager.SoulBoneBag.BuyShopItemAddSoulBone(soulBone.TypeId, soulBone);
                    if (item != null)//假如格子足够而没有进邮件
                    {
                        //RecordObtainLog(way, RewardType.SoulBone, soulBone.TypeId, 1, 1, item.Bone.ToString());
                        //获取埋点
                        BIRecordObtainItem(RewardType.SoulBone, way, soulBone.TypeId, 1, 1);
                        //BI 新增物品
                        KomoeEventLogItemFlow("add", "", soulBone.TypeId, RewardType.SoulBone.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                        //玩家行为
                        RecordAction(ActionType.GotQualitySoulBone, item);

                        SyncClientItemInfo(item);

                        //komoelog
                        SoulBoneItem soulBoneItem = item as SoulBoneItem;
                        KomoeLogRecordSoulboneResource(soulBoneItem, curr.Id.ToString(), item.Uid.ToString(), way, extraParam);
                    }
                }            
            }
            return manager;
        }

        private void KomoeLogRecordSoulboneResource(SoulBoneItem soulBoneItem, string id, string uid, ObtainWay way, string extraParam)
        {
            if (soulBoneItem == null)
            {
                return;
            }
            Dictionary<string, object> additionDic = new Dictionary<string, object>();
            additionDic.Add(((NatureType)soulBoneItem.Bone.MainNatureType).ToString(), soulBoneItem.Bone.MainNatureValue);          
            KomoeEventLogSoulboneResource(id, uid, "", soulBoneItem.Bone.Quality.ToString(), 0, "", soulBoneItem.Bone.PartType.ToString(), additionDic, way.ToString(), extraParam);
        }

        /// <summary>
        /// 活动奖励池随机奖励
        /// </summary>
        /// <param name="period"></param>
        /// <param name="rewardList"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private List<string> GenerateRandomReward(int period, List<RandomRewardModel> rewardList, int num)
        {
            List<string> rewards = new List<string>();
            string reward;
            for (int i = 0; i < num; i++)
            {
                reward = GetRandomRewardByWeight(rewardList);
                if (!string.IsNullOrEmpty(reward))
                {
                    rewards.Add(reward);
                }
            }

            return rewards;
        }

        private string GetRandomRewardByWeight(List<RandomRewardModel> rewardList)
        {
            string reward = string.Empty;
            if (rewardList == null)
            {
                return reward;
            }

            int totalWeight = 0;
            Dictionary<int, RandomRewardModel> weightDic = new Dictionary<int, RandomRewardModel>();
            foreach (var rewardModel in rewardList)
            {
                totalWeight += rewardModel.Weight;
                weightDic.Add(totalWeight, rewardModel);
            }

            int rand = NewRAND.Next(1, totalWeight);
            int cur = 0;
            foreach (var kv in weightDic)
            {
                if (rand > cur && rand <= kv.Key)
                {
                    reward = kv.Value.Reward;
                    break;
                }
                cur = kv.Key;
            }

            return reward;
        }

        /// <summary>
        /// 活动奖励池随机奖励(去重)
        /// </summary>      
        /// <param name="rewardList"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public void GenerateNoneRepeateRandomReward(List<string> rewards, List<int> itemIdList, List<RandomRewardModel> rewardList, int num, List<int> rewardIdList = null)
        {
            for (int i = 0; i < num; i++)
            {
                GetNoneRepeateRandomRewardByWeight(rewards, itemIdList, rewardList, rewardIdList);
            }
        }

        private void GetNoneRepeateRandomRewardByWeight(List<string> rewards, List<int> itemIdList, List<RandomRewardModel> rewardList, List<int> rewardIdList)
        {
            if (rewardList == null)
            {
                return;
            }

            RandomRewardModel randReward = null;
            int totalWeight = 0;
            Dictionary<int, RandomRewardModel> weightDic = new Dictionary<int, RandomRewardModel>();
            string[] rewardArr;
            foreach (var rewardModel in rewardList)
            {
                rewardArr = StringSplit.GetArray(":", rewardModel.Reward);
                if (!itemIdList.Contains(rewardArr[0].ToInt()))
                {
                    totalWeight += rewardModel.Weight;
                    weightDic.Add(totalWeight, rewardModel);
                }
            }

            int rand = NewRAND.Next(1, totalWeight);
            int cur = 0;
            foreach (var kv in weightDic)
            {
                if (rand > cur && rand <= kv.Key)
                {
                    randReward = kv.Value;
                    break;
                }
                cur = kv.Key;
            }

            string reward = randReward.Reward;
            if (!string.IsNullOrEmpty(reward))
            {
                rewardArr = StringSplit.GetArray(":", reward);
                rewards.Add(reward);
                itemIdList.Add(rewardArr[0].ToInt());
            }
            if (rewardIdList != null)
            {
                rewardIdList.Add(randReward.Id);
            }
        }
    }
}
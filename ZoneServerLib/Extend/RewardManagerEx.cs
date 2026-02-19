using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public static class RewardManagerEx
    {
        public static RepeatedField<REWARD_ITEM_INFO> GenerateRewardMsg(this RewardManager manager)
        {
            RepeatedField<REWARD_ITEM_INFO> rewards = new RepeatedField<REWARD_ITEM_INFO>();
            GenerateRewardMsg(manager, rewards);
            return rewards;
        }

        public static void GenerateRewardMsg(this RewardManager manager, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            ////魂环放在首位
            //var soulRings = manager.GetRewardList(RewardType.SoulRing);
            //if (soulRings != null && soulRings.Count > 0)
            //{
            //    GenerateRewardItemInfo(rewards, RewardType.SoulRing, soulRings);
            //    manager.RewardList.Remove(RewardType.SoulRing);
            //}

            //foreach (var kv in manager.RewardList)
            //{
            //    if (kv.Value.Count <= 0) continue;
            //    GenerateRewardItemInfo(rewards, kv.Key, kv.Value);
            //}

            manager.MergeRewards();

            GenerateRewardItemInfo(manager, rewards);
        }

        public static void GenerateRewardItemInfo(RepeatedField<REWARD_ITEM_INFO> rewards, RewardType rewardType, List<ItemBasicInfo> rewardList)
        {
            if (rewardList == null)
                return;

            foreach (var item in rewardList)
            {
                REWARD_ITEM_INFO rewardInfo = new REWARD_ITEM_INFO();
                rewardInfo.MainType = (int)rewardType;
                rewardInfo.TypeId = item.Id;
                rewardInfo.Num = item.Num;
                if (item.Attrs != null)
                {
                    foreach (var attr in item.Attrs)
                    {
                        rewardInfo.Param.Add(attr);
                    }
                }
                rewards.Add(rewardInfo);
            }
        }

        public static void GenerateRewardItemInfo(this RewardManager rewardMng, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            foreach (var item in rewardMng.AllRewards)
            {
                REWARD_ITEM_INFO reward = new REWARD_ITEM_INFO();
                reward.MainType = item.RewardType;
                reward.TypeId = item.Id;
                reward.Num = item.Num;
                if (item.Attrs != null)
                {
                    foreach (var attr in item.Attrs)
                    {
                        reward.Param.Add(attr);
                    }
                }
                rewards.Add(reward);
            }
        }

        public static void AddSimpleRewardWithSoulBoneCheck(this RewardManager rewardMng, string rewardString, int batchCount = 1)
        {
            var allRewards = GetSimpleRewards(rewardString, batchCount);
            rewardMng.AddReward(allRewards);
        }


        public static List<ItemBasicInfo> GetSimpleRewards(string resourceString, int batchCount = 1)
        {
            ItemBasicInfo info = null;
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            //拆开字符串
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string resourceItem in resourceList)
            {
                //取出单个设置
                List<ItemBasicInfo> soulBoneItemList = null;
                if (IsSoulBoneReward(resourceItem))
                {
                    soulBoneItemList = SoulBoneManager.GenerateSoulboneReward(resourceItem, null, 0);
                }
                else if (IsEquipReward(resourceItem))
                {
                    soulBoneItemList = EquipmentManager.GenerateEquipmentReward(resourceItem);
                }
                if (soulBoneItemList != null)
                {
                    getItems.AddRange(soulBoneItemList);
                }
                else
                {
                    info = ItemBasicInfo.Parse(resourceItem);
                    if (info == null) continue;

                    info.Num *= batchCount;
                    getItems.Add(info);
                }
            }
            return getItems;
        }

        public static List<ItemBasicInfo> GetRewardBasicInfoList(RewardDropItemList model, int job)
        {
            List<ItemBasicInfo> getItems = new List<ItemBasicInfo>();
            if (model != null)
            {
                ItemBasicInfo info = null;
                List<ItemBasicInfo> itemList;
                List<RewardDropItem> list = model.GetDropList();
                //拆开字符串
                foreach (var dropItem in list)
                {

                    itemList = new List<ItemBasicInfo>();
                    //取出单个设置
                    switch (dropItem.RewardType)
                    {
                        case RewardType.SoulBone:
                            itemList = SoulBoneManager.GenerateSoulboneReward(dropItem, job);
                            break;
                        case RewardType.Equip:
                            itemList = EquipmentManager.GenerateEquipmentReward(dropItem, job);
                            break;
                        case RewardType.SoulRing:
                            itemList = SoulRingManager.GenerateSoulRingReward(dropItem, model.type, job);
                            break;
                        default:
                            info = ItemBasicInfo.Parse(dropItem);
                            getItems.Add(info);
                            break; ;
                    }
                    if (itemList != null)
                    {
                        getItems.AddRange(itemList);
                    }
                }
            }

            return getItems;
        }
        public static bool IsSoulBoneReward(string resource)
        {
            if (resource.EndsWith("@"))
            {
                return true;
            }
            return false;
        }

        public static bool IsEquipReward(string resource)
        {
            if (resource.EndsWith("Equipment"))
            {
                return true;
            }
            return false;
        }
    }
}

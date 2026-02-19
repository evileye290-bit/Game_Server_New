using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //装备
        public EquipmentManager EquipmentManager { get; set; }
        public void InitEquipmentManager()
        {
            this.EquipmentManager = new EquipmentManager(this);
        }

        /// <summary>
        /// 装备or替换
        /// </summary>
        /// <param name="equipUid"></param>
        /// <param name="heroId"></param>
        public void EquipEquipment(ulong equipUid, int heroId)
        {
            MSG_ZGC_EQUIP_EQUIPMENT_RESULT result = new MSG_ZGC_EQUIP_EQUIPMENT_RESULT();
            List<BaseItem> updateList = new List<BaseItem>();
            result.Result = (int)EquipmentSwap(heroId, equipUid, ref updateList);
            SyncClientItemsInfo(updateList);
            Write(result);
        }

        /// <summary>
        /// 装备穿戴and替换
        /// </summary>
        private ErrorCode EquipmentSwap(int heroId, ulong newEquipUid, ref List<BaseItem> updateList)
        {
            ErrorCode result = ErrorCode.Success;
            Bag_Equip bag = BagManager.EquipBag;
            EquipmentItem newEquipItem = bag.GetItem(newEquipUid) as EquipmentItem;
            if (newEquipItem == null)
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed with no such item {2}", Uid, heroId, newEquipUid);
                result = ErrorCode.NoSuchItem;
                return result;
            }

            if (newEquipItem.EquipInfo.EquipHeroId > 0)
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed!already equipt to hero {2}", Uid, heroId, newEquipItem.EquipInfo.EquipHeroId);
                result = ErrorCode.Fail;
                return result;
            }

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed! heroid wrong {2}", Uid, heroId, heroId);
                result = ErrorCode.Fail;
                return result;
            }

            // 判断当前是否能穿
            if (heroInfo.Level < newEquipItem.Model.Data.GetInt("LevelLimit"))
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed: level {2} limit", Uid, heroId, heroInfo.Level);
                result = ErrorCode.LevelLimit;
                return result;
            }

            if (newEquipItem.Model.Job != heroInfo.GetData().GetInt("Job"))
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed with JobNotPermission", Uid, heroId);
                result = ErrorCode.JobNotPermission;
                return result;
            }

            ///获取槽信息
            Slot slot = EquipmentManager.GetSlot(heroId, newEquipItem.Model.Part);
            if (slot == null)
            {
                Log.Warn($"player {Uid} injectEquipment cannot find hero slot");
                result = ErrorCode.NoHeroInfo;
                return result;
            }

            Dictionary<NatureType, long> oldNature = null;
            Dictionary<NatureType, long> newNature = null;

            int operateType = 1;
            ulong oldEquipmentUid = slot.EquipmentUid;
            if (slot.EquipmentUid > 0)
            {
                //卸下 旧装备
                //卸下旧装备
                oldNature = EquipmentManager.CalcEquipBaseNature(slot);

                EquipmentItem oldEquip = BagManager.EquipBag.GetItem(slot.EquipmentUid) as EquipmentItem;
                oldEquip.EquipInfo.EquipHeroId = 0;
                bag.SyncDbItemInfo(oldEquip);
                updateList.Add(oldEquip);

                operateType = 3;
                //装备替换
                BIRecordEquipmentReplaceLog(heroInfo.Id, heroInfo.Level, newEquipItem.Model.Part, oldEquip.Id, oldEquip.Model.Score, newEquipItem.Id, newEquipItem.Model.Score);
            }
            else
            {
                //装备替换
                BIRecordEquipmentReplaceLog(heroInfo.Id, heroInfo.Level, newEquipItem.Model.Part, 0, 0, newEquipItem.Id, newEquipItem.Model.Score);
            }
            //穿新装备
            newEquipItem.EquipInfo.EquipHeroId = heroId;
            bag.SyncDbItemInfo(newEquipItem);
            updateList.Add(newEquipItem);

            slot.EquipmentUid = newEquipUid;
            EquipmentManager.UpdateSlot2DB(heroId, slot);

            newNature = EquipmentManager.CalcEquipBaseNature(slot);
            HeroMng.UpdateEquipmentNature(heroInfo, newNature, oldNature);

            //komoelog beforePower
            int beforePower = heroInfo.GetBattlePower();

            HeroMng.UpdateBattlePower(heroInfo.Id);

            int afterPower = heroInfo.GetBattlePower();

            HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);
            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
            SyncDbUpdateHeroItem(heroInfo);

            //穿戴装备
            AddTaskNumForType(TaskType.EquipEquipment, EquipmentManager.GetEquipedCount(), false);
            //指定位置穿戴装备数
            AddTaskNumForType(TaskType.EquipEquipmentBySlot, EquipmentManager.GetEquipedCountBySlot(newEquipItem.Model.Part), false, newEquipItem.Model.Part);

            //komoelog       
            KomoeEventLogEquitFlow(heroId.ToString(), "", "", 3, operateType, oldEquipmentUid.ToString(), newEquipUid.ToString(), beforePower, afterPower, afterPower-beforePower, 0, 0);

            //养成
            BIRecordDevelopLog(DevelopType.EquipEquipment, newEquipItem.Id, 0, newEquipItem.Model.Score, heroInfo.Id, heroInfo.Level);
            return result;
        }

        /// <summary>
        /// 装备强化
        /// </summary>
        /// <param name="msg"></param>
        public void UpgradeEquipment(MSG_GateZ_UPGRADE_EQUIPMENT msg)
        {
            int heroId = msg.HeroId;
            int slotId = msg.Slot;
            bool useLingshi = msg.CrackChecked; //是否使用灵石
            bool IsAdvanced = msg.IsAdvanced; //是否使用灵石
            int heroAndSlotId = heroId * 100 + slotId;
            int lingShiNum = 0;
            if (IsAdvanced)
            {
                useLingshi = false;
            }

            MSG_ZGC_UPGRADE_EQUIPMENT_RESULT result = new MSG_ZGC_UPGRADE_EQUIPMENT_RESULT();
            result.Slot = slotId;
            result.HeroId = heroId;
            result.IsAdvanced = IsAdvanced;

            //判断可进行
            HeroInfo info = HeroMng.GetHeroInfo(heroId);
            if (info == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipment error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            Slot slot = EquipmentManager.GetSlot(heroId, slotId);
            if (slot == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipment error: not find hero {heroId} slot {slotId}");
                result.Result = (int)ErrorCode.NoInjectionSlot;
                Write(result);
                return;
            }
            var bag = bagManager.GetBag(MainType.Equip);
            EquipmentItem equipmentItem = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
            if (equipmentItem == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipment error: not find hero {heroId} slot {slotId} equipmentItem {slot.EquipmentUid}");
                result.Result = (int)ErrorCode.NoEquipment;
                Write(result);
                return;
            }

            //判断装备等级上限 槽上是否有物品
            var tempLimit = EquipmentManager.GetLevelWithLevelLimit(heroId, slotId);
            bool canUpgrade = tempLimit.Item1;
            int oldLevel = tempLimit.Item2;

            result.CurLevel = oldLevel;
            if (!canUpgrade)
            {
                Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} slot {slotId} can not upgrade");
                result.Result = (int)ErrorCode.LevelLimit;
                Write(result);
                return;
            }

            EquipUpgradeModel limitUpModel = EquipLibrary.GetEquipUpgradeModel(oldLevel + 1);
            if (limitUpModel == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} slot {slotId} not upgrade level {oldLevel + 1}");
                result.Result = (int)ErrorCode.LevelLimit;
                Write(result);
                return;
            }

            List<int> levelList = new List<int>();
            int curLevel = oldLevel;
            int costGold = 0;
            int costCount = 0;
            int returnGold = 0;
            int returnCount = 0;
            Dictionary<int, int> costItems = new Dictionary<int, int>();
            int upNum = 1;
            if (IsAdvanced)
            {
                upNum = EquipLibrary.MaxOnkeyNum;
            }

            EquipUpgradeModel curUpModel = null;
            for (int i = 0; i < upNum; i++)
            {
                curUpModel = EquipLibrary.GetEquipUpgradeModel(curLevel + 1);
                if (curUpModel == null)
                {
                    Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} slot {slotId} not upgrade level {curLevel + 1} by num {i}");
                    break;
                }

                if (!CheckCoins(CurrenciesType.gold, costGold + curUpModel.CostGold))
                {
                    break;
                }

                //扣除物品
                costItems.TryGetValue(curUpModel.CostId, out costCount);
                costCount += curUpModel.CostCount;

                NormalItem item = BagManager.NormalBag.GetItem(curUpModel.CostId) as NormalItem;
                if (item == null || item.PileNum < costCount)
                {
                    break;
                }
                //检查通过，开始计算
                costGold += curUpModel.CostGold;
                costItems[curUpModel.CostId] = costCount;

                //计算概率
                int rate = curUpModel.BaseRate;

                if (useLingshi && curUpModel.LingshiId > 0)
                {
                    //检查物品
                    NormalItem lingshi = bagManager.GetItem(MainType.Material, curUpModel.LingshiId) as NormalItem;
                    if (lingshi != null && lingshi.PileNum >= curUpModel.LingshiCount)
                    {
                        rate = 10000;
                        costItems[curUpModel.LingshiId] = curUpModel.LingshiCount;
                        lingShiNum += curUpModel.LingshiCount;
                    }
                    else
                    {
                        useLingshi = false;
                    }
                }

                //计算结果
                int rand = RAND.Range(0, 10000);
                if (rand <= rate)
                {
                    curLevel++;
                }
                else
                {
                    curLevel = curUpModel.RollBackLevel;
                }
                //添加消耗
                returnGold += curUpModel.CostGold;
                returnCount += curUpModel.CostCount;
                levelList.Add(curLevel);
                if (curLevel > oldLevel)
                {
                    break;
                }
            }

            int state = 0;
            result.CurLevel = curLevel;
            if (curLevel > oldLevel)
            {
                state = 1;
                result.Result = (int)ErrorCode.Success;

                //升级成功的操作
                EquipmentManager.UpgradeLevel(heroId, slotId, 1);
                //装备升到指定等级发公告
                BroadcastEquipmentUpgradeToLevel(heroId, slotId);
                //装备强化指定等级
                AddTaskNumForType(TaskType.EquipmentUpgradeForLevel, 1, true, curLevel);
                //玩家行为
                RecordAction(ActionType.EquipmentTrain, curLevel);
                RecordAction(ActionType.HeroEquipmentTrain, heroId);
                //装备强化到指定等级发称号卡
                List<int> paramList = new List<int>() { curLevel };
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.EquipmentUpToMaxLevel, 1, paramList);
            }
            else
            {
                state = 2;
                result.Result = (int)ErrorCode.EquipmentRollBack;

                //升级未成功 回退
                Slot slot1 = EquipmentManager.GetSlot(heroId, slotId);
                if (slot1 != null)
                {
                    EquipInjectionModel injectModel = EquipLibrary.GetMaxInjectionSlot(curLevel, slot1.Part);
                    EquipmentManager.Upgrade2Level(heroId, slotId, curLevel, injectModel);
                }
            }

            //消耗金币
            ConsumeWay way = ConsumeWay.EquipmentUpgrade;
            DelCoins(CurrenciesType.gold, costGold, way, curLevel.ToString());

            //消耗物品
            Dictionary<BaseItem, int> itemAndCost = new Dictionary<BaseItem, int>();//用于消耗
            foreach (var kv in costItems)
            {
                BaseItem item = this.bagManager.GetItem(MainType.Material, kv.Key);
                if (item != null)
                {
                    itemAndCost.Add(item, kv.Value);
                }
            }
            List<BaseItem> items = DelItem2Bag(itemAndCost, RewardType.NormalItem, way);
            if (items != null)
            {
                SyncClientItemsInfo(items);
            }
            result.OldLevel = oldLevel;
            result.LevelList.AddRange(levelList);
            Write(result);

            //返还
            if (returnGold > 0 || returnCount > 0)
            {
                if (GetCoins(CurrenciesType.equipReturnItem) < EquipLibrary.LimitNum)
                {
                    //10是扩大倍数
                    returnCount = (int)(returnCount * EquipLibrary.ReturnNum * 10);
                    returnGold = (int)(returnGold * EquipLibrary.ReturnNum);
                    RewardManager manager = new RewardManager();
                    manager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.equipReturnItem, returnCount));
                    manager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.equipReturnGold, returnGold));
                    manager.BreakupRewards();
                    AddRewards(manager, ObtainWay.EquipmentUpgrade);
                }
            }


            UpdateSlotNature(info, equipmentItem, slotId, result.OldLevel, curLevel);

            //装备升级
            //BIRecordEquipmentUpgradeLog(info.Id, info.Level, msg.Slot, msg.Slot, state, oldPower, newPower, oldLevel, curLevel, lingShiNum);      

            //装备强化
            AddTaskNumForType(TaskType.EquipmentUpgrade, levelList.Count);
            AddPassCardTaskNum(TaskType.EquipmentUpgrade, levelList.Count);
            AddDriftExploreTaskNum(TaskType.EquipmentUpgrade, levelList.Count);
            //养成
            BIRecordDevelopLog(DevelopType.EquipmentLevel, heroAndSlotId, oldLevel, curLevel, info.Id, info.Level);

            //komoelog
            KomoeEventLogEquipmentStrengthen(info.Id.ToString(), "", (5 - info.GetData().GetInt("Quality")).ToString(), info.Level, 
                info.GetData().GetInt("Job").ToString(), msg.Slot.ToString(), oldLevel, curLevel);
        }
        

        /// <summary>
        /// 装备强化，使用强化石
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="slotId"></param>
        /// <param name="itemUid"></param>
        public void UpgradeEquipmentDirectly(int heroId, int slotId, ulong itemUid)
        {
            MSG_ZGC_UPGRADE_EQUIPMENT_DIRECTLY result = new MSG_ZGC_UPGRADE_EQUIPMENT_DIRECTLY()
            {
                HeroId = heroId, 
                Slot = slotId
            };
            
            HeroInfo info = HeroMng.GetHeroInfo(heroId);
            if (info == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            Slot slot = EquipmentManager.GetSlot(heroId, slotId);
            if (slot == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error: not find hero {heroId} slot {slotId}");
                result.Result = (int)ErrorCode.NoInjectionSlot;
                Write(result);
                return;
            }
            
            var bag = bagManager.GetBag(MainType.Equip);
            EquipmentItem equipmentItem = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
            if (equipmentItem == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error: not find hero {heroId} slot {slotId} equipmentItem {slot.EquipmentUid}");
                result.Result = (int)ErrorCode.NoEquipment;
                Write(result);
                return;
            }

            NormalItem normalItem = bagManager.NormalBag.GetItem(itemUid) as NormalItem;
            if (normalItem == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error: not find item {itemUid}");
                result.Result = (int)ErrorCode.NotFoundItem;
                Write(result);
                return;
            }

            ItemModel model = BagLibrary.GetItemModel(normalItem.Id);
            if (model == null)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error: not find item {normalItem.Id}");
                result.Result = (int)ErrorCode.NotFoundItem;
                Write(result);
                return;
            }

            result.OldLevel = slot.EquipLevel;
            if (model.SubType != (int) ConsumableType.EquipmentUpgradeLevelStone || slot.EquipLevel >= model.LevelUpNum)
            {
                Log.Warn($"player {Uid} UpgradeEquipmentDirectly error:  item level {model.LevelUpNum} less then slot level {slot.EquipLevel}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            //升级成功的操作
            EquipmentManager.UpgradeLevel(heroId, slotId, model.LevelUpNum - slot.EquipLevel);
            //装备升到指定等级发公告
            BroadcastEquipmentUpgradeToLevel(heroId, slotId);

            //玩家行为
            RecordAction(ActionType.EquipmentTrain, model.LevelUpNum);
            RecordAction(ActionType.HeroEquipmentTrain, heroId);
            
            //装备强化到指定等级发称号卡
            List<int> paramList = new List<int>() { model.LevelUpNum };
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.EquipmentUpToMaxLevel, 1, paramList);

            int oldPower = info.GetBattlePower();
            UpdateSlotNature(info, equipmentItem, slotId, result.OldLevel, model.LevelUpNum);

            BaseItem items = DelItem2Bag(normalItem, RewardType.NormalItem, 1, ConsumeWay.EquipmentUpgrade);
            if (items != null)
            {
                SyncClientItemInfo(items);
            }

            result.CurLevel = model.LevelUpNum;
            result.Result = (int)ErrorCode.Success;
            Write(result);

            //装备强化
            AddTaskNumForType(TaskType.EquipmentUpgrade, 1);
            AddPassCardTaskNum(TaskType.EquipmentUpgrade, 1);
            AddDriftExploreTaskNum(TaskType.EquipmentUpgrade, 1);
            //装备强化指定等级
            AddTaskNumForType(TaskType.EquipmentUpgradeForLevel, 1, true, model.LevelUpNum);
            
            int heroAndSlotId = heroId * 100 + slotId;
            int newPower = info.GetBattlePower();
            
            //装备升级
            //BIRecordEquipmentUpgradeLog(info.Id, info.Level, slotId, slotId, 1, oldPower, newPower, result.OldLevel, model.LevelUpNum, 0);
            
            //养成
            BIRecordDevelopLog(DevelopType.EquipmentLevel, heroAndSlotId, result.OldLevel, model.LevelUpNum, info.Id, info.Level);

            //komoelog
            KomoeEventLogEquipmentStrengthen(info.Id.ToString(), "", (5 - info.GetData().GetInt("Quality")).ToString(),
                info.Level, info.GetData().GetInt("Job").ToString(), slotId.ToString(), result.OldLevel, slot.EquipLevel);
        }
        
        private void UpdateSlotNature(HeroInfo info, EquipmentItem equipmentItem, int slotId, int oldLevel, int curLevel)
        {
            if (curLevel == oldLevel) return;

            int heroId = info.Id;
            Dictionary<NatureType, long> oldNature = new Dictionary<NatureType, long>();
            Dictionary<NatureType, long> curNature = new Dictionary<NatureType, long>();

            EquipmentModel model = EquipLibrary.GetEquipModel(equipmentItem.Model.Job, equipmentItem.Model.Part, 1);
            if (model != null)
            {
                if (oldLevel > 0)
                {
                    EquipUpgradeModel oldUpModel = EquipLibrary.GetEquipUpgradeModel(oldLevel);
                    if (oldUpModel != null)
                    {
                        //装备升级属性加成
                        foreach (var kv in model.BaseNatureDic)
                        {
                            oldNature.Add(kv.Key, (long)(kv.Value * (oldUpModel.StrengthRatio / 10000.0000f)));
                        }
                    }
                    else
                    {
                        Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} slot {slotId} not upgrade level {oldLevel} model");
                    }
                }
                
                if (curLevel > 0)
                {
                    var curUpModel = EquipLibrary.GetEquipUpgradeModel(curLevel);
                    if (curUpModel != null)
                    {
                        foreach (var kv in model.BaseNatureDic)
                        {
                            curNature.Add(kv.Key, (long)(kv.Value * (curUpModel.StrengthRatio / 10000.0000f)));
                        }
                    }
                    else
                    {
                        Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} slot {slotId} not upgrade level {curLevel} model");
                    }
                }

                HeroMng.UpdateEquipmentNature(info, curNature, oldNature);

                HeroMng.UpdateBattlePower(info.Id);
                HeroMng.NotifyClientBattlePowerFrom(info.Id);

                //同步
                SyncHeroChangeMessage(new List<HeroInfo>() { info });
            }
        }

        public void ReturnUpgradeEquipmentCost(int num)
        {
            MSG_ZGC_RETURN_UPGRADE_EQUIPMENT_COST response = new MSG_ZGC_RETURN_UPGRADE_EQUIPMENT_COST();
            if (num <= 0)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} return upgrade equip cost fail: num {1}", Uid, num);
                return;
            }

            int equipReturnItemCount = GetCoins(CurrenciesType.equipReturnItem);
            int equipReturnGold = GetCoins(CurrenciesType.equipReturnGold);
            if (equipReturnItemCount <= 0)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} return upgrade equip cost fail: equipReturnItem {1}", Uid, equipReturnItemCount);
                return;
            }

            int hasItemCount = (int)(equipReturnItemCount / 10.0f + 0.5f);
            int returnCount = Math.Min(hasItemCount, num);

            int minCount = EquipLibrary.Step1Num;
            float discountNum = 1;
            if (hasItemCount < EquipLibrary.Step1Num)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} return upgrade equip cost fail: equipReturnItem {1}", Uid, hasItemCount);
                return;
            }
            else if (EquipLibrary.Step1Num <= hasItemCount && hasItemCount < EquipLibrary.Step2Num)
            {
                discountNum = EquipLibrary.Discount1Num;
                minCount = EquipLibrary.Step1Num;
            }
            else if (EquipLibrary.Step2Num <= hasItemCount && hasItemCount < EquipLibrary.Step3Num)
            {
                discountNum = EquipLibrary.Discount2Num;
                minCount = EquipLibrary.Step2Num;
            }
            else if (EquipLibrary.Step3Num <= hasItemCount && hasItemCount < EquipLibrary.Step4Num)
            {
                discountNum = EquipLibrary.Discount3Num;
                minCount = EquipLibrary.Step3Num;
            }
            else
            {
                discountNum = EquipLibrary.Discount4Num;
                minCount = EquipLibrary.Step4Num;
            }
            returnCount = Math.Max(returnCount, minCount);

            int costDiamond = (int)(returnCount * EquipLibrary.CostDiamond * discountNum);
            if (GetCoins(CurrenciesType.diamond) < costDiamond)
            {
                //10是扩大倍数
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} return upgrade equip cost fail: diamond {1} need {2}", Uid, GetCoins(CurrenciesType.diamond), costDiamond);
                return;
            }

            int costReturnCount = Math.Min(returnCount * 10, equipReturnItemCount);
            int costReturnGold = Math.Min(returnCount * EquipLibrary.GoldMin, equipReturnGold);

            Dictionary<CurrenciesType, int> costCoins = new Dictionary<CurrenciesType, int>();
            costCoins.Add(CurrenciesType.diamond, costDiamond);
            costCoins.Add(CurrenciesType.equipReturnItem, costReturnCount);

            RewardManager manager = new RewardManager();
            manager.AddReward(new ItemBasicInfo((int)RewardType.NormalItem, EquipLibrary.ReturnItem, returnCount));

            if (costReturnGold > 0)
            {
                costCoins.Add(CurrenciesType.equipReturnGold, costReturnGold);
                manager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, costReturnGold));

            }

            DelCoins(costCoins, ConsumeWay.ReturnEquipmentUpgrade, returnCount.ToString());
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.ReturnEquipmentUpgrade);

            //发奖
            response.Result = (int)ErrorCode.Success;
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);

            BIRecordEquipRedeemLog(costDiamond, discountNum, returnCount, costReturnGold);
        }

        /// <summary>
        /// 注能
        /// </summary>
        /// <param name="msg"></param>
        public void InjectEquipment(MSG_GateZ_INJECTION_EQUIPMENT msg)
        {
            MSG_ZGC_EQUIPMENT_INJECTION_RESULT result = new MSG_ZGC_EQUIPMENT_INJECTION_RESULT();
            result.Result = (int)ErrorCode.Success;
            result.Slot = msg.Slot;
            result.HeroId = msg.HeroId;

            //检查是否有装备在
            Slot slot = EquipmentManager.GetSlot(msg.HeroId, msg.Slot);
            if (slot == null)
            {
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                Logger.Log.Warn($"player {Uid} injectEquipment cannot find hero slot");
                return;
            }
            if (slot.EquipmentUid <= 0)
            {
                result.Result = (int)ErrorCode.NoEquipment;
                Write(result);
                Log.Warn($"player {Uid} injectEquipment hero {msg.HeroId} slot{msg.Slot} failed: not find equipment on slot");
                return;
            }
            //检查槽开启了几个，是否在开启的范围内
            HeroInfo hero = HeroMng.GetHeroInfo(msg.HeroId);

            EquipInjectionModel model = EquipLibrary.GetMaxInjectionSlot(hero.Level, slot.Part);
            if (model == null)
            {
                Log.Warn($"player {Uid} injectEquipment hero {msg.HeroId} slot{msg.Slot} failed: level {hero.Level} part {slot.Part} injection slot not open");
                result.Result = (int)ErrorCode.InjectionSlotNotOpen;
                Write(result);
                return;
            }

            if (msg.Jewel.Uid < 0)
            {
                //canChangeJewel = false;
                Log.Warn($"player {Uid} injectEquipment hero {msg.HeroId} slot{msg.Slot} failed: jewel param error");
                result.Result = (int)ErrorCode.NoInjectionJewelChoose;
                Write(result);
                return;
            }

            ulong newJewel = msg.Jewel.Uid;

            int operateType = 1;
            bool newInject = false;
            ulong oldJewel = slot.JewelUid;
            if (newJewel == 0)  //卸下宝石
            {
                if (bagManager.BagFull())
                {
                    Log.Warn($"player {Uid} injectEquipment hero {msg.HeroId} slot{msg.Slot} failed: bag full");
                    result.Result = (int)ErrorCode.MaxBagSpace;
                    Write(result);
                    return;
                }

                //卸下
                DisboardJewel(slot, hero);

                operateType = 2;
            }
            else //嵌入宝石
            {
                if (!EquipmentManager.CheckXuanyuEnough(newJewel))
                {
                    Log.Warn($"player {Uid} injectEquipment hero {msg.HeroId} slot{msg.Slot} failed: jewel not enough");
                    result.Result = (int)ErrorCode.NotEnoughJewel;
                    Write(result);
                    return;
                }

                if (oldJewel > 0)
                {
                    //替换
                    newInject = SwapJewel(slot, newJewel, hero);

                    operateType = 3;
                }
                else
                {
                    //添加
                    newInject = InjectJewel(slot, newJewel, hero);
                }
            }

            EquipmentManager.UpdateSlot2DB(hero.Id, slot);

            //komoelog
            int beforePower = hero.GetBattlePower();

            HeroMng.UpdateBattlePower(hero.Id);
            HeroMng.NotifyClientBattlePowerFrom(hero.Id);

            int afterPower = hero.GetBattlePower();
            ////换新inject槽位
            //int tempInject = 0;
            //foreach (var item in msg.InjectionSlots)
            //{
            //    tempInject |= 1 << (item - 1);
            //}

            //slot.Injection = (uint)tempInject;

            result.InjectionSlots.AddRange(slot.GenerateInjections());

            if (slot.JewelUid > 0)
            {
                result.JewelUidHigh = slot.JewelUid.GetHigh();
                result.JewelUidLow = slot.JewelUid.GetLow();
                result.JewelId = BagManager.GetItem(slot.JewelUid).Id;

                //检查装备充能
                int fullInjectCount = EquipmentManager.GetFullInjectCount();
                if (fullInjectCount > 0)
                {
                    //装备注能100%
                    AddTaskNumForType(TaskType.EquipmentInjectForNum, fullInjectCount, false);
                }

                if (newInject)
                {
                    //装备镶嵌
                    int jewelCount = EquipmentManager.GetAllJewelCount();
                    if (jewelCount > 0)
                    {
                        AddTaskNumForType(TaskType.EquipmentJewel, jewelCount, false);
                    }

                    ItemXuanyuModel curModel = EquipmentManager.GetXuanyuModel(slot.JewelUid);
                    if (curModel != null)
                    {
                        //装备指定等级
                        AddTaskNumForType(TaskType.EquipmentJewelForLevel, 1, false, curModel.Level);
                        //镶嵌大于等于某品质的玄玉发公告
                        BroadCastInlayXuanyuLevel(curModel.Id, curModel.Level);
                    }

                    //装备指定等级
                    AddTaskNumForType(TaskType.EquipmentInject);

                    //玩家行为
                    RecordAction(ActionType.HeroEquipmentInject, msg.HeroId);
                }
                int oldJewelId = 0;
                if (oldJewel > 0)
                {
                    ItemXuanyuModel curModel = EquipmentManager.GetXuanyuModel(slot.JewelUid);
                    if (curModel != null)
                    {
                        oldJewelId = curModel.Id;
                    }
                }
                //养成
                BIRecordDevelopLog(DevelopType.InjectEquipment, msg.Slot, oldJewelId, result.JewelId, hero.Id, hero.Level);
                //komoelog           
                KomoeEventLogEquitFlow(hero.Id.ToString(), "", "", 4, operateType, oldJewel.ToString(), newJewel.ToString(), beforePower, afterPower, afterPower - beforePower, 0, 0);
            }

            Write(result);
        }

        /// <summary>
        /// 替换注能宝石
        /// </summary>
        /// <param name="solt"></param>
        /// <param name="newInject"></param>
        private bool SwapJewel(Slot solt, ulong newJewelUid, HeroInfo heroInfo)
        {
            ulong oldJewelUid = solt.JewelUid;
            if (newJewelUid == oldJewelUid)
            {
                return false;
            }

            //注能属性替换
            var oldNature = EquipmentManager.CalcEquipInjectionNature(solt, heroInfo.Level);
            solt.JewelUid = newJewelUid;
            var newNature = EquipmentManager.CalcEquipInjectionNature(solt, heroInfo.Level);
            HeroMng.InjectionNatureSwap(heroInfo, newNature, oldNature);

            //宝石数量变化统计
            EquipmentManager.MinusXuanyuCount(oldJewelUid);
            EquipmentManager.AddXuanyuCount(newJewelUid);

            var oldJewel = bagManager.GetItem(oldJewelUid);
            var newJewel = bagManager.GetItem(newJewelUid);

            List<BaseItem> updateList = new List<BaseItem>();
            updateList.Add(EquipmentManager.Bag.GetItem(solt.EquipmentUid));
            updateList.Add(oldJewel);
            updateList.Add(newJewel);
            SyncClientItemsInfo(updateList);

            return true;
        }

        /// <summary>
        ///注入宝石
        /// </summary>
        /// <param name="solt"></param>
        /// <param name="newInject"></param>
        private bool InjectJewel(Slot solt, ulong newJewelUid, HeroInfo heroInfo)
        {
            //注能属性替换
            solt.JewelUid = newJewelUid;
            var newNature = EquipmentManager.CalcEquipInjectionNature(solt, heroInfo.Level);
            HeroMng.InjectionNatureSwap(heroInfo, newNature, null);

            //宝石数量变化统计
            EquipmentManager.AddXuanyuCount(newJewelUid);

            var newJewel = bagManager.GetItem(newJewelUid);

            List<BaseItem> updateList = new List<BaseItem>();
            updateList.Add(EquipmentManager.Bag.GetItem(solt.EquipmentUid));
            updateList.Add(newJewel);
            SyncClientItemsInfo(updateList);

            return true;
        }

        /// <summary>
        /// 卸下宝石
        /// </summary>
        /// <param name="solt"></param>
        /// <param name="newJewel"></param>
        private void DisboardJewel(Slot solt, HeroInfo heroInfo)
        {
            ulong oldJewelUid = solt.JewelUid;

            //注能属性替换
            var oldNature = EquipmentManager.CalcEquipInjectionNature(solt, heroInfo.Level);
            solt.JewelUid = 0;
            HeroMng.InjectionNatureSwap(heroInfo, null, oldNature);

            //宝石数量变化统计
            EquipmentManager.MinusXuanyuCount(oldJewelUid);

            var oldJewel = bagManager.GetItem(oldJewelUid);

            List<BaseItem> updateList = new List<BaseItem>();
            updateList.Add(EquipmentManager.Bag.GetItem(solt.EquipmentUid));
            updateList.Add(oldJewel);
            SyncClientItemsInfo(updateList);
        }

        //魂玉洗练
        public void JewelAdvance(int id, int num)
        {
            MSG_ZGC_JEWEL_ADVANCE msg = new MSG_ZGC_JEWEL_ADVANCE();
            if (num < 0)
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            num = Math.Min(num, GameConfig.JewelAdvanceMaxCount);
            if (num < 0)
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            num = Math.Min(num, GameConfig.JewelAdvanceMaxCount);
            NormalItem normalItem = bagManager.NormalBag.GetItem(id) as NormalItem;
            if (normalItem == null || num > normalItem.PileNum)
            {
                Log.Warn($"player {uid} JewelAdvance item {id} not enough {num}");
                msg.Result = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            XuanyuAdvanceModel model = EquipLibrary.GetXuanyuAdvanceModel(id);
            if (model == null)
            {
                msg.Result = (int)ErrorCode.CurJewelCannotAdvance;
                Write(msg);
                return;
            }

            NormalItem costItem = bagManager.NormalBag.GetItem(model.CostItem.Id) as NormalItem;
            if (costItem == null || num > model.CostItem.Num)
            {
                Log.Warn($"player {uid} JewelAdvance cost item not enough");
                msg.Result = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            Dictionary<int, int> randomResult = new Dictionary<int, int>();
            for (int i = 0; i < num; i++)
            {
                int jewelId = model.RandomOne();

                ItemModel itemModel = BagLibrary.GetItemModel(jewelId);
                if (itemModel == null)
                {
                    Log.WarnLine($"player {uid} JewelAdvance have error : had not find  model {jewelId}");
                    continue;
                }

                randomResult.AddValue(jewelId, 1);
            }

            List<BaseItem> updateItems = new List<BaseItem>();
            updateItems.Add(DelItem2Bag(normalItem, RewardType.NormalItem, num, ConsumeWay.JewelAdvance));
            updateItems.Add(DelItem2Bag(costItem, RewardType.NormalItem, num * model.CostItem.Num, ConsumeWay.JewelAdvance));

            foreach (var kv in randomResult)
            {
                updateItems.AddRange(AddItem2Bag(MainType.Material, RewardType.NormalItem, kv.Key, kv.Value, ObtainWay.JewelAdvance));
                msg.Rewards.Add(new REWARD_ITEM_INFO() { MainType = (int)RewardType.NormalItem, TypeId = kv.Key, Num = kv.Value });
            }

            SyncClientItemsInfo(updateItems);

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        /// <summary>
        /// 卸下所有的并且传回 未同步前端的装备物品
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public void EquipmentRevert(int heroId, RewardManager manager)
        {
            //处理所有slot信息，包括 装备等级 注能宝石注能信息
            Dictionary<int, Slot> slots = null;
            string rew = "";
            Bag_Equip bag = bagManager.EquipBag;
            List<ItemBasicInfo> rewards = new List<ItemBasicInfo>();

            if (EquipmentManager.GetSlotDic().TryGetValue(heroId, out slots))
            {
                foreach (var kv in slots)
                {
                    var slot = kv.Value;
                    var equipmentUid = slot.EquipmentUid;
                    var equipment = bag.GetItem(equipmentUid) as EquipmentItem;
                    if (equipment == null)
                    {
                        continue;
                    }

                    string reward = EquipLibrary.GetLevelReward(slot.EquipLevel);
                    rew += "|" + reward;
                    ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.Equip, equipment.Id, 1, null);
                    rewards.Add(baseInfo);
                    EquipmentManager.TakeOff(heroId, slot, equipment);

                    bag.DelItem(equipmentUid, 1);
                }
            }

            List<ItemBasicInfo> allRewards = RewardDropLibrary.GetSimpleRewards(rew);
            manager.AddReward(allRewards);
            manager.AddReward(rewards);
        }

        public ZMZ_HERO_EQUIP_SLOT GetHeroSlotTransform(HeroInfo heroInfo)
        {
            ZMZ_HERO_EQUIP_SLOT equipSlots = new ZMZ_HERO_EQUIP_SLOT();
            Dictionary<int, Slot> slots;
            if (EquipmentManager.GetSlotDic().TryGetValue(heroInfo.Id, out slots))
            {
                foreach (var slot in slots)
                {
                    ZMZ_EQUIP_SLOT slotInfo = GetSlotTransform(slot.Key, slot.Value);
                    equipSlots.SlotInfos.Add(slotInfo);
                }
            }
            return equipSlots;
        }

        private ZMZ_EQUIP_SLOT GetSlotTransform(int part, Slot slot)
        {
            ZMZ_EQUIP_SLOT slotInfo = new ZMZ_EQUIP_SLOT();
            slotInfo.Part = part;
            slotInfo.EquipLevel = slot.EquipLevel;
            slotInfo.EquipmentUid = slot.EquipmentUid;
            slotInfo.Injection = slot.Injection;
            slotInfo.JewelUid = slot.JewelUid;
            return slotInfo;
        }

        private Slot GetSlotFromTransform(ZMZ_EQUIP_SLOT slot)
        {
            Slot slotInfo = new Slot();
            slotInfo.Part = slot.Part;
            slotInfo.EquipLevel = slot.EquipLevel;
            slotInfo.EquipmentUid = slot.EquipmentUid;
            slotInfo.Injection = slot.Injection;
            slotInfo.JewelUid = slot.JewelUid;
            return slotInfo;
        }

        /// <summary>
        /// 一键穿戴/替换
        /// </summary>
        public void EquipBetterEquipments(int heroId, RepeatedField<ulong> equipmentUids)
        {
            MSG_ZGC_EQUIP_BETTER_EQUIPMENT response = new MSG_ZGC_EQUIP_BETTER_EQUIPMENT();
            response.HeroId = heroId;
            response.Result = (int)ErrorCode.Fail;

            List<BaseItem> updateList = new List<BaseItem>();
            foreach (var equipUid in equipmentUids)
            {
                ErrorCode result = EquipmentSwap(heroId, equipUid, ref updateList);
                if (result == ErrorCode.Success)
                {
                    //只要有一个穿戴成功则返回结果就是成功，成功则返回穿戴成功的uid
                    response.Equipments.Add(new ZGC_EQUIPMENT_UID() { UidHigh = equipUid.GetHigh(), UidLow = equipUid.GetLow() });
                    response.Result = (int)result;
                }
            }
            SyncClientItemsInfo(updateList);
            Write(response);
        }

        /// <summary>
        /// 装备进阶
        /// </summary>
        public void EquipmentAdvance(int heroId, int slotId, RepeatedField<ulong> equipmentUid)
        {
            MSG_ZGC_EQUIPMENT_ADVANCE response = new MSG_ZGC_EQUIPMENT_ADVANCE() {HeroId = heroId};

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn("player {0} EquipmentAdvance to Hero {1} failed! heroid wrong {2}", Uid, heroId, heroId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //检查是否有装备在
            Slot slot = EquipmentManager.GetSlot(heroId, slotId);
            if (slot == null)
            {
                Log.Warn($"player {Uid} EquipmentAdvance cannot find hero slot");
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            if (slot.EquipmentUid <= 0)
            {
                Log.Warn($"player {Uid} EquipmentAdvance hero {heroId} slot{slotId} failed: not find equipment on slot");
                response.Result = (int)ErrorCode.NoEquipment;
                Write(response);
                return;
            }

            Bag_Equip bag = BagManager.EquipBag;
            List<BaseItem> removeList = new List<BaseItem>();

            EquipmentItem oldEquipItem = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
            if (oldEquipItem == null)
            {
                Log.Warn("player {0} EquipmentAdvance to Hero {1} failed with no such item {2}", Uid, heroId, slot.EquipmentUid);
                response.Result = (int)ErrorCode.NoSuchItem;
                Write(response);
                return;
            }

            EquipAdvanceModel advanceModel = EquipLibrary.GetEquipAdvanceModel(oldEquipItem.Model.ID);
            if (advanceModel == null)
            {
                Log.Warn("player {0} EquipmentAdvance to Hero {1} failed with no such advance model {2}", Uid, heroId, oldEquipItem.Model.ID);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (equipmentUid.Count != advanceModel.Num)
            {
                Log.Warn($"player {uid} EquipmentAdvance to Hero {heroId} failed equip not enough need {advanceModel.Num} cur {equipmentUid.Count}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            var conis = StringSplit.GetKVPairs(advanceModel.Data.GetString("Price"), 1);
            if (!CheckCoins(conis))
            {
                Log.Warn("player {0} EquipmentAdvance fail , item {1} compose coins not enough", Uid, advanceModel.Id);
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            //合成消耗的自生，需要配置在Material字段中，同时需要配置合成一个需要自身道具的数量Num
            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>();
            bool haveEnoughMaterial = CheckForgeCostMeterial(advanceModel, 1, costItems);
            if (!haveEnoughMaterial)
            {
                Log.Warn($"player {uid} EquipmentAdvance fail , item {advanceModel.Id} forge material not enough");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            ErrorCode errorCode = ErrorCode.Success;
            foreach (var equipUid in equipmentUid)
            {
                errorCode = EquipmentAdvanceCheck(advanceModel, heroInfo, equipUid, removeList);
                if(errorCode!= ErrorCode.Success) break;
            }

            if (errorCode != ErrorCode.Success)
            {
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            //扣除货币
            DelCoins(conis, ConsumeWay.EquipmentAdvance, advanceModel.Id.ToString());

            //先移除旧的装备
            List<BaseItem> updateList = new List<BaseItem>();
            updateList.AddRange(DelItem2Bag(costItems, RewardType.Equip, ConsumeWay.EquipmentAdvance));

            foreach (var item in removeList)
            {
                updateList.Add(DelItem2Bag(item, RewardType.Equip, item.PileNum, ConsumeWay.EquipmentAdvance, item.Id.ToString()));   
            }

            //再添加新装备
            EquipmentItem newEquipmentItem = bag.AddEquipment(advanceModel.Product, false);
            if (newEquipmentItem != null)
            {
                EquipmentSwap(heroId, newEquipmentItem.Uid, ref updateList);
            }

            DelItem2Bag(oldEquipItem, RewardType.Equip, oldEquipItem.PileNum, ConsumeWay.EquipmentAdvance, oldEquipItem.Id.ToString());

            response.Equipments = new ZGC_EQUIPMENT_UID()
            {
                UidHigh = newEquipmentItem.Uid.GetHigh(),
                UidLow = newEquipmentItem.Uid.GetLow(),
            };

            response.Result = (int) ErrorCode.Success;
            SyncClientItemsInfo(updateList);
            Write(response);
        }

        private ErrorCode EquipmentAdvanceCheck(EquipAdvanceModel advanceModel, HeroInfo heroInfo, ulong costUid, List<BaseItem> updateList)
        {
            int heroId = heroInfo.Id;
            ErrorCode result = ErrorCode.Success;
            Bag_Equip bag = BagManager.EquipBag;


            EquipmentItem newEquipItem = bag.GetItem(costUid) as EquipmentItem;
            if (newEquipItem == null)
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed with no such item {2}", Uid, heroId, costUid);
                result = ErrorCode.NoSuchItem;
                return result;
            }

            if (newEquipItem.EquipInfo.EquipHeroId > 0 || newEquipItem.Id != advanceModel.CostEquipId)
            {
                Log.Warn("player {0} EquipmentAdvanceCheck Hero {1} failed! already equipped to hero {2}", Uid, heroId, newEquipItem.EquipInfo.EquipHeroId);
                result = ErrorCode.Fail;
                return result;
            }

            if (newEquipItem.Model.Job != heroInfo.GetData().GetInt("Job"))
            {
                Log.Warn("player {0} equip equipment to Hero {1} failed with JobNotPermission", Uid, heroId);
                result = ErrorCode.JobNotPermission;
                return result;
            }

            Slot slot = EquipmentManager.GetSlot(heroId, newEquipItem.Model.Part);
            if (slot == null)
            {
                Log.Warn($"player {Uid} injectEquipment cannot find hero slot");
                result = ErrorCode.NoHeroInfo;
                return result;
            }

            updateList.Add(newEquipItem);

            return result;
        }

        /// <summary>
        /// 装备进阶
        /// </summary>
        public void EquipmentAdvanceOneKey(RepeatedField<int> equipmentIds, bool once)
        {
            MSG_ZGC_EQUIPMENT_ADVANCE_ONE_KEY response = new MSG_ZGC_EQUIPMENT_ADVANCE_ONE_KEY();

            Bag_Equip bag = BagManager.EquipBag;

            Dictionary<int, int> costCoin = new Dictionary<int, int>();
            Dictionary<int, int> giveNum = new Dictionary<int, int>();
            Dictionary<int, List<EquipmentItem>> costEquips = new Dictionary<int, List<EquipmentItem>>();

            //合成消耗的自生，需要配置在Material字段中，同时需要配置合成一个需要自身道具的数量Num
            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>();

            foreach (var id in equipmentIds)
            {
                EquipmentModel equipmentModel = EquipLibrary.GetEquipModel(id);
                EquipAdvanceModel advanceModel = EquipLibrary.GetEquipAdvanceModel(id);
                if (advanceModel == null || equipmentModel == null)
                {
                    Log.Warn($"EquipmentAdvanceOneKey had not find advance model {id}");
                    continue;
                }

                bool enchantEquip = equipmentModel.Suit > 0;

                if (!once && !advanceModel.OneKeyAdvance)
                {
                    Log.Warn($"EquipmentAdvanceOneKey not allow advance one key advance model {id}");
                    continue;
                }

                List<EquipmentItem> haveItems = bag.GetItems(advanceModel.CostEquipId);
                if (haveItems.Count < advanceModel.OneKeyAdvanceCostNum)
                {
                    Log.Warn($"EquipmentAdvanceOneKey have not enough cost equipment advance model {id}");
                    continue;
                }

                //批量合成，消耗所有现有的装备，单件合成需要只需要合成一件

                //需要给的
                int giveCount = 1;
                int more = haveItems.Count - advanceModel.OneKeyAdvanceCostNum;

                if (!once)
                {
                    //多出来的
                    more = haveItems.Count % advanceModel.OneKeyAdvanceCostNum;
                    giveCount = haveItems.Count / advanceModel.OneKeyAdvanceCostNum;
                }

                for (int i = 0; i < more; i++)
                {
                    haveItems.RemoveAt(0);
                }

                if (haveItems.Count != giveCount * advanceModel.OneKeyAdvanceCostNum)
                {
                    Log.Warn($"EquipmentAdvanceOneKey caculate error check");
                    continue;
                }

                if (enchantEquip)
                {
                    if (!once)
                    {
                        Log.Warn($"EquipmentAdvanceOneKey enchant equip can not do one key advance");
                        continue;
                    }

                    List<EquipmentItem> enchantItems = bag.GetItems(id);
                    if (haveItems.Count <= 0)
                    {
                        Log.Warn($"EquipmentAdvanceOneKey have not enough cost equipment advance model {id}");
                        continue;
                    }

                    haveItems.Add(enchantItems.First());
                }


                var coins = advanceModel.Data.GetString("Price").GetKVPairs(giveCount);
                costCoin.AddValue(coins);
                if (!CheckCoins(coins))
                {
                    Log.Warn("player {0} EquipmentAdvanceOneKey have not enough", Uid);
                    break;
                }

                bool haveEnoughMaterial = CheckForgeCostMeterial(advanceModel, giveCount, costItems);
                if (!haveEnoughMaterial)
                {
                    Log.Warn($"player {uid} EquipmentAdvanceOneKey fail , item {advanceModel.Id} forge material not enough");
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Write(response);
                    return;
                }

                giveNum.Add(advanceModel.Product, giveCount);
                costEquips.Add(id, haveItems);
            }

            if (giveNum.Count == 0)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //扣除货币
            DelCoins(costCoin, ConsumeWay.EquipmentAdvance, "");

            //先移除旧的装备
            List<BaseItem> updateList = new List<BaseItem>();

            //删除道具
            updateList.AddRange(DelItem2Bag(costItems, RewardType.NormalItem, ConsumeWay.EquipmentAdvance));

            foreach (var item in costEquips)
            {
                foreach (var curr in item.Value)
                {
                    updateList.Add(DelItem2Bag(curr, RewardType.Equip, curr.PileNum, ConsumeWay.EquipmentAdvance, curr.Id.ToString()));
                }
            }

            RewardManager manager = new RewardManager();

            //再添加新装备
            foreach (var kv in giveNum)
            {
                manager.AddReward(new ItemBasicInfo((int)RewardType.Equip, kv.Key, kv.Value));
            }
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.EquipmentAdvance);

            manager.GenerateRewardMsg(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            SyncClientItemsInfo(updateList);
            Write(response);
        }


        public  void EquipmentEnchant(ulong  equipUid, int itemId)
        {
            MSG_ZGC_EQUIPMENT_ENCHANT response = new MSG_ZGC_EQUIPMENT_ENCHANT();

            Bag_Equip bag = bagManager.EquipBag;

            EquipmentItem equipment = bag.GetItem(equipUid) as EquipmentItem;
            if (equipment == null)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : had not find equipment {equipUid}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            EquipmentModel model = EquipLibrary.GetEquipModel(equipment.Id);
            if (model == null)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : had not find equipment {equipment.Id}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            EquipmentSuitEnchantItemModel itemModel = EquipLibrary.GetEquipmentSuitEnchantItemModel(itemId);
            if (itemModel == null)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : had not find EquipmentSuitEnchantItemModel {model.ID}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem costItem = bagManager.NormalBag.GetItem(itemId);
            if (costItem == null || costItem.PileNum < 1)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : item {itemId} not enough");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            if (!model.IsEnchant)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : item {itemId} not enough");
                response.Result = (int)ErrorCode.EnchantNotAllowed;
                Write(response);
                return;
            }

            if (itemModel.JobType != model.Job || !itemModel.PartList.Contains(model.Part))
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : item {itemId} EnchantItemTypeError equip job {model.Job} item job {itemModel.JobType}");
                response.Result = (int)ErrorCode.EnchantItemTypeError;
                Write(response);
                return;
            }

            EquipmentModel enchantModel = EquipLibrary.EquipmentEnchant(model, itemModel);
            if (enchantModel == null)
            {
                Log.Warn($"player {uid} EquipmentEnchant failed : EquipmentModel null equip {model.ID} item id {itemModel.Id} ");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int heroId = equipment.EquipInfo.EquipHeroId;

            List<BaseItem> updateList = new List<BaseItem>();
            updateList.Add(DelItem2Bag(costItem, RewardType.Equip, 1, ConsumeWay.EquipmentEnchant));

            //再添加新装备
            EquipmentItem newEquipmentItem = bag.AddEquipment(enchantModel.ID, false);
            if (newEquipmentItem != null && heroId > 0)
            {
                EquipmentSwap(heroId, newEquipmentItem.Uid, ref updateList);
            }
            else
            {
                updateList.Add(newEquipmentItem);
                updateList.Add(equipment);
            }

            DelItem2Bag(equipment, RewardType.Equip, equipment.PileNum, ConsumeWay.EquipmentEnchant, equipment.Id.ToString());

            response.Equipments = new ZGC_EQUIPMENT_UID()
            {
                UidHigh = newEquipmentItem.Uid.GetHigh(),
                UidLow = newEquipmentItem.Uid.GetLow(),
            };

            response.Result = (int)ErrorCode.Success;
            SyncClientItemsInfo(updateList);

            Write(response);
        }
    }
}

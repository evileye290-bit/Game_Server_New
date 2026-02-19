using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Newtonsoft.Json;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public HiddenWeaponManager HiddenWeaponManager { get; private set; }

        private void InitWeaponManager()
        {
            HiddenWeaponManager = new HiddenWeaponManager(this);
        }

        //升级
        private void UpdateHiddenWeaponLevelNature(HeroInfo info, HiddenWeaponItem weaponItem, int oldLevel, int curLevel)
        {
            if (curLevel == oldLevel) return;

#if DEBUG
            Log.Debug($"------------------------------------------------------ {info.Id} weapon level before nature 1");
            Log.Info((object)JsonConvert.SerializeObject(info.Nature.GetNatureList()));
            Log.Debug($"------------------------------------------------------ {info.Id} weapon add before nature 2");
#endif

            int heroId = info.Id;
            Dictionary<NatureType, long> oldNature = new Dictionary<NatureType, long>();
            Dictionary<NatureType, long> curNature = new Dictionary<NatureType, long>();

            HiddenWeaponModel model = HiddenWeaponLibrary.GetHiddenWeaponModel(weaponItem.Model.Id);
            if (model == null) return;

            if (oldLevel > 0)
            {
                HiddenWeaponUpgradeModel oldUpModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(model.UpgradePool, oldLevel);
                if (oldUpModel != null)
                {
                    oldNature = new Dictionary<NatureType, long>(oldUpModel.UpgradeAddNature);
                }
                else
                {
                    Log.Warn($"player {Uid} UpdateHiddenWeaponNature error: hero {heroId} not upgrade level {oldLevel} model");
                }
            }

            if (curLevel > 0)
            {
                var curUpModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(model.UpgradePool, curLevel);
                if (curUpModel != null)
                {
                    curNature = new Dictionary<NatureType, long>(curUpModel.UpgradeAddNature);
                }
                else
                {
                    Log.Warn($"player {Uid} UpgradeEquipment error: hero {heroId} not upgrade level {curLevel} model");
                }
            }

            oldNature = HeroMng.Nature4To9(info, oldNature);
            curNature = HeroMng.Nature4To9(info, curNature);

            HeroMng.UpdateEquipmentNature(info, curNature, oldNature);

#if DEBUG
            Log.Debug($"------------------------------------------------------ {info.Id} weapon level after nature 1");
            Log.Info((object)JsonConvert.SerializeObject(info.Nature.GetNatureList()));
            Log.Debug($"------------------------------------------------------ {info.Id} weapon add after nature 2");
#endif

            HeroMng.UpdateBattlePower(info.Id);
            HeroMng.NotifyClientBattlePowerFrom(info.Id);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { info });
        }

        //洗练
        private void UpdateHiddenWeaponWashNature(HeroInfo info, HiddenWeaponItem weaponItem, List<int> oldList, List<int> newList)
        {
            int heroId = info.Id;
            Dictionary<NatureType, long> oldNature = new Dictionary<NatureType, long>();
            Dictionary<NatureType, long> curNature = new Dictionary<NatureType, long>();

            HiddenWeaponModel model = HiddenWeaponLibrary.GetHiddenWeaponModel(weaponItem.Model.Id);
            if (model == null) return;

            foreach (var id in oldList)
            {
                HiddenWeaponWashModel washModel= HiddenWeaponLibrary.GetHiddenWeaponWashModel(id);
                if (washModel != null)
                {
                    if (oldNature.ContainsKey(washModel.NatureType))
                    {
                        oldNature[washModel.NatureType] += washModel.NatureValue;
                    }
                    else
                    {
                        oldNature.Add(washModel.NatureType, washModel.NatureValue);
                    }
                }
                else
                {
                    Log.Warn($"player {Uid} UpdateHiddenWeaponWashNature error: hero {heroId} not wash {id} model");
                }
            }

            foreach (var id in newList)
            {
                HiddenWeaponWashModel washModel = HiddenWeaponLibrary.GetHiddenWeaponWashModel(id);
                if (washModel != null)
                {
                    if (curNature.ContainsKey(washModel.NatureType))
                    {
                        curNature[washModel.NatureType] += washModel.NatureValue;
                    }
                    else
                    {
                        curNature.Add(washModel.NatureType, washModel.NatureValue);
                    }
                }
                else
                {
                    Log.Warn($"player {Uid} UpdateHiddenWeaponWashNature error: hero {heroId} not wash {id} model");
                }
            }

            oldNature = HeroMng.Nature4To9(info, oldNature);
            curNature = HeroMng.Nature4To9(info, curNature);

            HeroMng.UpdateEquipmentNature(info, curNature, oldNature);

            HeroMng.UpdateBattlePower(info.Id);
            HeroMng.NotifyClientBattlePowerFrom(info.Id);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { info });
        }

        /// <summary>
        /// 装备or替换
        /// </summary>
        public void EquipHiddenWeapon(ulong weaponId, int heroId)
        {
            MSG_ZGC_HIDENWEAPON_EQUIP result = new MSG_ZGC_HIDENWEAPON_EQUIP();
            List<BaseItem> updateList = new List<BaseItem>();

            result.Result = (int)HiddenWeaponSwap(heroId, weaponId, ref updateList);
            SyncClientItemsInfo(updateList);
            Write(result);
        }

        /// <summary>
        /// 穿戴and替换
        /// </summary>
        private ErrorCode HiddenWeaponSwap(int heroId, ulong newWeaponId, ref List<BaseItem> updateList)
        {
            ErrorCode result = ErrorCode.Success;
            Bag_HiddenWeapon bag = BagManager.HiddenWeaponBag;
            HiddenWeaponItem weaponItem = bag.GetItem(newWeaponId) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                Log.Warn("player {0} HiddenWeaponSwap to Hero {1} failed with no such item {2}", Uid, heroId, newWeaponId);
                result = ErrorCode.NoSuchItem;
                return result;
            }

            if (weaponItem.Info.EquipHeroId > 0)
            {
                Log.Warn("player {0} HiddenWeaponSwap to Hero {1} failed!already equip to hero {2}", Uid, heroId, weaponItem.Info.EquipHeroId);
                result = ErrorCode.Fail;
                return result;
            }

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn("player {0} HiddenWeaponSwap to Hero {1} failed! hero id wrong {2}", Uid, heroId, heroId);
                result = ErrorCode.Fail;
                return result;
            }

            // 判断当前是否能穿
            if (heroInfo.Level < weaponItem.Model.Data.GetInt("LevelLimit"))
            {
                Log.Warn("player {0} HiddenWeaponSwap to Hero {1} failed: level {2} limit", Uid, heroId, heroInfo.Level);
                result = ErrorCode.LevelLimit;
                return result;
            }

            Dictionary<NatureType, long> oldNature = null;
            Dictionary<NatureType, long> newNature = null;

            ulong oldUid = HiddenWeaponManager.GetHeroEquipWeaponId(heroId);

            if (oldUid > 0)
            {
                //卸下 旧装备
                //卸下旧装备
                oldNature = HiddenWeaponManager.CalcNature(oldUid);

                HiddenWeaponItem hiddenWeaponItem = HiddenWeaponManager.Bag.GetItem(oldUid) as HiddenWeaponItem;
                hiddenWeaponItem.Info.EquipHeroId = 0;
                bag.SyncDbItemInfo(hiddenWeaponItem);
                updateList.Add(hiddenWeaponItem);

                //装备替换
                BIRecordEquipmentReplaceLog(heroInfo.Id, heroInfo.Level, 5, hiddenWeaponItem.Id, hiddenWeaponItem.Model.Score, weaponItem.Id, weaponItem.Model.Score);
            }
            else
            {
                //装备替换
                BIRecordEquipmentReplaceLog(heroInfo.Id, heroInfo.Level, 5, 0, 0, weaponItem.Id, weaponItem.Model.Score);
            }
            //穿新装备
            weaponItem.Info.EquipHeroId = heroId;
            bag.SyncDbItemInfo(weaponItem);
            updateList.Add(weaponItem);

            HiddenWeaponManager.SetHeroHiddenWeapon(heroId, newWeaponId);

            newNature = HiddenWeaponManager.CalcNature(newWeaponId);

            oldNature = HeroMng.Nature4To9(heroInfo, oldNature);
            newNature = HeroMng.Nature4To9(heroInfo, newNature);

            HeroMng.UpdateEquipmentNature(heroInfo, newNature, oldNature);

            HeroMng.UpdateBattlePower(heroInfo.Id);
            HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
            SyncDbUpdateHeroItem(heroInfo);

            ////穿戴装备
            //AddTaskNumForType(TaskType.EquipEquipment, EquipmentManager.GetEquipedCount(), false);
            ////指定位置穿戴装备数
            //AddTaskNumForType(TaskType.EquipEquipmentBySlot, EquipmentManager.GetEquipedCountBySlot(weaponItem.Model.Part), false, weaponItem.Model.Part);

            ////养成
            //BIRecordDevelopLog(DevelopType.EquipEquipment, weaponItem.Id, 0, weaponItem.Model.Score, heroInfo.Id, heroInfo.Level);

            return result;
        }


        /// <summary>
        /// 暗器强化
        /// </summary>
        /// <param name="msg"></param>
        public void UpgradeHiddenWeapon(int heroId)
        {
            MSG_ZGC_HIDENWEAPON_UPGRADE result = new MSG_ZGC_HIDENWEAPON_UPGRADE();
            result.HeroId = heroId;

            //判断可进行
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            ulong weaponId = HiddenWeaponManager.GetHeroEquipWeaponId(heroId);
            if (weaponId == 0)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId} hidden weapon");
                result.Result = (int)ErrorCode.HeroHadNotEquipHiddenWeapon;
                Write(result);
                return;
            }

            var bag = HiddenWeaponManager.Bag;
            HiddenWeaponItem weaponItem = bag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId} weapon id {weaponId}");
                result.Result = (int)ErrorCode.NoHiddenWeapon;
                Write(result);
                return;
            }

            HiddenWeaponDbInfo info = weaponItem.Info;
            if (weaponItem.Info.Level >= HiddenWeaponLibrary.MaxUpgradeLevel)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error:  weapon id {weaponId} upgrade to max level {HiddenWeaponLibrary.MaxUpgradeLevel}");
                result.Result = (int)ErrorCode.HiddenWeaponMaxLevel;
                Write(result);
                return;
            }

            if (info.NeedStar)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error:  weapon id {weaponId} need upgrade {info.Level}");
                result.Result = (int)ErrorCode.HiddenWeaponNeedStar;
                Write(result);
                return;
            }

            int oldLevel = info.Level;
            var upModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(weaponItem.Model.UpgradePool, oldLevel + 1);
            if (upModel == null)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: hero {heroId} not upgrade level {oldLevel + 1}");
                result.Result = (int)ErrorCode.LevelLimit;
                Write(result);
                return;
            }

            if (!CheckCoins(CurrenciesType.gold, upModel.CostGold))
            {
                result.Result = (int)ErrorCode.GoldNotEnough;
                Write(result);
                return;
            }

            if (upModel.IsNeedStar)
            {
                info.NeedStar = true;
            }

            //消耗金币
            ConsumeWay way = ConsumeWay.HiddenWeaponUpgrade;
            DelCoins(CurrenciesType.gold, upModel.CostGold, way, (info.Level + 1).ToString());

            info.Level += 1;

            bag.SyncDbItemInfo(weaponItem);
            SyncClientItemInfo(weaponItem);

            result.CurLevel = info.Level;
            result.Result = (int) ErrorCode.Success;
            Write(result);

            UpdateHiddenWeaponLevelNature(heroInfo, weaponItem, oldLevel, info.Level);
        }

        //暗器升星
        public void HiddenWeaponStar(int heroId)
        {
            MSG_ZGC_HIDENWEAPON_STAR result = new MSG_ZGC_HIDENWEAPON_STAR();
            result.HeroId = heroId;

            //判断可进行
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            ulong weaponId = HiddenWeaponManager.GetHeroEquipWeaponId(heroId);
            if (weaponId == 0)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId} hidden weapon");
                result.Result = (int)ErrorCode.HeroHadNotEquipHiddenWeapon;
                Write(result);
                return;
            }

            var bag = HiddenWeaponManager.Bag;
            HiddenWeaponItem weaponItem = bag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error: not find hero {heroId} weapon id {weaponId}");
                result.Result = (int)ErrorCode.NoHiddenWeapon;
                Write(result);
                return;
            }

            HiddenWeaponDbInfo info = weaponItem.Info;

            if (!info.NeedStar)
            {
                Log.Warn($"player {Uid} UpgradeHiddenWeapon error:  weapon id {weaponId} need upgrade {info.Level}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            HiddenWeaponStarModel starModel = HiddenWeaponLibrary.GetHiddenWeaponStarModel(weaponItem.Model.Quality,info.Star + 1);
            if (starModel == null || info.Star >= 6)
            {
                Log.Warn($"player {Uid} UpgradeStar error: not find hero {heroId} weapon id {weaponId} star mode {info.Star + 1}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            List<BaseItem> items = new List<BaseItem>() {weaponItem};

            NormalItem normalItem = bagManager.NormalBag.GetItem(weaponItem.Model.StarCostItem) as NormalItem;
            if (normalItem == null || normalItem.PileNum < starModel.StarCostCount)
            {
                Log.Warn($"player {Uid} UpgradeStar error: not find hero {heroId} weapon id {weaponId} star mode {info.Star + 1}");
                result.Result = (int)ErrorCode.ItemNotEnough;
                Write(result);
                return;
            }

            BaseItem item = DelItem2Bag(normalItem, RewardType.NormalItem, starModel.StarCostCount, ConsumeWay.HiddenWeaponUpgrade);
            if (item != null)
            {
                items.Add(item);
            }

            info.Star += 1;
            int id = 0;
            List<int> oldList = new List<int>(info.WashList);

            if (RandomOneNature(weaponItem.Info, weaponItem.Model.Quality, out id))
            {
                result.NewWashId = id;
                UpdateHiddenWeaponWashNature(heroInfo, weaponItem, oldList, info.WashList);
            }
            else
            {
                HeroMng.UpdateBattlePower(heroId);
                HeroMng.NotifyClientBattlePowerFrom(heroId);
                //同步
                SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
            }

            info.NeedStar = false;
            bag.SyncDbItemInfo(weaponItem);
            SyncClientItemsInfo(items);

            result.CurStar = info.Star;
            result.Result = (int) ErrorCode.Success;
            Write(result);
        }

        private bool RandomOneNature(HiddenWeaponDbInfo info, int quality, out int id)
        {
            id = 0;
            int natureCount = HiddenWeaponLibrary.GetWashNatureCount(info.Star);
            if(natureCount<=info.WashList.Count) return false;

            HiddenWeaponWashModel model = HiddenWeaponLibrary.Wash(quality, info.Star);
            if (model == null)
            {
                Log.Error($"wash error : random a null wash model quality {quality} star {info.Star}");
                return false;
            }

            id = model.Id;
            info.WashList.Add(model.Id);
            return true;
        }


        public void HiddenWeaponWash(MSG_GateZ_HIDENWEAPON_WASH msg)
        {
            int heroId = msg.HeroId;
            MSG_ZGC_HIDENWEAPON_WASH result = new MSG_ZGC_HIDENWEAPON_WASH();
            result.HeroId = heroId;

            if (msg.WashCount <= 0)
            {
                Log.Warn($"player {Uid} UpgradeWash error: wash count error {msg.WashCount}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            //判断可进行
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {Uid} UpgradeWash error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            ulong weaponId = HiddenWeaponManager.GetHeroEquipWeaponId(heroId);
            if (weaponId == 0)
            {
                Log.Warn($"player {Uid} UpgradeWash error: hero {heroId} hidden weapon");
                result.Result = (int)ErrorCode.HeroHadNotEquipHiddenWeapon;
                Write(result);
                return;
            }

            var bag = HiddenWeaponManager.Bag;
            HiddenWeaponItem weaponItem = bag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                Log.Warn($"player {Uid} UpgradeWash error: hero {heroId} weapon id {weaponId}");
                result.Result = (int)ErrorCode.NoHiddenWeapon;
                Write(result);
                return;
            }

            int count = msg.WashCount == 1? HiddenWeaponLibrary.WashCost.Num  : HiddenWeaponLibrary.WashCost10.Num;
            BaseItem costItem = BagManager.NormalBag.GetItem(HiddenWeaponLibrary.WashCost.Id);
            if (costItem == null || costItem.PileNum < count)
            {
                result.Result = (int)ErrorCode.HiddenWeaponWashItemNotEnough;
                Write(result);
                return;
            }

            Dictionary<CurrenciesType, int> costInts = new Dictionary<CurrenciesType, int>();

            if (msg.LockIndex.Count > 0)
            {
                ItemBasicInfo diamond = msg.WashCount == 1? HiddenWeaponLibrary.GetLockCost(msg.LockIndex.Count) : HiddenWeaponLibrary.GetLockCost10(msg.LockIndex.Count);
                if (diamond == null)
                {
                    result.Result = (int)ErrorCode.Fail;
                    Write(result);
                    return;
                }

                if (!CheckCoins(CurrenciesType.diamond, diamond.Num))
                {
                    result.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(result);
                    return;
                }

                costInts.Add(CurrenciesType.diamond, diamond.Num);
            }

            HiddenWeaponDbInfo info = weaponItem.Info;

            //达到最大值的不洗练
            for (int i = 0; i < info.WashList.Count; i++)
            {
                if (HiddenWeaponLibrary.RichMaxValue(weaponItem.Model.Quality, info.Star, info.WashList[i]))
                {
                    if (!info.WashList.Contains(info.WashList[i]))
                    {
                        msg.LockIndex.Add(i + 1);
                    }
                }
            }

            //需要洗练的条目数
            int washNatureMaxCount = HiddenWeaponLibrary.GetWashNatureCount(info.Star);
            int washNatureCount = washNatureMaxCount - msg.LockIndex.Count;

            if (washNatureMaxCount <= msg.LockIndex.Count || washNatureCount <= 0)
            {
                Log.Warn($"player {Uid} UpgradeWash error:  hero {heroId} weapon id {weaponId} lock {msg.LockIndex.Count} max wash nature count {washNatureMaxCount}");
                result.Result = (int)ErrorCode.HiddenWeaponHaveNotWashNature;
                Write(result);
                return;
            }

            List<MSG_WASH_RESULT_LIST> washResultList = new List<MSG_WASH_RESULT_LIST>();
            for (int i = 0; i < msg.WashCount; i++)
            {
                MSG_WASH_RESULT_LIST resultList = new MSG_WASH_RESULT_LIST() {Index = i};
                for (int index = 1; index <= washNatureMaxCount; index++)
                {
                    MSG_WASH_RESULT washResult = new MSG_WASH_RESULT() {Index = index};
                    if (msg.LockIndex.Contains(index))
                    {
                        if(info.WashList.Count<index) continue;

                        washResult.Id = info.WashList[index - 1];
                    }
                    else
                    {
                        HiddenWeaponWashModel model = HiddenWeaponLibrary.Wash(weaponItem.Model.Quality, info.Star);
                        if (model == null)
                        {
                            Log.Error($"wash error have not this star {info.Star} model id {model.Id}");
                            continue;
                        }

                        washResult.Id = model.Id;
                    }

                    resultList.IdList.Add(washResult);
                }
                washResultList.Add(resultList);
            }
            HiddenWeaponManager.SetWashList(washResultList);

            DelCoins(costInts, ConsumeWay.HiddenWeapon, "");
            BaseItem sysItem = DelItem2Bag(costItem, RewardType.NormalItem, count, ConsumeWay.HiddenWeapon);
            if (sysItem != null)
            {
                SyncClientItemInfo(sysItem);
            }

            result.WashResult.AddRange(washResultList);

            result.Result = (int)ErrorCode.Success;
            Write(result);
        }

        public void HiddenWeaponWashConfirm(int heroId, int index)
        {
            MSG_ZGC_HIDENWEAPON_WASH_CONFIRM result = new MSG_ZGC_HIDENWEAPON_WASH_CONFIRM();
            result.HeroId = heroId;

            if (index <= 0)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: index {index}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            //判断可进行
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: not find hero {heroId}");
                result.Result = (int)ErrorCode.NoHeroInfo;
                Write(result);
                return;
            }

            ulong weaponId = HiddenWeaponManager.GetHeroEquipWeaponId(heroId);
            if (weaponId == 0)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: hero {heroId} hidden weapon");
                result.Result = (int)ErrorCode.HeroHadNotEquipHiddenWeapon;
                Write(result);
                return;
            }

            var bag = HiddenWeaponManager.Bag;
            HiddenWeaponItem weaponItem = bag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: hero {heroId} weapon id {weaponId}");
                result.Result = (int)ErrorCode.NoHiddenWeapon;
                Write(result);
                return;
            }

            if (HiddenWeaponManager.WashResult.Count < index)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: hero {heroId} index out of range {index}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            HiddenWeaponDbInfo info = weaponItem.Info;

            //需要洗练的条目数
            int washNatureMaxCount = HiddenWeaponLibrary.GetWashNatureCount(info.Star);

            MSG_WASH_RESULT_LIST washResult = HiddenWeaponManager.WashResult[index - 1];
            if (washResult.IdList.Count > washNatureMaxCount)
            {
                Log.Warn($"player {Uid} UpgradeWashConfirm error: hero {heroId} wash cache count less then current weapon max nature count {washNatureMaxCount}");
                result.Result = (int)ErrorCode.Fail;
                Write(result);
                return;
            }

            HiddenWeaponManager.WashResult.Clear();

            List<int> oldList = new List<int>(info.WashList);

            info.WashList.Clear();
            washResult.IdList.OrderBy(x=>x.Index).ForEach(x=>info.WashList.Add(x.Id));

            UpdateHiddenWeaponWashNature(heroInfo, weaponItem, oldList, info.WashList);

            HiddenWeaponManager.Bag.SyncDbItemInfo(weaponItem);
            SyncClientItemInfo(weaponItem);

            server.TrackingLoggerMng.Write($"hidden weapon wash before {oldList.ToString("-")} after {info.WashList.ToString("-")}", TrackingLogType.HiddenWeapon);

            result.Result = (int)ErrorCode.Success;
            Write(result);
        }

        public void HiddenWeaponSmash(RepeatedField<ulong> idList)
        {
            MSG_ZGC_HIDENWEAPON_SMASH result = new MSG_ZGC_HIDENWEAPON_SMASH();

            RewardManager rewardManager = new RewardManager();
            Dictionary<BaseItem, int> smashItems = new Dictionary<BaseItem, int>();
            foreach (var id in idList)
            {
                HiddenWeaponItem baseItem = bagManager.HiddenWeaponBag.GetItem(id) as HiddenWeaponItem;
                if(baseItem == null) continue;

                HiddenWeaponStarModel starModel = HiddenWeaponLibrary.GetHiddenWeaponStarModel(baseItem.Model.Quality, baseItem.Info.Star);
                if (starModel == null)
                {
                    Log.Error($"hidden weapon smash error quality {baseItem.Model.Quality} star {baseItem.Info.Star}");
                    continue;
                }

                if (baseItem.Info.EquipHeroId > 0)
                {
                    continue;
                }

                smashItems.Add(baseItem, 1);

                ItemBasicInfo itemBasicInfo = ItemBasicInfo.Parse(baseItem.Model.Data.GetString("SmashReward"));
                itemBasicInfo.Num = starModel.SmashReward;
                baseItem.Deleted = true;

                rewardManager.AddReward(itemBasicInfo);

                if (baseItem.Info.Level > 0)
                {
                    var upModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(baseItem.Model.UpgradePool, baseItem.Info.Level);
                    if (upModel != null)
                    {
                        rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, upModel.ReturnGold));
                    }
                }
            }

            rewardManager.BreakupRewards();
            AddRewards(rewardManager, ObtainWay.HiddenWeapon);

            List<BaseItem> items = DelItem2Bag(smashItems, RewardType.HiddenWeapon, ConsumeWay.HiddenWeapon);
            if (items != null)
            {
                SyncClientItemsInfo(smashItems.Keys.ToList());
            }

            rewardManager.GenerateRewardItemInfo(result.Rewards);

            result.Result = (int)ErrorCode.Success;
            Write(result);
        }


        private bool HiddenWeaponRevert(int heroId, RewardManager manager)
        {
            HiddenWeaponItem weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(heroId);
            if (weaponItem != null)
            {
                HiddenWeaponManager.TackOff(heroId);
                weaponItem.Info.EquipHeroId = 0;
                HiddenWeaponManager.Bag.SyncDbItemInfo(weaponItem);
                SyncClientItemInfo(weaponItem);

                return true;
            }
            return false;
        }

    }
}

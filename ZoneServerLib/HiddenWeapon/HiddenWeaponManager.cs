using System.Collections.Generic;
using CommonUtility;
using ServerModels;
using ServerShared;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;

namespace ZoneServerLib
{
    public class HiddenWeaponManager
    {
        private Dictionary<int, ulong> heroEquipWeapon = new Dictionary<int, ulong>();
        private List<MSG_WASH_RESULT_LIST> washResult = new List<MSG_WASH_RESULT_LIST>();

        public PlayerChar Owner { get; }

        private Bag_HiddenWeapon bag = null;

        public Bag_HiddenWeapon Bag { get { return bag; } }
        public int EquipCount => heroEquipWeapon.Count;
        public List<MSG_WASH_RESULT_LIST> WashResult => washResult;


        public HiddenWeaponManager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void BindBag(Bag_HiddenWeapon bag)
        {
            this.bag = bag;
        }

        public void SetHeroHiddenWeapon(int heroId, ulong weaponId)
        {
            heroEquipWeapon[heroId] = weaponId;
        }

        public HiddenWeaponItem GetHeroEquipWeapon(int heroId)
        {
            ulong id;
            if (!heroEquipWeapon.TryGetValue(heroId, out id))
            {
                return null;
            }

            return bag.GetItem(id) as HiddenWeaponItem;
        }

        public int GetHeroEquipWeaponTypeId(int heroId)
        {
            var item = GetHeroEquipWeapon(heroId);
            return item?.Id ?? 0;
        }

        public ulong GetHeroEquipWeaponId(int heroId)
        {
            ulong id;
            heroEquipWeapon.TryGetValue(heroId, out id);
            return id;
        }

        public void SetWashList(List<MSG_WASH_RESULT_LIST> washList)
        {
            this.washResult.Clear();
            washResult.AddRange(washList);
        }

        public Dictionary<NatureType, long> CalcNature(ulong weaponUid)
        {
            Dictionary<NatureType, long> keyValuePairs = new Dictionary<NatureType, long>();

            HiddenWeaponItem weaponItem = Bag.GetItem(weaponUid) as HiddenWeaponItem;
            if (weaponItem == null)
            {
                return keyValuePairs;
            }

            var baseNatureDic = weaponItem.Model.BaseNatureDic;

            if (weaponItem.Info.Level >= 0)
            {
                foreach (var kv in baseNatureDic)
                {
                    keyValuePairs.AddValue(kv.Key, kv.Value);
                }
            }

            var upModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(weaponItem.Model.UpgradePool, weaponItem.Info.Level);
            if (upModel != null)
            {
                foreach (var kv in upModel.UpgradeAddNature)
                {
                    keyValuePairs.AddValue(kv.Key, kv.Value);
                }
            }

            if (weaponItem.Info.WashList.Count > 0)
            {
                foreach (var id in weaponItem.Info.WashList)
                {
                    HiddenWeaponWashModel washModel = HiddenWeaponLibrary.GetHiddenWeaponWashModel(id);
                    if (washModel == null) continue;

                    keyValuePairs.AddValue(washModel.NatureType, washModel.NatureValue);
                }
            }

            return keyValuePairs;
        }

        public MSG_ZGC_ITEM_EQUIPMENT GetFinalHiddenWeaponItemInfo(HiddenWeaponItem item, MSG_ZGC_ITEM_EQUIPMENT msg)
        {
            msg.Score = HiddenWeaponItem.HiddenWeaponScore(item.Id, item.Info.Level, item.Info.WashList, item.Info.Star);
            return msg;
        }

        public HiddenWeaponItem GetHeroEquipedHiddenWeapon(int hero)
        {
            ulong uid;
            if (heroEquipWeapon.TryGetValue(hero, out uid))
            {
                HiddenWeaponItem item = bag.GetItem(uid) as HiddenWeaponItem;
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        public void TackOff(int heroId)
        {
            heroEquipWeapon.Remove(heroId);
        }

        //暗器互换
        public void HeroSwapHiddenWeapon(int fromHeroId, int toHeroId, List<BaseItem> updateList, List<BaseItem> deleteList, Dictionary<int, int[]> biEquipsInfo)
        {
            //1:toOldEquipsId  2:toOldEquipsLevel  3:fromOldEquipsId  4:fromOldEquipsLevel
            //5:toNewEquipsId  6.toNewEquipsLevel  7.fromNewEquipsId  8.fromNewEquipsLevel
            int[] toOldEquipsId;
            biEquipsInfo.TryGetValue(1, out toOldEquipsId);
            int[] toOldEquipsLevel;
            biEquipsInfo.TryGetValue(2, out toOldEquipsLevel);
            int[] fromOldEquipsId;
            biEquipsInfo.TryGetValue(3, out fromOldEquipsId);
            int[] fromOldEquipsLevel;
            biEquipsInfo.TryGetValue(4, out fromOldEquipsLevel);
            int[] toNewEquipsId;
            biEquipsInfo.TryGetValue(5, out toNewEquipsId);
            int[] toNewEquipsLevel;
            biEquipsInfo.TryGetValue(6, out toNewEquipsLevel);
            int[] fromNewEquipsId;
            biEquipsInfo.TryGetValue(7, out fromNewEquipsId);
            int[] fromNewEquipsLevel;
            biEquipsInfo.TryGetValue(8, out fromNewEquipsLevel);

            ulong toUid;
            ulong fromUid;
            heroEquipWeapon.TryGetValue(toHeroId, out toUid);           
            HiddenWeaponItem item;         
            heroEquipWeapon.TryGetValue(fromHeroId, out fromUid);
                     
            item = bag.GetItem(toUid) as HiddenWeaponItem;
            if (item != null)
            {
                //BI
                toOldEquipsId[item.Model.Part-1] = item.Id;
                toOldEquipsLevel[item.Model.Part-1] = item.Info.Level;
                fromNewEquipsId[item.Model.Part-1] = item.Id;
                fromNewEquipsLevel[item.Model.Part-1] = item.Info.Level;

                deleteList.Add(item.GenerateDeleteInfo(item.Info));
                item.Info.EquipHeroId = fromHeroId;
                updateList.Add(item);
                bag.SyncDbItemInfo(item);
                heroEquipWeapon[fromHeroId] = toUid;
            }
            else
            {
                //BI
                toOldEquipsId[4] = 0;
                toOldEquipsLevel[4] = 0;
                fromNewEquipsId[4] = 0;
                fromNewEquipsLevel[4] = 0;

                if (fromUid != 0)
                {
                    heroEquipWeapon.Remove(fromHeroId);
                }
            }

            item = bag.GetItem(fromUid) as HiddenWeaponItem;
            if (item != null)
            {
                //BI
                fromOldEquipsId[item.Model.Part-1] = item.Id;
                fromOldEquipsLevel[item.Model.Part-1] = item.Info.Level;
                toNewEquipsId[item.Model.Part-1] = item.Id;
                toNewEquipsLevel[item.Model.Part-1] = item.Info.Level;

                deleteList.Add(item.GenerateDeleteInfo(item.Info));
                item.Info.EquipHeroId = toHeroId;
                updateList.Add(item);
                bag.SyncDbItemInfo(item);
                heroEquipWeapon[toHeroId] = fromUid;
            }
            else
            {
                //BI
                fromOldEquipsId[4] = 0;
                fromOldEquipsLevel[4] = 0;
                toNewEquipsId[4] = 0;
                toNewEquipsLevel[4] = 0;

                if (toUid != 0)
                {
                    heroEquipWeapon.Remove(toHeroId);
                }
            }
        }
    }
}

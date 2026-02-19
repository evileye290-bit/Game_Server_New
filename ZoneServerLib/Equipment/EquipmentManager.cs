using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class EquipmentManager
    {
        private PlayerChar owner;
        private Bag_Equip bag = null;
        private Dictionary<ulong, int> xuanYuCount = new Dictionary<ulong, int>();
        private Dictionary<int, Dictionary<int, Slot>> heroPartSlot = new Dictionary<int, Dictionary<int, Slot>>();

        public Bag_Equip Bag => bag;
        public PlayerChar Owner => owner;


        public EquipmentManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        internal Dictionary<NatureType, long> CalcAllEquipedEquipmentsNatures(int hero, int heroLevel)
        {
            Dictionary<NatureType, long> natures = new Dictionary<NatureType, long>();
            Dictionary<int, Slot> slots;
            if (heroPartSlot.TryGetValue(hero, out slots))
            {
                foreach (var item in slots)
                {
                    var tempEquip = CalcEquipBaseNature(item.Value);
                    var tempInject = CalcEquipInjectionNature(item.Value, heroLevel);

                    CalcTotal(ref natures, tempEquip);
                    CalcTotal(ref natures, tempInject);
                }
            }
            return natures;
        }

        private void CalcTotal(ref Dictionary<NatureType, long> total, Dictionary<NatureType, long> add)
        {
            foreach (var item in add)
            {
                long value;
                if (total.TryGetValue(item.Key, out value))
                {
                    value = value + item.Value;
                    total[item.Key] = value;
                }
                else
                {
                    total.Add(item.Key, item.Value);
                }
            }
        }

        internal List<EquipmentItem> GetAllEquipedEquipments(int hero)
        {
            List<EquipmentItem> equipmentItems = new List<EquipmentItem>();
            Dictionary<int, Slot> slots;
            if (heroPartSlot.TryGetValue(hero, out slots))
            {
                foreach (var item in slots)
                {
                    EquipmentItem equip = bag.GetItem(item.Value.EquipmentUid) as EquipmentItem;
                    if (equip != null)
                    {
                        equipmentItems.Add(equip);
                    }
                }
            }
            return equipmentItems;
        }

        public Dictionary<int, Dictionary<int, Slot>> GetSlotDic()
        {
            return heroPartSlot;
        }

        public int GetUsedOutXuanyuCount()
        {
            int count = 0;
            foreach (var item in xuanYuCount)
            {
                NormalItem nItem = bag.Manager.GetItem(item.Key) as NormalItem;
                if (nItem == null)
                {
                    continue;
                }
                if (nItem.PileNum <= item.Value)
                {
                    count++;
                }
            }
            return count;
        }

        public void InitSlot(int heroId)
        {
            Dictionary<int, Slot> part_slot = new Dictionary<int, Slot>();
            for (int i = 1; i <= 4; i++)
            {
                part_slot.Add(i, new Slot() { Part = i });
            }
            heroPartSlot[heroId] = part_slot;
        }

        public void BindBag(Bag_Equip bag)
        {
            this.bag = bag;
        }

        public Tuple<bool, int> GetLevelWithLevelLimit(int heroId, int slotId)
        {
            Slot slot = GetSlot(heroId, slotId);
            if (slot != null && slot.EquipmentUid > 0)
            {
                EquipmentItem item = Bag.GetItem(slot.EquipmentUid) as EquipmentItem;
                int limit = item.Model.Data.GetInt("UpgradeLimit");
                if (slot.EquipLevel < limit)
                {
                    return Tuple.Create(true, slot.EquipLevel);
                }
                else
                {
                    return Tuple.Create(false, slot.EquipLevel);
                }
            }
            else
            {
                return Tuple.Create(false, 0);
            }
        }

        public Slot GetSlot(int heroId, int slotId)
        {
            Dictionary<int, Slot> partSlot = GetHeroPartSlot(heroId);
            if (partSlot != null)
            {
                return GetSlot(partSlot, slotId);
            }
            return null;
        }

        private Slot GetSlot(Dictionary<int, Slot> partSlot, int slotId)
        {
            Slot slot;
            partSlot.TryGetValue(slotId, out slot);
            return slot;
        }

        public EquipmentItem GetEquipedItem(int heroId, int slotId)
        {
            EquipmentItem item = null;
            Slot slot = GetSlot(heroId, slotId);
            if (slot != null && slot.EquipmentUid > 0)
            {
                item = Bag.GetItem(slot.EquipmentUid) as EquipmentItem;
            }
            return item;
        }

        public Dictionary<int, Slot> GetHeroPartSlot(int heroId)
        {
            Dictionary<int, Slot> part_slot = null;
            heroPartSlot.TryGetValue(heroId, out part_slot);
            return part_slot;
        }

        private void MoveJewelToEmail(ulong jewelUid)
        {
            BaseItem item = Owner.BagManager.GetItem(jewelUid);
            int typeId = item.Id;
            //如果背包有多个会删掉一个，有一个会彻底删除，考虑玄玉没有提交任务
            BaseItem items2 = Owner.DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.EquipmentCrack);

            Owner.BagManager.SendItem2Mail((int)RewardType.NormalItem, typeId, 1);
        }


        public void UpgradeLevel(int heroId, int slotId, int op)
        {
            Dictionary<int, Slot> part_slot = null;
            Slot slot = null;
            if (heroPartSlot.TryGetValue(heroId, out part_slot) && part_slot.TryGetValue(slotId, out slot))
            {
                if (slot.EquipLevel + op > 0)
                {
                    slot.EquipLevel += op;
                    UpdateSlot(heroId, slot);
                }
            }
        }

        public void Upgrade2Level(int heroId, int slotId, int rollBack2Level, EquipInjectionModel back2model = null)
        {
            Dictionary<int, Slot> part_slot = null;
            Slot slot = null;
            if (heroPartSlot.TryGetValue(heroId, out part_slot) && part_slot.TryGetValue(slotId, out slot))
            {
                if (slot.EquipLevel > rollBack2Level)
                {
                    uint temp = slot.Injection;
                    uint param = 0;
                    if (back2model != null && back2model.Slot > 0)
                    {
                        for (int i = 0; i < back2model.Slot; i++)
                        {
                            param += (uint)1 << i;
                        }
                    }
                    slot.Injection = temp & param;
                }
                slot.EquipLevel = rollBack2Level;

                UpdateSlot(heroId, slot);
            }
        }
        private void SyncSlotItem(Slot slot)
        {
            BaseItem item = Bag.GetItem(slot.EquipmentUid);
            if (item != null)
            {
                Owner.SyncClientItemInfo(item);
            }
        }

        public void SyncItems2Client(int heroId)
        {
            Dictionary<int, Slot> slots;
            List<BaseItem> items = new List<BaseItem>();
            if (heroPartSlot.TryGetValue(heroId, out slots))
            {
                foreach (var slot in slots)
                {
                    BaseItem item = Bag.GetItem(slot.Value.EquipmentUid);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
            Owner.SyncClientItemsInfo(items);
        }

        public void SyncXuanyu2Client(ulong jewelUid)
        {
            BaseItem item = Owner.BagManager.GetItem(jewelUid);
            Owner.SyncClientItemInfo(item);
        }

        public bool TakeOff(int heroId, Slot slot, EquipmentItem equipItem)
        {
            slot.Injection = 0;
            slot.EquipmentUid = 0;
            slot.EquipLevel = 0;
            //归还珠宝
            if (slot.JewelUid > 0)
            {
                MinusXuanyuCount(slot.JewelUid);
                //判断是否满了
                if (Owner.BagManager.BagFull())
                {
                    MoveJewelToEmail(slot.JewelUid);
                }
                else
                {
                    SyncXuanyu2Client(slot.JewelUid);
                }
                slot.JewelUid = 0;
            }
            UpdateSlot2DB(heroId, slot);

            equipItem.EquipInfo.EquipHeroId = 0;
            Bag.UpdateEquipment(equipItem);
            return true;
        }

        public void AddEquipment(EquipmentItem item)
        {
            int heroId = item.EquipInfo.EquipHeroId;
            if (heroId > 0)
            {
                Dictionary<int, Slot> slots;
                if (!heroPartSlot.TryGetValue(heroId, out slots))
                {
                    slots = new Dictionary<int, Slot>();
                    heroPartSlot.Add(heroId, slots);
                }

                int part = item.Model.Part;
                Slot slot;
                if (!slots.TryGetValue(part, out slot))
                {
                    slot = new Slot()
                    {
                        Part = part,
                        EquipmentUid = item.Uid
                    };
                    slots.Add(part, slot);
                }
                else
                {
                    slot.Part = part;
                    slot.EquipmentUid = item.Uid;
                }
            }
        }

        public int GetEquipedCount()
        {
            int count = 0;
            foreach (var item in heroPartSlot)
            {
                foreach (var slot in item.Value)
                {
                    if (slot.Value.EquipmentUid > 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public int GetEquipedCountBySlot(int slotPart)
        {
            int count = 0;
            foreach (var item in heroPartSlot)
            {
                foreach (var slot in item.Value)
                {
                    if (slot.Key == slotPart && slot.Value.EquipmentUid > 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void LoadSlot(Dictionary<int, Dictionary<int, Slot>> heroPartSlot)
        {
            foreach (var item in heroPartSlot)
            {
                int heroId = item.Key;
                var partSlot = item.Value;
                foreach (var slotItem in partSlot)
                {
                    Slot tempslot = slotItem.Value;
                    Dictionary<int, Slot> slots;
                    if (!this.heroPartSlot.TryGetValue(heroId, out slots))
                    {
                        slots = new Dictionary<int, Slot>();
                        this.heroPartSlot.Add(heroId, slots);
                    }
                    Slot slot;
                    if (!slots.TryGetValue(slotItem.Key, out slot))
                    {
                        slot = new Slot();
                        slots.Add(slotItem.Key, slot);
                    }

                    if (slot.EquipmentUid > 0)
                    {
                        slot.JewelUid = tempslot.JewelUid;
                    }

                    slot.Part = tempslot.Part;
                    slot.Injection = tempslot.Injection;
                    slot.EquipLevel = tempslot.EquipLevel;

                    AddXuanyuCount(slot.JewelUid);
                }
            }
        }

        public void AddXuanyuCount(ulong jewelUid)
        {
            if (jewelUid > 0)
            {
                int count = 0;
                if (xuanYuCount.TryGetValue(jewelUid, out count))
                {
                    count++;
                    xuanYuCount[jewelUid] = count;
                }
                else
                {
                    xuanYuCount.Add(jewelUid, ++count);
                }
            }
        }

        public void MinusXuanyuCount(ulong jewelUid)
        {
            if (jewelUid > 0)
            {
                int count = 0;
                if (xuanYuCount.TryGetValue(jewelUid, out count))
                {
                    count--;
                    if (count > 0)
                    {
                        xuanYuCount[jewelUid] = count;
                    }
                    else
                    {
                        xuanYuCount.Remove(jewelUid);
                    }
                }
            }
        }

        public bool CheckXuanyuEnough(ulong jewelUid)
        {
            int count = 0;
            xuanYuCount.TryGetValue(jewelUid, out count);
            BaseItem item = Owner.BagManager.GetItem(jewelUid);
            if (item != null)
            {
                return item.PileNum > count;
            }
            else
            {
                return false;
            }
        }

        public bool CheckXuanyuEnough(ulong jewelUid, int cost)
        {
            int count = 0;
            xuanYuCount.TryGetValue(jewelUid, out count);
            BaseItem item = Owner.BagManager.GetItem(jewelUid);
            if (item != null)
            {

                return item.PileNum >= count + cost;

            }
            else
            {
                return false;
            }
        }

        private void UpdateSlot(int heroId, Slot slot)
        {
            UpdateSlot2DB(heroId, slot);
            SyncSlotItem(slot);
        }

        public void UpdateSlot2DB(int heroId, Slot slot)
        {
            QueryUpdateEquipmentSlot query = new QueryUpdateEquipmentSlot(Owner.Uid, heroId, slot);
            Owner.server.GameDBPool.Call(query);
        }

        public ITEM InteceptJewel(ulong uid, ITEM item)
        {
            int count = 0;
            xuanYuCount.TryGetValue(uid, out count);
            item.PileNum -= count;
            return item;
        }

        public MSG_ZGC_ITEM_EQUIPMENT InteceptEquipment(MSG_ZGC_ITEM_EQUIPMENT msg)
        {
            //评分
            Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            EquipmentModel equipModel = EquipLibrary.GetEquipModel(msg.Id);
            if (equipModel != null)
            {
                foreach (var item in equipModel.BaseNatureDic)
                {
                    dic.Add(item.Key, item.Value);
                }
            }

            if (msg.EquipedHeroId > 0)
            {
                HeroInfo hero = owner.HeroMng.GetHeroInfo(msg.EquipedHeroId);
                if (hero != null)
                {
                    Dictionary<int, Slot> slots = null;
                    Slot slot = null;
                    if (heroPartSlot.TryGetValue(msg.EquipedHeroId, out slots) && slots.TryGetValue(msg.PartType, out slot))
                    {
                        ulong jewel = slot.JewelUid;
                        NormalItem item = null;
                        msg.Level = slot.EquipLevel;

                        //计算评分的等级部分
                        if (slot.EquipLevel > 0)
                        {
                            EquipmentModel model = EquipLibrary.GetEquipModel(hero.GetData().GetInt("Job"), msg.PartType, 1);
                            EquipUpgradeModel upModel = EquipLibrary.GetEquipUpgradeModel(slot.EquipLevel);
                            Dictionary<NatureType, long> modeldic = model.GetNatureDic();

                            foreach (var kv in modeldic)
                            {
                                dic[kv.Key] += (int)(kv.Value * (upModel.StrengthRatio / 10000.0000f));
                            }
                        }

                        int percent = 0;
                        ItemXuanyuModel xuanyuModel = null;
                        msg.Slot = new MSG_ZGC_EQUIPMENT_SLOT();

                        if (jewel > 0)
                        {
                            item = Owner.BagManager.GetItem(jewel) as NormalItem;
                            if (item != null)
                            {
                                msg.Slot.JewelTypeId = item.Id;
                                xuanyuModel = EquipLibrary.GetXuanyuItem(item.Id);
                                if (xuanyuModel != null)
                                {
                                    percent = xuanyuModel.Percent;
                                }
                            }
                        }
                        HashSet<int> set = new HashSet<int>();//slot.GenerateInjections();
                        EquipInjectionModel injectModel = EquipLibrary.GetMaxInjectionSlot(slot.EquipLevel, slot.Part);
                        if (injectModel != null)
                        {
                            for (int i = 1; i <= injectModel.Slot; i++)
                            {
                                set.Add(i);
                            }
                        }

                        //处理比例分配
                        if (set.Count > 0)
                        {
                            long tempPer = percent;
                            Dictionary<int, int> natureTypes = EquipLibrary.GetNatureTypesFromInjections(set, slot.Part);
                            EquipmentItem it = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
                            Dictionary<NatureType, int> tempDic = EquipLibrary.GetLevelNatureBase4Injection(hero.Level);

                            //计算评分的注能部分
                            foreach (var nature in natureTypes)
                            {
                                MSG_ZGC_EQUIPMENT_INJECTION injection = new MSG_ZGC_EQUIPMENT_INJECTION
                                {
                                    NatureType = nature.Value,
                                    InjectionSlot = nature.Key
                                };

                                NatureType natureType = (NatureType)nature.Value;

                                int temp = 0;
                                if (tempDic.TryGetValue(natureType, out temp))
                                {
                                    long extraPercent = 0;
                                    if (xuanyuModel != null)
                                    {
                                        xuanyuModel.NatureList.TryGetValue(natureType, out extraPercent);
                                    }

                                    injection.NatureValue = (int)((tempPer + extraPercent) * (temp / 10000.0000f));
                                    dic[(NatureType)nature.Value] += injection.NatureValue;
                                }
                                msg.Slot.Injections.Add(injection);
                            }
                        }
                    }
                }
                else
                {
                    Logger.Log.Warn($"player {Owner.Uid} InteceptEquipment not find hero {msg.EquipedHeroId} by item {msg.UidHigh} {msg.UidLow}");
                }
            }

            msg.Score = ScriptManager.BattlePower.CaculateItemScore2(dic);
            return msg;
        }

        /// <summary>
        /// 注能属性
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="heroLevel"></param>
        /// <returns></returns>
        public Dictionary<NatureType, long> CalcEquipInjectionNature(Slot slot, int heroLevel)
        {
            Dictionary<NatureType, long> keyValuePairs = new Dictionary<NatureType, long>();

            if (slot != null && slot.JewelUid > 0)
            {
                BaseItem baseItem = Owner.BagManager.GetItem(slot.JewelUid);
                if (baseItem == null)
                {
                    Logger.Log.Warn($"player {Owner.Uid} CalcEquipInjectionNature not find slot jewel {slot.JewelUid}");
                    return keyValuePairs;
                }

                NormalItem nitem = baseItem as NormalItem;
                ItemXuanyuModel model = EquipLibrary.GetXuanyuItem(nitem.Id);
                if (model == null)
                {
                    Logger.Log.Warn($"player {Owner.Uid} CalcEquipInjectionNature not find xuan yun {nitem.Id}");
                    return keyValuePairs;
                }
                HashSet<int> set = new HashSet<int>();

                EquipInjectionModel injectModel = EquipLibrary.GetMaxInjectionSlot(slot.EquipLevel, slot.Part);
                if (injectModel != null)
                {
                    for (int i = 1; i <= injectModel.Slot; i++)
                    {
                        set.Add(i);
                    }
                }

                //处理比例分配
                if (set.Count > 0)
                {
                    long tempPer = model.Percent;

                    Dictionary<int, int> natureTypes = EquipLibrary.GetNatureTypesFromInjections(set, slot.Part);
                    Dictionary<NatureType, int> tempDic = EquipLibrary.GetLevelNatureBase4Injection(heroLevel);
                    foreach (var nature in natureTypes)
                    {
                        NatureType natureType = (NatureType)nature.Value;

                        long extraPercent = 0;
                        model.NatureList.TryGetValue(natureType, out extraPercent);

                        int natureValue = 0;
                        int tempNatureValue = 0;
                        if (tempDic.TryGetValue(natureType, out tempNatureValue))
                        {
                            natureValue = (int)((tempNatureValue / 10000.0000f) * (tempPer + extraPercent));
                        }

                        keyValuePairs.AddValue(natureType, natureValue);
                    }
                }

            }
            return keyValuePairs;
        }

        public Dictionary<NatureType, long> CalcEquipBaseNature(Slot slot)
        {
            Dictionary<NatureType, long> keyValuePairs = new Dictionary<NatureType, long>();

            EquipmentItem equipItem = Bag.GetItem(slot.EquipmentUid) as EquipmentItem;
            if (equipItem == null)
            {
                return keyValuePairs;
            }

            var baseNatureDic = equipItem.Model.BaseNatureDic;

            if (slot.EquipLevel > 0)
            {
                EquipmentModel model = EquipLibrary.GetEquipModel(equipItem.Model.Job, equipItem.Model.Part, 1);
                EquipUpgradeModel upModel = EquipLibrary.GetEquipUpgradeModel(slot.EquipLevel);

                if (model != null && upModel != null)
                {
                    foreach (var kv in model.BaseNatureDic)
                    {
                        keyValuePairs.Add(kv.Key, baseNatureDic[kv.Key] + (long)(kv.Value * (upModel.StrengthRatio * 0.0001f)));
                    }
                }
                else
                {
                    foreach (var kv in baseNatureDic)
                    {
                        keyValuePairs.Add(kv.Key, kv.Value);
                    }
                }
            }
            else
            {
                foreach (var kv in baseNatureDic)
                {
                    keyValuePairs.Add(kv.Key, kv.Value);
                }
            }

            return keyValuePairs;
        }

        public Tuple<bool, string> GetSlotEquipment(int heroId, int slotId)
        {
            Slot slot = GetSlot(heroId, slotId);
            if (slot != null && slot.EquipmentUid > 0)
            {
                EquipmentItem item = Bag.GetItem(slot.EquipmentUid) as EquipmentItem;
                int limit = item.Model.Data.GetInt("UpgradeLimit");
                string equipmentId = item.Model.ID.ToString();
                if (slot.EquipLevel == limit)
                {
                    return Tuple.Create(true, equipmentId);
                }
                else
                {
                    return Tuple.Create(false, equipmentId);
                }
            }
            else
            {
                return Tuple.Create(false, "");
            }
        }

        /// <summary>
        /// 获取所有注能100%的个数
        /// </summary>
        /// <returns></returns>
        public int GetFullInjectCount()
        {
            int count = 0;
            foreach (var kv in heroPartSlot)
            {
                foreach (var slot in kv.Value)
                {
                    int num = GetFullInjectBySlot(slot.Value);
                    count += num;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取宝石注能100%个数
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public int GetFullInjectBySlot(Slot slot)
        {
            //int count = 0;
            ulong jewel = slot.JewelUid;
            if (jewel > 0)
            {
                ItemXuanyuModel model = GetXuanyuModel(jewel);
                int percent = model.Percent;
                HashSet<int> set = slot.GenerateInjections();
                //处理比例分配
                if (set.Count > 0)
                {
                    long tempPer = percent / set.Count;
                    if (tempPer >= 10000)
                    {
                        return set.Count;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取所有镶嵌的宝石数
        /// </summary>
        /// <returns></returns>
        public int GetAllJewelCount()
        {
            int count = 0;
            foreach (var kv in heroPartSlot)
            {
                foreach (var slot in kv.Value)
                {
                    if (slot.Value.JewelUid > 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 获取指定等级宝石数
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetJewelCountByLevel(int level)
        {
            int count = 0;
            foreach (var kv in heroPartSlot)
            {
                foreach (var slot in kv.Value)
                {
                    if (slot.Value.JewelUid > 0)
                    {
                        ItemXuanyuModel model = GetXuanyuModel(slot.Value.JewelUid);
                        if (model.Level >= level)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        public ItemXuanyuModel GetXuanyuModel(ulong jewel)
        {
            NormalItem nitem = Owner.BagManager.GetItem(jewel) as NormalItem;
            if (nitem != null)
            {
                return EquipLibrary.GetXuanyuItem(nitem.Id);
            }
            return null;
        }

        public static List<ItemBasicInfo> GenerateEquipmentReward(string soulBoneReward, int job = 0)
        {
            EquipmentReward equipReward = new EquipmentReward(soulBoneReward);
            if (equipReward.GenerateInfo())
            {
                List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
                for (int i = 0; i < equipReward.Num; i++)
                {
                    if (RAND.Range(0, 10000) > equipReward.Prob)
                    {
                        continue;
                    }
                    int animal = equipReward.GetAnimal(job);
                    int part = equipReward.GetPart();
                    int grade = equipReward.GetGrade();

                    EquipmentModel model = EquipLibrary.GetEquipment(animal, part, grade);
                    ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.Equip, model.ID, 1);
                    infos.Add(baseInfo);

                }
                return infos;
            }
            return null;
        }

        public static List<ItemBasicInfo> GenerateEquipmentReward(RewardDropItem dropItem, int job = 0)
        {
            EquipmentReward equipReward = new EquipmentReward(dropItem);
            if (equipReward.Num > 0)
            {
                List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
                for (int i = 0; i < equipReward.Num; i++)
                {
                    if (RAND.Range(0, 10000) > equipReward.Prob)
                    {
                        continue;
                    }
                    if (!dropItem.HasJob)
                    {
                        job = 0;
                    }
                    int animal = equipReward.GetAnimal(job);
                    int part = equipReward.GetPart();
                    int grade = equipReward.GetGrade();

                    EquipmentModel model = EquipLibrary.GetEquipment(animal, part, grade);
                    ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.Equip, model.ID, 1);
                    infos.Add(baseInfo);

                }
                return infos;
            }
            return null;
        }

        public int GetSlotEquipmentLevel(int heroId, int slotId)
        {
            Slot slot = GetSlot(heroId, slotId);
            if (slot != null && slot.EquipmentUid > 0)
            {
                return slot.EquipLevel;
            }
            return 0;
        }

        //装备/玄玉互换
        public void HeroSwapEquipment(int fromHeroId, int toHeroId, List<BaseItem> updateList, List<BaseItem> deleteList, Dictionary<int, int[]> biEquipsInfo)
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

            Dictionary<int, Slot> toDic;
            heroPartSlot.TryGetValue(toHeroId, out toDic);

            EquipmentItem equip;
            Dictionary<int, Slot> fromDic;
            List<EquipmentInfo> equipList = new List<EquipmentInfo>();
            heroPartSlot.TryGetValue(fromHeroId, out fromDic);
            if (fromDic != null)
            {
                foreach (var slot in fromDic.Values)
                {
                    equip = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
                    if (equip != null)
                    {
                        //BI
                        fromOldEquipsId[slot.Part-1] = equip.Id;
                        fromOldEquipsLevel[slot.Part-1] = slot.EquipLevel;
                        toNewEquipsId[slot.Part-1] = equip.Id;
                        toNewEquipsLevel[slot.Part-1] = slot.EquipLevel;

                        deleteList.Add(equip.GenerateDeleteInfo(equip.EquipInfo));
                        equip.EquipInfo.EquipHeroId = toHeroId;
                        equipList.Add(equip.EquipInfo);
                        updateList.Add(equip);
                    }
                    else
                    {
                        //BI
                        fromOldEquipsId[slot.Part-1] = 0;
                        fromOldEquipsLevel[slot.Part-1] = 0;
                        toNewEquipsId[slot.Part-1] = 0;
                        toNewEquipsLevel[slot.Part-1] = 0;
                    }
                    UpdateSlot2DB(toHeroId, slot);
                }
                heroPartSlot[toHeroId] = fromDic;

            }
            else if (toDic != null)
            {
                heroPartSlot.Remove(toHeroId);
            }
            if (toDic != null)
            {
                foreach (var slot in toDic.Values)
                {
                    equip = bag.GetItem(slot.EquipmentUid) as EquipmentItem;
                    if (equip != null)
                    {
                        //BI
                        toOldEquipsId[slot.Part-1] = equip.Id;
                        toOldEquipsLevel[slot.Part-1] = slot.EquipLevel;
                        fromNewEquipsId[slot.Part-1] = equip.Id;
                        fromNewEquipsLevel[slot.Part-1] = slot.EquipLevel;

                        deleteList.Add(equip.GenerateDeleteInfo(equip.EquipInfo));
                        equip.EquipInfo.EquipHeroId = fromHeroId;
                        equipList.Add(equip.EquipInfo);
                        updateList.Add(equip);
                    }
                    else
                    {
                        //BI
                        toOldEquipsId[slot.Part-1] = 0;
                        toOldEquipsLevel[slot.Part-1] = 0;
                        fromNewEquipsId[slot.Part-1] = 0;
                        fromNewEquipsLevel[slot.Part-1] = 0;
                    }
                    UpdateSlot2DB(fromHeroId, slot);
                }
                heroPartSlot[fromHeroId] = toDic;
            }
            else if (fromDic != null)
            {
                heroPartSlot.Remove(fromHeroId);
            }
            bag.SyncDbBatchUpdateItemInfo(equipList);
        }
    }
}

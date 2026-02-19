using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class Bag_SoulRing : BaseBag
    {
        private Dictionary<ulong, SoulRingItem> itemList;//未吸收的魂环
        private SoulRingManager SoulRingManager { get; set; }

        private Dictionary<int, ulong> onAbsorbFlagList = new Dictionary<int, ulong>(); //正在吸收中的魂环标记

        public Bag_SoulRing(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, SoulRingItem>();
            this.SoulRingManager = manager.Owner.SoulRingManager;
        }

        public override int ItemsCount()
        {
            return itemList.Where(x => x.Value.EquipHeroId <= 0).Count();
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            //调用AddSoulRing方法添加魂环
            return null;
        }

        internal bool CheckIsAbsorbed(int heroId)
        {
            return onAbsorbFlagList.ContainsKey(heroId);
        }

        public void AddOnAbsorbFlag(int heroId,ulong uid)
        {
            ulong onAbsorbSoulRingUid;
            if (onAbsorbFlagList.TryGetValue(heroId, out onAbsorbSoulRingUid))
            {
                onAbsorbFlagList[heroId]= uid;
            }
            else
            {
                onAbsorbFlagList.Add(heroId, uid);
            }
        }

        public void DelOnAbsorbFlag(int heroId)
        {
            if (onAbsorbFlagList.ContainsKey(heroId))
            {
                onAbsorbFlagList.Remove(heroId);
            }
        }

        public Tuple<SoulRingItem, bool> AddSoulRing(int id, int mum, int difficulty, int research, ref List<BaseItem> items, ref SoulRingItem maxYearSoulRing, ref List<SoulRingItem> rewardItems
            )
        {
            int year=0;
            Tuple<SoulRingItem, bool> item = null;
            var model = SoulRingLibrary.GetSoulRingMode(id);
            for (int i = 0; i < mum; ++i)
            {
                year = ScriptManager.SoulRing.GetYear(difficulty,  research);
                item = AddSoulRing(id, year, model);

                //用于结算列表显示
                if (item?.Item1 != null)
                {
                    rewardItems.Add(item.Item1);
                }

                if (item != null && item.Item2)
                {
                    items.Add(item.Item1);
                }

                if (maxYearSoulRing == null || maxYearSoulRing.Year<year)
                {
                    maxYearSoulRing = item.Item1;
                }
            }
            return item;
        }

        public Tuple<SoulRingItem, bool> AddSoulRing(int id, int year, SoulRingModel model,bool spaceCheck=true)
        {
            ulong itemUid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
            SoulRingInfo itemInfo = new SoulRingInfo()
            {
                OwnerUid = Manager.Owner.Uid,
                Uid = itemUid,
                TypeId = id,
                PileNum = 1,
                Year = year,
                Level = model.IniLevel,
                EquipHeroId = -1,
                AbsorbState = -1,
                Position = 0,
                Element = 0,
            };

            var item = new SoulRingItem(model, itemInfo);

            item.CheckAddAdditionalNatures();

            if (spaceCheck)
            {
                return AddItemWithSpaceCheck(item);
            }
            else
            {
                return AddItemWithoutSpaceCheck(item);
            }
            //return item;
        }

        //public List<SoulRingItem> AddSoulRing(int id,int num, int year, SoulRingModel model)
        //{
        //    List<SoulRingItem> items = new List<SoulRingItem>();
        //    for (int i = 0; i < num; i++)
        //    {
        //        ulong itemUid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
        //        SoulRingInfo itemInfo = new SoulRingInfo()
        //        {
        //            OwnerUid = Manager.Owner.Uid,
        //            Uid = itemUid,
        //            TypeId = id,
        //            PileNum = 1,
        //            Year = year,
        //            Level = model.IniLevel,
        //            EquipHeroId = -1,
        //            AbsorbState = -1
        //        };

        //        var item = new SoulRingItem(model, itemInfo);
        //        AddItemWithSpaceCheck(item);
        //        items.Add(item);
        //    }
        //    return items;
        //}

        //public List<SoulRingItem> AddSoulRing(int id, int num, int year)
        //{
        //    SoulRingModel model = SoulRingbrary.GetSoulRingMode(id);
        //    if (model == null)
        //    {
        //        return null;
        //    }
        //    return this.AddSoulRing(id, num, year, model);
        //}

        public Tuple<SoulRingItem,bool> AddSoulRing(int id, int year, bool spaceCheck = true)
        {
            SoulRingModel model = SoulRingLibrary.GetSoulRingMode(id);
            if (model == null)
            {
                return null;
            }
            return this.AddSoulRing(id, year, model, spaceCheck);
        }

        public void AddItemAndCheckAbsorb(SoulRingItem item)
        {
            SoulRingItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return;
            }
            itemList.Add(item.Uid, item);

            //对正在吸收中（SoulRingAbsorbState.OnAbsort）的魂环做一个特殊标记，方便做吸收判断。注：需要在吸收完毕时候清除这个标记
            if (item.AbsorbState == (int)SoulRingAbsorbState.OnAbsort)
            {
                AddOnAbsorbFlag(item.EquipHeroId, item.Uid);
            }
        }

        private Tuple<SoulRingItem, bool> AddItemWithoutSpaceCheck(SoulRingItem item)
        {
            SoulRingItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }

            itemList.Add(item.Uid, item);
            InsertItemToDb(item);
            return Tuple.Create(item, true);
        }

        private Tuple<SoulRingItem,bool> AddItemWithSpaceCheck(SoulRingItem item)
        {
            bool toBi = true;
            //背包空间满了放入邮件
            if (this.Manager.BagFull())
            {
                this.Manager.SendItem2Mail((int)RewardType.SoulRing, item.Id, 1, item.Year);
                //return null;
                toBi = false;
            }
            else
            {
                //需要添加到背包
                SoulRingItem temp;
                if (itemList.TryGetValue(item.Uid, out temp))
                {
                    return null;
                }

                itemList.Add(item.Uid, item);
                InsertItemToDb(item);
            }
            return Tuple.Create(item, toBi);
        }

        public override void Clear()
        {
            itemList.Clear();
        }

        public override BaseItem DelItem(ulong uid, int num)
        {
            BaseItem item = GetItem(uid);
            if (item == null)
            {
                Log.WarnLine("player {0} del item fail :beacause item {1} is not found ", Manager.Owner.Uid, uid);
            }
            else
            {
                //已经有了。更新数量
                item.PileNum -= num;
                if (item.PileNum > 0)
                {
                    UpdateItem(item);
                }
                else
                {
                    item.PileNum = 0;
                    RemoveItem(item);
                    DeleteItemFromDb(item);
                }
            }

            return item;
        }

        public BaseItem DelItem(ulong uid)
        {
            BaseItem item = GetItem(uid);
            if (item == null)
            {
                Log.WarnLine("player {0} del item fail :beacause item {1} is not found ", Manager.Owner.Uid, uid);
            }
            else
            {
                //已经有了。更新数量
                RemoveItem(item);
                DeleteItemFromDb(item);
            }
            return item;
        }

        public override Dictionary<ulong, BaseItem> GetAllItems()
        {
            Dictionary<ulong, BaseItem> items = new Dictionary<ulong, BaseItem>();

            itemList.ForEach(kv => items.Add(kv.Key, kv.Value));

            this.SoulRingManager.EquipedSoulRings.ForEach(kv => {
                kv.Value.ForEach(kv1 => items.Add(kv1.Value.Uid,kv1.Value));
            });

            return items;
        }

        public override BaseItem GetItem(ulong uid)
        {
            SoulRingItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public void LoadItems(List<SoulRingInfo> items)
        {
            SoulRingItem soulRing = null;
            items.ForEach(item =>
            {
                soulRing = new SoulRingItem(item);

                //魂环附加属性
                soulRing.CheckAddAdditionalNatures();

                //吸收了的魂环直接放入manager,在统计背包容量的时候不计入
                if (soulRing.EquipHeroId > 0 && soulRing.AbsorbState!=(int)SoulRingAbsorbState.OnAbsort)
                {
                    this.SoulRingManager.AddEquipSoulRing(soulRing);
                }
                else
                {
                    //FIXME: BOIL在吸收状态的魂环需求是放在一个未知空间里。我这里还是把它放到背包里
                    AddItemAndCheckAbsorb(soulRing);
                }
            });
        }

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (itemList.ContainsKey(item.Uid))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void SyncDbItemInfo(BaseItem baseItem)
        {
            SoulRingItem item = baseItem as SoulRingItem;
            if (item != null)
            {
                Manager.DB.Call(new QueryUpdateSoulRing(item.SoulRingInfo));
            }
        }


        public override void UpdateItem(BaseItem baseItem)
        {
            SoulRingItem item = baseItem as SoulRingItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public override void InsertItemToDb(BaseItem baseItem)
        {
            SoulRingItem item = baseItem as SoulRingItem;
            if (item == null)
            {
                return;
            }

            Manager.DB.Call(new QueryInsertSoulRing(item.SoulRingInfo));
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            string tableName = "soul_ring";
            Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
        }

        public List<int> GetSoulRingAbsorbHeroList()
        {
            List<int> heroIds = new List<int>();
            foreach (var item in itemList)
            {
                if (item.Value.AbsorbState == (int)SoulRingAbsorbState.OnAbsort)
                {
                    if (!heroIds.Contains(item.Value.EquipHeroId))
                    {
                        heroIds.Add(item.Value.EquipHeroId);
                    }
                }
            }
            return heroIds;
        }

        public List<SoulRingItem> GetHeroAllSoulRing(int heroId)
        {
            List<SoulRingItem> soulRings = new List<SoulRingItem>();
            foreach (var item in itemList)
            {
                if (item.Value.EquipHeroId == heroId)
                {
                    soulRings.Add(item.Value);
                }
            }
            return soulRings;
        }

        public int GetSoulRingCountByLevel(int level)
        {
            int count = 0;
            foreach (var item in itemList)
            {
                if (item.Value.Level >= level)
                {
                    count++;
                }
            }

            foreach (var kv in SoulRingManager.EquipedSoulRings)
            {
                foreach (var soulRing in kv.Value)
                {
                    if (soulRing.Value.Level >= level)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public ulong GetItemUid(int heroId)
        {
            Dictionary<int, SoulRingItem> items;
            SoulRingManager.EquipedSoulRings.TryGetValue(heroId, out items);
            if (items != null)
            {
                List<int> rings = new List<int>();
                items.ForEach(kv => rings.Add(kv.Value.modelId));
                return itemList.Where(kv => !rings.Contains(kv.Value.modelId)).FirstOrDefault().Key;
            }
            else
            {
                return itemList.FirstOrDefault().Key;
            }
        }

        public SoulRingItem GetOnAbsorbSoulRingByHeroId(int heroId)
        {
            ulong uid;
            if (!onAbsorbFlagList.TryGetValue(heroId, out uid))
            {
                return null;
            }
            SoulRingItem item;
            itemList.TryGetValue(uid, out item);
            return item;
        }


        public int GetEquiptedSoulRingCountByHero(int heroId)
        {
            Dictionary<int, SoulRingItem> items;

            if (SoulRingManager.EquipedSoulRings.TryGetValue(heroId,out items))
            {
                return items.Count;
            }
            return 0;
        }

        public void SyncDbBatchUpdateItemInfo(List<SoulRingInfo> ringInfoList)
        {
            if (ringInfoList.Count > 0)
            {
                Manager.DB.Call(new QueryBatchUpdateSoulRing(ringInfoList));
            }
        }
    }
}

       
    


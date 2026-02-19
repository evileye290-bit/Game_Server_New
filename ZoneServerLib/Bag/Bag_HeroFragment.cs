using CommonUtility;
using EnumerateUtility;
using System.Collections.Generic;
using System;
using ServerModels;
using DBUtility;
using Logger;
using ServerShared;
using DataProperty;

namespace ZoneServerLib
{
    public class Bag_HeroFragment : BaseBag
    {
        private Dictionary<ulong, HeroFragmentItem> itemList;

        public Bag_HeroFragment(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, HeroFragmentItem>();
            //manager.Owner.EquipmentManager.BindBag(this);
        }

        public override void Clear()
        {
            itemList.Clear();
        }

        public override BaseItem GetItem(ulong uid)
        {
            HeroFragmentItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public override BaseItem GetItem(int id)
        {
            return GetItemBySubType(id);
        }

        public HeroFragmentItem GetItemBySubType(int id)
        {
            HeroFragmentItem item = null;

            foreach (var kv in itemList)
            {
                if (id == kv.Value.Id)
                {
                    item = kv.Value;
                    break;
                }
            }

            return item;
        }

        public override Dictionary<ulong, BaseItem> GetAllItems()
        {
            Dictionary<ulong, BaseItem> items = new Dictionary<ulong, BaseItem>();

            itemList.ForEach(kv => items.Add(kv.Key, kv.Value));

            return items;
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            HeroFragmentItem item;
            List<BaseItem> itemList = new List<BaseItem>();

            HeroFragmentModel model = BagLibrary.GetHeroFragmentModel(id);
            if (model == null)
            {
                return itemList;
            }

            item = GetItemBySubType(id);
            if (item == null)
            {
                uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
                HeroFragmentInfo info = new HeroFragmentInfo()
                {
                    OwnerUid = Manager.Owner.Uid,
                    Uid = uid,
                    TypeId = id,
                    PileNum = num,
                };

                item = new HeroFragmentItem(info);
                itemList.Add(item);

                AddItem(item);
                InsertItemToDb(item);
            }
            else
            {
                item.PileNum += num;
                itemList.Add(item);

                UpdateItem(item);
                SyncDbItemInfo(item);
            }

            return itemList;
        }

        public HeroFragmentItem AddItem(HeroFragmentItem item)
        {
            HeroFragmentItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }
            itemList.Add(item.Uid, item);
            return item;
        }

        public override BaseItem DelItem(ulong uid, int num)
        {
            BaseItem item = GetItem(uid);
            if (item == null)
            {
                Log.WarnLine("player {0} del hero Fragment fail :beacause item {1} is not found ", Manager.Owner.Uid, uid);
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

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            HeroFragmentItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            HeroFragmentItem item = baseItem as HeroFragmentItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }


        public void LoadItems(List<HeroFragmentInfo> items)
        {
            foreach (var item in items)
            {
                HeroFragmentItem getItem = GetItemBySubType(item.TypeId);
                if (getItem == null)
                {
                    AddItem(new HeroFragmentItem(item));
                }
                else
                {
                    //重复删除
                    getItem.PileNum += item.PileNum;

                    SyncDbItemInfo(getItem);
                    Manager.DB.Call(new QueryRemoveHeroFragment(item.Uid));
                    Log.WarnLine("player {0} load hero Fragment : del same {1} item uid {2} num {3}", Manager.Owner.Uid, item.TypeId, item.Uid, item.PileNum);
                }
            }
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item != null)
            {
                Manager.DB.Call(new QueryInsertHeroFragment(item.OwnerUid, item.Uid, item.Id, item.PileNum));
            }
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item != null)
            {
                Manager.DB.Call(new QueryRemoveHeroFragment(item.Uid));
            }

        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            HeroFragmentItem it = item as HeroFragmentItem;
            if (it != null)
            {
                Manager.DB.Call(new QueryUpdateHeroFragmentCount(item.Uid, item.PileNum));
            }
        }
    }
}

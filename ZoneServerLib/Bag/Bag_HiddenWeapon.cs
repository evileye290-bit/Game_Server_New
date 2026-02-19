using DBUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class Bag_HiddenWeapon : BaseBag
    {
        private Dictionary<ulong, HiddenWeaponItem> itemList;
        public HiddenWeaponManager HiddenWeaponManager { get; private set; }

        public Bag_HiddenWeapon(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, HiddenWeaponItem>();
            this.HiddenWeaponManager = manager.Owner.HiddenWeaponManager;
            HiddenWeaponManager.BindBag(this);
        }

        public override int ItemsCount()
        {
            return itemList.Where(x => x.Value.Info.EquipHeroId <= 0).Count();
        }

        public override BaseItem GetItem(ulong uid)
        {
            HiddenWeaponItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            HiddenWeaponItem item;
            List<BaseItem> itemList = new List<BaseItem>();

            int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.HiddenWeapon, id, num);
            if (add2Bag > 0)
            {
                //当有多个的时候分别创建
                for (int i = 0; i < add2Bag; ++i)
                {
                    //新添加
                    uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);

                    HiddenWeaponDbInfo info = new HiddenWeaponDbInfo()
                    {
                        OwnerUid = Manager.Owner.Uid,
                        TypeId = id,
                        Uid = uid,
                        PileNum = 1,
                        GenerateTime = 0,
                        EquipHeroId = -1,
                        WashList = new List<int>()
                    };

                    item = new HiddenWeaponItem(info);

                    itemList.Add(item);

                    AddItem(item);
                    InsertItemToDb(item);
                }
            }
            return itemList;
        }

        public HiddenWeaponItem AddItem(HiddenWeaponItem item)
        {
            HiddenWeaponItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }
            itemList.Add(item.Uid, item);
            return item;
        }

        public HiddenWeaponItem AddHiddenWeapon(int id)
        {
            HiddenWeaponModel model = HiddenWeaponLibrary.GetHiddenWeaponModel(id);
            if (model == null)
            {
                return null;
            }
            return BaseAddHiddenWeapon(id);
        }

        private HiddenWeaponItem BaseAddHiddenWeapon(int id)
        {
            ulong itemUid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
            HiddenWeaponDbInfo itemInfo = new HiddenWeaponDbInfo()
            {
                OwnerUid = Manager.Owner.Uid,
                Uid = itemUid,
                TypeId = id,
                PileNum = 1,
                EquipHeroId = -1,
                WashList = new List<int>()
            };

            var item = new HiddenWeaponItem(itemInfo);
            return AddItemWithSpaceCheck(item);
        }

        private HiddenWeaponItem AddItemWithSpaceCheck(HiddenWeaponItem item)
        {
            if (this.Manager.BagFull())
            {
                this.Manager.SendItem2Mail((int)RewardType.HiddenWeapon, item.Id, 1);
                return null;
            }

            HiddenWeaponItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }

            itemList.Add(item.Uid, item);
            InsertItemToDb(item);
            return item;
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

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            HiddenWeaponItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            HiddenWeaponItem item = baseItem as HiddenWeaponItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public void UpdateItemEquipHeroId(HiddenWeaponItem hiddenWeaponItem)
        {
            if (hiddenWeaponItem != null)
            {
                HiddenWeaponItem item = null;
                if (itemList.TryGetValue(hiddenWeaponItem.Uid, out item))
                {
                    item.Info.EquipHeroId = hiddenWeaponItem.Info.EquipHeroId;
                    SyncDbItemInfo(item);
                }
            }
        }

        public override void Clear()
        {
            itemList.Clear();
        }

        public override Dictionary<ulong, BaseItem> GetAllItems()
        {
            Dictionary<ulong, BaseItem> items = new Dictionary<ulong, BaseItem>();

            itemList.ForEach(kv => items.Add(kv.Key, kv.Value));         

            return items;
        }

        public void LoadItems(List<HiddenWeaponDbInfo> items)
        {
            HiddenWeaponItem hiddenWeapon = null;
            items.ForEach(item =>
            {
                hiddenWeapon = new HiddenWeaponItem(item);
                if (hiddenWeapon.Info.EquipHeroId > 0)
                {
                    this.HiddenWeaponManager.SetHeroHiddenWeapon(hiddenWeapon.Info.EquipHeroId, hiddenWeapon.Uid);
                }
                this.itemList.Add(item.Uid, hiddenWeapon);
            });
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            Manager.DB.Call(new QueryInsertHiddenWeapon(item.OwnerUid, item.Uid, item.Id));
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            string tableName = "hidden_weapon";
            Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            HiddenWeaponItem hiddenWeaponItem = item as HiddenWeaponItem;
            if (hiddenWeaponItem == null)
            {
                return;
            }
            Manager.DB.Call(new QueryUpdateHiddenWeaponIndex(hiddenWeaponItem.Info));
        }
    }
}

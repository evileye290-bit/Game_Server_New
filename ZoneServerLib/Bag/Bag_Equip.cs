using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class Bag_Equip : BaseBag
    {
        private Dictionary<ulong, EquipmentItem> itemList;
        public EquipmentManager EquipmentManager { get; private set; }

        public Bag_Equip(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, EquipmentItem>();
            this.EquipmentManager = manager.Owner.EquipmentManager;
            EquipmentManager.BindBag(this);
        }
        public override int ItemsCount()
        {
            return itemList.Where(x => x.Value.EquipInfo.EquipHeroId <= 0).Count();
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            EquipmentItem item;
            List<BaseItem> itemList = new List<BaseItem>();

            int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.Equip, id, num);
            if (add2Bag > 0)
            {
                //当有多个的时候分别创建
                for (int i = 0; i < add2Bag; ++i)
                {
                    //新添加
                    uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);

                    EquipmentInfo info = new EquipmentInfo()
                    {
                        OwnerUid = Manager.Owner.Uid,
                        TypeId = id,
                        Uid = uid,
                        PileNum = 1,
                        GenerateTime = 0,
                        EquipHeroId = -1,
                    };

                    item = new EquipmentItem(info);

                    itemList.Add(item);

                    AddItem(item);
                    InsertItemToDb(item);
                }
            }
            return itemList;
        }

        public EquipmentItem AddItem(EquipmentItem item)
        {
            EquipmentItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }
            itemList.Add(item.Uid, item);
            return item;
        }

        public EquipmentItem AddEquipment(int id, bool checkSpace = true)
        {
            EquipmentModel model = EquipLibrary.GetEquipModel(id);
            if (model == null)
            {
                return null;
            }
            return BaseAddEquipment(id, checkSpace);
        }

        private EquipmentItem BaseAddEquipment(int id, bool checkSpace)
        {
            ulong itemUid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
            EquipmentInfo itemInfo = new EquipmentInfo()
            {
                OwnerUid = Manager.Owner.Uid,
                Uid = itemUid,
                TypeId = id,
                PileNum = 1,
                EquipHeroId = -1
            };

            var item = new EquipmentItem(itemInfo);
            return AddItemWithSpaceCheck(item, checkSpace);
        }

        private EquipmentItem AddItemWithSpaceCheck(EquipmentItem item, bool checkSpace)
        {
            if (checkSpace)
            {
                if (this.Manager.BagFull())
                {
                    this.Manager.SendItem2Mail((int)RewardType.Equip, item.Id, 1);
                    return null;
                }
            }

            EquipmentItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }

            itemList.Add(item.Uid, item);
            InsertItemToDb(item);
            return item;
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

        public void UpdateEquipment(EquipmentItem equipItem)
        {
            if (equipItem != null)
            {
                EquipmentItem item = null;
                if (itemList.TryGetValue(equipItem.Uid, out item))
                {
                    item.EquipInfo.EquipHeroId = equipItem.EquipInfo.EquipHeroId;
                    Manager.DB.Call(new QueryUpdateEquipment(item.EquipInfo));
                }
            }
        }

        public override Dictionary<ulong, BaseItem> GetAllItems()
        {
            Dictionary<ulong, BaseItem> items = new Dictionary<ulong, BaseItem>();

            itemList.ForEach(kv => items.Add(kv.Key, kv.Value));
            //EquipmentManager.EquipedEquipment.ForEach(kv => items.Add(kv.Value.Uid, kv.Value));

            return items;
        }

        public override BaseItem GetItem(ulong uid)
        {
            EquipmentItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public List<EquipmentItem> GetItems(int id)
        {
            return itemList.Values.Where(x => x.Id == id && x.EquipInfo.EquipHeroId <= 0).ToList();
        }

        public void LoadItems(List<EquipmentInfo> items)
        {
            EquipmentItem equip = null;
            items.ForEach(item =>
            {
                equip = new EquipmentItem(item);
                if (equip.EquipInfo.EquipHeroId > 0)
                {
                    this.EquipmentManager.AddEquipment(equip);
                }
                this.itemList.Add(item.Uid, equip);
            });
        }

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            EquipmentItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            EquipmentItem item = baseItem as EquipmentItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            Manager.DB.Call(new QueryInsertEquipment(item.OwnerUid, item.Uid, item.Id));
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            string tableName = "equipment";
            Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            EquipmentItem equipItem = item as EquipmentItem;
            if (equipItem == null)
            {
                return;
            }
            Manager.DB.Call(new QueryUpdateEquipmentIndex(equipItem.Uid, equipItem.EquipInfo.EquipHeroId));
        }

        public void SyncDbBatchUpdateItemInfo(List<EquipmentInfo> equipList)
        {
            if (equipList.Count > 0)
            {
                Manager.DB.Call(new QueryBatchUpdateEquipmentIndex(equipList));
            }
        }
    }
}

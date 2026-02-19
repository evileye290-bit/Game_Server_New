using CommonUtility;
using EnumerateUtility;
using System.Collections.Generic;
using System;
using ServerModels;
using DBUtility;
using Logger;
using ServerShared;
using DataProperty;
using System.Linq;

namespace ZoneServerLib
{
    public class Bag_Normal : BaseBag
    {
        private Dictionary<ulong, NormalItem> itemList;

        public Bag_Normal(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, NormalItem>();
            //manager.Owner.EquipmentManager.BindBag(this);
        }

        public override int ItemsCount()
        {
            return itemList.Where(x=>x.Value.ItemModel.IsVisible).Count();
        }

        public override void Clear()
        {
            itemList.Clear();
        }

        public override BaseItem GetItem(ulong uid)
        {
            NormalItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public override BaseItem GetItem(int id)
        {
            return GetItemBySubType(id);
        }

        public NormalItem GetItemBySubType(int id)
        {
            NormalItem item = null;

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
            NormalItem item;
            List<BaseItem> itemList = new List<BaseItem>();
            int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

            ItemModel model = BagLibrary.GetItemModel(id);
            if(model == null)
            {
                return itemList;
            }
            if (Manager.Owner.CheckHasThisTitleCard(id, model.SubType))
            {
                return itemList;
            }
            if (model.PileMax == 1)
            {
                int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.NormalItem, id, num);
                if (add2Bag > 0)
                {
                    //当有多个的时候分别创建
                    for (int i = 0; i < num; ++i)
                    {
                        //新添加
                        uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
                        ItemInfo info = new ItemInfo()
                        {
                            OwnerUid = Manager.Owner.Uid,
                            Uid = uid,
                            TypeId = id,
                            PileNum = 1,
                            GenerateTime = time,
                        };

                        item = new NormalItem(info);

                        itemList.Add(item);

                        AddItem(item);
                        InsertItemToDb(item);
                    }
                }
            }
            else
            {
                item = GetItemBySubType(id);
                if (item == null)
                {
                    //一个空间都没有了
                    if (this.Manager.GetBagRestSpace() <= 0)
                    {
                        this.Manager.SendItem2Mail((int)RewardType.NormalItem, id, num);
                    }
                    else
                    {
                        uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
                        ItemInfo info = new ItemInfo()
                        {
                            OwnerUid = Manager.Owner.Uid,
                            Uid = uid,
                            TypeId = id,
                            PileNum = num,
                            GenerateTime = time,
                        };

                        item = new NormalItem(info);
                        itemList.Add(item);

                        AddItem(item);
                        InsertItemToDb(item);
                    }
                }
                else
                {
                    item.PileNum += num;
                    itemList.Add(item);

                    UpdateItem(item);
                    SyncDbItemInfo(item);
                }
            }

            return itemList;
        }

        public NormalItem AddItem(NormalItem item)
        {
            NormalItem temp;
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

            NormalItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            NormalItem item = baseItem as NormalItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public override bool CheckPastData(BaseItem baseItem)
        {
            NormalItem item = baseItem as NormalItem;

            if (item == null) return false;

            Data data = item.ItemModel.Data;
            if (data != null)
            {
                Log.Warn("TODO 道具过期逻辑！");
            }

            return false;
        }


        public void LoadItems(List<ServerModels.ItemInfo> items)
        {
            items.ForEach(item => this.itemList.Add(item.Uid, new NormalItem(item)));
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item != null)
            {
                Manager.DB.Call(new QueryInsertItem(item.OwnerUid, item.Uid, item.Id, (int)EquipIndexType.None, item.PileNum, item.GenerateTime));
            }
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item != null)
            {
                string tableName = "items";
                Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
            }

        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            NormalItem it = item as NormalItem;
            if (it != null)
            {
                Manager.DB.Call(new QueryUpdateItemCount(item.Uid, item.PileNum, (int)it.EquipIndexType, item.GenerateTime));
            }
        }
    }
}

using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using System.Collections.Generic;
using ServerShared;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class Bag_ChatFrame:BaseBag
    {
        private Dictionary<ulong, ChatFrameItem> itemList;

        public int CurChatFrameId { get; set; }


        public Bag_ChatFrame(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, ChatFrameItem>();
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            ChatFrameItem item;
            List<BaseItem> itemList = new List<BaseItem>();
            ChatFrameModel model = ChatLibrary.GetChatFrameModel(id);
            int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

            //int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.NormalItem, id, num);
            if (num > 0)
            {
                //当有多个的时候分别创建
                for (int i = 0; i < num; ++i)
                {
                    //新添加
                    uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);

                    ChatFrameInfo info = new ChatFrameInfo()
                    {
                        OwnerUid = Manager.Owner.Uid,
                        Uid = uid,
                        TypeId = id,
                        PileNum = 1,
                        GenerateTime = time,
                        NewObtain = 1,
                    };

                    item = new ChatFrameItem(info, model);

                    itemList.Add(item);

                    AddItem(item);
                    InsertItemToDb(item);

                    NotifyNewBubbleList();
                }
            }

            return itemList;
        }

        public ChatFrameItem AddItem(ChatFrameItem item)
        {
            ChatFrameItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }
            itemList.Add(item.Uid, item);
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

        public override Dictionary<ulong, BaseItem> GetAllItems()
        {
            Dictionary<ulong, BaseItem> items = new Dictionary<ulong, BaseItem>();

            itemList.ForEach(kv => items.Add(kv.Key, kv.Value));

            return items;
        }

        public override BaseItem GetItem(ulong uid)
        {
            ChatFrameItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public override BaseItem GetItem(int id)
        {
            BaseItem item = null;

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

        public void LoadItems(List<ChatFrameInfo> items)
        {
            items.ForEach(item => this.itemList.Add(item.Uid, new ChatFrameItem(item)));         
        }

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            ChatFrameItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            ChatFrameItem item = baseItem as ChatFrameItem;
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
            Manager.DB.Call(new QueryInsertChatFrame(item.OwnerUid, item.Uid, item.Id, 0, item.PileNum, item.GenerateTime, 1));
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            string tableName = "items_chatframe";
            Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            ChatFrameItem it = item as ChatFrameItem;
            if (it == null)
            {
                return;
            }
            Manager.DB.Call(new QueryUpdateChatFrame(item.Uid, it.ActivateState));
        }

        public void UpdateItemObtainInfo(ChatFrameItem item)
        {
            if (item == null)
            {
                return;
            }
            item.UpdateGenerateTime();
            item.NewObtain = 1;
            itemList[item.Uid] = item;

            Manager.DB.Call(new QueryUpdateChatFrameObtainInfo(item.Uid, item.GenerateTime, item.NewObtain));

            NotifyNewBubbleList();
        }

        //红点通知
        public void NotifyNewBubbleList()
        {
            MSG_ZGC_NEW_BUBBLE_LIST msg = new MSG_ZGC_NEW_BUBBLE_LIST();
            foreach (var item in itemList)
            {
                if (item.Value.NewObtain == 1)
                {
                    msg.ItemIdList.Add(item.Value.Id);
                }
            }
            Manager.Owner.Write(msg);
        }

        public void UpdateItemNewObtainState(int itemId)
        {
            ChatFrameItem item = GetItem(itemId) as ChatFrameItem;
            if (item != null && item.NewObtain == 1)
            {
                item.NewObtain = 0;
                Manager.DB.Call(new QueryUpdateChatFrameObtainState(item.Uid, item.NewObtain));
            }
        }
    }
}

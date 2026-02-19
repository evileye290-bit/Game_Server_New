using Logger;
using ServerModels;
using System.Collections.Generic;
using DBUtility;
using EnumerateUtility;
using CommonUtility;
using ServerShared;
using System;

namespace ZoneServerLib
{
    public class Bag_FaceFrame : BaseBag
    {
        private Dictionary<ulong, FaceFrameItem> itemList;

        /// <summary>
        /// 创角初始头像框
        /// </summary>
        private int initFaceFrame = 8001;
        public int InitFaceFrame
        {
            get { return initFaceFrame; }
            set { initFaceFrame = value; }
        }

        /// <summary>
        /// 当前图像框
        /// </summary>
        private int curFaceFrameId = 0;
        public int CurFaceFrameId
        {
            get { return curFaceFrameId; }
            set { curFaceFrameId = value; }
        }

        public Bag_FaceFrame(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, FaceFrameItem>();
        }

        #region 旧逻辑

        public void CheckFaceFrame()
        {
            if (CurFaceFrameId == 0)
            {
                Log.Warn("player {0} got en error faceframe ! CurFaceFrameId is 0 ", Manager.Owner.Uid);
                return;
            }
            else
            {
                //Data data = Item.GetItemData(CurFaceFrameId);
                //if (data != null)
                //{
                //    Item item = GetItem(CurFaceFrameId);
                //    if (item != null)
                //    {
                //        bool isPast = CheckPastData(item);
                //        if (isPast)
                //        {
                //            data = DataListManager.inst.GetData("CharacterConfig", 1);
                //            InitFaceFrame = data.GetInt("FaceFrame");
                //            data = Item.GetItemData(InitFaceFrame);
                //            if (data != null)
                //            {
                //                //先摘除
                //                item.ActivateState = 0;
                //                UpdateItem(item);
                //                //后穿
                //                item = GetItem(InitFaceFrame);//默认形象应该是必存在的，这里就不判断null了。
                //                //需要还原默认头像框 
                //                item.ActivateState = 1;
                //                item.GenerateTime = 0;
                //                item.DurationDay = 0;

                //                CurFaceFrameId = InitFaceFrame;
                //                UpdateItem(item);

                //                //更新到redis
                //                redis.Call(new OperateSetFaceFrame(knapsackManager.Owner.Uid, CurFaceFrameId));
                //            }
                //        }
                //        else
                //        {
                //            CurFaceFrameId = item.Uid;
                //        }
                //    }
                //}
            }
        }

        #endregion

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            FaceFrameItem item;
            List<BaseItem> itemList = new List<BaseItem>();
            FaceFrameModel model = BagLibrary.GetFaceFrameModel(id);
            int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

            int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.NormalItem, id, num);
            if (add2Bag > 0)
            {
                //当有多个的时候分别创建
                for (int i = 0; i < add2Bag; ++i)
                {
                    //新添加
                    uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);

                    FaceFrameInfo info = new FaceFrameInfo()
                    {
                        OwnerUid = Manager.Owner.Uid,
                        Uid = uid,
                        TypeId = id,
                        PileNum = 1,
                        GenerateTime = time,
                    };

                    item = new FaceFrameItem(info, model);

                    itemList.Add(item);

                    AddItem(item);
                    InsertItemToDb(item);
                }
            }

            return itemList;
        }

        public FaceFrameItem AddItem(FaceFrameItem item)
        {
            FaceFrameItem temp;
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
            FaceFrameItem item = null;
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

        public void LoadItems(List<FaceFrameInfo> items)
        {
            items.ForEach(item => this.itemList.Add(item.Uid, new FaceFrameItem(item)));
        }

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            FaceFrameItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            FaceFrameItem item = baseItem as FaceFrameItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item != null)
            {
                Manager.DB.Call(new QueryInsertFaceFrame(item.OwnerUid, item.Uid, item.Id, (int)EquipIndexType.None, item.PileNum, item.GenerateTime));
            }
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item != null)
            {
                string tableName = "items_faceframe";
                Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
            }
        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            FaceFrameItem it = item as FaceFrameItem;
            if (it != null)
            {
                Manager.DB.Call(new QueryUpdateFaceFrameCount(item.Uid, item.PileNum, it.ActivateState, item.GenerateTime));
                return;
            }
        }
    }
}

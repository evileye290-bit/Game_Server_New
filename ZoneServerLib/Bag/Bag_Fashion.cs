using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class Bag_Fashion : BaseBag
    {
        private Dictionary<ulong, FashionItem> itemList;


        /// <summary>
        /// 当前穿戴的时装。key 为 时装部位 sontype， value 为物品id
        /// </summary>
        private Dictionary<int, int> curFashion = new Dictionary<int, int>();
        public Dictionary<int, int> CurFashion
        {
            get { return curFashion; }
        }

        public Dictionary<ulong, string> fasionPastDataLst = new Dictionary<ulong, string>();

        public Bag_Fashion(BagType type, BagManager manager) : base(manager, type)
        {
            itemList = new Dictionary<ulong, FashionItem>();
        }

        public override int ItemsCount()
        {
            return itemList.Count;
        }


        /// <summary>
        /// 检查过期时装 ，如过期卸下操作
        /// </summary>
        public void CheckFashion()
        {
            if (CurFashion == null)
            {
                Log.Warn("player {0} got en error ! curfashion is null ", Manager.Owner.Uid);
                return;
            }

            Dictionary<int, int> curTempFashion = new Dictionary<int, int>();

            foreach (var itemId in CurFashion)
            {
                FashionModel fashionModel = BagLibrary.GetFashionModel(itemId.Value);
                if (fashionModel != null)
                {
                    int sonType = fashionModel.SonType;

                    FashionItem item = GetItem(itemId.Value) as FashionItem;

                    if (item != null)
                    {
                        bool isPast = CheckPastData(item);
                        if (isPast)
                        {
                            bool isEquip = false;

                            //string tableName = "character";

                            //先摘除
                            item.ActivateState = 0;
                            UpdateItem(item);

                            //后穿
                            switch ((FashionType)item.Id)
                            {
                                case FashionType.Weapon:
                                    {
                                        //后穿
                                        //需要还原默认武器 默认形象应该是永久的。
                                        //item = GetItem(InitWeaponModel) as FashionItem; //默认形象应该是必存在的，这里就不判断null了。
                                        isEquip = true;
                                    }
                                    break;
                                case FashionType.Head:
                                    {
                                        //后穿
                                        //需要还原默认头
                                        //item = GetItem(InitHeadModel) as FashionItem; //默认形象应该是必存在的，这里就不判断null了。
                                        isEquip = true;
                                    }
                                    break;
                                case FashionType.Clothes:
                                    {
                                        //后穿
                                        //需要还原默认身体 
                                        //item = GetItem(InitBodyModel) as FashionItem; //默认形象应该是必存在的，这里就不判断null了。
                                        isEquip = true;
                                    }
                                    break;
                                case FashionType.Face:
                                    {
                                        isEquip = false;
                                        //item = GetItem(InitFaceModel) as FashionItem; //默认形象应该是必存在的，这里就不判断null了。
                                    }
                                    break;
                                case FashionType.Back:
                                    {
                                        isEquip = false;
                                        //item = GetItem(InitBackModel) as FashionItem; //默认形象应该是必存在的，这里就不判断null了。
                                    }
                                    break;
                                default:
                                    break;
                            }

                            item.ActivateState = 1;
                            item.GenerateTime = 0;
                            item.DurationDay = 0;

                            curTempFashion[sonType] = item.Id;
                            UpdateItem(item);
                            isEquip = true;

                            Manager.DB.Call(new QuerySetFashion(Manager.Owner.Uid, item.Id, isEquip));
                            ////更新到redis
                            //Manager.Redis.Call(new OperateSetFashion(Manager.Owner.Uid, item.Id, isEquip));
                        }
                        else
                        {
                            curTempFashion[sonType] = item.Id;
                        }
                    }
                }
            }
            curFashion = curTempFashion;
        }

        public MODEL_INFO GetModel()
        {
            MODEL_INFO model = new MODEL_INFO();
            int i;
            if (CurFashion.TryGetValue((int)FashionType.Head, out i))
            {
                model.Head = i;
            }
            if (CurFashion.TryGetValue((int)FashionType.Clothes, out i))
            {
                model.Model = i;
            }
            model.FashionIds.AddRange(CurFashion.Values);
            return model;
        }


        public bool IsInitFashion(FashionItem item)
        {
            if (item == null) return false;

            //switch ((FashionType)item.SonType)
            //{
            //    case FashionType.Head:
            //        if (item.Id == InitHeadModel)
            //        {
            //            return true;
            //        }
            //        break;
            //    case FashionType.Weapon:
            //        if (item.Id == InitWeaponModel)
            //        {
            //            return true;
            //        }
            //        break;
            //    case FashionType.Clothes:
            //        if (item.Id == InitBodyModel)
            //        {
            //            return true;
            //        }
            //        break;
            //    case FashionType.Face:
            //        if (item.Id == InitFaceModel)
            //        {
            //            return true;
            //        }
            //        break;
            //    case FashionType.Back:
            //        if (item.Id == InitBackModel)
            //        {
            //            return true;
            //        }
            //        break;
            //    case FashionType.Other:
            //    default:
            //        break;

            //}
            return false;
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            ulong uid;
            FashionItem item;
            List<BaseItem> itemList = new List<BaseItem>();
            FashionModel model = BagLibrary.GetFashionModel(id);
            int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

            int add2Bag = this.Add2MailAndReturnAddBagNum(RewardType.NormalItem, id, num);
            if (add2Bag > 0)
            {

                //当有多个的时候分别创建
                for (int i = 0; i < num; ++i)
                {
                    //新添加
                    uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);

                    FashionInfo info = new FashionInfo()
                    {
                        OwnerUid = Manager.Owner.Uid,
                        Uid = uid,
                        TypeId = id,
                        PileNum = 1,
                        GenerateTime = time,
                    };

                    item = new FashionItem(model, info);

                    itemList.Add(item);

                    AddItem(item);
                    InsertItemToDb(item);
                }
            }

            return itemList;
        }

        public FashionItem AddItem(FashionItem item)
        {
            FashionItem temp;
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
            FashionItem item = null;
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


        public void LoadItems(List<FashionInfo> items)
        {
            items.ForEach(item => this.itemList.Add(item.Uid, new FashionItem(item)));
        }

        public override bool RemoveItem(BaseItem item)
        {
            if (item == null)
            {
                return false;
            }

            FashionItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                itemList.Remove(item.Uid);
            }
            return true;
        }

        public override void UpdateItem(BaseItem baseItem)
        {
            FashionItem item = baseItem as FashionItem;
            if (item == null)
            {
                return;
            }
            itemList[item.Uid] = item;

            SyncDbItemInfo(item);
        }

        public override void DisposePastData()
        {
            List<BaseItem> removeLst = new List<BaseItem>();

            int fashionRemoveCount = 0;
            foreach (var it in itemList.Values)
            {
                var data = it.FashionModel.Data;
                if (data != null)
                {
                    bool isPast = CheckPastData(it);
                    if (isPast)
                    {
                        fasionPastDataLst.Add(it.Uid, it.PastDateTime);
                        fashionRemoveCount++;
                    }
                }
            }

            //if (fashionRemoveCount > 0)
            //{
            //    Manager.Redis.Call(new OperateDecrementCharFashionCount(Manager.Owner.Uid, fashionRemoveCount));
            //}

            foreach (var rm in removeLst)
            {
                Log.Warn("player {0} del pastdata item {1} count {2}", Manager.Owner.Uid, rm.Uid, rm.PileNum);
                DelItem(rm.Uid, rm.PileNum); //删除
            }
        }

        public override bool CheckPastData(BaseItem baseItem)
        {
            //TODO 需要实现fashion过期逻辑
            return false;
        }

        public override void InsertItemToDb(BaseItem item)
        {
            if (item != null)
            {
                Manager.DB.Call(new QueryInsertFashion(item.OwnerUid, item.Uid, item.Id, 0, item.PileNum, item.GenerateTime));
            }
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item != null)
            {
                string tableName = "items_fashion";
                Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
            }
        }

        public override void SyncDbItemInfo(BaseItem item)
        {
            FashionItem it = item as FashionItem;
            if (it != null)
            {
                Manager.DB.Call(new QueryUpdateFashionCount(item.Uid, item.PileNum, it.ActivateState, item.GenerateTime));
            }
        }
    }
}

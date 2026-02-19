using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class BagManager
    {
        public Dictionary<BagType, BaseBag> bagList;

        public DBManagerPool DB { get; private set; }
        public RedisOperatePool Redis { get; private set; }
        public PlayerChar Owner { get; private set; }

        private int curOpenChestId = 0;
        public int CurOpenChestId
        {
            get { return this.curOpenChestId; }
            set { this.curOpenChestId = value; }
        }

        //背包使用了的容量
        public int BagUsedSpace
        {
            get
            {
                int usedSpace = 0;
                foreach (var kv in bagList)
                {
                    usedSpace += kv.Value.ItemsCount();
                }
                return usedSpace - Owner.EquipmentManager.GetUsedOutXuanyuCount();
            }
        }

        public Bag_Normal NormalBag
        {
            get { return bagList[BagType.Normal] as Bag_Normal; }
        }

        public Bag_ChatFrame ChatFrameBag
        {
            get { return bagList[BagType.ChatFrame] as Bag_ChatFrame; }
        }

        public Bag_FaceFrame FaceFrameBag
        {
            get { return bagList[BagType.FaceFrame] as Bag_FaceFrame; }
        }

        public Bag_Fashion FashionBag
        {
            get { return bagList[BagType.Fashion] as Bag_Fashion; }
        }

        public Bag_SoulBone SoulBoneBag
        {
            get { return bagList[BagType.SoulBone] as Bag_SoulBone; }
        }

        public Bag_Equip EquipBag
        {
            get { return bagList[BagType.Equip] as Bag_Equip; }
        }

        public Bag_SoulRing SoulRingBag
        {
            get { return bagList[BagType.SoulRing] as Bag_SoulRing; }
        }

        public Bag_HeroFragment HeroFragmentBag
        {
            get { return bagList[BagType.HeroFragment] as Bag_HeroFragment; }
        }

        public Bag_HiddenWeapon HiddenWeaponBag
        {
            get { return bagList[BagType.HiddenWeapon] as Bag_HiddenWeapon; }
        }

        public BagManager(DBManagerPool db, RedisOperatePool redis, PlayerChar pc)
        {
            this.DB = db;
            this.Redis = redis;
            Owner = pc;
            bagList = new Dictionary<BagType, BaseBag>();
        }

        public void Init()
        {
            foreach (BagType key in Enum.GetValues(typeof(BagType)))
            {
                BaseBag knapsack = BagFactory.CreatateBag(key, this);
                bagList.Add(key, knapsack);
            }
        }

        public void ClearBags(BagType type)
        {
            BaseBag knapsack = null;
            if (bagList.TryGetValue(type, out knapsack))
            {
                knapsack.Clear();
            }
        }

        public void Clear()
        {
            bagList.Clear();
        }


        /// <summary>
        /// 检查物品使用期限
        /// </summary>
        public void CheckPastData()
        {
            FashionBag.CheckFashion();
            FaceFrameBag.CheckFaceFrame();
            DisposePastData();
        }

        /// <summary>
        /// 过期物品具体处理 
        /// </summary>
        public void DisposePastData()
        {
            bagList.Values.ForEach(bag =>
            {
                bag.DisposePastData();
            });
        }

        public BaseBag GetBag(MainType type)
        {
            BaseBag bag = null;
            BagType ksType = GetBagType(type);
            bagList.TryGetValue(ksType, out bag);
            return bag;
        }

        public BaseItem GetItem(ulong uid)
        {
            BaseItem item = NormalBag.GetItem(uid);
            if (item != null) return item;

            item = ChatFrameBag.GetItem(uid);
            if (item != null) return item;

            item = FaceFrameBag.GetItem(uid);
            if (item != null) return item;

            item = FashionBag.GetItem(uid);
            return item;
        }

        public BaseItem GetItem(MainType mainType, int id)
        {
            BaseBag bag = GetBag(mainType);
            return bag.GetItem(id);
        }

        public BaseItem GetItem(MainType mainType, ulong uid)
        {
            BaseBag bag = GetBag(mainType);
            return bag.GetItem(uid);
        }

        public List<BaseItem> AddItem(MainType mainType, int Id, int count)
        {
            BaseBag bag = GetBag(mainType);
            List<BaseItem> itemList = bag.AddItem(Id, count);
            return itemList;
        }

        public BaseItem DelItem(BaseItem item, int count)
        {
            BaseBag bag = GetBag(item.MainType);
            BaseItem reItem = bag.DelItem(item.Uid, count);
            return reItem;
        }

        public BaseItem UpdateItem(BaseItem item)
        {
            BaseBag bag = GetBag(item.MainType);
            bag.UpdateItem(item);
            return item;
        }

        /// <summary>
        /// 检查背包空间是否足够(空间满了 保存道具到邮箱)
        /// </summary>
        /// <param name="mainType">道具MainType</param>
        /// <param name="id">道具id</param>
        /// <param name="num">道具数量</param>
        /// <param name="freeSpace">本次释放的空间</param>
        /// <returns></returns>
        public bool CheckBagSpace(MainType mainType, int id, int num, int freeSpace = 0)
        {
            //剩余空间
            int restSpace = GetBagRestSpace();
            int needSpace = GetNeedBagSpace(mainType, id, num);

            return restSpace + freeSpace >= needSpace;
        }

        public int GetBagRestSpace()
        {
            return Owner.BagSpace - this.BagUsedSpace;
        }

        public bool BagFull()
        {
            return Owner.BagSpace <= this.BagUsedSpace;
        }

        //获得道具所需要的背包空间
        public int GetNeedBagSpace(MainType mainType, int id, int num)
        {
            int needSpace = 0;
            switch (mainType)
            {
                case MainType.Consumable:
                    {
                        var model = BagLibrary.GetItemModel(id);
                        if (model == null || model.PileMax == 1)
                        {
                            needSpace = num;
                        }
                        else
                        {
                            needSpace = 1;//可以叠加
                        }
                    }
                    break;
                case MainType.Material:
                    needSpace = 1;
                    break;
                default:
                    needSpace = num;
                    break;
            }

            return needSpace;
        }

        //将道具添加到邮件
        public void SendItem2Mail(int rewardType, int id, int num, params object[] attrs)
        {
            string[] objects = null;
            if (attrs != null)
            {
                objects = new string[attrs.GetLength(0)];
            }
            for (int i = 0; i < attrs.Length; i++)
            {
                objects[i] = attrs[i].ToString();
            }
            var basicInfo = new ItemBasicInfo(rewardType, id, num, objects);
            string rewards = basicInfo.ToString();

            Owner.SendPersonEmail(BagLibrary.BagFullEmailId, reward: rewards);
        }

        public List<MSG_ZGC_BAG_SYNC> GetBagSyncMsg()
        {
            int bagUseSpace = this.BagUsedSpace;
            List<MSG_ZGC_BAG_SYNC> syncList = new List<MSG_ZGC_BAG_SYNC>();
            MSG_ZGC_BAG_SYNC syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
            int addCount = 0;

            foreach (var bag in bagList)
            {
                Dictionary<ulong, BaseItem> items = bag.Value.GetAllItems();

                switch (bag.Key)
                {
                    case BagType.SoulBone:
                        {
                            foreach (var kv in items)
                            {
                                if (++addCount > 50)
                                {
                                    syncList.Add(syncMsg);
                                    syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                    addCount = 0;
                                }

                                SoulBoneItem item = kv.Value as SoulBoneItem;
                                syncMsg.SoulBones.Add(item.GenerateMsg());
                            }
                        }
                        break;
                    case BagType.Normal:
                        {

                            items.ForEach(it =>
                            {
                                NormalItem item = it.Value as NormalItem;

                                if (++addCount > 50)
                                {
                                    syncList.Add(syncMsg);
                                    syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                    addCount = 0;
                                }

                                if (item.MainType == MainType.Material && item.SubType == 2)//钻石被使用
                                {
                                    syncMsg.Items.Add(EquipBag.EquipmentManager.InteceptJewel(item.Uid, item.GenerateSyncMessage()));
                                }
                                else
                                {
                                    syncMsg.Items.Add(item.GenerateSyncMessage());
                                }
                            });
                        }
                        break;
                    case BagType.HeroFragment:
                        {
                            items.ForEach(it =>
                            {
                                HeroFragmentItem item = it.Value as HeroFragmentItem;

                                if (++addCount > 50)
                                {
                                    syncList.Add(syncMsg);
                                    syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                    addCount = 0;
                                }
                                syncMsg.HeroFragments.Add(item.GenerateSyncMessage());
                            });
                        }
                        break;
                    case BagType.SoulRing:
                        items.ForEach(it =>
                        {
                            if (++addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                addCount = 0;
                            }
                            SoulRingItem item = it.Value as SoulRingItem;
                            syncMsg.SoulRings.Add(item.GenerateSyncMessage());
                        });
                        break;
                    case BagType.Equip:
                        items.ForEach(it =>
                        {
                            addCount += 3;
                            if (addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                addCount = 0;
                            }
                            EquipmentItem item = it.Value as EquipmentItem;
                            syncMsg.Equips.Add(EquipBag.EquipmentManager.InteceptEquipment(item.GenerateSyncMessage()));
                        });
                        break;
                    case BagType.Fashion:
                        break;
                    case BagType.FaceFrame:
                        items.ForEach(it =>
                        {
                            if (++addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                addCount = 0;
                            }
                            FaceFrameItem item = it.Value as FaceFrameItem;
                            syncMsg.FaceFrames.Add(item.GenerateSyncMessage());
                        });
                        break;
                    case BagType.ChatFrame:
                        items.ForEach(it =>
                        {
                            if (++addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                addCount = 0;
                            }
                            ChatFrameItem item = it.Value as ChatFrameItem;
                            syncMsg.ChatFrames.Add(item.GenerateSyncMessage());
                        });
                        break;
                    case BagType.HiddenWeapon:
                        items.ForEach(it =>
                        {
                            addCount += 3;
                            if (addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZGC_BAG_SYNC() { BagSpace = Owner.BagSpace, UseBagSpace = bagUseSpace };
                                addCount = 0;
                            }
                            HiddenWeaponItem item = it.Value as HiddenWeaponItem;
                            syncMsg.Equips.Add(HiddenWeaponBag.HiddenWeaponManager.GetFinalHiddenWeaponItemInfo(item, item.GenerateSyncMessage()));
                        });
                        break;
                    default:
                        {
                            Log.Warn($"bag {bag.Key.ToString()} have not sync mathord please check it !");
                        }
                        break;
                }
            }

            syncMsg.IsEnd = true;
            syncList.Add(syncMsg);
            return syncList;
        }


        readonly int BAGLISTMAXCOUNT = 200;

        public List<MSG_ZMZ_BAG_INFO> GetBagTransform()
        {
            List<MSG_ZMZ_BAG_INFO> syncList = new List<MSG_ZMZ_BAG_INFO>();
            MSG_ZMZ_BAG_INFO syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
            int addCount = 0;

            foreach (var bag in bagList)
            {
                Dictionary<ulong, BaseItem> items = bag.Value.GetAllItems();

                switch (bag.Key)
                {
                    case BagType.Normal:
                        {
                            items.ForEach(it =>
                            {
                                if (++addCount > BAGLISTMAXCOUNT)
                                {
                                    syncList.Add(syncMsg);
                                    syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                    addCount = 0;
                                }

                                NormalItem item = it.Value as NormalItem;
                                syncMsg.Items.Add(item.GenerateTransformMessage());
                            });
                        }
                        break;
                    case BagType.SoulBone:
                        {
                            foreach (var kv in items)
                            {
                                if (++addCount > BAGLISTMAXCOUNT)
                                {
                                    syncList.Add(syncMsg);
                                    syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                    addCount = 0;
                                }

                                SoulBoneItem item = kv.Value as SoulBoneItem;
                                syncMsg.SoulBones.Add(item.GenerateTransformMsg());
                            }
                        }
                        break;
                    case BagType.SoulRing:
                        items.ForEach(it =>
                        {
                            if (++addCount > BAGLISTMAXCOUNT)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            SoulRingItem item = it.Value as SoulRingItem;
                            syncMsg.SoulLinks.Add(item.GenerateTransformMessage());
                        });
                        break;
                    case BagType.Equip:
                        items.ForEach(it =>
                        {
                            if (++addCount > BAGLISTMAXCOUNT)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            EquipmentItem item = it.Value as EquipmentItem;
                            syncMsg.Equips.Add(item.GenerateTransformMessage());
                        });
                        break;
                    case BagType.Fashion:
                        break;
                    case BagType.FaceFrame:
                        items.ForEach(it =>
                        {
                            if (++addCount > BAGLISTMAXCOUNT)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            FaceFrameItem item = it.Value as FaceFrameItem;
                            syncMsg.FaceFrames.Add(item.GenerateTransformMessage());
                        });
                        break;
                    case BagType.ChatFrame:
                        items.ForEach(it =>
                        {
                            if (++addCount > BAGLISTMAXCOUNT)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            ChatFrameItem item = it.Value as ChatFrameItem;
                            syncMsg.ChatFrames.Add(item.GenerateTransformMessage());
                        });
                        break;
                    case BagType.HeroFragment:
                        items.ForEach(it =>
                        {
                            HeroFragmentItem item = it.Value as HeroFragmentItem;

                            if (++addCount > 50)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            syncMsg.HeroFragment.Add(item.GenerateTransformMessage());
                        });
                        break;
                    case BagType.HiddenWeapon:
                        items.ForEach(it =>
                        {
                            if (++addCount > BAGLISTMAXCOUNT)
                            {
                                syncList.Add(syncMsg);
                                syncMsg = new MSG_ZMZ_BAG_INFO() { Uid = Owner.Uid };
                                addCount = 0;
                            }
                            HiddenWeaponItem item = it.Value as HiddenWeaponItem;
                            syncMsg.HiddenWeapons.Add(item.GenerateTransformMessage());
                        });
                        break;
                    default:
                        {
                            Log.Warn($"bag {bag.Key.ToString()} have not sync mathord please check it !");
                        }
                        break;
                }
            }

            if (addCount > 0)
            {
                syncList.Add(syncMsg);
            }

            return syncList;
        }

        public void LoadTransform(MSG_ZMZ_BAG_INFO bagInfo)
        {
            //物品
            List<ItemInfo> normalItems = new List<ItemInfo>();
            foreach (var item in bagInfo.Items)
            {
                ItemInfo intfo = new ItemInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    PileNum = item.PileNum,
                    GenerateTime = item.GenerateTime,
                };
                normalItems.Add(intfo);
            }
            NormalBag.LoadItems(normalItems);

            //头像
            List<FaceFrameInfo> faceFrames = new List<FaceFrameInfo>();
            foreach (var item in bagInfo.FaceFrames)
            {
                FaceFrameInfo intfo = new FaceFrameInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    PileNum = item.PileNum,
                    GenerateTime = item.GenerateTime,
                    ActivateState = item.ActivateState,
                };
                faceFrames.Add(intfo);
            }
            FaceFrameBag.LoadItems(faceFrames);

            //聊天
            List<ChatFrameInfo> chatFrames = new List<ChatFrameInfo>();
            foreach (var item in bagInfo.ChatFrames)
            {
                ChatFrameInfo intfo = new ChatFrameInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    PileNum = item.PileNum,
                    GenerateTime = item.GenerateTime,
                    ActivateState = item.ActivateState,
                    NewObtain = item.NewObtain,
                };
                chatFrames.Add(intfo);
            }
            ChatFrameBag.LoadItems(chatFrames);
            Owner.CheckCurChatFrame(false);

            //魂环
            List<SoulRingInfo> soulRings = new List<SoulRingInfo>();
            foreach (var item in bagInfo.SoulLinks)
            {
                SoulRingInfo intfo = new SoulRingInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    Year = item.Year,
                    Level = item.Level,
                    PileNum = 1,
                    EquipHeroId = item.EquipedHeroId,
                    AbsorbState = item.AbsorbState,
                    Position = item.Position,
                    Element =  item.Element
                };
                soulRings.Add(intfo);
            }
            SoulRingBag.LoadItems(soulRings);

            //装备
            List<EquipmentInfo> equips = new List<EquipmentInfo>();
            foreach (var item in bagInfo.Equips)
            {
                EquipmentInfo intfo = new EquipmentInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    //Level = item.Level,
                    //BreakLevel = item.BreakLevel,
                    PileNum = 1,
                    EquipHeroId = item.EquipHeroId
                };
                equips.Add(intfo);
            }
            EquipBag.LoadItems(equips);

            //魂骨
            List<SoulBone> soulBones = new List<SoulBone>();
            foreach (var item in bagInfo.SoulBones)
            {
                SoulBone intfo = new SoulBone()
                {
                    Uid = item.Uid,
                    TypeId = item.Id,
                    EquipedHeroId = item.EquipedHeroId,
                    AnimalType = item.AnimalType,
                    PartType = item.PartType,
                    Quality = item.Quality,
                    Prefix = item.Prefix,
                    MainNatureValue = item.MainNatureValue,
                    MainNatureType = item.MainNatureType,
                    AdditionType1 = item.AdditionType1,
                    AdditionValue1 = item.AdditionValue1,
                    AdditionType2 = item.AdditionType2,
                    AdditionValue2 = item.AdditionValue2,
                    AdditionType3 = item.AdditionType3,
                    AdditionValue3 = item.AdditionValue3,
                    SpecId1 =  item.SpecId1,
                    SpecId2 =  item.SpecId2,
                    SpecId3 =  item.SpecId3,
                    SpecId4 =  item.SpecId4,
                };
                soulBones.Add(intfo);
            }
            SoulBoneBag.LoadItems(soulBones);

            //碎片
            List<HeroFragmentInfo> heroFragments = new List<HeroFragmentInfo>();
            bagInfo.HeroFragment.ForEach(x =>
            {
                heroFragments.Add(new HeroFragmentInfo()
                {
                    Uid = x.Uid,
                    TypeId = x.Id,
                    OwnerUid = Owner.Uid,
                    PileNum = x.PileNum,
                    GenerateTime = x.GenerateTime,
                });
            });

            HeroFragmentBag.LoadItems(heroFragments);

            //暗器
            List<HiddenWeaponDbInfo> hiddenWeapons = new List<HiddenWeaponDbInfo>();
            foreach (var item in bagInfo.HiddenWeapons)
            {
                HiddenWeaponDbInfo info = new HiddenWeaponDbInfo()
                {
                    OwnerUid = Owner.Uid,
                    Uid = item.Uid,
                    TypeId = item.Id,
                    Level = item.Level,
                    PileNum = 1,
                    EquipHeroId = item.EquipHeroId,
                    Star = item.Star,
                    NeedStar = item.NeedStar
                };
                info.WashList = new List<int>(item.WashList);
                hiddenWeapons.Add(info);
            }
            HiddenWeaponBag.LoadItems(hiddenWeapons);
        }

        private BagType GetBagType(MainType mainType)
        {
            BagType type = BagType.Normal;
            switch (mainType)
            {
                case MainType.Consumable:
                case MainType.Material:
                    type = BagType.Normal;
                    break;
                case MainType.SoulRing:
                    type = BagType.SoulRing;
                    break;
                case MainType.SoulBone:
                    type = BagType.SoulBone;
                    break;
                case MainType.Equip:
                    type = BagType.Equip;
                    break;
                case MainType.Fashion:
                    type = BagType.Fashion;
                    break;
                case MainType.FaceFrame:
                    type = BagType.FaceFrame;
                    break;
                case MainType.ChatFrame:
                    type = BagType.ChatFrame;
                    break;
                case MainType.HeroFragment:
                    type = BagType.HeroFragment;
                    break;
                case MainType.HiddenWeapon:
                    type = BagType.HiddenWeapon;
                    break;
                default:
                    break;
            }
            return type;
        }
    }
}

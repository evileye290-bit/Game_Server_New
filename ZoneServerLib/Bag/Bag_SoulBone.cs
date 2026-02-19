using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class Bag_SoulBone : BaseBag
    {
        private Dictionary<ulong, SoulBoneItem> itemList;

        public SoulBoneManager BoneManager { get; private set; }
        public Bag_SoulBone(BagManager manager, BagType type) : base(manager, type)
        {
            itemList = new Dictionary<ulong, SoulBoneItem>();
        }

        public override int ItemsCount()
        {
            return itemList.Where(x => x.Value.Bone.EquipedHeroId <= 0).Count();
        }

        public void BindBoneManager(SoulBoneManager mng)
        {
            this.BoneManager = mng;
            BoneManager.BindBag(this);

        }

        #region 带容量检查的方法组
        public SoulBoneItem AddItem(SoulBoneItem item)
        {
            if (!CheckCapacity())
            {
                return null;
            }
            return AddSoulBone(item);
        }
        public bool CheckCapacity()
        {
            //TODO:需要使用manager帮忙计算的容量在上层拦截，因为equip随时会改变
            if (itemList.Count-BoneManager.GetEquipedCount()>300)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public SoulBoneItem AddSoulBone(SoulBoneItem item)
        {
            if (item == null)
            {
                return null;
            }

            SoulBoneItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }
            itemList.Add(item.Uid, item);
            InsertItemToDb(item);
            return item;
        }

        public SoulBoneItem AddSoulBone(ItemBasicInfo itemBasicInfo, bool fixdb)
        {
            List<int> attrList = itemBasicInfo.Attrs.ConvertAll(x => int.Parse(x));

            SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(attrList[0], attrList[1], attrList[2]);

            int main = attrList[2];

            //生成info
            SoulBoneInfo info = new SoulBoneInfo();
            info.AnimalType = attrList[0];
            info.PartType = attrList[1];
            info.MainNature.Quality = itemInfo.Quality;
            info.MainNature.NatureType = itemInfo.MainNatureType;
            info.MainNature.Value = main;

            attrList.RemoveAt(0);
            attrList.RemoveAt(0);
            attrList.RemoveAt(0);
            info.SpecSkills.AddRange(attrList);

            string mvAAA = main.ToString();
            mvAAA += "|" + itemInfo.AddAttrType1 + "|" + itemInfo.AddAttrType2 + "|" + itemInfo.AddAttrType3;
            string adds = ScriptManager.SoulBone.GetAddAttr(mvAAA);
            SoulBoneLibrary.ProduceAdds(info, adds);
            SoulBoneLibrary.ProducePrefix(info, itemInfo.Id, attrList.Count <= ItemBasicInfo.SoulBoneFixAttrCount, fixdb);

            SoulBone bone = new SoulBone(info);

            return BaseAddSoulBone(itemInfo.Id, bone);
        }

        private SoulBoneItem BaseAddSoulBone(int id, SoulBone bone)
        {
            var item = new SoulBoneItem();
            if (!bone.SelfCheck())
            {
                return null;
            }
            ulong uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
            bone.OwnerId = Manager.Owner.Uid;
            bone.Uid = uid;
            //拿到typeId根据soulBoneItems表
            bone.TypeId = id;
            if (bone.TypeId <= 0)
            {
                return null;
            }
            //item值会自动配置
            item.Bone = bone;
            item.Id = bone.TypeId;
            return AddItemWithSpaceCheck(item);
            
        }

        private SoulBoneItem AddItemWithSpaceCheck(SoulBoneItem item)
        {
            if (this.Manager.BagFull())
            {
                SoulBone bone = item.Bone;
                this.Manager.SendItem2Mail((int)RewardType.SoulBone, item.Id, 1, bone.AnimalType,bone.PartType,
                    bone.MainNatureValue,item.Bone.SpecId1, item.Bone.SpecId2, item.Bone.SpecId3, item.Bone.SpecId4);
                return null;
            }

            SoulBoneItem temp;
            if (itemList.TryGetValue(item.Uid, out temp))
            {
                return null;
            }

            itemList.Add(item.Uid, item);
            InsertItemToDb(item);
            return item;
        }

        public override List<BaseItem> AddItem(int id, int num)
        {
            return null;
        }

        public override void Clear()
        {
            itemList.Clear();
        }

        public override void DeleteItemFromDb(BaseItem item)
        {
            if (item == null)
            {
                return;
            }
            string tableName = "soul_bone";
            Manager.DB.Call(new QueryRemoveItem(tableName, item.Uid));
        }

        public override BaseItem DelItem(ulong uid, int num)
        {
            if (num != 1)
            {
                return null;
            }
            return DelItem(uid);
        }

        //删除唯一方法
        public SoulBoneItem DelItem(ulong uid)
        {
            SoulBoneItem item = GetItem(uid) as SoulBoneItem;
            if (item != null)
            {
                RemoveItem(item);
                DeleteItemFromDb(item);
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
            SoulBoneItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public SoulBoneItem GetSoulBoneItem(ulong uid)
        {
            SoulBoneItem item = null;
            itemList.TryGetValue(uid, out item);
            return item;
        }

        public override void InsertItemToDb(BaseItem baseItem)
        {
            SoulBoneItem item = baseItem as SoulBoneItem;
            if (item != null)
            {
                Manager.DB.Call(new QueryInsertSoulBone(item.Bone));
            }
        }

        public void UpdateSoulBone(SoulBone bone)
        {
            if (bone != null)
            {
                Manager.DB.Call(new QueryUpdateSoulBoneIndex(bone));
            }
        }

        public SoulBoneItem NewItem(int ownerId,SoulBone bone)
        {
            SoulBoneItem item = new SoulBoneItem();
            if (!bone.SelfCheck())
            {
                return null;
            }
            ulong uid = Manager.Owner.server.UID.NewIuid(Manager.Owner.server.MainId, Manager.Owner.server.SubId);
            bone.OwnerId = ownerId;
            bone.Uid = uid;
            //拿到typeId根据soulBoneItems表
            bone.TypeId=SoulBoneLibrary.GetTypeId(bone);
            if (bone.TypeId <= 0)
            {
                return null;
            }
            //item值会自动配置
            item.Bone = bone;
            return item;
        }

        public void LoadItems(List<SoulBone> items, bool checkRepair = false)
        {
            List<SoulBone> suitBones = new List<SoulBone>();
            foreach(var item in items)
            {
                SoulBoneItem sItem = new SoulBoneItem();
                sItem.Bone = item;
                if (sItem.Uid != 0)
                {
                    itemList.Add(sItem.Uid, sItem);
                }
                if (item.EquipedHeroId > -1)
                {
                    suitBones.Add(item);
                }
            }
            if (BoneManager != null)
            {
                BoneManager.InitSuit(suitBones);
            }
            else
            {
                Log.Alert(" load soulBone to manager failed without BoneManager");
            }


            //if (checkRepair && Manager.Owner.server.Now()< fixBeforDate)
            //{
            //    RepairSoulBoneSpec(items);
            //}
        }

        ////小于该时间之前的需要修复
        //private static DateTime fixBeforDate = new DateTime(2022, 1, 1);

        //private void RepairSoulBoneSpec(List<SoulBone> items)
        //{
        //    List<SoulBone> syncList = new List<SoulBone>();
        //    foreach (var soulBone in items)
        //    {
        //        if (SoulBoneLibrary.ProducePrefix(soulBone, true))
        //        {
        //            if (soulBone.SpecId1 > 0 || soulBone.SpecId2 > 0 || soulBone.SpecId3 > 0 || soulBone.SpecId4 > 0)
        //            {
        //                syncList.Add(soulBone);
        //            }
        //        }
        //    }

        //    if (syncList.Count > 0)
        //    {
        //        syncList.ForEach(x=> SyncDbItemSpecInfo(x));
        //    }
        //}

        public override bool RemoveItem(BaseItem baseItem)
        {
            if (baseItem == null)
            {
                return false;
            }
            SoulBoneItem temp = null;
            if(itemList.TryGetValue(baseItem.Uid,out temp))
            {
                itemList.Remove(baseItem.Uid);
            }
            return true;
        }

        public override void SyncDbItemInfo(BaseItem baseItem)
        {
            SoulBoneItem item = baseItem as SoulBoneItem;
            if (item != null)
            {
                Manager.DB.Call(new QueryUpdateSoulBoneIndex(item.Bone));
            }
        }

        public void SyncDbItemSpecInfo(BaseItem baseItem)
        {
            SoulBoneItem item = baseItem as SoulBoneItem;
            if (item != null)
            {
                Manager.DB.Call(new QueryUpdateSoulBoneSpecId(item.Bone));
            }
        }

        public void SyncDbItemSpecInfo(SoulBone soulBone)
        {
            if (soulBone != null)
            {
                Manager.DB.Call(new QueryUpdateSoulBoneSpecId(soulBone));
            }
        }

        public override void UpdateItem(BaseItem baseItem)
        {

        }

        public SoulBoneItem BuyShopItemAddSoulBone(int id, SoulBone bone)
        {
            return BaseAddSoulBone(id, bone);
        }

        public void SyncDbBatchUpdateItemInfo(List<SoulBone> boneList)
        {
            if (boneList.Count > 0)
            {
                Manager.DB.Call(new QueryBatchUpdateSoulBoneInfo(boneList));
            }
        }
    }

    public class SoulBoneItem : BaseItem
    {
        private SoulBone bone = new SoulBone();

        public bool Deleted;

        public SoulBone Bone
        {
            get
            {
                return bone;
            }

            set
            {
                bone = InitSoulBoneItem(value);
            }
        }

        private SoulBone InitSoulBoneItem(SoulBone bone)
        {
            //在这里进行Item类的初始化
            Uid = bone.Uid;
            OwnerUid = bone.OwnerId;
            MainType = MainType.SoulBone;
            Id = bone.TypeId;
            return bone;
        }

        public override bool BindData(int id)
        {
            return false;
        }

        public SoulBoneInfo GenerateInfo()
        {
            SoulBoneInfo info = new SoulBoneInfo();
            info.AnimalType = bone.AnimalType;
            info.PartType = bone.PartType;
            info.PrefixId = bone.Prefix;
            SoulBoneNature nature = new SoulBoneNature();
            nature.NatureType = bone.MainNatureType;
            nature.Value = bone.MainNatureValue;
            nature.Quality = bone.Quality;
            info.MainNature = nature;
            if (bone.AdditionValue1 > 0 && bone.AdditionType1 > 0)
            {
                SoulBoneNature temp = new SoulBoneNature();
                temp.NatureType = bone.AdditionType1;
                temp.Value = bone.AdditionValue1;
                info.Natures.Add(1, temp);
            }
            if (bone.AdditionValue2 > 0 && bone.AdditionType2 > 0)
            {
                SoulBoneNature temp = new SoulBoneNature();
                temp.NatureType = bone.AdditionType2;
                temp.Value = bone.AdditionValue2;
                info.Natures.Add(2, temp);
            }
            if (bone.AdditionValue3 > 0 && bone.AdditionType3 > 0)
            {
                SoulBoneNature temp = new SoulBoneNature();
                temp.NatureType = bone.AdditionType3;
                temp.Value = bone.AdditionValue3;
                info.Natures.Add(3, temp);
            }
            info.SpecSkills.Add(bone.SpecId1);
            info.SpecSkills.Add(bone.SpecId2);
            info.SpecSkills.Add(bone.SpecId3);
            info.SpecSkills.Add(bone.SpecId4);
            return info;
        }

        public MSG_ZGC_SOUL_BONE_ITEM GenerateMsg()
        {
            MSG_ZGC_SOUL_BONE_ITEM msg = new MSG_ZGC_SOUL_BONE_ITEM();
            msg.UidHigh = (uint)(Uid >> 32);
            msg.UidLow = (uint)((Uid << 32) >> 32);
            msg.EquipedHeroId = bone.EquipedHeroId;
            msg.PartType = bone.PartType;
            msg.AnimalType = bone.AnimalType;
            msg.Quality = bone.Quality;
            msg.Prefix = bone.Prefix;
            msg.MainNatureType = bone.MainNatureType;
            msg.MainNatureValue = bone.MainNatureValue;
            msg.AdditionType1 = bone.AdditionType1;
            msg.AdditionType2 = bone.AdditionType2;
            msg.AdditionValue1 = bone.AdditionValue1;
            msg.AdditionValue2 = bone.AdditionValue2;
            msg.AdditionType3 = bone.AdditionType3;
            msg.AdditionValue3 = bone.AdditionValue3;
            msg.SpecId1 = bone.SpecId1;
            msg.SpecId2 = bone.SpecId2;
            msg.SpecId3 = bone.SpecId3;
            msg.SpecId4 = bone.SpecId4;
            msg.Deleted = Deleted;
            msg.PileNum = 1;
            msg.Id = Id;
            //Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            //dic.Add((NatureType)msg.MainNatureType, msg.MainNatureValue);
            //if (msg.AdditionType1 != 0)
            //{
            //    if (msg.AdditionType1 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType1, msg.AdditionValue1);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue1;
            //    }
            //}
            //if (msg.AdditionType2 != 0)
            //{
            //    if (msg.AdditionType2 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType2, msg.AdditionValue2);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue2;
            //    }
            //}
            //if (msg.AdditionType3 != 0)
            //{
            //    if (msg.AdditionType3 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType3, msg.AdditionValue3);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue3;
            //    }
            //}
            msg.Score = SoulBoneManager.GetSoulBoneScore(bone);
            return msg;
        }

        public MSG_ZGC_SOUL_BONE_ITEM GenerateShowMsg()
        {
            MSG_ZGC_SOUL_BONE_ITEM msg = new MSG_ZGC_SOUL_BONE_ITEM();
            msg.EquipedHeroId = bone.EquipedHeroId;
            msg.PartType = bone.PartType;
            msg.AnimalType = bone.AnimalType;
            msg.Quality = bone.Quality;
            msg.Prefix = bone.Prefix;
            msg.MainNatureType = bone.MainNatureType;
            msg.MainNatureValue = bone.MainNatureValue;
            msg.AdditionType1 = bone.AdditionType1;
            msg.AdditionType2 = bone.AdditionType2;
            msg.AdditionValue1 = bone.AdditionValue1;
            msg.AdditionValue2 = bone.AdditionValue2;
            msg.AdditionType3 = bone.AdditionType3;
            msg.AdditionValue3 = bone.AdditionValue3;
            msg.SpecId1 = bone.SpecId1;
            msg.SpecId2 = bone.SpecId2;
            msg.SpecId3 = bone.SpecId3;
            msg.SpecId4 = bone.SpecId4;
            msg.Id = Id;
            //Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            //dic.Add((NatureType)msg.MainNatureType, msg.MainNatureValue);
            //if (msg.AdditionType1 != 0)
            //{
            //    if (msg.AdditionType1 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType1, msg.AdditionValue1);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue1;
            //    }
            //}
            //if (msg.AdditionType2 != 0)
            //{
            //    if (msg.AdditionType2 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType2, msg.AdditionValue2);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue2;
            //    }
            //}
            //if (msg.AdditionType3 != 0)
            //{
            //    if (msg.AdditionType3 != msg.MainNatureType)
            //    {
            //        dic.Add((NatureType)msg.AdditionType3, msg.AdditionValue3);
            //    }
            //    else
            //    {
            //        dic[(NatureType)msg.MainNatureType] += msg.AdditionValue3;
            //    }
            //}
            msg.Score = SoulBoneManager.GetSoulBoneScore(bone);

            return msg;
        }

        public ZMZ_SOULBONE_ITEM GenerateTransformMsg()
        {
            ZMZ_SOULBONE_ITEM msg = new ZMZ_SOULBONE_ITEM();
            msg.Uid = this.Uid;
            msg.EquipedHeroId = bone.EquipedHeroId;
            msg.PartType = bone.PartType;
            msg.AnimalType = bone.AnimalType;
            msg.Quality = bone.Quality;
            msg.Prefix = bone.Prefix;
            msg.MainNatureType = bone.MainNatureType;
            msg.MainNatureValue = bone.MainNatureValue;
            msg.AdditionType1 = bone.AdditionType1;
            msg.AdditionType2 = bone.AdditionType2;
            msg.AdditionValue1 = bone.AdditionValue1;
            msg.AdditionValue2 = bone.AdditionValue2;
            msg.AdditionType3 = bone.AdditionType3;
            msg.AdditionValue3 = bone.AdditionValue3;
            msg.Deleted = Deleted;
            msg.Id = Id;
            msg.SpecId1 = bone.SpecId1;
            msg.SpecId2 = bone.SpecId2;
            msg.SpecId3 = bone.SpecId3;
            msg.SpecId4 = bone.SpecId4;

            return msg;
        }

        public SoulBoneItem GenerateDeleteInfo()
        {           
            SoulBoneItem item = new SoulBoneItem();
            item.Deleted = true;
            item.Bone = Bone.Clone();
            return item;
        }
    }
}

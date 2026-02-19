using CommonUtility;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    class SoulBoneShopItem : CommonShopItem
    {
        private SoulBone bone;
        public SoulBone SoulBone { get { return bone; } private set { bone = value; } }

        public SoulBoneShopItem(DBShopItemInfo dbShopItemInfo) : base(dbShopItemInfo)
        { }

        public void SetRewardInfo(string rewardInfo, SoulBone soulBone)
        {
            ItemInfo = rewardInfo;
            SoulBone = soulBone;
        }

        public override string GetItemInfo()
        {
            return ItemInfo;
        }

        public override MSG_ZGC_SHOP_ITEM GenerateMsg()
        {
            MSG_ZGC_SHOP_ITEM msg = base.GenerateMsg();
            msg.BoneInfo = GenerateSoulBoneMsg();
            return msg;
        }

        public MSG_ZGC_SOUL_BONE_ITEM GenerateSoulBoneMsg()
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
            msg.PileNum = 1;
            msg.Id = bone.TypeId;
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

            msg.SpecId1 = bone.SpecId1;
            msg.SpecId2 = bone.SpecId2;
            msg.SpecId3 = bone.SpecId3;
            msg.SpecId4 = bone.SpecId4;

            msg.Score = SoulBoneManager.GetSoulBoneScore(bone);
            return msg;
        }
    }
}

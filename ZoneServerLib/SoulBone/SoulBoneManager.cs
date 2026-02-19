using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class SoulBoneManager
    {
        private PlayerChar owner;

        private HashSet<SoulBone> soulBoneList;

        private Dictionary<int, SoulBoneSuit> heroAndSuit;

        private Bag_SoulBone bag = null;

        public PlayerChar Owner
        {
            get
            {
                return owner;
            }

            set
            {
                owner = value;
            }
        }

        public SoulBoneManager(PlayerChar owner)
        {
            this.Owner = owner;
            soulBoneList = new HashSet<SoulBone>();
            heroAndSuit = new Dictionary<int, SoulBoneSuit>();
        }

        public void BindBag(Bag_SoulBone bag)
        {
            this.bag = bag;
        }

        public void InitSuit(List<SoulBone> bones)
        {
            foreach (var item in bones)
            {
                SoulBoneSuit suit = null;
                if (heroAndSuit.TryGetValue(item.EquipedHeroId, out suit))
                {
                    suit.Load(item);
                }
                else
                {
                    suit = new SoulBoneSuit();
                    suit.Load(item);
                    heroAndSuit.Add(item.EquipedHeroId, suit);
                }
            }
            soulBoneList = new HashSet<SoulBone>(bones);
        }

        public Tuple<bool, List<BaseItem>> Equip(HeroInfo heroInfo, SoulBone bone)
        {
            if (bone.EquipedHeroId > 0)
            {
                TakeOff(bone);
            }
            return TakeOn(heroInfo, bone);
        }

    

        //通过bag拿出引用，然后使用manager更换,有可能要先把别人身上Takeoff，要通知client
        public Tuple<bool, List<BaseItem>> TakeOn(HeroInfo heroInfo, SoulBone bone)
        {
            //判断hero存在
            //成功要同步db
            List<BaseItem> list = new List<BaseItem>();
            SoulBoneSuit suit = null;
            if (heroAndSuit.TryGetValue(heroInfo.Id, out suit))
            {
                if (!suit.ContainPart(bone.PartType))
                {
                    suit.Load(bone);
                    soulBoneList.Add(bone);
                    bone.EquipedHeroId = heroInfo.Id;
                    bag.UpdateSoulBone(bone);
                    list.Add(bag.GetItem(bone.Uid));

                    //魂骨替换
                    Owner.BIRecordBoneReplaceLog(heroInfo.Id, heroInfo.Level, bone.PartType, 0, 0, bone.TypeId, GetSoulBoneScore(bone));

                    //养成
                    Owner.BIRecordDevelopLog(DevelopType.EquipSoulBone, bone.TypeId, 0, GetSoulBoneScore(bone), heroInfo.Id, heroInfo.Level);
                }
                else
                {
                    SoulBone old = suit.Unload(bone.PartType);
                    old.EquipedHeroId = -1;
                    bag.UpdateSoulBone(old);
                    soulBoneList.Remove(old);
                    list.Add(bag.GetItem(old.Uid));

                    suit.Load(bone);
                    soulBoneList.Add(bone);
                    bone.EquipedHeroId = heroInfo.Id;
                    bag.UpdateSoulBone(bone);
                    list.Add(bag.GetItem(bone.Uid));

                    //魂骨替换
                    Owner.BIRecordBoneReplaceLog(heroInfo.Id, heroInfo.Level, bone.PartType, old.TypeId, GetSoulBoneScore(old), bone.TypeId, GetSoulBoneScore(bone));

                    //养成
                    Owner.BIRecordDevelopLog(DevelopType.EquipSoulBone, bone.TypeId, GetSoulBoneScore(old), GetSoulBoneScore(bone), heroInfo.Id, heroInfo.Level);
                }
            }
            else
            {
                suit = new SoulBoneSuit();
                heroAndSuit.Add(heroInfo.Id, suit);
                suit.Load(bone);
                soulBoneList.Add(bone);
                bone.EquipedHeroId = heroInfo.Id;
                bag.UpdateSoulBone(bone);
                list.Add(bag.GetItem(bone.Uid));

                //魂骨替换
                Owner.BIRecordBoneReplaceLog(heroInfo.Id, heroInfo.Level, bone.PartType, 0, 0, bone.TypeId, GetSoulBoneScore(bone));
                //养成
                Owner.BIRecordDevelopLog(DevelopType.EquipSoulBone, bone.TypeId, 0, GetSoulBoneScore(bone), heroInfo.Id, heroInfo.Level);
            }

            return Tuple.Create(true, list);
        }

        /// <summary>
        /// 卸下失败只有一种情况，并没有装备上
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="bone"></param>
        /// <returns></returns>
        public bool TakeOff(int heroId, SoulBone bone)
        {
            SoulBoneSuit suit = null;
            if (heroAndSuit.TryGetValue(heroId, out suit))
            {
                if (!suit.Contain(bone.Uid))
                {
                    return false;
                }
                else
                {
                    SoulBone old = suit.Unload(bone.PartType);
                    old.EquipedHeroId = -1;
                    bag.UpdateSoulBone(bone);
                    soulBoneList.Remove(old);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 熔炼之前卸下
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool TakeOff(SoulBone bone)
        {
            SoulBoneSuit suit = null;
            if (heroAndSuit.TryGetValue(bone.EquipedHeroId, out suit))
            {
                if (!suit.Contain(bone.Uid))
                {
                    return false;
                }
                else
                {
                    SoulBone old = suit.Unload(bone.PartType);
                    old.EquipedHeroId = -1;
                    bag.UpdateSoulBone(bone);
                    soulBoneList.Remove(old);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public int GetEquipedCount()
        {
            return soulBoneList.Count;
        }

        public int GetEquipedCountBySlot(int slot)
        {
            int count = 0;
            foreach (var item in soulBoneList)
            {
                if (item.PartType == slot)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取某个英雄的魂骨(已经计算过套装加成的)// 现在是原装的bone，不加成到魂骨上
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public List<SoulBone> GetEnhancedHeroBones(int heroId)
        {
            SoulBoneSuit bones = null;
            if (heroAndSuit.TryGetValue(heroId, out bones))
            {
                return bones.GetSuitAdditions();
            }
            else
            {
                return null;
            }
        }

        public Dictionary<NatureType, int> GetEnhancedHeroBoneAdditions(int heroId)
        {
            SoulBoneSuit bones = null;
            if (heroAndSuit.TryGetValue(heroId, out bones))
            {
                return bones.GetSuitAddValues();
            }
            else
            {
                return null;
            }
        }

        public static SoulBone GenerateSoulBoneInfo(string itemBasicInfo, bool fixdb)
        {
            ItemBasicInfo basicIntem = ItemBasicInfo.Parse(itemBasicInfo);
            if (basicIntem == null || basicIntem.RewardType != (int)RewardType.SoulBone)
            {
                return null;
            }

            return GenerateSoulBoneInfo(basicIntem, fixdb);
        }

        public static SoulBone GenerateSoulBoneInfo(ItemBasicInfo basicItem, bool fixdb)
        {
            if (basicItem == null)
            {
                return null;
            }

            bool needFix = false;
            if (!basicItem.CheckParamCount())
            {
                if (basicItem.Attrs.Count != ItemBasicInfo.SoulBoneFixAttrCount)
                {
                    return null;
                }

                needFix = true;
            }

            List<int> attrList = basicItem.Attrs.ConvertAll(x => int.Parse(x));
            int animal = attrList[0];
            int part = attrList[1];
            int mainValue = attrList[2];

            SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(animal, part, mainValue);
            if (itemInfo == null) return null;
                
            SoulBoneInfo info = new SoulBoneInfo();
            info.AnimalType = animal;
            info.PartType = part;
            info.MainNature.Quality = itemInfo.Quality;
            info.MainNature.Value = mainValue;
            info.MainNature.NatureType = itemInfo.MainNatureType;

            attrList.RemoveAt(0);
            attrList.RemoveAt(0);
            attrList.RemoveAt(0);
            info.SpecSkills.AddRange(attrList);

            string mvAAA = $"{mainValue}|{itemInfo.AddAttrType1}|{itemInfo.AddAttrType2}|{itemInfo.AddAttrType3}";
            string adds = ScriptManager.SoulBone.GetAddAttr(mvAAA);

            SoulBoneLibrary.ProduceAdds(info, adds);
            SoulBoneLibrary.ProducePrefix(info, itemInfo.Id, needFix, fixdb);

            SoulBone bone = new SoulBone(info);
            bone.TypeId = basicItem.Id;

            if (needFix)
            {
                basicItem.Attrs.Add(bone.SpecId1.ToString());
                basicItem.Attrs.Add(bone.SpecId2.ToString());
                basicItem.Attrs.Add(bone.SpecId3.ToString());
                basicItem.Attrs.Add(bone.SpecId4.ToString());
            }

            return bone;
        }

        public static List<ItemBasicInfo> GenerateSoulboneReward(string soulBoneReward, List<SoulBone> soulBoneList = null, int job = 0, int num = 1, bool itemUse = false)
        {
            SoulBoneReward boneReward = new SoulBoneReward(soulBoneReward);
            if (boneReward.GenerateInfo())
            {
                List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
                for (int n = 0; n < num; n++)
                {
                    for (int i = 0; i < boneReward.Num; i++)
                    {
                        if (RAND.Range(0, 10000) > boneReward.Prob)
                        {
                            continue;
                        }
                        int animal = boneReward.GetAnimal(job);
                        int part = boneReward.GetPart();
                        int mainValue = 0;

                        SoulBoneInfo info = new SoulBoneInfo();

                        bool produceSpec = true;
                        SoulBoneDrop drop = SoulBoneLibrary.GetSoulBoneDrop(boneReward.MainValueRule);
                        if (drop != null)
                        {
                            mainValue = drop.GetValue();
                            if (drop.SpecList.Count > 0)
                            {
                                produceSpec = false;
                                info.SpecSkills.AddRange(drop.SpecList);
                            }
                        }

                        SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(animal, part, mainValue);

                        info.AnimalType = animal;
                        info.PartType = part;
                        string mvAAA = "" + mainValue;
                        if (itemInfo != null)
                        {
                            mvAAA += "|" + itemInfo.AddAttrType1 + "|" + itemInfo.AddAttrType2 + "|" + itemInfo.AddAttrType3;
                            info.MainNature.Quality = itemInfo.Quality;
                            info.MainNature.Value = mainValue;
                            info.MainNature.NatureType = itemInfo.MainNatureType;

                            string adds = ScriptManager.SoulBone.GetAddAttr(mvAAA);
                            SoulBoneLibrary.ProduceAdds(info, adds);
                            if (itemUse)
                            {
                                SoulBoneLibrary.ProducePrefixByUseItem(info, itemInfo.Id, boneReward.locationSpecSubTypeRange);
                            }
                            else
                            {
                                SoulBoneLibrary.ProducePrefix(info, itemInfo.Id, produceSpec, false);
                            }                           

                            SoulBone bone = new SoulBone(info);
                            bone.TypeId = itemInfo.Id;
                            soulBoneList?.Add(bone);

                            ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.SoulBone, itemInfo.Id, 1, animal.ToString(),
                                part.ToString(), mainValue.ToString(), bone.SpecId1.ToString(), bone.SpecId2.ToString(), bone.SpecId3.ToString(), bone.SpecId4.ToString());

                            infos.Add(baseInfo);
                        }
                    }
                }
                return infos;
            }
            return null;
        }

        public static List<ItemBasicInfo> GenerateSoulboneReward(RewardDropItem dropItem, int job = 0)
        {
            SoulBoneReward boneReward = new SoulBoneReward(dropItem);
            if (boneReward.Num > 0)
            {
                List<ItemBasicInfo> infos = new List<ItemBasicInfo>();
                for (int i = 0; i < boneReward.Num; i++)
                {
                    if (!dropItem.HasJob)
                    {
                        job = 0;
                    }
                    int animal = boneReward.GetAnimal(job);
                    int part = boneReward.GetPart();
                    int mainValue = 0;

                    SoulBoneInfo info = new SoulBoneInfo();

                    bool produceSpec = true;
                    SoulBoneDrop drop = SoulBoneLibrary.GetSoulBoneDrop(boneReward.MainValueRule);
                    if (drop != null)
                    {
                        mainValue = drop.GetValue();
                        if (drop.SpecList.Count > 0)
                        {
                            produceSpec = false;
                            info.SpecSkills.AddRange(drop.SpecList);
                        }
                    }

                    SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(animal, part, mainValue);

                    info.AnimalType = animal;
                    info.PartType = part;
                    string mvAAA = "" + mainValue;
                    if (itemInfo != null)
                    {
                        mvAAA += "|" + itemInfo.AddAttrType1 + "|" + itemInfo.AddAttrType2 + "|" + itemInfo.AddAttrType3;
                        info.MainNature.Quality = itemInfo.Quality;
                        info.MainNature.Value = mainValue;
                        info.MainNature.NatureType = itemInfo.MainNatureType;

                        string adds = ScriptManager.SoulBone.GetAddAttr(mvAAA);
                        SoulBoneLibrary.ProduceAdds(info, adds);

                        if (boneReward.locationSpecSubTypeRange.Count > 0)
                        {
                            SoulBoneLibrary.ProducePrefixByUseItem(info, itemInfo.Id, boneReward.locationSpecSubTypeRange);
                        }
                        else
                        {
                            SoulBoneLibrary.ProducePrefix(info, itemInfo.Id, produceSpec, false);
                        }
                        SoulBone bone = new SoulBone(info);
                        bone.TypeId = itemInfo.Id;

                        ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.SoulBone, itemInfo.Id, 1, animal.ToString(), 
                            part.ToString(), mainValue.ToString(), bone.SpecId1.ToString(), bone.SpecId2.ToString(), bone.SpecId3.ToString(), bone.SpecId4.ToString());

                        infos.Add(baseInfo);
                    }
                }
                return infos;
            }
            return null;
        }

        public static MSG_ZGC_SOUL_BONE_ITEM GenerateSoulBoneMsg(SoulBone bone)
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
            msg.PileNum = 1;
            msg.Id = bone.TypeId;
            msg.Score = GetSoulBoneScore(bone);
            return msg;
        }

        public static int GetSoulBoneScore(SoulBone bone)
        {
            Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            dic.Add((NatureType)bone.MainNatureType, bone.MainNatureValue);
            if (bone.AdditionType1 != 0)
            {
                if (bone.AdditionType1 != bone.MainNatureType)
                {
                    dic.Add((NatureType)bone.AdditionType1, bone.AdditionValue1);
                }
                else
                {
                    dic[(NatureType)bone.MainNatureType] += bone.AdditionValue1;
                }
            }
            if (bone.AdditionType2 != 0)
            {
                if (bone.AdditionType2 != bone.MainNatureType)
                {
                    dic.Add((NatureType)bone.AdditionType2, bone.AdditionValue2);
                }
                else
                {
                    dic[(NatureType)bone.MainNatureType] += bone.AdditionValue2;
                }
            }
            if (bone.AdditionType3 != 0)
            {
                if (bone.AdditionType3 != bone.MainNatureType)
                {
                    dic.Add((NatureType)bone.AdditionType3, bone.AdditionValue3);
                }
                else
                {
                    dic[(NatureType)bone.MainNatureType] += bone.AdditionValue3;
                }
            }
            int Score = ScriptManager.BattlePower.CaculateSoulBoneScore(dic);
            Score += GetSpecBattlePower(bone);

            return Score;
        }

        private static int GetSpecBattlePower(SoulBone soulBone)
        {
            int battlePower = 0;
            soulBone.GetSpecList().ForEach(x =>
            {
                SoulBoneSpecModel model = SoulBoneLibrary.GetSpecModel(x);
                if (model != null)
                {
                    battlePower += model.BattlePower;
                }
            });
            return battlePower;
        }

        public void HeroSwapSuit(int fromHeroId, int toHeroId, List<BaseItem> deleteList)
        {
            List<SoulBone> bones;
            SoulBoneItem item;
            SoulBoneSuit toSuit;

            heroAndSuit.TryGetValue(toHeroId, out toSuit);
            SoulBoneSuit fromSuit;
            heroAndSuit.TryGetValue(fromHeroId, out fromSuit);
            if (fromSuit != null)
            {
                bones = fromSuit.GetSoulBones();
                foreach (var bone in bones)
                {
                    item = bag.GetItem(bone.Uid) as SoulBoneItem;
                    deleteList.Add(item.GenerateDeleteInfo());
                }
                fromSuit.ChangeEquipHero(toHeroId);
                heroAndSuit[toHeroId] = fromSuit;
            }
            else if (toSuit != null)
            {
                //fromHero没有魂骨
                heroAndSuit.Remove(toHeroId);
            }
            if (toSuit != null)
            {
                bones = toSuit.GetSoulBones();
                foreach (var bone in bones)
                {
                    item = bag.GetItem(bone.Uid) as SoulBoneItem;
                    deleteList.Add(item.GenerateDeleteInfo());
                }
                toSuit.ChangeEquipHero(fromHeroId);
                heroAndSuit[fromHeroId] = toSuit;
            }
            else if (fromSuit != null)
            {
                //toero没有魂骨
                heroAndSuit.Remove(fromHeroId);
            }
        }

        public void UpdateSoulBoneInfo(int fromHeroId, int toHeroId, List<BaseItem> updateList)
        {
            SoulBoneSuit suit;
            List<SoulBone> bones;
            List<SoulBone> boneList = new List<SoulBone>();
            heroAndSuit.TryGetValue(fromHeroId, out suit);
            if (suit != null)
            {
                bones = suit.GetSoulBones();
                foreach (var bone in bones)
                {
                    updateList.Add(bag.GetItem(bone.Uid));
                    boneList.Add(bone);
                }
            }
            heroAndSuit.TryGetValue(toHeroId, out suit);
            if (suit != null)
            {
                bones = suit.GetSoulBones();
                foreach (var bone in bones)
                {
                    updateList.Add(bag.GetItem(bone.Uid));
                    boneList.Add(bone);
                }
            }
            bag.SyncDbBatchUpdateItemInfo(boneList);
        }
    }
}

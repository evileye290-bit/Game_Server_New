using System;
using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //魂骨
        public SoulBoneManager SoulboneMng { get; private set; }

        public void InitSoulBoneManager()
        {
            SoulboneMng = new SoulBoneManager(this);
            bagManager.SoulBoneBag.BindBoneManager(SoulboneMng);
        }

        /// <summary>
        /// 熔炼
        /// </summary>
        /// <param name="msg"></param>
        public void SmeltSoulBone(MSG_GateZ_SMELT_SOULBONE msg)
        {
            MSG_ZGC_SOULBONE_SMELT_RESULT result = new MSG_ZGC_SOULBONE_SMELT_RESULT();
            RewardManager rewarManager = new RewardManager();

            result.ErrorCode = (int)ErrorCode.Success;
            List<SoulBoneInfo> mainInfo = new List<SoulBoneInfo>();
            List<SoulBoneInfo> secInfo = new List<SoulBoneInfo>();
            List<SoulBoneItem> costItems = new List<SoulBoneItem>();
            Bag_SoulBone bag = bagManager.SoulBoneBag;
            foreach (var item in msg.MainBones)
            {
                SoulBoneItem temp = bag.GetSoulBoneItem(item.Uid);
                if (temp == null)
                {
                    Logger.Log.Warn("player {0} smelt soulBone failed: not find item {1}", msg.PcUid, item.Uid);
                    result.ErrorCode = (int)ErrorCode.NoSuchItem;
                    Write(result);
                    return;
                }

                SoulBoneItemInfo moduleInfo = SoulBoneLibrary.GetItemInfo(temp.Id);
                if (moduleInfo == null)
                {
                    Logger.Log.Warn("player {0} smelt soulBone failed: not find item model {1}", msg.PcUid, temp.Id);
                    result.ErrorCode = (int)ErrorCode.NoSuchItem;
                    Write(result);
                    return;
                }

                if (!moduleInfo.WearAble)
                {
                    Logger.Log.Warn("player {0} smelt soulBone failed: not wareAble item model {1}", msg.PcUid, temp.Id);
                    result.ErrorCode = (int)ErrorCode.SoulBoneNotWardAble;
                    Write(result);
                    return;
                }

                mainInfo.Add(temp.GenerateInfo());
                costItems.Add(temp);
            }

            //组装新魂骨信息
            string animalPositions = "";
            List<int> animalList = new List<int>();
            int count = 0;
            foreach (var temp in mainInfo)
            {
                if (count == 0)
                {
                    animalPositions += temp.AnimalType + ":" + temp.PartType;
                }
                else
                {
                    animalPositions += "|" + temp.AnimalType + ":" + temp.PartType;
                }
                count++;

                animalList.Add(temp.AnimalType);
            }
            if (count != 6)
            {
                Logger.Log.Warn("player {0} smelt soulBone with mainInfo error", Uid);
                result.ErrorCode = (int)ErrorCode.ItemNotEnough;
                Write(result);
                return;
            }

            Dictionary<int, int> animals = ScriptManager.SoulBone.GetAnimalProbability(animalList);
            SoulBoneInfo info = new SoulBoneInfo();
            info.AnimalType = SoulBoneLibrary.GetAnimal(animals);
            info.PartType = RAND.Range(1, 6);

            List<int> mainNatureInput = new List<int>();
            List<int> prefixes = new List<int>();
            foreach (var temp in mainInfo)
            {
                mainNatureInput.Add(temp.MainNature.Value);
                prefixes.Add(temp.PrefixId);

                SoulBonePrefix prefix = SoulBoneLibrary.GetPrefix(temp.PrefixId);
                if (prefix != null)
                {
                    if (!string.IsNullOrEmpty(prefix.SmeltReward))
                    {
                        rewarManager.AddSimpleReward(prefix.SmeltReward);
                    }
                }
            }

            int mainValuePrefix = ScriptManager.SoulBone.GetMainNatureValue3(prefixes.ToArray(), BagLibrary.MaxPrefix);

            var mainValueAndCost = SoulBoneLibrary.GetMainValueAndCost4Prefix(mainValuePrefix);
            info.MainNature.Value = mainValueAndCost.Item1;
            int cost = mainValueAndCost.Item2;
            if (GetCoins(CurrenciesType.gold) < cost)
            {
                Logger.Log.Warn("player {0} smelt soulBone failed: gold not enough, cost {1}", msg.PcUid, cost);
                result.ErrorCode = (int)ErrorCode.NoCoin;
                Write(result);
                return;
            }

            List<int> addAttrTypes = new List<int>();
            SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(info.AnimalType, info.PartType, mainValueAndCost.Item1);
            string mvAAA = "" + mainValueAndCost.Item1;
            if (itemInfo != null)
            {
                mvAAA += "|" + itemInfo.AddAttrType1 + "|" + itemInfo.AddAttrType2 + "|" + itemInfo.AddAttrType3;
                info.MainNature.Quality = itemInfo.Quality;
                info.MainNature.NatureType = itemInfo.MainNatureType;
            }
            else
            {
                Logger.Log.Warn($"player {uid} smelt soulBone with error  get Item info animal {info.AnimalType} part {info.PartType} main {mainValueAndCost.Item1}");
                result.ErrorCode = (int)ErrorCode.NoItemInfo;
                Write(result);
                return;
            }
            string adds = ScriptManager.SoulBone.GetAddAttr(mvAAA);

            SoulBoneLibrary.ProduceAdds(info, adds);
            SoulBoneLibrary.ProducePrefix(info, itemInfo.Id, true, false);
            
            if (info != null)
            {
                SoulBone bone = new SoulBone(info);
                SoulBoneItem item = bag.NewItem(Uid, bone);
                string extraParam = bone.ToString();

                //RecordObtainLog(ObtainWay.SmeltSoulBone, RewardType.SoulBone, item.Id, 1, 1, extraParam);
                //BI 新增物品
                KomoeEventLogItemFlow("add", "", item.Id, MainType.SoulBone.ToString(), 1, 0, 1, (int)ObtainWay.SmeltSoulBone, 0, 0, 0, 0);

                BIRecordObtainItem(RewardType.SoulBone, ObtainWay.SmeltSoulBone, item.Id, 1, 1);

                if (bag.AddSoulBone(item) == null)
                {
                    //合成失败同步流
                    result.ErrorCode = (int)ErrorCode.AddToBagFail;
                }
                List<int> heroIds = new List<int>();
                List<BaseItem> items = new List<BaseItem>();
                foreach (var costItem in costItems)
                {
                    SoulBoneItem temp = DelItem2Bag(costItem, RewardType.SoulBone, 1, ConsumeWay.SmeltSoulBone, costItem.ToString()) as SoulBoneItem;
                    if (temp != null)
                    {
                        //SoulBoneItem temp=bag.DelItem(uid);
                        temp.Deleted = true;
                        if (temp.Bone.EquipedHeroId > 0)
                        {
                            heroIds.Add(temp.Bone.EquipedHeroId);
                            SoulboneMng.TakeOff(temp.Bone);
                        }
                        items.Add(temp);
                    }
                }
                DelCoins(CurrenciesType.gold, cost, ConsumeWay.SmeltSoulBone, item.Id.ToString());
                items.Add(item);

                rewarManager.BreakupRewards();
                AddRewards(rewarManager, ObtainWay.SmeltSoulBone, msg.MainBones.Count.ToString());

                //通知以便删除和添加
                SyncClientItemsInfo(items);
                ulong itemUid = item.Bone.Uid;
                result.UidHigh = (uint)(itemUid >> 32);
                result.UidLow = (uint)((itemUid << 32) >> 32);

                //广播熔炼出最高品质且带词缀的魂骨
                if (result.ErrorCode == (int)ErrorCode.Success)
                {
                    BroadcastSmeltBestSoulBone(item.Bone.Quality, item.Bone.Prefix, item.Bone.TypeId);
                }

                List<HeroInfo> changeList = new List<HeroInfo>();
                foreach (var heroId in heroIds)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                    if (heroInfo != null)
                    {
                        //魂骨属性加成(属于加成属性)
                        HeroMng.InitHeroNatureInfo(heroInfo);
                        HeroMng.NotifyClientBattlePowerFrom(heroId);
                        changeList.Add(heroInfo);
                    }
                }

                SyncHeroChangeMessage(changeList);
                //熔炼魂骨
                AddTaskNumForType(TaskType.SoulBoneSmelt);
                AddPassCardTaskNum(TaskType.SoulBoneSmelt);
            }
            else
            {
                Logger.Log.Warn("player {0} smelt soulBone with error info is null ", Uid);
                result.ErrorCode = (int)ErrorCode.SmeltFail;
            }

            Write(result);
        }

        public void EquipSoulBone(MSG_GateZ_EQUIP_SOULBONE msg)
        {
            MSG_ZGC_EQUIP_SOULBONE_RESULT result = new MSG_ZGC_EQUIP_SOULBONE_RESULT();
            SoulBoneItem item = bagManager.SoulBoneBag.GetSoulBoneItem(msg.Uid);
            if (item == null)
            {
                result.ErrorCode = (int)ErrorCode.Fail;
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed with no such bone {2}", Uid, msg.Hero, msg.Uid);
                Write(result);
                return;
            }
            
            //判断动物
            SoulBoneItemInfo temp = SoulBoneLibrary.GetItemInfo(item.Bone.AnimalType, item.Bone.PartType, item.Bone.MainNatureValue);
            if (temp == null)
            {
                result.ErrorCode = (int) ErrorCode.Fail;
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed with no such bone info {2}", Uid, msg.Hero, msg.Uid);
                Write(result);
                return;
            }

            if (!temp.WearAble)
            {
                Logger.Log.Warn("player {0} equip soulBone failed: not wareAble item model {1}", msg.PcUid, temp.Id);
                result.ErrorCode = (int)ErrorCode.SoulBoneNotWardAble;
                Write(result);
                return;
            }

            HeroInfo heroInfo = HeroMng.GetHeroInfo(msg.Hero);
            if (heroInfo == null)
            {
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed with no such hero info {2}", Uid, msg.Hero, msg.Uid);
                result.ErrorCode = (int)ErrorCode.Fail;
                Write(result);
                return;
            }
            HeroModel heroModel = HeroLibrary.GetHeroModel(msg.Hero);
            if (heroModel == null)
            {
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed with no such hero model {2}", Uid, msg.Hero, msg.Uid);
                result.ErrorCode = (int)ErrorCode.Fail;
                Write(result);
                return;
            }
            if (temp.Job != heroModel.Job)
            {
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed  hero job {2} is not {3}", Uid, msg.Hero, heroModel.Job, temp.Job);
                result.ErrorCode = (int)ErrorCode.JobNotPermission;
                Write(result);
                return;
            }
            ////成长值 属性 4转9 (属于加成属性)

            //List<SoulBone> oldBoneList = SoulboneMng.GetEnhancedHeroBones(msg.Hero);
            //Dictionary<NatureType, int> oldSuit = SoulboneMng.GetEnhancedHeroBoneAdditions(msg.Hero);
            //komoelog
            int operateType = 1;
            if (item.Bone.EquipedHeroId > 0)
            {
                operateType = 3;
            }

            var equipInfo = SoulboneMng.Equip(heroInfo, item.Bone);
            if (equipInfo.Item1)
            {
                result.ErrorCode = (int)ErrorCode.Success;

                SyncClientItemsInfo(equipInfo.Item2);

                //komoelog
                int beforePower = heroInfo.GetBattlePower();

                ////穿戴魂骨属性加成
                HeroMng.InitHeroNatureInfo(heroInfo);
                HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);
                SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
                //List<SoulBone> newBoneList = SoulboneMng.GetEnhancedHeroBones(msg.Hero);
                //Dictionary<NatureType, int> newSuit = SoulboneMng.GetEnhancedHeroBoneAdditions(msg.Hero);
                //HeroMng.EquipSoulBoneNature(msg.Hero, oldBoneList, newBoneList);
                //HeroMng.EquipSoulBoneNature(msg.Hero, oldSuit, newSuit);

                //HeroMng.EquipSoulBoneNature(msg.Hero, oldSuit, newSuit, oldBoneList, newBoneList);
                //穿戴魂骨
                AddTaskNumForType(TaskType.EquipSoulBone, SoulboneMng.GetEquipedCount(), false);
                //指定位置穿戴魂骨数
                AddTaskNumForType(TaskType.EquipSoulBoneBySlot, SoulboneMng.GetEquipedCountBySlot(item.Bone.PartType), false, item.Bone.PartType);
                //komoelog
                int afterPower = heroInfo.GetBattlePower();
                KomoeEventLogEquitFlow(heroInfo.Id.ToString(), "", "", 2, operateType, item.Uid.ToString(), item.Uid.ToString(), beforePower, afterPower, afterPower - beforePower, 0, 0);//几件套信息取不到
            }
            else
            {
                result.ErrorCode = (int)ErrorCode.Fail;
                Logger.Log.Warn("player {0} equip soulBone to Hero {1} failed", Uid, msg.Hero);
            }

            Write(result);
        }


        /// <summary>
        /// 返回被卸下的所有装备用于同步物品信息 同时返回该有的奖励
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public void SoulBoneRevert(int heroId, RewardManager manager)
        {
            List<SoulBone> bones = SoulboneMng.GetEnhancedHeroBones(heroId);
            List<BaseItem> items = new List<BaseItem>();
            Bag_SoulBone bag = bagManager.SoulBoneBag;
            if (bones != null)
            {
                foreach (var bone in bones)
                {
                    SoulboneMng.TakeOff(bone);
                    //删掉放到rewardManager中
                    SoulBoneItemInfo itemInfo = SoulBoneLibrary.GetItemInfo(bone.AnimalType, bone.PartType, bone.MainNatureValue);
                    ItemBasicInfo baseInfo = new ItemBasicInfo((int) RewardType.SoulBone, itemInfo.Id, 1,
                        bone.AnimalType.ToString(), bone.PartType.ToString(), bone.MainNatureValue.ToString(),
                        bone.SpecId1.ToString(), bone.SpecId2.ToString(), bone.SpecId3.ToString(),
                        bone.SpecId4.ToString());
                    manager.AddReward(baseInfo);
                    SoulBoneItem soulBoneItem = bag.DelItem(bone.Uid);
                    soulBoneItem.Deleted = true;
                    items.Add(soulBoneItem);
                }
                //通知以便删除和添加
                SyncClientItemsInfo(items);
            }
        }

        /// <summary>
        /// 魂骨淬炼
        /// </summary>
        public void SoulBoneQuenching(MSG_GateZ_SOULBONE_QUENCHING msg)
        {
            MSG_ZGC_SOULBONE_QUENCHING response = new MSG_ZGC_SOULBONE_QUENCHING();

            Bag_SoulBone bag = bagManager.SoulBoneBag;
            SoulBoneItem mainItem = bag.GetSoulBoneItem(msg.MainBone);
            SoulBoneItem subItem = bag.GetSoulBoneItem(msg.SubBone);
            if (mainItem == null || subItem == null)
            {
                Logger.Log.Error($"SoulBoneQuenching have not this soul bone main {msg.MainBone} or sub {msg.SubBone}");
                response.Result = (int) ErrorCode.NoSuchItem;
                Write(response);
                return;
            }

            SoulBoneItemInfo mainItemInfo = SoulBoneLibrary.GetItemInfo(mainItem.Id);
            SoulBoneItemInfo subItemInfo = SoulBoneLibrary.GetItemInfo(subItem.Id);
            SoulBonePrefix mainSoulBonePrefix = SoulBoneLibrary.GetPrefix(mainItem.Bone.Prefix);
            SoulBonePrefix subSoulBonePrefix = SoulBoneLibrary.GetPrefix(mainItem.Bone.Prefix);

            if (mainItemInfo == null || subItemInfo == null || mainSoulBonePrefix == null || subSoulBonePrefix == null)
            {
                Logger.Log.Error($"SoulBoneQuenching have not this soul bone model main {mainItem.Id} or sub {subItem.Id}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (mainItemInfo.Job != subItemInfo.Job)
            {
                Logger.Log.Error($"SoulBoneQuenching job limit main {mainItem.Id} or sub {subItem.Id}");
                response.Result = (int)ErrorCode.JobNotPermission;
                Write(response);
                return;
            }

            List<int> mainSpecList = mainItem.Bone.GetSpecList();
            List<int> subSpecList = subItem.Bone.GetSpecList();
            Dictionary<CurrenciesType, int> costInts = new Dictionary<CurrenciesType, int>();

            if (msg.LockIndex.Count > 0)
            {
                if (msg.LockIndex.Count == mainSpecList.Count || msg.LockIndex.Count >= mainSoulBonePrefix.SpecialNum)
                {
                    response.Result = (int)ErrorCode.SoulBoneHaveNoSpecRepeat;
                    Write(response);
                    return;
                }

                ItemBasicInfo diamond = SoulBoneLibrary.GetLockCost(msg.LockIndex.Count);
                if (diamond == null)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                if (!CheckCoins((CurrenciesType)diamond.Id, diamond.Num))
                {
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }

                costInts.Add((CurrenciesType)diamond.Id, diamond.Num);
            }

            if (SoulBoneLibrary.SpecReplaceCost == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!CheckCoins((CurrenciesType)SoulBoneLibrary.SpecReplaceCost.Id, SoulBoneLibrary.SpecReplaceCost.Num))
            {
                response.Result = (int)ErrorCode.GoldNotEnough;
                Write(response);
                return;
            }

            costInts.Add((CurrenciesType)SoulBoneLibrary.SpecReplaceCost.Id, SoulBoneLibrary.SpecReplaceCost.Num);

            if (SoulBoneSpecReplace(mainItem.Bone, subItem.Bone, mainSoulBonePrefix, subSoulBonePrefix, msg.LockIndex.ToList(), costInts))
            {
                DelCoins(costInts, ConsumeWay.SmeltSoulBone, "");

                List<BaseItem> synItems = new List<BaseItem>() { mainItem, subItem };
                SyncClientItemsInfo(synItems);

                bag.SyncDbItemSpecInfo(mainItem);
                bag.SyncDbItemSpecInfo(subItem);
            }

            if (!subItemInfo.WearAble)
            {
                subItem.Deleted = true;
                BaseItem tempItem = DelItem2Bag(subItem, RewardType.SoulBone, 1, ConsumeWay.SoulBoneQuenching);
                if (tempItem != null)
                {
                    SyncClientItemInfo(tempItem);
                }
            }

            List<HeroInfo> changeList = new List<HeroInfo>();
            HeroInfo heroInfo = HeroMng.GetHeroInfo(mainItem.Bone.EquipedHeroId);
            if (heroInfo != null)
            {
                HeroMng.UpdateBattlePower(heroInfo);
                HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);
                changeList.Add(heroInfo);
            }

            heroInfo = HeroMng.GetHeroInfo(subItem.Bone.EquipedHeroId);
            if (heroInfo != null)
            {
                HeroMng.UpdateBattlePower(heroInfo);
                HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);
                changeList.Add(heroInfo);
            }

            SyncHeroChangeMessage(changeList);

            response.Result = (int) ErrorCode.Success;
            Write(response);
        }

        private bool SoulBoneSpecReplace(SoulBone mainBone, SoulBone subBone, SoulBonePrefix mainSoulBonePrefix, SoulBonePrefix subSoulBonePrefix, List<int> lockList, Dictionary<CurrenciesType, int> costInts)
        {
            server.TrackingLoggerMng.RecordSoulBoneQuenching(uid, "before", mainBone.Uid, mainBone.TypeId,
                mainBone.GetSpecList(), subBone.Uid, subBone.TypeId, subBone.GetSpecList(), server.Now());

            int pos = Math.Min(mainSoulBonePrefix.SpecialNum, subSoulBonePrefix.SpecialNum);
            List<int> replaceIndexList = new List<int>(pos);
            for (int i = 0; i < pos; i++)
            {
                replaceIndexList.Add(i + 1);
            }
            //komoelog
            List<int> mainFinalSpec = new List<int>();
            List<int> subFinalSpec = new List<int>();
            int oldScore = SoulBoneManager.GetSoulBoneScore(mainBone);

            bool changed = false;
            lockList.ForEach(x=>replaceIndexList.Remove(x));
            replaceIndexList.ForEach(id =>
            {
                int mainId = mainBone.GetSpecId(id);
                int subId = subBone.GetSpecId(id);

                if (mainId > 0 && subId > 0)
                {
                    changed = true;
                    mainBone.UpdateSpecId(id, subId);
                    subBone.UpdateSpecId(id, mainId);

                    mainFinalSpec.Add(subId);
                    subFinalSpec.Add(mainId);
                }
            });

            server.TrackingLoggerMng.RecordSoulBoneQuenching(uid, "after", mainBone.Uid, mainBone.TypeId,
                mainBone.GetSpecList(), subBone.Uid, subBone.TypeId, subBone.GetSpecList(), server.Now());

            //komoelog
            int newScore = SoulBoneManager.GetSoulBoneScore(mainBone);
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(costInts);
            KomoeEventLogSoulboneQuenching(mainBone.TypeId.ToString(), mainBone.Uid.ToString(), "", mainBone.Quality.ToString(), 
                mainSoulBonePrefix.Name.ToInt(), "", mainBone.AnimalType.ToString(), mainBone.TypeId.ToString(), string.Join("_", subFinalSpec), 
                string.Join("_", mainFinalSpec), oldScore, newScore, newScore-oldScore, consume);

            return changed;
        }
    }
}

using CommonUtility;
using DBUtility.Sql;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //背包
        private BagManager bagManager = null;

        public BagManager BagManager
        {
            get
            {
                return bagManager;
            }
        }

        public void InitBagManager()
        {
            bagManager = new BagManager(server.GameDBPool, server.GameRedis, this);
            bagManager.Init();
        }

        public void SyncClientItemInfo(params BaseItem[] items)
        {
            SyncClientItemsInfo(items.ToList());
        }

        public void SyncClientItemsInfo(List<BaseItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }
            if (items.Count > CONST.ITEM_UPDATE_MAX_COUNT)
            {
                int tempNum = 0;
                MSG_ZGC_BAG_UPDATE updateMsg = new MSG_ZGC_BAG_UPDATE();
                int usedBagSpace = this.BagManager.BagUsedSpace;

                foreach (var item in items)
                {
                    if (tempNum == 0)
                    {
                        updateMsg = new MSG_ZGC_BAG_UPDATE();
                        updateMsg.UsedBagSpace = usedBagSpace;
                    }
                    switch (item.MainType)
                    {
                        case MainType.SoulBone:
                            {
                                SoulBoneItem temp = item as SoulBoneItem;
                                updateMsg.SoulBones.Add(temp.GenerateMsg());
                            }
                            break;
                        case MainType.Consumable:
                        case MainType.Material:
                            {
                                NormalItem temp = item as NormalItem;
                                if (temp.SubType == 2)//如果是宝石
                                {
                                    updateMsg.Items.Add(EquipmentManager.InteceptJewel(item.Uid, temp.GenerateSyncMessage()));
                                }
                                else
                                {
                                    updateMsg.Items.Add(temp.GenerateSyncMessage());
                                }
                            }
                            break;
                        case MainType.HeroFragment:
                            {
                                HeroFragmentItem temp = item as HeroFragmentItem;
                                updateMsg.HeroFragments.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.SoulRing:
                            {
                                SoulRingItem temp = item as SoulRingItem;
                                updateMsg.SoulRings.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.Equip:
                            {
                                EquipmentItem temp = item as EquipmentItem;
                                updateMsg.Equips.Add(EquipmentManager.InteceptEquipment(temp.GenerateSyncMessage()));
                            }
                            break;
                        case MainType.ChatFrame:
                            {
                                ChatFrameItem temp = item as ChatFrameItem;
                                updateMsg.ChatFrames.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.HiddenWeapon:
                            {
                                HiddenWeaponItem temp = item as HiddenWeaponItem;
                                updateMsg.Equips.Add(HiddenWeaponManager.GetFinalHiddenWeaponItemInfo(temp, temp.GenerateSyncMessage()));
                            }
                            break;
                        default:
                            {
                                Log.Warn($"{item.MainType.ToString()} have not sync mathord please check it !");
                            }
                            break;
                    }
                    tempNum++;
                    if (tempNum == CONST.ITEM_UPDATE_MAX_COUNT)
                    {
                        Write(updateMsg);
                        tempNum = 0;
                    }
                }
                if (tempNum > 0)
                {
                    Write(updateMsg);
                }
            }
            else
            {
                MSG_ZGC_BAG_UPDATE updateMsg = new MSG_ZGC_BAG_UPDATE();
                updateMsg.UsedBagSpace = this.BagManager.BagUsedSpace;

                foreach (var item in items)
                {
                    switch (item.MainType)
                    {
                        case MainType.SoulBone:
                            {
                                SoulBoneItem temp = item as SoulBoneItem;
                                updateMsg.SoulBones.Add(temp.GenerateMsg());
                            }
                            break;
                        case MainType.Consumable:
                        case MainType.Material:
                            {
                                NormalItem temp = item as NormalItem;
                                if (temp.SubType == 2)//如果是宝石
                                {
                                    updateMsg.Items.Add(EquipmentManager.InteceptJewel(item.Uid, temp.GenerateSyncMessage()));
                                }
                                else
                                {
                                    updateMsg.Items.Add(temp.GenerateSyncMessage());
                                }
                            }
                            break;
                        case MainType.HeroFragment:
                            {
                                HeroFragmentItem temp = item as HeroFragmentItem;
                                updateMsg.HeroFragments.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.SoulRing:
                            {
                                SoulRingItem temp = item as SoulRingItem;
                                updateMsg.SoulRings.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.Equip:
                            {
                                EquipmentItem temp = item as EquipmentItem;
                                updateMsg.Equips.Add(EquipmentManager.InteceptEquipment(temp.GenerateSyncMessage()));
                            }
                            break;
                        case MainType.ChatFrame:
                            {
                                ChatFrameItem temp = item as ChatFrameItem;
                                updateMsg.ChatFrames.Add(temp.GenerateSyncMessage());
                            }
                            break;
                        case MainType.HiddenWeapon:
                            {
                                HiddenWeaponItem temp = item as HiddenWeaponItem;
                                updateMsg.Equips.Add(HiddenWeaponManager.GetFinalHiddenWeaponItemInfo(temp, temp.GenerateSyncMessage()));
                            }
                            break;
                        default:
                            {
                                Log.Warn($"{item.MainType.ToString()} have not sync mathord please check it !");
                            }
                            break;
                    }
                }
                Write(updateMsg);
            }
        }

        public void SyncClientBagInfo()
        {
            List<MSG_ZGC_BAG_SYNC> syncMsg = BagManager.GetBagSyncMsg();
            syncMsg.ForEach(item =>
            {
                Write(item);
            });
        }

        public List<BaseItem> AddItems(RewardManager rewards, ObtainWay way, string extraParam = "")
        {
            List<BaseItem> items = new List<BaseItem>();

            //消耗品
            AddItem2Bag(rewards, way, ref items, extraParam);
            //魂环
            AddSoulRing2Bag(rewards, way, ref items, extraParam);
            //魂骨
            AddSoulBone2Bag(rewards, way, ref items, extraParam);
            //装备
            AddEquipment2Bag(rewards, way, ref items, extraParam);
            //伙伴碎片
            AddHeroFragment2Bag(rewards, way, ref items, extraParam);
            //聊天气泡框
            AddChatFrame2Bag(rewards, way, ref items, extraParam);
            //暗器
            AddHiddenWeapon2Bag(rewards, way, ref items, extraParam);

            if (items.Count > 0)
            { 
                SyncClientItemsInfo(items);
            }
            return items;
        }

        public List<BaseItem> AddDefaultSoulRing(ObtainWay way, HeroInfo hero, bool spaceCheck = false)
        {
            List<BaseItem> items = new List<BaseItem>();
            HeroModel heroModel = HeroLibrary.GetHeroModel(hero.Id);
            //HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (heroModel.DefaultSoulRings.Count > 0)
            {
                int slot = 0;
                foreach (var kv in heroModel.DefaultSoulRings)
                {
                    Tuple<SoulRingItem, bool> item = this.bagManager.SoulRingBag.AddSoulRing(kv.Key, kv.Value, spaceCheck);
                    if (item != null)
                    {
                        items.Add(item.Item1);
                        if (item.Item2)
                        {
                            //RecordObtainLog(way, RewardType.SoulRing, kv.Key, 1, 1, kv.Value.ToString());
                            //BI 新增物品
                            KomoeEventLogItemFlow("add", "", kv.Key, MainType.SoulRing.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                            //拥有一个N年份魂环
                            AddTaskNumForType(TaskType.OwnSoulRingForYear, 1, true, item.Item1.Year);
                        }
                        SoulRingAdd(hero.Id, item.Item1, ++slot);
                        bagManager.SoulRingBag.SyncDbItemInfo(item.Item1);
                    }
                }
                hero.SetState(heroModel.DefaultSoulRingState);

                int maxTitle = GetHeroMaxTitle(hero);
                HeroMng.DefaultSoulRingAwaken(hero, maxTitle);
                //同步
                SyncDbUpdateHeroItem(hero);
                SyncHeroChangeMessage(new List<HeroInfo>() { hero });
                SyncClientItemsInfo(items);
            }
            //else
            //{
            //    HeroMng.UpdateBattlePower(hero.Id);
            //    //HeroMng.NotifyClientBattlePower(); //新英雄不在上阵队列里，需要计算但不需要通知
            //}
            return items;
        }

        public void AddItem2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            Dictionary<int, int> rewardItems = manager.GetRewardList(RewardType.NormalItem);
            if (rewardItems != null)
            {
                if (rewardItems != null)
                {
                    List<BaseItem> list;
                    foreach (var kv in rewardItems)
                    {
                        list = AddItem2Bag(MainType.Consumable, RewardType.NormalItem, kv.Key, kv.Value, way, extraParam);
                        if (list != null)
                        {
                            syncList.AddRange(list);
                        }
                    }
                }
            }
        }

        public void AddHeroFragment2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            Dictionary<int, int> rewardItems = manager.GetRewardList(RewardType.HeroFragment);
            if (rewardItems != null)
            {
                if (rewardItems != null)
                {
                    List<BaseItem> list;
                    foreach (var kv in rewardItems)
                    {
                        list = AddItem2Bag(MainType.HeroFragment, RewardType.HeroFragment, kv.Key, kv.Value, way, extraParam);
                        if (list != null)
                        {
                            syncList.AddRange(list);
                        }
                    }
                }
            }
        }

        public void AddChatFrame2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            Dictionary<int, int> rewardItems = manager.GetRewardList(RewardType.ChatFrame);
            if (rewardItems != null)
            {
                if (rewardItems != null)
                {
                    List<BaseItem> list;
                    foreach (var kv in rewardItems)
                    {
                        BaseItem item = BagManager.ChatFrameBag.GetItem(kv.Key);
                        if (item != null)
                        {
                            BagManager.ChatFrameBag.UpdateItemObtainInfo(item as ChatFrameItem);
                            list = new List<BaseItem>() { item };
                        }
                        else
                        {
                            list = AddItem2Bag(MainType.ChatFrame, RewardType.ChatFrame, kv.Key, kv.Value, way, extraParam);
                        }
                        if (list != null)
                        {
                            syncList.AddRange(list);
                        }
                    }
                }
            }
        }

        public List<BaseItem> AddItem2Bag(MainType mainType, RewardType rewardType, int id, int count, ObtainWay way, string param = "")
        {
            List<BaseItem> items = BagManager.AddItem(mainType, id, count);
            if (items != null)
            {
                SetHandinTaskNum(items, id);

                items.ForEach(item =>
                {
                    //RecordObtainLog(way, rewardType, id, item.PileNum, count);
                    //获取埋点
                    BIRecordObtainItem(rewardType, way, id, count, item.PileNum);
                    //BI 新增物品
                    KomoeEventLogItemFlow("add", "", item.Id, item.MainType.ToString(), count, Math.Max(0, item.PileNum - count), item.PileNum, (int)way, 0, 0, 0, 0);

                    if (item.MainType == MainType.Fashion)
                    {
                        FashionItem fashionItem = item as FashionItem;
                        if (fashionItem.Announce == 1)
                        {
                            List<string> list = new List<string>();
                            list.Add(Name);
                            list.Add(id.ToString());
                            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.GET_FASHION, list);
                        }
                    }
                });
                if (ThemePassLibrary.CheckIsThemePassExpItem(id))
                {
                    AddThemePassExp(id, count);
                }
            }
            return items;
        }

        /// <summary>
        /// 邮件，或者其他途径获取的固定年限的魂环
        /// </summary>
        public void AddSoulRing2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "", bool spaceCheck = true)
        {
            var soulRings = manager.GetRewardItemList(RewardType.SoulRing);
            var soulRingDic = manager.GetRewardList(RewardType.SoulRing);//用于检查是否已经获得过
            if (soulRingDic == null)
            {
                return;
            }
            //邮件相关的奖励
            foreach (var curr in soulRings)
            {
                if (!soulRingDic.ContainsKey(curr.Id))
                {
                    continue;
                }
                int year = 100;

                //带有年限的魂环
                if (curr.Attrs.Count > 0)
                {
                    if (!int.TryParse(curr.Attrs.First(), out year))
                    {
                        Log.Warn("player {0} AddSoulRing2Bag add id {1} num {2} error: TryParse year {3} error", uid, curr.Id, curr.Num, curr.Attrs.First());
                        year = 100;
                    }
                }
                else
                {
                    Log.Warn("player {0} AddSoulRing2Bag add id {1} num {2} error: Attrs count {3}", uid, curr.Id, curr.Num, curr.Attrs.Count);
                    year = 100;
                }

                SoulRingModel model = SoulRingLibrary.GetSoulRingMode(curr.Id);
                if (model == null)
                {
                    continue;
                }

                Dictionary<string, object> attrDic;
                for (int i = 0; i < curr.Num; i++)
                {
                    var item = this.bagManager.SoulRingBag.AddSoulRing(curr.Id, year, model, spaceCheck);
                    if (item != null)
                    {
                        if (item.Item2)//假如格子足够而没有进邮件
                        {
                            syncList.Add(item.Item1);
                            //RecordObtainLog(way, RewardType.SoulRing, curr.Id, 1, 1, year.ToString());
                            //获取埋点
                            BIRecordObtainItem(RewardType.SoulRing, way, curr.Id, 1, 1, year);
                            //BI 新增物品
                            KomoeEventLogItemFlow("add", "", curr.Id, MainType.SoulRing.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                            //komoelog
                            attrDic = new Dictionary<string, object>();
                            foreach (var kv in item.Item1.Model.UltAttrValue)
                            {
                                if (!attrDic.ContainsKey(kv.Key.ToString()))
                                {
                                    attrDic.Add(kv.Key.ToString(), kv.Value);
                                }
                            }
                            //item.Item1.Model.UltAttrValue.ForEach(x=>attrDic.Add(x.Key.ToString(), x.Value));
                            KomoeEventLogSoullinkResource(curr.Id.ToString(), item.Item1.Uid.ToString(), year.ToString(), attrDic, way.ToString(), extraParam);
                        }
                    }
                }

                //拥有一个N年份魂环
                AddTaskNumForType(TaskType.OwnSoulRingForYear, curr.Num, true, year);

                //获得一个指定年份的魂环
                List<int> paramList = new List<int>() { year };
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.GetHighYearSoulRing, 1, paramList);
            }
        }

        public void AddSoulBone2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            var soulBones = manager.GetRewardItemList(RewardType.SoulBone);
            var soulBoneDic = manager.GetRewardList(RewardType.SoulBone);//用于检查是否已经获得过
            Dictionary<string, object> additionDic;
            foreach (var curr in soulBones)
            {
                if (curr.Attrs.Count > 0 && soulBoneDic.ContainsKey(curr.Id))
                {
                    if (curr.Attrs.Count >= ItemBasicInfo.SoulBoneFixAttrCount)
                    {
                        var item = this.bagManager.SoulBoneBag.AddSoulBone(curr, false);
                        if (item != null)//假如格子足够而没有进邮件
                        {
                            //RecordObtainLog(way, RewardType.SoulBone, curr.Id, 1, 1, item.Bone.ToString());
                            //获取埋点
                            BIRecordObtainItem(RewardType.SoulBone, way, curr.Id, 1, 1);
                            //BI 新增物品
                            KomoeEventLogItemFlow("add", "", curr.Id, MainType.SoulBone.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);

                            //komoelog
                            SoulBoneItem soulBoneItem = item as SoulBoneItem;
                            KomoeLogRecordSoulboneResource(soulBoneItem, curr.Id.ToString(), item.Uid.ToString(), way, extraParam);                        

                            //玩家行为
                            RecordAction(ActionType.GotQualitySoulBone, item);

                            syncList.Add(item);
                        }
                    }
                }
            }
        }

        public void AddEquipment2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            var equipments = manager.GetRewardItemList(RewardType.Equip);
            var equipmentDic = manager.GetRewardList(RewardType.Equip);//用于检查是否已经获得过
            foreach (var curr in equipments)
            {
                if (curr.Attrs.Count == 0 && equipmentDic.ContainsKey(curr.Id))
                {
                    for (int i = 0; i < curr.Num; i++)
                    {
                        var item = this.bagManager.EquipBag.AddEquipment(curr.Id);
                        if (item != null)//假如格子足够而没有进邮件
                        {
                            //RecordObtainLog(way, RewardType.Equip, curr.Id, 1, 1, extraParam);
                            //获取埋点
                            BIRecordObtainItem(RewardType.Equip, way, curr.Id, 1, 1);
                            //BI 新增物品
                            KomoeEventLogItemFlow("add", "", curr.Id, MainType.Equip.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                            //komoelog
                            EquipmentItem equipItem = item as EquipmentItem;
                            KomoeEventLogEquipmentResource(curr.Id.ToString(), item.Uid.ToString(), equipItem.Model.Grade.ToString(), equipItem.Model.Color.ToString(), equipItem.Model.Star, equipItem.Model.Job.ToString(), equipItem.Model.Part.ToString(), equipItem.Model.Score.ToString(), way.ToString(), extraParam);

                            //玩家行为
                            RecordAction(ActionType.GotQualityEquipment, item);

                            //获得指定标准装备发称号卡
                            CheckGetTitleConditionEquipment(item);

                            syncList.Add(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通过宝箱开出魂环
        /// </summary>
        /// <param name="item">包厢</param>
        public void AddSoulRing2BagFromChest(NormalItem item, RewardManager manager, ObtainWay way)
        {
            var soulRings = manager.GetRewardList(RewardType.SoulRing);
            if (soulRings == null)
            {
                return;
            }

            int year = 0;
            Tuple<SoulRingItem, bool> tempItem;
            SoulRingModel model;
            List<BaseItem> items = new List<BaseItem>();
            foreach (var kv in soulRings)
            {
                model = SoulRingLibrary.GetSoulRingMode(kv.Key);
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < kv.Value; ++i)
                {
                    year = ScriptManager.SoulRing.GetYearByBox(model.Position, item.ItemModel.Quality);
                    tempItem = BagManager.SoulRingBag.AddSoulRing(kv.Key, year, model);
                    if (tempItem != null && tempItem.Item2)
                    {
                        //RecordObtainLog(way, RewardType.SoulRing, tempItem.Item1.Id, 1, 1, year.ToString());
                        //BI 新增物品
                        KomoeEventLogItemFlow("add", "", tempItem.Item1.Id, MainType.SoulRing.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                    }
                    items.Add(tempItem.Item1);
                }
            }

            SyncClientItemsInfo(items);

            //移除魂环奖励
            manager.RewardList.Remove(RewardType.SoulRing);
        }

        public void AddHiddenWeapon2Bag(RewardManager manager, ObtainWay way, ref List<BaseItem> syncList, string extraParam = "")
        {
            var weapons = manager.GetRewardItemList(RewardType.HiddenWeapon);
            var weaponDic = manager.GetRewardList(RewardType.HiddenWeapon);//用于检查是否已经获得过
            foreach (var curr in weapons)
            {
                if (curr.Attrs.Count >= 0 && weaponDic.ContainsKey(curr.Id))
                {
                    for (int i = 0; i < curr.Num; i++)
                    {
                        var item = this.bagManager.HiddenWeaponBag.AddHiddenWeapon(curr.Id);
                        if (item != null) //假如格子足够而没有进邮件
                        {
                            //RecordObtainLog(way, RewardType.HiddenWeapon, curr.Id, 1, 1, extraParam);
                            //获取埋点
                            BIRecordObtainItem(RewardType.HiddenWeapon, way, curr.Id, 1, 1);
                            //BI 新增物品
                            KomoeEventLogItemFlow("add", "", curr.Id, MainType.HiddenWeapon.ToString(), 1, 0, 1, (int)way, 0, 0, 0, 0);
                            //玩家行为
                            //RecordAction(ActionType.HiddenWeapon, item);

                            syncList.Add(item);
                        }
                    }
                }
            }
        }

        private void SetHandinTaskNum(List<BaseItem> list, int typeId)
        {
            int num = 0;
            foreach (var item in list)
            {
                num += item.PileNum;
            }
            AddTaskNumForType(TaskType.Handin, num, false, typeId);
        }

        public List<BaseItem> DelItem2Bag(Dictionary<BaseItem, int> costItems, RewardType rewardType, ConsumeWay way)
        {
            List<BaseItem> reItems = new List<BaseItem>();

            foreach (var item in costItems)
            {
                BaseItem it = DelItem2Bag(item.Key, rewardType, item.Value, way);
                if (it != null)
                {
                    reItems.Add(it);
                }
            }
            return reItems;
        }

        public BaseItem DelItem2Bag(BaseItem item, RewardType rewardType, int count, ConsumeWay way, string param = "")
        {
            int oldNum = item.PileNum;
            BaseItem it = BagManager.DelItem(item, count);
            if (it != null)
            {
                AddTaskNumForType(TaskType.Handin, it.PileNum, false, it.Id);

                //RecordConsumeLog(way, rewardType, item.Id, it.PileNum, count, param);
                //消耗埋点
                BIRecordConsumeItem(rewardType, way, item.Id, count, item.PileNum, item);
                //BI 消耗物品
                KomoeEventLogItemFlow("reduce", "", item.Id, item.MainType.ToString(), count, oldNum, it.PileNum, (int)way, 0, 0, 0, 0);
            }
            return it;
        }

        public void ItemUse(ulong uid, int num)
        {
            MSG_ZGC_ITEM_USE response = new MSG_ZGC_ITEM_USE();

            if (num <= 0)
            {
                Log.Warn($"BadPacket: player {Uid} use item uid {uid} got a wrong item num {num}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem item = BagManager.GetItem(uid);
            if (item == null)
            {
                Log.Warn("player {0} useitem fail, item {1} not exists.", Uid, uid);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            response.UidHigh = uid.GetHigh();
            response.UidLow = uid.GetLow();
            response.Result = (int)UseItem(item, num, response.Rewards);
            response.ItemId = item.Id;
            response.MainType = (int)item.MainType;
            Write(response);

            switch (item.MainType)
            {
                case MainType.FaceFrame:
                    MSG_ZGC_EQUIP_FACEFRAME responseFaceFrame = new MSG_ZGC_EQUIP_FACEFRAME();
                    responseFaceFrame.Result = response.Result;
                    responseFaceFrame.Id = item.Id;
                    Write(responseFaceFrame);
                    break;
                default:
                    break;
            }
        }

        public void ItemUseBatch(MSG_GateZ_ITEM_USE_BATCH msg)
        {
            MSG_ZGC_ITEM_USE_BATCH response = new MSG_ZGC_ITEM_USE_BATCH();

            List<NormalItem> items = new List<NormalItem>();
            Dictionary<ulong, int> useItemsNum = new Dictionary<ulong, int>();
            msg.Items.ForEach(x =>
            {
                NormalItem tempItem = bagManager.GetItem(MainType.Consumable, x.Uid) as NormalItem;
                if (tempItem != null && x.Num > 0)
                {
                    items.Add(tempItem);
                    useItemsNum.Add(x.Uid, x.Num);
                }

                ErrorCode errorCode = ErrorCode.Success;
                if (tempItem.ItemModel.SubType == (int)ConsumableType.SpaceTicket &&
                    !CheckBagSpaceIncreaseTicket(x.Num, ref errorCode))
                {
                    response.Result = (int)errorCode;
                    Write(response);
                    return;
                }
            });

            if (items.Count <= 0)
            {
                Log.Warn($"BadPacket: player {uid} use item got a wrong item num");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            items.ForEach(x => UseNormalItem(x, useItemsNum[x.Uid], response.Rewards));

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckBagSpaceIncreaseTicket(int num, ref ErrorCode errorCode)
        {
            //扩容达到上线
            if (BagSpace + num * BagLibrary.IncreaseSpacePerTicket > BagLibrary.BagMaxSpace)
            {
                Log.Warn($"BadPacket: player {Uid} max bag space cur space {BagSpace} increase num {num * BagLibrary.IncreaseSpacePerTicket}");
                errorCode = ErrorCode.MaxBagSpace;
                return false;
            }

            return true;
        }

        public ErrorCode UseItem(int id, int num)
        {
            BaseItem item = BagManager.GetItem(MainType.Consumable, id);
            if (item == null)
            {
                return ErrorCode.NotFoundItem;
            }
            else
            {
                return UseItem(item, num);
            }
        }

        private ErrorCode UseItem(BaseItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards = null)
        {
            ErrorCode errorCode = ErrorCode.Fail;

            if (!CheckItemInfo(item, num, ref errorCode))
            {
                return errorCode;
            }
            //到这里这个物品可以正常使用了，下面是具体作用
            switch (item.MainType)
            {
                case MainType.FaceFrame:  //使用不消耗，只是状态改变
                    Log.Write("player {0} Use FaceFrame Id:{1}", Uid, item.Id);
                    UseFaceFrame(item as FaceFrameItem);
                    break;
                case MainType.Fashion:  //使用不消耗，只是状态改变
                    Log.Write("player {0} Use Fashion Id:{1}", Uid, item.Id);
                    UseFashion(item as FashionItem);
                    break;
                case MainType.ChatFrame:
                    Log.Write("player {0} Use ChatFrame Id:{1}", Uid, item.Id);
                    UseChatFrame(item as ChatFrameItem);
                    break;
                case MainType.Consumable:
                    {
                        ItemModel model = BagLibrary.GetItemModel(item.Id);
                        if (model == null)
                        {
                            Log.Warn($"player {Uid} ItemUse have not model ItemModel item id {item.Id}");
                            return ErrorCode.Fail;
                        }
                        if (!model.IsUsable)
                        {
                            Log.Warn($"player {Uid} ItemUse item id {item.Id} IsUsable ");
                            return ErrorCode.CanNotUse;
                        }
                        if (Level < model.LevelLimit)
                        {
                            Log.Warn($"player {Uid} ItemUse item id {item.Id} levellimit ");
                            return ErrorCode.UseItemLevelLimt;
                        }
                        //月卡激活道具使用
                        if (model.MainType == MainType.Consumable && model.SubType == (int)ConsumableType.MonthCardActivate)
                        {
                            ErrorCode result = CheckCanActivateMonthCard(item.Id);
                            if (result != ErrorCode.Success)
                            {
                                return result;
                            }
                        }

                        //单次使用数量限制
                        if (model.UsableNum > 0)
                        {
                            num = Math.Min(num, model.UsableNum);
                        }
                        else
                        {
                            num = 1;
                        }

                        //扩容达到上线
                        if (model.SubType == (int)ConsumableType.SpaceTicket && !CheckBagSpaceIncreaseTicket(num, ref errorCode))
                        {
                            Log.Warn($"BadPacket: player {Uid} max bag space cur space {BagSpace} increase num {num * BagLibrary.IncreaseSpacePerTicket}");
                            return ErrorCode.MaxBagSpace;
                        }

                        //物品使用获取收益 礼包类
                        Log.Write("player {0} Use Id:{1} Num:{2}", Uid, item.Id, num);
                        UseNormalItem(item as NormalItem, num, rewards);
                    }
                    break;
                default:
                    break;
            }
            return ErrorCode.Success;
        }

        private void UseNormalItem(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            switch ((ConsumableType)item.SubType)
            {
                case ConsumableType.SpaceTicket:
                    {
                        int addSpace = num * BagLibrary.IncreaseSpacePerTicket;

                        BagSpace += addSpace;

                        //更新数据库
                        server.GameDBPool.Call(new QueryUpdateBagSpace(uid, BagSpace));

                        Write(new MSG_ZGC_BAGSPACE_INC() { BagSpace = BagSpace, Result = (int)ErrorCode.Success });
                    }
                    break;
                case ConsumableType.HeroTitleLevelBadge:
                case ConsumableType.CampBattleNatureItem:
                case ConsumableType.ChangeNameCard:
                    break;
                case ConsumableType.FireWork:
                    return;
                case ConsumableType.OnhookExpCard:
                case ConsumableType.OnhookGoldCard:
                    UseItemGetOnhookReward(item, num, rewards);
                    break;
                case ConsumableType.JigsawPuzzlePieces:
                    num = 1;
                    RandomPuzzleType();
                    return;
                case ConsumableType.TreasureMap:
                    num = 1;
                    RandomTreasureAndGoToDestination(item);
                    return;
                case ConsumableType.TitleCard:
                    ActivateTitle(item.Id);
                    break;
                case ConsumableType.EquipQualityByMaxQualityEquip:
                    UseEquipQualityByMaxQualityEquipBox(item, num, rewards);
                    break;
                case ConsumableType.SoulBoneQualityByMaxQualitySoulBone:
                    SoulBoneQualityByMaxQualitySoulBone(item, num, rewards);
                    break;
                case ConsumableType.MonthCardActivate:
                    ActivateMonthCard(item.Id);
                    break;
                case ConsumableType.ResetDiamondGiftDoubleFlag:
                    ResetDiamondGiftDoubleFlag();
                    break;
                case ConsumableType.XuanyuBySecretArea:
                    XuanyuBySecretArea(item, num, rewards);
                    break;
                case ConsumableType.ThemeFirework:
                    UseThemeFirework(item, num);
                    return;
                case ConsumableType.ChangeDiamondGiftRatio:
                    if (!ChangeDiamondGiftRatio(item))
                    {
                        return;
                    }
                    break;
                default:
                {
                        //默认包含box task
                        ItemUsingModel usingModel = BagLibrary.GetItemUsingModel(item.Id);
                        if (usingModel == null) return;

                        if (!string.IsNullOrEmpty(usingModel.Rewards))
                        {
                            RewardManager manager = new RewardManager();
                            RewardDropType dropType = RewardDropType.Independent;
                            switch (usingModel.Type)
                            {
                                case 2:
                                    dropType = RewardDropType.EntiretyNew;
                                    break;
                                case 1:
                                default:
                                    break;
                            }
                            List<ItemBasicInfo> rewardList = new List<ItemBasicInfo>();
                            RewardDropItemList rewardDrop = new RewardDropItemList(dropType, usingModel.Rewards);
                            for (int i = 0; i < num; i++)
                            {
                                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, HeroMng.GetFirstHeroJob());
                                rewardList.AddRange(items);
                            }
                            manager.AddReward(rewardList);
                            manager.BreakupRewards(true);
                            // 发放奖励
                            AddRewards(manager, ObtainWay.ItemUse, item.Id.ToString());

                            manager.GenerateRewardItemInfo(rewards);
                        }

                        if (!string.IsNullOrEmpty(usingModel.SoulBoneReward))
                        {
                            //产出魂骨
                            List<ItemBasicInfo> itemsList = SoulBoneManager.GenerateSoulboneReward(usingModel.SoulBoneReward, null, (int)HeroMng.GetFirstHeroJob(), num, true);
                            if (itemsList != null)
                            {
                                RewardManager manager = new RewardManager();
                                manager.AddReward(itemsList);
                                manager.BreakupRewards();

                                AddRewards(manager, ObtainWay.ItemUse);

                                manager.GenerateRewardItemInfo(rewards);
                            }
                        }

                        if (!string.IsNullOrEmpty(usingModel.SoulRingReward))
                        {
                            //魂环
                            List<ItemBasicInfo> itemsList = SoulRingManager.GenerateSoulRingReward(usingModel.SoulRingReward, num);
                            if (itemsList != null)
                            {
                                RewardManager manager = new RewardManager();
                                manager.AddReward(itemsList);
                                manager.BreakupRewards();

                                AddRewards(manager, ObtainWay.ItemUse);

                                manager.GenerateRewardItemInfo(rewards);
                            }
                        }
                    }
                    break;
            }

            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.ItemUse);

            if (baseItem != null)
            {
                SyncClientItemInfo(item);
                //使用消耗品
                AddTaskNumForType(TaskType.UseConsumable, 1, true, item.SubType);
            }
        }

        public void OpenChooseBox(ulong itemUid, MapField<int, GateZ_CHOOSE_BOX_ITEM> chooseItems)
        {
            MSG_ZGC_OPEN_CHOOSE_BOX response = new MSG_ZGC_OPEN_CHOOSE_BOX();

            BaseItem item = BagManager.GetItem(itemUid);
            if (item == null)
            {
                Log.Warn("player {0} open choose box fail, item {1} not exists.", Uid, itemUid);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            ItemModel model = BagLibrary.GetItemModel(item.Id);
            if (model == null)
            {
                Log.Warn("player {0} open choose box fail, item model {1} not exists.", Uid, item.Id);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            if (model.MainType != MainType.Consumable || model.SubType != (int)ConsumableType.ChooseBox)
            {
                Log.Warn("player {0} open choose box fail, item {1} type {2} subtype {3} error.", Uid, item.Id, model.MainType, model.SubType);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            if (!model.IsUsable)
            {
                Log.Warn($"player {Uid} open choose box fail, item id {item.Id} IsUsable ");
                response.Result = (int)ErrorCode.CanNotUse;
                Write(response);
                return;
            }

            if (Level < model.LevelLimit)
            {
                Log.Warn($"player {Uid} open choose box fail, item id {item.Id} levellimit ");
                response.Result = (int)ErrorCode.UseItemLevelLimt;
                Write(response);
                return;
            }


            //默认包含box task
            ItemChooseBoxModel boxModel = BagLibrary.GetChooseBox(item.Id);
            if (boxModel == null)
            {
                Log.Warn($"player {Uid} open choose box fail, item id {item.Id} not find box model ");
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            if (chooseItems.Count != boxModel.Grade.Count)
            {
                Log.Warn($"player {Uid} open choose box fail, item id {item.Id}  choose {chooseItems.Count} not {boxModel.Grade.Count} ");
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }
            foreach (var kv in chooseItems)
            {
                if (!boxModel.Grade.Contains(kv.Key))
                {
                    Log.Warn($"player {Uid} open choose box fail, item id {item.Id}  choose Grade not have {kv.Key} ");
                    response.Result = (int)ErrorCode.NotFoundItem;
                    Write(response);
                    return;
                }
            }


            List<ItemBasicInfo> rewardList = new List<ItemBasicInfo>();
            RewardManager manager = new RewardManager();
            RewardDropType dropType = RewardDropType.Fixed;

            foreach (var kv in chooseItems)
            {
                ItemChooseBoxRewardModel rewardModel = BagLibrary.GetChooseBoxReward(kv.Key);
                if (rewardModel == null)
                {
                    Log.Warn($"player {Uid} open choose box fail, not find box reward {kv.Key} model ");
                    response.Result = (int)ErrorCode.NotFoundItem;
                    Write(response);
                    return;
                }

                if (rewardModel.Count != kv.Value.List.Count)
                {
                    Log.Warn($"player {Uid} open choose box fail,item id {item.Id} box reward {kv.Key} choose {kv.Value.List.Count} ");
                    response.Result = (int)ErrorCode.NotFoundItem;
                    Write(response);
                    return;
                }

                foreach (var index in kv.Value.List)
                {
                    if (index.Key >= rewardModel.Rewards.Count)
                    {
                        Log.Warn($"player {Uid} open choose box fail,item id {item.Id} box reward {kv.Key} choose {index.Key} error {rewardModel.Count}  ");
                        response.Result = (int)ErrorCode.NotFoundItem;
                        Write(response);
                        return;
                    }

                    string reward = rewardModel.Rewards[index.Key];
                    if (!string.IsNullOrEmpty(reward))
                    {
                        //RewardDropItemList rewardDrop = new RewardDropItemList(dropType, reward);
                        //List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        //rewardList.AddRange(items);

                        manager.AddSimpleReward(reward);
                    }
                }
            }

            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.ItemUse);
            if (baseItem != null)
            {
                SyncClientItemInfo(item);
                //使用消耗品
                AddTaskNumForType(TaskType.UseConsumable, 1, true, model.SubType);
            }

            //manager.AddReward(rewardList);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.OpenChooseBox, item.Id.ToString());
            manager.GenerateRewardItemInfo(response.Rewards);

            response.ItemUid = itemUid;
            response.Result = (int)ErrorCode.Success;
            response.ItemId = item.Id;
            Write(response);
        }

        #region 随机装备和魂骨

        //1.取保底和商店相同取quality中大者
        //2.加上表配置的增加的quality
        private void UseEquipQualityByMaxQualityEquipBox(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            int addQuality;
            List<int> range;
            int qualityLimit;
            if (!GetParams(item.Id, out addQuality, out range, out qualityLimit)) return;

            int quality = ShopManager.GetHighestEquipQuality(false);
            quality = Math.Max(BagLibrary.RandomEquipBoxMinQuality, quality);

            quality += addQuality;
            //quality = Math.Min(TowerLibrary.EquipMaxQuality, quality);

            if (qualityLimit > 0)
            {
                quality = Math.Min(qualityLimit, quality);
            }

            List<CommonShopItemModel> items = RandSomeCommonShopItemByQuality(range[0], range[1], quality, num);
            EquipAndSoulBoneShopItemReward(items, rewards);
        }

        private void SoulBoneQualityByMaxQualitySoulBone(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            int addQuality;
            List<int> range;
            int qualityLimit;
            if (!GetParams(item.Id, out addQuality, out range, out qualityLimit)) return;

            int quality = ShopManager.GetHighestSoulBoneQuality(false);
            quality = Math.Max(BagLibrary.RandomSoulBoneBoxMinQuality, quality);

            quality += addQuality;
            //quality = Math.Min(TowerLibrary.SoulBoneMaxQuality, quality);

            if (qualityLimit > 0)
            {
                quality = Math.Min(qualityLimit, quality);
            }

            List<CommonShopItemModel> items = RandSomeCommonShopItemByQuality(range[0], range[1], quality, num);
            EquipAndSoulBoneShopItemReward(items, rewards);
        }

        private void XuanyuBySecretArea(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            if(num <=0) return;

            int tire = SecretAreaManager.GetTire();
            var model = BagLibrary.GetXuanyuWeightBySecretModel(tire + item.ItemModel.Data.GetInt("Value"));
            if (model == null)
            {
                Log.Error($"can not find XuanyuWeightBySecretModel model cur tire {tire} model id {item.Id}");
                return;
            }

            RewardManager manager = new RewardManager();

            List<ItemBasicInfo> rewardList = new List<ItemBasicInfo>();
            RewardDropItemList rewardDrop = new RewardDropItemList((RewardDropType)model.DropType, model.Rewards);
            for (int i = 0; i < num; i++)
            {
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, HeroMng.GetFirstHeroJob());
                rewardList.AddRange(items);
            }
            manager.AddReward(rewardList);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.ItemUse, item.Id.ToString());

            manager.GenerateRewardItemInfo(rewards);
        }

        private bool GetParams(int id, out int addQuality, out List<int> range, out int qualityLimit)
        {
            range = null;
            addQuality = 0;
            qualityLimit = 0;
            ItemUsingModel usingModel = BagLibrary.GetItemUsingModel(id);
            if (usingModel == null) return false;

            addQuality = usingModel.Data.GetInt("UpQuality");
            List<int> itemIdRange = usingModel.Data.GetString("ShopItemRnage").ToList(':');
            if (itemIdRange.Count != 2) return false;

            range = itemIdRange;

            qualityLimit = usingModel.Data.GetInt("QualityLimit");

            return true;
        }

        private List<CommonShopItemModel> RandSomeCommonShopItemByQuality(int minId, int maxId, int quality, int num)
        {
            List<CommonShopItemModel> models = CommonShopLibrary.GetQualityItems(minId, maxId, quality);
            if (models?.Count > 0)
            {
                List<CommonShopItemModel> randomItems = new List<CommonShopItemModel>();
                for (int i = 0; i < num; i++)
                {
                    int index = RAND.Range(0, models.Count - 1);
                    randomItems.Add(models[index]);
                }
                return randomItems;
            }

            return null;
        }

        private void EquipAndSoulBoneShopItemReward(List<CommonShopItemModel> items, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            if (items == null || items.Count <= 0) return;

            foreach (var shopItem in items)
            {
                RewardManager manager = GetSimpleReward(shopItem.Reward, ObtainWay.ItemUse, 1);
                manager.GenerateRewardItemInfo(rewards);
            }
        }

        #endregion 随机装备和魂骨


        private bool CheckItemInfo(BaseItem item, int num, ref ErrorCode errorCode)
        {
            if (num <= 0)
            {
                Log.Warn($"BadPacket: player {Uid} use item uid {uid} got a wrong item num {num}");
                errorCode = ErrorCode.Fail;
                return false;
            }

            if (item == null)
            {
                //没有这类物品
                Log.Warn("player {0} use a wrong item", Uid);
                errorCode = ErrorCode.NotFoundItem;
                return false;
            }
            else
            {
                if (item.PileNum == 0)
                {
                    //传入错误数量参数
                    Log.Warn($"BadPacket: player {Uid} use item uid {item.Uid} id {item.Id} got a wrong item PileNum {item.PileNum}");
                    errorCode = ErrorCode.NotFoundItem;
                    return false;
                }

                if (item.PileNum < num)
                {
                    Log.Warn($"player {Uid} use a wrong item uid {item.Uid} id {item.Id} PileNum {item.PileNum} num {num}");
                    //num = item.PileNum;
                    errorCode = ErrorCode.NotFoundItem;
                    return false;
                }

                //TODO 道具过期时间检测
            }

            return true;
        }

        public void ItemBuy(int id, int num)
        {
            MSG_ZGC_ITEM_BUY response = new MSG_ZGC_ITEM_BUY();
            response.Id = id;
            if (num <= 0)
            {
                //传入错误数量参数
                Log.Warn("BadPacket: player {0} buyItem {1} got a wrong item num {1}", Uid, id, num);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            ItemModel dataModel = BagLibrary.GetItemModel(id);
            if (dataModel == null)
            {
                Log.Warn("player {0} buyItem fail ,can not find item {1} in xml", Uid, id);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            else
            {
                string coinStr = dataModel.Data.GetString("Price");
                if (string.IsNullOrEmpty(coinStr))
                {
                    Log.Warn("player {0} buyItem fail ,can not find {1} price in xml", Uid, id);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                var coinCost = ParseReward(coinStr);
                int sellingPrice = coinCost.Item2 * num;
                int curCoin = GetCoins(coinCost.Item1);
                if (curCoin < sellingPrice)
                {
                    //传入错误数量参数
                    Log.Warn("player {0} buyItem {1} fail, coin {2} not enough, price {3}", Uid, id, curCoin, sellingPrice);
                    response.Result = (int)ErrorCode.DiamondNotEnough; //这里是钻石消耗
                    Write(response);
                    return;
                }
                else
                {
                    Log.Warn("TODO item buy");
                    ////扣除货币
                    //DelCoins((CurrenciesType)coinCost.Item1, sellingPrice, ConsumeWay.ItemBuy, id.ToString());
                    ////添加物品
                    //List<BaseItem> items = AddItem2Bag(dataModel.MainType, id, num, ObtainWay.ItemBuy);
                    //response.Result = (int)ErrorCode.Success;

                    //SyncClientItemsInfo(items);
                    //Write(response);
                }
                return;
            }
        }

        #region 出售相关逻辑

        /// <summary>
        /// 批量出售
        /// </summary>
        public void ItemSell(int mainType, RepeatedField<MSG_GateZ_ITEM_ID_NUM> sellItems)
        {
            MSG_ZGC_ITEM_SELL response = new MSG_ZGC_ITEM_SELL();
            response.Result = (int)ErrorCode.Fail;

            if (sellItems.Count > BagLibrary.BatchCountLimit)
            {
                Log.Warn($"player {Uid} mainType {mainType} item sell failed: sell totalCount {sellItems.Count} over limit");
                Write(response);
                return;
            }

            List<BaseItem> itemList = new List<BaseItem>();
            Dictionary<int, int> coinDic = new Dictionary<int, int>();

            foreach (var item in sellItems)
            {
                BaseItem it;
                string coinStr;
                ErrorCode errorCode = ItemSell((MainType)mainType, item.Uid, item.Num, out it, out coinStr);
                if (errorCode == ErrorCode.Success)
                {
                    //只要有一个售卖成功则返回结果就是成功，成功则返回售卖成功的uid
                    response.Uids.Add(new MSG_ZGC_ITEM_ID() { UidHigh = item.Uid.GetHigh(), UidLow = item.Uid.GetLow() });
                    response.Result = (int)errorCode;
                    if (it != null)
                    {
                        itemList.Add(it);
                    }
                    MergeItemSellRewards(coinStr, item.Num, coinDic);
                }
            }
            if (itemList.Count > 0)
            {
                SyncClientItemsInfo(itemList);
            }
            if (coinDic.Count > 0)
            {
                AddItemSellRewards(coinDic, response.Rewards);
            }

            Write(response);
        }

        private ErrorCode ItemSell(MainType mainType, ulong uid, int num, out BaseItem it, out string coinStr)
        {
            it = null;
            coinStr = string.Empty;

            if (num <= 0)
            {
                //传入错误数量参数
                Log.Warn("BadPacket: player {0} SellItems got a wrong item num {1}", Uid, num);
                return ErrorCode.Fail;
            }

            BaseItem item = BagManager.GetItem(mainType, uid);
            if (item == null)
            {
                //没有这类物品
                Log.Warn($"BadPacket: player {uid} SellItems got a wrong item uid{uid}");
                return ErrorCode.NotFoundItem;
            }

            if (item.PileNum < num)
            {
                //此类物品不可出售
                Log.Warn($"BadPacket: player {Uid} SellItems item uid {item.Uid} id {item.Id} num {num} is error, this item can not sell");
                return ErrorCode.ItemNotEnough;
            }

            switch (item.MainType)
            {
                case MainType.Material:
                case MainType.Consumable:
                    {
                        ItemModel model = BagLibrary.GetItemModel(item.Id);

                        if (model == null)
                        {
                            Log.Warn($"player {Uid} sellItem fail ,can not find item uid {item.Uid} id {item.Id} in xml");
                            return ErrorCode.Fail;
                        }

                        if (!model.IsSalable)
                        {
                            return ErrorCode.CanNotSell;
                        }

                        coinStr = model.Data.GetString("SellingPrice");
                        if (string.IsNullOrEmpty(coinStr))
                        {
                            return ErrorCode.CanNotSell;
                        }

                        return DelSellingItem(item, RewardType.NormalItem, num, out it);
                    }
                case MainType.Equip:
                    {
                        var model = EquipLibrary.GetEquipModel(item.Id);

                        if (model == null)
                        {
                            Log.Warn($"player {Uid} sellItem fail ,can not find item uid {item.Uid} id {item.Id} in xml");
                            return ErrorCode.Fail;
                        }

                        coinStr = model.Data.GetString("SellingPrice");
                        if (string.IsNullOrEmpty(coinStr))
                        {
                            return ErrorCode.CanNotSell;
                        }

                        return DelSellingItem(item, RewardType.Equip, num, out it);
                    }
                default:
                    {
                        //此类物品不可出售
                        Log.Warn($"BadPacket: player {Uid} SellItems got a wrong item uid {item.Uid} id {item.Id} this item can not sell");
                        return ErrorCode.Fail;
                    }
            }
        }

        private ErrorCode DelSellingItem(BaseItem item, RewardType rewardType, int num, out BaseItem it)
        {
            item = DelItem2Bag(item, rewardType, num, ConsumeWay.ItemSell);
            it = item;
            if (item == null)
            {
                return ErrorCode.NotFoundItem;
            }
            else
            {
                //SyncClientItemInfo(item);
                return ErrorCode.Success;
            }
        }


        private void MergeItemSellRewards(string coinStr, int num, Dictionary<int, int> coinDic)
        {
            var coins = StringSplit.GetKVPairs(coinStr);
            var rewardCoin = coins.First();

            if (!coinDic.ContainsKey(rewardCoin.Key))
            {
                coinDic.Add(rewardCoin.Key, rewardCoin.Value * num);
            }
            else
            {
                coinDic[rewardCoin.Key] += rewardCoin.Value * num;
            }
        }

        private void AddItemSellRewards(Dictionary<int, int> coinDic, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            foreach (var coin in coinDic)
            {
                //成功出售物品，获取货币
                AddCoins((CurrenciesType)coin.Key, coin.Value, ObtainWay.ItemSell);

                REWARD_ITEM_INFO rewardInfo = new REWARD_ITEM_INFO();
                rewardInfo.TypeId = coin.Key;
                rewardInfo.Num = coin.Value;
                rewards.Add(rewardInfo);
            }
        }
        #endregion

        //和成
        public void ItemCompose(int id, int num)
        {
            MSG_ZGC_ITEM_COMPOSE response = new MSG_ZGC_ITEM_COMPOSE();
            if (num <= 0)
            {
                //传入错误数量参数
                Log.Warn("BadPacket: player {0} ItemCompose {1} got a wrong item num {1}", Uid, id, num);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            ItemModel itemModel = BagLibrary.GetItemModel(id);
            ItemForgeModel model = BagLibrary.GetItemForgeModel(id);
            if (itemModel == null || model == null)
            {
                Log.Warn("player {0} ItemCompose fail ,can not find item {1} in xml", Uid, id);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //合成消耗的自生，需要配置在Material字段中，同时需要配置合成一个需要自身道具的数量Num
            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>();
            bool haveEnoughMaterial = CheckForgeCostMeterial(model, num, costItems);
            if (!haveEnoughMaterial)
            {
                Log.Warn("player {0} ItemCompose fail , item {1} forge material not enough", Uid, id);
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            var conis = StringSplit.GetKVPairs(model.Data.GetString("Price"), num);
            if (!CheckCoins(conis))
            {
                Log.Warn("player {0} ItemCompose fail , item {1} compose coins not enough", Uid, id);
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            int costNum = model.Num * num;
            BaseItem item = this.bagManager.NormalBag.GetItem(model.Id);
            if (item == null)
            {
                //没有这类物品
                Log.Warn($"BadPacket: player {uid} ItemCompose got no item id {model.Id}");
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            if (item.PileNum < costNum || !EquipmentManager.CheckXuanyuEnough(item.Uid, costNum) || costNum <= 0)
            {
                //此类物品数量不足或异常
                Log.Warn($"BadPacket: player {Uid} SellItems item uid {item.Uid} id {item.Id} num {num} is error, this item can not sell");
                response.Result = (int)ErrorCode.ItemNotEnough;//材料不够
                Write(response);
                return;
            }

            if (id == ShovelTreasureLibrary.HighTrerasureMap && !CheckLimitOpen(LimitType.TreasureMapCompose))
            {
                Log.Warn($"player {Uid} compose treasureMap failed: not open yet");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //扣除货币
            DelCoins(conis, ConsumeWay.Compose, id.ToString());

            //扣除道具
            List<BaseItem> reItem = DelItem2Bag(costItems, RewardType.NormalItem, ConsumeWay.Forge);
            SyncClientItemsInfo(reItem);

            int realNum = 0;
            if (model.Num > 0)
            {
                realNum = costNum / model.Num;
            }

            bool added = false;
            //添加物品
            List<BaseItem> items = new List<BaseItem>();
            List<BaseItem> xuanyuItems = new List<BaseItem>();

            //玄玉9级需要特殊处理， 随机从四种玄玉中选取一种
            ItemXuanyuModel xuanyuModel = EquipLibrary.GetXuanyuItem(id);
            if (xuanyuModel != null && xuanyuModel.Level - 1 == EquipLibrary.XuanyuBreakLevel)
            {
                Dictionary<int, int> addXuanyu = new Dictionary<int, int>();

                added = true;
                for (int i = 0; i < realNum; i++)
                {
                    var breakModel = EquipLibrary.GetXuanyuLevl9BreakModel();
                    if (breakModel != null)
                    {
                        if (!addXuanyu.ContainsKey(breakModel.Id))
                        {
                            addXuanyu.Add(breakModel.Id, 1);
                        }
                        else
                        {
                            addXuanyu[breakModel.Id] += 1;
                        }
                    }
                }

                addXuanyu.ForEach(x =>
                {
                    xuanyuItems.AddRange(AddItem2Bag(MainType.Material, RewardType.NormalItem, x.Key, x.Value, ObtainWay.Compose));
                    response.Rewards.Add(new REWARD_ITEM_INFO() { MainType = (int)RewardType.NormalItem, TypeId = x.Key, Num = x.Value});
                });
            }

            if(!added)
            {
                items.AddRange(AddItem2Bag(MainType.Material, RewardType.NormalItem, id, realNum, ObtainWay.Compose));
            }

            response.Result = (int)ErrorCode.Success;

            items.ForEach(x =>
            {
                response.Rewards.Add(new REWARD_ITEM_INFO() { MainType = (int)RewardType.NormalItem, TypeId = x.Id, Num = x.PileNum });
            });

            items.AddRange(xuanyuItems);
            SyncClientItemsInfo(items);
            Write(response);

            //注能石合成
            AddTaskNumForType(TaskType.ItemForge, realNum, true, new List<int>() { (int)itemModel.MainType, itemModel.SubType });
            AddPassCardTaskNum(TaskType.ItemForge, new int[] { (int)itemModel.MainType, itemModel.SubType }, new string[] { TaskParamType.TYPE, TaskParamType.SUB_TYPE });
            AddSchoolTaskNum(TaskType.ItemForge, new int[] { (int)itemModel.MainType, itemModel.SubType }, new string[] { TaskParamType.TYPE, TaskParamType.SUB_TYPE });
        }

        //打造
        public void ItemForge(MainType mainType, int id, int num)
        {
            MSG_ZGC_ITEM_FORGE response = new MSG_ZGC_ITEM_FORGE();
            if (num <= 0)
            {
                //传入错误数量参数
                Log.Warn("BadPacket: player {0} ItemCompose {1} got a wrong item num {1}", Uid, id, num);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
         
            ItemForgeModel model = BagLibrary.GetItemForgeModel(id);
            if (model == null)
            {
                Log.Warn("player {0} ItemCompose fail ,can not find item {1} in xml", Uid, id);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            switch (mainType)
            {
                case MainType.Material:
                case MainType.Consumable:
                    ItemForge(model, response, mainType, RewardType.NormalItem, id, num);
                    break;
                case MainType.Equip:
                    EquipmentModel equipmentModel = EquipLibrary.GetEquipModel(id);
                    if (equipmentModel == null)
                    {
                        Log.Warn("player {0} ItemCompose equipment fail ,can not find item {1} in xml", Uid, id);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    ItemForge(model, response, mainType, RewardType.Equip, id, num);
                    break;
                case MainType.HiddenWeapon:
                    HiddenWeaponModel hiddenWeaponModel = HiddenWeaponLibrary.GetHiddenWeaponModel(id);
                    if (hiddenWeaponModel == null)
                    {
                        Log.Warn("player {0} ItemCompose hidden weapon fail ,can not find item {1} in xml", Uid, id);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    ItemForge(model, response, mainType, RewardType.HiddenWeapon, id, num);
                    break;
            }

            Write(response);
        }

        private bool CheckForgeCostMeterial(ItemForgeModel model, int num, Dictionary<BaseItem, int> costItems)
        {
            bool haveEnoughMaterial = true;
            foreach (var kv in model.CostMeterial)
            {
                if (int.MaxValue / kv.Value < num)
                {
                    haveEnoughMaterial = false;
                    break;
                }
                int needNum = kv.Value * num;

                BaseItem item = this.bagManager.GetItem(MainType.Material, kv.Key);
                BaseItem consumeItem = this.bagManager.GetItem(MainType.Consumable, kv.Key);
                if ((item == null || item.PileNum < needNum) && (consumeItem == null || consumeItem.PileNum < needNum))
                {
                    haveEnoughMaterial = false;
                    break;
                }
                else
                {
                    if (item != null)
                    {
                        costItems.Add(item, needNum);
                    }
                    else if (consumeItem != null)
                    {
                        costItems.Add(consumeItem, needNum);
                    }
                }
            }

            return haveEnoughMaterial;
        }

        private void ItemForge(ItemForgeModel model, MSG_ZGC_ITEM_FORGE response, MainType mainType, RewardType rewardType, int id, int num)
        {
            var conis = StringSplit.GetKVPairs(model.Data.GetString("Price"), num);
            if (!CheckCoins(conis))
            {
                Log.Warn("player {0} ItemForge fail , item {1} forge coins not enough", Uid, id);
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>();
            bool haveEnoughMaterial = CheckForgeCostMeterial(model, num, costItems);
            if (!haveEnoughMaterial)
            {
                Log.Warn("player {0} ItemForge fail , item {1} forge material not enough", Uid, id);
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }
            
            //扣除货币
            DelCoins(conis, ConsumeWay.Forge, id.ToString());

            //扣除道具
            List<BaseItem> reItem = DelItem2Bag(costItems, rewardType, ConsumeWay.Forge);
            if (reItem != null)
            {
                SyncClientItemsInfo(reItem);
            }
            
            //添加物品
            List<BaseItem> items = new List<BaseItem>();
            switch (mainType)
            {
                case MainType.HiddenWeapon:
                    for (int i = 0; i < num; i++)
                    {
                        var item = this.bagManager.HiddenWeaponBag.AddHiddenWeapon(id);
                        if (item != null)//假如格子足够而没有进邮件
                        {
                            RecordObtainLog(ObtainWay.Forge, rewardType, id, 1, 1);
                            //获取埋点
                            BIRecordObtainItem(rewardType, ObtainWay.Forge, id, 1, 1);
                                //BI 新增物品
                                KomoeEventLogItemFlow("add", "", id, MainType.HiddenWeapon.ToString(), 1, 0, 1, (int)ObtainWay.Forge, 0, 0, 0, 0);
                            items.Add(item);
                        }
                    }
                    break;
                default:
                    List<BaseItem> normalItems = AddItem2Bag(mainType, rewardType, id, num, ObtainWay.Forge);
                    if (normalItems != null)
                    {
                        items.AddRange(normalItems);
                    }
                    break;
            }
            
            response.Result = (int)ErrorCode.Success;
            response.TypeId = id;
            if (items.Count > 0)
            {
                items.ForEach(x =>
                {
                    response.Rewards.Add(new REWARD_ITEM_INFO() { MainType = (int)rewardType, TypeId = x.Id, Num = x.PileNum });
                });

                SyncClientItemsInfo(items);

                var item = items.First();
                response.UidHigh = item.Uid.GetHigh();
                response.UidLow = item.Uid.GetLow();
            }
        }

        #region 分解

        public void ItemResolve(int mainType, ulong uid, int num)
        {
            MSG_ZGC_ITEM_RESOLVE response = new MSG_ZGC_ITEM_RESOLVE();
            response.Result = (int)ErrorCode.Fail;

            if (num <= 0)
            {
                Log.Warn("BadPacket: player {0} ItemResolve got a wrong item num {1}", Uid, num);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem item = BagManager.GetItem((MainType)mainType, uid);
            if (item == null)
            {
                //没有这类物品
                Log.Warn($"BadPacket: player {Uid} ItemResolve got a wrong item uid{uid}");
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }

            if (item.PileNum < num)
            {
                //此类物品不可出售
                Log.Warn($"BadPacket: player {Uid} ItemResolve item uid {item.Uid} id {item.Id} num {num} is error, this item can not sell");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            ItemResolveModel model = BagLibrary.GetItemResolveModel(item.Id);
            if (model == null)
            {
                if (item.MainType != MainType.SoulRing)
                {
                    Log.Warn($"player {Uid} ItemResolve fail ,can not find item uid {item.Uid} id {item.Id} in xml");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }

            var conis = StringSplit.GetKVPairs(model.Data.GetString("Price"), num);
            if (!CheckCoins(conis))
            {
                Log.Warn("player {0} ItemResolve fail , item {1} resolve coins not enough", Uid, uid);
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            switch (item.MainType)
            {
                case MainType.Material:
                    {
                        ItemModel itemModel = BagLibrary.GetItemModel(item.Id);
                        //不允许分解
                        if (!itemModel.IsResolve)
                        {
                            Log.Warn($"player {Uid} ItemResolve fail ,item can not resolve uid {item.Uid} id {item.Id}");
                            response.Result = (int)ErrorCode.CanNotResolve;
                            Write(response);
                            return;
                        }

                        //扣除货币
                        DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        response.Result = (int)ItemResolve(item, RewardType.NormalItem, num, model.Data.GetString("Result"), response.Rewards);
                    }
                    break;
                case MainType.HeroFragment:
                    {
                        HeroFragmentModel itemModel = BagLibrary.GetHeroFragmentModel(item.Id);

                        //扣除货币
                        DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        response.Result = (int)ItemResolve(item, RewardType.HeroFragment, num, model.Data.GetString("Result"), response.Rewards);
                    }
                    break;
                case MainType.SoulRing:
                    {
                        SoulRingItem soulRingItem = item as SoulRingItem;
                        SoulRingResolveModel resolveModel = BagLibrary.GetSoulRingResolveModel(soulRingItem.Year);

                        if (resolveModel != null)
                        {
                            conis = StringSplit.GetKVPairs(resolveModel.Data.GetString("Price"), num);
                            if (!CheckCoins(conis))
                            {
                                Log.Warn("player {0} ItemResolve fail , item {1} resolve coins not enough", Uid, uid);
                                response.Result = (int)ErrorCode.NoCoin;
                                Write(response);
                                return;
                            }

                            //扣除货币
                            DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                            //下发奖励
                            response.Result = (int)ItemResolve(item, RewardType.SoulRing, num, resolveModel.Result, response.Rewards);
                        }
                    }
                    break;
                case MainType.Equip:
                    {
                        //扣除货币
                        DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        response.Result = (int)ItemResolve(item, RewardType.Equip, num, model.Data.GetString("Result"), response.Rewards);

                        //随机奖励
                        ItemResolveRandomReward(model, num, item.Id, response.Rewards);
                    }
                    break;
                case MainType.HiddenWeapon:
                    {
                        //扣除货币
                        DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        response.Result = (int)ItemResolve(item, RewardType.HiddenWeapon, num, model.Data.GetString("Result"), response.Rewards);
                    }
                    break;
                default:
                    {
                        ItemModel itemModel = BagLibrary.GetItemModel(item.Id);
                        //不允许分解
                        if (!itemModel.IsResolve)
                        {
                            Log.Warn($"player {Uid} ItemResolve fail ,item can not resolve uid {item.Uid} id {item.Id}");
                            response.Result = (int)ErrorCode.CanNotResolve;
                            Write(response);
                            return;
                        }

                        //扣除货币
                        DelCoins(conis, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        response.Result = (int)ItemResolve(item, RewardType.NormalItem, num, model.Data.GetString("Result"), response.Rewards);
                    }
                    break;
            }

            Write(response);
        }

        private void ItemResolveRandomReward(ItemResolveModel model, int num, int id, RepeatedField<REWARD_ITEM_INFO> rewards, RewardManager manager = null)
        {
            string randomReward = model.Data.GetString("RandomReward");
            if (!string.IsNullOrEmpty(randomReward))
            {
                List<ItemBasicInfo> rewardList = new List<ItemBasicInfo>();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Independent, randomReward);
                for (int i = 0; i < num; i++)
                {
                    List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, HeroMng.GetFirstHeroJob());
                    rewardList.AddRange(items);
                }

                if (manager != null)
                {
                    manager.AddReward(rewardList);
                }
                else
                {
                    manager = new RewardManager();
                    manager.AddReward(rewardList);
                    manager.BreakupRewards();

                    // 发放奖励
                    AddRewards(manager, ObtainWay.BreakUp, id.ToString());

                    manager.GenerateRewardItemInfo(rewards);
                }
               
            }
        }

        private ErrorCode ItemResolve(BaseItem item, RewardType rewardType, int itemNum, string rewardStr, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            HiddenWeaponItem weaponItem = item as HiddenWeaponItem;

            //已经有了。更新数量
            item = DelItem2Bag(item, rewardType, itemNum, ConsumeWay.BreakUp);
            RewardManager manager = GetSimpleReward(rewardStr, ObtainWay.BreakUp, itemNum);

            if (item != null)
            {
                SyncClientItemInfo(item);

                if (weaponItem != null && weaponItem.Info.Level > 0)
                {
                    var upModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(weaponItem.Model.UpgradePool, weaponItem.Info.Level);
                    if (upModel != null)
                    {
                        AddCoins(CurrenciesType.gold, upModel.CostGold, ObtainWay.BreakUp);
                        manager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, upModel.ReturnGold));
                    }
                }
            }
            manager.GenerateRewardMsg(rewards);

            //分解物品
            AddTaskNumForType(TaskType.ItemResolve, 1, true, item.MainType);
            AddPassCardTaskNum(TaskType.ItemResolve, (int)item.MainType, TaskParamType.TYPE);

            return ErrorCode.Success;
        }

        public void ItemBatchResolve(int mainType, RepeatedField<GateZ_ITEM_RESOLVE> items)
        {
            MSG_ZGC_ITEM_BATCH_RESOLVE response = new MSG_ZGC_ITEM_BATCH_RESOLVE();
            response.Result = (int)ErrorCode.Fail;

            if (items.Count > BagLibrary.BatchCountLimit)
            {
                Log.Warn($"player {Uid} mainType {mainType} item batch resolve failed: batch resolve totalCount {items.Count} over limit");
                Write(response);
                return;
            }

            RewardManager manager = new RewardManager();
            List<BaseItem> itemList = new List<BaseItem>();
            foreach (var item in items)
            {
                BaseItem delItem;
                ErrorCode errorCode = ItemSingleResolve(mainType, item.Uid, item.Num, response.Rewards, manager, out delItem);
                if (errorCode == ErrorCode.Success)
                {
                    //返回成功分解的
                    response.Uids.Add(new MSG_ZGC_ITEM_ID() { UidHigh = item.Uid.GetHigh(), UidLow = item.Uid.GetLow() });
                    response.Result = (int)errorCode;
                    if (delItem != null)
                    {
                        itemList.Add(delItem);
                    }
                }
            }
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.BreakUp, "");
            manager.GenerateRewardMsg(response.Rewards);

            if (itemList.Count > 0)
            {
                SyncClientItemsInfo(itemList);
            }

            Write(response);
        }

        private ErrorCode ItemSingleResolve(int mainType, ulong uid, int num, RepeatedField<REWARD_ITEM_INFO> rewards, RewardManager manager, out BaseItem item)
        {
            ErrorCode result = ErrorCode.Fail;
            item = null;

            if (num <= 0)
            {
                Log.Warn("BadPacket: player {0} ItemResolve got a wrong item num {1}", Uid, num);
                return result;
            }

            item = BagManager.GetItem((MainType)mainType, uid);
            if (item == null)
            {
                //没有这类物品
                Log.Warn($"BadPacket: player {Uid} ItemResolve got a wrong item uid{uid}");
                result = ErrorCode.NotFoundItem;
                return result;
            }

            if (item.PileNum < num)
            {
                Log.Warn($"BadPacket: player {Uid} ItemResolve item uid {item.Uid} id {item.Id} num {num} is error, this item can not sell");
                result = ErrorCode.ItemNotEnough;
                return result;
            }

            ItemResolveModel model = BagLibrary.GetItemResolveModel(item.Id);
            if (model == null)
            {
                if (item.MainType != MainType.SoulRing)
                {
                    Log.Warn($"player {Uid} ItemResolve fail ,can not find item uid {item.Uid} id {item.Id} in xml");
                    result = ErrorCode.Fail;
                    return result;
                }
            }
            Dictionary<int, int> coins;
            if (model != null)
            {
                coins = StringSplit.GetKVPairs(model.Data.GetString("Price"), num);
                if (!CheckCoins(coins))
                {
                    Log.Warn($"player {Uid} ItemResolve item {item.Id} fail, coin not enough");
                    result = ErrorCode.NoCoin;
                    return result;
                }
            }
            else
            {
                coins = null;
            }

            switch (item.MainType)
            {
                case MainType.Material:
                    {
                        ItemModel itemModel = BagLibrary.GetItemModel(item.Id);
                        //不允许分解
                        if (!itemModel.IsResolve)
                        {
                            Log.Warn($"player {Uid} ItemResolve fail ,item can not resolve uid {item.Uid} id {item.Id}");
                            result = ErrorCode.CanNotResolve;
                            return result;
                        }

                        //扣除货币
                        DelCoins(coins, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        result = ItemResolveGenerateReward(item, RewardType.NormalItem, num, model.Data.GetString("Result"), rewards, manager);
                    }
                    break;
                case MainType.HeroFragment:
                    {
                        HeroFragmentModel itemModel = BagLibrary.GetHeroFragmentModel(item.Id);

                        //扣除货币
                        DelCoins(coins, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        result = ItemResolveGenerateReward(item, RewardType.HeroFragment, num, model.Data.GetString("Result"), rewards, manager);
                    }
                    break;
                case MainType.SoulRing:
                    {
                        SoulRingItem soulRingItem = item as SoulRingItem;
                        SoulRingResolveModel resolveModel = BagLibrary.GetSoulRingResolveModel(soulRingItem.Year);

                        if (resolveModel != null)
                        {
                            //coins = StringSplit.GetKVPairs(resolveModel.Data.GetString("Price"), num);
                            //if (!CheckCoins(coins))
                            //{
                            //    result = ErrorCode.NoCoin;
                            //    return result;
                            //}

                            //扣除货币
                            //DelCoins(coins, ConsumeWay.BreakUp, item.Id.ToString());

                            //下发奖励
                            result = ItemResolveGenerateReward(item, RewardType.SoulRing, num, resolveModel.Result, rewards, manager);
                        }
                    }
                    break;
                case MainType.Equip:
                    {
                        //扣除货币
                        DelCoins(coins, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        result = ItemResolveGenerateReward(item, RewardType.Equip, num, model.Data.GetString("Result"), rewards, manager);

                        //随机奖励
                        ItemResolveRandomReward(model, num, item.Id, rewards, manager);
                    }
                    break;
                case MainType.HiddenWeapon:
                    {
                        //扣除货币
                        DelCoins(coins, ConsumeWay.BreakUp, item.Id.ToString());

                        //下发奖励
                        result = ItemResolveGenerateReward(item, RewardType.HiddenWeapon, num, model.Data.GetString("Result"), rewards, manager);
                    }
                    break;
            }
            return result;
        }

        private ErrorCode ItemResolveGenerateReward(BaseItem item, RewardType rewardType, int itemNum, string rewardStr, RepeatedField<REWARD_ITEM_INFO> rewards, RewardManager manager)
        {
            rewardStr = SoulBoneLibrary.ReplaceSoulBone4AllRewards(rewardStr, HeroMng.GetFirstHeroJob());
            //TODO 获取奖励
            if (!string.IsNullOrEmpty(rewardStr))
            {
                manager.AddSimpleReward(rewardStr, itemNum);
            }
            //已经有了。更新数量
            item = DelItem2Bag(item, rewardType, itemNum, ConsumeWay.BreakUp);

            //分解物品
            AddTaskNumForType(TaskType.ItemResolve, 1, true, item.MainType);
            AddPassCardTaskNum(TaskType.ItemResolve, (int)item.MainType, TaskParamType.TYPE);

            return ErrorCode.Success;
        }
        #endregion

        #region 领取物品
        /// <summary>
        /// 领取物品
        /// </summary>
        public void ReceiveItem(int itemId)
        {
            MSG_ZGC_RECEIVE_ITEM response = new MSG_ZGC_RECEIVE_ITEM();

            ItemReceive itemReceive = BagLibrary.GetItemReceive(itemId);
            if (itemReceive == null)
            {
                Log.Warn($"player {Uid} receive item {itemId} failed : not find data in itemReceive xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (!CheckCanReceiveItem(itemReceive, response))
            {
                return;
            }

            if (!string.IsNullOrEmpty(itemReceive.Rewards))
            {
                RewardManager manager = GetSimpleReward(itemReceive.Rewards, ObtainWay.ReceiveItem);
                manager.GenerateRewardItemInfo(response.Rewards);

                AddPassCardTaskNumByMainType(itemReceive);

                response.Result = (int)ErrorCode.Success;
                Write(response);
            }
        }

        private bool CheckCanReceiveItem(ItemReceive itemReceive, MSG_ZGC_RECEIVE_ITEM response)
        {

            switch ((MainType)itemReceive.MainType)
            {
                case MainType.Consumable:
                    ItemModel item = BagLibrary.GetItemModel(itemReceive.Id);
                    if (item == null)
                    {
                        Log.Warn($"player {Uid} receive item {item.Id} failed : not find item in xml");
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return false;
                    }
                    if (!CheckCanReceiveNormalItem(item, response))
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        private bool CheckCanReceiveNormalItem(ItemModel item, MSG_ZGC_RECEIVE_ITEM response)
        {
            switch ((ConsumableType)item.SubType)
            {
                case ConsumableType.TreasureMap:
                    PassCardTaskItem taskItem = PassCardMng.GetPasscardTaskItemByType(TaskType.ReceiveTreasureMap);
                    if (taskItem == null)
                    {
                        Log.Warn($"player {Uid} receive treasureMap {item.Id} failed: not find passcard task");
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return false;
                    }
                    if (taskItem.CurNum >= 1)
                    {
                        Log.Warn($"player {Uid} receive treasureMap {item.Id} failed: already received");
                        response.Result = (int)ErrorCode.AlreadyGot;
                        Write(response);
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
        #endregion

        #region 道具兑换奖励
        public void ItemExchangeReward(int id, int num)
        {
            MSG_ZGC_ITEM_EXCHANGE_REWARD response = new MSG_ZGC_ITEM_EXCHANGE_REWARD();
            response.Id = id;

            ItemExchangeRewardModel model = BagLibrary.GetItemExchangeReward(id);
            if (model == null)
            {
                Log.Warn($"player {Uid} item exchange reward {id} failed : not find data in ItemExchangeReward xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            RechargeGiftModel activityModel = null;
            if (model.ActivityType != 0 && !RechargeLibrary.InitRechargeGiftTime((RechargeGiftType)model.ActivityType, ZoneServerApi.now, out activityModel))
            {
                Log.Warn($"player {Uid} item exchange reward {id} failed : activity {model.ActivityType} not open yet");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }

            if (activityModel != null && activityModel.SubType != model.Period)
            {
                Log.Warn($"player {Uid} item exchange reward {id} failed : cur period {activityModel.SubType}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Dictionary<BaseItem, int> costItemDic = new Dictionary<BaseItem, int>();
            //检查道具数量          
            string[] costItems = StringSplit.GetArray("|", model.CostItems);
            string[] itemArr;
            foreach (var itemStr in costItems)
            {
                itemArr = StringSplit.GetArray(":", itemStr);
                if (itemArr.Length != 3)
                {
                    Log.Warn($"player {Uid} item exchange reward {id} failed : cost item param error");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                int itemId;
                int.TryParse(itemArr[0], out itemId);
                int rewardType;
                int.TryParse(itemArr[1], out rewardType);
                int unitNum;
                int.TryParse(itemArr[2], out unitNum);

                if ((RewardType)rewardType != RewardType.NormalItem)
                {
                    Log.Warn($"player {Uid} item exchange reward {id} failed: cost item not normal item");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
                if (item == null || item.PileNum < unitNum * num)
                {
                    Log.Warn($"player {Uid} item exchange reward {id} failed: item {itemId} not enough");
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Write(response);
                    return;
                }
                costItemDic.Add(item, unitNum * num);
            }

            //常规道具兑换目前没有处理数据
            if (!CheckExchangeCountLimit((RechargeGiftType)model.ActivityType, model.Id, model.CountLimit, num))
            {
                Log.Warn($"player {Uid} item exchange reward {id} failed: count limit");
                response.Result = (int)ErrorCode.MaxCount;
                Write(response);
                return;
            }
            
            UpdateExchangeCount((RechargeGiftType)model.ActivityType, id, num, model.CountLimit);

            ConsumeWay consumeWay;
            ObtainWay obtainWay;
            switch ((RechargeGiftType)model.ActivityType)
            {           
                case RechargeGiftType.MidAutumn:
                    consumeWay = ConsumeWay.MidAutumn;
                    obtainWay = ObtainWay.MidAutumn;
                    break;
                default:
                    consumeWay = ConsumeWay.ItemExchange;
                    obtainWay = ObtainWay.ItemExchange;
                    break;
            }

            //扣除道具
            List<BaseItem> costItemList = DelItem2Bag(costItemDic, RewardType.NormalItem, consumeWay);
            SyncClientItemsInfo(costItemList);

            if (!string.IsNullOrEmpty(model.Reward))
            {
                //按有装备和魂骨生成奖励
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, model.Reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                for (int i = 0; i < num; i++)
                {
                    rewardItems.AddRange(items);
                }

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, obtainWay);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckExchangeCountLimit(RechargeGiftType activityType, int id, int countLimit, int num)
        {
            if (countLimit == 0)//没有数量限制
            {
                return true;
            }
            int count;
            switch (activityType)
            {
                case RechargeGiftType.MidAutumn:
                    MidAutumnMng.Info.ItemExchangeCounts.TryGetValue(id, out count);
                    return count + num <= countLimit;
                default:
                    //TODO
                    return false;
            }
        }

        private void UpdateExchangeCount(RechargeGiftType activityType, int id, int num, int countLimit)
        {
            if (countLimit == 0)
            {
                return;
            }
            switch (activityType)
            {
                case RechargeGiftType.MidAutumn:
                    MidAutumnMng.UpdateItemExchangeCount(id, num);
                    break;
                default:
                    break;
            }
        }
        #endregion

        //背包扩容
        public void BagSpaceInc(int num)
        {
            MSG_ZGC_BAGSPACE_INC response = new MSG_ZGC_BAGSPACE_INC();
            if (num <= 0)
            {
                Log.Warn($"BadPacket: player {Uid} BagSpaceInc got a wrong item num {num}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int currSpace = BagSpace;
            int afterSpace = currSpace + num;
            if (currSpace + num > BagLibrary.BagMaxSpace)
            {
                Log.Warn($"BadPacket: player {Uid} maxbagspace curr space {currSpace} increase num {num}");
                response.Result = (int)ErrorCode.MaxBagSpace;
                Write(response);
                return;
            }

            int costTicketNum = BagLibrary.GetBagSpaceIncreaseCostTicketNum(currSpace, num);
            BaseItem item = this.bagManager.NormalBag.GetItem(BagLibrary.BagTicketId);

            int needBuyTicketNum = 0;
            if (item == null)
            {
                needBuyTicketNum = costTicketNum;
            }
            else if (item.PileNum < costTicketNum)
            {
                needBuyTicketNum = costTicketNum - item.PileNum;
            }

            int costDiamond = needBuyTicketNum * BagLibrary.BagTicketPrice;

            int coin = this.GetCoins(CurrenciesType.diamond);
            if (coin < costDiamond)
            {
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }
            else
            {
                BaseItem reItem = null;

                //扩容券不够需要购买
                if (needBuyTicketNum > 0)
                {
                    DelCoins(CurrenciesType.diamond, costDiamond, ConsumeWay.BagSpaceInc, num.ToString());

                    if (item != null)
                    {
                        //扣除现有的扩容券
                        reItem = DelItem2Bag(item, RewardType.NormalItem, item.PileNum, ConsumeWay.BagSpaceInc);
                    }
                }
                else
                {
                    //扣除道具
                    reItem = DelItem2Bag(item, RewardType.NormalItem, costTicketNum, ConsumeWay.BagSpaceInc);
                }

                if (reItem != null)
                {
                    SyncClientItemInfo(reItem);
                }

                this.BagSpace += num;

                //更新数据库
                server.GameDBPool.Call(new QueryUpdateBagSpace(uid, this.BagSpace));

                response.Result = (int)ErrorCode.Success;
                response.BagSpace = this.BagSpace;

                Write(response);
            }
        }

        private Tuple<int, int> ParseReward(string coinStr)
        {
            string[] info = coinStr.Split(':');
            int ItemTypeId = int.Parse(info[0]);
            int count = int.Parse(info[1]);

            return Tuple.Create<int, int>(ItemTypeId, count);
        }

        /// <summary>
        /// 检测是否需要刷新
        /// </summary>
        public void CheckPastData()
        {
            BagManager.CheckPastData();
            SendFashionPastDataEmail();
            //SendChestOpen();
        }
    }
}

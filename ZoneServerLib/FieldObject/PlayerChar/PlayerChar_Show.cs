using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //查看信息
        internal void ShowPlayerInfo(int showPcUid, bool syncPlayer, int mainId)
        {
            //int mianId = BaseApi.GetMainIdByUid(showPcUid);
            if (mainId == 0 || mainId == server.MainId)
            {
                //说明是同服玩家
                //先找同服务器玩家
                PlayerChar showPlayer = server.PCManager.FindPc(showPcUid);
                if (showPlayer != null)
                {
                    List<int> heroIdList = showPlayer.HeroMng.GetAllHeroPosHeroId();

                    //找到玩家，直接获取信息
                    MSG_ZGC_SHOW_PLAYER response = showPlayer.GetShowPlayerMsg(heroIdList);
                    SendPlayerInfoMsg(response);

                    //发送给Relation 缓存信息
                    MSG_ZR_ADD_PLAYER_SHOW addMsg = new MSG_ZR_ADD_PLAYER_SHOW();
                    addMsg.Info = new MSG_ZRZ_RETURN_PLAYER_SHOW();
                    addMsg.Info.Result = (int)ErrorCode.Success;
                    addMsg.Info.PcUid = Uid;
                    addMsg.Info.ShowPcUid = showPcUid;
                    addMsg.Info.ShowInfo = response;
                    server.SendToRelation(addMsg, Uid);

                    if (syncPlayer)
                    {
                        //通知被查看者有人偷窥
                        MSG_ZGC_NOTIFY_PLAYER_SHOW notifyMsg = new MSG_ZGC_NOTIFY_PLAYER_SHOW();
                        notifyMsg.PlayerInfo = GetShowPlayerInfo();
                        showPlayer.Write(notifyMsg);
                    }
                }
                else
                {
                    showPlayer = server.PCManager.FindOfflinePc(showPcUid);
                    if (showPlayer != null)
                    {
                        List<int> heroIdList = showPlayer.HeroMng.GetAllHeroPosHeroId();
                        //找到离线玩家，直接获取信息
                        MSG_ZGC_SHOW_PLAYER response = showPlayer.GetShowPlayerMsg(heroIdList);
                        SendPlayerInfoMsg(response);

                        //发送给Relation 缓存信息
                        MSG_ZR_ADD_PLAYER_SHOW addMsg = new MSG_ZR_ADD_PLAYER_SHOW();
                        addMsg.Info = new MSG_ZRZ_RETURN_PLAYER_SHOW();
                        addMsg.Info.Result = (int)ErrorCode.Success;
                        addMsg.Info.PcUid = Uid;
                        addMsg.Info.ShowPcUid = showPcUid;
                        addMsg.Info.ShowInfo = response;
                        server.SendToRelation(addMsg, Uid);
                    }
                    else
                    {
                        //Log.WarnLine("player {0} show player info fail,can not find player {1}.", Uid, showPcUid);
                        //没找到玩家，去relation获取信息
                        MSG_ZR_GET_SHOW_PLAYER msg = new MSG_ZR_GET_SHOW_PLAYER();
                        msg.PcUid = Uid;
                        msg.ShowPcUid = showPcUid;
                        server.SendToRelation(msg, Uid);
                    }
                }
            }
            else
            {
                //不同服玩家，去relation获取信息
                MSG_ZR_GET_CROSS_SHOW_PLAYER msg = new MSG_ZR_GET_CROSS_SHOW_PLAYER();
                msg.PcUid = Uid;
                msg.ShowPcUid = showPcUid;
                msg.MainId = mainId;
                server.SendToRelation(msg, Uid);
            }
        }

        public void SendPlayerInfoMsg(MSG_ZGC_SHOW_PLAYER msg)
        {
            MSG_ZGC_SHOW_PLAYER msg1 = new MSG_ZGC_SHOW_PLAYER();
            msg1.Result = msg.Result;
            msg1.PlayerInfo = msg.PlayerInfo;
            msg1.Equips.AddRange(msg.Equips);
            msg1.IsEnd = false;
            Write(msg1);

            MSG_ZGC_SHOW_PLAYER msg2 = new MSG_ZGC_SHOW_PLAYER();
            msg2.Result = msg.Result;
            msg2.HeroList.AddRange(msg.HeroList);
            msg2.IsEnd = false;
            Write(msg2);

            MSG_ZGC_SHOW_PLAYER msg3 = new MSG_ZGC_SHOW_PLAYER();
            msg3.Result = msg.Result;
            msg3.SoulRings.AddRange(msg.SoulRings);
            msg3.IsEnd = false;
            Write(msg3);

            MSG_ZGC_SHOW_PLAYER msg4 = new MSG_ZGC_SHOW_PLAYER();
            msg4.Result = msg.Result;
            msg4.SoulBones.AddRange(msg.SoulBones);
            msg4.IsEnd = true;
            Write(msg4);
        }

        public MSG_ZGC_SHOW_PLAYER GetShowPlayerMsg(List<int> heroIdList)
        {
            MSG_ZGC_SHOW_PLAYER response = new MSG_ZGC_SHOW_PLAYER();
            response.Result = (int)ErrorCode.Success;
            //人物信息
            response.PlayerInfo = GetShowPlayerInfo();
            //伙伴信息
            foreach (var hero in heroIdList)
            {
                HeroInfo info = HeroMng.GetHeroInfo(hero);
                if (info != null)
                {
                    //伙伴信息
                    response.HeroList.Add(GetHeroMessage(info));

                    //魂环
                    Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(hero);
                    if (soulRingDic != null)
                    {
                        //有魂环
                        foreach (var soulRing in soulRingDic)
                        {
                            try
                            {
                                response.SoulRings.Add(soulRing.Value.GenerateSyncShowMessage());
                            }
                            catch (Exception e)
                            {
                                //没找到魂环信息
                                Log.WarnLine("player {0} get show player info fail,can not find soulBone {1}, {2}.", Uid, soulRing.Value.Uid, e);
                            }
                        }
                    }

                    //魂骨
                    List<SoulBone> soulBoneLsit = SoulboneMng.GetEnhancedHeroBones(hero);
                    if (soulBoneLsit != null)
                    {
                        //有魂骨
                        foreach (var soulBone in soulBoneLsit)
                        {
                            try
                            {
                                SoulBoneItem item = bagManager.SoulBoneBag.GetItem(soulBone.Uid) as SoulBoneItem;
                                response.SoulBones.Add(item.GenerateShowMsg());
                            }
                            catch (Exception e)
                            {
                                //没找到魂骨信息
                                Log.WarnLine("player {0} get show player info fail,can not find soulBone {1}, {2}.", Uid, soulBone.Uid, e);
                            }
                        }
                    }

                    //装备
                    List<EquipmentItem> equipments = EquipmentManager.GetAllEquipedEquipments(hero);
                    if (equipments != null)
                    {
                        //有装备
                        foreach (var item in equipments)
                        {
                            try
                            {
                                response.Equips.Add(EquipmentManager.InteceptEquipment(item.GenerateSyncShowMessage()));
                            }
                            catch (Exception e)
                            {
                                //没找到装备信息
                                Log.WarnLine("player {0} get show player info fail,can not find equipment {1}, {2}.", Uid, item.Uid, e);
                            }
                        }
                    }

                    //暗器
                    HiddenWeaponItem hiddenWeaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(hero);
                    if (hiddenWeaponItem != null)
                    {
                        try
                        {
                            response.Equips.Add(HiddenWeaponManager.GetFinalHiddenWeaponItemInfo(hiddenWeaponItem, hiddenWeaponItem.GenerateSyncShowMessage()));
                        }
                        catch (Exception e)
                        {
                            //没找到装备信息
                            Log.WarnLine("player {0} get show player info fail,can not find hidden weapon {1}, {2}.", Uid, hiddenWeaponItem.Uid, e);
                        }
                    }

                    //总战力
                    response.PlayerInfo.Power += info.GetBattlePower();
                }
                else
                {
                    //没找到伙伴信息
                    Log.WarnLine("player {0} get hero info fail,can not find hero {1}.", Uid, hero);
                }
            }
            return response;
        }

        private SHOW_PLAYER_INFO GetShowPlayerInfo()
        {
            SHOW_PLAYER_INFO info = new SHOW_PLAYER_INFO();
            info.Uid = Uid;
            info.Name = Name;
            info.Level = Level;
            info.Job = (int)Job;
            info.Sex = Sex;
            info.Camp = (int)Camp;
            info.FaceIcon = GetFaceFrame();
            info.ShowFaceJpg = ShowDIYIcon;
            info.MainTask = MainTaskId;
            info.GodType = GodType;
            //info.Power = BattlePower;
            //info.FashionId = fashion;                              
            //info.FamilyName = 11;                          
            //info.LadderLevel = 12;                            
            //info.Title = 13;									
            return info;
        }


        //public void LoadSpaceInfo()
        //{
        //    OperateLoadCharacterSpaceInfo operate = new OperateLoadCharacterSpaceInfo(Uid);
        //    server.Redis.Call(operate, result =>
        //    {
        //        if ((int)result == 1)
        //        {
        //            if (operate.Info == null)
        //            {
        //                //没找到对应id的信息
        //                Log.ErrorLine("player {0} load player space info from redis fail", Uid);
        //            }
        //            Signature = operate.Info.Signature;
        //            SpaceSex = operate.Info.SpaceSex;
        //            Birthday = operate.Info.Birthday;
        //            ShowVoice = operate.Info.ShowVoice;

        //            PopScore = operate.Info.PopScore;
        //            HighestPopScore = operate.Info.HighestPopScore;
        //            CheckTitles();
        //            geography.GeoHashStr = operate.Info.GeoHash;
        //            geography.Latitude = operate.Info.Latitude;
        //            geography.Longitude = operate.Info.Longitude;

        //            WNum = operate.Info.WNum;
        //            QNum = operate.Info.QNum;

        //            SHOW_SPACEINFO msg = GetSpaceShowInfo();
        //            Write(msg);
        //        }
        //    });
        //}

        //public void SendSomeShowMsg()
        //{
        //    MSG_ZGC_UPDATE_SOME_SHOW msg = new MSG_ZGC_UPDATE_SOME_SHOW();
        //    //msg.LadderHistoryMaxScore = GetLadderHistoryMaxScore();
        //    msg.LadderTotalWinNum = GetCounterValue(CounterType.LadderTotalWinNum);

        //    OperateLoadCharacterSpaceInfo operate = new OperateLoadCharacterSpaceInfo(Uid);
        //    server.Redis.Call(operate, result =>
        //    {
        //        if ((int)result == 1)
        //        {
        //            if (operate.Info == null)
        //            {
        //                //没找到对应id的信息
        //                Log.ErrorLine("player {0} load player space info from redis fail", Uid);
        //            }
        //            Signature = operate.Info.Signature;
        //            SpaceSex = operate.Info.SpaceSex;
        //            Birthday = operate.Info.Birthday;
        //            ShowVoice = operate.Info.ShowVoice;

        //            PopScore = operate.Info.PopScore;
        //            HighestPopScore = operate.Info.HighestPopScore;

        //            geography.GeoHashStr = operate.Info.GeoHash;
        //            geography.Latitude = operate.Info.Latitude;
        //            geography.Longitude = operate.Info.Longitude;

        //            if (HighestPopScore < PopScore)
        //            {
        //                HighestPopScore = PopScore;
        //                CheckTitles();
        //                server.Redis.Call(new OperateSetHighestPopScore(Uid, HighestPopScore));
        //            }

        //            msg.HighestPopScore = HighestPopScore;
        //            msg.PopScore = PopScore;
        //            msg.GiftMsg = operate.GiftMsg;
        //            msg.Sex = SpaceSex;
        //            Write(msg);
        //        }
        //    });

        //}

        //internal void ShowCharacterInfo(int characterId)
        //{
        //    //Log.Warn("player {0} show character {1} info", Uid, characterId);
        //    string strCharacterID = characterId.ToString();

        //    if (Uid == characterId)
        //    {
        //        SendSomeShowMsg();
        //    }
        //    else
        //    {
        //        OperateCharacterShow opreate = new OperateCharacterShow(characterId);
        //        server.Redis.Call(opreate, (ret) =>
        //        {
        //            MSG_ZGC_SHOW_CHARACTER response = new MSG_ZGC_SHOW_CHARACTER();
        //            if (opreate.Info == null)
        //            {
        //                response.Result = (int)ErrorCode.Fail;
        //                //没找到对应id的信息
        //                Log.ErrorLine("player {0} show character Id {1}", Uid, characterId);
        //            }
        //            else
        //            {
        //                response.Result = (int)ErrorCode.Success;
        //                response.CharacterShowInfo = new SHOW_CHARACTERINFO();
        //                response.CharacterShowInfo.Name = opreate.Info.Name;
        //                response.CharacterShowInfo.FaceIcon = opreate.Info.FaceIcon;
        //                response.CharacterShowInfo.FaceFrame = opreate.Info.FaceFrame;
        //                response.CharacterShowInfo.ShowFaceJpg = opreate.Info.ShowFaceJpg;

        //                response.CharacterShowInfo.Level = opreate.Info.Level;
        //                response.CharacterShowInfo.Exp = opreate.Info.Exp;
        //                response.CharacterShowInfo.LadderScore = opreate.Info.LadderScore;
        //                response.CharacterShowInfo.LadderLevel = opreate.Info.LadderLevel;
        //                response.CharacterShowInfo.LadderHistoryMaxScore = opreate.Info.LadderHistoryMaxScore;
        //                response.CharacterShowInfo.LadderTotalWinNum = opreate.Info.LadderTotalWinNum;

        //                response.CharacterShowInfo.CurQueueName = opreate.Info.CurQueueName;
        //                response.CharacterShowInfo.HeroCount = opreate.Info.HeroCount;
        //                response.CharacterShowInfo.HeroSkinCount = opreate.Info.SkinCount;
        //                response.CharacterShowInfo.FashionCount = opreate.Info.FashionCount;

        //                response.CharacterShowInfo.Title = opreate.Info.Title;
        //                response.CharacterShowInfo.FamilyName = opreate.Info.FamilyName;

        //                response.SpaceShowInfo = new SHOW_SPACEINFO();
        //                response.SpaceShowInfo.Signature = opreate.Info.Signature;
        //                response.SpaceShowInfo.Sex = opreate.Info.SpaceSex;
        //                response.SpaceShowInfo.Birthday = opreate.Info.Birthday;
        //                response.SpaceShowInfo.PopScore = opreate.Info.PopScore;
        //                response.SpaceShowInfo.HighestPopScore = opreate.Info.HighestPopScore;
        //                response.SpaceShowInfo.Title = opreate.Info.CurTitle;

        //                if (response.SpaceShowInfo.HighestPopScore < response.SpaceShowInfo.PopScore)
        //                {
        //                    response.SpaceShowInfo.HighestPopScore = response.SpaceShowInfo.PopScore;
        //                    //PlayerChar player = server.PCManager.FindPc(characterId);
        //                    //if (player != null)
        //                    //{
        //                    //    player.CheckTitles();
        //                    //}
        //                    //else
        //                    //{
        //                    //    CheckTitles(characterId);
        //                    //}
        //                    server.Redis.Call(new OperateSetHighestPopScore(characterId, response.SpaceShowInfo.HighestPopScore));
        //                }
        //                response.SpaceShowInfo.ShowVoice = opreate.Info.ShowVoice;
        //                response.SpaceShowInfo.Latitude = opreate.Info.Latitude;
        //                response.SpaceShowInfo.Longitude = opreate.Info.Longitude;

        //                response.FashionIds.Add(opreate.Info.Weapon);
        //                response.FashionIds.Add(opreate.Info.Head);
        //                response.FashionIds.Add(opreate.Info.Face);
        //                response.FashionIds.Add(opreate.Info.Clothes);
        //                response.FashionIds.Add(opreate.Info.Back);
        //                response.Result = (int)ErrorCode.Success;
        //            }
        //            Write(response);
        //        });
        //    }

        //}

        //internal void ShowFaceIcon(int faceIconId)
        //{
        //    Log.Warn("player {0} change face icon {1} to {2}", Uid, Icon, faceIconId);
        //    Icon = faceIconId;
        //    ShowFaceJpg = false;
        //    //更新到数据库
        //    string tableName = "character";
        //    server.GameDBPool.Call(new QuerySetFaceIcon(uid, tableName, Icon));
        //    //更新到redis
        //    server.Redis.Call(new OperateSetFaceIcon(uid, Icon, ShowFaceJpg));

        //    MSG_ZGC_SHOW_FACEICON response = new MSG_ZGC_SHOW_FACEICON();
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}


        //internal void SetShowFaceJpg(bool show)
        //{
        //    if (ShowFaceJpg != show)
        //    {
        //        ShowFaceJpg = show;
        //        //更新到数据库
        //        string tableName = "character";
        //        server.GameDBPool.Call(new QuerySetShowFaceJpg(uid, tableName, ShowFaceJpg));
        //        //更新到redis
        //        server.Redis.Call(new OperateSetFaceIcon(uid, Icon, ShowFaceJpg));
        //    }

        //    MSG_ZGC_SHOW_FACEJPG response = new MSG_ZGC_SHOW_FACEJPG();
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //internal void SetShowVoice(bool show)
        //{
        //    if (showVoice != show)
        //    {
        //        ShowVoice = show;
        //        ////更新到数据库
        //        //string tableName = server.DB.GetTableName("character", uid, DBTableParamType.Character);
        //        //server.DB.Call(new QuerySetShowVoice(uid, tableName, ShowVoice), tableName);
        //        //更新到redis
        //        server.Redis.Call(new OperateSetVoice(uid, ShowVoice));
        //    }

        //    MSG_ZGC_SHOW_VOICE response = new MSG_ZGC_SHOW_VOICE();
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //public void GetGiftRecord(int page)
        //{
        //    MSG_ZGC_GET_GIFTRECORD response = new MSG_ZGC_GET_GIFTRECORD();
        //    OperateGetGiftRecord operate = new OperateGetGiftRecord(Uid, page, BagLibrary.CountPerPage);
        //    server.Redis.Call(operate, ret =>
        //     {
        //         if ((int)ret == 1)
        //         {
        //             if (operate.GiftRecords != null && operate.GiftRecords.Length > 0)
        //             {
        //                 foreach (var item in operate.GiftRecords)
        //                 {
        //                     response.GiftRecords.Add(item);
        //                 }
        //             }
        //             response.Result = (int)ErrorCode.Success;
        //         }
        //         else
        //         {
        //             response.Result = (int)ErrorCode.Fail;
        //         }
        //         Write(response);
        //         return;
        //     });
        //}

        //public void PresentGift(int characterId, int itemId, int num)
        //{
        //    Log.Debug("TODO PresentGift");

        //    MSG_ZGC_PRESENT_GIFT response = new MSG_ZGC_PRESENT_GIFT();
        //    response.Id = itemId;
        //    response.Num = num;
        //    response.TargetId = characterId;

        //    if (num <= 0)
        //    {
        //        Log.Error("Bad packet:player {0} PresentGift got an wrong num{1}", Uid, num);
        //        return;
        //    }
        //    else
        //    {
        //        Data itemSaleData = DataListManager.inst.GetData("ItemsSale", itemId);
        //        if (itemSaleData != null)
        //        {
        //            string itemSalePrice = itemSaleData.GetString("Price");
        //            string[] split = itemSalePrice.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        //            CurrenciesType coinType = CurrenciesType.diamond;
        //            int coinCost = 0;
        //            if (split.Length > 1)
        //            {
        //                coinType = (CurrenciesType)int.Parse(split[0]);
        //                coinCost = int.Parse(split[1]);
        //            }

        //            int curCoinCount = 0;
        //            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
        //            if (item != null)
        //            {
        //                if (item.PileNum > 0)
        //                {
        //                    int itemUseCount = num;
        //                    int deltaCount = num - item.PileNum;
        //                    if (deltaCount > 0)
        //                    {
        //                        coinCost = coinCost * deltaCount;
        //                        curCoinCount = GetCoins(coinType);
        //                        if (coinCost > curCoinCount)
        //                        {
        //                            //钻石不够
        //                            response.Result = (int)ErrorCode.DiamondNotEnough;
        //                            Write(response);
        //                            return;
        //                        }
        //                        else
        //                        {
        //                            //扣钻
        //                            DelCoins(coinType, coinCost, ConsumeWay.PresentGift, string.Format("{0}:{1}:{2}", characterId, itemId, num));

        //                            Log.Alert("TODO PresentGift update PopScore");

        //                            //if (item.PopScore > 0)
        //                            //{
        //                            //    // 排行榜计算
        //                            //    OperateAddPop add = new OperateAddPop(characterId, item.PopScore * num);
        //                            //    server.Redis.Call(add, (ret) =>
        //                            //    {
        //                            //        double curHighScore = add.CurHighestScore;
        //                            //        double historyScore = add.HistoryScore;
        //                            //        if (PlayerChar.CheckBroadCastTitleInfo(add.pcUid, curHighScore, historyScore))
        //                            //        {
        //                            //            BroadCastTitleInfo(add.pcUid, curHighScore);
        //                            //        }

        //                            //    });
        //                            //}
        //                            itemUseCount = item.PileNum;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //足够 
        //                        //if (item.PopScore > 0)
        //                        //{
        //                        //    // 排行榜计算
        //                        //    OperateAddPop add = new OperateAddPop(characterId, item.PopScore * num);
        //                        //    server.Redis.Call(add, (ret) =>
        //                        //    {
        //                        //        double curHighScore = add.CurHighestScore;
        //                        //        double historyScore = add.HistoryScore;
        //                        //        if (PlayerChar.CheckBroadCastTitleInfo(add.pcUid, curHighScore, historyScore))
        //                        //        {
        //                        //            BroadCastTitleInfo(add.pcUid, curHighScore);
        //                        //        }
        //                        //    });
        //                        //}
        //                    }
        //                    //消耗掉物品
        //                    ItemUse(item.Uid, itemUseCount);

        //                }
        //            }
        //            else
        //            {
        //                //if (!NormalItem.CheckData(itemId))
        //                //{
        //                //    Log.Error("Bad packet:player {0} PresentGift got an wrong item id {1}", Uid, itemId);
        //                //    return;
        //                //}
        //                //不够
        //                //计算钻石数量
        //                coinCost = coinCost * num;
        //                curCoinCount = GetCoins(coinType);
        //                if (coinCost > curCoinCount)
        //                {
        //                    //钻石不够
        //                    response.Result = (int)ErrorCode.DiamondNotEnough;
        //                    Write(response);
        //                    return;
        //                }
        //                else
        //                {
        //                    //扣钻
        //                    DelCoins(coinType, coinCost, ConsumeWay.PresentGift, string.Format("{0}:{1}:{2}", characterId, itemId, num));
        //                    //int popScorePerItem = NormalItem.GetPopScore(itemId);
        //                    //if (popScorePerItem > 0)
        //                    //{
        //                    //    // 排行榜计算
        //                    //    //server.Redis.Call(new OperateAddPop(characterId, popScorePerItem * num));
        //                    //    OperateAddPop add = new OperateAddPop(characterId, popScorePerItem * num);
        //                    //    server.Redis.Call(add, (ret) =>
        //                    //    {
        //                    //        double curHighScore = add.CurHighestScore;
        //                    //        double historyScore = add.HistoryScore;
        //                    //        if (PlayerChar.CheckBroadCastTitleInfo(add.pcUid, curHighScore, historyScore))
        //                    //        {
        //                    //            BroadCastTitleInfo(add.pcUid, curHighScore);
        //                    //        }
        //                    //    });
        //                    //}
        //                    //else
        //                    //{
        //                    //    response.Result = (int)ErrorCode.Fail;
        //                    //    Write(response);
        //                    //}
        //                }
        //            }

        //            server.Redis.Call(new OperateAddGiftRecord(Uid, characterId, itemId, num, Timestamp.GetUnixTimeStamp(ZoneServerApi.now)));
        //            response.Result = (int)ErrorCode.Success;
        //            Write(response);

        //            Data itemData = DataListManager.inst.GetData("Items", itemId);
        //            int Quality = 0;
        //            if (itemData != null)
        //            {
        //                Quality = itemData.GetInt("Quality");
        //            }

        //            if (num >= ChatLibrary.GiftGivingCount || Quality >= ChatLibrary.GiftGivingQuality)
        //            {
        //                OperateGetNameById oper = new OperateGetNameById(characterId);
        //                server.Redis.Call(oper, r =>
        //                {
        //                    if ((int)r == 1)
        //                    {
        //                        if (string.IsNullOrEmpty(oper.Name))
        //                        {
        //                            Log.Error("player {0} get player {1} to black fail: cannot find player {2} ", Uid, characterId, characterId);
        //                            return;
        //                        }
        //                        else
        //                        {
        //                            List<string> list = new List<string>();
        //                            list.Add(Name);
        //                            list.Add(oper.Name);
        //                            list.Add(itemId.ToString());
        //                            list.Add(num.ToString());
        //                            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.GIFT_GIVING, list);
        //                            return;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Log.Error("player {0} get name not exist player {1} at redis ", Uid, characterId);
        //                        return;
        //                    }
        //                });
        //            }
        //        }
        //    }
        //    return;
        //}


        public void ChangeName(string name)
        {
            MSG_ZGC_CHANGE_NAME response = new MSG_ZGC_CHANGE_NAME();
            //1 消耗改名卡
            ErrorCode Result = UseItem(BagLibrary.ChangeNameTicketId, 1);
            bool needCostDiamond = true;
            if (Result == ErrorCode.Success)
            {
                SetName(name);
                response.Name = Name;
                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                //扣钻石
                if (Name.Equals(CharacterInitLibrary.InitName))
                {
                    needCostDiamond = false;
                }

                int coinCost = BagLibrary.ChangeNameTicketCost;
                int curDiamond = GetCoins(CurrenciesType.diamond);

                if (!needCostDiamond)
                {
                    //
                    coinCost = 0;
                }
                if (coinCost > curDiamond)
                {
                    //钻石不够
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Log.Warn($"player {Uid} change name {name} failed: diamond not enough");
                }
                else
                {
                    //扣钻等操作
                    if (coinCost > 0)
                    {
                        DelCoins(CurrenciesType.diamond, coinCost, ConsumeWay.ChangeName, name);
                    }
                    SetName(name);
                    response.Name = Name;
                    response.Result = (int)ErrorCode.Success;
                }
            }

            if (response.Result == (int)ErrorCode.Success)
            {
                AddTaskNumForType(TaskType.ChangeName);
            }

            Write(response);
            return;
        }

        void SetName(string name)
        {
            //Log.Warn("player {0} change name from {1} to {2}", Uid, Name, name);
            //
            BIRecordRenameLog(Name, name);
            //BI: name
            KomoeEventLoguUserInfoChange("1", Name, name);

            if (Name != name)
            {
                Name = name;
                //更新到数据库
                //string tableName = "character";
                server.GameDBPool.Call(new QueryChangeName(uid, name));
                //server.GameDBPool.Call(new QueryUpdateCharacterName(uid, name));
                //更新到redis
                server.GameRedis.Call(new OperateSetName(uid, Name));

                UpdateAccountLoginServers();
            }
        }

        //internal void SetSignature(string signature)
        //{
        //    //Log.Warn("player {0} set signature {1}", Uid, signature);
        //    if (Signature != signature)
        //    {
        //        Signature = signature;

        //        //更新到redis
        //        server.Redis.Call(new OperateSetSignature(uid, Signature));

        //        ///更新到数据库
        //        //string tableName = server.DB.GetTableName("character", uid, DBTableParamType.Character);
        //        //server.DB.Call(new QuerySetSignature(uid, tableName, Signature), tableName);   
        //    }

        //}

        //internal void SetSocialNum(string wNum, string qNum, int showType)
        //{
        //    //Log.Warn("player {0} set wNum {1} qq {2} showtype {3}", Uid, wNum, qNum, showType);
        //    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
        //    bool bUpdateRedis = false;
        //    if (WNum != wNum)
        //    {
        //        WNum = wNum;
        //        bUpdateRedis = true;
        //    }
        //    if (QNum != qNum)
        //    {
        //        QNum = qNum;
        //        bUpdateRedis = true;
        //    }
        //    if (InfoShowType != showType)
        //    {
        //        InfoShowType = showType;
        //        bUpdateRedis = true;
        //    }

        //    if (bUpdateRedis)
        //    {
        //        //更新到redis
        //        server.Redis.Call(new OperateSetSocialNum(Uid, WNum, QNum, InfoShowType));
        //    }

        //    ///更新到数据库
        //    //string tableName = server.DB.GetTableName("character", uid, DBTableParamType.Character);
        //    //server.DB.Call(new QuerySetSignature(uid, tableName, Signature), tableName);
        //}


        //internal void GetWNumAndQNum(int characterId)
        //{
        //    if (Uid == characterId)
        //    {
        //        //自己的信息
        //        //Log.Warn("player {0} get wNumAndQQ", Uid);
        //        MSG_ZGC_GET_SOCIAL_NUM response = new MSG_ZGC_GET_SOCIAL_NUM();

        //        response.Result = (int)ErrorCode.Success;
        //        response.InfoShowType = InfoShowType;
        //        response.QNum = QNum;
        //        response.WNum = WNum;
        //        Write(response);
        //    }
        //    else
        //    {
        //        //Log.Warn("player {0} get characterId {1} wNumAndQQ", Uid, characterId);

        //        OperateGetSocialNum operate = new OperateGetSocialNum(characterId);
        //        MSG_ZGC_GET_SOCIAL_NUM response = new MSG_ZGC_GET_SOCIAL_NUM();

        //        server.Redis.Call(operate, ret =>
        //        {
        //            if ((int)ret == 1)
        //            {

        //                switch ((wNumAndQQShowType)operate.info.InfoShowType)
        //                {
        //                    case wNumAndQQShowType.All:
        //                    //break;
        //                    case wNumAndQQShowType.OnlyFriend:
        //                        //frTODO:判断是否好友关系
        //                        response.InfoShowType = operate.info.InfoShowType;
        //                        response.QNum = operate.info.QNum;
        //                        response.WNum = operate.info.WNum;

        //                        response.Result = (int)ErrorCode.Success;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                Write(response);
        //                return;
        //            }
        //            else
        //            {
        //                response.Result = (int)ErrorCode.Fail;
        //                //frTODO:错误处理，没有读到数据，
        //            }
        //            Write(response);
        //        });
        //    }

        //    ////更新到数据库
        //    //string tableName = server.DB.GetTableName("character", uid, DBTableParamType.Character);
        //    //server.DB.Call(new QuerySetSignature(uid, tableName, Signature), tableName);
        //}
        //internal void SetSex(int sex)
        //{
        //    Log.Warn("player {0} set sex from {1} to {2}", Uid, SpaceSex, Sex);
        //    SpaceSex = sex;
        //    //更新到redis
        //    server.Redis.Call(new OperateSetSpaceSex(Uid, SpaceSex));

        //    MSG_ZGC_SET_SEX response = new MSG_ZGC_SET_SEX();
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //internal void SetBirthday(string birthday)
        //{
        //    Log.Warn("player {0} set birthday from {1} to {2}", Uid, Birthday, birthday);
        //    Birthday = birthday;
        //    //更新到redis
        //    server.Redis.Call(new OperateSetBrithday(Uid, Birthday));

        //    MSG_ZGC_SET_BIRTHDAY response = new MSG_ZGC_SET_BIRTHDAY();
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}

        //internal SHOW_SPACEINFO GetSpaceShowInfo()
        //{
        //    SHOW_SPACEINFO info = new SHOW_SPACEINFO();
        //    info.Signature = Signature;
        //    info.Sex = SpaceSex;
        //    info.Birthday = Birthday;
        //    info.PopScore = PopScore;
        //    info.HighestPopScore = HighestPopScore;
        //    info.ShowVoice = ShowVoice;
        //    info.Longitude = geography.Longitude;
        //    info.Latitude = geography.Latitude;
        //    info.Title = CurTitleId;

        //    return info;
        //}

        //public void GetRankingFriendList()
        //{
        //    OperateGetFriendInfoList operate = new OperateGetFriendInfoList(Uid);
        //    server.Redis.Call(operate, ret =>
        //    {
        //        MSG_ZGC_RANKING_FRIEND_LIST response = new MSG_ZGC_RANKING_FRIEND_LIST();
        //        response.Info = PlayerInfo.GetPlayerBaseInfo(this);
        //        if ((int)ret == 1)
        //        {
        //            if (operate.Characters != null)
        //            {
        //                var list = (from tempInfo in operate.Characters orderby tempInfo.LadderScore descending select tempInfo);
        //                int iRank = 0;
        //                foreach (var item in list)
        //                {
        //                    iRank++;
        //                    PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(item);
        //                    if (response.Info.Rank <= 0 && response.Info.LadderScore >= info.LadderScore)
        //                    {
        //                        response.Info.Rank = iRank;
        //                        response.List.Add(response.Info);
        //                        iRank++;
        //                    }
        //                    info.Rank = iRank;
        //                    response.List.Add(info);
        //                }

        //                if (response.Info.Rank <= 0)
        //                {
        //                    iRank++;
        //                    response.Info.Rank = iRank;
        //                    response.List.Add(response.Info);
        //                }
        //                Write(response);
        //                return;
        //            }
        //            else
        //            {
        //                Write(response);
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Write(response);
        //            return;
        //        }
        //    });
        //}

        //public void GetRankingAllList(int page)
        //{
        //    //if (page <= server.SeasonMng.Page)
        //    //{
        //    //    if (page == 0)
        //    //    {
        //    //        server.SeasonMng.UpdatePage();
        //    //    }

        //    //    MSG_ZGC_RANKING_ALL_LIST response = new MSG_ZGC_RANKING_ALL_LIST();
        //    //    response.Total = server.SeasonMng.Page;
        //    //    response.Index = page;

        //    //    int start = 0;
        //    //    int end = 100;
        //    //    Log.Debug("//TODO:Ranking count!!");
        //    //    //int start = page * BattleLibrary.AllRankingShowCount;
        //    //    //int end = Math.Min(server.SeasonMng.AllRankList.Count, start + BattleLibrary.AllRankingShowCount);

        //    //    PlayerSimpleInfo simpleInfo = server.SeasonMng.GetPlayerSimpleInfo(Uid);
        //    //    if (simpleInfo == null)
        //    //    {
        //    //        response.Info = PlayerInfo.GetPlayerBaseInfo(this);

        //    //        for (int i = start; i < end; i++)
        //    //        {
        //    //            PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(server.SeasonMng.AllRankList.ElementAt(i).Value);
        //    //            response.List.Add(info);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        response.Info = PlayerInfo.GetPlayerBaseInfo(simpleInfo);

        //    //        for (int i = start; i < end; i++)
        //    //        {
        //    //            PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(server.SeasonMng.AllRankList.ElementAt(i).Value);
        //    //            response.List.Add(info);
        //    //        }
        //    //    }
        //    //    Write(response);
        //    //}
        //    //else
        //    //{
        //    //    Log.Warn("player {0} GetRankingAllList page {1} is error", Uid, page);
        //    //}
        //}

        //public void GetRankingNearbyList()
        //{
        //    MSG_ZGC_RANKING_NEARBY_LIST response = new MSG_ZGC_RANKING_NEARBY_LIST();
        //    if (string.IsNullOrEmpty(geography.GeoHashStr))
        //    {
        //        Write(response);
        //    }
        //    else
        //    {
        //        //OperateGetGeoHashIds operate = new OperateGetGeoHashIds(Uid, geography.GeoHashStr, GameConfig.NearbyRankingCount, blackList.Keys.ToList(), friendList);
        //        //server.Redis.Call(operate, ret =>
        //        //{
        //        //    if ((int)ret == 1)
        //        //    {
        //        //        List<RedisValue> tmpIds = new List<RedisValue>();

        //        //        tmpIds.AddRange(operate.Ids);
        //        //        int count = FriendLib.RECOMMEND_COUNT - operate.Ids.Count;
        //        //        if (count > 0)
        //        //        {
        //        //            DateTime start = Api.now.Subtract(new TimeSpan(24, 0, 0));//1天内登录
        //        //            OperateRecommendPlayers opr = new OperateRecommendPlayers(Uid, start, Api.now, count, blackList.Keys.ToList(), friendList);
        //        //            server.Redis.Call(opr, re =>
        //        //            {
        //        //                if ((int)re == 1)
        //        //                {
        //        //                    if (opr.Ids.Count() > 0)
        //        //                    {
        //        //                        tmpIds.AddRange(opr.Ids);
        //        //                        GetRecommendPlayers(response, tmpIds);
        //        //                    }
        //        //                }
        //        //                else
        //        //                {
        //        //                    Log.Error("player {0} GetRankingNearbyList fail redis error data", Uid);
        //        //                }
        //        //            });
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            GetRecommendPlayers(response, operate.Ids.ToList());
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        Log.Error("player {0} GetRankingNearbyList fail geohash null,redis error data", Uid);
        //        //    }
        //        //});
        //    }
        //}

    }

}

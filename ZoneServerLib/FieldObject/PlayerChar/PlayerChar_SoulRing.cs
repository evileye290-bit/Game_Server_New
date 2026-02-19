using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using RedisUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //魂环
        public SoulRingManager SoulRingManager { get; set; }

        public void InitSoulRingManager()
        {
            this.SoulRingManager = new SoulRingManager(this);
        }

        //吸收魂环
        public void AbsorbSoulRing(int heroId, ulong roulRingUid, int slot)
        {
            //吸收魂环相关逻辑
            MSG_ZGC_ABSORB_SOULRING response = new MSG_ZGC_ABSORB_SOULRING();
            response.HeroId = heroId;
            response.SoulRingUidHigh = roulRingUid.GetHigh();
            response.SoulRingUidLow = roulRingUid.GetLow();

            Bag_SoulRing bag = bagManager.SoulRingBag;

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {uid} absorb soulring failed: heroId error {heroId}");
                return;
            }
            if (bag.CheckIsAbsorbed(heroId)) //已经在吸收别的魂环
            {
                Log.Warn($"player {uid} hero {heroId} is absorbed other soul ring !");
                return;
            }

            //获取魂环
            SoulRingItem soulRing = (SoulRingItem)bag.GetItem(roulRingUid);
            if (soulRing == null || slot <= 0)
            {
                Log.Error($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail, because this ring not exist");
                return;
            }

            SoulRingSlotUnlockModel unlockModel = SoulRingLibrary.GetSoulRingSlotUnlockMode(slot);
            if (heroInfo.Level < unlockModel.Level)
            {
                Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail, level limit");
                response.Result = (int)ErrorCode.SoulRingABLevelLimit;
                Write(response);
                return;
            }

            int absorbHeroLevel = soulRing.GetAbsorbHeroLevel();
            if (heroInfo.Level < absorbHeroLevel)
            {
                Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail,hero level not enough");
                response.Result = (int)ErrorCode.SoulRingABHeroLevelLimit;
                Write(response);
                return;
            }

            if (SoulRingManager.CheckEquipedSoulRingType(heroId, soulRing.SoulRingInfo.TypeId, slot))
            {
                Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail,hero has equiped same type ");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //获取当前槽魂环
            SoulRingItem equipedSoulRing = SoulRingManager.GetEquipedSoulRing(heroId, slot);
            if (equipedSoulRing == null)
            {
                if (SoulRingManager.CheckRepeated(heroId, soulRing.modelId))
                {
                    Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail,soulring  repeated {soulRing.modelId}");
                    response.Result = (int)ErrorCode.SoulRingOwnMonsterRepeated;
                    Write(response);
                    return;
                }
                //去掉等级和魂环联系
                ////当前槽无魂环
                //if (!CheckWuhunStateWaitAbsorb(heroId, slot))
                //{
                //    response.Result = (int)ErrorCode.WaitAwaken;
                //    Write(response);
                //    Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} fail,wait awaken");
                //    return;
                //}
                SoulRingAbsorbAdd(heroId, soulRing, slot);
                //魂环替换
                BIRecordRingReplaceLog(heroInfo.Id, heroInfo.Level, slot, 0, 0, soulRing.Id, soulRing.Year);
                //养成
                BIRecordDevelopLog(DevelopType.EquipSoulRing, soulRing.Id, 0, soulRing.Year, heroInfo.Id, heroInfo.Level);
            }
            else
            {
                ////已经装有魂环
                //if (soulRing.Year < equipedSoulRing.Year)
                //{
                //    Log.Warn($"player {uid} hero {heroId} absorb soul ring {roulRingUid} year {soulRing.Level} < equiped soul ring {equipedSoulRing.Id} year {equipedSoulRing.Year}");
                //    response.Result = (int)ErrorCode.SoulRingSwapYearWrong;
                //    Write(response);
                //    return;
                //}
                SoulRingAbsortSwap(equipedSoulRing, soulRing);
                //魂环替换
                BIRecordRingReplaceLog(heroInfo.Id, heroInfo.Level, slot, equipedSoulRing.Id, equipedSoulRing.Year, soulRing.Id, soulRing.Year);
                //养成
                BIRecordDevelopLog(DevelopType.EquipSoulRing, soulRing.Id, equipedSoulRing.Year, soulRing.Year, heroInfo.Id, heroInfo.Level);
            }
            //魂环吸收标记
            bag.AddOnAbsorbFlag(heroId, soulRing.Uid);

            List<BaseItem> updateList = new List<BaseItem>();

            response.Result = (int)ErrorCode.Success;

            //记录魂环吸收信息
            if (equipedSoulRing == null)
            {
                DateTime finishTime = server.Now().Add(SoulRingLibrary.GetAbsorbTime(soulRing.Year));

                server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, soulRing.Uid, Timestamp.GetUnixTimeStampSeconds(finishTime)));

                bag.SyncDbItemInfo(soulRing);
                updateList.Add(soulRing);
            }
            else
            {
                DateTime finishTime = server.Now();
                server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, soulRing.Uid, Timestamp.GetUnixTimeStampSeconds(finishTime), equipedSoulRing.Uid));
                //bagManager.SoulRingBag.UpdateItem(equipedSoulRing);
                //bag.SyncDbItemInfo(equipedSoulRing);
                //updateList.Add(equipedSoulRing);

                ErrorCode result;
                List<BaseItem> tempList = ReplaceSoulRing(heroId, equipedSoulRing.Uid, soulRing.Uid, out result);
                SyncRedisSoulRingInfo(heroId);
                if (result == ErrorCode.Success)
                {
                    updateList.AddRange(tempList);
                }
            }
            SyncClientItemsInfo(updateList);

            Write(response);
        }



        /// <summary>
        /// 护持吸收魂环
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="uids"></param>
        public void HelpAbsorbSoulRing(int heroId, List<int> uids)
        {
            if (uids == null || uids.Count == 0)
            {
                Log.Warn($"player {uid} hero {heroId} absorb helpers too less: uids == null !");
                return;
            }
            if (uids.Count > SoulRingLibrary.SoulRingConfig.AbsorbHelperMaxCount)
            {
                Log.Warn($"player {uid} hero {heroId} absorb helpers too many !");
                return;
            }
            //护持吸收魂环相关逻辑
            OperateGetSoulRingAbsorbInfo absorbInfo = new OperateGetSoulRingAbsorbInfo(uid, heroId);
            server.GameRedis.Call(absorbInfo, ret =>
            {
                MSG_ZGC_HELP_ABSORB_SOULRING resp = new MSG_ZGC_HELP_ABSORB_SOULRING();
                resp.HeroId = heroId;

                if ((int)ret == 1)
                {
                    ulong newSoulRingUid = absorbInfo.Info.NewSoulRingUid;
                    Bag_SoulRing bag = bagManager.SoulRingBag;
                    SoulRingItem newItem = (SoulRingItem)bag.GetItem(newSoulRingUid);
                    if (newItem == null)
                    {
                        Log.Warn($"player {uid} hero {heroId} help absorb fail,no absorbed soul ring");
                        //前没有魂环在吸收，返回护持失败
                        resp.Result = (int)ErrorCode.Fail;
                        Write(resp);
                        return;
                    }
                    else
                    {
                        DateTime dtFinish = absorbInfo.Info.AbsorbFinishTime;
                        TimeSpan tsAbsorbHelpMinTime = SoulRingLibrary.SoulRingConfig.AbsorbHelpMinTime;

                        //剩余时长
                        TimeSpan tsDelta = dtFinish - ZoneServerApi.now;
                        Log.Alert($"player {uid} absorb soul ring {newItem.Id} dtFinish {dtFinish.ToString()} delta seconds {tsDelta.TotalSeconds}");
                        int abTime = 0;
                        if (tsDelta > tsAbsorbHelpMinTime)
                        {
                            int cutTime = CalcHelpAbsorbTime(uids, newItem.Position);

                            TimeSpan tsCutTime = TimeSpan.FromSeconds(cutTime);

                            tsDelta = tsDelta - tsCutTime;
                            if (tsDelta > tsAbsorbHelpMinTime)
                            {
                                dtFinish = ZoneServerApi.now.Add(tsDelta);
                            }
                            else
                            {
                                dtFinish = ZoneServerApi.now.Add(tsAbsorbHelpMinTime);
                            }

                            absorbInfo.Info.AbsorbHelpList.AddRange(uids);

                            abTime = Timestamp.GetUnixTimeStampSeconds(dtFinish);

                            Log.Alert($"player {uid} dtFinish {dtFinish.ToString()} cutTime {tsCutTime.TotalSeconds}");

                            //记录英雄当前护持人员，记录完成时间
                            server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, absorbInfo.Info.NewSoulRingUid, abTime, absorbInfo.Info.OldSoulRingUid, absorbInfo.Info.AbsorbHelpList));

                        }
                        else
                        {
                            abTime = Timestamp.GetUnixTimeStampSeconds(dtFinish);
                            Log.Alert($"player {uid} dtFinish {dtFinish.ToString()}");
                        }
                        resp.Result = (int)ErrorCode.Success;
                        resp.Time = abTime;
                        Write(resp);
                        //好友护持任务
                        AddTaskNumForType(TaskType.SoulRingHelpAbsorb, uids.Count);
                    }
                    return;
                }
                else
                {
                    //当前没有魂环在吸收，返回护持失败
                    Log.Warn($"player {uid} hero {heroId} help absorb fail,no absorbed soul ring");
                    resp.Result = (int)ErrorCode.Fail;
                    Write(resp);
                    return;
                }
            });
        }

        private int CalcHelpAbsorbTime(List<int> uids, int position)
        {
            //根据护持人员友好度计算剩余时间（若减少的时间超出了魂环吸收的剩余时间，则至多减少至30秒）
            Dictionary<int, int> helpPlayerScoreDic = GetHelpPlayerScore(uids);
            //根据友好度计算完成时间
            int cutTime = 0;
            foreach (var item in helpPlayerScoreDic)
            {
                cutTime += ScriptManager.SoulRing.GetHelpAbsorbAbTime(position, item.Value);
            }

            return cutTime;
        }

        /// <summary>
        /// 魂环吸收信息
        /// </summary>
        /// <param name="heroUid"></param>
        public void GetSoulRingAbsorbInfo(int heroId)
        {
            Log.Warn($"player {uid} get soul ring absorb info fail!the MSG_CG_GET_ABSORBINFO is discard!");
            //获取魂环 吸收信息
            //OperateGetSoulRingAbsorbInfo absorbInfo = new OperateGetSoulRingAbsorbInfo(uid, heroId);
            //server.Redis.Call(absorbInfo, ret =>
            //{
            //    MSG_ZGC_GET_ABSORBINFO resp = new MSG_ZGC_GET_ABSORBINFO();
            //    resp.HeroId = heroId;

            //    if ((int)ret == 1)
            //    {
            //        if (absorbInfo.Info.NewSoulRingUid < 1)
            //        {
            //            Log.Warn($"player {uid} hero {heroId} help absorb fail,no absorbed soul ring");
            //            resp.Result = (int)ErrorCode.Fail;
            //            Write(resp);
            //            //前没有魂环在吸收，返回护持失败
            //            return;
            //        }
            //        else
            //        {
            //            //完成时间，护持人员列表等
            //            resp.Time = Timestamp.GetUnixTimeStampSeconds(absorbInfo.Info.AbsorbFinishTime);

            //            if (absorbInfo.Info.AbsorbHelpList.Count > 0)
            //            {
            //                OperateGetFriendInfoListByIds oper = new OperateGetFriendInfoListByIds(uid, absorbInfo.Info.AbsorbHelpList);
            //                server.Redis.Call(oper, re =>
            //                {
            //                    if ((int)re == 1)
            //                    {
            //                        //完成时间，护持人员列表等
            //                        resp.Time = Timestamp.GetUnixTimeStampSeconds(absorbInfo.Info.AbsorbFinishTime);
            //                        if (oper.Characters != null)
            //                        {
            //                            foreach (var item in oper.Characters)
            //                            {
            //                                //这里设置好友当前友好度
            //                                item.Value.SetFriendScore(GetScoreByFriendId(item.Value.GetUid()));
            //                                HELP_PLAYERINFO friendInfo = GetHelpAbsorbPlayerInfo(item.Value);
            //                                resp.PlayerList.Add(friendInfo);
            //                            }
            //                            resp.Result = (int)ErrorCode.Success;
            //                            Write(resp);
            //                            return;
            //                        }
            //                        else
            //                        {
            //                            Log.Error("player {0} get help absorb players fail oper.characters is null", Uid);
            //                            resp.Result = (int)ErrorCode.Fail;
            //                            Write(resp);
            //                            return;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        Log.Error("player {0} get help absorb players fail , can not get info from redis", Uid);
            //                        resp.Result = (int)ErrorCode.Fail;
            //                        Write(resp);
            //                        return;
            //                    }
            //                });
            //            }
            //            else
            //            {
            //                resp.Result = (int)ErrorCode.Success;
            //                Write(resp);
            //            }
            //        }
            //        return;
            //    }
            //    else
            //    {
            //        //当前没有魂环在吸收
            //        Log.Warn($"player {uid} hero {heroId} help absorb fail,no absorbed soul ring");
            //        resp.Result = (int)ErrorCode.Fail;
            //        Write(resp);
            //        return;
            //    }
            //});
        }


        //public HELP_PLAYERINFO GetHelpAbsorbPlayerInfo(FriendInfo info)
        //{
        //    HELP_PLAYERINFO helpPlayer = new HELP_PLAYERINFO()
        //    {
        //        Uid = info.BaseInfo.Uid,
        //        Name = info.BaseInfo.Name,
        //        FriendScore = info.FriendScore
        //    };
        //    return helpPlayer;
        //}


        /// <summary>
        /// 取消魂环吸收
        /// </summary>
        /// <param name="heroUid"></param>
        public void CancelSoulRingAbsorb(int heroId)
        {
            //删除魂环吸收标记，删除 吸收信息。
            OperateGetSoulRingAbsorbInfo absorbInfo = new OperateGetSoulRingAbsorbInfo(uid, heroId);
            server.GameRedis.Call(absorbInfo, ret =>
            {
                MSG_ZGC_CANCEL_ABSORB resp = new MSG_ZGC_CANCEL_ABSORB();
                resp.HeroId = heroId;

                if ((int)ret == 1)
                {
                    ulong newSoulRingUid = absorbInfo.Info.NewSoulRingUid;
                    if (newSoulRingUid < 1)
                    {
                        Log.Warn($"player {uid} hero {heroId} cancel absorb fail,no absorbed soul ring");
                        //前没有魂环在吸收，返回护持失败
                        resp.Result = (int)ErrorCode.Fail;
                        Write(resp);
                        return;
                    }
                    else
                    {
                        Bag_SoulRing bag = bagManager.SoulRingBag;
                        SoulRingItem newItem = (SoulRingItem)bag.GetItem(newSoulRingUid);
                        if (newItem == null)
                        {
                            Log.Error($"player {uid} hero {heroId} cancel absorb soul ring {newSoulRingUid} fail, because this ring not exist");
                            return;
                        }

                        List<BaseItem> updateList = new List<BaseItem>();

                        ulong oldSoulRingUid = absorbInfo.Info.OldSoulRingUid;
                        if (oldSoulRingUid > 0)
                        {
                            //获取魂环
                            SoulRingItem oldItem = SoulRingManager.GetEquipedSoulRing(heroId, newItem.Position);
                            if (oldItem == null)
                            {
                                Log.Error($"player {uid} hero {heroId} cancel swap old soul ring {oldSoulRingUid} fail, because this ring not exist");
                                return;
                            }

                            //之前替换的
                            CancelSoulRingAbsorbSwap(newItem, oldItem);
                            bag.SyncDbItemInfo(oldItem);

                            updateList.Add(oldItem);
                        }
                        else
                        {
                            //直接添加的
                            CancelSoulRingAbsorbAdd(newItem);
                        }
                        ///取消正在吸收中的魂环标记
                        bag.DelOnAbsorbFlag(heroId);
                        bag.SyncDbItemInfo(newItem);
                        updateList.Add(newItem);

                        SyncClientItemsInfo(updateList);
                        ///删除吸收信息
                        server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, 0));

                        resp.SoulRingUidHigh = newSoulRingUid.GetHigh();
                        resp.SoulRingUidLow = newSoulRingUid.GetLow();
                        resp.Result = (int)ErrorCode.Success;
                        Write(resp);
                    }
                    return;
                }
                else
                {
                    //当前没有魂环在吸收，返回护持失败
                    resp.Result = (int)ErrorCode.Fail;
                    Write(resp);
                    return;
                }
            });
        }

        /// <summary>
        /// 魂环吸收完毕
        /// </summary>
        /// <param name="heroUid"></param>
        public void SoulRingAbsorbFinish(int heroId)
        {
            //魂环吸收完毕
            OperateGetSoulRingAbsorbInfo absorbInfo = new OperateGetSoulRingAbsorbInfo(uid, heroId);
            server.GameRedis.Call(absorbInfo, ret =>
            {
                MSG_ZGC_ABSORB_FINISH resp = new MSG_ZGC_ABSORB_FINISH();
                resp.HeroId = heroId;

                if ((int)ret == 1)
                {

                    DateTime finishTime = absorbInfo.Info.AbsorbFinishTime;
                    if (finishTime > ZoneServerApi.now)
                    {
                        resp.Result = (int)ErrorCode.NotOnTime;
                        Write(resp);
                        Log.Error($"player {uid} hero {heroId} finish absorb fail, soul ring  {absorbInfo.Info.NewSoulRingUid} not on time {finishTime}");
                        return;
                    }

                    ulong newSoulRingUid = absorbInfo.Info.NewSoulRingUid;

                    Bag_SoulRing bag = bagManager.SoulRingBag;
                    SoulRingItem newItem = (SoulRingItem)bag.GetItem(newSoulRingUid);
                    if (newItem == null)
                    {
                        Log.Error($"player {uid} hero {heroId} finish absorb fail, soul ring  {newSoulRingUid} not exist in bag");
                        return;
                    }
                    List<BaseItem> updateList = new List<BaseItem>();
                    bool isAdd = false;
                    ulong oldSoulRingUid = absorbInfo.Info.OldSoulRingUid;
                    SoulRingItem oldItem = SoulRingManager.GetEquipedSoulRing(heroId, newItem.Position);
                    if (oldItem == null)
                    {
                        SoulRingAdd(heroId, newItem, newItem.Position);
                        isAdd = true;
                    }
                    else
                    {
                        SoulRingSwap(oldItem, newItem);
                        bag.SyncDbItemInfo(oldItem);
                        updateList.Add(oldItem);
                    }

                    //吸收完毕清除吸收标记
                    bag.DelOnAbsorbFlag(heroId);
                    ///删除吸收信息
                    server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, 0));

                    bag.SyncDbItemInfo(newItem);

                    updateList.Add(newItem);
                    SyncClientItemsInfo(updateList);

                    resp.Result = (int)ErrorCode.Success;
                    resp.SoulRingUidHigh = newSoulRingUid.GetHigh();
                    resp.SoulRingUidLow = newSoulRingUid.GetLow();
                    Write(resp);

                    if (isAdd)
                    {
                        HeroTitleUp(heroId);
                    }

                    //吸收魂环次数
                    AddTaskNumForType(TaskType.SoulRingAbsorb);
                    //装备指定等级魂环
                    AddTaskNumForType(TaskType.EquipSoulRingForLevel, 1, true, newItem.Level);
                    //装备魂环个数
                    AddTaskNumForType(TaskType.EquipSoulRing, SoulRingManager.GetEquipedCount(), false);
                    //装备指定年限魂环个数
                    AddTaskNumForType(TaskType.EquipSoulRingForYear);
                    //装备位置魂环
                    AddTaskNumForType(TaskType.EquipSoulRingBySlot, 1, true, newItem.Position);
                    //出战伙伴装备魂环
                    AddTaskNumForType(TaskType.EquipHeroSoulRing, GetEquipedHeroSoulRings(), false);

                    //玩家行为
                    RecordAction(ActionType.HeroSoulRingYear, heroId);
                    return;
                }
                else
                {
                    resp.Result = (int)ErrorCode.Fail;
                    Write(resp);
                    Log.Error($"player {uid} hero {heroId} finish absorb fail, soul ring redis not exist");
                    return;
                }
            });

        }


        /// <summary>
        /// 吸收添加
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="item"></param>
        private void SoulRingAbsorbAdd(int heroId, SoulRingItem item, int pos)
        {
            item.SetAbsorbState(SoulRingAbsorbState.OnAbsort);
            item.SetEquipHeroId(heroId);
            item.SetPosition(pos);
        }

        /// <summary>
        /// 取消 吸收添加
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        private void CancelSoulRingAbsorbAdd(SoulRingItem newItem)
        {
            //等级继承
            newItem.SetAbsorbState(SoulRingAbsorbState.Deafult);
            newItem.SetEquipHeroId(-1);
            newItem.SetPosition(0);
        }

        /// <summary>
        /// 吸收 替换
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        private void SoulRingAbsortSwap(SoulRingItem oldItem, SoulRingItem newItem)
        {
            oldItem.SetAbsorbState(SoulRingAbsorbState.BeReplace);
            newItem.SetAbsorbState(SoulRingAbsorbState.OnAbsort);
            newItem.SetEquipHeroId(oldItem.EquipHeroId);
            newItem.SetPosition(oldItem.Position);
        }


        /// <summary>
        /// 取消吸收替换
        /// </summary>
        private void CancelSoulRingAbsorbSwap(SoulRingItem newItem, SoulRingItem oldItem)
        {
            newItem.SetAbsorbState(SoulRingAbsorbState.Deafult);
            oldItem.SetAbsorbState(SoulRingAbsorbState.Deafult);

            newItem.SetEquipHeroId(-1);
            newItem.SetPosition(0);
        }

        /// <summary>
        /// 真正 吸收添加
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="item"></param>
        private void SoulRingAdd(int heroId, SoulRingItem item, int pos)
        {
            item.SetAbsorbState(SoulRingAbsorbState.Deafult);
            item.SetEquipHeroId(heroId);
            item.SetPosition(pos);

            SoulRingManager.AddEquipSoulRing(item);
            BagManager.SoulRingBag.RemoveItem(item);

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero != null)
            {
                //int addYearRatio = SoulRingManager.GetAddYearRatio(hero.StepsLevel);
                //HeroMng.SoulRingAdd(hero, item.GetMainAttrs(addYearRatio));
                //HeroMng.SoulRingAdd(hero, item.GetUltAttrs());

                //HeroMng.AbsorbSoulRing(hero);
                // SoulSkillNatureAdd(hero);
                HeroMng.InitHeroNatureInfo(hero);
                //HeroMng.UpdateBattlePower(heroId);
                HeroMng.NotifyClientBattlePowerFrom(heroId);

                //同步
                SyncHeroChangeMessage(new List<HeroInfo>() { hero });
                SyncDbUpdateHeroItem(hero);

                //玩家行为
                RecordAction(ActionType.HeroSoulRingYear, heroId);
            }
            else
            {
                Log.Error($"player {uid} hero {heroId} SoulRingAdd fail: hero not exist");
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //private void SoulSkillNatureAdd(HeroInfo hero)
        //{
        //    if (hero == null) {
        //        Log.Error($"player {uid} soul skill nature add fail：hero is null");
        //        return;
        //    }
        //    int level = hero.SoulSkillLevel;

        //    Dictionary<NatureType, float> enhanceDic = new Dictionary<NatureType, float>();

        //    for (int i = 0; i <= level; i++)
        //    {
        //        NatureDataModel incrNatureDic = SoulSkillLibrary.GetBasicNatureIncrModel(i);
        //        foreach (var item in incrNatureDic.NatureList)
        //        {
        //            float value = 0;
        //            if (enhanceDic.TryGetValue(item.Key,out value))
        //            {
        //                enhanceDic[item.Key] =  item.Value - value;
        //            }
        //            else
        //            {
        //                enhanceDic[item.Key] = item.Value;
        //            }
        //        }
        //    }

        //    hero.AddSoulSkillNature(enhanceDic);
        //}


        /// <summary>
        /// 真正替换，继承相关属性
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        private void SoulRingSwap(SoulRingItem oldItem, SoulRingItem newItem)
        {
            HeroInfo hero = HeroMng.GetHeroInfo(newItem.EquipHeroId);
            if (hero != null)
            {

                //int addYearRatio = SoulRingManager.GetAddYearRatio(hero.StepsLevel);

                //Dictionary<NatureType, long> oldMainNature = oldItem.GetMainAttrs(addYearRatio);
                //Dictionary<NatureType, long> oldUltNature = oldItem.GetUltAttrs();

                //等级继承
                newItem.SetLevel(oldItem.Level);
                oldItem.SetLevel(oldItem.IniLevel);

                oldItem.SetAbsorbState(SoulRingAbsorbState.Deafult);
                newItem.SetAbsorbState(SoulRingAbsorbState.Deafult);

                //旧魂环放背包
                SoulRingManager.DelEquipSoulRing(oldItem);
                oldItem.SetEquipHeroId(-1);
                oldItem.SetPosition(0);
                BagManager.SoulRingBag.AddItemAndCheckAbsorb(oldItem);

                //装备新的魂环
                SoulRingManager.AddEquipSoulRing(newItem);
                BagManager.SoulRingBag.RemoveItem(newItem);

                ReplaceSoulRingElement(oldItem, newItem);

                HeroMng.InitHeroNatureInfo(hero);
                HeroMng.NotifyClientBattlePowerFrom(hero.Id);

                //同步
                SyncHeroChangeMessage(new List<HeroInfo>() { hero });
                SyncDbUpdateHeroItem(hero);


                //装备魂环个数
                AddTaskNumForType(TaskType.EquipSoulRing, SoulRingManager.GetEquipedCount(), false);
                //装备指定年限魂环个数
                AddTaskNumForType(TaskType.EquipSoulRingForYear);

                //玩家行为
                RecordAction(ActionType.HeroSoulRingYear, hero.Id);
            }
            else
            {
                Log.Error($"player {uid}  hero {newItem.EquipHeroId} SoulRingSwap {oldItem.Uid} {newItem.Uid} fail: not find heroId .");
            }
        }

        /// <summary>
        /// 检查槽位的可吸收解锁等级
        /// </summary>
        /// <param name="postion"></param>
        /// <returns></returns>
        private bool CheckSoulRingAbsorbLevel(int heroId, int position)
        {
            HeroInfo info = HeroMng.GetHeroInfo(heroId);
            if (info != null)
            {
                SoulRingSlotUnlockModel unlockModel = SoulRingLibrary.GetSoulRingSlotUnlockMode(position);
                if (info.Level < unlockModel.Level)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 检查可吸收状态
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        private bool CheckWuhunStateWaitAbsorb(int heroId, int position)
        {
            HeroInfo info = HeroMng.GetHeroInfo(heroId);
            SoulRingSlotUnlockModel unlockModel = SoulRingLibrary.GetSoulRingSlotUnlockMode(position);

            //if (info.State == WuhunState.WaitAbsorb && info.AwakenLevel >= unlockModel.AwakeLevel)
            {
                return true;
            }
            //return false;
        }

        private Dictionary<int, int> GetHelpPlayerScore(List<int> uids)
        {
            Dictionary<int, int> helpPlayerScoreDic = new Dictionary<int, int>();
            foreach (var uid in uids)
            {
                int score;
                //if (friendList.TryGetValue(uid,out score))
                if (friendBattlePowerList.TryGetValue(uid, out score))
                {
                    helpPlayerScoreDic.Add(uid, score);
                }
            }
            return helpPlayerScoreDic;
        }


        /// <summary>
        /// 获取护持感谢好友列表
        /// </summary>
        /// <param name="uids"></param>
        internal void GetHelpThanksList(RepeatedField<int> uids)
        {
            MSG_ZGC_GET_HELP_THANKS_LIST resp = new MSG_ZGC_GET_HELP_THANKS_LIST();

            var fields = uids.Select(x => (RedisValue)x);

            OperateGetFriendInfoListByIds operate = new OperateGetFriendInfoListByIds(Uid, fields);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        foreach (var item in operate.Characters)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            resp.List.Add(friendInfo);
                        }
                        resp.Result = (int)ErrorCode.Success;
                        Write(resp);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //Log.Error("player {0} execute GetRecentList fail: redis data error!", Uid);
                    return;
                }
            });
        }

        /// <summary>
        /// 答谢好友(赠送)
        /// </summary>
        /// <param name="friendUid"></param>
        /// <param name="itemUid"></param>
        internal void ThankFriend(int friendUid, ulong itemUid)
        {
            MSG_ZGC_THANK_FRIEND resp = new MSG_ZGC_THANK_FRIEND()
            {
                FriendUid = friendUid,
                ItemUidHigh = itemUid.GetHigh(),
                ItemUidLow = itemUid.GetLow()
            };

            if (!CheckFriendExist(friendUid))
            {
                return;
            }

            //获取赠送物品
            Bag_Normal bag = bagManager.NormalBag;
            NormalItem item = (NormalItem)bag.GetItem(itemUid);
            if (item == null)
            {
                Log.Error($"player {uid} thank friend {friendUid} item {itemUid} not exist in bag");
                return;
            }
            //判断物品是否足够
            if (item.PileNum < 1)
            {
                //如果不够 返回错误码
                resp.Result = (int)ErrorCode.ItemNotEnough;
                Write(resp);
                Log.Warn($"player {uid} thank friend {friendUid} item {itemUid} not enough");
                return;
            }
            //消耗物品
            bag.DelItem(item.Uid, 1);
            SyncClientItemInfo(item);

            //增加友好值
            int addSore = FriendLib.GetGiftScore(item.Id);
            AddFriendScore(friendUid, addSore);

            resp.Result = (int)ErrorCode.Success;
            Write(resp);
        }

        /// <summary>
        /// 魂环强化（突破）
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="soulRingUid"></param>
        internal void SoulRingEnhance(int heroId, ulong soulRingUid, int type)
        {
            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Error($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: cannot find hero {heroId} .");
                return;
            }

            MSG_ZGC_ENHANCE_SOULRING resp = new MSG_ZGC_ENHANCE_SOULRING()
            {
                HeroId = heroId,
                SoulRingUidHigh = soulRingUid.GetHigh(),
                SoulRingUidLow = soulRingUid.GetLow()
            };

            Dictionary<CurrenciesType, int> costs = new Dictionary<CurrenciesType, int>();
            int oldLevel = hero.SoulSkillLevel;

            int enhanceNum = 0;
            int breakNum = 0;
            if (type != 0)
            {
                ErrorCode result = SoulRingOneKeyEnhance(heroId, type, costs, out enhanceNum, out breakNum);

                if (result != ErrorCode.Success)
                {
                    Log.Warn($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: {result} .");
                    resp.Result = (int)result;
                    return;
                }

                DelCoins(costs, ConsumeWay.EnhanceSoulSkill, heroId.ToString());

                if (enhanceNum > 0)
                {
                    //魂环升级次数
                    AddTaskNumForType(TaskType.SoulSkillLevelUpgrade, enhanceNum);

                    AddTaskNumForType(TaskType.SoulSkillLevelCountForLevel, enhanceNum, false);

                    //玩家行为
                    RecordAction(ActionType.HerosSoulRingLevel, heroId);
                }
                if (breakNum > 0)
                {
                    //魂环升级次数
                    AddTaskNumForType(TaskType.SoulRingBreak, breakNum);
                }
            }
            else
            {
                //根据等级读取消耗金币，魂尘
                SoulSkillEnhanceModel enhanceModel = SoulSkillLibrary.GetSoulSkillEnhanceMode(oldLevel);
                if (enhanceModel != null)
                {
                    //强化
                    if (!CheckCoins(CurrenciesType.soulDust, enhanceModel.DustCost))
                    {
                        Log.Warn($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: soulDust not enough");
                        resp.Result = (int)ErrorCode.SoulDustNotEnough;
                        Write(resp);
                        return;
                    }
                    if (!CheckCoins(CurrenciesType.gold, enhanceModel.GoldCost))
                    {
                        Log.Warn($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: gold not enough");
                        resp.Result = (int)ErrorCode.GoldNotEnough;
                        Write(resp);
                        return;
                    }

                    enhanceNum = 1;
                    //消耗金币，消耗魂尘
                    costs.Add(CurrenciesType.soulDust, enhanceModel.DustCost);
                    costs.Add(CurrenciesType.gold, enhanceModel.GoldCost);

                    DelCoins(costs, ConsumeWay.EnhanceSoulSkill, heroId.ToString());

                    //魂环升级次数
                    AddTaskNumForType(TaskType.SoulSkillLevelUpgrade);

                    //玩家行为
                    RecordAction(ActionType.HerosSoulRingLevel, heroId);
                }
                else
                {
                    if (hero.SoulSkillLevel >= hero.Level)
                    {
                        Log.Error($"player {uid}  hero {heroId} enhance soul skill fail: hero level {hero.Level} limit.");
                        resp.Result = (int)ErrorCode.HeroLevelLimit;
                        Write(resp);
                        return;
                    }

                    //突破
                    //根据等级读取消耗金币，魂尘
                    SoulSkillBreakModel breakModel = SoulSkillLibrary.GetSoulSkillBreakMode(oldLevel);
                    if (breakModel != null)
                    {
                        if (!CheckCoins(CurrenciesType.soulBreath, breakModel.BreathCost))
                        {
                            Log.Warn($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: soulBreath not enough");
                            resp.Result = (int)ErrorCode.SoulBreathNotEnough;
                            Write(resp);
                            return;
                        }
                        if (!CheckCoins(CurrenciesType.gold, breakModel.GoldCost))
                        {
                            Log.Warn($"player {uid}  hero {heroId} enhance soulring {soulRingUid} fail: gold not enough");
                            resp.Result = (int)ErrorCode.GoldNotEnough;
                            Write(resp);
                            return;
                        }
                        enhanceNum = 1;

                        //消耗金币，消耗魂息
                        costs.Add(CurrenciesType.soulBreath, breakModel.BreathCost);
                        costs.Add(CurrenciesType.gold, breakModel.GoldCost);

                        DelCoins(costs, ConsumeWay.BreakSoulSkill, heroId.ToString());

                        //魂环突破次数
                        AddTaskNumForType(TaskType.SoulRingBreak);
                    }
                }
            }

            hero.SetSoulSkillLevel(oldLevel + enhanceNum);

            NatureDataModel oldNatureDic = SoulSkillLibrary.GetBasicNatureIncrModel(oldLevel);
            NatureDataModel incrNatureDic = SoulSkillLibrary.GetBasicNatureIncrModel(hero.SoulSkillLevel);

            Dictionary<NatureType, float> enhanceDic = new Dictionary<NatureType, float>();
            if (oldNatureDic == null)
            {
                enhanceDic = incrNatureDic.NatureList;
            }
            else
            {
                if (incrNatureDic != null)
                {
                    foreach (var item in incrNatureDic.NatureList)
                    {
                        float value = item.Value - oldNatureDic.NatureList[item.Key];
                        enhanceDic.Add(item.Key, value);
                    }
                }
            }
            hero.AddSoulSkillNature(enhanceDic);

            HeroMng.InitHeroNatureInfo(hero);
            HeroMng.NotifyClientBattlePowerFrom(heroId);
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            resp.Result = (int)ErrorCode.Success;
            Write(resp);

            //养成
            BIRecordDevelopLog(DevelopType.SoulRingLevel, hero.Id, oldLevel, hero.SoulSkillLevel, hero.Id, hero.Level);
        }

        /// <summary>
        /// type 提升到的等级
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="type"></param>
        /// <param name="costs"></param>
        /// <param name="enhanceNum"></param>
        /// <param name="breakNum"></param>
        /// <returns></returns>
        internal ErrorCode SoulRingOneKeyEnhance(int heroId, int type, Dictionary<CurrenciesType, int> costs, out int enhanceNum, out int breakNum)
        {
            breakNum = 0;
            enhanceNum = 0;

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null || type > hero.Level || hero.SoulSkillLevel >= hero.Level || type <= hero.SoulSkillLevel || type % 10 != 0)
            {
                Log.Warn($"player {uid}  hero {heroId} one key enhance fail: cannot find hero {heroId} or level limit.");
                return ErrorCode.Fail;
            }

            int oldLevel = hero.SoulSkillLevel;

            int soulDust = 0;
            int soulBreath = 0;
            int gold = 0;

            int tempSoulDust = 0;
            int tempSoulBreath = 0;
            int tempGold = 0;

            int templevel = oldLevel;
            int enhanceLevel = type - hero.SoulSkillLevel;

            while (enhanceLevel > 0)
            {
                var isEnhance = false;
                SoulSkillEnhanceModel enhanceModel = SoulSkillLibrary.GetSoulSkillEnhanceMode(templevel);
                if (enhanceModel == null)
                {
                    SoulSkillBreakModel breakModel = SoulSkillLibrary.GetSoulSkillBreakMode(templevel);
                    if (breakModel == null)
                    {
                        Log.Error($"player {uid}  hero {heroId} one key enhance fail: soul ring {hero.Level} can not failed breach.please check xml");
                        return ErrorCode.Fail;
                    }
                    tempSoulBreath += breakModel.BreathCost;
                    tempGold += breakModel.GoldCost;
                }
                else
                {
                    tempSoulDust += enhanceModel.DustCost;
                    tempGold += enhanceModel.GoldCost;
                    isEnhance = true;
                }

                if (!CheckCoins(CurrenciesType.soulDust, tempSoulDust + soulDust))
                {
                    if (enhanceNum == 0)
                    {
                        Log.Warn($"player {uid}  hero {heroId} one key enhance fail: soul ring {hero.Level} soulDust not enough");
                        return ErrorCode.SoulDustNotEnough;
                    }
                    break;
                }
                if (!CheckCoins(CurrenciesType.gold, tempGold + gold))
                {
                    if (enhanceNum == 0)
                    {
                        Log.Warn($"player {uid}  hero {heroId} one key enhance fail: soul ring {hero.Level} gold not enough");
                        return ErrorCode.GoldNotEnough;
                    }
                    break;
                }

                if (!CheckCoins(CurrenciesType.soulBreath, tempSoulBreath + soulBreath))
                {
                    if (enhanceNum == 0)
                    {
                        Log.Warn($"player {uid}  hero {heroId} one key enhance fail: soul ring {hero.Level} soulBreath not enough");
                        return ErrorCode.SoulBreathNotEnough;
                    }
                    break;
                }

                --enhanceLevel;
                ++templevel;

                //突破的时候再计算道具消耗，避免到下一次突破的时候道具不够
                if (!isEnhance)
                {
                    ++breakNum;
                    gold += tempGold;
                    soulDust += tempSoulDust;
                    soulBreath += tempSoulBreath;

                    tempGold = 0;
                    tempSoulDust = 0;
                    tempSoulBreath = 0;

                    enhanceNum = templevel - oldLevel;
                }
            }

            //消耗金币，消耗魂尘
            costs.Add(CurrenciesType.soulDust, soulDust);
            costs.Add(CurrenciesType.soulBreath, soulBreath);
            costs.Add(CurrenciesType.gold, gold);

            return ErrorCode.Success;
        }

        internal void SoulSkillReset(HeroInfo heroInfo, RewardManager rewardManager)
        {
            List<BaseItem> items = new List<BaseItem>();

            if (heroInfo.SoulSkillLevel > 0)
            {
                int totalGold = 0;
                int totalDust = 0;
                int totalBreath = 0;
                for (int i = 0; i < heroInfo.SoulSkillLevel; i++)
                {
                    if (i % 10 == 9)
                    {
                        SoulSkillBreakModel breakModel = SoulSkillLibrary.GetSoulSkillBreakMode(i);
                        if (breakModel != null)
                        {
                            totalGold += breakModel.GoldCost;
                            totalBreath += breakModel.BreathCost;
                        }
                    }
                    else
                    {
                        SoulSkillEnhanceModel enhanceModel = SoulSkillLibrary.GetSoulSkillEnhanceMode(i);
                        if (enhanceModel != null)
                        {
                            totalGold += enhanceModel.GoldCost;
                            totalDust += enhanceModel.DustCost;
                        }
                    }
                }

                if (totalGold > 0)
                {
                    rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, totalGold));
                }
                if (totalDust > 0)
                {
                    rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulDust, totalDust));
                }
                if (totalBreath > 0)
                {
                    rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulBreath, totalBreath));
                }

                heroInfo.SoulSkillLevel = 0;
            }
        }

        internal void SoulRingReset(int heroId, RewardManager rewardManager)
        {
            List<BaseItem> items = new List<BaseItem>();

            Dictionary<int, SoulRingItem> equipedSoulRings = SoulRingManager.GetAllEquipedSoulRings(heroId);
            if (equipedSoulRings != null)
            {
                foreach (var kv in equipedSoulRings)
                {

                    SoulRingItem soulRing = kv.Value;

                    if (soulRing.Level > 1)
                    {
                        int totalGold = 0;
                        int totalDust = 0;
                        int totalBreath = 0;
                        for (int i = 1; i <= soulRing.Level - 1; i++)
                        {
                            if (i % 10 == 9)
                            {
                                SoulRingBreakModel breakModel = SoulRingLibrary.GetSoulRingBreakCostMode(i);
                                if (breakModel != null)
                                {
                                    totalGold += breakModel.GoldCost;
                                    totalBreath += breakModel.BreathCost;
                                    //rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulBreath, breakModel.BreathTotalCost));
                                    //rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, breakModel.GoldTotalCost));
                                }
                            }
                            else
                            {
                                SoulRingEnhanceModel enhanceModel = SoulRingLibrary.GetSoulRingEnhanceMode(i);
                                if (enhanceModel != null)
                                {
                                    totalGold += enhanceModel.GoldCost;
                                    totalDust += enhanceModel.DustCost;
                                    //rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulDust, enhanceModel.DustTotalCost));
                                    //rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, enhanceModel.GoldTotalCost));
                                }
                            }
                        }

                        if (totalGold > 0)
                        {
                            rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold, totalGold));
                        }
                        if (totalDust > 0)
                        {
                            rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulDust, totalDust));
                        }
                        if (totalBreath > 0)
                        {
                            rewardManager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulBreath, totalBreath));
                        }
                    }

                    //string reward = SoulRingLibrary.GetSoulRingRevert(soulRing.Level);
                    //if (!string.IsNullOrEmpty(reward))
                    //{
                    //    rewardManager.AddSimpleReward(reward);
                    //}
                    items.Add(kv.Value);
                }
            }
            List<SoulRingItem> soulRings = bagManager.SoulRingBag.GetHeroAllSoulRing(heroId);
            if (soulRings.Count > 0)
            {
                foreach (var soulRing in soulRings)
                {
                    string reward = SoulRingLibrary.GetSoulRingRevert(soulRing.Level);
                    if (!string.IsNullOrEmpty(reward))
                    {
                        rewardManager.AddSimpleReward(reward);
                    }
                    items.Add(soulRing);
                }
            }

            foreach (var item in items)
            {
                SoulRingItem soulRing = item as SoulRingItem;
                //等级
                soulRing.SetLevel(soulRing.IniLevel);
                soulRing.SetAbsorbState(SoulRingAbsorbState.Deafult);

                SoulRingManager.DelEquipSoulRing(soulRing);

                soulRing.SetPosition(0);
                soulRing.SetEquipHeroId(-1);
                //旧魂环放背包
                BagManager.SoulRingBag.AddItemAndCheckAbsorb(soulRing);
                ///取消正在吸收中的魂环标记
                BagManager.SoulRingBag.DelOnAbsorbFlag(heroId);
                //BagManager.SoulRingBag.SyncDbItemInfo(soulRing);
                BagManager.SoulRingBag.DelItem(soulRing.Uid);

                //删掉放到rewardManager中
                ItemBasicInfo baseInfo = new ItemBasicInfo((int)RewardType.SoulRing, soulRing.Id, 1, soulRing.Year.ToString());
                rewardManager.AddReward(baseInfo);

                soulRing.Delete = true;
                //items.Add(soulRing);

            }
            if (items.Count > 0)
            {
                SyncClientItemsInfo(items);
            }
        }

        internal void GetAllAbsorbInfo()
        {
            Bag_SoulRing bag = (Bag_SoulRing)BagManager.GetBag(MainType.SoulRing);
            List<int> heroIds = bag.GetSoulRingAbsorbHeroList();
            if (heroIds == null || heroIds.Count == 0)
            {
                //Log.Warn($"player {uid} get all absorb info fail,no absorbinfo");
                return;
            }

            //获取魂环 吸收信息
            OperateGetSoulRingAbsorbInfoList absorbInfoList = new OperateGetSoulRingAbsorbInfoList(uid, heroIds);
            server.GameRedis.Call(absorbInfoList, ret =>
            {
                MSG_ZGC_GET_All_ABSORBINFO resp = new MSG_ZGC_GET_All_ABSORBINFO();

                if ((int)ret == 1)
                {
                    foreach (var item in absorbInfoList.Infos)
                    {
                        SOULRING_ABSORB_INFO abInfo = new SOULRING_ABSORB_INFO()
                        {
                            HeroId = item.Value.EquipHeroId,
                            Time = Timestamp.GetUnixTimeStampSeconds(item.Value.AbsorbFinishTime),
                        };
                        abInfo.PlayerIds.AddRange(item.Value.AbsorbHelpList);
                        resp.InfoList.Add(abInfo);
                    }

                    resp.Result = (int)ErrorCode.Success;
                    Write(resp);
                }
                else
                {
                    Log.Warn($"player {uid} get all absorb info fail, not find absorbinfo");
                    resp.Result = (int)ErrorCode.Fail;
                    Write(resp);
                }
            });
        }

        internal void GetAbsorbFriendInfo(RepeatedField<int> uids, int heroId)
        {
            var fields = uids.Select(x => (RedisValue)x);
            MSG_ZGC_GET_FRIEND_INFO resp = new MSG_ZGC_GET_FRIEND_INFO()
            {
                HeroId = heroId
            };
            OperateGetAbsorbFriendInfoListByIds operate = new OperateGetAbsorbFriendInfoListByIds(Uid, fields);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        foreach (var item in operate.Characters)
                        {
                            //这里设置好友当前友好度
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            resp.FriendInfoList.Add(friendInfo);
                        }
                        resp.Result = (int)ErrorCode.Success;
                        Write(resp);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //Log.Error("player {0} execute GetRecentList fail: redis data error!", Uid);
                    return;
                }
            });
        }


        public int GetEquipedHeroSoulRings()
        {
            int count = 0;
            List<int> heros = HeroMng.GetAllHeroPosHeroId();
            foreach (var heroId in heros)
            {
                //魂环
                Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(heroId);
                if (soulRingDic != null)
                {
                    count += soulRingDic.Count;
                }
            }
            return count;
        }

        public void ShowHeroSoulRing()
        {
            //if (IsMoving)
            //{
            //    OnMoveStop();
            //    BroadCastStop();
            //}
            //FsmManager.SetNextFsmStateType(FsmStateType.IDLE);

            MSG_ZGC_SHOW_HERO_SOULRING msg = new MSG_ZGC_SHOW_HERO_SOULRING();
            msg.InstanceId = InstanceId;

            Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(HeroId);
            if (soulRingDic != null)
            {
                foreach (var item in soulRingDic)
                {
                    msg.SoulRings.Add(new HERO_SOULRING_INFO() { Position = item.Key, Year = item.Value.Year });
                }
            }
            BroadCast(msg);
        }

        private void SyncRedisSoulRingInfo(int heroId)
        {
            //魂环替换
            OperateGetSoulRingAbsorbInfo absorbInfo = new OperateGetSoulRingAbsorbInfo(uid, heroId);
            server.GameRedis.Call(absorbInfo, ret =>
            {
                if ((int)ret == 1)
                {
                    ///删除吸收信息
                    server.GameRedis.Call(new OperateUpdateSoulRingAbsorbInfo(Uid, heroId, 0));
                }
                //else
                //{
                //    response.Result = (int)ErrorCode.Fail;
                //    Log.Error($"player {uid} hero {heroId} replace soulring fail, soul ring redis not exist");
                //}
            });
        }

        public void ReplaceAllBetterSoulRings(int heroId, RepeatedField<GateZ_SOULRING_ITEM> soulRings)
        {
            MSG_ZGC_REPLACE_BETTER_SOULRING response = new MSG_ZGC_REPLACE_BETTER_SOULRING();

            Bag_SoulRing bag = bagManager.SoulRingBag;

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {Uid} replace all better soulrings failed: not find hero {heroId}");
                return;
            }

            //if (bag.CheckIsAbsorbed(heroId)) //已经在吸收别的魂环
            //{
            //    Log.Warn($"player {uid} hero {heroId} is absorbed other soul ring !");
            //    return;
            //}

            response.HeroId = heroId;
            response.Result = (int)ErrorCode.Fail;
            List<BaseItem> itemList = new List<BaseItem>();
            foreach (var item in soulRings)
            {
                List<BaseItem> updateList = new List<BaseItem>();
                ErrorCode result = ReplaceSingleBetterSoulRing(item.SoulRingUid, item.Slot, bag, heroInfo, out updateList);
                if (result == ErrorCode.Success)
                {
                    response.SoulRings.Add(new ZGC_SOULRING_ITEM() { SoulRingUidHigh = item.SoulRingUid.GetHigh(), SoulRingUidLow = item.SoulRingUid.GetLow() });
                    response.Result = (int)result;
                    if (updateList.Count > 0)
                    {
                        itemList.AddRange(updateList);
                    }
                }
            }
            if (itemList.Count > 0)
            {
                SyncClientItemsInfo(itemList);
            }
            Write(response);
        }

        private ErrorCode ReplaceSingleBetterSoulRing(ulong soulRingUid, int slot, Bag_SoulRing bag, HeroInfo heroInfo, out List<BaseItem> updateList)
        {
            updateList = new List<BaseItem>();
            ErrorCode result = ErrorCode.Fail;
            //获取魂环
            SoulRingItem soulRing = (SoulRingItem)bag.GetItem(soulRingUid);
            if (soulRing == null || slot <= 0)
            {
                Log.Error($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail, because this ring not exist");
                result = ErrorCode.SoulRingNotExist;
                return result;
            }

            SoulRingSlotUnlockModel unlockModel = SoulRingLibrary.GetSoulRingSlotUnlockMode(slot);
            if (heroInfo.Level < unlockModel.Level)
            {
                Log.Warn($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail, level limit");
                result = ErrorCode.SoulRingABLevelLimit;
                return result;
            }

            int absorbHeroLevel = soulRing.GetAbsorbHeroLevel();
            if (heroInfo.Level < absorbHeroLevel)
            {
                Log.Warn($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail,hero level not enough");
                result = ErrorCode.SoulRingABHeroLevelLimit;
                return result;
            }
            SoulRingItem onAbsortSoulRing = (SoulRingItem)bag.GetOnAbsorbSoulRingByHeroId(heroInfo.Id);
            if (onAbsortSoulRing != null && onAbsortSoulRing.Position == slot)
            {
                Log.Warn($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail, slot {slot} soulring is on absort");
                result = ErrorCode.SoulRingOnAbsort;
                return result;
            }
            //获取当前槽魂环
            SoulRingItem equipedSoulRing = SoulRingManager.GetEquipedSoulRing(heroInfo.Id, slot);
            if (equipedSoulRing == null)
            {
                if (SoulRingManager.CheckRepeated(heroInfo.Id, soulRing.modelId))
                {
                    Log.Warn($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail,soulring  repeated {soulRing.modelId}");
                    result = ErrorCode.SoulRingOwnMonsterRepeated;
                    return result;
                }
                SoulRingAbsorbAdd(heroInfo.Id, soulRing, slot);

                //魂环替换
                BIRecordRingReplaceLog(heroInfo.Id, heroInfo.Level, slot, 0, 0, soulRing.Id, soulRing.Year);
                //养成
                BIRecordDevelopLog(DevelopType.EquipSoulRing, soulRing.Id, 0, soulRing.Year, heroInfo.Id, heroInfo.Level);
            }
            else
            {
                //吸收中的魂环不可替换
                if ((SoulRingAbsorbState)equipedSoulRing.SoulRingInfo.AbsorbState == SoulRingAbsorbState.OnAbsort)
                {
                    Log.Warn($"player {uid} hero {heroInfo.Id} replace batter soul ring {soulRingUid} fail,soulring is onAbsort");
                    result = ErrorCode.SoulRingOnAbsort;
                    return result;
                }
                SoulRingAbsortSwap(equipedSoulRing, soulRing);

                //魂环替换
                BIRecordRingReplaceLog(heroInfo.Id, heroInfo.Level, slot, equipedSoulRing.Id, equipedSoulRing.Year, soulRing.Id, soulRing.Year);
                //养成
                BIRecordDevelopLog(DevelopType.EquipSoulRing, soulRing.Id, equipedSoulRing.Year, soulRing.Year, heroInfo.Id, heroInfo.Level);
            }
            result = ErrorCode.Success;

            //记录魂环吸收信息
            if (equipedSoulRing == null)
            {
                bag.SyncDbItemInfo(soulRing);
                updateList.Add(soulRing);
            }
            else
            {
                List<BaseItem> tempList = ReplaceSoulRing(heroInfo.Id, equipedSoulRing.Uid, soulRing.Uid, out result, true);
                if (result == ErrorCode.Success)
                {
                    updateList.AddRange(tempList);
                }
            }
            return result;
        }

        private List<BaseItem> ReplaceSoulRing(int heroId, ulong oldSoulRingUid, ulong newSoulRingUid, out ErrorCode result, bool replaceAll = false)
        {
            List<BaseItem> updateList = new List<BaseItem>();
            Bag_SoulRing bag = bagManager.SoulRingBag;
            SoulRingItem newItem = (SoulRingItem)bag.GetItem(newSoulRingUid);
            if (newItem == null)
            {
                Log.Error($"player {uid} hero {heroId} replace soulring fail, soul ring  {newSoulRingUid} not exist in bag");
                result = ErrorCode.Fail;
                return updateList;
            }
            SoulRingItem oldItem = SoulRingManager.GetEquipedSoulRing(heroId, newItem.Position);
            if (oldItem == null)
            {
                Log.Error($"player {uid} hero {heroId} replace soulring fail, soul ring  {oldSoulRingUid} not exist in bag");
                result = ErrorCode.Fail;
                return updateList;
            }
            //判断是否是年限更高的
            if (replaceAll && newItem.SoulRingInfo.Year <= oldItem.SoulRingInfo.Year)
            {
                Log.Error($"player {uid} hero {heroId} replace soulring fail, new soul ring  {newSoulRingUid} year {newItem.SoulRingInfo.Year} not higher than old soul ring {oldSoulRingUid} year {oldItem.SoulRingInfo.Year}");
                result = ErrorCode.SoulRingSwapYearWrong;
                return updateList;
            }
            SoulRingSwap(oldItem, newItem);

            //替换完毕清除吸收标记
            if (!replaceAll)
            {
                bag.DelOnAbsorbFlag(heroId);
            }

            bag.SyncDbItemInfo(oldItem);
            bag.SyncDbItemInfo(newItem);

            result = ErrorCode.Success;

            //吸收魂环次数
            AddTaskNumForType(TaskType.SoulRingAbsorb);
            //装备指定等级魂环
            AddTaskNumForType(TaskType.EquipSoulRingForLevel, 1, true, newItem.Level);
            //装备魂环个数
            AddTaskNumForType(TaskType.EquipSoulRing, SoulRingManager.GetEquipedCount(), false);
            //装备指定年限魂环个数
            AddTaskNumForType(TaskType.EquipSoulRingForYear);
            //装备位置魂环
            AddTaskNumForType(TaskType.EquipSoulRingBySlot, 1, true, newItem.Position);
            //出战伙伴装备魂环
            AddTaskNumForType(TaskType.EquipHeroSoulRing, GetEquipedHeroSoulRings(), false);

            updateList.Add(newItem);
            updateList.Add(oldItem);

            return updateList;
        }

        public void SelectSoulRingElement(int heroId, int pos, int elementId)
        {
            MSG_ZGC_SELECT_SOULRING_ELEMENT response = new MSG_ZGC_SELECT_SOULRING_ELEMENT();

            SoulRingItem soulRingItem = SoulRingManager.GetEquipedSoulRing(heroId, pos);
            if (soulRingItem == null)
            {
                Log.Error($"player {uid} hero {heroId} SelectSoulRingElement fail, soul ring  {pos} not exist in bag");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            SoulRingModel model = SoulRingLibrary.GetSoulRingMode(soulRingItem.modelId);
            var elementModel = SoulRingLibrary.GetSoulRingElementModel(elementId);
            if (elementModel == null || model == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (soulRingItem.Year < SoulRingLibrary.SoulRingConfig.ElementYearLimit)
            {
                response.Result = (int)ErrorCode.SoulRingYearNotEnough;
                Write(response);
                return;
            }

            if (model.Element?.Contains(elementId) == true)
            {
                soulRingItem.SetElement(elementId);

                BagManager.SoulRingBag.SyncDbItemInfo(soulRingItem);
                SyncClientItemsInfo(new List<BaseItem>() { soulRingItem });

                response.HeroId = heroId;
                response.ElementId = elementId;
                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Error($"player {uid} hero {heroId} SelectSoulRingElement fail, soul ring  id {soulRingItem.Id} not exist element {elementId} ");
            }

            Write(response);

            HeroMng.UpdateBattlePower(heroId);
            HeroMng.NotifyClientBattlePowerFrom(heroId);
        }

        private void ReplaceSoulRingElement(SoulRingItem oldItem, SoulRingItem newItem)
        {
            if (oldItem == null || newItem == null) return;

            //替换为相同魂环，并且年限更大则继承之前魂环选择的元素
            if (newItem.Id == oldItem.Id && newItem.Year >= SoulRingLibrary.SoulRingConfig.ElementYearLimit)
            {
                newItem?.SetElement(oldItem.Element);
            }

            oldItem.SetElement(0);
        }
    }
}

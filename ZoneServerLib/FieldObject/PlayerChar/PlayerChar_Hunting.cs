using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public HuntingManager HuntingManager { get; private set; }

        public void InitHuntingManager()
        {
            this.HuntingManager = new HuntingManager(this);
        }

        public bool HuntingCheckResearch(int dungeonId)
        {
            HuntingModel model = HuntingLibrary.GetByMapId(dungeonId);
            return HuntingManager.Research >= model?.ResearchLimit;
        }

        public void GetHuntingInfo()
        {
            SendHuntingInfo();
        }

        public void HuntingReward(RewardManager manager, DungeonModel model, DungeonMap dungeonMap, bool updateRecord = true)
        {
            if (HuntingCheckIsNormalReward(dungeonMap))
            {
                HuntingAddReward(manager, model);
                UpdateRecord(model);
            }
            else
            {
                //帮杀次数用完
                if (CheckCounter(CounterType.TeamHelpCount))
                {
                    NotifyDungeonHelpUeslessRewardMsg(model.Id);
                    return;
                }

                manager = new RewardManager();
                manager.InitSimpleReward(HuntingLibrary.OfflineHelpReward);
                UpdateCounter(CounterType.TeamHelpCount, 1);
            }

            //玩家还在副本中，通知前端奖励, 避免出现玩家在主城弹出结算面板
            if (CurrentMap?.IsDungeon == true)
            {
                //先添加魂环奖励  再添加其他奖励
                MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                //有限魂环信息
                RewardManagerEx.GenerateRewardItemInfo(rewardMsg.Rewards, RewardType.SoulRing, manager.SpecialRewards);

                manager.GenerateRewardMsg(rewardMsg.Rewards);

                rewardMsg.DungeonId = model.Id;
                rewardMsg.Result = (int)dungeonMap.DungeonResult;
                Write(rewardMsg);
            }
        }

        //非组队、正常组队和请求协助的发起者正常奖励
        internal bool HuntingCheckIsNormalReward(DungeonMap dungeon)
        {
            //不是组队副本
            TeamDungeonMap dungeonMap = dungeon as TeamDungeonMap;
            if (dungeonMap == null) return true;

            //正常组队副本
            if (!dungeonMap.IsHelpDungeon)
            {
                return true;
            }
            else
            {
                //请求发起者正常奖励
                if (uid == dungeonMap.AskHelpUid)
                {
                    UpdateCounter(CounterType.AskTopRankHelp, 1);
                    return true;
                }

                //请求协助者协助奖励
                return false;
            }
        }

        private void UpdateRecord(DungeonModel model)
        {
            if (HuntingLibrary.IsActivityDungeon(model.Id))
            {
                HuntingActivityModel huntingActivityModel = HuntingLibrary.GetHuntingActivityModelByMapId(model.Id);
                if (huntingActivityModel != null)
                {
                    HuntingManager.AddResearch(GetHuntingPassResearch(model.Difficulty));
                    HuntingManager.AddActivityPassed(huntingActivityModel.Id);
                }
            }
            else
            {
                HuntingModel huntingModel = HuntingLibrary.GetByMapId(model.Id);
                if (huntingModel != null)
                {
                    HuntingManager.AddResearch(GetHuntingPassResearch(model.Difficulty));
                    HuntingManager.AddPassedId(huntingModel.Id);
                }
            }

            SendHuntingInfo();
        }

        private int GetHuntingPassResearch(DungeonDifficulty difficulty)
        {
            switch (difficulty)
            {
                case DungeonDifficulty.Easy: return HuntingLibrary.ResearchEasy;
                case DungeonDifficulty.Hard: return HuntingLibrary.ResearchHard;
                case DungeonDifficulty.Devil: return HuntingLibrary.ResearchDevil;
                default: return 0;
            }
        }

        private void HuntingAddReward(RewardManager manager, DungeonModel model)
        {
            // 发放奖励
            AddSoulRingFromHunting(manager, model);

            //猎杀魂兽副本发放完奖励后需要从 manager中共删除soulring，否则会在AddRewards中重复发放
            manager.RemoveReward(RewardType.SoulRing);

            AddRewards(manager, ObtainWay.Hunting, model.Data.Name);
        }

        private SoulRingItem AddSoulRingFromHunting(RewardManager manager, DungeonModel dungeonModel)
        {
            if (HuntingLibrary.IsActivityDungeon(dungeonModel.Id))
            {
                if (HuntingLibrary.GetHuntingActivityModelByMapId(dungeonModel.Id) == null) return null;
            }
            else
            {
                if (HuntingLibrary.GetByMapId(dungeonModel.Id) == null) return null;
            }

            int research = Math.Min(HuntingLibrary.ResearchMax, HuntingManager.Research + HuntingLibrary.GetResearch(dungeonModel.Difficulty));
            SoulRingItem maxYearItem = AddSoulRing(manager, ObtainWay.Hunting, (int)dungeonModel.Difficulty, research);

            //魂环推送
            if (maxYearItem != null)
            {
                //广播掉落最高年份魂环
                BroadcastDropMaxYearSoulRing(maxYearItem, ActivityType.Hunting);

                //在狩猎魂兽中通关全部魂兽的所有难度
                //  BroadcastPassAllDifficultyHunting(ActivityType.Hunting);
            }
            return maxYearItem;
        }

        private SoulRingItem AddSoulRing(RewardManager manager, ObtainWay obtainWay, int difficulty = 1, int serach = 0)
        {
            var items = manager.GetRewardList(RewardType.SoulRing);
            if (items == null)
            {
                return null;
            }

            SoulRingItem maxYearItem = null;
            List<BaseItem> syncList = new List<BaseItem>();
            List<SoulRingItem> rewardItems = new List<SoulRingItem>();
            foreach (var kv in items)
            {
                Tuple<SoulRingItem, bool> item = BagManager.SoulRingBag.AddSoulRing(kv.Key, kv.Value, difficulty, serach, ref syncList, ref maxYearItem, ref rewardItems);
                if (item != null && item.Item2)
                {
                    RecordObtainLog(obtainWay, RewardType.SoulRing, item.Item1.Id, 0, kv.Value);
                    //获取埋点
                    BIRecordObtainItem(RewardType.SoulRing, obtainWay, item.Item1.Id, kv.Value, 1, item.Item1.Year);
                }
            }

            foreach (var kv in rewardItems)
            {
                ItemBasicInfo info = new ItemBasicInfo((int)RewardType.SoulRing, kv.Id, 1, new string[] { kv.Year.ToString() });
                manager.AddSpecialReward(info);
            }

            SyncClientItemsInfo(syncList);

            foreach (SoulRingItem item in syncList)
            {
                //拥有一个N年份魂环
                AddTaskNumForType(TaskType.OwnSoulRingForYear, 1, true, item.Year);

                //获得一个指定年份的魂环
                List<int> paramList = new List<int>() { item.Year };
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.GetHighYearSoulRing, 1, paramList);
            }


            if (maxYearItem != null && InDungeon)
            {
                NotifyHuntingDropSoulRing(maxYearItem);
            }

            return maxYearItem;
        }

        #region 协议

        public void SendHuntingInfo()
        {
            MSG_ZGC_HUNTING_INFO response = HuntingManager.GenerateHuntingMsg();
            Write(response);
        }

        public void HuntingSweep(int id)
        {
            MSG_ZGC_HUNTING_SWEEP msg = new MSG_ZGC_HUNTING_SWEEP();

            HuntingModel model = HuntingLibrary.Get(id);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} hunting sweep {id} failed: not find in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(model.EasyMapId);
            if (dungeonModel == null)
            {
                Logger.Log.Warn($"player {Uid} hunting sweep {id} failed: not find dungeon");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!HuntingManager.CheckPassed(id))
            {
                Logger.Log.Warn($"player {Uid} hunting sweep {id} failed: not pass cur hunting");
                msg.Result = (int)ErrorCode.HuntingCurrNotPassedCanNotSweep;
                Write(msg);
                return;
            }

            bool useItem = false;
            if (GetCounter(CounterType.HuntingCount).Count <= 0)
            {
                ErrorCode errorCode = ErrorCode.HuntingSweepCountNotEnough;
                int num = 1;
                //没有次数检查扫荡券
                BaseItem item = BagManager.GetItem(MainType.Consumable, HuntingLibrary.SweepItem);
                if (!CheckItemInfo(item, num, ref errorCode))
                {
                    Logger.Log.Warn($"player {Uid} hunting sweep {id} failed: hunting count not enough");
                    msg.Result = (int)ErrorCode.HuntingSweepCountNotEnough;
                    Write(msg);
                    return;
                }

                //扣除扫荡券
                BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.Hunting);
                if (baseItem != null)
                {
                    SyncClientItemInfo(item);
                    //使用消耗品
                    AddTaskNumForType(TaskType.UseConsumable, 1, true, 1);
                }
                useItem = true;
            }

            if (!useItem)
            {
                UpdateCounter(CounterType.HuntingCount, -1);
            }

            RewardManager manager = new RewardManager();
            //manager.InitSimpleReward(dungeonModel.Data.GetString("GeneralReward"));
            List<ItemBasicInfo> getList = AddRewardDrop(dungeonModel.Data.GetIntList("GeneralRewardId", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards();

            HuntingAddReward(manager, dungeonModel);

            //有限魂环信息
            RewardManagerEx.GenerateRewardItemInfo(msg.Rewards, RewardType.SoulRing, manager.SpecialRewards);
            manager.GenerateRewardMsg(msg.Rewards);



            msg.Result = (int)ErrorCode.Success;
            Write(msg);


            MapModel tempMapModel = MapLibrary.GetMap(model.EasyMapId);
            AddTaskNumForType(TaskType.CompleteDungeons, 1, true, tempMapModel.MapType);
            AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, dungeonModel.Id);
            AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, dungeonModel.Id);
            AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, tempMapModel.MapType);
            //完成通行证任务
            AddPassCardTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddPassCardTaskNum(TaskType.CompleteOneDungeon, dungeonModel.Id, TaskParamType.DUNGEON);
            //完成学院任务
            AddSchoolTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddSchoolTaskNum(TaskType.CompleteOneDungeon, dungeonModel.Id, TaskParamType.DUNGEON);
            AddSchoolTaskNum(TaskType.CompleteDungeonList, dungeonModel.Id, TaskParamType.DUNGEON_LIST);

            //漂流探宝
            AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, tempMapModel.MapType);
            AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, tempMapModel.MapType);
            //日志
            BIRecordCheckPointLog(MapType.HuntingTeamDevil, dungeonModel.Id.ToString(), 1, 0);
        }


        private void NotifyHuntingDropSoulRing(SoulRingItem item)
        {
            MSG_ZGC_HUNTING_DROP_SOULRING notify = new MSG_ZGC_HUNTING_DROP_SOULRING()
            {
                Id = item.Id,
                Year = item.Year
            };
            Write(notify);
        }

        public void ContinueHunting(bool isContinue)
        {
            MSG_ZGC_CONTINUE_HUNTING response = new MSG_ZGC_CONTINUE_HUNTING();

            if (Team != null && !IsCaptain())
            {
                Logger.Log.Warn($"player {Uid} continue hunting failed: not team captain");
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            MSG_ZR_NOTIFY_TEAM_CONT_HUNTING request = new MSG_ZR_NOTIFY_TEAM_CONT_HUNTING();
            request.Uid = Uid;
            if (Team != null)
            {
                request.TeamId = Team.TeamId;
            }
            request.Continue = isContinue;
            server.SendToRelation(request);
        }

        public void ChangeHuntingState(bool isContinue, int result = 1)
        {
            MSG_ZGC_CONTINUE_HUNTING response = new MSG_ZGC_CONTINUE_HUNTING();
            response.Continue = isContinue;
            if (result == (int)ErrorCode.Success)
            {
                bool canChange = HuntingManager.ChangeHuntingState(isContinue);
                if (canChange)
                {
                    response.Result = (int)ErrorCode.Success;
                    Write(response);
                    return;
                }
            }
            response.Result = result;
            Write(response);
        }

        public void HuntingActivityUnlock(int id)
        {
            MSG_ZGC_HUNTING_ACTICITY_UNLOCK msg = new MSG_ZGC_HUNTING_ACTICITY_UNLOCK();

            HuntingActivityModel model = HuntingLibrary.GetHuntingActivityModel(id);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: not find in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(model.SingleMapId);
            if (dungeonModel == null)
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: not find dungeon");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (HuntingManager.IsActivityUnlocked(id))
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: already unlocked");
                msg.Result = (int)ErrorCode.HuntingActivityUnlocked;
                Write(msg);
                return;
            }

            Data data = DataListManager.inst.GetData("HuntingActivityUnlockCost", HuntingManager.HuntingActivityUnlockList.Count);
            if (data == null)
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: not find unlock item in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            string costItemStr = data.GetString("UnlockCostItem");
            if (string.IsNullOrEmpty(costItemStr))
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: not find unlock item in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            ItemBasicInfo basicInfo = ItemBasicInfo.Parse(costItemStr);
            NormalItem item = bagManager.NormalBag.GetItemBySubType(basicInfo.Id);
            if (basicInfo == null || item == null || item.PileNum < basicInfo.Num)
            {
                Logger.Log.Warn($"player {Uid} unlock hunting activity {id} failed: unlock item error");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, basicInfo.Num, ConsumeWay.Hunting);
            if (baseItem != null)
            {
                SyncClientItemInfo(baseItem);
            }

            HuntingManager.AddActivityUnlocked(id);

            msg.Result = (int)ErrorCode.Success;
            msg.UnlockedId = id;
            msg.UnlockedList.AddRange(HuntingManager.HuntingActivityUnlockList);

            Write(msg);
        }

        public void HuntingActivitySweep(int id, int type)
        {
            //type 1 -解锁后扫荡
            //type 2 -未解锁扫荡 需要消耗当局
            MSG_ZGC_HUNTING_ACTICITY_SWEEP msg = new MSG_ZGC_HUNTING_ACTICITY_SWEEP() { Type = type };

            HuntingActivityModel model = HuntingLibrary.GetHuntingActivityModel(id);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} hunting activity sweep {id} failed: not find in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(model.SingleMapId);
            if (dungeonModel == null)
            {
                Logger.Log.Warn($"player {Uid} hunting activity sweep {id} failed: not find dungeon");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            switch (type)
            {
                case 1:
                    {
                        if (!HuntingManager.IsActivityPassed(id))
                        {
                            Logger.Log.Warn($"player {Uid} hunting activity sweep {id} failed: not pass cur hunting");
                            msg.Result = (int)ErrorCode.HuntingCurrNotPassedCanNotSweep;
                            Write(msg);
                            return;
                        }

                        bool useItem = false;
                        if (GetCounter(CounterType.HuntingCount).Count <= 0)
                        {
                            ErrorCode errorCode = ErrorCode.HuntingSweepCountNotEnough;
                            int num = 1;
                            //没有次数检查扫荡券
                            BaseItem item = BagManager.GetItem(MainType.Consumable, HuntingLibrary.SweepItem);
                            if (!CheckItemInfo(item, num, ref errorCode))
                            {
                                Logger.Log.Warn($"player {Uid} hunting sweep {id} failed: hunting count not enough");
                                msg.Result = (int)ErrorCode.HuntingSweepCountNotEnough;
                                Write(msg);
                                return;
                            }

                            //扣除扫荡券
                            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.Hunting);
                            if (baseItem != null)
                            {
                                SyncClientItemInfo(item);
                                //使用消耗品
                                AddTaskNumForType(TaskType.UseConsumable, 1, true, 1);
                            }

                            useItem = true;
                        }

                        if (!useItem)
                        {
                            UpdateCounter(CounterType.HuntingCount, -1);
                        }
                    }
                    break;
                case 2:
                    {
                        NormalItem item = bagManager.NormalBag.GetItemBySubType(HuntingLibrary.UnlockSweepItem.Id);
                        if (item == null || item.PileNum < HuntingLibrary.UnlockSweepItem.Num)
                        {
                            Logger.Log.Warn($"player {Uid} unlock sweep hunting activity {id} failed: unlock item error");
                            msg.Result = (int)ErrorCode.ItemNotEnough;
                            Write(msg);
                            return;
                        }

                        BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, HuntingLibrary.UnlockSweepItem.Num, ConsumeWay.Hunting);
                        if (baseItem != null)
                        {
                            SyncClientItemInfo(item);
                            //使用消耗品
                            AddTaskNumForType(TaskType.UseConsumable, HuntingLibrary.UnlockSweepItem.Num, false, 1);
                        }

                        HuntingManager.AddResearch(GetHuntingPassResearch(dungeonModel.Difficulty), true);
                        HuntingManager.AddActivityUnlocked(id);
                        HuntingManager.AddActivityPassed(id);
                        msg.ActivityPassedList.Add(HuntingManager.HuntingActivityList);
                        msg.ActivityUnlockList.Add(HuntingManager.HuntingActivityUnlockList);
                    }
                    break;
                default:
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
            }


            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> getList = AddRewardDrop(dungeonModel.Data.GetIntList("GeneralRewardId", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards();

            HuntingAddReward(manager, dungeonModel);

            //有限魂环信息
            RewardManagerEx.GenerateRewardItemInfo(msg.Rewards, RewardType.SoulRing, manager.SpecialRewards);
            manager.GenerateRewardMsg(msg.Rewards);


            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            MapModel tempMapModel = MapLibrary.GetMap(model.SingleMapId);
            AddTaskNumForType(TaskType.CompleteDungeons, 1, true, tempMapModel.MapType);
            AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, dungeonModel.Id);
            AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, dungeonModel.Id);
            AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, tempMapModel.MapType);
            //完成通行证任务
            AddPassCardTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddPassCardTaskNum(TaskType.CompleteOneDungeon, dungeonModel.Id, TaskParamType.DUNGEON);
            //完成学院任务
            AddSchoolTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddSchoolTaskNum(TaskType.CompleteOneDungeon, dungeonModel.Id, TaskParamType.DUNGEON);
            AddSchoolTaskNum(TaskType.CompleteDungeonList, dungeonModel.Id, TaskParamType.DUNGEON_LIST);

            //漂流探宝
            AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, tempMapModel.MapType);
            AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, tempMapModel.MapType);
            //日志
            BIRecordCheckPointLog(MapType.HuntingTeamDevil, dungeonModel.Id.ToString(), 1, 0);
        }

        public void HuntingHelp(int dungeonId)
        {
            PlayerChar player = null;
            ErrorCode errorCode = CheckHuntingHelp(dungeonId, ref player);
            MSG_ZGC_HUNTING_HELP response = new MSG_ZGC_HUNTING_HELP() { DungeonId = dungeonId };
            if (errorCode != ErrorCode.Success)
            {
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            if (player != null && !Team.IsInviteMirror)
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);

                MSG_ZGC_HUNTING_HELP_ASK msg = new MSG_ZGC_HUNTING_HELP_ASK()
                {
                    CapUid = uid,
                    CapName = Name,
                    CapLevel = Level,
                    DungeonId = dungeonId
                };
                player.Write(msg);
                return;
            }
            else
            {
                foreach (var kv in Team.MemberList)
                {
                    if (kv.Key == uid) continue;

                    //机器人，离线好友自动同意
                    if (kv.Value.IsRobot || Team.IsInviteMirror || (!kv.Value.IsOnline && kv.Value.IsAllowOffline))
                    {
                        response.Result = (int)ErrorCode.Success;
                        Write(response);


                        MSG_ZGC_HUNTING_HELP_ANSWER_JOIN msg = new MSG_ZGC_HUNTING_HELP_ANSWER_JOIN() { Agree = true };
                        Write(msg);
                        return;
                    }
                }
            }

            MSG_ZR_HUNTING_HELP request = new MSG_ZR_HUNTING_HELP()
            {
                CapUid = uid,
                CapName = Name,
                CapLevel = Level,
                DungeonId = dungeonId
            };
            server.SendToRelation(request, uid);
        }

        private ErrorCode CheckHuntingHelp(int dungeonId, ref PlayerChar tergetPlayer)
        {
            DungeonModel model = DungeonLibrary.GetDungeon(dungeonId);
            if (model == null) return ErrorCode.Fail;

            if (!model.CheckMemberCountLimit(Team.MemberCount))
            {
                return ErrorCode.DungeonMemberCountLimit;
            }

            if (!IsCaptain())
            {
                return ErrorCode.NotTeamCaptain;
            }

            if (CheckCounter(CounterType.AskTopRankHelp))
            {
                return ErrorCode.AskTopRankCountNotEnough;
            }

            if (NotStableInMap() || currentMap.IsDungeon)
            {
                return ErrorCode.InDungeon;
            }

            if (Team == null)
            {
                return ErrorCode.NotInTeam;
            }

            if (Team.MemberCount <= 1)
            {
                return ErrorCode.Fail;
            }

            int memberId = Team.MemberList.FirstOrDefault(x => x.Key != uid).Key;
            if (memberId == 0)
            {
                return ErrorCode.Fail;
            }

            tergetPlayer = server.PCManager.FindPc(memberId);
            if (tergetPlayer != null)
            {
                if (tergetPlayer.CheckBlackExist(uid))
                {
                    return ErrorCode.InTargetBlack;
                }

                if (Team?.IsInviteMirror == false)
                {
                    if (tergetPlayer.NotStableInMap() || tergetPlayer.CurrentMap.IsDungeon)
                    {
                        return ErrorCode.InDungeon;
                    }
                }
            }

            return ErrorCode.Success;
        }

        public void HuntingHelpAnswer(int capUid, bool agree)
        {
            if (Team == null || Team.MemberCount <= 1) return;

            //信息变了
            if (Team.CaptainUid != capUid) return;

            PlayerChar tergetPlayer = server.PCManager.FindPc(capUid);

            if (tergetPlayer != null)
            {
                MSG_ZGC_HUNTING_HELP_ANSWER_JOIN msg = new MSG_ZGC_HUNTING_HELP_ANSWER_JOIN() { Agree = agree };
                tergetPlayer.Write(msg);
            }
            else
            {
                MSG_ZR_HUNTING_HELP_ANSWER request = new MSG_ZR_HUNTING_HELP_ANSWER()
                {
                    CapUid = capUid,
                    Agree = agree,
                };
                server.SendToRelation(request, uid);
            }

            //拒绝则退出队伍
            if (!agree)
            {
                if (Team != null && Team.CaptainUid == capUid)
                {
                    MSG_ZR_QUIT_TEAM request = new MSG_ZR_QUIT_TEAM();
                    request.Uid = uid;
                    request.TeamId = Team.TeamId;
                    server.SendToRelation(request);
                }
            }
        }

        public void AddHuntingIntrude()
        {
            if (HuntingManager.CheckCountLimit()) return;

            HuntingIntrudeModel model;
            HuntingIntrudeBuffSuitModel buffSuitModel;
            HuntingLibrary.RandomHuntingIntrude(out model, out buffSuitModel);
            if (model == null)
            {
                Log.Error($"had not random a valid HuntingIntrudeModel ");
                return;
            }

            HuntingIntrudeInfo info = new HuntingIntrudeInfo()
            {
                Uid = uid,
                Id = server.UID.NewIuid(server.MainId, server.SubId),
                IntrudeId = model.Id,
                BuffSuitId = buffSuitModel.Id,
                JobLimit = model.RandomJobLimit(),
                EndTime = BaseApi.now.AddHours(HuntingLibrary.HuntingIntrudeExistHour)
            };

            HuntingManager.AddHuntingIntrudeInfo(info);
            SendHuntingIntrudeInfo();
        }


        public void RefreshHuntingIntrudeOutOfTime()
        {
            HuntingManager.CheckHuntingIntrudeOutOfTime();
        }

        public bool CheckHuntingPeriodBuffEffect()
        {
            return HuntingManager.Research >= HuntingLibrary.PeriodBuffResearchLimit;
        }

        public void SendHuntingIntrudeInfo()
        {
            MSG_ZGC_HUNTING_INTRUDE_INFO response = HuntingManager.GenerateHuntingIntrudeMsg();
            Write(response);
        }

        public void HuntingIntrudeUpdateHeroPos(RepeatedField<MSG_GateZ_HERO_POS> heroPos)
        {
            MSG_ZGC_HUNTING_INTRUDE_HERO_POS msg = new MSG_ZGC_HUNTING_INTRUDE_HERO_POS();
            if (heroPos.Count <= 0)
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            foreach (var kv in heroPos)
            {
                if (HeroMng.GetHeroInfo(kv.HeroId) == null)
                {
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }

            HuntingManager.UpdateHeroPosInfo(heroPos);
            SendHuntingIntrudeInfo();

            HuntingManager.GenerateHuntignIntrudeHeroPosMsg(msg.HeroPos);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void HuntingIntrudeChallenge(ulong id)
        {
            MSG_ZGC_HUNTING_INTRUDE_CHALLENGE msg = new MSG_ZGC_HUNTING_INTRUDE_CHALLENGE();

            HuntingIntrudeInfo info = HuntingManager.GetHuntingIntrudeInfo(id);
            if (info == null)
            {
                Log.Warn($"HuntingIntrudeChallenge errror : had not find info {id}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            HuntingIntrudeModel model = HuntingLibrary.GetHuntingIntrudeModel(info.IntrudeId);
            if (model == null)
            {
                Log.Warn($"HuntingIntrudeChallenge errror : had not find model {info.IntrudeId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (info.EndTime < BaseApi.now)
            {
                Log.Warn($"HuntingIntrudeChallenge errror : had not find info {id}");
                msg.Result = (int)ErrorCode.HuntingIntrudeOOT;
                Write(msg);
                return;
            }

            if (HuntingManager.HuntingIntrudeHeroPos.Count == 0)
            {
                Log.Warn($"HuntingIntrudeChallenge errror : have not hero pos info");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            var Ienum = HuntingManager.HuntingIntrudeHeroPos.Keys.Where(x => HeroLibrary.GetHeroModel(x).Job == info.JobLimit);
            if (Ienum.Any())
            {
                Log.Warn($"HuntingIntrudeChallenge errror : hero job limit");
                msg.Result = (int)ErrorCode.HeroJobLimited;
                Write(msg);
                return;
            }

            ErrorCode errorCode = CanCreateDungeon(model.DungeonId);
            if (errorCode != ErrorCode.Success)
            {
                msg.Result = (int)errorCode;
                Write(msg);
                return;
            }

            HuntingIntrudeDungeonMap dungeon = server.MapManager.CreateDungeon(model.DungeonId) as HuntingIntrudeDungeonMap;
            if (dungeon == null)
            {
                Log.Write($"player {Uid} request to create dungeon {model.DungeonId} failed: create dungeon failed");
                msg.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(msg);
                return;
            }

            dungeon.SetIntrudeId(info);

            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }


        #endregion
    }
}

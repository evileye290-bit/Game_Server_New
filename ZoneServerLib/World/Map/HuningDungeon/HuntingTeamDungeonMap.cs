using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    public partial class HuntingTeamDungeonMap : TeamDungeonMap
    {
        protected PlayerChar cacheBrotherPlayer = null;
        protected int offlineBrotherUid;

        public HuntingTeamDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override bool HadPassedHuntingDungeon()
        {
            foreach (var kv in PcList)
            {
                if (kv.Value.IsCaptain())
                {
                    return kv.Value.HuntingManager?.CheckPassedByDungeonId(DungeonModel.Id) == true;
                }
            }
            return false;
        }

        protected override bool CheckHuntingPeriodBuffEffectCondition()
        {
            if (cacheBrotherPlayer?.CheckHuntingPeriodBuffEffect() == true) return true;

            if (PcList.Values.FirstOrDefault(x => x.CheckHuntingPeriodBuffEffect()) != null) return true;

            return false;
        }

        protected override void Start()
        {
            int huntingResearch = GerResearch();

            //队长首次打改副本，副本难度降低
            float growth = HuntingLibrary.GetGrowth(huntingResearch);
            float discount = HadPassedHuntingDungeon() ? 1.0f : HuntingLibrary.Discount;
            SetMonsterGrowth(growth * discount);

            base.Start();
        }

        private int GerResearch()
        {
            if (IsHelpDungeon)
            {
                PlayerChar player = PcList.Values.FirstOrDefault(x => x.Uid == AskHelpUid);
                if (player != null)
                {
                    return player.HuntingManager.Research;
                }
            }

            List<int> researchList = new List<int>() { cacheBrotherPlayer == null ? 0 : cacheBrotherPlayer.HuntingManager.Research };
            PcList.ForEach(x => researchList.Add(x.Value.HuntingManager.Research));

            Log.Info($"HuntingTeamDungeonMap uid {cacheBrotherPlayer?.Uid} offline uid {offlineBrotherUid} research {cacheBrotherPlayer?.HuntingManager.Research} list {string.Join("|", researchList)}");

            return researchList.Max();
        }

        public void FriendlyScoreCalc()
        {
            int length = PcList.Count;

            for (int i = 0; i < length; i++)
            {
                var pc1 = PcList.ElementAt(i);
                for (int j = i+1; j < length; j++)
                {
                    var pc2 = PcList.ElementAt(j);
                    if (pc1.Value.CheckFriendExist(pc2.Value.Uid))
                    {
                        pc1.Value.AddFriendScore(pc2.Value.Uid);
                    }
                }

                if (offlineBrotherUid>0)
                {
                    if (pc1.Value.CheckFriendExist(offlineBrotherUid))
                    {
                        pc1.Value.AddFriendScore(offlineBrotherUid);
                    }
                }
            }
        }

        protected override void Failed()
        {
            base.Failed();
            PlayerChar player;
            foreach (var kv in PcList)
            {
                player = kv.Value;

                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                //komoelog
                player.KomoeLogRecordPveFight(2, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            DoReward();
            OnTeamDungeonFinished();
            string reward = string.Empty;
            PlayerChar player = null;

            bool huntingIntrude = CheckHuntingIntrude();

            foreach (var kv in PcList)
            {
                try
                {
                    player = kv.Value;
                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                    reward = mng.ToString();

                    player.HuntingReward(mng, DungeonModel, this);

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);


                    if (huntingIntrude)
                    {
                        player.AddHuntingIntrude();
                    }

                    switch ((MapType)DungeonModel.Type)
                    {
                        case MapType.Hunting:
                        case MapType.HuntingDeficute:
                        case MapType.HuntingTeamDevil:
                            //日志
                            if (IsHelpDungeon)
                            {
                                player.BIRecordCheckPointLog(MapType.HuntingTeamHelp, DungeonModel.Id.ToString(), 1, GetFinishTime());
                            }
                            else
                            {
                                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
                            }
                            break;
                        case MapType.HuntingActivitySingle:
                        case MapType.HuntingActivityTeam:
                            if (IsHelpDungeon)
                            {
                                player.BIRecordCheckPointLog(MapType.HuntingActivityTeamHelp, DungeonModel.Id.ToString(), 1, GetFinishTime());
                            }
                            else
                            {
                                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
                            }
                            break;
                        default:
                            break;
                    }

                    //komoelog
                    player.KomoeLogRecordPveFight(2, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());

                    player.AddRunawayActivityNumForType(RunawayAction.Hunting);
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }


            OfflineBrotherReward(reward, player == null ? "" : player.Name, huntingIntrude);

            FriendlyScoreCalc();

            ResetReward();
        }

        protected virtual void OfflineBrotherReward(string reward, string name, bool huntingIntrude)
        {
            string param = $"{CommonConst.NAME}:{name}";

            if (offlineBrotherUid > 0)
            {
                int emilId = 0;
                RewardManager mng = new RewardManager();

                PlayerChar player = server.PCManager.FindPc(offlineBrotherUid);
                if (player == null)
                {
                    player = server.PCManager.FindOfflinePc(offlineBrotherUid);
                }

                //离线帮杀奖励
                if (IsHelpDungeon)
                {
                    if (offlineBrotherUid != AskHelpUid)
                    {
                        emilId = HuntingLibrary.OfflineHelpEmailId;
                        mng.InitSimpleReward(HuntingLibrary.OfflineHelpReward);
                        reward = mng.ToString();
                    }
                }
                else
                {
                    //emilId = HuntingLibrary.OfflineBrotherEmailId;
                    var huntingModel = HuntingLibrary.GetByMapId(Model.MapId);
                    DungeonModel dungeon = DungeonLibrary.GetDungeon(Model.MapId);
                    if (huntingModel == null || dungeon == null)
                    {
                        return;
                    }

                    mng.InitSimpleReward(reward);

                    int addResearch = HuntingLibrary.GetResearch(dungeon.Difficulty);
                    int research = cacheBrotherPlayer == null ? addResearch : cacheBrotherPlayer.HuntingManager.Research;
                    research = Math.Min(HuntingLibrary.ResearchMax, research);

                    Log.Info($"OfflineBrotherReward dungeon id {DungeonModel.Id} reward research {research}");

                    foreach (var kv in mng.GetRewardItemList(RewardType.SoulRing))
                    {
                        int year = ScriptManager.SoulRing.GetYear((int)dungeon.Difficulty, research);
                        kv.Attrs.Add(year.ToString());
                    }
                    reward = mng.ToString();

                    if (player != null)
                    {
                        player.HuntingManager.AddPassedId(huntingModel.Id);
                        player.HuntingManager.AddResearch(addResearch);

                        if (huntingIntrude)
                        {
                            player.AddHuntingIntrude();
                        }
                    }
                    else
                    {
                        cacheBrotherPlayer.HuntingManager.AddResearch(addResearch, false);
                        Send2ManagerHuntingInfo(cacheBrotherPlayer.Uid, research, addResearch, huntingModel.Id, false, huntingIntrude);
                    }
                    //加入魂环助战箱
                    AddItemInfoToWarehouse(offlineBrotherUid, reward, param, ItemWarehouseType.SoulRing);
                }

                if (emilId > 0)
                {
                    if (player != null)
                    {
                        player.SendPersonEmail(emilId, reward: reward, param: param);
                    }
                    else
                    {
                        MSG_ZR_SEND_EMAIL msg = new MSG_ZR_SEND_EMAIL
                        {
                            EmailId = emilId,
                            Uid = offlineBrotherUid,
                            Reward = reward,
                            SaveTime = 0,
                            Param = param
                        };
                        server.SendToRelation(msg);
                    }
                }
            }
        }

        protected void Send2ManagerHuntingInfo(int uid, int research, int addResearch, int passId, bool isActivity, bool huntingIntrude)
        {
            server.ManagerServer.Write(new MSG_ZMZ_HUNTING_CHANGE() 
            { 
                Uid = uid, 
                ResearchChange = addResearch, 
                PassedId = passId , 
                Research = research, 
                IsActivity = isActivity, 
                MainId = server.MainId,
                HuntingIntrude = huntingIntrude,
                HuntingIntrudeId = server.UID.NewIuid(server.MainId, server.SubId)
            });
        }

        public void SetOfflineBrother(PlayerChar player)
        {
            cacheBrotherPlayer = player;
            offlineBrotherUid = player.Uid;
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            base.OnPlayerMapLoadingDone(player);
            ChangeHuntingStateByCaptainState(player);
            RecordCaptainInfo(player);
        }

        public override void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            base.OnPlayerLeave(player, cache);
            NotifyTeamMemberLeaveMap(player);
        }

        private void ChangeHuntingStateByCaptainState(PlayerChar player)
        {
            if (player.Team == null) return;

            if (player.Uid != player.Team.CaptainUid)
            {
                PlayerChar captain = server.PCManager.FindPc(player.Team.CaptainUid);
                if (captain != null && captain.HuntingManager.ContinueHunting && !player.HuntingManager.ContinueHunting)
                {
                    player.ChangeHuntingState(true);
                }
            }
        }

        private void RecordCaptainInfo(PlayerChar player)
        {
            if (player.Team == null) return;

            player.RecordCaptinUid(player.Team.CaptainUid);
        }

        private void NotifyTeamMemberLeaveMap(PlayerChar player)
        {
            player.NotifyTeamMemberLeaveMap();
            NotifyCaptainMemberLeaveMap(player);
        }

        private void NotifyCaptainMemberLeaveMap(PlayerChar player)
        {
            if (player.CaptainUid != 0 && player.CaptainUid != player.Uid)
            {
                PlayerChar captain = server.PCManager.FindPc(player.CaptainUid);
                if (captain != null)
                {
                    captain.NotifyMemberLeaveMap();
                }
            }
        }

        /// <summary>
        /// 加入仓库物品信息
        /// </summary>
        /// <param name="offlineBrotherUid"></param>
        /// <param name="reward"></param>
        /// <param name="param"></param>
        protected void AddItemInfoToWarehouse(int offlineBrotherUid, string reward, string param, ItemWarehouseType type)
        {
            MSG_ZR_ADD_WAREHOUSE_ITEMINFO msg = new MSG_ZR_ADD_WAREHOUSE_ITEMINFO();         
            msg.Uid = offlineBrotherUid;
            msg.Type = (int)type;
            msg.Reward = reward;
            msg.Param = param;
            server.SendToRelation(msg);
        }

        private bool CheckHuntingIntrude()
        {
            if (PcList.Count == 0) return false;

            if (PcList.Values.FirstOrDefault(x => x.HuntingManager.Research < HuntingLibrary.HuntingIntrudeResearchLimit) != null)
            {
                return false;
            }

            if (cacheBrotherPlayer != null)
            {
                if (cacheBrotherPlayer.HuntingManager.Research < HuntingLibrary.HuntingIntrudeResearchLimit)
                {
                    return false;
                }
            }

            return HuntingLibrary.HuntingIntrudeProbability >= RAND.Range(0, 10000);
        }
    }
}

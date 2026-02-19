using System;
using System.Linq;
using CommonUtility;
using Logger;
using Message.Zone.Protocol.ZR;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using EnumerateUtility;

namespace ZoneServerLib
{
    class HuntingActivityDungeonMap : HuntingDungeonMap
    {
        public HuntingActivityDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override bool HadPassedHuntingDungeon()
        {
            return PcList.Values.First()?.HuntingManager?.CheckActivityPassedByDungeonId(DungeonModel.Id) == true;
        }
    }

    class HuntingActivityTeamDungeonMap : HuntingTeamDungeonMap
    {
        public HuntingActivityTeamDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override bool HadPassedHuntingDungeon()
        {
            foreach (var kv in PcList)
            {
                if (kv.Value.IsCaptain())
                {
                    return kv.Value.HuntingManager?.CheckActivityPassedByDungeonId(DungeonModel.Id) == true;
                }
            }
            return false;
        }

        protected override void OfflineBrotherReward(string reward, string name, bool huntingIntrude)
        {
            string param = $"{CommonConst.NAME}:{name}";
            //int time = Timestamp.GetUnixTimeStampSeconds(server.Now());
            //ulong emilUid = server.UID.NewEuid(server.MainId, server.MainId);

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
                    var huntingModel = HuntingLibrary.GetHuntingActivityModelByMapId(Model.MapId);
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
                        player.HuntingManager.AddActivityPassed(huntingModel.Id);
                        player.HuntingManager.AddResearch(addResearch);
                        if (huntingIntrude)
                        {
                            player.AddHuntingIntrude();
                        }
                    }
                    else
                    {
                        cacheBrotherPlayer.HuntingManager.AddResearch(addResearch, false);
                        Send2ManagerHuntingInfo(cacheBrotherPlayer.Uid, research, addResearch, huntingModel.Id, true, huntingIntrude);
                        //cacheBrotherPlayer.HuntingManager.AddActivityPassed(huntingModel.Id);
                        //cacheBrotherPlayer.HuntingManager.AddResearch(HuntingLibrary.GetResearch(dungeon.Difficulty));
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
    }
}

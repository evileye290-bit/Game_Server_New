using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        //获取玩家1信息
        private void OnResponse_GetCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_CHALLENGE_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_CHALLENGE_PLAYER>(stream);
            Log.Warn($"player {msg.Player1.Uid} GetCrossChallengePlayerInfo challenge {msg.Player2.MainId} player {msg.Player2.Uid}  ");
            LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.Player1.Uid, msg, uid);
        }

        //返回的玩家信息
        private void OnResponse_ReturnCrossChallengePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>(stream);
            Log.Warn($"player {msg.Player1.Uid} ReturnCrossChallengePlayerInfo challenge {msg.Player2.MainId} player {msg.Player2.Uid}  ");

            if (msg.GetType == (int)ChallengeIntoType.CrossChallengeFinalsRobot)
            {
                PlayerChar findPlayer = new PlayerChar(Api, msg.Player2.Uid);
                if (msg.Player2.BaseInfo != null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in msg.Player2.BaseInfo)
                    {
                        dataList[(HFPlayerInfo)item.Key] = item.Value;
                    }

                    RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                    findPlayer.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                    findPlayer.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
                    findPlayer.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
                    findPlayer.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                    //findPlayer.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
                    findPlayer.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                    findPlayer.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
                    //findPlayer.BattlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);
                    //findPlayer.CrossLevel = rInfo.GetIntValue(HFPlayerInfo.CrossLevel);
                    //findPlayer.CrossStar = rInfo.GetIntValue(HFPlayerInfo.CrossScore);
                    findPlayer.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                    //findPlayer.Camp = (CampType)queryBasic.Camp;
                    //伙伴列表

                    foreach (var hero in msg.Player2.Heros)
                    {
                        HeroInfo info = new HeroInfo();
                        info.Id = hero.Id;
                        info.Level = hero.Level;
                        info.StepsLevel = hero.StepsLevel;
                        info.SoulSkillLevel = hero.SoulSkillLevel;
                        info.GodType = hero.GodType;
                        info.CrossChallengeQueueNum = hero.QueueNum;
                        info.CrossChallengePositionNum = hero.PositionNum;

                        foreach (var item in hero.Natures.List)
                        {
                            info.Nature.AddNatureBaseValue((NatureType)item.NatureType, item.Value);
                        }

                        findPlayer.HeroMng.BindHeroInfo(info);
                        findPlayer.HeroMng.BindHeroQueueList(info);
                    }
                    //findPlayer.InitHero(msg.Player2.Heros);
                    //一开始初始化FSM会报错
                    findPlayer.InitFSMAfterHero();

                    findPlayer.NatureValues = new Dictionary<int, int>(msg.Player2.NatureValues);
                    findPlayer.NatureRatios = new Dictionary<int, int>(msg.Player2.NatureRatios);
                    ////初始化伙伴属性
                    //findPlayer.BindHerosNature();
                }
                else
                {
                    // 未找到该角色
                    Log.Warn("player {0} LoadBattlePlayerInfoWithQuerys load  failed: not find {1}", uid, msg.Player2.Uid);
                    return;
                }
                PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(msg.Player1);
                if (fightInfo != null)
                {
                    fightInfo.Type = ChallengeIntoType.CrossChallengeFinals;
                    fightInfo.TimingId = msg.TimingId;
                    fightInfo.GroupId = msg.GroupId;
                    fightInfo.TeamId = msg.TeamId;
                    fightInfo.FightId = msg.FightId;
                    fightInfo.HeroIndex[msg.Player1.Uid] = msg.Player1.Index;
                    fightInfo.HeroIndex[msg.Player2.Uid] = msg.Player2.Index;
                    findPlayer.EnterCrossChallengeMap(fightInfo);
                }
            }
            else
            {
                LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.Player2.Uid, msg, uid);
            }
        }

        private void OnResponse_GetCrossChallengeChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO>(stream);

            Log.Warn($"player {uid} ReturnCrossChallengePlayerInfo result {msg.Result} type {msg.GetType}  ");

            if (msg.Result != (int)ErrorCode.Success)
            {
                LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.ChallengerUid, msg, uid);
            }
            else
            {
                PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(msg.Challenger);
                if (fightInfo != null)
                {
                    PlayerChar player = Api.PCManager.FindPc(uid);
                    if (player == null)
                    {
                        player = Api.PCManager.FindOfflinePc(uid);
                        if (player == null)
                        {
                            Log.Warn("player {0} not find return cross challenger from relation find show player {1} failed: not find ", uid, msg.Challenger.Uid);
                            return;
                        }
                    }
                    fightInfo.Type = ChallengeIntoType.CrossChallengePreliminary;
                    player.EnterCrossChallengeMap(fightInfo);
                }
            }
        }

        private void OnResponse_ShowCrossChallengeFinals(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_CHALLENGE_FINALS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_CHALLENGE_FINALS_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get cross finals info from relation failed: not find player ", uid);
                return;
            }

            MSG_ZGC_SHOW_CROSS_CHALLENGE_FINALS_INFO msg = new MSG_ZGC_SHOW_CROSS_CHALLENGE_FINALS_INFO();
            msg.TeamId = pks.TeamId;

            foreach (var item in pks.List)
            {
                CROSS_CHALLENGE_FINALS_PLAYER_INFO itemMsg = new CROSS_CHALLENGE_FINALS_PLAYER_INFO();
                itemMsg.Index = item.Index;
                if (item.Uid > 0)
                {
                    RedisPlayerInfo rInfo = GetBaseInfoMsg(item.BaseInfo);
                    itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);

                    Corss_BattlePlayerBattlePowerMsg powerMsg = new Corss_BattlePlayerBattlePowerMsg() { Uid = item.Uid };
                    string powerInfo = rInfo.GetStringValue(HFPlayerInfo.CrossChallengeQueuePower);
                    if (string.IsNullOrEmpty(powerInfo))
                    {
                        powerMsg.BattlePower.Add(new List<int>() {itemMsg.BaseInfo.BattlePower, itemMsg.BaseInfo.BattlePower, itemMsg.BaseInfo.BattlePower });
                    }
                    else
                    {
                        powerMsg.BattlePower.Add(powerInfo.ToList('|'));
                    }
                    long power = powerMsg.BattlePower.Sum(x => (long)x);

                    if(power > int.MaxValue)
                    {
                        itemMsg.BaseInfo.BattlePower = -1;
                    }
                    else
                    {
                        itemMsg.BaseInfo.BattlePower = (int)power;
                    }
                    itemMsg.BaseInfo.BattlePower64 = power;

                    msg.CrossChallengeBattlePower.Add(powerMsg);
                }
                else
                {
                    if (pks.TeamId > 0)
                    {
                        //机器人
                        CrossFinalsRobotInfo rInfo = RobotLibrary.GetCrossChallengeFinalsRobotInfo(pks.TeamId, item.Index);
                        if (rInfo != null)
                        {
                            itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);
                            Corss_BattlePlayerBattlePowerMsg powerMsg = new Corss_BattlePlayerBattlePowerMsg() { Uid = item.Uid };
                            powerMsg.BattlePower.Add(new List<int>() { rInfo.BattlePower, rInfo.BattlePower, rInfo.BattlePower });
                            msg.CrossChallengeBattlePower.Add(powerMsg);
                        }
                    }
                    else
                    {
                        //总决赛机器人
                        CrossFinalsRobotInfo rInfo = RobotLibrary.GetCrossChallengeFinalsRobotInfo(item.Index, item.OldTeam);
                        if (rInfo != null)
                        {
                            itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);
                            Corss_BattlePlayerBattlePowerMsg powerMsg = new Corss_BattlePlayerBattlePowerMsg() { Uid = item.Uid };
                            powerMsg.BattlePower.Add(new List<int>() { rInfo.BattlePower, rInfo.BattlePower, rInfo.BattlePower });
                            msg.CrossChallengeBattlePower.Add(powerMsg);
                        }
                    }
                }
                msg.List.Add(itemMsg);
            }

            pks.BattleInfoList.ForEach(x =>
            {
                CROSS_CHALLENGE_WIN_INFO info = new CROSS_CHALLENGE_WIN_INFO() {BattleId = x.BattleId};
                info.BattleInfo.Add(x.BattleInfo);
                msg.BattleInfoList.Add(info);
            });

            msg.Fight1.AddRange(pks.Fight1);
            msg.Fight2.AddRange(pks.Fight2);
            msg.Fight3.AddRange(pks.Fight3);
            player.Write(msg);
        }

        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_CHALLENGER>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get cross player hero info from relation failed: not find player ", uid);
                return;
            }

            Dictionary<int, int> dic = new Dictionary<int, int>();
            Queue<ZGC_Show_HeroInfo> queue = new Queue<ZGC_Show_HeroInfo>();
            int power = 0;
            foreach (var item in pks.Heros)
            {
                ZGC_Show_HeroInfo info = GetPlayerHeroInfoMsg(item);
                queue.Enqueue(info);

                dic.TryGetValue(info.QueueNum, out power);
                dic[info.QueueNum] = power + info.Power;
            }

            MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_INFO msg = new MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_INFO();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.Result = pks.Result;
            foreach (var item in dic)
            {
                msg.BattlePowers[item.Key] = item.Value;
            }

            while (queue.Count > 0)
            {
                ZGC_Show_HeroInfo info = queue.Dequeue();
                msg.HeroList.Add(info);

                if (msg.HeroList.Count > 2)
                {
                    msg.IsEnd = false;
                    player.Write(msg);

                    msg = new MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_INFO();
                    msg.Uid = pks.Uid;
                    msg.MainId = pks.MainId;
                    msg.Result = pks.Result;
                    foreach (var item in dic)
                    {
                        msg.BattlePowers[item.Key] = item.Value;
                    }
                }
            }

            msg.IsEnd = true;
            player.Write(msg);
        }

        public void OnResponse_GetCrossChallengeHeros(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_CHALLENGE_HEROS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_CHALLENGE_HEROS>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player == null)
                {
                    LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.CrossChallengeHeroInfo, pks.Uid, pks, uid);
                }
                else
                {
                    player.SyncCrossChallengeHeroQueueMsg(pks.SeeUid, pks.SeeMainId);
                }
            }
            else
            {
                player.SyncCrossChallengeHeroQueueMsg(pks.SeeUid, pks.SeeMainId);
            }
        }


        private void OnResponse_ShowCrossChallengeRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_CHALLENGE_RANK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_CHALLENGE_RANK_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get cross rank info from relation failed: not find player ", msg.PcUid);
                return;
            }
            player.ShowCrossChallengeRankInfosMsg(msg);
        }

        private void OnResponse_ShowCrossChallengeLeaderInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_CHALLENGE_LEADER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_CHALLENGE_LEADER_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get cross Challenge leader info from relation failed: not find player ", msg.PcUid);
                return;
            }
            player.ShowCrossChallengeLeaderInfosMsg(msg);
        }

        private void OnResponse_UpdateCrossChallengeRank(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_CROSS_CHALLENGE_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_CROSS_CHALLENGE_RANK>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.UpdateCrossChallengeSeasonRank(msg.Rank);
                player.SendCrossChallengeManagerMessage();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    player.UpdateCrossChallengeSeasonRank(msg.Rank);
                }
            }
        }

        public void OnResponse_GetCrossChallengeVedio(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_CHALLENGE_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_CHALLENGE_VIDEO>(stream);
            MSG_ZGC_GET_CROSS_CHALLENGE_VIDEO request = new MSG_ZGC_GET_CROSS_CHALLENGE_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.VideoName = pks.VideoName;
            request.Index = pks.Index;
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get cross battle vedio info from relation failed: not find player ", uid);
                return;
            }
            player.Write(request);
        }

        public void OnResponse_ClearCrossChallengeFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            Log.Write("cross server ClearCrossChallengeFinalsPlayerRank");
            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.CrossChallengeInfoMng.ChangeLastFinalsRank(0);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.CrossChallengeInfoMng.ChangeLastFinalsRank(0);
            }
        }

        public void OnResponse_UpdateCrossChallengeFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL>(stream);
            Log.Write("cross server UpdateCrossChallengeFinalsPlayerRank");

            foreach (var kv in pks.List)
            {
                PlayerChar player = Api.PCManager.FindPc(kv.Key);
                if (player == null)
                {
                    player = Api.PCManager.FindOfflinePc(kv.Key);
                    if (player == null)
                    {
                        continue; ;
                    }
                }
                player.CrossChallengeInfoMng.ChangeLastFinalsRank(kv.Value);
            }
        }

        public void OnResponse_ClearCrossChallengeRanks(MemoryStream stream, int uid = 0)
        {
            Log.Write("cross server ClearCrossChallengeRanks");
            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.RefreshCrossChallengeRank(true);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.RefreshCrossChallengeRank(false);
            }
        }

        public void OnResponse_CrossChallengeStart(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_BATTLE_START>(stream);
            Log.Write("cross server CrossChallengeStart");
            //清空所有人决战排名
            Api.CrossChallengeMng.FirstStartTime = pks.Time;
            Api.CrossChallengeMng.TeamId = pks.TeamId;
            Api.CrossChallengeMng.StartTime = Timestamp.TimeStampToDateTime(pks.Time);
        }

        public void OnResponse_GetCrossChallengeStart(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_CHALLENGE_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_CHALLENGE_BATTLE_START>(stream);
            Log.Write("cross server GetCrossChallengeStart");
            //清空所有人决战排名
            Api.CrossChallengeMng.FirstStartTime = pks.Time;
            Api.CrossChallengeMng.TeamId = pks.TeamId;
            Api.CrossChallengeMng.StartTime = Timestamp.TimeStampToDateTime(pks.Time);
            if (uid > 0)
            {
                PlayerChar player = Api.PCManager.FindPc(uid);
                if (player != null)
                {
                    player.SyncCrossChallengeManagerMessage();
                }
            }
        }

        public void OnResponse_CrossChallengeServerReward(MemoryStream stream, int uid = 0)
        {
            Log.Write("cross server CrossChallengeServerReward");
            int state = (int)CrossRewardState.None;
            MSG_ZGC_NEW_CROSS_CHALLENGE_SERVER_REWARD msg = new MSG_ZGC_NEW_CROSS_CHALLENGE_SERVER_REWARD();
            msg.ServerReward = state;

            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.CrossChallengeInfoMng.Info.ServerReward = state;
                player.Value.Write(msg);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.CrossChallengeInfoMng.Info.ServerReward = state;
            }
        }

        public void OnResponse_GetCrossChallengeGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_CHALLENGE_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_CHALLENGE_GUESSING_INFO>(stream);
            Log.Write("cross server GetCrossChallengeGuessingPlayersInfo");
            //清空所有人决战排名
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                Dictionary<int, RedisPlayerInfo> dic = new Dictionary<int, RedisPlayerInfo>();
                foreach (var item in pks.InfoList)
                {
                    RedisPlayerInfo rInfo = GetBaseInfoMsg(item.BaseInfo);
                    dic[item.Uid] = rInfo;
                }

                MSG_ZGC_GET_CROSS_CHALLENGE_GUESSING_INFO msg = new MSG_ZGC_GET_CROSS_CHALLENGE_GUESSING_INFO();
                foreach (var guessingInfo in pks.GuessingInfos)
                {
                    CROSS_GUESSING_ITEM_INFO itemMsg = new CROSS_GUESSING_ITEM_INFO();

                    RedisPlayerInfo rInfo;
                    if (!dic.TryGetValue(guessingInfo.Player1, out rInfo))
                    {
                        continue;
                    }
                    itemMsg.Player1 = new CROSS_CHALLENGE_FINALS_PLAYER_INFO();
                    itemMsg.Player1.Index = 1;
                    rInfo.SetValue(HFPlayerInfo.BattlePower64, rInfo.GetlongValue(HFPlayerInfo.CrossChallengePower));
                    itemMsg.Player1.BaseInfo = GetBaseInfoMsg(rInfo);
                    if (!dic.TryGetValue(guessingInfo.Player2, out rInfo))
                    {
                        continue;
                    }
                    itemMsg.Player2 = new CROSS_CHALLENGE_FINALS_PLAYER_INFO();
                    itemMsg.Player2.Index = 2;
                    rInfo.SetValue(HFPlayerInfo.BattlePower64, rInfo.GetlongValue(HFPlayerInfo.CrossChallengePower));
                    itemMsg.Player2.BaseInfo = GetBaseInfoMsg(rInfo);

                    itemMsg.TimingId = guessingInfo.TimingId;
                    itemMsg.Choose = guessingInfo.Choose;
                    itemMsg.Player1Choose = guessingInfo.Player1Choose;
                    itemMsg.Player2Choose = guessingInfo.Player2Choose;
                    itemMsg.Winner = guessingInfo.Winner;
                    msg.List.Add(itemMsg);
                }

                CrossBattleTiming endGuessing = CrossChallengeLibrary.GetGuessingTime(Api.CrossChallengeMng.StartTime, Api.Now());
                CrossBattleTiming timing = CrossChallengeLibrary.GetCrossBattleTiming(endGuessing);
                msg.CurrentTimingId = (int)timing;
                player.Write(msg);
            }
        }

        public void OnResponse_CrossChallengeGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_GUESSING_CHOOSE>(stream);
            Log.Write("cross server CrossChallengeGuessingChoose");
            //清空所有人决战排名
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CrossChallengeGuessingChoose(pks.Result, pks.TimingId, pks.Choose, pks.HasReward);
            }
        }

        public void OnResponse_CrossChallengeGuessingTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM>(stream);
            Log.Write("cross server CrossChallengeGuessingTeam");
            //清空所有人决战排名
            Api.CrossChallengeMng.TeamId = pks.TeamId;
        }

        public void OnResponse_UpdateCrossChallengeTeamId(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID>(stream);
            Log.Write("cross server UpdateCrossChallengeTeamId");
            //清空所有人决战排名
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpdateCrossChallengeTeamId(pks.TeanId);
            }
        }
    }
}

using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using Message.Gate.Protocol.GateZ;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;
using StackExchange.Redis;
using Google.Protobuf.Collections;
using ServerShared;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_GeArenaChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_ARENA_CHALLENGERS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_ARENA_CHALLENGERS>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get arena challenger from relation failed: not find player ", msg.PcUid);
                return;
            }

            List<RedisValue> uids = new List<RedisValue>();
            Dictionary<int, PlayerRankBaseInfo> list = new Dictionary<int, PlayerRankBaseInfo>();
            foreach (var item in msg.List)
            {
                int index = item.Index;

                PlayerRankBaseInfo info = new PlayerRankBaseInfo();
                info.IsRobot = item.IsRobot;
                info.Uid = item.Uid;
                info.Rank = item.Rank;
                info.Index = index;
                list.Add(index, info);

                if (!item.IsRobot)
                {
                    uids.Add(item.Uid);
                }
            }

            player.GetChallengerInfos(msg.Rank, uids, list);

            //komoelog
            player.KomoeLogRecordPvpFight(1, 5, null, 1, player.ArenaMng.Rank, player.ArenaMng.Rank, player.ArenaMng.Level.ToString(), player.ArenaMng.Level.ToString(), 0, 0);
        }

        private void OnResponse_ShowArenaRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_ARENA_RANK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_ARENA_RANK_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get arena rank info from relation failed: not find player ", msg.PcUid);
                return;
            }

            List<RedisValue> uids = new List<RedisValue>();
            Dictionary<int, PlayerRankBaseInfo> list = new Dictionary<int, PlayerRankBaseInfo>();
            foreach (var item in msg.List)
            {
                uids.Add(item.Uid);

                PlayerRankBaseInfo info = new PlayerRankBaseInfo();
                info.Uid = item.Uid;
                info.Rank = item.Rank;
                list.Add(item.Uid, info);
            }

            player.ShowArenaRankInfos(msg.Rank, msg.Page, msg.TotalCount, uids, list);
        }

        private void OnResponse_ChallengeWinChangeRank(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHALLENGE_WIN_CHANGE_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGE_WIN_CHANGE_RANK>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} arena challenge win change rank from relation failed: not find ", msg.PcUid);
                return;
            }

            //更新排行榜
            player.ArenaMng.ChargeRank(msg.NewRank, msg.HistoryRank);


            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(msg.Reward);
            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = player.GetRewardSyncMsg(rewards);
            rewardMsg.DungeonId = ArenaLibrary.MapId;
            rewardMsg.Result = msg.Result;

            //基本信息
            rewardMsg.ArenaInfo = player.GetChallengeResultInfo(msg.OldRank, msg.OldScore);

            //player.Write(rewardMsg);
            player.CurDungeon?.CachePlayerMessage(rewardMsg, player);


            //刷新竞技场信息
            player.SenArenaManagerMessage();

            if (msg.Result == (int)DungeonResult.Success && msg.OldRank != msg.NewRank)
            {
                player.GetArenaChallengers();

                if (msg.NewRank == 1)
                {
                    List<string> list = new List<string>();
                    list.Add(((int)player.Camp).ToString());
                    list.Add(player.Name);
                    player.BroadcastAnnouncement(ANNOUNCEMENT_TYPE.ARENA_FIRST, list);

                    player.TitleMng.UpdateTitleConditionCount(TitleObtainCondition.ArenaRankFirst);
                }
            }         
        }


        private void OnResponse_ChallengerRankChange(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHALLENGER_RANK_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGER_RANK_CHANGE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player == null)
                {
                    //Log.Warn("player {0} arena challenger rank change from relation failed: not find ", msg.PcUid);
                    return;
                }
            }
            //通知前端奖励
            MSG_ZGC_CHALLENGER_RANK_CHANGE request = new MSG_ZGC_CHALLENGER_RANK_CHANGE();
            request.PcUid = msg.PcUid;
            request.OldRank = msg.OldRank;
            request.NewRank = msg.NewRank;
            player.Write(request);

            //更新排行榜
            player.ArenaMng.ChargeRank(msg.NewRank);
        }

        private void OnResponse_GetChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZGC_ARENA_CHALLENGER_HERO_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_ARENA_CHALLENGER_HERO_INFO>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get arena challenger info failed: not find ", uid);
                return;
            }
            //更新排行榜
            player.Write(msg);

            PlayerRankBaseInfo rankInfo = player.ArenaMng.GetArenaRankInfo(msg.Info.BaseInfo.Uid);
            if (rankInfo == null)
            {
                Log.WarnLine("player {0} get arena challenger info not find  show arena challenger info failed: not find rank info index {1}", uid, msg.Info.BaseInfo.Uid);
                return;
            }
            //缓存信息
            foreach (var item in msg.HeroList)
            {
                RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
                rankInfo.HeroInfos.Add(robotInfo);
            }
            rankInfo.PetInfo = PlayerChar.GetRobotPetInfo(msg.Pet);
        }
    }
}

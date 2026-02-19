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
using Message.Zone.Protocol.ZR;
using RedisUtility;
using Message.Zone.Protocol.ZGate;
using ServerModels.Monster;

namespace ZoneServerLib
{
    public class CampBattlePhaseInfo
    {
        public CampBattleStep CampBattleStep;
        public DateTime BeginTime;
        public DateTime EndTime;
        public int PhaseNum;
    }

    public partial class RelationServer
    {
        public Dictionary<CampType, int> campCoins = new Dictionary<CampType, int>();
        public CampBattlePhaseInfo CampBattlePhaseInfo = new CampBattlePhaseInfo();

        private void OnResponse_SyncCampBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SYNC_CAMPBATTLE_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SYNC_CAMPBATTLE_DATA>(stream);
            Log.Write($"camp battle sync info, phase {msg.PhaseNum} step {msg.Step} Begin {msg.BeginTime} End {msg.EndTime}");

            CampBattlePhaseInfo.PhaseNum = msg.PhaseNum;
            CampBattlePhaseInfo.CampBattleStep = (CampBattleStep)msg.Step;

            //FIXME:这里是zone主动请求的，但是存在relation数据还未初始化完全就回流了。
            //TODO：BOIL 回头改relation 阵营战数据初始化位置。
            DateTime.TryParse(msg.BeginTime, out CampBattlePhaseInfo.BeginTime);
            DateTime.TryParse(msg.EndTime, out CampBattlePhaseInfo.EndTime);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZGC_SYNC_CAMPBATTLE response = new MSG_ZGC_SYNC_CAMPBATTLE();
                response.PhaseNum = msg.PhaseNum;
                response.Step = msg.Step;
                response.BeginTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Parse(msg.BeginTime));
                response.EndTime = Timestamp.GetUnixTimeStampSeconds(DateTime.Parse(msg.EndTime));
                response.CampScore = player.CampBattleMng.HistoricalMaxCampScore;
                response.MyCampScore = player.CampBattleMng.CampScore;
                response.ScoreUp = msg.ScoreUp;
                response.SpecialEnd = 0;
                response.InspireCamp = msg.InspireCamp;
                response.InspireDValue = msg.InspireDValue;
                foreach (var item in msg.BattleInfoList)
                {
                    CAMPBATTLE_INFO info = new CAMPBATTLE_INFO();
                    info.CampId = item.CampId;
                    info.Progress = item.Progress;
                    info.Grain = item.Grain;
                    info.TotalCampScore = item.TotalCampScore;
                    response.BattleInfoList.Add(info);
                }
                foreach (var item in msg.FortList)
                {
                    FORT_INFO info = new FORT_INFO();
                    info.Id = item.Id;
                    info.Progress = item.Progress.ToInt64TypeMsg();
                    info.MaxProgress = item.MaxProgress.ToInt64TypeMsg();
                    info.Icon = item.Icon;
                    info.Camp = item.Camp;
                    info.State = item.State;
                    info.CDTime = item.CDTime;
                    info.Uid = item.Uid;
                    response.FortList.Add(info);
                }

                player.Write(response);
            }
            else
            {
                if (uid != 0)
                {
                    Log.Error("player {0} mainId {1} get camp battle phase info fail,can not find player", uid, MainId);
                }
            }
        }

        private void OnResponse_GetFortInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_FORT_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_FORT_DATA>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                OperateGetBaseInfo operateGetBaseInfo = new OperateGetBaseInfo(msg.Uid);
                api.GameRedis.Call(operateGetBaseInfo, ret => 
                {
                    MSG_ZGC_FORT_INFO response = new MSG_ZGC_FORT_INFO();
                    response.FortId = msg.FortId;
                    response.FortState = msg.FortState;
                    response.CDTime = msg.CDTime;
                    response.PlayerInfo = player.GetPlayerBaseInfo(operateGetBaseInfo.Player);
                    response.PlayerInfo.Uid = msg.Uid;
                    response.ScoreUp = msg.ScoreUp;
                    foreach (var item in msg.AddNatures)
                    {
                        ADD_NATURE_INFO info = new ADD_NATURE_INFO();
                        info.Id = item.Id;
                        info.Value = item.Value;
                        response.AddNatures.Add(info);
                    }
                    foreach (var item in msg.Dungeons)
                    {
                        FORT_DUNGEON_INFO dungeon = new FORT_DUNGEON_INFO();
                        dungeon.DungeonId = item.DungeonId;
                        dungeon.Power = item.Power;
                        dungeon.IsBeenHold = item.IsBeenHold;
                        dungeon.HeroList.AddRange(GetHeroQueueData(item.HeroList));
                        response.Dungeons.Add(dungeon);
                    }

                    player.Write(response);
                });
            }
            else
            {
                Log.Error("player {0} mainId {1} get camp battle fort info fail,can not find player", uid, MainId);
            }

        }

        private List<HERO_QUEUE_DATA> GetHeroQueueData(RepeatedField<CAMP_CHALLENGER_HERO_INFO> heroList)
        {
            List<HERO_QUEUE_DATA> list = new List<HERO_QUEUE_DATA>();
            foreach (var item in heroList)
            {
                var hero = item;
                HERO_QUEUE_DATA info = new HERO_QUEUE_DATA();
                info.HeroId = hero.Id;
                info.Level = hero.Level;
                info.AwakenLevel = hero.AwakenLevel;
                info.StepsLevel = hero.StepsLevel;
                info.HeroId = hero.HeroId;
                info.GodType = hero.GodType;
                info.Name = hero.Name;
                info.BattlePower = hero.BattlePower;
                info.DefensiveQueueNum = hero.DefensiveQueueNum;
                info.DefensivePositionNum = hero.DefensivePositionNum;
                list.Add(info);
            }
            return list;
        }

        public void OnResponse_ClearCampBattleScore(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CLEAR_CAMP_BATTLE_SCORE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CLEAR_CAMP_BATTLE_SCORE>(stream);
            //给所有人发
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.CampBattleMng.CampScore = 0;
                player.Value.CampBattleMng.CampFight = 0;
                player.Value.CampBattleMng.CampCollection = 0;
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.CampBattleMng.CampScore = 0;
                player.Value.CampBattleMng.CampFight = 0;
                player.Value.CampBattleMng.CampCollection = 0;
            }
        }

        private void OnResponse_CampCoin(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMP_GRAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_GRAIN>(stream);
            foreach (var item in msg.GrainMap)
            {
                campCoins[(CampType)item.Key] = item.Value;
            }
        }

        private void OnResponse_GetRankList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMPBATTLE_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMPBATTLE_RANK_LIST>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZGC_CAMP_RANK_LIST_BY_TYPE response = new MSG_ZGC_CAMP_RANK_LIST_BY_TYPE();
                response.Page = msg.Page;
                response.Count = msg.Count;
                response.Camp = msg.Camp;
                response.RankType = msg.RankType;

                response.OwnerInfo = GetCampBuildRankPlayerInfo(msg.Info);
                foreach (var item in msg.RankList)
                {
                    response.RankList.Add(GetCampBuildRankPlayerInfo(item));
                }
                ////----------------------------测试数据------------------------------ 记得删除
                //CAMP_RANK_INFO testInfo = new CAMP_RANK_INFO();
                //testInfo.Name = player.Name;
                //testInfo.ShowValue = 1000;
                //testInfo.Rank = 1;
                //testInfo.Uid = player.Level;
                //testInfo.Icon = player.Icon;

                //response.OwnerInfo = testInfo;

                //for (int i = 0; i < 10; i++)
                //{
                //    CAMP_RANK_INFO info = new CAMP_RANK_INFO();
                //    info.Name = "qssx" + i;
                //    info.ShowValue = 1000 - i;
                //    info.Rank = 1 + i;
                //    info.Icon = player.Icon;
                //    info.Uid = 10 + i;
                //    response.RankList.Add(info);
                //}
                ////-----------------------------------------------------------------

                player.Write(response);
            }
            else
            {
                Log.Error("player {0} mainId {1} get camp build rank list fail,can not find player {0}", uid, MainId, uid);
            }
        }

        private CAMP_RANK_INFO GetCampBuildRankPlayerInfo(CAMPBATTLE_RANK_INFO playerInfo)
        {
            if (playerInfo == null)
            {
                return null;
            }

            CAMP_RANK_INFO info = new CAMP_RANK_INFO();
            info.Uid = playerInfo.Uid;
            info.Rank = playerInfo.Rank;
            info.Name = playerInfo.Name;
            info.Sex = playerInfo.Sex;
            info.Icon = playerInfo.Icon;
            info.ShowDIYIcon = playerInfo.ShowDIYIcon;
            info.IconFrame = playerInfo.IconFrame;
            info.Level = playerInfo.Level;
            info.TitleLevel = playerInfo.TitleLevel;
            info.ShowValue = playerInfo.ShowValue;

            return info;
        }

        public int GetGrain(CampType camp)
        {
            int ans = 0;
            campCoins.TryGetValue(camp, out ans);
            return ans;
        }

        public int AddGrain(int camp, int add)
        {
            MSG_ZR_ADD_CAMP_GRAIN msg = new MSG_ZR_ADD_CAMP_GRAIN();
            msg.Camp = camp;
            msg.GrainAdd = add;
            Write(msg);

            int ans = 0;
            campCoins.TryGetValue((CampType)camp, out ans);
            ans += add;
            campCoins[(CampType)camp] = ans;//临时变化，不一定准确但是方便快速取值
            return ans;
        }

        public void InitGrain(int camp, int add)
        {
            int ans = 0;
            campCoins.TryGetValue((CampType)camp, out ans);
            ans += add;
            campCoins[(CampType)camp] = ans;
        }


        private void AskForCampGrianInfo()
        {
            MSG_ZR_GET_CAMP_GRAIN msg = new MSG_ZR_GET_CAMP_GRAIN();
            Write(msg);
        }

        private void AskForCampBattleInfo()
        {
            MSG_ZR_GET_CAMPBATTLE_INFO msg = new MSG_ZR_GET_CAMPBATTLE_INFO();
            Write(msg);
        }


        private void OnResponse_CampCreateDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMP_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_CREATE_DUNGEON>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CampCreateDungeon from relation failed: not find player ", uid);
                return;
            }
            MSG_ZGC_CAMP_CREATE_DUNGEON response = new MSG_ZGC_CAMP_CREATE_DUNGEON();
            if (msg.Result != (int)ErrorCode.Success)
            {
                response.Result = msg.Result;
                player.Write(response);
                return;
            }

            //创建成功 

            //加次数
            //player.AddBattleFight(1);
            //player.SyncDbUpdateCampFight(1);
            //player.AddCampBattleRankScore(RankType.CampBattleFight, 1);

            //player.EnterCampMap(msg);
            List<int> heros = new List<int>();
            foreach (var item in msg.HeroList)
            {
                heros.Add(item.HeroId);
            }

            if (msg.DefenderUid > 0)
            {
                LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.CampDefender, msg.DefenderUid, msg, uid, heros);
            }
            else
            {
                PlayerCampFightInfo fightInfo = new PlayerCampFightInfo();
                fightInfo.Camp = msg.Camp;
                fightInfo.FortId = msg.FortId;
                fightInfo.DungeonIndex = msg.DungeonIndex;
                fightInfo.DungeonId = msg.DungeonId;
                fightInfo.InspireCamp = msg.InspireCamp;
                fightInfo.InspireDValue = msg.InspireDValue;
                fightInfo.FortCamp = msg.FortCamp;
                fightInfo.DefenderUid = msg.DefenderUid;
                foreach (var item in msg.AddNature)
                {
                    fightInfo.AddNature[item.Key] = item.Value;
                }
                //没有防守不需要加载
                //fightInfo.NatureValues = new Dictionary<int, int>(msg.NatureValues);
                //fightInfo.NatureRatios = new Dictionary<int, int>(msg.NatureRatios);

                player.EnterCampMap(fightInfo);
            }
        }

        private void OnResponse_CampDungeonEnd(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMP_DUNGEON_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_DUNGEON_END>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_CampDungeonEnd from relation failed: not find player ", uid);
                return;
            }
            
            int fortId = msg.FortId;
            string defenderName = msg.DefenderName;
            int monsterId = 0;
            List<MonsterGenModel> lst = MonsterGenLibrary.GetModelsByMap(msg.DungeonId);
            if (lst!=null)
            {
                foreach (var item in lst)
                {
                    monsterId = item.MonsterId;
                }
            }

            //跑马灯
            if (monsterId != 0)
            {
                player.BroadcastCampBattleHoldBoss(monsterId, fortId);
            }
            else
            {
                player.BroadcastCampBattleHoldDefender(defenderName, fortId);
            }
        }

        private void OnResponse_CampBattleEnd(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMPBATTLE_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMPBATTLE_END>(stream);

            MSG_ZGate_BROADCAST_ANNOUNCEMENT notify = new MSG_ZGate_BROADCAST_ANNOUNCEMENT();

            notify.Type = (int)ANNOUNCEMENT_TYPE.CAMP_WIN;
            notify.List.Add(msg.WinCamp.ToString());
            Api.GateManager.Broadcast(msg);
        }


        private void OnResponse_CheckUseNatureItem(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHECK_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHECK_USE_NATURE_ITEM>(stream);

            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_CheckUseNatureItem failed: not find player ", uid);
                return;
            }

            player.RealUseNatureItem(msg);
        }

        private void OnResponse_UseNatureItem(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_USE_NATURE_ITEM>(stream);

            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_UseNatureItem failed: not find player ", uid);
            }
            MSG_ZGC_USE_NATURE_ITEM response = new MSG_ZGC_USE_NATURE_ITEM();
            response.Result = msg.Result;
            response.FortId = msg.FortId;
            response.ItemId = msg.ItemId;
            player.Write(response);
        }


        private void OnResponse_SyncCampBattleScoreAdd(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMP_BATTLE_SCORE_ADD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_BATTLE_SCORE_ADD>(stream);
            int pcUid = msg.Uid;
            int addScore = msg.AddScore;
            CampType camp = (CampType)msg.CampType;

            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.SyncDbUpdateCampScore(addScore);
            }
            else
            {
                OperateGetCampScore operateGetCampScore = new OperateGetCampScore(Api.MainId, (int)camp, RankType.CampBattleScore, uid);
                Api.GameRedis.Call(operateGetCampScore, ret =>
                {
                    if ((int)ret == 1)
                    {
                        int score = operateGetCampScore.Score;

                        QueryUpdateCampScore query = new QueryUpdateCampScore(pcUid, score);
                        Api.GameDBPool.Call(query);
                    }
                });
            }
        }

        private void OnResponse_CampBoxCount(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMP_BOX_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMP_BOX_COUNT>(stream);
            CampType camp = (CampType)msg.Camp;
            int totalCount = msg.Count;

            foreach (var item in Api.PCManager.PcList)
            {
                PlayerChar player = item.Value;
                if (player.Camp == camp)
                {
                    Counter counter = player.GetCounter(CounterType.CampBoxCount);
                    int gotCount = 0;
                    if (counter == null)
                    {
                        gotCount = 0;
                    }
                    else
                    {
                        gotCount = counter.Count;
                    }
                    player.CampBoxLeftCount = totalCount - gotCount;
                    if (player.CampBoxLeftCount <0)
                    {
                        player.CampBoxLeftCount = 0;
                    }
                    MSG_ZGC_CAMP_BOX_COUNT response = new MSG_ZGC_CAMP_BOX_COUNT();
                    response.Count = player.CampBoxLeftCount;
                    player.Write(response);
                }
            }
        }

        private void OnResponse_GiveUpFort(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GIVEUP_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GIVEUP_FORT>(stream);

            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_GiveUpFort failed: not find player ", uid);
            }
            MSG_ZGC_GIVEUP_FORT response = new MSG_ZGC_GIVEUP_FORT();
            response.Result = msg.Result;
            player.Write(response);
        }

        private void OnResponse_HoldFort(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_HOLD_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_HOLD_FORT>(stream);

            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} OnResponse_HoldFort failed: not find player ", uid);
            }
            if (msg.Result == (int)ErrorCode.Success)
            {
                var expend = CampBattleLibrary.GetCampBattleExpend();
                player.UpdateCounter(CounterType.ActionCount, -expend.StrongPoint.Item1);
            }

            MSG_ZGC_HOLD_FORT response = new MSG_ZGC_HOLD_FORT();
            response.Result = msg.Result;
            response.FortId = msg.FortId;
            player.Write(response);
        }

    }
}

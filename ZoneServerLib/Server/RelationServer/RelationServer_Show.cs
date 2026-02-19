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
using Google.Protobuf.Collections;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_GetShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_SHOW_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_SHOW_PLAYER>(stream);
            MSG_ZRZ_RETURN_PLAYER_SHOW returnMsg = new MSG_ZRZ_RETURN_PLAYER_SHOW();
            returnMsg.PcUid = msg.PcUid;
            returnMsg.ShowPcUid = msg.ShowPcUid;
            returnMsg.SeeMainId = msg.SeeMainId;
            PlayerChar showPlayer = Api.PCManager.FindPc(msg.ShowPcUid);
            if (showPlayer == null)
            {
                Log.Warn("player {0} get show player from relation find show player {1} failed: not find ", msg.PcUid, msg.ShowPcUid);
                //通知Relation失败
                returnMsg.Result = (int)ErrorCode.Fail;
            }
            else
            {
                //找到玩家，直接获取信息
                returnMsg.Result = (int)ErrorCode.Success;
                List<int> heroIdList = showPlayer.HeroMng.GetAllHeroPosHeroId();
                returnMsg.ShowInfo = showPlayer.GetShowPlayerMsg(heroIdList);
            }
            Write(returnMsg, msg.PcUid);
        }

        private void OnResponse_ReturnShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_PLAYER_SHOW msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_PLAYER_SHOW>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} return show player from relation find show player {1} failed: not find ", uid, msg.ShowPcUid);
                return;
            }
            if (msg.Result != (int)ErrorCode.Success)
            {
                MSG_ZGC_SHOW_PLAYER infoMsg = new MSG_ZGC_SHOW_PLAYER();
                infoMsg.Result = (int)ErrorCode.Fail;
                infoMsg.IsEnd = true;
                player.Write(infoMsg);
            }
            else
            {
                player.SendPlayerInfoMsg(msg.ShowInfo);
            }
        }

        private void OnResponse_OneServerFindShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER>(stream);
            LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.ShowFind, msg.ShowPcUid, msg, uid);
        }

        private void OnResponse_NotFindShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NOT_FIND_SHOW_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NOT_FIND_SHOW_PLAYER>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} not find show player from relation find show player {1} failed: not find ", uid, msg.ShowPcUid);
                return;
            }
            LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.ShowNotFind, msg.ShowPcUid, msg, uid);
        }

  

        private void OnResponse_GetArenaChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_ARENA_CHALLENGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_ARENA_CHALLENGER>(stream);
            MSG_ZRZ_RETURN_ARENA_CHALLENGER returnMsg = new MSG_ZRZ_RETURN_ARENA_CHALLENGER();
            returnMsg.PcUid = msg.PcUid;
            returnMsg.ChallengerUid = msg.ChallengerUid;
            returnMsg.ChallengerDefensive.AddRange(msg.ChallengerDefensive);
            returnMsg.PcDefensive.AddRange(msg.PcDefensive);
            returnMsg.CDefPoses.AddRange(msg.CDefPoses);
            returnMsg.PDefPoses.AddRange(msg.PDefPoses);
            //
            returnMsg.GetType = msg.GetType;
            PlayerChar challenger = Api.PCManager.FindPc(uid);
            if (challenger == null)
            {
                Log.Warn("player {0} get arena challenger from relation find challenger {1} failed: not find ", msg.PcUid, uid);
                //通知Relation失败
                returnMsg.Result = (int)ErrorCode.Fail;
            }
            else
            {
                //找到玩家，直接获取信息
                returnMsg.Result = (int)ErrorCode.Success;
                MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = challenger.GetChallengerMsg();
                response.Info = challenger.GetArenaRankBaseInfo();
                returnMsg.Info = response;  //这个返回时候赋值
            }
            Write(returnMsg, msg.PcUid);
        }

        private void OnResponse_ReturnArenaChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_ARENA_CHALLENGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_ARENA_CHALLENGER>(stream);
     
            switch ((ChallengeIntoType)msg.GetType)
            {
                case ChallengeIntoType.Arena:
                    {
                        PlayerChar player = Api.PCManager.FindPc(uid);
                        if (player == null)
                        {
                            Log.Warn("player {0} return arena challenger from relation find show player {1} failed: not find ", uid, msg.ChallengerUid);
                            return;
                        }

                        PlayerRankBaseInfo rankInfo = player.ArenaMng.GetArenaRankInfo(msg.ChallengerUid);
                        if (rankInfo == null)
                        {
                            Log.WarnLine("player {0} return arena challenger info failed: not find rank info uid {1}", uid, msg.ChallengerUid);
                            return;
                        }
                        msg.Info.Info = player.GetArenaRankBaseInfo(rankInfo);
                        player.Write(msg.Info);

                        //缓存信息
                        foreach (var item in msg.Info.HeroList)
                        {
                            RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
                            rankInfo.HeroInfos.Add(robotInfo);
                        }
                        rankInfo.PetInfo = PlayerChar.GetRobotPetInfo(msg.Info.Pet);

                        rankInfo.NatureValues = new Dictionary<int, int>(msg.Info.NatureValues);
                        rankInfo.NatureRatios = new Dictionary<int, int>(msg.Info.NatureRatios);
                        //获取数据时间
                        rankInfo.UpdateTime = ZoneServerApi.now;
                    }
                    break;
                //case ChallengeIntoType.CrossPreliminary:
                //    {
                //        PlayerChar player = Api.PCManager.FindPc(uid);
                //        if (player == null)
                //        {
                //            Log.Warn("player {0} return arena challenger from relation find show player {1} failed: not find ", uid, msg.ChallengerUid);
                //            return;
                //        }

                //        PlayerRankBaseInfo rankInfo = player.CrossInfoMng.TempChallengerInfo;
                //        if (rankInfo == null)
                //        {
                //            Log.WarnLine("player {0} return cross challenger info failed: not find rank info uid {1}", uid, msg.ChallengerUid);
                //            return;
                //        }
                //        if (rankInfo.Uid != msg.ChallengerUid)
                //        {
                //            Log.WarnLine("player {0} return cross challenger info failed: temp uid {1} is not {2}", uid, rankInfo.Uid, msg.ChallengerUid);
                //            return;
                //        }
                //        //缓存信息
                //        foreach (var item in msg.Info.HeroList)
                //        {
                //            RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
                //            rankInfo.HeroInfos.Add(robotInfo);
                //        }
                //        //获取数据时间
                //        rankInfo.UpdateTime = ZoneServerApi.now;

                //        //进入战斗
                //        //player.EnterCrossBattleMap(rankInfo);
                //    }
                   
                //    break;
                //case ChallengeIntoType.CrossFinals:
                //    {
                //        PlayerRankBaseInfo player2 = GetArenaRankInfo(msg.Info);

                //        //player.EnterCrossBattleMap(rankInfo);
                //        PlayerChar player = Api.PCManager.FindPc(uid);
                //        if (player == null)
                //        {
                //            Api.CrossBattleMng.AddTempBattleInfo(uid, player2);

                //            //DB 获取
                //            LoadChallengerWithQuerys(msg.ChallengerUid, msg.ChallengerDefensive, msg.PcUid, msg.PcDefensive, (int)ChallengeIntoType.CrossFinalsTeturn,msg.PDefPoses,msg.CDefPoses);
                //        }
                //        else
                //        {
                //            //直接获取

                //            //找到玩家，直接获取信息
                //            MSG_ZRZ_RETURN_ARENA_CHALLENGER returnMsg = new MSG_ZRZ_RETURN_ARENA_CHALLENGER();
                //            returnMsg.PcUid = msg.PcUid;
                //            returnMsg.ChallengerUid = msg.ChallengerUid;
                //            returnMsg.ChallengerDefensive.AddRange(msg.ChallengerDefensive);
                //            returnMsg.PcDefensive.AddRange(msg.PcDefensive);
                //            returnMsg.CDefPoses.AddRange(msg.CDefPoses);
                //            returnMsg.PDefPoses.AddRange(msg.PDefPoses);
                //            returnMsg.GetType = msg.GetType;
                //            returnMsg.Result = (int)ErrorCode.Success;

                //            MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = player.GetChallengerMsg();
                //            response.Info = player.GetArenaRankBaseInfo();
                //            returnMsg.Info = response;  //这个返回时候赋值
                //            //returnMsg.Info = player.GetChallengerMsg();  //这个返回时候赋值
                //            Write(returnMsg, msg.PcUid);

                //            PlayerRankBaseInfo player1 = GetArenaRankInfo(returnMsg.Info);
                //            //添加到等待队列
                //            Api.CrossBattleMng.AddPlayerRankInfo(uid, player1, player2);

                //        }
                //    }

                    //break;
                default:
                    break;
            }
  
        }

        private static PlayerRankBaseInfo GetArenaRankInfo(MSG_ZGC_ARENA_CHALLENGER_HERO_INFO info)
        {
            PlayerRankBaseInfo rankInfo = PlayerChar.GetArenaRankInfo(info.Info);
            //缓存信息
            foreach (var item in info.HeroList)
            {
                RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
                rankInfo.HeroInfos.Add(robotInfo);
            }
            //获取数据时间
            rankInfo.UpdateTime = ZoneServerApi.now;
            return rankInfo;
        }

        private void OnResponse_NotFindArenaChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NOT_FIND_ARENA_CHALLENGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NOT_FIND_ARENA_CHALLENGER>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} not find arena challenger from relation find show player {1} failed: not find ", uid, msg.ChallengerUid);
                return;
            }
            LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.ChallengerUid, msg, uid);
        }
    }
}

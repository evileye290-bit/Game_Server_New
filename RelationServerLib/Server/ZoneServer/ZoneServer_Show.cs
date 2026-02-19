using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GetShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_SHOW_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_SHOW_PLAYER>(stream);
            Log.Write("player {0} get show player info find player {1}.", uid, pks.ShowPcUid);
            //到缓存中获取缓存信息
            ShowInfoMessage showInfo = ZoneManager.ShowMng.GetShowInfo(pks.ShowPcUid);
            if (showInfo != null)
            {
                //在缓存中找到信息，将信息发回ZONE
                MSG_ZRZ_RETURN_PLAYER_SHOW info = showInfo.Message;
                Write(info, uid);
            }
            else
            {
                //int mianId = BaseApi.GetMainIdByUid(pks.ShowPcUid);
                //if (mianId == api.MainId)
                {
                    //没有缓存信息，查看玩家是否在线
                    Client client = ZoneManager.GetClient(pks.ShowPcUid);
                    if (client != null)
                    {
                        //找到玩家说明玩家在线，通知玩家发送信息回来
                        MSG_RZ_GET_SHOW_PLAYER msg = new MSG_RZ_GET_SHOW_PLAYER();
                        msg.PcUid = pks.PcUid;
                        msg.ShowPcUid = pks.ShowPcUid;
                        client.Write(msg);
                    }
                    else
                    {
                        //没有找到玩家，通知ZONE自己去DB读取玩家信息
                        MSG_RZ_NOT_FIND_SHOW_PLAYER msg = new MSG_RZ_NOT_FIND_SHOW_PLAYER();
                        msg.PcUid = pks.PcUid;
                        msg.ShowPcUid = pks.ShowPcUid;
                        Write(msg, uid);
                    }
                }
                //else
                //{
                //    if (Api.CrossServer != null)
                //    {
                //        //通知cross server，获取信息
                //        MSG_RC_GET_SHOW_PLAYER msg = new MSG_RC_GET_SHOW_PLAYER();
                //        msg.PcUid = pks.PcUid;
                //        msg.ShowPcUid = pks.ShowPcUid;
                //        msg.MainId = Api.MainId;
                //        Api.CrossServer.Write(msg, uid);
                //    }
                //}
            }
        }

        public void OnResponse_GetCrossShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CROSS_SHOW_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_SHOW_PLAYER>(stream);
            Log.Write("player {0} get cross show player info find player {1}.", uid, pks.ShowPcUid);
            if (Api.CrossServer != null)
            {
                //通知cross server，获取信息
                MSG_RC_GET_SHOW_PLAYER msg = new MSG_RC_GET_SHOW_PLAYER();
                msg.PcUid = pks.PcUid;
                msg.ShowPcUid = pks.ShowPcUid;
                msg.MainId = pks.MainId;
                Api.CrossServer.Write(msg, uid);
            }
        }

        public void OnResponse_ReturnShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_PLAYER_SHOW pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_PLAYER_SHOW>(stream);

            //int mianId = BaseApi.GetMainIdByUid(uid);
            if (pks.SeeMainId == 0)
            {
                Client client = ZoneManager.GetClient(uid);
                if (client == null)
                {
                    //没有缓存信息，查看玩家是否在线
                    Log.Warn("player {0} return show player find show player {1} failed: not find ", uid, pks.ShowPcUid);
                    return;
                }

                if (pks.Result != (int)ErrorCode.Success)
                {
                    //没有找到玩家，通知ZONE自己去DB读取玩家信息
                    MSG_RZ_NOT_FIND_SHOW_PLAYER msg = new MSG_RZ_NOT_FIND_SHOW_PLAYER();
                    msg.PcUid = pks.PcUid;
                    msg.ShowPcUid = pks.ShowPcUid;
                    client.Write(msg);
                }
                else
                {
                    //找到玩家，将信息返回给zone
                    client.Write(pks);

                    //将信息添加到缓存中
                    ZoneManager.ShowMng.AddShowInfo(pks);
                }
            }
            else
            {
                //不是本服务器
                Api.CrossServer.Write(pks, uid);
            }
        }

        public void OnResponse_AddShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_PLAYER_SHOW pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_PLAYER_SHOW>(stream);
            if (pks.Info != null)
            {
                //将信息添加到缓存中
                ZoneManager.ShowMng.AddShowInfo(pks.Info);

                //    int mianId = BaseApi.GetMainIdByUid(uid);
                //    if (mianId != api.MainId)
                //    {
                //        //不是本服务器
                //        Api.CrossServer.Write(pks, uid);
                //    }
            }
        }

        public void OnResponse_GetChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_ARENA_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_ARENA_CHALLENGER>(stream);
            Log.Write("player {0} get arena challenger info find player {1}.", uid, pks.ChallengerUid);

            //int mianId = BaseApi.GetMainIdByUid(pks.ChallengerUid);
            //if (mianId == api.MainId)
            {
                //到缓存中获取缓存信息
                ArenaChallengerInfoMessage challengerInfo = ZoneManager.GetArenaChallengerInfo(pks.ChallengerUid);
                if (challengerInfo != null)
                {
                    //在缓存中找到信息，将信息发回ZONE
                    Write(challengerInfo.Message, uid);
                }
                else
                {
                    //没有缓存信息，查看玩家是否在线
                    Client client = ZoneManager.GetClient(pks.ChallengerUid);
                    if (client != null)
                    {
                        //找到玩家说明玩家在线，通知玩家发送信息回来
                        MSG_RZ_GET_ARENA_CHALLENGER msg = new MSG_RZ_GET_ARENA_CHALLENGER();
                        msg.PcUid = pks.PcUid;
                        msg.ChallengerUid = pks.ChallengerUid;
                        msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                        msg.PcDefensive.AddRange(pks.PcDefensive);
                        msg.CDefPoses.AddRange(pks.CDefPoses);
                        msg.PDefPoses.AddRange(pks.PDefPoses);
                        msg.GetType = pks.GetType;
                        client.Write(msg);
                    }
                    else
                    {
                        //没有找到玩家，通知ZONE自己去DB读取玩家信息
                        MSG_RZ_NOT_FIND_ARENA_CHALLENGER msg = new MSG_RZ_NOT_FIND_ARENA_CHALLENGER();
                        msg.PcUid = pks.PcUid;
                        msg.ChallengerUid = pks.ChallengerUid;

                        RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(pks.ChallengerUid);
                        if (baseInfo != null)
                        {
                            //int heroId = baseInfo.GetIntValue(HFPlayerInfo.HeroId);
                            string arenaDefensive = baseInfo.GetStringValue(HFPlayerInfo.ArenaDefensive);
                            string[] defensives = StringSplit.GetArray("|", arenaDefensive);
                            foreach (var defensive in defensives)
                            {
                                string[] hero = StringSplit.GetArray(":", defensive);
                                msg.ChallengerDefensive.Add(int.Parse(hero[0]));
                                msg.CDefPoses.Add(int.Parse(hero[1]));
                            }
                            //msg.ChallengerDefensive.Add(heroId);
                            //msg.ChallengerDefensive.AddRange(heroIds);
                            //msg.CDefPoses.AddRange(baseInfo.GetIntList4HeroPos());
                        }
                        else
                        {
                            msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                            msg.CDefPoses.AddRange(pks.CDefPoses);
                        }

                        baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(pks.PcUid);
                        if (baseInfo != null)
                        {
                            //int heroId = baseInfo.GetIntValue(HFPlayerInfo.HeroId);
                            //List<int> heroIds = baseInfo.GetIntList(HFPlayerInfo.ArenaDefensive);
                            string arenaDefensive = baseInfo.GetStringValue(HFPlayerInfo.ArenaDefensive);
                            string[] defensives = StringSplit.GetArray("|", arenaDefensive);
                            foreach (var defensive in defensives)
                            {
                                string[] hero = StringSplit.GetArray(":", defensive);
                                msg.PcDefensive.Add(int.Parse(hero[0]));
                                msg.PDefPoses.Add(int.Parse(hero[1]));
                            }
                            //msg.ChallengerDefensive.Add(heroId);
                            //msg.ChallengerDefensive.AddRange(heroIds);
                            //msg.CDefPoses.AddRange(baseInfo.GetIntList4HeroPos());
                        }
                        else
                        {
                            msg.PcDefensive.AddRange(pks.PcDefensive);
                            msg.PDefPoses.AddRange(pks.PDefPoses);
                        }
                       
                        msg.GetType = pks.GetType;
                        Write(msg, uid);

                        Api.RPlayerInfoMng.CheckUpdatePlayerInfo(pks.ChallengerUid);
                        Api.RPlayerInfoMng.CheckUpdatePlayerInfo(pks.PcUid);

                    }
                }
            }
            //else
            //{
            //    //需要跨服务器获取信息
            //    MSG_RC_GET_CHALLENGER msg = new MSG_RC_GET_CHALLENGER();
            //    msg.PcUid = pks.PcUid;
            //    msg.ChallengerUid = pks.ChallengerUid;
            //    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
            //    msg.PcDefensive.AddRange(pks.PcDefensive);
            //    msg.PDefPoses.AddRange(pks.PDefPoses);
            //    msg.CDefPoses.AddRange(pks.CDefPoses);
            //    msg.GetType = pks.GetType;
            //    Api.CrossServer.Write(msg, uid);
            //}
        }

        public void OnResponse_ReturnChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_ARENA_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_ARENA_CHALLENGER>(stream);

            //int mianId = BaseApi.GetMainIdByUid(uid);
            //if (mianId == api.MainId)
            {
                Client client = ZoneManager.GetClient(uid);
                if (client == null)
                {
                    //没有缓存信息，查看玩家是否在线
                    Log.Warn("player {0} return arena challenger find show player {1} failed: not find ", uid, pks.ChallengerUid);
                    return;
                }

                if (pks.Result != (int)ErrorCode.Success)
                {
                    //没有找到玩家，通知ZONE自己去DB读取玩家信息
                    MSG_RZ_NOT_FIND_ARENA_CHALLENGER msg = new MSG_RZ_NOT_FIND_ARENA_CHALLENGER();
                    msg.PcUid = pks.PcUid;
                    msg.ChallengerUid = pks.ChallengerUid;
                    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                    msg.PcDefensive.AddRange(pks.PcDefensive);
                    msg.CDefPoses.AddRange(pks.CDefPoses);
                    msg.PDefPoses.AddRange(pks.PDefPoses);
                    client.Write(msg);
                }
                else
                {
                    //找到玩家，将信息返回给zone
                    client.Write(pks);

                    //将信息添加到缓存中
                    ZoneManager.AddArenaChallengerInfo(pks.Info, pks.ChallengerUid);
                }
            }
            //else
            //{
            //    //不是本服务器
            //    Api.CrossServer.Write(pks, uid);
            //}
        }

        //public void OnResponse_ReturnCrossChallenger(MemoryStream stream, int uid = 0)
        //{
        //    MSG_ZRZ_RETURN_CROSS_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_CROSS_CHALLENGER>(stream);
        //    //将信息添加到缓存中
        //    ZoneManager.AddArenaChallengerInfo(pks.Info, pks.ChallengerUid);
        //    //不是本服务器
        //    Api.CrossServer.Write(pks, uid);
        //}

        public void OnResponse_AddChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO>(stream);
            if (pks.Info != null)
            {
                //将信息添加到缓存中
                ZoneManager.AddArenaChallengerInfo(pks.Info, pks.ChallengerUid);

                //int mianId = BaseApi.GetMainIdByUid(uid);
                //if (mianId != api.MainId)
                //{
                //    //不是本服务器
                //    Api.CrossServer.Write(pks, uid);
                //}
            }
        }
    }
}

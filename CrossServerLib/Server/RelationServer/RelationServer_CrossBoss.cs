using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossServerLib
{
    public partial class RelationServer
    {

        //获取决赛玩家信息
        public void OnResponse_GetCrossBossInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_CROSS_BOSS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_BOSS_INFO>(stream);
            Log.Write($"player {uid} GetCrossBossInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetCrossBossInfo from main {MainId} not find group id ");
                return;
            }
            SendCrossBossInfo(uid, groupId);
        }

        private void SendCrossBossInfo(int uid, int groupId, ErrorCode errorCode = ErrorCode.Success)
        {
            //获取当前总值值
            CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
            if (group == null)
            {
                Log.Warn($"player {uid} SendCrossBossInfo from main {MainId} not find group {groupId} ");
                return;
            }
            List<int> serverList = CrossBattleLibrary.GetGroupServers(groupId);
            int currentSiteId = 0;
            MSG_CorssR_GET_CROSS_BOSS_INFO msg = new MSG_CorssR_GET_CROSS_BOSS_INFO();
            msg.Uid = uid;
            msg.MainId = MainId;
            foreach (var kv in group.CurrentSiteInfos)
            {
                int siteId = kv.Value;
                if (!msg.SiteList.ContainsKey(siteId))
                {
                    CurrentBossSiteInfo siteInfo = group.GetSiteInfo(siteId);
                    if (siteInfo != null)
                    {
                        CorssR_CrossBossSiteInfo info = GetCrossBossSiteInfoMsg(siteInfo);
                        msg.SiteList.Add(siteInfo.Id, info);
                    }
                }

                int mainId = CrossBattleLibrary.GetGroupMainId(kv.Key, serverList);
                if (mainId > 0)
                {
                    msg.CurrentSiteList.Add(mainId, siteId);

                    if (mainId == MainId)
                    {
                        currentSiteId = siteId;
                    }
                }
            }

            foreach (var kv in group.defenseSiteInfos)
            {
                if (!msg.Defensers.ContainsKey(kv.Value))
                {
                    JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, kv.Value);
                    if (playerInfo != null)
                    {
                        CorssR_CrossBossDefenser playerMsg = new CorssR_CrossBossDefenser();

                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Uid, playerInfo.Uid.ToString()));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Name, playerInfo.Name));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.MainId, playerInfo.MainId.ToString()));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.HeroId, playerInfo.HeroId.ToString()));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.BattlePower, playerInfo.BattlePower.ToString()));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.GodType, playerInfo.GodType.ToString()));
                        playerMsg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Icon, playerInfo.HeroId.ToString()));

                        msg.Defensers.Add(kv.Value, playerMsg);
                    }
                    else
                    {
                        //获取
                        Log.Warn($"player {uid} SendCrossBossInfo from main {MainId} not find player info {kv.Value} ");
                    }
                }
                msg.SiteDefenseList.Add(kv.Key, kv.Value);
            }

            CrossBossRankManager rankMng = Api.RankMng.GetCrossBossRankManager(groupId);
            if (rankMng != null)
            {
                //查看有没有防守信息
                CrossBossDungeonModel dungeonModel = CrossBossLibrary.GetDungeonModel(currentSiteId);
                if (dungeonModel != null)
                {
                    CrossBossChapterRank chapterRank = rankMng.GetChapterRank(dungeonModel.Chapter);
                    if (chapterRank != null)
                    {
                        RankBaseModel rankInfo = chapterRank.GetRankBaseInfo(uid);
                        if (rankInfo != null)
                        {
                            msg.Score = rankInfo.Score;
                        }
                    }
                }
            }
            msg.Result = (int)errorCode;
            Write(msg, uid);
        }

        private static CorssR_HFPlayerBaseInfoItem GetPlayerInfoMsg(HFPlayerInfo key, string value)
        {
            CorssR_HFPlayerBaseInfoItem item = new CorssR_HFPlayerBaseInfoItem();
            item.Key = (int)key;
            item.Value = value;
            return item;
        }

        private static CorssR_CrossBossSiteInfo GetCrossBossSiteInfoMsg(CurrentBossSiteInfo siteInfo)
        {
            CorssR_CrossBossSiteInfo info = new CorssR_CrossBossSiteInfo();
            info.Id = siteInfo.Id;
            info.Hp = siteInfo.Hp;
            info.MaxHp = siteInfo.TotalHp;
            return info;
        }

        public void OnResponse_StartChallengeCrossBoss(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_ENTER_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_ENTER_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} StartChallengeCrossBoss.", uid);
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find group id ");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }
            //获取当前总值值
            CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
            if (group == null)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find group {groupId} ");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }

            int serverId = CrossBattleLibrary.GetGroupServerId(MainId);
            int siteId = group.GetCurrentSite(serverId);
            if (siteId <= 0)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find serverId {serverId} ");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }

            CurrentBossSiteInfo siteInfo = group.GetSiteInfo(siteId);
            if (siteInfo == null)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find siteId {siteId} ");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }

            if (siteInfo.Hp <= 0)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} siteId {siteId} hp {siteInfo.Hp}");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }
            //查看有没有防守信息
            CrossBossDungeonModel dungeonModel = CrossBossLibrary.GetDungeonModel(siteId);
            if (dungeonModel == null)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find siteId model {siteId} ");
                SendCrossBossInfo(uid, groupId, ErrorCode.CreateDungeonFailed);
                return;
            }


            CrossBossDungeonModel defenseModel = CrossBossLibrary.GetDefenseDungon(serverId, siteId);
            if (defenseModel != null)
            {
                //说明有防守阵容
                int defenseUid = group.GetDefenser(defenseModel.Id);
                if (defenseUid > 0)
                {
                    PlayerInfoMsgModel info = Api.CrossBossMng.GetPlayerInfoMsg(defenseUid);
                    if (info == null)
                    {
                        JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, defenseUid);
                        if (playerInfo != null)
                        {
                            //有防守人员，需要去服务器获取镜像信息
                            MSG_ZRZ_GET_BOSS_PLAYER_INFO msg = new MSG_ZRZ_GET_BOSS_PLAYER_INFO();
                            msg.FindPcUid = playerInfo.Uid;
                            msg.FindPcMainId = playerInfo.MainId;
                            msg.GetType = (int)ChallengeIntoType.CrossBoss;
                            msg.PcUid = uid;
                            msg.PcMainId = MainId;
                            msg.Hp = siteInfo.Hp;
                            msg.MaxHp = siteInfo.TotalHp;
                            msg.DungeonId = siteInfo.Id;
                            msg.Addbuff = group.CheckAddBuff(dungeonModel.BuffFrom);
                            Api.RelationManager.WriteToRelation(msg, msg.FindPcMainId, msg.FindPcUid);
                        }
                    }
                    else
                    {
                        //没有防守阵容，直接返回，开始战斗
                        MSG_ZRZ_RETURN_BOSS_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                        msg.GetType = (int)ChallengeIntoType.CrossBossReturn;
                        msg.Player1 = info.Item as ZR_BattlePlayerMsg;
                        msg.PcUid = uid;
                        msg.PcMainId = MainId;
                        msg.Hp = siteInfo.Hp;
                        msg.MaxHp = siteInfo.TotalHp;
                        msg.DungeonId = siteInfo.Id;
                        msg.Addbuff = group.CheckAddBuff(dungeonModel.BuffFrom);
                        Api.RelationManager.WriteToRelation(msg, msg.PcMainId, msg.PcUid);
                    }
                }
                else
                {
                    //没有防守阵容，直接返回，开始战斗
                    MSG_ZRZ_RETURN_BOSS_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                    msg.GetType = (int)ChallengeIntoType.CrossBossReturn;
                    //msg.Player1 = player.GetBossPlayerInfoMsg();
                    msg.PcUid = uid;
                    msg.PcMainId = MainId;
                    msg.Hp = siteInfo.Hp;
                    msg.MaxHp = siteInfo.TotalHp;
                    msg.DungeonId = siteInfo.Id;
                    msg.Addbuff = group.CheckAddBuff(dungeonModel.BuffFrom);
                    Api.RelationManager.WriteToRelation(msg, msg.PcMainId, msg.PcUid);
                }
            }
            else
            {
                //没有防守阵容，直接返回，开始战斗
                MSG_ZRZ_RETURN_BOSS_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                msg.GetType = (int)ChallengeIntoType.CrossBossReturn;
                //msg.Player1 = player.GetBossPlayerInfoMsg();
                msg.PcUid = uid;
                msg.PcMainId = MainId;
                msg.Hp = siteInfo.Hp;
                msg.MaxHp = siteInfo.TotalHp;
                msg.DungeonId = siteInfo.Id;
                msg.Addbuff = group.CheckAddBuff(dungeonModel.BuffFrom);
                Api.RelationManager.WriteToRelation(msg, msg.PcMainId, msg.PcUid);
            }
        }


        public void OnResponse_ReturnCrossBossPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>(stream);
            //Log.Write("player {0} ReturnCrossBossPlayerInfo.", uid);
            if (pks.Player1 == null)
            {
                Log.Warn($"player {uid} ReturnCrossBossPlayerInfo from main {MainId} not find player info ");
                return;
            }
            if (pks.PcUid > 0)
            {
                Api.RelationManager.WriteToRelation(pks, pks.PcMainId, pks.PcUid);
            }

            string name = "";
            if (pks.Player1 != null)
            {
                //保存挑战信息
                AddPlayerInfoMsg(pks.Player1);

                int groupId = CrossBattleLibrary.GetGroupId(MainId);
                if (groupId == 0)
                {
                    Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find group id ");
                    return;
                }

                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in pks.Player1.BaseInfo)
                {
                    HFPlayerInfo key = (HFPlayerInfo)item.Key;
                    switch (key)
                    {
                        case HFPlayerInfo.MainId:
                            dataList[key] = MainId;
                            break;
                        default:
                            dataList[key] = item.Value;
                            break;
                    }
                }
                //添加信息
                JsonPlayerInfo playerInfo = new JsonPlayerInfo(dataList);
                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);

                name = playerInfo.Name;
            }

            if (pks.DungeonId > 0)
            {
                //查看有没有防守信息
                CrossBossDungeonModel dungeonModel = CrossBossLibrary.GetDungeonModel(pks.DungeonId);
                if (dungeonModel == null)
                {
                    Log.Warn($"player {uid} StartChallengeCrossBoss from main {MainId} not find siteId model {pks.DungeonId} ");
                    return;
                }
                if (dungeonModel.Type == CrossBossSiteType.Defense && pks.GetType == (int)ChallengeIntoType.CrossBossSiteDefenseReturn)
                {
                    int groupId = CrossBattleLibrary.GetGroupId(MainId);
                    if (groupId == 0)
                    {
                        Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find group id ");
                        return;
                    }
                    //获取当前总值值
                    CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
                    if (group == null)
                    {
                        Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find group {groupId} ");
                        return;
                    }

                    CurrentBossSiteInfo siteInfo = group.GetSiteInfo(pks.DungeonId);
                    if (siteInfo == null)
                    {
                        Log.Warn($"player {uid} AddCrossBossScore from main {MainId} not find siteId {pks.DungeonId} ");
                        return;
                    }

                    //int serverId = CrossBattleLibrary.GetGroupServerId(MainId);
                    //CrossBossDungeonModel defenseModel = CrossBossLibrary.GetDefenseDungon(serverId, pks.DungeonId);


                    int defenseUid = group.GetDefenser(pks.DungeonId);
                    JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, defenseUid);
                    if (playerInfo != null)
                    {
                        MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                        rankMsg.MainId = playerInfo.MainId;
                        rankMsg.Uid = playerInfo.Uid;
                        rankMsg.EmailId = CrossBossLibrary.DefenseEmail;
                        rankMsg.Param = $"{CommonConst.DUNGEON_ID}:{pks.DungeonId}|{CommonConst.NAME}:{name}";
                        Api.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);
                        Api.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, Api.Now());
                    }

                    group.SetDefenser(pks.DungeonId, uid);
                    Api.CrossRedis.Call(new OperateSetCrossBossDefenseInfo(groupId, pks.DungeonId, uid));

                    SendCrossBossInfo(uid, groupId);
                }
            }
            
          
        }

        private void AddPlayerInfoMsg(ZR_BattlePlayerMsg msg)
        {
            PlayerInfoMsgModel info = new PlayerInfoMsgModel();
            info.Time = CrossServerApi.now.AddMinutes(CrossBossLibrary.InfoUpdateTime);
            info.Item = msg;
            Api.CrossBossMng.AddPlayerInfoMsg(msg.Uid, info);
        }

        public void OnResponse_GetCrossBossChallenger(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_CROSS_BOSS_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CROSS_BOSS_CHALLENGER>(stream);
            //Log.Write("player {0} GetCrossBossChallenger.", uid);
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find group id ");
                return;
            }
            //获取当前总值值
            CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
            if (group == null)
            {
                Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find group {groupId} ");
                return;
            }

            int serverId = CrossBattleLibrary.GetGroupServerId(MainId);
            int siteId = group.GetCurrentSite(serverId);
            if (siteId <= 0)
            {
                Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find serverId {serverId} ");
                return;
            }

            CrossBossDungeonModel defenseModel = CrossBossLibrary.GetDefenseDungon(serverId, siteId);
            if (defenseModel != null)
            {
                //说明有防守阵容
                int defenseUid = group.GetDefenser(defenseModel.Id);
                PlayerInfoMsgModel info = Api.CrossBossMng.GetPlayerInfoMsg(defenseUid);
                if (info == null)
                {
                    JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, defenseUid);
                    if (playerInfo != null)
                    {
                        //有防守人员，需要去服务器获取镜像信息
                        MSG_ZRZ_GET_BOSS_PLAYER_INFO msg = new MSG_ZRZ_GET_BOSS_PLAYER_INFO();
                        msg.FindPcUid = playerInfo.Uid;
                        msg.FindPcMainId = playerInfo.MainId;
                        msg.GetType = (int)ChallengeIntoType.CrossBossSite;
                        msg.PcUid = uid;
                        msg.PcMainId = MainId;
                        msg.DungeonId = defenseModel.Id;
                        Api.RelationManager.WriteToRelation(msg, msg.FindPcMainId, msg.FindPcUid);
                    }
                }
                else
                {
                    MSG_ZRZ_RETURN_BOSS_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                    msg.GetType = (int)ChallengeIntoType.CrossBossSiteReturn;
                    msg.Player1 = info.Item as ZR_BattlePlayerMsg;
                    msg.PcUid = uid;
                    msg.PcMainId = MainId;
                    msg.PcMainId = MainId;
                    msg.DungeonId = defenseModel.Id;
                    Api.RelationManager.WriteToRelation(msg, msg.PcMainId, msg.PcUid);
                }
            }
            else
            {
                //没有防守阵容
                Log.Warn($"player {uid} GetCrossBossChallenger from main {MainId} not find serverId {serverId}  site {siteId} defenser");
                return;
            }
        }

        public void OnResponse_ChallengeCrossBoss(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_CHALLENGE_CROSS_BOSS_MAP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CHALLENGE_CROSS_BOSS_MAP>(stream);
            Log.Write("player {0} ChallengeCrossBoss.", uid);
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find group id ");
                return;
            }
            //获取当前总值值
            CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
            if (group == null)
            {
                Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find group {groupId} ");
                return;
            }

            int serverId = CrossBattleLibrary.GetGroupServerId(MainId);
            int siteId = group.GetCurrentSite(serverId);
            if (siteId <= 0)
            {
                Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find serverId {serverId} ");
                return;
            }
            CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(siteId);
            if (model == null)
            {
                Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find model {siteId} ");
                return;
            }


            CrossBossDungeonModel defenseModel = CrossBossLibrary.GetDefenseDungon(serverId, siteId);
            if (defenseModel != null)
            {
                //说明有防守阵容
                int defenseUid = group.GetDefenser(defenseModel.Id);
                PlayerInfoMsgModel info = Api.CrossBossMng.GetPlayerInfoMsg(defenseUid);
                if (info == null)
                {
                    JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, defenseUid);
                    if (playerInfo != null)
                    {
                        //有防守人员，需要去服务器获取镜像信息
                        MSG_ZRZ_GET_BOSS_PLAYER_INFO msg = new MSG_ZRZ_GET_BOSS_PLAYER_INFO();
                        msg.FindPcUid = playerInfo.Uid;
                        msg.FindPcMainId = playerInfo.MainId;
                        msg.GetType = (int)ChallengeIntoType.CrossBossSiteFight;
                        msg.PcUid = uid;
                        msg.PcMainId = MainId;
                        msg.DungeonId = defenseModel.Id;
                        msg.Addbuff = group.CheckAddBuff(model.BuffFrom);
                        Api.RelationManager.WriteToRelation(msg, msg.FindPcMainId, msg.FindPcUid);
                    }
                    else
                    {
                        //没有防守阵容
                        Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find serverId {serverId}  site {siteId} defenser {defenseUid}");
                        return;
                    }
                }
                else
                {
                    MSG_ZRZ_RETURN_BOSS_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                    msg.GetType = (int)ChallengeIntoType.CrossBossSiteFightReturn;
                    msg.Player1 = info.Item as ZR_BattlePlayerMsg;
                    msg.PcUid = uid;
                    msg.PcMainId = MainId;
                    msg.DungeonId = defenseModel.Id;
                    msg.Addbuff = group.CheckAddBuff(model.BuffFrom);
                    Api.RelationManager.WriteToRelation(msg, msg.PcMainId, msg.PcUid);
                }
            }
            else
            {
                //没有防守阵容
                Log.Warn($"player {uid} ChallengeCrossBoss from main {MainId} not find serverId {serverId}  site {siteId} defenser");
                return;
            }
        }

        public void OnResponse_AddCrossBossScore(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CHANGE_CROSS_BOSS_SCORE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CHANGE_CROSS_BOSS_SCORE>(stream);
            int siteId = pks.SiteId;
            ulong scoreHp = pks.ScoreHp;
            int defenseUid = pks.DefenseUid;
            Log.Write("player {0} AddCrossBossScore hp {1}.", uid, scoreHp);

            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} AddCrossBossScore from main {MainId} not find group id ");
                return;
            }
            //获取当前总值值
            CrossBossGroupItem group = Api.CrossBossMng.GetGroup(groupId);
            if (group == null)
            {
                Log.Warn($"player {uid} AddCrossBossScore from main {MainId} not find group {groupId} ");
                return;
            }

            //查看有没有防守信息
            CrossBossDungeonModel dungeonModel = CrossBossLibrary.GetDungeonModel(siteId);
            if (dungeonModel == null)
            {
                Log.Warn($"player {uid} AddCrossBossScore from main {MainId} not find siteId model {siteId} ");
                return;
            }

            CurrentBossSiteInfo siteInfo = group.GetSiteInfo(siteId);
            if (siteInfo == null)
            {
                Log.Warn($"player {uid} AddCrossBossScore from main {MainId} not find siteId {siteId} ");
                return;
            }

            if (siteInfo.Hp <= 0)
            {
                //有人通关，不计分，返回通知增加次数
                MSG_CorssR_STOP_CROSS_BOSS_DUNGEON stopMsg = new MSG_CorssR_STOP_CROSS_BOSS_DUNGEON();
                stopMsg.Uid = uid;
                stopMsg.DungeonId = siteId;
                Api.RelationManager.BroadcastToGroupRelation(stopMsg, groupId);
                SendCrossBossInfo(uid, groupId);
                return;
            }

            ulong currentHp = 0;
            if (siteInfo.Hp > scoreHp)
            {
                currentHp = siteInfo.Hp - scoreHp;
            }
            siteInfo.Hp = currentHp;
            Api.CrossRedis.Call(new OperateSetCrossBossSiteInfo(groupId, siteInfo));

            bool addBossScore = true;
            int bossSiteId = CrossBossLibrary.GetBossDungeonId(dungeonModel.Chapter);
            CurrentBossSiteInfo bossSiteInfo = group.GetSiteInfo(bossSiteId);
            if (bossSiteInfo != null && bossSiteInfo.Hp == 0)
            {
                //说明已经击杀
                addBossScore = false;
            }
            //增加积分
            UpdateBossScore(groupId, dungeonModel.Chapter, siteId, uid, defenseUid, scoreHp, addBossScore);

            if (siteInfo.Hp <= 0)
            {
                SetNextSiteInfo(uid, siteId, group);

                //RankConfigInfo configInfo = RankLibrary.GetConfig(RankType.CrossBossSite);
                //if (configInfo == null)
                //{
                //    Log.Error($"rank {RankType.CrossBossSite} AddCrossBossScore init fail,can not find xml data ");
                //    return;
                //}
                //获取这个BOSS排行榜发奖励
                OperateGetCrossRankScore op = new OperateGetCrossRankScore(RankType.CrossBossSite,
                    groupId, siteId, 0, -1);
                Api.CrossRedis.Call(op, ret =>
                {
                    Dictionary<int, List<int>> rewardList = new Dictionary<int, List<int>>();
                    Dictionary<int, RankBaseModel> uidRankInfoDic = op.uidRank;
                    foreach (var item in uidRankInfoDic)
                    {
                        if (item.Value.Rank == 1)
                        {
                            //设置防守
                            if (dungeonModel.Type == CrossBossSiteType.Defense)
                            {
                                group.SetDefenser(siteId, item.Value.Uid);
                                Api.CrossRedis.Call(new OperateSetCrossBossDefenseInfo(groupId, siteId, item.Value.Uid));
                            }
                        }
                        JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, item.Value.Uid);
                        if (playerInfo != null)
                        {
                            //排行奖励
                            if (dungeonModel.RankRewards.Count >= item.Value.Rank)
                            {
                                string reward = dungeonModel.RankRewards[item.Value.Rank - 1];
                                MSG_CorssR_SEND_FINALS_REWARD msg = new MSG_CorssR_SEND_FINALS_REWARD();
                                msg.MainId = playerInfo.MainId;
                                msg.Uid = playerInfo.Uid;
                                msg.Reward = reward;
                                msg.EmailId = dungeonModel.RankEmail;
                                msg.Param = $"{CommonConst.DUNGEON_ID}:{dungeonModel.Id}|{CommonConst.RANK}:{item.Value.Rank}";
                                Api.RelationManager.WriteToRelation(msg, msg.MainId);

                                Api.TrackingLoggerMng.RecordSendEmailRewardLog(msg.Uid, msg.EmailId, msg.Reward, msg.Param, msg.MainId, Api.Now());
                                Api.TrackingLoggerMng.TrackRankEmailLog(groupId, siteId, RankType.CrossBossSite.ToString(), item.Value.Uid, item.Value.Score, dungeonModel.RankEmail, item.Value.Rank, Api.Now());
                                //BI
                                Api.KomoeEventLogRankFlow(item.Value.Uid, playerInfo.MainId, RankType.CrossBossSite, item.Value.Rank, item.Value.Rank, item.Value.Score, RewardManager.GetRewardDic(reward));
                            }
                        }
                        if (dungeonModel.Type != CrossBossSiteType.Boss)
                        {
                            MSG_CorssR_SEND_FINALS_REWARD msg = new MSG_CorssR_SEND_FINALS_REWARD();
                            msg.MainId = playerInfo.MainId;
                            msg.Uid = playerInfo.Uid;
                            msg.Reward = dungeonModel.Reward;
                            msg.EmailId = dungeonModel.Email;
                            msg.Param = $"{CommonConst.DUNGEON_ID}:{dungeonModel.Id}";
                            Api.RelationManager.WriteToRelation(msg, msg.MainId);

                            Api.TrackingLoggerMng.RecordSendEmailRewardLog(msg.Uid, msg.EmailId, msg.Reward, msg.Param, msg.MainId, Api.Now());
                            Api.TrackingLoggerMng.TrackRankEmailLog(groupId, siteId, "CrossBossSiteEnd", item.Value.Uid, item.Value.Score, dungeonModel.Email, item.Value.Rank, Api.Now());

                            //BI
                            Api.KomoeEventLogRankFlow(item.Value.Uid, playerInfo.MainId, RankType.CrossBossSite, item.Value.Rank, item.Value.Rank, item.Value.Score, RewardManager.GetRewardDic(dungeonModel.Reward));

                        }
                    }

                    //通关奖励
                    if (dungeonModel.Type == CrossBossSiteType.Boss)
                    {
                        //公告
                        Api.RelationManager.BroadcastAnnouncement(ANNOUNCEMENT_TYPE.CROSS_BOSS_PASS, MainId, siteInfo.Id);

                        //configInfo = RankLibrary.GetConfig(RankType.CrossBoss);
                        //if (configInfo == null)
                        //{
                        //    Log.Error($"rank {RankType.CrossBoss} AddCrossBossScore init fail,can not find xml data ");
                        //    return;
                        //}

                        //获取这个BOSS排行榜发奖励
                        OperateGetCrossRankScore totalOp = new OperateGetCrossRankScore(RankType.CrossBoss,
                            groupId, dungeonModel.Chapter, 0, -1);
                        Api.CrossRedis.Call(totalOp, totalRet =>
                        {
                            if (totalOp.uidRank.Count > 0)
                            {
                                List<string> rankInfoList = new List<string>();
                                int randMainId = 0;

                                foreach (var rankItem in totalOp.uidRank)
                                {
                                    JsonPlayerInfo rankPlayerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, rankItem.Value.Uid);
                                    if (rankPlayerInfo != null)
                                    {
                                        if (rankItem.Value.Rank == 1)
                                        {
                                            MSG_CorssR_CROSS_BOSS_RANK_REWARD firstMsg = new MSG_CorssR_CROSS_BOSS_RANK_REWARD();
                                            firstMsg.DungeonId = siteId;
                                            Api.RelationManager.WriteToRelation(firstMsg, rankPlayerInfo.MainId);
                                        }

                                        CampBuildRankRewardData data = CrossBossLibrary.GetRankRewardInfo(dungeonModel.Chapter, rankItem.Value.Rank);
                                        if (data == null)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            MSG_CorssR_SEND_FINALS_REWARD rankMsg = new MSG_CorssR_SEND_FINALS_REWARD();
                                            rankMsg.MainId = rankPlayerInfo.MainId;
                                            rankMsg.Uid = rankPlayerInfo.Uid;
                                            rankMsg.Reward = data.Rewards;
                                            rankMsg.EmailId = data.EmailId;
                                            rankMsg.Param = $"{CommonConst.RANK}:{rankItem.Value.Rank}";
                                            Api.RelationManager.WriteToRelation(rankMsg, rankMsg.MainId);

                                            Api.TrackingLoggerMng.RecordSendEmailRewardLog(rankMsg.Uid, rankMsg.EmailId, rankMsg.Reward, rankMsg.Param, rankMsg.MainId, Api.Now());
                                            Api.TrackingLoggerMng.TrackRankEmailLog(groupId, dungeonModel.Chapter, RankType.CrossBoss.ToString(), rankItem.Value.Uid, rankItem.Value.Score, data.EmailId, rankItem.Value.Rank, Api.Now());

                                            if (rankItem.Value.Rank < 100)
                                            {
                                                rankInfoList.Add(rankItem.Value.Rank + "_" + rankItem.Value.Uid + "_" + rankItem.Value.Score);
                                                randMainId = rankPlayerInfo.MainId;
                                            }
                                            //BI
                                            Api.KomoeEventLogRankFlow(rankItem.Value.Uid, rankPlayerInfo.MainId, RankType.CrossBoss, rankItem.Value.Rank, rankItem.Value.Rank, rankItem.Value.Score, RewardManager.GetRewardDic(data.Rewards));

                                        }
                                    }
                                }
                                //通知玩家自己领取
                                MSG_CorssR_CROSS_BOSS_PASS_REWARD msg = new MSG_CorssR_CROSS_BOSS_PASS_REWARD();
                                msg.DungeonId = siteId;
                                Api.RelationManager.BroadcastToGroupRelation(msg, groupId);

                                RelationManager.SendRankInfoToRelation("crossBoss", rankInfoList, randMainId);
                            }
                            SendCrossBossInfo(uid, groupId);
                        });
                    }
                    else
                    {
                        SendCrossBossInfo(uid, groupId);
                    }
                });
            }
            else
            {
                SendCrossBossInfo(uid, groupId);
            }
        }

        private void UpdateBossScore(int groupId,int chapter, int siteId, int uid, int defenseUid, ulong scoreHp, bool addBossScore)
        {
            int pcAddScore = (int)((scoreHp * CrossBossLibrary.ScoreParamA) + CrossBossLibrary.ScoreParamB);
            int defenseAddScore = 0;
            if (defenseUid > 0)
            {
                defenseAddScore = (int)(pcAddScore * CrossBossLibrary.ScoreParamC);
            }

            CrossBossRankManager rankMng = Api.RankMng.GetCrossBossRankManager(groupId);
            if (rankMng != null)
            {
                CrossBossChapterRank chapterRank = rankMng.GetChapterRank(chapter);
                if (chapterRank == null)
                {
                    rankMng.AddChapterRank(groupId, chapter);
                    chapterRank = rankMng.GetChapterRank(chapter);
                }

                CrossBossSiteRank siteRank = rankMng.GetSiteRank(siteId);
                if (siteRank == null)
                {
                    rankMng.AddSiteRank(groupId, siteId);
                    siteRank = rankMng.GetSiteRank(siteId);
                }

                if (addBossScore)
                {
                    chapterRank.UpdateScore(uid, pcAddScore);
                }
                siteRank.UpdateScore(uid, pcAddScore);

                if (defenseAddScore > 0)
                {
                    if (addBossScore)
                    {
                        chapterRank.UpdateScore(defenseUid, defenseAddScore);
                    }
                    siteRank.UpdateScore(defenseUid, defenseAddScore);
                }
            }
        }

        private void SetNextSiteInfo(int uid, int siteId, CrossBossGroupItem group)
        {
            ////获取当前服务器序号
            //int curentSserverId = CrossBattleLibrary.GetGroupServerId(MainId);
            ////获取当前服务器副本列表
            //List<int> list = CrossBossLibrary.GetDungeonIds(curentSserverId);
            //if (list.Count > 0)
            //{
            //    //当前副本的位置
            //    int index = list.IndexOf(siteId);
            //    if (index != -1)
            //    {
            //        //判断旧节点信息
            //        int nextIndex = index;
            //        bool findNext = FindNextSiteByOldId(group, curentSserverId, list, 0, index);
            //        if (!findNext)
            //        {
            //            //说明旧的节点都打过了，查看新节点
            //            FindNextSiteByOldId(group, curentSserverId, list, index, list.Count);
            //        }


            //        if (list.Count > index + 1)
            //        {
            //            //说明包含这个ID，有下一个节点
            //            nextIndex = index + 1;

            //        }

            //        for (int i = nextIndex; i < list.Count; i++)
            //        {
            //            int nextSitId = list[nextIndex];
            //            CurrentBossSiteInfo nextDungeonInfo = group.GetSiteInfo(nextSitId);
            //            if (nextDungeonInfo != null)
            //            {
            //                group.SetCurrentSite(serverId, nextSitId);
            //                //说明已经攻击
            //                if (nextDungeonInfo.Hp > 0)
            //                {
            //                    break;
            //                }
            //                else
            //                {
            //                    //已经攻击过
            //                    continue;
            //                }
            //            }
            //            else
            //            {
            //                //新节点
            //                CrossBossDungeonModel nextDungeonModel = CrossBossLibrary.GetDungeonModel(nextSitId);
            //                if (nextDungeonModel != null)
            //                {
            //                    Api.CrossBossMng.AddAddSiteInfo(group, serverId, nextDungeonModel);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //没有找到
            //        Log.Warn($"player {uid} SetNextSiteInfo from main {MainId} not find siteId {siteId} by curent server id {curentSserverId} ");
            //    }
            //}
            //else
            //{
            //    //没有找到
            //    Log.Warn($"player {uid} SetNextSiteInfo from main {MainId} not find siteId list by curent server id {curentSserverId} ");
            //}


            //击杀BOSS, 遍历副本
            foreach (var item in CrossBossLibrary.ServerDungeonList)
            {
                int serverId = item.Key;
                List<int> list = item.Value; // CrossBossLibrary.GetDungeonIds(serverId);
                if (list.Count > 0)
                {
                    int index = list.IndexOf(siteId);
                    if (index == -1)
                    {
                        //没有找到
                        Log.Warn($"player {uid} SetNextSiteInfo from main {MainId} not find siteId {siteId} by  server id {serverId} ");
                        continue;
                    }
                    else
                    {
                        bool findNext = FindNextSiteByOldId(group, serverId, list, 0, index);
                        if (!findNext)
                        {
                            //说明旧的节点都打过了，查看新节点
                            FindNextSiteByOldId(group, serverId, list, index, list.Count);
                        }

                        //for (int i = nextIndex; i < list.Count; i++)
                        //{
                        //    int nextSitId = list[nextIndex];
                        //    CurrentBossSiteInfo nextDungeonInfo = group.GetSiteInfo(nextSitId);
                        //    if (nextDungeonInfo != null)
                        //    {
                        //        group.SetCurrentSite(serverId, nextSitId);
                        //        //说明已经攻击
                        //        if (nextDungeonInfo.Hp > 0)
                        //        {
                        //            break;
                        //        }
                        //        else
                        //        {
                        //            //已经攻击过
                        //            continue;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //新节点
                        //        CrossBossDungeonModel nextDungeonModel = CrossBossLibrary.GetDungeonModel(nextSitId);
                        //        if (nextDungeonModel != null)
                        //        {
                        //            Api.CrossBossMng.AddAddSiteInfo(group, serverId, nextDungeonModel);
                        //            break;
                        //        }
                        //    }
                        //}
                        //if (list.Count > nextIndex)
                        //{
                        //    int nextSitId = list[nextIndex];
                        //    CrossBossDungeonModel nextDungeonModel = CrossBossLibrary.GetDungeonModel(nextSitId);
                        //    if (nextDungeonModel != null)
                        //    {
                        //        Api.CrossBossMng.AddAddSiteInfo(group, serverId, nextDungeonModel);
                        //    }
                        //}
                    }
                }
            }
        }

        private bool FindNextSiteByOldId(CrossBossGroupItem group, int serverId, List<int> list, int start, int end)
        {
            bool findNext = false;
            for (int i = start; i < end; i++)
            {
                //确认之前副本都打过了
                int nextSitId = list[i];
                CurrentBossSiteInfo nextDungeonInfo = group.GetSiteInfo(nextSitId);
                if (nextDungeonInfo != null)
                {
                    //说明已经攻击
                    if (nextDungeonInfo.Hp > 0)
                    {
                        //没有攻击完，继续攻击
                        group.SetCurrentSite(serverId, nextSitId);
                        findNext = true;
                        break;
                    }
                    else
                    {
                        //已经攻击过,检查下一个
                        continue;
                    }
                }
                else
                {
                    //这个旧节点没有攻击过，需要补上
                    CrossBossDungeonModel nextDungeonModel = CrossBossLibrary.GetDungeonModel(nextSitId);
                    if (nextDungeonModel != null)
                    {
                        Api.CrossBossMng.AddAddSiteInfo(group, serverId, nextDungeonModel);
                        findNext = true;
                        break;
                    }
                }
            }
            return findNext;
        }
    }
}

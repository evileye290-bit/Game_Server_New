using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using Message.Corss.Protocol.CorssR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using CommonUtility;
using DBUtility;
using Google.Protobuf.Collections;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        /// <summary>
        /// 获取当前服务器前8
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_GetCrossFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_BATTLE_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_BATTLE_RANK>(stream);
            Log.Write("cross server GetCrossFinalsPlayerRank");
            //获取当前赛季人数
            Api.CrossBattleMng.LoadFinalsPlayers();
        }

        //获取玩家对战信息
        public void OnResponse_GetCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_BATTLE_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_BATTLE_PLAYER>(stream);
            Log.Write("cross server GetCrossBattlePlayerInfo");

            MSG_RZ_GET_BATTLE_PLAYER msg = new MSG_RZ_GET_BATTLE_PLAYER();
            msg.GetType = pks.GetType;
            msg.Player1 = GetPlayerBaseInfoMsg(pks.Player1);
            msg.Player2 = GetPlayerBaseInfoMsg(pks.Player2);
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;
            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg, pks.Player1.Uid);
            }
        }

        public RZ_BattlePlayerMsg GetPlayerBaseInfoMsg(CorssR_BattlePlayerMsg baseInfo)
        {
            RZ_BattlePlayerMsg info = new RZ_BattlePlayerMsg();
            info.Uid = baseInfo.Uid;
            info.MainId = baseInfo.MainId;
            info.Index = baseInfo.Index;
            info.OldTeam = baseInfo.OldTeam;
            HFPlayerBaseInfoItem item;
            foreach (var kv in baseInfo.BaseInfo)
            {
                item = new HFPlayerBaseInfoItem();
                item.Key = kv.Key;
                item.Value = kv.Value;
                info.BaseInfo.Add(item);
            }
            return info;
        }

        public void OnResponse_ReturnCrossBattlePlayerInfoNew(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossBattlePlayerInfoNew");

            //MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO();
            //msg.GetType = pks.GetType;
            //msg.Player1 = GetBattlePlayerInfoMsg(pks.Player1);
            //msg.Player2 = GetBattlePlayerInfoMsg(pks.Player2);
            //msg.TimingId = pks.TimingId;
            //msg.GroupId = pks.GroupId;
            //msg.TeamId = pks.TeamId;
            //msg.FightId = pks.FightId;

            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(pks, pks.Player1.Uid);
            }
        }

        //返回信息
        public void OnResponse_ReturnCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RCR_RETURN_BATTLE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RCR_RETURN_BATTLE_PLAYER_INFO>(stream);
            Log.Write("cross server ReturnCrossBattlePlayerInfo");

            MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO msg = new MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO();
            msg.GetType = pks.GetType;
            msg.Player1 = GetBattlePlayerInfoMsg(pks.Player1);
            msg.Player2 = GetBattlePlayerInfoMsg(pks.Player2);
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;

            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg, pks.Player1.Uid);
            }
        }

        public ZR_BattlePlayerMsg GetBattlePlayerInfoMsg(RC_BattlePlayerMsg info)
        {
            ZR_BattlePlayerMsg response = new ZR_BattlePlayerMsg();
            response.Uid = info.Uid;
            response.MainId = info.MainId;
            response.Index = info.Index;
            //基本信息
            foreach (var item in info.BaseInfo)
            {
                ZR_HFPlayerBaseInfoItem baseInfo = new ZR_HFPlayerBaseInfoItem();
                baseInfo.Key = item.Key;
                baseInfo.Value = item.Value;
                response.BaseInfo.Add(baseInfo);
            }

            foreach (var item in info.BaseInfo)
            {
                ZR_HFPlayerBaseInfoItem baseInfo = new ZR_HFPlayerBaseInfoItem();
                baseInfo.Key = item.Key;
                baseInfo.Value = item.Value;
                response.BaseInfo.Add(baseInfo);
            }

            //伙伴信息
            foreach (var item in info.Heros)
            {
                ZR_Show_HeroInfo rCInfo = new ZR_Show_HeroInfo();
                rCInfo.Id = item.Id;
                rCInfo.Level = item.Level;
                rCInfo.StepsLevel = item.StepsLevel;
                rCInfo.SoulSkillLevel = item.SoulSkillLevel;
                rCInfo.GodType = item.GodType;
                rCInfo.QueueNum = item.CrossQueueNum;
                rCInfo.PositionNum = item.CrossPositionNum;
                rCInfo.Power = item.Power;

                //魂环
                foreach (var ring in item.SoulRings)
                {
                    ZR_Show_SoulRing ringMsg = new ZR_Show_SoulRing();
                    ringMsg.Id = ring.SpecId;
                    ringMsg.SpecId = ring.SpecId;
                    ringMsg.Element = ring.Element;
                    ringMsg.Pos = ring.Pos;
                    ringMsg.Year = ring.Year;
                    rCInfo.SoulRings.Add(ringMsg);
                }
                //魂骨
                foreach (var bone in item.SoulBones)
                {
                    ZR_Show_SoulBone boneMsg = new ZR_Show_SoulBone();
                    boneMsg.Id = bone.Id;
                    boneMsg.SpecIds.AddRange(bone.SpecIds);
                    rCInfo.SoulBones.Add(boneMsg);
                }
                //属性
                if (item.Natures != null)
                {
                    rCInfo.Natures = GetNature(item.Natures);
                }
                //暗器
                if (item.HiddenWeapon != null)
                {
                    ZR_Hero_HiddenWeapon weapon = new ZR_Hero_HiddenWeapon()
                    {
                        Id = item.HiddenWeapon.Id,
                        Star = item.HiddenWeapon.Star
                    };
                    rCInfo.HiddenWeapon = weapon;
                }
                //装备
                foreach (var id in item.Equipments)
                {
                    rCInfo.Equipments.Add(new ZR_Show_Equipment() { Id = id });
                }
                response.Heros.Add(rCInfo);
            }
            return response;
        }

        public ZR_Hero_Nature GetNature(RC_Hero_Nature nature)
        {
            ZR_Hero_Nature heroNature = new ZR_Hero_Nature();
            foreach (var item in nature.List)
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = item.NatureType;
                info.Value = item.Value;
                heroNature.List.Add(info);
            }
            return heroNature;
        }

        //显示决赛信息
        public void OnResponse_ShowCrossBattleFinals(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} ShowCrossBattleFinals failed: not find ", uid);
                return;
            }

            MSG_RZ_SHOW_CROSS_BATTLE_FINALS_INFO msg = new MSG_RZ_SHOW_CROSS_BATTLE_FINALS_INFO();
            msg.TeamId = pks.TeamId;

            foreach (var item in pks.List)
            {
                msg.List.Add(GetPlayerBaseInfoMsg(item));
            }

            msg.Fight1.AddRange(pks.Fight1);
            msg.Fight2.AddRange(pks.Fight2);
            msg.Fight3.AddRange(pks.Fight3);

            client.Write(msg);
        }

        //显示玩家信息
        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RCR_CROSS_BATTLE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RCR_CROSS_BATTLE_CHALLENGER>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} ShowCrossBattleChallenger failed: not find ", uid);
                return;
            }

            MSG_RZ_CROSS_BATTLE_CHALLENGER msg = new MSG_RZ_CROSS_BATTLE_CHALLENGER();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.Result = pks.Result;
            foreach (var item in pks.Heros)
            {
                msg.Heros.Add(GetPlayerHeroInfoMsg(item));
            }
            client.Write(msg);
        }

        public RZ_Show_HeroInfo GetPlayerHeroInfoMsg(RCR_Show_HeroInfo baseInfo)
        {
            RZ_Show_HeroInfo info = new RZ_Show_HeroInfo();
            info.Id = baseInfo.Id;
            info.Level = baseInfo.Level;
            info.StepsLevel = baseInfo.StepsLevel;
            info.TitleLevel = baseInfo.TitleLevel;
            info.SoulSkillLevel = baseInfo.SoulSkillLevel;
            info.GodType = baseInfo.GodType;
            info.QueueNum = baseInfo.QueueNum;
            info.PositionNum = baseInfo.PositionNum;
            info.ComboPower = baseInfo.ComboPower;
            info.Power = baseInfo.Power;

            foreach (var item in baseInfo.SoulRings)
            {
                info.SoulRings.Add(GetShowSoulRingMsg(item));
            }

            foreach (var item in baseInfo.SoulBones)
            {
                info.SoulBones.Add(GetShowSoulBoneMsg(item));
            }

            foreach (var item in baseInfo.Equipments)
            {
                info.Equipments.Add(GetShowEquipmentMsg(item));
            }

            if (baseInfo.HiddenWeapon != null)
            {
                info.HiddenWeapon = new RZ_Show_HiddenWeapon()
                {
                    Id = baseInfo.HiddenWeapon.Id,
                    Level = baseInfo.HiddenWeapon.Level,
                    Star = baseInfo.HiddenWeapon.Star,
                    WashList = { baseInfo.HiddenWeapon.WashList }
                };
            }

            return info;
        }

        private RZ_Show_SoulRing GetShowSoulRingMsg(RCR_Show_SoulRing rInfo)
        {
            RZ_Show_SoulRing BaseInfo = new RZ_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private RZ_Show_SoulBone GetShowSoulBoneMsg(RCR_Show_SoulBone rInfo)
        {
            RZ_Show_SoulBone BaseInfo = new RZ_Show_SoulBone();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Prefix = rInfo.Prefix;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.AnimalType = rInfo.AnimalType;
            BaseInfo.Quality = rInfo.Quality;
            BaseInfo.Prefix = rInfo.Prefix;
            BaseInfo.MainNatureType = rInfo.MainNatureType;
            BaseInfo.MainNatureValue = rInfo.MainNatureValue;
            BaseInfo.AdditionType1 = rInfo.AdditionType1;
            BaseInfo.AdditionType2 = rInfo.AdditionType2;
            BaseInfo.AdditionValue1 = rInfo.AdditionValue1;
            BaseInfo.AdditionValue2 = rInfo.AdditionValue2;
            BaseInfo.AdditionType3 = rInfo.AdditionType3;
            BaseInfo.AdditionValue3 = rInfo.AdditionValue3;
            BaseInfo.SpecIds.AddRange(rInfo.SpecIds);
            BaseInfo.Score = rInfo.Score;

            return BaseInfo;
        }
        private RZ_Show_Equipment GetShowEquipmentMsg(RCR_Show_Equipment rInfo)
        {
            RZ_Show_Equipment BaseInfo = new RZ_Show_Equipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Level = rInfo.Level;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.Score = rInfo.Score;

            BaseInfo.Slot = GetShowEquipmentSlotMsg(rInfo.Slot);
            return BaseInfo;
        }

        private RZ_Show_Equipment_Slot GetShowEquipmentSlotMsg(RCR_Show_Equipment_Slot rInfo)
        {
            RZ_Show_Equipment_Slot BaseInfo = new RZ_Show_Equipment_Slot();
            BaseInfo.JewelTypeId = rInfo.JewelTypeId;

            foreach (var item in rInfo.Injections)
            {
                RZ_Show_Equipment_Injection info = new RZ_Show_Equipment_Injection();
                info.NatureType = item.NatureType;
                info.NatureValue = item.NatureValue;
                info.InjectionSlot = item.InjectionSlot;
                BaseInfo.Injections.Add(info);
            }
            return BaseInfo;
        }

        //显示玩家阵容信息
        public void OnResponse_GetCrossBattleHeros(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_BATTLE_HEROS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_BATTLE_HEROS>(stream);

            MSG_RZ_GET_BATTLE_HEROS msg = new MSG_RZ_GET_BATTLE_HEROS();
            msg.Uid = pks.Uid;
            msg.MainId = pks.MainId;
            msg.SeeUid = pks.SeeUid;
            msg.SeeMainId = pks.SeeMainId;
            Client client = Api.ZoneManager.GetClient(pks.Uid);
            if (client == null)
            {
                FrontendServer server = Api.ZoneManager.GetOneServer();
                if (server != null)
                {
                    server.Write(msg);
                }
            }
            else
            {
                client.Write(msg);
            }
        }

        //获取录像名
        public void OnResponse_GetCrossBattleVedio(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_CROSS_VIDEO>(stream);
            MSG_RZ_GET_CROSS_VIDEO request = new MSG_RZ_GET_CROSS_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.VideoName = pks.VideoName;
            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.Write(request);
            }
        }

        //发送决赛奖励
        public void OnResponse_SendCrossBattleFinalsReward(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_SEND_FINALS_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_SEND_FINALS_REWARD>(stream);
            //发送邮件
            Api.EmailMng.SendPersonEmail(pks.Uid, pks.EmailId, pks.Reward, 0, pks.Param);
        }     

        //清理排行榜
        public void OnResponse_ClearCrossFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            //MSG_CorssR_CLEAR_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CLEAR_PLAYER_FINAL>(stream);
            Log.Write("cross server ClearCrossFinalsPlayerRank");
            //清空所有人决战排名
            Api.CrossBattleMng.ClaerLastFinalsPlayers();
        }

        //更新玩家排名
        public void OnResponse_UpdateCrossFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_UPDATE_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_UPDATE_PLAYER_FINAL>(stream);
            Log.Write("cross server UpdateCrossFinalsPlayerRank");
            //更新当前赛季人数
            Api.CrossBattleMng.SyncFinalsPlayerResult(pks.List);
        }

        //清理排行榜
        public void OnResponse_ClearCrossBattleRanks(MemoryStream stream, int uid = 0)
        {
            //MSG_CorssR_CLEAR_BATTLE_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CLEAR_BATTLE_RANK>(stream);
            Log.Write("cross server ClearCrossBattleRanks");
            //更新当前赛季人数
            Api.RankMng.CrossBattleRank.ResetRankList();

            //清理数据
            Api.GameDBPool.Call(new QueryRefreshAllCrossBattleResult());

            MSG_RZ_CLEAR_BATTLE_RANK msg = new MSG_RZ_CLEAR_BATTLE_RANK();
            Api.ZoneManager.Broadcast(msg);

            //清理下注
            Api.CrossGuessingMng.ClearGuessingInfo();

            MSG_RZ_OPEN_GUESSING_TEAM guessMsg = new MSG_RZ_OPEN_GUESSING_TEAM();
            guessMsg.TeamId = Api.CrossGuessingMng.TeamId;
            Api.ZoneManager.Broadcast(guessMsg);
        }

        //同步开启时间
        public void OnResponse_CrossBattleStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_BATTLE_START>(stream);
            Log.Write("cross server CrossBattleStart");
   

            MSG_RZ_BATTLE_START msg = new MSG_RZ_BATTLE_START();
            msg.Time = pks.Time;
            msg.TeamId = Api.CrossGuessingMng.TeamId;
            foreach (var server in Api.ZoneManager.ServerList)
            {
                server.Value.Write(msg, uid);
            }
        }

        //通知跑马灯和邮件
        public void OnResponse_NoticePlayerBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_PLAYER_BATTLE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_PLAYER_BATTLE_INFO>(stream);
            Log.Write("cross server NoticePlayerBattleInfo");
            //更新当前赛季人数
            Api.CrossBattleMng.NoticePlayerBattleInfo(pks.TimingId, pks.List);
        }
        //通知第一名
        public void OnResponse_NoticePlayerFirst(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_CROSS_BATTLE_WIN_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_CROSS_BATTLE_WIN_FINAL>(stream);
            Log.Write("cross server NoticePlayerFirst");
            //更新当前赛季人数
            Api.CrossBattleMng.NoticePlayerFirst(pks.MainId, pks.Name);
            RepeatedField<int> list = new RepeatedField<int>();
            list.Add(pks.Uid);
            Api.CrossGuessingMng.SendGuessingReward((int)CrossBattleTiming.BattleTime6, list);
        }
        //获取开战时间
        public void OnResponse_GetCrossBattleStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_BATTLE_START>(stream);
            Log.Write("cross server GetCrossBattleStart");

            MSG_RZ_GET_BATTLE_START msg = new MSG_RZ_GET_BATTLE_START();
            msg.Time = pks.Time;
            msg.TeamId = Api.CrossGuessingMng.TeamId;
            foreach (var server in Api.ZoneManager.ServerList)
            {
                server.Value.Write(msg, uid);
            }
        }

        //开启竞猜
        public void OnResponse_CrossBattleGuessingStart(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_CROSS_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_CROSS_GUESSING_INFO>(stream);
            Log.Write("cross server CrossBattleGuessingStart");
            Api.CrossGuessingMng.SetGuessingPlayers(pks.TimingId, pks.Uid1, pks.Uid2, pks.TeanId);

            MSG_RZ_OPEN_GUESSING_TEAM msg = new MSG_RZ_OPEN_GUESSING_TEAM();
            msg.TeamId = Api.CrossGuessingMng.TeamId;
            Api.ZoneManager.Broadcast(msg);
        }

        public void OnResponse_GetGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_GET_GUESSING_INFO>(stream);
            Log.Write("cross server GetGuessingPlayersInfo");

            Client client = Api.ZoneManager.GetClient(uid);
            if (client != null)
            {
                MSG_RZ_GET_GET_GUESSING_INFO msg = new MSG_RZ_GET_GET_GUESSING_INFO();
                foreach (var info in pks.InfoList)
                {
                    msg.InfoList.Add(GetPlayerBaseInfoMsg(info));
                }
                msg.GuessingInfos.AddRange(Api.CrossGuessingMng.GetGuessingPlayersInfoMsg(uid));
                client.Write(msg);
            }
        }


        public void OnResponse_CrossBattleGuessingResult(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_CROSS_GUESSING_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_CROSS_GUESSING_RESULT>(stream);
            Log.Write("cross server CrossBattleGuessingResult");
            Api.CrossGuessingMng.SendGuessingReward(pks.TimingId, pks.UidList);
        }

        //通知跑马灯和邮件
        public void OnResponse_NoticePlayerBattleTeamId(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_NOTICE_PLAYER_TEAM_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_NOTICE_PLAYER_TEAM_ID>(stream);
            Log.Write("cross server NoticePlayerBattleTeamId");
            //更新当前赛季人数
            Api.CrossBattleMng.SyncFinalsPlayerTeamId(pks.Uid, pks.TeanId);
        }

        //聊天喇叭
        public void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_CrossR_CHAT_TRUMPET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CrossR_CHAT_TRUMPET>(stream);
            Log.Write("cross server ChatTrumpet");
            MSG_RZ_CHAT_TRUMPET msg = new MSG_RZ_CHAT_TRUMPET();
            msg.MainId = pks.MainId;
            msg.ItemId = pks.ItemId;
            msg.Words = pks.Words;
            msg.PcInfo = GetRZSpeakerInfo(pks.PcInfo);

            FrontendServer zServer = Api.ZoneManager.GetOneServer();
            zServer.Write(msg);
            //Api.ZoneManager.Broadcast(msg);
        }

        private RZ_SPEAKER_INFO GetRZSpeakerInfo(CR_SPEAKER_INFO msg)
        {
            RZ_SPEAKER_INFO pcInfo = new RZ_SPEAKER_INFO();
            pcInfo.Uid = msg.Uid;
            pcInfo.Name = msg.Name;
            pcInfo.Camp = msg.Camp;
            pcInfo.Level = msg.Level;
            pcInfo.FaceIcon = msg.FaceIcon;
            pcInfo.ShowFaceJpg = msg.ShowFaceJpg;
            pcInfo.FaceFrame = msg.FaceFrame;
            pcInfo.Sex = msg.Sex;
            pcInfo.Title = msg.Title;
            pcInfo.TeamId = msg.TeamId;
            pcInfo.HeroId = msg.HeroId;
            pcInfo.GodType = msg.GodType;
            pcInfo.ChatFrameId = msg.ChatFrameId;
            pcInfo.ArenaLevel = msg.ArenaLevel;
            return pcInfo;
        }
    }
}

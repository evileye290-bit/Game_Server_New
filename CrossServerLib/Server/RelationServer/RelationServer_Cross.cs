using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace CrossServerLib
{
    public partial class RelationServer
    {
        //获取决赛玩家信息
        public void OnResponse_GetFinalsPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_FINALS_PLAYER_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_FINALS_PLAYER_LIST>(stream);
            Log.Write($"player {uid} get GetFinalsPlayerInfo from main {pks.MainId} rank {pks.RankList.Count}");
            int groupId = CrossBattleLibrary.GetGroupId(pks.MainId);
            if (groupId > 0)
            {
                int serverId = CrossBattleLibrary.GetGroupServerId(pks.MainId);
                //将信息添加到缓存中
                foreach (var playerInfo in pks.RankList)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in playerInfo.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = pks.MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                    RelationManager.CrossBattleMng.AddPlayerBaseInfo(playerInfo.Rank.Uid, groupId, rInfo);


                    RelationManager.CrossBattleMng.AddBattleGroupInfo(playerInfo.Rank.Uid, groupId, serverId, pks.MainId, playerInfo.Rank.Rank);
                }
            }
        }

        //返回挑战者信息
        public void OnResponse_ReturnCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>(stream);

            int pcUid = msg.Player2.Uid;
            int mainId = msg.Player2.MainId;
            Log.Write($"player {msg.Player1.Uid} ReturnCrossBattlePlayerInfo player 2 {pcUid} mainId {mainId}");
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = Api.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, pcUid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", pcUid, mainId);
            }
        }

        //决赛对战结果
        public void OnResponse_SetCrossBattleResult(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SET_BATTLE_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SET_BATTLE_RESULT>(stream);
            Log.Write($"player {uid} SetCrossBattleResult player 2 {pks.TimingId} GroupId {pks.GroupId} TeamId {pks.TeamId} FightId {pks.FightId} FileName {pks.FileName}");

            RelationManager.CrossBattleMng.SetCrossBattleResult(pks.TimingId, pks.GroupId, pks.TeamId, pks.FightId, pks.WinUid);

            RelationManager.CrossBattleMng.SetCrossBattleVedio(pks.TimingId, pks.GroupId, pks.TeamId, pks.FightId, pks.FileName);
        }

        //获取决赛信息
        public void OnResponse_ShowCrossBattleFinals(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SHOW_CROSS_BATTLE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SHOW_CROSS_BATTLE_FINALS>(stream);
            Log.Write($"player {uid} ShowCrossBattleFinals MianId{pks.MianId} TeamId {pks.TeamId} ");
            MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO msg = RelationManager.CrossBattleMng.GetFinalsInfoMsg(uid, pks.MianId, pks.TeamId);
            if (msg != null)
            {
                Write(msg, uid);
            }
        }

        //查看决赛阵容信息
        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RC_SHOW_CROSS_BATTLE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_SHOW_CROSS_BATTLE_CHALLENGER>(stream);
            Log.Write($"player {uid} ShowCrossBattleChallenger uid {pks.Uid} MianId{pks.MainId} MainId {MainId} ");
            RelationManager.CrossBattleMng.GetPlayerHeroInfoMsg(pks.Uid, pks.MainId, uid, MainId);
        }

        //更新阵容信息
        public void OnResponse_UpdateCrossBattleHeroInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CROSS_BATTLE_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CROSS_BATTLE_CHALLENGER_HERO_INFO>(stream);
            //Log.Write($"player {uid} UpdateCrossBattleHeroInfo SeeUid {pks.SeeUid} SeeMainId {pks.SeeMainId}  ");

            long battlePower = 0;
            List<CrossHeroInfo> list = new List<CrossHeroInfo>();
            foreach (var item in pks.Heros)
            {
                list.Add(GetPlayerHeroInfoMsg(item));
                battlePower += item.Power;
            }
            RelationManager.CrossBattleMng.UpdateHeroInfo(uid, list);

            RedisPlayerInfo playerInfo = RelationManager.CrossBattleMng.GetRedisPlayerInfo(pks.Uid);
            if (playerInfo != null)
            {
                //修改战力
                playerInfo.SetValue(HFPlayerInfo.BattlePower, battlePower);
                playerInfo.SetValue(HFPlayerInfo.CrossPower, battlePower);

                Api.CrossRedis.Call(new OperateUpdatePlayerPowerInfo(pks.Uid, battlePower));
            }

            if (pks.SeeUid > 0 && pks.SeeMainId > 0)
            {
                RelationManager.CrossBattleMng.GetPlayerHeroInfoMsg(pks.Uid, pks.MainId, pks.SeeUid, pks.SeeMainId);
            }
        }

        public CrossHeroInfo GetPlayerHeroInfoMsg(RCR_Show_HeroInfo baseInfo)
        {
            CrossHeroInfo info = new CrossHeroInfo();
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
                info.Equipment.Add(GetShowEquipmentMsg(item));
            }

            if (baseInfo.HiddenWeapon != null)
            {
                info.HiddenWeapon = new CrossHeroHiddenWeapon()
                {
                    Id = baseInfo.HiddenWeapon.Id,
                    Level = baseInfo.HiddenWeapon.Level,
                    Star = baseInfo.HiddenWeapon.Star,
                };
                info.HiddenWeapon.WashList.AddRange(baseInfo.HiddenWeapon.WashList);
            }

            return info;
        }

        private CrossHeroSoulRing GetShowSoulRingMsg(RCR_Show_SoulRing rInfo)
        {
            CrossHeroSoulRing BaseInfo = new CrossHeroSoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private CrossHeroSoulBone GetShowSoulBoneMsg(RCR_Show_SoulBone rInfo)
        {
            CrossHeroSoulBone BaseInfo = new CrossHeroSoulBone();
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
        private CrossHeroEquipment GetShowEquipmentMsg(RCR_Show_Equipment rInfo)
        {
            CrossHeroEquipment BaseInfo = new CrossHeroEquipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Level = rInfo.Level;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.Score = rInfo.Score;

            BaseInfo.JewelTypeId = rInfo.Slot.JewelTypeId;

            foreach (var item in rInfo.Slot.Injections)
            {
                CrossHeroEquipmentInjection info = new CrossHeroEquipmentInjection();
                info.NatureType = item.NatureType;
                info.NatureValue = item.NatureValue;
                info.InjectionSlot = item.InjectionSlot;
                BaseInfo.Injections.Add(info);
            }
            return BaseInfo;
        }

        public void OnResponse_GetCrossBattleVedio(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_VIDEO>(stream);
            Log.Write($"player {uid} GetCrossBattleVedio  {pks.MainId} uid {uid} TeamId {pks.TeamId} VedioId {pks.VedioId}  ");
            RelationManager.CrossBattleMng.GetCrossBattleVedio(pks.MainId, uid, pks.TeamId, pks.VedioId);
        }

        public void OnResponse_GetCrossBattleStartTime(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_CROSS_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CROSS_BATTLE_START>(stream);
            MSG_CorssR_GET_BATTLE_START msg = new MSG_CorssR_GET_BATTLE_START();
            msg.Time = RelationManager.CrossBattleMng.FirstStartTime;
            Write(msg, uid);
        }

        public void OnResponse_GetGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_GET_GUESSING_INFO>(stream);

            MSG_CorssR_GET_GET_GUESSING_INFO msg = new MSG_CorssR_GET_GET_GUESSING_INFO();
            foreach (var pcUid in pks.Uids)
            {
                RedisPlayerInfo rInfo = RelationManager.CrossBattleMng.GetRedisPlayerInfo(pcUid);
                if (rInfo != null)
                {
                    CorssR_BattlePlayerMsg info = RelationManager.CrossBattleMng.GetPlayerBaseInfoMsg(rInfo, 0);
                    msg.InfoList.Add(info);
                }
            }
            Write(msg, uid);
        }

        public void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CHAT_TRUMPET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CHAT_TRUMPET>(stream);
            Log.Write($"player {uid} ChatTrumpet mainId{pks.MainId} itemId {pks.ItemId} words {pks.Words}");
            RelationManager.CrossBattleMng.SendChatTrumpetInfo(pks.MainId, uid, pks.ItemId, pks.Words, pks.PcInfo);
        }
    }
}

using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using Message.Gate.Protocol.GateZ;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using Google.Protobuf.Collections;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZR;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_GetCrossBossInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_BOSS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_BOSS_INFO>(stream);
            Log.Write($"player {uid} GetCrossBossInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetCrossBossInfo from main {MainId} not find player ");
                return;
            }


            MSG_ZGC_GET_CROSS_BOSS_INFO msg = new MSG_ZGC_GET_CROSS_BOSS_INFO();
            msg.Score = pks.Score;
            msg.Result = pks.Result;
            foreach (var kv in pks.SiteList)
            {
                CrossBossSiteInfo info = GetCrossBossSiteInfoMsg(kv.Value);
                msg.SiteList.Add(kv.Key, info);
            }

            foreach (var kv in pks.SiteDefenseList)
            {
                msg.SiteDefenseList.Add(kv.Key, kv.Value);
            }

            foreach (var kv in pks.CurrentSiteList)
            {
                KeyValuePairIntIntMsg keyMsg = new KeyValuePairIntIntMsg();
                keyMsg.Key = kv.Key;
                keyMsg.Value = kv.Value;
                msg.CurrentSiteList.Add(keyMsg);
            }

            foreach (var kv in pks.Defensers)
            {
                if (kv.Value.BaseInfo.Count > 0)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in kv.Value.BaseInfo)
                    {
                        dataList[(HFPlayerInfo)item.Key] = item.Value;
                    }
                    RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                    PlayerSimpleInfo info = new PlayerSimpleInfo();
                    info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                    info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                    info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                    info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                    info.BattlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);
                    info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                    msg.Defensers.Add(kv.Key, info);
                }
            }
            
            player.Write(msg);
        }

        private static CrossBossSiteInfo GetCrossBossSiteInfoMsg(RZ_CrossBossSiteInfo siteInfo)
        {
            CrossBossSiteInfo info = new CrossBossSiteInfo();
            info.Id = siteInfo.Id;
            info.Hp = (siteInfo.Hp * 0.0001f) / (siteInfo.MaxHp * 0.0001f);
            //info.MaxHp = siteInfo.MaxHp;
            return info;
        }

        public void OnResponse_GetCrossBossPlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_BOSS_PLAYER_INFO>(stream);
            Log.Write($"player {uid} GetCrossBossPlayerInfo from main {MainId} ");

   
            PlayerChar player = Api.PCManager.FindPc(pks.FindPcUid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(pks.FindPcUid);
                if (player == null)
                {
                    LoadBattlePlayerInfoWithQuerys(pks.GetType, pks.FindPcUid, pks, pks.PcUid);
                    return;
                }
            }

            switch ((ChallengeIntoType)pks.GetType)
            {
                case ChallengeIntoType.CrossBoss:
                    SyncCrosHeroQueuMsg(pks, player, (int)ChallengeIntoType.CrossBossReturn);
                    break;
                case ChallengeIntoType.CrossBossSite:
                    SyncCrosHeroQueuMsg(pks, player, (int)ChallengeIntoType.CrossBossSiteReturn);
                    break;
                case ChallengeIntoType.CrossBossSiteFight:
                    SyncCrosHeroQueuMsg(pks, player, (int)ChallengeIntoType.CrossBossSiteFightReturn);
                    break;
                default:
                    break;
            }
        }

        private void SyncCrosHeroQueuMsg(MSG_ZRZ_GET_BOSS_PLAYER_INFO pks, PlayerChar player, int getType)
        {
            //获取到玩家1 信息，返回到Relation
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
            addMsg.GetType = getType; // (int)ChallengeIntoType.CrossBossReturn;
            addMsg.Player1 = player.GetBossPlayerInfoMsg();
            addMsg.PcUid = pks.PcUid; ;
            addMsg.PcMainId = pks.PcMainId; ;
            addMsg.Hp = pks.Hp;
            addMsg.MaxHp = pks.MaxHp;
            addMsg.DungeonId = pks.DungeonId;
            addMsg.Addbuff = pks.Addbuff;
            Write(addMsg, addMsg.PcUid);
        }


        public void OnResponse_ReturnCrossBossPlayerInfoFromCross(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>(stream);
            Log.Write($"player {uid} ReturnCrossBossPlayerInfoFromCross from main {MainId} ");
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player != null)
            {
                switch ((ChallengeIntoType)pks.GetType)
                {
                    case ChallengeIntoType.CrossBossReturn:
                        {
                            PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(pks.Player1);
                            if (fightInfo == null)
                            {
                                fightInfo = new PlayerCrossFightInfo();
                            }
                            fightInfo.Hp = pks.Hp;
                            fightInfo.MaxHp = pks.MaxHp;
                            fightInfo.DungeonId = pks.DungeonId;
                            fightInfo.AddBuff = pks.Addbuff;
                            player.EnterCrossBossMap(fightInfo);
                        }
                        break;
                    case ChallengeIntoType.CrossBossSiteFightReturn:
                        {
                            if (pks.Player1 != null)
                            {
                                PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(pks.Player1);
                                if (fightInfo != null)
                                {
                                    fightInfo.DungeonId = pks.DungeonId;
                                    player.EnterCrossBossDefenseMap(fightInfo);
                                }
                            }
                        }
                        break;
                    case ChallengeIntoType.CrossBossSiteReturn:
                        {
                            MSG_ZGC_CROSS_BOSS_CHALLENGER_INFO msg = new MSG_ZGC_CROSS_BOSS_CHALLENGER_INFO();
                            if (pks.Player1 == null)
                            {
                                msg.Result = (int)ErrorCode.NotFindChallengerInfo;
                                msg.IsEnd = true;
                                player.Write(msg);
                            }
                            else
                            {

                                Dictionary<int, int> dic = new Dictionary<int, int>();
                                Queue<ZGC_Show_HeroInfo> queue = new Queue<ZGC_Show_HeroInfo>();
                                int power = 0;
                                foreach (var item in pks.Player1.Heros)
                                {
                                    ZGC_Show_HeroInfo info = GetPlayerHeroInfoMsg(item);
                                    queue.Enqueue(info);

                                    dic.TryGetValue(info.QueueNum, out power);
                                    dic[info.QueueNum] = power + info.Power;
                                }


                                PLAYER_BASE_INFO rankInfo = new PLAYER_BASE_INFO();
                                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                                foreach (var item in pks.Player1.BaseInfo)
                                {
                                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                                }
                                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                                rankInfo.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                                rankInfo.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                                rankInfo.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                                rankInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                                rankInfo.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
                                rankInfo.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                                rankInfo.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);


                                msg.Info = rankInfo;
                                msg.Uid = pks.Player1.Uid;
                                msg.MainId = pks.Player1.MainId;
                                msg.Result = (int)ErrorCode.Success;
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

                                        msg = new MSG_ZGC_CROSS_BOSS_CHALLENGER_INFO();
                                        msg.Info = rankInfo;
                                        msg.Uid = pks.Player1.Uid;
                                        msg.MainId = pks.Player1.MainId;
                                        msg.Result = (int)ErrorCode.Success;
                                        foreach (var item in dic)
                                        {
                                            msg.BattlePowers[item.Key] = item.Value;
                                        }
                                    }
                                }
                                msg.IsEnd = true;
                                player.Write(msg);
                            }
                           
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Log.Warn($"player {uid} ReturnCrossBossPlayerInfoFromCross not find client ");
            }
        }

        public ZGC_Show_HeroInfo GetPlayerHeroInfoMsg(ZR_Show_HeroInfo baseInfo)
        {
            ZGC_Show_HeroInfo info = new ZGC_Show_HeroInfo();
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
                ZGC_Show_Equipment weapon = new ZGC_Show_Equipment()
                {
                    PartType = 5, 
                    Id = baseInfo.HiddenWeapon.Id,
                    Level = baseInfo.HiddenWeapon.Level,
                    Score = HiddenWeaponItem.HiddenWeaponScore(baseInfo.HiddenWeapon.Id, baseInfo.HiddenWeapon.Level, baseInfo.HiddenWeapon.WashList.ToList(), baseInfo.HiddenWeapon.Star),
                };
                weapon.HiddenWeapon = new ZGC_HIDDEN_WEAPON_INFO()
                {
                    Level = baseInfo.HiddenWeapon.Level,
                    Star = baseInfo.HiddenWeapon.Star,
                    WashList = { baseInfo.HiddenWeapon.WashList }
                };
                info.Equipments.Add(weapon);
            }

            return info;
        }

        private ZGC_Show_SoulRing GetShowSoulRingMsg(ZR_Show_SoulRing rInfo)
        {
            ZGC_Show_SoulRing BaseInfo = new ZGC_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private ZGC_Show_SoulBone GetShowSoulBoneMsg(ZR_Show_SoulBone rInfo)
        {
            ZGC_Show_SoulBone BaseInfo = new ZGC_Show_SoulBone();
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
        private ZGC_Show_Equipment GetShowEquipmentMsg(ZR_Show_Equipment rInfo)
        {
            ZGC_Show_Equipment BaseInfo = new ZGC_Show_Equipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Level = rInfo.Level;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.Score = rInfo.Score;

            BaseInfo.Slot = GetShowEquipmentSlotMsg(rInfo.Slot);

            return BaseInfo;
        }

        private ZGC_Show_Equipment_Slot GetShowEquipmentSlotMsg(ZR_Show_Equipment_Slot rInfo)
        {
            ZGC_Show_Equipment_Slot BaseInfo = new ZGC_Show_Equipment_Slot();
            BaseInfo.JewelTypeId = rInfo.JewelTypeId;

            foreach (var item in rInfo.Injections)
            {
                ZGC_Show_Equipment_Injection info = new ZGC_Show_Equipment_Injection();
                info.NatureType = item.NatureType;
                info.NatureValue = item.NatureValue;
                info.InjectionSlot = item.InjectionSlot;
                BaseInfo.Injections.Add(info);
            }
            return BaseInfo;
        }

        public void OnResponse_StopCrossBossDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_STOP_CROSS_BOSS_DUNGEON pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_STOP_CROSS_BOSS_DUNGEON>(stream);
            Log.Write($"player {uid} StopCrossBossDungeon from main {MainId} ");

            foreach (var pc in Api.PCManager.PcList)
            {
                if (pc.Value.InDungeon && pc.Value.Uid != pks.Uid)
                {
                    DungeonMap dungeon = pc.Value.CurrentMap as DungeonMap;
                    if (dungeon != null && dungeon.DungeonModel.Id == pks.DungeonId)
                    {
                        dungeon.Stop(DungeonResult.Tie);
                    }
                }
            }

            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                if (pc.Value.InDungeon && pc.Value.Uid != pks.Uid)
                {
                    DungeonMap dungeon = pc.Value.CurrentMap as DungeonMap;
                    if (dungeon != null && dungeon.DungeonModel.Id == pks.DungeonId)
                    {
                        dungeon.Stop(DungeonResult.Tie);
                    }
                }
            }
        }

        public void OnResponse_SendCrossBossPassReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_BOSS_PASS_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_BOSS_PASS_REWARD>(stream);
            Log.Write($"player {uid} SendCrossBossPassRewar from main {MainId} ");

            foreach (var pc in Api.PCManager.PcList)
            {
                pc.Value.CrossBossInfoMng.SetPassRewardState(pks.DungeonId);
                pc.Value.SyncCrossBattleManagerMessage();
            }

            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                pc.Value.CrossBossInfoMng.SetPassRewardState(pks.DungeonId);
            }
        }

        public void OnResponse_SendCrossBossRankReward(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_BOSS_RANK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_BOSS_RANK_REWARD>(stream);
            Log.Write($"player {uid} SendCrossBossRankReward from main {MainId} ");

            foreach (var pc in Api.PCManager.PcList)
            {
                pc.Value.CrossBossInfoMng.SetScoreState(pks.DungeonId);
            }

            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                pc.Value.CrossBossInfoMng.SetScoreState(pks.DungeonId);
            }
        }
    }
}

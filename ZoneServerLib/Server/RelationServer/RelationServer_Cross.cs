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
        private void OnResponse_GetCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_BATTLE_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_BATTLE_PLAYER>(stream);
            Log.Warn($"player {msg.Player1.Uid} GetCrossBattlePlayerInfo challenge {msg.Player2.MainId} player {msg.Player2.Uid}  ");
            LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.Player1.Uid, msg, uid);
        }

        //返回的玩家信息
        private void OnResponse_ReturnCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>(stream);
            Log.Warn($"player {msg.Player1.Uid} ReturnCrossBattlePlayerInfo challenge {msg.Player2.MainId} player {msg.Player2.Uid}  ");

            if (msg.GetType == (int)ChallengeIntoType.CrossFinalsRobot)
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
                        info.CrossQueueNum = hero.QueueNum;
                        info.CrossPositionNum = hero.PositionNum;

                        foreach (var item in hero.Natures.List)
                        {
                            info.Nature.AddNatureBaseValue((NatureType)item.NatureType, item.Value);
                        }

                        findPlayer.HeroMng.BindHeroInfo(info);
                        findPlayer.HeroMng.BindHeroQueueList(info);

                        //foreach (var item in hero.SoulRings)
                        //{
                        //    SoulRingInfo soulRingItem = new SoulRingInfo();
                        //    soulRingItem.EquipHeroId = hero.Id;
                        //    soulRingItem.Year = item.Year;
                        //    soulRingItem.TypeId = item.SpecId;
                        //    soulRingItem.Position = item.Pos;
                        //    soulRingItem.Level = item.Level;

                        //    SoulRingItem soulRing = new SoulRingItem(soulRingItem);
                        //    findPlayer.SoulRingManager.AddEquipSoulRing(soulRing);
                        //}
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
                    fightInfo.Type = ChallengeIntoType.CrossFinals;
                    fightInfo.TimingId = msg.TimingId;
                    fightInfo.GroupId = msg.GroupId;
                    fightInfo.TeamId = msg.TeamId;
                    fightInfo.FightId = msg.FightId;
                    fightInfo.HeroIndex[msg.Player1.Uid] = msg.Player1.Index;
                    fightInfo.HeroIndex[msg.Player2.Uid] = msg.Player2.Index;
                    findPlayer.EnterCrossBattleMap(fightInfo);
                }
            }
            else
            {
                LoadBattlePlayerInfoWithQuerys(msg.GetType, msg.Player2.Uid, msg, uid);
            }
        }

        private void OnResponse_GetCrossBattleChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO>(stream);

            Log.Warn($"player {uid} ReturnCrossBattlePlayerInfo result {msg.Result} type {msg.GetType}  ");

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
                    fightInfo.Type = ChallengeIntoType.CrossPreliminary;
                    player.EnterCrossBattleMap(fightInfo);
                }
            }
        }

        private PlayerCrossFightInfo GetCrossRobotInfo(ZR_BattlePlayerMsg info)
        {
            PlayerCrossFightInfo rankInfo = null;
            //机器人，直接读表
            if (info != null)
            {
                rankInfo = new PlayerCrossFightInfo();
                rankInfo.Uid = info.Uid;
                rankInfo.MainId = info.MainId;

                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in info.BaseInfo)
                {
                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                }
                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
                rankInfo.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                rankInfo.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                rankInfo.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
                rankInfo.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
                rankInfo.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
                rankInfo.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
                rankInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                rankInfo.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
                rankInfo.BattlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);
                rankInfo.CrossLevel = rInfo.GetIntValue(HFPlayerInfo.CrossLevel);
                rankInfo.CrossStar = rInfo.GetIntValue(HFPlayerInfo.CrossScore);
                rankInfo.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);


                foreach (var hero in info.Heros)
                {
                    RobotHeroInfo robotHero = new RobotHeroInfo();
                    robotHero.HeroId = hero.Id;
                    robotHero.Id = hero.Id;
                    robotHero.Level = hero.Level;
                    robotHero.BattlePower = hero.Power;
                    robotHero.StepsLevel = hero.StepsLevel;
                    robotHero.GodType = hero.GodType;
                    robotHero.SoulSkillLevel = hero.SoulSkillLevel;

                    List<string> soulBoneStr = hero.SoulBones.ToList().ConvertAll(x => x.SpecIds.Count <= 0 ? x.Id.ToString() : x.Id.ToString() + ":" + string.Join(":", x.SpecIds));
                    robotHero.SoulBones = string.Join("|", soulBoneStr);

                    foreach (var curr in hero.SoulRings)
                    {
                        robotHero.SoulRings += $"{curr.Pos}:{1}:{curr.SpecId}:{curr.Year}:{curr.Element}|";
                    }

                    foreach (var nature in hero.Natures.List)
                    {
                        robotHero.NatureList[(NatureType)nature.NatureType] = nature.Value;
                    }

                    if (hero.HiddenWeapon != null)
                    {
                        robotHero.HiddenWeapon = $"{hero.HiddenWeapon.Id}:{hero.HiddenWeapon.Star}";
                    }

                    if (hero.Equipments?.Count > 0)
                    {
                        robotHero.Equipment = string.Join("|", hero.Equipments.Select(x => x.Id));
                    }

                    rankInfo.AddHero(robotHero, hero.PositionNum, hero.QueueNum);
                }
                rankInfo.NatureValues = new Dictionary<int, int>(info.NatureValues);
                rankInfo.NatureRatios = new Dictionary<int, int>(info.NatureRatios);

            }
            return rankInfo;
        }

        private void OnResponse_ShowCrossBattleFinals(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_BATTLE_FINALS_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_BATTLE_FINALS_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get cross finals info from relation failed: not find player ", uid);
                return;
            }

            MSG_ZGC_SHOW_CROSS_BATTLE_FINALS_INFO msg = new MSG_ZGC_SHOW_CROSS_BATTLE_FINALS_INFO();
            msg.TeamId = pks.TeamId;

            foreach (var item in pks.List)
            {
                CROSS_BATTLE_FINALS_PLAYER_INFO itemMsg = new CROSS_BATTLE_FINALS_PLAYER_INFO();
                itemMsg.Index = item.Index;
                if (item.Uid > 0)
                {
                    RedisPlayerInfo rInfo = GetBaseInfoMsg(item.BaseInfo);
                    rInfo.SetValue(HFPlayerInfo.BattlePower64, rInfo.GetlongValue(HFPlayerInfo.BattlePower));
                    itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);
                }
                else
                {
                    if (pks.TeamId > 0)
                    {
                        //机器人
                        CrossFinalsRobotInfo rInfo = RobotLibrary.GetCrossFinalsRobotInfo(pks.TeamId, item.Index);
                        if (rInfo != null)
                        {
                            itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);
                        }
                    }
                    else
                    {
                        //总决赛机器人
                        CrossFinalsRobotInfo rInfo = RobotLibrary.GetCrossFinalsRobotInfo(item.Index, item.OldTeam);
                        if (rInfo != null)
                        {
                            itemMsg.BaseInfo = GetBaseInfoMsg(rInfo);
                        }
                    }
                }
                msg.List.Add(itemMsg);
            }

            msg.Fight1.AddRange(pks.Fight1);
            msg.Fight2.AddRange(pks.Fight2);
            msg.Fight3.AddRange(pks.Fight3);
            player.Write(msg);
        }

        private RedisPlayerInfo GetBaseInfoMsg(RepeatedField<HFPlayerBaseInfoItem> baseInfoList)
        {
            Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
            if (baseInfoList != null)
            {
                foreach (var baseInfo in baseInfoList)
                {
                    dataList[(HFPlayerInfo)baseInfo.Key] = baseInfo.Value;
                }
            }
            RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);
            return rInfo;
        }

        private PLAYER_BASE_INFO GetBaseInfoMsg(RedisPlayerInfo rInfo)
        {
            PLAYER_BASE_INFO BaseInfo = new PLAYER_BASE_INFO();
            BaseInfo.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
            BaseInfo.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
            BaseInfo.Sex = rInfo.GetIntValue(HFPlayerInfo.Sex);
            BaseInfo.Icon = rInfo.GetIntValue(HFPlayerInfo.Icon);
            BaseInfo.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
            BaseInfo.IconFrame = rInfo.GetIntValue(HFPlayerInfo.IconFrame);
            BaseInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
            BaseInfo.Level = rInfo.GetIntValue(HFPlayerInfo.Level);
            BaseInfo.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
            BaseInfo.BattlePower64 = rInfo.GetlongValue(HFPlayerInfo.BattlePower64);
            int battlePower = rInfo.GetIntValue(HFPlayerInfo.BattlePower);
            if (battlePower > 0)
            {
                BaseInfo.BattlePower = battlePower;
            }
            else
            {
                if (BaseInfo.BattlePower64 < int.MaxValue)
                {
                    BaseInfo.BattlePower = (int)BaseInfo.BattlePower64;
                }
            }

            return BaseInfo;
        }

        private PLAYER_BASE_INFO GetBaseInfoMsg(CrossFinalsRobotInfo rInfo)
        {
            PLAYER_BASE_INFO BaseInfo = new PLAYER_BASE_INFO();
            //BaseInfo.Uid = rInfo./*GetIntValue(HFPlayerInfo.Uid)*/;
            BaseInfo.Name = rInfo.Name;
            BaseInfo.Icon = rInfo.Icon;
            //BaseInfo.ShowDIYIcon = rInfo.GetBoolValue(HFPlayerInfo.ShowDIYIcon);
            BaseInfo.IconFrame = rInfo.IconFrame;
            //BaseInfo.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
            BaseInfo.Level = rInfo.Level;
            //BaseInfo.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
            BaseInfo.BattlePower = rInfo.BattlePower;
            BaseInfo.BattlePower64 = rInfo.BattlePower;
            return BaseInfo;
        }

        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_BATTLE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_BATTLE_CHALLENGER>(stream);

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

            MSG_ZGC_CROSS_BATTLE_CHALLENGER_INFO msg = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_INFO();
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

                    msg = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_INFO();
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

        public ZGC_Show_HeroInfo GetPlayerHeroInfoMsg(RZ_Show_HeroInfo baseInfo)
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
                };
                weapon.HiddenWeapon.WashList.Add(baseInfo.HiddenWeapon.WashList);

                info.Equipments.Add(weapon);
            }

            return info;
        }

        private ZGC_Show_SoulRing GetShowSoulRingMsg(RZ_Show_SoulRing rInfo)
        {
            ZGC_Show_SoulRing BaseInfo = new ZGC_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private ZGC_Show_SoulBone GetShowSoulBoneMsg(RZ_Show_SoulBone rInfo)
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
        private ZGC_Show_Equipment GetShowEquipmentMsg(RZ_Show_Equipment rInfo)
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

        private ZGC_Show_Equipment_Slot GetShowEquipmentSlotMsg(RZ_Show_Equipment_Slot rInfo)
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

        public void OnResponse_GetCrossBattleHeros(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_BATTLE_HEROS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_BATTLE_HEROS>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player == null)
                {
                    LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.CrossHeroInfo, pks.Uid, pks, uid);
                }
                else
                {
                    player.SyncCrossHeroQueueMsg(pks.SeeUid, pks.SeeMainId);
                }
            }
            else
            {
                player.SyncCrossHeroQueueMsg(pks.SeeUid, pks.SeeMainId);
            }
        }


        private void OnResponse_ShowCrossRankInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_RANK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_RANK_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get cross rank info from relation failed: not find player ", msg.PcUid);
                return;
            }
            player.ShowCrossRankInfosMsg(msg);
        }

        private void OnResponse_ShowCrossLeaderInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SHOW_CROSS_LEADER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SHOW_CROSS_LEADER_INFO>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get cross leader info from relation failed: not find player ", msg.PcUid);
                return;
            }
            player.ShowCrossLeaderInfosMsg(msg);
        }

        private void OnResponse_UpdateCrossRank(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_CROSS_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_CROSS_RANK>(stream);
            //找到个人并由个人发送
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.UpdateCrossSeasonRank(msg.Rank);
                player.SendCrossBattleManagerMessage();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    player.UpdateCrossSeasonRank(msg.Rank);
                }
                //Log.Warn("player {0} get cross leader info from relation failed: not find player ", msg.PcUid);
            }
        }

        public void OnResponse_GetCrossBattleVedio(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_CROSS_VIDEO>(stream);
            MSG_ZGC_GET_CROSS_VIDEO request = new MSG_ZGC_GET_CROSS_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.VideoName = pks.VideoName;
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get cross battle vedio info from relation failed: not find player ", uid);
                return;
            }
            player.Write(request);
        }

        public void OnResponse_ClearCrossFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CLEAR_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CLEAR_PLAYER_FINAL>(stream);
            Log.Write("cross server ClearCrossFinalsPlayerRank");
            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.CrossInfoMng.ChangeLastFinalsRank(0);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.CrossInfoMng.ChangeLastFinalsRank(0);
            }
        }

        public void OnResponse_UpdateCrossFinalsPlayerRank(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_UPDATE_PLAYER_FINAL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_PLAYER_FINAL>(stream);
            Log.Write("cross server UpdateCrossFinalsPlayerRank");

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
                player.CrossInfoMng.ChangeLastFinalsRank(kv.Value);
            }
        }

        public void OnResponse_ClearCrossBattleRanks(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CLEAR_BATTLE_RANK pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CLEAR_BATTLE_RANK>(stream);
            Log.Write("cross server ClearCrossBattleRank");
            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.RefreshCrossRank(true);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.RefreshCrossRank(false);
            }
        }

        public void OnResponse_CrossBattleStart(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_BATTLE_START>(stream);
            Log.Write("cross server CrossBattleStart");
            //清空所有人决战排名
            Api.CrossBattleMng.FirstStartTime = pks.Time;
            Api.CrossBattleMng.TeamId = pks.TeamId;
            Api.CrossBattleMng.StartTime = Timestamp.TimeStampToDateTime(pks.Time);
        }

        public void OnResponse_GetCrossBattleStart(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_BATTLE_START>(stream);
            Log.Write("cross server GetCrossBattleStart");
            //清空所有人决战排名
            Api.CrossBattleMng.FirstStartTime = pks.Time;
            Api.CrossBattleMng.TeamId = pks.TeamId;
            Api.CrossBattleMng.StartTime = Timestamp.TimeStampToDateTime(pks.Time);
            if (uid > 0)
            {
                PlayerChar player = Api.PCManager.FindPc(uid);
                if (player != null)
                {
                    player.SyncCrossBattleManagerMessage();
                }
            }
        }

        public void OnResponse_CrossBattleServerReward(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CROSS_BATTLE_SERVER_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_BATTLE_SERVER_REWARD>(stream);
            Log.Write("cross server CrossBattleServerReward");
            int state = (int)CrossRewardState.None;
            MSG_ZGC_NEW_CROSS_BATTLE_SERVER_REWARD msg = new MSG_ZGC_NEW_CROSS_BATTLE_SERVER_REWARD();
            msg.ServerReward = state;

            //清空所有人决战排名
            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.CrossInfoMng.Info.ServerReward = state;
                player.Value.Write(msg);
            }
            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.CrossInfoMng.Info.ServerReward = state;
            }
        }

        public void OnResponse_GetGuessingPlayersInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_GET_GUESSING_INFO>(stream);
            Log.Write("cross server GetGuessingPlayersInfo");
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

                MSG_ZGC_GET_GUESSING_INFO msg = new MSG_ZGC_GET_GUESSING_INFO();
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
                    rInfo.SetValue(HFPlayerInfo.BattlePower64, rInfo.GetlongValue(HFPlayerInfo.BattlePower));
                    itemMsg.Player1.BaseInfo = GetBaseInfoMsg(rInfo);
                    if (!dic.TryGetValue(guessingInfo.Player2, out rInfo))
                    {
                        continue;
                    }
                    itemMsg.Player2 = new CROSS_CHALLENGE_FINALS_PLAYER_INFO();
                    itemMsg.Player2.Index = 2;
                    rInfo.SetValue(HFPlayerInfo.BattlePower64, rInfo.GetlongValue(HFPlayerInfo.BattlePower));
                    itemMsg.Player2.BaseInfo = GetBaseInfoMsg(rInfo);

                    itemMsg.TimingId = guessingInfo.TimingId;
                    itemMsg.Choose = guessingInfo.Choose;
                    itemMsg.Player1Choose = guessingInfo.Player1Choose;
                    itemMsg.Player2Choose = guessingInfo.Player2Choose;
                    itemMsg.Winner = guessingInfo.Winner;
                    msg.List.Add(itemMsg);
                }

                CrossBattleTiming endGuessing = CrossBattleLibrary.GetGuessingTime(Api.CrossBattleMng.StartTime, Api.Now());
                CrossBattleTiming timing = CrossBattleLibrary.GetCrossBattleTiming(endGuessing);
                msg.CurrentTimingId = (int)timing;
                player.Write(msg);
            }
        }

        public void OnResponse_CrossGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CROSS_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CROSS_GUESSING_CHOOSE>(stream);
            Log.Write("cross server CrossGuessingChoose");
            //清空所有人决战排名
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CrossGuessingChoose(pks.Result, pks.TimingId, pks.Choose, pks.HasReward);
            }
        }

        public void OnResponse_CrossGuessingTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_OPEN_GUESSING_TEAM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_OPEN_GUESSING_TEAM>(stream);
            Log.Write("cross server GetCrossBattleStart");
            //清空所有人决战排名
            Api.CrossBattleMng.TeamId = pks.TeamId;
        }

        public void OnResponse_UpdateCrossBattleTeamId(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NOTICE_PLAYER_TEAM_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NOTICE_PLAYER_TEAM_ID>(stream);
            Log.Write("cross server UpdateCrossBattleTeamId");
            //清空所有人决战排名
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpdateCrossBattleTeamId(pks.TeanId);
            }
        }
    }
}

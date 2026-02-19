using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using System.IO;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_ReturnCrossBattlePlayerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>(stream);

            //MSG_RCR_RETURN_BATTLE_PLAYER_INFO msg = new MSG_RCR_RETURN_BATTLE_PLAYER_INFO();
            //msg.GetType = pks.GetType;
            //msg.Player1 = GetBattlePlayerInfoMsg(pks.Player1);
            //msg.Player2 = GetBattlePlayerInfoMsg(pks.Player2);
            //msg.TimingId = pks.TimingId;
            //msg.GroupId = pks.GroupId;
            //msg.TeamId = pks.TeamId;
            //msg.FightId = pks.FightId;
            Api.WriteToCross(pks);
        }

        public void OnResponse_GetCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO>(stream);
            Client client = ZoneManager.GetClient(uid);
            if (client != null)
            {
                ZR_BattlePlayerMsg infoMsg = Api.CrossBattleMng.GetPlayerInfoMsg(pks.ChallengerUid);
                if (infoMsg != null)
                {
                    pks.Challenger = infoMsg;
                    pks.Result = (int)ErrorCode.Success;
                }
                else
                {
                    pks.Result = (int)ErrorCode.NotFindChallengerInfo;
                }
                client.Write(pks);
            }
            else
            {
                Log.Warn($"player {uid} get client cross challenger failed : not find player");
            }
        }

        public void OnResponse_AddCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_BATTLE_CHALLENGER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_BATTLE_CHALLENGER_INFO>(stream);
            Api.CrossBattleMng.AddPlayerInfoMsg(pks.Info1);
            Api.CrossBattleMng.AddPlayerInfoMsg(pks.Info2);
        }

        //public RZ_BattlePlayerMsg GetBattleChllengerInfoMsg(ZR_BattlePlayerMsg info)
        //{
        //    RZ_BattlePlayerMsg response = new RZ_BattlePlayerMsg();
        //    response.Uid = info.Uid;
        //    response.MainId = info.MainId;
        //    //基本信息
        //    foreach (var item in info.BaseInfo)
        //    {
        //        HFPlayerBaseInfoItem baseInfo = new HFPlayerBaseInfoItem();
        //        baseInfo.Key = item.Key;
        //        baseInfo.Value = item.Value;
        //        response.BaseInfo.Add(baseInfo);
        //    }

        //    foreach (var item in info.BaseInfo)
        //    {
        //        HFPlayerBaseInfoItem baseInfo = new HFPlayerBaseInfoItem();
        //        baseInfo.Key = item.Key;
        //        baseInfo.Value = item.Value;
        //        response.BaseInfo.Add(baseInfo);
        //    }

        //    //伙伴信息
        //    foreach (var item in info.Heros)
        //    {
        //        RZ_HeroInfo rCInfo = new RZ_HeroInfo();
        //        rCInfo.Id = item.Id;
        //        rCInfo.Level = item.Level;
        //        rCInfo.StepsLevel = item.StepsLevel;
        //        rCInfo.SoulSkillLevel = item.SoulSkillLevel;
        //        rCInfo.GodType = item.GodType;
        //        rCInfo.CrossQueueNum = item.CrossQueueNum;
        //        rCInfo.CrossPositionNum = item.CrossPositionNum;
        //        rCInfo.Power = item.Power;

        //        //魂环
        //        foreach (var ring in item.SoulRings)
        //        {
        //            RZ_Hero_SoulRing ringMsg = new RZ_Hero_SoulRing();
        //            ringMsg.SpecId = ring.SpecId;
        //            ringMsg.Level = ring.Level;
        //            ringMsg.Pos = ring.Pos;
        //            ringMsg.Year = ring.Year;
        //            rCInfo.SoulRings.Add(ringMsg);
        //        }
        //        //魂骨
        //        foreach (var bone in item.SoulBones)
        //        {
        //            RZ_Hero_SoulBone boneMsg = new RZ_Hero_SoulBone();
        //            boneMsg.Id = bone.Id;
        //            rCInfo.SoulBones.Add(boneMsg);
        //        }
        //        //属性
        //        if (item.Natures != null)
        //        {
        //            rCInfo.Natures = GetRZNature(item.Natures);

        //        }
        //        response.Heros.Add(rCInfo);
        //    }
        //    return response;
        //}
        //public RZ_Hero_Nature GetRZNature(ZR_Hero_Nature nature)
        //{
        //    RZ_Hero_Nature heroNature = new RZ_Hero_Nature();
        //    foreach (var item in nature.List)
        //    {
        //        RZ_Hero_Nature_Item info = new RZ_Hero_Nature_Item();
        //        info.NatureType = item.NatureType;
        //        info.Value = item.Value;
        //        heroNature.List.Add(info);
        //    }
        //    return heroNature;
        //}

        //public RC_BattlePlayerMsg GetBattlePlayerInfoMsg(ZR_BattlePlayerMsg info)
        //{
        //    RC_BattlePlayerMsg response = new RC_BattlePlayerMsg();
        //    response.Uid = info.Uid;
        //    response.MainId = info.MainId;
        //    response.Index = info.Index;
        //    //基本信息
        //    foreach (var item in info.BaseInfo)
        //    {
        //        RC_HFPlayerBaseInfoItem baseInfo = new RC_HFPlayerBaseInfoItem();
        //        baseInfo.Key = item.Key;
        //        baseInfo.Value = item.Value;
        //        response.BaseInfo.Add(baseInfo);
        //    }

        //    foreach (var item in info.BaseInfo)
        //    {
        //        RC_HFPlayerBaseInfoItem baseInfo = new RC_HFPlayerBaseInfoItem();
        //        baseInfo.Key = item.Key;
        //        baseInfo.Value = item.Value;
        //        response.BaseInfo.Add(baseInfo);
        //    }

        //    //伙伴信息
        //    foreach (var item in info.Heros)
        //    {
        //        RC_Hero_Info rCInfo = new RC_Hero_Info();
        //        rCInfo.Id = item.Id;
        //        rCInfo.Level = item.Level;
        //        rCInfo.StepsLevel = item.StepsLevel;
        //        rCInfo.SoulSkillLevel = item.SoulSkillLevel;
        //        rCInfo.GodType = item.GodType;
        //        rCInfo.CrossQueueNum = item.QueueNum;
        //        rCInfo.CrossPositionNum = item.PositionNum;
        //        rCInfo.Power = item.Power;

        //        //魂环
        //        foreach (var ring in item.SoulRings)
        //        {
        //            RC_Hero_SoulRing ringMsg = new RC_Hero_SoulRing();
        //            ringMsg.SpecId = ring.SpecId;
        //            ringMsg.Level = ring.Level;
        //            ringMsg.Pos = ring.Pos;
        //            ringMsg.Year = ring.Year;
        //            rCInfo.SoulRings.Add(ringMsg);
        //        }
        //        //魂骨
        //        foreach (var bone in item.SoulBones)
        //        {
        //            RC_Hero_SoulBone boneMsg = new RC_Hero_SoulBone();
        //            boneMsg.Id = bone.Id;
        //            rCInfo.SoulBones.Add(boneMsg);
        //        }
        //        //属性
        //        if (item.Natures != null)
        //        {
        //            rCInfo.Natures = GetNature(item.Natures);

        //        }
        //        response.Heros.Add(rCInfo);
        //    }
        //    return response;
        //}

        //public RC_Hero_Nature GetNature(ZR_Hero_Nature nature)
        //{
        //    RC_Hero_Nature heroNature = new RC_Hero_Nature();
        //    foreach (var item in nature.List)
        //    {
        //        RC_Hero_Nature_Item info = new RC_Hero_Nature_Item();
        //        info.NatureType = item.NatureType;
        //        info.Value = item.Value;
        //        heroNature.List.Add(info);
        //    }
        //    return heroNature;
        //}

        public void OnResponse_SetCrossBattleResult(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SET_BATTLE_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SET_BATTLE_RESULT>(stream);

            MSG_RC_SET_BATTLE_RESULT msg = new MSG_RC_SET_BATTLE_RESULT();
            msg.TimingId = pks.TimingId;
            msg.GroupId = pks.GroupId;
            msg.TeamId = pks.TeamId;
            msg.FightId = pks.FightId;
            msg.WinUid = pks.WinUid;
            msg.FileName = pks.FileName;
            Api.CrossServer.Write(msg);
        }

        public void OnResponse_ShowCrossBattleFinals(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SHOW_CROSS_BATTLE_FINALS pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_CROSS_BATTLE_FINALS>(stream);
            Log.Write($"player {uid} get cross battle finals info by team {pks.TeamId}.");
            //找到玩家说明玩家在线 ，通知玩家发送信息回来
            MSG_RC_SHOW_CROSS_BATTLE_FINALS msg = new MSG_RC_SHOW_CROSS_BATTLE_FINALS();
            msg.TeamId = pks.TeamId;
            msg.MianId = Api.MainId;
            Api.CrossServer.Write(msg, uid);
        }

        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SHOW_CROSS_BATTLE_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_CROSS_BATTLE_CHALLENGER>(stream);
            MSG_RC_SHOW_CROSS_BATTLE_CHALLENGER request = new MSG_RC_SHOW_CROSS_BATTLE_CHALLENGER();
            request.Uid = pks.Uid;
            request.MainId = pks.MainId;
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_UpdateCrossBattleHeroInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_BATTLE_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_BATTLE_CHALLENGER_HERO_INFO>(stream);
            MSG_RC_CROSS_BATTLE_CHALLENGER_HERO_INFO request = new MSG_RC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            request.Uid = pks.Uid;
            request.MainId = pks.MainId;
            request.SeeUid = pks.SeeUid;
            request.SeeMainId = pks.SeeMainId;
            foreach (var item in pks.Heros)
            {
                request.Heros.Add(GetPlayerHeroInfoMsg(item));
            }
            Api.CrossServer.Write(request, uid);
        }

        public RCR_Show_HeroInfo GetPlayerHeroInfoMsg(ZR_Show_HeroInfo baseInfo)
        {
            RCR_Show_HeroInfo info = new RCR_Show_HeroInfo();
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
                info.HiddenWeapon = new RCR_Show_HiddenWeapon()
                {
                    Id = baseInfo.HiddenWeapon.Id,
                    Level = baseInfo.HiddenWeapon.Level,
                    Star = baseInfo.HiddenWeapon.Star,
                    WashList = { baseInfo.HiddenWeapon.WashList }
                };
            }

            return info;
        }

        private RCR_Show_SoulRing GetShowSoulRingMsg(ZR_Show_SoulRing rInfo)
        {
            RCR_Show_SoulRing BaseInfo = new RCR_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.SpecId = rInfo.SpecId;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private RCR_Show_SoulBone GetShowSoulBoneMsg(ZR_Show_SoulBone rInfo)
        {
            RCR_Show_SoulBone BaseInfo = new RCR_Show_SoulBone();
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
        private RCR_Show_Equipment GetShowEquipmentMsg(ZR_Show_Equipment rInfo)
        {
            RCR_Show_Equipment BaseInfo = new RCR_Show_Equipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Level = rInfo.Level;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.Score = rInfo.Score;

            BaseInfo.Slot = GetShowEquipmentSlotMsg(rInfo.Slot);
            return BaseInfo;
        }

        private RCR_Show_Equipment_Slot GetShowEquipmentSlotMsg(ZR_Show_Equipment_Slot rInfo)
        {
            RCR_Show_Equipment_Slot BaseInfo = new RCR_Show_Equipment_Slot();
            BaseInfo.JewelTypeId = rInfo.JewelTypeId;

            foreach (var item in rInfo.Injections)
            {
                RCR_Show_Equipment_Injection info = new RCR_Show_Equipment_Injection();
                info.NatureType = item.NatureType;
                info.NatureValue = item.NatureValue;
                info.InjectionSlot = item.InjectionSlot;
                BaseInfo.Injections.Add(info);
            }
            return BaseInfo;
        }

        //public void OnResponse_ShowCrossleaderInfo(MemoryStream stream, int uid = 0)
        //{
        //    MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO>(stream);

        //    MSG_RZ_SHOW_CROSS_LEADER_INFO msg = new MSG_RZ_SHOW_CROSS_LEADER_INFO();
        //    msg.PcUid = uid;
        //    foreach (var item in Api.CrossBattleMng.LeaderRankList)
        //    {
        //        MSG_RZ_CROS_RANK_INFO info = Api.CrossBattleMng.GetArenaRankInfoMsg(item.Value);
        //        msg.List.Add(info);
        //    }
        //    Write(msg);
        //}


        public void OnResponse_GetCrossBattleVedio(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CROSS_VIDEO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_VIDEO>(stream);
            MSG_RC_GET_CROSS_VIDEO request = new MSG_RC_GET_CROSS_VIDEO();
            request.TeamId = pks.TeamId;
            request.VedioId = pks.VedioId;
            request.MainId = Api.MainId;
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_GetCrossBattleStartTime(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_CROSS_BATTLE_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CROSS_BATTLE_START>(stream);
            MSG_RC_GET_CROSS_BATTLE_START request = new MSG_RC_GET_CROSS_BATTLE_START();
            Api.CrossServer.Write(request, uid);
        }

        public void OnResponse_GetCrossGuessingInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_GUESSING_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_GUESSING_INFO>(stream);
            Api.CrossGuessingMng.GetGuessingPlayersInfo(uid);
        }

        public void OnResponse_CrossGuessingChoose(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_GUESSING_CHOOSE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_GUESSING_CHOOSE>(stream);

            MSG_RZ_CROSS_GUESSING_CHOOSE msg = new MSG_RZ_CROSS_GUESSING_CHOOSE();
            msg.TimingId = pks.TimingId;
            msg.Choose = pks.Choose;

            msg.Result = Api.CrossGuessingMng.CheckGuessingModel(pks.TimingId);
            if (msg.Result == (int)ErrorCode.Success)
            {
                string reward = Api.CrossGuessingMng.GetGuessingChooseReward(pks.TimingId, uid);
                if (string.IsNullOrEmpty(reward))
                {
                    msg.HasReward = false;
                }
                else
                {
                    msg.HasReward = true;
                    Api.CrossGuessingMng.SetPlayerChoose(pks.TimingId, uid, pks.Choose);
                }
            }
            Write(msg, uid);
        }

        public void OnResponse_CrossGuessingReward(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_GUESSING_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_GUESSING_REWARD>(stream);

            Api.CrossGuessingMng.SetPlayerChoose(pks.TimingId, uid, pks.Choose);

            Api.CrossGuessingMng.SetPlayerReward(pks.TimingId, uid, pks.Reward);
        }
    }
}

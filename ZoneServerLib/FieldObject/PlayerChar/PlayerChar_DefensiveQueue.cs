using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        internal void UpdateDefensiveQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_DEFENSIVE_QUEUE response = new MSG_ZGC_UPDATE_DEFENSIVE_QUEUE();
            Dictionary<int, Dictionary<int, HeroInfo>> oldQueue =new Dictionary<int, Dictionary<int, HeroInfo>>(HeroMng.DefensiveQueue);

            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update defensive queue fail,hero {heroId} not exist");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.DefensiveQueueNum, item.QueueNum))
                {
                    Log.Error($"player {uid} carnival defensive queue fail,hero {heroId} exist in queue {heroInfo.DefensiveQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.CampDefensive, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update camp defensive queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.CampDefensive, heroInfo, item.QueueNum, item.PositionNum, updateList);
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItem(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                TrackDungeonQueueLog(HeroQueueType.CampDefensive, updateList);
            }

            UpdateFortDefensiveQueue(true);

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //komoeLog
            KomoeLogRecordBattleteamFlow("阵营战", oldQueue, HeroMng.DefensiveQueue);
        }

        public void UpdateFortDefensiveQueue(bool setforts = false)
        {
            MSG_ZR_UPDATE_DEFENSIVEQUEUE updateMsg = new MSG_ZR_UPDATE_DEFENSIVEQUEUE();
            updateMsg.SetForts = setforts;
            foreach (var item in GetDefensiveQueueHeros())
            {
                HERO_INFO heroInfo = GetCampHeroInfoMsgData(item.Value);
                updateMsg.HeroList.Add(heroInfo);
            }
            server.RelationServer.Write(updateMsg, uid);
        }

        internal void UpdateCrossQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_CROSS_QUEUE response = new MSG_ZGC_UPDATE_CROSS_QUEUE();
            Dictionary<int, Dictionary<int, HeroInfo>> oldQueue = new Dictionary<int, Dictionary<int, HeroInfo>>(HeroMng.CrossQueue);
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update cross queue fail,hero {heroId} not exist");
                    continue;
                }
                if (item.QueueNum > CrossBattleLibrary.CrossQueueCount)
                {
                    Log.Error($"player {uid} update cross queue fail,hero {heroId} not exist queue {item.QueueNum}");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.CrossQueueNum, item.QueueNum))
                {
                        Log.Error($"player {uid} update cross queue fail,hero {heroId} exist in queue {heroInfo.CrossQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.CrossBattle, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update cross queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.CrossBattle, heroInfo, item.QueueNum, item.PositionNum, updateList);
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItem(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                TrackDungeonQueueLog(HeroQueueType.CrossBattle, updateList);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);

            SyncCrossHeroQueueMsg(0, 0);

            //komoeLog
            KomoeLogRecordBattleteamFlow("跨服战", oldQueue, HeroMng.CrossQueue);
        }

        public void SyncCrossHeroQueueMsg(int seeuUd, int seeMainId)
        {
            MSG_ZR_CROSS_BATTLE_CHALLENGER_HERO_INFO updateMsg = new MSG_ZR_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            foreach (var kv in HeroMng.CrossQueue)
            {
                foreach (var item in kv.Value)
                {
                    updateMsg.Heros.Add(GetZrPlayerHeroInfoMsg(item.Value, HeroQueueType.CrossBattle));
                }
            }
            updateMsg.Uid = uid;
            updateMsg.MainId = server.MainId;
            updateMsg.SeeUid = seeuUd;
            updateMsg.SeeMainId = seeMainId;
            server.RelationServer.Write(updateMsg, uid);
        }

        internal void UpdateCrossChallengeQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_CROSS_CHALLENGE_QUEUE response = new MSG_ZGC_UPDATE_CROSS_CHALLENGE_QUEUE();
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update cross challenge queue fail,hero {heroId} not exist");
                    continue;
                }
                if (item.QueueNum > CrossChallengeLibrary.CrossQueueCount)
                {
                    Log.Error($"player {uid} update cross challenge queue fail,hero {heroId} not exist queue {item.QueueNum}");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.CrossChallengeQueueNum, item.QueueNum))
                {
                        Log.Error($"player {uid} carnival cross challenge queue fail,hero {heroId} exist in queue {heroInfo.CrossChallengeQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.CrossChallenge, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update cross challenge queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.CrossChallenge, heroInfo, item.QueueNum, item.PositionNum, updateList);
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItem(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                TrackDungeonQueueLog(HeroQueueType.CrossChallenge, updateList);
            }


            response.Result = (int)ErrorCode.Success;
            Write(response);

            SyncCrossChallengeHeroQueueMsg(0, 0);
        }

        private bool CheckHeroRepeatedInQueue(int sourceQueue, int aimQueue)
        {
            return aimQueue > 0 && sourceQueue > 0 && sourceQueue != aimQueue;
        }

        private bool CheckQueueFull(HeroQueueType queueType, int queueNum, int heroId, int posNum)
        {
            //只判断上阵的
            if (queueNum <= 0) return false;

            Dictionary<int, HeroInfo> heroInfos = HeroMng.GetDefensivePos(queueType, queueNum);
            if (heroInfos == null || heroInfos.Count < 5) return false;

            //队伍满了,还要上阵
            if (heroInfos.Values.FirstOrDefault(x => x.Id == heroId) == null)
            {
                //当前队伍已满并且自己还不在阵位上，强行上阵
                return true;
            }

            return false;
        }

        public void SyncCrossChallengeHeroQueueMsg(int seeuUd, int seeMainId)
        {
            MSG_ZR_CROSS_CHALLENGE_CHALLENGER_HERO_INFO updateMsg = new MSG_ZR_CROSS_CHALLENGE_CHALLENGER_HERO_INFO();
            foreach (var kv in HeroMng.CrossChallengeQueue)
            {
                foreach (var item in kv.Value)
                {
                    updateMsg.Heros.Add(GetZrPlayerHeroInfoMsg(item.Value, HeroQueueType.CrossChallenge));
                }
            }
            updateMsg.Uid = uid;
            updateMsg.MainId = server.MainId;
            updateMsg.SeeUid = seeuUd;
            updateMsg.SeeMainId = seeMainId;
            server.RelationServer.Write(updateMsg, uid);
        }

        public ZR_Show_HeroInfo GetZrPlayerHeroInfoMsg(HeroInfo baseInfo, HeroQueueType type)
        {
            ZR_Show_HeroInfo info = new ZR_Show_HeroInfo();
            info.Id = baseInfo.Id;
            info.Level = baseInfo.Level;
            info.StepsLevel = baseInfo.StepsLevel;
            info.TitleLevel = baseInfo.TitleLevel;
            info.SoulSkillLevel = baseInfo.SoulSkillLevel;
            info.GodType = baseInfo.GodType;
            info.ComboPower = HeroMng.GetComboPower(baseInfo.Nature);
            info.Power = baseInfo.GetBattlePower();

            switch (type)
            {
                case HeroQueueType.CampDefensive:
                    info.QueueNum = baseInfo.DefensiveQueueNum;
                    info.PositionNum = baseInfo.DefensivePositionNum;
                    break;
                case HeroQueueType.CrossBattle:
                    info.QueueNum = baseInfo.CrossQueueNum;
                    info.PositionNum = baseInfo.CrossPositionNum;
                    break;
                case HeroQueueType.ThemeBoss:
                    info.QueueNum = baseInfo.ThemeBossQueueNum;
                    info.PositionNum = baseInfo.ThemeBossPositionNum;
                    break;
                case HeroQueueType.CrossBoss:
                    info.QueueNum = baseInfo.CrossBossQueueNum;
                    info.PositionNum = baseInfo.CrossBossPositionNum;
                    break;
                case HeroQueueType.CarnivalBoss:
                    info.QueueNum = baseInfo.CarnivalBossQueueNum;
                    info.PositionNum = baseInfo.CarnivalBossPositionNum;
                    break;
                case HeroQueueType.CrossChallenge:
                    info.QueueNum = baseInfo.CrossChallengeQueueNum;
                    info.PositionNum = baseInfo.CrossChallengePositionNum;
                    break;
                default:
                    break;
            }

            Dictionary<int, SoulRingItem> soulRing = SoulRingManager.GetAllEquipedSoulRings(baseInfo.Id);
            if (soulRing != null)
            {
                foreach (var item in soulRing)
                {
                    info.SoulRings.Add(GetShowSoulRingMsg(item.Value));
                }
            }

            List<SoulBone> soulBoneList = SoulboneMng.GetEnhancedHeroBones(baseInfo.Id);
            if (soulBoneList != null)
            {
                foreach (var item in soulBoneList)
                {
                    info.SoulBones.Add(GetShowSoulBoneMsg(item));
                }
            }

            //装备属性加成(属于加成属性)
            List<EquipmentItem> equipmentList = EquipmentManager.GetAllEquipedEquipments(baseInfo.Id);
            if (equipmentList != null)
            {
                foreach (var item in equipmentList)
                {
                    info.Equipments.Add(GetShowEquipmentMsg(baseInfo, item));
                }
            }

            HiddenWeaponItem weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(baseInfo.Id);
            if (weaponItem != null)
            {
                info.HiddenWeapon = new ZR_Hero_HiddenWeapon()
                {
                    Id = weaponItem.Id, Star = weaponItem.Info.Star, 
                    Level = weaponItem.Info.Level,
                    WashList = {weaponItem.Info.WashList}
                };
            }

            //属性
            info.Natures = GetNature(baseInfo);

            return info;
        }

        private ZR_Show_SoulRing GetShowSoulRingMsg(SoulRingItem rInfo)
        {
            ZR_Show_SoulRing BaseInfo = new ZR_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Position;
            BaseInfo.SpecId = rInfo.SpecId;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private ZR_Show_SoulBone GetShowSoulBoneMsg(SoulBone rInfo)
        {
            ZR_Show_SoulBone BaseInfo = new ZR_Show_SoulBone();
            BaseInfo.Id = rInfo.TypeId;
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
            BaseInfo.SpecIds.Add(rInfo.SpecId1);
            BaseInfo.SpecIds.Add(rInfo.SpecId2);
            BaseInfo.SpecIds.Add(rInfo.SpecId3);
            BaseInfo.SpecIds.Add(rInfo.SpecId4);

            BaseInfo.Score = SoulBoneManager.GetSoulBoneScore(rInfo);
            return BaseInfo;
        }
        private ZR_Show_Equipment GetShowEquipmentMsg(HeroInfo hero, EquipmentItem rInfo)
        {
            ZR_Show_Equipment BaseInfo = new ZR_Show_Equipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.EquipedHeroId = rInfo.EquipInfo.EquipHeroId;
            BaseInfo.PartType = rInfo.Model.Part;

            Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            EquipmentModel equipModel = EquipLibrary.GetEquipModel(rInfo.Id);
            if (equipModel != null)
            {
                foreach (var item in equipModel.BaseNatureDic)
                {
                    dic.Add(item.Key, item.Value);
                }
            }

            Slot slot = EquipmentManager.GetSlot(hero.Id, rInfo.Model.Part);
            if (slot != null)
            {
                BaseInfo.Level = slot.EquipLevel;
                BaseInfo.Slot = new ZR_Show_Equipment_Slot();

                ulong jewel = slot.JewelUid;
                NormalItem item = null;

                //计算评分的等级部分
                if (slot.EquipLevel > 0)
                {
                    EquipmentModel model = EquipLibrary.GetEquipModel(hero.GetData().GetInt("Job"), BaseInfo.PartType, 1);
                    EquipUpgradeModel upModel = EquipLibrary.GetEquipUpgradeModel(slot.EquipLevel);
                    Dictionary<NatureType, long> modeldic = model.GetNatureDic();

                    foreach (var kv in modeldic)
                    {
                        dic[kv.Key] += (int)(kv.Value * (upModel.StrengthRatio / 10000.0000f));
                    }
                }

                int percent = 0;
                ItemXuanyuModel xuanyuModel = null;
                if (jewel > 0)
                {
                    item = BagManager.GetItem(jewel) as NormalItem;
                    if (item != null)
                    {
                        BaseInfo.Slot.JewelTypeId = item.Id;
                        xuanyuModel = EquipLibrary.GetXuanyuItem(item.Id);
                        if (xuanyuModel != null)
                        {
                            percent = xuanyuModel.Percent;
                        }
                    }
                }
                HashSet<int> set = new HashSet<int>();//slot.GenerateInjections();
                EquipInjectionModel injectModel = EquipLibrary.GetMaxInjectionSlot(slot.EquipLevel, slot.Part);
                if (injectModel != null)
                {
                    for (int i = 1; i <= injectModel.Slot; i++)
                    {
                        set.Add(i);
                    }
                }

                //处理比例分配
                if (set.Count > 0)
                {
                    long tempPer = percent;// /set.Count;
                    Dictionary<int, int> natureTypes = EquipLibrary.GetNatureTypesFromInjections(set, slot.Part);
                    Dictionary<NatureType, int> tempDic = EquipLibrary.GetLevelNatureBase4Injection(hero.Level);
                    //计算评分的注能部分
                    foreach (var nature in natureTypes)
                    {
                        ZR_Show_Equipment_Injection injection = new ZR_Show_Equipment_Injection();
                        injection.NatureType = nature.Value;
                        injection.InjectionSlot = nature.Key;

                        NatureType natureType = (NatureType)nature.Value;
                        long extraPercent = 0L;
                        if (xuanyuModel != null)
                        {
                            xuanyuModel.NatureList.TryGetValue(natureType, out extraPercent);
                        }

                        int temp;
                        if (tempDic.TryGetValue((NatureType)nature.Value, out temp))
                        {
                            injection.NatureValue = (int) ((tempPer + extraPercent) * (temp / 10000.0000f));
                            dic[(NatureType)nature.Value] += injection.NatureValue;
                        }
                        BaseInfo.Slot.Injections.Add(injection);
                    }
                }
            }

            BaseInfo.Score = ScriptManager.BattlePower.CaculateItemScore2(dic);


            return BaseInfo;
        }

        public MSG_ZGC_HERO_INFO GetZgcPlayerHeroInfoMsg(RobotHeroInfo baseInfo)
        {
            MSG_ZGC_HERO_INFO info = new MSG_ZGC_HERO_INFO();
            info.Id = baseInfo.HeroId;
            info.Level = baseInfo.Level;
            info.Power = baseInfo.BattlePower;
            //info.SoulSkillLevel = baseInfo.SoulSkillLevel;
            info.AwakenLevel = baseInfo.AwakenLevel;
            info.StepsLevel = baseInfo.StepsLevel;
            info.SoulSkillLevel = baseInfo.SoulSkillLevel;
            info.GodType = baseInfo.GodType;
            info.EquipIndex = baseInfo.EquipIndex;
            //info.CrossQueueNum = baseInfo.CrossQueueNum;
            //info.CrossPositionNum = baseInfo.CrossPositionNum;
            return info;
        }

        internal void UpdateCrossBossQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_CROSS_BOSS_QUEUE response = new MSG_ZGC_UPDATE_CROSS_BOSS_QUEUE();
            Dictionary<int, Dictionary<int, HeroInfo>> oldQueue = new Dictionary<int, Dictionary<int, HeroInfo>>(HeroMng.CrossBossQueue);
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update cross boss queue fail,hero {heroId} not exist");
                    continue;
                }
                if (item.QueueNum > CrossBossLibrary.CrossBossQueueCount)
                {
                    Log.Error($"player {uid} update cross boss queue fail,hero {heroId} not exist queue {item.QueueNum}");
                    continue;
                }
                if (CheckHeroRepeatedInQueue(heroInfo.CrossBossQueueNum, item.QueueNum))
                {
                        Log.Error($"player {uid} carnival cross boss queue fail,hero {heroId} exist in queue {heroInfo.CrossBossQueueNum}");
                    continue;
                }

                if (CheckQueueFull(HeroQueueType.CrossBoss, item.QueueNum, item.HeroId, item.PositionNum))
                {
                    Log.Error($"player {uid} update cross boss queue fail,hero queue {item.QueueNum} is full");
                    break;
                }

                HeroMng.UpdateDefQueue(HeroQueueType.CrossBoss, heroInfo, item.QueueNum, item.PositionNum, updateList);
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItem(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                SyncCrosBossHeroQueuMsg(ChallengeIntoType.CrossBossReturn);

                TrackDungeonQueueLog(HeroQueueType.CrossBoss, updateList);
            }


            response.Result = (int)ErrorCode.Success;
            Write(response);

            //komoeLog
            KomoeLogRecordBattleteamFlow("跨服BOSS", oldQueue, HeroMng.CrossBossQueue);
        }

        public void TrackDungeonQueueLog(HeroQueueType queueType, Dictionary<int, HeroInfo> updateList, string specialStr = null)
        {
            string queueInfoStr = string.Empty;

            switch (queueType)
            {
                case HeroQueueType.CampDefensive:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.DefensiveQueueNum + ":" + item.Value.Id + ":" + item.Value.DefensivePositionNum + "_";
                    }
                    break;
                case HeroQueueType.CrossBattle:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.CrossQueueNum + ":" + item.Value.Id + ":" + item.Value.CrossPositionNum + "_";
                    }
                    break;
                case HeroQueueType.ThemeBoss:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.ThemeBossQueueNum + ":" + item.Value.Id + ":" + item.Value.ThemeBossPositionNum + "_";
                    }
                    break;
                case HeroQueueType.CrossBoss:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.CrossBossQueueNum + ":" + item.Value.Id + ":" + item.Value.CrossBossPositionNum + "_";
                    }
                    break;
                case HeroQueueType.CarnivalBoss:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.CarnivalBossQueueNum + ":" + item.Value.Id + ":" + item.Value.CarnivalBossPositionNum + "_";
                    }
                    break;
                case HeroQueueType.IslandChallenge:
                    queueInfoStr = specialStr;
                    break;
                case HeroQueueType.CrossChallenge:
                    foreach (var item in updateList)
                    {
                        queueInfoStr += item.Value.CrossChallengeQueueNum + ":" + item.Value.Id + ":" + item.Value.CrossChallengePositionNum + "_";
                    }
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(queueInfoStr))
            {
                server.TrackingLoggerMng.TrackDungeonQueueLog(Uid, (int)queueType, queueInfoStr, server.Now());
            }
        }
    }
}

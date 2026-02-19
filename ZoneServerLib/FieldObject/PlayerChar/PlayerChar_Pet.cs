using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private PetManager petManager;
        public PetManager PetManager
        { get { return petManager; } }

        private void InitPetManager()
        {
            petManager = new PetManager(this);
        }

        public void InitPets(List<PetInfo> petList)
        {
            Dictionary<ulong, Dictionary<int, int>> needCorrectPets = new  Dictionary<ulong, Dictionary<int, int>>();
            foreach (var pet in petList)
            {
                PetManager.AddPetInfo(pet);
                CheckNeedCorrectPetSkill(pet, needCorrectPets);
            }
            CorrectPetsSkills(needCorrectPets);
        }

        public void SendPetMsg()
        {
            SendPetListMsg();
            SendPetEggListMsg();
            SendPetDungeonQueuesMsg();
        }

        private void SendPetListMsg()
        {
            Dictionary<ulong, PetInfo> petInfoList = PetManager.GetPetInfoList();
            if (petInfoList.Count > 50)
            {
                int total = 0;
                int count = 0;
                MSG_ZGC_PET_LIST petMsg = new MSG_ZGC_PET_LIST();
                foreach (var item in petInfoList)
                {
                    petMsg.List.Add(GeneratePetInfoMsg(item.Value));
                    count++;
                    total++;
                    if (count == 50)
                    {
                        if (total == petInfoList.Count)
                        {
                            petMsg.IsEnd = true;
                        }
                        Write(petMsg);
                        count = 0;
                        petMsg = new MSG_ZGC_PET_LIST();
                    }
                }
                if (count > 0)
                {
                    petMsg.IsEnd = true;
                    Write(petMsg);
                }
            }
            else
            {
                MSG_ZGC_PET_LIST petMsg = new MSG_ZGC_PET_LIST();
                foreach (var info in petInfoList)
                {
                    petMsg.List.Add(GeneratePetInfoMsg(info.Value));
                }
                petMsg.IsEnd = true;
                Write(petMsg);
            }
        }

        private ZGC_PET_INFO GeneratePetInfoMsg(PetInfo info)
        {
            ZGC_PET_INFO msg = new ZGC_PET_INFO();
            msg.UidHigh = info.PetUid.GetHigh();
            msg.UidLow = info.PetUid.GetLow();
            msg.Id = info.PetId;
            msg.Aptitude = info.Aptitude;
            msg.Level = info.Level;
            msg.BreakLevel = info.BreakLevel;
            msg.SummonState = info.Summoned;
            msg.BattlePower = info.GetBattlePower();
            msg.Skill1Level = info.InbornSkillsLevel[1];
            msg.Skill2Level = info.InbornSkillsLevel[2];
            msg.PassiveSkills.AddRange(info.PassiveSkills.Values);
            msg.Shape = info.Shape.ToString();
            msg.Satiety = info.Satiety;
            return msg;
        }

        private void AddPetInfo(PetInfo pet)
        {
            PetManager.AddPetInfo(pet);
            RecordObtainLog(ObtainWay.PetHatch, RewardType.Pet, pet.PetId, 1, 1);
            BIRecordObtainItem(RewardType.Pet, ObtainWay.PetHatch, pet.PetId, 1, 1);
        }

        private void RemovePetInfo(PetInfo info, ConsumeWay way)
        {
            //如果有正在跟随的宠物就召回
            if (info.Summoned)
            {
                RecallPet(info.PetUid);
            }

            PetManager.RemovePetInfo(info.PetUid);
            RecordConsumeLog(way, RewardType.Pet, info.PetId, 1, 1, "");
            //消耗埋点
            BIRecordConsumeItem(RewardType.Pet, way, info.PetId, 1, 1, null);
        }

        public void NotifyClientPetBattlePower(ulong petUid)
        {
            if (petUid > 0 && petUid == PetManager.OnFightPet)
            {
                HeroMng.NotifyClientBattlePower();
            }
        }

        #region 宠物放生
        /// <summary>
        /// 放生宠物
        /// </summary>
        /// <param name="petUids"></param>
        public void ReleasePet(RepeatedField<ulong> petUids)
        {
            MSG_ZGC_RELEASE_PET response = new MSG_ZGC_RELEASE_PET();

            List<ulong> inQueuePets = GetInQueuePetList();

            List<PetInfo> removeList = new List<PetInfo>();
            RewardManager rewardsManager = new RewardManager();
            foreach (ulong petUid in petUids)
            {
                response.Uids.Add(new ZGC_PET_UID() { UidHigh = petUid.GetHigh(), UidLow = petUid.GetLow()});

                PetInfo petInfo = PetManager.GetPetInfo(petUid);
                if (petInfo == null)
                {
                    Log.Warn($"player {Uid} release pet {petUid} failed: not find pet info");
                    continue;
                }
                //在阵上的不让放生
                if (inQueuePets.Contains(petUid))
                {
                    Log.Warn($"player {Uid} release pet {petUid} failed: pet in battle queue");
                    continue;
                }

                removeList.Add(petInfo);
                RemovePetInfo(petInfo, ConsumeWay.PetRelease);//考虑是否删除其他pet信息

                string rewards = RandomPetReleaseRewards(petInfo.BreakLevel);//策划需求每放生一个需要随机一次
                if (!string.IsNullOrEmpty(rewards))
                {
                    rewardsManager.AddSimpleReward(rewards);
                }
            }

            if (removeList.Count > 0)
            {
                response.Result = (int)ErrorCode.Success;
                SyncDbBatchDeletePetInfo(removeList);
            }

            if (rewardsManager.AllRewards.Count > 0)
            {
                rewardsManager.BreakupRewards();
                AddRewards(rewardsManager, ObtainWay.PetRelease, "");
                rewardsManager.GenerateRewardMsg(response.Rewards);
            }

            Write(response);
        }

        private string RandomPetReleaseRewards(int breakLevel)
        {
            string rewards = string.Empty;
            int randomWeight = PetLibrary.RandomReleasePetRewardsWeight(breakLevel);
            if (randomWeight == 0)
            {
                return rewards;
            }
            rewards = PetLibrary.GetReleaseRewards(breakLevel, randomWeight);
            return rewards;
        }

        private void SyncDbBatchDeletePetInfo(List<PetInfo> removeList)
        {
            server.GameDBPool.Call(new QueryDeletePetsInfo(removeList));
        }

        #endregion

        #region 宠物属性
        public void BindPetsNature()
        {
            PetManager.BindPetsNature();
            //HeroMng.CheckAndFixCrossPos();
            //HeroMng.CheckAndFixCrossChallengePos();
        }

        /// <summary>
        /// 查看宠物属性
        /// </summary>
        /// <param name="petUid"></param>
        public void ShowPetNature(ulong petUid)
        {
            MSG_ZGC_SHOW_PET_NATURE response = new MSG_ZGC_SHOW_PET_NATURE();
            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} show pet {1} nature failed", Uid, petUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PetManager.CheckUpdateSatiety(petInfo, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now));

            response.Info = GeneratePetInfoMsg(petInfo);
            response.Nature = GeneratePetNatureMsg(petInfo.Nature);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private ZGC_PET_NATURE GeneratePetNatureMsg(Natures natures)
        {
            ZGC_PET_NATURE nature = new ZGC_PET_NATURE();
            nature.MaxHp = natures.GetNatureValue(NatureType.PRO_MAX_HP).ToInt64TypeMsg();
            nature.Def = natures.GetNatureValue(NatureType.PRO_DEF).ToInt64TypeMsg();
            nature.Atk = natures.GetNatureValue(NatureType.PRO_ATK).ToInt64TypeMsg();
            nature.Cri = natures.GetNatureValue(NatureType.PRO_CRI).ToInt64TypeMsg();
            nature.Hit = natures.GetNatureValue(NatureType.PRO_HIT).ToInt64TypeMsg();
            nature.Flee = natures.GetNatureValue(NatureType.PRO_FLEE).ToInt64TypeMsg();
            nature.Imp = natures.GetNatureValue(NatureType.PRO_IMP).ToInt64TypeMsg();
            nature.Arm = natures.GetNatureValue(NatureType.PRO_ARM).ToInt64TypeMsg();
            nature.Res = natures.GetNatureValue(NatureType.PRO_RES).ToInt64TypeMsg();
            return nature;
        }

        /// <summary>
        /// 宠物升级
        /// </summary>
        public void PetLevelUp(ulong petUid, bool isMulti)
        {
            MSG_ZGC_PET_LEVEL_UP response = new MSG_ZGC_PET_LEVEL_UP();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();
            response.Multi = isMulti;

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} pet {1} level up failed: no such pet", Uid, petUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int upLevel = 1;
            if (isMulti)
            {
                upLevel = PetLibrary.PetConfig.PromoteMultiLevel;
            }
            if (PetLibrary.PetConfig.MaxLevel < petInfo.Level + upLevel)
            {
                Log.Warn("player {0} pet {1} level up failed: pet already max level", Uid, petUid);
                response.Result = (int)ErrorCode.MaxLevel;
                Write(response);
                return;
            }

            int costNum = 0;
            for (int i = 0; i < upLevel; i++)
            {
                int tempCostNum = PetLibrary.GetPetLevelUpSoulPower(petInfo.Level + i);
                if (tempCostNum == 0)
                {
                    Log.Warn("player {0} pet {1} level up failed: level {2} soulPower not find in xml", Uid, petUid, petInfo.Level+i);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                costNum += tempCostNum;
            }

            int soulPower = GetCoins(CurrenciesType.soulPower);
            if (soulPower < costNum)
            {
                Log.Warn("player {0} pet {1} level up failed: level {2} soulPower not enough", petUid, petUid, petInfo.Level);
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }
            //扣除魂力
            DelCoins(CurrenciesType.soulPower, costNum, ConsumeWay.PetLevelUp, petInfo.PetId.ToString());

            //升级
            PetManager.LevelUp(petInfo, upLevel);
            PetManager.LevelUpUpdateNature(petInfo, upLevel);

            //同步
            NotifyClientPetBattlePower(petInfo.PetUid);
            SyncPetChangeMsg(new List<PetInfo>(){ petInfo});
            SyncDbUpdatePetInfo(petInfo);

            response.Level = petInfo.Level;
            response.Result = (int)ErrorCode.Success;
            Write(response);
            
            BIRecordPetDevelopLog("level_up", petInfo, upLevel, (int)CurrenciesType.soulPower, costNum);
        }

        public void SyncPetChangeMsg(List<PetInfo> updateList)
        {
            List<PetInfo> list = new List<PetInfo>();
            if (updateList != null)
            {
                foreach (var petInfo in updateList)
                {
                    list.Add(petInfo);
                    if (list.Count >= 10)
                    {
                        SendPetChangeMessage(list);
                        list.Clear();
                    }
                }
                if (list.Count > 0)
                {
                    SendPetChangeMessage(list);
                    list.Clear();
                }
            }
        }

        private void SendPetChangeMessage(List<PetInfo> updateList)
        {
            MSG_ZGC_PETS_CHANGE msg = new MSG_ZGC_PETS_CHANGE();
            if (updateList != null)
            {
                foreach (var petInfo in updateList)
                {
                    msg.UpdateList.Add(GeneratePetChangeMsg(petInfo));
                    petInfo.AddPassiveSkillsClear();
                }
            }
            Write(msg);
        }

        private ZGC_PET_CHANGE GeneratePetChangeMsg(PetInfo info)
        {
            ZGC_PET_CHANGE msg = new ZGC_PET_CHANGE();
            msg.UidHigh = info.PetUid.GetHigh();
            msg.UidLow = info.PetUid.GetLow();
            msg.BattlePower = info.GetBattlePower();
            msg.Nature = GeneratePetNatureMsg(info.Nature);
            msg.Level = info.Level;
            msg.Aptitude = info.Aptitude;
            msg.Skill1Level = info.InbornSkillsLevel[1];
            msg.Skill2Level = info.InbornSkillsLevel[2];
            msg.AddPassiveSkills.AddRange(info.AddPassiveSkills);
            msg.BreakLevel = info.BreakLevel;
            return msg;
        }

        private void SyncDbUpdatePetInfo(PetInfo petInfo)
        {
            server.GameDBPool.Call(new QueryUpdatePetInfo(petInfo));
        }

        #endregion

        /// <summary>
        /// 召唤宠物
        /// </summary>
        /// <param name="petUid"></param>
        public void CallPet(ulong petUid)
        {
            MSG_ZGC_CALL_PET response = new MSG_ZGC_CALL_PET();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} request to call pet {1} failed: pet not exists", Uid, petUid);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            Pet pet = PetManager.GetSummonedPet(petUid);
            if (pet != null)
            {
                Log.Warn("player {0} request to call pet {1} failed: pet already summoned", Uid, petUid);
                response.Result = (int)ErrorCode.AlreadySummoned;
                Write(response);
                return;
            }

            PetManager.ChangeFollowPet(petUid);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 收回宠物
        /// </summary>
        /// <param name="petUid"></param>
        public void RecallPet(ulong petUid)
        {
            MSG_ZGC_RECALL_PET response = new MSG_ZGC_RECALL_PET();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} request to recall pet {1} failed: pet not exists", uid, petUid);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            Pet pet = PetManager.GetSummonedPet(petUid);
            if (pet == null)
            {
                Log.Warn("player {0} request to recall pet {1} failed: pet not summoned", uid, petUid);
                response.Result = (int)ErrorCode.NotSummoned;
                Write(response);
                return;
            }

            PetManager.RecallPet(petUid);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        #region 宠物养成
        /// <summary>
        /// 宠物传承
        /// </summary>
        /// <param name="fromUid"></param>
        /// <param name="toUid"></param>
        public void PetInherit(ulong fromUid, ulong toUid)
        {
            MSG_ZGC_PET_INHERIT response = new MSG_ZGC_PET_INHERIT();
            PetInfo fromPet = PetManager.GetPetInfo(fromUid);
            PetInfo toPet = PetManager.GetPetInfo(toUid);
            if (fromPet == null || toPet == null)
            {
                Log.Warn("player {0} PetInherit failed: not find from pet {1} or to pet {2}", uid, fromUid, toUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (fromPet.GetData().GetInt("Type") != toPet.GetData().GetInt("Type"))
            {
                Log.Warn("player {0} PetInherit from pet {1} to pet {2} failed: not same type", uid, fromUid, toUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            string[] inheritItem = PetLibrary.PetConfig.InheritItem;
            if (inheritItem.Length < 3)
            {
                Log.Warn("player {0} PetInherit from pet {1} to pet {2} failed: item cost param error", uid, fromUid, toUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem item = null;
            int costId = int.Parse(inheritItem[0]);
            //int costType = int.Parse(inheritItem[1]);
            int costNum = int.Parse(inheritItem[2]);

            item = BagManager.GetItem(MainType.Consumable, costId);
            if (item == null || item.PileNum < costNum)
            {
                Log.Warn($"player {Uid} PetInherit from pet {fromUid} to pet {toUid} failed: item not enough");
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            BaseItem it = DelItem2Bag(item, RewardType.NormalItem, costNum, ConsumeWay.PetInherit);
            if (it != null)
            {
                SyncClientItemInfo(it);
            }
           
            Log.Write($"player {Uid} pet inherit before fromPet uid {fromPet.PetUid} id {fromPet.PetId} breakLevel {fromPet.BreakLevel} level {fromPet.Level}," +
                      $"toPet uid {toPet.PetUid} id {toPet.PetId} breakLevel {toPet.BreakLevel} level {toPet.Level}");

            PetManager.PetInherit(fromPet, toPet);

            Log.Write($"player {Uid} pet inherit after fromPet uid {fromPet.PetUid} id {fromPet.PetId} breakLevel {fromPet.BreakLevel} level {fromPet.Level}," +
                      $"toPet uid {toPet.PetUid} id {toPet.PetId} breakLevel {toPet.BreakLevel} level {toPet.Level}");
            
            PetManager.InitPetNatureInfo(fromPet);
            PetManager.InitPetNatureInfo(toPet);

            NotifyClientPetBattlePower(fromPet.PetUid);
            NotifyClientPetBattlePower(toPet.PetUid);

            SyncPetChangeMsg(new List<PetInfo>() { fromPet, toPet });

            SyncDbUpdatePetInfo(fromPet);
            SyncDbUpdatePetInfo(toPet);

            response.FromUid = new ZGC_PET_UID() { UidHigh = fromPet.PetUid.GetHigh(), UidLow = fromPet.PetUid.GetLow() };
            response.ToUid = new ZGC_PET_UID() { UidHigh = toPet.PetUid.GetHigh(), UidLow = toPet.PetUid.GetLow() };

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }
        
        /// <summary>
        /// 宠物技能洗炼
        /// </summary>
        /// <param name="petUid"></param>
        /// <param name="skillId"></param>
        public void PetSkillBaptize(ulong petUid, int slot, bool useItem)
        {
            MSG_ZGC_PET_SKILL_BAPTIZE response = new MSG_ZGC_PET_SKILL_BAPTIZE();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} pet {1} skill baptize, slot {2} failed: no such pet", Uid, petUid, slot);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            int skillId;  
            if (!petInfo.PassiveSkills.TryGetValue(slot, out skillId))
            {
                Log.Warn("player {0} pet {1} skill baptize failed: pet slot {2} not has skill", Uid, petUid, slot);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            PetPassiveSkillModel oldSkillModel = PetLibrary.GetPetPassiveSkillModel(skillId);
            if (oldSkillModel == null)
            {
                Log.Warn("player {0} pet {1} skill baptize failed: pet slot {2} not find skill {3} in xml", Uid, petUid, slot, skillId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (GetCoins(CurrenciesType.soulDust) < PetLibrary.PetConfig.BaptizeSoulDustCost)
            {
                Log.Warn($"player {Uid} pet {petUid} skill baptize, slot {slot} failed: soul dust not enough");
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            if (useItem)
            {
                BaseItem item;
                int costNum;
                string[] baptizeItemCost = PetLibrary.PetConfig.BaptizeItem;
                if (!CheckItemCost(baptizeItemCost, out item, out costNum))
                {
                    Log.Warn($"player {Uid} pet {petUid} skill baptize, slot {slot} failed: item not enough");
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Write(response);
                    return;
                }

                BaseItem it = DelItem2Bag(item, RewardType.NormalItem, costNum, ConsumeWay.PetSkillBaptize);
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
            }

            DelCoins(CurrenciesType.soulDust, PetLibrary.PetConfig.BaptizeSoulDustCost, ConsumeWay.PetSkillBaptize, petInfo.PetId.ToString());

            PetPassiveSkillModel skillModel = PetLibrary.RandomPetSlotPassiveSkill(slot, useItem, oldSkillModel.Quality);
            if (skillModel != null)
            {
                PetManager.SetPasiveSkill(petInfo, slot, skillModel.Id);
                //通知客户端战力变化
                NotifyClientPetBattlePower(petInfo.PetUid);
                SyncPetChangeMsg(new List<PetInfo>() { petInfo});
                response.SkillId = skillModel.Id;
                //BI埋点
                BIRecordPetSkillLog(petInfo, skillId.ToString(), skillModel.Id.ToString(), oldSkillModel.Quality.ToString(), skillModel.Quality.ToString(), useItem ? 1 : 0);
            }
            response.OldSkillId = skillId;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private bool CheckItemCost(string[] itemCost, out BaseItem item, out int costNum)
        {
            item = null;
            costNum = 0;
            if (itemCost.Length < 3)
            {
                return false;
            }
            int costId = int.Parse(itemCost[0]);
            costNum = int.Parse(itemCost[2]);

            item = BagManager.GetItem(MainType.Consumable, costId);
            if (item == null || item.PileNum < costNum)
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 宠物突破
        /// </summary>
        /// <param name="petUid"></param>
        /// <param name="consumeUids"></param>
        public void PetBreak(ulong petUid, RepeatedField<ulong> consumeUids)
        {
            MSG_ZGC_PET_BREAK response = new MSG_ZGC_PET_BREAK();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} pet {1} break failed: no such pet", Uid, petUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PetBreakModel breakModel = PetLibrary.GetPetBreakModel(petInfo.BreakLevel+1);
            if (breakModel == null || breakModel.BreakConsumeNum <= 0 || breakModel.BreakConsumeNum > consumeUids.Count)
            {
                Log.Warn("player {0} pet {1} break failed: break param error", Uid, petUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            List<ulong> inQueuePets = GetInQueuePetList();
            List<PetInfo> removeList = new List<PetInfo>();
            foreach (ulong consumeUid in consumeUids)
            {
                response.ConsumeUids.Add(new ZGC_PET_UID() { UidHigh = consumeUid.GetHigh(), UidLow = consumeUid.GetLow()});

                PetInfo consumePet = PetManager.GetPetInfo(consumeUid);
                if (consumePet == null)
                {
                    Log.Warn("player {0} pet {1} break failed: not find consume pet {2}", Uid, petUid, consumeUid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                if (consumePet.BreakLevel != petInfo.BreakLevel)
                {
                    Log.Warn("player {0} pet {1} break failed: consume pet {2} breakLevel error", Uid, petUid, consumeUid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                if (inQueuePets.Contains(consumeUid))
                {
                    Log.Warn("player {0} pet {1} break failed:consume pet {2} in queue", Uid, petUid, consumeUid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                removeList.Add(consumePet);
            }
            
            Log.Write($"player {Uid} pet break before main pet uid {petInfo.PetUid} id {petInfo.PetId} breakLevel {petInfo.BreakLevel}");
            foreach (PetInfo consumePet in removeList)
            {
                RemovePetInfo(consumePet, ConsumeWay.PetBreak);
                Log.Write($"player {Uid} pet break consume pet uid {consumePet.PetUid} id {consumePet.PetId} breakLevel {consumePet.BreakLevel}");
            }
            if (removeList.Count > 0)
            {
                SyncDbBatchDeletePetInfo(removeList);
            }

            PetManager.PetBreak(petInfo, breakModel);
            Log.Write($"player {Uid} pet break after main pet uid {petInfo.PetUid} id {petInfo.PetId} breakLevel {petInfo.BreakLevel}");

            NotifyClientPetBattlePower(petInfo.PetUid);
            SyncPetChangeMsg(new List<PetInfo>() { petInfo });
            SyncDbUpdatePetInfo(petInfo);

            response.Result = (int)ErrorCode.Success;
            Write(response);
            
            BIRecordPetDevelopLog("break", petInfo, 1, petInfo.PetId, removeList.Count);
        }

        /// <summary>
        /// 一键进阶
        /// </summary>
        /// <param name="msgList"></param>
        public void OneKeyPetBreak(RepeatedField<MSG_GateZ_PET_BREAK> msgList)
        {
            MSG_ZGC_ONE_KEY_PET_BREAK response = new MSG_ZGC_ONE_KEY_PET_BREAK();
            response.Result = (int)ErrorCode.Fail;

            List<PetInfo> updateList = new List<PetInfo>();
            List<PetInfo> removeList = new List<PetInfo>();
            foreach (var msg in msgList)
            {
                ZGC_PET_BREAK info = new ZGC_PET_BREAK() { };
                ErrorCode result = SinglePetBreak(msg.Uid, msg.ConsumeUids, info, updateList, removeList);
                if (result == ErrorCode.Success)
                {
                    response.Result = (int)ErrorCode.Success;
                }
                response.List.Add(info);
            }
            if (removeList.Count > 0)
            {
                SyncDbBatchDeletePetInfo(removeList);
            }
            SyncPetChangeMsg(updateList);
            SyncDbBatchUpdatePetInfo(updateList);
            Write(response);
        }

        private ErrorCode SinglePetBreak(ulong petUid, RepeatedField<ulong> consumeUids, ZGC_PET_BREAK msgInfo, List<PetInfo> updateList, List<PetInfo> removeList)
        {
            msgInfo.UidHigh = petUid.GetHigh();
            msgInfo.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} pet {1} one key break failed: no such pet", Uid, petUid);
                return ErrorCode.Fail;
            }

            PetBreakModel breakModel = PetLibrary.GetPetBreakModel(petInfo.BreakLevel+1);
            if (breakModel == null || breakModel.BreakConsumeNum <= 0 || breakModel.BreakConsumeNum > consumeUids.Count)
            {
                Log.Warn("player {0} pet {1} one key break failed: break param error", Uid, petUid);
                return ErrorCode.Fail;
            }

            List<ulong> inQueuePets = GetInQueuePetList();

            int consumeCount = 0;
            foreach (ulong consumeUid in consumeUids)
            {
                msgInfo.ConsumeUids.Add(new ZGC_PET_UID() { UidHigh = consumeUid.GetHigh(), UidLow = consumeUid.GetLow() });

                PetInfo consumePet = PetManager.GetPetInfo(consumeUid);
                if (consumePet == null)
                {
                    Log.Warn("player {0} pet {1} one key break failed: not find consume pet {2}", Uid, petUid, consumeUid);
                    return ErrorCode.Fail;
                }
                if (consumePet.BreakLevel != petInfo.BreakLevel)
                {
                    Log.Warn("player {0} pet {1} one key break failed: consume pet {2} breakLevel error", Uid, petUid, consumeUid);
                    return ErrorCode.Fail;
                }
                if (inQueuePets.Contains(consumeUid))
                {
                    Log.Warn("player {0} pet {1} one key break failed:consume pet {2} in queue", Uid, petUid, consumeUid);
                    return ErrorCode.Fail;
                }
                if (consumePet.Level > consumePet.GetData().GetInt("InitLevel"))
                {
                    Log.Warn("player {0} pet {1} one key break failed:consume pet {2} already level up", Uid, petUid, consumeUid);
                    return ErrorCode.Fail;
                }
                if (consumePet.Aptitude >= PetLibrary.PetConfig.HighAptitude)
                {
                    Log.Warn("player {0} pet {1} one key break failed:consume pet {2} aptitude is high", Uid, petUid, consumeUid);
                    return ErrorCode.Fail;
                }
                removeList.Add(consumePet);
                consumeCount++;
            }
            updateList.Add(petInfo);

            Log.Write($"player {Uid} pet break before main pet uid {petInfo.PetUid} id {petInfo.PetId} breakLevel {petInfo.BreakLevel}");
            foreach (PetInfo consumePet in removeList)
            {
                RemovePetInfo(consumePet, ConsumeWay.PetBreak);
                Log.Write($"player {Uid} pet break consume pet uid {consumePet.PetUid} id {consumePet.PetId} breakLevel {consumePet.BreakLevel}");
            }

            PetManager.PetBreak(petInfo, breakModel);
            Log.Write($"player {Uid} pet break after main pet uid {petInfo.PetUid} id {petInfo.PetId} breakLevel {petInfo.BreakLevel}");

            BIRecordPetDevelopLog("break", petInfo, 1, petInfo.PetId, consumeCount);

            NotifyClientPetBattlePower(petInfo.PetUid);
            return ErrorCode.Success;
        }
       
        /// <summary>
        /// 宠物融合
        /// </summary>
        public void PetBlend(ulong mainUid, ulong blendUid)
        {
            MSG_ZGC_PET_BLEND response = new MSG_ZGC_PET_BLEND();
            response.MainUid = new ZGC_PET_UID() { UidHigh = mainUid.GetHigh(), UidLow = mainUid.GetLow() };
            response.BlendUid = new ZGC_PET_UID() { UidHigh = blendUid.GetHigh(), UidLow = blendUid.GetLow() };

            PetInfo mainPet = PetManager.GetPetInfo(mainUid);
            PetInfo blendPet = PetManager.GetPetInfo(blendUid);
            if (mainPet == null || blendPet == null)
            {
                Log.Warn("player {0} PetBlend failed: not find main pet {1} or blend pet {2}", uid, mainUid, blendUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (mainPet.PetId!= blendPet.PetId)
            {
                Log.Warn("player {0} PetBlend main pet {1} blend pet {2} failed: not same type", uid, mainUid, blendUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (mainPet.Aptitude > blendPet.Aptitude)
            {
                Log.Warn("player {0} PetBlend main pet {1} blend pet {2} failed: aptitude not enough", uid, mainUid, blendUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            List<ulong> inQueuePets = GetInQueuePetList();
            if (inQueuePets.Contains(blendPet.PetUid))
            {
                Log.Warn("player {0} PetBlend main pet {1} blend pet {2} failed: blend pet in queue", uid, mainUid, blendUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Log.Write($"player {Uid} pet blend before main pet uid {mainPet.PetUid} id {mainPet.PetId} breakLevel {mainPet.BreakLevel} aptitude {mainPet.Aptitude}");

            PetManager.PetBlend(mainPet, blendPet);
            RemovePetInfo(blendPet, ConsumeWay.PetBlend);
            
            Log.Write($"player {Uid} pet blend after main pet uid {mainPet.PetUid} id {mainPet.PetId} breakLevel {mainPet.BreakLevel} aptitude {mainPet.Aptitude}");
            Log.Write($"player {Uid} pet blend consume blend pet uid {blendPet.PetUid} id {blendPet.PetId} breakLevel {blendPet.BreakLevel} aptitude {blendPet.Aptitude}");

            SyncDbBatchDeletePetInfo(new List<PetInfo>() { blendPet });

            SyncPetChangeMsg(new List<PetInfo>() { mainPet });

            SyncDbUpdatePetInfo(mainPet);

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //融合魂兽在阵上
            //UpdatePetInQueue(blendPet.PetUid, true);
            HeroMng.NotifyClientBattlePower();
        }

        private void UpdatePetInQueue(ulong petUid, bool remove)
        {
            //主战阵容
            foreach (var queue in HeroMng.MainBattleQueue)
            {
                if (queue.Value.PetUid == petUid)
                {
                    PetManager.UpdateMainQueuePetInfo(queue.Value, petUid, remove);
                    if (queue.Value.BattleState == 1)//出战阵容
                    {
                        PetManager.SetMainQueueOnFightPet(queue.Value.PetUid);
                    }
                }
            }
            //TODO 其他副本阵容
        }

        /// <summary>
        /// 宠物喂养
        /// </summary>
        /// <param name="petUid"></param>
        /// <param name="itemId"></param>
        public void PetFeed(ulong petUid, int itemId)
        {
            MSG_ZGC_PET_FEED response = new MSG_ZGC_PET_FEED();
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();

            PetInfo petInfo = PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                Log.Warn("player {0} feed pet {1} failed: no such pet", Uid, petUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            PetFoodModel foodModel = PetLibrary.GetPetFoodModel(itemId);
            if (foodModel == null)
            {
                Log.Warn("player {0} feed pet {1} failed: not find food {2} in xml", Uid, petUid, itemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item == null)
            {
                Log.Warn("player {0} feed pet {1} failed: not find food {2} in bag", Uid, petUid, itemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            BaseItem it = DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.PetFeed);
            if (it != null)
            {
                SyncClientItemInfo(it);
            }
            
            PetManager.PetFeed(petInfo, foodModel.ShapeChange, foodModel.AddSatiety, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now), itemId);

            response.Shape = petInfo.Shape.ToString();
            response.Satiety = petInfo.Satiety;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void SyncDbBatchUpdatePetInfo(List<PetInfo> updateList)
        {
            server.GameDBPool.Call(new QueryBatchUpdatePetInfo(updateList));
        }
        #endregion

        /// <summary>
        /// 宠物上阵
        /// </summary>
        /// <param name="queueNum"></param>
        /// <param name="petUid"></param>
        public void UpdateMainQueuePet(int queueNum, ulong petUid, bool remove)
        {
            MSG_ZGC_UPDATE_MAINQUEUE_PET response = new MSG_ZGC_UPDATE_MAINQUEUE_PET();
            response.QueueNum = queueNum;
            response.UidHigh = petUid.GetHigh();
            response.UidLow = petUid.GetLow();
            response.Remove = remove;

            MainBattleQueueInfo info;
            HeroMng.MainBattleQueue.TryGetValue(queueNum, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} UpdateMainQueuePet failed: queue {queueNum} is locked");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!remove)
            {
                PetInfo petInfo = PetManager.GetPetInfo(petUid);
                if (petInfo == null)
                {
                    Log.Warn($"player {Uid} UpdateMainQueuePet failed: not find pet {petUid}");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }
            else
            {
                if (info.PetUid <= 0)
                {
                    Log.Warn($"player {Uid} UpdateMainQueuePet failed: have no pet to remove");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }

            PetManager.UpdateMainQueuePetInfo(info, petUid, remove);

            if (info.BattleState == 1)//出战阵容
            {
                PetManager.SetMainQueueOnFightPet(info.PetUid);
                HeroMng.NotifyClientBattlePower();
                //HeroMng.UpdatePlayerDefensiveHerosToRedis();
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private List<ulong> GetInQueuePetList()
        {
            List<ulong> inQueuePets = new List<ulong>();
            //mainBattleQueue
            foreach (var queue in HeroMng.MainBattleQueue)
            {
                if (!inQueuePets.Contains(queue.Value.PetUid))
                {
                    inQueuePets.Add(queue.Value.PetUid);
                }
            }
            //dugeonQueue
            Dictionary<DungeonQueueType, Dictionary<int, PetInfo>> dungeonQueues = PetManager.GetPetDungeonQueueList();
            foreach (var queue in dungeonQueues)
            {
                Dictionary<int, PetInfo> petDic = queue.Value;
                foreach (var kv in petDic)
                {
                    if (!inQueuePets.Contains(kv.Value.PetUid))
                    {
                        inQueuePets.Add(kv.Value.PetUid);
                    }
                }
            }
            return inQueuePets;
        }

        #region 宠物蛋
        public void InitPetEggs(List<PetEggItem> petEggList)
        {
            foreach (var petEgg in petEggList)
            {
                PetManager.AddPetEggItem(petEgg);
                PetManager.AddHatchPetEgg(petEgg);
            }
        }

        private void SendPetEggListMsg()
        {
            MSG_ZGC_PET_EGG_LIST msg = new MSG_ZGC_PET_EGG_LIST();
            Dictionary<ulong, PetEggItem> petEggList = PetManager.GetPetEggList();
            if (petEggList.Count > 50)
            {
                int count = 0;
                foreach (var petEgg in petEggList)
                {                   
                    msg.Items.Add(GeneratePetEggItemMsg(petEgg.Value));
                    count++;
                    if (count == 50)
                    {
                        Write(msg);
                        count = 0;
                        msg = new MSG_ZGC_PET_EGG_LIST();
                    }
                }
                if (count > 0)
                {
                    Write(msg);
                }
            }
            else
            {
                foreach (var petEgg in petEggList)
                {
                    msg.Items.Add(GeneratePetEggItemMsg(petEgg.Value));
                }
                Write(msg);
            }
        }

        private PET_EGG_ITEM GeneratePetEggItemMsg(PetEggItem item)
        {
            PET_EGG_ITEM itemMsg = new PET_EGG_ITEM()
            {
                UidHigh = item.Uid.GetHigh(),
                UidLow = item.Uid.GetLow(),
                Id = item.Id,
                HatchStartTime = item.HatchStartTime
            };
            return itemMsg;
        }

        public void AddPets(RewardManager manager, ObtainWay way, string extraParam = "")
        {
            List<PetEggItem> addList = new List<PetEggItem>();
            var pets = manager.GetRewardItemList(RewardType.Pet);
            var petDic = manager.GetRewardList(RewardType.Pet);//用于检查是否已经获得过
            foreach (var curr in pets)
            {
                if (curr.Attrs.Count == 0 && petDic.ContainsKey(curr.Id))//
                {
                    for (int i = 0; i < curr.Num; i++)
                    {
                        var pet = PetManager.AddPetEggItem(curr.Id);
                        if (pet != null)//假如格子足够而没有进邮件
                        {
                            addList.Add(pet);

                            //获取埋点
                            RecordObtainLog(way, RewardType.Pet, curr.Id, 1, 1, extraParam);
                            BIRecordObtainItem(RewardType.Pet, way, curr.Id, 1, 1);
                        }
                    }
                }
            }
            SendPetEggItemsUpdateMsg(addList);
        }

        private void SendPetEggItemsUpdateMsg(List<PetEggItem> itemList)
        {
            if (itemList.Count > 0)
            {
                MSG_ZGC_UPDATE_PET_EGG msg = new MSG_ZGC_UPDATE_PET_EGG();
                foreach (var item in itemList)
                {
                    msg.Items.Add(GeneratePetEggItemMsg(item));
                }
                Write(msg);
            }
        }

        /// <summary>
        /// 开始孵蛋
        /// </summary>
        /// <param name="petUid"></param>
        public void HatchPetEgg(ulong petUid)
        {
            MSG_ZGC_HATCH_PET_EGG response = new MSG_ZGC_HATCH_PET_EGG();
            PetEggItem item = PetManager.GetPetEggItem(petUid);
            if (item == null)
            {
                Log.Warn($"player {Uid} hatch pet egg {petUid} failed: not have this item");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PetEggModel eggModel = PetLibrary.GetPetEggModel(item.Id);
            if (eggModel == null)
            {
                Log.Warn($"player {Uid} hatch pet egg {petUid} failed: not find egg {item.Id} in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (item.HatchStartTime > 0)
            {
                Log.Warn($"player {Uid} hatch pet egg {petUid} failed: already in hatch queue");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
          
            int hatchCountLimit = PetLibrary.PetConfig.BasicHatchNum;
            if (CheckSuperMonthCardState())
            {
                hatchCountLimit = PetLibrary.PetConfig.BuyHatchNum;
            }
            int hatchCount = PetManager.GetHatchCount();
            if (hatchCount >= hatchCountLimit)
            {
                Log.Warn($"player {Uid} hatch pet egg {petUid} failed: hatch queue limit");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PetManager.SetPetEggHatchStartTime(item);
            PetManager.AddHatchPetEgg(item);

            response.Result = (int)ErrorCode.Success;
            response.Item = GeneratePetEggItemMsg(item);
            Write(response);
        }  

        /// <summary>
        /// 完成孵蛋
        /// </summary>
        /// <param name="petUid"></param>
        public void FinishHatchPetEgg(ulong petUid)
        {
            MSG_ZGC_FINISH_HATCH_PET_EGG response = new MSG_ZGC_FINISH_HATCH_PET_EGG();
            PetEggItem item = PetManager.GetPetEggItem(petUid);
            if (item == null)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: not have this item");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            PetEggModel eggModel = PetLibrary.GetPetEggModel(item.Id);
            if (eggModel == null)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: not find egg {item.Id} in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (item.HatchStartTime == 0)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: not in hatch queue");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
         
            int hatchTime = eggModel.HatchTime;
            if (CheckSuperMonthCardState())
            {
                hatchTime = (int)(hatchTime * (1 - PetLibrary.PetConfig.BuySubHatchTimeRatio * 0.01f));
            }
            int nowTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            if (nowTime < item.HatchStartTime + hatchTime)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: not reach time");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int petsCount = PetManager.GetPetsCount();
            if (petsCount >= PetLibrary.PetConfig.PetMaxNum)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: pets num already max");
                response.Result = (int)ErrorCode.PetMaxNumLimit;
                Write(response);
                return;
            }

            PetInfo pet = PetManager.CreateNewPetInfo(eggModel);
            if (pet == null)
            {
                Log.Warn($"player {Uid} finish hatch pet egg {petUid} failed: create pet error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            AddPetInfo(pet);
            PetManager.BindPetNature(pet);

            RemovePetEggItem(item);
            PetManager.RemoveHatchPetEgg(petUid);

            response.Result = (int)ErrorCode.Success;
            response.UidHigh = item.Uid.GetHigh();
            response.UidLow = item.Uid.GetLow();
            response.PetInfo = GeneratePetInfoMsg(pet);
            response.Nature = GeneratePetNatureMsg(pet.Nature);
            Write(response);
        }

        private void RemovePetEggItem(PetEggItem item)
        {
            PetManager.RemovePetEggItem(item.Uid);
            RecordConsumeLog(ConsumeWay.PetHatch, RewardType.Pet, item.Id, 1, 1, "");
            //消耗埋点
            BIRecordConsumeItem(RewardType.Pet, ConsumeWay.PetHatch, item.Id, 1, 1, null);
        }
        #endregion

        private void CheckNeedCorrectPetSkill(PetInfo petInfo, Dictionary<ulong, Dictionary<int, int>> petsSkillSlots)
        {
            Dictionary<int, int> correctSkillSlots = new Dictionary<int, int>();
            foreach (var kv in petInfo.PassiveSkills)
            {
                int slot = kv.Key;
                int skillId = kv.Value;
                PetPassiveSkillModel skillModel = PetLibrary.GetPetPassiveSkillModel(skillId);
                if (skillModel?.Slot != slot)
                {
                    correctSkillSlots.Add(slot, skillId);
                }
            }
            petsSkillSlots.Add(petInfo.PetUid, correctSkillSlots);
        }

        private void CorrectPetsSkills( Dictionary<ulong, Dictionary<int, int>> needCorrectPets)
        {
            foreach (var kv in needCorrectPets)
            {
                PetInfo petInfo = PetManager.GetPetInfo(kv.Key);
                foreach (var skill in kv.Value)
                {
                    int slot = skill.Key;
                    int skillId = skill.Value;
                    PetPassiveSkillModel oldSkillModel = PetLibrary.GetPetPassiveSkillModel(skillId);
                    PetPassiveSkillModel skillModel = PetLibrary.RandomPetSlotPassiveSkill(slot, true, oldSkillModel != null? oldSkillModel.Quality : 1);
                    if (skillModel != null)
                    {
                        PetManager.SetPasiveSkill(petInfo, slot, skillModel.Id);
                    }
                }
            }
        }
        
        public void SendPetTransformMsg()
        {
            SendPetInfoListMsg();
            SendPetEggItemsMsg();
            SendPetDungeonQueuesTransMsg();
        }

        private void SendPetInfoListMsg()
        {
            MSG_ZMZ_PET_LIST msg = new MSG_ZMZ_PET_LIST();
            msg.OnFightPet = PetManager.OnFightPet;

            Dictionary<ulong, PetInfo> petInfoList = PetManager.GetPetInfoList();
            if (petInfoList.Count > CONST.PET_MSG_MAX_COUNT)
            {
                int count = 0;              
                foreach (var info in petInfoList)
                {
                    msg.List.Add(GeneratePetInfoTransformMsg(info.Value));
                    count++;
                    if (count >= CONST.PET_MSG_MAX_COUNT)
                    {
                        server.ManagerServer.Write(msg, Uid);
                        count = 0;
                        msg = new MSG_ZMZ_PET_LIST();
                        msg.OnFightPet = PetManager.OnFightPet;
                    }
                }
                if (count > 0)
                {
                    server.ManagerServer.Write(msg, Uid);
                }
            }
            else
            {
                foreach (var info in petInfoList)
                {
                    msg.List.Add(GeneratePetInfoTransformMsg(info.Value));
                }
                server.ManagerServer.Write(msg, Uid);
            }
        }

        private ZMZ_PET_INFO GeneratePetInfoTransformMsg(PetInfo info)
        {
            ZMZ_PET_INFO infoMsg = new ZMZ_PET_INFO();
            infoMsg.PetUid = info.PetUid;
            infoMsg.PetId = info.PetId;
            infoMsg.Aptitude = info.Aptitude;
            infoMsg.Level = info.Level;
            infoMsg.BreakLevel = info.BreakLevel;
            infoMsg.Summoned = info.Summoned;
            infoMsg.NatureList = GetNaturesTransform(info.Nature.GetNatureList());
            infoMsg.BattlePower = info.GetBattlePower();
            foreach (var kv in info.InbornSkillsLevel)
            {
                infoMsg.InbornSkillsLevel.Add(kv.Key, kv.Value);
            }
            foreach (var kv in info.PassiveSkills)
            {
                infoMsg.PassiveSkills.Add(kv.Key, kv.Value);
            }
            infoMsg.Shape = info.Shape;
            infoMsg.Satiety = info.Satiety;
            infoMsg.LastFeedTime = info.LastFeedTime;
            infoMsg.SatietyUpdateTime = info.SatietyUpdateTime;
            infoMsg.SatietyProtectFlag = info.SatietyProtectFlag;
            return infoMsg;
        }

        public void LoadPetListTransformMsg(MSG_ZMZ_PET_LIST msg)
        {
            foreach (var pet in msg.List)
            {
                int summoned = pet.Summoned ? 1: 0;
                Dictionary<int, int> inbornSkillsLevel = LoadPetSkillsInfo(pet.InbornSkillsLevel);
                Dictionary<int, int> passiveSkills = LoadPetSkillsInfo(pet.PassiveSkills);
                PetInfo info = new PetInfo(pet.PetUid, pet.PetId, pet.Aptitude, pet.Level, pet.BreakLevel, summoned, inbornSkillsLevel, passiveSkills, pet.Shape, pet.Satiety, pet.LastFeedTime, pet.SatietyUpdateTime, pet.SatietyProtectFlag);
                foreach (var natureIt in pet.NatureList.Natures)
                {
                    info.Nature.SetNewNature((NatureType)natureIt.Type, natureIt.BaseValue, natureIt.AddedValue, natureIt.BaseRatio);
                }
                info.UpdateBattlePower(pet.BattlePower);
                PetManager.AddPetInfo(info);
                //TODO bind queue
            }
            PetManager.SetMainQueueOnFightPet(msg.OnFightPet);
        }

        private Dictionary<int, int> LoadPetSkillsInfo(MapField<int, int> skillMap)
        {
            Dictionary<int, int> skillsDic = new Dictionary<int, int>();
            foreach (var kv in skillMap)
            {
                skillsDic.Add(kv.Key, kv.Value);
            }
            return skillsDic;
        }

        public void SendPetEggItemsMsg()
        {
            MSG_ZMZ_PET_EGG_ITEMS msg = new MSG_ZMZ_PET_EGG_ITEMS();
            Dictionary<ulong, PetEggItem> eggList = PetManager.GetPetEggList();
            if (eggList.Count > CONST.PET_MSG_MAX_COUNT)
            {
                int count = 0;
                foreach (var item in eggList)
                {
                    msg.Items.Add(GeneratePetEggTransformMsg(item.Value));
                    count++;
                    if (count >= CONST.PET_MSG_MAX_COUNT)
                    {
                        server.ManagerServer.Write(msg, Uid);
                        count = 0;
                        msg = new MSG_ZMZ_PET_EGG_ITEMS();
                    }
                }
                if (count > 0)
                {
                    server.ManagerServer.Write(msg, Uid);
                }
            }
            else
            {
                foreach (var item in eggList)
                {
                    msg.Items.Add(GeneratePetEggTransformMsg(item.Value));
                }
                server.ManagerServer.Write(msg, Uid);
            }
        }

        private ZMZ_PET_EGG_ITEM GeneratePetEggTransformMsg(PetEggItem item)
        {
            ZMZ_PET_EGG_ITEM msg = new ZMZ_PET_EGG_ITEM();
            msg.Uid = item.Uid;
            msg.Id = item.Id;
            msg.HatchStartTime = item.HatchStartTime;
            return msg;
        }

        public void LoadPetEggsTransformMsg(MSG_ZMZ_PET_EGG_ITEMS msg)
        {
            foreach (var item in msg.Items)
            {
                PetEggItem eggItem = new PetEggItem(item.Uid, item.Id, item.HatchStartTime);
                PetManager.BindTransformPetEggItem(eggItem);
            }
        }
    }
}

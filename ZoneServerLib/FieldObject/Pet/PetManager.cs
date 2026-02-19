using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class PetManager
    {
        PlayerChar owner;
        //拥有的宠物列表
        Dictionary<ulong, PetInfo> petInfoList;
        //召唤出来的宠物列表
        Dictionary<ulong, Pet> petSummonedList;
        //宠物蛋列表
        Dictionary<ulong, PetEggItem> petEggList;
        //孵化队列
        Dictionary<ulong, PetEggItem> hatchList;
        //主阵容出战宠物
        public ulong OnFightPet { get; private set; }
        //宠物副本阵容 key:dungeonType, queueNum
        Dictionary<DungeonQueueType, Dictionary<int, PetInfo>> dungeonQueueList;

        public PetManager(PlayerChar owner)
        {
            this.owner = owner;
            petInfoList = new Dictionary<ulong, PetInfo>();
            petSummonedList = new Dictionary<ulong, Pet>();
            petEggList = new Dictionary<ulong, PetEggItem>();
            hatchList = new Dictionary<ulong, PetEggItem>();
            dungeonQueueList = new Dictionary<DungeonQueueType, Dictionary<int, PetInfo>>();
        }

        public void AddPetInfo(PetInfo info)
        {
            if (info == null)
            {
                return;
            }
            if (petInfoList.ContainsKey(info.PetUid))
            {
                petInfoList[info.PetUid] = info;
            }
            else
            {
                petInfoList.Add(info.PetUid, info);
            }
        }

        public void RemovePetInfo(ulong petUid)
        {
            petInfoList.Remove(petUid);
        }

        public PetInfo GetPetInfo(ulong petUid)
        {
            PetInfo info = null;
            petInfoList.TryGetValue(petUid, out info);
            return info;
        }

        public Dictionary<ulong, PetInfo> GetPetInfoList()
        {
            return petInfoList;
        }

        public int GetPetsCount()
        {
            return petInfoList.Count;
        }

        #region 宠物召唤
        public Pet GetSummonedPet(ulong petUid)
        {
            Pet pet = null;
            petSummonedList.TryGetValue(petUid, out pet);
            return pet;
        }

        // 召唤宠物
        private void CallPet(ulong petUid, bool syncDb = false, int queueNum = 0)
        {
            PetInfo info = GetPetInfo(petUid);
            if (info == null || petSummonedList.ContainsKey(petUid))
            {
                return;
            }
            PetModel petModel = PetLibrary.GetPetModel(info.PetId);
            if (petModel == null)
            {
                return;
            }

            Pet pet = new Pet(owner.server, owner, info, petModel, queueNum);
            pet.Init();
            petSummonedList.Add(petUid, pet);
            // 地图内同步
            owner.CurrentMap.CreatePet(pet);
            if (syncDb)
            {
                info.SetSummoned(true);
                owner.server.GameDBPool.Call(new QueryUpdatePetSummonState(petUid, info.Summoned));
            }
        }

        // 召回宠物
        public void RecallPet(ulong petUid)
        {
            Pet pet = null;
            PetInfo info = GetPetInfo(petUid);
            if (info == null || !petSummonedList.TryGetValue(petUid, out pet))
            {
                return;
            }
            // 召回
            petSummonedList.Remove(petUid);
            owner.CurrentMap.RemovePet(pet.InstanceId);
            info.SetSummoned(false);
            // todo 属性同步
            owner.server.GameDBPool.Call(new QueryUpdatePetSummonState(petUid, info.Summoned));
        }

        // 玩家上线后，自动召唤
        public void CallPetsToMap()
        {
            MapType mapType = owner.CurrentMap.GetMapType();
            switch (mapType)
            {
                case MapType.Map:
                    CallPetsToMap(CallPetRule.Follower);
                    break;
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.IntegralBoss:
                case MapType.HuntingActivityTeam:
                    TeamDungeonMap map = owner.CurrentMap as TeamDungeonMap;
                    if (map == null)
                    {
                        return;
                    }
                    CallPetsToMap(CallPetRule.MainBattleQueue);
                    break;
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                    CallPetsToMap(CallPetRule.MainBattleQueue);
                    break;
                case MapType.ThemeBoss:
                case MapType.CarnivalBoss:
                case MapType.HuntingIntrude:
                case MapType.Tower:
                    CallDungeonQueuePetsToMap(mapType);
                    break;
                default:
                    break;
            }
        }

        private void CallPetsToMap(CallPetRule type)
        {
            switch (type)
            {
                case CallPetRule.Follower:
                    CallSunmoned();
                    break;
                case CallPetRule.MainBattleQueue:
                    if (OnFightPet > 0 && owner.CheckLimitOpen(LimitType.PetBattle))
                    {
                        CallPet(OnFightPet, false, 1);
                    }
                    break;
                default:
                    break;
            }
        }

        private void CallSunmoned()
        {
            if (!owner.CurrentMap.IsDungeon)
            {
                foreach (var info in petInfoList)
                {
                    if (info.Value.Summoned)
                    {
                        CallPet(info.Value.PetUid);
                        break;
                    }
                }
            }
        }

        private void CallDungeonQueuePetsToMap(MapType mapType)
        {
            if (!owner.CheckLimitOpen(LimitType.PetBattle))
            {
                return;
            }
            Dictionary<int, PetInfo> queuePets = null;
            switch (mapType)
            {
                case MapType.ThemeBoss:
                    queuePets = GetDungeonQueuePetsInfo(DungeonQueueType.ThemeBoss);
                    break;
                case MapType.CarnivalBoss:
                    queuePets = GetDungeonQueuePetsInfo(DungeonQueueType.CarnivalBoss);
                    break;
                case MapType.HuntingIntrude:
                    queuePets = GetDungeonQueuePetsInfo(DungeonQueueType.HuntingIntrude);
                    break;
                case MapType.Tower:
                    queuePets = GetDungeonQueuePetsInfo(DungeonQueueType.Tower);
                    break;
                default:
                    break;
            }
            if (queuePets == null || queuePets.Count == 0)
            {
                return;
            }
            foreach (var kv in queuePets)
            {
                CallPet(kv.Value.PetUid, false, kv.Key);
            }
        }

        //更换跟随宠物，自动召回
        public void RecallSummoned()
        {
            List<ulong> removeList = new List<ulong>();
            foreach (var pet in petSummonedList)
            {
                removeList.Add(pet.Key);
            }
            foreach (var pet in removeList)
            {
                RecallPet(pet);
            }
        }

        public void ChangeFollowPet(ulong petUid)
        {
            RecallSummoned();
            CallPet(petUid, true);
        }

        public void TakeBackPetFromMap()
        {
            switch (owner.CurrentMap.GetMapType())
            {
                case MapType.Map:
                    TakeBackPetFromMap(CallPetRule.Follower);
                    break;
                case MapType.TeamDungeon:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                    TeamDungeonMap map = owner.CurrentMap as TeamDungeonMap;
                    if (map == null)
                    {
                        return;
                    }
                    TakeBackPetFromMap(CallPetRule.MainBattleQueue);
                    break;
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.IntegralBoss:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                    TakeBackPetFromMap(CallPetRule.MainBattleQueue);
                    break;
                case MapType.Tower:
                case MapType.ThemeBoss:
                //case MapType.CrossBoss:
                case MapType.CarnivalBoss:
                case MapType.HuntingIntrude:
                    TakeBackPetFromMap(CallPetRule.DungeonQueue);
                    break;
                default:
                    break;
            }
        }

        private void TakeBackPetFromMap(CallPetRule type)
        {
            switch (type)
            {
                case CallPetRule.Follower:
                case CallPetRule.MainBattleQueue:
                case CallPetRule.DungeonQueue:
                    foreach (var item in petSummonedList)
                    {
                        if (item.Value.InstanceId > 0)
                        {
                            owner.CurrentMap.RemovePet(item.Value.InstanceId);
                            //item.Value.SetInstanceId(0);
                        }
                    }
                    petSummonedList.Clear();
                    break;
                default:
                    break;
            }
        }

        public int GetCallPetCount(int dungeonId = 0)
        {
            MapType mapType = owner.CurrentMap.GetMapType();
            switch (mapType)
            {
                case MapType.Map:
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                case MapType.IntegralBoss:
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                //case MapType.Arena:
                case MapType.CrossBattle:
                case MapType.CrossFinals:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                case MapType.CrossChallenge:
                case MapType.CrossChallengeFinals:
                    if (OnFightPet > 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                case MapType.ThemeBoss:
                    return GetCallPetCount(DungeonQueueType.ThemeBoss);
                //case MapType.CrossBoss:                   
                case MapType.CarnivalBoss:
                    return GetCallPetCount(DungeonQueueType.CarnivalBoss);
                case MapType.HuntingIntrude:
                    return GetCallPetCount(DungeonQueueType.HuntingIntrude);
                case MapType.Tower:
                    return GetCallPetCount(DungeonQueueType.Tower);
                case MapType.IslandChallenge:
                    IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(owner.IslandChallengeManager.Task.TaskInfo.param[0]);
                    if (model == null || !model.Dungeon2Queue.ContainsKey(dungeonId)) return 0;
                    return GetCallPetCount(DungeonQueueType.IslandChallenge, model.Dungeon2Queue[dungeonId]);
                case MapType.SpaceTimeTower:
                    return GetCallPetCount(DungeonQueueType.SpaceTimeTower);
                default:
                    return 0;
            }
        }

        private int GetCallPetCount(DungeonQueueType queueType)
        {
            Dictionary<int, PetInfo> inQueuePets = GetDungeonQueuePetsInfo(queueType);
            if (inQueuePets == null)
            {
                return 0;
            }
            return inQueuePets.Count;
        }

        private int GetCallPetCount(DungeonQueueType queueType, int queueNum)
        {
            Dictionary<int, PetInfo> inQueuePets = GetDungeonQueuePetsInfo(queueType);
            if (inQueuePets == null)
            {
                return 0;
            }
            if (inQueuePets.ContainsKey(queueNum))
            {
                return 1;
            }
            return 0;
        }
        #endregion

        #region 孵蛋
        public PetInfo CreateNewPetInfo(PetEggModel eggModel)
        {
            int petId = eggModel.GeneratePetId();
            PetModel petModel = PetLibrary.GetPetModel(petId);
            if (petModel == null)
            {
                return null;
            }
            int aptitude = eggModel.GenerateAptitude();
            ulong petUid = owner.server.UID.NewPuid(owner.server.MainId, owner.server.SubId);
            int breakLevel = GenerateInitBreakLevel(eggModel);
            Dictionary<int, int> inbronSkillLevel = GenerateInbornSkillLevel(breakLevel);
            Dictionary<int, int> passiveSkills = GeneratePassiveSkills(aptitude, breakLevel);

            PetInfo pet = CreateNewPetInfo(petUid, petId, aptitude, petModel.InitLevel, breakLevel, inbronSkillLevel, passiveSkills);
            return pet;
        }

        private int GenerateInitBreakLevel(PetEggModel eggModel)
        {
            int radomBreakLevel = eggModel.GenerateInitBreakLevel();
            int maxBreakLevel = PetLibrary.PetConfig != null ? PetLibrary.PetConfig.BreakMaxLevel : 30;
            int breakLevel = Math.Min(radomBreakLevel, maxBreakLevel);
            return breakLevel;
        }

        private Dictionary<int, int> GenerateInbornSkillLevel(int breakLevel)
        {
            int skill1Level = 1;
            int skill2Level = 1;
            for (int i = 1; i <= breakLevel; i++)
            {
                PetBreakModel breakModel = PetLibrary.GetPetBreakModel(i);
                if (breakModel == null || breakModel.InbornSkillLevelUp == 0)
                {
                    continue;
                }
                PetInbornSkillModel skill1Model = PetLibrary.GetPetInbornSkillModel(1, skill1Level);
                PetInbornSkillModel skill2Model = PetLibrary.GetPetInbornSkillModel(2, skill2Level);
                int weight = 0;
                if (skill1Model != null)
                {
                    weight += skill1Model.LevelUpWeight;
                }
                if (skill2Model != null)
                {
                    weight += skill2Model.LevelUpWeight;
                }
                int random = NewRAND.Next(1, weight);//
                if (random <= skill1Model?.LevelUpWeight)
                {
                    skill1Level += breakModel.InbornSkillLevelUp;
                }
                else
                {
                    skill2Level += breakModel.InbornSkillLevelUp;
                }
            }
            Dictionary<int, int> skillsLevel = new Dictionary<int, int>();
            skillsLevel.Add(1, skill1Level);
            skillsLevel.Add(2, skill2Level);
            return skillsLevel;
        }

        private Dictionary<int, int> GeneratePassiveSkills(int aptitude, int breakLevel)
        {
            int skillNum = PetLibrary.GetInitPassiveSkillNum(aptitude);
            PetBreakModel breakModel = PetLibrary.GetPetBreakModel(breakLevel);
            if (breakModel != null)
            {
                skillNum += breakModel.ExtraPassiveSkillNum;
            }
            Dictionary<int, int> passiveSkills = new Dictionary<int, int>();
            for (int i = 1; i <= skillNum; i++)
            {
                PetPassiveSkillModel skillModel = PetLibrary.RandomPetSlotPassiveSkill(i);
                if (skillModel != null)
                {
                    passiveSkills.Add(i, skillModel.Id);
                }
            }
            return passiveSkills;
        }

        private PetInfo CreateNewPetInfo(ulong petUid, int petId, int aptitude, int initLevel, int initBreakLevel, Dictionary<int, int> inbronSkillLevel, Dictionary<int, int> passiveSkills)
        {
            PetInfo pet = new PetInfo(petUid, petId, aptitude, initLevel, initBreakLevel, 0, inbronSkillLevel, passiveSkills, 1.00f, PetLibrary.PetConfig.MaxSatiety, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now), 0, false);
            SyncDbInsertPetInfo(pet);
            return pet;
        }

        private void SyncDbInsertPetInfo(PetInfo pet)
        {
            owner.server.GameDBPool.Call(new QueryInsertPetInfo(owner.Uid, pet));
        }

        #endregion

        #region 宠物属性
        public void BindPetsNature()
        {
            foreach (var info in petInfoList)
            {
                BindPetNature(info.Value);
            }
        }

        public void BindPetNature(PetInfo info)
        {
            //BindHeroQueueList(info);
            InitPetNatureInfo(info); 
        }

        //初始化宠物属性
        public void InitPetNatureInfo(PetInfo info)
        {
            info.Nature.Clear();
            //基础属性
            Dictionary<NatureType, Int64> basicNatures = GetBasicNatureList(info, info.Level);
            //9项基础属性
            info.InitBasicNature(basicNatures);
            //初始化移动速度
            PetModel petModel = PetLibrary.GetPetModel(info.PetId);
            info.InitSpeed(petModel.PRO_RUN_IN_BATTLE, petModel.PRO_RUN_OUT_BATTLE);

            //被动技能属性加成
            PassiveSkillsBonusNature(info);

            //最后设置
            info.InitHp();
            //计算战力
            UpdateBattlePower(info);
        }

        public Dictionary<NatureType, Int64> GetBasicNatureList(PetInfo info, int level)
        {
            Dictionary<NatureType, Int64> NatureList = new Dictionary<NatureType, Int64>();
            Dictionary<NatureType, double> tempList = new Dictionary<NatureType, double>();

            Dictionary<NatureType, float> basicNatureList = PetLibrary.GetPetBasicNatureList(info.PetId);
            if (basicNatureList == null)
            {
                Log.WarnLine("player {0} pet {1} InitPetNatureInfo GetPetBasicNatureList is null, pet id {2}", owner.Uid, info.PetUid, info.PetId);
            }
            Dictionary<NatureType, float> basicGrowthNatures = PetLibrary.GetPetBasicGrowthNatures(info.PetId);
            if (basicGrowthNatures == null)
            {
                Log.WarnLine("player {0} pet {1} InitPetNatureInfo GetPetBasicGrowthNatures is null, pet id {2}", owner.Uid, info.PetUid, info.PetId);
            }
            Dictionary<NatureType, float> growthFactors = PetLibrary.GetPetNatureGrowthFactors(info.PetId, info.BreakLevel);
            if (growthFactors == null)
            {
                Log.WarnLine("player {0} pet {1} InitPetNatureInfo GetPetNatureGrowthFactors is null, pet id {2}", owner.Uid, info.PetUid, info.PetId);
            }

            //基础成长属性
            if (basicGrowthNatures != null)
            {
                foreach (var basic in basicGrowthNatures)
                {
                    tempList[basic.Key] = basic.Value;
                }
            }
            //属性成长系数
            if (growthFactors != null)
            {
                foreach (var growth in growthFactors)
                {
                    double value;
                    tempList.TryGetValue(growth.Key, out value);
                    tempList[growth.Key] = growth.Value * value * (level - 1);
                }
            }
            //基础属性
            if (basicNatureList != null)
            {
                foreach (var added in basicNatureList)
                {
                    double value;
                    tempList.TryGetValue(added.Key, out value);
                    tempList[added.Key] = added.Value + value;
                }
            }

            if (tempList.Count > 0)
            {
                foreach (var temp in tempList)
                {
                    NatureList[temp.Key] = (Int64)temp.Value;
                }
            }

            return NatureList;
        }

        public void UpdateBattlePower(PetInfo info)
        {
            //计算战力
            Natures caculateNatures = GetPetNaturesForCaculateBattlePower(info.Nature, info.Aptitude);

            int power = ScriptManager.BattlePower.CaculateBattlePower(caculateNatures);
            //提升基础战力百分比         
            //power += CaculateSkillBattleAndSoulRingPower(info);          
            //更新战力
            info.UpdateBattlePower(power);

            //if (owner.ArenaMng.DefensiveHeros.ContainsKey(info.Id))
            //{
            //    owner.UpdateDefensivePower();
            //}

            //if (info.CrossQueueNum > 0)
            //{
            //    owner.SyncCrossHeroQueueMsg(0, 0);
            //}

            //if (info.DefensiveQueueNum > 0)
            //{
            //    owner.UpdateFortDefensiveQueue();
            //}

            //if (info.CrossBossQueueNum > 0)
            //{
            //    owner.SyncCrosBossHeroQueuMsg(ChallengeIntoType.CrossBossReturn);
            //}

            //if (info.CrossChallengeQueueNum > 0)
            //{
            //    owner.SyncCrossChallengeHeroQueueMsg(0, 0);
            //}
        }

        /// <summary>
        /// 获取用于计算宠物战力的属性
        /// </summary>
        /// <param name="natures"></param>
        /// <param name="aptitude"></param>
        /// <returns></returns>
        private Natures GetPetNaturesForCaculateBattlePower(Natures natures, int aptitude)
        {
            Natures caculateNattures = new Natures();
            Dictionary<NatureType, NatureItem> natureList = natures.GetNatureList();
            foreach (var nature in natureList)
            {
                long natureVal = natures.GetNatureValue(nature.Key);
                //宠物自身属性 + 宠物自身属性 * 资质转化比 * 小队最大人数
                natureVal += (long)(natureVal * 0.01f * aptitude * 5);
                caculateNattures.AddNatureBaseValue(nature.Key, natureVal);
            }
            return caculateNattures;
        }

        public void LevelUp(PetInfo petInfo, int upLevel)
        {
            petInfo.LevelUp(upLevel);
        }

        public void LevelUpUpdateNature(PetInfo petInfo, int upLevel)
        {
            Dictionary<NatureType, Int64> oldNatures = GetBasicNatureList(petInfo, petInfo.Level - upLevel);
            Dictionary<NatureType, Int64> newNatures = GetBasicNatureList(petInfo, petInfo.Level);
            petInfo.LevelUpUpdateNature(newNatures, oldNatures);

            petInfo.InitHp();
            UpdateBattlePower(petInfo);
        }

        public int GetPetBattlePower()
        {
            int battlePower = 0;
            if (OnFightPet > 0)
            {
                PetInfo petInfo = GetPetInfo(OnFightPet);
                if (petInfo != null)
                {
                    battlePower = petInfo.GetBattlePower();
                }
            }
            return battlePower;
        }

        /// <summary>
        /// 弃用
        /// </summary>      
        public void AddNatureRatioOrValue(PetInfo petInfo, Dictionary<NatureType, int> natureRatios, Dictionary<NatureType, int> natureValues)
        {
            int skillLevel = petInfo.GetSkillLevel();
            int growth = PetLibrary.GetPetPassiveSkillGrowth(skillLevel);

            AddNatureRatio(petInfo, natureRatios, growth);
            AddNatureValue(petInfo, natureValues);
            //最后设置
            petInfo.InitHp();
            //计算战力
            UpdateBattlePower(petInfo);
        }

        private void AddNatureRatio(PetInfo petInfo, Dictionary<NatureType, int> natureRatios, int growth = 1)
        {
            if (natureRatios.Count == 0)
            {
                return;
            }
            foreach (var nature in natureRatios)
            {
                long baseValue = petInfo.GetNatureBaseValue(nature.Key);
                petInfo.AddNatureAddedValue(nature.Key, (long)(baseValue * (0.0001f * nature.Value * growth)));
            }
        }

        private void AddNatureValue(PetInfo petInfo, Dictionary<NatureType, int> natureValues)
        {
            if (natureValues.Count == 0)
            {
                return;
            }
            foreach (var nature in natureValues)
            {
                petInfo.AddNatureAddedValue(nature.Key, nature.Value);
            }
        }

        /// <summary>
        /// 弃用
        /// </summary>
        public void SubNatureRatioOrValue(PetInfo petInfo, Dictionary<NatureType, int> natureRatios, Dictionary<NatureType, int> natureValues)
        {
            SubNatureRatio(petInfo, natureRatios);
            SubNatureValue(petInfo, natureValues);
            //TODO：暂时不需要
            //最后设置
            //petInfo.InitHp();
            //计算战力
            //UpdateBattlePower(petInfo);
        }

        private void SubNatureRatio(PetInfo petInfo, Dictionary<NatureType, int> natureRatios)
        {
            if (natureRatios.Count == 0)
            {
                return;
            }
            foreach (var nature in natureRatios)
            {
                long baseValue = petInfo.GetNatureBaseValue(nature.Key);
                petInfo.AddNatureAddedValue(nature.Key, (long)(-1 * baseValue * (0.0001f * nature.Value)));
            }
        }

        private void SubNatureValue(PetInfo petInfo, Dictionary<NatureType, int> natureValues)
        {
            if (natureValues.Count == 0)
            {
                return;
            }
            foreach (var nature in natureValues)
            {
                petInfo.AddNatureAddedValue(nature.Key, nature.Value * (-1));
            }
        }

        private void PassiveSkillsBonusNature(PetInfo petInfo)
        {
            int skillLevel = petInfo.GetSkillLevel();
            int growth = PetLibrary.GetPetPassiveSkillGrowth(skillLevel);
            
            foreach (var kv in petInfo.PassiveSkills)
            {
                int skillId = kv.Value;
                PetPassiveSkillModel skillModel = PetLibrary.GetPetPassiveSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }
                AddNatureRatio(petInfo, skillModel.AddNatureRatios, growth);
                //AddNatureValue(petInfo, skillModel.AddNatureValues);
                AddNatureRatio(petInfo, skillModel.ExtraNatureRatios);
            }
        }
        #endregion

        #region 宠物副本阵容

        #region 主战阵容上阵
        public void UpdateMainQueuePetInfo(MainBattleQueueInfo queueInfo, ulong petUid, bool remove)
        {
            if (!remove)
            {
                queueInfo.PetUid = petUid;
            }
            else
            {
                queueInfo.PetUid = 0;
            }
            SyncDbUpdateMainBattleQueuePetInfo(queueInfo);
        }

        public void SetMainQueueOnFightPet(ulong petUid)
        {
            OnFightPet = petUid;
        }

        public void CheckSetMainQueuePet(MainBattleQueueInfo queueInfo)
        {
            if (queueInfo.BattleState == 1 && queueInfo.PetUid > 0)
            {
                SetMainQueueOnFightPet(queueInfo.PetUid);
            }
        }

        public PetInfo GetInQueuePetInfo(MapType mapType, HeroInfo heroInfo, int dungeonId)
        {
            switch (mapType)
            {
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.IntegralBoss:
                case MapType.HuntingActivityTeam:
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                    PetInfo petInfo = GetPetInfo(OnFightPet);
                    return petInfo;
                case MapType.ThemeBoss:
                    return GetDungeonQueuePet(DungeonQueueType.ThemeBoss, heroInfo.ThemeBossQueueNum);
                case MapType.CarnivalBoss:
                    return GetDungeonQueuePet(DungeonQueueType.CarnivalBoss, heroInfo.CarnivalBossQueueNum);
                case MapType.HuntingIntrude:
                    return GetDungeonQueuePet(DungeonQueueType.HuntingIntrude, 1);
                case MapType.Tower:
                    return GetDungeonQueuePet(DungeonQueueType.Tower, 1);
                case MapType.IslandChallenge:
                    IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(owner.IslandChallengeManager.Task.TaskInfo.param[0]);
                    if (model == null || !model.Dungeon2Queue.ContainsKey(dungeonId)) return null;
                    return GetDungeonQueuePet(DungeonQueueType.IslandChallenge, model.Dungeon2Queue[dungeonId]);
                case MapType.SpaceTimeTower:
                    return GetDungeonQueuePet(DungeonQueueType.SpaceTimeTower, 1);
                default:
                    break;
            }
            return null;
        }

        public int GetPetNatureBonusRatio(PetInfo petInfo)
        {
            int ratio = PetLibrary.GetPetAptitudeBonusNatureRatio(petInfo.Aptitude);
            int curSatiety = CheckUpdateSatiety(petInfo, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now));
            PetSatietyModel satietyModel = PetLibrary.GetPetSatietyModel(curSatiety);
            if (satietyModel != null)
            {
                ratio += satietyModel.NatureBonusRatio;
            }
            return ratio;
        }

        private void SyncDbUpdateMainBattleQueuePetInfo(MainBattleQueueInfo queueInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdateMainBattleQueuePetInfo(owner.Uid, queueInfo.QueueNum, queueInfo.PetUid));
        }

        #endregion

        #region 副本阵容
        public void AddPetDungeonQueueInfo(DungeonQueueType queueType, List<ulong> pets)
        {
            Dictionary<int, PetInfo> petDic = new Dictionary<int, PetInfo>();
            for (int i = 0; i < pets.Count; i++)
            {
                PetInfo petInfo = GetPetInfo(pets[i]);
                if (petInfo != null)
                {
                    petDic.Add(i + 1, petInfo);
                }
            }
            if (dungeonQueueList.ContainsKey(queueType))
            {
                dungeonQueueList[queueType] = petDic;
            }
            else
            {
                dungeonQueueList.Add(queueType, petDic);
            }
        }
        
        public Dictionary<DungeonQueueType, Dictionary<int, PetInfo>> GetPetDungeonQueueList()
        {
            return dungeonQueueList;
        }

        public Dictionary<int, PetInfo> GetDungeonQueuePetsInfo(DungeonQueueType queueType)
        {
            Dictionary<int, PetInfo> queuePetList;
            dungeonQueueList.TryGetValue(queueType, out queuePetList);
            return queuePetList;
        }

        public PetInfo GetDungeonQueuePet(DungeonQueueType queueType, int queueNum)
        {
            PetInfo petInfo;
            Dictionary<int, PetInfo> queuePetList;
            if (dungeonQueueList.TryGetValue(queueType, out queuePetList) && queuePetList.TryGetValue(queueNum, out petInfo))
            {
                return petInfo;
            }
            return null;
        }

        public bool UpdateDungeonQueuePetInfo(DungeonQueueType queueType, int queueNum, PetInfo petInfo)
        {
            Dictionary<int, PetInfo> petList;
            if (dungeonQueueList.TryGetValue(queueType, out petList))
            {
                PetInfo oldPet;
                if (petList.TryGetValue(queueNum, out oldPet) && oldPet.PetUid == petInfo.PetUid)
                {
                    return false;
                }
                petList[queueNum] = petInfo;
                SyncDbUpdatePetDungeonQueueInfo((int)queueType, queueNum, petInfo.PetUid);
            }
            else
            {
                petList = new Dictionary<int, PetInfo>();
                petList.Add(queueNum, petInfo);
                dungeonQueueList.Add(queueType, petList);
                SyncDbInsertPetDungeonQueueInfo((int)queueType, queueNum, petInfo.PetUid);
            }
            return true;
        } 

        public bool RemoveDungeonQueuePet(DungeonQueueType queueType, int queueNum)
        {
            Dictionary<int, PetInfo> queuePetList;
            dungeonQueueList.TryGetValue(queueType, out queuePetList);
            if (queuePetList == null)
            {
                return false;
            }
            PetInfo pet;
            if (queuePetList.TryGetValue(queueNum, out pet))
            {
                queuePetList.Remove(queueNum);
                SyncDbUpdatePetDungeonQueueInfo((int)queueType, queueNum, 0);
                return true;
            }
            return false;
        }
 
        public Dictionary<int, PetInfo> GetIslandDungeonPet(int dungeonId)
        {
            Dictionary<int, PetInfo> queuePet = new Dictionary<int, PetInfo>();
            if (owner.IslandChallengeManager.Task == null || owner.IslandChallengeManager.Task.Type != TowerTaskType.Dungeon) return queuePet;

            IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(owner.IslandChallengeManager.Task.TaskInfo.param[0]);
            if (model == null || !model.Dungeon2Queue.ContainsKey(dungeonId)) return queuePet;
            Dictionary<int, PetInfo> petDic;
            PetInfo petInfo;
            if (!dungeonQueueList.TryGetValue(DungeonQueueType.IslandChallenge, out petDic) || !petDic.TryGetValue(model.Dungeon2Queue[dungeonId], out petInfo))
            {
                return queuePet;
            }
            queuePet.Add(model.Dungeon2Queue[dungeonId], petInfo);
            return queuePet;
        }

        private void SyncDbInsertPetDungeonQueueInfo(int queueType, int queueNum, ulong petUid)
        {
            owner.server.GameDBPool.Call(new QueryInsertPetDungeonQueueInfo(owner.Uid, queueType, queueNum, petUid));
        }

        private void SyncDbUpdatePetDungeonQueueInfo(int queueType, int queueNum, ulong petUid)
        {
            owner.server.GameDBPool.Call(new QueryUpdatePetDungeonQueueInfo(owner.Uid, queueType, queueNum, petUid));
        }
        #endregion

        #endregion

        #region 宠物蛋
        public bool PetEggsFull()
        {
           return petEggList.Count >= PetLibrary.PetConfig.EggMaxNum;
        }

        public int GetPetEggBagRestSpace()
        {
            return PetLibrary.PetConfig.EggMaxNum - petEggList.Count;
        }

        public PetEggItem AddPetEggItem(int id)
        {
            PetEggModel model = PetLibrary.GetPetEggModel(id);
            if (model == null)
            {
                return null;
            }
            ulong eggUid = owner.server.UID.NewPuid(owner.server.MainId, owner.server.SubId);
            PetEggItem eggItem = new PetEggItem(eggUid, model.Id);

            if (PetEggsFull())
            {
                owner.BagManager.SendItem2Mail((int)RewardType.Pet, eggItem.Id, 1);
                return null;
            }

            if (AddPetEggItem(eggItem))
            {
                InsertPetEggItemToDb(eggItem);
                return eggItem;
            }
            else
            {
                return null;
            }
        }

        public bool AddPetEggItem(PetEggItem item)
        {
            PetEggItem temp;
            if (petEggList.TryGetValue(item.Uid, out temp))
            {
                return false;
            }
            petEggList.Add(item.Uid, item);
            return true;
        }

        public void BindTransformPetEggItem(PetEggItem item)
        {
            if (petEggList.ContainsKey(item.Uid))
            {
                petEggList[item.Uid] = item;
            }
            else
            {
                petEggList.Add(item.Uid, item);
            }
            AddHatchPetEgg(item);
        }

        public void AddHatchPetEgg(PetEggItem item)
        {
            if (item.HatchStartTime > 0)
            {
                hatchList.Add(item.Uid, item);
            }          
        }

        public Dictionary<ulong, PetEggItem> GetPetEggList()
        {
            return petEggList;
        }

        public PetEggItem GetPetEggItem(ulong uid)
        {
            PetEggItem item;
            petEggList.TryGetValue(uid, out item);
            return item;
        }

        public void SetPetEggHatchStartTime(PetEggItem item)
        {
            int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            item.SetHatchStartTime(time);
            UpdatePetEggItemToDb(item);
        }

        public int GetHatchCount()
        {
            return hatchList.Count;
        }

        public void RemovePetEggItem(ulong uid)
        {
            petEggList.Remove(uid);
            DeletePetEggItemFromDb(uid);
        }

        public void RemoveHatchPetEgg(ulong uid)
        {
            hatchList.Remove(uid);
        }  

        private void InsertPetEggItemToDb(PetEggItem item)
        {
            owner.server.GameDBPool.Call(new QueryInsertPetEggItem(owner.Uid, item.Uid, item.Id));
        }

        private void UpdatePetEggItemToDb(PetEggItem item)
        {
            owner.server.GameDBPool.Call(new QueryUpdatePetEggItem(item.Uid, item.HatchStartTime));
        }

        private void DeletePetEggItemFromDb(ulong uid)
        {
            owner.server.GameDBPool.Call(new QueryDeltePetEggItem(uid));
        }
        #endregion

        #region 宠物养成
        public void PetInherit(PetInfo fromPet, PetInfo toPet)
        {
            //等级互换
            PetsSwapLevel(fromPet, toPet);
        }

        private void PetsSwapLevel(PetInfo fromPet, PetInfo toPet)
        {
            int toOldLevel = toPet.Level;
            toPet.SetLevel(fromPet.Level);
            fromPet.SetLevel(toOldLevel);
        }

        public void SetPasiveSkill(PetInfo petInfo, int slot, int skillId)
        {
            petInfo.SetPasiveSkill(slot, skillId);
            SyncDbUpdatePetPassiveSkill(petInfo);
            InitPetNatureInfo(petInfo);
        }

        public void PetBreak(PetInfo petInfo, PetBreakModel breakModel)
        {
            petInfo.BreakLevelUp();
            InbornSkillLevelUp(petInfo, breakModel.InbornSkillLevelUp);
            UnlockPassiveSkill(petInfo, breakModel.UnlockPassiveSkill);
            InitPetNatureInfo(petInfo);
        }

        private void InbornSkillLevelUp(PetInfo petInfo, int addLevel)
        {
            if (addLevel <= 0)
            {
                return;
            }
            PetInbornSkillModel skill1Model = PetLibrary.GetPetInbornSkillModel(1, petInfo.InbornSkillsLevel[1]);
            PetInbornSkillModel skill2Model = PetLibrary.GetPetInbornSkillModel(2, petInfo.InbornSkillsLevel[2]);
            int weight = 0;
            if (skill1Model != null)
            {
                weight += skill1Model.LevelUpWeight;
            }
            if (skill2Model != null)
            {
                weight += skill2Model.LevelUpWeight;
            }
            int random = NewRAND.Next(1, weight);//
            if (random <= skill1Model?.LevelUpWeight)
            {
                petInfo.SkillLevelUp(1, addLevel);
            }
            else
            {
                petInfo.SkillLevelUp(2, addLevel);
            }
        }

        private void UnlockPassiveSkill(PetInfo petInfo, int unlockSkillNum)
        {
            int lastSlot = petInfo.PassiveSkills.Count;
            for (int i = 1; i <= unlockSkillNum; i++)
            {
                int slot = lastSlot + i;
                PetPassiveSkillModel skillModel = PetLibrary.RandomPetSlotPassiveSkill(slot);
                if (skillModel != null)
                {
                    petInfo.SetPasiveSkill(slot, skillModel.Id);
                    petInfo.AddPassiveSkill(skillModel.Id);
                }
            }
        }

        public void PetBlend(PetInfo mainPet, PetInfo blendPet)
        {
            int mainSkillNum = PetLibrary.GetInitPassiveSkillNum(mainPet.Aptitude);
            int blendSkillNum = PetLibrary.GetInitPassiveSkillNum(blendPet.Aptitude);
            if (blendSkillNum > mainSkillNum)
            {
                //解锁新技能槽
                UnlockPassiveSkill(mainPet, blendSkillNum - mainSkillNum);
            }
            //资质提升
            mainPet.SetAptitude(blendPet.Aptitude);
            InitPetNatureInfo(mainPet);
        }

        public void PetFeed(PetInfo petInfo, float shapeChange, int addSatiety, int curFeedTime, int foodId)
        {
            SetShape(petInfo, shapeChange);
            SetSatiety(petInfo, addSatiety, curFeedTime, foodId);
            petInfo.SetLastFeedTime(curFeedTime);
            petInfo.SetSatietyUpdateTime(curFeedTime);
            SyncDbUpdatePetFeedInfo(petInfo);
        }

        private void SetShape(PetInfo petInfo, float shapeChange)
        {
            float curShape = petInfo.Shape;
            decimal result = Convert.ToDecimal(curShape) + Convert.ToDecimal(shapeChange);
            float temp = Convert.ToSingle(string.Format("{0:N2}", result));
            if (shapeChange < 0.00f)
            {
                curShape = temp >= PetLibrary.PetConfig.MinShape ? temp : PetLibrary.PetConfig.MinShape;
            }
            else if (shapeChange > 0.00f)
            {
                curShape = temp <= PetLibrary.PetConfig.MaxShape ? temp : PetLibrary.PetConfig.MaxShape;
            }
            petInfo.SetShape(curShape);
        }

        private void SetSatiety(PetInfo petInfo, int addSatiety, int curFeedTime, int foodId)
        {
            //需根据上次喂养时间获取当前真实饱食度
            int curSatiety = GetCurSatietyByTime(petInfo, curFeedTime);
            //before用于bi埋点
            int before = curSatiety;
            int temp = curSatiety + addSatiety;
            PetSatietyModel oldSatietyModel = PetLibrary.GetPetSatietyModel(curSatiety);
            curSatiety = temp <= PetLibrary.PetConfig.MaxSatiety ? temp : PetLibrary.PetConfig.MaxSatiety;
            petInfo.SetSatiety(curSatiety);
            PetSatietyModel curSatietyModel = PetLibrary.GetPetSatietyModel(curSatiety);
            if (oldSatietyModel?.Id != curSatietyModel?.Id)
            {
                petInfo.SetSatietyProtectFlag(false);
            }
            //BI埋点
            owner.BIRecordPetDevelopLog("feed", petInfo, petInfo.Satiety - before, foodId, 1);
        }

        public int CheckUpdateSatiety(PetInfo petInfo, int curTime)
        {
            //需根据上次喂养时间获取当前真实饱食度
            int curSatiety = GetCurSatietyByTime(petInfo, curTime);
            if (curSatiety != petInfo.Satiety)
            {
                petInfo.SetSatiety(curSatiety);
                petInfo.SetSatietyUpdateTime(curTime);
                SyncDbUpdatePetFeedInfo(petInfo);
            }
            return curSatiety;
        }

        private int GetCurSatietyByTime(PetInfo petInfo, int curTime)
        {
            int curSatiety = 0;
            PetSatietyModel satietyModel = PetLibrary.GetPetSatietyModel(petInfo.Satiety);
            if (satietyModel == null)
            {
                return curSatiety;
            }
            curSatiety = GetCurSatietyByDuration(petInfo, petInfo.Satiety, curTime - petInfo.LastFeedTime, curTime, satietyModel);       
            return curSatiety;
        }

        private int GetCurSatietyByDuration(PetInfo petInfo, int satiety, int duration, int curTime, PetSatietyModel satietyModel)
        {
            int curSatiety;
            //饱食度减少到临界值需要的时间
            int subSatiety = satiety - (satietyModel.MinSatiety - 1);
            double hours = Math.Round(((double)subSatiety) / satietyModel.DeclinePerHour, 1);
            int subTime = (int)(hours * 3600);

            int beyondProTime = duration - satietyModel.ProtectionTime;
            //超出保护期
            if (beyondProTime > 0)
            {
                //上次饱食度更新时间已超过保护期
                if (petInfo.SatietyUpdateTime > petInfo.LastFeedTime + satietyModel.ProtectionTime)
                {
                    if (!petInfo.SatietyProtectFlag)
                    {
                        beyondProTime = curTime - petInfo.SatietyUpdateTime - satietyModel.ProtectionTime;
                        beyondProTime = beyondProTime > 0 ? beyondProTime : 0;
                        petInfo.SetSatietyProtectFlag(true);
                    }
                    else
                    {
                        beyondProTime = curTime - petInfo.SatietyUpdateTime;
                    }
                }
                PetSatietyModel lowSatietyModel = PetLibrary.GetPetSatietyModel(satietyModel.MinSatiety-1);
                //说明当前已是最低饱食度阶段
                if (lowSatietyModel == null)
                {
                    curSatiety = CalculateSatiety(satiety, beyondProTime, satietyModel.DeclinePerHour);
                    return curSatiety;
                }
                //进入到下一饱食度阶段
                if (beyondProTime > subTime + lowSatietyModel.ProtectionTime)
                {
                    petInfo.SetSatietyProtectFlag(false);
                    curSatiety = GetCurSatietyByDuration(petInfo, lowSatietyModel.MaxSatiety, beyondProTime - subTime, curTime, lowSatietyModel);
                }
                //没有进入到下一饱食度阶段
                else if (beyondProTime < subTime)
                {
                    curSatiety = CalculateSatiety(satiety, beyondProTime, satietyModel.DeclinePerHour);
                }
                //在下一饱食度阶段保护期
                else
                {
                    petInfo.SetSatietyProtectFlag(false);
                    curSatiety = lowSatietyModel.MaxSatiety;
                }
            }
            else
            {
                curSatiety = satiety;
            }
            return curSatiety;
        }

        private int CalculateSatiety(int satiety, int beyondProTime, int declinePerHour)
        {
            double hours = Math.Round(((double)beyondProTime) / 3600, 1);
            int tempSatiety = satiety - (int)(hours * declinePerHour);
            int curSatiety = tempSatiety > 0 ? tempSatiety : 0;
            return curSatiety;
        }

        private void SyncDbUpdatePetPassiveSkill(PetInfo petInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdatePetPassiveSkill(petInfo.PetUid, petInfo.PassiveSkills));
        }

        private void SyncDbUpdatePetFeedInfo(PetInfo petInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdatePetFeedInfo(petInfo.PetUid, petInfo.Shape, petInfo.Satiety, petInfo.LastFeedTime, petInfo.SatietyUpdateTime, petInfo.SatietyProtectFlag));
        }
        #endregion
    }
}

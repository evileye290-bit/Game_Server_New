using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class PetLibrary
    {
        private static PetConfig petConfig = new PetConfig();
        public static PetConfig PetConfig { get { return petConfig; } }

        private static Dictionary<int, PetModel> petList = new Dictionary<int, PetModel>();
        private static Dictionary<int, PetEggModel> petEggList = new Dictionary<int, PetEggModel>();
        private static Dictionary<int, SortedDictionary<int, string>> petReleaseRewards = new Dictionary<int, SortedDictionary<int, string>>();
        private static Dictionary<int, NatureDataModel> petBasicNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, NatureDataModel> petBasicGrowthNatures = new Dictionary<int, NatureDataModel>();
        private static DoubleDepthMap<int, int, NatureDataModel> petNatureGrowthFactor = new DoubleDepthMap<int, int, NatureDataModel>();
        private static Dictionary<int, int> petLevelUpSoulPowers = new Dictionary<int, int>();
        private static SortedDictionary<int, int> petAptitudeBonusList = new SortedDictionary<int, int>();
        private static DoubleDepthMap<int, int, PetInbornSkillModel> petInbornSkills = new DoubleDepthMap<int, int, PetInbornSkillModel>();
        private static Dictionary<int, PetBreakModel> petBreakList = new Dictionary<int, PetBreakModel>();
        private static Dictionary<int, PetPassiveSkillModel> petPassiveSkillsLib = new Dictionary<int, PetPassiveSkillModel>();
        private static DoubleDepthMap<int, int, PetPassiveSkillModel> petSlotPassiveSkills = new DoubleDepthMap<int, int, PetPassiveSkillModel>();
        private static Dictionary<int, int> slotSkillsTotalWeight = new Dictionary<int, int>();
        private static SortedDictionary<int, int> petSkillNum = new SortedDictionary<int, int>();
        private static Dictionary<int, PetFoodModel> petFoodList = new Dictionary<int, PetFoodModel>();
        private static SortedDictionary<int, PetSatietyModel> petSatietyList = new SortedDictionary<int, PetSatietyModel>();
        private static Dictionary<int, int> passiveSkillGrowth = new Dictionary<int, int>();
        private static Dictionary<int, int> inbornSkillGrowth = new Dictionary<int, int>();
        private static Dictionary<int, int> dungeonQueueNum = new Dictionary<int, int>();

        public static void Init()
        {
            InitPet();
            InitPetEgg();
            InitPetConfig();
            InitPetReleaseRewards();
            InitPetBasicNature();
            InitPetBasicGrowthNature();
            InitPetNatureGrowthFactor();
            InitPetLevelUp();
            InitPetAptitudeBonusNature();
            InitPetInbornSkill();
            InitPetBreak();
            InitPetPassiveSkillLibrary();
            InitPetSkillNum();
            InitPetFood();
            InitPetSatiety();
            InitPetSkillGrowth();
            InitPetDungeonQueueNum();
        }

        private static void InitPet()
        {
            //petList.Clear();
            Dictionary<int, PetModel> petList = new Dictionary<int, PetModel>();

            DataList dataList = DataListManager.inst.GetDataList("Pet");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petList.ContainsKey(item.Key))
                {
                    PetModel petModel = new PetModel(data);
                    petList.Add(item.Key, petModel);
                }
            }
            PetLibrary.petList = petList;
        }

        private static void InitPetEgg()
        {
            Dictionary<int, PetEggModel> petEggList = new Dictionary<int, PetEggModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetEgg");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petEggList.ContainsKey(item.Key))
                {
                    PetEggModel petEggModel = new PetEggModel(data);
                    petEggList.Add(item.Key, petEggModel);
                }
            }
            PetLibrary.petEggList = petEggList;
        }

        private static void InitPetConfig()
        {
            PetConfig petConfig = new PetConfig();

            Data data = DataListManager.inst.GetData("PetConfig", 1);
            petConfig.Init(data);

            PetLibrary.petConfig = petConfig;
        }

        private static void InitPetReleaseRewards()
        {
            Dictionary<int, SortedDictionary<int, string>> petReleaseRewards = new Dictionary<int, SortedDictionary<int, string>>();

            DataList dataList = DataListManager.inst.GetDataList("PetRelease");
            SortedDictionary<int, string> rewardsDic;
            Dictionary<int, int> weightDic = new Dictionary<int, int>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int breakLevel = data.GetInt("BreakLevel");
                int weight = data.GetInt("Weight");
                string rewards = data.GetString("Rewards");

                if (!weightDic.ContainsKey(breakLevel))
                {
                    weightDic.Add(breakLevel, weight);
                }
                else
                {
                    weightDic[breakLevel] += weight;
                }

                int totalWeight = weightDic[breakLevel];
                if (!petReleaseRewards.TryGetValue(breakLevel, out rewardsDic))
                {
                    rewardsDic = new SortedDictionary<int, string>();                   
                    petReleaseRewards.Add(breakLevel, rewardsDic);
                }
                if (!rewardsDic.ContainsKey(totalWeight))
                {
                    rewardsDic.Add(totalWeight, rewards);
                }//debug检查下
            }
            weightDic.Clear();

            PetLibrary.petReleaseRewards = petReleaseRewards;
        }

        private static void InitPetBasicNature()
        {
            Dictionary<int, NatureDataModel> petBasicNatureList = new Dictionary<int, NatureDataModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetBasicNature");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petBasicNatureList.ContainsKey(data.ID))
                {
                    petBasicNatureList.Add(data.ID, new NatureDataModel(data));
                }
            }

            PetLibrary.petBasicNatureList = petBasicNatureList;
        }

        private static void InitPetBasicGrowthNature()
        {
            Dictionary<int, NatureDataModel> petBasicGrowthNatures = new Dictionary<int, NatureDataModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetBasicGrowthNature");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petBasicGrowthNatures.ContainsKey(data.ID))
                {
                    petBasicGrowthNatures.Add(data.ID, new NatureDataModel(data));
                }
            }

            PetLibrary.petBasicGrowthNatures = petBasicGrowthNatures;
        }

        private static void InitPetNatureGrowthFactor()
        {
            DoubleDepthMap<int, int, NatureDataModel> petNatureGrowthFactor = new DoubleDepthMap<int, int, NatureDataModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetNatureGrowthFactor");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                petNatureGrowthFactor.Add(data.GetInt("PetId"), data.GetInt("BreakLevel"), new NatureDataModel(data));
            }

            PetLibrary.petNatureGrowthFactor = petNatureGrowthFactor;
        }

        private static void InitPetLevelUp()
        {
            Dictionary<int, int> petLevelUpSoulPowers = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetLevelUp");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petLevelUpSoulPowers.ContainsKey(data.ID))
                {
                    petLevelUpSoulPowers.Add(data.ID, data.GetInt("SoulPower"));
                }
            }

            PetLibrary.petLevelUpSoulPowers = petLevelUpSoulPowers;
        }

        private static void InitPetAptitudeBonusNature()
        {
            SortedDictionary<int, int> petAptitudeBonusList = new SortedDictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetAptitudeBonusNature");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int key = data.GetInt("MinAptitude");
                if (!petAptitudeBonusList.ContainsKey(key))
                {
                    petAptitudeBonusList.Add(key, data.GetInt("BonusRatio"));
                }
            }

            PetLibrary.petAptitudeBonusList = petAptitudeBonusList;
        }

        private static void InitPetInbornSkill()
        {
            DoubleDepthMap<int, int, PetInbornSkillModel> petInbornSkills = new DoubleDepthMap<int, int, PetInbornSkillModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetInbornSkill");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                PetInbornSkillModel model = new PetInbornSkillModel(data);
                petInbornSkills.Add(model.SkillSlot, model.Level, model);
            }

            PetLibrary.petInbornSkills = petInbornSkills;
        }

        private static void InitPetBreak()
        {
            Dictionary<int, PetBreakModel> petBreakList = new Dictionary<int, PetBreakModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetBreak");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petBreakList.ContainsKey(item.Key))
                {
                    PetBreakModel breakModel = new PetBreakModel(data);
                    petBreakList.Add(item.Key, breakModel);
                }
            }
            PetLibrary.petBreakList = petBreakList;
        }

        private static void InitPetPassiveSkillLibrary()
        {
            Dictionary<int, PetPassiveSkillModel> petPassiveSkillsLib = new Dictionary<int, PetPassiveSkillModel>();
            DoubleDepthMap<int, int, PetPassiveSkillModel> petSlotPassiveSkills = new DoubleDepthMap<int, int, PetPassiveSkillModel>();
            Dictionary<int, int> slotSkillsTotalWeight = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetPassiveSkillLibrary");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                PetPassiveSkillModel skillLibModel = new PetPassiveSkillModel(data);
                if (!petPassiveSkillsLib.ContainsKey(item.Key))
                {
                    petPassiveSkillsLib.Add(item.Key, skillLibModel);
                }
                if (slotSkillsTotalWeight.ContainsKey(skillLibModel.Slot))
                {
                    slotSkillsTotalWeight[skillLibModel.Slot] += skillLibModel.Weight;
                }
                else
                {
                    slotSkillsTotalWeight.Add(skillLibModel.Slot, skillLibModel.Weight);
                }
                petSlotPassiveSkills.Add(skillLibModel.Slot, slotSkillsTotalWeight[skillLibModel.Slot], skillLibModel);
            }
            PetLibrary.petPassiveSkillsLib = petPassiveSkillsLib;
            PetLibrary.petSlotPassiveSkills = petSlotPassiveSkills;
            PetLibrary.slotSkillsTotalWeight = slotSkillsTotalWeight;
        }

        private static void InitPetSkillNum()
        {
            SortedDictionary<int, int> petSkillNum = new SortedDictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetSkillNum");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int key = data.GetInt("MinAptitude");
                if (!petSkillNum.ContainsKey(key))
                {
                    petSkillNum.Add(key, data.GetInt("PassiveSkillNum"));
                }
            }

            PetLibrary.petSkillNum = petSkillNum;
        }

        private static void InitPetFood()
        {
            Dictionary<int, PetFoodModel> petFoodList = new Dictionary<int, PetFoodModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetFood");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!petFoodList.ContainsKey(item.Key))
                {
                    PetFoodModel foodModel = new PetFoodModel(data);
                    petFoodList.Add(item.Key, foodModel);
                }
            }
            PetLibrary.petFoodList = petFoodList;
        }

        private static void InitPetSatiety()
        {
            SortedDictionary<int, PetSatietyModel> petSatietyList = new SortedDictionary<int, PetSatietyModel>();

            DataList dataList = DataListManager.inst.GetDataList("PetSatiety");
            foreach (var item in dataList)
            {
                PetSatietyModel model = new PetSatietyModel(item.Value);
                if (!petSatietyList.ContainsKey(model.MinSatiety))
                {
                    petSatietyList.Add(model.MinSatiety, model);
                }
            }

            PetLibrary.petSatietyList = petSatietyList;
        }

        private static void InitPetSkillGrowth()
        {
            Dictionary<int, int> passiveSkillGrowth = new Dictionary<int, int>();
            Dictionary<int, int> inbornSkillGrowth = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetSkillGrowth");
            foreach (var item in dataList)
            {
                passiveSkillGrowth[item.Key] = item.Value.GetInt("PassiveSkillGrowth");
                inbornSkillGrowth[item.Key] = item.Value.GetInt("InbornSkillGrowth");
            }
            PetLibrary.passiveSkillGrowth = passiveSkillGrowth;
            PetLibrary.inbornSkillGrowth = inbornSkillGrowth;
        }

        private static void InitPetDungeonQueueNum()
        {
            Dictionary<int, int> dungeonQueueNum = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("PetDungeonQueueNum");
            foreach (var item in dataList)
            {
                dungeonQueueNum[item.Key] = item.Value.GetInt("QueueNum");
            }
            PetLibrary.dungeonQueueNum = dungeonQueueNum;
        }

        public static PetModel GetPetModel(int id)
        {
            PetModel model = null;
            petList.TryGetValue(id, out model);
            return model;
        }

        public static PetEggModel GetPetEggModel(int id)
        {
            PetEggModel model;
            petEggList.TryGetValue(id, out model);
            return model;
        }

        public static int RandomReleasePetRewardsWeight(int breakLevel)
        {
            SortedDictionary<int, string> rewardsDic;
            if (petReleaseRewards.TryGetValue(breakLevel, out rewardsDic))
            {
                int curWeight = 0;
                int totalWeight = rewardsDic.Last().Key;
                int randomWeight = NewRAND.Next(1, totalWeight);
                foreach (var kv in rewardsDic)
                {
                    if (randomWeight > curWeight && randomWeight <= kv.Key)
                    {
                        curWeight = kv.Key;
                        return curWeight;
                    }
                    curWeight = kv.Key;
                }
            }
            return 0;
        }

        public static string GetReleaseRewards(int breakLevel, int randomWeight)
        {
            string rewards = string.Empty;
            SortedDictionary<int, string> rewardsDic;
            if (petReleaseRewards.TryGetValue(breakLevel, out rewardsDic))
            {
                rewardsDic.TryGetValue(randomWeight, out rewards);
            }
            return rewards;
        }

        public static Dictionary<NatureType, float> GetPetBasicNatureList(int petId)
        {
            NatureDataModel model;
            petBasicNatureList.TryGetValue(petId, out model);
            if (model == null)
            {
                return null;
            }
            return model.NatureList;
        }

        public static Dictionary<NatureType, float> GetPetBasicGrowthNatures(int petId)
        {
            NatureDataModel model;
            petBasicGrowthNatures.TryGetValue(petId, out model);
            if (model == null)
            {
                return null;
            }
            return model.NatureList;
        }

        public static Dictionary<NatureType, float> GetPetNatureGrowthFactors(int petId, int breakLevel)
        {
            NatureDataModel model;
            petNatureGrowthFactor.TryGetValue(petId, breakLevel, out model);
            if (model == null)
            {
                return null;
            }
            return model.NatureList;
        }

        public static int GetPetLevelUpSoulPower(int level)
        {
            int soulPower;
            petLevelUpSoulPowers.TryGetValue(level, out soulPower);
            return soulPower;
        }

        public static int GetPetAptitudeBonusNatureRatio(int aptitude)
        {
            int bonusRatio = 0;
            foreach (var kv in petAptitudeBonusList)
            {
                if (aptitude >= kv.Key)
                {
                    bonusRatio = kv.Value;
                }
            }
            return bonusRatio;
        }

        public static PetInbornSkillModel GetPetInbornSkillModel(int skillSlot, int level)
        {
            PetInbornSkillModel model;
            petInbornSkills.TryGetValue(skillSlot, level, out model);
            return model;
        }

        public static PetBreakModel GetPetBreakModel(int breakLevel)
        {
            PetBreakModel model;
            petBreakList.TryGetValue(breakLevel, out model);
            return model;
        }

        public static PetPassiveSkillModel GetPetPassiveSkillModel(int id)
        {
            PetPassiveSkillModel model;
            petPassiveSkillsLib.TryGetValue(id, out model);
            return model;
        }

        public static PetPassiveSkillModel RandomPetSlotPassiveSkill(int slot, bool useItem, int curQuality)
        {
            if (useItem)
            {
                return RandomBetterPetSlotPassiveSkill(slot, curQuality);
            }           
            return RandomPetSlotPassiveSkill(slot);
        }

        public static PetPassiveSkillModel RandomPetSlotPassiveSkill(int slot)
        {
            int totalWeight;
            slotSkillsTotalWeight.TryGetValue(slot, out totalWeight);
            int random = NewRAND.Next(1, totalWeight);

            Dictionary<int, PetPassiveSkillModel> skillDic;
            petSlotPassiveSkills.TryGetValue(slot, out skillDic);
            if (totalWeight != 0 && skillDic != null)
            {
                int curWeight = 0;
                foreach (var kv in skillDic)
                {
                    if (random > curWeight && random <= kv.Key)
                    {
                        curWeight = kv.Key;
                        return kv.Value;
                    }
                    curWeight = kv.Key;
                }
            }
            return null;
        }

        private static PetPassiveSkillModel RandomBetterPetSlotPassiveSkill(int slot, int curQuality)
        {
            Dictionary<int, PetPassiveSkillModel> betterDic = new Dictionary<int, PetPassiveSkillModel>();
            int totalWeight = 0;

            Dictionary<int, PetPassiveSkillModel> skillDic;
            petSlotPassiveSkills.TryGetValue(slot, out skillDic);
            if (skillDic != null)
            {
                foreach (var skill in skillDic)
                {
                    if (skill.Value.Quality >= curQuality)
                    {
                        totalWeight += skill.Value.Weight;
                        betterDic.Add(totalWeight, skill.Value);
                    }
                }

                int random = NewRAND.Next(1, totalWeight);
                int curWeight = 0;
                foreach (var kv in betterDic)
                {
                    if (random > curWeight && random <= kv.Key)
                    {
                        curWeight = kv.Key;
                        return kv.Value;
                    }
                    curWeight = kv.Key;
                }
            }
            return null;
        }

        public static int GetInitPassiveSkillNum(int aptitude)
        {
            int skillNum = 0;
            foreach (var kv in petSkillNum)
            {
                if (aptitude >= kv.Key)
                {
                    skillNum = kv.Value;
                }
                else
                {
                    break;
                }
            }
            return skillNum;
        }

        public static PetFoodModel GetPetFoodModel(int id)
        {
            PetFoodModel model;
            petFoodList.TryGetValue(id, out model);
            return model;
        }

        public static PetSatietyModel GetPetSatietyModel(int satiety)
        {
            PetSatietyModel model = null;
            foreach (var kv in petSatietyList)
            {
                if (satiety >= kv.Key)
                {
                    model = kv.Value;
                }
                else
                {
                    break;
                }
            }
            return model;
        }

        public static int GetPetPassiveSkillGrowth(int level)
        {
            int growth;
            passiveSkillGrowth.TryGetValue(level, out growth);
            return growth;
        }

        public static int GetPetInbornSkillGrowth(int level)
        {
            int growth;
            inbornSkillGrowth.TryGetValue(level, out growth);
            return growth;
        }

        public static int GetPetDungeonQueueNum(int queueType)
        {
            int queueNum;
            dungeonQueueNum.TryGetValue(queueType, out queueNum);
            return queueNum;
        }
    }
}

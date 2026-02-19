using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class HeroLibrary
    {
        private static List<int> awakenLevellist = new List<int>();
        //private static Dictionary<int, HeroInitModel> heroInitList = new Dictionary<int, HeroInitModel>();

        private static Dictionary<int, HeroLevelModel> heroLevelList = new Dictionary<int, HeroLevelModel>();
        //private static Dictionary<int, int> heroExpList = new Dictionary<int, int>();
        private static Dictionary<int, HeroModel> heroList = new Dictionary<int, HeroModel>();

        private static Dictionary<int, Dictionary<int, HeroJobModel>> heroJobList = new Dictionary<int, Dictionary<int, HeroJobModel>>();

        private static Dictionary<int, HeroQualityModel> heroQualityList = new Dictionary<int, HeroQualityModel>();

        private static Dictionary<int, HeroTitleModel> heroTitleList = new Dictionary<int, HeroTitleModel>();

        private static Dictionary<int, Vec2> heroPos = new Dictionary<int, Vec2>();
        private static Dictionary<int, HeroStepSpecialModel> heroStepSpecialModels = new Dictionary<int, HeroStepSpecialModel>();

        private static int resetTalentItem = 0;
        private static int resetTalentDiamond = 0;
        public static int HeroEquipMax
        { get; private set; }

        public static int ExpToSoulPower
        { get; private set; }

        public static int RevertHeroCurrenciesType
        { get; private set; }

        public static int RevertHeroCurrenciesNum
        { get; private set; }
        public static int ResetTalentItem
        {
            get
            {
                return resetTalentItem;
            }
        }

        public static int ResetTalentDiamond
        {
            get
            {
                return resetTalentDiamond;
            }
        }

        public static int HeroPosCount
        { get; private set; }

        public static int TalentMaxNum { get; set; }
        public static float HeroCollisionRadius { get; set; }

        private static Dictionary<int, int> HeroPosCollisions = new Dictionary<int, int>();

        public static int SSRHeroQuality { get; private set; }

        //public static float MonsterCollisionLeastRadius = 10;

        private static Dictionary<int, MainBattleQueueModel> heroMainQueueDic = new Dictionary<int, MainBattleQueueModel>();
        public static int BattleQueueNameLength { get; private set; }
        private static string[] heroInheritCostItem;
        
        public static int HeroStepMax { get; private set; }
        public static int HeroGodStepMax { get; private set; }
        public static int HeroGodStepUpCostItemId { get; private set; }
        public static int HeroGodFragmentNum { get; private set; }

        public static void Init()
        {
            InitHeroCard();

            BindHeroConfig();

            InitHeroLevel();

            InitHeroJob();

            InitHeroQuality();

            InitHeroTitle();

            InitHeroPos();

            InitStepModel();

            InitHeroMainQueue();
        }

        public static void InitHeroQuality()
        {
            //heroQualityList.Clear();
            Dictionary<int, HeroQualityModel> heroQualityList = new Dictionary<int, HeroQualityModel>();

            DataList dataList = DataListManager.inst.GetDataList("HeroQuality");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                HeroQualityModel heroQuality = new HeroQualityModel();
                heroQuality.MaxLevel = data.GetInt("MaxLevel");
                heroQuality.ReturnSoulCrystal = data.GetInt("ReturnSoulCrystal");
                heroQualityList.Add(data.ID, heroQuality);
            }
            HeroLibrary.heroQualityList = heroQualityList;
        }

        public static void InitHeroJob()
        {
            //heroJobList.Clear();
            Dictionary<int, Dictionary<int, HeroJobModel>> heroJobList = new Dictionary<int, Dictionary<int, HeroJobModel>>();

            Dictionary<int, HeroJobModel> list;
            DataList dataList = DataListManager.inst.GetDataList("HeroJob");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                HeroJobModel sex1 = new HeroJobModel();
                sex1.HeroId = data.GetInt("Sex1HeroId");
                sex1.FaceIconId = data.GetInt("Sex1FaceIcon");

                HeroJobModel sex2 = new HeroJobModel();
                sex2.HeroId = data.GetInt("Sex2HeroId");
                sex2.FaceIconId = data.GetInt("Sex2FaceIcon");

                list = new Dictionary<int, HeroJobModel>();
                list.Add(SexType.Male, sex1);
                list.Add(SexType.Famale, sex2);

                heroJobList.Add(data.ID, list);
            }
            HeroLibrary.heroJobList = heroJobList;
        }

        public static void InitHeroLevel()
        {
            //heroLevelList.Clear();
            //awakenLevellist.Clear();
            //heroExpList.Clear();

            List<int> awakenLevellist = new List<int>();
            Dictionary<int, HeroLevelModel> heroLevelList = new Dictionary<int, HeroLevelModel>();
            //Dictionary<int, int> heroExpList = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("HeroLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                HeroLevelModel heroLevel = new HeroLevelModel();
                heroLevel.Exp = data.GetInt("Exp");
                heroLevel.SoulCrystal = data.GetInt("SoulCrystal");
                if (heroLevel.SoulCrystal > 0)
                {
                    awakenLevellist.Add(data.ID);
                }
                heroLevelList.Add(data.ID, heroLevel);
            }
            HeroLibrary.awakenLevellist = awakenLevellist;
            HeroLibrary.heroLevelList = heroLevelList;
            //HeroLibrary.heroExpList = heroExpList;
        }

        public static void InitHeroCard()
        {
            //heroList.Clear();
            //heroInitList.Clear();
            Dictionary<int, HeroModel> heroList = new Dictionary<int, HeroModel>();

            HeroInitModel init = new HeroInitModel();
            DataList dataList = DataListManager.inst.GetDataList("HeroCard");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!heroList.ContainsKey(item.Key))
                {
                    HeroModel petModel = new HeroModel(data);
                    heroList.Add(item.Key, petModel);

                    //init = new HeroInitModel();
                    //init.InitLevel = data.GetInt("InitLevel");
                    //init.AwakenLevel = data.GetInt("InitAwakenLevel");
                    //heroInitList.Add(data.ID, init);
                }
            }
            HeroLibrary.heroList = heroList;
        }

        private static void BindHeroConfig()
        {
            Dictionary<int, int> HeroPosCollisions = new Dictionary<int, int>();
            Data data = DataListManager.inst.GetData("HeroConfig", 1);
            resetTalentItem = data.GetInt("ResetTalentItem");
            resetTalentDiamond = data.GetInt("ResetTalentDiamond");
            HeroEquipMax = data.GetInt("HeroEquipMax");
            ExpToSoulPower = data.GetInt("ExpToSoulPower");
            TalentMaxNum = data.GetInt("TalentMaxNum");
            RevertHeroCurrenciesType = data.GetInt("RevertHeroCurrenciesType");
            RevertHeroCurrenciesNum = data.GetInt("RevertHeroCurrenciesNum");
            HeroPosCount = data.GetInt("HeroPosCount");
            HeroCollisionRadius = data.GetFloat("HeroCollisionRadius");

            List<string> lsit = data.GetStringList("HeroPosCollision", "|");
            foreach (var item in lsit)
            {
                string[] kv = StringSplit.GetArray(":", item);
                HeroPosCollisions[int.Parse(kv[0])] = int.Parse(kv[1]);
            }

            SSRHeroQuality = data.GetInt("SSRHeroQuality");
            HeroLibrary.HeroPosCollisions = HeroPosCollisions;
            // MonsterCollisionLeastRadius = data.GetFloat("MonsterCollisionRadius");

            BattleQueueNameLength = data.GetInt("BattleQueueNameLength");
            heroInheritCostItem = StringSplit.GetArray(":", data.GetString("HeroInheritCostItem"));

            HeroStepMax = data.GetInt("HeroStepMax");
            HeroGodStepMax = data.GetInt("HeroGodStepMax");
            HeroGodStepUpCostItemId = data.GetInt("HeroGodStepUpCostItemId");
            HeroGodFragmentNum = data.GetInt("HeroGodFragmentNum");
        }

        private static void InitHeroTitle()
        {
            //heroTitleList.Clear();
            Dictionary<int, HeroTitleModel> heroTitleList = new Dictionary<int, HeroTitleModel>();

            HeroTitleModel info;
            Dictionary<int, int> nature = new Dictionary<int, int>();
            int talent = 0;
            DataList dataList = DataListManager.inst.GetDataList("HeroTitle");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new HeroTitleModel(data);
                info.AddNature(nature);
                info.AddTalent(talent);
                nature = info.TotalNature;
                talent = info.TotalTalent;
                heroTitleList.Add(info.Level, info);
            }

            HeroLibrary.heroTitleList = heroTitleList;
        }

        private static void InitHeroPos()
        {
            //heroPos.Clear();
            Dictionary<int, Vec2> heroPos = new Dictionary<int, Vec2>();

            DataList dataList = DataListManager.inst.GetDataList("HeroPos");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                Vec2 temp = new Vec2();
                temp.X = data.GetFloat("PosX");
                temp.Y = data.GetFloat("PosY");
                int pos = data.GetInt("Pos");
                heroPos.Add(pos, temp);
            }

            HeroLibrary.heroPos = heroPos;
        }

        private static void InitHeroMainQueue()
        {
            Dictionary<int, MainBattleQueueModel> heroMainQueueDic = new Dictionary<int, MainBattleQueueModel>();

            MainBattleQueueModel info;
            DataList dataList = DataListManager.inst.GetDataList("HeroMainQueue");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new MainBattleQueueModel(data);             
                heroMainQueueDic.Add(info.Id, info);
            }

            HeroLibrary.heroMainQueueDic = heroMainQueueDic;
        }

        public static Vec2 GetHeroPos(int id)
        {
            Vec2 temp = new Vec2();
            if (heroPos.ContainsKey(id))
            {
                temp = heroPos[id].Clone();
            }

            return temp;
        }

        public static int GetHeroPosCollisions(int pos)
        {
            int value;
            HeroPosCollisions.TryGetValue(pos, out value);
            return value;
        }
        public static HeroModel GetHeroModel(int heroId)
        {
            HeroModel model = null;
            heroList.TryGetValue(heroId, out model);
            return model;
        }

        public static Dictionary<int, HeroModel> GetHeroAllModel()
        {
            return heroList;
        }

        public static HeroLevelModel GetHeroLevel(int level)
        {
            HeroLevelModel model = null;
            heroLevelList.TryGetValue(level, out model);
            return model;
        }

        public static int GetHeroTotalExp(int levelStart, int levelEnd)
        {
            int exp = 0;
            foreach (var item in heroLevelList)
            {
                if (levelStart <= item.Key && item.Key <= levelEnd)
                {
                    exp += item.Value.Exp;
                }
            }
            return exp;
        }

        public static HeroAwakenModel GetHeroAwakenModel(int heroId)
        {
            HeroAwakenModel info = new HeroAwakenModel();
            HeroModel model = GetHeroModel(heroId);
            if (model != null)
            {
                info.InitLevel = model.AwakenLevel;
            }
            info.AwakenLevelList = awakenLevellist;
            return info;
        }

        public static List<int> GetAwakenLevels(int heroId)
        {
            return awakenLevellist;
        }

        //public static HeroInitModel GetHeroInitInfo(int heroId)
        //{
        //    HeroInitModel model = null;
        //    heroInitList.TryGetValue(heroId, out model);
        //    return model;
        //}

        public static HeroJobModel GetHeroJobInfo(int type, int sex)
        {
            HeroJobModel info = null;
            Dictionary<int, HeroJobModel> list;
            if (heroJobList.TryGetValue(type, out list))
            {
                list.TryGetValue(sex, out info);
            }
            return info;
        }

        public static HeroQualityModel GetHeroQuality(int quality)
        {
            HeroQualityModel model = null;
            heroQualityList.TryGetValue(quality, out model);
            return model;
        }

        public static HeroTitleModel GetHeroTitle(int titleLevel)
        {
            HeroTitleModel model = null;
            heroTitleList.TryGetValue(titleLevel, out model);
            return model;
        }

        public static int GetMaxTitleLevel()
        {
            int maxTitleLevel = 0;
            foreach (var titleLevel in heroTitleList)
            {
                if (maxTitleLevel < titleLevel.Key)
                {
                    maxTitleLevel = titleLevel.Key;
                }
            }
            return maxTitleLevel;
        }

        public int GetHeroMaxLevel(int heroId)
        {
            HeroModel model = GetHeroModel(heroId);
            if (model != null)
            {
                HeroQualityModel quality = GetHeroQuality(model.Quality);
                if (quality != null)
                {
                    return quality.MaxLevel;
                }

            }
            return 0;
        }

        private static void InitStepModel()
        {
            Dictionary<int, HeroStepSpecialModel> heroStepSpecialModels = new Dictionary<int, HeroStepSpecialModel>();

            HeroStepSpecialModel model = null;

            DataList groValFactorDataList = DataListManager.inst.GetDataList("HeroStepSpecial");
            foreach (var item in groValFactorDataList)
            {
                model = new HeroStepSpecialModel(item.Value);
                heroStepSpecialModels[model.Id] = model;
            }
            HeroLibrary.heroStepSpecialModels = heroStepSpecialModels;
        }

        public static HeroStepSpecialModel GeHeroStepSpecialModel(int id)
        {
            HeroStepSpecialModel model;
            heroStepSpecialModels.TryGetValue(id, out model);
            return model;
        }

        public static MainBattleQueueModel GetHeroMainQueueModel(int id)
        {
            MainBattleQueueModel model;
            heroMainQueueDic.TryGetValue(id, out model);
            return model;
        }

        public static List<MainBattleQueueModel> GetAllSuitableLevelMainQueueModels(int level)
        {
            List<MainBattleQueueModel> list = new List<MainBattleQueueModel>();
            foreach (var item in heroMainQueueDic)
            {
                if (level >= item.Value.LevelLimit)
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }

        public static List<MainBattleQueueModel> GetCurLevelMainQueueModels(int level)
        {
            List<MainBattleQueueModel> list = new List<MainBattleQueueModel>();
            foreach (var item in heroMainQueueDic)
            {
                if (level == item.Value.LevelLimit)
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }

        public static int GetHeroPosMaxNum()
        {
            return heroPos.Keys.Max();
        }

        public static string[] GetHeroInheritCost()
        {
            return heroInheritCostItem;
        }
    }
}

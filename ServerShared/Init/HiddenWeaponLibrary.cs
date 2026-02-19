using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using CommonUtility;
using DataProperty;
using ServerModels;

namespace ServerShared
{
    public class HiddenWeaponLibrary
    {
        private static Dictionary<int, HiddenWeaponModel> hiddenWeapon = new Dictionary<int, HiddenWeaponModel>();

        private static DoubleDepthMap<int, int, HiddenWeaponUpgradeModel> upgrades = new DoubleDepthMap<int, int, HiddenWeaponUpgradeModel>();

        private static DoubleDepthMap<int, int, HiddenWeaponStarWashPool> starPools = new DoubleDepthMap<int, int, HiddenWeaponStarWashPool>();
        private static DoubleDepthMap<int, int, HiddenWeaponStarModel> starModels = new DoubleDepthMap<int, int, HiddenWeaponStarModel>();

        private static Dictionary<int, HiddenWeaponSpecialModel> hiddenWeaponSpec = new Dictionary<int, HiddenWeaponSpecialModel>();


        private static Dictionary<int, HiddenWeaponWashModel> hiddenWeaponWashModels = new Dictionary<int, HiddenWeaponWashModel>();
        private static Dictionary<int, HiddenWeaponWashPool> hiddenWeaponWashPools = new Dictionary<int, HiddenWeaponWashPool>();
        //每个池子的最大id
        private static ListMap<int, int> poolMaxNatureValue = new ListMap<int, int>();

        private static List<ItemBasicInfo> lockCostList = new List<ItemBasicInfo>();
        private static List<ItemBasicInfo> lockCost10List = new List<ItemBasicInfo>();

        public static int MaxUpgradeLevel { get; private set; }

        public static Dictionary<int, int> StarWashNum { get; private set; }

        public static ItemBasicInfo WashCost { get; set; }
        public static ItemBasicInfo WashCost10 { get; set; }

        public static void Init()
        {
            InitConfig();
            InitHiddenWeapon();
            InitUpgrade();
            InitStar();
            InitSpec();
            InitWashPool();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("HiddenWeaponConfig", 1);
            MaxUpgradeLevel = data.GetInt("UpgradeMaxLevel");
            StarWashNum = data.GetString("StarWashNum").ToDictionary('|', ':');
            WashCost = ItemBasicInfo.Parse(data.GetString("WashCost"));
            WashCost10 = ItemBasicInfo.Parse(data.GetString("WashCost10"));

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(data.GetString("LockCost"));
            lockCostList = new List<ItemBasicInfo>(manager.AllRewards);

            manager.Clear();
            manager.InitSimpleReward(data.GetString("LockCost10"));
            lockCost10List = new List<ItemBasicInfo>(manager.AllRewards);
        }

        private static void InitHiddenWeapon()
        {
            Dictionary<int, HiddenWeaponModel> hiddenWeapon = new Dictionary<int, HiddenWeaponModel>();

            DataList dataList = DataListManager.inst.GetDataList("HiddenWeapon");
            foreach (var kv in dataList)
            {
                HiddenWeaponModel model = new HiddenWeaponModel(kv.Value);
                hiddenWeapon[model.Id] = model;
            }

            HiddenWeaponLibrary.hiddenWeapon = hiddenWeapon;
        }

        private static void InitUpgrade()
        {
            DoubleDepthMap<int, int, HiddenWeaponUpgradeModel> upgrades = new DoubleDepthMap<int, int, HiddenWeaponUpgradeModel>();

            DataList dataList = DataListManager.inst.GetDataList("HiddenWeaponUpgrade");

            foreach (var kv in dataList)
            {
                var model = new HiddenWeaponUpgradeModel(kv.Value);
                upgrades.Add(model.Pool, model.Level, model);
            }
            HiddenWeaponLibrary.upgrades = upgrades;
        }

        private static void InitStar()
        {
            DoubleDepthMap<int, int, HiddenWeaponStarModel> starModels = new DoubleDepthMap<int, int, HiddenWeaponStarModel>();
            DoubleDepthMap<int, int, HiddenWeaponStarWashPool> starPools = new DoubleDepthMap<int, int, HiddenWeaponStarWashPool>();

            DataList dataList = DataListManager.inst.GetDataList("HiddenWeaponStar");

            foreach (var kv in dataList)
            {
                var model = new HiddenWeaponStarModel(kv.Value);
                starModels.Add(model.Quality,model.Star, model);

                HiddenWeaponStarWashPool pool;
                if (!starPools.TryGetValue(model.Quality, model.Star, out pool))
                {
                    pool = new HiddenWeaponStarWashPool(model.Star);
                    starPools.Add(model.Quality, pool.Star, pool);
                }

                Dictionary<int,int> dic = kv.Value.GetString("PoolWeight").ToDictionary('|', ':');
                dic.ForEach(x => pool.Add(x.Key, x.Value));
            }
            HiddenWeaponLibrary.starModels = starModels;
            HiddenWeaponLibrary.starPools = starPools;
        }

        private static void InitSpec()
        {
            Dictionary<int, HiddenWeaponSpecialModel> hiddenWeaponSpec = new Dictionary<int, HiddenWeaponSpecialModel>();
            DataList dataList = DataListManager.inst.GetDataList("HiddenWeaponSpec");

            foreach (var data in dataList)
            {
                HiddenWeaponSpecialModel model = new HiddenWeaponSpecialModel(data.Value);
                hiddenWeaponSpec.Add(model.Id, model);
            }
            HiddenWeaponLibrary.hiddenWeaponSpec = hiddenWeaponSpec;
        }

        private static void InitWashPool()
        {
            ListMap<int, int> poolMaxNatureValue = new ListMap<int, int>();
            Dictionary<int, HiddenWeaponWashPool> hiddenWeaponWashPools = new Dictionary<int, HiddenWeaponWashPool>();
            Dictionary<int, HiddenWeaponWashModel> hiddenWeaponWashModels = new Dictionary<int, HiddenWeaponWashModel>();

            DataList dataList = DataListManager.inst.GetDataList("HiddenWeaponWashPool");

            foreach (var data in dataList)
            {
                HiddenWeaponWashModel model = new HiddenWeaponWashModel(data.Value);
                hiddenWeaponWashModels.Add(model.Id, model);

                HiddenWeaponWashPool pool;
                if (!hiddenWeaponWashPools.TryGetValue(model.Pool, out pool))
                {
                    pool = new HiddenWeaponWashPool(model.Pool);
                    hiddenWeaponWashPools.Add(model.Pool, pool);
                }
                pool.Add(model);

                if (data.Value.GetInt("MaxValue") == model.NatureValue)
                {
                    poolMaxNatureValue.Add(model.Pool, model.Id);
                }
            }

            HiddenWeaponLibrary.hiddenWeaponWashPools = hiddenWeaponWashPools;
            HiddenWeaponLibrary.poolMaxNatureValue = poolMaxNatureValue;
            HiddenWeaponLibrary.hiddenWeaponWashModels = hiddenWeaponWashModels;
        }


        public static ItemBasicInfo GetLockCost(int num)
        {
            if (num >= lockCostList.Count) return lockCostList[lockCostList.Count - 1];
            return lockCostList[num - 1];
        }

        public static ItemBasicInfo GetLockCost10(int num)
        {
            if (num >= lockCost10List.Count) return lockCost10List[lockCost10List.Count - 1];
            return lockCost10List[num - 1];
        }

        public static HiddenWeaponModel GetHiddenWeaponModel(int id)
        {
            HiddenWeaponModel model;
            hiddenWeapon.TryGetValue(id, out model);
            return model;
        }

        public static HiddenWeaponUpgradeModel GetHiddenWeaponUpgradeModel(int pool, int level)
        {
            HiddenWeaponUpgradeModel model;
            upgrades.TryGetValue(pool,level, out model);
            return model;
        }

        public static HiddenWeaponStarModel GetHiddenWeaponStarModel(int quality, int star)
        {
            HiddenWeaponStarModel model;
            starModels.TryGetValue(quality,star, out model);
            return model;
        }

        public static HiddenWeaponSpecialModel GetHiddenWeaponSpecialModel(int id)
        {
            HiddenWeaponSpecialModel model;
            hiddenWeaponSpec.TryGetValue(id, out model);
            return model;
        }

        public static HiddenWeaponWashModel GetHiddenWeaponWashModel(int id)
        {
            HiddenWeaponWashModel model;
            hiddenWeaponWashModels.TryGetValue(id, out model);
            return model;
        }

        public static HiddenWeaponStarWashPool GetStarWashPool(int quality, int star)
        {
            HiddenWeaponStarWashPool pool;
            starPools.TryGetValue(quality, star, out pool);
            return pool;
        }

        public static int RandomOnePool(int quality, int star)
        {
            HiddenWeaponStarWashPool pool = GetStarWashPool(quality, star);
            if (pool == null) return 0;

            int secondPool = pool.RandomPool();

            return secondPool;
        }


        public static int GetWashNatureCount(int start)
        {
            int count;
            StarWashNum.TryGetValue(start, out count);
            return count;
        }

        public static HiddenWeaponWashModel Wash(int quality, int star)
        {
            int pool = RandomOnePool(quality, star);

            HiddenWeaponWashPool washPool;

            if (!hiddenWeaponWashPools.TryGetValue(pool, out washPool)) return null;

            return washPool.Random();
        }

        public static bool RichMaxValue(int quality,  int star, int washId)
        {
            HiddenWeaponStarWashPool pool = GetStarWashPool(quality, star);
            if (pool == null) return false;

            HiddenWeaponWashModel model = GetHiddenWeaponWashModel(washId);
            if(model ==null) return false;

            //还有更高的池子
            if (model.Pool < pool.MaxPool) return false;

            //当前星级最大的池子
            List<int> ids;
            if (!poolMaxNatureValue.TryGetValue(pool.MaxPool, out  ids))
            {
                return false;
            }

            return ids.Contains(washId);
        }
    }
}

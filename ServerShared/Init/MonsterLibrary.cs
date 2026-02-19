using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class MonsterLibrary
    {
        private static Dictionary<int, MonsterModel> monsterList = new Dictionary<int, MonsterModel>();
        private static Dictionary<int, MonsterHeroModel> monsterHeroList = new Dictionary<int, MonsterHeroModel>();
        
        public static void Init()
        {
            InitMonster();
            InitMonsterHero();
        }

        private static void InitMonster()
        {
            //monsterList.Clear();
            Dictionary<int, MonsterModel> monsterList = new Dictionary<int, MonsterModel>();

            DataList dataList = DataListManager.inst.GetDataList("Monster");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!monsterList.ContainsKey(item.Key))
                {
                    MonsterModel monsterModel = new MonsterModel(data);
                    monsterList.Add(item.Key, monsterModel);
                }
            }
            MonsterLibrary.monsterList = monsterList;
        }

        private static void InitMonsterHero()
        {
            //monsterHeroList.Clear();
            Dictionary<int, MonsterHeroModel> monsterHeroList = new Dictionary<int, MonsterHeroModel>();

            DataList dataList = DataListManager.inst.GetDataList("MonsterHero");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!monsterHeroList.ContainsKey(item.Key))
                {
                    MonsterHeroModel monsterModel = new MonsterHeroModel(data);
                    monsterHeroList.Add(item.Key, monsterModel);
                }
            }
            MonsterLibrary.monsterHeroList = monsterHeroList;
        }

        public static MonsterModel GetMonsterModel(int id)
        {
            MonsterModel model = null;
            monsterList.TryGetValue(id, out model);
            return model;
        }

        public static MonsterHeroModel GetMonsterHeroModel(int id)
        {
            MonsterHeroModel model = null;
            monsterHeroList.TryGetValue(id, out model);
            return model;
        }
    }
}

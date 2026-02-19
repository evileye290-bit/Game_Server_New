using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class CharacterLibrary
    {
        private static Dictionary<int, CharacterLevelModel> characterLevelList = new Dictionary<int, CharacterLevelModel>();

        public static void Init()
        {
            InitCharacterLevel();
        }

        private static void InitCharacterLevel()
        {
            //characterLevelList.Clear();
            Dictionary<int, CharacterLevelModel> characterLevelList = new Dictionary<int, CharacterLevelModel>();

            DataList dataList = DataListManager.inst.GetDataList("CharacterLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CharacterLevelModel level = new CharacterLevelModel();
                level.Level = data.ID;
                level.Exp = data.GetInt("Exp");

                characterLevelList.Add(data.ID, level);
            }
            CharacterLibrary.characterLevelList = characterLevelList;
        }

        public static CharacterLevelModel GetCharacterLevelModel(int level)
        {
            CharacterLevelModel model = null;
            characterLevelList.TryGetValue(level, out model);
            return model;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProperty;
using Logger;
using EnumerateUtility;
using CommonUtility;
using ServerModels;

namespace ServerShared
{
    public class CharacterInitLibrary
    {
        public static float BeginPosX { get; private set; }
        public static float BeginPosY { get; private set; }
        public static int BeginAngle { get; private set; }
        public static int MapId { get; private set; }
        public static int FirstTask { get; private set; }
        public static int Level { get; private set; }
        public static int Exp { get; private set; }
        public static int Gold { get; private set; }
        public static int Diamond { get; private set; }
        public static int FriendlyHeart { get; private set; }
        public static int ArenaCoin { get; private set; }
        public static int SoulCrystal { get; private set; }
        public static int SoulPower { get; private set; }
        public static int FaceFrame { get; private set; }
        public static int ChatFrame { get; private set; }

        public static string InitName { get; private set; }
        public static int InitHeroId { get; private set; }
        public static int InitHeroSex { get; private set; }
        public static int InitJob { get; private set; }
        public static int FirstMainQueueNum { get; private set; }

        public static List<HeroCreateModel> Heros = new List<HeroCreateModel>();

        public static List<ItemCreateModel> Items = new List<ItemCreateModel>();

        private static Dictionary<CurrenciesType, int> InitCurrencies = new Dictionary<CurrenciesType, int>();
        public static void Init()
        {
            List<HeroCreateModel> Heros = new List<HeroCreateModel>();
            List<ItemCreateModel> Items = new List<ItemCreateModel>();
            Dictionary<CurrenciesType, int> InitCurrencies = new Dictionary<CurrenciesType, int>();
            //InitCurrencies.Clear();
            //Heros.Clear();
            //Items.Clear();
            Data data = DataListManager.inst.GetData("CharacterConfig", 1);
            if (data != null)
            {
                BeginPosX = data.GetFloat("BeginPosX");
                BeginPosY = data.GetFloat("BeginPosY");
                BeginAngle = data.GetInt("BeginAngle");
                MapId = data.GetInt("MapId");
                FirstTask = data.GetInt("FirstTask");
                Level = data.GetInt("Level");

   
                FaceFrame = data.GetInt("FaceFrame");
                ChatFrame = data.GetInt("ChatFrame");

                Exp = data.GetInt("Exp");
                Gold = data.GetInt("Gold");
                Diamond = data.GetInt("Diamond");
                SoulCrystal = data.GetInt("SoulCrystal");
                SoulPower = data.GetInt("SoulPower");
                FriendlyHeart = data.GetInt("FriendlyHeart");
                ArenaCoin = data.GetInt("ArenaCoin");

                InitName = data.GetString("InitName");
                InitHeroId = data.GetInt("InitHeroId");
                InitHeroSex = data.GetInt("InitHeroSex");
                InitJob = data.GetInt("InitJob");
                FirstMainQueueNum = data.GetInt("FirstMainQueueNum");

                InitCurrencies = new Dictionary<CurrenciesType, int>()
                {
                    { CurrenciesType.diamond, Diamond},
                    { CurrenciesType.exp, Exp},
                    { CurrenciesType.gold, Gold},
                    { CurrenciesType.soulCrystal, SoulCrystal},
                    { CurrenciesType.soulPower, SoulPower},
                    { CurrenciesType.friendlyHeart, FriendlyHeart},
                    { CurrenciesType.arenaCoin, ArenaCoin},
                    { CurrenciesType.spaceTimePower, ChapterLibrary.MinPower}
                };

                string heroString = data.GetString("InitHeros");
                if (!string.IsNullOrEmpty(heroString))
                {
                    string[] heros = StringSplit.GetArray("|", heroString);
                    foreach (var hero in heros)
                    {
                        int heroId = int.Parse(hero);
                        Data heroData = DataListManager.inst.GetData("HeroCard", heroId);
                        if (heroData != null)
                        {
                            HeroCreateModel heroInfo = new HeroCreateModel();
                            heroInfo.id = heroId;
                            heroInfo.level = heroData.GetInt("InitLevel");
                            heroInfo.equipIndex = 1;
                            heroInfo.state = (int)WuhunState.WaitAwaken;
                            Heros.Add(heroInfo);
                        }
                    }
                }

                string itemString = data.GetString("InitItems");
                if (!string.IsNullOrEmpty(itemString))
                {
                    string[] items = StringSplit.GetArray("|", itemString);
                    foreach (var item in items)
                    {
                        string[] itemArray = StringSplit.GetArray(":", item);
                        int typeId = int.Parse(itemArray[0]);
                        int num = int.Parse(itemArray[1]);

                        ItemCreateModel itemInfo = new ItemCreateModel();
                        itemInfo.typeId = typeId;
                        itemInfo.num = num;
                        Items.Add(itemInfo);
                    }
                }
            }
            else
            {
                Log.Warn("Can not find CharacterInitInfo xml");
            }
            CharacterInitLibrary.Heros = Heros;
            CharacterInitLibrary.Items = Items;
            CharacterInitLibrary.InitCurrencies = InitCurrencies;
        }

        public static int GetInitCurrencies(int type)
        {
            int value = 0;
            InitCurrencies.TryGetValue((CurrenciesType)type, out value);
            return value;
        }

        //public static Dictionary<RewardType, int> GetInitItems()
        //{
        //    return new Dictionary<RewardType, int>()
        //    {
        //        { RewardType.NormalItem, FaceFrame},
        //        { RewardType.NormalItem, ChatFrame}
        //    };
        //}
    }
}

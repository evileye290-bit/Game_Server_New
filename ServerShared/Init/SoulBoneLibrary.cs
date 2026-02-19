using CommonUtility;
using DataProperty;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerShared
{
    public class SoulBoneLibrary
    {
        #region basicInfos

        private static Dictionary<JobType, SoulBoneAnimal> jobAnimals = new Dictionary<JobType, SoulBoneAnimal>();

        private static Dictionary<int, SoulBonePrefix> prefixes = new Dictionary<int, SoulBonePrefix>();

        private static Dictionary<int, SoulBoneSuitPart2> suitParts2 = new Dictionary<int, SoulBoneSuitPart2>();

        private static Dictionary<int, SoulBoneItemInfo> items = new Dictionary<int, SoulBoneItemInfo>();

        private static Dictionary<int, SoulBoneSmeltCost> costs = new Dictionary<int, SoulBoneSmeltCost>();

        private static Dictionary<int, SoulBoneDrop> drops = new Dictionary<int, SoulBoneDrop>();

        private static Dictionary<int, SoulBoneSpecModel> soulBoneSpec = new Dictionary<int, SoulBoneSpecModel>();

        //maintype,subtype,List<Id>
        private static DoubleDeapthListMap<int, int, int> soulBoneSpecPool = new DoubleDeapthListMap<int, int, int>();
        private static Dictionary<int, int> mainTypeWeight = new Dictionary<int, int>();

        #endregion

        private static Dictionary<string, List<SoulBoneItemInfo>> animalPart_itemList = new Dictionary<string, List<SoulBoneItemInfo>>();

        private static Dictionary<int, Dictionary<int, int>> qualityMinMaxMainValue = new Dictionary<int, Dictionary<int, int>>();
        private static int maxPrefixId = 1;

        private static List<ItemBasicInfo> lockCostList = new List<ItemBasicInfo>();
        public static ItemBasicInfo SpecReplaceCost { get; private set; }

        public static void Init()
        {
            DataList animalDataList = DataListManager.inst.GetDataList("SoulBoneAnimal");
            DataList bodyPartDataList = DataListManager.inst.GetDataList("SoulBoneBodyPart");
            DataList prefixDataList = DataListManager.inst.GetDataList("SoulBoneAffix");
            DataList suitPartDataList = DataListManager.inst.GetDataList("SoulBoneSuit");
            DataList itemDataList = DataListManager.inst.GetDataList("SoulBoneItems");
            DataList costDataList = DataListManager.inst.GetDataList("SoulBoneSmeltCost");
            DataList dropDataList = DataListManager.inst.GetDataList("SoulBoneDrop");
            DataList specDataList = DataListManager.inst.GetDataList("SoulBoneSpec");

            DataList suitPartDataList2 = DataListManager.inst.GetDataList("SoulBoneSuit2");

            InitConfig();
            InitAnimal(animalDataList);
            InitPrefix(prefixDataList);
            InitPrefix2Quality();
            InitSuitPart2(suitPartDataList2);
            InitItem(itemDataList);
            InitCost(costDataList);
            InitDrop(dropDataList);
            InitSoulBoneSpec(specDataList);

            InitItemSearchDic();
        }

        #region 初始化

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("QuenchConfig", 1);

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(data.GetString("LockCost"));
            lockCostList = new List<ItemBasicInfo>(manager.AllRewards);

            manager.AllRewards.Clear();
            manager.InitSimpleReward(data.GetString("QuenchCost"));

            SpecReplaceCost = manager.AllRewards.First();
        }

        private static void InitDrop(DataList dataList)
        {
            Dictionary<int, SoulBoneDrop> drops = new Dictionary<int, SoulBoneDrop>();
            foreach (var item in dataList)
            {
                SoulBoneDrop temp = new SoulBoneDrop();
                Data data = item.Value;
                temp.ID = data.ID;
                temp.Rule = data.GetString("rule");
                temp.SpecList = data.GetIntList("SpecList","|");
                if (temp.GenerateInfos())
                {
                    drops.Add(temp.ID, temp);
                }
            }
            SoulBoneLibrary.drops = drops;
        }

        private static void InitSoulBoneSpec(DataList dataList)
        {
            Dictionary<int, SoulBoneSpecModel> soulBoneSpec = new Dictionary<int, SoulBoneSpecModel>();
            DoubleDeapthListMap<int, int, int> soulBoneSpecPool = new DoubleDeapthListMap<int, int, int>();
            Dictionary<int, int> mainTypeWeight = new Dictionary<int, int>();

            foreach (var data in dataList)
            {
                SoulBoneSpecModel model = new SoulBoneSpecModel(data.Value);
                soulBoneSpec.Add(model.Id, model);

                soulBoneSpecPool.Add(model.MainType, model.SubType, model.Id);

                mainTypeWeight.AddValue(model.MainType, model.Weight);
            }

            SoulBoneLibrary.soulBoneSpec = soulBoneSpec;
            SoulBoneLibrary.soulBoneSpecPool = soulBoneSpecPool;
            SoulBoneLibrary.mainTypeWeight = mainTypeWeight;
        }

        private static void InitCost(DataList dataList)
        {
            Dictionary<int, SoulBoneSmeltCost> costs = new Dictionary<int, SoulBoneSmeltCost>();
            foreach (var item in dataList)
            {
                SoulBoneSmeltCost temp = new SoulBoneSmeltCost();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.Color = data.GetInt("color");
                temp.Cost = data.GetInt("cost");
                string[] sectionParts = data.GetString("section").Split(':');
                Section section = new Section();
                if (sectionParts.Length == 2)
                {
                    section.Begin = sectionParts[0].ToInt();
                    section.End = sectionParts[1].ToInt();
                }
                temp.Section = section;
                costs.Add(temp.Id, temp);
            }
            SoulBoneLibrary.costs = costs;
        }

        private static void InitItem(DataList dataList)
        {
            Dictionary<int, SoulBoneItemInfo> items = new Dictionary<int, SoulBoneItemInfo>();
            foreach (var item in dataList)
            {
                SoulBoneItemInfo temp = new SoulBoneItemInfo();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.MainNatureType = data.GetInt("mainProperty");
                temp.AnimalType = data.GetInt("animalType");
                temp.PartType = data.GetInt("position");
                temp.Quality = data.GetInt("Color");
                string[] sectionParts = data.GetString("mainPropertyRange").Split(':');
                Section section = new Section();
                if (sectionParts.Length == 2)
                {
                    section.Begin = sectionParts[0].ToInt();
                    section.End = sectionParts[1].ToInt();
                }
                else
                {
                    Console.WriteLine("InitItem With section error ,part id {0},section {1}", temp.Id, data.GetString("mainPropertyRange"));
                }
                temp.MainNatureSection = section;
                temp.data = data;
                temp.AddAttrType1 = data.GetInt("addiProperty1");
                temp.AddAttrType2 = data.GetInt("addiProperty2");
                temp.AddAttrType3 = data.GetInt("addiProperty3");
                temp.TitleLimit = data.GetInt("titleLimit");
                temp.Job = data.GetInt("jobLimit");
                temp.WearAble = data.GetBoolean("WearAble");

                string skillInfo = data.GetString("SpecSkill");
                if (!string.IsNullOrEmpty(skillInfo))
                {
                    string[] skillValues = skillInfo.Split('|');
                    foreach (var skill in skillValues)
                    {
                        string[] skillStr = skill.Split(':');
                        if (skillStr.Count() > 1)
                        {
                            temp.SpecSkills.Add(int.Parse(skillStr[0]), float.Parse(skillStr[1]));
                        }
                        else
                        {
                            Log.WarnLine("soulboneitems specskill param {0} error", skillInfo);
                            continue;
                        }
                    }
                }

                temp.SpecMainType.AddRange(data.GetString("SpecMainType").ToList('|'));

                items.Add(temp.Id, temp);
            }
            SoulBoneLibrary.items = items;
        }

        private static void InitSuitPart2(DataList dataList)
        {
            Dictionary<int, SoulBoneSuitPart2> suitParts2 = new Dictionary<int, SoulBoneSuitPart2>();

            foreach (var item in dataList)
            {
                SoulBoneSuitPart2 part = new SoulBoneSuitPart2();
                Data data = item.Value;
                part.Id = data.ID;
                part.Animal = data.GetInt("animal");
                part.Color = data.GetInt("Color");
                part.PartCount = data.GetInt("partCount");
                part.Percent = data.GetInt("param");
                part.Prefix = data.GetInt("affix");
                string nature = data.GetString("natureType");
                string[] natures = nature.Split('|');
                foreach (var temp in natures)
                {
                    part.NatureTypes.Add(temp.ToInt());
                }
                suitParts2.Add(part.Id, part);

            }
            SoulBoneLibrary.suitParts2 = suitParts2;
        }

        private static void InitPrefix(DataList dataList)
        {
            Dictionary<int, SoulBonePrefix> prefixes = new Dictionary<int, SoulBonePrefix>();
            foreach (var item in dataList)
            {
                SoulBonePrefix part = new SoulBonePrefix();
                Data data = item.Value;
                part.Id = data.ID;
                part.Name = data.GetString("nameId");
                part.SmeltReward = data.GetString("SmeltReward");
                string[] sectionParts = data.GetString("section").Split(':');
                Section section = new Section();
                if (sectionParts.Length == 2)
                {
                    section.Begin = sectionParts[0].ToInt();
                    section.End = sectionParts[1].ToInt();
                }
                else
                {
                    Console.WriteLine(" InitPrefix With section error ,part id {0},section {1}", part.Id, data.GetString("section"));
                }
                part.Section = section;
                part.Num = data.GetInt("num");
                part.Color = data.GetInt("color");

                part.SpecialNum = data.GetInt("SpecialNum");

                part.SpecSubTypeRange = new List<Section>();
                string[] specSubTypeRange = data.GetString("SpecSubTypeRange").Split('|');
                for (int i = 0; i < specSubTypeRange.Length; i++)
                {
                    string[] parts = specSubTypeRange[i].Split(':');
                    Section tempSection = new Section();
                    if (sectionParts.Length == 2)
                    {
                        tempSection.Begin = parts[0].ToInt();
                        tempSection.End = parts[1].ToInt();
                    }
                    else
                    {
                        Log.Warn(" InitPrefix SpecSubTypeRange With section error ,part id {0},section {1}", part.Id, specSubTypeRange[i]);
                    }
                    part.SpecSubTypeRange.Add(tempSection);
                }

                prefixes.Add(part.Id, part);
            }
            SoulBoneLibrary.prefixes = prefixes;
        }

        private static void InitPrefix2Quality()
        {
            //qualityMinMaxMainValue.Clear();
            Dictionary<int, Dictionary<int, int>> qualityMinMaxMainValue = new Dictionary<int, Dictionary<int, int>>();
            maxPrefixId = 1;
            foreach (var item in prefixes)
            {
                Dictionary<int, int> minMax = new Dictionary<int, int>();
                if (qualityMinMaxMainValue.TryGetValue(item.Value.Color, out minMax))
                {
                    int tempMin = minMax[0];
                    int tempMax = minMax[1];
                    if (item.Value.Section.Begin < tempMin)
                    {
                        minMax[0] = item.Value.Section.Begin;
                    }
                    if (item.Value.Section.End > tempMax)
                    {
                        minMax[1] = item.Value.Section.End;
                    }
                }
                else
                {
                    minMax = new Dictionary<int, int>();
                    qualityMinMaxMainValue.Add(item.Value.Color, minMax);
                    minMax[0] = item.Value.Section.Begin;
                    minMax[1] = item.Value.Section.End;
                }
                if (item.Value.Id > maxPrefixId)
                {
                    maxPrefixId = item.Value.Id;
                }
            }
            SoulBoneLibrary.qualityMinMaxMainValue = qualityMinMaxMainValue;
        }

        private static void InitAnimal(DataList dataList)
        {
            //Dictionary<int, SoulBoneAnimal> animals = new Dictionary<int, SoulBoneAnimal>();
            Dictionary<JobType, SoulBoneAnimal> jobAnimals = new Dictionary<JobType, SoulBoneAnimal>();

            foreach (var item in dataList)
            {
                SoulBoneAnimal animal = new SoulBoneAnimal();
                Data data = item.Value;
                animal.Id = data.ID;
                animal.Name = data.Name;
                animal.Job = (JobType)data.GetInt("job");
                //animals.Add(animal.Id, animal);
                jobAnimals.Add(animal.Job, animal);
            }
            //SoulBoneLibrary.animals = animals;
            SoulBoneLibrary.jobAnimals = jobAnimals;
        }

        private static void InitItemSearchDic()
        {
            Dictionary<string, List<SoulBoneItemInfo>> animalPart_itemList = new Dictionary<string, List<SoulBoneItemInfo>>();

            foreach (var item in items)
            {
                List<SoulBoneItemInfo> list = null;
                if (animalPart_itemList.TryGetValue(item.Value.AnimalType + "_" + item.Value.PartType, out list))
                {
                    list.Add(item.Value);
                }
                else
                {
                    list = new List<SoulBoneItemInfo>();
                    list.Add(item.Value);
                    animalPart_itemList.Add(item.Value.AnimalType + "_" + item.Value.PartType, list);
                }
            }
            SoulBoneLibrary.animalPart_itemList = animalPart_itemList;
        }

        #endregion

        public static int GetTypeId(SoulBone bone)
        {
            string key = bone.AnimalType + "_" + bone.PartType;
            List<SoulBoneItemInfo> list = null;
            if (animalPart_itemList.TryGetValue(key, out list))
            {
                foreach (var item in list)
                {
                    if (bone.MainNatureValue <= item.MainNatureSection.End && bone.MainNatureValue >= item.MainNatureSection.Begin)
                    {
                        return item.Id;
                    }
                }
                return -2;
            }
            else
            {
                return -1;
            }

        }

        #region 套装

        //public static List<SoulBone> GetEnhancedBones(List<SoulBone> list)
        //{
        //    List<SoulBone> tempBones = null;
        //    //分组
        //    Dictionary<string, List<SoulBone>> boneParts = new Dictionary<string, List<SoulBone>>();
        //    foreach (var item in list)
        //    {
        //        int animal = item.AnimalType;
        //        int prefix = item.Prefix;
        //        int quality = item.Quality;
        //        string key = animal + "_" + prefix + "_" + quality;
        //        List<SoulBone> bones = null;
        //        if (boneParts.TryGetValue(key, out bones))
        //        {
        //            bones.Add(item);
        //        }
        //        else
        //        {
        //            bones = new List<SoulBone>();
        //            bones.Add(item);
        //            boneParts.Add(key, bones);
        //        }
        //    }

        //    //拿到加成信息
        //    tempBones = GetEnhancedBones(boneParts);
        //    return tempBones;
        //}

        //private static List<SoulBone> GetEnhancedBones(Dictionary<string, List<SoulBone>> boneParts)
        //{
        //    //Dictionary<SoulBoneSuitPart, List<SoulBone>> parts = new Dictionary<SoulBoneSuitPart, List<SoulBone>>();
        //    List<SoulBone> ans = new List<SoulBone>();
        //    foreach (var item in boneParts.Values)
        //    {
        //        foreach (var temp in item)
        //        {
        //            ans.Add(temp.Clone());
        //        }
        //    }

        //    List<Tuple<bool, SoulBoneSuitPart>> tuples = new List<Tuple<bool, SoulBoneSuitPart>>();
        //    foreach (var item in boneParts.Values)
        //    {
        //        var ret = GetMainNatureSuitParts(item);
        //        tuples.Add(ret);
        //    }
        //    foreach (var bone in ans)
        //    {
        //        double nature1 = bone.MainNatureValue, nature2 = bone.AdditionValue1, nature3 = bone.AdditionValue2, nature4 = bone.AdditionValue3;
        //        foreach (var temp in tuples)
        //        {
        //            SoulBoneSuitPart suitInfo = temp.Item2;
        //            int percent = suitInfo.Percent;

        //            if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //            {
        //                nature1 += bone.MainNatureValue * (double)percent / 10000;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType1))
        //            {
        //                nature2 += bone.AdditionValue1 * (double)percent / 10000;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType2))
        //            {
        //                nature3 += bone.AdditionValue2 * (double)percent / 10000;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType3))
        //            {
        //                nature4 += bone.AdditionValue3 * (double)percent / 10000;
        //            }
        //        }
        //        bone.MainNatureValue = (int)(nature1 + 0.5);
        //        bone.AdditionValue1 = (int)(nature2 + 0.5);
        //        bone.AdditionValue2 = (int)(nature3 + 0.5);
        //        bone.AdditionValue3 = (int)(nature4 + 0.5);
        //    }

        //    return ans;
        //    //foreach (var item in boneParts.Values)
        //    //{
        //    //    var ret = GetMainNatureSuitParts(item);
        //    //    if (ret.Item1 == true)
        //    //    {
        //    //        parts.Add(ret.Item2, item);
        //    //    }
        //    //    else
        //    //    {
        //    //        foreach (var temp in item)
        //    //        {
        //    //            ans.Add(temp.Clone());
        //    //        }
        //    //    }
        //    //}

        //    ////返回克隆并修改了信息的值
        //    //foreach (var part in parts)
        //    //{
        //    //    SoulBoneSuitPart suitInfo = part.Key;
        //    //    int percent = suitInfo.Percent;
        //    //    foreach (var item in part.Value)
        //    //    {
        //    //        SoulBone bone = item.Clone();
        //    //        if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //    //        {
        //    //            bone.MainNatureValue += bone.MainNatureValue * percent / 10000;
        //    //        }
        //    //        if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //    //        {
        //    //            bone.AdditionValue1 += bone.AdditionValue1 * percent / 10000;
        //    //        }
        //    //        if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //    //        {
        //    //            bone.AdditionValue2 += bone.AdditionValue2 * percent / 10000;
        //    //        }
        //    //        if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //    //        {
        //    //            bone.AdditionValue3 += bone.AdditionValue3 * percent / 10000;
        //    //        }
        //    //        ans.Add(bone);
        //    //    }
        //    //}
        //}

        //private static Tuple<bool, SoulBoneSuitPart> GetMainNatureSuitParts(List<SoulBone> suitBones)
        //{
        //    //在上层调用前检查是否是一套
        //    SoulBone bone = suitBones.First();
        //    int mainNatureValue = bone.MainNatureValue;
        //    foreach (var item in suitParts)
        //    {
        //        if (mainNatureValue >= item.Value.Section.Begin && mainNatureValue <= item.Value.Section.End && item.Value.PartCount == suitBones.Count)
        //        {
        //            return Tuple.Create(true, item.Value);
        //        }
        //    }
        //    return Tuple.Create(false, new SoulBoneSuitPart());
        //}

        //public static Tuple<int, int, List<SoulBone>> GetEnhancedBones2(List<SoulBone> list)
        //{
        //    List<SoulBone> tempBones = new List<SoulBone>();
        //    list.ForEach(item =>
        //    {
        //        tempBones.Add(item.Clone());
        //    });

        //    //按颜色降序排序
        //    tempBones.Sort((x, y) => -x.CompareTo(y));

        //    //判断数量能成套
        //    if (tempBones.Count < 3)//一个套装都没有
        //    {
        //        return Tuple.Create(0, 0, tempBones);
        //    }
        //    //按照6件套最高quality判断，然后判断3件套，然后降quality判断直到quality为1
        //    int quality = 0;
        //    int suit = 0;
        //    for (int i = 6; i > 0; i--)
        //    {
        //        bool success = false;
        //        //判断6件套
        //        if (tempBones.Count == 6)
        //        {
        //            success = true;
        //            foreach (var item in tempBones)
        //            {
        //                if (item.Quality < i)
        //                {
        //                    success = false;
        //                    break;
        //                }
        //            }

        //        }
        //        if (success)
        //        {
        //            quality = i;
        //            suit = 6;
        //            break;
        //        }
        //        //判断3件套
        //        success = true;
        //        for (int j = 0; j < 3; j++)
        //        {
        //            if (tempBones[j].Quality < i)
        //            {
        //                success = false;
        //                break;
        //            }
        //        }
        //        if (success)
        //        {
        //            quality = i;
        //            suit = 3;
        //            break;
        //        }
        //    }

        //    //假如 quality>0 加成
        //    if (quality > 0)
        //    {
        //        var suitPart = GetMainNatureSuitParts2(tempBones, tempBones.First().AnimalType, quality, suit);
        //        foreach (var bone in tempBones)
        //        {
        //            double nature1 = bone.MainNatureValue, nature2 = bone.AdditionValue1, nature3 = bone.AdditionValue2, nature4 = bone.AdditionValue3;

        //            SoulBoneSuitPart2 suitInfo = suitPart.Item2;
        //            int percent = suitInfo.Percent;
        //            if (suitInfo.NatureTypes.Contains(bone.MainNatureType))
        //            {
        //                //nature1 += bone.MainNatureValue * (double)percent / 10000;
        //                nature1 += percent;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType1))
        //            {
        //                //nature2 += bone.AdditionValue1 * (double)percent / 10000;
        //                nature2 += percent;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType2))
        //            {
        //                //nature3 += bone.AdditionValue2 * (double)percent / 10000;
        //                nature3 += percent;
        //            }
        //            if (suitInfo.NatureTypes.Contains(bone.AdditionType3))
        //            {
        //                //nature4 += bone.AdditionValue3 * (double)percent / 10000;
        //                nature4 += percent;
        //            }
        //            bone.MainNatureValue = (int)(nature1 + 0.5);
        //            bone.AdditionValue1 = (int)(nature2 + 0.5);
        //            bone.AdditionValue2 = (int)(nature3 + 0.5);
        //            bone.AdditionValue3 = (int)(nature4 + 0.5);
        //        }
        //        return Tuple.Create(quality, suit, tempBones);
        //    }
        //    return Tuple.Create(0, 0, tempBones);
        //}

        //public static Dictionary<NatureType, int> GetEnhancedBones3(List<SoulBone> list)
        //{
        //    Dictionary<NatureType, int> ret = new Dictionary<NatureType, int>();
        //    List<SoulBone> tempBones = new List<SoulBone>();
        //    list.ForEach(item =>
        //    {
        //        tempBones.Add(item);
        //    });

        //    //按颜色降序排序
        //    tempBones.Sort((x, y) => -x.CompareTo(y));

        //    //判断数量能成套
        //    if (tempBones.Count < 3)//一个套装都没有
        //    {
        //        return ret;
        //    }
        //    //按照6件套最高quality判断，然后判断3件套，然后降quality判断直到quality为1
        //    int quality = 0;
        //    int suit = 0;
        //    for (int i = 6; i > 0; i--)
        //    {
        //        bool success = false;
        //        //判断6件套
        //        if (tempBones.Count == 6)
        //        {
        //            success = true;
        //            foreach (var item in tempBones)
        //            {
        //                if (item.Quality < i)
        //                {
        //                    success = false;
        //                    break;
        //                }
        //            }

        //        }
        //        if (success)
        //        {
        //            quality = i;
        //            suit = 6;
        //            break;
        //        }
        //        //判断3件套
        //        success = true;
        //        for (int j = 0; j < 3; j++)
        //        {
        //            if (tempBones[j].Quality < i)
        //            {
        //                success = false;
        //                break;
        //            }
        //        }
        //        if (success)
        //        {
        //            quality = i;
        //            suit = 3;
        //            break;
        //        }
        //    }

        //    //假如 quality>0 加成
        //    if (quality > 0)
        //    {
        //        var suitPart = GetMainNatureSuitParts2(tempBones, tempBones.First().AnimalType, quality, suit);

        //        SoulBoneSuitPart2 suitInfo = suitPart.Item2;

        //        foreach (var item in suitInfo.NatureTypes)
        //        {
        //            ret.Add((NatureType)item, suitInfo.Percent);
        //        }
        //    }
        //    return ret;
        //}

        public static Dictionary<NatureType, int> GetEnhancedBones4(List<SoulBone> list)
        {
            Dictionary<NatureType, int> ret = new Dictionary<NatureType, int>();
            List<SoulBone> tempBones = new List<SoulBone>();
            list.ForEach(item =>
            {
                tempBones.Add(item);
            });

            //按颜色降序排序
            tempBones.Sort((x, y) => -x.CompareTo(y));

            //判断数量能成套
            if (tempBones.Count < 3)//一个套装都没有
            {
                return ret;
            }
            //按照6件套最高quality判断，然后判断3件套，然后降quality判断直到quality为1 !!!!!!!!!!!! 此处quality为prefix
            int quality = 0;
            int suit = 0;
            for (int i = maxPrefixId; i > 0; i--)//此处i指prefix
            {
                bool success = false;
                //判断6件套
                if (tempBones.Count == 6)
                {
                    success = true;
                    foreach (var item in tempBones)
                    {
                        if (item.Prefix < i)
                        {
                            success = false;
                            break;
                        }
                    }

                }
                if (success)
                {
                    quality = i;
                    suit = 6;
                    break;
                }
                //判断3件套
                success = true;
                for (int j = 0; j < 3; j++)
                {
                    if (tempBones[j].Prefix < i)
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                {
                    quality = i;
                    suit = 3;
                    break;
                }
            }

            //假如 quality>0 加成
            if (quality > 0)
            {
                var suitPart = GetMainNatureSuitParts2(tempBones, tempBones.First().AnimalType, quality, suit);

                SoulBoneSuitPart2 suitInfo = suitPart.Item2;

                foreach (var item in suitInfo.NatureTypes)
                {
                    ret.Add((NatureType)item, suitInfo.Percent);
                }
            }
            return ret;
        }

        private static Tuple<bool, SoulBoneSuitPart2> GetMainNatureSuitParts2(List<SoulBone> suitBones, int animal, int prefix, int suit)
        {
            //在上层调用前检查是否是一套
            SoulBone bone = suitBones.First();
            int mainNatureValue = bone.MainNatureValue;
            foreach (var item in suitParts2)
            {
                if (item.Value.Animal == animal && item.Value.Prefix == prefix && item.Value.PartCount == suit)
                {
                    return Tuple.Create(true, item.Value);
                }
            }
            return Tuple.Create(false, new SoulBoneSuitPart2());
        }

        #endregion

        #region 掉落 宝箱开启获取

        public static SoulBoneDrop GetSoulBoneDrop(int id)
        {
            SoulBoneDrop drop;
            drops.TryGetValue(id, out drop);
            return drop;
        }

        public static int GetDropMainValue(int id)
        {
            SoulBoneDrop drop = null;
            if (drops.TryGetValue(id, out drop))
            {
                return drop.GetValue();
            }
            return 0;
        }

        public static int GetSoulBoneItemId(int startId, int job)
        {
            //jobAnimals
            SoulBoneAnimal temp = null;
            jobAnimals.TryGetValue((JobType)job, out temp);
            return startId + (temp.Id - 1) * 10;//按照配表的规律
        }

        public static string ReplaceSoulBone4AllRewards(string resourceString, int job)
        {
            string[] resourceList = resourceString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> ans = new List<string>();
            foreach (string resourceItem in resourceList)
            {
                ans.Add(ReplaceSoulBoneJob(resourceItem, job));
            }
            //
            StringBuilder rewardString = new StringBuilder();
            bool first = true;
            foreach (var item in ans)
            {
                if (first)
                {
                    first = false;
                    rewardString.Append(item);
                }
                else
                {
                    rewardString.Append("|" + item);
                }
            }
            return rewardString.ToString();
        }

        public static string ReplaceSoulBoneJob(string rewardString, int job)
        {
            string ans = rewardString;
            string[] itemAndAtt = rewardString.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
            if (itemAndAtt.Length < 2)
            {
                return ans;
            }
            string[] temps = itemAndAtt[0].Split(':');

            int trueId = 0;
            if (temps[0].Equals("job"))
            {
                int startId = int.Parse(temps[1]);
                trueId = GetSoulBoneItemId(startId, job);
                ans = trueId + "";
                for (int i = 2; i < temps.Count(); i++)
                {
                    ans += ":" + temps[i];
                }
                ans += "@";
                ans += job;
                bool first = true;
                foreach (var item in itemAndAtt[1].Split(':'))
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    ans += ":" + item;
                }
            }

            return ans;

        }
        #endregion

        #region 熔炼

        public static void ProducePrefix(SoulBoneInfo info, int typeId, bool produceSpec, bool fixdb)
        {
            SoulBonePrefix soulBonePrefix = null;
            int mainValue = info.MainNature.Value;
            foreach (var item in prefixes)
            {
                if (mainValue <= item.Value.Section.End && mainValue >= item.Value.Section.Begin)
                {
                    soulBonePrefix = item.Value;
                    info.PrefixId = item.Key;
                }
            }

            if (produceSpec)
            {
                //随机特殊属性
                ProduceSpecSkill(typeId, info, soulBonePrefix, fixdb);
            }
        }


        public static bool ProducePrefix(SoulBone soulBone, bool fixdb)
        {
            bool changed = false;
            SoulBonePrefix soulBonePrefix = prefixes.Values
                .FirstOrDefault(x => soulBone.MainNatureValue <= x.Section.End && soulBone.MainNatureValue >= x.Section.Begin);
            if (soulBonePrefix == null) return false;

            if (soulBonePrefix.SpecialNum <= 0) return false;

            if (soulBone.GetSpecList().Count >= soulBonePrefix.SpecialNum) return false;

            //随机特殊属性
            List<int> idList = ProduceSpecSkill(soulBone.TypeId, soulBone.MainNatureValue, soulBonePrefix, fixdb);
            if (idList == null || idList.Count <= 0) return false;

            soulBone.UpdateSpecId(idList);

            return true;
        }

        public static void ProduceSpecSkill(int typeId, SoulBoneInfo info, SoulBonePrefix soulBonePrefix, bool fixdb)
        {
            if (info.SpecSkills.Count > 0) return;

            List<int> idList = ProduceSpecSkill(typeId, info.MainNature.Value, soulBonePrefix);
            if(idList == null|| idList.Count==0) return;

            info.SpecSkills.AddRange(idList);
        }

        public static List<int> ProduceSpecSkill(int typeId, int mainNatureValue, SoulBonePrefix soulBonePrefix, bool fixdb = false)
        {
            SoulBoneItemInfo itemInfo = GetItemInfo(typeId);
            if (itemInfo == null) return null;

            if (soulBonePrefix.SpecialNum <= 0) return  null;

            List<int> idList = new List<int>();

            //修复老数据，读取表中原来的配置信息
            if (fixdb && itemInfo.SpecSkills.Count > 0)
            {
                idList.AddRange(itemInfo.SpecSkills.Keys);
                return idList;
            }

            if (itemInfo.SpecMainType.Count < soulBonePrefix.SpecialNum || itemInfo.SpecMainType.Count > soulBonePrefix.SpecSubTypeRange.Count)
            {
                Log.Warn($"ProduceSpecSkill data error typeid{ typeId} mainNatureValue {mainNatureValue}, specNum {soulBonePrefix.SpecialNum } mainType count {itemInfo.SpecMainType.Count} not equal to subType count {soulBonePrefix.SpecSubTypeRange.Count}");
            }

            int mainTypeWeightSum = 0;
            //maintype, weight, index
            List<Tuple<int, int, int>> mainType2Weight = new List<Tuple<int, int, int>>();

            for (int i = 0; i < itemInfo.SpecMainType.Count; i++)
            {
                int value = 0;
                if (mainTypeWeight.TryGetValue(itemInfo.SpecMainType[i], out value))
                {
                    mainTypeWeightSum += value;
                    mainType2Weight.Add(new Tuple<int, int, int>(itemInfo.SpecMainType[i], value, i));
                }
            }

            //随机主类型
            List<Tuple<int, int, int>> randomList = new List<Tuple<int, int, int>>();
            for (int i = 0; i < soulBonePrefix.SpecialNum; i++)
            {
                int num = RAND.Range(0, mainTypeWeightSum);
                foreach (var cur in mainType2Weight)
                {
                    if (num <= cur.Item2)
                    {
                        randomList.Add(cur);
                        break;
                    }
                    num -= cur.Item2;
                }
            }

            foreach (var item in randomList)
            {
                int weight = 0;
                //子类型池子
                Dictionary<int, int> idWeigh = new Dictionary<int, int>();

                Section section = soulBonePrefix.SpecSubTypeRange[item.Item3];
                for (int j = section.Begin; j < section.End; j++)
                {
                    List<int> ids;
                    if (soulBoneSpecPool.TryGetValue(item.Item1, j, out ids))
                    {
                        foreach (var id in ids)
                        {
                            var model = GetSpecModel(id);
                            if(model== null) continue;

                            weight += model.Weight;
                            idWeigh[model.Id] = model.Weight;
                        }
                    }
                }

                if (idWeigh.Count == 0)
                {
                    //没有合适的池子
                    Log.Warn($"ProduceSpecSkill data error typeid{ typeId} mainNatureValue {mainNatureValue}, mainType count {itemInfo.SpecMainType.Count}  subType {soulBonePrefix.SpecSubTypeRange.Count}");
                    return null;
                }

                int specId = RAND.RandValue(idWeigh, weight);
                idList.Add(specId);
            }

            return idList;
        }

        public static void ProduceAdds(SoulBoneInfo info, string adds)
        {
            string[] addinfos = adds.Split('|');
            int count = 1;
            foreach (var temp in addinfos)
            {
                if (count > 3)
                {
                    return;
                }
                string[] pair = temp.Split(':');
                if (pair.Length == 2)
                {
                    SoulBoneNature nature = new SoulBoneNature();
                    nature.NatureType = pair[0].ToInt();
                    nature.Value = pair[1].ToInt();
                    if (nature.NatureType > 0 && nature.Value > 0)
                    {
                        info.Natures.Add(count, nature);
                        count++;
                    }
                }
            }
        }

        public static SoulBoneItemInfo GetItemInfo(int animalType, int partType, int mainValue)
        {
            string key = animalType + "_" + partType;
            List<SoulBoneItemInfo> list = null;
            if (animalPart_itemList.TryGetValue(key, out list))
            {
                foreach (var item in list)
                {
                    if (mainValue <= item.MainNatureSection.End && mainValue >= item.MainNatureSection.Begin)
                    {
                        return item;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public static Tuple<int, int> GetAnimalPart(string animalParts)
        {
            string[] ap = animalParts.Split('|');
            int retAnimal = 0;
            int retPart = 0;

            int randomValueSum = 0;
            List<int> randomCount = new List<int>();
            randomCount.Add(0);
            List<int> animalCount = new List<int>();
            if (ap.Length != 8)
            {
                return Tuple.Create(0, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                string[] animalRand = ap[i].Split(':');
                if (animalRand.Length != 2)
                {
                    return Tuple.Create(0, 0);
                }
                int animal = animalRand[0].ToInt();
                int randNum = animalRand[1].ToInt();
                randomValueSum += randNum;
                randomCount.Add(randomValueSum);
                animalCount.Add(animal);
            }
            int rand = RAND.Range(0, randomValueSum);

            for (int i = 0; i < randomCount.Count; i++)
            {
                if (rand >= randomCount[i] && rand <= randomCount[i + 1])
                {
                    retAnimal = animalCount[i];
                    break;
                }
            }

            randomValueSum = 0;
            randomCount.Clear();
            randomCount.Add(0);
            List<int> partCount = new List<int>();
            for (int i = 4; i < 8; i++)
            {
                string[] partRand = ap[i].Split(':');
                if (partRand.Length != 2)
                {
                    return Tuple.Create(0, 0);
                }
                int part = partRand[0].ToInt();
                int randNum = partRand[1].ToInt();
                randomValueSum += randNum;
                randomCount.Add(randomValueSum);
                partCount.Add(part);
            }

            rand = RAND.Range(0, randomValueSum);

            for (int i = 0; i < randomCount.Count; i++)
            {
                if (rand >= randomCount[i] && rand <= randomCount[i + 1])
                {
                    retPart = partCount[i];
                    break;
                }
            }

            return Tuple.Create(retAnimal, retPart);
        }

        /// <summary>
        /// 动物和概率
        /// </summary>
        /// <param name="animals"></param>
        /// <returns></returns>
        public static int GetAnimal(Dictionary<int, int> animals)
        {
            int retAnimal = 0;


            int randomValueSum = 0;
            List<int> randomCount = new List<int>();
            randomCount.Add(0);
            List<int> animalCount = new List<int>();


            foreach (var kv in animals)
            {
                int animal = kv.Key;
                int randNum = kv.Value;
                randomValueSum += randNum;
                randomCount.Add(randomValueSum);
                animalCount.Add(animal);
            }
            int rand = RAND.Range(0, randomValueSum);

            for (int i = 0; i < randomCount.Count; i++)
            {
                if (rand >= randomCount[i] && rand <= randomCount[i + 1])
                {
                    retAnimal = animalCount[i];
                    break;
                }
            }


            return retAnimal;
        }

        public static Tuple<int, int> GetMainValueAndCost4Prefix(int prefixId)
        {
            int retValue = 0;
            int cost = 0;
            int randomValueSum = 0;
            int maxMainValue = 0;
            List<int> randomCount = new List<int>();
            randomCount.Add(0);
            List<int> natureValue = new List<int>();


            SoulBonePrefix prefix = GetPrefix(prefixId);
            if (prefix == null)
            {
                prefix = GetPrefix(prefixId - 1);
            }
            if (prefix == null)
            {
                Logger.Log.Warn($"no affix {prefixId} for affix of soulbone");
                return Tuple.Create(retValue, cost);
            }
            int begin = prefix.Section.Begin;
            int end = prefix.Section.End;

            for (int i = begin; i <= end; i++)
            {
                int mainValue = i;
                int randNum = 1;
                randomValueSum += randNum;
                randomCount.Add(randomValueSum);
                natureValue.Add(mainValue);
                if (mainValue > maxMainValue)
                {
                    maxMainValue = mainValue;
                }
            }
            cost = GetMainValueCost(maxMainValue);


            int rand = RAND.Range(0, randomValueSum);

            for (int i = 0; i < randomCount.Count; i++)
            {
                if (rand >= randomCount[i] && rand <= randomCount[i + 1])
                {
                    retValue = natureValue[i];
                    break;
                }
            }

            return Tuple.Create(retValue, cost);
        }

        //public static int GetMaxPrefix()
        //{
        //    //return prefixes.Keys.Max();
        //    return 40;
        //}

        public static SoulBonePrefix GetPrefix(int prefixId)
        {
            SoulBonePrefix prefix = null;
            prefixes.TryGetValue(prefixId, out prefix);
            return prefix;
        }

        public static int GetMainValueCost(int value)
        {
            foreach (var item in costs.Values)
            {
                if (item.Section.InSection(value))
                {
                    return item.Cost;
                }
            }
            return 0;
        }

        public static int GetSoulBoneBestQuality()
        {
            int best = 0;
            foreach (var item in costs.Values)
            {
                if (item.Color > best)
                {
                    best = item.Color;
                }
            }
            return best;
        }

        public static SoulBoneSpecModel GetSpecModel(int id)
        {
            SoulBoneSpecModel model;
            soulBoneSpec.TryGetValue(id, out model);
            return model;
        }

        public static List<SoulBoneSpecModel> GetSpecModel4ItemId(int typeId, List<int> specList)
        {
            SoulBoneSpecModel model = null;
            SoulBoneItemInfo temp = GetItemInfo(typeId);
            if (temp == null && temp.SpecSkills.Count <= 0)
            {
                return null;

            }
            List<SoulBoneSpecModel> soulBoneSpecs = new List<SoulBoneSpecModel>();
            foreach (var kv in specList)
            {
                soulBoneSpec.TryGetValue(kv, out model);
                if (model == null) continue;

                model = model.Clone();
                model.growth = 1;
                soulBoneSpecs.Add(model);
            }
            return soulBoneSpecs;
        }

        public static SoulBoneItemInfo GetItemInfo(int typeId)
        {
            SoulBoneItemInfo temp = null;
            items.TryGetValue(typeId, out temp);
            return temp;
        }

        public static void ProducePrefixByUseItem(SoulBoneInfo info, int typeId, Dictionary<int, Section> locationSpecSubTypeRange)
        {
            SoulBonePrefix soulBonePrefix = null;
            int mainValue = info.MainNature.Value;
            foreach (var item in prefixes)
            {
                if (mainValue <= item.Value.Section.End && mainValue >= item.Value.Section.Begin)
                {
                    soulBonePrefix = item.Value;
                    info.PrefixId = item.Key;
                }
            }

            //随机特殊属性
            ProduceSpecSkillByUseItem(typeId, info, soulBonePrefix, locationSpecSubTypeRange);
        }

        public static void ProduceSpecSkillByUseItem(int typeId, SoulBoneInfo info, SoulBonePrefix soulBonePrefix, Dictionary<int, Section> locationSpecSubTypeRange)
        {
            if (info.SpecSkills.Count > 0) return;

            List<int> idList = ProduceSpecSkillByUseItem(typeId, info.MainNature.Value, soulBonePrefix, locationSpecSubTypeRange);
            if (idList == null || idList.Count == 0) return;

            info.SpecSkills.AddRange(idList);
        }

        public static List<int> ProduceSpecSkillByUseItem(int typeId, int mainNatureValue, SoulBonePrefix soulBonePrefix, Dictionary<int, Section> locationSpecSubTypeRange)
        {
            SoulBoneItemInfo itemInfo = GetItemInfo(typeId);
            if (itemInfo == null) return null;

            if (soulBonePrefix.SpecialNum <= 0) return null;

            List<int> specIds = new List<int>();
            List<int> idList = new List<int>();
        
            if (itemInfo.SpecMainType.Count < soulBonePrefix.SpecialNum || itemInfo.SpecMainType.Count > soulBonePrefix.SpecSubTypeRange.Count)
            {
                Log.Warn($"ProduceSpecSkillByUseItem data error typeid{ typeId} mainNatureValue {mainNatureValue}, specNum {soulBonePrefix.SpecialNum } mainType count {itemInfo.SpecMainType.Count} not equal to subType count {soulBonePrefix.SpecSubTypeRange.Count}");
            }

            idList = ProduceLoactionSpecIdList(typeId, mainNatureValue, soulBonePrefix, itemInfo, locationSpecSubTypeRange);
            return idList;
        }

        public static List<int> ProduceLoactionSpecIdList(int typeId, int mainNatureValue, SoulBonePrefix soulBonePrefix, SoulBoneItemInfo itemInfo, Dictionary<int, Section> locationSpecSubTypeRange)
        {
            List<int> specIds = new List<int>();
            List<int> idList = new List<int>();
            List<int> specNumList = new List<int>();
            for (int i = 1; i <= soulBonePrefix.SpecialNum; i++)
            {
                specNumList.Add(i);
            }

            Dictionary<int, int> randSpecIds = new Dictionary<int, int>();
            if (itemInfo.SpecMainType.Count > 0 && locationSpecSubTypeRange.Count > 0)
            {
                //去除0
                List<int> realSpecMainTypePool = new List<int>();
                foreach (var item in itemInfo.SpecMainType)
                {
                    if (item != 0)
                    {
                        realSpecMainTypePool.Add(item);
                    }
                }

                List<int> tempIds;
                List<int> ruleSpecIds = new List<int>();
                int k = 0;
                int mainType = 0;
                foreach (var item in locationSpecSubTypeRange)
                {
                    k = NewRAND.Next(0, realSpecMainTypePool.Count - 1);
                    mainType = realSpecMainTypePool[k];
                    Section oneSection = item.Value;
                    for (int i = oneSection.Begin; i < oneSection.End; i++)
                    {
                        if (soulBoneSpecPool.TryGetValue(mainType, i, out tempIds))
                        {
                            ruleSpecIds.AddRange(tempIds);
                        }
                    }
                    k = NewRAND.Next(0, ruleSpecIds.Count - 1);
                    randSpecIds.Add(item.Key, ruleSpecIds[k]);
                    ruleSpecIds.Clear();
                    if (specNumList.Contains(item.Key))
                    {
                        specNumList.Remove(item.Key);
                    }
                }
                int zeroSpecId;
                if (specNumList.Count > 0 && randSpecIds.TryGetValue(0, out zeroSpecId))
                {
                    k = NewRAND.Next(0, specNumList.Count - 1);
                    int randLocation = specNumList[k];
                    randSpecIds.Add(randLocation, zeroSpecId);
                    randSpecIds.Remove(0);
                }
            }
            for (int i = 0; i < itemInfo.SpecMainType.Count; i++)
            {
                if (itemInfo.SpecMainType.Count <= i || soulBonePrefix.SpecSubTypeRange.Count <= i) continue;

                List<int> ids;
                Section section = soulBonePrefix.SpecSubTypeRange[i];
                for (int j = section.Begin; j < section.End; j++)
                {
                    if (soulBoneSpecPool.TryGetValue(itemInfo.SpecMainType[i], j, out ids))
                    {
                        specIds.AddRange(ids);
                    }
                }
            }

            if (specIds.Count == 0)
            {
                //没有合适的池子
                Log.Warn($"ProduceSpecSkill data error typeid{ typeId} mainNatureValue {mainNatureValue}, mainType count {itemInfo.SpecMainType.Count}  subType {soulBonePrefix.SpecSubTypeRange.Count} selected pool count {specIds.Count}");
                return null;
            }

            int maxIndex = specIds.Count - 1;
            if (randSpecIds.Count > 0)
            {
                int locationSpecId;
                for (int i = 1; i <= soulBonePrefix.SpecialNum; i++)
                {
                    if (randSpecIds.TryGetValue(i, out locationSpecId))
                    {
                        idList.Add(locationSpecId);
                    }
                    else
                    {
                        int index = RAND.Range(0, maxIndex);
                        idList.Add(specIds[index]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < soulBonePrefix.SpecialNum; i++)
                {
                    int index = RAND.Range(0, maxIndex);
                    idList.Add(specIds[index]);
                }
            }
            return idList;
        }
        #endregion

        public static ItemBasicInfo GetLockCost(int num)
        {
            if (num >= lockCostList.Count) return lockCostList[lockCostList.Count - 1];
            return lockCostList[num - 1];
        }
    }
}

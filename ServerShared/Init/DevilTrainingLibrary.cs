using CommonUtility;
using DataProperty;
using Logger;
using ServerModels.HidderWeapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class DevilTrainingLibrary
    {
        private static Dictionary<int, DevilTrainingConfig> devilTrainingConfigs = new Dictionary<int, DevilTrainingConfig>();
        private static Dictionary<int, DevilTrainingRewardModel> devilTrainingRewardDic = new Dictionary<int, DevilTrainingRewardModel>();
        private static Dictionary<int, DevilTrainingCumulativeRewardModel> devilTrainingCumulativeRewardDic = new Dictionary<int, DevilTrainingCumulativeRewardModel>();
        private static Dictionary<int, DevilTrainingBuffModel> devilTrainingbuffDic = new Dictionary<int, DevilTrainingBuffModel>();

        //<period, <type, <grade, List<model>>> > 普通奖励
        private static Dictionary<int, Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>>> PeriodTypeGradeDic = new Dictionary<int, Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>>>();
        //<period, idList > 累计奖励
        private static Dictionary<int, List<DevilTrainingCumulativeRewardModel>> PeriodDic = new Dictionary<int, List<DevilTrainingCumulativeRewardModel>>();
        public static void Init()
        {
            InitDevilTrainingConfig();
            InitDevilTrainingRewardModel();
            InitDevilTrainingCumulativeRewardModel();
            InitDevilTrainingBuff();
            InitPeriodTypeDic();
            InitPeriodDic();
        }

        private static void InitDevilTrainingConfig()
        {
            Dictionary<int, DevilTrainingConfig> devilTrainingConfigs = new Dictionary<int, DevilTrainingConfig>();
            DataList dataList = DataListManager.inst.GetDataList("DevilTraining");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DevilTrainingConfig info = new DevilTrainingConfig(data);
                devilTrainingConfigs.Add(info.Type, info);
            }
            DevilTrainingLibrary.devilTrainingConfigs = devilTrainingConfigs;
        }

        private static void InitDevilTrainingRewardModel()
        {
            Dictionary<int, DevilTrainingRewardModel> devilTrainingRewardDic = new Dictionary<int, DevilTrainingRewardModel>();
            DataList dataList = DataListManager.inst.GetDataList("DevilTrainingReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DevilTrainingRewardModel info = new DevilTrainingRewardModel(data);
                devilTrainingRewardDic.Add(info.Id, info);
            }
            DevilTrainingLibrary.devilTrainingRewardDic = devilTrainingRewardDic;
        }
        private static void InitDevilTrainingCumulativeRewardModel()
        {
            Dictionary<int, DevilTrainingCumulativeRewardModel> devilTrainingCumulativeRewardDic = new Dictionary<int, DevilTrainingCumulativeRewardModel>();
            DataList dataList = DataListManager.inst.GetDataList("DevilTrainingCumulativeReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DevilTrainingCumulativeRewardModel info = new DevilTrainingCumulativeRewardModel(data);
                devilTrainingCumulativeRewardDic.Add(info.Id, info);
            }
            DevilTrainingLibrary.devilTrainingCumulativeRewardDic = devilTrainingCumulativeRewardDic;
        }

        private static void InitDevilTrainingBuff()
        {
            Dictionary<int, DevilTrainingBuffModel> devilTrainingbuffDic = new Dictionary<int, DevilTrainingBuffModel>();
            DataList dataList = DataListManager.inst.GetDataList("DevilTrainingBuff");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                DevilTrainingBuffModel info = new DevilTrainingBuffModel(data);
                devilTrainingbuffDic.Add(info.Id, info);
            }
            DevilTrainingLibrary.devilTrainingbuffDic = devilTrainingbuffDic;
        }

        public static int GetSuperRewardNum(int type)
        {
            DevilTrainingConfig config;
            devilTrainingConfigs.TryGetValue(type, out config);
            int num;
            if (config.SuperRewardNumRatioDic == null || config.SuperRewardNumRatioDic.Count == 0)
            {
                return -1;
            }
            num = RAND.RandValue(config.SuperRewardNumRatioDic, config.SuperNumRatioSum);
            return num;
        }
        public static int GetHighRewardNum(int type)
        {
            DevilTrainingConfig config;
            devilTrainingConfigs.TryGetValue(type, out config);
            int num;
            if (config.HighRewardNumRatioDic == null || config.HighRewardNumRatioDic.Count == 0)
            {
                return -1;
            }
            num = RAND.RandValue(config.HighRewardNumRatioDic, config.HighNumRatioSum);
            return num;
        }
        public static int GetMidRewardNum(int type)
        {
            DevilTrainingConfig config;
            devilTrainingConfigs.TryGetValue(type, out config);
            int num;
            if (config.MidRewardNumRatioDic == null || config.MidRewardNumRatioDic.Count == 0)
            {
                return -1;
            }
            num = RAND.RandValue(config.MidRewardNumRatioDic, config.MidNumRatioSum);
            return num;
        }

        

        public static DevilTrainingConfig GetDevilTrainingConfig(int type)
        {
            DevilTrainingConfig config;
            devilTrainingConfigs.TryGetValue(type, out config);
            return config;
        }
        public static DevilTrainingRewardModel GetDevilTrainingRewardModel(int id)
        {
            DevilTrainingRewardModel config;
            devilTrainingRewardDic.TryGetValue(id, out config);
            return config;
        }
        public static DevilTrainingCumulativeRewardModel GetDevilTrainingCumulativeRewardModel(int id)
        {
            DevilTrainingCumulativeRewardModel config;
            devilTrainingCumulativeRewardDic.TryGetValue(id, out config);
            return config;
        }
        public static DevilTrainingBuffModel GetDevilTrainingBuff(int id)
        {
            DevilTrainingBuffModel config;
            devilTrainingbuffDic.TryGetValue(id, out config);
            return config;
        }


        //public static void InitPeriodTypeDic()
        //{
        //    foreach (DevilTrainingRewardModel model in devilTrainingRewardDic.Values)
        //    {
        //        List<DevilTrainingRewardModel> list = new List<DevilTrainingRewardModel>();
        //        Dictionary<int, List<DevilTrainingRewardModel>> typeDic = new Dictionary<int, List<DevilTrainingRewardModel>>();
        //        if (!PeriodTypeDic.TryGetValue(model.Period, out typeDic))
        //        {
        //            PeriodTypeDic.Add(model.Period, typeDic);
        //            if (!typeDic.TryGetValue(model.Type, out list))
        //            {
        //                typeDic.Add(model.Type, list);
        //            }
        //            else
        //            {
        //                if (!list.Contains(model))
        //                {
        //                    list.Add(model);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!typeDic.TryGetValue(model.Type, out list))
        //            {
        //                typeDic.Add(model.Type, list);
        //            }
        //            else
        //            {
        //                if (!list.Contains(model))
        //                {
        //                    list.Add(model);
        //                }
        //            }
        //        }
        //    }
        //}


        public static void InitPeriodTypeDic()
        {
            List<DevilTrainingRewardModel> list;
            Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>> typeDic;
            Dictionary<int, List<DevilTrainingRewardModel>> gradeDic;
            foreach (DevilTrainingRewardModel model in devilTrainingRewardDic.Values)
            {
                if (PeriodTypeGradeDic.TryGetValue(model.Period, out typeDic))
                {
                    if (typeDic.TryGetValue(model.Type, out gradeDic))
                    {
                        if (gradeDic.TryGetValue(model.Grade, out list))
                        {
                            bool isAdd = true;
                            foreach (DevilTrainingRewardModel temp in list)
                            {
                                if (temp.Id != model.Id)
                                {
                                    continue;
                                }
                                isAdd = false;
                            }
                            if (isAdd)
                            {
                                list.Add(model);
                            }
                        }
                        else
                        {
                            list = new List<DevilTrainingRewardModel>();
                            list.Add(model);
                            gradeDic.Add(model.Grade, list);
                        }
                    }
                    else
                    {
                        gradeDic = new Dictionary<int, List<DevilTrainingRewardModel>>();
                        list = new List<DevilTrainingRewardModel>();
                        list.Add(model);
                        gradeDic.Add(model.Grade, list);
                        typeDic.Add(model.Type, gradeDic);
                    }
                }
                else
                {
                    typeDic = new Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>>();
                    gradeDic = new Dictionary<int, List<DevilTrainingRewardModel>>();
                    list = new List<DevilTrainingRewardModel>();
                    list.Add(model);
                    gradeDic.Add(model.Grade, list);
                    typeDic.Add(model.Type, gradeDic);
                    PeriodTypeGradeDic.Add(model.Period, typeDic);
                }
            }

        }

        //public static void InitPeriodTypeDic()
        //{
        //    foreach (DevilTrainingRewardModel model in devilTrainingRewardDic.Values)
        //    {
        //        List<DevilTrainingRewardModel> list;
        //        Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>> typeDic;
        //        Dictionary<int, List<DevilTrainingRewardModel>> gradeDic;

        //        if (!PeriodTypeGradeDic.TryGetValue(model.Period, out typeDic))
        //        {
        //            typeDic = new Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>>();
        //            PeriodTypeGradeDic.Add(model.Period, typeDic);
        //            if (!typeDic.TryGetValue(model.Type, out gradeDic))
        //            {
        //                gradeDic = new Dictionary<int, List<DevilTrainingRewardModel>>();
        //                typeDic.Add(model.Type, gradeDic);
        //            }
        //            else
        //            {
        //                if (!gradeDic.TryGetValue(model.Grade, out list))
        //                {
        //                    list = new List<DevilTrainingRewardModel>();
        //                    gradeDic.Add(model.Grade, list);
        //                }
        //                else
        //                {
        //                    list.Add(model);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!typeDic.TryGetValue(model.Type, out gradeDic))
        //            {
        //                gradeDic = new Dictionary<int, List<DevilTrainingRewardModel>>();
        //                typeDic.Add(model.Type, gradeDic);
        //            }
        //            else
        //            {
        //                if (!gradeDic.TryGetValue(model.Grade, out list))
        //                {
        //                    list = new List<DevilTrainingRewardModel>();
        //                    gradeDic.Add(model.Grade, list);
        //                }
        //                else
        //                {
        //                    list.Add(model);
        //                }
        //            }
        //        }
        //    }
        //}

        public static void InitPeriodDic()
        {
            List<DevilTrainingCumulativeRewardModel> idList;
            foreach (DevilTrainingCumulativeRewardModel model in devilTrainingCumulativeRewardDic.Values)
            {
                if (!PeriodDic.TryGetValue(model.Period, out idList))
                {
                    idList = new List<DevilTrainingCumulativeRewardModel>();
                    PeriodDic.Add(model.Period, idList);
                }
                else
                {
                    if (!idList.Contains(model))
                    {
                        idList.Add(model);
                    }
                }
            }
        }

        public static List<DevilTrainingCumulativeRewardModel> GetCumulativePeriodList(int period)
        {
            List<DevilTrainingCumulativeRewardModel> list;
            if (PeriodDic.TryGetValue(period, out list))
            {
                return list;
            }
            return null;
        }

        public static List<DevilTrainingRewardModel> GetGradeList(int type, int period, int grade)
        {
            List<DevilTrainingRewardModel> list;
            Dictionary<int, Dictionary<int, List<DevilTrainingRewardModel>>> typeDic;
            Dictionary<int, List<DevilTrainingRewardModel>> gradeDic;
            if (PeriodTypeGradeDic.TryGetValue(period, out typeDic))
            {
                if (typeDic.TryGetValue(type, out gradeDic))
                {
                    if (gradeDic.TryGetValue(grade, out list))
                    {
                        return list;
                    }
                }
            }
            return null;
        }
    }
}

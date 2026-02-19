using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels;
using ServerModels.HidderWeapon;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DevilTrainingManager
    {
        private PlayerChar owner;
        private Dictionary<int, DevilTrainingInfo> devilTrainingList = new Dictionary<int, DevilTrainingInfo>();
        private int period;
        public int Period { get { return period; } }

        private int point;
        public int Point { get { return point; } }

        private int buffId;
        public int BuffId { get { return buffId; } }

        private Dictionary<int, int> firstRewards;
        private Dictionary<int, int> secondRewards;
        private Dictionary<int, int> thirdRewards;
        private Dictionary<int, int> fourthRewards;
        private List<int> receivePointRewards;

        public DevilTrainingManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(Dictionary<int, DevilTrainingInfo> list)
        {
            foreach (var item in list)
            {
                devilTrainingList.Add(item.Key, item.Value);
            }
        }

        public MSG_ZGC_DEVIL_TRAINING_INFO InitDevilTrainingInfoMsg()
        {
            MSG_ZGC_DEVIL_TRAINING_INFO msg = new MSG_ZGC_DEVIL_TRAINING_INFO();
            RechargeGiftModel model;
            if (devilTrainingList == null || devilTrainingList.Count == 0)
            {
                DevilTrainingInfo normalInfo = new DevilTrainingInfo();

                if (RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.DevilTraining, BaseApi.now, out model))
                {
                    normalInfo.Period = model.SubType;
                }
                normalInfo.Type = 1;
                normalInfo.FirstRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 0);
                normalInfo.SecondRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 5);
                normalInfo.ThirdRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 10);
                normalInfo.FourthRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 15);
                normalInfo.BuffId = 1;
                SyncDbInsertDevilTraining(normalInfo.Type, normalInfo.Period, normalInfo.FirstRewardsDic, normalInfo.SecondRewardsDic, normalInfo.ThirdRewardsDic, normalInfo.FourthRewardsDic);
                devilTrainingList.Add(normalInfo.Type, normalInfo);
                DevilTrainingInfo highInfo = new DevilTrainingInfo();
                highInfo.Type = 2;
                highInfo.Period = model.SubType;
                highInfo.FirstRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 0);
                highInfo.SecondRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 5);
                highInfo.ThirdRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 10);
                highInfo.FourthRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 15);
                highInfo.BuffId = 1;
                SyncDbInsertDevilTraining(highInfo.Type, highInfo.Period, highInfo.FirstRewardsDic, highInfo.SecondRewardsDic, highInfo.ThirdRewardsDic, highInfo.FourthRewardsDic);
                devilTrainingList.Add(highInfo.Type, highInfo);
            }
            else
            {
                DevilTrainingInfo info = devilTrainingList[1];
                RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.DevilTraining, BaseApi.now, out model);
                if (info.Period != model.SubType)
                {
                    Clear();
                    return ClearDevilTrainingInfoMsg();
                }
            }

            FillDevilTrainingInfo(msg);


            return msg;
        }


        public MSG_ZGC_DEVIL_TRAINING_INFO ClearDevilTrainingInfoMsg()
        {
            MSG_ZGC_DEVIL_TRAINING_INFO msg = new MSG_ZGC_DEVIL_TRAINING_INFO();

            devilTrainingList.Clear();
            DevilTrainingInfo normalInfo = new DevilTrainingInfo();
            RechargeGiftModel model;
            if (RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.DevilTraining, BaseApi.now, out model))
            {
                normalInfo.Period = model.SubType;
            }
            normalInfo.Type = 1;
            normalInfo.FirstRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 0);
            normalInfo.SecondRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 5);
            normalInfo.ThirdRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 10);
            normalInfo.FourthRewardsDic = GetRewardsIndex(normalInfo.Type, normalInfo.Period, 15);
            normalInfo.BuffId = 1;
            SyncDbUpdateInitInfo(normalInfo.Type, normalInfo.Period, normalInfo.FirstRewardsDic, normalInfo.SecondRewardsDic, normalInfo.ThirdRewardsDic, normalInfo.FourthRewardsDic);
            devilTrainingList.Add(normalInfo.Type, normalInfo);
            DevilTrainingInfo highInfo = new DevilTrainingInfo();
            highInfo.Type = 2;
            highInfo.Period = model.SubType;
            highInfo.FirstRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 0);
            highInfo.SecondRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 5);
            highInfo.ThirdRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 10);
            highInfo.FourthRewardsDic = GetRewardsIndex(highInfo.Type, highInfo.Period, 15);
            highInfo.BuffId = 1;
            SyncDbUpdateInitInfo(highInfo.Type, highInfo.Period, highInfo.FirstRewardsDic, highInfo.SecondRewardsDic, highInfo.ThirdRewardsDic, highInfo.FourthRewardsDic);
            devilTrainingList.Add(highInfo.Type, highInfo);
            FillDevilTrainingInfo(msg);
            return msg;

        }

        private void FillDevilTrainingInfo(MSG_ZGC_DEVIL_TRAINING_INFO msg)
        {
            foreach (DevilTrainingInfo info in devilTrainingList.Values)
            {
                if (info.FirstRewardsDic.Count == 0)
                {
                    info.FirstRewardsDic = GetRewardsIndex(info.Type, info.Period, 0);
                }
                if (info.SecondRewardsDic.Count == 0)
                {
                    info.SecondRewardsDic = GetRewardsIndex(info.Type, info.Period, 5);
                }
                if (info.ThirdRewardsDic.Count == 0)
                {
                    info.ThirdRewardsDic = GetRewardsIndex(info.Type, info.Period, 10);
                }
                if (info.FourthRewardsDic.Count == 0)
                {
                    info.FourthRewardsDic = GetRewardsIndex(info.Type, info.Period, 15);
                }

            }
            foreach (DevilTrainingInfo info in devilTrainingList.Values)
            {
                List<DEVIL_TRAINING_INFO> list = new List<DEVIL_TRAINING_INFO>();
                list.AddRange(FillDevilTrainingDicInfo(info.FirstRewardsDic, info.Type));
                list.AddRange(FillDevilTrainingDicInfo(info.SecondRewardsDic, info.Type));
                list.AddRange(FillDevilTrainingDicInfo(info.ThirdRewardsDic, info.Type));
                list.AddRange(FillDevilTrainingDicInfo(info.FourthRewardsDic, info.Type));
                msg.RewardInfo.AddRange(list);
                if (info.Type == 2)
                {
                    msg.Point = info.Point;
                    msg.BuffId = info.BuffId;
                    msg.ReceviedPointRewardList.AddRange(info.ReceivePointRewardsList);
                }
            }
        }
        //获得奖池的一个区域
        public Dictionary<int, int> GetReawardsDic(List<DevilTrainingRewardModel> list, int baseIndex, int num)
        {
            Dictionary<int, int> indexDic = new Dictionary<int, int>();
            Dictionary<int, int> idWeightDic = new Dictionary<int, int>();
            int sum = 0;
            foreach (DevilTrainingRewardModel model in list)
            {
                idWeightDic.Add(model.Id, model.Ratio);
                sum += model.Ratio;

            }
            
            for (int i = baseIndex; i < num + 1; i++)
            {
                int id = RAND.RandValue(idWeightDic, sum);
                indexDic.Add(i, id);
            }
            return indexDic;
        }
        //乱序Dic
        private Dictionary<int, int> RandomDic(Dictionary<int, int> indexDic, int partIndex)
        {
            List<int> idList = indexDic.Values.ToList();
            Dictionary<int, int> newIndexDic = new Dictionary<int, int>();
            for (int i = 0; i < idList.Count; i++)
            {
                int rand = new Random().Next(0, idList.Count);
                int temp = idList[i];
                idList[i] = idList[rand];
                idList[rand] = temp;
            }
            for (int i = 0; i < idList.Count; i++)
            {
                newIndexDic.Add(i + 1 + partIndex, idList[i]);
            }
            return newIndexDic;
        }

        public Dictionary<int, int> GetRewardsIndex(int type, int period, int partIndex)
        {
            Dictionary<int, int> indexDic = new Dictionary<int, int>();
            DevilTrainingConfig config = DevilTrainingLibrary.GetDevilTrainingConfig(type);
            int sum = 0;
            if (config == null)
            {
                Log.Warn($"deviltraining config is null, type{0}", type);
                return null;
            }
            int superNum = DevilTrainingLibrary.GetSuperRewardNum(type);
            sum += superNum;
            if (superNum != 0)
            {
                List<DevilTrainingRewardModel> superList = DevilTrainingLibrary.GetGradeList(type, period, 4);
                Dictionary<int, int> superDic = new Dictionary<int, int>();
                superDic = GetReawardsDic(superList, 1, superNum);
                foreach (var kvpair in superDic)
                {
                    indexDic.Add(kvpair.Key, kvpair.Value);
                }
            }
            int highNum = DevilTrainingLibrary.GetHighRewardNum(type);
            sum += highNum;
            if (sum > 5)
            {
                highNum = 5 - superNum;
            }
            List<DevilTrainingRewardModel> highList = DevilTrainingLibrary.GetGradeList(type, period, 3);
            Dictionary<int, int> highDic = new Dictionary<int, int>();
            highDic = GetReawardsDic(highList, superNum + 1, superNum + highNum);
            foreach (var kvpair in highDic)
            {
                indexDic.Add(kvpair.Key, kvpair.Value);
            }
            if (indexDic.Count == 5)
            {
                indexDic = RandomDic(indexDic, partIndex);
                return indexDic;
            }
            int midNum = DevilTrainingLibrary.GetMidRewardNum(type);
            sum += midNum;
            if (sum > 5)
            {
                midNum = 5 - superNum - highNum;
            }
            List<DevilTrainingRewardModel> midList = DevilTrainingLibrary.GetGradeList(type, period, 2);
            Dictionary<int, int> midDic = new Dictionary<int, int>();
            midDic = GetReawardsDic(midList, superNum + highNum + 1, superNum + highNum + midNum);
            foreach (var kvpair in midDic)
            {
                indexDic.Add(kvpair.Key, kvpair.Value);
            }
            if (indexDic.Count == 5)
            {
                indexDic = RandomDic(indexDic, partIndex);
                return indexDic;
            }
            int lowNum = 5 - (superNum + highNum + midNum);
            if (lowNum >= 0)
            {
                List<DevilTrainingRewardModel> lowList = DevilTrainingLibrary.GetGradeList(type, period, 1);
                Dictionary<int, int> lowDic = new Dictionary<int, int>();
                lowDic = GetReawardsDic(midList, superNum + highNum + midNum + 1, superNum + highNum + midNum + lowNum);
                
                foreach (var kvpair in lowDic)
                {
                    indexDic.Add(kvpair.Key, kvpair.Value);
                }
            }
            indexDic = RandomDic(indexDic, partIndex);
            return indexDic;
        }

        private Dictionary<int, int> UnloadingDic(Dictionary<int, int> mainDic, Dictionary<int, int> tempDic)
        {
            foreach (var kvpair in tempDic)
            {
                mainDic.Add(kvpair.Key, kvpair.Value);
            }
            return mainDic;
        }


        public Dictionary<int, int> GetAllRewards(int type, int period)
        {
            DevilTrainingInfo info = devilTrainingList[type];
            if (info == null)
            {
                return null;
            }
            Dictionary<int, int> allRewardsDic = new Dictionary<int, int>();
            allRewardsDic = UnloadingDic(allRewardsDic, info.FirstRewardsDic);
            allRewardsDic = UnloadingDic(allRewardsDic, info.SecondRewardsDic);
            allRewardsDic = UnloadingDic(allRewardsDic, info.ThirdRewardsDic);
            allRewardsDic = UnloadingDic(allRewardsDic, info.FourthRewardsDic);
            return allRewardsDic;

        }

        public Dictionary<int, int> GetNewRewards(List<int> indexList, int type, out Dictionary<int, int> buffCountDic)
        {
            Dictionary<int, int> newRewardsDic = new Dictionary<int, int>();
            Dictionary<int, int> tempRewardsDic = new Dictionary<int, int>();
            buffCountDic = null;
            bool firstRefresh = false ;
            bool secondRefresh = false;
            bool thirdRefresh = false;
            bool fourthRefresh = false;
            List<int> firstList = new List<int>();
            List<int> secondList = new List<int>();
            List<int> thirdList = new List<int>();
            List<int> fourthList = new List<int>();
            DevilTrainingInfo info;
            if (!devilTrainingList.TryGetValue(type, out info))
            {
                return newRewardsDic;
            }
            firstRewards = info.FirstRewardsDic;
            secondRewards = info.SecondRewardsDic;
            thirdRewards = info.ThirdRewardsDic;
            fourthRewards = info.FourthRewardsDic;
            period = info.Period;

            foreach (int index in indexList)
            {
                if (index > 0 && index <= 5)
                {
                    firstRewards.Remove(index);
                    firstList.Add(index);
                    firstRefresh = true;
                }
                else if (index > 5 && index <= 10)
                {
                    secondRewards.Remove(index);
                    secondList.Add(index);
                    secondRefresh = true;
                }
                else if (index > 10 && index <= 15)
                {
                    thirdRewards.Remove(index);
                    thirdList.Add(index);
                    thirdRefresh = true;
                }
                else
                {
                    fourthRewards.Remove(index);
                    fourthList.Add(index);
                    fourthRefresh = true;
                }
            }
            if (firstRefresh)
            {
                tempRewardsDic = RefreshRewards(firstList, 1, type, out buffCountDic);
                foreach (var temp in tempRewardsDic)
                {
                    newRewardsDic.Add(temp.Key, temp.Value);
                }
            }
            if (secondRefresh)
            {
                tempRewardsDic = RefreshRewards(secondList, 2, type, out buffCountDic);
                foreach (var temp in tempRewardsDic)
                {
                    newRewardsDic.Add(temp.Key, temp.Value);
                }
            }
            if (thirdRefresh)
            {
                tempRewardsDic = RefreshRewards(thirdList, 3, type, out buffCountDic);
                foreach (var temp in tempRewardsDic)
                {
                    newRewardsDic.Add(temp.Key, temp.Value);
                }
            }
            if (fourthRefresh)
            {
                tempRewardsDic = RefreshRewards(fourthList, 4, type, out buffCountDic);
                foreach (var temp in tempRewardsDic)
                {
                    newRewardsDic.Add(temp.Key, temp.Value);
                }
            }
            return newRewardsDic;
        }

        private Dictionary<int, int> RefreshRewards(List<int> partList, int part, int type, out Dictionary<int, int> buffCountDic)
        {
            Dictionary<int, int> newRewardsDic = new Dictionary<int, int>();
            Dictionary<int, int> tempRewardsDic = new Dictionary<int, int>();
            DevilTrainingConfig config = DevilTrainingLibrary.GetDevilTrainingConfig(type);
            // TODO List  ID grade
            int superNum = 0;
            int highNum = 0;
            int midNum = 0;

            switch (part)
            {
                case 1:
                    foreach (int id in firstRewards.Values)
                    {
                        DevilTrainingRewardModel model = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                        if (model.Grade == 4)
                        {
                            superNum += 1;
                            continue;
                        }
                        if (model.Grade == 3)
                        {
                            highNum += 1;
                            continue;
                        }
                        if (model.Grade == 2)
                        {
                            midNum += 1;
                            continue;
                        }
                    }
                    int sum = superNum + highNum + midNum;
                    sum = 0;
                    
                    buffCountDic = FillNewRewardsDic(partList, config, type, superNum, highNum, midNum, newRewardsDic);
                    return newRewardsDic;

                case 2:
                    foreach (int id in secondRewards.Values)
                    {
                        DevilTrainingRewardModel model = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                        if (model.Grade == 4)
                        {
                            superNum += 1;
                            continue;
                        }
                        if (model.Grade == 3)
                        {
                            highNum += 1;
                            continue;
                        }
                        if (model.Grade == 2)
                        {
                            midNum += 1;
                            continue;
                        }
                    }
                    sum = superNum + highNum + midNum;
                    sum = 0;
                    buffCountDic = FillNewRewardsDic(partList, config, type, superNum, highNum, midNum, newRewardsDic);
                    return newRewardsDic;
                case 3:
                    foreach (int id in thirdRewards.Values)
                    {
                        DevilTrainingRewardModel model = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                        if (model.Grade == 4)
                        {
                            superNum += 1;
                            continue;
                        }
                        if (model.Grade == 3)
                        {
                            highNum += 1;
                            continue;
                        }
                        if (model.Grade == 2)
                        {
                            midNum += 1;
                            continue;
                        }
                    }
                    sum = superNum + highNum + midNum;
                    sum = 0;
                    buffCountDic = FillNewRewardsDic(partList, config, type, superNum, highNum, midNum, newRewardsDic);
                    return newRewardsDic;
                case 4:
                    foreach (int id in fourthRewards.Values)
                    {
                        DevilTrainingRewardModel model = DevilTrainingLibrary.GetDevilTrainingRewardModel(id);
                        if (model.Grade == 4)
                        {
                            superNum += 1;
                            continue;
                        }
                        if (model.Grade == 3)
                        {
                            highNum += 1;
                            continue;
                        }
                        if (model.Grade == 2)
                        {
                            midNum += 1;
                            continue;
                        }
                    }
                    sum = superNum + highNum + midNum;
                    sum = 0;
                    buffCountDic = FillNewRewardsDic(partList, config, type, superNum, highNum, midNum, newRewardsDic);
                    return newRewardsDic;
                default:
                    buffCountDic = null;
                    return null;
            }
        }

        public void SetNewRewards(Dictionary<int, int> baseNewRewards, Dictionary<int, int> buffNewRewards)
        {
            foreach (var basePair in baseNewRewards)
            {
                int id;
                if (basePair.Key > 0 && basePair.Key <= 5)
                {
                    if (!firstRewards.TryGetValue(basePair.Key, out id))
                    {
                        firstRewards.Add(basePair.Key, basePair.Value);
                    }
                }
                if (basePair.Key > 5 && basePair.Key <= 10)
                {
                    if (!secondRewards.TryGetValue(basePair.Key, out id))
                    {
                        secondRewards.Add(basePair.Key, basePair.Value);
                    }
                }
                if (basePair.Key > 10 && basePair.Key <= 15)
                {
                    if (!thirdRewards.TryGetValue(basePair.Key, out id))
                    {
                        thirdRewards.Add(basePair.Key, basePair.Value);
                    }
                }
                if (basePair.Key > 15 && basePair.Key <= 20)
                {
                    if (!fourthRewards.TryGetValue(basePair.Key, out id))
                    {
                        fourthRewards.Add(basePair.Key, basePair.Value);
                    }
                }
            }
            foreach (var buffPair in buffNewRewards)
            {
                int id;
                if (buffPair.Key > 0 && buffPair.Key <= 5)
                {
                    if (!firstRewards.TryGetValue(buffPair.Key, out id))
                    {
                        firstRewards.Add(buffPair.Key, buffPair.Value);
                    }
                }
                if (buffPair.Key > 5 && buffPair.Key <= 10)
                {
                    if (!secondRewards.TryGetValue(buffPair.Key, out id))
                    {
                        secondRewards.Add(buffPair.Key, buffPair.Value);
                    }
                }
                if (buffPair.Key > 10 && buffPair.Key <= 15)
                {
                    if (!thirdRewards.TryGetValue(buffPair.Key, out id))
                    {
                        thirdRewards.Add(buffPair.Key, buffPair.Value);
                    }
                }
                if (buffPair.Key > 15 && buffPair.Key <= 20)
                {
                    if (!fourthRewards.TryGetValue(buffPair.Key, out id))
                    {
                        fourthRewards.Add(buffPair.Key, buffPair.Value);
                    }
                }
            }
        }

        private Dictionary<int, int> FillNewRewardsDic(List<int> partList, DevilTrainingConfig config, int type, int superNum, int highNum, int midNum, Dictionary<int, int> newRewardsDic)
        {

            Dictionary<int, int> buffCountDic = new Dictionary<int, int>();
            for (int i = 0; i < partList.Count; i++)
            {
                if (newRewardsDic.ContainsKey(partList[i]))
                {
                    if (buffCountDic.ContainsKey(partList[i]))
                    {
                        buffCountDic[partList[i]] += 1;
                        continue;
                    }
                    buffCountDic.Add(partList[i], 0);
                    continue;
                }
                int sumNum = superNum + highNum + midNum;
                if (superNum < config.SuperRewardMaxNum)
                {
                    int temp = DevilTrainingLibrary.GetSuperRewardNum(type);
                    if (config.SuperRewardMaxNum >= temp && temp > superNum)
                    {
                        superNum += 1;
                        Dictionary<int, int> idWeightDic = new Dictionary<int, int>();
                        List<DevilTrainingRewardModel> list = DevilTrainingLibrary.GetGradeList(type, period, 4);
                        int sum = 0;
                        foreach (DevilTrainingRewardModel model in list)
                        {
                            idWeightDic.Add(model.Id, model.Ratio);
                            sum += model.Ratio;

                        }
                        int id = RAND.RandValue(idWeightDic, sum);
                        newRewardsDic.Add(partList[i], id);
                        continue;
                    }
                }
                if (highNum < config.HighRewardMaxNum && sumNum < 5)
                {
                    int temp = DevilTrainingLibrary.GetHighRewardNum(type);
                    if (config.HighRewardMaxNum >= temp && temp > highNum)
                    {
                        highNum += 1;
                        Dictionary<int, int> idWeightDic = new Dictionary<int, int>();
                        List<DevilTrainingRewardModel> list = DevilTrainingLibrary.GetGradeList(type, period, 3);
                        int sum = 0;
                        foreach (DevilTrainingRewardModel model in list)
                        {
                            idWeightDic.Add(model.Id, model.Ratio);
                            sum += model.Ratio;

                        }
                        int id = RAND.RandValue(idWeightDic, sum);
                        newRewardsDic.Add(partList[i], id);
                        continue;
                    }
                }
                if (midNum < config.MidRewardMaxNum && sumNum < 5)
                {
                    int temp = DevilTrainingLibrary.GetMidRewardNum(type);
                    if (config.MidRewardMaxNum >= temp && temp > midNum)
                    {
                        midNum += 1;
                        Dictionary<int, int> idWeightDic = new Dictionary<int, int>();
                        List<DevilTrainingRewardModel> list = DevilTrainingLibrary.GetGradeList(type, period, 2);
                        int sum = 0;
                        foreach (DevilTrainingRewardModel model in list)
                        {
                            idWeightDic.Add(model.Id, model.Ratio);
                            sum += model.Ratio;

                        }
                        int id = RAND.RandValue(idWeightDic, sum);
                        newRewardsDic.Add(partList[i], id);
                        continue;
                    }
                }
                if (sumNum < 5)
                {
                    Dictionary<int, int> idWeightDic = new Dictionary<int, int>();
                    List<DevilTrainingRewardModel> list = DevilTrainingLibrary.GetGradeList(type, period, 1);
                    int sum = 0;
                    foreach (DevilTrainingRewardModel model in list)
                    {
                        idWeightDic.Add(model.Id, model.Ratio);
                        sum += model.Ratio;

                    }
                    int id = RAND.RandValue(idWeightDic, sum);
                    newRewardsDic.Add(partList[i], id);
                    continue;
                }
            }
            return buffCountDic;
        }

        public List<DEVIL_TRAINING_INFO> FillDevilTrainingDicInfo(Dictionary<int, int> dic,int type)
        {
            List<DEVIL_TRAINING_INFO> list = new List<DEVIL_TRAINING_INFO>();
            foreach (KeyValuePair<int, int> kvpair in dic)
            {
                DEVIL_TRAINING_INFO subInfo = new DEVIL_TRAINING_INFO();
                subInfo.Type = type;
                subInfo.Index = kvpair.Key;
                subInfo.Id = kvpair.Value;
                list.Add(subInfo);
            }
            return list;
        }
        private List<DEVIL_TRAINING_INDEX_INFO> FillZMZDevilTrainingInfo(Dictionary<int, int> dic, int part)
        {
            List<DEVIL_TRAINING_INDEX_INFO> list = new List<DEVIL_TRAINING_INDEX_INFO>();
            foreach (KeyValuePair<int, int> kvpair in dic)
            {
                DEVIL_TRAINING_INDEX_INFO subInfo = new DEVIL_TRAINING_INDEX_INFO();
                subInfo.Part = part;
                subInfo.Index = kvpair.Key;
                subInfo.Id = kvpair.Value;
                list.Add(subInfo);
            }
            return list;
        }


        public void Clear()
        {
            firstRewards = null;
            secondRewards = null;
            thirdRewards = null;
            fourthRewards = null;
            receivePointRewards = new List<int>();
            buffId = 1;
            point = 0;
        }

        public DevilTrainingInfo GetDevilTrainingInfo(int type)
        {
            DevilTrainingInfo info = devilTrainingList[type];
            if (info == null)
            {
                return null;
            }
            return info;
        }

        public List<int> GetReceiveList()
        {
            receivePointRewards = devilTrainingList[2].ReceivePointRewardsList;
            return receivePointRewards;
        }

        public int GetBuffId()
        {
            buffId = devilTrainingList[2].BuffId;
            return buffId;
        }

        public int GetPoint()
        {
            point = devilTrainingList[2].Point;
            return point;
        }

        public void AddPointReward(int rewardId)
        {
            receivePointRewards.Add(rewardId);
            SyncDbUpdatePointRewards();
        }

        public void ChangeBuffId(int buffId)
        {
            this.buffId = buffId;
            devilTrainingList[2].BuffId = buffId;
            SyncDbUpdateChangeBuffId();
        }

        private void SyncDbInsertDevilTraining(int type, int period, Dictionary<int, int> firstDic, Dictionary<int, int> secondDic, Dictionary<int, int> thirdDic, Dictionary<int, int> fourthDic)
        {
            owner.server.GameDBPool.Call(new QueryInsertDevilTrainingInfo(owner.Uid, type, period, firstDic, secondDic, thirdDic, fourthDic));
        }

        private void SyncDbUpdatePointRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateDevilTrainingGetPointRewardInfo(owner.Uid, 2, receivePointRewards));
        }

        private void SyncDbUpdateChangeBuffId()
        {
            owner.server.GameDBPool.Call(new QueryUpdateDevilTrainingChangeBuffInfo(owner.Uid, 2, BuffId));
        }

        private void SyncDbUpdateInitInfo(int type, int period, Dictionary<int, int> firstDic, Dictionary<int, int> secondDic, Dictionary<int, int> thirdDic, Dictionary<int, int> fourthDic)
        {
            owner.server.GameDBPool.Call(new QueryUpdateDevilTrainingInfo(owner.Uid, type, firstDic, secondDic, thirdDic, fourthDic, period));
        }

        public void SyncDbUpdateGetReward(DevilTrainingInfo info)
        {
            owner.server.GameDBPool.Call(new QueryUpdateDevilTrainingGetRewardInfo(owner.Uid, info.Type, firstRewards, secondRewards, thirdRewards, fourthRewards, info.Point, info.Period));
        }

        public MSG_ZMZ_DEVIL_TRAINING_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_DEVIL_TRAINING_INFO msg = new MSG_ZMZ_DEVIL_TRAINING_INFO();
            List<DEVIL_TRAINING_TRANSFORM_INFO> typeList = new List<DEVIL_TRAINING_TRANSFORM_INFO>();

            foreach (DevilTrainingInfo info in devilTrainingList.Values)
            {
                DEVIL_TRAINING_TRANSFORM_INFO typeInfo = new DEVIL_TRAINING_TRANSFORM_INFO();
                typeInfo.Type = info.Type;
                typeInfo.Point = info.Point;
                typeInfo.Period = info.Period;
                typeInfo.BuffId = info.BuffId;
                typeInfo.ReceviedPointRewardList.AddRange(info.ReceivePointRewardsList);

                List<DEVIL_TRAINING_INDEX_INFO> list = new List<DEVIL_TRAINING_INDEX_INFO>();
                list.AddRange(FillZMZDevilTrainingInfo(info.FirstRewardsDic, 1));
                list.AddRange(FillZMZDevilTrainingInfo(info.SecondRewardsDic, 2));
                list.AddRange(FillZMZDevilTrainingInfo(info.ThirdRewardsDic, 3));
                list.AddRange(FillZMZDevilTrainingInfo(info.FourthRewardsDic, 4));
                typeInfo.RewardInfo.AddRange(list);
                typeList.Add(typeInfo);
            }
            msg.Info.AddRange(typeList);
            return msg;
        }

        public void LoadTransformMsg(MSG_ZMZ_DEVIL_TRAINING_INFO msg)
        {
            devilTrainingList.Clear();
            
            foreach (var typeInfo in msg.Info)
            {
                
                if (typeInfo.Type == 1)
                {
                    DevilTrainingInfo info = new DevilTrainingInfo();
                    foreach (var rewardInfo in typeInfo.RewardInfo)
                    {
                        if (rewardInfo.Part == 1)
                        {
                            info.FirstRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 2)
                        {
                            info.SecondRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 3)
                        {
                            info.ThirdRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 4)
                        {
                            info.FourthRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                    }
                    info.Type = typeInfo.Type;
                    info.Period = typeInfo.Period;
                    info.ReceivePointRewardsList.AddRange(typeInfo.ReceviedPointRewardList);
                    info.BuffId = typeInfo.BuffId;
                    info.Point = typeInfo.Point;
                    
                    devilTrainingList.Add(info.Type, info);
                }
                else
                {
                    DevilTrainingInfo info = new DevilTrainingInfo();
                    foreach (var rewardInfo in typeInfo.RewardInfo)
                    {
                        if (rewardInfo.Part == 1)
                        {
                            info.FirstRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 2)
                        {
                            info.SecondRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 3)
                        {
                            info.ThirdRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                        if (rewardInfo.Part == 4)
                        {
                            info.FourthRewardsDic.Add(rewardInfo.Index, rewardInfo.Id);
                        }
                    }
                    info.Type = typeInfo.Type;
                    info.Period = typeInfo.Period;
                    info.ReceivePointRewardsList.AddRange(typeInfo.ReceviedPointRewardList);
                    info.BuffId = typeInfo.BuffId;
                    info.Point = typeInfo.Point;

                    devilTrainingList.Add(info.Type, info);
                }
            }
        }
    }
}

using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        /// <summary>
        /// 阵营星宿等级
        /// </summary>
        public int DragonLevel { get; set; }
        public int TigerLevel { get; set; }
        public int PhoenixLevel { get; set; }
        public int TortoiseLevel { get; set; }

        public void UpdateStarLevel(MSG_GateZ_STAR_LEVELUP msg)
        {
            MSG_ZGC_STAR_LEVELUP response = new MSG_ZGC_STAR_LEVELUP();

            if (!CheckLimitOpen(LimitType.CampStars))
            {
                Log.WarnLine("player {0} update camp star level fail : level {1} is not enough", Uid, Level);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            int titleLevel = CampStarsLibrary.GetCampTitleLevel(HisPrestige);
            response.TitleLevel = titleLevel;

            int starId = msg.StarId;
            CampBlessingModel campBlessingModel = CampStarsLibrary.GetCampBlessModel();
            switch ((CampStarsType)starId)
            {
                case CampStarsType.GreenDragon:
                    CampStarModel oldDragonModel = CampStarsLibrary.GetDragonModel(DragonLevel);
                    if (oldDragonModel == null)
                    {
                        Log.WarnLine("level {0} dragonModel is null", DragonLevel);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    CampStarModel newDragonModel = CampStarsLibrary.GetDragonModel(DragonLevel + 1);
                    if (newDragonModel == null)
                    {
                        Log.WarnLine("level {0} dragonModel is null", DragonLevel + 1);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    if (newDragonModel.TitleLevel > titleLevel)
                    {
                        Log.WarnLine("titleLevel limit {0}, curTitleLevel is {1}", newDragonModel.TitleLevel, titleLevel);
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }
                    CampStarLevelUp(starId, response, oldDragonModel, newDragonModel, campBlessingModel);
                    break;
                case CampStarsType.WhiteTiger:
                    CampStarModel oldTigerModel = CampStarsLibrary.GetTigerModel(TigerLevel);
                    if (oldTigerModel == null)
                    {
                        Log.WarnLine("level {0} tigerModel is null", TigerLevel);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    CampStarModel newTigerModel = CampStarsLibrary.GetTigerModel(TigerLevel + 1);
                    if (newTigerModel == null)
                    {
                        Log.WarnLine("level {0} tigerModel is null", TigerLevel + 1);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    if (newTigerModel.TitleLevel > titleLevel)
                    {
                        Log.WarnLine("titleLevel limit {0}, curTitleLevel is {1}", newTigerModel.TitleLevel, titleLevel);
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }
                    CampStarLevelUp(starId, response, oldTigerModel, newTigerModel, campBlessingModel);
                    break;
                case CampStarsType.RedPhoenix:
                    CampStarModel oldPhoenixModel = CampStarsLibrary.GetPhoenixModel(PhoenixLevel);
                    if (oldPhoenixModel == null)
                    {
                        Log.WarnLine("level {0} phoenixModel is null", PhoenixLevel);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    CampStarModel newPhoenixModel = CampStarsLibrary.GetPhoenixModel(PhoenixLevel + 1);
                    if (newPhoenixModel == null)
                    {
                        Log.WarnLine("level {0} phoenixModel is null", PhoenixLevel + 1);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    if (newPhoenixModel.TitleLevel > titleLevel)
                    {
                        Log.WarnLine("titleLevel limit {0}, curTitleLevel is {1}", newPhoenixModel.TitleLevel, titleLevel);
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }
                    CampStarLevelUp(starId, response, oldPhoenixModel, newPhoenixModel, campBlessingModel);
                    break;
                case CampStarsType.BlackTortoise:
                    CampStarModel oldTortoiseModel = CampStarsLibrary.GetTortoiseModel(TortoiseLevel);
                    if (oldTortoiseModel == null)
                    {
                        Log.WarnLine("level {0} tortoiseModel is null", TortoiseLevel);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    CampStarModel newTortoiseModel = CampStarsLibrary.GetTortoiseModel(TortoiseLevel + 1);
                    if (newTortoiseModel == null)
                    {
                        Log.WarnLine("level {0} tortoiseModel is null", TortoiseLevel + 1);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                    if (newTortoiseModel.TitleLevel > titleLevel)
                    {
                        Log.WarnLine("titleLevel limit {0}, curTitleLevel is {1}", newTortoiseModel.TitleLevel, titleLevel);
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }
                    CampStarLevelUp(starId, response, oldTortoiseModel, newTortoiseModel, campBlessingModel);
                    break;
                default:
                    break;
            }
            Write(response);
        }
        private void CampStarLevelUp(int starId, MSG_ZGC_STAR_LEVELUP response, CampStarModel oldModel, CampStarModel model, CampBlessingModel campBlessingModel)
        {          
            //判断金币是否足够
            int coins = GetCoins(CurrenciesType.gold);
            if (coins < model.Fee)
            {
                Log.WarnLine("coin is not enough, curCoin is {0}", coins);
                response.Result = (int)ErrorCode.GoldLimit;
                //Write(response);
                return;
            }
            //判断材料是否足够
            BaseItem item = BagManager.GetItem(MainType.Material, model.CostItemId);
            if (item == null)
            {
                Log.WarnLine("material {0} not found ", model.CostItemId);
                response.Result = (int)ErrorCode.ItemNotEnough;
                //Write(response);
                return;
            }
            if (item.PileNum < model.CostNum)
            {
                Log.WarnLine("material {0} not found or num {1} is not enough, costNum is {2}", model.CostItemId, item.PileNum, model.CostNum);
                response.Result = (int)ErrorCode.ItemNotEnough;
                //Write(response);
                return;
            }
            //判断是否成功
            int rate = RAND.Range(0, 999);
            int blessing = GetCounterValue(CounterType.CampBlessingCount);
            bool skip = false;
            if (rate >= model.Rate && blessing != campBlessingModel.MaxCount)
            {
                Log.WarnLine("level up fail, generate rate is {0}, rate limit is {1}", rate, model.Rate);
                response.Result = (int)ErrorCode.Fail;
            }
            else
            {
                response.Result = (int)ErrorCode.Success;
            }
            //清空祝福值
            if (blessing == campBlessingModel.MaxCount)
            {
                SetCounter(CounterType.CampBlessingCount, 0, false);
                skip = true;
            }
            //增加祝福值
            if (blessing < campBlessingModel.MaxCount)
            {
                if (response.Result == (int)ErrorCode.Success)
                {
                    int growth = RAND.Range(campBlessingModel.MinGrowth, campBlessingModel.MaxGrowth);
                    if (blessing + growth <= campBlessingModel.MaxCount)
                    {
                        UpdateCounter(CounterType.CampBlessingCount, growth);
                    }
                    else
                    {
                        UpdateCounter(CounterType.CampBlessingCount, campBlessingModel.MaxCount - blessing);
                    }
                }
                //else
                //{
                //    UpdateCounter(CounterType.CampBlessingCount, campBlessingModel.Failure);
                //}
            }
            response.Blessing = GetCounterValue(CounterType.CampBlessingCount);
            int oldLevel = 0;
            int newLevel = 0;
            //升级
            switch ((CampStarsType)starId)
            {
                case CampStarsType.GreenDragon:
                    if (response.Result == (int)ErrorCode.Success && DragonLevel < campBlessingModel.TotalLevel)
                    {
                        oldLevel = DragonLevel;
                        if (skip)
                        {
                            DragonLevel += campBlessingModel.SkipLevel;
                        }
                        else
                        {
                            DragonLevel++;
                        }
                        if (DragonLevel > campBlessingModel.TotalLevel)
                        {
                            DragonLevel = campBlessingModel.TotalLevel;
                        }
                        newLevel = DragonLevel;
                    }
                    break;
                case CampStarsType.WhiteTiger:
                    if (response.Result == (int)ErrorCode.Success && TigerLevel < campBlessingModel.TotalLevel)
                    {
                        oldLevel = TigerLevel;
                        if (skip)
                        {
                            TigerLevel += campBlessingModel.SkipLevel;
                        }
                        else
                        {
                            TigerLevel++;
                        }
                        if (TigerLevel > campBlessingModel.TotalLevel)
                        {
                            TigerLevel = campBlessingModel.TotalLevel;
                        }
                        newLevel = TigerLevel;
                    }
                    break;
                case CampStarsType.RedPhoenix:
                    if (response.Result == (int)ErrorCode.Success && PhoenixLevel < campBlessingModel.TotalLevel)
                    {
                        oldLevel = PhoenixLevel;
                        if (skip)
                        {
                            PhoenixLevel += campBlessingModel.SkipLevel;
                        }
                        else
                        {
                            PhoenixLevel++;
                        }
                        if (PhoenixLevel > campBlessingModel.TotalLevel)
                        {
                            PhoenixLevel = campBlessingModel.TotalLevel;
                        }
                        newLevel = PhoenixLevel;
                    }
                    break;
                case CampStarsType.BlackTortoise:
                    if (response.Result == (int)ErrorCode.Success && TortoiseLevel < campBlessingModel.TotalLevel)
                    {
                        oldLevel = TortoiseLevel;
                        if (skip)
                        {
                            TortoiseLevel += campBlessingModel.SkipLevel;
                        }
                        else
                        {
                            TortoiseLevel++;
                        }
                        if (TortoiseLevel > campBlessingModel.TotalLevel)
                        {
                            TortoiseLevel = campBlessingModel.TotalLevel;
                        }
                        newLevel = TortoiseLevel;
                    }
                    break;
                default:
                    break;
            }       
            response.DragonLevel = DragonLevel;
            response.TigerLevel = TigerLevel;
            response.PhoenixLevel = PhoenixLevel;
            response.TortoiseLevel = TortoiseLevel;
            //更新数据库
            SyncUpdateStarLevel();
            //更新属性加成
            HeroMng.CampStarLevelUp(oldModel.AttrList, model.AttrList);
            //扣货币
            DelCoins(CurrenciesType.gold, model.Fee, ConsumeWay.CampStarLevelUp, starId.ToString());
            //扣材料
            var reItem = DelItem2Bag(item, RewardType.NormalItem, model.CostNum, ConsumeWay.CampStarLevelUp);
            if (reItem != null)
            {
                //更新背包
                SyncClientItemInfo(reItem);
            }
            //阵营培养
            AddTaskNumForType(TaskType.CampStarLevel);
            AddPassCardTaskNum(TaskType.CampStarLevel);

            //养成
            //BIRecordDevelopLog(DevelopType.CampStarLevel, starId, oldLevel, newLevel);

            //komoelog
            Dictionary<CurrenciesType, int> costCoin = new Dictionary<CurrenciesType, int>();
            costCoin.Add(CurrenciesType.gold, model.Fee);
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(costCoin, item.Id, model.CostNum);
            KomoeEventLogCampConstellation(((int)Camp).ToString(), Camp.ToString(), 0, starId, ((CampStarsType)starId).ToString(), oldLevel, newLevel, consume);
        }           
       
        private void SyncUpdateStarLevel()
        {
            server.GameDBPool.Call(new QueryUpdateStarLevel(Uid, DragonLevel, TigerLevel, PhoenixLevel, TortoiseLevel));
        }

        private void AddCampStarsNatures()
        {
            HeroMng.AddCampStarsNatures(Camp);
        }

        public ZMZ_CAMP_STATR GetCampStarTransform()
        {
            ZMZ_CAMP_STATR msg = new ZMZ_CAMP_STATR()
            {
                DragonLevel = DragonLevel,
                PhoenixLevel = PhoenixLevel,
                TigerLevel = TigerLevel,
                TortoiseLevel = TortoiseLevel
            };
            return msg;
        }

        public void LoadCampStarTransform(ZMZ_CAMP_STATR campStarInfo)
        {
            DragonLevel =campStarInfo.DragonLevel;
            PhoenixLevel = campStarInfo.PhoenixLevel;
            TigerLevel = campStarInfo.TigerLevel;
            TortoiseLevel = campStarInfo.TortoiseLevel;
        }
    }
}

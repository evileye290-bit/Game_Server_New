using CommonUtility;
using DBUtility.Sql;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public int ResonanceLevel { get; set; }
        //背包
        private WuhunResonanceManager wuhunResonanceMng = null;

        public WuhunResonanceManager WuhunResonanceMng
        {
            get
            {
                return wuhunResonanceMng;
            }
        }

        public void InitWuhunResonanceManager()
        {
            wuhunResonanceMng = new WuhunResonanceManager(this);
            //wuhunResonanceMng.Init();
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="resonanceInfoDic"></param>
        internal void LoadResonanceInfo(Dictionary<int, ResonanceGridInfo> resonanceInfoDic)
        {
            wuhunResonanceMng.Load(resonanceInfoDic);
        }

        /// <summary>
        /// 开启槽位
        /// </summary>
        public void OpenResonanceGrid()
        {
            MSG_ZGC_OPEN_RESONANCE_GRID response = new MSG_ZGC_OPEN_RESONANCE_GRID();
            //檢查開啓上限
            if (!wuhunResonanceMng.CheckCanOpenGrid())
            {
                Log.Warn($"player {uid} OpenResonanceGrid fail, grid count max limit");
                return;
            }
            //檢查貨幣消耗
            int newGridIndex = wuhunResonanceMng.GetGridCout() + 1;

            int mainCoin = GetCoins(CurrenciesType.resonanceCrystal);
            var costConfig = WuhunResonanceConfig.GetBuyResonanceGridCostConfig(newGridIndex);
            if (costConfig == null)
            {
                Log.Warn($"player {uid} OpenResonanceGrid fail,not find config grid index {newGridIndex}");
                return;
            }

            float mainCoinCost = costConfig.GetCostCount(CurrenciesType.resonanceCrystal);
            if (mainCoin < mainCoinCost)
            {
                float delta = mainCoinCost - mainCoin;
                float prop = costConfig.GetProportion();

                float subCoinCost = delta * prop;
                if (GetCoins(CurrenciesType.diamond) < subCoinCost)
                {
                    Log.Warn($"player {uid} OpenResonanceGrid fail, coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {subCoinCost}");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    response.GridCount = wuhunResonanceMng.GetGridCout();
                    Write(response);
                    return;
                }

                //扣幣
                Dictionary<CurrenciesType, int> costCoins = new Dictionary<CurrenciesType, int>();
                costCoins.Add(CurrenciesType.resonanceCrystal, (int)mainCoin);
                costCoins.Add(CurrenciesType.diamond, (int)subCoinCost);
                DelCoins(costCoins, ConsumeWay.BuyResonanceGrid, newGridIndex.ToString());
            }
            else
            {
                //扣幣
                DelCoins(CurrenciesType.resonanceCrystal, (int)mainCoinCost, ConsumeWay.BuyResonanceGrid, newGridIndex.ToString());
            }

            //開啓
            wuhunResonanceMng.OpenResonanceGrid();

            //komoeLog
            Dictionary<CurrenciesType, int> consumeDic = new Dictionary<CurrenciesType, int>();
            consumeDic.Add(CurrenciesType.resonanceCrystal, (int)mainCoinCost);
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(consumeDic);
            KomoeEventLogHeroResonance("1", "", "", "", newGridIndex.ToString(), consume);

            //反饋客戶端
            response.Result = (int)ErrorCode.Success;
            response.GridCount = wuhunResonanceMng.GetGridCout();
            Write(response);
        }

        //添加共鳴
        public void AddResonance(int heroId)
        {
            wuhunResonanceMng.AddResonanceHero(heroId);
        }

        //去除共鳴
        public void SubResonance(int heroId)
        {
            wuhunResonanceMng.RollbackResonanceHero(heroId, server.Now(), true);

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo != null)
            {
                wuhunResonanceMng.UpdateResonance(heroInfo, true);
            }
        }

        //登录共鸣信息
        public void SendWuhunResonanceGridInfo()
        {
            //wuhunResonanceMng.CheckAndFixBug(HeroMng.GetHeroInfoList());

            MSG_ZGC_RESONANCE_GRID_INFO msg = new MSG_ZGC_RESONANCE_GRID_INFO();
            msg.GridCount = wuhunResonanceMng.GetGridCout();
            msg.ResonanceList.AddRange(wuhunResonanceMng.GetResonanceGridListMsg());
            Write(msg);
        }

        //共鸣集体升级
        public void ResonanceLevelUp()
        {
            MSG_ZGC_RESONANCE_LEVEL response = new MSG_ZGC_RESONANCE_LEVEL();
            if (ResonanceLevel < WuhunResonanceConfig.ResonanceUpLevel)
            {
                Log.Warn("player {0} ResonanceLevel up failed: level {1} error", uid, ResonanceLevel);
                response.Result = (int)ErrorCode.NoHeroLevelInfo;
                Write(response);
                return;
            }

            //材料检查
            HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(ResonanceLevel);
            if (heroLevel == null)
            {
                Log.Warn("player {0} ResonanceLevel up failed: level {1} error", uid, ResonanceLevel);
                response.Result = (int)ErrorCode.NoHeroLevelInfo;
                Write(response);
                return;
            }

            //判断魂力是否足够
            if (GetCoins(CurrenciesType.soulPower) < heroLevel.Exp)
            {
                Log.Warn("player {0} ResonanceLevel up failed: SoulPower {1} error", uid, GetCoins(CurrenciesType.soulPower));
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }
            if (GetCoins(CurrenciesType.soulCrystal) < heroLevel.SoulCrystal)
            {
                Log.Warn("player {0} ResonanceLevel up failed: SoulCrystal {1} error", uid, GetCoins(CurrenciesType.soulCrystal));
                response.Result = (int)ErrorCode.NoCoin;
                Write(response);
                return;
            }

            int oldLevel = ResonanceLevel;

            //扣除
            DelCoins(CurrenciesType.soulPower, heroLevel.Exp, ConsumeWay.HeroLevelUp, oldLevel.ToString());
            DelCoins(CurrenciesType.soulCrystal, heroLevel.SoulCrystal, ConsumeWay.HeroLevelUp, oldLevel.ToString());
            //等级增加
            ResonanceLevel += 1;
            UpdateResonanceLevel2DB();

            //更新所有共鸣伙伴
            Dictionary<int, HeroInfo> updateList = wuhunResonanceMng.UpdateResonance();
            SyncHeroChangeMessage(updateList.Values.ToList());

            int beforePower = HeroMng.CalcBattlePower();

            //通知战力变化
            HeroMng.NotifyClientBattlePower();

            int afterPower = HeroMng.CalcBattlePower();

            response.Result = (int)ErrorCode.Success;
            response.ResonanceLevel = ResonanceLevel;
            Write(response);

            //养成
            BIRecordDevelopLog(DevelopType.ResonanceLevel, 0, oldLevel, ResonanceLevel);           

            //玩家行为记录
            RecordAction(ActionType.Resonance, ResonanceLevel);        

            UpdateFortDefensiveQueue();

            //komoelog
            Dictionary<CurrenciesType, int> costCoins = new Dictionary<CurrenciesType, int>();
            costCoins.Add(CurrenciesType.soulPower, heroLevel.Exp);
            costCoins.Add(CurrenciesType.soulCrystal, heroLevel.SoulCrystal);
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(costCoins);
            KomoeEventLogHeroLevelup(string.Join("_", updateList.Keys), "", "", "", ResonanceLevel, oldLevel, ResonanceLevel, afterPower, beforePower, afterPower, afterPower - beforePower, "共鸣升级", consume);
        }


        internal void UpdateResonanceLevel2DB()
        {
            server.GameDBPool.Call(new QueryUpdateResonanceLevel(Uid, ResonanceLevel));

            HeroMng.UpdateMaxHeroLevel(ResonanceLevel);
        }

        public bool CheckResonanceLevel()
        {
            return ResonanceLevel >= WuhunResonanceConfig.ResonanceUpLevel;
        }

        #region 跨ZONE
        const int RESONANCELISTMAXCOUNT = 200;
        public void SendWuhunResonanceGridListTransform()
        {
            Dictionary<int, ResonanceGridInfo> resonanceGridInfo = wuhunResonanceMng.GetResonanceGridList();
            if (resonanceGridInfo.Count > RESONANCELISTMAXCOUNT)
            {
                int tempNum = 0;
                int totalNum = 0;
                MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST infoMsg = new MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST();
                foreach (var item in resonanceGridInfo)
                {
                    if (tempNum == 0)
                    {
                        infoMsg = new MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST();
                    }
                    infoMsg.List.Add(GetResonanceGridTransformMsg(item.Value));
                    tempNum++;
                    totalNum++;
                    if (totalNum == resonanceGridInfo.Count)
                    {
                        infoMsg.IsEnd = true;
                    }
                    if (tempNum == RESONANCELISTMAXCOUNT)
                    {
                        server.ManagerServer.Write(infoMsg, Uid);
                        tempNum = 0;
                    }
                }
                if (tempNum > 0)
                {
                    server.ManagerServer.Write(infoMsg, Uid);
                }
            }
            else
            {
                MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST heroMsg = new MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST();
                heroMsg.IsEnd = true;
                foreach (var item in resonanceGridInfo)
                {
                    heroMsg.List.Add(GetResonanceGridTransformMsg(item.Value));
                }
                server.ManagerServer.Write(heroMsg, Uid);
            }
        }


        internal RESONANCE_GRIDINFO GetResonanceGridTransformMsg(ResonanceGridInfo gridInfo)
        {
            RESONANCE_GRIDINFO resonanceInfoMsg = new RESONANCE_GRIDINFO()
            {
                Index = gridInfo.Index,
                CDTime = gridInfo.GridCdTime.ToString(),
            };
            if (gridInfo.RollbackInfo != null)
            {
                resonanceInfoMsg.HeroBackUp = GetResonanceHeroTransform(gridInfo.RollbackInfo);
            }

            return resonanceInfoMsg;
        }

        private ZMZ_RESONANCE_HERO_INFO GetResonanceHeroTransform(ResonanceHeroInfo hero)
        {
            ZMZ_RESONANCE_HERO_INFO info = new ZMZ_RESONANCE_HERO_INFO()
            {
                Id = hero.Id,
                Level = hero.Level,
                Exp = hero.Exp,
                AwakenLevel = hero.AwakenLevel,
            };
            return info;
        }

        public void LoadWuhunResonanceTransform(MSG_ZMZ_WUHUN_RESONANCE_INFO_LIST msg)
        {
            Dictionary<int, ResonanceGridInfo> list = new Dictionary<int, ResonanceGridInfo>();

            foreach (var grid in msg.List)
            {
                ResonanceGridInfo info = new ResonanceGridInfo(grid.Index);
                info.GridCdTime = DateTime.Parse(grid.CDTime);

                if (grid.HeroBackUp != null)
                {
                    HeroInfo hero = new HeroInfo();
                    hero.Id = grid.HeroBackUp.Id;
                    hero.Level = grid.HeroBackUp.Level;
                    hero.Exp = grid.HeroBackUp.Exp;
                    hero.AwakenLevel = grid.HeroBackUp.AwakenLevel;
                    info.AddNew(hero);
                }

                list.Add(info.Index, info);
            }
            wuhunResonanceMng.Load(list);
        }
        #endregion
    }
}
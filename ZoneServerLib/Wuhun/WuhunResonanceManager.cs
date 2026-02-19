using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class WuhunResonanceManager
    {
        //private DBManagerPool gameDBPool;
        //private RedisOperatePool redis;
        private PlayerChar owner { get; set; }
        public int GridCount { get { return GridInfoDic.Count; } }

        ///// <summary>
        ///// 共鳴參照
        ///// </summary>
        //public HeroInfo referHero { get; set; }

        public ResonanceHeroInfo ReferHeroInfo = new ResonanceHeroInfo();

        /// <summary>
        ///  等級排名前topHeroCount個英雄（key heroId）
        /// </summary>
        Dictionary<int, HeroInfo> topHeros = new Dictionary<int, HeroInfo>();

        /// <summary>
        /// 共鳴隊列 key從 1開始
        /// </summary>
        Dictionary<int, ResonanceGridInfo> GridInfoDic = new Dictionary<int, ResonanceGridInfo>();

        public WuhunResonanceManager(PlayerChar owner)
        {
            //this.gameDBPool = gameDBPool;
            //this.redis = redis;
            this.owner = owner;
            //topHeros = new Dictionary<int, HeroInfo>();
        }

        //internal void Init()
        //{
        //    GridInfoDic = new Dictionary<int, ResonanceGridInfo>();
        //}

        internal void Load(Dictionary<int, ResonanceGridInfo> resonanceInfoDic)
        {
            int index = 0;
            foreach (var kv in resonanceInfoDic)
            {
                GridInfoDic.Add(kv.Key, kv.Value);

                if (kv.Key > index)
                {
                    index = kv.Key;
                }
            }
            int num = index - GridInfoDic.Count;
            if (num != 0)
            {
                //说明缺少槽位，先查看伙伴身上有没有相应的槽位空缺
                Dictionary<int, HeroInfo> heroDic = owner.HeroMng.GetHeroInfoList();
                foreach (var hero in heroDic)
                {
                    if (hero.Value.ResonanceIndex > 0)
                    {
                        ResonanceGridInfo grid = GetGridHeroInfo(hero.Value.ResonanceIndex);
                        if (grid == null)
                        {
                            Log.Warn($"player {owner.Uid} Load  WuhunResonance not find {hero.Value.Id} resonance: no {hero.Value.ResonanceIndex}!");
                            //说明缺少槽位，添加槽位
                            int newGridIndex = hero.Value.ResonanceIndex;
                            ResonanceGridInfo newGrid = new ResonanceGridInfo(newGridIndex);
                            newGrid.AddNew(hero.Value);
                            AddResonanceGrid(newGrid);

                            //插入
                            owner.server.GameDBPool.Call(new QueryInsertWuhunResonanceInfo(owner.Uid, newGridIndex));
                            //更新
                            UpdateGridInfo2DB(newGrid);

                            num = index - GridInfoDic.Count;
                            if (num == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                if (num != 0)
                {
                    int count = Math.Max(GridInfoDic.Count, index);
                    //说明缺少
                    for (int i = 1; i <= count; i++)
                    {
                        ResonanceGridInfo grid = GetGridHeroInfo(i);
                        if (grid == null)
                        {
                            Log.Warn($"player {owner.Uid} Load  WuhunResonance not find resonance: no {i}!");
                            //说明缺少槽位
                            ResonanceGridInfo newGrid = new ResonanceGridInfo(i);
                            AddResonanceGrid(newGrid);
                            //插入
                            owner.server.GameDBPool.Call(new QueryInsertWuhunResonanceInfo(owner.Uid, i));

                            num = index - GridInfoDic.Count;
                            if (num == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            InitInfoLsit();
        }

        private void InitInfoLsit()
        {
            topHeros.Clear();

            Dictionary<int, HeroInfo> heroDic = owner.HeroMng.GetHeroInfoList();

            List<int> resonanceHeros = new List<int>();
            Dictionary<int, HeroInfo> tempDic = new Dictionary<int, HeroInfo>();
            //开启共鸣
            foreach (var hero in heroDic)
            {
                if (hero.Value.ResonanceIndex > 0)
                {
                    resonanceHeros.Add(hero.Value.Id);
                }
                else
                {
                    //重置初始化
                    hero.Value.ResonanceIndex = 0;
                    tempDic.Add(hero.Key, hero.Value);
                }
            }

            if (heroDic.Count >= WuhunResonanceConfig.ReferHeroCount)
            {
                if (tempDic.Count < WuhunResonanceConfig.ReferHeroCount)
                {
                    //说明人数不足，进行回退
                    foreach (var heroId in resonanceHeros)
                    {
                        //从已经共鸣的角色中下阵一个
                        RollbackResonanceHero(heroId, Timestamp.UnixStartTime, false);

                        HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(heroId);
                        if (heroInfo != null)
                        {
                            tempDic.Add(heroId, heroInfo);
                        }
                        if (tempDic.Count >= WuhunResonanceConfig.ReferHeroCount)
                        {
                            break;
                        }
                    }
                }

                if (tempDic.Count >= WuhunResonanceConfig.ReferHeroCount)
                {
                    //排序
                    //tempDic = tempDic.OrderByDescending(v => v.Value.Level).ThenByDescending(v => v.Value.AwakenLevel).ToDictionary(o => o.Key, p => p.Value);
                    var newDic = from n in tempDic orderby n.Value.Level descending, n.Value.AwakenLevel descending select n;
                    //选出前5
                    foreach (var hero in newDic)
                    {
                        //修改标志位
                        hero.Value.ResonanceIndex = -1;
                        AddTopHero(hero.Value);
                        if (topHeros.Count == WuhunResonanceConfig.ReferHeroCount)
                        {
                            SetNewReferHeroInfo(hero.Value);
                            break;
                        }
                    }
                }
            }
        }

        private void SetNewReferHeroInfo(HeroInfo info)
        {
            ReferHeroInfo = new ResonanceHeroInfo();
            ReferHeroInfo.Id = info.Id;
            ReferHeroInfo.Level = info.Level;
            ReferHeroInfo.AwakenLevel = info.AwakenLevel;


            owner.ResonanceLevel = info.Level;

            owner.RecordAction(ActionType.Resonance, info.Level);
        }

        /// <summary>
        /// 添加到模板队列中
        /// </summary>
        /// <param name="hero"></param>
        private void AddTopHero(HeroInfo hero)
        {
            if (!topHeros.ContainsKey(hero.Id))
            {
                topHeros.Add(hero.Id, hero);
            }
            else
            {
                Log.Warn($"player {owner.Uid} AddTopHero {hero.Id} resonance fail: has add !");
            }
        }

        private void RemoveTopHero(int id)
        {
            topHeros.Remove(id);
        }

        /// <summary>
        /// 下共鸣回滚
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        public bool RollbackResonanceHero(int heroId, DateTime nowTime, bool notifyClient)
        {
            HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {owner.Uid} sub hero {heroId} resonance fail: got is null !");
                return false;
            }
            if (heroInfo.ResonanceIndex <= 0)
            {
                Log.Warn($"player {owner.Uid} sub hero {heroId} resonance fail: hero not in resonance grid {heroInfo.ResonanceIndex}!");
                return false;
            }

            int oldlevel = heroInfo.Level;
            int gridIndex = heroInfo.ResonanceIndex;

            ResonanceGridInfo gridInfo = GetGridHeroInfo(gridIndex);
            if (gridInfo == null)
            {
                Log.Warn($"player {owner.Uid} sub hero {heroId} resonance fail: resonance index {gridIndex} can not find!");
                return false;
            }

            //heroInfo.Nature.Clear();
            //修改伙伴基本信息
            heroInfo.CancelResonance(gridInfo.RollbackInfo);
            //属性战力
            owner.HeroMng.InitHeroNatureInfo(heroInfo);
            UpdateHeroResonance2DB(heroInfo);

            //gridInfo.GridCdTime = nowTime;
            //共鸣位置信息
            gridInfo.Rollback(nowTime); // = null;
            //更新数据库
            UpdateGridInfo2DB(gridInfo);

            //反饋客戶端
            owner.SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
            owner.HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);

            if (notifyClient)
            {
                MSG_ZGC_SUB_RESONANCE response = new MSG_ZGC_SUB_RESONANCE();
                response.Result = (int)ErrorCode.Success;
                response.HeroId = heroId;
                response.ResonanceGridInfo = GetResonanceGridMsg(gridIndex);
                owner.Write(response);
            }

            //养成
            owner.BIRecordDevelopLog(DevelopType.Resonance, heroInfo.Id, oldlevel, heroInfo.Level, heroInfo.Id, heroInfo.Level);

            //komoeLog
            owner.KomoeEventLogHeroResonance("3", heroInfo.Id.ToString(), "", (5 - heroInfo.GetData().GetInt("Quality")).ToString(), heroInfo.ResonanceIndex.ToString(), new List<Dictionary<string, object>>());

            return true;
        }

        /// <summary>
        /// 添加到槽位
        /// </summary>
        /// <param name="hero"></param>
        private void AddResonanceGrid(ResonanceGridInfo info)
        {
            if (!GridInfoDic.ContainsKey(info.Index))
            {
                GridInfoDic.Add(info.Index, info);
            }
            else
            {
                Log.Warn($"player {owner.Uid} AddResonanceGrid {info.Index} resonance fail: has add !");
            }
        }
        /// <summary>
        /// 开启槽位
        /// </summary>
        public void OpenResonanceGrid()
        {
            int newGridIndex = GridInfoDic.Count + 1;
            ResonanceGridInfo newGrid = new ResonanceGridInfo(newGridIndex);
            AddResonanceGrid(newGrid);

            owner.server.GameDBPool.Call(new QueryInsertWuhunResonanceInfo(owner.Uid, newGridIndex));

            //行为埋点
            owner.RecordAction(ActionType.ResonancePosCount, GridInfoDic.Count);
        }

        /// <summary>
        /// 添加共鸣
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public bool AddResonanceHero(int heroId)
        {
            MSG_ZGC_ADD_RESONANCE response = new MSG_ZGC_ADD_RESONANCE();
            response.HeroId = heroId;
            if (ReferHeroInfo.Level <= 0)
            {
                //还未开启
                Log.Warn($"player {owner.Uid} add hero resonance {heroId} fail: referLevel {ReferHeroInfo.Level}!");
                response.Result = (int)ErrorCode.Fail;
                owner.Write(response);
                return false;
            }
            //头5人不能参加共鸣
            if (!CheckCanResonance(heroId))
            {
                //还未开启
                Log.Warn($"player {owner.Uid} add hero resonance {heroId} fail: topHeros count {topHeros.Count}!");
                response.Result = (int)ErrorCode.Fail;
                owner.Write(response);
                return false;
            }

            HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null)
            {
                Log.Warn($"player {owner.Uid} add hero resonance {heroId} fail: mot find hero info !");
                response.Result = (int)ErrorCode.Fail;
                owner.Write(response);
                return false;
            }
            if (heroInfo.ResonanceIndex > 0 || heroInfo.ResonanceIndex == -1)
            {
                Log.Warn($"player {owner.Uid} add hero resonance {heroId} fail: hero already in resonance grid {heroInfo.ResonanceIndex}!");
                response.Result = (int)ErrorCode.Fail;
                owner.Write(response);
                return false;
            }

            ResonanceGridInfo gridInfo = GetOpenResonanceGrid();
            if (gridInfo == null)
            {
                Log.Warn($"player {owner.Uid} add hero resonance {heroId} fail: hero resonance grid  not enough cout:{GetGridCout()}!");
                response.Result = (int)ErrorCode.Fail;
                owner.Write(response);
                return false;
            }
            int oldLevel = heroInfo.Level;

            //修改槽位信息
            gridInfo.AddNew(heroInfo);
            //保存DB
            UpdateGridInfo2DB(gridInfo);

            //修改
            heroInfo.ResonanceIndex = gridInfo.Index;
            ResonanceHeroInfo(heroInfo);
            //數據庫 更新
            UpdateHeroResonance2DB(heroInfo);
            owner.HeroMng.InitHeroNatureInfo(heroInfo);
            owner.HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);

            //反饋客戶端
            List<HeroInfo> updateList = new List<HeroInfo>();
            updateList.Add(heroInfo);
            owner.SyncHeroChangeMessage(updateList);

            response.Result = (int)ErrorCode.Success;
            response.HeroId = heroId;
            owner.Write(response);

            //养成
            owner.BIRecordDevelopLog(DevelopType.Resonance, heroInfo.Id, oldLevel, heroInfo.Level, heroInfo.Id, heroInfo.Level);

            //komoeLog
            owner.KomoeEventLogHeroResonance("2", heroInfo.Id.ToString(), "", (5-heroInfo.GetData().GetInt("Quality")).ToString(), heroInfo.ResonanceIndex.ToString(), new List<Dictionary<string, object>>());

            RemoveTopHero(heroInfo.Id);

            return true;
        }

        public void ResonanceHeroInfo(HeroInfo heroInfo, bool notifyClient = true)
        {
            //共鸣等级
            heroInfo.Level = GetHeroMaxLevel(heroInfo.Id);
            heroInfo.AwakenLevel = ReferHeroInfo.AwakenLevel;

            ////heroInfo.StepsLevel = referHero.StepsLevel;
            //int maxTitleLevel = owner.GetHeroMaxTitle(heroInfo);
            //if (referHero.TitleLevel > maxTitleLevel)
            //{
            //    heroInfo.TitleLevel = maxTitleLevel;
            //}
            //else
            //{
            //    heroInfo.TitleLevel = referHero.TitleLevel;
            //}
            //int awakenLevel = 0;
            //HeroAwakenModel awaken = HeroLibrary.GetHeroAwakenModel(heroInfo.Id);
            //if (heroInfo.Level >= awaken.InitLevel)
            //{
            //    awakenLevel++;
            //}
            //foreach (var item in awaken.AwakenLevelList)
            //{
            //    if (heroInfo.Level >= item)
            //    {
            //        awakenLevel++;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //HeroTitleModel title = HeroLibrary.GetHeroTitle(heroInfo.TitleLevel);
            //if (title != null)
            //{
            //    heroInfo.TalentMng.SetTalentNum(title.TotalTalent);
            //}
            //owner.HeroMng.InitHeroNatureInfo(heroInfo);
            //if (notifyClient)
            //{
            //    owner.HeroMng.NotifyClientBattlePowerFrom(heroInfo.Id);
            //}
            //if (heroInfo.DefensiveQueueNum > 0)
            //{
            //    owner.UpdateFortDefensiveQueue();
            //}
        }

        private int GetHeroMaxLevel(int heroId)
        {
            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model != null)
            {
                HeroQualityModel quality = HeroLibrary.GetHeroQuality(model.Quality);
                if (quality != null)
                {
                    if (owner.ResonanceLevel >= WuhunResonanceConfig.ResonanceUpLevel)
                    {
                        if (model.Quality > 2)
                        {
                            return quality.MaxLevel;
                        }
                        else
                        {
                            return owner.ResonanceLevel;
                        }
                    }
                    else
                    {
                        if (ReferHeroInfo.Level > quality.MaxLevel)
                        {
                            return quality.MaxLevel;
                        }
                        else
                        {
                            return ReferHeroInfo.Level;
                        }
                    }
                }
            }
            return 0;
        }

        private ResonanceGridInfo GetOpenResonanceGrid()
        {
            foreach (var item in GridInfoDic)
            {
                if (item.Value.RollbackInfo != null && item.Value.RollbackInfo.Id > 0)
                {
                    //位置有人
                    continue;
                }
                TimeSpan ts = ZoneServerApi.now - item.Value.GridCdTime;
                if (ts.TotalSeconds < WuhunResonanceConfig.GridCdTime)
                {
                    //已经CD中
                    continue;
                }
                return item.Value;
            }

            return null;
        }

        internal bool CheckCanResonance(int heroId)
        {
            //if (topHeros.ContainsKey(heroId))
            //{
            //    int i = 0;
            //    foreach (var item in topHeros)
            //    {
            //        if (item.Key == heroId)
            //        {
            //            Log.Warn($"player {owner.Uid} hero {heroId} can not resonance,because in level top {WuhunResonanceConfig.ReferHeroCount}");
            //            return false;
            //        }
            //        else
            //        {
            //            i++;
            //            if (i >= WuhunResonanceConfig.ReferHeroCount)
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}
            if (topHeros.ContainsKey(heroId))
            {
                Log.Warn($"player {owner.Uid} hero {heroId} can not resonance,because in level top {WuhunResonanceConfig.ReferHeroCount}");
                return false;
            }
            return true;
        }


        internal List<RESONANCE_GRID_CDINFO> GetResonanceGridListMsg()
        {
            List<RESONANCE_GRID_CDINFO> list = new List<RESONANCE_GRID_CDINFO>();
            foreach (var item in GridInfoDic)
            {
                ResonanceGridInfo gridInfo = item.Value;
                list.Add(ResonanceInfoMsgFormat(gridInfo));
            }
            return list;
        }

        internal RESONANCE_GRID_CDINFO GetResonanceGridMsg(int resonanceIndex)
        {
            ResonanceGridInfo gridInfo = GetGridHeroInfo(resonanceIndex);
            if (gridInfo != null)
            {
                return ResonanceInfoMsgFormat(gridInfo);
            }
            return null;
        }

        public RESONANCE_GRID_CDINFO ResonanceInfoMsgFormat(ResonanceGridInfo info)
        {
            RESONANCE_GRID_CDINFO resonanceInfoMsg = new RESONANCE_GRID_CDINFO()
            {
                GridIndex = info.Index,
                CdTime = Timestamp.GetUnixTimeStampSeconds(info.GridCdTime)
            };
            return resonanceInfoMsg;
        }

        public void UpdateResonance(HeroInfo hero, bool checkTop)
        {
            if (topHeros.ContainsKey(hero.Id))
            {
                //说明在头5
                if (hero.Id != ReferHeroInfo.Id)
                {
                    //说明不是第5个人,是前4个人，对共鸣不会产生影响
                    if (checkTop)
                    {
                        ResetTopHerosUpdateResonance();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //说明是第5人
                    if (hero.Level == ReferHeroInfo.Level && hero.AwakenLevel == ReferHeroInfo.AwakenLevel)
                    {
                        //没变化
                        return;
                    }
                    else
                    {
                        //有变化，重新调整
                        ResetTopHerosUpdateResonance();
                    }
                }
            }
            else
            {
                //判断是否在共鸣队列中
                ResonanceGridInfo gridInfo = GetGridHeroInfoById(hero.Id);
                if (gridInfo != null)
                {
                    //共鸣队里中伙伴对共鸣没有影响
                    return;
                }
                else
                {
                    //不在,而且不是共鸣人物，跟当前的对比一下
                    if (hero.Level < ReferHeroInfo.Level)
                    {
                        //小于参照等级，对共鸣没有影响
                        return;
                    }
                    else if (hero.Level == ReferHeroInfo.Level)
                    {
                        //两个等级相等，查看觉醒等级
                        if (hero.AwakenLevel <= ReferHeroInfo.AwakenLevel)
                        {
                            //对共鸣没有影响
                            return;
                        }
                        else
                        {
                            //有变化，重新调整
                            ResetTopHerosUpdateResonance();
                        }
                    }
                    else
                    {
                        //有变化，重新调整
                        ResetTopHerosUpdateResonance();
                    }
                }
            }
            //if (ReferHeroInfo == null || ReferHeroInfo.Level <= 0)
            //{
            //    //没有共鸣
            //    return;
            //}
            //if (hero != null && hero.Id != ReferHeroInfo.Id)
            //{
            //    return;
            //}
            //UpdateResonance();
        }

        private void ResetTopHerosUpdateResonance()
        {
            List<int> oldTopDic = topHeros.Keys.ToList();

            InitInfoLsit();

            Dictionary<int, HeroInfo> updateList = UpdateResonance();

            foreach (var heroId in oldTopDic)
            {
                if (!topHeros.ContainsKey(heroId))
                {
                    if (!updateList.ContainsKey(heroId))
                    {
                        HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(heroId);
                        if (heroInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }
                    }
                }
            }
            foreach (var hero in topHeros)
            {
                if (!oldTopDic.Contains(hero.Key))
                {
                    if (!updateList.ContainsKey(hero.Key))
                    {
                        HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(hero.Key);
                        if (heroInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }
                    }
                }
            }

            owner.SyncHeroChangeMessage(updateList.Values.ToList());

            owner.HeroMng.NotifyClientBattlePower();

            if (ReferHeroInfo.Level == WuhunResonanceConfig.ResonanceUpLevel)
            {
                owner.ResonanceLevel = WuhunResonanceConfig.ResonanceUpLevel;
                owner.UpdateResonanceLevel2DB();

                MSG_ZGC_RESONANCE_LEVEL response = new MSG_ZGC_RESONANCE_LEVEL();
                response.ResonanceLevel = owner.ResonanceLevel;
                response.Result = (int)ErrorCode.Success;
                owner.Write(response);
            }
        }

        public Dictionary<int, HeroInfo> UpdateResonance()
        {
            //反饋客戶端
            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
            foreach (var item in GridInfoDic)
            {
                ResonanceGridInfo gridInfo = item.Value;
                if (gridInfo.RollbackInfo.Id > 0)
                {
                    HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(gridInfo.RollbackInfo.Id);
                    if (heroInfo != null && heroInfo.ResonanceIndex > 0)
                    {
                        ResonanceHeroInfo(heroInfo);
                        UpdateHeroResonance2DB(heroInfo);
                        owner.HeroMng.InitHeroNatureInfo(heroInfo);
                        updateList[heroInfo.Id] = heroInfo;
                    }
                }
            }

            if (owner.CheckResonanceLevel())
            {
                foreach (var item in topHeros)
                {
                    HeroInfo heroInfo = owner.HeroMng.GetHeroInfo(item.Value.Id);
                    if (heroInfo != null)
                    {
                        ResonanceHeroInfo(heroInfo);
                        UpdateHeroResonance2DB(heroInfo);
                        owner.HeroMng.InitHeroNatureInfo(heroInfo);
                        updateList[heroInfo.Id] = heroInfo;
                    }
                }
            }
            return updateList;
        }

        internal Dictionary<int, ResonanceGridInfo> GetResonanceGridList()
        {
            return GridInfoDic;
        }

        internal bool CheckCanOpenGrid()
        {
            if (GridInfoDic.Count < WuhunResonanceConfig.ResonanceGridMaxCount)
            {
                return true;
            }
            return false;
        }

        internal int GetHeroRealLevel(HeroInfo info)
        {
            ResonanceGridInfo gridInfo = GetGridHeroInfo(info.ResonanceIndex);
            if (gridInfo != null)
            {
                if (gridInfo.RollbackInfo != null && gridInfo.RollbackInfo.Id == info.Id)
                {
                    return gridInfo.RollbackInfo.Level;
                }
            }
            else
            {
                Log.Error("player {0} GetHeroRealLevel fail! please check: hero {1} resonance_index {2} ", owner.Uid, info.Id, info.ResonanceIndex);
            }
            return info.Level;
        }

        ///// <summary>
        ///// 重新计算的代码。用于出错时候纠错。bug改正后这个函数是不需要的
        ///// </summary>
        ///// <param name="heroDic"></param>
        //public void CheckAndFixBug(Dictionary<int, HeroInfo> heroDic)
        //{
        //    bool needFixBug = false;

        //    if (topHeros.Count != WuhunResonanceConfig.ReferHeroCount || referHero == null)
        //    {
        //        needFixBug = true;
        //    }
        //    else
        //    {
        //        foreach (var item in topHeros)
        //        {
        //            if (item.Value.ResonanceIndex >= 0)
        //            {
        //                needFixBug = true;
        //            }
        //        }
        //    }

        //    if (needFixBug)
        //    {
        //        Log.Warn($"player {owner.Uid} load wuhun resonance info refer count wrong! {topHeros.Count}");
        //        ReCalcResonance(heroDic);
        //    }
        //}


        //private void ReCalcResonance(Dictionary<int, HeroInfo> heroDic)
        //{
        //    if (heroDic.Count < WuhunResonanceConfig.ReferHeroCount)
        //    {
        //        return;
        //    }
        //    //
        //    List<int> tops = new List<int>();

        //    //重排序
        //    Dictionary<int, HeroInfo> heroOrderDic = new Dictionary<int, HeroInfo>();
        //    foreach (var item in heroDic)
        //    {
        //        HeroInfo orderInfo = item.Value;
        //        if (item.Value.ResonanceIndex > 0)
        //        {
        //            //在共鳴
        //            orderInfo = GridInfoDic[item.Value.ResonanceIndex].HeroInfoBak;
        //        }
        //        if (item.Value.ResonanceIndex <0)
        //        {
        //            tops.Add(item.Key);
        //        }
        //        heroOrderDic[item.Value.Id] = orderInfo;
        //    }
        //    heroOrderDic = heroOrderDic.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);
        //    topHeros.Clear();

        //    List<HeroInfo> updateList = new List<HeroInfo>();

        //    int count = 0;
        //    foreach (var item in heroOrderDic)
        //    {
        //        HeroInfo info = item.Value;
        //        topHeros.Add(info.Id, info);
        //        count++;
        //        if (count == WuhunResonanceConfig.ReferHeroCount)
        //        {
        //            break;
        //        }
        //    }

        //    foreach (var item in topHeros)
        //    {
        //        HeroInfo info = item.Value;
        //        if (info.ResonanceIndex !=-1)
        //        {
        //            info.ResonanceIndex = -1;
        //            UpdateHeroResonance2DB(info);
        //            updateList.Add(info);
        //        }
        //    }

        //    foreach (var item in tops)
        //    {
        //        if (!topHeros.ContainsKey(item))
        //        {
        //            HeroInfo info = owner.HeroMng.GetHeroInfo(item);
        //            info.ResonanceIndex = 0;
        //            UpdateHeroResonance2DB(info);
        //            updateList.Add(info);
        //        }
        //    }

        //    owner.SyncHeroChangeMessage(updateList);
        //}

        internal int GetGridCout()
        {
            return GridInfoDic.Count;
        }

        //internal void RevertCheckResonance(HeroInfo info)
        //{
        //    if (info.ResonanceIndex >= 0)
        //    {
        //        return;
        //    }

        //    Dictionary<int, HeroInfo> heroDic = owner.HeroMng.GetHeroInfoList();

        //    if (heroDic.Count < WuhunResonanceConfig.ReferHeroCount)
        //    {
        //        return;
        //    }

        //    if (!topHeros.ContainsKey(info.Id))
        //    {
        //        Log.Warn($"player {owner.Uid} RevertCheckResonance fail: hero {info.Id} ");
        //        return;
        //    }

        //    topHeros.Remove(info.Id);
        //    topHeros.Add(info.Id, info);
        //    topHeros = topHeros.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);

        //    //重排序
        //    Dictionary<int, HeroInfo> heroOrderDic = new Dictionary<int, HeroInfo>();
        //    foreach (var item in heroDic)
        //    {
        //        HeroInfo orderInfo = item.Value;
        //        if (item.Value.ResonanceIndex > 0)
        //        {
        //            //在共鳴
        //            orderInfo = GridInfoDic[item.Value.ResonanceIndex].HeroInfoBak;
        //        }
        //        heroOrderDic[item.Value.Id] = orderInfo;
        //    }
        //    heroOrderDic = heroOrderDic.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);

        //    HeroInfo heroInfo1 = topHeros.ElementAtOrDefault(WuhunResonanceConfig.ReferHeroCount - 1).Value;
        //    HeroInfo heroInfo2 = heroOrderDic.ElementAtOrDefault(WuhunResonanceConfig.ReferHeroCount - 1).Value;

        //    //反饋客戶端
        //    List<HeroInfo> updateList = new List<HeroInfo>();
        //    if (heroInfo1.Level > heroInfo2.Level)
        //    {
        //        return;
        //    }
        //    if (heroInfo1.Level == heroInfo2.Level)
        //    {
        //        if (referHero.Id != info.Id && referHero.Id == heroInfo1.Id)
        //        {
        //            return;
        //        }

        //        if (heroInfo1.Level != referHero.Level)
        //        {
        //            referHero = heroInfo1;
        //            UpdateResonance();
        //        }
        //    }
        //    else if (heroInfo1.Level < heroInfo2.Level)
        //    {
        //        heroInfo1.ResonanceIndex = 0;
        //        UpdateHeroResonance2DB(heroInfo1);
        //        updateList.Add(heroInfo1);
        //        topHeros.Remove(heroInfo1.Id);

        //        HeroInfo referHeroInfo = null;

        //        foreach (var item in heroOrderDic)
        //        {
        //            int heroId = item.Value.Id;
        //            int level = item.Value.Level;

        //            if (level == heroInfo2.Level)
        //            {
        //                if (!topHeros.ContainsKey(heroId))
        //                {
        //                    var hero = owner.HeroMng.GetHeroInfo(heroId);
        //                    if (hero.ResonanceIndex == 0)
        //                    {
        //                        referHeroInfo = hero;
        //                        break;
        //                    }
        //                }
        //            }

        //            if (level < heroInfo2.Level)
        //            {
        //                break;
        //            }
        //        }

        //        if (referHeroInfo == null)
        //        {
        //            foreach (var item in heroOrderDic)
        //            {
        //                int heroId = item.Value.Id;
        //                int level = item.Value.Level;

        //                if (level == heroInfo2.Level)
        //                {
        //                    if (!topHeros.ContainsKey(heroId))
        //                    {
        //                        var hero = owner.HeroMng.GetHeroInfo(heroId);
        //                        if (hero.ResonanceIndex > 0)
        //                        {
        //                            referHeroInfo = hero;
        //                            break;
        //                        }
        //                    }
        //                }

        //                if (level < heroInfo2.Level)
        //                {
        //                    break;
        //                }
        //            }
        //        }

        //        if (referHeroInfo == null )
        //        {
        //            referHeroInfo = heroInfo2;
        //        }

        //        if (referHeroInfo.ResonanceIndex > 0)
        //        {
        //            //去除共鳴
        //            ResonanceGridInfo gridInfo;
        //            if (GridInfoDic.TryGetValue(referHeroInfo.ResonanceIndex, out gridInfo))
        //            {
        //                referHeroInfo.Nature.Clear();

        //                referHeroInfo.CancelResonance(gridInfo.HeroInfoBak);
        //                referHeroInfo.ResonanceIndex = 0;

        //                gridInfo.GridCdTime = owner.server.Now();
        //                gridInfo.HeroInfoBak = null;

        //                UpdateGridInfo2DB(gridInfo);

        //                MSG_ZGC_SUB_RESONANCE response = new MSG_ZGC_SUB_RESONANCE();
        //                response.Result = (int)ErrorCode.Success;
        //                response.HeroId = referHeroInfo.Id;
        //                response.ResonanceGridInfo = GetResonanceGridMsg(gridInfo.Index);
        //                owner.Write(response);
        //            }
        //        }

        //        referHeroInfo.ResonanceIndex = -1;
        //        UpdateHeroResonance2DB(referHeroInfo);
        //        if (!topHeros.ContainsKey(referHeroInfo.Id))
        //        {
        //            topHeros.Add(referHeroInfo.Id, referHeroInfo);
        //        }
        //        updateList.Add(referHeroInfo);

        //        referHero = referHeroInfo;

        //        UpdateResonance();
        //    }
        //    owner.SyncHeroChangeMessage(updateList);
        //}

        //internal void LevelUpCheckResonance(HeroInfo info)
        //{
        //    Dictionary<int, HeroInfo> heroDic = owner.HeroMng.GetHeroInfoList();
        //    if (heroDic.Count >= WuhunResonanceConfig.ReferHeroCount)
        //    {
        //        List<HeroInfo> updateList = new List<HeroInfo>();
        //        if (topHeros.Count < WuhunResonanceConfig.ReferHeroCount || referHero == null || referHero.ResonanceIndex >= 0)
        //        {
        //            return;
        //        }

        //        if (info.Id == referHero.Id)
        //        {
        //            topHeros.Remove(referHero.Id);
        //            topHeros.Add(info.Id, info);
        //            topHeros = topHeros.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);
        //        }
        //        else
        //        {
        //            if (referHero.Level >= info.Level)
        //            {
        //                return;
        //            }

        //            if (topHeros.ContainsKey(info.Id))
        //            {
        //                return;
        //            }

        //            referHero.ResonanceIndex = 0;
        //            updateList.Add(referHero);
        //            UpdateHeroResonance2DB(referHero);

        //            topHeros.Remove(referHero.Id);

        //            info.ResonanceIndex = -1;
        //            topHeros.Add(info.Id, info);

        //            topHeros = topHeros.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);
        //        }

        //        referHero = topHeros.ElementAtOrDefault(WuhunResonanceConfig.ReferHeroCount - 1).Value;
        //        owner.SyncHeroChangeMessage(updateList);

        //        UpdateResonance();

        //        if (referHero.Level == WuhunResonanceConfig.ResonanceUpLevel)
        //        {
        //            owner.ResonanceLevel = WuhunResonanceConfig.ResonanceUpLevel;
        //            owner.UpdateResonanceLevel2DB();
        //            MSG_ZGC_RESONANCE_LEVEL response = new MSG_ZGC_RESONANCE_LEVEL();
        //            response.ResonanceLevel = owner.ResonanceLevel;
        //            response.Result = (int)ErrorCode.Success;
        //            owner.Write(response);
        //        }

        //        owner.RecordAction(ActionType.Resonance, referHero.Level);
        //    }
        //}

        //internal void AddHeroCheckResonance(HeroInfo info)
        //{
        //    Dictionary<int, HeroInfo> heroDic = owner.HeroMng.GetHeroInfoList();
        //    if (heroDic.Count >= WuhunResonanceConfig.ReferHeroCount)
        //    {
        //        List<HeroInfo> updateList = new List<HeroInfo>();
        //        if (topHeros.Count == 0)
        //        {
        //            heroDic = heroDic.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);

        //            int count = 0;
        //            foreach (var item in heroDic)
        //            {
        //                item.Value.ResonanceIndex = -1;
        //                topHeros.Add(item.Value.Id, item.Value);
        //                updateList.Add(item.Value);
        //                referHero = item.Value;

        //                UpdateHeroResonance2DB(item.Value);
        //                count++;
        //                if (count == WuhunResonanceConfig.ReferHeroCount)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (referHero != null && info.Level > referHero.Level)
        //            {
        //                HeroInfo heroInfo1 = referHero;
        //                heroInfo1.ResonanceIndex = 0;
        //                topHeros.Remove(referHero.Id);

        //                UpdateHeroResonance2DB(heroInfo1);
        //                updateList.Add(heroInfo1);

        //                info.ResonanceIndex = -1;
        //                topHeros.Add(info.Id, info);

        //                topHeros = topHeros.OrderByDescending(v => v.Value.Level).ToDictionary(o => o.Key, p => p.Value);
        //                referHero = topHeros.ElementAtOrDefault(WuhunResonanceConfig.ReferHeroCount - 1).Value;
        //            }
        //        }

        //        owner.SyncHeroChangeMessage(updateList);
        //    }

        //    UpdateResonance();
        //}

        /// <summary>
        /// 共鳴數據
        /// </summary>
        /// <param name="heroId"></param>
        private void UpdateHeroResonance2DB(HeroInfo heroInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdateHeroInfo(owner.Uid, heroInfo));
        }

        /// <summary>
        /// 共鳴格子數據
        /// </summary>
        /// <param name="heroInfoBak"></param>
        private void UpdateGridInfo2DB(ResonanceGridInfo gridInfo)
        {
            owner.server.GameDBPool.Call(new QueryUpdateResonanceListInfo(owner.Uid, gridInfo.Index, gridInfo.GridCdTime, gridInfo.RollbackInfo));
        }

        internal ResonanceGridInfo GetGridHeroInfo(int index)
        {
            ResonanceGridInfo gridInfo;
            GridInfoDic.TryGetValue(index, out gridInfo);
            return gridInfo;
        }

        internal ResonanceGridInfo GetGridHeroInfoById(int heroId)
        {
            foreach (var item in GridInfoDic)
            {
                if (heroId == item.Value.RollbackInfo.Id)
                {
                    return item.Value;
                }
            }
            return null;
        }

        internal void UpdateGridHeroInfo(HeroInfo info)
        {
            ResonanceGridInfo gridInfo = GetGridHeroInfo(info.ResonanceIndex);
            if (gridInfo != null)
            {
                gridInfo.AddNew(info);
                UpdateGridInfo2DB(gridInfo);
            }
            else
            {
                gridInfo = new ResonanceGridInfo(info.ResonanceIndex);
                gridInfo.AddNew(info);
                GridInfoDic[info.ResonanceIndex] = gridInfo;
            }
        }
    }
}

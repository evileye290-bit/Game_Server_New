using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //伙伴

        public int HeroId { get; set; }
        public int GodType { get; set; }
        public int FollowerId { get; set; }

        public HeroManager HeroMng;
        public void InitHeroManager()
        {
            HeroMng = new HeroManager(this);
        }

        public void InitHero(List<HeroInfo> heroList)
        {
            foreach (var hero in heroList)
            {
                HeroMng.BindHeroInfo(hero);
            }

            //初始化人物属性
            InitNature();
        }

        public void BindHerosNature()
        {
            HeroMng.BindHerosNature();
            HeroMng.CheckAndFixCrossPos();
            HeroMng.CheckAndFixCrossChallengePos();

            //初始化人物属性
            InitNature();

            //初始化时空伙伴属性
            SpaceTimeTowerMng.BindHerosNature();
        }


        public int GetHeroPos(int heroId)
        {
            MapType mapType = currentMap.GetMapType();
            switch (mapType)
            {
                case MapType.Tower:
                    return TowerManager.GetHeroPos(heroId);
                case MapType.ThemeBoss:
                    foreach (var kv in HeroMng.ThemeBossQueue)
                    {
                        foreach (var hero in kv.Value)
                        {
                            if (hero.Value.Id == heroId)
                            {
                                return hero.Value.ThemeBossPositionNum;
                            }
                        }
                    }
                    return HeroMng?.GetHeroPosInfo(heroId)?.Item2 ?? -1;
                case MapType.CrossBoss:
                case MapType.CrossBossSite:
                    foreach (var kv in HeroMng.CrossBossQueue)
                    {
                        foreach (var hero in kv.Value)
                        {
                            if (hero.Value.Id == heroId)
                            {
                                return hero.Value.CrossBossPositionNum;
                            }
                        }
                    }
                    return HeroMng?.GetHeroPosInfo(heroId)?.Item2 ?? -1;
                case MapType.CarnivalBoss:
                    foreach (var kv in HeroMng.CarnivalBossQueue)
                    {
                        foreach (var hero in kv.Value)
                        {
                            if (hero.Value.Id == heroId)
                            {
                                return hero.Value.CarnivalBossPositionNum;
                            }
                        }
                    }
                    return HeroMng?.GetHeroPosInfo(heroId)?.Item2 ?? -1;
                case MapType.HuntingIntrude:
                    return HuntingManager.HuntingIntrudeHeroPos.ContainsKey(heroId) ? HuntingManager.HuntingIntrudeHeroPos[heroId] : -1;
                default:
                    return HeroMng?.GetHeroPosInfo(heroId)?.Item2 ?? -1;
            }
        }

        public void AddHeros(RewardManager rewards, RewardResult resulet, ObtainWay way, string extraParam = "")
        {
            Dictionary<int, int> heros = rewards.GetRewardList(RewardType.Hero);
            if (heros == null)
            {
                return;
            }
            List<ItemBasicInfo> removeHeros = new List<ItemBasicInfo>();
            List<ItemBasicInfo> moreHeros = new List<ItemBasicInfo>();

            foreach (var hero in rewards.AllRewards)
            {
                if (hero.RewardType != (int)RewardType.Hero)
                {
                    continue;
                }
                HeroInfo info = HeroMng.GetHeroInfo(hero.Id);
                if (info == null)
                {
                    HeroModel model = HeroLibrary.GetHeroModel(hero.Id);
                    if (model == null)
                    {
                        Log.Warn("player {0} add hero {1} failed: not find model id {2} error", uid, hero.Id, hero.Id);
                        removeHeros.Add(hero);
                        continue;
                    }
                    //增加新的
                    info = HeroMng.InitNewHero(model);

                    EquipmentManager.InitSlot(hero.Id);

                    //假如是第一次需要添加默认魂环 
                    AddDefaultSoulRing(way, info); //穿到身上 会同时计算当前英雄的战力和增加总战力

                    HeroMng.BindHeroInfo(info);
                    HeroMng.BindHeroNature(info);

                    wuhunResonanceMng.UpdateResonance(info, true);

                    //同步数据
                    SyncDbInsertHeroItem(info);


                    resulet.Heros.Add(info.Id, info);

                    if (hero.Num > 1)
                    {
                        hero.Num = 1;
                        ItemBasicInfo newItem = new ItemBasicInfo(hero.RewardType, hero.Id, hero.Num - 1);
                        //转换成材料
                        moreHeros.Add(newItem);
                    }

                    //伙伴获取埋点
                    BIRecordObtainItem(RewardType.Hero, way, hero.Id, 1, 1);

                    //RecordObtainLog(way, RewardType.Hero, hero.Id, 0, 1);

                    //BI 新增物品
                    KomoeEventLogItemFlow("add", "", hero.Id, "Hero", 1, 0, 1, (int)way, 0, 0, 0, 0);
                    //玩家行为
                    RecordAction(ActionType.GainIdLimitHero, hero.Id);
                    RecordAction(ActionType.GainQualityHeroNum, hero.Id);

                    if (model.Quality == HeroLibrary.SSRHeroQuality)
                    {
                        //获得SSR达到指定数量发称号卡
                        TitleMng.UpdateTitleConditionCount(TitleObtainCondition.SSRHeroCount);
                    }

                    //某周期内获得指定角色解锁七日奖励
                    AddHeroDaysRewardsInfo(hero.Id);

                    //komoelog
                    KomoeEventLogHeroResource(info.Id.ToString(), "", model.Quality.ToString(), info.Level, model.Job.ToString(), way.ToString(), extraParam);

                }
                else
                {
                    //转换成材料
                    moreHeros.Add(hero);
                    removeHeros.Add(hero);
                }
            }



            if (moreHeros.Count > 0)
            {
                //转换成魂晶
                foreach (var hero in moreHeros)
                {
                    //判断是否已经满级
                    HeroModel model = HeroLibrary.GetHeroModel(hero.Id);

                    if(model ==null)
                    {
                        Log.Warn("player {0} add hero {1} to Fragment  failed: not find model id {2} error", uid, hero.Id, hero.Id);
                        //移除原奖励
                        removeHeros.Add(hero);
                        continue;
                    }

                    if (model.HeroFragment > 0 && model.ResolveNum > 0)
                    {
                        int addNum = model.ResolveNum * hero.Num;
                        rewards.AddBreakupReward((int)RewardType.HeroFragment, model.HeroFragment, addNum);
                    }
                    else
                    {
                        Log.Warn("player {0} add hero {1} to Fragment failed: not find HeroFragment id {2} error", uid, hero.Id, model.HeroFragment);
                    }

                    //掉落神阶碎片
                    if (NeedDropHeroFragment(hero.Id))
                    {
                        if (model.HeroGodFragment > 0 && HeroLibrary.HeroGodFragmentNum > 0)
                        {
                            int addNum = HeroLibrary.HeroGodFragmentNum * hero.Num;
                            rewards.AddBreakupReward((int)RewardType.HeroFragment, model.HeroGodFragment, addNum);
                        }
                        else
                        {
                            Log.Warn("player {0} add hero {1} to Fragment failed: not find HeroGodFragment id {2} error", uid, hero.Id, model.HeroGodFragment);
                        }
                    }
                }
            }

            if (removeHeros.Count > 0)
            {
                foreach (var reward in removeHeros)
                {
                    rewards.RemoveReward(reward);
                }
            }

            if (resulet.Heros.Count > 0)
            {
                MSG_ZGC_HERO_CHANGE msg = new MSG_ZGC_HERO_CHANGE();
                foreach (var hero in resulet.Heros)
                {
                    msg.AddList.Add(GetHeroMessage(hero.Value));
                }
                Write(msg);
            }
        }

        //当金色6星并且高阶神位解锁了之后才会掉落神位碎片
        //当高阶神位培养满了不在掉落神位碎片
        private bool NeedDropHeroFragment(int heroId)
        {
            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo == null) return false;

            if (heroInfo.StepsLevel < HeroLibrary.HeroStepMax || heroInfo.StepsLevel >= HeroLibrary.HeroGodStepMax)
            {
                return false;
            }

            HeroGodInfo heroGodInfo = HeroGodManager.GetHeroGodInfo(heroInfo.Id);

            return heroGodInfo?.GodType.Count >= 2;
        }

        /// <summary>
        /// 武魂升级
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroLevelUp(int heroId)
        {
            //if (CheckResonanceLevel())
            //{
            //    return;
            //}

            MSG_ZGC_HERO_LEVEL_UP response = new MSG_ZGC_HERO_LEVEL_UP();
            response.HeroId = heroId;

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} hero {1} level up failed: no such hero", uid, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            //判断是否可升级
            ErrorCode errorCode = CheckHeroLevelUp(hero);
            if (errorCode != ErrorCode.Success)
            {
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            //判断魂力是否足够
            HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(hero.Level);
            if (heroLevel == null)
            {
                Log.Warn("player {0} hero {1} level up failed: level {2} error", uid, heroId, hero.Level);
                response.Result = (int)ErrorCode.NoHeroLevelInfo;
                Write(response);
                return;
            }
            int costSoulPower = 0;
            if (hero.Exp < heroLevel.Exp)
            {
                costSoulPower = HeroLibrary.ExpToSoulPower * (heroLevel.Exp - hero.Exp);

                if (GetCoins(CurrenciesType.soulPower) < costSoulPower)
                {
                    Log.Warn("player {0} hero {1} level up failed: SoulPower {2} error", uid, heroId, GetCoins(CurrenciesType.soulPower));
                    response.Result = (int)ErrorCode.NoCoin;
                    Write(response);
                    return;
                }
                //扣除魂力
                DelCoins(CurrenciesType.soulPower, costSoulPower, ConsumeWay.HeroLevelUp, heroId.ToString());
                //升级后清除经验
                hero.Exp = 0;
            }
            else
            {
                //当前经验已经可以升级， 直接升级，不消耗，扣除经验
                hero.Exp -= heroLevel.Exp;
            }
            int oldLevel = hero.Level;
            //升级
            HeroMng.LevelUp(hero);
            //升级属性加成
            EquipmentManager.SyncItems2Client(hero.Id);

            //before
            int beforePower = hero.GetBattlePower();

            HeroMng.HeroLevelUpUpdateNature(hero);

            //after
            int afterPower = hero.GetBattlePower();

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //N个伙伴达到某个魂师等级
            AddTaskNumForType(TaskType.HeroLevelCount, 1, true, hero.Level);
            //伙伴升级
            AddTaskNumForType(TaskType.HeroUpLevel);
            //养成
            //BIRecordDevelopLog(DevelopType.HeroLevel, hero.Id, oldLevel, hero.Level, hero.Id, oldLevel);

            if (hero.DefensiveQueueNum > 0)
            {
                UpdateFortDefensiveQueue();
            }

            //komoelog
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)CurrenciesType.soulPower, costSoulPower);
            KomoeEventLogHeroLevelup(hero.Id.ToString(), "", (5 - hero.GetData().GetInt("Quality")).ToString(), hero.GodType.ToString(), hero.Level, oldLevel, hero.Level, afterPower, beforePower, afterPower, afterPower - beforePower, "单角色魂力升级", consume);
        }

        public void AddHeroExp(int exp)
        {
            if (exp > 0)
            {
                List<HeroInfo> updateList = new List<HeroInfo>();
                List<int> equipedHero = HeroMng.GetAllHeroPosHeroId();
                foreach (var heroId in equipedHero)
                {
                    //int heroId = kv.Value;
                    HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                    if (hero == null)
                    {
                        Log.Warn("player {0} hero {1} add hero exp failed: no such hero", uid, heroId);
                        continue;
                    }

                    //判断是否可升级
                    ErrorCode errorCode = CheckHeroLevelUp(hero);
                    if (errorCode != ErrorCode.Success)
                    {
                        continue;
                    }

                    //增加经验
                    hero.Exp += exp;

                    //检查伙伴升级
                    HeroAddExpCheckLevelUp(hero);

                    //同步
                    updateList.Add(hero);
                    SyncDbUpdateHeroItem(hero);

                    //N个伙伴达到某个魂师等级
                    AddTaskNumForType(TaskType.HeroLevelCount, 1, true, hero.Level);
                }
                if (updateList.Count > 0)
                {
                    SyncHeroChangeMessage(updateList);
                }
            }
        }

        private void HeroAddExpCheckLevelUp(HeroInfo hero)
        {
            //判断魂力是否足够
            HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(hero.Level);
            if (heroLevel == null)
            {
                Log.Warn("player {0} hero {1} level up failed: level {2} error", uid, hero.Id, hero.Level);
                return;
            }

            if (hero.Exp >= heroLevel.Exp)
            {
                //升级
                hero.Exp -= heroLevel.Exp;
                //升级
                HeroMng.LevelUp(hero);
                //升级属性加成
                HeroMng.HeroLevelUpUpdateNature(hero);
                //伙伴升级
                AddTaskNumForType(TaskType.HeroUpLevel);
                if (hero.Exp > 0)
                {
                    //判断是否可升级
                    ErrorCode errorCode = CheckHeroLevelUp(hero);
                    if (errorCode != ErrorCode.Success)
                    {
                        //不可升级，经验清0
                        hero.Exp = 0;
                    }
                    else
                    {
                        //继续升级
                        HeroAddExpCheckLevelUp(hero);
                    }
                }
            }
        }

        private ErrorCode CheckHeroLevelUp(HeroInfo hero)
        {
            int heroId = hero.Id;

            if (CheckResonanceLevel())
            {
                Log.Warn("player {0} hero {1} check hero level up failed: ResonanceLevel is {2}", uid, heroId, ResonanceLevel);
                return ErrorCode.HeroInResonanceList;
            }

            if (hero.ResonanceIndex > 0)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: resonanceIndex is {2}", uid, heroId, hero.ResonanceIndex);
                return ErrorCode.HeroInResonanceList;
            }

            if (hero.State != WuhunState.Normal)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: state {2} error", uid, heroId, hero.State);
                return ErrorCode.HeroStateError;
            }
            //判断是否已经满级
            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model == null)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: not find model id {2} error", uid, heroId, heroId);
                return ErrorCode.NoHeroInfo;
            }
            HeroQualityModel quality = HeroLibrary.GetHeroQuality(model.Quality);
            if (quality == null)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: not find quality id {2} error", uid, heroId, model.Quality);
                return ErrorCode.NoHeroInfo;
            }
            if (quality.MaxLevel <= hero.Level)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: level {2} is max", uid, heroId, hero.Level);
                return ErrorCode.MaxLevel;
            }
            return ErrorCode.Success;
        }

        /// <summary>
        /// 武魂觉醒
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroAwaken(int heroId)
        {
            if (CheckResonanceLevel())
            {
                return;
            }

            MSG_ZGC_HERO_AWAKEN response = new MSG_ZGC_HERO_AWAKEN();
            response.HeroId = heroId;

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} hero {1} awaken failed: no such hero", uid, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            if (hero.ResonanceIndex > 0)
            {
                Log.Warn("player {0} hero {1} check hero level up failed: resonanceIndex is {2}", uid, heroId, hero.ResonanceIndex);
                response.Result = (int)ErrorCode.NoHeroInfo;
                //Write(response);
                return;
            }

            //判断是否等待觉醒
            if (hero.State != WuhunState.WaitAwaken)
            {
                Log.Warn("player {0} hero {1} awaken failed: state {2} error", uid, heroId, hero.State);
                response.Result = (int)ErrorCode.HeroStateError;
                Write(response);
                return;
            }
            int costSoulCrystal = 0;
            //觉醒等级为-1 时，是初始觉醒， 不消耗魂晶
            if (hero.AwakenLevel >= 0)
            {
                //判断魂晶是否足够
                HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(hero.Level);
                if (heroLevel == null)
                {
                    Log.Warn("player {0} hero {1} awaken failed: level {2} error", uid, heroId, hero.Level);
                    response.Result = (int)ErrorCode.NoHeroLevelInfo;
                    Write(response);
                    return;
                }
                costSoulCrystal = heroLevel.SoulCrystal;
                if (costSoulCrystal > 0)
                {
                    if (GetCoins(CurrenciesType.soulCrystal) < costSoulCrystal)
                    {
                        Log.Warn("player {0} hero {1} awaken failed: SoulCrystal {2} error", uid, heroId, GetCoins(CurrenciesType.soulCrystal));
                        response.Result = (int)ErrorCode.NoCoin;
                        Write(response);
                        return;
                    }

                    //扣除魂力
                    DelCoins(CurrenciesType.soulCrystal, costSoulCrystal, ConsumeWay.HeroAwaken, heroId.ToString());
                }
            }
            else
            {
                //初始觉醒需要称号认证
                int maxTitle = GetHeroMaxTitle(hero);
                if (hero.TitleLevel >= maxTitle)
                {
                    Log.Warn("player {0} hero {1} awaken title up failed: title level {2} error {3}", uid, heroId, hero.TitleLevel, maxTitle);
                    response.Result = (int)ErrorCode.HeroStateError;
                    Write(response);
                    return;
                }
                //认证
                HeroMng.TitleUp(hero, maxTitle);
            }
            int oldLevel = hero.AwakenLevel;
            //觉醒
            HeroMng.Awaken(hero);

            wuhunResonanceMng.UpdateResonance(hero, false);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            response.Result = (int)ErrorCode.Success;
            Write(response);

            AddTaskNumForType(TaskType.HeroAwaken);

            //养成
            BIRecordDevelopLog(DevelopType.HeroAwakeLevel, hero.Id, oldLevel, hero.AwakenLevel, hero.Id, hero.Level);
        }

        /// <summary>
        /// 判断称号等级
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        public int GetHeroMaxTitle(HeroInfo hero)
        {
            int maxTitle = 0;
            int tempValue = hero.Level / 10;

            //if (hero.Level % 10 == 0)
            {
                int soulRingCount = 0;
                //判断是否已经吸收魂环
                Dictionary<int, SoulRingItem> soulRingList = SoulRingManager.GetAllEquipedSoulRings(hero.Id);
                if (soulRingList != null)
                {
                    foreach (var item in soulRingList)
                    {
                        if (item.Value.AbsorbState == (int)SoulRingAbsorbState.Deafult)
                        {
                            soulRingCount++;
                        }
                    }
                }

                if (soulRingCount >= tempValue)
                {
                    //吸收魂环
                    maxTitle = tempValue + 1;
                }
                else
                {
                    //没有吸收魂环
                    maxTitle = tempValue;
                }
            }
            //else
            //{
            //    maxTitle = tempValue + 1;
            //}
            //超过100级依然是封号斗罗
            maxTitle = Math.Min(10, maxTitle);

            return maxTitle;
        }

        /// <summary>
        /// 称号认证
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroTitleUp(int heroId)
        {
            MSG_ZGC_HERO_TITLE_UP response = new MSG_ZGC_HERO_TITLE_UP();
            response.HeroId = heroId;

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} hero {1} title up failed: no such hero", uid, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            //判断称号等级
            int maxTitle = GetHeroMaxTitle(hero);
            if (hero.TitleLevel >= maxTitle)
            {
                Log.Warn("player {0} hero {1} title up failed: title level {2} error {3}", uid, heroId, hero.TitleLevel, maxTitle);
                //response.Result = (int)ErrorCode.HeroStateError;
                //Write(response);
                return;
            }
            int oldLevel = hero.TitleLevel;
            //认证
            HeroMng.TitleUp(hero, maxTitle);
            //升级属性加成
            HeroMng.HeroTitleUpUpdateNature(hero, oldLevel);

            //广播达到封号斗罗
            BroadCastRaiseMaxTitleLevel(hero);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            response.Result = (int)ErrorCode.Success;
            Write(response);

            // 认证一次称号
            AddTaskNumForType(TaskType.HeroTitleLevelUpNum);
            //N个伙伴达到某个魂师称号
            AddTaskNumForType(TaskType.HeroTitleLevelCount, 1, true, hero.TitleLevel);

            ////养成
            //BIRecordDevelopLog(DevelopType.HeroTitleLevel, hero.Id, oldLevel, hero.TitleLevel, 0, 0, CurrenciesType.soulCrystal.ToString(), costSoulCrystal, hero.Id, hero.Level);
        }

        /// <summary>
        /// 点天赋点
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="strength"></param>
        /// <param name="physical"></param>
        /// <param name="agility"></param>
        /// <param name="outburst"></param>
        public void HeroClickTalent(int heroId, int strength, int physical, int agility, int outburst)
        {
            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} hero {1} click talent failed: no such hero", uid, heroId);
                return;
            }
            if (strength < 0 || physical < 0 || agility < 0 || outburst < 0)
            {
                Log.Warn("player {0} hero {1} click talent failed: error num {2} {3} {4} {5}", uid, heroId, strength, physical, agility, outburst);
                return;
            }
            if (strength > HeroLibrary.TalentMaxNum || physical > HeroLibrary.TalentMaxNum || agility > HeroLibrary.TalentMaxNum || outburst > HeroLibrary.TalentMaxNum)
            {
                Log.Warn("player {0} hero {1} click talent failed: error num {2} {3} {4} {5}", uid, heroId, strength, physical, agility, outburst);
                return;
            }
            //判断是否可以点天赋
            if (!hero.TalentMng.CheckCanClick())
            {
                Log.Warn("player {0} hero {1} click talent  failed: free num is 0", uid, heroId, hero.State);
                return;
            }
            Dictionary<NatureType, int> oldTalents = GetResetHeroTalents(hero);
            //点天赋
            int oldTalent = hero.TalentMng.FreeNum;
            hero.TalentMng.Click(strength, physical, agility, outburst);
            int newTalent = hero.TalentMng.FreeNum - oldTalent;
            Dictionary<NatureType, int> newTalents = GetResetHeroTalents(hero);
            //komoelog
            int beforePower = hero.GetBattlePower();

            //天赋属性增加
            HeroMng.HeroClickTalent(hero, oldTalents, newTalents);

            int afterPower = hero.GetBattlePower();

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            //天赋加点
            AddTaskNumForType(TaskType.HeroTalent);

            //养成
            BIRecordDevelopLog(DevelopType.HeroClickTalent, hero.Id, hero.TalentMng.TotalNum, newTalent, hero.Id, hero.Level);

            //komoelog
            List<Dictionary<string, object>> natureList = Parse4NatureList(strength, physical, agility, outburst);
            KomoeEventLogModifytpFlow(hero.Id.ToString(), "", hero.GetData().GetString("Job"), 2, natureList, beforePower, afterPower, afterPower - beforePower);
        }

        /// <summary>
        /// 重置天赋点
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroResetTalent(int heroId)
        {
            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                Log.Warn("player {0} hero {1} reset talent failed: no such hero", uid, heroId);
                return;
            }

            //判断是否有物品
            ErrorCode Result = UseItem(HeroLibrary.ResetTalentItem, 1);
            if (Result != ErrorCode.Success)
            {
                if (GetCoins(CurrenciesType.diamond) < HeroLibrary.ResetTalentDiamond)
                {
                    Log.Warn("player {0} hero {1} reset talent  failed: diamond is {2}", uid, heroId, GetCoins(CurrenciesType.diamond));
                    return;
                }
                else
                {
                    DelCoins(CurrenciesType.diamond, HeroLibrary.ResetTalentDiamond, ConsumeWay.HeroResetTalent, heroId.ToString());
                }
            }
            Dictionary<NatureType, int> oldTalents = GetResetHeroTalents(hero);
            //点天赋
            hero.TalentMng.Reset();

            //komoelog
            int beforePower = hero.GetBattlePower();

            //重置天赋属性
            HeroMng.HeroClickTalent(hero, oldTalents, null);
            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            int afterPower = hero.GetBattlePower();
            List<Dictionary<string, object>> natureList = Parse4NatureList(0, 0, 0, 0);
            KomoeEventLogModifytpFlow(hero.Id.ToString(), "", hero.GetData().GetString("Job"), 1, natureList, beforePower, afterPower, afterPower - beforePower);
        }

        /// <summary>
        /// 进阶
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroStepsUp(int heroId)
        {
            MSG_ZGC_HERO_STEPS_UP response = new MSG_ZGC_HERO_STEPS_UP();
            response.HeroId = heroId;

            //获取信息
            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model == null)
            {
                Log.Warn("player {0} hero {1} steps up failed: not find model id {2} error", uid, heroId, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                //合成激活伙伴
                HeroFragmentToHero(model);
            }
            else
            {
                //伙伴进阶
                HeroStepsUp(hero, model);
            }
        }

        /// <summary>
        /// 羁绊
        /// </summary>
        /// <param name="heroId"></param>
        public void ActivateHeroCombo(int comboId)
        {
            MSG_ZGC_ACTIVATE_HERO_COMBO response = new MSG_ZGC_ACTIVATE_HERO_COMBO();
            response.ComboId = comboId;
            response.ComboList.Add(comboId);
            HeroComboModel model = DrawLibrary.GetHeroComboModel(comboId);
            if (model == null)
            {
                Log.Warn("player {0} ActivateHeroCombo error: not find combo {1}", Uid, comboId);
                response.Result = (int)ErrorCode.NoHeroCombo;
                Write(response);
                return;
            }

            if (!HeroMng.CheckCanCombo(model))
            {
                Log.Warn("player {0} ActivateHeroCombo error: has activate combo {1}", Uid, comboId);
                response.Result = (int)ErrorCode.SameHeroCombo;
                Write(response);
                return;
            }

            foreach (var heroId in model.Member)
            {
                HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                if (hero == null)
                {
                    Log.Warn("player {0} ActivateHeroCombo error: no such hero {1}", uid, heroId);
                    response.Result = (int)ErrorCode.NoHeroInfo;
                    Write(response);
                    return;
                }

                if (hero.StepsLevel < model.Steps)
                {
                    Log.Warn("player {0} ActivateHeroCombo error: hero {1} steps is {2}", uid, heroId, hero.StepsLevel);
                    response.Result = (int)ErrorCode.NoHeroInfo;
                    Write(response);
                    return;
                }
            }
            Log.Debug("player {0} ActivateHeroCombo error: old power is {1}", uid, HeroMng.CalcBattlePower());
            //激活
            HeroMng.AddCombo(comboId);

            ////所有伙伴增加属性
            //HeroMng.AddComboNature(model.NatureRatio);
            int oldPower = HeroMng.CalcBattlePower();
            foreach (var hero in HeroMng.GetHeroInfoList())
            {
                HeroMng.InitHeroNatureInfo(hero.Value);
            }
            HeroMng.NotifyClientBattlePower();

            ////战力到指定值发称号卡
            //List<int> paramList = new List<int>() { HeroMng.CalcBattlePower() };
            //TitleMng.UpdateTitleConditionCount(TitleObtainCondition.BattlePower, 1, paramList);

            Log.Debug("player {0} ActivateHeroCombo error: new power is {1}", uid, HeroMng.CalcBattlePower());
            SyncHeroChangeMessage(HeroMng.GetHeroInfoList().Values.ToList());

            //同步
            SyncDbUpdateHeroCombo();

            foreach (var id in HeroMng.GetHeroPos())
            {
                HeroInfo info = HeroMng.GetHeroInfo(id.Key);
                response.ComboPower += HeroMng.GetComboPower(info.Nature);
            }
            response.Result = (int)ErrorCode.Success;
            response.ComboList.AddRange(HeroMng.HeroComboList);
            Write(response);

            //养成
            BIRecordDevelopLog(DevelopType.HeroCombo, comboId, 0, 1);

            KomoeEventLogTieFlowstring(comboId, model.Group.ToString(), 2, oldPower, HeroMng.CalcBattlePower());
            //解锁指定数量羁绊发称号卡
            //TitleMng.UpdateTitleConditionCount(TitleObtainCondition.HeroComboCount);
        }

        /// <summary>
        /// 一键羁绊
        /// </summary>
        /// <param name="comboId"></param>
        public void OnekeyActivateHeroCombo()
        {
            MSG_ZGC_ACTIVATE_HERO_COMBO response = new MSG_ZGC_ACTIVATE_HERO_COMBO();

            Dictionary<int, HeroComboModel> dic = new Dictionary<int, HeroComboModel>();
            foreach (var groupList in DrawLibrary.ComboGroupList)
            {
                bool isEnd = false;
                foreach (var comboId in groupList.Value)
                {
                    HeroComboModel model = DrawLibrary.GetHeroComboModel(comboId);
                    if (model == null)
                    {
                        continue;
                    }
                    if (!HeroMng.CheckCanCombo(model))
                    {
                        continue;
                    }

                    foreach (var heroId in model.Member)
                    {
                        HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                        if (hero == null)
                        {
                            isEnd = true;
                            break;
                        }
                        if (hero.StepsLevel < model.Steps)
                        {
                            isEnd = true;
                            break;
                        }
                    }
                    if (isEnd)
                    {
                        break;
                    }
                    dic[comboId] = model;
                }
            }

            if (dic.Count > 0)
            {
                foreach (var kv in dic)
                {
                    //激活
                    HeroMng.AddCombo(kv.Key);
                    //所有伙伴增加属性
                    HeroMng.AddComboNature(kv.Value.NatureRatio);
                    //养成
                    BIRecordDevelopLog(DevelopType.HeroCombo, kv.Key, 0, 1);

                    response.ActivateList.Add(kv.Key);
                }

                foreach (var hero in HeroMng.GetHeroInfoList())
                {
                    HeroMng.InitHeroNatureInfo(hero.Value);
                }

                ////战力到指定值发称号卡
                //List<int> paramList = new List<int>() { HeroMng.CalcBattlePower() };
                //TitleMng.UpdateTitleConditionCount(TitleObtainCondition.BattlePower, 1, paramList);

                HeroMng.NotifyClientBattlePower();

                SyncHeroChangeMessage(HeroMng.GetHeroInfoList().Values.ToList());

                //同步
                SyncDbUpdateHeroCombo();

                foreach (var id in HeroMng.GetHeroPos())
                {
                    HeroInfo info = HeroMng.GetHeroInfo(id.Key);
                    response.ComboPower += HeroMng.GetComboPower(info.Nature);
                }

                response.Result = (int)ErrorCode.Success;
                response.ComboList.AddRange(HeroMng.HeroComboList);
                //解锁指定数量羁绊发称号卡
                //TitleMng.UpdateTitleConditionCount(TitleObtainCondition.HeroComboCount, dic.Count);
            }
            else
            {
                Log.Warn($"player {Uid} oneKey activate hero combo failed: hero combo count error");
                response.Result = (int)ErrorCode.NoHeroCombo;
            }
            Write(response);
        }

        /// <summary>
        /// 一键进阶
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="model"></param>
        public void OnekeyHeroStepsUp(RepeatedField<int> heroIds)
        {
            MSG_ZGC_ONEKEY_HERO_STEPS_UP response = new MSG_ZGC_ONEKEY_HERO_STEPS_UP();

            List<HeroInfo> upHeros = new List<HeroInfo>();
            foreach (var heroId in heroIds)
            {
                //获取信息
                HeroModel model = HeroLibrary.GetHeroModel(heroId);
                if (model == null)
                {
                    Log.Warn($"player {uid} OnekeyHeroStepsUp hero {heroId} failed: not find model");
                    continue;
                }

                HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                if (hero == null)
                {
                    Log.Warn($"player {uid} OnekeyHeroStepsUp hero {heroId} failed: not find hero info");
                    continue;
                }
                //检查碎片个数
                int itemId = model.HeroFragment;
                BaseItem item = BagManager.GetItem(MainType.HeroFragment, itemId);
                if (item == null)
                {
                    //没有碎片
                    Log.Warn($"player {uid} OnekeyHeroStepsUp hero {heroId} failed: not no item {itemId}");
                    continue;
                }

                int stepsLevel = hero.StepsLevel;
                if (model.UpSteps.Count < stepsLevel)
                {
                    Log.Warn($"player {uid} OnekeyHeroStepsUp hero {heroId} failed: can up {model.UpSteps.Count}  current is {stepsLevel}");
                    continue;
                }

                //进阶需要碎片数
                int upStepsNum = 0;
                int addLevel = 0;
                for (int i = 1; i < model.UpSteps.Count - stepsLevel; i++)
                {
                    //进阶需要碎片数
                    int num = model.UpSteps[stepsLevel + i];
                    //碎片个数
                    if (item.PileNum >= upStepsNum + num)
                    {
                        addLevel = i;
                        upStepsNum += num;
                    }
                    else
                    {
                        break;
                    }
                }
                if (addLevel == 0 || upStepsNum == 0)
                {
                    Log.Warn($"player {uid} OnekeyHeroStepsUp hero {heroId} failed: can up {addLevel}  num is {upStepsNum}");
                    continue;
                }

                BaseItem it = DelItem2Bag(item, RewardType.HeroFragment, upStepsNum, ConsumeWay.HeroUpSteps);
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
                int oldLevel = hero.StepsLevel;
                //升级
                HeroMng.StepsLevelUp(hero, addLevel);

                //同步
                SyncDbUpdateHeroItem(hero);
                upHeros.Add(hero);
                response.HeroIds.Add(hero.Id);
                //养成
                BIRecordDevelopLog(DevelopType.HeroStepLevel, hero.Id, oldLevel, hero.StepsLevel, hero.Id, hero.Level);

                //每6阶触发一次 oldLevel/6 已经触发的次数
                int needActionCount = (addLevel + oldLevel % 6) / 6;
                for (int i = 0; i < Math.Max(1, needActionCount); i++)
                {
                    //玩家行为记录
                    RecordAction(ActionType.PerHeroAdvance, heroId);
                    RecordAction(ActionType.OneQualityHeroAdvanceAction, heroId);
                }
            }
            if (upHeros.Count > 0)
            {
                SyncHeroChangeMessage(upHeros);
                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                Log.Warn($"player {uid} OnekeyHeroStepsUp failed: upHeros count is zero");
                response.Result = (int)ErrorCode.NoHeroInfo;
            }
            Write(response);
        }

        /// <summary>
        /// 进阶
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="model"></param>
        private void HeroStepsUp(HeroInfo hero, HeroModel model)
        {
            int heroId = model.Id;
            int stepsLevel = hero.StepsLevel;

            MSG_ZGC_HERO_STEPS_UP response = new MSG_ZGC_HERO_STEPS_UP();
            response.HeroId = heroId;

            if (model.UpSteps.Count < stepsLevel)
            {
                Log.Warn("player {0} hero {1} steps up failed: not find UpSteps count {2} and steps is {3} error", uid, heroId, model.UpSteps.Count, stepsLevel);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            if (stepsLevel >= HeroLibrary.HeroStepMax)
            {
                Log.Warn($"player {uid} hero {heroId} steps up failed: step up max {stepsLevel}  error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //进阶需要碎片数
            int upStepsNum = model.UpSteps[stepsLevel + 1];
            //检查碎片个数
            int itemId = model.HeroFragment;
            BaseItem item = BagManager.GetItem(MainType.HeroFragment, itemId);
            if (item != null)
            {
                //碎片个数
                if (item.PileNum < upStepsNum)
                {
                    //没有碎片
                    Log.Warn("player {0} steps up failed: no item {1} num {2}", uid, itemId, item.PileNum);
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Write(response);
                    return;
                }
                else
                {

                    BaseItem it = DelItem2Bag(item, RewardType.HeroFragment, upStepsNum, ConsumeWay.HeroUpSteps);
                    if (it != null)
                    {
                        SyncClientItemInfo(it);
                    }
                    int oldLevel = hero.StepsLevel;
                    //升级
                    HeroMng.StepsLevelUp(hero, 1);

                    //同步
                    SyncHeroChangeMessage(new List<HeroInfo>() { hero });
                    SyncDbUpdateHeroItem(hero);

                    response.StepsLevel = hero.StepsLevel;
                    response.Result = (int)ErrorCode.Success;
                    Write(response);

                    //养成
                    BIRecordDevelopLog(DevelopType.HeroStepLevel, hero.Id, oldLevel, hero.StepsLevel, hero.Id, hero.Level);

                    //玩家行为记录
                    RecordAction(ActionType.PerHeroAdvance, heroId);
                    RecordAction(ActionType.OneQualityHeroAdvanceAction, heroId);

                    //komoelog
                    List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, item.Id, upStepsNum);
                    KomoeEventLogHeroStarup(hero.Id.ToString(), "", (5 - model.Quality).ToString(), oldLevel, hero.StepsLevel, 0, consume);

                    return;
                }
            }
            else
            {
                //没有碎片
                Log.Warn("player {0} steps up failed: no item {1} ", uid, itemId);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
            }
        }

        public void HeroGodStepsUp(int heroId)
        {
            MSG_ZGC_HERO_GOD_STEPS_UP response = new MSG_ZGC_HERO_GOD_STEPS_UP();
            response.HeroId = heroId;

            //获取信息
            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model == null)
            {
                Log.Warn("player {0} hero {1} steps up god failed: not find model id {2} error", uid, heroId, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
            if (hero == null)
            {
                response.Result = (int) ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            HeroGodStepUpModel stepUpModel = GodHeroLibrary.GetHeroGodStepUpModel(heroId);
            if (stepUpModel == null)
            {
                Log.Warn($"player {uid} hero {heroId} steps up god failed: not find step up model id {heroId} error");
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            if (hero.StepsLevel < stepUpModel.StepsLimit)
            {
                Log.Warn($"player {uid} hero {heroId} steps up god failed: not find step up model id {heroId} error");
                response.Result = (int)ErrorCode.HeroGodStepUpStepLimit;
                Write(response);
                return;
            }

            HeroGodInfo godInfo = HeroGodManager.GetHeroGodInfo(heroId);
            if (godInfo == null || !godInfo.GodType.Contains(stepUpModel.GodLimit))
            {
                Log.Warn($"player {uid} hero {heroId} steps up god failed: not find step up model id {heroId} error");
                response.Result = (int)ErrorCode.HeroGodStepUpGodTypeLimit;
                Write(response);
                return;
            }

            if (hero.StepsLevel >= HeroLibrary.HeroGodStepMax)
            {
                Log.Warn($"player {uid} hero {heroId} steps up god failed: step up max {hero.StepsLevel}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int stepsLevel = hero.StepsLevel;
            int aimStepLevel = stepsLevel + 1;
            int costShipNum = 0, costItemNum = 0;
            if(!stepUpModel.ShipCost.TryGetValue(aimStepLevel, out costShipNum) ||
               !stepUpModel.ItemCost.TryGetValue(aimStepLevel, out costItemNum))
            {
                Log.Warn($"player {uid} hero {heroId} steps up god failed: can not find cost ship or item data {aimStepLevel}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>();

            //检查碎片个数
            BaseItem item = BagManager.GetItem(MainType.HeroFragment, stepUpModel.ShipId);
            if (item == null || item.PileNum < costShipNum)
            {
                //没有碎片
                Log.Warn("player {0} steps up god failed: no item {1} num {2}", uid, stepUpModel.ShipId, item?.PileNum);
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }
            costItems.Add(item, costShipNum);

            item = BagManager.GetItem(MainType.Consumable, HeroLibrary.HeroGodStepUpCostItemId);
            if (costItemNum > 0)
            {
                if (item == null || item.PileNum < costItemNum)
                {
                    //没有碎片
                    Log.Warn("player {0} steps up god failed: no item {1} num {2}", uid, HeroLibrary.HeroGodStepUpCostItemId, item?.PileNum);
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Write(response);
                    return;
                }
                costItems.Add(item, costItemNum);
            }

            List<BaseItem> it = DelItem2Bag(costItems, RewardType.NormalItem, ConsumeWay.HeroUpSteps);
            if (it != null)
            {
                SyncClientItemsInfo(it);
            }

            int oldLevel = hero.StepsLevel;
            //升级
            HeroMng.StepsLevelUp(hero, 1);

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { hero });
            SyncDbUpdateHeroItem(hero);

            response.StepsLevel = hero.StepsLevel;
            response.Result = (int)ErrorCode.Success;
            Write(response);

            //养成
            BIRecordDevelopLog(DevelopType.HeroStepLevel, hero.Id, oldLevel, hero.StepsLevel, hero.Id, hero.Level);

            //玩家行为记录
            RecordAction(ActionType.PerHeroAdvance, heroId);
            RecordAction(ActionType.OneQualityHeroAdvanceAction, heroId);

            //BI 神阶提升
            BIRecordGodStepUpLog(hero.Id, hero.StepsLevel);
        }

        private void HeroFragmentToHero(HeroModel model)
        {
            int heroId = model.Id;

            MSG_ZGC_HERO_STEPS_UP response = new MSG_ZGC_HERO_STEPS_UP();
            response.HeroId = heroId;

            if (model.UpSteps.Count == 0)
            {
                Log.Warn("player {0} hero {1} steps up failed: not find UpSteps id {2} error", uid, heroId, heroId);
                response.Result = (int)ErrorCode.NoHeroInfo;
                Write(response);
                return;
            }

            //激活伙伴是第一个
            int upStepsNum = model.UpSteps[0];
            //检查碎片个数
            int itemId = model.HeroFragment;
            BaseItem item = BagManager.GetItem(MainType.HeroFragment, itemId);
            if (item != null)
            {
                //碎片个数
                if (item.PileNum < upStepsNum)
                {
                    //没有兑换券
                    Log.Warn("player {0} steps up failed: no item {1} num {2}", uid, itemId, item.PileNum);
                    response.Result = (int)ErrorCode.NotFoundItem;
                    Write(response);
                    return;
                }
                else
                {

                    BaseItem it = DelItem2Bag(item, RewardType.HeroFragment, upStepsNum, ConsumeWay.HeroUpSteps);
                    if (it != null)
                    {
                        SyncClientItemInfo(it);
                    }

                    RewardManager rewards = new RewardManager();
                    rewards.AddBreakupReward((int)RewardType.Hero, heroId, 1);
                    AddRewards(rewards, ObtainWay.DrawHeroCard);

                    response.Result = (int)ErrorCode.Success;
                    Write(response);
                    return;
                }
            }
            else
            {
                //没有兑换券
                Log.Warn("player {0} steps up failed: no item {1} ", uid, itemId);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }
        }

        /// <summary>
        /// 羁绊状态
        /// </summary>
        public void SyncDbUpdateHeroCombo()
        {
            string combo = HeroMng.GetComboList();
            server.GameDBPool.Call(new QueryUpdatHeroCombo(Uid, combo));
        }

        /// <summary>
        /// 伙伴重置
        /// </summary>
        /// <param name="heroId"></param>
        public void HeroRevert(int heroId)
        {
            MSG_ZGC_HERO_REVERT response = new MSG_ZGC_HERO_REVERT();
            response.HeroId = heroId;

            HeroInfo info = HeroMng.GetHeroInfo(heroId);
            if (info == null)
            {
                Log.Warn("player {0} hero {1} revert failed: no such hero info", uid, heroId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int level = info.Level;
            int resonanceLevel = info.Level;
            if (info.ResonanceIndex > 0)
            {
                level = wuhunResonanceMng.GetHeroRealLevel(info);
            }
            //if (info.IsPlayer())
            //{
            //    Log.Warn("player {0} hero {1} revert failed:  hero is player", uid, heroId);
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}

            //重置花费
            NormalItem costItem = BagManager.NormalBag.GetItem(HeroLibrary.RevertHeroCurrenciesType) as NormalItem;
            if (costItem == null || costItem.PileNum < HeroLibrary.RevertHeroCurrenciesNum)
            {
                //物品不足
                Log.Warn("player {0} hero {1} revert failed: item {2} num {3} error",
                    uid, heroId, HeroLibrary.RevertHeroCurrenciesType, HeroLibrary.RevertHeroCurrenciesNum);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //if (GetCoins((CurrenciesType)HeroLibrary.RevertHeroCurrenciesType) < HeroLibrary.RevertHeroCurrenciesNum)
            //{
            //    Log.Warn("player {0} hero {1} revert failed: SoulPower {2} error", uid, heroId, GetCoins((CurrenciesType)HeroLibrary.RevertHeroCurrenciesType));
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}

            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model == null)
            {
                Log.Warn("player {0} hero {1} revert failed: no such hero model", uid, heroId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            RewardManager reward = new RewardManager();

            if (!CheckResonanceLevel())
            {
                //返还魂力
                SoulPowerReset(level, model, reward);

                //返还魂晶
                SoulCrystalReset(info, level, reward);
            }
            //返还魂技材料
            SoulSkillReset(info, reward);
            //返还魂环
            SoulRingReset(heroId, reward);
            //返还魂骨
            SoulBoneRevert(heroId, reward);
            //返还装备
            EquipmentRevert(heroId, reward);
            //暗器
            bool revertHiddenWeapon = HiddenWeaponRevert(heroId, reward);

            //保留进阶
            //int stepsLevel = info.StepsLevel;
            //int equipIdex = info.EquipIndex;
            //int resonanceIndex = info.ResonanceIndex;
            //int godType = info.GodType;

            //增加新的
            //info = HeroMng.InitNewHero(model);
            //info.StepsLevel = stepsLevel;
            //info.EquipIndex = equipIdex;
            //info.ResonanceIndex = resonanceIndex;
            //info.GodType = godType;
            info.SetState(WuhunState.Normal);

            int beforePower = info.GetBattlePower();

            if (!CheckResonanceLevel())
            {
                //100级前会重置等级
                info.Level = model.InitLevel;
                info.AwakenLevel = model.AwakenLevel;
                info.Exp = 0;

                InitHeroTitleLevel(info);

                if (info.ResonanceIndex > 0)
                {
                    wuhunResonanceMng.UpdateGridHeroInfo(info);
                    wuhunResonanceMng.ResonanceHeroInfo(info);
                }

                HeroMng.InitHeroNatureInfo(info);
                wuhunResonanceMng.UpdateResonance(info, true);
            }
            else
            {
                InitHeroTitleLevel(info);

                HeroMng.InitHeroNatureInfo(info);
            }

            int afterPower = info.GetBattlePower();

            //同步
            SyncHeroChangeMessage(new List<HeroInfo>() { info });
            SyncDbUpdateHeroItem(info);
            HeroMng.NotifyClientBattlePowerFrom(info.Id);

            //发放奖励
            reward.BreakupRewards();
            AddRewards(reward, ObtainWay.HeroRevert);
            reward.GenerateRewardItemInfo(response.Rewards);


            if (response.Rewards.Count > 0 || revertHiddenWeapon)
            {
                //扣除花费
                //DelCoins((CurrenciesType)HeroLibrary.RevertHeroCurrenciesType, HeroLibrary.RevertHeroCurrenciesNum, ConsumeWay.HeroRevert, heroId.ToString());
                BaseItem item = DelItem2Bag(costItem, RewardType.NormalItem, HeroLibrary.RevertHeroCurrenciesNum, ConsumeWay.HeroRevert, heroId.ToString());
                if (item != null)
                {
                    SyncClientItemInfo(item);
                }

                response.Result = (int)ErrorCode.Success;
                Write(response);

                //伙伴重生
                AddTaskNumForType(TaskType.HeroRevert);

                //养成
                //BIRecordDevelopLog(DevelopType.HeroRevert, info.Id, resonanceLevel, info.Level, info.Id, info.Level);

                //komoelog
                List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, costItem.Id, HeroLibrary.RevertHeroCurrenciesNum);
                List<Dictionary<string, object>> rewardList = ParseRewardInfoToList(reward.RewardList);
                KomoeEventLogHeroReset(info.Id.ToString(), "", info.GetData().GetString("Job"), resonanceLevel, info.Level, beforePower, afterPower, afterPower - beforePower, consume, rewardList);
            }
            else
            {
                if (CheckResonanceLevel())
                {
                    response.Result = (int)ErrorCode.Success;
                }
                else
                {
                    Log.Warn("player {0} hero {1} revert failed: resonance level not enough", uid, heroId);
                    response.Result = (int)ErrorCode.Fail;
                }
                Write(response);
            }
        }

        private void InitHeroTitleLevel(HeroInfo info)
        {
            info.TitleLevel = GetHeroMaxTitle(info);
            HeroTitleModel title = HeroLibrary.GetHeroTitle(info.TitleLevel);
            if (title != null)
            {
                //增加天赋点
                info.InitTalentManager(title.TotalTalent, 0, 0, 0, 0);
            }
            else
            {
                info.InitTalentManager(0, 0, 0, 0, 0);
            }
        }

        private static void SoulCrystalReset(HeroInfo info, int level, RewardManager reward)
        {
            List<int> awakenLevels = HeroLibrary.GetAwakenLevels(info.Id);
            int getSoulCrystal = 0;
            foreach (var awakenLevel in awakenLevels)
            {
                if (awakenLevel < level)
                {
                    HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(awakenLevel);
                    if (heroLevel != null)
                    {
                        getSoulCrystal += heroLevel.SoulCrystal;
                    }
                }
                else if (info.State != WuhunState.WaitAwaken)
                {
                    if (awakenLevel == level)
                    {
                        HeroLevelModel heroLevel = HeroLibrary.GetHeroLevel(awakenLevel);
                        if (heroLevel != null)
                        {
                            getSoulCrystal += heroLevel.SoulCrystal;
                        }
                    }
                }
            }
            if (getSoulCrystal > 0)
            {
                reward.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulCrystal, getSoulCrystal));
            }
        }

        private static void SoulPowerReset(int level, HeroModel model, RewardManager reward)
        {
            int getSoulPower = 0;
            if (level > model.InitLevel)
            {
                int heroExp = HeroLibrary.GetHeroTotalExp(model.InitLevel, level - 1);
                getSoulPower = HeroLibrary.ExpToSoulPower * heroExp;
                if (getSoulPower > 0)
                {
                    reward.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.soulPower, getSoulPower));
                }
            }
        }

        public void SyncDbInsertHeroItem(HeroInfo hero)
        {
            server.GameDBPool.Call(new QueryInsertHeroInfo(Uid, hero));
            server.GameDBPool.Call(new QueryInsertEquipSlotInfo(Uid, hero.Id));
        }

        public void SyncDbUpdateHeroItem(HeroInfo hero)
        {
            server.GameDBPool.Call(new QueryUpdateHeroInfo(Uid, hero));
        }

        public void SyncDbUpdateHeroItemCrossChallengeQueue(HeroInfo hero)
        {
            server.GameDBPool.Call(new QueryUpdateHeroCrossChallengeInfo(Uid, hero));
        }

        public void SendHeroListMessage()
        {
            Dictionary<int, HeroInfo> heroInfo = HeroMng.GetHeroInfoList();
            if (heroInfo.Count > 50)
            {
                int total = 0;
                int count = 0;
                MSG_ZGC_HERO_LIST heroMsg = new MSG_ZGC_HERO_LIST();
                foreach (var item in heroInfo)
                {
                    if (count == 0)
                    {
                        heroMsg = new MSG_ZGC_HERO_LIST();
                    }
                    heroMsg.List.Add(GetHeroMessage(item.Value));
                    count++;
                    total++;
                    if (count == 50)
                    {
                        if (total == heroInfo.Count)
                        {
                            heroMsg.IsEnd = true;
                        }
                        Write(heroMsg);
                        count = 0;
                    }
                }
                if (count > 0)
                {
                    heroMsg.IsEnd = true;
                    Write(heroMsg);
                }
            }
            else
            {
                MSG_ZGC_HERO_LIST heroMsg = new MSG_ZGC_HERO_LIST();
                foreach (var task in heroInfo)
                {
                    heroMsg.List.Add(GetHeroMessage(task.Value));
                }
                heroMsg.IsEnd = true;
                Write(heroMsg);
            }

            //发送位置信息
            SendHeroPosInfos();
        }

        public void SyncHeroChangeMessage(List<HeroInfo> updateList, List<HeroInfo> addList = null)
        {
            List<HeroInfo> list = new List<HeroInfo>();
            if (updateList != null)
            {
                foreach (var heroInfo in updateList)
                {
                    list.Add(heroInfo);
                    if (list.Count > 10)
                    {
                        SendHeroChangeMessage(list);
                        list.Clear();
                    }
                }
            }
            if (list.Count > 0)
            {
                SendHeroChangeMessage(list, addList);
                list.Clear();
            }
            else if (addList != null && addList.Count > 0)
            {
                SendHeroChangeMessage(null, addList);
            }
        }

        private void SendHeroChangeMessage(List<HeroInfo> updateList, List<HeroInfo> addList = null)
        {
            MSG_ZGC_HERO_CHANGE msg = new MSG_ZGC_HERO_CHANGE();
            if (updateList != null)
            {
                foreach (var hero in updateList)
                {
                    msg.UpdateList.Add(GetHeroMessage(hero));
                }
            }
            if (addList != null)
            {
                foreach (var hero in addList)
                {
                    msg.AddList.Add(GetHeroMessage(hero));
                }
            }
            Write(msg);
        }

        public void SyncClientHeroInfo(List<int> updateHeros, List<int> addHeros = null)
        {
            if (addHeros == null || updateHeros == null) return;
            MSG_ZGC_HERO_CHANGE notify = new MSG_ZGC_HERO_CHANGE();
            if (addHeros.Count > 0)
            {
                foreach (var heroId in addHeros)
                {
                    HeroInfo info = HeroMng.GetHeroInfo(heroId);
                    notify.AddList.Add(GetHeroMessage(info));
                }
            }
            if (updateHeros.Count > 0)
            {
                foreach (var heroId in updateHeros)
                {
                    HeroInfo info = HeroMng.GetHeroInfo(heroId);
                    notify.UpdateList.Add(GetHeroMessage(info));
                }
            }
            Write(notify);
        }

        private MSG_ZGC_HERO_INFO GetHeroMessage(HeroInfo hero)
        {
            MSG_ZGC_HERO_INFO info = new MSG_ZGC_HERO_INFO();
            info.Id = hero.Id;
            info.EquipIndex = hero.EquipIndex;
            info.State = (int)hero.State;
            info.Level = hero.Level;
            info.Exp = hero.Exp;
            info.TitleLevel = hero.TitleLevel;
            info.AwakenLevel = hero.AwakenLevel;
            info.StepsLevel = hero.StepsLevel;
            info.Talent = GetHeroTalentInfo(hero);
            info.IsGod = hero.IsGod;
            info.ResonanceIndex = hero.ResonanceIndex;
            info.Power = hero.GetBattlePower();
            info.SoulSkillLevel = hero.SoulSkillLevel;
            info.GodType = hero.GodType;
            info.DefensivePositionNum = hero.DefensivePositionNum;
            info.DefensiveQueueNum = hero.DefensiveQueueNum;
            info.CrossPositionNum = hero.CrossPositionNum;
            info.CrossQueueNum = hero.CrossQueueNum;
            info.ThemeBossPositionNum = hero.ThemeBossPositionNum;
            info.ThemeBossQueueNum = hero.ThemeBossQueueNum;
            info.ComboPower = HeroMng.GetComboPower(hero.Nature);
            info.CrossBossPositionNum = hero.CrossBossPositionNum;
            info.CrossBossQueueNum = hero.CrossBossQueueNum;
            info.CarnivalBossPositionNum = hero.CarnivalBossPositionNum;
            info.CarnivalBossQueueNum = hero.CarnivalBossQueueNum;
            info.CrossChallengePositionNum = hero.CrossChallengePositionNum;
            info.CrossChallengeQueueNum = hero.CrossChallengeQueueNum;
            return info;
        }

        private MSG_ZGC_HERO_TALENT GetHeroTalentInfo(HeroInfo hero)
        {
            MSG_ZGC_HERO_TALENT info = new MSG_ZGC_HERO_TALENT();
            info.Strength = hero.TalentMng.StrengthNum;
            info.Physical = hero.TalentMng.PhysicalNum;
            info.Agility = hero.TalentMng.AgilityNum;
            info.Outburst = hero.TalentMng.OutburstNum;
            info.FreeNum = hero.TalentMng.FreeNum;
            return info;
        }

        const int HEROLISTMAXCOUNT = 200;
        public void SendHeroListTransform()
        {
            Dictionary<int, HeroInfo> heroInfo = HeroMng.GetHeroInfoList();
            MSG_ZMZ_HERO_POSES posMsg = new MSG_ZMZ_HERO_POSES();
            List<Tuple<int, int, Vec2>> defs = HeroMng.GetAllHeroPos();
            foreach (var item in defs)
            {
                posMsg.Defensive.Add(item.Item1);
                posMsg.HeroPos.Add(item.Item2);
            }
            server.ManagerServer.Write(posMsg, Uid);

            if (heroInfo.Count > HEROLISTMAXCOUNT)
            {
                int tempNum = 0;
                int totalNum = 0;
                MSG_ZMZ_HERO_LIST heroMsg = new MSG_ZMZ_HERO_LIST();
                foreach (var item in heroInfo)
                {
                    if (tempNum == 0)
                    {
                        heroMsg = new MSG_ZMZ_HERO_LIST();
                    }
                    heroMsg.List.Add(GetHeroTransform(item.Value));
                    tempNum++;
                    totalNum++;
                    if (totalNum == heroInfo.Count)
                    {
                        heroMsg.IsEnd = true;
                    }
                    if (tempNum == HEROLISTMAXCOUNT)
                    {
                        server.ManagerServer.Write(heroMsg, Uid);
                        tempNum = 0;
                    }
                }
                if (tempNum > 0)
                {
                    server.ManagerServer.Write(heroMsg, Uid);
                }
            }
            else
            {
                MSG_ZMZ_HERO_LIST heroMsg = new MSG_ZMZ_HERO_LIST();
                heroMsg.IsEnd = true;
                foreach (var task in heroInfo)
                {
                    heroMsg.List.Add(GetHeroTransform(task.Value));
                }
                server.ManagerServer.Write(heroMsg, Uid);
            }

            MSG_ZMZ_MAINQUEUE_INFO mainQueueMsg = HeroMng.GenerateMainQueueInfoTransformMsg();
            server.ManagerServer.Write(mainQueueMsg, Uid);
        }


        private ZMZ_HERO_INFO GetHeroTransform(HeroInfo hero)
        {
            ZMZ_HERO_INFO info = new ZMZ_HERO_INFO()
            {
                Id = hero.Id,
                EquipIndex = hero.EquipIndex,
                State = (int)hero.State,
                Level = hero.Level,
                Exp = hero.Exp,
                TitleLevel = hero.TitleLevel,
                AwakenLevel = hero.AwakenLevel,
                StepsLevel = hero.StepsLevel,
                Talent = GetHeroTalentTransform(hero),
                NatureList = GetNaturesTransform(hero),
                EquipSlotList = GetHeroSlotTransform(hero),
                IsGod = hero.IsGod,
                BattlePower = hero.GetBattlePower(),
                ResonanceIndex = hero.ResonanceIndex,
                SoulSkillLevel = hero.SoulSkillLevel,

                DefensivePositionNum = hero.DefensivePositionNum,
                DefensiveQueueNum = hero.DefensiveQueueNum,
                CrossQueueNum = hero.CrossQueueNum,
                CrossPositionNum = hero.CrossPositionNum,
                ThemeBossQueueNum = hero.ThemeBossQueueNum,
                ThemeBossPositionNum = hero.ThemeBossPositionNum,
                GodType = hero.GodType,
                CrossBossQueueNum = hero.CrossBossQueueNum,
                CrossBossPositionNum = hero.CrossBossPositionNum,
                CarnivalBossQueueNum = hero.CarnivalBossQueueNum,
                CarnivalBossPositionNum = hero.CarnivalBossPositionNum,
                CrossChallengeQueueNum = hero.CrossChallengeQueueNum,
                CrossChallengePositionNum = hero.CrossChallengePositionNum,
            };
            return info;
        }

        private ZMZ_HERO_TALENT GetHeroTalentTransform(HeroInfo hero)
        {
            ZMZ_HERO_TALENT info = new ZMZ_HERO_TALENT();
            if (hero.TalentMng != null)
            {
                info.Strength = hero.TalentMng.StrengthNum;
                info.Physical = hero.TalentMng.PhysicalNum;
                info.Agility = hero.TalentMng.AgilityNum;
                info.Outburst = hero.TalentMng.OutburstNum;
                info.FreeNum = hero.TalentMng.FreeNum;
            }
            return info;
        }

        public void LoadHeroPosTransform(MSG_ZMZ_HERO_POSES msg)
        {
            foreach (var item in msg.Defensive)
            {
                HeroMng.InitHeroPosFromTransform(item, msg.HeroPos[msg.Defensive.IndexOf(item)]);
            }
        }
        public void LoadHeroTransform(MSG_ZMZ_HERO_LIST msg)
        {
            foreach (var hero in msg.List)
            {
                HeroInfo info = new HeroInfo();
                info.Id = hero.Id;
                info.EquipIndex = hero.EquipIndex;
                info.SetState(hero.State);
                info.Level = hero.Level;
                info.Exp = hero.Exp;
                info.TitleLevel = hero.TitleLevel;
                info.AwakenLevel = hero.AwakenLevel;
                info.StepsLevel = hero.StepsLevel;
                info.IsGod = hero.IsGod;
                info.ResonanceIndex = hero.ResonanceIndex;
                info.UpdateBattlePower(hero.BattlePower);
                info.SoulSkillLevel = hero.SoulSkillLevel;


                info.DefensivePositionNum = hero.DefensivePositionNum;
                info.DefensiveQueueNum = hero.DefensiveQueueNum;
                info.CrossQueueNum = hero.CrossQueueNum;
                info.CrossPositionNum = hero.CrossPositionNum;
                info.ThemeBossQueueNum = hero.ThemeBossQueueNum;
                info.ThemeBossPositionNum = hero.ThemeBossPositionNum;
                info.GodType = hero.GodType;
                info.CrossBossQueueNum = hero.CrossBossQueueNum;
                info.CrossBossPositionNum = hero.CrossBossPositionNum;
                info.CarnivalBossQueueNum = hero.CarnivalBossQueueNum;
                info.CarnivalBossPositionNum = hero.CarnivalBossPositionNum;
                info.CrossChallengeQueueNum = hero.CrossChallengeQueueNum;
                info.CrossChallengePositionNum = hero.CrossChallengePositionNum;

                HeroTitleModel title = HeroLibrary.GetHeroTitle(info.TitleLevel);
                if (title != null)
                {
                    //增加天赋点
                    info.InitTalentManager(title.TotalTalent, hero.Talent.Strength, hero.Talent.Physical, hero.Talent.Agility, hero.Talent.Outburst);
                }
                else
                {
                    info.InitTalentManager(0, 0, 0, 0, 0);
                }

                info.BindData();

                foreach (var natureIt in hero.NatureList.Natures)
                {
                    info.Nature.SetNewNature((NatureType)natureIt.Type, natureIt.BaseValue, natureIt.AddedValue, natureIt.BaseRatio);
                }

                foreach (var slot in hero.EquipSlotList.SlotInfos)
                {
                    Dictionary<int, Slot> slots;
                    if (!EquipmentManager.GetSlotDic().TryGetValue(info.Id, out slots))
                    {
                        slots = new Dictionary<int, Slot>();
                        EquipmentManager.GetSlotDic().Add(info.Id, slots);
                    }

                    Slot slotItem = GetSlotFromTransform(slot);
                    Slot slotInfo;
                    if (!slots.TryGetValue(slotItem.Part, out slotInfo))
                    {
                        slotInfo = new Slot();
                        slots.Add(slot.Part, slotInfo);
                    }
                    slotInfo.CloneFrome(slotItem);
                    EquipmentManager.AddXuanyuCount(slotItem.JewelUid);
                }

                HeroMng.BindHeroInfoTransform(info);
            }
            InitNature();
        }

        private Dictionary<NatureType, int> GetResetHeroTalents(HeroInfo heroInfo)
        {
            Dictionary<NatureType, int> heroTalents = new Dictionary<NatureType, int>();
            heroTalents.Add(NatureType.PRO_POW, heroInfo.TalentMng.StrengthNum);
            heroTalents.Add(NatureType.PRO_CON, heroInfo.TalentMng.PhysicalNum);
            heroTalents.Add(NatureType.PRO_AGI, heroInfo.TalentMng.AgilityNum);
            heroTalents.Add(NatureType.PRO_EXP, heroInfo.TalentMng.OutburstNum);
            return heroTalents;
        }

        /// <summary>
        /// 伙伴出阵
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="equip"></param>
        //public void EquipHero(int heroId, bool equip)
        //{
        //    MSG_ZGC_EQUIP_HERO_RESULT res = new MSG_ZGC_EQUIP_HERO_RESULT();
        //    res.HeroId = heroId;
        //    res.Equip = equip;
        //    res.Result = (int)ErrorCode.Success;
        //    //
        //    var ans = HeroMng.TryEquipHero(heroId, equip);
        //    res.Result = (int)ans.Item2;
        //    Write(res);
        //}

        public void ChangeFollower(int heroId)
        {
            HeroMng.RemoveFollower();
            FollowerId = heroId;
            HeroMng.CallFollower();
            SyncDBFollower(heroId);

            if (heroId > 0)
            {
                AddTaskNumForType(TaskType.FollowHeroCount, 1, false);
            }
        }

        public void ChangeMainHero(int heroId)
        {

            HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId);
            if (heroInfo != null)
            {
                this.Icon = heroId;
                this.HeroId = heroId;
                this.GodType = HeroGodManager.GetHeroGodType(heroId);
                heroInfo.GodType = GodType;

                SyncDBMainHero();
                SyncRedisMainHero();
                //同步视野信息
                RemoveFromAoi();
                AddToAoi();

                Sync2ClientChangeMainHero();
            }
            else
            {
                MSG_ZGC_MAIN_HERO_CHANGE msg = new MSG_ZGC_MAIN_HERO_CHANGE();
                msg.HeroId = heroId;
                msg.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} change main hero {heroId} failed : hero not exists");
                Write(msg);
            }
        }

        public void Sync2ClientChangeMainHero()
        {
            //同步相关信息 通知前端更换模型
            MSG_ZGC_MAIN_HERO_CHANGE msg = new MSG_ZGC_MAIN_HERO_CHANGE();
            msg.HeroId = HeroId;
            msg.GodType = GodType;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        private void SyncDBMainHero()
        {
            string heroInfoString = "";
            HeroInfo heroInfo = HeroMng.GetHeroInfo(HeroId);
            if (heroInfo != null)
            {
                //heroInfoString = string.Format("{0}|{1}|{2}|{3}", heroInfo.Id, heroInfo.Level, heroInfo.TitleLevel, heroInfo.AwakenLevel);
                heroInfoString = heroInfo.Id.ToString();
            }
            else
            {
                int heroId = HeroMng.GetAllHeroPosHeroId().First();
                heroInfo = HeroMng.GetHeroInfo(heroId);
                if (heroInfo != null)
                {
                    //heroInfoString = string.Format("{0}|{1}|{2}|{3}", heroInfo.Id, heroInfo.Level, heroInfo.TitleLevel, heroInfo.AwakenLevel);
                    heroInfoString = heroInfo.Id.ToString();
                }
                else
                {
                    Log.Warn("player {0} SyncDBMainHero GetEquipHeroId {1} : not find info", Uid, heroId);
                }
            }
            QueryUpdateMainHero req = new QueryUpdateMainHero(Uid, heroInfoString, HeroGodManager.GetHeroGodType(HeroId));
            server.GameDBPool.Call(req);
        }

        private void SyncRedisMainHero()
        {
            OperateUpdateHeroId op = new OperateUpdateHeroId(Uid, HeroId);
            server.GameRedis.Call(op);

            server.GameRedis.Call(new OperateSetFaceIcon(Uid, HeroId, false));
            server.GameRedis.Call(new OperateSetGodType(Uid, HeroGodManager.GetHeroGodType(HeroId)));
        }

        public void SyncDBFollower(int heroId)
        {
            QueryUpdateHeroFollower req = new QueryUpdateHeroFollower(Uid, heroId);
            server.GameDBPool.Call(req);
        }

        public int GetMaxEquipHeroLevel()
        {
            int level = 1;
            if (WuhunResonanceMng != null && WuhunResonanceMng.ReferHeroInfo != null)
            {
                level = WuhunResonanceMng.ReferHeroInfo.Level;
            }
            else
            {
                Dictionary<int, HeroInfo> heroInfoList = HeroMng.GetEquipHeros();
                foreach (var heroInfo in heroInfoList)
                {
                    if (level == 1 || heroInfo.Value.Level < level)
                    {
                        level = heroInfo.Value.Level;
                    }
                }
            }
            return level;
        }

        /// <summary>
        /// 布阵 选取9个点中的一个
        /// </summary>
        public void UpdateHeroPos(MSG_GateZ_UPDATE_HERO_POS msg)
        {
            //before
            Dictionary<int, int> heroPosBefore = new Dictionary<int, int>(HeroMng.GetHeroPos());
            //int totalPowerBefore = 0;
            //foreach (var item in heroPosBefore)
            //{
            //    HeroInfo hero = HeroMng.GetHeroInfo(item.Key);
            //    if (hero != null)
            //    {
            //        totalPowerBefore += hero.GetBattlePower();
            //    }              
            //}

            MSG_ZGC_UPDATE_HERO_POS_RESULT result = new MSG_ZGC_UPDATE_HERO_POS_RESULT();
            result.Result = (int)ErrorCode.Success;
            HeroMng.hateIndexd = false;
            bool updateBattlePower = false;
            foreach (var temp in msg.HeroPos)
            {
                if (temp.Delete)
                {
                    HeroMng.DeleteHeroPos(temp.HeroId);
                    updateBattlePower = true;
                }
                else if (!HeroMng.UpdateHeroPos(temp.HeroId, temp.PosId))
                {
                    result.Result = (int)ErrorCode.Fail;
                    HeroMng.GetHeroPosMessage(result.HeroPos);
                    Write(result);
                    if (updateBattlePower)
                    {
                        //HeroMng.UpdateBattlePower(heroId);
                        //HeroMng.NotifyClientBattlePowerFrom(heroId);
                        HeroMng.NotifyClientBattlePower();
                        HeroMng.UpdatePlayerDefensiveHerosToRedis();
                    }
                    //UpdateJob();
                    return;
                }
            }

            HeroMng.UpdateMainBattleQueueHeroPos();

            HeroMng.GetHeroPosMessage(result.HeroPos);

            //HeroMng.UpdateBattlePower(heroId);
            //HeroMng.NotifyClientBattlePowerFrom(heroId);
            HeroMng.NotifyClientBattlePower();
            HeroMng.UpdatePlayerDefensiveHerosToRedis();

            Write(result);
            //UpdateJob();
            AddTaskNumForType(TaskType.HeroEquipCount, HeroMng.GetEquipHeroCount(), false);

            //komoeLog
            KomoeLogRecordBattleteamFlow("普通", heroPosBefore, new Dictionary<int, int>(HeroMng.GetHeroPos()));
        }

        public void UpdateJob()
        {
            Job = (JobType)HeroMng.GetFirstHeroJob();
        }

        private void UpdateJob2DB()
        {

        }

        public void SendHeroPosInfos()
        {
            MSG_ZGC_INIT_HERO_POS msg = new MSG_ZGC_INIT_HERO_POS();
            HeroMng.GetHeroPosMessage(msg.HeroPos);
            Write(msg);
        }

        public Hero NewHero(ZoneServerApi server, FieldObject owner, HeroInfo info)
        {
            Hero hero = HeroMng.NewHero(server, owner, info);
            hero.OwnerIsRobot = false;
            return hero;
        }


        public void SendMainBattleQueueInfoByLoading()
        {
            List<MainBattleQueueModel> queueModelList = HeroLibrary.GetAllSuitableLevelMainQueueModels(Level);
            HeroMng.UnlockMultiMainBattleQueue(queueModelList);         
        }

        public void SendMainBattleQueueInfo()
        {
            MSG_ZGC_MAINQUEUE_INFO response = new MSG_ZGC_MAINQUEUE_INFO();
            HeroMng.GenerateMainBattleQueueInfo(response);           
            Write(response);
        }

        public void UpdateMainBattleQueueHeroPos(MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS msg)
        {
            MSG_ZGC_UPDATE_MAINQUEUE_HEROPOS response = new MSG_ZGC_UPDATE_MAINQUEUE_HEROPOS();
            response.QueueNum = msg.QueueNum;

            MainBattleQueueInfo info;
            HeroMng.MainBattleQueue.TryGetValue(msg.QueueNum, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} UpdateMainBattleQueueHeroPos failed: queue {msg.QueueNum} is locked");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (msg.HeroPos.Count > HeroLibrary.HeroPosCount)
            {
                Log.Warn($"player {Uid} UpdateMainBattleQueueHeroPos queue {msg.QueueNum} failed: pos count error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            HeroMng.UpdateMainBattleQueueHeroPos(info, msg.HeroPos, response.HeroPos);

            if (info.BattleState == 1)//出战阵容
            {
                //更新heroPos
                HeroMng.UpdateOriginalMainHeroPos(info);

                HeroMng.NotifyClientBattlePower();
                HeroMng.UpdatePlayerDefensiveHerosToRedis();
                AddTaskNumForType(TaskType.HeroEquipCount, HeroMng.GetEquipHeroCount(), false);

                //副本英雄仇恨排序需更新
                HeroMng.hateIndexd = false;

                //主阵容同步
                MSG_ZGC_UPDATE_HERO_POS_RESULT result = new MSG_ZGC_UPDATE_HERO_POS_RESULT();
                result.Result = (int)ErrorCode.Success;
                HeroMng.GetHeroPosMessage(result.HeroPos);
                Write(result);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void UnlockMainBattleQueue(int queueNum)
        {
            MSG_ZGC_UNLOCK_MAINQUEUE response = new MSG_ZGC_UNLOCK_MAINQUEUE();
            response.QueueNum = queueNum;

            MainBattleQueueInfo info;
            HeroMng.MainBattleQueue.TryGetValue(queueNum, out info);
            if (info != null)
            {
                Log.Warn($"player {Uid} UnlockMainBattleQueue failed: queue {queueNum} is unlocked");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            MainBattleQueueModel queueModel = HeroLibrary.GetHeroMainQueueModel(queueNum);
            if (queueModel == null)
            {
                Log.Warn($"player {Uid} UnlockMainBattleQueue failed: not find queue {queueNum} in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            bool costDiamond = true;
            if (queueModel.LevelLimit > Level)
            {
                if (!queueModel.UnlockAhead)
                {
                    Log.Warn($"player {Uid} UnlockMainBattleQueue queue {queueNum} failed: can not unlock ahead");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                if (queueModel.CostDiamond > GetCoins(CurrenciesType.diamond))
                {
                    Log.Warn($"player {Uid} UnlockMainBattleQueue queue {queueNum} ahead failed: diamond not enough");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }             
            }
            else
            {
                if (!queueModel.LevelUnlock && queueModel.CostDiamond > GetCoins(CurrenciesType.diamond))
                {
                    Log.Warn($"player {Uid} UnlockMainBattleQueue queue {queueNum} failed: diamond not enough");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }
                if (queueModel.LevelUnlock)
                {
                    costDiamond = false;
                }
            }

            if (costDiamond && queueModel.CostDiamond > 0)
            {
                DelCoins(CurrenciesType.diamond, queueModel.CostDiamond, ConsumeWay.MainBattleQueue, queueNum.ToString());
            }

            HeroMng.UnlockMainBattleQueue(queueNum, queueModel, false);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void ChangeMainBattleQueueName(int queueNum, string queueName)
        {
            MSG_ZGC_CHANGE_MAINQUEUE_NAME response = new MSG_ZGC_CHANGE_MAINQUEUE_NAME();
            response.QueueNum = queueNum;
            response.Name = queueName;

            MainBattleQueueInfo info;
            HeroMng.MainBattleQueue.TryGetValue(queueNum, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} ChangeMainBattleQueueName failed: queue {queueNum} is locked");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!queueName.Equals(info.QueueName))
            {
                info.QueueName = queueName;
                SyncDbUpdateMainBttleQueueName(info);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        //设置出战
        public void MainBattleQueueDispatchBattle(int queueNum)
        {
            MSG_ZGC_MAINQUEUE_DISPATCH_BATTLE response = new MSG_ZGC_MAINQUEUE_DISPATCH_BATTLE();
            response.QueueNum = queueNum;

            MainBattleQueueInfo info;
            HeroMng.MainBattleQueue.TryGetValue(queueNum, out info);
            if (info == null)
            {
                Log.Warn($"player {Uid} MainBattleQueueDispatchBattle failed: queue {queueNum} is locked");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.HeroPosList.Count == 0)
            {
                Log.Warn($"player {Uid} MainBattleQueueDispatchBattle failed: heros not in queue {queueNum}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            var oldBattleQueue = HeroMng.MainBattleQueue.Where(x => x.Value.BattleState == 1).First();
            if (oldBattleQueue.Value != null)
            {
                oldBattleQueue.Value.BattleState = 0;
                SyncDbUpdateMainQueueBattleState(oldBattleQueue.Value);
            }

            info.BattleState = 1;
            SyncDbUpdateMainQueueBattleState(info);
           
            //更新heroPos
            HeroMng.UpdateOriginalMainHeroPos(info);

            //更新pet
            PetManager.SetMainQueueOnFightPet(info.PetUid);

            HeroMng.NotifyClientBattlePower();
            HeroMng.UpdatePlayerDefensiveHerosToRedis();
            AddTaskNumForType(TaskType.HeroEquipCount, HeroMng.GetEquipHeroCount(), false);

            //副本英雄仇恨排序需更新
            HeroMng.hateIndexd = false;

            //主阵容同步
            MSG_ZGC_UPDATE_HERO_POS_RESULT result = new MSG_ZGC_UPDATE_HERO_POS_RESULT();
            result.Result = (int)ErrorCode.Success;
            HeroMng.GetHeroPosMessage(result.HeroPos);
            Write(result);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void CheckUnlockMainBattleQueue()
        {
            List<MainBattleQueueModel> queueModelList = HeroLibrary.GetCurLevelMainQueueModels(Level);
            HeroMng.UnlockMultiMainBattleQueue(queueModelList);
        }

        //传承
        public void HeroInherit(int fromHeroId, int toHeroId)
        {
            MSG_ZGC_HERO_INHERIT response = new MSG_ZGC_HERO_INHERIT();
     
            HeroInfo fromHero = HeroMng.GetHeroInfo(fromHeroId);
            HeroInfo toHero = HeroMng.GetHeroInfo(toHeroId);
            if (fromHero == null || toHero == null)
            {
                Log.Warn("player {0} HeroInherit failed: not find from hero {1} or to hero {2}", uid, fromHeroId, toHeroId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (fromHero.ResonanceIndex == 0 || toHero.ResonanceIndex == 0)
            {
                Log.Warn("player {0} HeroInherit failed: from hero {1} or to hero {2} not in resonance", uid, fromHeroId, toHeroId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (fromHero.GetData().GetInt("Job") != toHero.GetData().GetInt("Job"))
            {
                Log.Warn("player {0} HeroInherit failed: not same job", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (fromHero.Level != toHero.Level || ResonanceLevel < WuhunResonanceConfig.ResonanceUpLevel)
            {
                Log.Warn("player {0} HeroInherit failed: level not same or resonanceLevel not enough", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            string[] inheritCost = HeroLibrary.GetHeroInheritCost();
            if (inheritCost.Length < 3)
            {
                Log.Warn("player {0} HeroInherit failed: item cost param error", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //有吸收中的魂环不给操作
            Bag_SoulRing soulRingBag = bagManager.SoulRingBag;
            List<int> onAbsorbHeros = soulRingBag.GetSoulRingAbsorbHeroList();
            if (onAbsorbHeros.Contains(fromHero.Id) || onAbsorbHeros.Contains(toHero.Id))
            {
                Log.Warn($"player {Uid} HeroInherit failed: hero has soulRing on absorb");
                response.Result = (int)ErrorCode.SoulRingOnAbsort;
                Write(response);
                return;
            }

            BaseItem item = null;
            int costId = int.Parse(inheritCost[0]);
            int costType = int.Parse(inheritCost[1]);
            int costNum = int.Parse(inheritCost[2]);

            switch ((RewardType)costType)
            {
                case RewardType.Currencies:
                    
                    if (GetCoins((CurrenciesType)costId) < costNum)
                    {
                        Log.Warn($"player {Uid} CanoeGameStart failed: diamond {GetCoins((CurrenciesType)costId)} not enough, need num {costNum}");
                        response.Result = (int)ErrorCode.DiamondNotEnough;
                        Log.Write(response);
                        return;
                    }
                    DelCoins((CurrenciesType)costId, costNum, ConsumeWay.HeroInherit, toHero.Id.ToString());
                    break;
                case RewardType.NormalItem:
                    item = BagManager.GetItem(MainType.Consumable, costId);
                    if (item == null || item.PileNum < costNum)
                    {
                        Log.Warn($"player {Uid} HeroInherit failed: item not enough");
                        response.Result = (int)ErrorCode.ItemNotEnough;
                        Write(response);
                        return;
                    }
                    BaseItem it = DelItem2Bag(item, RewardType.NormalItem, costNum, ConsumeWay.HeroInherit);
                    if (it != null)
                    {
                        SyncClientItemInfo(it);
                    }
                    break;
                default:
                    break;
            }
                            
            //天赋互换
            HeroSwapTalent(fromHero, toHero);

            //称号等级互换
            HeroSwapTitleLevel(fromHero, toHero);

            List<BaseItem> updateList = new List<BaseItem>();
            List<BaseItem> deleteList = new List<BaseItem>();

            //魂环互换
            SoulRingManager.HeroSwapSoulRing(fromHero, toHero, updateList, deleteList);

            //魂骨互换
            HeroSwapSoulBone(fromHero, toHero, updateList, deleteList);

            //BI
            Dictionary<int, int[]> biEquipsInfo = new Dictionary<int, int[]>();
            CreateInheritBiEquipsInfo(biEquipsInfo);

            //装备/玄玉互换
            EquipmentManager.HeroSwapEquipment(fromHero.Id, toHero.Id, updateList, deleteList, biEquipsInfo);

            //暗器互换
            HiddenWeaponManager.HeroSwapHiddenWeapon(fromHero.Id, toHero.Id, updateList, deleteList, biEquipsInfo);

            //魂技互换
            HeroSwapSoulSkillLevel(fromHero, toHero);

            HeroMng.InitHeroNatureInfo(fromHero);
            HeroMng.InitHeroNatureInfo(toHero);

            HeroMng.NotifyClientBattlePowerFrom(fromHero.Id);
            HeroMng.NotifyClientBattlePowerFrom(toHero.Id);
           
            SyncHeroChangeMessage(new List<HeroInfo>() { fromHero, toHero });

            SyncDbUpdateHeroItem(fromHero);
            SyncDbUpdateHeroItem(toHero);

            response.FromHeroId = fromHero.Id;
            response.ToHeroId = toHero.Id;

            response.Result = (int)ErrorCode.Success;
            Write(response);

            //同步
            SyncClientItemsInfo(deleteList);
            SyncClientItemsInfo(updateList);

            //BILog
            BIRecordInheritLog(toHero.Id, fromHero.Id, biEquipsInfo);
        }

        //天赋互换
        private void HeroSwapTalent(HeroInfo fromHero, HeroInfo toHero)
        {
            Dictionary<NatureType, int> fromOldTalents = GetResetHeroTalents(fromHero);
            Dictionary<NatureType, int> toOldTalents = GetResetHeroTalents(toHero);

            HeroMng.SwapTalent(fromHero, toHero);
          
            Dictionary<NatureType, int> fromNewTalents = GetResetHeroTalents(fromHero);
            Dictionary<NatureType, int> toNewTalents = GetResetHeroTalents(toHero);

            //天赋属性变动
            HeroMng.HeroInheritTalent(fromHero, fromOldTalents, fromNewTalents);
            HeroMng.HeroInheritTalent(toHero, toOldTalents, toNewTalents);
        }

        //魂骨互换
        private void HeroSwapSoulBone(HeroInfo fromHero, HeroInfo toHero, List<BaseItem> updateList, List<BaseItem> deleteList)
        {
            SoulboneMng.HeroSwapSuit(fromHero.Id, toHero.Id, deleteList);
            SoulboneMng.UpdateSoulBoneInfo(fromHero.Id, toHero.Id, updateList);
        }

        //魂技互换
        private void HeroSwapSoulSkillLevel(HeroInfo fromHero, HeroInfo toHero)
        {
            int toOldLevel = toHero.SoulSkillLevel;
            toHero.SetSoulSkillLevel(fromHero.SoulSkillLevel);
            fromHero.SetSoulSkillLevel(toOldLevel);
        }

        //称号等级互换
        private void HeroSwapTitleLevel(HeroInfo fromHero, HeroInfo toHero)
        {
            int toOldLevel = toHero.TitleLevel;
            toHero.TitleLevel = fromHero.TitleLevel;
            fromHero.TitleLevel = toOldLevel;
        }

        private void SyncDbUpdateMainBttleQueueName(MainBattleQueueInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateMainBattleQueueName(Uid, info.QueueNum, info.QueueName));
        }

        private void SyncDbUpdateMainQueueBattleState(MainBattleQueueInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateMainQueueBattleState(Uid, info.QueueNum, info.BattleState));
        }
        private void KomoeLogRecordBattleteamFlow(string stage_type, Dictionary<int, int> heroPosBefore, Dictionary<int, int> heroPosAfter)
        {
            Dictionary<string, object> beforeDic;

            List<Dictionary<string, object>> beforeList = new List<Dictionary<string, object>>();
            int totalPowerBefore = 0;
            if (heroPosBefore != null)
            {
                foreach (var hero in heroPosBefore)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(hero.Key);
                    if (heroInfo != null)
                    {
                        int battlePower = heroInfo.GetBattlePower();
                        beforeDic = new Dictionary<string, object>();
                        beforeDic.Add("位置id", hero.Value);
                        beforeDic.Add("hero_id", hero.Key);
                        beforeDic.Add("hero_power", battlePower);
                        beforeList.Add(beforeDic);

                        totalPowerBefore += battlePower;
                    }
                }
            }
        

            List<Dictionary<string, object>> afterList = new List<Dictionary<string, object>>();
            int totalPowerAfter = 0;
            if (heroPosAfter != null)
            {
                foreach (var hero in heroPosAfter)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(hero.Key);
                    if (heroInfo != null)
                    {
                        int battlePower = heroInfo.GetBattlePower();
                        beforeDic = new Dictionary<string, object>();
                        beforeDic.Add("位置id", hero.Value);
                        beforeDic.Add("hero_id", hero.Key);
                        beforeDic.Add("hero_power", battlePower);
                        afterList.Add(beforeDic);

                        totalPowerAfter += battlePower;
                    }
                }
            }
          
            //List<Dictionary<string, object>> afterList = ParseMainHeroPosPowerList(heroPosAfter, out totalPowerAfter);
            KomoeEventLogBattleteamFlow(stage_type, beforeList, afterList, totalPowerBefore, totalPowerAfter, totalPowerAfter - totalPowerBefore);
        }

        private void KomoeLogRecordBattleteamFlow(string stage_type, Dictionary<int, Dictionary<int, HeroInfo>> heroPosBefore, Dictionary<int, Dictionary<int, HeroInfo>> heroPosAfter)
        {
            Dictionary<string, object> beforeDic;

            List<Dictionary<string, object>> beforeList = new List<Dictionary<string, object>>();
            int totalPowerBefore = 0;
            foreach (var heroQueen in heroPosBefore)
            {
                foreach (var hero in heroQueen.Value)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(hero.Key);
                    if (heroInfo != null)
                    {
                        int battlePower = heroInfo.GetBattlePower();
                        beforeDic = new Dictionary<string, object>();
                        beforeDic.Add("位置id", hero.Value);
                        beforeDic.Add("hero_id", hero.Key);
                        beforeDic.Add("hero_power", battlePower);
                        beforeList.Add(beforeDic);

                        totalPowerBefore += battlePower;
                    }
                }
            }

            List<Dictionary<string, object>> afterList = new List<Dictionary<string, object>>();
            int totalPowerAfter = 0;
            foreach (var heroQueen in heroPosAfter)
            {
                foreach (var hero in heroQueen.Value)
                {
                    HeroInfo heroInfo = HeroMng.GetHeroInfo(hero.Key);
                    if (heroInfo != null)
                    {
                        int battlePower = heroInfo.GetBattlePower();
                        beforeDic = new Dictionary<string, object>();
                        beforeDic.Add("位置id", hero.Value);
                        beforeDic.Add("hero_id", hero.Key);
                        beforeDic.Add("hero_power", battlePower);
                        afterList.Add(beforeDic);

                        totalPowerAfter += battlePower;
                    }
                }
            }
            //List<Dictionary<string, object>> afterList = ParseMainHeroPosPowerList(heroPosAfter, out totalPowerAfter);
            KomoeEventLogBattleteamFlow(stage_type, beforeList, afterList, totalPowerBefore, totalPowerAfter, totalPowerAfter - totalPowerBefore);
        }
    }
}

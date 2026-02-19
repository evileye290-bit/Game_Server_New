using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using DataProperty;
using System.Text;
using Newtonsoft.Json;

namespace ZoneServerLib
{
    public partial class HeroManager
    {
        PlayerChar owner;

        // 实例化了的英雄列表
        Dictionary<int, Hero> heroList;

        // 拥有的英雄列表
        Dictionary<int, HeroInfo> heroInfoList;

        //// 装备出战的英雄列表
        //Dictionary<int, int> heroEquipList;

        //激活的羁绊
        public List<int> HeroComboList = new List<int>();
        Dictionary<int, int> ComboGroupList = new Dictionary<int, int>();
        //羁绊属性
        public Dictionary<int, int> NatureRatio = new Dictionary<int, int>();

        private bool enterMapCalced = false;

        public HeroManager(PlayerChar owner)
        {
            this.owner = owner;
            heroInfoList = new Dictionary<int, HeroInfo>();
            heroList = new Dictionary<int, Hero>();
            //heroEquipList = new Dictionary<int, int>();
        }

        public void InitCombo(string heroCombo)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            List<int> heroCombos = StringSplit.GetInts(heroCombo);
            heroCombos.Sort();
            foreach (var comboId in heroCombos)
            {
                HeroComboModel model = DrawLibrary.GetHeroComboModel(comboId);
                if (model != null)
                {
                    ComboGroupList[model.Group] = comboId;

                    List<int> groupList = DrawLibrary.GetComboGroupList(model.Group);
                    if (groupList != null)
                    {
                        foreach (var item in groupList)
                        {
                            dic[item] = 0;
                            if (item == comboId)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            foreach (var kv in dic)
            {
                HeroComboModel model = DrawLibrary.GetHeroComboModel(kv.Key);
                if (model != null)
                {
                    AddNatureRatio(model);
                }
            }
            foreach (var kv in ComboGroupList)
            {
                HeroComboList.Add(kv.Value);
            }
        }

        public void PlayerHeroOnceCalcs()
        {
            if (enterMapCalced)
            {
                return;
            }
            foreach (var info in heroInfoList)
            {

                UpdateBattlePower(info.Value);
                //Natures nature = info.Value.Calc4to9Natures();
                //int power = ScriptManager.BattlePower.CaculateBattlePower(nature);
                //info.Value.UpdateBattlePower(power);
            }
            enterMapCalced = true;
            UpdatePlayerBattlePower2Redis();
        }

        public Dictionary<int, HeroInfo> GetEquipHeros()
        {
            Dictionary<int, HeroInfo> heroInfoList = new Dictionary<int, HeroInfo>();
            foreach (var id in heroPos)
            {
                HeroInfo info = GetHeroInfo(id.Key);
                heroInfoList.Add(id.Key, info);
            }
            return heroInfoList;
        }
        public void NotifyClientBattlePower()
        {
            int battlePower = CalcBattlePower();
            //非equip的成员也会有触发计算，会有重复发送的问题，前端来控制
            MSG_ZGC_HERO_BATTLEPOWER msg = new MSG_ZGC_HERO_BATTLEPOWER();
            //foreach (var id in heroPos)
            //{
            //    HeroInfo info = GetHeroInfo(id.Key);
            //    battlePower += info.GetBattlePower();
            //}
            msg.FightingCapacity = battlePower;
            owner.Write(msg);

            //活动：战力
            //owner.AddActivityNumForType(EnumerateUtility.Activity.ActivityAction.BattlePower, battlePower);
            UpdatePlayerBattlePower2Redis(battlePower);

            owner.SerndUpdateRankValue(RankType.BattlePower, battlePower);

            owner.server.GameRedis.Call(new OperateUpdateRankScore(RankType.BattlePower, owner.server.MainId, owner.Uid, battlePower, owner.server.Now()));

            owner.server.GameRedis.Call(new OperateUpdateCampRankScore(RankType.CampBattlePower, owner.Camp, owner.server.MainId, owner.Uid, battlePower, owner.server.Now()));

            //战力到指定值发称号卡               
            List<int> paramList = new List<int>() { battlePower };
            owner.TitleMng.UpdateTitleConditionCount(TitleObtainCondition.BattlePower, 1, paramList);
        }

        public void NotifyClientBattlePowerFrom(int heroId)
        {
            if (heroPos.ContainsKey(heroId))
            {
                NotifyClientBattlePower();
            }
        }

        public int CalcBattlePower()
        {
            int FightingCapacity = 0;
            foreach (var id in heroPos)
            {
                HeroInfo info = GetHeroInfo(id.Key);
                if (info != null)
                {
                    int battlePower = info.GetBattlePower();
                    FightingCapacity += battlePower;
                }
            }
            //pet在阵上
            int petBattlePower = owner.PetManager.GetPetBattlePower();
            FightingCapacity += petBattlePower;
            return FightingCapacity;
        }

        public int GetComboPower(Natures nature)
        {
            int ratio = 0;
            Natures newNature = new Natures();
            foreach (var item in NatureLibrary.Basic9Nature)
            {
                if (NatureRatio.TryGetValue(item.Value, out ratio))
                {
                    //有属性加成
                    long value = nature.GetNatureBaseValue(item.Key);
                    value = (long)(value * (ratio * 0.0001f) + 1);
                    newNature.AddNatureBaseValue(item.Key, value);
                }
            }
            //计算战力
            int power = ScriptManager.BattlePower.CaculateBattlePower(newNature);
            return power;
        }

        public void UpdatePlayerBattlePower2Redis(int battlePower = 0)
        {
            //int heroId = heroEquipList[1];
            //HeroInfo info = null;
            //if (heroInfoList.TryGetValue(heroId, out info))
            {
                if (battlePower == 0)
                {
                    battlePower = CalcBattlePower();// info.GetBattlePower();
                }
                //int power = CalcBattlePower();// info.GetBattlePower();
                owner.server.GameRedis.Call(new OperateUpdateBattlePower(owner.Uid, battlePower));

                owner.ShopManager.UpdateMaxBattlePower(battlePower);

                UpdateMaxBattlePower(battlePower);
            }
        }

        public void UpdateMaxHeroLevel(int level)
        {
            OperateLoadMaxBattlePower operateLoad = new OperateLoadMaxBattlePower(owner.Uid);
            owner.server.GameRedis.Call(operateLoad, (object msg) =>
            {
                if ((int)msg == 1 && level > operateLoad.HeroLevel)
                {
                    OperateUpdateMaxHeroLevel operateUpdate = new OperateUpdateMaxHeroLevel(owner.Uid, level);
                    owner.server.GameRedis.Call(operateUpdate);
                }
            });
        }

        public void UpdateMaxBattlePower(int battlePower)
        {
            OperateLoadMaxBattlePower operateLoad = new OperateLoadMaxBattlePower(owner.Uid);
            owner.server.GameRedis.Call(operateLoad, (object msg) =>
            {
                if ((int)msg == 1 && battlePower > operateLoad.BattlePower)
                {
                    OperateUpdateMaxBattlePower operateUpdate = new OperateUpdateMaxBattlePower(owner.Uid, battlePower);
                    owner.server.GameRedis.Call(operateUpdate);
                }
            });
        }


        public string GetDefensiveHeros()
        {
            string heros = string.Empty;

            foreach (var kv in heroPos)
            {
                heros += kv.Key + "|";
            }

            return heros;
        }

        public void UpdateBattlePower(int heroId)
        {
            HeroInfo info = GetHeroInfo(heroId);
            if (info != null)
            {
                UpdateBattlePower(info);     
            }
            else
            {
                Log.Warn("player {0} UpdateBattlePower not find heroId {1}", owner.Uid, heroId);
            }
        }

        private int CaculateSkillBattleAndSoulRingPower(HeroInfo heroInfo)
        {
            float battlePower = 0;
            float skillLevelFactor = SkillLibrary.GetSkillBattlePowerFactor(heroInfo.SoulSkillLevel / 10 + 1);

            int addYearRatio = SoulRingManager.GetAddYearRatio(heroInfo.StepsLevel);
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            if (heroModel == null) return 0;
            List<int> equipedSoulRings = new List<int>();
            foreach (var skill in heroModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skill);
                if (skillModel == null) continue;

                SoulRingItem item = owner.SoulRingManager.GetEquipedSoulRing(heroInfo.Id, skillModel.SoulRingPos);
                if (item == null) continue;

                battlePower += skillModel.BattlePower * skillLevelFactor;

                int currentYear = SoulRingManager.GetAffterAddYear(item.Year, addYearRatio);
                //魂环被动战力
                battlePower += BattlePowerLibrary.SoulRingBattlePowerBasic * ScriptManager.SoulRing.GetNatureGrowthFactor(currentYear);

                if (item.Element > 0)
                {
                    //魂环元素战力
                    int yearLevel = ScriptManager.SoulRing.GetElementGrowthFactor(item.Year);
                    Data data = DataListManager.inst.GetData("SoulRingElementBattlePower", yearLevel);
                    if (data != null)
                    {
                        battlePower += BattlePowerLibrary.SoulRingElementBattlePowerBasic * data.GetInt("SoulRingElementBattlePowerFactor");
                    }
                }

                //魂环附加效果战力（没有转换成对应战力值的属性） 
                if (!equipedSoulRings.Contains(item.Id))
                {
                    foreach (var nature in item.AdditionalNatures)
                    {
                        battlePower += SoulRingLibrary.GetAdditionalNatrueBattlePower(nature.Key, nature.Value);
                    }
                    equipedSoulRings.Add(item.Id);
                }
            }

            return (int)battlePower;
        }

        private int CaculateSoulBoneBattlePower(HeroInfo heroInfo)
        {
            int battlePower = 0;
            List<SoulBone> soulBoneList = owner.SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
            if (soulBoneList == null)
            {
                return 0;
            }
            foreach (var item in soulBoneList)
            {
                item.GetSpecList().ForEach(x =>
                {
                    SoulBoneSpecModel model = SoulBoneLibrary.GetSpecModel(x);
                    if (model != null)
                    {
                        battlePower += model.BattlePower;
                    }
                });
            }
            return battlePower;
        }

        private int CaculateTravelCardBattlePower(HeroInfo heroInfo)
        {
            return owner.TravelMng.BattlePower;
        }

        private float CaculateStepBattlePowerRatio(HeroInfo heroInfo)
        {
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            if (heroModel == null) return 0f;
            int level = heroInfo.StepsLevel / 6;
            if (level > 0 && heroModel.StepBattlePowerFactor.Count>=level)
            {
                return heroModel.StepBattlePowerFactor[level - 1];
            }
            return 0;
        }

        private int CaculateHiddenWeaponBattlePower(HeroInfo heroInfo)
        {
            int battlePower = 0;
            ulong weaponId = owner.HiddenWeaponManager.GetHeroEquipWeaponId(heroInfo.Id);
            if (weaponId == 0) return 0;

            HiddenWeaponItem weaponItem = owner.BagManager.HiddenWeaponBag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null) return 0;

            HiddenWeaponStarModel model = HiddenWeaponLibrary.GetHiddenWeaponStarModel(weaponItem.Model.Quality, weaponItem.Info.Star);
            if (model != null)
            {
                battlePower += model.BattlePower;
            }

            foreach (var item in weaponItem.Model.SpecList)
            {
                HiddenWeaponSpecialModel specialModel = HiddenWeaponLibrary.GetHiddenWeaponSpecialModel(item);
                if (specialModel != null)
                {
                    battlePower += specialModel.BattlePower;
                }
            }

            return battlePower;
        }

        private void AddNatureRatio(Natures nature)
        {
            foreach (var ratio in NatureRatio)
            {
                nature.AddNatureRatio((NatureType)ratio.Key, ratio.Value);
            }
        }

        public void BindHeroInfo(HeroInfo info)
        {
            if (info == null)
            {
                return;
            }
            info.BindData();

            if (heroInfoList.ContainsKey(info.Id))
            {
                heroInfoList[info.Id] = info;
            }
            else
            {
                heroInfoList.Add(info.Id, info);
            }

        }

        public void BindHeroNature(HeroInfo info)
        {
            BindHeroQueueList(info);
            InitHeroNatureInfo(info); //属性初始化及附加物品属性
            //if (info.EquipIndex > 0)
            //{
            //    AddHero2Equip(info);
            //}
        }

        public void BindHerosNature()
        {
            foreach (var info in heroInfoList)
            {
                BindHeroNature(info.Value);
            }
        }

        public void BindHeroInfoTransform(HeroInfo info)
        {
            if (info == null)
            {
                return;
            }

            if (heroInfoList.ContainsKey(info.Id))
            {
                heroInfoList[info.Id] = info;
            }
            else
            {
                heroInfoList.Add(info.Id, info);
            }
            BindHeroQueueList(info);
            //if (info.EquipIndex > 0)
            //{
            //    AddHero2Equip(info);
            //}
        }

        public HeroInfo InitNewHero(HeroModel model)
        {
            HeroInfo info = new HeroInfo();
            info.Id = model.Id;
            info.EquipIndex = 0;
            info.Exp = 0;
            info.Level = model.InitLevel;
            info.AwakenLevel = model.AwakenLevel;
            info.TitleLevel = owner.GetHeroMaxTitle(info);

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

            info.StepsLevel = 0;


            //if (info.Level >= 10)
            //{
            //    //等待吸收魂环
            //    info.State = WuhunState.WaitAbsorb;
            //}
            //else
            //{
            info.SetState(WuhunState.Normal);
            //}


            info.BindData();
            return info;
        }

        public Dictionary<int, HeroInfo> GetHeroInfoList()
        {
            return heroInfoList;
        }

        public HeroInfo GetHeroInfo(int heroId)
        {
            HeroInfo info = null;
            heroInfoList.TryGetValue(heroId, out info);
            return info;
        }

        public HeroInfo GetPlayerHeroInfo()
        {
            return GetHeroInfo(owner.HeroId);
        }

        public Hero GetHero(int heroId)
        {
            Hero hero = null;
            heroList.TryGetValue(heroId, out hero);
            return hero;
        }

        public Hero AddHero(int heroId)
        {
            Hero hero = NewHero(heroId);
            hero.InitNatureExt(owner.NatureValues, owner.NatureRatios);
            hero.Init();
            AddHeroList(hero);
            return hero;
        }

        public Hero NewHero(int heroId)
        {
            HeroInfo info = GetHeroInfo(heroId);
            if (info == null)
            {
                Log.Error($"Owner {owner.Uid} new hero {heroId} fail");
            }
            return NewHero(owner.server, owner, info);
        }

        public Hero NewHero(ZoneServerApi server, FieldObject owner, HeroInfo info)
        {
            Hero hero = new Hero(server, owner, info);
            return hero;
        }

        public int GetEquipHeroCount()
        {
            return heroPos.Count();
        }

        public int GetGoldHeroCount()
        {
            return heroInfoList.Values.Where(x => x.IsGod).Count();
        }

        public Dictionary<int, int> hateIndexed = new Dictionary<int, int>();
        public bool hateIndexd = false;

        //获取hate的排序
        public int GetHeroIdHateEquip(int heroId)
        {
            if (hateIndexd)
            {
                return hateIndexed[heroId];
            }
            else
            {
                List<int> ids = new List<int>();
                foreach (var kv in heroPos)
                {
                    ids.Add(kv.Key);
                }
                ids.Sort((left, right) =>
                {
                    if (HeroLibrary.GetHeroModel(left).HateRatio < HeroLibrary.GetHeroModel(right).HateRatio)
                    {
                        return -1;
                    }
                    return 1;
                });
                for (int i = 0; i < ids.Count; i++)
                {
                    hateIndexed[ids[i]] = i + 1;
                }
                hateIndexd = true;
            }

            return hateIndexed[heroId];
        }

        private void AddHeroList(Hero hero)
        {
            heroList[hero.HeroId] = hero;
        }

        private void RemoveFollower(int heroId)
        {
            Hero hero = null;
            if (heroList.TryGetValue(heroId, out hero))
            {
                heroList.Remove(hero.HeroId);
                if (!owner.CurrentMap.IsDungeon)
                {
                    owner.CurrentMap.RemoveHero(hero.InstanceId);
                }
            }
        }

        public void CallFollower()
        {
            int heroId = owner.FollowerId;
            if (heroId <= 0 || heroId == owner.HeroId)
            {
                return;
            }
            if (!owner.CurrentMap.IsDungeon)
            {
                CallHero(heroId);
            }
        }

        public void RemoveFollower()
        {
            int heroId = owner.FollowerId;
            RemoveFollower(heroId);
        }

        public void UpdatePlayerDefensiveHerosToRedis()
        {
            owner.server.GameRedis.Call(new OperateUpdateDefensive(owner.Uid, GetDefensiveHeros()));
        }

        private void UpdateHeroIndex2Info(int heroId, int index)
        {
            HeroInfo info = GetHeroInfo(heroId);
            if (info == null)
            {
                return;
            }
            info.EquipIndex = index;
        }

        private void SyncDBEquipHero(int heroId, int index)
        {
            HeroInfo info = GetHeroInfo(heroId);
            if (info == null)
            {
                return;
            }
            owner.server.GameDBPool.Call(new QueryUpdateHeroIndex(owner.Uid, info.Id, index));
        }

        private void SyncClientEquipHero(List<int> ids)
        {
            List<HeroInfo> infos = new List<HeroInfo>();
            foreach (int id in ids)
            {
                HeroInfo info = GetHeroInfo(id);
                infos.Add(info);
            }
            owner.SyncHeroChangeMessage(infos);
        }

        public void LevelUp(HeroInfo info)
        {
            if (info.ResonanceIndex > 0)
            {
                return;
            }
            //升级
            info.Level++;

            //判断状态
            HeroAwakenModel awaken = HeroLibrary.GetHeroAwakenModel(info.Id);
            if (awaken.AwakenLevelList.Contains(info.Level))
            {
                //需要觉醒
                info.State = WuhunState.WaitAwaken;
            }
            //else
            //{
            //    if ((info.Level % 10) == 0)
            //    {
            //        //等待吸收魂环
            //        info.State = WuhunState.WaitAbsorb;
            //    }
            //}

            owner.WuhunResonanceMng.UpdateResonance(info, false);
        }


        public void StepsLevelUp(HeroInfo info, int addLevel)
        {
            int oldLevel = info.StepsLevel;
            //升级
            info.StepsLevel += addLevel;

            //HeroRemoveStepsRatio(info, oldLevel);
            ////添加进阶加成
            //HeroAddStepsRatio(info);

            //更新属性
            InitHeroNatureInfo(info);
            NotifyClientBattlePowerFrom(info.Id);
        }

        public void HeroRemoveStepsRatio(HeroInfo info, int level)
        {
            int sC = 0;
            GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(level);
            if (stepsModel != null)
            {
                sC = stepsModel.StepsC;
            }
            if (sC > 0)
            {
                foreach (var type in NatureLibrary.Basic9Nature)
                {
                    info.AddNatureRatio(type.Key, -sC);
                }
            }

            HeroModel heroModel = HeroLibrary.GetHeroModel(info.Id);
            if (heroModel == null) return;
            HeroStepNatureModel natureModel = NatureLibrary.GetHeroStepNatureModel(heroModel.Quality, level);
            if (natureModel == null) return;

            natureModel.NatureList?.ForEach(x => info.AddNatureAddedValue(x.Key, -(long)x.Value));
        }

        public void HeroAddStepsRatio(HeroInfo info)
        {
            int sC = 0;
            GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(info.StepsLevel);
            if (stepsModel != null)
            {
                sC = stepsModel.StepsC;
            }
            if (sC > 0)
            {
                foreach (var type in NatureLibrary.Basic9Nature)
                {
                    info.AddNatureRatio(type.Key, sC);
                }
            }

            HeroModel heroModel = HeroLibrary.GetHeroModel(info.Id);
            if (heroModel == null) return;

            HeroStepNatureModel natureModel = NatureLibrary.GetHeroStepNatureModel(heroModel.Quality, info.StepsLevel);
            if (natureModel == null) return;

            natureModel.NatureList?.ForEach(x => info.AddNatureAddedValue(x.Key, (long)x.Value));
        }

        public void Awaken(HeroInfo info)
        {
            if (info.ResonanceIndex > 0)
            {
                return;
            }

            int oldLevel = info.AwakenLevel;
            ////觉醒
            //if ((info.Level % 10) == 0)
            //{
            //    //等待吸收魂环
            //    info.State = WuhunState.WaitAbsorb;
            //}
            //else
            //{
            info.State = WuhunState.Normal;
            //}
            //提升觉醒等级
            info.SetAwakenLevel(HeroLibrary.GetHeroAwakenModel(info.Id));
            int newLevel = info.AwakenLevel;

            if (oldLevel != newLevel)
            {

                ////属性 4转9 (属于加成属性)
                //////成长值
                //float oldGC = GetGroVal(oldLevel, info.IsPlayer());
                //float newGC = GetGroVal(newLevel, info.IsPlayer());


                //Dictionary<NatureType, Int64> oldNatures = GetAwakenNature9List(info, oldGC);
                //Dictionary<NatureType, Int64> newNatures = GetAwakenNature9List(info, newGC);
                //Dictionary<NatureType, Int64> changeNature = GetChangeNature9List(oldNatures, newNatures);
                //foreach (var item in changeNature)
                //{
                //    info.AddNatureAddedValue(item.Key, item.Value);
                //}


                //更新属性
                InitHeroNatureInfo(info);
                NotifyClientBattlePowerFrom(info.Id);
            }
        }
        public Dictionary<NatureType, Int64> GetChangeNature9List(Dictionary<NatureType, Int64> oldList, Dictionary<NatureType, Int64> newList)
        {
            Dictionary<NatureType, Int64> NatureList = new Dictionary<NatureType, Int64>();

            if (oldList != null && newList != null)
            {
                foreach (var old in oldList)
                {
                    Int64 incrValue;
                    if (newList.TryGetValue(old.Key, out incrValue))
                    {
                        NatureList[old.Key] = incrValue - old.Value;
                    }
                    else
                    {
                        NatureList[old.Key] = old.Value;
                    }
                }
            }
            else if (oldList != null)
            {
                foreach (var basic in oldList)
                {
                    NatureList[basic.Key] = -basic.Value;
                }
            }
            else if (newList != null)
            {
                foreach (var incr in newList)
                {
                    NatureList[incr.Key] = incr.Value;
                }
            }

            return NatureList;
        }

        public Dictionary<NatureType, Int64> GetAwakenNature9List(HeroInfo info, float gC)
        {
            Dictionary<NatureType, Int64> natures = new Dictionary<NatureType, Int64>();
            foreach (var nature4 in NatureLibrary.GetNature4To9List())
            {
                Int64 nature4Value = info.GetNatureValue(nature4.Key);
                foreach (var nature9 in nature4.Value)
                {
                    Int64 newValue = (Int64)(nature9.Value * nature4Value * gC);
                    if (natures.ContainsKey(nature9.Key))
                    {
                        natures[nature9.Key] += newValue;
                    }
                    else
                    {
                        natures[nature9.Key] = newValue;
                    }
                }
            }

            return natures;
        }

        public void DefaultSoulRingAwaken(HeroInfo info, int maxTitle)
        {
            HeroAwakenModel awaken = HeroLibrary.GetHeroAwakenModel(info.Id);
            info.SetAwakenLevel(awaken);

            TitleUp(info, maxTitle);
        }

        public void TitleUp(HeroInfo info, int maxTitle)
        {
            //称号等级增加
            info.TitleLevel = maxTitle;

            HeroTitleModel title = HeroLibrary.GetHeroTitle(info.TitleLevel);
            if (title != null)
            {
                //增加天赋点
                info.TalentMng.SetTalentNum(title.TotalTalent);
            }
            //owner.WuhunResonanceMng.UpdateResonance(info);
        }

        //public void AbsorbSoulRing(HeroInfo info)
        //{
        //    if (info.ResonanceIndex>0)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        //觉醒
        //        info.State = WuhunState.Normal;
        //    }
        //}

        // 召唤英雄
        public void CallHero(int heroId)
        {
            HeroInfo info = GetHeroInfo(heroId);
            if (info == null)
            {
                return;
            }

            if (GetHero(heroId) != null)
            {
                return;
            }

            Hero hero = AddHero(heroId);

            if (hero == null)
            {
                return;
            }
            // 地图内同步
            owner.CurrentMap.CreateHero(hero);

        }

        ///// <summary>
        ///// 废弃 不再需要 地图自己删除hero
        ///// </summary>
        ///// <param name="heroId"></param>
        ///// <param name="syncDb"></param>
        //public void RecallHero(int heroId, bool syncDb)
        //{
        //    //Hero hero = GetHero(heroId);
        //    //if (hero == null)
        //    //{
        //    //    return;
        //    //}
        //    //RecallHero(hero, syncDb);
        //}

        // 玩家进图后，自动召唤:分类处理召唤跟随和组队等
        public void CallHero2Map()
        {
            switch (owner.CurrentMap.GetMapType())
            {
                case MapType.Map:
                    CallHero2Map(CallHeroRule.Follower);
                    break;
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.IntegralBoss:
                case MapType.HuntingActivityTeam:
                    TeamDungeonMap map = owner.CurrentMap as TeamDungeonMap;
                    if (map == null)
                    {
                        return;
                    }
                    if (map.TheoryMemberCount < 3)
                    {
                        CallHero2Map(CallHeroRule.AllEquip);
                    }
                    else
                    {
                        CallHero2Map(CallHeroRule.AllEquip);
                    }
                    break;
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                    CallHero2Map(CallHeroRule.AllEquip);
                    break;
                case MapType.Tower:
                    {
                        //有自己的上阵hero信息
                        owner.TowerManager.HeroPos.ForEach(x => CallHero(x.Key));
                    }
                    break;
                case MapType.Arena:
                case MapType.CrossBattle:
                case MapType.CrossFinals:
                case MapType.IslandChallenge:
                case MapType.CrossChallenge:
                case MapType.CrossChallengeFinals:
                    break;
                case MapType.ThemeBoss:
                    foreach (var kv in ThemeBossQueue)
                    {
                        foreach (var hero in kv.Value)
                        {
                            CallHero(hero.Value.Id);
                        }
                    }
                    break;
                case MapType.CarnivalBoss:
                    foreach (var kv in CarnivalBossQueue)
                    {
                        foreach (var hero in kv.Value)
                        {
                            CallHero(hero.Value.Id);
                        }
                    }
                    break;
                case MapType.HuntingIntrude:
                    {
                        //有自己的上阵hero信息
                        owner.HuntingManager.HuntingIntrudeHeroPos.ForEach(x => CallHero(x.Key));
                    }
                    break;
                default:
                    break;
            }
        }

        public int CallHeroCount()
        {
            int count = GetEquipHeroCount();
            switch (owner.CurrentMap.GetMapType())
            {
                case MapType.Map:
                    return 1 < count ? 1 : count;
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                case MapType.IntegralBoss:
                //TeamDungeonMap map = owner.CurrentMap as TeamDungeonMap;
                //if (map == null)
                //{
                //    return 0;
                //}
                //if (map.TheoryMemberCount < 3)
                //{
                //    return 2 < count ? 2 : count;
                //}
                //else
                //{
                //    return 1 < count ? 1 : count;
                //}
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                //case MapType.Arena:
                case MapType.CrossBattle:
                case MapType.CrossFinals:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.Tower:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                case MapType.CrossChallenge:
                case MapType.CrossChallengeFinals:
                    return count;
                case MapType.ThemeBoss:
                    return ThemeBossQueue.Values.Select(x => x.Values.Count).Sum();
                case MapType.CrossBoss:
                    return  CrossBossQueue.Values.Select(x => x.Values.Count).Sum();
                case MapType.CarnivalBoss:
                    return CarnivalBossQueue.Values.Select(x => x.Values.Count).Sum();
                case MapType.HuntingIntrude:
                    return owner.HuntingManager.HuntingIntrudeHeroPos.Count;
                default:
                    return 0;
            }
        }

        // 玩家离图后，自动召回
        public void TakeBackHeroFromMap()
        {
            switch (owner.CurrentMap.GetMapType())
            {
                case MapType.Map:
                    TakeBackHeroFromMap(CallHeroRule.Follower);
                    break;
                case MapType.TeamDungeon:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                    TeamDungeonMap map = owner.CurrentMap as TeamDungeonMap;
                    if (map == null)
                    {
                        return;
                    }
                    if (map.TheoryMemberCount < 3)
                    {
                        TakeBackHeroFromMap(CallHeroRule.AllEquip);
                    }
                    else
                    {
                        TakeBackHeroFromMap(CallHeroRule.AllEquip);
                    }
                    break;
                case MapType.CommonSingleDungeon:
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulBreath:
                case MapType.SoulPower:
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.IntegralBoss:
                case MapType.SecretArea:
                case MapType.Chapter:
                case MapType.GodPath:
                case MapType.GodPathAcrossOcean:
                case MapType.CampBattle:
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                case MapType.CampDefense:
                case MapType.Tower:
                case MapType.CampBattleNeutral:
                case MapType.PushFigure:
                case MapType.HuntingActivitySingle:
                case MapType.ThemeBoss:
                case MapType.CrossBoss:
                case MapType.CarnivalBoss:
                case MapType.HuntingIntrude:
                    TakeBackHeroFromMap(CallHeroRule.AllEquip);
                    break;
                default:
                    break;
            }
        }

        private void CallHero2Map(CallHeroRule type)
        {
            switch (type)
            {
                //case CallHeroRule.None:
                //    break;
                //case CallHeroRule.One:
                //    //if (heroEquipList.Count > 0)
                //    //{
                //    //    int heroId = 0;
                //    //    if ((heroEquipList.TryGetValue(1, out heroId)))
                //    //    {
                //    //        if (heroId != owner.HeroId)
                //    //        {
                //    //            CallHero(heroId);
                //    //        }
                //    //    }
                //    //}
                //    if (heroEquipList.Count > 0)
                //    {
                //        int heroId = 0;
                //        for (int callCount = 1; callCount <= 2; callCount++)
                //        {
                //            if ((heroEquipList.TryGetValue(callCount, out heroId)))
                //            {
                //                //if (heroId != owner.HeroId)
                //                {
                //                    CallHero(heroId);
                //                }
                //            }
                //        }
                //    }
                //    break;
                //case CallHeroRule.Two:
                //    if (heroEquipList.Count > 0)
                //    {
                //        int heroId = 0;
                //        for (int callCount = 1; callCount <= 3; callCount++)
                //        {
                //            if ((heroEquipList.TryGetValue(callCount, out heroId)))
                //            {
                //                //if (heroId != owner.HeroId)
                //                {
                //                    CallHero(heroId);
                //                }
                //            }
                //        }
                //    }
                //    break;
                case CallHeroRule.AllEquip:
                    foreach (var equip in heroPos)//改为新的上阵逻辑
                    {
                        //if (equip.Value != owner.HeroId)
                        {
                            CallHero(equip.Key);
                        }
                    }
                    break;
                case CallHeroRule.Follower:
                    CallFollower();
                    break;
                default:
                    break;
            }
        }

        private void TakeBackHeroFromMap(CallHeroRule type)
        {
            switch (type)
            {
                case CallHeroRule.None:
                    break;
                case CallHeroRule.One:
                case CallHeroRule.Two:
                case CallHeroRule.Follower:
                case CallHeroRule.AllEquip:
                    foreach (var item in heroList)
                    {
                        if (item.Value.InstanceId > 0 && item.Value.HeroId != owner.HeroId)
                        {
                            owner.CurrentMap.RemoveHero(item.Value.InstanceId);
                            //item.Value.SetInstanceId(0);
                        }
                    }
                    heroList.Clear();
                    break;
                default:
                    break;
            }
        }

        //初始化英雄信息
        public void InitHeroNatureInfo(HeroInfo heroInfo)
        {
            //基础属性
            NatureDataModel heroBasicNatureModel = NatureLibrary.GetHeroBasicNatureModel(heroInfo.Id);
            if (heroBasicNatureModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetHeroBasicNatureModel is null, hero id is {1}", owner.Uid, heroInfo.Id);
            }
            NatureDataModel heroBasicAddedNatureModel = NatureLibrary.GetHeroBasicAddedNatureModel(heroInfo.Id);
            if (heroBasicAddedNatureModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetHeroBasicAddedNatureModel is null, hero id is {1}", owner.Uid, heroInfo.Id);
            }
            //成长属性
            NatureDataModel tempNatureIncrModel = NatureLibrary.GetBasicNatureIncrModel(heroInfo.Level);
            if (tempNatureIncrModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetBasicNatureIncrModel is null, hero Level is {2}", owner.Uid, heroInfo.Id, heroInfo.Level);
            }
            heroInfo.Nature.Clear();
            //两个属性相乘
            Dictionary<NatureType, Int64> basicNatures = GetBasicNatureList(heroBasicNatureModel, tempNatureIncrModel, heroBasicAddedNatureModel);
            //13项基础属性
            heroInfo.InitBasicNature(basicNatures);
            //初始化移动速度
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            heroInfo.InitSpeed(heroModel.PRO_RUN_IN_BATTLE, heroModel.PRO_RUN_OUT_BATTLE);
            //称号徽章属性
            HeroTitleModel title = HeroLibrary.GetHeroTitle(heroInfo.TitleLevel);
            if (title != null)
            {
                //增加属性
                if (title.BaseNature.Count > 0)
                {
                    foreach (var item in title.BaseNature)
                    {
                        heroInfo.AddNatureAddedValue((NatureType)item.Key, item.Value);
                    }
                }
            }

            //魂骨套装
            //魂骨属性加成(属于加成属性)
            List<SoulBone> soulBoneList = owner.SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
            if (soulBoneList != null)
            {
                foreach (var bone in soulBoneList)
                {
                    heroInfo.AddBoneNatureEffect(bone);
                }
            }
            Dictionary<NatureType, int> soulBoneAdds = owner.SoulboneMng.GetEnhancedHeroBoneAdditions(heroInfo.Id);
            if (soulBoneAdds != null)
            {
                foreach (var kv in soulBoneAdds)
                {
                    heroInfo.AddNatureAddedValue(kv.Key, kv.Value);
                }
            }

            //阵营养成属性加成 (属于加成属性)        
            List<CampStarModel> campStarsList = GetCampStarsList();
            if (owner.Camp != CampType.None && owner.CheckLimitOpen(LimitType.CampStars) && campStarsList != null)
            {
                foreach (var model in campStarsList)
                {
                    foreach (var kv in model.AttrList)
                    {
                        heroInfo.AddCampStarNatureEffect(kv);
                    }
                }
            }

            int addYearRatio = SoulRingManager.GetAddYearRatio(heroInfo.StepsLevel);

            //魂环属性加成(属于加成属性)
            Dictionary<int, SoulRingItem> soulRingList = owner.SoulRingManager.GetAllEquipedSoulRings(heroInfo.Id);
            int soulRingCount = 0;

            if (soulRingList != null)
            {
                soulRingCount = soulRingList.Count;
                foreach (var item in soulRingList)
                {
                    SoulRingAdd(heroInfo, item.Value.GetMainAttrs(addYearRatio));
                    //SoulRingAdd(heroInfo, item.Value.GetUltAttrs());
                }
            }

            if (soulRingCount > 0)
            {
                //魂技属性加成
                int soulRingLevel = heroInfo.SoulSkillLevel;

                Dictionary<NatureType, float> addNature = new Dictionary<NatureType, float>();

                NatureDataModel natureDataModel = SoulSkillLibrary.GetBasicNatureIncrModel(soulRingLevel);
                heroInfo.AddSoulSkillNature(natureDataModel.NatureList);
            }

            //装备属性加成(属于加成属性)
            Dictionary<NatureType, long> equipmentNatureList = owner.EquipmentManager.CalcAllEquipedEquipmentsNatures(heroInfo.Id, heroInfo.Level); ;
            UpdateEquipmentNature(heroInfo, equipmentNatureList, null);

            //羁绊加成
            foreach (var item in NatureRatio)
            {
                heroInfo.AddNatureRatio((NatureType)item.Key, item.Value);
            }

            //添加进阶加成
            HeroAddStepsRatio(heroInfo);

            //暗器
            HeroAddHiddenWeaponNature(heroInfo);

            //属性 4转9 (属于加成属性)
            ////成长值
            float gC = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());
            foreach (var nature4 in NatureLibrary.GetNature4To9List())
            {
                Int64 nature4Value = heroInfo.GetNatureValue(nature4.Key);
                foreach (var nature9 in nature4.Value)
                {
                    Int64 newValue = (Int64)(nature9.Value * gC * nature4Value);
                    heroInfo.AddNatureAddedValue(nature9.Key, newValue);
                }
            }

            //成神
            //int godType = owner.GetHeroGod(heroInfo.Id);
            //HeroGodDetailModel detilModel = GodHeroLibrary.GetHeroGodDetailModel(godType);
            //if (heroInfo != null && detilModel != null)
            //{
            //    GodHeroLibrary.NatureTypes.ForEach(x => heroInfo.AddNatureRatio(x, detilModel.NatureRatio));
            //}

            //称号加成
            foreach (var nature in owner.TitleMng.NatureList)
            {
                heroInfo.AddNatureAddedValue((NatureType)nature.Key, nature.Value);
            }

            //魂环附加效果 策划需求总属性的百分比
            if (soulRingList != null)
            {
                foreach (var soulRing in soulRingList.Values)
                {
                    foreach (var nature in soulRing.AdditionalNatures)
                    {
                        long natureValue = heroInfo.GetNatureValue((NatureType)nature.Key);
                        heroInfo.AddNatureAddedValue((NatureType)nature.Key, (long)(natureValue * (0.0001f * nature.Value)));
                    }
                }
            }

            //最后设置
            heroInfo.InitHp();
            //计算战力
            UpdateBattlePower(heroInfo);
        }

        public void UpdateBattlePower(HeroInfo info)
        {
            ////四项转九项属性
            //Natures nature = info.Calc4to9Natures();
            ////添加羁绊加成
            //AddNatureRatio(nature);
            ////添加进阶加成
            //FieldObject.AddStepsRatio(info, nature);
            //计算战力
            int power = ScriptManager.BattlePower.CaculateBattlePower(info.Nature);

            int fixBattlePower = 0;
            // 星级变色提升战力百分比
            float ratio = CaculateStepBattlePowerRatio(info);

            //成神战斗力
            if (info.GodType > 0)
            {
                HeroGodStepUpGrowthModel model = GodHeroLibrary.GetGodStepUpGrowthModel(info.GodType, info.StepsLevel);
                if (model != null)
                {
                    ratio += model.BattlePowerRatio;
                    fixBattlePower += model.BattlePowerFixValue;
                }
                else
                {
                    Log.Error($"UpdateBattlePower had not find HeroGodStepUpGrowthModel model hero {info.Id} god {info.GodType} step {info.StepsLevel}");
                }
            }

            //装备附魔战斗力
            List<EquipmentItem> equipments = owner.EquipmentManager.GetAllEquipedEquipments(info.Id);
            if (equipments?.Count > 0)
            {
                var specialModels = EquipLibrary.GetEquipSeSpecialModelsByEquipIds(equipments.Select(x => x.Model.ID));
                if (specialModels.Count > 0)
                {
                    power += specialModels.Sum(x => x.BattlePower);
                }
            }

            //提升基础战力百分比
            power = (int)(power * (1 + ratio)) + fixBattlePower;
            power += CaculateSkillBattleAndSoulRingPower(info);
            power += CaculateSoulBoneBattlePower(info);
            power += CaculateTravelCardBattlePower(info);
            power += CaculateHiddenWeaponBattlePower(info);
            //更新战力
            info.UpdateBattlePower(power);

            if (owner.ArenaMng.DefensiveHeros.ContainsKey(info.Id))
            {
                owner.UpdateDefensivePower();
            }

            if (info.CrossQueueNum > 0)
            {
                owner.SyncCrossHeroQueueMsg(0, 0);
            }

            if (info.DefensiveQueueNum > 0)
            {
                owner.UpdateFortDefensiveQueue();
            }

            if (info.CrossBossQueueNum > 0)
            {
                owner.SyncCrosBossHeroQueuMsg(ChallengeIntoType.CrossBossReturn);
            }

            if (info.CrossChallengeQueueNum > 0)
            {
                owner.SyncCrossChallengeHeroQueueMsg(0, 0);
            }

            //#if DEBUG
            //            Log.Debug($"------------------------------------------------------ {info.Id} UpdateBattlePower nature 1");
            //            Log.Info1(JsonConvert.SerializeObject(info.Nature.GetNatureList()));
            //            Log.Debug($"------------------------------------------------------ {info.Id} UpdateBattlePower nature 2 {info.GetBattlePower()}");
            //#endif
        }


        public void InitHeroNatureInfoForLog(HeroInfo heroInfo)
        {
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo ———————————————————— start", owner.Uid, heroInfo.Id);
            //基础属性
            NatureDataModel heroBasicNatureModel = NatureLibrary.GetHeroBasicNatureModel(heroInfo.Id);
            if (heroBasicNatureModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetHeroBasicNatureModel is null, hero id is {1}", owner.Uid, heroInfo.Id);
            }
            NatureDataModel heroBasicAddedNatureModel = NatureLibrary.GetHeroBasicAddedNatureModel(heroInfo.Id);
            if (heroBasicAddedNatureModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetHeroBasicAddedNatureModel is null, hero id is {1}", owner.Uid, heroInfo.Id);
            }
            //成长属性
            NatureDataModel tempNatureIncrModel = NatureLibrary.GetBasicNatureIncrModel(heroInfo.Level);
            if (tempNatureIncrModel == null)
            {
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo GetBasicNatureIncrModel is null, hero Level is {2}", owner.Uid, heroInfo.Id, heroInfo.Level);
            }
            heroInfo.Nature.Clear();
            //两个属性相乘
            Dictionary<NatureType, Int64> basicNatures = GetBasicNatureList(heroBasicNatureModel, tempNatureIncrModel, heroBasicAddedNatureModel);

            //if (heroBasicNatureModel != null)
            //{
            //    Log.ErrorLine("player {0} hero {1} InitHeroNatureInfo heroBasicNatureModel: {2}", owner.Uid, heroInfo.Id, owner.server.JsonSerialize.Serialize(heroBasicNatureModel));
            //}
            //if (tempNatureIncrModel != null)
            //{
            //    Log.ErrorLine("player {0} hero {1} InitHeroNatureInfo tempNatureIncrModel: {2}", owner.Uid, heroInfo.Id, owner.server.JsonSerialize.Serialize(tempNatureIncrModel));
            //}
            //if (heroBasicAddedNatureModel != null)
            //{
            //    Log.ErrorLine("player {0} hero {1} InitHeroNatureInfo heroBasicAddedNatureModel: {2}", owner.Uid, heroInfo.Id, owner.server.JsonSerialize.Serialize(heroBasicAddedNatureModel));
            //}
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo basicNatures: {2}", owner.Uid, heroInfo.Id, ParseToString(basicNatures));
            //13项基础属性
            heroInfo.InitBasicNature(basicNatures);
            //初始化移动速度
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            heroInfo.InitSpeed(heroModel.PRO_RUN_IN_BATTLE, heroModel.PRO_RUN_OUT_BATTLE);
            //称号徽章属性
            HeroTitleModel title = HeroLibrary.GetHeroTitle(heroInfo.TitleLevel);
            if (title != null)
            {
                //增加属性
                if (title.BaseNature.Count > 0)
                {
                    foreach (var item in title.BaseNature)
                    {
                        heroInfo.AddNatureAddedValue((NatureType)item.Key, item.Value);
                    }
                    Log.WarnLine("player {0} hero {1} InitHeroNatureInfo title.BaseNature: {2}", owner.Uid, heroInfo.Id, ParseToString(title.BaseNature));
                }
            }

            //魂骨套装
            //魂骨属性加成(属于加成属性)
            List<SoulBone> soulBoneList = owner.SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
            if (soulBoneList != null)
            {
                foreach (var bone in soulBoneList)
                {
                    heroInfo.AddBoneNatureEffect(bone);
                    Log.WarnLine("player {0} hero {1} InitHeroNatureInfo bone: {2}", owner.Uid, heroInfo.Id, owner.server.JsonSerialize.Serialize(bone));
                }
            }
            Dictionary<NatureType, int> soulBoneAdds = owner.SoulboneMng.GetEnhancedHeroBoneAdditions(heroInfo.Id);
            if (soulBoneAdds != null)
            {
                foreach (var kv in soulBoneAdds)
                {
                    heroInfo.AddNatureAddedValue(kv.Key, kv.Value);
                }
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo soulBoneAdds: {2}", owner.Uid, heroInfo.Id, ParseToString(soulBoneAdds));
            }

            //阵营养成属性加成 (属于加成属性)        
            List<CampStarModel> campStarsList = GetCampStarsList();
            if (owner.Camp != CampType.None && owner.CheckLimitOpen(LimitType.CampStars) && campStarsList != null)
            {
                List<string> tempCampStars = new List<string>();
                foreach (var model in campStarsList)
                {
                    foreach (var kv in model.AttrList)
                    {
                        heroInfo.AddCampStarNatureEffect(kv);
                    }
                    tempCampStars.Add(ParseToString(model.AttrList));
                }
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo campStarsList: {2}", owner.Uid, heroInfo.Id, owner.server.JsonSerialize.Serialize(tempCampStars));
            }

            int addYearRatio = SoulRingManager.GetAddYearRatio(heroInfo.StepsLevel);
            Dictionary<int, List<string>> tempSoulRingList = new Dictionary<int, List<string>>();
            //魂环属性加成(属于加成属性)
            Dictionary<int, SoulRingItem> soulRingList = owner.SoulRingManager.GetAllEquipedSoulRings(heroInfo.Id);
            int soulRingCount = 0;

            if (soulRingList != null)
            {
                soulRingCount = soulRingList.Count;
                foreach (var item in soulRingList)
                {
                    SoulRingAdd(heroInfo, item.Value.GetMainAttrs(addYearRatio));
                    //SoulRingAdd(heroInfo, item.Value.GetUltAttrs());

                    string tempString1 = ParseToString(item.Value.GetMainAttrs(addYearRatio));
                    //string tempString2 = ParseToString(item.Value.GetUltAttrs());
                    tempSoulRingList.Add(item.Key, new List<string>() { tempString1 });
                }
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo tempSoulRingList: {2}", owner.Uid, heroInfo.Id, ParseToString(tempSoulRingList));
            }

            if (soulRingCount > 0)
            {
                //魂技属性加成
                int soulRingLevel = heroInfo.SoulSkillLevel;
       
                NatureDataModel natureDataModel = SoulSkillLibrary.GetBasicNatureIncrModel(soulRingLevel);
                heroInfo.AddSoulSkillNature(natureDataModel.NatureList);

                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo natureDataModel.NatureList: {2}", owner.Uid, heroInfo.Id, ParseToString(natureDataModel.NatureList));
            }

            //装备属性加成(属于加成属性)
            Dictionary<NatureType, long> equipmentNatureList = owner.EquipmentManager.CalcAllEquipedEquipmentsNatures(heroInfo.Id, heroInfo.Level); ;
            UpdateEquipmentNature(heroInfo, equipmentNatureList, null);
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo equipmentNatureList: {2}", owner.Uid, heroInfo.Id, ParseToString(equipmentNatureList));

            //羁绊加成
            foreach (var item in NatureRatio)
            {
                heroInfo.AddNatureRatio((NatureType)item.Key, item.Value);
            }
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo NatureRatio: {2}", owner.Uid, heroInfo.Id, ParseToString(NatureRatio));

            //添加进阶加成
            int sC = 0;
            GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(heroInfo.StepsLevel);
            if (stepsModel != null)
            {
                sC = stepsModel.StepsC;
            }
            if (sC > 0)
            {
                foreach (var type in NatureLibrary.Basic9Nature)
                {
                    heroInfo.AddNatureRatio(type.Key, sC);
                }
            }
      

            HeroStepNatureModel natureModel = NatureLibrary.GetHeroStepNatureModel(heroModel.Quality, heroInfo.StepsLevel);
            if (natureModel != null)
            {
                natureModel.NatureList?.ForEach(x => heroInfo.AddNatureAddedValue(x.Key, (long)x.Value));
                Log.WarnLine("player {0} hero {1} InitHeroNatureInfo natureModel.NatureList: sC {2} add {3}", owner.Uid, heroInfo.Id, sC, ParseToString(natureModel.NatureList));
            }

            HeroAddHiddenWeaponNature(heroInfo);

            //属性 4转9 (属于加成属性)
            ////成长值
            Dictionary<NatureType, Int64> tempDic = new Dictionary<NatureType, long>();

            float gC = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());
            foreach (var nature4 in NatureLibrary.GetNature4To9List())
            {
                Int64 nature4Value = heroInfo.GetNatureValue(nature4.Key);
                tempDic[nature4.Key] = nature4Value;
                foreach (var nature9 in nature4.Value)
                {
                    Int64 newValue = (Int64)(nature9.Value * gC * nature4Value);
                    heroInfo.AddNatureAddedValue(nature9.Key, newValue);

                }
            }
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo tempDic: gC {2} add {3}", owner.Uid, heroInfo.Id, gC, ParseToString(tempDic));

            //称号加成
            foreach (var nature in owner.TitleMng.NatureList)
            {
                heroInfo.AddNatureAddedValue((NatureType)nature.Key, nature.Value);
            }
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo owner.TitleMng.NatureList: {2}", owner.Uid, heroInfo.Id, ParseToString(owner.TitleMng.NatureList));

            //魂环附加效果
            if (soulRingList != null)
            {
                foreach (var soulRing in soulRingList.Values)
                {
                    foreach (var nature in soulRing.AdditionalNatures)
                    {
                        long natureValue = heroInfo.GetNatureValue((NatureType)nature.Key);
                        heroInfo.AddNatureAddedValue((NatureType)nature.Key, (long)(natureValue * (0.0001f * nature.Value)));
                    }
                    Log.WarnLine("player {0} hero {1} InitHeroNatureInfo soulRing.AdditionalNatures: {2}", owner.Uid, heroInfo.Id, ParseToString(soulRing.AdditionalNatures));
                }
            }

            //最后设置
            heroInfo.InitHp();
            //计算战力
            UpdateBattlePower(heroInfo);
            Log.WarnLine("player {0} hero {1} InitHeroNatureInfo ———————————————————— end", owner.Uid, heroInfo.Id);
        }


        public Dictionary<NatureType, long> Nature4To9(HeroInfo heroInfo, Dictionary<NatureType, long> nature)
        {
            if (nature == null) return new Dictionary<NatureType, long>();

            float gC = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());
            Dictionary<NatureType, long> nature9 = Nature4To9(heroInfo.AwakenLevel, nature);

            return nature9;
        }

        public static Dictionary<NatureType, long> Nature4To9(int level, Dictionary<NatureType, long> nature)
        {
            if (nature == null) return new Dictionary<NatureType, long>();

            float gC = GetGroVal(level, false);
            Dictionary<NatureType, long> nature9 = new Dictionary<NatureType, long>(nature);

            foreach (var nature4 in NatureLibrary.GetNature4To9List())
            {
                nature9.Remove(nature4.Key);
                if (nature.ContainsKey(nature4.Key))
                {
                    foreach (var kv in nature4.Value)
                    {
                        long newValue = (long)(kv.Value * gC * nature[nature4.Key]);
                        nature9.AddValue(kv.Key, newValue);
                    }
                }
            }

            return nature9;
        }

        #region Parse

        #region Dictionary Parse To String
        /// <summary>
        /// Dictionary Parse To String
        /// </summary>
        /// <param name="parameters">Dictionary</param>
        /// <returns>String</returns>
        public string ParseToString<T>(IDictionary<NatureType, T> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return "";
            }
            IDictionary<NatureType, T> sortedParams = new SortedDictionary<NatureType, T>(parameters);
            IEnumerator<KeyValuePair<NatureType, T>> dem = sortedParams.GetEnumerator();

            StringBuilder query = new StringBuilder("");
            while (dem.MoveNext())
            {
                NatureType key = dem.Current.Key;
                T value = dem.Current.Value;
                //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            string content = query.ToString().Substring(0, query.Length - 1);

            return content;
        }
        public string ParseToString<T>(IDictionary<int, T> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return "";
            }
            IDictionary<int, T> sortedParams = new SortedDictionary<int, T>(parameters);
            IEnumerator<KeyValuePair<int, T>> dem = sortedParams.GetEnumerator();

            StringBuilder query = new StringBuilder("");
            while (dem.MoveNext())
            {
                int key = dem.Current.Key;
                T value = dem.Current.Value;
                //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            string content = query.ToString().Substring(0, query.Length - 1);

            return content;
        }
        public string ParseToString(IDictionary<int, List<string>> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return "";
            }
            IDictionary<int, List<string>> sortedParams = new SortedDictionary<int, List<string>>(parameters);
            IEnumerator<KeyValuePair<int, List<string>>> dem = sortedParams.GetEnumerator();

            StringBuilder query = new StringBuilder("");
            while (dem.MoveNext())
            {
                int key = dem.Current.Key;
                List<string> value = dem.Current.Value;
                //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            string content = query.ToString().Substring(0, query.Length - 1);

            return content;
        }
        //public string ParseToString(IDictionary<NatureType, float> parameters)
        //{
        //    IDictionary<NatureType, float> sortedParams = new SortedDictionary<NatureType, float>(parameters);
        //    IEnumerator<KeyValuePair<NatureType, float>> dem = sortedParams.GetEnumerator();

        //    StringBuilder query = new StringBuilder("");
        //    while (dem.MoveNext())
        //    {
        //        NatureType key = dem.Current.Key;
        //        float value = dem.Current.Value;
        //        //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        //        {
        //            query.Append(key).Append("=").Append(value).Append("&");
        //        }
        //    }
        //    string content = query.ToString().Substring(0, query.Length - 1);

        //    return content;
        //}
        //public string ParseToString(IDictionary<int, Int64> parameters)
        //{
        //    IDictionary<int, Int64> sortedParams = new SortedDictionary<int, Int64>(parameters);
        //    IEnumerator<KeyValuePair<int, Int64>> dem = sortedParams.GetEnumerator();

        //    StringBuilder query = new StringBuilder("");
        //    while (dem.MoveNext())
        //    {
        //        int key = dem.Current.Key;
        //        Int64 value = dem.Current.Value;
        //        //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        //        {
        //            query.Append(key).Append("=").Append(value).Append("&");
        //        }
        //    }
        //    string content = query.ToString().Substring(0, query.Length - 1);

        //    return content;
        //}
        //public string ParseToString(IDictionary<int, int> parameters)
        //{
        //    IDictionary<int, int> sortedParams = new SortedDictionary<int, int>(parameters);
        //    IEnumerator<KeyValuePair<int, int>> dem = sortedParams.GetEnumerator();

        //    StringBuilder query = new StringBuilder("");
        //    while (dem.MoveNext())
        //    {
        //        int key = dem.Current.Key;
        //        int value = dem.Current.Value;
        //        //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        //        {
        //            query.Append(key).Append("=").Append(value).Append("&");
        //        }
        //    }
        //    string content = query.ToString().Substring(0, query.Length - 1);

        //    return content;
        //}
        //public string ParseToString(IDictionary<int, float> parameters)
        //{
        //    IDictionary<int, float> sortedParams = new SortedDictionary<int, float>(parameters);
        //    IEnumerator<KeyValuePair<int, float>> dem = sortedParams.GetEnumerator();

        //    StringBuilder query = new StringBuilder("");
        //    while (dem.MoveNext())
        //    {
        //        int key = dem.Current.Key;
        //        float value = dem.Current.Value;
        //        //if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        //        {
        //            query.Append(key).Append("=").Append(value).Append("&");
        //        }
        //    }
        //    string content = query.ToString().Substring(0, query.Length - 1);

        //    return content;
        //}
        #endregion

        #region String Parse To Dictionary
        /// <summary>
        /// String Parse To Dictionary
        /// </summary>
        /// <param name="parameter">String</param>
        /// <returns>Dictionary</returns>
        static public Dictionary<string, string> ParseToDictionary(string parameter)
        {
            try
            {
                String[] dataArry = parameter.Split('&');
                Dictionary<string, string> dataDic = new Dictionary<string, string>();
                for (int i = 0; i <= dataArry.Length - 1; i++)
                {
                    String dataParm = dataArry[i];
                    int dIndex = dataParm.IndexOf("=");
                    if (dIndex != -1)
                    {
                        String key = dataParm.Substring(0, dIndex);
                        String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                        dataDic.Add(key, value);
                    }
                }

                return dataDic;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #endregion

        public static float GetGroVal(int awakenLevel, bool isPlayer)
        {
            float groC = 0;
            //成长值
            GroValFactorModel groValFactorModel = NatureLibrary.GetGroValFactorModel(awakenLevel);
            if (groValFactorModel != null)
            {
                if (isPlayer)
                {
                    //成长值
                    groC = groValFactorModel.PlayerAwakenC;
                }
                else
                {
                    groC = groValFactorModel.AwakenC;
                }
            }
            return groC;
        }

        private List<CampStarModel> GetCampStarsList()
        {
            int[] starsLevelArray = new int[4];
            starsLevelArray[0] = owner.DragonLevel;
            starsLevelArray[1] = owner.TigerLevel;
            starsLevelArray[2] = owner.PhoenixLevel;
            starsLevelArray[3] = owner.TortoiseLevel;
            CampStarModel dragonModel = CampStarsLibrary.GetDragonModel(starsLevelArray[0]);
            if (dragonModel == null || dragonModel.AttrList == null)
            {
                Logger.Log.WarnLine("dragonModel is null or attrList is null, dragon level is {0}", starsLevelArray[0]);
                return null;
            }
            CampStarModel tigerModel = CampStarsLibrary.GetTigerModel(starsLevelArray[1]);
            if (tigerModel == null || tigerModel.AttrList == null)
            {
                Logger.Log.WarnLine("tigerModel is null or attrList is null, tiger level is {0}", starsLevelArray[1]);
                return null;
            }
            CampStarModel phoenixModel = CampStarsLibrary.GetPhoenixModel(starsLevelArray[2]);
            if (phoenixModel == null || phoenixModel.AttrList == null)
            {
                Logger.Log.WarnLine("phoenixModel is null or attrList is null, phoenix level is {0}", starsLevelArray[2]);
                return null;
            }
            CampStarModel tortoiseMedel = CampStarsLibrary.GetTortoiseModel(starsLevelArray[3]);
            if (tortoiseMedel == null || tortoiseMedel.AttrList == null)
            {
                Logger.Log.WarnLine("tortoiseMedel is null or attrList is null, tortoise level is {0}", starsLevelArray[3]);
                return null;
            }
            List<CampStarModel> campStarsList = new List<CampStarModel>();
            campStarsList.Add(dragonModel);
            campStarsList.Add(tigerModel);
            campStarsList.Add(phoenixModel);
            campStarsList.Add(tortoiseMedel);
            return campStarsList;
        }

        //武魂升级
        public void HeroLevelUpUpdateNature(HeroInfo heroInfo)
        {
            int newHeroLevel = heroInfo.Level;

            NatureDataModel oldLevelModel = NatureLibrary.GetBasicNatureIncrModel(newHeroLevel - 1);
            NatureDataModel newLevelModel = NatureLibrary.GetBasicNatureIncrModel(newHeroLevel);
            if (newLevelModel == null || oldLevelModel == null)
            {
                Log.Warn("not find data in xml BasicNatureIncreasement, old level is {0}, new level is {1}", newHeroLevel - 1, newHeroLevel);
                return;
            }
            NatureDataModel tempBasicNatureModel = NatureLibrary.GetHeroBasicNatureModel(heroInfo.Id);
            NatureDataModel tempBasicAddedNatureModel = NatureLibrary.GetHeroBasicAddedNatureModel(heroInfo.Id);

            Dictionary<NatureType, Int64> oldNatures = GetBasicNatureList(tempBasicNatureModel, oldLevelModel, tempBasicAddedNatureModel);
            Dictionary<NatureType, Int64> newNatures = GetBasicNatureList(tempBasicNatureModel, newLevelModel, tempBasicAddedNatureModel);
            heroInfo.HeroLevelUpUpdateNature(newNatures, oldNatures);

            UpdateBattlePower(heroInfo.Id);
            NotifyClientBattlePowerFrom(heroInfo.Id);
        }


        public Dictionary<NatureType, Int64> GetBasicNatureList(NatureDataModel basicModel, NatureDataModel incrModel, NatureDataModel addedModel)
        {
            Dictionary<NatureType, Int64> NatureList = new Dictionary<NatureType, Int64>();

            Dictionary<NatureType, double> tempList = new Dictionary<NatureType, double>();
            if (basicModel != null)
            {
                foreach (var basic in basicModel.NatureList)
                {
                    tempList[basic.Key] = basic.Value;
                }
            }

            if (incrModel != null)
            {
                foreach (var incr in incrModel.NatureList)
                {
                    double value;
                    tempList.TryGetValue(incr.Key, out value);
                    tempList[incr.Key] = incr.Value * value;
                }
            }

            if (addedModel != null)
            {
                foreach (var added in addedModel.NatureList)
                {
                    double value;
                    tempList.TryGetValue(added.Key, out value);
                    tempList[added.Key] = added.Value + value;
                }
            }

            if (tempList.Count > 0)
            {
                foreach (var temp in tempList)
                {
                    NatureList[temp.Key] = (Int64)temp.Value;
                }
            }

            return NatureList;
        }


        //称号升级
        public void HeroTitleUpUpdateNature(HeroInfo info, int oldLevel)
        {
            HeroTitleModel oldTitle = HeroLibrary.GetHeroTitle(oldLevel);
            if (oldTitle != null)
            {
                //增加属性
                if (oldTitle.BaseNature.Count > 0)
                {
                    foreach (var item in oldTitle.BaseNature)
                    {
                        info.AddNatureBaseValue((NatureType)item.Key, -item.Value);
                    }
                }
            }

            HeroTitleModel title = HeroLibrary.GetHeroTitle(info.TitleLevel);
            if (title != null)
            {
                //增加属性
                if (title.BaseNature.Count > 0)
                {
                    foreach (var item in title.BaseNature)
                    {
                        info.AddNatureBaseValue((NatureType)item.Key, item.Value);
                    }
                }
            }

            UpdateBattlePower(info.Id);
            NotifyClientBattlePowerFrom(info.Id);
        }

        //private List<Dictionary<NatureType, long>> GetSoulRingNature(HeroInfo heroInfo)
        //{
        //    Dictionary<int, SoulRingItem> soulRingDic = owner.SoulRingManager.GetAllEquipedSoulRings(heroInfo.Id);
        //    if (soulRingDic == null)
        //    {
        //        //Log.Write("hero {0} does not equip soulRing", heroInfo.Id);
        //        return null;
        //    }

        //    List<Dictionary<NatureType, long>> ringNatureList = new List<Dictionary<NatureType, long>>();
        //    foreach (var kv in soulRingDic)
        //    {
        //        Dictionary<NatureType, long> mainAttrs = kv.Value.GetMainAttrs();
        //        Dictionary<NatureType, long> ulAttrs = kv.Value.GetUltAttrs();

        //        ringNatureList.Add(mainAttrs);
        //    }
        //    return ringNatureList;
        //}

        ////穿戴(替换)魂骨
        //public void EquipSoulBoneNature(int heroId, Dictionary<NatureType, int> oldBoneNartures, Dictionary<NatureType, int> newBoneNatures)
        //{
        //    HeroInfo heroInfo = GetHeroInfo(heroId);

        //    if (oldBoneNartures != null)
        //    {
        //        foreach (var kv in oldBoneNartures)
        //        {
        //            heroInfo.AddNatureAddedValue(kv.Key, kv.Value * -1);
        //        }
        //    }

        //    foreach (var kv in newBoneNatures)
        //    {
        //        heroInfo.AddNatureAddedValue(kv.Key, kv.Value);
        //    }
        //    UpdateBattlePower(heroId);
        //    NotifyClientBattlePowerFrom(heroId);
        //}

        //public void EquipSoulBoneNature(int heroId, List<SoulBone> oldBoneList, List<SoulBone> newBoneList)
        //{
        //    HeroInfo heroInfo = GetHeroInfo(heroId);

        //    if (oldBoneList != null)
        //    {
        //        foreach (var bone in oldBoneList)
        //        {
        //            heroInfo.ReduceBoneNatureEffect(bone);
        //        }
        //    }

        //    foreach (var bone in newBoneList)
        //    {
        //        heroInfo.AddBoneNatureEffect(bone);
        //    }
        //    UpdateBattlePower(heroId);
        //    NotifyClientBattlePowerFrom(heroId);
        //}

        //public void EquipSoulBoneNature(int heroId, Dictionary<NatureType, int> oldBoneNartures, Dictionary<NatureType, int> newBoneNatures, List<SoulBone> oldBoneList, List<SoulBone> newBoneList)
        //{
        //    HeroInfo heroInfo = GetHeroInfo(heroId);
        //    if (heroInfo != null)
        //    {

        //        float gC = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());
        //        Dictionary<NatureType, Int64> oldNatures = GetAwakenNature9List(heroInfo, gC);
        //        if (oldBoneNartures != null)
        //        {
        //            foreach (var kv in oldBoneNartures)
        //            {
        //                heroInfo.AddNatureAddedValue(kv.Key, kv.Value * -1);
        //            }
        //        }
        //        if (newBoneNatures != null)
        //        {
        //            foreach (var kv in newBoneNatures)
        //            {
        //                heroInfo.AddNatureAddedValue(kv.Key, kv.Value);
        //            }
        //        }

        //        if (oldBoneList != null)
        //        {
        //            foreach (var bone in oldBoneList)
        //            {
        //                heroInfo.ReduceBoneNatureEffect(bone);
        //            }
        //        }
        //        if (newBoneList != null)
        //        {
        //            foreach (var bone in newBoneList)
        //            {
        //                heroInfo.AddBoneNatureEffect(bone);
        //            }
        //        }

        //        Dictionary<NatureType, Int64> newNatures = GetAwakenNature9List(heroInfo, gC);


        //        Dictionary<NatureType, Int64> changeNature = GetChangeNature9List(oldNatures, newNatures);
        //        foreach (var nature in changeNature)
        //        {
        //            heroInfo.AddNatureAddedValue(nature.Key, nature.Value);
        //        }

        //        UpdateBattlePower(heroId);
        //        NotifyClientBattlePowerFrom(heroId);
        //    }

        //}

        //public void SoulSkillEnhance(HeroInfo heroInfo)
        //{

        //}


        //魂环突破(强化)
        public void SoulRingEnhance(int heroId, Dictionary<NatureType, long> beforeEnhanceDic, Dictionary<NatureType, long> afterEnhanceDic)
        {
            HeroInfo heroInfo = GetHeroInfo(heroId);
            if (beforeEnhanceDic != null)
            {
                foreach (var kv in beforeEnhanceDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.ReduceRingNatureEffect(kv);
                    }
                }
            }
            if (afterEnhanceDic != null)
            {
                foreach (var kv in afterEnhanceDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.AddRingNatureEffect(kv);
                    }
                }
            }
        }

        //魂环添加(属性添加)
        public void SoulRingAdd(HeroInfo heroInfo, Dictionary<NatureType, long> newNatureDic)
        {
            if (newNatureDic == null)
            {
                return;
            }

            foreach (var kv in newNatureDic)
            {
                if (kv.Value != 0)
                {
                    heroInfo.AddRingNatureEffect(kv);
                }
            }
        }


        //魂环替换
        public void InjectionNatureSwap(HeroInfo heroInfo, Dictionary<NatureType, long> newNatureDic, Dictionary<NatureType, long> oldNatureDic)
        {
            if (oldNatureDic != null)
            {
                foreach (var kv in oldNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.ReduceRingNatureEffect(kv);
                    }
                }
            }

            if (newNatureDic != null)
            {
                foreach (var kv in newNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.AddRingNatureEffect(kv);
                    }
                }
            }
        }


        //魂环替换
        public void SoulRingSwap(HeroInfo heroInfo, Dictionary<NatureType, long> newNatureDic, Dictionary<NatureType, long> oldNatureDic)
        {
            if (oldNatureDic != null)
            {
                foreach (var kv in oldNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.ReduceRingNatureEffect(kv);
                    }
                }
            }

            if (newNatureDic != null)
            {
                foreach (var kv in newNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.AddRingNatureEffect(kv);
                    }
                }
            }
            //UpdateBattlePower(heroInfo.Id);
        }

        //装备属性更新
        public void UpdateEquipmentNature(HeroInfo heroInfo, Dictionary<NatureType, long> newNatureDic, Dictionary<NatureType, long> oldNatureDic)
        {
            if (oldNatureDic != null)
            {
                foreach (var kv in oldNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.AddNatureAddedValue(kv.Key, -kv.Value);
                    }
                }
            }

            if (newNatureDic != null)
            {
                foreach (var kv in newNatureDic)
                {
                    if (kv.Value != 0)
                    {
                        heroInfo.AddNatureAddedValue(kv.Key, kv.Value);
                    }
                }
            }
        }

        //点天赋/重置天赋
        public void HeroClickTalent(HeroInfo heroInfo, Dictionary<NatureType, int> oldTalents, Dictionary<NatureType, int> newTalents)
        {
            float groVal = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());

            if (oldTalents != null)
            {
                foreach (var kv in oldTalents)
                {
                    //heroInfo.ReduceTalentsNatureEffect(kv);
                    NatureType natureType = kv.Key;
                    int value = kv.Value * -1;

                    heroInfo.AddNatureBaseValue(kv.Key, value);
                    UpdateNature4to9(heroInfo, kv.Key, value, groVal);
                }
            }
            if (newTalents != null)
            {
                foreach (var kv in newTalents)
                {
                    //heroInfo.AddTalentsNatureEffect(kv);
                    NatureType natureType = kv.Key;
                    int value = kv.Value;
                    heroInfo.AddNatureBaseValue(kv.Key, kv.Value);
                    UpdateNature4to9(heroInfo, kv.Key, value, groVal);
                }
            }

            UpdateBattlePower(heroInfo.Id);
            NotifyClientBattlePowerFrom(heroInfo.Id);
        }
        //private Dictionary<NatureType, Int64> GetHeroTalents(HeroInfo heroInfo)
        //{
        //    Dictionary<NatureType, Int64> heroTalents = new Dictionary<NatureType, Int64>();
        //    foreach (var item in NatureLibrary.Basic4Nature)
        //    {
        //        heroTalents.Add(item.Key, heroInfo.GetNatureValue(item.Key));
        //    }
        //    //heroTalents.Add(NatureType.PRO_POW, heroInfo.TalentMng.PhysicalNum);
        //    //heroTalents.Add(NatureType.PRO_CON, heroInfo.TalentMng.PhysicalNum);
        //    //heroTalents.Add(NatureType.PRO_AGI, heroInfo.TalentMng.AgilityNum);
        //    //heroTalents.Add(NatureType.PRO_EXP, heroInfo.TalentMng.OutburstNum);
        //    return heroTalents;
        //}


        //天赋属性转基础属性
        private void UpdateNature4to9(HeroInfo heroInfo, NatureType type, int value, float groVal)//天赋点数
        {
            Dictionary<NatureType, float> Lsit = NatureLibrary.GetNature9List(type);
            if (Lsit != null)
            {
                foreach (var nature9 in Lsit)
                {
                    int addValue = (int)(nature9.Value * groVal * value);
                    heroInfo.AddNatureAddedValue(nature9.Key, addValue);
                }
            }
        }

        //阵营养成升级
        public void CampStarLevelUp(Dictionary<int, int> oldAttrList, Dictionary<int, int> newAttrList)
        {
            foreach (var heroInfo in heroInfoList)
            {
                HeroInfo info = GetHeroInfo(heroInfo.Key);
                if (info != null)
                {
                    //if (oldAttrList != null)
                    //{
                    //    foreach (var kv in oldAttrList)
                    //    {
                    //        info.ReduceCampStarNatureEffect(kv);
                    //    }
                    //}

                    //foreach (var kv in newAttrList)
                    //{
                    //    info.AddCampStarNatureEffect(kv);
                    //}
                    InitHeroNatureInfo(info);
                    NotifyClientBattlePowerFrom(info.Id);
                }
            }
        }

        //选择阵营属性加成
        public void AddCampStarsNatures(CampType camp)
        {
            if (camp == CampType.None)
            {
                return;
            }

            if (!owner.CheckLimitOpen(LimitType.CampStars))
            {
                return;
            }

            CampStarModel dragonModel = CampStarsLibrary.GetDragonModel(0);
            CampStarModel tigerModel = CampStarsLibrary.GetTigerModel(0);
            CampStarModel phoenixModel = CampStarsLibrary.GetPhoenixModel(0);
            CampStarModel tortoiseModel = CampStarsLibrary.GetTortoiseModel(0);

            foreach (var heroInfo in heroInfoList)
            {
                HeroInfo info = GetHeroInfo(heroInfo.Key);
                foreach (var kv in dragonModel.AttrList)
                {
                    info.AddCampStarNatureEffect(kv);
                }
                foreach (var kv in tigerModel.AttrList)
                {
                    info.AddCampStarNatureEffect(kv);
                }
                foreach (var kv in phoenixModel.AttrList)
                {
                    info.AddCampStarNatureEffect(kv);
                }
                foreach (var kv in tortoiseModel.AttrList)
                {
                    info.AddCampStarNatureEffect(kv);
                }
                UpdateBattlePower(info.Id);
            }
            NotifyClientBattlePower();
        }

        /// <summary>
        /// 出战伙伴职业限定
        /// </summary>
        public bool CheckJobPermission(Dictionary<int, int> job2Count)
        {
            foreach (var kv in job2Count)
            {
                if (GetJobCount(kv.Key) < kv.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetJobCount(int job)
        {
            return heroInfoList.Values.Where(x => x.GetData().GetInt("Job") == job).Count();
        }

        public int GetFirstHeroJob()
        {

            return heroInfoList[heroPos.Where(kv => kv.Value == heroPos.Values.Min()).First().Key].GetData().GetInt("Job");
        }

        public int GetSoulSkillCountByLevel(int level)
        {
            int count = 0;
            foreach (var item in heroInfoList)
            {
                if (item.Value.SoulSkillLevel >= level)
                {
                    count++;
                }
            }
            return count;
        }


        #region 羁绊

        private void AddNatureRatio(HeroComboModel model)
        {
            if (model != null)
            {
                foreach (var item in model.NatureRatio)
                {
                    if (NatureRatio.ContainsKey(item.Key))
                    {
                        NatureRatio[item.Key] += item.Value;
                    }
                    else
                    {
                        NatureRatio[item.Key] = item.Value;
                    }
                }
            }
            else
            {
                Logger.Log.Warn("player {0} init ComboManager error: not find combo {1}", owner.Uid, model.Id);
            }
        }

        public void AddCombo(int comboId)
        {
            HeroComboModel model = DrawLibrary.GetHeroComboModel(comboId);
            if (model != null)
            {
                int oldId;
                if (ComboGroupList.TryGetValue(model.Group, out oldId))
                {
                    HeroComboList.Remove(oldId);
                }
                ComboGroupList[model.Group] = comboId;
                HeroComboList.Add(comboId);
                AddNatureRatio(model);
            }
        }

        public void GetAllCombo(int comboId, Dictionary<int, int> dic)
        {
            HeroComboModel model = DrawLibrary.GetHeroComboModel(comboId);
            if (model != null)
            {
                List<int> groupList = DrawLibrary.GetComboGroupList(model.Group);
                if (groupList != null)
                {
                    foreach (var item in groupList)
                    {
                        dic[comboId] = 0;
                        if (item == comboId)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public bool CheckCanCombo(HeroComboModel model)
        {
            if (HeroComboList.Contains(model.Id))
            {
                return false;
            }
            List<int> list = DrawLibrary.GetComboGroupList(model.Group);
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (HeroComboList.Contains(item))
                    {
                        if (item >= model.Id)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return true;
        }

        public void AddComboNature(Dictionary<int, int> natureRatio)
        {
            foreach (var hero in heroInfoList)
            {
                foreach (var ratio in natureRatio)
                {
                    hero.Value.AddNatureRatio((NatureType)ratio.Key, ratio.Value);
                }
            }
        }

        public string GetComboList()
        {
            string combo = string.Empty;

            foreach (var item in HeroComboList)
            {
                combo += string.Format("{0}|", item);
            }
            return combo;
        }
        #endregion

        public void HeroAddHiddenWeaponNature(HeroInfo heroInfo)
        {
            ulong weaponId = owner.HiddenWeaponManager.GetHeroEquipWeaponId(heroInfo.Id);
            if (weaponId == 0) return;

            HiddenWeaponItem weaponItem = owner.BagManager.HiddenWeaponBag.GetItem(weaponId) as HiddenWeaponItem;
            if (weaponItem == null) return;

            Dictionary<NatureType, long> weaponNature = new Dictionary<NatureType, long>(weaponItem.Model.BaseNatureDic);

            if (weaponItem.Info.Level > 0)
            {
                HiddenWeaponUpgradeModel upgradeModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(weaponItem.Model.UpgradePool, weaponItem.Info.Level);
                if (upgradeModel != null)
                {
                    weaponNature.AddValue(upgradeModel.UpgradeAddNature);
                }
            }

            if (weaponItem.Info.WashList.Count > 0)
            {
                foreach (var id in weaponItem.Info.WashList)
                {
                    HiddenWeaponWashModel washModel = HiddenWeaponLibrary.GetHiddenWeaponWashModel(id);
                    if (washModel == null) continue;

                    weaponNature.AddValue(washModel.NatureType, washModel.NatureValue);
                }
            }

            weaponNature = Nature4To9(heroInfo, weaponNature);

            weaponNature.ForEach(x => heroInfo.AddNatureAddedValue(x.Key, x.Value));

//#if DEBUG
//            Log.Debug($"------------------------------------------------------ {heroInfo.Id} weapon add nature 1");
//            Log.Info1(JsonConvert.SerializeObject(weaponNature));
//            Log.Debug($"------------------------------------------------------ {heroInfo.Id} weapon add nature 2");
//#endif
        }

        public void SwapTalent(HeroInfo fromHero, HeroInfo toHero)
        {
            TalentManager temp = new TalentManager(toHero.TalentMng.TotalNum, toHero.TalentMng.StrengthNum, toHero.TalentMng.PhysicalNum, toHero.TalentMng.AgilityNum, toHero.TalentMng.OutburstNum);
            toHero.TalentMng.Inherit(fromHero.TalentMng);
            fromHero.TalentMng.Inherit(temp);
        }

        //传承天赋
        public void HeroInheritTalent(HeroInfo heroInfo, Dictionary<NatureType, int> oldTalents, Dictionary<NatureType, int> newTalents)
        {
            float groVal = GetGroVal(heroInfo.AwakenLevel, heroInfo.IsPlayer());

            if (oldTalents != null)
            {
                foreach (var kv in oldTalents)
                {
                    //heroInfo.ReduceTalentsNatureEffect(kv);
                    NatureType natureType = kv.Key;
                    int value = kv.Value * -1;

                    heroInfo.AddNatureBaseValue(kv.Key, value);
                    UpdateNature4to9(heroInfo, kv.Key, value, groVal);
                }
            }
            if (newTalents != null)
            {
                foreach (var kv in newTalents)
                {
                    //heroInfo.AddTalentsNatureEffect(kv);
                    NatureType natureType = kv.Key;
                    int value = kv.Value;
                    heroInfo.AddNatureBaseValue(kv.Key, kv.Value);
                    UpdateNature4to9(heroInfo, kv.Key, value, groVal);
                }
            }

            //UpdateBattlePower(heroInfo.Id);
            //NotifyClientBattlePowerFrom(heroInfo.Id);
        }

    }
}

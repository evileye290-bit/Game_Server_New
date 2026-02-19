using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public CampBattleManager CampBattleMng;

        public void InitCampBattleManager()
        {
            CampBattleMng = new CampBattleManager(this);
        }

        public void LoadCampBattleInfo(QueryLoadCampInfo queryLoadCampBattleInfo)
        {
            CampBattleMng.Init(queryLoadCampBattleInfo);

        }

        public void LoadCampBattleInfo()
        {
            //FIXME:这里主要是为了刷新离线缓存数据
            CampBattleMng.LoadCampScoreFromRedis();
        }


        internal void OpenCampBox()
        {
            //开宝箱逻辑
            MSG_ZGC_OPEN_CAMP_BOX msg = new MSG_ZGC_OPEN_CAMP_BOX();
            msg.Result = (int)ErrorCode.Success;
            if (CampBoxLeftCount <= 0)
            {
                Log.Warn($"player {Uid} open camp box failed: count not enough");
                msg.Result = (int)ErrorCode.Fail;
                return;
            }
            //int count = GetCounter(CounterType.CampBoxCount).Count;
            RewardManager reward = new RewardManager();
            string rewardStr = CampBattleLibrary.GetCampBoxReward(server.RelationServer.CampBattlePhaseInfo.PhaseNum);
            string reStr = rewardStr;
            for (int i = 1; i < CampBoxLeftCount; i++)
            {
                reStr = string.Format($"{reStr}|{rewardStr}");
            }
            reward.InitSimpleReward(reStr);
            AddRewards(reward, ObtainWay.CampBox);
            UpdateCounter(CounterType.CampBoxCount, CampBoxLeftCount);
            reward.GenerateRewardMsg(msg.Rewards);
            CampBoxLeftCount = 0;
            msg.Count = CampBoxLeftCount;
            Write(msg);
        }

        //internal void CheckInBattleRank()
        //{
        //    CampBattleMng.IsInBattleRank = false;
        //    OperateGetCampRankList op = new OperateGetCampRankList(MainId,(int)Camp, RankType.CampBattleFight);
        //    server.Redis.Call(op, ret =>   {
        //        Dictionary<int, RankBaseModel> uidRankInfoDic = op.uidRank;
        //        if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
        //        {
        //            Log.Warn($"load rank list fail ,can not find data in redis");
        //        }
        //        uidRankInfoDic = uidRankInfoDic.OrderByDescending(v => v.Value.Score).ThenBy(v => v.Value.Time).ToDictionary(o => o.Key, p => p.Value);
        //        int length = uidRankInfoDic.Count;
        //        int i = 0;
        //        MSG_ZGC_CHECK_IN_BATTLE_RANK msg = new MSG_ZGC_CHECK_IN_BATTLE_RANK();
        //        msg.InRank = false;

        //        foreach (var item in uidRankInfoDic)
        //        {
        //            i++;
        //            if (uid == item.Value.Uid)
        //            {
        //                if (i<= RankLibrary.GetConfig(RankType.CampBattleFight).ShowCount)
        //                {
        //                    msg.InRank = true;
        //                    CampBattleMng.IsInBattleRank = true;
        //                }
        //            }
        //        }

        //        Write(msg);
        //    });
        //}

        public void GetCampBattleInfo()
        {
            MSG_ZR_GET_CAMPBATTLE_INFO request = new MSG_ZR_GET_CAMPBATTLE_INFO();
            server.SendToRelation(request, uid);
        }

        public void GetFortInfo(int fortId)
        {
            MSG_ZR_GET_FORT_INFO request = new MSG_ZR_GET_FORT_INFO();
            request.FortId = fortId;
            server.SendToRelation(request, uid);
        }

        public void GetCampBattleRankList(int type, int page, int camp)
        {
            MSG_ZR_GET_CAMPBATTLE_RANK_LIST request = new MSG_ZR_GET_CAMPBATTLE_RANK_LIST();
            request.RankType = type;
            request.Page = page;
            request.Camp = camp;
            server.SendToRelation(request, uid);
        }

        public void CreateCampDungeon(int fortId, int dungeonId)
        {

            //fortId,dungeonId 正确性的检测
            //if (!CampActivityLibrary.CheckFortAndDungeon(fortId, dungeonId))
            //{
            //    Log.Warn($"player {uid} create camp fort {fortId} dungeon {dungeonId} fail, wrong fort or dungeon !");
            //    return;
            //}
            int realDungeonId = 0;
            if (fortId == 0)
            {
                realDungeonId = dungeonId;
            }
            else
            {
                CampFortData campFortData;
                if (!CampActivityLibrary.CampFortLayout.TryGetValue(fortId, out campFortData))
                {
                    return;
                }
                if (dungeonId == 0)
                {
                    realDungeonId = campFortData.BossDungeonId;
                }
                else
                {
                    realDungeonId = campFortData.DefenderDungeonId;
                }
            }


            MapModel mapModel = MapLibrary.GetMap(realDungeonId);
            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(realDungeonId);
            if (mapModel == null || dungeonModel == null)
            {
                return;
            }

            CampBattleStep step = GetCampBattleStep();

            var expend = CampBattleLibrary.GetCampBattleExpend();

            MSG_ZGC_CAMP_CREATE_DUNGEON response = new MSG_ZGC_CAMP_CREATE_DUNGEON();
            response.DungeonId = dungeonId;
            response.FortId = fortId;
            //if (step == CampBattleStep.Final && !CampBattleMng.IsInBattleRank)
            //{
            //    response.Result = (int)ErrorCode.NotInRank;
            //    Write(response);
            //    return;
            //}
            if (step == CampBattleStep.Rest)
            {
                Log.Warn($"player {Uid} create camp dungeon fort {fortId} dungeon {dungeonId} failed: camp battle not open");
                response.Result = (int)ErrorCode.CampBattlNotOpen;
                Write(response);
                return;
            }
            response.Result = (int)ErrorCode.Success;

            //行动力检测            //粮草检查
            switch ((MapType)dungeonModel.Type)
            {
                case MapType.CampBattle:
                case MapType.CampDefense:
                    if (GetCounterValue(CounterType.ActionCount) < expend.StrongPoint.Item1)
                    {
                        response.Result = (int)ErrorCode.ActionCountNotEnough;
                        Log.Warn($"player {uid} create camp fort {fortId} dungeon {dungeonId} fail, action count not enough!");
                        break;
                    }

                    if (server.RelationServer.campCoins[Camp] < expend.NeutralityPoint.Item1)
                    {
                        response.Result = (int)ErrorCode.GrianNotEnough;
                        Log.Warn($"player {uid} create camp fort {fortId} dungeon {dungeonId} fail, grain count not enough!");
                        break;
                    }

                    break;
                case MapType.CampBattleNeutral:
                    if (GetCounterValue(CounterType.ActionCount) < expend.NeutralityPoint.Item1)
                    {
                        response.Result = (int)ErrorCode.ActionCountNotEnough;
                        Log.Warn($"player {uid} create camp fort {fortId} dungeon {dungeonId} fail, action count not enough!");
                        break;
                    }

                    if (server.RelationServer.campCoins[Camp] < expend.NeutralityPoint.Item1)
                    {
                        response.Result = (int)ErrorCode.GrianNotEnough;
                        Log.Warn($"player {uid} create camp fort {fortId} dungeon {dungeonId} fail, grain count not enough!");
                        break;
                    }
                    break;
                default:
                    return;
            }
            if (response.Result != (int)ErrorCode.Success)
            {
                Write(response);
            }

            MSG_ZR_CAMP_CREATE_DUNGEON msg = new MSG_ZR_CAMP_CREATE_DUNGEON()
            {
                Camp = (int)Camp,
                DungeonId = dungeonId,
                FortId = fortId
            };

            //扣粮草在relation上
            server.RelationServer.Write(msg, uid);

            //if(...)
            //{
            //    MSG_ZGC_CAMP_CREATE_DUNGEON response = new MSG_ZGC_CAMP_CREATE_DUNGEON();
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}
        }

        public PlayerCampFightInfo GetCampFightInfo()
        {
            PlayerCampFightInfo rankInfo = new PlayerCampFightInfo();
            rankInfo.Uid = Uid;
            rankInfo.IsRobot = true;
            rankInfo.Name = Name;
            rankInfo.Level = Level;
            rankInfo.Icon = Icon;
            //rankInfo.IconFrame = IconFrame;
            rankInfo.HeroId = HeroId;
            rankInfo.BattlePower = HeroMng.CalcBattlePower();

            rankInfo.NatureValues = NatureValues;
            rankInfo.NatureRatios = NatureRatios;
            //伙伴信息
            foreach (var item in HeroMng.GetHeroInfoList())
            {
                RobotHeroInfo robotHero = new RobotHeroInfo();
                robotHero.HeroId = item.Value.Id;
                robotHero.Level = item.Value.Level;
                robotHero.AwakenLevel = item.Value.AwakenLevel;
                robotHero.StepsLevel = item.Value.StepsLevel;
                robotHero.SoulSkillLevel = item.Value.SoulSkillLevel;
                robotHero.EquipIndex = item.Value.EquipIndex;
                robotHero.GodType = item.Value.GodType;
                robotHero.BattlePower = item.Value.GetBattlePower();
                robotHero.HeroPos = item.Value.DefensivePositionNum;

                foreach (var nature in item.Value.Nature.GetNatureList())
                {
                    robotHero.NatureList[nature.Key] = nature.Value.Value;
                }

                Dictionary<int, SoulRingItem> soulRing = SoulRingManager.GetAllEquipedSoulRings(item.Value.Id);
                if (soulRing != null)
                {
                    foreach (var curr in soulRing)
                    {
                        robotHero.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position,
                            curr.Value.Level, curr.Value.SpecId, curr.Value.Year, curr.Value.Element);
                    }
                }

                List<SoulBone> soulBoneList = SoulboneMng.GetEnhancedHeroBones(item.Value.Id);
                if (soulBoneList != null)
                {
                    //foreach (var curr in soulBoneList)
                    //{
                    //    robotHero.SoulBones += string.Format("{0}|", curr.TypeId);
                    //}

                    List<string> soulBoneStr = soulBoneList.ToList().ConvertAll(x =>
                    {
                        List<int> specList = x.GetSpecList();
                        return specList.Count <= 0 ? x.TypeId.ToString() : x.TypeId.ToString() + ":" + string.Join(":", specList);
                    });
                    robotHero.SoulBones = string.Join("|", soulBoneStr);
                }

                HiddenWeaponItem weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(item.Value.Id);
                if (weaponItem != null)
                {
                    robotHero.HiddenWeapon = $"{weaponItem.Id}:{weaponItem.Info.Star}";
                }

                List<EquipmentItem> equipmentItems = EquipmentManager.GetAllEquipedEquipments(item.Value.Id);
                robotHero.Equipment = string.Join("|", equipmentItems.Select(x => x.Id));

                rankInfo.AddHero(robotHero);
            }
            return rankInfo;
        }

        public void EnterCampMap(PlayerCampFightInfo fightInfo)
        {
            MSG_ZGC_CAMP_CREATE_DUNGEON response = new MSG_ZGC_CAMP_CREATE_DUNGEON();

            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(fightInfo.DungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} enter camp map request to create dungeon {fightInfo.DungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }
            MapField<int, float> addNatures = new MapField<int, float>();

            int count;
            Counter counter = GetCounter(CounterType.BuyCampBattleNattureCount);
            if (counter == null)
            {
                count = 0;
            }
            else
            {
                count = counter.Count;
            }
            //1 购买的属性
            CalcBuyAddNature(count, addNatures);
            //2 鼓舞属性
            CalcInspireAddNatures((CampType)fightInfo.InspireCamp, fightInfo.InspireDValue, addNatures);

            if (dungeon.GetMapType() == MapType.CampDefense)
            {
                (dungeon as CampBattleDefenseDungeon).SetPlayerNature(addNatures);
            }
            if (dungeon.GetMapType() == MapType.CampBattle)
            {
                (dungeon as CampBattleDungeon).SetPlayerNature(addNatures);
            }

            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();

            if (dungeon.GetMapType() == MapType.CampDefense)
            {
                CampBattleDefenseDungeon defenseDungeon = dungeon as CampBattleDefenseDungeon;
                defenseDungeon.SetFortId(fightInfo.FortId, fightInfo.DungeonIndex);

                List<RobotHeroInfo> heroInfos = new List<RobotHeroInfo>();

                Dictionary<int, Int64> heroHP = new Dictionary<int, Int64>();

                MapField<int, float> addNatures1 = new MapField<int, float>();
                foreach (var item in fightInfo.AddNature)
                {
                    addNatures1.Add(item.Key, item.Value * 0.01f);
                }

                CalcInspireAddNatures((CampType)fightInfo.InspireCamp, fightInfo.InspireDValue, addNatures1);

                foreach (var heroInfo in fightInfo.HeroInfos)
                {
                    foreach (var addNature in addNatures1)
                    {
                        NatureType type = (NatureType)addNature.Key;
                        long value;
                        if (heroInfo.NatureList.TryGetValue(type, out value))
                        {
                            heroInfo.NatureList[type] = (long)(value * (1 + addNature.Value));
                        }
                    }
                    heroInfo.PRO_HP = heroInfo.NatureList[NatureType.PRO_MAX_HP];
                    heroInfos.Add(heroInfo);
                    heroHP[heroInfo.HeroId] = heroInfo.NatureList[NatureType.PRO_MAX_HP];
                }

                //战力压制
                dungeon.SetBattlePowerSuppress( HeroMng.GetBattlePower(HeroMng.GetHeroPos()), fightInfo.GetBattlePower());

                defenseDungeon.AddDefenderRobotHero(fightInfo.HeroInfos, heroHP, -1, fightInfo.NatureValues, fightInfo.NatureRatios);
            }
            if (dungeon.GetMapType() == MapType.CampBattle)
            {
                CampBattleDungeon campDungeon = dungeon as CampBattleDungeon;
                campDungeon.SetFortId(fightInfo.FortId, fightInfo.DungeonIndex);
                //campDungeon.SetMonsterNature(msg.AddNature);
            }
            //第一次阵营战发称号卡
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.FirstCampBattle);
        }

        private void CalcBuyAddNature(int buyCount, MapField<int, float> addNatures)
        {
            if (buyCount == 0 || addNatures == null)
            {
                return;
            }
            //1 购买属性 //FIXME:这三个属性写死的，后续是否改为配置
            int deltaValue = CampBattleLibrary.GetAttributeOneIntensifyValue();
            float fValue = deltaValue * buyCount * 0.01f;

            addNatures.Add((int)NatureType.PRO_ATK, fValue);
            addNatures.Add((int)NatureType.PRO_DEF, fValue);
            addNatures.Add((int)NatureType.PRO_MAX_HP, fValue);

            return;
        }


        private void CalcInspireAddNatures(CampType inspireCamp, int inspireDValue, MapField<int, float> addNatures)
        {
            if (addNatures == null)
            {
                return;
            }

            if (inspireCamp == Camp)
            {
                var model = CampBattleLibrary.GetCampBattleAttributeIntenisfy(inspireDValue);
                if (model == null)
                {
                    return;
                }
                foreach (var item in model.natures)
                {
                    int key = (int)item.Key;
                    float value;
                    if (addNatures.TryGetValue(key, out value))
                    {
                        value = (1 + value) * (1 + item.Value) - 1;  //这里用乘法。没有为什么
                        addNatures[key] = value;
                    }
                    else
                    {
                        addNatures.Add(key, item.Value);
                    }
                }
            }
            return;
        }

        public Dictionary<int, Hero> GetAttackerHeros()
        {
            Dictionary<int, Hero> attackerHeroDic = new Dictionary<int, Hero>();

            foreach (var equip in HeroMng.GetHeroPos())
            {
                int heroId = equip.Key;
                HeroInfo info = HeroMng.GetHeroInfo(heroId);
                if (info == null)
                {
                    continue;
                }
                Hero hero = NewHero(server, this, info);
                hero.InitNatureExt(NatureValues, NatureRatios);
                hero.InitNature();
                attackerHeroDic.Add(heroId, hero);
            }
            return attackerHeroDic;
        }

        public Dictionary<int, Hero> GetDefensiveQueueHeros()
        {
            Dictionary<int, Hero> attackerHeroDic = new Dictionary<int, Hero>();
            foreach (var queue in HeroMng.DefensiveQueue)
            {
                foreach (var item in queue.Value)
                {
                    Hero hero = NewHero(server, this, item.Value);
                    hero.InitNatureExt(NatureValues, NatureRatios);
                    hero.InitNature();
                    attackerHeroDic.Add(hero.HeroId, hero);
                }
            }
            return attackerHeroDic;
        }

        /// <summary>
        /// 阵营防守副本
        /// </summary>
        /// <param name="dungeonId"></param>
        /// <param name="result"></param>
        /// <param name="monsterDamage"></param>
        /// <param name="attackerHeroDic"></param>
        /// <param name="defenderDamageDic"></param>
        public void NotifyReleationBattleResult(int fortId, int dungeonId, MapType type, DungeonResult result, long monsterDamage = 0, Dictionary<int, Int64> defenderDamageDic = null)
        {
            //扣行动力
            var expend = CampBattleLibrary.GetCampBattleExpend();
            if (result == DungeonResult.Success)
            {
                UpdateCounter(CounterType.ActionCount, -expend.StrongPoint.Item1);
            }
            else
            {
                server.RelationServer.AddGrain((int)Camp, expend.StrongPoint.Item2);
                return;
            }

            MSG_ZR_CAMP_DUNGEON_END msg = new MSG_ZR_CAMP_DUNGEON_END
            {
                FortId = fortId,
                DungeonId = dungeonId,
                Camp = (int)Camp,
                Result = (int)result,
                MonsterDamage = monsterDamage,
                AttackerInfo = GetPlayerSimpleInfo()
            };

            Dictionary<int, Hero> attackerHeroDic = GetDefensiveQueueHeros();

            if (attackerHeroDic.Count == 0)
            {
                attackerHeroDic = GetAttackerHeros();
            }

            foreach (var heroItem in attackerHeroDic)
            {
                HERO_INFO info = GetCampHeroInfoMsgData(heroItem.Value);
                msg.AttackerHeroList.Add(info);
            }

            defenderDamageDic?.ForEach(x => msg.DefenderDamageMap.Add(x.Key, x.Value));

            server.RelationServer.Write(msg, uid);
        }

        ///// <summary>
        ///// 阵营怪物副本
        ///// </summary>
        ///// <param name="dungeonId"></param>
        ///// <param name="result"></param>
        ///// <param name="monsterDamage"></param>
        ///// <param name="attackerHeroDic"></param>
        //public void NotifyReleationBattleResult(int fortId,int dungeonId, int result, long monsterDamage)
        //{
        //    MSG_ZR_CAMP_DUNGEON_END msg = new MSG_ZR_CAMP_DUNGEON_END
        //    {
        //        FortId = fortId,
        //        DungeonId = dungeonId,
        //        Camp = (int)Camp,
        //        Result = result,
        //        MonsterDamage = monsterDamage,
        //        AttackerInfo = GetPlayerSimpleInfo()
        //    };

        //    Dictionary<int, Hero> attackerHeroDic = GetDefensiveQueueHeros();

        //    foreach (var heroItem in attackerHeroDic)
        //    {
        //        HERO_INFO info =GetCampHeroInfoMsgData(heroItem.Value);
        //        info.HeroNature.Hp = info.HeroNature.MaxHp;
        //        info.HeroNature.MaxHp = info.HeroNature.MaxHp;
        //        msg.AttackerHeroList.Add(info);
        //    }

        //    //HERO_INFO playInfo = GetPlayerHeroInfoCampMsgData();
        //    //playInfo.HeroNature.Hp = playInfo.HeroNature.MaxHp;

        //    //msg.AttackerHeroList.Add(playInfo);

        //    server.RelationServer.Write(msg, uid);
        //}

        private ZR_Hero_Nature GetNature(Hero hero)
        {
            ZR_Hero_Nature heroNature = new ZR_Hero_Nature();
            foreach (var item in NatureLibrary.Basic4Nature)
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = hero.GetNatureValue(item.Key);
                heroNature.List.Add(info);
            }
            foreach (var item in NatureLibrary.Basic9Nature)
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = hero.GetNatureValue(item.Key);
                heroNature.List.Add(info);
            }
            heroNature.MaxHp = hero.GetNatureValue(NatureType.PRO_MAX_HP);
            heroNature.Hp = hero.GetNatureValue(NatureType.PRO_HP);

            return heroNature;
        }


        private ZR_Hero_Nature GetPlayerNature()
        {
            ZR_Hero_Nature heroNature = new ZR_Hero_Nature();
            foreach (var item in NatureLibrary.Basic4Nature)
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = GetNatureValue(item.Key);
                heroNature.List.Add(info);
            }
            foreach (var item in NatureLibrary.Basic9Nature)
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = GetNatureValue(item.Key);
                heroNature.List.Add(info);
            }
            heroNature.MaxHp = GetNatureValue(NatureType.PRO_MAX_HP);
            heroNature.Hp = GetNatureValue(NatureType.PRO_HP);
            return heroNature;
        }

        private PLAY_BASE_INFO GetPlayerSimpleInfo()
        {
            PLAY_BASE_INFO heroInfo = new PLAY_BASE_INFO
            {
                Uid = Uid,
                Name = Name,
                Icon = Icon,
                Camp = (int)Camp,
                //IconFrame = GetFaceFrame(),
                //ShowDIYIcon = ShowDIYIcon
            };
            var counter = GetCounter(CounterType.BuyCampBattleNattureCount);
            heroInfo.BuyNatureCount = counter == null ? 0 : counter.Count;

            return heroInfo;
        }

        private HERO_INFO GetPlayerHeroInfoCampMsgData()
        {
            HeroInfo playerHeroInfo = HeroMng.GetPlayerHeroInfo();

            HERO_INFO heroInfoMsg = new HERO_INFO
            {
                Id = InstanceId,
                Level = Level,
                AwakenLevel = playerHeroInfo.AwakenLevel,
                EquipIndex = playerHeroInfo.EquipIndex,
                StepsLevel = playerHeroInfo.StepsLevel,
                HeroId = HeroId,
                GodType = GodType,
                HeroNature = GetPlayerNature(),
                BattlePower = playerHeroInfo.GetBattlePower()
            };


            Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(HeroId);
            if (soulRingDic != null)
            {
                //有魂环
                foreach (var soulRing in soulRingDic)
                {
                    try
                    {
                        HERO_SOULRING soulRingData = new HERO_SOULRING();
                        soulRingData.Pos = soulRing.Key;
                        soulRingData.Level = soulRing.Value.Level;
                        soulRingData.SpecId = soulRing.Value.SpecId;
                        soulRingData.Year = soulRing.Value.Year;
                        heroInfoMsg.SoulRings.Add(soulRingData);
                    }
                    catch (Exception e)
                    {
                        //没找到魂环信息
                        Log.WarnLine("player {0} get challenger hero info fail,can not find soulBone {1}, {2}.", Uid, soulRing.Value.Uid, e);
                    }
                }
            }
            return heroInfoMsg;
        }

        private HERO_INFO GetCampHeroInfoMsgData(Hero hero)
        {
            HERO_INFO heroInfo = new HERO_INFO();

            heroInfo.Id = hero.InstanceId;
            heroInfo.Level = hero.Level;
            heroInfo.AwakenLevel = hero.HeroInfo.AwakenLevel;
            heroInfo.EquipIndex = hero.HeroInfo.EquipIndex;
            heroInfo.StepsLevel = hero.HeroInfo.StepsLevel;
            heroInfo.HeroId = hero.HeroId;
            heroInfo.GodType = hero.HeroInfo.GodType;
            heroInfo.HeroNature = GetNature(hero);
            heroInfo.BattlePower = hero.HeroInfo.GetBattlePower();
            heroInfo.DefensiveQueueNum = hero.HeroInfo.DefensiveQueueNum;
            heroInfo.DefensivePositionNum = hero.HeroInfo.DefensivePositionNum;

            Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(hero.HeroId);
            if (soulRingDic != null)
            {
                //有魂环
                foreach (var soulRing in soulRingDic)
                {
                    try
                    {
                        HERO_SOULRING soulRingData = new HERO_SOULRING();
                        soulRingData.Pos = soulRing.Key;
                        soulRingData.Level = soulRing.Value.Level;
                        soulRingData.SpecId = soulRing.Value.SpecId;
                        soulRingData.Year = soulRing.Value.Year;
                        heroInfo.SoulRings.Add(soulRingData);
                    }
                    catch (Exception e)
                    {
                        //没找到魂环信息
                        Log.WarnLine("player {0} get challenger hero info fail,can not find soulBone {1}, {2}.", Uid, soulRing.Value.Uid, e);
                    }
                }
            }
            return heroInfo;
        }

        /// <summary>
        /// 胜利1，失败2
        /// </summary>
        /// <param name="result"></param>
        public void CampBattleAddScore(int result)
        {
            CampBattleStep step = GetCampBattleStep();
            CampScoreRuleModel scoreModel = GetBattleStepScore(result);
            if (scoreModel != null)
            {
                //UpdateCampScore(scoreModel.BattleLayoutScore);
                AddCampBattleRankScore(RankType.CampBattleScore, scoreModel.BattleLayoutScore);
            }
        }

        /// <summary>
        /// 胜利1，失败2
        /// </summary>
        /// <param name="result"></param>
        public void CampBattleNeutralAddScore(int result)
        {
            CampBattleStep step = GetCampBattleStep();
            CampScoreRuleModel scoreModel = GetBattleStepScore(result);
            if (scoreModel != null)
            {
                //UpdateCampScore(scoreModel.NeutralScore);
                AddCampBattleRankScore(RankType.CampBattleScore, scoreModel.NeutralScore);
            }
        }

        public CampBattleStep GetCampBattleStep()
        {
            return server.RelationServer.CampBattlePhaseInfo.CampBattleStep;
        }

        private void UpdateBattleFortNature(int totalCount)
        {
            MSG_ZR_UPDATE_NATURE_COUNT msg = new MSG_ZR_UPDATE_NATURE_COUNT();
            msg.NewCount = totalCount;
            server.SendToRelation(msg, Uid);
        }

        public void AddCampBattleRankScore(RankType rankType, int score)
        {
            //加到redis
            server.GameRedis.Call(new OperateIncrementCampScore(server.MainId, (int)Camp, rankType, uid, score, server.Now()));

            MSG_ZR_ADD_RANK_SCORE msg = new MSG_ZR_ADD_RANK_SCORE();
            msg.Camp = (int)Camp;
            msg.RankType = (int)rankType;
            msg.Score = score;

            server.SendToRelation(msg, Uid);

            switch (rankType)
            {
                case RankType.CampBattleScore:
                    SyncDbUpdateCampScore(score);
                    break;
                case RankType.CampBattleCollection:
                    SyncDbUpdateCampCollection(score);
                    break;
                //case RankType.CampBattleFight:
                //    SyncDbUpdateCampFight(score);
                //    break;
                case RankType.CampBuild:
                    SyncDbUpdateCampBuildValue(score);
                    break;
                case RankType.CampLeader:
                    break;
                default:
                    break;
            }
        }

        public void SyncDbUpdateCampScore(int addCampScore)
        {
            CampBattleMng.CampScore += addCampScore;
            if (CampBattleMng.CampScore > CampBattleMng.HistoricalMaxCampScore)
            {
                CampBattleMng.HistoricalMaxCampScore = CampBattleMng.CampScore;

                MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE msg = new MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE();
                msg.Uid = uid;
                msg.Score = CampBattleMng.HistoricalMaxCampScore;
                server.RelationServer.Write(msg);

                QueryUpdateCampScore query = new QueryUpdateCampScore(Uid, CampBattleMng.CampScore, CampBattleMng.HistoricalMaxCampScore);
                server.GameDBPool.Call(query);

                OperateRecordHistoricalMaxCampScore oper = new OperateRecordHistoricalMaxCampScore(server.MainId, uid, CampBattleMng.HistoricalMaxCampScore);
                server.GameRedis.Call(oper);
            }
            else
            {
                QueryUpdateCampScore query = new QueryUpdateCampScore(Uid, CampBattleMng.CampScore);
                server.GameDBPool.Call(query);
            }
        }

        public void SyncDbUpdateCampBuildValue(int addvalue)
        {
            CampBattleMng.CampBuildValue += addvalue;

            QueryUpdateCampBuildValue query = new QueryUpdateCampBuildValue(Uid, CampBattleMng.CampBuildValue);
            server.GameDBPool.Call(query);
        }



        public void SyncDbUpdateCampFight(int campFight)
        {
            CampBattleMng.CampFight += campFight;
            QueryUpdateCampFight query = new QueryUpdateCampFight(Uid, CampBattleMng.CampFight);
            server.GameDBPool.Call(query);
        }

        public void SyncDbUpdateCampCollection(int campCollection)
        {
            CampBattleMng.CampCollection += campCollection;
            QueryUpdateCampCollection query = new QueryUpdateCampCollection(Uid, CampBattleMng.CampCollection);
            server.GameDBPool.Call(query);
        }

        private ZMZ_CAMP_BATTLE_INFO GetCampBattleTransform()
        {
            return CampBattleMng.GetCampBattleTransform();
        }

        public void LoadCampBattleTransform(ZMZ_CAMP_BATTLE_INFO info)
        {
            CampBattleMng.LoadCampBattleTransform(info);
        }

        bool natureItemIsUsing = false;
        public void UseNatureItem(int fortId, int itemId)
        {
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item == null)
            {
                MSG_ZGC_USE_NATURE_ITEM response = new MSG_ZGC_USE_NATURE_ITEM();
                response.Result = (int)ErrorCode.NotFoundItem;
                response.FortId = fortId;
                response.ItemId = itemId;
                Write(response);
                Log.Warn($"player {Uid} use nature item failed: not find item {itemId}");
            }

            if (natureItemIsUsing)
            {
                return;
            }

            MSG_ZR_CHECK_USE_NATURE_ITEM msg = new MSG_ZR_CHECK_USE_NATURE_ITEM();
            msg.FortId = fortId;
            msg.ItemId = itemId;
            msg.Camp = (int)Camp;
            server.RelationServer.Write(msg, uid);
        }

        public void RealUseNatureItem(MSG_RZ_CHECK_USE_NATURE_ITEM msg)
        {
            if (msg.Result == (int)ErrorCode.Success)
            {
                natureItemIsUsing = false;
                BaseItem item = BagManager.GetItem(MainType.Consumable, msg.ItemId);
                if (item == null)
                {
                    MSG_ZGC_USE_NATURE_ITEM response = new MSG_ZGC_USE_NATURE_ITEM();
                    response.Result = (int)ErrorCode.NotFoundItem;
                    response.FortId = msg.FortId;
                    response.ItemId = msg.ItemId;
                    Write(response);
                    return;
                }

                DelItem2Bag(item, (RewardType)(int)item.MainType, 1, ConsumeWay.CampFortAddNature, string.Format("fort {0}", msg.FortId));
                SyncClientItemInfo(item);

                MSG_ZR_USE_NATURE_ITEM request = new MSG_ZR_USE_NATURE_ITEM();
                request.FortId = msg.FortId;
                request.ItemId = msg.ItemId;
                server.RelationServer.Write(request, uid);
            }
            else
            {
                MSG_ZGC_USE_NATURE_ITEM response = new MSG_ZGC_USE_NATURE_ITEM();
                response.Result = msg.Result;
                response.FortId = msg.FortId;
                response.ItemId = msg.ItemId;
                Write(response);
            }
        }


        public void NeutralDungeonReward(RewardManager manager, DungeonModel model)
        {
            //string probabilityRewardStr = model.Data.GetString("FirstReward");
            //string normalRewardStr = model.Data.GetString("GeneralReward");

            //manager.AddCalculateReward(1,probabilityRewardStr);
            //manager.AddSimpleReward(normalRewardStr);
            List<ItemBasicInfo> getList = AddRewardDrop(model.Data.GetIntList("FirstRewardId", "|"));
            manager.AddReward(getList);
            getList = AddRewardDrop(model.Data.GetIntList("GeneralRewardId", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards(true);

            // 发放奖励
            AddRewards(manager, ObtainWay.CampBattleNeutral);

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = model.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }

        public void GiveUpFort(int fortId)
        {
            MSG_ZR_GIVEUP_FORT request = new MSG_ZR_GIVEUP_FORT();
            request.FortId = fortId;
            server.RelationServer.Write(request, uid);

            MSG_ZGC_GIVEUP_FORT response = new MSG_ZGC_GIVEUP_FORT();
            response.Result = (int)DungeonResult.Success;
            Write(response);
        }

        public void HoldFort(int fortId)
        {

            var expend = CampBattleLibrary.GetCampBattleExpend();
            if (GetCounterValue(CounterType.ActionCount) < expend.StrongPoint.Item1)
            {
                MSG_RZ_HOLD_FORT response = new MSG_RZ_HOLD_FORT();
                response.Result = (int)ErrorCode.ActionCountNotEnough;
                Log.Warn($"player {uid} hold camp fort {fortId} fail, action count not enough!");
                Write(response);
                return;
            }

            if (server.RelationServer.campCoins[Camp] < expend.NeutralityPoint.Item1)
            {
                MSG_RZ_HOLD_FORT response = new MSG_RZ_HOLD_FORT();
                response.Result = (int)ErrorCode.GrianNotEnough;
                Log.Warn($"player {uid} hold camp fort {fortId} dungeon fail, grain count not enough!");
                Write(response);
                return;
            }

            MSG_ZR_HOLD_FORT request = new MSG_ZR_HOLD_FORT();
            request.FortId = fortId;
            request.AttackerInfo = GetPlayerSimpleInfo();

            Dictionary<int, Hero> attackerHeroDic = GetDefensiveQueueHeros();

            foreach (var heroItem in attackerHeroDic)
            {
                HERO_INFO info = GetCampHeroInfoMsgData(heroItem.Value);
                request.AttackerHeroList.Add(info);
            }
            server.RelationServer.Write(request, uid);
        }


        public void SendCampBattleAnnoucementList()
        {
            var expend = CampBattleLibrary.GetCampBattleExpend();
            OperateCampBattleAnnoucementList oper = new OperateCampBattleAnnoucementList(server.MainId, expend.CampBattleAnnoucementListMaxCnt);
            server.GameRedis.Call(oper, ret =>
            {
                if ((int)ret > 0)
                {
                    MSG_ZGC_CAMPBATTLE_ANNOUNCE_LIST response = new MSG_ZGC_CAMPBATTLE_ANNOUNCE_LIST();
                    foreach (var item in oper.CampBattleAnnoucementList)
                    {
                        CAMPBATTLE_ANNOUNCE msg = MessagePacker.ProtobufHelper.DeserializeFromString<CAMPBATTLE_ANNOUNCE>(item);
                        response.List.Add(msg);
                    }
                    Write(response);
                }
            });

        }



    }
}

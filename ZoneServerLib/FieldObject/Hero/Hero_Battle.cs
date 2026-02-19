using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Logger;
using Newtonsoft.Json;
using ServerModels.School;

namespace ZoneServerLib
{
    partial class Hero
    {
        public override float DeadDelay
        { get { return heroDataModel.DeadDelay; } }

        //public override bool IsEnemy(FieldObject target)
        //{
        //    return IsAttacker == target.IsAttacker;
        //}

        //public override bool IsAlly(FieldObject target)
        //{
        //    return IsAttacker != target.IsAttacker;
        //}

        public bool IsChanllenger()
        {
            if (owner != null)
            {
                return true;
            }
            return false;
        }

        public override void StartFighting()
        {
            if (CurDungeon == null || InBattle) return;

            base.StartFighting();
            UpdateProSpd(NatureType.PRO_RUN_IN_BATTLE);
            InitAI();
            InitHolaEffect();

            //检测存货单位
            DispatchAliveCountMessage();

//#if DEBUG

//            Log.Info($"--------------------------------------------------------------------------------------------------------------------------nature1-----instanceId :  {HeroId} instanceId {instanceId}");

//            Log.Info1(JsonConvert.SerializeObject(GetNature().GetNatureList()));
//            Log.Info("--------------------------------------------------------------------------------------------------------------------------nature2-----instanceId :" + instanceId);

//            Log.Info("--------------------------------------------------------------------------------------------------------------------------trigger1-----instanceId :" + instanceId);
//            Log.Info1("trigger count " + triggerManager.GetTriggerCount());
//            Log.Info("--------------------------------------------------------------------------------------------------------------------------trigger2-----instanceId :" + instanceId);

//            Log.Info("--------------------------------------------------------------------------------------------------------------------------skill1-----instanceId :" + instanceId);
//            Log.Info1("skill level " + heroInfo.SoulSkillLevel);
//            Log.Info("--------------------------------------------------------------------------------------------------------------------------skill2-----instanceId :" + instanceId);
//#endif

            //战力压制
            CurDungeon.CheckBattlePowerSuppress(this);
        }

        public override void BindSkills()
        {
            int skillLevel = heroInfo.SoulSkillLevel / 10 + 1;
            foreach (var skillId in heroDataModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }

                if (IsMonsterHero)
                {
                    // 魂环技单独处理
                    if (skillModel.SoulRingPos > MonsterHeroSoulRingCount)
                    {
                        continue;
                    }
                    SkillManager.Add(skillId, MonsterHeroSkillLevel);
                }
                else
                {
                    // 魂环技单独处理
                    if (skillModel.SoulRingPos > 0)
                    {
                        continue;
                    }

                    // 非魂环技能 技能等级为1
                    SkillManager.Add(skillId, skillLevel);
                }
            }
        }

        public override void BindSoulRingSkills()
        {
            //人性怪没有魂环技能
            if (!IsMonsterHero)
            {
                int skillLevel = heroInfo.SoulSkillLevel / 10 + 1;
                int addYearRatio = SoulRingManager.GetAddYearRatio(heroInfo.StepsLevel);
                List<BattleSoulRingInfo> soulRingSpecList = new List<BattleSoulRingInfo>();

                foreach (var skillId in heroDataModel.Skills)
                {
                    SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                    if (skillModel == null)
                    {
                        continue;
                    }
                    // 魂环技， 通过魂环等级确定技能等级
                    if (skillModel.SoulRingPos > 0)
                    {
                        SoulRingItem soulRing;
                        if (!OwnerIsRobot)
                        {
                            PlayerChar ow = owner as PlayerChar;
                            soulRing = ow.SoulRingManager.GetSoulRing(heroDataModel.HeroId, skillModel.SoulRingPos);
                            if (soulRing == null)
                            {
                                // 未装备该魂环
                                continue;
                            }

                            SkillManager.Add(skillId, skillLevel);
                            int currentYear = SoulRingManager.GetAffterAddYear(soulRing.Year, addYearRatio);
                            
                            BattleSoulRingInfo soulRingInfo = new BattleSoulRingInfo(soulRing.Position, soulRing.Level, currentYear, soulRing.SpecId, soulRing.Element);
                            soulRingSpecList.Add(soulRingInfo);
                            
// #if  DEBUG
//                             Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring1-----instanceId :"+instanceId);
//                             Log.Info1("soulring "+ JsonConvert.SerializeObject(soulRingInfo));
//                             Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring2-----instanceId :"+instanceId);
// #endif
                        }
                        else
                        {
                            Robot robot = Owner as Robot;

                            if (robot.HeroSoulRings.ContainsKey(HeroId) && robot.HeroSoulRings[HeroId].ContainsKey(skillModel.SoulRingPos))
                            {
                                SkillManager.Add(skillId, skillLevel);

                                BattleSoulRingInfo soulRingInfo = robot.HeroSoulRings[HeroId][skillModel.SoulRingPos];
                                soulRingSpecList.Add(soulRingInfo);
                                
// #if  DEBUG
//                                 Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring1-----instanceId :"+instanceId);
//                                 Log.Info1("soulring "+ JsonConvert.SerializeObject(soulRingInfo));
//                                 Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring2-----instanceId :"+instanceId);
// #endif
                            }
                        }
                    }
                }
                SoulRingSpecUtil.DoEffect(soulRingSpecList, this);
            }

            PrintNatures(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", heroInfo.Id);
        }

        public override void BindSoulBoneSkills()
        {
            base.BindSoulBoneSkills();

            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                List<SoulBone> soulBoneList = ow.SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
                if (soulBoneList == null) return;
                
                List<SoulBoneSpecModel> specs = new List<SoulBoneSpecModel>();
                foreach (var item in soulBoneList)
                {
                    List<SoulBoneSpecModel> soulBoneSpecs = SoulBoneLibrary.GetSpecModel4ItemId(item.TypeId, item.GetSpecList());
                    if (soulBoneSpecs != null && soulBoneSpecs.Count > 0)
                    {
                        specs.AddRange(soulBoneSpecs);
                    }
                }

                SoulBoneSpecUtil.DoEffect(specs, this);
                
// #if  DEBUG
//                 Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring1-----instanceId :"+instanceId);
//                 Log.Info1("soulbone hero"+ JsonConvert.SerializeObject(specs));
//                 Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring2-----instanceId :"+instanceId);
// #endif
            }
            else
            {
                Robot robot = Owner as Robot;
                if (robot.HeroSoulBones.ContainsKey(heroInfo.Id))
                {
                    List<BattleSoulBoneInfo> soulBoneList = robot.GetHeroSoulBones(heroInfo.Id);
                    if (soulBoneList == null) return;
                    List<SoulBoneSpecModel> specs = new List<SoulBoneSpecModel>();
                    foreach (var kv in soulBoneList)
                    {
                        List<SoulBoneSpecModel> soulBoneSpecs = SoulBoneLibrary.GetSpecModel4ItemId(kv.Id, kv.SpecList);

                        if (soulBoneSpecs != null && soulBoneSpecs.Count > 0)
                        {
                            specs.AddRange(soulBoneSpecs);
                        }
                    }
                    SoulBoneSpecUtil.DoEffect(specs, this);
// #if  DEBUG
//                         Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring1-----instanceId :"+instanceId);
//                         Log.Info1("soulbone robot"+ JsonConvert.SerializeObject(specs));
//                         Log.Info("--------------------------------------------------------------------------------------------------------------------------soulring2-----instanceId :"+instanceId);
// #endif
                }
            }
        }

        /// <summary>
        /// 星级特殊效果
        /// </summary>
        private void StepsSpecialEffect()
        {
            int count = heroInfo.StepsLevel / 6;
            if(count<=0) return;

            HeroModel heroModel = HeroLibrary.GetHeroModel(heroInfo.Id);
            if(heroModel == null) return;

            List<int> specIds = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                if (heroModel.StepSpecial.Count <= i)
                {
                    Log.Error($"step spec effect error, have not spec {count}");
                    break;
                }
                
                specIds.Add(heroModel.StepSpecial[i]);
            }

            if (specIds.Count <= 0) return;

            foreach (var kv in specIds)
            {
                var model = HeroLibrary.GeHeroStepSpecialModel(kv);
                if (model == null)
                {
                    Logger.Log.Warn($"have not step special id {kv}");
                    continue;
                }
                
// #if  DEBUG
//                 Log.Info("--------------------------------------------------------------------------------------------------------------------------element1-----instanceId :"+instanceId);
//                 Log.Info1("element "+model.Id );
//                 Log.Info("--------------------------------------------------------------------------------------------------------------------------element2-----instanceId :"+instanceId);
// #endif
                
                SoulRingSpecUtil.DoSpecialEffect(this, model.Type, 1, model.Param1, (int)model.Param2.Value);
            }
        }

        private void HiddenWeaponEffect()
        {
            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                HiddenWeaponItem weaponItem = ow?.HiddenWeaponManager.GetHeroEquipWeapon(heroInfo.Id);
                if (weaponItem == null) return;

                BattleHiddenWeaponInfo info = new BattleHiddenWeaponInfo(weaponItem.Id, weaponItem.Info.Star);
                SoulRingSpecUtil.DoEffect(info, this);

//#if DEBUG
//                Log.Info("--------------------------------------------------------------------------------------------------------------------------hidden weapon 1-----instanceId :" + instanceId);
//                Log.Info1($"hidden weapon hero {heroInfo}" + JsonConvert.SerializeObject(info));
//                Log.Info("--------------------------------------------------------------------------------------------------------------------------hidden weapon 2-----instanceId :" + instanceId);
//#endif
            }
            else
            {
                Robot robot = Owner as Robot;
                BattleHiddenWeaponInfo info = robot?.GetHiddenWeaponInfo(heroInfo.Id);
                if (info == null) return;

                SoulRingSpecUtil.DoEffect(info, this);

//#if DEBUG
//                Log.Info("--------------------------------------------------------------------------------------------------------------------------hidden weapon 1-----instanceId :" + instanceId);
//                Log.Info1("hidden weapon robot" + JsonConvert.SerializeObject(info));
//                Log.Info("--------------------------------------------------------------------------------------------------------------------------hidden weapon 2-----instanceId :" + instanceId);
//#endif
            }
        }

        private void EquipEnchantEffect()
        {
            List<EquipmentSpecialModel> specialModels = null;
            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                List<EquipmentItem> equipments = ow?.EquipmentManager.GetAllEquipedEquipments(heroInfo.Id);
                if (equipments == null || equipments.Count < 2) return;

                specialModels = EquipLibrary.GetEquipSeSpecialModelsByEquipIds(equipments.Select(x => x.Model.ID));
            }
            else
            {
                Robot robot = Owner as Robot;
                if (robot == null) return;

                List<int> equipList;
                if (!robot.EquipmentList.TryGetValue(heroInfo.Id, out equipList))
                {
                    return;
                }

                specialModels = EquipLibrary.GetEquipSeSpecialModelsByEquipIds(equipList);
            }

            if (specialModels?.Count > 0)
            {
                foreach (var model in specialModels)
                {
                    SoulRingSpecUtil.DoSpecialEffect(this, model.Type, 1, model.Param1, (int)model.Param2.Value);
                }
            }
        }

        private void SchoolPoolEffect()
        {
            if(!CurDungeon.IsSchoolPoolBuffValid()) return;

            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                if(!ow.SchoolManager.IsBuffValid()) return;

                SchoolInfo schoolInfo = ow.SchoolManager.SchoolInfo;
                if(schoolInfo.SchoolId==0) return;

                List<int> buffList = SchoolLibrary.GetPoolItemBuffList(schoolInfo.PoolItemId, (SchoolType)schoolInfo.SchoolId);
                if (buffList == null) return;

                foreach (var id in buffList)
                {
                    SchoolSpecialModel model = SchoolLibrary.GetSchoolSpecialModel(id);
                    PoolGrowthModel growthModel = SchoolLibrary.GetPoolGrowthModel(schoolInfo.PoolLevel);
                    if (model == null || growthModel == null)
                    {
                        Log.Warn($"had not find SchoolSpecialModel {id}");
                        continue;
                    }

                    int value = (int)(model.Param2.Key * growthModel.Growth + model.Param2.Value);

                    //推荐使用增加属性百分比使用类型 model.Type AddNatureRatio = 1,  // 属性值百分比增加 NatureType NatureRatio
                    SoulRingSpecUtil.DoSpecialEffect(this, model.Type, 1, model.Param1, value);
                }
            }
            //else
            //{
            //    Robot robot = Owner as Robot;
            //    BattleHiddenWeaponInfo info = robot?.GetHiddenWeaponInfo(heroInfo.Id);
            //    if (info == null) return;

            //    SoulRingSpecUtil.DoEffect(info, this);
            //}
        }

        private void PetEffect()
        {
            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                PetInfo petInfo = ow.PetManager.GetInQueuePetInfo(CurDungeon.GetMapType(), HeroInfo, CurDungeon.DungeonModel.Id);
                NaturesAddPetsBonusValue(ow, petInfo, OwnerIsRobot);
            }
            else
            {
                Robot robot = Owner as Robot;
                //目前只有镜像
                if (robot.CheckUseMirrorPet(CurDungeon.GetMapType()))
                {
                    NaturesAddPetsBonusValue(robot, robot.RobPetInfo, OwnerIsRobot);
                }
            }
            BroadCastHp();
        }

        private void BindTriggers()
        {
            foreach (var triggerId in heroDataModel.Triggers)
            {
                BaseTrigger trigger = new TriggerInHero(this, triggerId);
                AddTrigger(trigger);
            }

            BindGodTriggers();
        }

        private void BindGodTriggers()
        {
            //装备了神位
            if (heroInfo != null && heroInfo.GodType > 0)
            {
                HeroGodStepUpGrowthModel godModel = GodHeroLibrary.GetGodStepUpGrowthModel(heroInfo.GodType, heroInfo.StepsLevel);
                if (godModel == null)
                {
                    Log.Error($"had not find HeroGodStepUpGrowthModel model hero {heroInfo.Id} god {heroInfo.GodType} step {heroInfo.StepsLevel}");
                    return;
                }

                if (godModel.Triggers.Count <= 0)
                {
                    return;
                }

                foreach (var triggerId in godModel.Triggers)
                {
                    BaseTrigger trigger = new TriggerInGod(this, triggerId);
                    AddTrigger(trigger);
                }
            }
        }

        public bool InBattleField()
        {
            if (InBattle)
            {
                return true;
            }
            return false;
        }

        //按最新策划要求 没有仇恨也要有目标
        public bool FindTarget()
        {
            List<FieldObject> tempTargetList = new List<FieldObject>();
            FieldObject target = null;
            Vec2 targetPos = null;

            if (HateManager.Target == null)
            {
                GetEnemyInSplash(this, SplashType.Circle, Position, new Vec2(0, 0), HateRange, 0f, 0f, tempTargetList, 10, -1, true);
                if (tempTargetList.Count > 0)
                {
                    //FieldObject temp = null;
                    //float hateRatio = 0;
                    //foreach (var item in tempTargetList)
                    //{
                    //    if (item.HateRatio >= hateRatio)
                    //    {
                    //        temp = item;
                    //    }
                    //}
                    FieldObject temp = null;

                    float curMaxHateRatio = 0f;
                    float curMinLengthPow = float.MaxValue;
                    foreach (var item in tempTargetList)
                    {
                        float tempHateRatio = 0f;
                        float tempLengthPow = 0f;
                        if (temp == null)
                        {
                            temp = item;
                            curMaxHateRatio = item.HateRatio;
                            curMinLengthPow = (Owner.Position - item.Position).magnitudePower;
                            continue;
                        }

                        tempHateRatio = item.HateRatio;
                        tempLengthPow = (Owner.Position - item.Position).magnitudePower;
                        if (tempLengthPow < curMinLengthPow || (tempLengthPow == curMinLengthPow && tempHateRatio > curMaxHateRatio))
                        {
                            temp = item;
                            curMaxHateRatio = tempHateRatio;
                            curMinLengthPow = tempLengthPow;
                        }
                    }
                    HateManager.SetTarget(temp);
                }
            }

            Skill skill;
            if (SkillEngine == null)
            {
                return false;
            }
            if (SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 有准备好的技能，且该技能满足释放条件
                return true;
            }
            return false;
        }

        public override long OnHit(FieldObject caster, DamageType damageType, long damage, ref bool immune, float multipleParam = 1)
        {
            damage = base.OnHit(caster, damageType, damage, ref immune, multipleParam);
            if (damage > 0 && caster != null && IsEnemy(caster))
            {
                ////调用队友的hatemanager
                //int hateValue = (int)(damage * caster.HateRatio + 0.5f);
                //int teamMateHate = hateValue / 3;
                //foreach (var hero in Owner.CurrentMap.HeroList)
                //{
                //    if (hero.Value.Owner == Owner && hero.Value == this)
                //    {
                //        Hero temp = hero.Value as Hero;
                //        hateManager?.AddHate(caster, hateValue);
                //    }
                //    else if (hero.Value.Owner == Owner && !(hero.Value.CurrentMap is ArenaDungeonMap))
                //    {
                //        hero.Value.HateManager?.AddHate(caster, teamMateHate);
                //    }
                //}

                SkillManager?.AddOnHitBodyEnergy(damage);
            }
            return damage;
        }

        public override void OnChanged()
        {
            //不触发死亡，该方法的主要作用是，变身，并需要继承血量

            //副本不存在、未在正常开启状态不接收消息
            DungeonMap dungeonMap = currentMap as DungeonMap;
            if (dungeonMap == null || dungeonMap.State != DungeonState.Started)
            {
                return;
            }

            StopFighting();
            dungeonMap.OnFieldObjectDead(this);

            currentMap.RemoveHero(instanceId);
        }

        //开始复活操作
        public override void Revive()
        {
            if (!(currentMap as DungeonMap)?.CanRevive ?? false)
            {
                return;
            }
            IsReviving = true;
            FsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
        }

        //复活后事件
        public override void OnRevived()
        {
            base.OnRevived();
            InitNature();

            BroadCastHp();
            BroadCastRevived();

            StartFighting();

            DispatchHeroStartFightMsg(HeroId);
        }

        public override void OnDead()
        {
            if (CurDungeon == null)
            {
                return;
            }
            base.OnDead();
        }

        public override void StopFighting()
        {
            base.StopFighting();
            ClearBasicBattleState();
        }

        protected override void BroadCastHiddenWeaponInfo()
        {
            MSG_ZGC_HIDDEN_WEAPON_INFO msg = GetHiddenWeaponInfo();
            if (msg != null)
            {
                BroadCast(msg);
            }
        }

        public MSG_ZGC_HIDDEN_WEAPON_INFO GetHiddenWeaponInfo()
        {
            MSG_ZGC_HIDDEN_WEAPON_INFO msg = new MSG_ZGC_HIDDEN_WEAPON_INFO() { InstanceId = instanceId };

            if (!OwnerIsRobot)
            {
                PlayerChar ow = owner as PlayerChar;
                HiddenWeaponItem weaponItem = ow?.HiddenWeaponManager.GetHeroEquipWeapon(heroInfo.Id);
                if (weaponItem == null) return null;

                msg.HiddenWeaponId = weaponItem.Id;
            }
            else
            {
                Robot robot = Owner as Robot;
                BattleHiddenWeaponInfo info = robot?.GetHiddenWeaponInfo(heroInfo.Id);
                if (info == null) return null;

                msg.HiddenWeaponId = info.Id;
            }

            return msg;
        }

        public bool CheckCollisionWithPriority(FieldObject target, Vec2 pos)
        {
            foreach (var kv in currentMap.HeroList)
            {
                Hero hero = kv.Value;
                if (hero == this)
                {
                    continue;
                }
                //根据优先级可被计算的目标
                if (hero.CollisionPriority > CollisionPriority
                    || hero.CollisionPriority ==CollisionPriority && hero.InstanceId < InstanceId)
                {
                    //计算是否碰撞
                    if (Vec2.GetDistance(pos, hero.Position) < Radius + hero.Radius + HeroLibrary.HeroCollisionRadius)
                    {
                        Logger.Log.Debug($"{hero.HeroId} radius {hero.Radius} collision with {HeroId} radius {Radius}");
                        return true;
                    }
                }
            }
            return false;
        }
        public bool CheckCollision(FieldObject target, Vec2 pos)
        {
            //float tempRadius = 0;
            //switch (CurrentMap.Model.MapType)
            //{
            //    case MapType.Arena:
            //    case MapType.CrossBattle:
            //        break;
            //    default:
            //        tempRadius = HeroLibrary.HeroCollisionRadius;
            //        break;
            //}
            foreach (var kv in currentMap.HeroList)
            {
                Hero hero = kv.Value;
                if (hero == this || GetOwner() != hero.GetOwner())
                {
                    continue;
                }
                if (hero.CollisionPriority > CollisionPriority)
                {
                    //计算是否碰撞
                    if (Vec2.GetDistance(pos, hero.Position) < Radius + hero.Radius + HeroLibrary.HeroCollisionRadius)
                    {
                        Logger.Log.Debug($"{hero.HeroId} radius {hero.Radius} collision with {HeroId} radius {Radius}");
                        return true;
                    }
                }
                ////计算是否碰撞
                //if (Vec2.GetDistance(pos, hero.Position) < HeroModel.Radius + hero.HeroModel.Radius + HeroLibrary.HeroCollisionRadius)
                //{
                //    Logger.Log.Debug($"{hero.HeroId} radius {hero.HeroModel.Radius} collision with {HeroId} radius {HeroModel.Radius}");
                //    return true;
                //}
            }
            //foreach (var kv in currentMap.MonsterList)
            //{
            //    Monster monster = kv.Value;
            //    //计算是否碰撞
            //    if (Vec2.GetDistance(pos, monster.Position) < HeroModel.Radius + monster.MonsterModel.Radius)
            //    {
            //        Logger.Log.Debug($"{monster.MonsterModel.Id} radius {monster.MonsterModel.Radius} collision with {HeroModel.Id} radius {HeroModel.Radius}");
            //        return true;
            //    }
            //}
            return false;
        }

        //public Tuple<bool,FieldObject>

        public bool CheckAllCollision(Monster monster, Vec2 pos)
        {
            foreach (var kv in currentMap.HeroList)
            {
                Hero hero = kv.Value;
                if (hero == this)
                {
                    continue;
                }
                if (Vec2.GetRangePower(pos, hero.Position) < Math.Pow((Radius + hero.Radius), 2))
                {
                    return true;
                }
            }
            foreach (var kv in currentMap.MonsterList)
            {
                Monster mon = kv.Value;
                if (Vec2.GetRangePower(pos, mon.Position) < Math.Pow((Radius + mon.Radius), 2))
                {
                    return true;
                }
            }
            return false;
        }

        //逐层随机点法获取非碰撞点
        //public Tuple<bool, Vec2> GetNonCollisionPos(FieldObject target, Vec2 pos, float skillDis, int maxCount = 4, float deltaLength = 0.1f)
        //{
        //    float dis = target.Radius + HeroModel.Radius + skillDis;
        //    int count = maxCount;
        //    int allCount = 0;
        //    float temp = HeroModel.Radius;

        //    temp = target.Radius + HeroModel.Radius + skillDis - deltaLength;
        //    for (int i = 0; i < 50; i++)
        //    {
        //        allCount++;
        //        Vec2 tempPos = GetRandomVec2FromTo(temp, dis);
        //        if (!CheckCollision(target, tempPos + target.Position))
        //        {
        //            Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
        //            return Tuple.Create(true, tempPos + target.Position);
        //        }
        //        else
        //        {
        //            Logger.Log.Debug($"hero {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
        //        }
        //    }
        //    Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
        //    return Tuple.Create(false, pos);
        //}

        /// <summary>
        /// 获取尽量靠近player的点
        /// </summary>
        /// <param name="player"></param>
        /// <param name="target"></param>
        /// <param name="pos"></param>
        /// <param name="skillDis"></param>
        /// <param name="maxCount"></param>
        /// <param name="deltaLength"></param>
        /// <returns></returns>
        //public Tuple<bool, Vec2> GetNonCollisionPos(Vec2 playerPos, float playerRadius, FieldObject target, Vec2 pos, float skillDis, int maxCount = 4, float deltaLength = 0.1f)
        //{
        //    float dis = target.Radius + HeroModel.Radius + skillDis;
        //    int count = maxCount;
        //    int allCount = 0;
        //    float temp = HeroModel.Radius;

        //    temp = target.Radius + HeroModel.Radius + skillDis - deltaLength;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        List<Vec2> availableVecs = new List<Vec2>();

        //        for (int j = 0; j < 5; j++)
        //        {
        //            allCount++;
        //            Vec2 tempPos = GetRandomVec2FromTo(temp, dis);
        //            if (!CheckCollision(target, tempPos + target.Position))
        //            {
        //                Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
        //                availableVecs.Add(tempPos + target.Position);
        //            }
        //            else
        //            {
        //                Logger.Log.Debug($"hero {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
        //            }
        //        }

        //        float disPower = float.MaxValue;
        //        bool got = false;
        //        Vec2 ans = null;

        //        float playerDisPow = (float)Math.Pow((playerRadius + radius + 0.1), 2);
        //        foreach (var item in availableVecs)
        //        {
        //            float tempDisPow = (item - playerPos).magnitudePower;

        //            if (tempDisPow < disPower && tempDisPow >= playerDisPow)
        //            {
        //                disPower = tempDisPow;
        //                got = true;
        //                ans = item;
        //            }
        //        }
        //        if (got)
        //        {
        //            return Tuple.Create(true, ans);
        //        }
        //        //return Tuple.Create(true, tempPos + target.Position);
        //    }
        //    Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
        //    return Tuple.Create(false, pos);
        //}

        public Tuple<bool, Vec2> GetNonCollisionPos(FieldObject target, Vec2 pos, float skillDis, float deltaLength = 0.1f)
        {
            //float dis = target.Radius + HeroModel.Radius + skillDis;
            //int count = maxCount;
            int allCount = 0;
            float temp = Radius;

            temp = target.Radius + Radius + skillDis - deltaLength;

            Vec2 delta = Position- target.Position ;
            delta = delta * temp / (float)delta.GetLength();
            float rad = 0;
            while(rad<360f)
            {
                List<Vec2> availableVecs = new List<Vec2>();

                rad += 5f;
                allCount++;

                Vec2 tempPos = GetVec2FromTo(delta, rad) + target.Position;
                Vec2 tempPos1 = GetVec2FromTo(delta, -rad) + target.Position;
                if (!CheckCollision(target, tempPos))
                {
                    Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos);
                }
                else if ( !CheckCollision(target, tempPos1))
                {
                    Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos1);
                }
                else
                {
                    Logger.Log.Debug($"hero {InstanceId} randomPos {tempPos1} collision for monster {target.InstanceId} with {allCount} allcount");
                    Logger.Log.Debug($"hero {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
                }

                bool got = false;
                Vec2 ans = null;
                if (availableVecs.Count > 0)
                {
                    got = true;
                    ans =availableVecs[0];
                }
                if (got)
                {
                    return Tuple.Create(true, ans);
                }
                //return Tuple.Create(true, tempPos + target.Position);
            }
            Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
            return Tuple.Create(false, Position);
        }

        private Vec2 GetRandomVec2(float length)
        {
            Vec2 vec = new Vec2();
            double dis = RAND.RangeFloat(0, length);
            double angle = RAND.RangeFloat(0, 1f) * Math.PI * 2;
            vec.x = (float)(dis * Math.Sin(angle));
            vec.y = (float)(dis * Math.Cos(angle));
            return vec;
        }

        private Vec2 GetRandomVec2FromTo(float length, float toLength)
        {
            Vec2 vec = new Vec2();
            double dis = RAND.RangeFloat(length, toLength);
            double angle = RAND.RangeFloat(0, 1f) * Math.PI * 2;
            vec.x = (float)(dis * Math.Sin(angle));
            vec.y = (float)(dis * Math.Cos(angle));
            return vec;
        }

        private Vec2 GetVec2FromTo(Vec2 temp, double rad)
        {
            rad = rad * Math.PI / 180;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double x = temp.X * cos - temp.Y * sin;
            double y = temp.X * sin + temp.Y * cos;

            return new Vec2((float)x, (float)y);
        }

        //private Vec2 GetRandomVec2FromTo(float length, float toLength)
        //{
        //    Vec2 vec = new Vec2();
        //    double dis = RAND.RangeFloat(length, toLength);
        //    double angle = RAND.RangeFloat(0, 1f) * Math.PI * 2;
        //    vec.x = (float)(dis * Math.Sin(angle));
        //    vec.y = (float)(dis * Math.Cos(angle));
        //    return vec;
        //}
    }
}
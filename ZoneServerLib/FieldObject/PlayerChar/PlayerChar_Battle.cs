using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private HeroModel heroModel;
        public HeroModel HeroModel
        { get { return heroModel; } }

        public override float DeadDelay
        { get { return heroModel.DeadDelay; } }

        //只在第一次进入和复活时被调用
        public override void StartFighting()
        {
            if (CurDungeon == null) return;
            if (CurDungeon.CheckPlayerStartedFighting(uid))
            {
                return;
            }

            CurDungeon.RecordPlayerStartedFighting(uid);


            base.StartFighting();
            //InitBattleState(IsDead);
            InitHolaEffect();

            //检测存货单位
            DispatchAliveCountMessage();
        }

        public void SendStartFightingMsg(bool first)
        {
            MSG_ZGC_STARTFIGHTING msg = new MSG_ZGC_STARTFIGHTING();
            msg.FirstStart = first;
            Write(msg);
        }

        private void InitBattleState(bool noUpdateHP)
        {
            //1. 战斗
            InitSkillManager();
            InitBuffManager();
            InitMarkManager();
            InitTrigger();
            if (noUpdateHP)
            {
                InitNatureWithoutHpUpdate();
            }
            else
            {
                InitNature();
            }
            UpdateProSpd(NatureType.PRO_RUN_IN_BATTLE);

            // 2. 魂环技能
            BindSoulRingSkills();
            // 3. 被动技能释放 被动技能可能会改变技能信息 所以待被动技能起效后再同步客户端技能信息
            PassiveSkillEffect();



            // 4. 同步技能
            MSG_ZGC_SKILL_ENERGY_LIST skillMsg = skillManager.GetSkillEnergyMsg();
            Write(skillMsg);
        }

        public override void BindSkills()
        {
            HeroInfo playerHeroInfo = HeroMng.GetPlayerHeroInfo();
            if (playerHeroInfo == null)
            {
                return;
            }
            int skillLevel = playerHeroInfo.SoulSkillLevel / 10 + 1;

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(HeroId);
            if (heroModel == null)
            {
                return;
            }

            foreach (var skillId in heroModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }

                // 魂环技单独处理
                if (skillModel.SoulRingPos > 0)
                {
                    continue;
                }

                // 非魂环技能 技能等级为1
                SkillManager.Add(skillId, skillLevel);
            }
        }

        public override void BindSoulRingSkills()
        {
            HeroInfo playerHeroInfo = HeroMng.GetPlayerHeroInfo();
            if (playerHeroInfo == null)
            {
                return;
            }

            int skillLevel = playerHeroInfo.SoulSkillLevel / 10 + 1;

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(HeroId);
            if (heroModel == null)
            {
                return;
            }
            List<BattleSoulRingInfo> soulRingSpecList = new List<BattleSoulRingInfo>();
            int addYearRatio = SoulRingManager.GetAddYearRatio(playerHeroInfo.StepsLevel);
            foreach (var skillId in heroModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }

                // 魂环技， 通过魂环等级确定技能等级
                if (skillModel.SoulRingPos > 0)
                {
                    SoulRingItem soulRing = SoulRingManager.GetSoulRing(heroModel.Id, skillModel.SoulRingPos);
                    if (soulRing == null)
                    {
                        // 未装备该魂环
                        continue;
                    }
                    SkillManager.Add(skillId, skillLevel);
                    int currentYear = SoulRingManager.GetAffterAddYear(soulRing.Year, addYearRatio);
                    soulRingSpecList.Add(new BattleSoulRingInfo(skillModel.SoulRingPos, soulRing.Level, currentYear, soulRing.SpecId, soulRing.Element));
                }
            }

            // 魂环特殊效果起效
            SoulRingSpecUtil.DoEffect(soulRingSpecList, this);

            PrintNatures(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", HeroId);
        }

        ////// todo pvp相关待修正
        ////public override bool IsEnemy(FieldObject target)
        ////{
        ////    if (currentMap == null)
        ////    {
        ////        return false;
        ////    }

        ////    return IsAttacker != target.IsAttacker;

        ////    //switch (target.FieldObjectType)
        ////    //{
        ////    //    case TYPE.MONSTER:
        ////    //        return true;
        ////    //    case TYPE.ROBOT:
        ////    //    case TYPE.PC:
        ////    //        if (currentMap.PVPType == PvpType.Person && target != this)
        ////    //        {
        ////    //            return true;
        ////    //        }
        ////    //        return false;
        ////    //    case TYPE.PET:
        ////    //        Pet pet = (Pet)target;
        ////    //        if (pet != null && currentMap.PVPType == PvpType.Person && pet.Owner != this)
        ////    //        {
        ////    //            return true;
        ////    //        }
        ////    //        return false;
        ////    //    case TYPE.HERO:
        ////    //        Hero hero = (Hero)target;
        ////    //        if (hero != null && currentMap.PVPType == PvpType.Person && hero.Owner != this)
        ////    //        {
        ////    //            return true;
        ////    //        }
        ////    //        return false;

        ////    //    default:
        ////    //        return false;
        ////    //}
        ////}

        ////// todo pvp相关待修正
        ////public override bool IsAlly(FieldObject target)
        ////{
        ////    if (currentMap == null)
        ////    {
        ////        return false;
        ////    }
        ////    return IsAttacker == target.IsAttacker;
        ////    //switch (target.FieldObjectType)
        ////    //{
        ////    //    case TYPE.MONSTER:
        ////    //        return false;
        ////    //    case TYPE.ROBOT:
        ////    //    case TYPE.PC:
        ////    //        if (currentMap.PVPType == PvpType.Person && target != this)
        ////    //        {
        ////    //            return false;
        ////    //        }
        ////    //        return true;
        ////    //    case TYPE.PET:
        ////    //        Pet pet = (Pet)target;
        ////    //        if (pet != null && currentMap.PVPType == PvpType.Person && pet.Owner != this)
        ////    //        {
        ////    //            return false;
        ////    //        }
        ////    //        return true;
        ////    //    case TYPE.HERO:
        ////    //        Hero hero = (Hero)target;
        ////    //        if (hero != null && currentMap.PVPType == PvpType.Person && hero.Owner != this)
        ////    //        {
        ////    //            return false;
        ////    //        }
        ////    //        return true;
        ////    //    default:
        ////    //        return true;
        ////    //}
        ////}

        public override void OnDead()
        {
            base.OnDead();
        }

        //开始复活操作
        public override void Revive()
        {
            if (!(currentMap as DungeonMap)?.CanRevive ?? false)
            {
                return;
            }
            IsReviving = true;
            FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
        }

        //复活后事件
        public override void OnRevived()
        {
            base.OnRevived();
            StartFighting();
            NotifyRevived();
            BroadCastRevived();
        }

        private void NotifyRevived()
        {
            MSG_ZGC_REVIVE notify = new MSG_ZGC_REVIVE();
            Write(notify);
        }

        public override long OnHit(FieldObject caster, DamageType damageType, long damage, ref bool immune, float multipleParam = 1)
        {
            damage = base.OnHit(caster, damageType, damage, ref immune, multipleParam);
            if (damage > 0 && caster != null && IsEnemy(caster))
            {
                //int hateValue = (int)(damage * caster.HateRatio + 0.5f);
                //int teamMateHate = hateValue / 3;
                ////调用队友的hatemanager
                //if (!(CurrentMap is ArenaDungeonMap))
                //{
                //    foreach (var hero in CurrentMap.HeroList)
                //    {
                //        if (hero.Value.Owner == this)
                //        {
                //            hero.Value.HateManager?.AddHate(caster, teamMateHate);
                //        }
                //    }
                //}
                SkillManager.AddOnHitBodyEnergy(damage);
            }
            return damage;
        }

        public void ClearAllBattleState()
        {
            // 属性重置
            InBattle = false;
            isDead = false;
            ClearBasicBattleState();

            messageDispatcher = null;
            triggerManager = null;

            FsmManager.SetNextFsmStateType(FsmStateType.IDLE);

            HeroInfo heroInfo = HeroMng.GetHeroInfo(HeroId);
            if (heroInfo != null)
            {
                InitNatureExt(NatureValues, NatureRatios);
                InitNatures(heroInfo);
                //// 速度设置为非战斗状态下的跑动速度
                //SetNatureBaseValue(NatureType.PRO_SPD, heroInfo.GetNatureValue(NatureType.PRO_RUN_OUT_BATTLE));
                //SetNatureBaseValue(NatureType.PRO_MAX_HP, heroInfo.Nature.GetNatureBaseValue(NatureType.PRO_MAX_HP));
                //SetNatureBaseValue(NatureType.PRO_HP, heroInfo.Nature.GetNatureBaseValue(NatureType.PRO_HP));
            }

        }

    }
}

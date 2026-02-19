
using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using Logger;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        protected Natures nature = new Natures();
        public Natures Nature
        { get { return nature; } }

        public Dictionary<int, int> NatureValues = new Dictionary<int, int>();
        public Dictionary<int, int> NatureRatios = new Dictionary<int, int>();

        public virtual void InitNature()
        {
        }

        public void ResetNature()
        {
            nature.Clear();
        }

        public void InitNatureExt(Dictionary<int, int> natureValues, Dictionary<int, int> natureRatios)
        {
            NatureValues = natureValues;
            NatureRatios = natureRatios;
        }

        public void InitNatures(HeroInfo heroInfo)
        {
            if (heroInfo == null)
            {
                return;
            }
            ResetNature();

            foreach (var item in heroInfo.Nature.GetNatureList())
            {
                SetNatureBaseValue(item.Key, item.Value.Value);
            }

            if (heroInfo.GodType > 0)
            {
                //成神属性提升
                HeroGodStepUpGrowthModel detilModel = GodHeroLibrary.GetGodStepUpGrowthModel(heroInfo.GodType, heroInfo.StepsLevel);
                if (detilModel == null)
                {
                    Log.Error($"InitNatures had not find HeroGodStepUpGrowthModel model hero {heroInfo.Id} god {heroInfo.GodType} step {heroInfo.StepsLevel}");
                }
                else
                {
                    foreach (var item in GodHeroLibrary.NatureTypes)
                    {
                        long value = GetNatureBaseValue(item);
                        value += (long)(1 + value * (detilModel.NatureRatio * 0.0001f));
                        SetNatureBaseValue(item, value);
                    }
                }
            }

            if (NatureValues.Count > 0)
            {
                foreach (var item in NatureValues)
                {
                    NatureType type = (NatureType)item.Key;
                    long value = GetNatureBaseValue(type);
                    value += item.Value;
                    SetNatureBaseValue(type, value);
                }
            }

            if (NatureRatios.Count > 0)
            {
                foreach (var item in NatureRatios)
                {
                    NatureType type = (NatureType)item.Key;
                    long value = GetNatureBaseValue(type);
                    value += (long)(1 + value * (item.Value * 0.0001f));
                    SetNatureBaseValue(type, value);
                }
            }
        }

        public void InitNature(HeroInfo heroInfo, bool setHp = true)
        {
            InitNatures(heroInfo);

            //////成长值
            //int groC = heroInfo.GetGroVal();

            //InitBasicHeroNature(heroInfo, groC);

            //AddRatioLsit(natureRatio);

            //AddStepsRatio(heroInfo, nature);

            //最后设置
            if (setHp)
            {
                SetNatureBaseValue(NatureType.PRO_HP, GetNatureValue(NatureType.PRO_MAX_HP));
            }

            //PrintNatures(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<",heroInfo.Id);
        }

        public void PrintNatures(string begin, string end, int heroId)
        {
#if DEBUG
            //Logger.Log.Warn(begin);
            //foreach (var kv in nature.GetNatureList())
            //{
            //    Logger.Log.Debug($"fieldObject {FieldObjectType} heroId {heroId} natureType {kv.Key} value {kv.Value}");
            //}
            //Logger.Log.Warn(end);
#endif
        }

        public void AddRatioLsit(Dictionary<int, int> natureRatio)
        {
            foreach (var ratio in natureRatio)
            {
                AddNatureRatio((NatureType)ratio.Key, ratio.Value);
            }
        }

        //public static void AddStepsRatio(HeroInfo info, Natures nature)
        //{
        //    int sC = 0;
        //    GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(info.StepsLevel);
        //    if (stepsModel != null)
        //    {
        //        sC = stepsModel.StepsC;
        //    }
        //    if (sC > 0)
        //    {
        //        foreach (var type in NatureLibrary.Basic9Nature)
        //        {
        //            nature.AddNatureRatio(type.Key, sC);
        //        }
        //    }
        //}

        public void SetNatureAddedValue(NatureType type, Int64 value)
        {
            //if (NatureLibrary.IsBasic4Nature(type))
            //{
            //    int oldNatureValue = nature.GetNatureBaseValue(type);
            //    nature.SetNatureBaseValue(type, value);
            //    int newNatureValue = nature.GetNatureBaseValue(type);
            //    UpdateNature4to9(type, newNatureValue - oldNatureValue);
            //}
            //else if (type == NatureType.PRO_GRO_VAL)
            //{
            //    UpdateGroNature(value);
            //}
            //else
            //{
            nature.SetNatureAddedValue(type, value);
            //}
        }

        public void SetNatureBaseValue(NatureType type, Int64 value)
        {
            nature.SetNatureBaseValue(type, value);
        }

        public void AddNatureBaseValue(NatureType type, Int64 value)
        {
            nature.AddNatureBaseValue(type, value);
        }

        public void AddNatureRatio(NatureType type, int ratio)
        {
            //// 成长值 基础四项 HP 不支持ratio变化 
            //if (type == NatureType.PRO_GRO_VAL || NatureLibrary.IsBasic4Nature(type) || type == NatureType.PRO_HP)
            //{
            //    return;
            //}
            nature.AddNatureRatio(type, ratio);
        }

        public Int64 GetNatureValue(NatureType type)
        {
            return nature.GetNatureValue(type);
        }

        public Int64 GetNatureBaseValue(NatureType type)
        {
            return nature.GetNatureBaseValue(type);
        }

        public Int64 GetNatureAddedValue(NatureType type)
        {
            return nature.GetNatureAddedValue(type);
        }

        //public int GetNatureRatio(NatureType type)
        //{
        //    return nature.GetNatureRatio(type);
        //}

        // hp更改后与maxHp的校验Nature底层接口，上层无需关注
        public void UpdateHp(DamageType damageType, long hp, FieldObject caster = null)
        {
            if (IsDead || hp == 0 || !InBattle) return;

            if (hp < 0)
            {
                long damage = hp;
                long curHp = GetNatureValue(NatureType.PRO_HP);

                if (-hp >= curHp) //濒死
                {
                    BaseBuff baseBuff = buffManager?.GetOneBuffByType(BuffType.IgnoreAttackLockHP);
                    IgnoreAttackLockHPSomeTimeBuff ignoreAttack = buffManager?.GetOneBuffByType(BuffType.IgnoreAttackLockHPSomeTime) as IgnoreAttackLockHPSomeTimeBuff;

                    if (baseBuff != null) //免疫死亡
                    {
                        //锁血量1
                        damage = -(curHp - 1);
                        DispatchImmnueDeadMsg();
                    }
                    else if (ignoreAttack?.IgnoreDamageStart() == true)
                    {
                        damage = -(curHp - 1);
                        DispatchImmnueDeadMsg();
                    }
                    else if (GetDeadlyHurt)
                    {
                        //锁血量1
                        damage = -(curHp - 1);
                        DispathGetDeadlyHurtMsg();
                        GetDeadlyHurt = false;
                    }
                }

                AddNatureBaseValue(NatureType.PRO_HP, damage);
                AddNatureAddedValue(NatureType.PRO_TOTAL_DAMAGE, damage * -1);

                DispatchDamageMsg(damageType, hp * -1, caster);
                currentMap.RecordBattleDataHurt(caster, this, BattleDataType.Hurt, hp * -1);
            }
            else
            {
                long addHp = hp;

                long maxHP = GetNatureValue(NatureType.PRO_MAX_HP);
                long currHP = GetNatureValue(NatureType.PRO_HP);

                if (hp + currHP > maxHP)
                {
                    addHp = maxHP - currHP;
                }

                if (CastSkillThenDeadSkillId > 0)
                {
                    addHp = 0;
                }
                if (addHp > 0)
                {
                    DispatchAddHpMsg(hp);
                    currentMap.RecordBattleDataCure(caster, this, BattleDataType.Cure, hp);
                    AddNatureBaseValue(NatureType.PRO_HP, addHp);
                }
            }

            CheckDead();
            BroadCastHp();
            if (IsDead)
            {
                fsmManager.SetNextFsmStateType(FsmStateType.DEAD);
            }
        }

        public void CheckDead()
        {
            if (GetNatureValue(NatureType.PRO_HP) <= 0)
            {
                isDead = true;
            }
        }

        private void DispatchDamageMsg(DamageType damageType, Int64 damage, FieldObject caster)
        {
            DispatchMonsterMessageToMap(damage);

            switch (damageType)
            {
                case DamageType.Normal:
                    if (SubcribedMessage(TriggerMessageType.NormalAttackDamage))
                    {
                        DispatchMessage(TriggerMessageType.NormalAttackDamage, new SkillDamageTriMsg(caster, damage, damageType));
                    }
                    if (SubcribedMessage(TriggerMessageType.AnySkillDamage))
                    {
                        DispatchMessage(TriggerMessageType.AnySkillDamage, new SkillDamageTriMsg(caster, damage, damageType));
                    }
                    DispatchDamageMesssage(caster, damageType, damage);
                    break;
                case DamageType.Skill:
                    if (SubcribedMessage(TriggerMessageType.SkillDamage))
                    {
                        DispatchMessage(TriggerMessageType.SkillDamage, new SkillDamageTriMsg(caster, damage, damageType));
                    }
                    if (SubcribedMessage(TriggerMessageType.AnySkillDamage))
                    {
                        DispatchMessage(TriggerMessageType.AnySkillDamage, new SkillDamageTriMsg(caster, damage, damageType));
                    }
                    DispatchDamageMesssage(caster, damageType, damage);
                    break;
                case DamageType.Body:
                    if (SubcribedMessage(TriggerMessageType.BodyDamage))
                    {
                        DispatchMessage(TriggerMessageType.BodyDamage, new BodyDamageMsg(caster, damage));
                    }
                    if (SubcribedMessage(TriggerMessageType.AnySkillDamage))
                    {
                        DispatchMessage(TriggerMessageType.AnySkillDamage, new SkillDamageTriMsg(caster, damage, damageType));
                    }
                    DispatchDamageMesssage(caster, damageType, damage);
                    break;

                default:
                    break;
            }

            //受到伤害通知hero
            DispatchMessageToHero(damageType, damage, caster);
        }

        private void DispatchDamageMesssage(FieldObject caster, DamageType damageType, long damage)
        {
            if (SubcribedMessage(TriggerMessageType.DamageTotal))
            {
                DispatchMessage(TriggerMessageType.DamageTotal, damage);
            }
            if (SubcribedMessage(TriggerMessageType.DamageOnce))
            {
                if (caster != this)
                {
                    DispatchMessage(TriggerMessageType.DamageOnce, new DamageTriMsg(caster, damageType, damage, this));
                }
            }
        }


        private void DispatchMonsterMessageToMap(long damage)
        {
            Monster monster = this as Monster;
            if (monster == null) return;

            currentMap.GetMessageDispatcher()?.Dispatch(TriggerMessageType.MonsterDamage,
                new MonsterDamageTriMsg() { MonsterId = monster.MonsterModel.Id, Damage = damage });
        }

        private void DispatchMessageToHero(DamageType damageType, long damage, FieldObject caster)
        {
            PlayerChar player = this as PlayerChar;
            if (player == null) return;

            List<int> equipedHero = player.HeroMng.GetAllHeroPosHeroId();
            foreach (var heroId in equipedHero)
            {
                Hero hero = player.HeroMng.GetHero(heroId);
                if (hero != null)
                {
                    if (hero.SubcribedMessage(TriggerMessageType.PlayerDamage))
                    {
                        hero.DispatchMessage(TriggerMessageType.PlayerDamage, damage);
                    }
                }
            }
        }

        private void DispatchImmnueDeadMsg()
        {
            if (SubcribedMessage(TriggerMessageType.ImmuneDead))
            {
                DispatchMessage(TriggerMessageType.ImmuneDead, null);
            }
        }

        private void DispatchWillDeadMsg()
        {
            if (SubcribedMessage(TriggerMessageType.WillDead))
            {
                DispatchMessage(TriggerMessageType.WillDead, this);
            }
        }

        private void DispatchAddHpMsg(long hp)
        {
            if (SubcribedMessage(TriggerMessageType.AddHp))
            {
                DispatchMessage(TriggerMessageType.AddHp, hp);
            }
        }      

        private void DispathGetDeadlyHurtMsg()
        {
            if (SubcribedMessage(TriggerMessageType.GetDeadluHurt))
            {
                DispatchMessage(TriggerMessageType.GetDeadluHurt, this);
            }
        }

        public void UpdateShieldHp(long shieldHp)
        {
            if (shieldHp == 0) return;
            AddNatureAddedValue(NatureType.PRO_SHIELD_HP, shieldHp);
            BroadCastHp();
        }

        public long GetHp()
        {
            return GetNatureValue(NatureType.PRO_HP);
        }

        public long GetMaxHp()
        {
            return GetNatureValue(NatureType.PRO_MAX_HP);
        }
        public bool FullHp()
        {
            long hp = GetNatureValue(NatureType.PRO_HP);
            long maxHp = GetNatureValue(NatureType.PRO_MAX_HP);

            return hp >= maxHp;
        }

        public double GetHpRate()
        {
            long hp = GetNatureValue(NatureType.PRO_HP);
            long maxHp = GetNatureValue(NatureType.PRO_MAX_HP);
            return (double)hp / maxHp;
        }

        public float GetHpRatio()
        {
            return GetNatureValue(NatureType.PRO_HP) * 1f / GetNatureValue(NatureType.PRO_MAX_HP);
        }

        public bool HpGreaterThanRate(int rate)
        {
            return GetNatureValue(NatureType.PRO_HP) > (int)(GetNatureValue(NatureType.PRO_MAX_HP) * (rate * 0.0001f));
        }

        public bool HpLessThanRate(int rate)
        {
            return GetNatureValue(NatureType.PRO_HP) < (int)(GetNatureValue(NatureType.PRO_MAX_HP) * (rate * 0.0001f));
        }

        public bool HpEqualOrGreaterThanRate(int rate)
        {
            return GetNatureValue(NatureType.PRO_HP) >= (int)(GetNatureValue(NatureType.PRO_MAX_HP) * (rate * 0.0001f));
        }

        //public void SetMoveSpeed(float speed)
        //{
        //    nature.SetNatureValue(NatureType.PRO_SPD, (int)(speed * 1000));
        //}

        //public float GetMoveSpeed()
        //{
        //    return ((float)nature.PRO_SPD) * 0.001f;
        //}

        public void BroadCastHp()
        {
            MSG_ZGC_CHARACTER_HP msg = new MSG_ZGC_CHARACTER_HP();
            msg.InstanceId = instanceId;
            msg.MaxHp = GetNatureValue(NatureType.PRO_MAX_HP);
            msg.Hp = GetNatureValue(NatureType.PRO_HP);
            msg.ShieldMaxHp = GetNatureValue(NatureType.PRO_SHIELD_MAX_HP);
            msg.ShieldHp = GetNatureValue(NatureType.PRO_SHIELD_HP);
            BroadCast(msg);

            if (this.FieldObjectType == TYPE.MONSTER)
            {
                Logger.Log.Debug("monsterhp--------------------->hp " + msg.Hp + "---maxhp--" + msg.MaxHp + "--shield----" + msg.ShieldHp);
            }
        }

        //5 -> 9
        public void InitBasicHeroNature(HeroInfo heroInfo)
        {
            if (heroInfo == null)
            {
                return;
            }
            //SetNatureBaseValue(NatureType.PRO_GRO_VAL, groVal);
            SetNatureBaseValue(NatureType.PRO_POW, heroInfo.TalentMng.StrengthNum);
            SetNatureBaseValue(NatureType.PRO_CON, heroInfo.TalentMng.PhysicalNum);
            SetNatureBaseValue(NatureType.PRO_EXP, heroInfo.TalentMng.OutburstNum);
            SetNatureBaseValue(NatureType.PRO_AGI, heroInfo.TalentMng.AgilityNum);

        }

        ////天赋属性转基础属性
        //private void UpdateNature4to9(NatureType type, int value)//天赋点数
        //{
        //    Dictionary<NatureType, float> Lsit = NatureLibrary.GetNature9List(type);
        //    if (Lsit != null)
        //    {
        //        int groVal = GetNatureValue(NatureType.PRO_GRO_VAL);

        //        foreach (var nature9 in Lsit)
        //        {
        //            int addValue = (int)(nature9.Value * groVal * value);
        //            AddNatureAddedValue(nature9.Key, addValue);
        //        }
        //    }


        //    //List<NatureType> nature9List = NatureLibrary.GetNature9List(type);
        //    //if (nature9List == null)
        //    //{
        //    //    return;
        //    //}
        //    //foreach (var item in nature9List)//item:atk,hit
        //    //{
        //    //    // old 100
        //    //    int oldNature = CalcNature4to9(item, oldValue);
        //    //    // new 350
        //    //    int newNature = CalcNature4to9(item, newValue);
        //    //    //AddNatureValue(item, new - old)
        //    //    AddNatureValue(item, newNature - oldNature);
        //    //}
        //}

        //计算Y值
        //private int CalcNature4to9(NatureType type, int value, int c = 0)
        //{
        //    if (c == 0)
        //    {
        //        c = GetNatureValue(NatureType.PRO_GRO_VAL);
        //    }

        //    CommonNatureParamModel commonNatureParamModel = NatureLibrary.GetCommonNatureParam();

        //    BasicNatureFactorModel basicNatureFactorModel = NatureLibrary.GetBasicNatureFactor(type);

        //    if (commonNatureParamModel == null || basicNatureFactorModel == null)
        //    {
        //        return -1;
        //    }

        //    float basic = basicNatureFactorModel.B;
        //    float a = basicNatureFactorModel.A;
        //    int lMax = commonNatureParamModel.LMax;
        //    int t = basicNatureFactorModel.T;
        //    float pF = commonNatureParamModel.Pf;
        //    int f = value;
        //    int fMax = commonNatureParamModel.FMax;
        //    float b = commonNatureParamModel.B;
        //    float k = basicNatureFactorModel.K;
        //    int cMax = commonNatureParamModel.CMax;

        //    //Y = (B * Lmax ^ a + T) * Pf * (F / Fmax) ^ b * K * C / Cmax
        //    float y = (basic * (float)Math.Pow(lMax, a) + t) * pF * (float)Math.Pow((f * 1.0) / (fMax * 1.0), b) * k * (float)((c * 1.0) / (cMax * 1.0));
        //    int natureValue = (int)(y + 0.5);
        //    return natureValue;
        //}

        //成长值变化对应基础属性变化
        private void UpdateGroNature(long newGroVal)
        {
            long oldGroVal = GetNatureValue(NatureType.PRO_GRO_VAL);

            if (oldGroVal != newGroVal)
            {
                foreach (var nature4 in NatureLibrary.GetNature4To9List())
                {
                    long nature4Value = GetNatureValue(nature4.Key);
                    foreach (var nature9 in nature4.Value)
                    {
                        long oldValue = (long)(nature9.Value * oldGroVal * nature4Value);
                        long newValue = (long)(nature9.Value * newGroVal * nature4Value);
                        AddNatureAddedValue(nature9.Key, newValue - oldValue);
                    }
                }
            }

            //int pow = GetNatureValue(NatureType.PRO_POW);
            //List<NatureType> powList = NatureLibrary.GetNature9List(NatureType.PRO_POW);
            //foreach (var type in powList)
            //{
            //    int oldValue = CalcNature4to9(type, pow, oldGroVal);
            //    int newValue = CalcNature4to9(type, pow, newGroVal);
            //    AddNatureValue(type, newValue - oldValue);
            //}

            //int con = GetNatureValue(NatureType.PRO_CON);
            //List<NatureType> conList = NatureLibrary.GetNature9List(NatureType.PRO_CON);
            //foreach (var type in conList)
            //{
            //    int oldValue = CalcNature4to9(type, con, oldGroVal);
            //    int newValue = CalcNature4to9(type, con, newGroVal);
            //    AddNatureValue(type, newValue - oldValue);
            //}

            //int exp = GetNatureValue(NatureType.PRO_EXP);
            //List<NatureType> expList = NatureLibrary.GetNature9List(NatureType.PRO_EXP);
            //foreach (var type in expList)
            //{
            //    int oldValue = CalcNature4to9(type, exp, oldGroVal);
            //    int newValue = CalcNature4to9(type, exp, newGroVal);
            //    AddNatureValue(type, newValue - oldValue);
            //}

            //int agi = GetNatureValue(NatureType.PRO_AGI);
            //List<NatureType> agiList = NatureLibrary.GetNature9List(NatureType.PRO_AGI);
            //foreach (var type in agiList)
            //{
            //    int oldValue = CalcNature4to9(type, agi, oldGroVal);
            //    int newValue = CalcNature4to9(type, agi, newGroVal);
            //    AddNatureValue(type, newValue - oldValue);
            //}
        }

        public void AddNatureAddedValue(NatureType type, long deltaValue, bool broadcast = false)
        {
            if (deltaValue == 0)
            {
                return;
            }
            else if (type == NatureType.PRO_HP)
            {
                long hp = GetNatureValue(NatureType.PRO_HP);
                long maxHp = GetNatureValue(NatureType.PRO_MAX_HP);
                if (deltaValue + hp > maxHp)
                {
                    deltaValue = maxHp - hp;
                }
            }
            //int oldValue = GetNatureValue(type);
            //int oldAddedValue = GetNatureAddedValue(type);
            nature.AddNatureAddedValue(type, deltaValue);

            if (broadcast)
            {
                //int curValue = GetNatureValue(type);
                BroadcastUpdateNature(type, deltaValue);
            }
        }

        //public void SetNatureAddedValue(NatureType type, int deltaValue, bool broadcast = false)
        //{
        //    if (deltaValue == 0)
        //    {
        //        return;
        //    }
        //    else if (type == NatureType.PRO_HP)
        //    {
        //        int hp = nature.GetNatureValue(NatureType.PRO_HP);
        //        int maxHp = nature.GetNatureValue(NatureType.PRO_MAX_HP);
        //        if (deltaValue + hp > maxHp)
        //        {
        //            deltaValue = maxHp - hp;
        //        }
        //    }
        //    int oldValue = GetNatureValue(type);
        //    int oldAddedValue = GetNatureAddedValue(type);
        //    SetNatureAddedValue(type, oldAddedValue + deltaValue);

        //    if (broadcast)
        //    {
        //        int curValue = GetNatureValue(type);
        //        BroadcastUpdateNature(type, curValue - oldValue);
        //    }
        //}

        public void AddNatureRatio(NatureType type, int ratio, bool broadcast = false)
        {
            if (ratio == 0) return;

            nature.AddNatureRatio(type, ratio);

            if (broadcast)
            {
                int deltaValue = (int)(GetNatureBaseValue(type) * (ratio * 0.0001f));
                BroadcastUpdateNature(type, deltaValue);
            }
        }

        ////初始化9项基础属性值
        //public void InitBasic9Nature(HeroInfo info)
        //{
        //    InitNatures(info);

        //    //SetNatureAddedValue(NatureType.PRO_MAX_HP, info.GetNatureValue(NatureType.PRO_MAX_HP));
        //    //SetNatureAddedValue(NatureType.PRO_ATK, info.GetNatureValue(NatureType.PRO_ATK));
        //    //SetNatureAddedValue(NatureType.PRO_DEF, info.GetNatureValue(NatureType.PRO_DEF));
        //    //SetNatureAddedValue(NatureType.PRO_HIT, info.GetNatureValue(NatureType.PRO_HIT));
        //    //SetNatureAddedValue(NatureType.PRO_FLEE, info.GetNatureValue(NatureType.PRO_FLEE));
        //    //SetNatureAddedValue(NatureType.PRO_CRI, info.GetNatureValue(NatureType.PRO_CRI));
        //    //SetNatureAddedValue(NatureType.PRO_RES, info.GetNatureValue(NatureType.PRO_RES));
        //    //SetNatureAddedValue(NatureType.PRO_IMP, info.GetNatureValue(NatureType.PRO_IMP));
        //    //SetNatureAddedValue(NatureType.PRO_ARM, info.GetNatureValue(NatureType.PRO_ARM));
        //    //SetNatureAddedValue(NatureType.PRO_RUN_IN_BATTLE, info.GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
        //    //SetNatureAddedValue(NatureType.PRO_RUN_OUT_BATTLE, info.GetNatureValue(NatureType.PRO_RUN_OUT_BATTLE));
        //    //SetNatureAddedValue(NatureType.PRO_SPD, info.GetNatureValue(NatureType.PRO_SPD));
        //}

        public void BroadcastUpdateNature(NatureType type, Int64 value)
        {
            MSG_ZGC_UPDATE_BASIC_NATURE msg = new MSG_ZGC_UPDATE_BASIC_NATURE()
            {
                InstanceId = instanceId,
                NatureType = (int)type,
                NatureValue = value
            };
            BroadCast(msg);
        }

        public void UpdateProSpd(NatureType spdType)
        {
            if (spdType == NatureType.PRO_RUN_IN_BATTLE || spdType == NatureType.PRO_RUN_OUT_BATTLE)
            {
                SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(spdType));
            }
        }

        public int GetRefuseControlledNature(BuffType buffType)
        {
            long value = 0;
            switch (buffType)
            {
                case BuffType.Dizzy:
                    value = GetNatureValue(NatureType.PRO_REFUSE_DIZZY);
                    break;
                case BuffType.Silent:
                    value = GetNatureValue(NatureType.PRO_REFUSE_SILENT);
                    break;
                case BuffType.Fixed:
                    value = GetNatureValue(NatureType.PRO_REFUSE_FIXED);
                    break;
                case BuffType.Disarm:
                    value = GetNatureValue(NatureType.PRO_REFUSE_DISARM);
                    break;
            }
            return (int)value;
        }

        //public static Int64Type GetInt64TypeMsg(double value)
        //{
        //    long newValue = (long)value;
        //    Int64Type value64 = new Int64Type();
        //    value64.High = newValue.GetHigh();
        //    value64.Low = newValue.GetLow();
        //    return value64;
        //}

        public void NaturesAddPetsBonusValue(FieldObject ow, PetInfo petInfo, bool ownerIsRobot)
        {
            if (petInfo == null)
            {
                return;
            }
            int bonusRatio;
            if (!ownerIsRobot)
            {
                PlayerChar pc = ow as PlayerChar;
                bonusRatio = pc.PetManager.GetPetNatureBonusRatio(petInfo);
            }
            else
            {
                Robot robot = ow as Robot;
                bonusRatio = robot.GetPetNatureBonusRatio(petInfo);
            }
            //时空塔副本忽略饱食度对转化比例的影响
            if (CurDungeon.GetMapType() == MapType.SpaceTimeTower)
            {
                bonusRatio = PetLibrary.GetPetAptitudeBonusNatureRatio(petInfo.Aptitude);
            }
            if (bonusRatio == 0)
            {
                return;
            }
            Dictionary<NatureType, NatureItem> petNatures = petInfo.Nature.GetNatureList();
            foreach (var nature in petNatures)
            {
                AddNatureBaseValue(nature.Key, (Int64)(nature.Value.Value * 0.0001f * bonusRatio));
                if (nature.Key == NatureType.PRO_MAX_HP)
                {
                    SetNatureBaseValue(NatureType.PRO_HP, GetNatureValue(NatureType.PRO_MAX_HP));
                }
            }
        }
    }
}

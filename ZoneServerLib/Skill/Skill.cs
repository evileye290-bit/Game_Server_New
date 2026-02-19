using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class Skill
    {
        class CastParam
        {
            public Vec2 DestPos;
            public Vec2 LookDir;
            public int TargetId;
        }

        private FieldObject owner;
        public FieldObject Owner
        { get { return owner; } }
        public Vec2 CastDestPos
        { get { return castParam.DestPos; } }
        public Vec2 CastLookDir
        { get { return castParam.LookDir; } }
        public int CastTargetId
        { get { return castParam.TargetId; } }

        private CastParam castParam;

        private SkillModel skillModel;
        public SkillModel SkillModel
        { get { return skillModel; } }

        private List<SkillEffect> skillEffectList = new List<SkillEffect>();
        public List<SkillEffect> SkillEffectList
        { get { return skillEffectList; } }

        private int level;
        public int Level
        { get { return level; } }

        public int Id { get; private set; }
        public int Priority { get; private set; }
        public int UsedCount { get; private set; }


        private int energy;
        public int Energy
        { get { return energy; } }

        private int energyLimit;

        private int frozenCount;
        public bool InFrozen
        { get { return frozenCount > 0; } }

        public Skill(FieldObject owner, SkillModel model, int level = 1, int usedCount = 0)
        {
            this.owner = owner;
            skillModel = model;
            Id = model.Id;
            Priority = model.Priority;
            this.level = level;
            castParam = new CastParam();
            energyLimit = model.Energy;
            energy = model.InitEnergy;
            UsedCount = usedCount;
            foreach (var effModel in model.SkillEffectModelList)
            {
                SkillEffect eff = new SkillEffect(effModel, level);
                skillEffectList.Add(eff);
            }
        }

        public void InitCastParam(Vec2 lookDir, Vec2 destPos, int targetId)
        {
            castParam.LookDir = lookDir;
            castParam.DestPos = destPos;
            castParam.TargetId = targetId;
        }

        public void AddEnergy(int value, bool needSync = true, bool fromOtherSkill = false, bool dispatchEnergyMsg = false)
        {
            //if (value <= 0 || energy == energyLimit)
            if (value <= 0)
            {
                return;
            }
            energy += value;
            if (energy > energyLimit)
            {
                energy = energyLimit;
            }
            if(energy == energyLimit)
            {
                CheckNeedNotifyOnwer();
            }

            if (needSync)
            {
                SyncSkillEnergy(value, fromOtherSkill);
            }
            CheckNotifyRobotEnergy();

            if (dispatchEnergyMsg)
            {
                owner.CurDungeon?.DispatchBridgeTriggerMessage(owner, TriggerMessageType.EnergyChange, new EnergyChangeMsg(skillModel, owner));
            }
        }

        public void ReduceEnergy(int value, bool needSync = true, bool fromOtherSkill = false)
        {
            if (value < 0 || energy == 0)
            {
                return;
            }
            energy -= value;
            if (energy < 0)
            {
                energy = 0;
            }
            if (needSync)
            {
                SyncSkillEnergy(-value, fromOtherSkill);
            }
        }

        private void SyncSkillEnergy(int addValue = 0, bool fromOtherSkill = false)
        {
            PlayerChar player = null;
            if (owner.FieldObjectType == TYPE.PC)
            {
                player = owner as PlayerChar;
            }
            else if (owner.FieldObjectType == TYPE.HERO)
            {
                Hero hero = owner as Hero;
                if (hero.OwnerIsRobot)
                {
                    Robot r = hero.Owner as Robot;
                    if (r.CopyedFromPlayer)
                    {
                        player = r.playerMirror;
                    }
                }
                else
                {
                    player = hero.Owner as PlayerChar;
                }
            }
            else if (owner.FieldObjectType == TYPE.ROBOT)
            {
                Robot robot = owner as Robot;
                if (robot != null && robot.CopyedFromPlayer)
                {
                    player = robot.playerMirror;
                }
            }

            //向战场广播技能能量变动
            if (fromOtherSkill && skillModel.Type == SkillType.Body)
            {
                BroadCastSkillEnergyChange(addValue);
            }

            if (player == null )
            {
                return;
            }

            MSG_ZGC_SKILL_ENERGY energyMsg = GenerateEnergyMsg(addValue, fromOtherSkill);
            player.Write(energyMsg);
        }

        private void BroadCastSkillEnergyChange(int value)
        {
            MSG_ZGC_SKILL_ENERGY_CHANGE msg = new MSG_ZGC_SKILL_ENERGY_CHANGE();
            msg.InstanceId = owner.InstanceId;
            msg.AddEnergy = value;
            owner.CurDungeon?.BroadCast(msg);
        }

        private void CheckNotifyRobotEnergy()
        {
            if (owner.FieldObjectType != TYPE.ROBOT)
            {
                return;
            }
            if (owner is Robot)
            {
                (owner as Robot).CastSkill(this);//替robot释放
                return;
            }
        }

        // 对于CastedByOwner技能 技能可释放情况下 需要通知
        private void CheckNeedNotifyOnwer()
        {
            if (!skillModel.CastedByOwner)
            {
                return;
            }

            if(energy < energyLimit)
            {
                return;
            }

            Hero hero = owner as Hero;
            if(hero == null || hero.Owner == null)
            {
                return;
            }

            if(owner.SkillEngine.InReadyList(Id))
            {
                return;
            }

            if (hero.OwnerIsRobot)
            {
                owner.SkillEngine.AddSkill(Id, null);
                (hero.Owner as Robot)?.ReleaseSkill(hero.InstanceId, Id);
            }
            
            if(owner.CurDungeon?.IsRealSpeedUp == true)
            {
                //自动战斗相关逻辑
                hero.SkillEngine.AddSkill(Id, null);
            }

            MSG_ZGC_HERO_SKILL_READY notify = new MSG_ZGC_HERO_SKILL_READY();
            notify.HeroId = hero.HeroId;
            notify.SkillId = Id;
            notify.InstanceId = hero.InstanceId;

            if (hero.OwnerIsRobot)
            {
                Robot ro = hero.Owner as Robot;
                if (ro.CopyedFromPlayer)
                {
                    (ro.playerMirror as PlayerChar).Write(notify);
                }
            }
            else
            {
                (hero.Owner as PlayerChar).Write(notify);
            }
        }

        public MSG_ZGC_SKILL_ENERGY GenerateEnergyMsg(int addValue = 0, bool fromOtherSkill = false)
        {
            MSG_ZGC_SKILL_ENERGY energyMsg = new MSG_ZGC_SKILL_ENERGY();
            //energyMsg.InstanceId = owner.InstanceId;
            energyMsg.SkillId = Id;
            energyMsg.Energy = energy;
            energyMsg.EnergyLimit = energyLimit;
            //energyMsg.AddEnergy = addValue;
            //energyMsg.FromSkill = fromOtherSkill;
            switch (owner.FieldObjectType)
            {
                case TYPE.HERO:
                    Hero hero = owner as Hero;
                    energyMsg.HeroId = hero.HeroId;
                    break;
                case TYPE.PC:
                    PlayerChar player = owner as PlayerChar;
                    energyMsg.HeroId = player.HeroId;
                    break;
                case TYPE.ROBOT:
                    Robot robot = owner as Robot;
                    energyMsg.HeroId = robot.HeroId;
                    break;
                default:
                    break;
            }
            return energyMsg;
        }

        public void ResetEnergy()
        {
            energy = 0;

            //hero 释放玩技能通知前端
            if (owner.FieldObjectType == TYPE.HERO && skillModel.CastedByOwner)
            {
                SyncSkillEnergy();
            }
        }

        public void SetEnergyEnough()
        {
            int addV = energyLimit - energy;
            energy = energyLimit;
            SyncSkillEnergy(addV, true);
            CheckNotifyRobotEnergy();
            CheckNeedNotifyOnwer();
        }

        public int GetEnergyLimit()
        {
            return energyLimit;
        }
        public void SetEnergyLimit(int value)
        {
            energyLimit = value;
            if (energyLimit < 0)
            {
                energyLimit = 0;
            }
            if (energy >= energyLimit)
            {
                energy = energyLimit;
                CheckNeedNotifyOnwer();
                CheckNotifyRobotEnergy();
            }
            SyncSkillEnergy();
        }

        public bool IsEnergyEnough()
        {
            return energy >= energyLimit;
        }

        public void LevelUp()
        {
            level++;
            foreach (var eff in skillEffectList)
            {
                eff.SetBasicLevel(level);
            }
        }

        // 是否为普攻
        public bool IsNormalAttack()
        {
            return skillModel.IsNormalAttack();
        }

        public void AddSkillEffect(SkillEffectModel model)
        {
            SkillEffect eff = new SkillEffect(model, level);
            skillEffectList.Add(eff);

            if (skillEffectList.Count > 10)
            {
                Log.Warn($"player {owner.Uid} skill {skillModel.Id} effect count error : effect id {model.Id} current effect count {skillEffectList.Count}");
            }
        }

        public void AddSkillEffect(SkillEffectModel model, int level)
        {
            SkillEffect eff = new SkillEffect(model, level);
            skillEffectList.Add(eff);
        }

        public bool IsBodySkill()
        {
            return skillModel.Type == SkillType.Body; 
        }

        public bool IsBodyAttack()
        {
            return skillModel.Type == SkillType.Body_Attack_1 || skillModel.Type == SkillType.Body_Attack_2
                || skillModel.Type == SkillType.Body_Attack_3;
        }

        public DamageType GetDamageType()
        {
            switch(skillModel.Type)
            {
                case SkillType.Normal_Attack_1:
                case SkillType.Normal_Attack_2:
                case SkillType.Normal_Attack_3:
                    return DamageType.Normal;

                case SkillType.Body:
                case SkillType.Body_Attack_1:
                case SkillType.Body_Attack_2:
                case SkillType.Body_Attack_3:
                    return DamageType.Body;

                default:
                    return DamageType.Skill;
            }
        }

        public void UpdateFrozenCount (int cnt)
        {
            frozenCount += cnt;
        }

        public bool Check()
        {
            if (!IsEnergyEnough())
            {
                return false;
            }
            if(frozenCount > 0)
            {
                return false;
            }
            return true;
        }

        public void AddSkillEffectT(int skillEffectId, float addV)
        {
            skillEffectList.Where(x => x.BasicModel.Id == skillEffectId).FirstOrDefault()?.AddFixT(addV);
        }
    }

    public class SkillDelayInfo
    {
        public string Key;
        public int InstanceId;
        public int SkillId;
        public float Delay = 0f;
    }
}

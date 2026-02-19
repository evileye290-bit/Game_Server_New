using System;
using EnumerateUtility;
using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class Hero : FieldObject
    {
        override public TYPE FieldObjectType
        { get { return TYPE.HERO; } }

        private HeroInfo heroInfo;
        public HeroInfo HeroInfo
        {
            get { return heroInfo; }
        }


        public int HeroId
        {
            get { return HeroInfo.Id; }
        }

        public int Level
        {
            get { return HeroInfo.Level; }
        }

        public bool IsMonsterHero { get; set; }
        public BaseMonsterGen MonGenerator { get; set; }


        private HeroDataModel heroDataModel;

        public HeroDataModel HeroDataModel
        {
            get { return heroDataModel; }
        }

        private FieldObject owner;
        public FieldObject Owner
        {
            get
            {
                return owner;
            }
        }


        public float WalkRange
        { get { return heroDataModel.WalkRange; } }
        public float TransmitDistance
        { get { return heroDataModel.TransmitDistance; } }
        public float FollowDistance
        { get { return heroDataModel.FollowDistance; } }

        public int CollisionPriority { get; set; }

        public bool OwnerIsRobot { get; set; }

        public int MonsterHeroSoulRingCount { get; set; }
        public int MonsterHeroSkillLevel { get; set; }

        public List<int> MonsterHeroYears = new List<int>();


        public Hero(ZoneServerApi server, FieldObject owner, HeroInfo info) : base(server)
        {
            autoAI = true;
            this.owner = owner;
            IsAttacker = owner.IsAttacker;

            currentMap = owner.CurrentMap;

            Vec2 pos = CalcBornPos();
            SetPosition(pos);

            heroInfo = info;

            InitHeroDataModel(heroInfo.Id);
        }

        private void InitHeroDataModel(int heroId)
        {
            heroDataModel = new HeroDataModel();
            HeroModel heroModel = HeroLibrary.GetHeroModel(heroId);
            if (heroModel == null)
            {
                Log.Error("player {0} create hero {1} failed: no such hero model", owner.Uid, heroId);
            }
            else
            {
                heroDataModel.InitHeroModel(heroModel);
                radius = heroDataModel.Radius;
                HateRatio = heroDataModel.HateRatio;
                HateRange = heroDataModel.HateRange;
                HateRefreshTime = heroDataModel.HateRefreshTime;
            }
        }

        public void InitMonsterHero(MonsterHeroModel monsterModel, BaseMonsterGen monGenerator)
        {
            heroDataModel.InitMonsterHeroModel(monsterModel);
            if (heroDataModel.IsHeroModleNull())
            {
                InitHeroDataModel(monsterModel.HeroId);
            }
            IsMonsterHero = true;
            MonGenerator = monGenerator;
            InitMonsterHeroNature(heroInfo);
            InitFSM();
            FsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
        }


        public void Init()
        {
            InitFSM();
            InitNature();
            FsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
        }

        public void InitFromRobot(HeroInfo info)
        {
            heroInfo = info;
            InitFSM();
            InitRobotNature(heroInfo);
            FsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE);
        }



        public void InitMonsterHeroNature(HeroInfo info)
        {
            float growth = currentMap.MonsterGrowth;
            if (currentMap.Model.MapType == MapType.Tower)
            {
                foreach (var item in heroInfo.Nature.GetNatureList())
                {
                    switch (item.Key)
                    {
                        case NatureType.PRO_MAX_HP:
                        case NatureType.PRO_ATK:
                        case NatureType.PRO_DEF:
                        case NatureType.PRO_IMP:
                        case NatureType.PRO_ARM:
                            SetNatureBaseValue(item.Key, (long)(growth * item.Value.Value));
                            break;
                        default:
                            SetNatureBaseValue(item.Key, item.Value.Value);
                            break;
                    }
                }
            }
            else
            {
                foreach (var item in heroInfo.Nature.GetNatureList())
                {
                    if (NatureLibrary.Basic9Nature.ContainsKey(item.Key))
                    {
                        SetNatureBaseValue(item.Key, (long)(growth * item.Value.Value));
                    }
                }
            }
            SetNatureBaseValue(NatureType.PRO_HP, GetMaxHp());
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, heroDataModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
            SetNatureBaseValue(NatureType.PRO_MUL_CRI, info.GetNatureValue(NatureType.PRO_MUL_CRI));
        }

        public void InitRobotNature(HeroInfo info)
        {
            InitNatures(info);
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, heroDataModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
        }


        public JobType GetJobType()
        {
            return (JobType)heroDataModel.JobType;
        }

        protected override void OnUpdate(float deltaTime)
        {
            //不在统一地图或者同一线

            if (currentMap != null && Owner.CurrentMap != null && (Owner.CurrentMap.MapId != CurrentMap.MapId || Owner.CurrentMap.Channel != CurrentMap.Channel))
            {
                CurrentMap.RemoveHero(InstanceId);
                return;
            }
        }

        private Vec2 CalcBornPos()
        {
            Vec2 ownerPos = Owner.Position;
            Vec2 bornPos = new Vec2();
            for (int i = 0; i < 3; i++)
            {
                Vec2 vec = Vec2.GetRandomPos(ownerPos, 2);
                if (Vec2.DisPower(ownerPos, vec) < 1.2f)
                {
                    bornPos.x = vec.x;
                    bornPos.y = vec.y;
                    return bornPos;
                }
            }

            bornPos.x = Owner.Position.x + RAND.Range(-1, 1);
            bornPos.y = Owner.Position.y + RAND.Range(-1, 1);
            return bornPos;
        }

        public void GetSimpleInfo(MSG_ZGC_HERO_SIMPLE_INFO info)
        {
            if (info == null) return;
            info.InstanceId = instanceId;
            info.HeroId = HeroId;
            info.Level = Level;
            info.PosX = Position.X;
            info.PosY = Position.Y;
            info.HP = GetHp().ToInt64TypeMsg();
            info.MaxHP = GetMaxHp().ToInt64TypeMsg();
            info.OwnerUid = Owner.Uid;
            info.InRealBody = InRealBody;
            info.HeroPos = -1;
            info.SoulSkillLevel = heroInfo.SoulSkillLevel;
            info.GodType = heroInfo.GodType;

            if (OwnerIsRobot)
            {
                Robot ow = (owner as Robot);
                if (ow.heroPoses.ContainsKey(HeroId))
                {
                    info.HeroPos = ow.heroPoses[HeroId];
                }
            }
            else
            {
                PlayerChar ow = (owner as PlayerChar);
                info.HeroPos = ow.GetHeroPos(HeroId);
            }

            // 移动中 则同步移动信息
            if (IsMoving)
            {
                info.DestX = MoveHandler.MoveToPosition.x;
                info.DestY = MoveHandler.MoveToPosition.y;
                info.Speed = MoveHandler.MoveSpeed;
                info.Speed = GetNatureValue(NatureType.PRO_SPD) * 0.0001f;
            }
            info.AwakenLevel = heroInfo.AwakenLevel;
            info.EquipIndex = heroInfo.EquipIndex;
            info.StepsLevel = heroInfo.StepsLevel;

            if (currentMap != null && currentMap.AoiType == AOIType.All)
            {
                if (OwnerIsRobot)
                {
                    Robot ow = owner as Robot;

                    if (IsMonsterHero)
                    {
                        info.SoulRingYears.AddRange(MonsterHeroYears);
                        info.MonsterHero = heroDataModel.MonsterHeroId;
                    }
                    else
                    {
                        string[] soulRingInfo = StringSplit.GetArray("|", HeroInfo.RobotInfo.SoulRings);
                        //有魂环
                        foreach (var temp in soulRingInfo)
                        {
                            try
                            {
                                string[] tempInfo = StringSplit.GetArray(":", temp);
                                int year = int.Parse(tempInfo[3]);
                                info.SoulRingYears.Add(year);
                            }
                            catch (Exception e)
                            {
                                //没找到魂环信息
                                Log.WarnLine("player {0} get GetSimpleInfo hero info fail,can not find SoulRings {1}, {2}.", Uid, HeroInfo.RobotInfo.SoulRings, e);
                            }
                        }
                    }
                }
                else
                {
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

                                soulRing = (owner as PlayerChar)?.SoulRingManager?.GetSoulRing(heroDataModel.HeroId, skillModel.SoulRingPos);
                                if (soulRing != null)
                                {
                                    info.SoulRingYears.Add(soulRing.Year);
                                }

                            }
                        }
                }
            }
        }

        public bool InTransmitDis()
        {
            return Vec2.InRange(Position, Owner.Position, TransmitDistance);
        }

        public bool InFollowDis()
        {
            return Vec2.InRange(Position, Owner.Position, FollowDistance);
        }

        public bool InWalkDis()
        {
            return Vec2.InRange(Position, Owner.Position, WalkRange);
        }

        public override void BroadcastSimpleInfo()
        {
            MSG_ZGC_HERO_SIMPLE_INFO info = new MSG_ZGC_HERO_SIMPLE_INFO();
            GetSimpleInfo(info);
            BroadCast(info);
        }

        public override FieldObject GetOwner()
        {
            return owner;
        }

        public override void ResetRadius()
        {
            radius = heroDataModel.Radius;
        }
    }
}

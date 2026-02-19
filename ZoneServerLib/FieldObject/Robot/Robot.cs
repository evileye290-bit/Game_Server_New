using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    //不考虑机器人分配和入场等相关内容
    #region 机器人FieldObject要点 
    /*
     * 1 初始化的信息，需要放到map中的，uid等
     * 1.1 初始化要点 魂环以及等级 自己的heroInfo 伙伴 自己的属性
     * 2 对于fieledobject类型的分类处理 都需要多一个robot
     * 3 AOI 要假装成玩家进行通知
     * 4 是否在玩家全部离线后回收地图
     * 5 状态机
     * 6 结算时不要考虑机器人
     * 7 复活处理/// 考虑竞技场挑战不复活？
     * 8 关于其中的hero，只有hero引用robot，robot暂时不知道自己的hero，hero自行管理
     * 9 仇恨溅射
     * 10 主动技能直接释放
     **/
    #endregion
    public partial class Robot : FieldObject
    {
        private int ownerUid;
        private HeroInfo heroInfo;


        public bool CopyedFromPlayer;
        public PlayerChar playerMirror;
        public RobotHeroInfo robotInfo;

        public bool hateIndexd = false;
        public List<HeroInfo> heroInfos = null;
        public Dictionary<int, int> heroPoses = new Dictionary<int, int>();
        public Dictionary<int, int> hateIndexed = new Dictionary<int, int>();

        private Dictionary<int, Hero> heros = new Dictionary<int, Hero>();
        public Dictionary<int, List<BattleSoulBoneInfo>> HeroSoulBones = new Dictionary<int, List<BattleSoulBoneInfo>>();
        public Dictionary<int, SortedDictionary<int, BattleSoulRingInfo>> HeroSoulRings = new Dictionary<int, SortedDictionary<int, BattleSoulRingInfo>>();
        public Dictionary<int, BattleHiddenWeaponInfo> HiddenWeapons = new Dictionary<int, BattleHiddenWeaponInfo>();//暗器
        public Dictionary<int, List<int>> EquipmentList = new Dictionary<int, List<int>>();

        public PetInfo RobPetInfo = null;
        private Dictionary<ulong, Pet> pets = new Dictionary<ulong, Pet>();


        public int HeroId { get; set; }
        public HeroModel HeroModel { get; set; }
        public SoulRingManager SoulRingManager { get; set; }//debug用


        public HeroInfo Info => heroInfo;

        override public TYPE FieldObjectType => TYPE.ROBOT;

        public FieldObject Owner
        {
            get
            {
                if (CopyedFromPlayer) return playerMirror;
                return null;
            }
        }


        public Robot(ZoneServerApi server, int uid) : base(server)
        {
            autoAI = true;
            SetUid(uid);//从别处产生，最好能够区分，如果别的地方没有判断负数的处理，可以使用负数
            skillDelayTime = DataListManager.inst.GetData("RobotConfig", 1).GetFloat("SkillDelay");

            InitBaseBattleInfo();
        }

        public override void InitAI()
        {
            //hateManager = new HeroHateManager(this, HeroModel.HateRange, HeroModel.HateRefreshTime);

            //BindTriggers();
            //BindSoulRingSkills();
            //PassiveSkillEffect();
        }

        private void BindTriggers()
        {
            //foreach (var triggerId in HeroModel.Triggers)
            //{
            //    BaseTrigger trigger = new TriggerInHero(this, triggerId);
            //    AddTrigger(trigger);
            //}

            ////此处拿出额外的heromodel的全部trigger
            //HeroModel model = HeroLibrary.GetHeroModel(HeroId);
            //foreach (var triggerId in model.Triggers)
            //{
            //    BaseTrigger trigger = new TriggerInHero(this, triggerId);
            //    AddTrigger(trigger);
            //}
        }

       

        public void InitRobot(int heroId, HeroInfo info)
        {
            //model info
            HeroId = heroId;
            heroInfo = info;
            robotInfo = info.RobotInfo;
            HeroModel = HeroLibrary.GetHeroModel(HeroId);
            HateRatio = HeroModel.HateRatio;

            InitNature();
            RobotHeroInfo rInfo = info.RobotInfo;
            int addYearRatio = SoulRingManager.GetAddYearRatio(heroInfo.StepsLevel);
            string[] soulRingInfo = rInfo.SoulRings.Split('|');

            SortedDictionary<int, BattleSoulRingInfo> soulRingList = new SortedDictionary<int, BattleSoulRingInfo>();

            if (soulRingInfo.Count() > 0)
            {
                foreach (string temp in soulRingInfo)
                {
                    if (string.IsNullOrEmpty(temp)) continue;

                    string[] temps = temp.Split(':');
                    if (temps.Count() != 5)
                    {
                        Log.WarnLine("InitRobot error param num not enough " + temp);
                        continue;
                    }
                    int pos = int.Parse(temps[0]);
                    int level = int.Parse(temps[1]);
                    int spec = int.Parse(temps[2]);
                    int year = int.Parse(temps[3]);
                    int element = int.Parse(temps[4]);
                    int currentYear = SoulRingManager.GetAffterAddYear(year, addYearRatio);
                    soulRingList[pos] = new BattleSoulRingInfo(pos, level, currentYear, spec, element);
                }
                HeroSoulRings[HeroId] = soulRingList;
            }

            List<BattleSoulBoneInfo> soulBoneIds = new List<BattleSoulBoneInfo>();
            string[] soulBoneInfo = rInfo.SoulBones.Split('|');
            if (soulBoneInfo.Count() > 0)
            {
                foreach (string temp in soulBoneInfo)
                {
                    if (string.IsNullOrEmpty(temp)) continue;

                    List<int> soulBoneAttr = temp.ToList(':');
                    if (soulBoneAttr.Count < 1) continue;

                    int typeId = soulBoneAttr[0];
                    soulBoneAttr.RemoveAt(0);

                    soulBoneIds.Add(new BattleSoulBoneInfo(typeId, soulBoneAttr));
                }
                HeroSoulBones[HeroId] = soulBoneIds;
            }

            //暗器
            List<int> weaponInfos = rInfo.HiddenWeapon.ToList(':');
            //魂骨
            if (weaponInfos.Count == 2)
            {
                BattleHiddenWeaponInfo weaponInfo = new BattleHiddenWeaponInfo(weaponInfos[0], weaponInfos[1]);
                HiddenWeapons[HeroId] = weaponInfo;
            }

            //装备(套装)
            EquipmentList[HeroId] = rInfo.Equipment.ToList('|');
        }

        public void RobotHerosStartFighting()
        {
            (CurrentMap as DungeonMap).StartRobotHeros(this);
        }

        private void UpdateSpeed(Hero hero)
        {
            //更新速度
            HeroModel = hero.HeroDataModel.hero;
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, HeroModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
        }

        public Hero NewHero(ZoneServerApi server, FieldObject owner, HeroInfo info)
        {
            Hero hero = new Hero(server, owner, info);
            hero.OwnerIsRobot = true;
            hero.InitNatureExt(NatureValues, NatureRatios);
            hero.Init();
            return hero;
        }

        public static Robot CopyFromPlayer(ZoneServerApi server, PlayerChar player)
        {
            Robot robot = new Robot(server, player.Uid);
            robot.CopyFromPlayer(player);
            robot.SetOwnerUid(player.Uid);
            return robot;
        }

        private void CopyFromPlayer(PlayerChar player)
        {
            //copy model info
            HeroId = player.HeroId;
            heroInfo = player.HeroMng.GetPlayerHeroInfo();
            HeroModel = HeroLibrary.GetHeroModel(player.HeroId);
            IsAttacker = player.IsAttacker;
            this.playerMirror = player;
            CopyedFromPlayer = true;

            InitNatureExt(player.NatureValues, player.NatureRatios);

            InitNature(heroInfo);

            // 伙伴  在更上层

            //InitAI 初始化ai相关 会在start时添加
            skillManager = new SkillManager(this);
            buffManager = new BuffManager(this);
            skillEngine = new SkillEngine(this);
            markManager = new MarkManager(this);
            InitTrigger();
            BindTriggers();
            CopyBindSkill(player);
            //soulring 
            CopySoulRingSkill(player);
            //DebugCopySoulRingSkill(player);

            //todo 仇恨manager 需要新的manager，给机器人使用
            HeroModel heroModel = HeroLibrary.GetHeroModel(player.HeroId);
            hateManager = new HeroHateManager(this, heroModel.HateRange, heroModel.HateRefreshTime); //由于不会被溅射仇恨，所以可能不知道该打谁，从而不战斗

            //更新速度
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, HeroModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, HeroModel.PRO_RUN_IN_BATTLE);

            //PassiveSkillEffect();

            //foreach(var kv in player.HeroMng.GetAllHeroPos())
            //{
            //    heroPoses[kv.Item1] = kv.Item2;
            //}

        }


        public void CopyHeros2Map(PlayerChar player)
        {
            List<HeroInfo> infos = new List<HeroInfo>();

            SoulRingManager = player.SoulRingManager.Clone();

            foreach (var kv in player.HeroMng.GetAllHeroPos())//.Where(kv => kv.Key != 1))
            {
                if (!heroPoses.ContainsKey(kv.Item1))
                {
                    heroPoses.Add(kv.Item1, kv.Item2);
                }
                HeroInfo info = player.HeroMng.GetHeroInfo(kv.Item1).Clone();//Todo:检查一下
                info.GodType = player.HeroGodManager.GetHeroGodType(kv.Item1);
                info.RobotInfo = new RobotHeroInfo();

                Dictionary<int, SoulRingItem> soulRing = player.SoulRingManager.GetAllEquipedSoulRings(kv.Item1);
                if (soulRing != null)
                {
                    foreach (var curr in soulRing)
                    {
                        info.RobotInfo.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position, curr.Value.Level, curr.Value.SpecId, curr.Value.Year, curr.Value.Element);
                    }
                }

                List<SoulBone> soulBoneList = player.SoulboneMng.GetEnhancedHeroBones(kv.Item1);
                if (soulBoneList != null)
                {
                    List<string> soulBoneStr = soulBoneList.ToList().ConvertAll(x =>
                    {
                        List<int> specList = x.GetSpecList();
                        return specList.Count <= 0 ? x.TypeId.ToString() : x.TypeId.ToString() + ":" + string.Join(":", specList);
                    });
                    info.RobotInfo.SoulBones = string.Join("|", soulBoneStr);
                }

                HiddenWeaponItem weaponItem = player.HiddenWeaponManager.GetHeroEquipedHiddenWeapon(kv.Item1);
                if (weaponItem != null)
                {
                    info.RobotInfo.HiddenWeapon = $"{weaponItem.Id}:{weaponItem.Info.Star}";
                    HiddenWeapons[kv.Item1] = new BattleHiddenWeaponInfo(weaponItem.Id, weaponItem.Info.Star);
                }

                List<EquipmentItem> equipmentItems = player.EquipmentManager.GetAllEquipedEquipments(kv.Item1);
                if (equipmentItems.Count > 0)
                {
                    var temp = equipmentItems.Select(x => x.Id).ToList();
                    info.RobotInfo.Equipment = string.Join("|", temp);
                    EquipmentList[kv.Item1] = new List<int>(temp);
                }

                infos.Add(info);
            }

            heroInfos = infos;

            foreach (var kv in infos)
            {
                Hero hero = NewHero(player.server, this, kv);
                hero.IsAttacker = this.IsAttacker;
                hero.InitNatureExt(NatureValues, NatureRatios);
                hero.Init();
                heros.Add(hero.HeroId, hero);
                AddHeroSoulRingInfo(kv);
                AddHeroSoulBoneInfo(kv);
                UpdateSpeed(hero);
                CurrentMap.CreateHero(hero);
            }
        }

        public void CopyHeros2CrossMap(PlayerChar player)
        {
            List<HeroInfo> infos = new List<HeroInfo>();

            SoulRingManager = player.SoulRingManager.Clone();

            foreach (var hero in heroInfos)//.Where(kv => kv.Key != 1))
            {
                HeroInfo info = hero.Clone();//Todo:检查一下
                info.GodType = player.HeroGodManager.GetHeroGodType(hero.Id);
                info.RobotInfo = new RobotHeroInfo();
                info.CrossBossQueueNum = hero.CrossBossQueueNum;
                info.CrossBossPositionNum = hero.CrossBossPositionNum;

                Dictionary<int, SoulRingItem> soulRing = player.SoulRingManager.GetAllEquipedSoulRings(hero.Id);
                if (soulRing != null)
                {
                    foreach (var curr in soulRing)
                    {
                        info.RobotInfo.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position,
                            curr.Value.Level, curr.Value.SpecId, curr.Value.Year, curr.Value.Element);
                    }
                }

                List<SoulBone> soulBoneList = player.SoulboneMng.GetEnhancedHeroBones(hero.Id);
                if (soulBoneList != null)
                {
                    List<string> soulBoneStr = soulBoneList.ToList().ConvertAll(x =>
                    {
                        List<int> specList = x.GetSpecList();
                        return specList.Count <= 0 ? x.TypeId.ToString() : x.TypeId.ToString() + ":" + string.Join(":", specList);
                    });
                    info.RobotInfo.SoulBones = string.Join("|", soulBoneStr);
                }

                HiddenWeaponItem weaponItem = player.HiddenWeaponManager.GetHeroEquipedHiddenWeapon(hero.Id);
                if (weaponItem != null)
                {
                    info.RobotInfo.HiddenWeapon = $"{weaponItem.Id}:{weaponItem.Info.Star}";
                    HiddenWeapons[hero.Id] = new BattleHiddenWeaponInfo(weaponItem.Id, weaponItem.Info.Star);
                }

                List<EquipmentItem> equipmentItems = player.EquipmentManager.GetAllEquipedEquipments(hero.Id);
                if (equipmentItems.Count > 0)
                {
                    var temp = equipmentItems.Select(x => x.Id).ToList();
                    info.RobotInfo.Equipment = string.Join("|", temp);
                    EquipmentList[hero.Id] = new List<int>(temp);
                }

                infos.Add(info);
            }
            heroInfos = infos;
            heros.Clear();
            foreach (var kv in infos)
            {
                Hero hero = NewHero(player.server, this, kv);
                hero.IsAttacker = IsAttacker;
                hero.InitNatureExt(NatureValues, NatureRatios);
                hero.Init();
                heros.Add(hero.HeroId, hero);
                AddHeroSoulRingInfo(kv);
                AddHeroSoulBoneInfo(kv);
                UpdateSpeed(hero);
                CurrentMap.CreateHero(hero);
            }
        }

        public void CreateRobotHeros(List<HeroInfo> heroInfos)
        {
            //HeroInfo temp = heroInfos.First();
            //for (int i = 0; i < heroInfos.Count; i++) //foreach (var info in heroInfos.Where(hero => hero.Id > 20))
            foreach (var info in heroInfos)
            {
                //HeroInfo info = heroInfos[i].Clone();
                Hero hero = NewHero(server, this, info);
                hero.InitNatureExt(NatureValues, NatureRatios);
                hero.Init();
                hero.InitNatureExt(NatureValues, NatureRatios);
                hero.InitFromRobot(info);
                if (!heros.ContainsKey(hero.HeroId))
                {
                    if (!string.IsNullOrEmpty(info.RobotInfo.HiddenWeapon))
                    {
                        List<int> weaponAtt = info.RobotInfo.HiddenWeapon.ToList(':');
                        if (weaponAtt.Count == 2)
                        {
                            HiddenWeapons[info.Id] = new BattleHiddenWeaponInfo(weaponAtt[0], weaponAtt[1]);
                        }
                    }

                    EquipmentList[info.Id] = info.RobotInfo.Equipment.ToList('|');

                    heros[hero.HeroId] = hero;

                    AddHeroSoulRingInfo(info);
                    AddHeroSoulBoneInfo(info);
                    UpdateSpeed(hero);
                    CurrentMap.CreateHero(hero);
                }
                else
                {
                    Log.WarnLine($"player {Owner?.Uid} CreateRobotHeros error : same hero {hero.HeroId} ");
                }
            }
        }

        private void AddHeroSoulRingInfo(HeroInfo info)
        {
            RobotHeroInfo rInfo = info.RobotInfo;
            int addYearRatio = SoulRingManager.GetAddYearRatio(info.StepsLevel);
            SortedDictionary<int, BattleSoulRingInfo> soulRingPos = new SortedDictionary<int, BattleSoulRingInfo>();
            string[] soulRingInfo = rInfo.SoulRings.Split('|');
            if (soulRingInfo.Count() > 0)
            {
                foreach (string temp in soulRingInfo)
                {
                    if (string.IsNullOrEmpty(temp)) continue;

                    string[] temps = temp.Split(':');
                    if (temps.Count() != 5)
                    {
                        Log.WarnLine("InitRobot error param num not enough " + temp);
                        continue;
                    }
                    int pos = int.Parse(temps[0]);
                    int level = int.Parse(temps[1]);
                    int spec = int.Parse(temps[2]);
                    int year = int.Parse(temps[3]);
                    int element = int.Parse(temps[4]);
                    int currentYear = SoulRingManager.GetAffterAddYear(year, addYearRatio);
                    BattleSoulRingInfo ringInfo = new BattleSoulRingInfo(pos, level, currentYear, spec, element);
                    soulRingPos[pos] = ringInfo;
                }
            }
            HeroSoulRings[info.Id] = soulRingPos;

            //暗器
            List<int> weaponInfos = rInfo.HiddenWeapon.ToList(':');
            //魂骨
            if (weaponInfos.Count == 2)
            {
                BattleHiddenWeaponInfo weaponInfo = new BattleHiddenWeaponInfo(weaponInfos[0], weaponInfos[1]);
                HiddenWeapons[HeroId] = weaponInfo;
            }

            EquipmentList[info.Id] = rInfo.Equipment.ToList('|');
        }

        private void AddHeroSoulBoneInfo(HeroInfo info)
        {
            RobotHeroInfo rInfo = info.RobotInfo;
            List<BattleSoulBoneInfo> soulBonePos = new List<BattleSoulBoneInfo>();
            string[] soulBoneInfo = rInfo.SoulBones.Split('|');
            if (soulBoneInfo.Count() > 0)
            {
                foreach (string temp in soulBoneInfo)
                {
                    List<int> soulBoneAttr = temp.ToList(':');
                    if (soulBoneAttr.Count < 1) continue;

                    int typeId = soulBoneAttr[0];
                    soulBoneAttr.RemoveAt(0);

                    soulBonePos.Add(new BattleSoulBoneInfo(typeId, soulBoneAttr));
                }
            }
            HeroSoulBones[info.Id] = soulBonePos;
        }

        public void SetOwnerUid(int ownerUid)
        {
            this.ownerUid = ownerUid;
        }
        public int GetOwnerUid()
        {
            return CopyedFromPlayer ? playerMirror.Uid : ownerUid;
        }

        public void EnterMap(FieldMap map)
        {
            SetCurrentMap(map);
            SetInstanceId(map.TokenId);

            DungeonMap dmap = map as DungeonMap;
            dmap.RecordFieldObjectEnter(this);

            int i = 1;
            if (dmap.Model.MapType == MapType.CrossBattle ||
                dmap.Model.MapType == MapType.CrossFinals || 
                dmap.Model.MapType == MapType.CrossBossSite||
                dmap.Model.MapType == MapType.CrossChallenge ||
                dmap.Model.MapType == MapType.CrossChallengeFinals)
            {
                if (IsAttacker)
                {
                    i = dmap.AttackerPosIndex;
                }
                else
                {
                    i = dmap.DefenderPosIndex;
                }
            }
  
            SetPosition(dmap.CalcBeginPos(i, this));//更改进图时位置
            //记录进副本时候的人数避免计算伙伴坐标错误

            MoveHandler.MoveStop();

            AddToAoi();
        }

        private void CopySoulRingSkill(PlayerChar player)
        {
            //更新技能
            HeroInfo playerHeroInfo = player.HeroMng.GetPlayerHeroInfo();
            if (playerHeroInfo == null)
            {
                return;
            }

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(player.HeroId);
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
                    SoulRingItem soulRing = player.SoulRingManager.GetSoulRing(heroModel.Id, skillModel.SoulRingPos);
                    if (soulRing == null)
                    {
                        // 未装备该魂环
                        continue;
                    }
                    int skillLevel = soulRing.Level / 10 + 1;
                    // 针对100级魂环，技能等级也为10级
                    skillLevel = skillLevel > 10 ? 10 : skillLevel;
                    SkillManager.Add(skillId, skillLevel);
                    int currentYear = SoulRingManager.GetAffterAddYear(soulRing.Year, addYearRatio);
                    soulRingSpecList.Add(new BattleSoulRingInfo(skillModel.SoulRingPos, soulRing.Level, currentYear, soulRing.SpecId, soulRing.Element));
                }
            }
            SoulRingSpecUtil.DoEffect(soulRingSpecList, this);

            PrintNatures(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", 0);
        }

        private void DebugCopySoulRingSkill(PlayerChar player)
        {
            //更新技能
            HeroInfo playerHeroInfo = player.HeroMng.GetPlayerHeroInfo();
            if (playerHeroInfo == null)
            {
                return;
            }

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(player.HeroId);
            if (heroModel == null)
            {
                return;
            }
            int addYearRatio = SoulRingManager.GetAddYearRatio(playerHeroInfo.StepsLevel);
            List<BattleSoulRingInfo> soulRingSpecList = new List<BattleSoulRingInfo>();
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
                    SoulRingItem soulRing = player.SoulRingManager.GetSoulRing(heroModel.Id, skillModel.SoulRingPos);
                    if (soulRing == null)
                    {
                        // 未装备该魂环
                        continue;
                    }
                    int skillLevel = 2;
                    // 针对100级魂环，技能等级也为10级
                    skillLevel = skillLevel > 10 ? 10 : skillLevel;
                    SkillManager.Add(skillId, skillLevel);
                    int currentYear = SoulRingManager.GetAffterAddYear(soulRing.Year, addYearRatio);
                    soulRingSpecList.Add(new BattleSoulRingInfo(skillModel.SoulRingPos, soulRing.Level, currentYear, soulRing.SpecId, soulRing.Element));
                }
            }
            // 魂环特殊效果起效
            SoulRingSpecUtil.DoEffect(soulRingSpecList, this);
        }

        private void CopyBindSkill(PlayerChar player)
        {
            HeroInfo playerHeroInfo = player.HeroMng.GetPlayerHeroInfo();
            if (playerHeroInfo == null)
            {
                return;
            }

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(player.HeroId);
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
                SkillManager.Add(skillId, 1);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            //更新技能和绑定的人，主动技释放
            UpdateSkillRelease(deltaTime);
        }

        public void GetSimpleInfo(MSG_GC_CHAR_SIMPLE_INFO info)
        {
            info.InstanceId = InstanceId;

            //info.Level = heroInfo.Level;
            info.HeroId = HeroId;
            info.PosX = Position.x;
            info.PosY = Position.y;
            info.Angle = GenAngle;

            info.Uid = Uid;

            info.Hp = GetHp();
            info.MaxHp = GetMaxHp();
            if (CopyedFromPlayer)
            {
                info.Name = playerMirror.Name;
                info.Sex = playerMirror.Sex;
                info.Title = playerMirror.TitleMng.CurTitleId;
                info.Level = playerMirror.Level;
                info.GodType = playerMirror.GodType;
                info.Model = playerMirror.BagManager.FashionBag.GetModel();
            }
            else
            {
                info.Name = robotInfo.Name;
                info.Sex = robotInfo.Sex;
                info.Title = robotInfo.CurTitleId;
                info.Model = new MODEL_INFO();//todo 添加模型信息
            }

            info.InRealBody = InRealBody;
            if (IsMoving)
            {
                // 移动中 需要添加移动相关信息
                info.DestX = MoveHandler.MoveToPosition.x;
                info.DestY = MoveHandler.MoveToPosition.y;
                info.Speed = MoveHandler.MoveSpeed;
            }

            if (Info != null)
            {
                info.AwakenLevel = Info.AwakenLevel;
            }
        }

        public override void BroadcastSimpleInfo()
        {
            MSG_GC_CHAR_SIMPLE_INFO info = new MSG_GC_CHAR_SIMPLE_INFO();
            GetSimpleInfo(info);
            BroadCast(info);
        }

        public void SetHeroPoses(Dictionary<int, int> heroPos)
        {
            heroPoses = heroPos;
        }


        public Vec2 GetHeroPosPosition(int heroId)
        {
            if (heroPoses != null && heroPoses.ContainsKey(heroId))
            {
                return HeroLibrary.GetHeroPos(heroPoses[heroId]);
            }
            return null;
        }

        public int GetHeroPos(int heroId)
        {
            if (heroPoses != null && heroPoses.ContainsKey(heroId))
            {
                return heroPoses[heroId];
            }
            return 0;
        }

        public void SetHeroInfos(List<HeroInfo> infos)
        {
            heroInfos = infos;
            if (!CopyedFromPlayer)
            {
                CreateRobotHeros(heroInfos);
            }
        }

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
                foreach (var item in heroInfos)
                {
                    //if (item.Id > 20)//假如是非主角
                    {
                        ids.Add(item.Id);
                    }
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

        public override FieldObject GetOwner()
        {
            return Owner;
        }

        public List<BattleSoulBoneInfo> GetHeroSoulBones(int heroId)
        {
            List<BattleSoulBoneInfo> list;
            HeroSoulBones.TryGetValue(heroId, out list);
            return list;
        }

        public BattleHiddenWeaponInfo GetHiddenWeaponInfo(int heroId)
        {
            BattleHiddenWeaponInfo info;
            HiddenWeapons.TryGetValue(heroId, out info);
            return info;
        }

        public void CopyPet2Map(PlayerChar player)
        {
            ulong petUid = player.PetManager.OnFightPet;
            if (petUid == 0)
            {
                return;
            }
            PetInfo petInfo = player.PetManager.GetPetInfo(petUid);
            if (petInfo == null)
            {
                return;
            }
            PetInfo temp = petInfo.Clone();
            PetModel petModel = PetLibrary.GetPetModel(temp.PetId);
            if (petModel == null)
            {
                return;
            }
            RobPetInfo = temp;
            Pet pet = NewPet(player.server, this, temp, petModel);
            pet.IsAttacker = IsAttacker;
            //pet.InitNatureExt(NatureValues, NatureRatios);
            //UpdateSpeed(pet);
            CurrentMap.CreatePet(pet);
        }

        public Pet NewPet(ZoneServerApi server, FieldObject owner, PetInfo info, PetModel petModel, int queueNum = 1)
        {
            Pet pet = new Pet(server, owner, info, petModel, queueNum);
            //pet.InitNatureExt(NatureValues, NatureRatios);
            pet.Init();
            return pet;
        }

        public int GetPetNatureBonusRatio(PetInfo petInfo)
        {
            int ratio = PetLibrary.GetPetAptitudeBonusNatureRatio(petInfo.Aptitude);
            int curSatiety = CheckUpdateSatiety(petInfo, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now));
            PetSatietyModel satietyModel = PetLibrary.GetPetSatietyModel(curSatiety);
            if (satietyModel != null)
            {
                ratio += satietyModel.NatureBonusRatio;
            }
            return ratio;
        }

        #region 计算镜像宠物当前饱食度
        private int CheckUpdateSatiety(PetInfo petInfo, int curTime)
        {
            //需根据上次喂养时间获取当前真实饱食度
            int curSatiety = GetCurSatietyByTime(petInfo, curTime);
            if (curSatiety != petInfo.Satiety)
            {
                petInfo.SetSatiety(curSatiety);
                petInfo.SetSatietyUpdateTime(curTime);
            }
            return curSatiety;
        }

        private int GetCurSatietyByTime(PetInfo petInfo, int curTime)
        {
            int curSatiety = 0;
            PetSatietyModel satietyModel = PetLibrary.GetPetSatietyModel(petInfo.Satiety);
            if (satietyModel == null)
            {
                return curSatiety;
            }
            curSatiety = GetCurSatietyByDuration(petInfo, petInfo.Satiety, curTime - petInfo.LastFeedTime, curTime, satietyModel);
            return curSatiety;
        }

        private int GetCurSatietyByDuration(PetInfo petInfo, int satiety, int duration, int curTime, PetSatietyModel satietyModel)
        {
            int curSatiety;
            //饱食度减少到临界值需要的时间
            int subSatiety = satiety - (satietyModel.MinSatiety - 1);
            double hours = Math.Round(((double)subSatiety) / satietyModel.DeclinePerHour, 1);
            int subTime = (int)(hours * 3600);

            int beyondProTime = duration - satietyModel.ProtectionTime;
            //超出保护期
            if (beyondProTime > 0)
            {
                //上次饱食度更新时间已超过保护期
                if (petInfo.SatietyUpdateTime > petInfo.LastFeedTime + satietyModel.ProtectionTime)
                {
                    if (!petInfo.SatietyProtectFlag)
                    {
                        beyondProTime = curTime - petInfo.SatietyUpdateTime - satietyModel.ProtectionTime;
                        beyondProTime = beyondProTime > 0 ? beyondProTime : 0;
                        petInfo.SetSatietyProtectFlag(true);
                    }
                    else
                    {
                        beyondProTime = curTime - petInfo.SatietyUpdateTime;
                    }
                }
                PetSatietyModel lowSatietyModel = PetLibrary.GetPetSatietyModel(satietyModel.MinSatiety - 1);
                //说明当前已是最低饱食度阶段
                if (lowSatietyModel == null)
                {
                    curSatiety = CalculateSatiety(satiety, beyondProTime, satietyModel.DeclinePerHour);
                    return curSatiety;
                }
                //进入到下一饱食度阶段
                if (beyondProTime > subTime + lowSatietyModel.ProtectionTime)
                {
                    petInfo.SetSatietyProtectFlag(false);
                    curSatiety = GetCurSatietyByDuration(petInfo, lowSatietyModel.MaxSatiety, beyondProTime - subTime, curTime, lowSatietyModel);
                }
                //没有进入到下一饱食度阶段
                else if (beyondProTime < subTime)
                {
                    curSatiety = CalculateSatiety(satiety, beyondProTime, satietyModel.DeclinePerHour);
                }
                //在下一饱食度阶段保护期
                else
                {
                    petInfo.SetSatietyProtectFlag(false);
                    curSatiety = lowSatietyModel.MaxSatiety;
                }
            }
            else
            {
                curSatiety = satiety;
            }
            return curSatiety;
        }

        private int CalculateSatiety(int satiety, int beyondProTime, int declinePerHour)
        {
            double hours = Math.Round(((double)beyondProTime) / 3600, 1);
            int tempSatiety = satiety - (int)(hours * declinePerHour);
            int curSatiety = tempSatiety > 0 ? tempSatiety : 0;
            return curSatiety;
        }
        #endregion

        public bool CheckUseMirrorPet(MapType mapType)
        {
            //if (!CopyedFromPlayer)
            //{
            //    return false;
            //}
            switch (mapType)
            {
                case MapType.Arena:
                case MapType.TeamDungeon:
                case MapType.HuntingDeficute:
                case MapType.IntegralBoss:             
                case MapType.HuntingTeamDevil:
                case MapType.Versus:
                case MapType.HuntingActivityTeam:
                case MapType.IslandChallenge:
                case MapType.SpaceTimeTower:
                    //case MapType.CrossBoss:
                    return true;
                default:
                    break;
            }
            return false;
        }

        public void SetQueuePet(Dictionary<int, PetInfo> queuePet)
        {
            int queueNum = queuePet.Keys.First();
            RobPetInfo = queuePet.Values.First();
            if (!CopyedFromPlayer)
            {
                CreateRobotPet(queueNum, RobPetInfo);
            }
        }

        public void CopyPet2CrossMap(PlayerChar player, Dictionary<int, PetInfo> queuePet)
        {
            if (queuePet.Count == 0) return;
            SetQueuePet(queuePet);
            PetModel petModel = PetLibrary.GetPetModel(RobPetInfo.PetId);
            if (petModel == null)
            {
                return;
            }
            PetInfo info = RobPetInfo.Clone();
            pets.Clear();
            Pet pet = NewPet(player.server, this, info, petModel, queuePet.Keys.First());
            pet.IsAttacker = IsAttacker;
            pets.Add(pet.PetUid, pet);
            //UpdateSpeed(pet);
            CurrentMap.CreatePet(pet);
        }

        public void CreateRobotPet(int queueNum, PetInfo info)
        {
            PetModel petModel = PetLibrary.GetPetModel(info.PetId);
            if (petModel == null)
            {
                return;
            }
            Pet pet = NewPet(server, this, info, petModel, queueNum);
            pet.InitSpeedInBattle();
            CurrentMap.CreatePet(pet);
        }

        public void CopyHeros2SpaceTimeTowerDungeon(PlayerChar player)
        {
            List<HeroInfo> infos = new List<HeroInfo>();

            foreach (var hero in heroInfos)//.Where(kv => kv.Key != 1))
            {
                HeroInfo info = hero.Clone();//Todo:检查一下
                info.GodType = hero.GodType;
                info.RobotInfo = new RobotHeroInfo();

                for (int i = 1; i <= 10; i++)
                {
                    info.RobotInfo.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", i, 1, 0, 0, 0);//此副本无魂环效果
                }

                infos.Add(info);
            }
            heroInfos = infos;
            heros.Clear();
            foreach (var kv in infos)
            {
                Hero hero = NewHeroWithoutNatureExt(player.server, this, kv);
                hero.IsAttacker = IsAttacker;
                heros.Add(hero.HeroId, hero);
                AddHeroSoulRingInfo(kv);
                UpdateSpeed(hero);
                CurrentMap.CreateHero(hero);
            }
        }
        
        public Hero NewHeroWithoutNatureExt(ZoneServerApi server, FieldObject owner, HeroInfo info)
        {
            Hero hero = new Hero(server, owner, info);
            hero.OwnerIsRobot = true;
            hero.Init();
            return hero;
        }
    }
}

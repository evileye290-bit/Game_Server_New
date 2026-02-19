using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap
    {
        private bool kickedPlayer;
        private float kickDelayTime = 30.0f;//剔除玩家倒计时s

        private bool attackerSuppress;
        private BattlePowerSuppressModel battlePowerSuppressModel;

        // 已经执行过StartFighting的player列表 key uid
        private HashSet<int> startedPlayerList = new HashSet<int>();
        public HashSet<int> StartedPlayerList { get { return startedPlayerList; } }

        public HashSet<int> FirstStartedPlayer = new HashSet<int>();

        private int energyNum = 0;
        private DateTime lastReviveTime = BaseApi.now;
        public int ReviveCount { get; set; }
        public int EnergyCanCount { get; set; }
        public DateTime LastReviveTime
        { get { return lastReviveTime; } }

        public bool OnePlayerDone//是否有一个主角整队完成加载
        { get; set; }

        public bool AllHeroDone//是否整场角色完成加载
        { get; set; }

        public bool OnePlayerPetDone//是否有一个主角的宠物完成加载
        { get; set; }

        private Dictionary<int, bool> monsterGeneratedWalkSynced = new Dictionary<int, bool>();//用于怪物出生后同步前端行进
        private Dictionary<int, float> monsterGeneratedWalkWaitTime = new Dictionary<int, float>();
        private Dictionary<int, MSG_ZGC_MONSTER_GENERATED_WALK> monsterGeneratedWalkInfo = new Dictionary<int, MSG_ZGC_MONSTER_GENERATED_WALK>();


        public void SetKickPlayerDelayTime(float time)
        {
            kickDelayTime = time;
        }

        public override void OnPlayerEnter(PlayerChar player, bool reEnter = false)
        {
            if (!reEnter)
            {
                //记录进副本时候的人数避免计算伙伴坐标错误
                RecordFieldObjectEnter(player);
                int i = 1;
                if (Model.MapType == MapType.CrossBattle)
                {
                    if (player.IsAttacker)
                    {
                        i = AttackerPosIndex;
                    }
                    else
                    {
                        i = DefenderPosIndex;
                    }
                }
                player.EnterMapInfo.SetPosition(CalcBeginPos(i, player));//更改进图时位置
            }

            player.IsObserver = true;//尝试让玩家的playerchar变成观察者，从而不对别人显示视野信息
            player.IsVisable = false;
            if (!IsDungeon)
            {
                player.ClearAllBattleState();
            }

            base.OnPlayerEnter(player, reEnter);
            
            player.SendHiddenWeaponInfo();

            player.NotifyRelationEnterDungeon();
        }

        public override void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            // todo 根据副本玩法，在玩家离开时做处理
            player.IsObserver = false;
            player.IsVisable = true;
            base.OnPlayerLeave(player, cache);
            player.NotifyRelationLeaveDungeon();
        }

        public override bool CanEnter()
        {
            if (!base.CanEnter())
            {
                return false;
            }
            if (State >= DungeonState.Stopped)
            {
                return false;
            }
            return true;
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            NotifyDungeonStopTime(player);
            NotifyEnrgyCanInfo(player);
            if (IsFirstStartFight(player))
            {
                switch (State)
                {
                    // 副本还在准备阶段，那么不需要再做额外等待，直接开始
                    case DungeonState.Open:
                        Start();
                        break;
                    // 副本已经开始 则开始战斗
                    case DungeonState.Started:

                        player.StartFighting();
                        StartHeros(player);

                        break;
                    default:
                        break;
                }
            }
            NotifyIsFirstStart(player);
            if (player.GetReEnterDungeon())
            {
                SyncReEnterDungeonInfo(player);
                player.SetReEnterDungeon(false);
            }
        }

        /// <summary>
        /// 单独处理当前角色的heros
        /// </summary>
        /// <param name="player"></param>
        public void StartHeros(PlayerChar player)
        {
            foreach (var hero in HeroList)
            {
                if (hero.Value.Owner == player)
                {
                    hero.Value.StartFighting();
                }
            }
            foreach (var hero in HeroList)
            {
                hero.Value.DispatchHeroStartFightMsg(hero.Value.HeroId);
            }
        }

        public void StartRobotHeros(Robot robot)
        {
            HeroList.Where(hero => hero.Value.Owner == robot).ForEach(kv => kv.Value.StartFighting());
        }

        //结束后30s将玩家剔出副本
        protected void CheckKickPlayer(float dt)
        {
            if (State == DungeonState.Stopped && !kickedPlayer)
            {
                kickDelayTime -= dt;
                if (kickDelayTime <= 0)
                {
                    KickPlayer();
                }
            }
        }

        protected void KickPlayer()
        {
            if (kickedPlayer) return;

            KickPlayers(PcList);

            kickedPlayer = true;
            Log.Debug("Kick player after dungeon stoped 30s");
        }

        private void KickPlayers(IReadOnlyDictionary<int, PlayerChar> playerList)
        {
            foreach (var item in playerList)
            {
                try
                {
                    if (item.Value.IsOnline())
                    {
                        item.Value.LeaveDungeon();
                    }
                    else
                    {
                        item.Value.LeaveMap();
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public bool CheckPlayerStartedFighting(int uid)
        {
            return startedPlayerList.Contains(uid);
        }

        public void RecordPlayerStartedFighting(int uid)
        {
            if (!startedPlayerList.Contains(uid))
            {
                startedPlayerList.Add(uid);
            }
        }

        public void RemovePlayerStartedFighting(int uid)
        {
            startedPlayerList.Remove(uid);
        }

        public PlayerChar FindPc(int uid)
        {
            var player = server.PCManager.FindPc(uid);
            if (player == null)
            {
                player = server.PCManager.FindOfflinePc(uid);
            }
            return player;
        }

        public void RevivePlayer(PlayerChar player)
        {
            ReviveCount += 1;
            OnReviveFieldObject();
            player.Revive();
        }

        public void ReviveHero(Hero hero)
        {
            ReviveCount += 1;
            OnReviveFieldObject();
            hero.Revive();
        }

        public void OnReviveFieldObject()
        {
            lastReviveTime = CurrTime;
            UpdateReviveCanValue(-1);
        }

        public void AddReviveEnergy()
        {
            energyNum += TeamLibrary.EnergyAddNum;
            if (energyNum >= TeamLibrary.EnergyMaxNum)
            {
                energyNum = 0;
                if (EnergyCanCount < TeamLibrary.EnergyCanMaxNum)
                {
                    UpdateReviveCanValue(1);
                }
            }
        }

        private void UpdateReviveCanValue(int value)
        {
            EnergyCanCount += value;
            NotifyEnrgyCanInfo();
        }

        protected void NotifyEnrgyCanInfo()
        {
            MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO msg = GenerateReliveMedicineSyncMsg();
            PcList.ForEach(player => player.Value.Write(msg));
        }

        protected void NotifyEnrgyCanInfo(PlayerChar player)
        {
            MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO msg = GenerateReliveMedicineSyncMsg();
            player.Write(msg);
        }

        private MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO GenerateReliveMedicineSyncMsg()
        {
            MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO msg = new MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO()
            {
                EnergyCanNum = EnergyCanCount,
                EnergyNum = energyNum,
                ReviveCount = ReviveCount,
            };
            return msg;
        }

        protected void NotifyDungeonStopTime(PlayerChar player)
        {
            MSG_ZGC_DUNGEON_STOPTIME msg = new MSG_ZGC_DUNGEON_STOPTIME();
            msg.StartTime = Timestamp.GetUnixTimeStampSeconds(StartTime);
            msg.StopTime = Timestamp.GetUnixTimeStampSeconds(CurrTime.AddSeconds(StopTime));
            player.Write(msg);
        }

        protected void NotifyDungeonBattleStart()
        {
            if (Model.MapType != MapType.NoCheckSingleDungeon)
            {
                MSG_ZGC_DUNGEON_START msg = new MSG_ZGC_DUNGEON_START();
                msg.StopTime = Timestamp.GetUnixTimeStampSeconds(CurrTime.AddSeconds(StopTime));

                BroadCast(msg);
            }
        }


        public void NotifyIsFirstStart(PlayerChar player)
        {
            if (!FirstStartedPlayer.Contains(player.Uid))
            {
                player.SendStartFightingMsg(true);
                FirstStartedPlayer.Add(player.Uid);
            }
            else
            {
                player.SendStartFightingMsg(false);
            }
        }

        public bool IsFirstStartFight(PlayerChar player)
        {
            return !FirstStartedPlayer.Contains(player.Uid);
        }

        public void PlayerAddTaskNum(PlayerChar player)
        {
            player.AddTaskNumForType(TaskType.CompleteDungeons, 1, true, Model.MapType);
            player.AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, DungeonModel.Id);
            player.AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, DungeonModel.Id);
            player.AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, Model.MapType);

            //完成通行证任务
            player.AddPassCardTaskNum(TaskType.CompleteDungeons, (int)Model.MapType, TaskParamType.TYPE);
            player.AddPassCardTaskNum(TaskType.CompleteOneDungeon, DungeonModel.Id, TaskParamType.DUNGEON);
            player.AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)Model.MapType, TaskParamType.DUNGEON_TYPES);

            //完成学院任务
            player.AddSchoolTaskNum(TaskType.CompleteDungeons, (int)Model.MapType, TaskParamType.TYPE);
            player.AddSchoolTaskNum(TaskType.CompleteOneDungeon, DungeonModel.Id, TaskParamType.DUNGEON);
            player.AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)Model.MapType, TaskParamType.DUNGEON_TYPES);
            player.AddSchoolTaskNum(TaskType.CompleteDungeonList, DungeonModel.Id, TaskParamType.DUNGEON_LIST);

            player.AddRunawayActivityNumForType(RunawayAction.Fight);
            
            //漂流探宝
            player.AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, Model.MapType);
            player.AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, Model.MapType);
        }

        public void SyncReEnterDungeonInfo(PlayerChar player)
        {
            MSG_ZGC_REENTER_DUNGEON notify = new MSG_ZGC_REENTER_DUNGEON();
            foreach (var kv in PcList)
            {
                DUNGEON_FIELDOBJECT info = kv.Value.GetDungeonFieldObjectMsg();
                notify.FieldObjectList.Add(info);
            }

            foreach (var kv in HeroList)
            {
                DUNGEON_FIELDOBJECT info = kv.Value.GetDungeonFieldObjectMsg();
                notify.FieldObjectList.Add(info);

                // 如果是自己的伙伴 获取可由主角释放的伙伴技能
                if (kv.Value.Owner != null && kv.Value.Owner.Uid == player.Uid)
                {
                    MSG_ZGC_SKILL_ENERGY_LIST heroSkillMsg = kv.Value.SkillManager.GetSkillEnergyForOwner();
                    foreach (var skill in heroSkillMsg.List)
                    {
                        notify.SkillList.Add(skill);
                    }
                }
            }

            foreach (var kv in MonsterList)
            {
                DUNGEON_FIELDOBJECT info = kv.Value.GetDungeonFieldObjectMsg();
                notify.FieldObjectList.Add(info);
            }

            foreach (var kv in PetList)
            {
                DUNGEON_FIELDOBJECT info = kv.Value.GetDungeonFieldObjectMsg();
                notify.FieldObjectList.Add(info);
            }

            // 自身技能
            //MSG_ZGC_SKILL_ENERGY_LIST playerSkillMsg = player.SkillManager.GetSkillEnergyMsg();
            //foreach (var skill in playerSkillMsg.List)
            //{
            //    notify.SkillList.Add(skill);
            //}

            notify.NeedAlarm = CheckNeedAlarm();
            //notify.Hp = player.GetHp();
            //notify.MaxHp = player.GetMaxHp();
            player.Write(notify);
        }

        public void OnFieldObjectDead(FieldObject dead)
        {
            if (dead == null)
            {
                return;
            }
            // 处理其他FieldObject中 由dead创建的trigger
            // 当前只有hero 加到 player上 如果后续由其他情况 此处做添加
            if (dead.IsHero)
            {
                foreach (var kv in PcList)
                {
                    PlayerChar player = kv.Value;
                    if (player.TriggerMng != null)
                    {
                        player.TriggerMng.RemoveTriggersFromOther(dead.InstanceId);
                    }
                }
                foreach (var kv in RobotList)
                {
                    Robot robot = kv.Value;
                    if (robot.TriggerMng != null)
                    {
                        robot.TriggerMng.RemoveTriggersFromOther(dead.InstanceId);
                    }
                }
            }

            //移除player在地图startedFighting信息
            if (dead.IsPlayer || dead.IsRobot)
            {
                RemovePlayerStartedFighting(dead.Uid);
            }

        }

        public void BroadCastMonsterGen2Hero(int genId, Vec2 vec, float bornTime = 0f)
        {
            foreach (var kv in HeroList)
            {
                Hero hero = kv.Value;
                hero.SetWalkStraightVec(genId, vec, bornTime);
            }

            MSG_ZGC_MONSTER_GENERATED_WALK msg = new MSG_ZGC_MONSTER_GENERATED_WALK();
            msg.GenId = genId;
            msg.WalkVecX = vec.X;
            msg.WalkVecY = vec.Y;

            //BroadCast(msg);
            monsterGeneratedWalkSynced[genId] = false;
            monsterGeneratedWalkWaitTime[genId] = bornTime;
            monsterGeneratedWalkInfo[genId] = msg;
        }

        protected void CheckBroadCastMonsterGeneratedWalk(float dt)
        {
            List<int> needChecks = new List<int>();
            monsterGeneratedWalkSynced.ForEach(kv =>
            {
                if (kv.Value == false)
                {
                    needChecks.Add(kv.Key);
                }
            });

            foreach (var key in needChecks)
            {
                float temp = monsterGeneratedWalkWaitTime[key];
                temp -= dt;
                monsterGeneratedWalkWaitTime[key] = temp;
                if (temp <= 0)
                {
                    monsterGeneratedWalkSynced[key] = true;
                    BroadCast(monsterGeneratedWalkInfo[key]);
                }
            }
        }

        public void AddDefenderHeros(List<int> heros)
        {
            List<HeroInfo> infos = RobotManager.GetHeroList(heros);
            AddRobotAndHeros(false, infos, -1, new Dictionary<int, int>(), new Dictionary<int, int>());
        }

        //添加robot的唯一方法
        protected void AddRobotAndHeros(bool isAttacker, List<HeroInfo> infos, int ownerUid,
            Dictionary<int, int> natureValues, Dictionary<int, int> natureRatios, Dictionary<int, int> heroPos = null, Dictionary<int, PetInfo> queuePet = null)
        {
            HeroInfo info = infos.First();
            Robot robot = new Robot(server, ownerUid);
            robot.IsAttacker = isAttacker;

            robot.InitNatureExt(natureValues, natureRatios);
            robot.InitRobot(info.Id, info);

            robot.EnterMap(this);

            AddRobot(robot);

            robot.SetOwnerUid(ownerUid);
            robot.SetHeroPoses(heroPos);
            robot.SetHeroInfos(infos);

            if (queuePet != null)
            {
                robot.SetQueuePet(queuePet);
            }
        }

        public void AddMirrorRobot(bool isAttacker, PlayerChar player, Dictionary<int, int> heroPos = null)
        {
            player.IsAttacker = isAttacker;

            Robot robot = Robot.CopyFromPlayer(server, player);
            robot.IsAttacker = isAttacker;

            robot.EnterMap(this);

            AddRobot(robot);

            if (heroPos != null)
            {
                robot.SetHeroPoses(heroPos);
            }
            robot.CopyHeros2Map(player);
            robot.CopyPet2Map(player);
        }


        internal int DefenderPosIndex = 2;

        internal int AttackerPosIndex = 0; //FIXME:防御者标识，这里必须大于2 

        public void RecordFieldObjectEnter(FieldObject fieldObject)
        {
            if (fieldObject.IsAttacker)
            {
                AttackerPosIndex++;
            }
            else
            {
                DefenderPosIndex++;
            }
        }

        public void SetBattlePowerSuppress(long attackerBattlePower, long defBattlePower)
        {
            //由于robot的Uid为-1 所以当为机器人的时候用进攻方防守方来做加成
            attackerSuppress = attackerBattlePower > defBattlePower;
            battlePowerSuppressModel = BattlePowerLibrary.GertBattlePowerSuppressModel(attackerBattlePower, defBattlePower);

#if DEBUG
            Log.Warn($"battle power suppress attacker battle power {attackerBattlePower} defender battle power {defBattlePower}, suppress id {battlePowerSuppressModel?.Id}");
#endif
        }

        public void CheckBattlePowerSuppress(FieldObject field)
        {
            if (battlePowerSuppressModel == null) return;

            if (field.IsAttacker == attackerSuppress)
            {
                battlePowerSuppressModel.IncreaseRatio.ForEach(x => field.AddNatureRatio(x.Key, x.Value));
            }
        }

        public void NotifyMonsterGen2Pet(float bornTime)
        {
            foreach (var kv in PetList)
            {
                Pet pet = kv.Value;
                pet.RecordFightFsmWaitTime(bornTime);
            }
        }
    }
}

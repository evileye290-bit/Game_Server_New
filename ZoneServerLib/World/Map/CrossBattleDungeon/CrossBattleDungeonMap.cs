using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class CrossBattleDungeonMap : DungeonMap
    {

        public SortedDictionary<int, Vec2> CrossBattlePlayerPos = new SortedDictionary<int, Vec2>();

        public CrossBattleDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            IsSpeedUpDungeon = true;

            if (DungeonModel != null)
            {
                CrossBattlePlayerPos[1] = new Vec2(DungeonModel.FirstPoint.X + CrossBattleLibrary.CrossInitPointOffsetDistance, DungeonModel.FirstPoint.Y);
                CrossBattlePlayerPos[2] = new Vec2(DungeonModel.FirstPoint.X - CrossBattleLibrary.CrossInitPointOffsetDistance, DungeonModel.FirstPoint.Y);

                CrossBattlePlayerPos[3] = new Vec2(DungeonModel.PlayerPos[4].X + CrossBattleLibrary.CrossInitPointOffsetDistance, DungeonModel.PlayerPos[4].Y);
                CrossBattlePlayerPos[4] = new Vec2(DungeonModel.PlayerPos[4].X - CrossBattleLibrary.CrossInitPointOffsetDistance, DungeonModel.PlayerPos[4].Y);
            }

        }
        public Vec2 GetCrossBattlePosition4Count(int nowCount, Vec2 vec)
        {
            switch (nowCount)
            {
                case 1:
                    return CrossBattlePlayerPos[1] + vec;
                case 2:
                    return CrossBattlePlayerPos[2] + vec;
                case 3:
                    return CrossBattlePlayerPos[3] - vec;
                case 4:
                    return CrossBattlePlayerPos[4] - vec;
                default:
                    return CrossBattlePlayerPos[1] + vec;
            }
        }

        protected override void InitBattleFpsManager()
        {
            BattleFpsManager = new CrossBattleFpsManager(this);
        }

        public override void OnStopBattle(PlayerChar player)
        {
            Stop(DungeonResult.Failed);
            OnSkipBattle(player);
        }

        protected int playerUid;
        protected PlayerCrossFightInfo FightInfo;
        public override void Stop(DungeonResult result)
        {
            // 已经有胜负结果，不再更新（防止临界状态下下，有可能又赢又输）
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //副本结束取消所有trigger
            DungeonResult = result;
            State = DungeonState.Stopped;

            OnStopFighting();
            SetSpeedUp(false);

            if (FightInfo.Type == ChallengeIntoType.CrossPreliminary)
            {
                foreach (var player in PcList)
                {
                    NotifySpeedUpEnd(player.Value);

                    player.Value.SendCrossBattleResult(result);

                    //战斗录像
                    BattleFpsManager.Close(DungeonResult, result == DungeonResult.Success ? player.Value.Uid : FightInfo.Uid, player.Value.Uid);

                    int pointState = GetPointState(result);
                    //日志
                    player.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
                }
            }
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            NotifyIsFirstStart(player);
            NotifyDungeonStopTime(player);
            NotifyEnrgyCanInfo(player);
            switch (State)
            {
                // 副本还在准备阶段，那么不需要再做额外等待，直接开始
                case DungeonState.Open:
                    Start();
                    break;
                // 副本已经开始 则开始战斗
                case DungeonState.Started:
                    //player.StartFighting();
                    //StartHeros(player);
                    //Start();
                    break;
                default:
                    break;
            }
            if (player.GetReEnterDungeon())
            {
                SyncReEnterDungeonInfo(player);
                player.SetReEnterDungeon(false);
            }
        }
        

        public virtual void AddAttackerMirror(PlayerChar player)
        {
            player.IsAttacker = true;

            foreach (var queue in player.HeroMng.CrossQueue)
            {
                Dictionary<int, int> poses = new Dictionary<int, int>();
                List<HeroInfo> heros = new List<HeroInfo>();

                foreach (var pos in queue.Value)
                {
                    int posId = pos.Key;
                    HeroInfo heroInfo = pos.Value;
                    poses.Add(heroInfo.Id, posId);
                    heros.Add(heroInfo);
                }

                if (player.Uid > 0)
                {
                    Robot robot = Robot.CopyFromPlayer(server, player);
                    robot.IsAttacker = true;
                    robot.EnterMap(this);
                    robot.SetOwnerUid(player.Uid);
                    base.AddRobot(robot);

                    robot.SetHeroPoses(poses);
                    robot.SetHeroInfos(heros);
                    robot.CopyHeros2CrossMap(player);
                }
                else
                {
                    AddRobotAndHeros(true, heros, player.Uid, player.NatureValues, player.NatureRatios, poses);
                }
            }
        }

        public void AddCrossDefender(PlayerCrossFightInfo info, int ownerUid = -1)
        {
            foreach (var queue in info.HeroQueue)
            {
                List<HeroInfo> infos = new List<HeroInfo>();
                Dictionary<int, int> heroPoses = new Dictionary<int, int>();
                foreach (var kv in queue.Value)
                {
                    heroPoses.Add(kv.Value.HeroId, kv.Key);

                    HeroInfo heroInfo = RobotManager.InitFromRobotInfo(kv.Value);
                    heroInfo.CrossQueueNum = queue.Key;
                    heroInfo.CrossPositionNum = kv.Key;
                    infos.Add(heroInfo);
                }

                HeroInfo temp = infos.First();
                temp.RobotInfo.Name = info.Name;
                temp.RobotInfo.Sex = info.Sex;
                AddRobotAndHeros(false, infos, ownerUid, info.NatureValues, info.NatureRatios, heroPoses);
            }
        }
        private int GetPositionIndex(int count)
        {
            switch (count)
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 4;
                case 4:
                    return 3;
                default:
                    return 1;
            }
        }
        public override Vec2 CalcBeginPos(int i, FieldObject field)
        {
            return CrossBattlePlayerPos[GetPositionIndex(i)] ?? OldCalcBeginPos();
        }

        public void SetChallengeIntoType(PlayerCrossFightInfo fightInfo, int playerUid)
        {
            this.FightInfo = fightInfo;
            this.playerUid = playerUid;
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            // 加到地图里
            AddHero(hero);

            if (IsDungeon)
            {
                //设置位置，一定在aoi前
                Robot robotOwner = hero.Owner as Robot;

                int pcCount = 0;
                if (robotOwner.IsAttacker)
                {
                    pcCount = AttackerPosIndex;
                }
                else
                {
                    pcCount = DefenderPosIndex;
                }

                int heroPos = robotOwner.GetHeroPos(hero.HeroId);
                Vec2 temp = robotOwner.GetHeroPosPosition(hero.HeroId);

                if (temp != null)
                {
                    temp = GetCrossBattlePosition4Count(pcCount, temp);
                }

                hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(heroPos);

                Vec2 positon = temp ?? hero.Position;

                hero.SetPosition(positon);

                //Vec2 walkVec = DungeonModel.CenterPoint - positon;
                //walkVec = walkVec.Change(CrossBattleLibrary.CrossWalkDistance);
                Vec2 walkVec;
                if (hero.IsAttacker)
                {
                    walkVec = new Vec2(0.0f, CrossBattleLibrary.CrossWalkDistance);
                }
                else
                {
                    walkVec = new Vec2(0.0f, -CrossBattleLibrary.CrossWalkDistance);
                }
                hero.SetHeroOneSideWalkVec(walkVec);

                hero.InitBaseBattleInfo();
            }

            if (add2Aoi)
            {
                hero.AddToAoi();
                hero.BroadCastHp();
            }


        }

        public void SendCrossBattleResult(int winUid, string fileName)
        {
            //获取到玩家2 信息，开始战斗
            MSG_ZR_SET_BATTLE_RESULT addMsg = new MSG_ZR_SET_BATTLE_RESULT();
            addMsg.TimingId = FightInfo.TimingId;
            addMsg.GroupId = FightInfo.GroupId;
            addMsg.TeamId = FightInfo.TeamId;
            addMsg.FightId = FightInfo.FightId;
            addMsg.FileName = fileName;

            int index = 0;
            FightInfo.HeroIndex.TryGetValue(winUid, out index);
            addMsg.WinUid = index;
            //int winP = NewRAND.Next(1, 2);
            //if (winP == 1)
            //{
            //    addMsg.WinUid = msgInfo.Player1.Index;
            //}
            //else
            //{
            //    addMsg.WinUid = msgInfo.Player2.Index;
            //}
            server.RelationServer.Write(addMsg, FightInfo.Uid);
        }

    }
}
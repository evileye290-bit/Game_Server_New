using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class RankManager
    {
        private CrossServerApi server { get; set; }

        public Dictionary<int, HidderWeaponRank> HidderWeaponList;
        public Dictionary<int, SeaTreasureRank> SeaTreasureList;
        public Dictionary<int, CrossBossRankManager> CrossBossRankMngList;
        public Dictionary<int, GardenRank> GardenRankList;
        public Dictionary<int, DivineLoveRank> DivineLoveList;
        public Dictionary<int, IslandHighRank> IslandHighRankList;
        public Dictionary<int, IslandHighLastStageRank> IslandHighLastStageRankList;
        public Dictionary<int, StoneWallRank> StoneWallList;
        public Dictionary<int, CarnivalBossRank> CarnivalBossList;
        public Dictionary<int, RouletteRank> RouletteRankList;
        public Dictionary<int, CanoeRank> CanoeList;
        public Dictionary<int, MidAutumnRank> MidAutumnList;
        public Dictionary<int, ThemeFireworkRank> ThemeFireworkList;
        public Dictionary<int, NineTestRank> NineTestRankList;

        public RankManager(CrossServerApi server)
        {
            this.server = server;

            HidderWeaponList = new Dictionary<int, HidderWeaponRank>();
            SeaTreasureList = new Dictionary<int, SeaTreasureRank>();
            CrossBossRankMngList = new Dictionary<int, CrossBossRankManager>();
            GardenRankList = new Dictionary<int, GardenRank>();
            DivineLoveList = new Dictionary<int, DivineLoveRank>();
            IslandHighRankList = new Dictionary<int, IslandHighRank>();
            IslandHighLastStageRankList = new Dictionary<int, IslandHighLastStageRank>();
            StoneWallList = new Dictionary<int, StoneWallRank>();
            CarnivalBossList = new Dictionary<int, CarnivalBossRank>();
            RouletteRankList = new Dictionary<int, RouletteRank>();
            CanoeList = new Dictionary<int, CanoeRank>();
            MidAutumnList = new Dictionary<int, MidAutumnRank>();
            ThemeFireworkList = new Dictionary<int, ThemeFireworkRank>();
            NineTestRankList = new Dictionary<int, NineTestRank>();
        }

        public void Init()
        {
            foreach (var group in CrossBattleLibrary.GroupList)
            {
                AddHidderWeaponRank(group.Key);

                AddCrossBossRankManager(group.Key);

                AddSeaTreasureRank(group.Key);

                AddGardenRank(group.Key);

                AddDivineLoveRank(group.Key);

                AddIslandHighRank(group.Key);

                AddIslandHighLastStageRank(group.Key);

                AddCarnivalBossRank(group.Key);

                AddStoneWallRank(group.Key);

                AddRouletteRank(group.Key);

                AddCanoeRank(group.Key);

                AddMidAutumnRank(group.Key);

                AddThemeFireworkRank(group.Key);

                AddNineTestRank(group.Key);
            }


            //double interval = 30000;
            //Log.Info($"RankManager add task timer：after {interval}");
            //CrossBattleTimerQuery timer = new CrossBattleTimerQuery(interval);
            //server.TaskTimerMng.Call(timer, (ret) =>
            //{
            //    server.TrackingLoggerMng.TrackTimerLog(server.MainId, "cross", "CheckRankScore", server.Now());
            //    CheckCrossBossRankScore();
            //});
        }

        public void AddCrossBossRankManager(int group)
        {
            CrossBossRankManager crossBoss = new CrossBossRankManager(server);
            CrossBossRankMngList.Add(group, crossBoss);
        }

        public void AddHidderWeaponRank(int group)
        {
            HidderWeaponRank HidderWeapon = new HidderWeaponRank(server);
            HidderWeapon.Init(group, 2);
            HidderWeapon.LoadInitRankFromRedis();
            HidderWeaponList.Add(group, HidderWeapon);
        }
        public void AddSeaTreasureRank(int group)
        {
            SeaTreasureRank seaTreasure = new SeaTreasureRank(server);
            seaTreasure.Init(group, 2);
            seaTreasure.LoadInitRankFromRedis();
            SeaTreasureList.Add(group, seaTreasure);
        }

        public void AddGardenRank(int group)
        {
            GardenRank gardenRank = new GardenRank(server);
            gardenRank.Init(group, 0);
            gardenRank.LoadInitRankFromRedis();
            GardenRankList.Add(group, gardenRank);
        }

        public void AddDivineLoveRank(int group)
        {
            DivineLoveRank divineLove = new DivineLoveRank(server);
            divineLove.Init(group, 2);
            divineLove.LoadInitRankFromRedis();
            DivineLoveList.Add(group, divineLove);
        }

        public void AddIslandHighRank(int group)
        {
            IslandHighRank rank = new IslandHighRank(server);
            rank.Init(group, 2);
            rank.LoadInitRankFromRedis();
            IslandHighRankList.Add(group, rank);
        }

        public void AddIslandHighLastStageRank(int group)
        {
            IslandHighLastStageRank rank = new IslandHighLastStageRank(server);
            rank.Init(group, 2);
            rank.LoadInitRankFromRedis();
            IslandHighLastStageRankList.Add(group, rank);
        }

        public void AddStoneWallRank(int group)
        {
            StoneWallRank stoneWall = new StoneWallRank(server);
            stoneWall.Init(group, 2);
            stoneWall.LoadInitRankFromRedis();
            StoneWallList.Add(group, stoneWall);
        }

        public void AddCarnivalBossRank(int group)
        {
            CarnivalBossRank carnivalBossRank = new CarnivalBossRank(server);
            carnivalBossRank.Init(group, 0);
            carnivalBossRank.LoadInitRankFromRedis();
            CarnivalBossList.Add(group, carnivalBossRank);
        }

        public void AddRouletteRank(int group)
        {
            RouletteRank rank = new RouletteRank(server);
            rank.Init(group, 0);
            rank.LoadInitRankFromRedis();
            RouletteRankList.Add(group, rank);
        }

        public RouletteRank GetRouletteRank(int groupId)
        {
            RouletteRank value;
            RouletteRankList.TryGetValue(groupId, out value);
            return value;
        }

        public void AddCanoeRank(int group)
        {
            CanoeRank canoeRank = new CanoeRank(server);
            canoeRank.Init(group, 2);
            canoeRank.LoadInitRankFromRedis();
            CanoeList.Add(group, canoeRank);
        }

        public void AddMidAutumnRank(int group)
        {
            MidAutumnRank rank = new MidAutumnRank(server);
            rank.Init(group, 0);
            rank.LoadInitRankFromRedis();
            MidAutumnList.Add(group, rank);
        }

        public void AddThemeFireworkRank(int group)
        {
            ThemeFireworkRank rank = new ThemeFireworkRank(server);
            rank.Init(group, 0);
            rank.LoadInitRankFromRedis();
            ThemeFireworkList.Add(group, rank);
        }

        public void AddNineTestRank(int group)
        {
            NineTestRank rank = new NineTestRank(server);
            rank.Init(group, 0);
            rank.LoadInitRankFromRedis();
            NineTestRankList.Add(group, rank);
        }

        public GardenRank GetGardenRank(int groupId)
        {
            GardenRank value;
            GardenRankList.TryGetValue(groupId, out value);
            return value;
        }

        public IslandHighRank GetIslandHighRank(int groupId)
        {
            IslandHighRank value;
            IslandHighRankList.TryGetValue(groupId, out value);
            return value;
        }

        public IslandHighLastStageRank GetIslandHighLastStageRank(int groupId)
        {
            IslandHighLastStageRank value;
            IslandHighLastStageRankList.TryGetValue(groupId, out value);
            return value;
        }

        public SeaTreasureRank GetSeaTreasureRank(int groupId)
        {
            SeaTreasureRank value;
            SeaTreasureList.TryGetValue(groupId, out value);
            return value;
        }

        public HidderWeaponRank GetHidderWeaponRank(int groupId)
        {
            HidderWeaponRank value;
            HidderWeaponList.TryGetValue(groupId, out value);
            return value;
        }      

        public CrossBossRankManager GetCrossBossRankManager(int groupId)
        {
            CrossBossRankManager value;
            CrossBossRankMngList.TryGetValue(groupId, out value);
            return value;
        }

        public DivineLoveRank GetDivineLoveRank(int groupId)
        {
            DivineLoveRank value;
            DivineLoveList.TryGetValue(groupId, out value);
            return value;
        }

        public StoneWallRank GetStoneWallRank(int groupId)
        {
            StoneWallRank value;
            StoneWallList.TryGetValue(groupId, out value);
            return value;
        }

        public CarnivalBossRank GetCarnivalBossRank(int groupId)
        {
            CarnivalBossRank value;
            CarnivalBossList.TryGetValue(groupId, out value);
            return value;
        }

        public CanoeRank GetCanoeRank(int groupId)
        {
            CanoeRank value;
            CanoeList.TryGetValue(groupId, out value);
            return value;
        }

        public MidAutumnRank GetMidAutumnRank(int groupId)
        {
            MidAutumnRank value;
            MidAutumnList.TryGetValue(groupId, out value);
            return value;
        }

        public ThemeFireworkRank GetThemeFireworkRank(int groupId)
        {
            ThemeFireworkRank value;
            ThemeFireworkList.TryGetValue(groupId, out value);
            return value;
        }

        public NineTestRank GetNineTestRank(int groupId)
        {
            NineTestRank value;
            NineTestRankList.TryGetValue(groupId, out value);
            return value;
        }

        public void ClearHidderWeaponRank()
        {
            foreach (var rank in HidderWeaponList)
            {
                rank.Value.Clear();
            }
            //HidderWeaponList.Clear();
        }

        public void ClearSeaTreasureRank()
        {
            foreach (var rank in SeaTreasureList)
            {
                rank.Value.Clear();
            }
            //SeaTreasureList.Clear();
        }

        public void ClearGardenRank()
        {
            foreach (var rank in GardenRankList)
            {
                rank.Value.Clear();
            }
            //SeaTreasureList.Clear();
        }

        public void ClearDivineLoveRank()
        {
            foreach (var rank in DivineLoveList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearIslandHighRank()
        {
            foreach (var rank in IslandHighRankList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearIslandHighLastStageRank()
        {
            foreach (var rank in IslandHighLastStageRankList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearStoneWallRank()
        {
            foreach (var rank in StoneWallList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearCarnivalBossRank()
        {
            foreach (var rank in CarnivalBossList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearIsRouletteRank()
        {
            foreach (var rank in RouletteRankList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearCanoeRank()
        {
            foreach (var rank in CanoeList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearMidAutumnRank()
        {
            foreach (var rank in MidAutumnList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearThemeFireworkRank()
        {
            foreach (var rank in ThemeFireworkList)
            {
                rank.Value.Clear();
            }
        }

        public void ClearNineTestRank()
        {
            foreach (var rank in NineTestRankList)
            {
                rank.Value.Clear();
            }
        }

        public RankBaseModel GetFirst(RankType rankType, int groupId)
        {
            switch (rankType)
            {
                case RankType.Garden:
                    GardenRank gardenRank =GetGardenRank(groupId);
                    if (gardenRank == null)
                    {
                        AddGardenRank(groupId);
                        gardenRank = GetGardenRank(groupId);
                    }
                    return gardenRank.GetFirst();
                case RankType.Roulette:
                    RouletteRank rouletteRank = GetRouletteRank(groupId);
                    if (rouletteRank == null)
                    {
                        AddGardenRank(groupId);
                        rouletteRank = GetRouletteRank(groupId);
                    }
                    return rouletteRank.GetFirst();
                case RankType.Canoe:
                    CanoeRank canoeRank = GetCanoeRank(groupId);
                    if (canoeRank == null)
                    {
                        AddCanoeRank(groupId);
                        canoeRank = GetCanoeRank(groupId);
                    }
                    return canoeRank.GetFirst();
                case RankType.MidAutumn:
                    MidAutumnRank midAutumnRank = GetMidAutumnRank(groupId);
                    if (midAutumnRank == null)
                    {
                        AddMidAutumnRank(groupId);
                        midAutumnRank = GetMidAutumnRank(groupId);
                    }
                    return midAutumnRank.GetFirst();
                case RankType.ThemeFirework:
                    ThemeFireworkRank fireworkRank = GetThemeFireworkRank(groupId);
                    if (fireworkRank == null)
                    {
                        AddThemeFireworkRank(groupId);
                        fireworkRank = GetThemeFireworkRank(groupId);
                    }
                    return fireworkRank.GetFirst();
                case RankType.NineTest:
                    NineTestRank nineTestRank = GetNineTestRank(groupId);
                    if (nineTestRank == null)
                    {
                        AddNineTestRank(groupId);
                        nineTestRank = GetNineTestRank(groupId);
                    }
                    return nineTestRank.GetFirst();
                default:
                    break;
            }
            return null;
        }

        public void CheckCrossBossRankScore()
        {
            Log.Warn($"server CheckCrossBossRankScore.............. start");
            foreach (var crossBossRank in CrossBossRankMngList)
            {
                crossBossRank.Value.CheckRankScore();
            }
            Log.Warn($"server CheckCrossBossRankScore.............. end");
        }
    }
}

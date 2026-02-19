using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;

namespace ZoneServerLib
{
    public class CrossInfoManager
    {
        public CrossBattleManagerInfo Info = new CrossBattleManagerInfo();
        private ZoneServerApi server { get; set; }
        private PlayerChar owner { get; set; }

        public PlayerRankBaseInfo TempChallengerInfo { get; set; }
        public CrossInfoManager(PlayerChar owner, ZoneServerApi server)
        {
            this.owner = owner;
            this.server = server;
        }

        public void Init(CrossBattleManagerInfo info)
        {
            this.Info = info;
        }

        public void ChangeLastFinalsRank(int time)
        {
            if (Info != null)
            {
                Info.LastFinalsRank = time;
            }
        }

        public void RefreshDaily()
        {
            Info.ActiveReward = (int)CrossRewardState.None;
            Info.DailyFight = 0;
        }

        public void RefreshSeason()
        {
            CrossLevelInfo levelInfo = CrossBattleLibrary.GetCrossLevelInfo(Info.Level);
            if (levelInfo == null)
            {
                Info.Star = 0;
            }
            else
            {
                Info.Star = levelInfo.ResetStar;
            }
            Info.Level = 1;
            CheckCrossLevelChange();
            Info.WinStreak = 0;
            Info.ActiveReward = 0;
            Info.PreliminaryReward = 0;
            Info.SeasonFight = 0;
            Info.DailyFight = 0;
            Info.BattleTeam = 0;
            server.GameRedis.Call(new OperateUpdateCrossLevel(owner.Uid, Info.Level, Info.Star));
        }
        public void AddWinStreak()
        {
            Info.WinStreak++;
            Info.WinTotal++;

            if (Info.WinStreak == CrossBattleLibrary.WinStreakNum)
            {
                AddStar(true, CrossBattleLibrary.WinStreakStar);
                Info.HistoryWinStreak++;
            }
            else if (Info.WinStreak > CrossBattleLibrary.WinStreakNum)
            {
                Info.WinStreak = 1;
            }
        }

        public void AddFightCount()
        {
            Info.FightTotal++;
            Info.DailyFight++;
            Info.SeasonFight++;
        }

        public void ResetWinStreak()
        {
            Info.WinStreak = 0;
        }
        public void AddStar(bool isWin, int value)
        {
            if (isWin)
            {
                Info.Star += value;
            }
            else
            {
                Info.Star = Math.Max(Info.Star - value, 0);
            }
            Info.HistoryMaxStar = Math.Max(Info.Star, Info.HistoryMaxStar);

            CheckCrossLevelChange();
            //int checkLevel = CheckCrossLevel();
            //if (checkLevel != Info.Level)
            //{
            //    Info.Level = checkLevel;
            //}
            server.GameRedis.Call(new OperateUpdateCrossLevel(owner.Uid, Info.Level, Info.Star));

            owner.ChangeRankScore(RankType.CrossServer, Info.Star);
        }

        public bool CheckCrossLevelChange()
        {
            if (Info.Star > 0)
            {
                int oldLevel = Info.Level;
                CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(Info.Star);
                if (info.Level != oldLevel)
                {
                    Info.Level = info.Level;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Info.Star = 0;
                Info.Level = 1;
                return false;
            }
            //CrossLevelInfo info = CrossBattleLibrary.GetCrossLevelInfo(Info.Level);
            //if (info != null)
            //{
            //    int num = Info.Star - info.AddStar;
            //    if (num <= 0 && Info.Level > 1)
            //    {
            //        Info.Level--;
            //    }
            //    else if (info.LimitStar > 0 && num > info.LimitStar)
            //    {
            //        Info.Level++;
            //    }
            //}
        }

        public void ChargeRank(int rank)
        {
            Info.Rank = rank;

            if (Info.HistoryMaxRank > 0)
            {
                Info.HistoryMaxRank = Math.Min(rank, Info.HistoryMaxRank);
            }
        }

        public void ChargeBattleTeam(int teamId)
        {
            Info.BattleTeam = teamId;
        }
        //public void ChargeRank(int rank, int hisory)
        //{
        //    ChargeRank(rank);
        //    if (Info.HistoryMaxRank > 0)
        //    {
        //        if (hisory > 0)
        //        {
        //            Info.HistoryMaxRank = Math.Min(hisory, Info.HistoryMaxRank);
        //        }
        //    }
        //    else
        //    {
        //        Info.HistoryMaxRank = hisory;
        //    }
        //}

        public void GetActivityReward()
        {
            Info.ActiveReward = (int)CrossRewardState.Get;
        }

        public void GetPreliminaryReward()
        {
            Info.PreliminaryReward = (int)CrossRewardState.Get;
        }
        public void GetServerReward()
        {
            Info.ServerReward = (int)CrossRewardState.Get;
        }

        //public void AddDefensiveHero(int heroId)
        //{
        //    Info.DefensiveHeros.Add(heroId);
        //}

        //public void ClearDefensiveHero()
        //{
        //    Info.DefensiveHeros.Clear();
        //}

        //public int GetDefensiveHeroByIndex(int index)
        //{
        //    if (index < Info.DefensiveHeros.Count)
        //    {
        //        return Info.DefensiveHeros[index];
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        //public string GetDefensiveHeros()
        //{
        //    string heros = string.Empty;

        //    foreach (var heroId in Info.DefensiveHeros)
        //    {
        //        heros += heroId + "|";
        //    }

        //    return heros;
        //}

        public MSG_ZMZ_CROSSBATTLE_INFO GenerateCrossBattleInfo()
        {
            MSG_ZMZ_CROSSBATTLE_INFO msg = new MSG_ZMZ_CROSSBATTLE_INFO();
            msg.Rank = Info.Rank;
            msg.HistoryMaxRank = Info.HistoryMaxRank;
            msg.Level = Info.Level;
            msg.Star = Info.Star;
            msg.TimeKey = Info.LastFinalsRank;
            msg.HistoryMaxStar = Info.HistoryMaxStar;
            msg.FightTotal = Info.FightTotal;
            msg.WinTotal = Info.WinTotal;
            msg.WinStreak = Info.WinStreak;
            msg.HistoryWinStreak = Info.HistoryWinStreak;
            //msg.DefensiveHeros.AddRange(Info.DefensiveHeros);
            msg.ActiveReward = Info.ActiveReward;
            msg.PreliminaryReward = Info.PreliminaryReward;
            msg.ServerReward = Info.ServerReward;
            msg.DailyFight = Info.DailyFight;
            msg.SeasonFight = Info.SeasonFight;

            msg.BossStateReward = owner.CrossBossInfoMng.CounterInfo.PassReward;
            msg.BossRankReward = owner.CrossBossInfoMng.CounterInfo.Score;
            msg.BlessingNum = owner.CrossBossInfoMng.CounterInfo.BlessingNum;
            msg.BlessingMultiple = owner.CrossBossInfoMng.CounterInfo.BlessingMultiple;
            msg.ItemList.AddRange(owner.CrossBossInfoMng.CounterInfo.ItemList);
            msg.HiddenWeaponNum = owner.CrossBossInfoMng.CounterInfo.HiddenWeaponNum;
            msg.HiddenWeaponGet = owner.CrossBossInfoMng.CounterInfo.HiddenWeaponGet;
            msg.HiddenWeaponRingNum = owner.CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum;
            msg.RewardList.AddRange(owner.CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards);
            msg.SeaTreasureNum = owner.CrossBossInfoMng.CounterInfo.SeaTreasureNum;
            msg.SeaTreasureNumRewards.AddRange(owner.CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards);
            return msg;
        }

        public void LoadTransform(MSG_ZMZ_CROSSBATTLE_INFO msg)
        {
            Info.Rank = msg.Rank;
            Info.HistoryMaxRank = msg.HistoryMaxRank;
            Info.Level = msg.Level;
            Info.Star = msg.Star;
            Info.LastFinalsRank = msg.TimeKey;
            Info.HistoryMaxStar = msg.HistoryMaxStar;
            Info.FightTotal = msg.FightTotal;
            Info.WinTotal = msg.WinTotal;
            Info.WinStreak = msg.WinStreak;
            Info.HistoryWinStreak = msg.HistoryWinStreak;
            //Info.DefensiveHeros.AddRange(msg.DefensiveHeros);
            Info.ActiveReward = msg.ActiveReward;
            Info.PreliminaryReward = msg.PreliminaryReward;
            Info.ServerReward = msg.ServerReward;
            Info.DailyFight = msg.DailyFight;
            Info.SeasonFight = msg.SeasonFight;

            owner.CrossBossInfoMng.CounterInfo.PassReward = msg.BossStateReward;
            owner.CrossBossInfoMng.CounterInfo.Score = msg.BossRankReward;

            owner.CrossBossInfoMng.CounterInfo.BlessingNum = msg.BlessingNum;
            owner.CrossBossInfoMng.CounterInfo.BlessingMultiple = msg.BlessingMultiple;
            owner.CrossBossInfoMng.CounterInfo.ItemList.AddRange(msg.ItemList);
            owner.CrossBossInfoMng.CounterInfo.HiddenWeaponNum = msg.HiddenWeaponNum;
            owner.CrossBossInfoMng.CounterInfo.HiddenWeaponGet = msg.HiddenWeaponGet;
            owner.CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards.AddRange(msg.RewardList);
            owner.CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum = msg.HiddenWeaponRingNum;
            owner.CrossBossInfoMng.CounterInfo.SeaTreasureNum = msg.SeaTreasureNum;
            owner.CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards.AddRange(msg.SeaTreasureNumRewards);
        }
    }
}

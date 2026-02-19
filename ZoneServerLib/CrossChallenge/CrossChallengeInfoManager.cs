using System;
using System.Collections.Generic;
using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class CrossChallengeInfoManager
    {
        public CrossChallengeManagerInfo Info = new CrossChallengeManagerInfo();
        private ZoneServerApi server { get; set; }
        private PlayerChar owner { get; set; }

        #region Cache 

        //只是在一场，三轮战斗中有效，每一场战斗都需要重置

        //当前战斗轮次，后端维护，每战斗完一次，后端+1，最大为3
        private int battleRound = 1;
        public int BattleRound => battleRound;

        public EnterMapInfo EnterMapInfo { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int CrossChallengeLastBattleUid { get; set; }
        public PlayerCrossFightInfo LastBattlePlayerInfo { get; set; }


        private List<int> battleResult = new List<int>();
        private List<string> videoPathList = new List<string>();
        public List<int> BattleResult => battleResult;
        public List<string> VideoPathList => videoPathList;

        #endregion


        public CrossChallengeInfoManager(PlayerChar owner, ZoneServerApi server)
        {
            this.owner = owner;
            this.server = server;
        }

        public void Init(CrossChallengeManagerInfo info)
        {
            this.Info = info;
        }

        public void CacheFinalInfo(int winUid, string videoPath)
        {
            battleResult.Add(winUid);
            videoPathList.Add(videoPath);
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

            owner.SyncCrossChallengeManagerMessage();
        }

        public void RefreshSeason()
        {
            CrossLevelInfo levelInfo = CrossChallengeLibrary.GetCrossLevelInfo(Info.Level);
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
            Info.ActiveReward = 0;
            Info.PreliminaryReward = 0;
            Info.SeasonFight = 0;
            Info.DailyFight = 0;
            Info.BattleTeam = 0;
            server.GameRedis.Call(new OperateUpdateCrossChallengeLevel(owner.Uid, Info.Level, Info.Star));
        }

        public void ResetBattleRound()
        {
            battleRound = 1;
            WinCount = 0;
            LoseCount = 0;
            CrossChallengeLastBattleUid = 0;
            LastBattlePlayerInfo = null;
            EnterMapInfo = null;
        }

        public void AddBattleRound()
        {
            battleRound++;
        }

        public void SetBattleRound(int round)
        {
            battleRound = round;
        }

        public void SetOriginalMapInfo(int mapId, int channel, Vec2 pos)
        {
            EnterMapInfo = new EnterMapInfo();
            EnterMapInfo.SetInfo(mapId, channel, pos);
        }

        public void AddFightCount()
        {
            Info.FightTotal++;
            Info.DailyFight++;
            Info.SeasonFight++;
        }

        public void AddStar(bool isWin, int value)
        {
            if (isWin)
            {
                Info.Star += value;
            }
            else
            {
                Info.Star = Math.Max(Info.Star + value, 0);
            }
            Info.HistoryMaxStar = Math.Max(Info.Star, Info.HistoryMaxStar);

            CheckCrossLevelChange();
            server.GameRedis.Call(new OperateUpdateCrossChallengeLevel(owner.Uid, Info.Level, Info.Star));

            owner.ChangeRankScore(RankType.CrossChallenge, Info.Star);
        }

        public bool CheckCrossLevelChange()
        {
            if (Info.Star > 0)
            {
                int oldLevel = Info.Level;
                CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(Info.Star);
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

        public MSG_ZMZ_CROSS_CHALLENGE_INFO GenerateCrossChallengeInfo()
        {
            MSG_ZMZ_CROSS_CHALLENGE_INFO msg = new MSG_ZMZ_CROSS_CHALLENGE_INFO();
            msg.Rank = Info.Rank;
            msg.HistoryMaxRank = Info.HistoryMaxRank;
            msg.Level = Info.Level;
            msg.Star = Info.Star;
            msg.TimeKey = Info.LastFinalsRank;
            msg.HistoryMaxStar = Info.HistoryMaxStar;
            msg.FightTotal = Info.FightTotal;
            msg.WinTotal = Info.WinTotal;
            msg.ActiveReward = Info.ActiveReward;
            msg.PreliminaryReward = Info.PreliminaryReward;
            msg.ServerReward = Info.ServerReward;
            msg.DailyFight = Info.DailyFight;
            msg.SeasonFight = Info.SeasonFight;

            return msg;
        }

        public void LoadTransform(MSG_ZMZ_CROSS_CHALLENGE_INFO msg)
        {
            Info.Rank = msg.Rank;
            Info.HistoryMaxRank = msg.HistoryMaxRank;
            Info.Level = msg.Level;
            Info.Star = msg.Star;
            Info.LastFinalsRank = msg.TimeKey;
            Info.HistoryMaxStar = msg.HistoryMaxStar;
            Info.FightTotal = msg.FightTotal;
            Info.WinTotal = msg.WinTotal;
            Info.ActiveReward = msg.ActiveReward;
            Info.PreliminaryReward = msg.PreliminaryReward;
            Info.ServerReward = msg.ServerReward;
            Info.DailyFight = msg.DailyFight;
            Info.SeasonFight = msg.SeasonFight;
        }
    }
}

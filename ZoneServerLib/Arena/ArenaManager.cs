using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerModels;
using ServerModels.Arena;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ArenaManager
    {
        private ZoneServerApi server { get; set; }
        private PlayerChar owner { get; set; }
        public ArenaManager(PlayerChar owner, ZoneServerApi server)
        {
            this.owner = owner;
            this.server = server;
        }

        #region 属性

        private int rank = 0;
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank
        {
            get
            {
                return rank;
            }
        }

        private int historyMaxRank = 0;
        /// <summary>
        /// 历史最高排名
        /// </summary>
        public int HistoryMaxRank
        {
            get
            {
                return historyMaxRank;
            }
        }

        private int level = 1;
        /// <summary>
        /// 段位
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
        }

        private int score = 0;
        /// <summary>
        /// 分数
        /// </summary>
        public int Score
        {
            get
            {
                return score;
            }
        }

        private int historyMaxScore = 0;
        /// <summary>
        /// 历史最高分数
        /// </summary>
        public int HistoryMaxScore
        {
            get
            {
                return historyMaxScore;
            }
        }

        private int fightTotal = 0;
        /// <summary>
        /// 总战斗数
        /// </summary>
        public int FightTotal
        {
            get
            {
                return fightTotal;
            }
        }

        private int winTotal = 0;
        /// <summary>
        /// 总赢数
        /// </summary>
        public int WinTotal
        {
            get
            {
                return winTotal;
            }
        }

        private int winStreak = 0;
        /// <summary>
        /// 连胜数
        /// </summary>
        public int WinStreak
        {
            get
            {
                return winStreak;
            }
        }

        private int historyWinStreak = 0;
        /// <summary>
        /// 最高连胜数
        /// </summary>
        public int HistoryWinStreak
        {
            get
            {
                return historyWinStreak;
            }
        }

        private DateTime changeChallengerTime = DateTime.MinValue;
        /// <summary>
        /// 刷新时间
        /// </summary>
        public DateTime ChangeChallengerTime
        {
            get
            {
                return changeChallengerTime;
            }
        }

        private DateTime fightTime = DateTime.MinValue;
        /// <summary>
        /// 挑战时间
        /// </summary>
        public DateTime FightTime
        {
            get
            {
                return fightTime;
            }
        }

        //private List<int> defensiveHeros = new List<int>();
        ///// <summary>
        ///// 防守阵容
        ///// </summary>
        //public List<int> DefensiveHeros
        //{
        //    get
        //    {
        //        return defensiveHeros;
        //    }
        //}

        private SortedDictionary<int, int> defensiveHeros = new SortedDictionary<int, int>();

        private List<int> levelReward = new List<int>();
        /// <summary>
        /// 段位奖励
        /// </summary>
        public List<int> LevelReward
        {
            get
            {
                return levelReward;
            }
        }

        public SortedDictionary<int, int> DefensiveHeros
        {
            get
            {
                return defensiveHeros;
            }

            set
            {
                defensiveHeros = value;
            }
        }

        public Dictionary<int, ServerModels.PlayerRankBaseInfo> ChallengerInfolist = new Dictionary<int, ServerModels.PlayerRankBaseInfo>();

        #endregion

        public void Init(ArenaManagerInfo Info)
        {
            this.rank = Info.Rank;
            this.historyMaxRank = Info.HistoryMaxRank;
            this.level = Info.Level;
            this.score = Info.Score;
            this.historyMaxScore = Info.HistoryMaxScore;
            this.fightTotal = Info.FightTotal;
            this.winTotal = Info.WinTotal;
            this.winStreak = Info.WinStreak;
            this.historyWinStreak = Info.HistoryWinStreak;

            SetFightTime(DateTime.Parse(Info.FightTime));

            ClearDefensiveHero();

            string[] defensive = StringSplit.GetArray("|", Info.DefensiveHeros);
            for (int i = 0; i < defensive.Length; i++)
            {
                string[] temp = StringSplit.GetArray(":", defensive[i]);
                int heroId = int.Parse(temp[0]);
                //this.defensiveHeros.Add(heroId);
                if (temp.Length > 1)
                {
                    defensiveHeros.Add(heroId, int.Parse(temp[1]));
                }
                else
                {
                    defensiveHeros.Add(heroId, i + 1);
                }
            }

            string[] rewards = StringSplit.GetArray("|", Info.LevelReward);
            foreach (var item in rewards)
            {
                AddLevelReward(int.Parse(item));
            }
        }

        public void AddWinStreak()
        {
            this.winStreak++;
            this.winTotal++;
            this.fightTotal++;
            historyWinStreak = Math.Max(winStreak, historyWinStreak);
            int winStreakScore = ArenaLibrary.GetWinStreakScore(winStreak);
            AddScore(winStreakScore);
        }
        public void ResetWinStreak()
        {
            this.winStreak = 0;
            this.fightTotal++;
        }
        public void AddScore(int value)
        {
            this.score += value;
            historyMaxScore = Math.Max(score, historyMaxScore);

            int checkLevel = ArenaLibrary.CheckRankLevel(Score);
            if (checkLevel != Level)
            {
                level = checkLevel;
                server.GameRedis.Call(new OperateUpdateLadderLevel(owner.Uid, level));

                owner.SerndUpdateRankValue(RankType.Arena, level);
            }
        }

        public void ChargeRank(int rank)
        {
            this.rank = rank;
        }
        public void ChargeRank(int rank, int  hisory)
        {
            ChargeRank(rank);
            if (HistoryMaxRank > 0)
            {
                if (hisory > 0)
                {
                    historyMaxRank = Math.Min(hisory, HistoryMaxRank);
                }
            }
            else
            {
                historyMaxRank = hisory;
            }
        }

        public void AddLevelReward(int level)
        {
            this.levelReward.Add(level);
        }

        public void SetFightTime(DateTime time)
        {
            this.fightTime = time;
        }

        public void SetChangeChallengerTime()
        {
            this.changeChallengerTime = ZoneServerApi.now;
        }

        public bool CanChangeChallengers()
        {
            TimeSpan time = ZoneServerApi.now - changeChallengerTime;
            if (time.TotalSeconds > ArenaLibrary.ChangeTimeCD)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void AddDefensiveHero(int heroId)
        //{
        //    this.defensiveHeros.Add(heroId);
        //}
        public void AddDefensiveHero(int heroId,int pos)
        {
            //this.defensiveHeros.Add(heroId);
            defensiveHeros[heroId]= pos;
        }

        //public void AddDefensiveHero(string heroId)
        //{
        //    string[] temp = heroId.Split(':');

        //    //this.defensiveHeros.Add(int.Parse(temp[0]));
        //    if (temp.Count() > 1)
        //    {
        //        defensiveHeros.Add(int.Parse(temp[0]), int.Parse(temp[1]));
        //    }
        //}

        public void ClearDefensiveHero()
        {
            defensiveHeros.Clear();
            //HeroPos.Clear();
        }

        public int GetDefensiveHeroByIndex(int index)
        {
            if (index < defensiveHeros.Count)
            {
                return defensiveHeros[index];
            }
            else
            {
                return 0;
            }
        }

        public string GetDefensiveHeros()
        {
            string heros = string.Empty;

            foreach (var kv in defensiveHeros)
            {
                heros += kv .Key+ ":"+ kv.Value + "|";
            }

            return heros;
        }

        //public List<int> GetDefensiveHeroPoses()
        //{
        //    List<int> ret = new List<int>();
        //    foreach (var id in defensiveHeros)
        //    {
        //        ret.Add(HeroPos[id]);
        //    }

        //    return ret;
        //}

        public string GetLevelRewards()
        {
            string reward = string.Empty;

            foreach (var level in levelReward)
            {
                reward += level + "|";
            }

            return reward;
        }

        public int GetDefensiveBattlePower()
        {
            int power = 0;
            foreach (var kv in DefensiveHeros)
            {
                //判断是否拥有这个伙伴
                HeroInfo hero = owner.HeroMng.GetHeroInfo(kv.Key);
                if (hero == null)
                {
                    Logger.Log.Warn("player {0} GetDefensiveBattlePower failed: no such hero {1}", owner.Uid, kv.Key);
                    continue; ;
                }
                power += hero.GetBattlePower();
            }
            return power;
        }

        public PlayerRankBaseInfo GetArenaRankInfo(int uid)
        {
            //ArenaRankInfo info;
            //ChallengerInfolist.TryGetValue(Index, out info);
            foreach (var item in ChallengerInfolist)
            {
                if (item.Value.Uid == uid)
                {
                    return item.Value;
                }
            }
            return null;
        }


        public PlayerRankBaseInfo GetArenaRankInfoByIndex(int index)
        {
            PlayerRankBaseInfo info;
            ChallengerInfolist.TryGetValue(index, out info);
            return info;
        }

        public void SetArenaRankInfoList(Dictionary<int, ServerModels.PlayerRankBaseInfo> list)
        {
            ChallengerInfolist = list;
        }

        public void AddArenaRankInfoList(ServerModels.PlayerRankBaseInfo info)
        {
            ChallengerInfolist[info.Index] = info;
        }
    }
}

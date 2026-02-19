using DataProperty;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class CampLibrary
    {
        #region camp consts

        public static int GainDiamond; //随机加入钻石数
        public static int ChangeCost;
        public static int QualificationCost;
        public static int SaluteLeaderCost;
        public static int SaluteMarshalCost;
        public static int SaluteCommanderCost;
        public static string SaluteLeaderReward;
        public static string SaluteMarshalReward;
        public static string SaluteCommanderReward;
        public static int Compensation;
        public static int IsDouble;
        public static int PopulationGap; //人口差异
        public static int PowerGap; //实力差异
        public static string VoteExpireDate;
        public static int LeaderSkill1;
        public static int LeaderSkill2;
        public static int MarshalSkill1;
        public static string BattleOpeningDate;
        public static string PrestigeRankingDate;

        public static int RunInElectionLevelLimit;    
        public static int BattleRankRefreshTime;
        public static int BattlePerPage;
        public static int BattleFightMax;
        public static int BattleCollectionMax;
        public static int GainExtraGrain;
        public static int GrainInitValue;
        
        public static bool InDebug;

        #endregion

        public static Dictionary<int, PrestigeReward> rankRewards = new Dictionary<int, PrestigeReward>();

        public static Dictionary<int, WorshipBase> worships = new Dictionary<int, WorshipBase>();

        public static Dictionary<int, VoteItem> votes = new Dictionary<int, VoteItem>();      

        public static void Init()
        {
            //rankRewards.Clear();
            //worships.Clear();
            //votes.Clear();       

            Data campConfig = DataListManager.inst.GetData("CampConfig", 1);
            DataList prestigeReWard = DataListManager.inst.GetDataList("CampPrestigeReward");
            DataList worshipData= DataListManager.inst.GetDataList("CampWorship");
            DataList campVote = DataListManager.inst.GetDataList("CampVote");         

            InitConsts(campConfig);
            InitPrestigeReward(prestigeReWard);
            InitWorship(worshipData);
            InitVote(campVote);         
        }

        private static void InitWorship(DataList worshipData)
        {
            Dictionary<int, WorshipBase> worships = new Dictionary<int, WorshipBase>();
            foreach (var item in worshipData)
            {
                WorshipBase worship = new WorshipBase();
                worship.Id = item.Value.ID;
                worship.Name = item.Value.Name;
                worship.Reward = item.Value.GetString("award");
                worship.IsDouble = item.Value.GetBoolean("double");
                worship.Spend = item.Value.GetString("spend");
                worship.ModelId = item.Value.GetInt("modelId");
                worship.HeroId = item.Value.GetInt("heroId");
                worship.Icon = item.Value.GetInt("Icon");
                worship.Level = item.Value.GetInt("Level");
                worship.BattlePower = item.Value.GetInt("BattlePower");
                worship.GenerateInfo();
                worships.Add(worship.Id, worship);
            }
            CampLibrary.worships = worships;
        }

        private static void InitVote(DataList datas)
        {
            Dictionary<int, VoteItem> votes = new Dictionary<int, VoteItem>();
            foreach (var item in datas)
            {
                VoteItem temp = new VoteItem();
                temp.Id = item.Value.ID;
                temp.Ticket = item.Value.GetInt("ticket");
                votes.Add(temp.Id, temp);
            }
            CampLibrary.votes = votes;
        }

        private static void InitConsts(Data data)
        {
            GainDiamond = data.GetInt("GainDiamond");
            ChangeCost = data.GetInt("ChangeCost");
            QualificationCost = data.GetInt("QualificationCost");
            SaluteLeaderCost = data.GetInt("SaluteLeaderCost");
            SaluteMarshalCost = data.GetInt("SaluteMarshalCost");
            SaluteCommanderCost = data.GetInt("SaluteCommanderCost");
            SaluteLeaderReward = data.GetString("SaluteLeaderReward");
            SaluteMarshalReward = data.GetString("SaluteMarshalReward");
            SaluteCommanderReward = data.GetString("SaluteCommanderReward");
            Compensation = data.GetInt("Compensation");
            IsDouble = data.GetInt("IsDouble");
            PopulationGap = data.GetInt("PopulationGap");
            PowerGap = data.GetInt("PowerGap");
            VoteExpireDate = data.GetString("VoteExpireDate");
            LeaderSkill1 = data.GetInt("LeaderSkill1");
            LeaderSkill2 = data.GetInt("LeaderSkill2");
            MarshalSkill1 = data.GetInt("MarshalSkill1");
            BattleOpeningDate = data.GetString("BattleOpeningDate");
            PrestigeRankingDate = data.GetString("PrestigeRankingDate");
            InDebug = data.GetBoolean("InDebug");
            RunInElectionLevelLimit = data.GetInt("VoteLvLimit");         

            BattleRankRefreshTime = data.GetInt("BattleRankRefreshTime");
            BattlePerPage = data.GetInt("BattlePerPage");
            BattleFightMax = data.GetInt("BattleFightMax");
            BattleCollectionMax = data.GetInt("BattleCollectionMax");
            GainExtraGrain = data.GetInt("GainExtraGrain");
            GrainInitValue = data.GetInt("InitGrain");
            
        }

        private static void InitPrestigeReward(DataList data)
        {
            Dictionary<int, PrestigeReward> rankRewards = new Dictionary<int, PrestigeReward>();
            foreach (var item in data)
            {
                PrestigeReward reward = new PrestigeReward();
                reward.Id = item.Value.ID;
                reward.Rank = item.Value.GetString("Rank");
                reward.Reward = item.Value.GetString("Reward");
                reward.GenerateRank();
                rankRewards.Add(reward.Id, reward);
            }
            CampLibrary.rankRewards = rankRewards;
        }       

        #region 对外方法提供

        public static WorshipBase GetWorshipBase(int rank)
        {
            WorshipBase temp = null;
            worships.TryGetValue(rank, out temp);
            return temp;

        }

        public static PrestigeReward GetReward(int rank)
        {
            PrestigeReward reward = null;
            foreach (var item in rankRewards)
            {
                if (item.Value.RankLow != -1)
                {
                    if (rank >= item.Value.RankHigh && rank <= item.Value.RankLow)
                    {
                        reward = item.Value;
                        break;
                    }
                }
                else
                {
                    if (rank >= item.Value.RankHigh)
                    {
                        reward = item.Value;
                        break;
                    }
                }
            }

            return reward;
        }

        public static VoteItem GetVoteItem(int id)
        {
            VoteItem item = null;
            votes.TryGetValue(id, out item);
            return item;
        }
         
        #endregion
    }

    public class PrestigeReward
    {
        public int Id { get; set; }
        public string Rank { get; set; }
        public string Reward { get; set; }

        public int RankHigh;
        public int RankLow;

        public void GenerateRank()
        {
            string[] infos = Rank.Split(':');
            RankHigh = int.Parse(infos[0]);
            RankLow = int.Parse(infos[1]);
        }
    }

    public class WorshipBase
    {
        public int Id;
        public string Name;
        public string Reward;
        public bool IsDouble;
        public string Spend;
        public int ModelId;
        public int HeroId;
        public int Icon;
        public int Level;
        public int BattlePower;

        public CurrenciesType Type;
        public int CurrencyCount;
        public void GenerateInfo()
        {
            string[] spend = Spend.Split(':');
            Type = (CurrenciesType)int.Parse(spend[0]);
            CurrencyCount = int.Parse(spend[1]);
        }
    }

    public class VoteItem
    {
        public int Id { get; set; }
        public int Ticket { get; set; }
    }
}

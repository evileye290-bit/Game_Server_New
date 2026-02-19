using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class RouletteManager : BasePeriodActivity
    {
        private RouletteInfo info;

        public RouletteInfo RouletteInfo => info;

        public int Score
        {
            get { return info.Score; }
            set { info.Score = value; }
        }


        public PlayerChar Owner { get; }

        public RouletteManager(PlayerChar owner) : base(RechargeGiftType.Roulette)
        {
            Owner = owner;
        }

        public void Init(RouletteInfo info)
        {
            this.info = info;
            InitPeriodInfo();

            if (Period > 0 && info.IdList.Count == 0)
            {
                RandGroupItems();
            }
        }

        public void AddScore(int score)
        {
            info.Score += score;
            SyncUpdateToDB();
            UpdateRank();
        }

        public void Refresh()
        {
            RandGroupItems();
        }

        public void RandGroupItems()
        {
            List<int> idList = RouletteLibrary.RandomGroupList(Period);
            if (idList.Count == 0)
            {
                Log.Error($"have not roulette info period {Period}, check it !");
                return;
            }

            info.IdList.Clear();
            info.IdList.AddRange(idList);

            SyncUpdateToDB();
        }

        public override void Clear()
        {
            info.Reset();
            InitPeriodInfo();
            RandGroupItems();
            SyncUpdateToDB();
            Owner.Write(GenerateRouletteInfo());
        }

        public bool CheckItems(List<RouletteItemModel> models, out int weight)
        {
            weight = 0;
            if (info.IdList.Count == 0)
            {
                if(!CheckPeriodInfo())
                    return false;
                Owner.RouletteRefresh(false);
            }

            foreach (var id in info.IdList)
            {
                RouletteItemModel model = RouletteLibrary.GeItemModel(id);
                if (model == null || model.Period != Period)
                {
                    Log.Error("Roulette item error ");
                    return false;
                }

                weight += model.RandomWeight;
                models.Add(model);
            }

            return true;
        }

        public bool AddRewardId(int id)
        {
            if (!info.RewardList.Contains(id))
            {
                info.RewardList.Add(id);
                SyncUpdateToDB();
                return true;
            }
            return false;
        }

        public int MaxScoreRewardId()
        {
            return info.RewardList.Count > 0 ? info.RewardList.Max() : 0;
        }

        public MSG_ZGC_ROULETTE_GET_INFO GenerateRouletteInfo()
        {
            MSG_ZGC_ROULETTE_GET_INFO msg = new MSG_ZGC_ROULETTE_GET_INFO();
            msg.Score = info.Score;
            msg.IdList.Add(info.IdList);
            msg.RewardId = MaxScoreRewardId();
            return msg;
        }

        public MSG_ZMZ_ROULETTE_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_ROULETTE_INFO msg = new MSG_ZMZ_ROULETTE_INFO {Score = Score};
            msg.IdList.AddRange(info.IdList);
            msg.RewardId.AddRange(info.RewardList);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_ROULETTE_INFO info)
        {
            RouletteInfo gardenInfo = new RouletteInfo
            {
                Score = info.Score, 
                IdList = new List<int>(info.IdList), 
                RewardList = new List<int>(info.RewardId)
            };

            this.info = gardenInfo;

            InitPeriodInfo();
        }

        private void UpdateRank()
        {
           Owner.SerndUpdateRankValue(RankType.Roulette, Score);
        }

        private void SyncUpdateToDB()
        {
            QueryUpdateRoulette query = new QueryUpdateRoulette(Owner.Uid, info);
            Owner.server.GameDBPool.Call(query);
        }
    }
}

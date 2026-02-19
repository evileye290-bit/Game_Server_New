using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MidAutumnManager
    {
        private PlayerChar owner { get; set; }

        private MidAutumnInfo info = new MidAutumnInfo();
        public MidAutumnInfo Info { get { return info; } }

        public MidAutumnManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(MidAutumnInfo info)
        {
            this.info = info;          
            //如没有折扣更新为默认折扣
            UpdateDiscountInfo();
        }      

        private void UpdateDiscountInfo()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.MidAutumn, ZoneServerApi.now, out model))
            {
                int period = model.SubType;
                MidAutumnConfig config = MidAutumnLibrary.GetConfig(period);
                if (config != null)
                {
                    UpdateDefaultDiscount(config.DefaultDiscount);
                    UpdateCurSpecialDiscountRatio(config.SpecialDiscountRatios);
                }
            }
        }

        private void UpdateDefaultDiscount(int discount)
        {
            if (info.CurDiscount == 0)
            {
                info.CurDiscount = discount;
            }
        }

        private void UpdateCurSpecialDiscountRatio(List<int> specialDiscountRatios)
        {
            if (info.CurSpecialDiscountRatio == 0)
            {
                info.CurSpecialDiscountRatio = specialDiscountRatios.First();
            }
        }

        public void UpdateDrawInfo(bool useFree, int score, MidAutumnConfig config)
        {
            if (useFree)
            {
                info.FreeUsed = true;
            }
            else
            {
                int nextSpecialRatio;
                info.CurDiscount = config.RandDiscount(info.CurSpecialDiscountRatio, out nextSpecialRatio);
                info.CurSpecialDiscountRatio = nextSpecialRatio;
            }
            info.Score += score;

            SyncDbUpdateMidAutumnDrawInfo();
        }

        public void UpdateItemExchangeCount(int id, int num)
        {
            if (!info.ItemExchangeCounts.ContainsKey(id))
            {
                info.ItemExchangeCounts.Add(id, num);
            }
            else
            {
                info.ItemExchangeCounts[id] += num;//
            }           
            SyndDbUpdateItemExchangeCount();
        }

        public void UpdateScoreRewards(int rewardId)
        {
            info.ScoreRewards.Add(rewardId);
            SyncDbUpdateScoreRewards();
        }

        public void UpdateFreeFlag(bool freeUsed)
        {
            info.FreeUsed = freeUsed;
            SyncDbUpdateFreeFlag();
        }

        public void Clear()
        {
            info.Clear();           
            UpdateDiscountInfo();
        }

        private void SyncDbUpdateMidAutumnDrawInfo()
        {
            owner.server.GameDBPool.Call(new QueryUpdateMidAutumnDrawInfo(owner.Uid, info.Score, info.CurDiscount, info.FreeUsed, info.CurSpecialDiscountRatio));
        }

        private void SyncDbUpdateScoreRewards()
        {
            owner.server.GameDBPool.Call(new QueryUpdateMidAutumnScoreRewards(owner.Uid, string.Join("|", info.ScoreRewards)));
        }

        private void SyndDbUpdateItemExchangeCount()
        {
            owner.server.GameDBPool.Call(new QueryUpdateMidAutumnItemExchangeCount(owner.Uid, info.ItemExchangeCounts.ToString("|", ":")));
        }

        private void SyncDbUpdateFreeFlag()
        {
            owner.server.GameDBPool.Call(new QueryUpdateMidAutumnFreeUsed(owner.Uid, info.FreeUsed));
        }

        public MSG_ZMZ_MIDAUTUMN_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_MIDAUTUMN_INFO msg = new MSG_ZMZ_MIDAUTUMN_INFO();
            msg.Score = info.Score;
            msg.CurDiscount = info.CurDiscount;
            msg.FreeUsed = info.FreeUsed;
            msg.ScoreRewards.AddRange(info.ScoreRewards);
            foreach (var kv in info.ItemExchangeCounts)
            {
                msg.ItemExchangeCounts.Add(kv.Key, kv.Value);
            }
            msg.CurSpecialDiscountRatio = info.CurSpecialDiscountRatio;
            return msg;
        }

        public void LoadTransformMsg(MSG_ZMZ_MIDAUTUMN_INFO msg)
        {
            info.Score = msg.Score;
            info.CurDiscount = msg.CurDiscount;
            info.FreeUsed = msg.FreeUsed;
            info.ScoreRewards.AddRange(msg.ScoreRewards);
            foreach (var kv in msg.ItemExchangeCounts)
            {
                info.ItemExchangeCounts.Add(kv.Key, kv.Value);
            }
            info.CurSpecialDiscountRatio = msg.CurSpecialDiscountRatio;
        }
    }
}

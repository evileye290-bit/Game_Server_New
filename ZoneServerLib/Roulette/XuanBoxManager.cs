using System.Collections.Generic;
using System.Linq;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;

namespace ZoneServerLib
{
    public class XuanBoxManager : BasePeriodActivity
    {
        private XuanBoxInfo info;
        private PlayerChar owner;

        public XuanBoxInfo XuanBoxInfo => info;

        public int Lucky
        {
            get { return info.Lucky; }
            set { info.Lucky = value; }
        }

        public int LuckySum
        {
            get { return info.LuckySum; }
            set { info.LuckySum = value; }
    }

        public XuanBoxManager(PlayerChar owner) : base(RechargeGiftType.XuanBox)
        {
            this.owner = owner;
        }

        public void Init(XuanBoxInfo info)
        {
            this.info = info;
        }

        public void AddLucky(int lucky, int luckySum)
        {
            info.Lucky = lucky;
            info.LuckySum += luckySum;
            SyncUpdateToDB();
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

        public override void Clear()
        {
            info.Reset();
            SyncUpdateToDB();
            InitPeriodInfo();
            owner.GetXuanBoxInfoByLoading();
        }

        public int MaxScoreRewardId()
        {
            return info.RewardList.Count > 0 ? info.RewardList.Max() : 0;
        }

        public MSG_ZGC_XUANBOX_GET_INFO GenerateInfo()
        {
            MSG_ZGC_XUANBOX_GET_INFO msg = new MSG_ZGC_XUANBOX_GET_INFO
            {
                Lucky = info.Lucky,
                LuckySum = info.LuckySum,
            };
            msg.RewardId.Add(info.RewardList);
            return msg;
        }

        public MSG_ZMZ_XUANBOX_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_XUANBOX_INFO msg = new MSG_ZMZ_XUANBOX_INFO { Lucky = Lucky, LuckySum = LuckySum};
            msg.RewardId.AddRange(info.RewardList);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_XUANBOX_INFO info)
        {
            XuanBoxInfo gardenInfo = new XuanBoxInfo
            {
                Lucky = info.Lucky,
                LuckySum = info.LuckySum,
                RewardList = new List<int>(info.RewardId)
            };

            this.info = gardenInfo;
            CheckPeriodInfo();
        }

        private void SyncUpdateToDB()
        {
            QueryUpdateXuanBox query = new QueryUpdateXuanBox(owner.Uid, info);
            owner.server.GameDBPool.Call(query);
        }
    }
}
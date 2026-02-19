using System.Collections.Generic;
using System.Linq;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class WishLanternManager : BasePeriodActivity
    {
        private WishLanternInfo info;
        private PlayerChar owner;

        public WishLanternInfo WishLanternInfo => info;

        public WishLanternManager(PlayerChar owner) : base(RechargeGiftType.WishLantern)
        {
            this.owner = owner;
        }

        public void Init(WishLanternInfo info)
        {
            this.info = info;
        }

        public void SetSelectReward(int index)
        {
            info.BoxIndex = index;
            //info.LanternIndex.Clear();
            SyncUpdateToDB();
        }

        public void AddLanternLightIndex(int index, bool sync)
        {
            info.LanternIndex.Add(index);
            if (sync)
            {
                SyncUpdateToDB();
            }
        }

        public bool AddBoxRewardIndex(int index)
        {
            bool reset = false;
            info.RewardBoxIndex.Add(index);
            if (CheckGotAllReward())
            {
                reset = true;
                Reset(1, false);
            }
            SyncUpdateToDB();
            return reset;
        }

        public bool CheckGotAllReward()
        {
            return info.RewardBoxIndex.Count >= WishLanternLibrary.GetBoxItemCount(Period);
        }

        public void Reset(int addCount, bool sync, bool clearRewardBox = true)
        {
            info.ResetCount += addCount;
            info.BoxIndex = 0;
            info.LanternIndex.Clear();

            if (clearRewardBox)
            {
                info.RewardBoxIndex.Clear();
            }
            if (sync)
            {
                SyncUpdateToDB();
            }
        }

        public void Clear()
        {
            info.BoxIndex = 0;
            info.ResetCount = 0;
            info.LanternIndex.Clear();
            info.RewardBoxIndex.Clear();
            owner.SendWishLanternInfo();
        }

        public MSG_ZGC_WISH_LANTERN_INFO GenerateInfo()
        {
            MSG_ZGC_WISH_LANTERN_INFO msg = new MSG_ZGC_WISH_LANTERN_INFO
            {
                BoxIndex = info.BoxIndex,
                ResetCount = info.ResetCount,
            };
            msg.LanternIndex.Add(info.LanternIndex);
            msg.RewardBoxIndex.Add(info.RewardBoxIndex);
            return msg;
        }

        public MSG_ZMZ_WISH_LANTERN GenerateTransformMsg()
        {
            MSG_ZMZ_WISH_LANTERN msg = new MSG_ZMZ_WISH_LANTERN { BoxIndex = info.BoxIndex, ResetCount = info.ResetCount };
            msg.LanternIndex.AddRange(info.LanternIndex);
            msg.RewardBoxIndex.AddRange(info.RewardBoxIndex);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_WISH_LANTERN info)
        {
            WishLanternInfo gardenInfo = new WishLanternInfo
            {
                BoxIndex = info.BoxIndex,
                ResetCount = info.ResetCount,
                LanternIndex = new List<int>(info.LanternIndex),
                RewardBoxIndex = new List<int>(info.RewardBoxIndex),
            };

            this.info = gardenInfo;
            CheckPeriodInfo();
        }

        public void SyncUpdateToDB()
        {
            QueryUpdateWishLantern query= new QueryUpdateWishLantern(owner.Uid, info);
            owner.server.GameDBPool.Call(query);
        }
    }
}
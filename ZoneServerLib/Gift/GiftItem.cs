using DataProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class GiftItem
    {
        public ulong Uid { get; private set; }
        public int Id { get; private set; }
        public int BuyCount { get; private set; }
        public int CurBuyCount { get; private set; }
        /// <summary>
        /// 之前用于记录首充翻倍状态，现用于记录首充状态
        /// </summary>
        public int DoubleFlag { get; private set; }
        public int Discount { get; private set; }
        public string StartTime { get; private set; }
        
        public int DiamondRatio { get; set; }
        public bool IsSdkGift { get; private set; }
        public string DataBox { get; private set; }

        public GiftItem(int id, int buyCount, int curBuyCount, int doubleFlag, int discount, int diamondRatio, bool isSdkGift = false, string dataBox = "")
        {
            Id = id;
            BuyCount = buyCount;
            CurBuyCount = curBuyCount;
            DoubleFlag = doubleFlag;
            Discount = discount;
            DiamondRatio = diamondRatio;
            IsSdkGift = isSdkGift;
            DataBox = dataBox;
        }

        public GiftItem(ulong uid, int id, int buyCount, string startTime, bool isSdkGift = false, string dataBox = "")
        {
            Uid = uid;
            Id = id;
            BuyCount = buyCount;
            StartTime = startTime;
            IsSdkGift = isSdkGift;
            DataBox = dataBox;
        }

        public void UpdateBuyCount()
        {
            BuyCount++;
            CurBuyCount++;
        }
        
        public void ResetDoubleFlag()
        {
            DoubleFlag = 1;
        }

        public bool GetDoubleFlag()
        {
            if (DoubleFlag == 0)
            {
                return false;
            }
            return true;
        }

        public void CheckChangeDoubleFlag()
        {
            if (DoubleFlag == 1)
            {
                DoubleFlag = 0;
            }
        }

        public void ReSetItemCurBuyCount()
        {
            CurBuyCount = 0;
        }

        public void ResetDiscount()
        {
            Discount = 1;
        }

        public bool GetDiscount()
        {
            if (Discount == 0)
            {
                return false;
            }
            return true;
        }

        public void CheckChangeDiscount()
        {
            if (Discount == 1)
            {
                Discount = 0;
            }
        }

        public void ResetDiamondRatio()
        {
            DiamondRatio = 2;
        }
        
        public void CheckChangeDiamondRatio()
        {
            if (DiamondRatio != 1)
            {
                DiamondRatio = 1;
            }
        }
    }
}

/*************************************************
    文 件 : DomainBenedictionManager.cs
    日 期 : 2022年4月6日 17:52:41
    作 者 : jinzi
    策 划 : 
    说 明 : 神域赐福管理
*************************************************/

using System;
using System.Collections.Generic;
using ServerModels.DomainBenediction;

namespace ZoneServerLib.DomainBenediction
{
    public class DomainBenedictionManager
    {
        #region [成员变量]

        /*\ 玩家信息 /*/
        private PlayerChar owner;

        /*\ 当前成功次数 /*/
        private int iCurrSuccNum;

        /*\ 当前免费次数 /*/
        private int iCurrFreeNum;

        /*\ 当前打折次数 /*/
        private int iCurrHalfNum;

        /*\ 当前积分数 /*/
        private int iCurrIntegralNum;

        /*\ 当前领取过的积分奖励id /*/
        private List<int> lstGetAwardId;

        /*\ 基础奖励 /*/
        private string strBaseAward;

        /*\ 当前抽卡类型 /*/
        private int curDrawType;

        /*\ 上一次祈福失敗标记 /*/
        private bool lastPrayFailed;
        #endregion

        #region [构造函数]

        public DomainBenedictionManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        #endregion

        #region [db]

        /// <summary>
        /// 数据库数据转换
        /// </summary>
        /// <param name="oDbInfo"></param>
        public void Init(DbDomainBenedictionInfo oDbInfo)
        {
            if (oDbInfo == null)
            {
                return;
            }

            iCurrSuccNum = oDbInfo.ICurrSuccNum;
            iCurrFreeNum = oDbInfo.ICurrFreeNum;
            iCurrHalfNum = oDbInfo.ICurrHalfNum;
            iCurrIntegralNum = oDbInfo.ICurrIntegralNum;
            lstGetAwardId = oDbInfo.LstGetAwardId;
            strBaseAward = oDbInfo.StrBaseAward;
            curDrawType = oDbInfo.CurDrawType;
        }

        #endregion

        #region [Get Set]

        public PlayerChar Owner
        {
            get => owner;
            set => owner = value;
        }

        public int ICurrSuccNum
        {
            get => iCurrSuccNum;
            set => iCurrSuccNum = value;
        }

        public int ICurrFreeNum
        {
            get => iCurrFreeNum;
            set => iCurrFreeNum = value;
        }

        public int ICurrHalfNum
        {
            get => iCurrHalfNum;
            set => iCurrHalfNum = value;
        }

        public int ICurrIntegralNum
        {
            get => iCurrIntegralNum;
            set => iCurrIntegralNum = value;
        }

        public List<int> LstGetAwardId
        {
            get => lstGetAwardId;
            set => lstGetAwardId = value;
        }

        public string StrBaseAward
        {
            get => strBaseAward;
            set => strBaseAward = value;
        }

        public int CurDrawType
        {
            get => curDrawType;
            set => curDrawType = value;
        }

        public bool LastPrayFailed
        {
            get => lastPrayFailed;
            set => lastPrayFailed = value;
        }
        
        #endregion

        #region [接口函数]

        /// <summary>
        /// 每日次数重置
        /// </summary>
        public void ClearEveryDay()
        {
            iCurrFreeNum = 0;
            iCurrHalfNum = 0;
        }

        /// <summary>
        /// 根据倍数计算基础奖励
        /// </summary>
        /// <returns></returns>
        public string ClacBaseAward()
        {
            string strAward = string.Empty;
            if (string.IsNullOrEmpty(strBaseAward))
            {
                return strAward;
            }

            string[] strArr = strBaseAward.Split(':');
            if (strArr.Length < 3)
            {
                return strAward;
            }

            int iBaseNum = 0;
            int.TryParse(strArr[2], out iBaseNum);
            int iNum = iBaseNum * (int) Math.Pow(2, ICurrSuccNum);
            strAward = string.Format($"{strArr[0]}:{strArr[1]}:{iNum}");
            return strAward;
        }

        /// <summary>
        /// 重置所有数据
        /// </summary>
        public void ResetAll()
        {
            iCurrSuccNum = 0;
            iCurrFreeNum = 0;
            iCurrHalfNum = 0;
            iCurrIntegralNum = 0;
            lstGetAwardId.Clear();
            strBaseAward = string.Empty;
            curDrawType = 0;
        }
        #endregion
    }
}
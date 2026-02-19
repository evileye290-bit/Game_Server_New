/*************************************************
    文 件 : DomainBenedictionLibrary.cs
    日 期 : 2022年4月6日 14:52:23
    作 者 : jinzi
    策 划 : 费腾
    说 明 : 神域赐福表预处理相关
*************************************************/

using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DataProperty;
using EnumerateUtility.DomainBenediction;
using ServerModels.DomainBenediction;

namespace ServerShared
{
    public class DomainBenedictionLibrary
    {
        #region [table_DomainBenedictionConfig]

        public static int iFreeNumDay; /*\ 每日免费次数 /*/
        public static int iHalfFareNum; /*\ 每日半价次数 /*/
        public static string strTitleAward; /*\ 称号奖励 /*/
        public static int iTitleSuccNum; /*\ 获取称号连续成功次数 /*/
        public static int iNotictNum; /*\ 抽取多少次发送公告 /*/
        public static int iMaxNum; /*\ 赐福最大次数 /*/
        public static int iMinDiscount; /*\ 单抽折扣最小 /*/
        public static int iMaxDisCount; /*\ 单抽折扣最大 /*/

        public static List<int> lstProbability = new List<int>(); /*\ 赐福成功概率 万分比 /*/

        #endregion

        #region [table_DomainBenedictionType]

        /*\ <抽取类型, 数据> /*/
        public static Dictionary<EnumDomainBenedictionType, DomainBenedictionTypeModel> dicDomainBenedictionType =
            new Dictionary<EnumDomainBenedictionType, DomainBenedictionTypeModel>();

        #endregion

        #region [table_DomainBenedictionStageAward]

        /*\ <表id, 数据> /*/
        public static Dictionary<int, DomainBenedictionStageAwardModel> dicDomainBenedictionStageAward =
            new Dictionary<int, DomainBenedictionStageAwardModel>();

        #endregion

        #region [table_DomainBenedictionPrayProb]

        private static DoubleDepthMap<int, int, int> prayProbBonusDic = new DoubleDepthMap<int, int, int>();

        #endregion
        
        #region [初始化函数]

        /// <summary>
        /// 初始化表信息
        /// </summary>
        public static void Init()
        {
            initConfig();
            initType();
            initStageAward();
            initPrayProbBonus();
        }

        /// <summary>
        /// 预处理 DomainBenedictionConfig表
        /// </summary>
        private static void initConfig()
        {
            DataList oXmlDataList = DataListManager.inst.GetDataList("DomainBenedictionConfig");
            if (oXmlDataList == null)
            {
                return;
            }

            Data oData = oXmlDataList.Get(1);
            if (oData == null)
            {
                return;
            }

            iFreeNumDay = oData.GetInt("FreeNumDay");
            iHalfFareNum = oData.GetInt("HalfFareNum");
            strTitleAward = oData.GetString("ReawardTitle");
            iTitleSuccNum = oData.GetInt("TitleSuccNum");
            iNotictNum = oData.GetInt("NoticeNum");
            iMaxNum = oData.GetInt("MaxNum");
            iMinDiscount = oData.GetInt("DisCountMin");
            iMaxDisCount = oData.GetInt("DisCountMax");

            List<int> lstRandom = new List<int>();
            var lstStr = oData.GetStringList("Probability", "|");
            foreach (var info in lstStr)
            {
                int iProbability = 0;
                int.TryParse(info, out iProbability);
                lstRandom.Add(iProbability);
            }

            lstProbability = lstRandom;
        }

        /// <summary>
        /// 预处理 DomainBenedictionType表
        /// </summary>
        private static void initType()
        {
            DataList oXmlDataList = DataListManager.inst.GetDataList("DomainBenedictionType");
            if (oXmlDataList == null)
            {
                return;
            }

            Dictionary<EnumDomainBenedictionType, DomainBenedictionTypeModel> dicTableInfo =
                new Dictionary<EnumDomainBenedictionType, DomainBenedictionTypeModel>();

            foreach (var info in oXmlDataList)
            {
                DomainBenedictionTypeModel oInfo = new DomainBenedictionTypeModel(info.Value);
                if (!dicTableInfo.ContainsKey((EnumDomainBenedictionType) info.Key))
                {
                    dicTableInfo.Add((EnumDomainBenedictionType) info.Key, oInfo);
                }
            }

            dicDomainBenedictionType = dicTableInfo;
        }

        /// <summary>
        /// 预处理 DomainBenedictionStageAward
        /// </summary>
        private static void initStageAward()
        {
            DataList oXmlDataList = DataListManager.inst.GetDataList("DomainBenedictionStageAward");
            if (oXmlDataList == null)
            {
                return;
            }
            
            Dictionary<int, DomainBenedictionStageAwardModel> dicTableInfo =
                new Dictionary<int, DomainBenedictionStageAwardModel>();

            foreach (var info in oXmlDataList)
            {
                DomainBenedictionStageAwardModel oInfo = new DomainBenedictionStageAwardModel(info.Value);
                if (!dicTableInfo.ContainsKey(info.Key))
                {
                    dicTableInfo.Add(info.Key, oInfo);
                }
            }

            dicDomainBenedictionStageAward = dicTableInfo;
        }

        /// <summary>
        /// 预处理 DomainBenedictionPrayProbBonus
        /// </summary>
        private static void initPrayProbBonus()
        {
            DataList oXmlDataList = DataListManager.inst.GetDataList("DomainBenedictionPrayProbBonus");
            if (oXmlDataList == null)
            {
                return;
            }
            
            DoubleDepthMap<int, int, int> prayProbBonusDic = new DoubleDepthMap<int, int, int>();

            foreach (var info in oXmlDataList)
            {
                int probType = info.Value.GetInt("ProbType");
                int rechargeDiamond = info.Value.GetInt("RechargeDiamond");
                int probBonus = info.Value.GetInt("ProbBonus");
                prayProbBonusDic.Add(probType, rechargeDiamond, probBonus);
            }

            DomainBenedictionLibrary.prayProbBonusDic = prayProbBonusDic;
        }
        #endregion

        #region [接口函数]

        /// <summary>
        /// 根据当前次数返回随机概率
        /// </summary>
        /// <param name="iNum">当前次数默认从0开始</param>
        /// <returns></returns>
        public static int GetProbabilityWithNum(int iNum)
        {
            if (lstProbability.Count <= 0)
            {
                return -1;
            }

            if (lstProbability.Count <= iNum)
            {
                iNum = lstProbability.Count - 1;
            }

            return lstProbability[iNum];
        }

        /// <summary>
        /// 随机奖励信息
        /// </summary>
        /// <param name="eType">单抽还是十连抽</param>
        /// <returns></returns>
        public static List<string> RandomAwardInfo(EnumDomainBenedictionType eType)
        {
            if (!dicDomainBenedictionType.TryGetValue(eType, out var oInfo))
            {
                return null;
            }

            return oInfo.RandomAwardInfo();
        }

        /// <summary>
        /// 获取阶段奖励信息
        /// </summary>
        /// <param name="iTbId">表id</param>
        /// <returns></returns>
        public static DomainBenedictionStageAwardModel GetStageAward(int iTbId)
        {
            if (!dicDomainBenedictionStageAward.TryGetValue(iTbId, out var oInfo))
            {
                return null;
            }

            return oInfo;
        }

        /// <summary>
        /// 获取类型表
        /// </summary>
        /// <param name="eType"></param>
        /// <returns></returns>
        public static DomainBenedictionTypeModel GetDomainBenedictionTypeInfo(EnumDomainBenedictionType eType)
        {
            if (!dicDomainBenedictionType.TryGetValue(eType, out var oInfo))
            {
                return null;
            }

            return oInfo;
        }

        /// <summary>
        /// 随机折扣
        /// </summary>
        /// <returns></returns>
        public static int RandomDiscount()
        {
            return RAND.Range(iMinDiscount, iMaxDisCount);
        }

        /// <summary>
        /// 获取祈福概率加成
        /// </summary>
        /// <param name="probType"></param>
        /// <param name="rechargePrice"></param>
        /// <returns></returns>
        public static int GetDomainBenedictionPrayProbBonus(int probType, int rechargeDiamond)
        {
            int probBonus = 0;
            int curRechargeLevel = 0;
            Dictionary<int, int> dic;
            if (prayProbBonusDic.TryGetValue(probType, out dic))
            {
                foreach (var kv in dic.OrderBy(x=>x.Key))
                {
                    if (kv.Key <= rechargeDiamond)
                    {
                        curRechargeLevel = kv.Key;
                    }
                    else
                    {
                        break;
                    }
                }

                dic.TryGetValue(curRechargeLevel, out probBonus);
            }
            return probBonus;
        }
        #endregion
    }
}
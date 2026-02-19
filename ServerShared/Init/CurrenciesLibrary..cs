using DataProperty;
using EnumerateUtility;
using ServerModels;
using System.Collections.Generic;
namespace ServerShared
{
    public class CurrenciesLibrary
    {
        public static List<int> Ids = new List<int>();
        public static List<string> Names = new List<string>();
        public static Dictionary<int, int> MaxNum = new Dictionary<int, int>();//货币累计上限
     
        private static List<int> currencyIds = new List<int>();
        private static List<string> currencyNames = new List<string>();
        private static Dictionary<int, int> carryMaxNum = new Dictionary<int, int>();//携带货币数量上限

        public static void Init()
        {
            List<int> Ids = new List<int>();
            List<string> Names = new List<string>();
            Dictionary<int, int> MaxNum = new Dictionary<int, int>();//货币累计上限
            //Ids.Clear();
            //Names.Clear();
            //MaxNum.Clear();

            DataList dataList = DataListManager.inst.GetDataList("Currencies");
            foreach (var item in dataList)
            {
                Ids.Add(item.Value.ID);
                Names.Add(item.Value.Name);

                int maxNum = item.Value.GetInt("maxNum");
                if (maxNum > 0)
                {
                    MaxNum.Add(item.Value.ID, maxNum);
                }
            }
            CurrenciesLibrary.Ids = Ids;
            CurrenciesLibrary.Names = Names;
            CurrenciesLibrary.MaxNum = MaxNum;

            InitCurrenciesExtension();
        }

        private static void InitCurrenciesExtension()
        {
            List<int> currencyIds = new List<int>();
            List<string> currencyNames = new List<string>();
            Dictionary<int, int> carryMaxNum = new Dictionary<int, int>();
         
            DataList dataList = DataListManager.inst.GetDataList("WareHouseCurrencies");
            foreach (var item in dataList)
            {
                currencyIds.Add(item.Value.ID);
                currencyNames.Add(item.Value.Name);

                int maxNum = item.Value.GetInt("CarryMaxNum");
                if (maxNum > 0)
                {
                    carryMaxNum.Add(item.Value.ID, maxNum);
                }
            }

            CurrenciesLibrary.currencyIds = currencyIds;
            CurrenciesLibrary.currencyNames = currencyNames;
            CurrenciesLibrary.carryMaxNum = carryMaxNum;
        }

        public static int GetMaxNum(int currenciesType)
        {
            int maxNum = 0;
            if (MaxNum.TryGetValue(currenciesType, out maxNum))
            {
                return maxNum;
            }
            return int.MaxValue;
        }

        public static string GetUpdateSql(Dictionary<CurrenciesType, int> currencies, int pcUid)
        {
            string sqlString = string.Empty;
            if (currencies.Count > 0)
            {
                string parameter = string.Empty;
                foreach (var item in currencies)
                {
                    parameter += string.Format(", `{0}` = {1}", item.Key.ToString(), item.Value);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);


                if (!string.IsNullOrEmpty(parameter))
                {
                    string sqlBase = @"	UPDATE `character_resource` SET  {0}  WHERE `uid` = {1};";
                    sqlString = string.Format(sqlBase, parameter, pcUid);
                }
            }

            return sqlString;
        }


        public static string GetUpdateSqlIncrement(Dictionary<CurrenciesType, int> currencies, int pcUid)
        {
            string sqlString = string.Empty;
            if (currencies.Count > 0)
            {
                string parameter = string.Empty;
                foreach (var item in currencies)
                {
                    parameter += string.Format(", `{0}` = {0}+{1}", item.Key.ToString(), item.Value);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);


                if (!string.IsNullOrEmpty(parameter))
                {
                    string sqlBase = @"	UPDATE `character_resource` SET  {0}  WHERE `uid` = {1};";
                    sqlString = string.Format(sqlBase, parameter, pcUid);
                }
            }

            return sqlString;
        }

        public static string GetSelectSql()
        {
            List<string> nameList = Names;
            string parameter = string.Empty;
            if (nameList.Count > 0)
            {
                foreach (var name in nameList)
                {
                    parameter += string.Format(", `{0}`", name);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);
            }
            return parameter;
        }

        public static string GetSelectParametersSql()
        {
            List<string> nameList = Names;
            string parameter = string.Empty;
            if (nameList.Count > 0)
            {
                foreach (var name in nameList)
                {
                    parameter += string.Format(", @{0} ", name);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);
            }
            return parameter;
        }

        #region 资源仓库
        public static string GetWarehouseResourceSelectSql()
        {
            string parameter = string.Empty;
            if (currencyNames.Count > 0)
            {
                foreach (var name in currencyNames)
                {
                    parameter += string.Format(", `{0}`", name);
                }
                //去掉第一个逗号
                parameter = parameter.Substring(1);
            }
            return parameter;
        }

        public static List<int> GetCurrencyIds()
        {
            return currencyIds;
        }

        public static int GetCarryMaxNum(int type)
        {
            int maxNum;
            carryMaxNum.TryGetValue(type, out maxNum);
            return maxNum;
        }
        #endregion
    }
}

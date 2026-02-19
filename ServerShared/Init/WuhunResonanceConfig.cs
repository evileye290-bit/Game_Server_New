using DataProperty;
using EnumerateUtility;
using EnumerateUtility.Activity;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class BuyResonanceGridCost
    {
        public int Max = 0;
        public int Min = 0;
        Dictionary<CurrenciesType, float> costPair;

        internal void Init(Data data)
        {
            costPair = new Dictionary<CurrenciesType, float>();
            Min = data.GetInt("Min");
            Max = data.GetInt("Max");
            string strCosts = data.GetString("CostCoin");
            string[] arrCoin = strCosts.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arrCoin)
            {
                string[] arr=  item.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length>1)
                {
                    CurrenciesType currenciesType = (CurrenciesType)arr[0].ToInt();
                    float count = arr[1].ToFloat();
                    costPair.Add(currenciesType, count);
                }
            }
        }

        public float GetCostCount(CurrenciesType type)
        {
            float cost = 0;
            costPair.TryGetValue(type, out cost);
            return cost;
        }


        public float GetProportion(CurrenciesType mainType = CurrenciesType.resonanceCrystal,CurrenciesType subType=CurrenciesType.diamond)
        {
            float mainCount = 0;
            costPair.TryGetValue(mainType, out mainCount);

            float subCount = 0;
            costPair.TryGetValue(subType, out subCount);

            if (mainCount >0)
            {
                return subCount / mainCount;
            }
            return 0;
        }

    }

    public static class WuhunResonanceConfig
    {
        public static int ResonanceGridDefaultCount = 0;
        public static int ResonanceGridMaxCount = 0;
        public static int ReferHeroCount = 0;
        public static int GridCdTime = 0;
        public static int ResonanceUpLevel = 100;
        
        public static List<BuyResonanceGridCost> buyResonanceGridCosts = new List<BuyResonanceGridCost>();

        public static BuyResonanceGridCost GetBuyResonanceGridCostConfig(int v)
        {
            foreach (var item in buyResonanceGridCosts)
            {
                if (item.Min <= v && item.Max >= v)
                {
                    return item;
                }
            }
            return null;
        }

        public static void Init()
        {
            DataList dataList = DataListManager.inst.GetDataList("WuhunResonanceConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                ResonanceGridDefaultCount = data.GetInt("ResonanceGridDefaultCount");
                ResonanceGridMaxCount = data.GetInt("ResonanceGridMaxCount");
                ReferHeroCount = data.GetInt("ReferHeroCount");
                GridCdTime = data.GetInt("GridCdTime")*3600;
                ResonanceUpLevel = data.GetInt("ResonanceUpLevel");
            }

            List<BuyResonanceGridCost> buyResonanceGridCosts = new List<BuyResonanceGridCost>();

            dataList = DataListManager.inst.GetDataList("BuyResonanceGridCost");
            //buyResonanceGridCosts.Clear();
            foreach (var item in dataList)
            {
                Data data = item.Value;

                BuyResonanceGridCost configModel = new BuyResonanceGridCost();
                configModel.Init(data);
                buyResonanceGridCosts.Add(configModel);
            }
            WuhunResonanceConfig.buyResonanceGridCosts = buyResonanceGridCosts;
        }

    }
}

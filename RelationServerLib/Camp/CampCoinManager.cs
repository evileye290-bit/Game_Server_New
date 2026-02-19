using DBUtility;
using EnumerateUtility;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class CampCoinManager
    {
        private CampType campType = CampType.None;

        private RelationServerApi server = null;

        public float DBUpdateTimeSpan = 600f;

        public float ZoneUpdateTimeSpan = 1f;

        private Dictionary<CampCoin, int> coins = new Dictionary<CampCoin, int>();

        private bool currenciesChanged = false;
        private bool coinsChangedZoneSync = false;

        /// <summary>
        /// 最后一次同步货币时间
        /// </summary>
        private DateTime lastSyncCurrenciesTime = DateTime.MinValue;
        private DateTime lastZoneSyncCurrenciesTime = DateTime.MinValue;

        public CampCoinManager(RelationServerApi api,CampType camp)
        {
            //
            server = api;
            campType = camp;
            coins.Add(CampCoin.Grain, 0);
            coins.Add(CampCoin.BoxCount, 0);
            coins.Add(CampCoin.BuildValue, 0);
            coins.Add(CampCoin.BattleScore, 0);
        }

        public void Update()
        {
            SyncDbDelayCurrencies();
            SyncZoneDelayCurrencies(true);
        }

        /// <summary>
        /// 定期延迟同步到zone，zone自己取自己的值即可，如果是被加过，立即同步该zone，要先于回复操作结果从而保证先处理zone内存数据
        /// </summary>
        public void SyncZoneDelayCurrencies(bool force = false)
        {
            bool sync = false;
            if (coinsChangedZoneSync)
            {
                if (force || (BaseApi.now - lastZoneSyncCurrenciesTime).TotalSeconds >= ZoneUpdateTimeSpan )
                {
                    sync = true;
                }
            }
            if (sync)
            {
                // 同步db经验和金币
                SynchronizeCurriencies2Zone(coins);
                coinsChangedZoneSync = false;
                lastZoneSyncCurrenciesTime = BaseApi.now;
            }
        }

        private void SynchronizeCurriencies2Zone(Dictionary<CampCoin, int> currenciesList)
        {
            NotifyZone((int)campType, currenciesList[CampCoin.Grain]);
        }

        public void NotifyZone(int camp,int grain)
        {
            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).NotifyCampCoin(camp,grain);
            }
        }

        public void SyncDbDelayCurrencies(bool force = false)
        {
            bool sync = false;
            if (currenciesChanged)
            {
                if (force || (BaseApi.now - lastSyncCurrenciesTime).TotalSeconds >= DBUpdateTimeSpan || server.State == ServerState.Stopping)
                {
                    sync = true;
                }
            }
            if (sync)
            {
                // 同步db经验和金币
                SynchronizeCurrienciesToDB(coins);
                currenciesChanged = false;
                lastSyncCurrenciesTime = BaseApi.now;
            }
        }

        public int GetCoins(CampCoin type)
        {
            int count = 0;
            coins.TryGetValue(type, out count);
            return count;
        }

        public int AddCoin(CampCoin type,int value)
        {
            if (CampCoin.None == type)
            {
                return 0;
            }
            currenciesChanged = true;
            coinsChangedZoneSync = true;
            int oldTemp;
            if (!coins.TryGetValue(type,out oldTemp))
            {
                coins.Add(type, value);
            }
            int temp= oldTemp + value;
            coins[type] = temp;
            return temp;
        }

        public int DelCoin(CampCoin type, int value)
        {
            if (CampCoin.None == type)
            {
                return 0;
            }
            currenciesChanged = true;
            coinsChangedZoneSync = true;
            int oldTemp;
            if (!coins.TryGetValue(type, out oldTemp))
            {
                return 0;
            }

            int temp = oldTemp - value;
            coins[type] = temp;
            return temp;
        }

        public void ClearCoin(CampCoin type)
        {
            currenciesChanged = true;
            coinsChangedZoneSync = true;
            coins[type] = 0;
        }


        public void SetCoin(CampCoin type,int value)
        {
            currenciesChanged = true;
            coinsChangedZoneSync = true;
            coins[type] = value;
        }

        public void LoadCoin(CampCoin type, int value)
        {
            coinsChangedZoneSync = true;
            coins[type] = value;
        }

        /// <summary>
        /// 同步货币变化到DB
        /// </summary>
        private void SynchronizeCurrienciesToDB(Dictionary<CampCoin, int> currenciesList)
        {
            if (currenciesList.Count > 0)
            {
                QueryUpdateCampCoins query = new QueryUpdateCampCoins((int)campType, currenciesList);
                server.GameDBPool.Call(query);
            }
        }

    }
}

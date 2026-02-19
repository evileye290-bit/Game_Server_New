using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        #region 货币仓库

        private Dictionary<CurrenciesType, long> currencyDic = new Dictionary<CurrenciesType, long>();

        /// <summary>
        /// 延迟货币更改
        /// </summary>
        private bool whCurrenciesChanged = false;
        /// <summary>
        /// 最后一次同步货币时间
        /// </summary>
        private DateTime lastSyncWhCurrenciesTime = BaseApi.now;

        public void BindWarehouseCurrencies(Dictionary<CurrenciesType, long> currencyDic)
        {
            this.currencyDic = currencyDic;
            CheckCurrenciesChange();
        }

        public bool CheckBeyondCarryMaxNum(CurrenciesType type, int addNum, out int realAddNum, out int storeNum)
        {
            realAddNum = addNum;
            storeNum = 0;

            int maxNum = CurrenciesLibrary.GetCarryMaxNum((int)type);
            if (maxNum == 0)//配表里没配说明没限制
            {
                return false;
            }
            int origin;
            if (currencies.TryGetValue(type, out origin))
            {
                if (maxNum >= currencies[type] && maxNum - currencies[type] < addNum)//超出携带上限
                {
                    realAddNum = maxNum - currencies[type];
                    storeNum = addNum - realAddNum;
                    return true;
                }
                //else if (maxNum < currencies[type])//已携带的已超出上限
                //{
                //    realAddNum = -(currencies[type] - maxNum);
                //    storeNum = currencies[type] - maxNum + addNum;
                //    return true;
                //}
            }
            else
            {
                if (addNum > maxNum)
                {
                    realAddNum = maxNum;
                    storeNum = addNum - maxNum;
                    return true;
                }
            }
            return false;
        }

        private void AddWarehouseCoinWithoutSync(CurrenciesType type, int addcount, ObtainWay way, string extraParam)
        {
            long original = 0;
            if (currencyDic.TryGetValue(type, out original))
            {
                currencyDic[type] = original + addcount;
            }
            else
            {
                currencyDic[type] = addcount;
            }
            RecordObtainLog(way, RewardType.Currencies, (int)type, original, addcount, extraParam);
            //货币获取埋点
            BIRecordObtainWarehouseCurrency(addcount, currencyDic[type], type, way, extraParam);
        }

        /// <summary>
        /// 同步单个货币变化
        /// </summary>
        public void SynchronizeWarehouseCurrienciesChange(CurrenciesType type)
        {
            List<CurrenciesType> currencies = new List<CurrenciesType>();
            currencies.Add(type);
            SynchronizeWarehouseCurrienciesChange(currencies);
        }

        /// <summary>
        /// 同步多种货币变化
        /// </summary>
        public void SynchronizeWarehouseCurrienciesChange(Dictionary<int, int> list)
        {
            List<CurrenciesType> currencies = new List<CurrenciesType>();
            foreach (var item in list)
            {
                currencies.Add((CurrenciesType)item.Key);
            }
            SynchronizeWarehouseCurrienciesChange(currencies);
        }

        public void SynchronizeWarehouseCurrienciesChange(List<CurrenciesType> keyList)
        {
            SynchronizeWarehouseCurrienciesToClient(keyList);
            SynchronizeWarehouseCurrienciesToDB(keyList);
        }

        /// <summary>
        /// 仓库货币更新
        /// </summary>
        /// <param name="keyList"></param>
        private void SynchronizeWarehouseCurrienciesToClient(List<CurrenciesType> keyList)
        {
            MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES msg = new MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES();
            foreach (var key in keyList)
            {
                long currencyCount = GetWareHouseCoins(key);
                msg.Currencies.Add((int)key, currencyCount);
            }
            Write(msg);
        }

        /// <summary>
        /// 获取仓库所有货币值
        /// </summary>
        /// <returns></returns>
        private MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES GetWarehouseCurrenciesMsg()
        {
            MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES msg = new MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES();

            foreach (var currency in currencyDic)
            {
                msg.Currencies.Add((int)currency.Key, currency.Value);
            }

            return msg;
        }

        /// <summary>
        /// 获取货币数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public long GetWareHouseCoins(CurrenciesType type)
        {
            long count = 0;
            currencyDic.TryGetValue(type, out count);
            return count;
        }

        /// <summary>
        /// 同步货币变化到DB
        /// </summary>
        public void SynchronizeWarehouseCurrienciesToDB(List<CurrenciesType> currencyTypes)
        {
            Dictionary<CurrenciesType, long> currenciesList = new Dictionary<CurrenciesType, long>();
            foreach (var currenciesType in currencyTypes)
            {
                // 延迟同步的货币不立即同步db
                if (DelaySyncDb(currenciesType))
                {
                    whCurrenciesChanged = true;
                    continue;
                }
                else
                {
                    currenciesList.Add(currenciesType, GetWareHouseCoins(currenciesType));
                }
            }
            //DB
            SynchronizeWarehouseCurrienciesToDB(currenciesList);
        }

        private void SynchronizeWarehouseCurrienciesToDB(Dictionary<CurrenciesType, long> currenciesList)
        {
            if (currenciesList.Count > 0)
            {
                string updateSql = GetWarehouseCurrenciesUpdateSql(currenciesList, Uid);
                if (!string.IsNullOrEmpty(updateSql))
                {
                    server.GameDBPool.Call(new QueryUpdateCurrencies(updateSql));
                }
            }
        }

        private string GetWarehouseCurrenciesUpdateSql(Dictionary<CurrenciesType, long> currencies, int pcUid)
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
                    string sqlBase = @"	UPDATE `warehouse_resource` SET  {0}  WHERE `uid` = {1};";
                    sqlString = string.Format(sqlBase, parameter, pcUid);
                }
            }

            return sqlString;
        }

        /// <summary>
        /// 延迟同步货币
        /// </summary>
        /// <param name="force"></param>
        public void DelaySyncDbWarehouseCurrencies(bool force = false)
        {
            bool sync = false;
            if (whCurrenciesChanged)
            {
                if (force || (BaseApi.now - lastSyncWhCurrenciesTime).TotalSeconds >= CONST.SYNC_CURRIENCIES_TIME)
                {
                    sync = true;
                }
            }
            if (sync)
            {
                // 同步db经验和金币
                Dictionary<CurrenciesType, long> currenciesList = new Dictionary<CurrenciesType, long>();
                List<int> ids = CurrenciesLibrary.GetCurrencyIds();
                foreach (int id in ids)
                {
                    CurrenciesType coinType = (CurrenciesType)id;
                    if (DelaySyncDb(coinType))
                    {
                        currenciesList.Add(coinType, GetWareHouseCoins(coinType));
                    }
                }
                SynchronizeWarehouseCurrienciesToDB(currenciesList);

                whCurrenciesChanged = false;
                lastSyncWhCurrenciesTime = BaseApi.now;
            }
        }

        /// <summary>
        /// 提取仓库货币
        /// </summary>
        /// <param name="currencyId">货币类型</param>
        public void GetWareHouseCurrencies(int currencyId)
        {
            MSG_ZGC_GET_WAREHOUSE_CURRENCIES response = new MSG_ZGC_GET_WAREHOUSE_CURRENCIES();
            response.CurrencyType = currencyId;

            List<int> ids = CurrenciesLibrary.GetCurrencyIds();
            if (!ids.Contains(currencyId))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} GetWareHouseCurrencies failed: not find {currencyId} in xml");
                Write(response);
                return;
            }

            CurrenciesType currencyType = (CurrenciesType)currencyId;
            //携带货币数量
            int carryCoins = GetCoins(currencyType);
            int carryMaxNum = CurrenciesLibrary.GetCarryMaxNum(currencyId);
            if (carryCoins >= carryMaxNum)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} GetWareHouseCurrencies failed: currencyType {currencyId} carry num already max");
                Write(response);
                return;
            }

            //仓库货币数量
            long restCoins = GetWareHouseCoins(currencyType);
            if (restCoins == 0)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} GetWareHouseCurrencies failed: currencyType {currencyId} have no coin");
                Write(response);
                return;
            }
          
            int getCoins = GetExtractCoinNum(carryMaxNum, carryCoins, restCoins);
            DelWarehouseCoins(currencyType, getCoins, ConsumeWay.Warehouse, currencyId.ToString());

            //发奖
            AddCoins(currencyType, getCoins, ObtainWay.Warehouse, currencyId.ToString());

            RewardManager manager = new RewardManager();
            manager.AddReward(new ItemBasicInfo((int)RewardType.Currencies, currencyId, getCoins));
            manager.GenerateRewardItemInfo(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 获得提取数量
        /// </summary>
        /// <param name="carryMaxNum">最大携带数量</param>
        /// <param name="carryCoins">当前携带货币量</param>
        /// <param name="restCoins">仓库剩余货币量</param>
        /// <returns></returns>
        private int GetExtractCoinNum(int carryMaxNum, int carryCoins, long restCoins)
        {
            int getCoins;
            int canGetMax = carryMaxNum - carryCoins;
            if (restCoins < canGetMax)
            {
                getCoins = (int)restCoins;
            }
            else
            {
                getCoins = canGetMax;
            }
            return getCoins;
        }

        /// <summary>
        /// 消耗货币数量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="delCoins"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public void DelWarehouseCoins(CurrenciesType type, int delCoins, ConsumeWay way, string extraParam)
        {
            if (delCoins > 0 && DelWarehouseCoinWithoutSync(type, delCoins, way, extraParam))
            {
                SynchronizeWarehouseCurrienciesChange(type);
            }          
        }

        private bool DelWarehouseCoinWithoutSync(CurrenciesType type, int delCoins, ConsumeWay way, string extraParam)
        {
            long original = 0;
            if (currencyDic.TryGetValue(type, out original))
            {
                if (original - delCoins < 0)
                {
                    return false;
                    //currencies[type] = 0;
                }
                else
                {
                    currencyDic[type] = original - delCoins;
                }             

                RecordConsumeLog(way, RewardType.Currencies, (int)type, original, delCoins, extraParam);
                //货币消耗埋点
                BIRecordConsumeWarehouseCurrency(delCoins, currencyDic[type], type, way, extraParam);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// 检查货币变更
        /// </summary>
        private void CheckCurrenciesChange()
        {
            Dictionary<CurrenciesType, int> tempDic = new Dictionary<CurrenciesType, int>();
            foreach (var currency in currencyDic)
            {
                int carryMaxNum = CurrenciesLibrary.GetCarryMaxNum((int)currency.Key);
                if (carryMaxNum == 0)
                {
                    continue;
                }
                int carryNum = GetCoins(currency.Key);
                if (carryNum > carryMaxNum)
                {
                    int changeNum = carryNum - carryMaxNum;
                    tempDic.Add(currency.Key, changeNum);
                    DelCoinWithoutSync(currency.Key, changeNum, ConsumeWay.BeyondCurrencyConvert, currency.Key.ToString());
                    currenciesChanged = true;
                }
            }

            foreach (var kv in tempDic)
            {
                AddWarehouseCoinWithoutSync(kv.Key, kv.Value, ObtainWay.BeyondCurrencyConvert, kv.Key.ToString());
                whCurrenciesChanged = true;
            }
        }

        /// <summary>
        /// 获取跨zone人物货币信息
        /// </summary>
        /// <returns></returns>
        public MSG_ZMZ_WAREHOUSE_CURRENCIES GetWarehouseCurrenciesTransform()
        {
            MSG_ZMZ_WAREHOUSE_CURRENCIES msg = new MSG_ZMZ_WAREHOUSE_CURRENCIES();
            msg.Uid = Uid;
            foreach (var currency in currencyDic)
            {
                msg.Currencies.Add((int)currency.Key, currency.Value);
            }
            msg.CurrenciesChanged = whCurrenciesChanged;
            return msg;
        }

        /// <summary>
        /// 跨zone加载人物货币信息
        /// </summary>
        /// <param name="currencies"></param>
        public void LoadWarehouseCurrenciesTransform(MSG_ZMZ_WAREHOUSE_CURRENCIES msg)
        {
            foreach (var kv in msg.Currencies)
            {
                currencyDic[(CurrenciesType)kv.Key] = kv.Value;
            }
            whCurrenciesChanged = msg.CurrenciesChanged;
        }
        #endregion


        #region 物品仓库    

        /// <summary>
        /// 新增仓库信息项
        /// </summary>
        /// <param name="warehouseUid"></param>
        /// <param name="getTime"></param>
        /// <param name="type"></param>
        /// <param name="rewards"></param>
        /// <param name="param"></param>
        public void AddNewWarehouseItem(ulong warehouseUid, int getTime, int type, string rewards, string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                param = string.Empty;
            }

            WarehouseItem item = new WarehouseItem()
            {
                Uid = warehouseUid,
                GetTime = getTime,
                Rewards = rewards,
                Param = param
            };

            WareHouseModel config = WarehouseLibrary.GetConfig(type);
            if (config == null)
            {
                Log.Warn($"{Uid} AddNewWarehouseItem failed: not find config {type} xml");
                return;
            }

            List<WarehouseItem> itemList = null;
            switch ((ItemWarehouseType)type)
            {
                case ItemWarehouseType.SoulRing:
                    itemList = warehouseSoulRings;                
                    break;
                default:
                    break;
            }
            AddItem(itemList, item, config.StoreLimit);
        }

        /// <summary>
        /// 新增仓库物品
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="newItem"></param>
        /// <param name="storeLimit"></param>
        private void AddItem(List<WarehouseItem> itemList, WarehouseItem newItem, int storeLimit)
        {
            if (itemList.Count >= storeLimit)
            {
                WarehouseItem lastItem = itemList.Last();
                removeSoulRings.Add(lastItem.Uid);
                itemList.RemoveAt(storeLimit - 1);//StoreLimit               
            }
            itemList.Insert(0, newItem);
        }

        /// <summary>
        /// 助战玩家在线时魂环仓库红点通知
        /// </summary>
        public void SyncNewWarehouseItem(int type)
        {
            MSG_ZGC_NEW_WAREHOUSE_SOULRING msg = new MSG_ZGC_NEW_WAREHOUSE_SOULRING();
            Write(msg);
        }

        /// <summary>
        /// 发送仓库物品信息
        /// </summary>
        public void SendWarehouseItemsMsg()
        {
            MSG_ZGC_SYNC_WAREHOUSE_ITEMS msg = new MSG_ZGC_SYNC_WAREHOUSE_ITEMS();
            //魂环仓库
            GenerateSyncWarehouseItemsMsg((int)ItemWarehouseType.SoulRing, warehouseSoulRings, msg.List);

            Write(msg);
        }

        /// <summary>
        /// 生成初始化仓库物品消息
        /// </summary>
        /// <param name="warehouseList"></param>
        private void GenerateSyncWarehouseItemsMsg(int type, List<WarehouseItem> itemList, RepeatedField<WAREHOUSE_INFO> warehouseList)
        {
            WAREHOUSE_INFO info = new WAREHOUSE_INFO();
            info.Type = type;
            info.Page = 1;
            info.TotalCount = itemList.Count;

            int count = 0;
            WareHouseModel config = WarehouseLibrary.GetConfig(type);
            if (config == null)
            {
                return;
            }
            foreach (var item in itemList)
            {
                if (count >= config.PageCount)
                {
                    break;
                }
                WAREHOUSE_ITEM itemMsg = GenerateWarehouseItemMsg(item);
                info.Items.Add(itemMsg);
                count++;
            }
            warehouseList.Add(info);

            //仓库满发邮件
            if (itemList.Count >= config.StoreLimit && !sendSoulRingFull)
            {
                SendPersonEmail(config.WarehouseFullEmail);
                sendSoulRingFull = true;
            }
        }

        /// <summary>
        /// 显示仓库物品信息
        /// </summary>
        /// <param name="type">仓库类型</param>
        /// <param name="page">页码</param>
        public void ShowWareHouseItems(int type, int page)
        {
            MSG_ZGC_SHOW_WAREHOUSE_ITEMS response = new MSG_ZGC_SHOW_WAREHOUSE_ITEMS();
            response.Type = type;
            response.Page = page;

            WareHouseModel config = WarehouseLibrary.GetConfig(type);
            if (config == null)
            {
                Log.Warn($"{Uid} show warehouse items failed: type {type} error");
                Write(response);
                return;
            }

            switch ((ItemWarehouseType)type)
            {
                case ItemWarehouseType.SoulRing:
                    GenerateShowWarehouseItemsMsg(warehouseSoulRings, page, config, response.Items);
                    break;
                default:
                    break;
            }

            Write(response);
        }

        /// <summary>
        /// 生成展示仓库物品消息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemMsgList"></param>
        private void GenerateShowWarehouseItemsMsg(List<WarehouseItem> itemList, int page, WareHouseModel config, RepeatedField<WAREHOUSE_ITEM> itemMsgList)
        {
            int startIndex = (page - 1) * config.PageCount;
            for (int i = startIndex; i < page * config.PageCount; i++)
            {
                if (i >= itemList.Count)
                {
                    break;
                }
                WAREHOUSE_ITEM itemMsg = GenerateWarehouseItemMsg(itemList[startIndex]);
                itemMsgList.Add(itemMsg);
            }
        }

        public WAREHOUSE_ITEM GenerateWarehouseItemMsg(WarehouseItem item)
        {
            WAREHOUSE_ITEM msg = new WAREHOUSE_ITEM()
            {
                UidHigh = item.Uid.GetHigh(),
                UidLow = item.Uid.GetLow(),
                GetTime = item.GetTime,
                Rewards = item.Rewards,
                Param = item.Param
            };
            return msg;
        }

        /// <summary>
        /// 一键领取仓库物品
        /// </summary>
        /// <param name="type"></param>
        public void BatchGetWareHouseItems(int type)
        {
            MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS response = new MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS();

            WareHouseModel config = WarehouseLibrary.GetConfig(type);
            if (config == null)
            {
                Log.Warn($"{Uid} batch get warehouse items failed: type {type} error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
                     
            GetWarehouseItemsRewardsByType((ItemWarehouseType)type, config, response);          

            Write(response);
        }

        /// <summary>
        /// 根据仓库类型获取仓库物品奖励
        /// </summary>
        /// <param name="type"></param>
        /// <param name="getCount"></param>
        /// <param name="pageCount"></param>
        /// <param name="response"></param>
        private void GetWarehouseItemsRewardsByType(ItemWarehouseType type, WareHouseModel config, MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS response)
        {
            List<WarehouseItem> itemList = null;
            switch (type)
            {
                case ItemWarehouseType.SoulRing:
                    itemList = warehouseSoulRings;
                    break;
                default:
                    break;
            }

            if (itemList == null)
            {
                response.Result = (int)ErrorCode.Fail;
                return;
            }
            
            List<ulong> getList = new List<ulong>();
            int count = GenerateWarehouseRewardsInfoAndSendRewards(type, itemList, getList, config.OnceGetLimit, response);

            RemoveItems(type, itemList, count, getList);
            
            GenerateWarehouseFirstPageInfo((int)type, itemList, config.PageCount, response);

            response.Result = (int)ErrorCode.Success;
        }

        /// <summary>
        /// 生成仓库奖励信息并发奖
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="getList"></param>
        /// <param name="getCount"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private int GenerateWarehouseRewardsInfoAndSendRewards(ItemWarehouseType type, List<WarehouseItem> itemList, List<ulong> getList, int countLimit, MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS response)
        {
            int count = 0;           

            RewardManager manager = new RewardManager();        

            foreach (var item in itemList)
            {
                int needCount = GetNeedBagCountByRewardType(manager);
                if (count >= countLimit || BagManager.GetBagRestSpace() <= needCount)//已达提取上限或者背包容量上限
                {
                    break;
                }
                if (!string.IsNullOrEmpty(item.Rewards))
                {
                    manager.AddSimpleReward(item.Rewards);                   
                }
                getList.Add(item.Uid);
                count++;
            }

            if (manager.AllRewards.Count > 0)
            {
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.ItemWarehouse);
                manager.GenerateRewardMsg(response.Rewards);
            }

            return count;
        }

        /// <summary>
        /// 删除仓库物品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="itemList"></param>
        /// <param name="count"></param>
        /// <param name="getList"></param>
        private void RemoveItems(ItemWarehouseType type, List<WarehouseItem> itemList, int count, List<ulong> getList)
        {
            if (count > 0)
            {
                removeSoulRings.AddRange(getList);
                itemList.RemoveRange(0, count);
            }

            switch (type)
            {
                case ItemWarehouseType.SoulRing:
                    server.GameDBPool.Call(new QueryUpdateWrehouseSoulRingGetState(getList));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 生成仓库首页信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="itemList"></param>
        /// <param name="pageCount"></param>
        /// <param name="PageInfoMsg"></param>
        private void GenerateWarehouseFirstPageInfo(int type, List<WarehouseItem> itemList, int pageCount, MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS response)
        {
            response.CurPageInfo = new WAREHOUSE_INFO();
            response.CurPageInfo.Type = type;
            response.CurPageInfo.Page = 1;
            List<WarehouseItem> tempList;
            int realCount = Math.Min(itemList.Count, pageCount);
            tempList = itemList.GetRange(0, realCount);
            foreach (var item in tempList)
            {
                response.CurPageInfo.Items.Add(GenerateWarehouseItemMsg(item));
            }
            response.CurPageInfo.TotalCount = itemList.Count;
        }

        /// <summary>
        /// 生成仓库物品跨zone信息
        /// </summary>
        public MSG_ZMZ_WAREHOUSE_ITEMS GenerateWarehouseItemsTransformMsg()
        {
            MSG_ZMZ_WAREHOUSE_ITEMS msg = new MSG_ZMZ_WAREHOUSE_ITEMS();

            GenerateWarehouseItemsTransformMsgByType(warehouseSoulRings, msg.SoulRings);
            msg.SendSoulRingFull = sendSoulRingFull;

            return msg;
        }

        private void GenerateWarehouseItemsTransformMsgByType(List<WarehouseItem> itemList, RepeatedField<ZMZ_WAREHOUSE_ITEM> itemMsgList)
        {
            foreach (var item in itemList)
            {
                itemMsgList.Add(new ZMZ_WAREHOUSE_ITEM() { Uid = item.Uid, GetTime = item.GetTime, Rewards = item.Rewards, Param = item.Param});
            }
        }

        private void UpdateWarehouseItems()
        {
            RemoveWarehouseSoulRings();
        }

        /// <summary>
        /// 加载仓库物品跨zone信息
        /// </summary>
        /// <param name="msg"></param>
        public void LoadWarehouseItemsTransformMsg(MSG_ZMZ_WAREHOUSE_ITEMS msg)
        {
            LoadItemsWarehouseTransformMsg(msg.SoulRings, warehouseSoulRings);
            sendSoulRingFull = msg.SendSoulRingFull;
        }

        private void LoadItemsWarehouseTransformMsg(RepeatedField<ZMZ_WAREHOUSE_ITEM> itemMsgList, List<WarehouseItem> itemList)
        {
            foreach (var itemMsg in itemMsgList)
            {
                itemList.Add(new WarehouseItem() { Uid = itemMsg.Uid, GetTime = itemMsg.GetTime, Rewards = itemMsg.Rewards, Param = itemMsg.Param});
            }
        }

        #region 魂环仓库

        private List<WarehouseItem> warehouseSoulRings = new List<WarehouseItem>();
        private List<ulong> removeSoulRings = new List<ulong>();
        private bool sendSoulRingFull = false;

        public void BindWarehouseSoulRings(List<WarehouseItem> list)
        {
            warehouseSoulRings = list;
        }
        
        private void RemoveWarehouseSoulRings()
        {
            if (removeSoulRings.Count > 0)
            {
                QueryDeleteWarehouseSoulRing delete = new QueryDeleteWarehouseSoulRing(removeSoulRings);
                server.GameDBPool.Call(delete, ret => 
                {
                    removeSoulRings.Clear();
                });
            }
        }
        #endregion

        #endregion
    }
}

using ServerLogger;
using System.Collections.Generic;
using ThinkingData.Analytics;

namespace Logger
{
    public enum BILoggerType
    {
        //ACTIVITE = 1,       //注册账号时候
        //CREATECHAR = 2,     //创建角色
        LOGIN = 3,          //登录
        LOGOUT = 4,         //退出
        //ONLINE = 5,         //在线

        RECHARGE = 6,       //充值
        CONSUMECURRENCY = 7,//货币获取
        OBTAINCURRENCY = 8, //货币消耗

        //ACTIVITY = 9,       //活动
        //TASK = 10,          //任务

        //CHECKPOINT = 11,    //关卡
        //DEVELOP = 12,       //养成

        ITEMPRODUCE = 13,  //物品获取
        ITEMCONSUME = 14,  //物品获取
        SHOP = 15,          //商店
        //RECRUIT = 16,       //抽卡
        //RINGREPLACE = 17,  //魂环替换
        //BONEREPLACE = 18,  //魂环替换
        //EQUIPMENTREPLACE = 19,     //魂环替换
        //EQUIPMENT = 20,     //装备强化

        //LIMITPACK = 21, //限时礼包
        //GODREPLACE = 22, //神位替换
        //GOD = 23, //神位激活
        //WISHINGWELL = 24,//许愿池
        //TREASUREMAP = 25,//挖宝事件
        //REFRESH = 26,          //刷新
    }


    public partial class BILoggerManager
    {
        public static string LogBusDir = "c:/Log/LogBus/";
        //ThinkingdataAnalytics ta;/* = new ThinkingdataAnalytics(new LoggerConsumer(LogBusDir));*/

        public static void NlogUserSet(string socalledUserId, string temp, Dictionary<string, object> dic)
        {
            Dictionary<string, object> userDic = new Dictionary<string, object>();
            userDic.Add("user", socalledUserId);
            userDic.Add("temp", temp);
            userDic.Add("data", dic);

            string msgString = KomoeLogManager.DictionaryToJson(userDic);
            BILogger.Trace(msgString);
        }

        public static void NlogTrack(string socalledUserId, string temp, string eventType, Dictionary<string, object> dic)
        {
            Dictionary<string, object> userDic = new Dictionary<string, object>();
            userDic.Add("user", socalledUserId);
            userDic.Add("temp", temp);
            userDic.Add("event", eventType);
            userDic.Add("data", dic);

            string msgString = KomoeLogManager.DictionaryToJson(userDic);
            BILogger.Trace(msgString);
        }

        private static string GetsocalledAccountId(string accountId, string channel, int serverId)
        {
            return string.Format("{0}_{1}_{2}", serverId, channel, accountId);
        }

        public static string GetsocalledUserId(int userId, string channel, int serverId)
        {
            //return string.Format("{0}_{1}_{2}", serverId, channel, userId);
            return string.Format("{0}", userId);
        }

        private void UserSetBattlePower(string name, int level, int vipLevel, int battlePower, string socalledUserId)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            userProperties.Add("power_value", battlePower);
            userProperties.Add("vip_level", vipLevel);
            userProperties.Add("level", level);
            userProperties.Add("user_name", name);
            NlogUserSet(socalledUserId, "", userProperties);
        }
        #region taLog

        //Activate 激活
        public void ActivateTaLog(string accountId, string deviceId, string channel, int serverId, string sdkUuid = "default", string pachageName = "cmge_pachage_name_9e25")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledAccount_id = GetsocalledAccountId(accountId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("package_name", pachageName);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack("1000000001", "", "activate", properties);


            Dictionary<string, object> properties1 = new Dictionary<string, object>();
            properties1.Add("#account_id", "1000000001");
            properties1.Add("account_id", socalledAccount_id);
            properties1.Add("device_id", deviceId);
            properties1.Add("channel", channel);
            properties1.Add("server_id", serverId);
            properties1.Add("package_name", pachageName);
            properties1.Add("uuid", sdkUuid);
            properties1.Add("#time", loggerTimestamp.Now());

            NlogTrack("1000000001", "", "registerACT", properties1);
            //ta.Close();
        }

        //register Create  
        public void RegisterTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string registerIp, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string sdkUuid, string extendId)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("server_id", serverId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("register_ip", registerIp);

            properties.Add("idfa", idfa);
            properties.Add("caid", caid);
            properties.Add("idfv", idfv);
            properties.Add("imei", imei);
            properties.Add("oa_id", oa_id);
            properties.Add("imsi", imsi);
            properties.Add("AN_id", AN_id);

            properties.Add("country_code", "CN");
            properties.Add("package_name", pachageName);
            properties.Add("uuid", sdkUuid);
            properties.Add("extend_id", extendId);
            properties.Add("#time", loggerTimestamp.Now());
            properties.Add("#ip", registerIp);
            NlogTrack(socalledUserId, "", "register", properties);

            LoginACTTaLog(accountId, deviceId, channel, registerIp, idfa, caid, idfv, imei, oa_id, imsi, AN_id, pachageName, socalledUserId);
            //Dictionary<string, object> userProerties = new Dictionary<string, object>();
            //userProerties.Add("device_id", deviceId);
            //userProerties.Add("account_id", accountId);
            //userProerties.Add("user_id", socalledUserId);
            //userProerties.Add("channel", channel);
            //userProerties.Add("server_id", serverId);
            //userProerties.Add("user_name", "NoBody");
            //userProerties.Add("idfa_imei", "0000000000");

            //userProerties.Add("register_ip", registerIp);
            //userProerties.Add("register_time", loggerTimestamp.Now());
            //userProerties.Add("country_code", "CN");
            //userProerties.Add("cmgeSDK_id", cmgeSDKId);
            //userProerties.Add("package_name", pachageName);
            //userProerties.Add("power_value", 0);
            //userProerties.Add("vip_level", 1);
            //userProerties.Add("level", 1);
            //userProerties.Add("uuid", sdkUuid);

            //NlogUserSet(socalledUserId, "", userProerties);

            Dictionary<string, object> userCreateProerties = new Dictionary<string, object>();
            userCreateProerties.Add("register_time", loggerTimestamp.Now());
            userCreateProerties.Add("user_name", "NoBody");
            userCreateProerties.Add("user_id", socalledUserId);
            userCreateProerties.Add("account_id", accountId);
            userCreateProerties.Add("device_id", deviceId);
            userCreateProerties.Add("channel", channel);
            userCreateProerties.Add("server_id", serverId);

            userCreateProerties.Add("idfa", idfa);
            userCreateProerties.Add("caid", caid);
            userCreateProerties.Add("idfv", idfv);
            userCreateProerties.Add("imei", imei);
            userCreateProerties.Add("oa_id", oa_id);
            userCreateProerties.Add("imsi", imsi);
            userCreateProerties.Add("AN_id", AN_id);

            userCreateProerties.Add("country_code", "CN");
            userCreateProerties.Add("package_name", pachageName);
            userCreateProerties.Add("uuid", sdkUuid);
            userCreateProerties.Add("extend_id", extendId);
            NlogUserSet(socalledUserId, "", userCreateProerties);

            //ta.Close();
        }

        private void LoginACTTaLog(string accountId, string deviceId, string channel, string registerIp, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string socalledUserId)
        {
            Dictionary<string, object> act_properties = new Dictionary<string, object>();
            act_properties.Add("#account_id", "1000000001");
            act_properties.Add("account_id", accountId);
            act_properties.Add("channel", channel);
            act_properties.Add("device_id", deviceId);
            act_properties.Add("idfa", idfa);
            act_properties.Add("caid", caid);
            act_properties.Add("idfv", idfv);
            act_properties.Add("imei", imei);
            act_properties.Add("oa_id", oa_id);
            act_properties.Add("imsi", imsi);
            act_properties.Add("AN_id", AN_id);
            act_properties.Add("package_name", pachageName);
            act_properties.Add("#time", loggerTimestamp.Now());
            act_properties.Add("#ip", registerIp);
            NlogTrack(socalledUserId, "", "loginACT", act_properties);
            //ta.Close();
        }

        //Login
        public void LoginTaLog(int userId, string accountId, string name, string deviceId, string channel, int serverId, string loginIp, int level, int battlePower, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string extendId)
        {
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("login_ip", loginIp);
            properties.Add("vip_level", 1);
            properties.Add("level", level);
            properties.Add("package_name", pachageName);

            properties.Add("idfa", idfa);
            properties.Add("caid", caid);
            properties.Add("idfv", idfv);
            properties.Add("imei", imei);
            properties.Add("oa_id", oa_id);
            properties.Add("imsi", imsi);
            properties.Add("AN_id", AN_id);
            properties.Add("power_value", battlePower);

            properties.Add("#time", loggerTimestamp.Now());
            properties.Add("#ip", loginIp);
            NlogTrack(socalledUserId, "", "login", properties);


            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            userProperties.Add("lastlogin_time", loggerTimestamp.Now());
            userProperties.Add("lastpackage_name", pachageName);
            userProperties.Add("lastextend_id", extendId);
            NlogUserSet(socalledUserId, "", userProperties);

            //更新战力事件
            UserSetBattlePower(name, level, 1, battlePower, socalledUserId);

            //ta.Close();
        }


        //Logout
        public void LogoutTaLog(int userId, string accountId, string name, string deviceId, string channel, int serverId, string logoutIp, int gold, int diamond, int exp, int friendlyHeart, int sotoCoin, int resonanceCrystal, int shellCoin, int battlePower, int onlineTime = 0, int level = 1, string sdkUuid = "default", int vipLevel = 1, int huntingLevel = 0, string extendId = "Natural")
        {
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("logout_ip", logoutIp);
            properties.Add("vip_level", vipLevel);
            properties.Add("level", level);
            properties.Add("online_time", onlineTime);
            properties.Add("uuid", sdkUuid);
            properties.Add("extend_id", extendId);
            properties.Add("gold", gold);
            properties.Add("diamond", diamond);
            properties.Add("friendlyHeart", friendlyHeart);
            properties.Add("sotoCoin", sotoCoin);
            properties.Add("resonanceCrystal", resonanceCrystal);
            properties.Add("shellCoin", shellCoin);
            properties.Add("power_value", battlePower);
            properties.Add("hunting_level", huntingLevel);

            properties.Add("#time", loggerTimestamp.Now());
            NlogTrack(socalledUserId, "", "logout", properties);

            Dictionary<string, object> userLogoutProerties = new Dictionary<string, object>();
            userLogoutProerties.Add("lastlogout_time", loggerTimestamp.Now());
            NlogUserSet(socalledUserId, "", userLogoutProerties);

            Dictionary<string, object> userProerties = new Dictionary<string, object>();
            userProerties.Add("vip_level", vipLevel);
            userProerties.Add("level", level);
            userProerties.Add("hunting_level", huntingLevel);
            NlogUserSet(socalledUserId, "", userProerties);

            //ta.Close();
        }


        //onlineNum
        public void OnlineTaLog(int userCount, int accountCount, int deviceCount, int serverId, string channel = "00")//
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("server_id", serverId);
            properties.Add("online_user", userCount);
            properties.Add("online_account", accountCount);
            properties.Add("online_device", deviceCount);
            properties.Add("channel", channel);
            properties.Add("#time", loggerTimestamp.Now());
            NlogTrack("1000000001", "", "onlineNum", properties);

            //ta.Close();
        }

        //Recharge
        public void RechargeTaLog(int userId, string accountId, string deviceId, string channel, int serverId, float money, string gameOrderId, string sdkOrderId, string payOrderId, string moneyType, string payWay, string productId, int level, string sdkUuid, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, int vipLevel = 1)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("vip_level", vipLevel);
            properties.Add("level", level);
            properties.Add("money", money);
            properties.Add("game_order_id", gameOrderId);
            properties.Add("SDK_order_id", sdkOrderId);
            properties.Add("pay_order_id", payOrderId);
            properties.Add("money_type", moneyType);
            properties.Add("payway", payWay);
            properties.Add("is_success", 1);
            properties.Add("product_id", productId);

            properties.Add("idfa", idfa);
            properties.Add("caid", caid);
            properties.Add("idfv", idfv);
            properties.Add("imei", imei);
            properties.Add("oa_id", oa_id);
            properties.Add("imsi", imsi);
            properties.Add("AN_id", AN_id);
            properties.Add("package_name", pachageName);

            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());
            NlogTrack(socalledUserId, "", "recharge", properties);

            Dictionary<string, object> userProerties = new Dictionary<string, object>();
            userProerties.Add("lastrecharge_time", loggerTimestamp.Now());
            userProerties.Add("lastrecharge_money", money);
            NlogUserSet(socalledUserId, "", userProerties);

            Dictionary<string, object> addProerties = new Dictionary<string, object>();
            addProerties.Add("totalrecharge_money", money);
            addProerties.Add("totalrecharge_times", 1);
            NlogUserSet(socalledUserId, "", addProerties);

            //ta.Close();
        }

        //FirstRecharge
        public void FirstRechargeTaLog(int userId, string channel, int serverId, float money)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> userProerties = new Dictionary<string, object>();
            userProerties.Add("firstrecharge_money", money);
            userProerties.Add("firstrecharge_time", loggerTimestamp.Now());
            NlogUserSet(socalledUserId, "", userProerties);
        }

        //Obtain Currency
        public void ObtainCurrencyTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string currencyType, string obtainType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("currency_type", currencyType);
            properties.Add("module_id", obtainType);
            properties.Add("module_2_id", moduleId);
            properties.Add("quantity", count);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "currency_produce", properties);

            //ta.Close();
        }

        //Consume Currency
        public void ConsumeCurrencyTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string currencyType, string consumeType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("currency_type", currencyType);
            properties.Add("module_id", consumeType);
            properties.Add("module_2_id", moduleId);
            properties.Add("quantity", count);
            properties.Add("product_id", consumeType);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "currency_consume", properties);

            //ta.Close();
        }

        //Obtain item
        public void ObtainItemTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string itemType, string obtainType, string moduleId, int level = 1, int year = 0, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("item_type", itemType);
            properties.Add("module_id", obtainType);
            properties.Add("module_2_id", moduleId);
            properties.Add("soulring_level", year);
            properties.Add("quantity", count);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "item_produce", properties);

            //ta.Close();
        }

        //Consume item
        public void ConsumeItemTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string itemType, string consumeType, string moduleId, int level = 1, string sdkUuid = "default", int ringLevel = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("item_type", itemType);
            properties.Add("module_id", consumeType);
            properties.Add("module_2_id", moduleId);
            properties.Add("soulring_level", ringLevel);
            properties.Add("quantity", count);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "item_consume", properties);

            //ta.Close();
        }

        //shop
        public void ShopTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string shopType, string currencyType, int currencyCount, string obtainType, string moduleId, int itemType, int itemCount, int level = 1, string sdkUuid = "default", string timingGiftType = "", int buyCount = 1)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("shop_type", shopType);
            properties.Add("currency_type", currencyType);
            properties.Add("quantity", currencyCount);
            properties.Add("module_id", obtainType);
            properties.Add("module_2_id", moduleId);
            properties.Add("product_id", itemType);
            properties.Add("product_quantity", itemCount);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "shop", properties);

            //ta.Close();
        }

        //Task
        public void TaskTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string taskType, int taskId, int taskState, int powerValue, int level = 1, string sdkUuid = "default", int vipLevel = 1)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("task_type", taskType);
            properties.Add("task_id", taskId);
            properties.Add("task_state", taskState);
            properties.Add("power_value", powerValue);
            properties.Add("vip_level", vipLevel);
            properties.Add("level", level);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "task", properties);

            //ta.Close();
        }

        //关卡
        public void CheckPointTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string pointType, string pointId, int pointState, int useTime, int powerValue, int level = 1, string sdkUuid = "default", int vipLevel = 1)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("point_type", pointType);
            properties.Add("point_id", pointId);
            properties.Add("point_state", pointState);
            properties.Add("use_second", useTime);
            properties.Add("power_value", powerValue);
            properties.Add("vip_level", vipLevel);
            properties.Add("level", level);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "checkpoint", properties);

            //ta.Close();
        }

        //Activity 活动
        public void ActivityTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string activityType, int activityId, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("activity_type", activityType);
            properties.Add("activity_id", activityId);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "activity", properties);

            //ta.Close();
        }

        //养成
        public void DevelopTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string developType, string targetId, int beforeLevel, int afterLevel, int heroId = 0, int heroLevel = 0, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("develop_type", developType);
            properties.Add("card_id", heroId);
            properties.Add("card_level", heroLevel);
            properties.Add("target_id", targetId);
            properties.Add("before_level", beforeLevel);
            properties.Add("after_level", afterLevel);
            properties.Add("product_id", 0);
            properties.Add("quantity", 0);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "develop", properties);

            //ta.Close();
        }

        //抽卡
        public void RecruitHeroTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string drawType, string currencyType, int currencyCount, List<int> heroIds, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("recruit_type", drawType);
            properties.Add("item_type", currencyType);
            properties.Add("quantity", currencyCount);

            //string heroId = string.Empty;
            //if (heroIds.Count > 0)
            //{
            //    foreach (var item in heroIds)
            //    {
            //        heroId += ":" + item;
            //    }
            //    //去掉第一个逗号
            //    heroId = heroId.Substring(1);
            //}

            properties.Add("hero_id", heroIds);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "recruit", properties);

            //ta.Close();
        }

        //魂环替换
        public void RingReplaceTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int ringIndex, int oldId, int oldYyear, int newId, int newYear, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", heroId);
            properties.Add("card_level", heroLevel);
            properties.Add("number", ringIndex);
            properties.Add("ring_id_old", oldId);
            properties.Add("ring_number_old", oldYyear);
            properties.Add("ring_id_new", newId);
            properties.Add("ring_number_new", newYear);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "ring_replace", properties);

            //ta.Close();
        }
        //魂骨替换
        public void BoneReplaceTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int boneIndex, int oldId, int oldPower, int newId, int newPower, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", heroId);
            properties.Add("card_level", heroLevel);
            properties.Add("number", boneIndex);
            properties.Add("bone_id_old", oldId);
            properties.Add("bone_number_old", oldPower);
            properties.Add("bone_id_new", newId);
            properties.Add("bone_number_new", newPower);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "bone_replace", properties);

            //ta.Close();
        }
        //装备替换
        public void EquipmentReplaceTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int equipmentIndex, int oldId, int oldPower, int newId, int newPower, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", heroId);
            properties.Add("card_level", heroLevel);
            properties.Add("number", equipmentIndex);
            properties.Add("equip_id_old", oldId);
            properties.Add("equip_number_old", oldPower);
            properties.Add("equip_id_new", newId);
            properties.Add("equip_number_new", newPower);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "equioment_replace", properties);

            //ta.Close();
        }
        //装备
        public void EquipmentTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int equipmentIndex, int id, int state, int oldPower, int newPower, int oldlevel, int newlevel, int stoneNum, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", heroId);
            properties.Add("card_level", heroLevel);
            properties.Add("number", equipmentIndex);
            properties.Add("equip_id_old", id);
            properties.Add("equip_state", state);
            properties.Add("equip_lv_old", oldlevel);
            properties.Add("equip_lv_new", newlevel);
            properties.Add("equip_level_old", oldPower);
            properties.Add("equip_level_new", newPower);
            properties.Add("stone_num", stoneNum);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "equipment", properties);

            //ta.Close();
        }

        //20201110


        //限时礼包
        public void LimitPackTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, float money, string money_type, int launch_id, int pack_id, int operation_type, int operation_time, ulong sequence, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("level", level);

            properties.Add("money", money);
            properties.Add("money_type", money_type);
            properties.Add("launch_id", launch_id);
            properties.Add("pack_id", pack_id);
            properties.Add("sequence", sequence);
            properties.Add("operation_type", operation_type);
            properties.Add("operation_time", operation_time);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "limit_pack", properties);

            //ta.Close();
        }

        //神位替换
        public void GodReplaceTaLog(int userId, int level, string accountId, string deviceId, string channel, int serverId, int card_id, int card_level, int god_id_old, int card_number_old, int god_id_new, int card_number_new, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", card_id);
            properties.Add("card_level", card_level);
            properties.Add("god_id_old", god_id_old);
            properties.Add("card_number_old", card_number_old);
            properties.Add("god_id_new", god_id_new);
            properties.Add("card_number_new", card_number_new);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "god_replace", properties);

            //ta.Close();
        }

        public void GodTaLog(int userId, int level, string accountId, string deviceId, string channel, int serverId, int card_id, int card_level, int god_id, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", card_id);
            properties.Add("card_level", card_level);
            properties.Add("god_id", god_id);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "god", properties);

            //ta.Close();
        }

        public void WishingWellTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int well_state, string currency_in, string currency_out, int quantity_in, int quantity_out, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("well_state", well_state);
            properties.Add("currency_in", currency_in);
            properties.Add("currency_out", currency_out);
            properties.Add("quantity_in", quantity_in);
            properties.Add("quantity_out", quantity_out);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "wishing_well", properties);

            //ta.Close();
        }

        public void TreasureMapTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int map_state, int blood, int quantity, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("map_state", map_state);
            properties.Add("blood", blood);
            properties.Add("quantity", quantity);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "treasure_map", properties);

            //ta.Close();
        }

        //20210225

        //currencyRemain
        public void CurrencyRemainTaLog(string channel, int serverId, ulong gold, ulong diamond, ulong friendlyHeart, ulong sotoCoin, ulong resonanceCrystal, ulong shellCoin)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("#account_id", 1000000001);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("gold", gold);
            properties.Add("diamond", diamond);
            properties.Add("friendlyHeart", friendlyHeart);
            properties.Add("sotoCoin", sotoCoin);
            properties.Add("resonanceCrystal", resonanceCrystal);
            properties.Add("shellCoin", shellCoin);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(1000000001.ToString(), "", "currencyRemain", properties);

            //ta.Close();
        }

        //levelup
        public void LevelupTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string level_type, int before_level, int after_level, int dt_exp_time, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("level_type", level_type);
            properties.Add("before_level", before_level);
            properties.Add("after_level", after_level);
            properties.Add("dt_exp_time", dt_exp_time);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "levelup", properties);

            //ta.Close();
        }

        //equip_redeem
        public void EquipRedeemTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int quantity, float discount, int beast_soul, int gold, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("quantity", quantity);
            properties.Add("discount", discount);
            properties.Add("beast_soul", beast_soul);
            properties.Add("gold", gold);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "equip_redeem", properties);

            //ta.Close();
        }



        //token_consume
        public void TokenConsumeTaLog(int userId, string accountId, string deviceId, string channel, int serverId, float money, string gameOrderId, string sdkOrderId, string payOrderId, string moneyType, string payWay, string productId, int level, string extendId = "Naturl", int vipLevel = 0, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("vip_level", vipLevel);
            properties.Add("level", level);
            properties.Add("money", money);
            properties.Add("game_order_id", gameOrderId);
            properties.Add("SDK_order_id", sdkOrderId);
            properties.Add("pay_order_id", payOrderId);
            properties.Add("money_type", moneyType);
            properties.Add("payway", payWay);
            properties.Add("is_success", 1);
            properties.Add("product_id", productId);

            properties.Add("uuid", sdkUuid);
            properties.Add("extend_id", extendId);
            properties.Add("#time", loggerTimestamp.Now());
            NlogTrack(socalledUserId, "", "token_consume", properties);

            //Dictionary<string, object> userProerties = new Dictionary<string, object>();
            //userProerties.Add("lastrecharge_time", loggerTimestamp.Now());
            //userProerties.Add("lastrecharge_money", money);
            //NlogUserSet(socalledUserId, "", userProerties);

            //Dictionary<string, object> addProerties = new Dictionary<string, object>();
            //addProerties.Add("totalrecharge_money", money);
            //addProerties.Add("totalrecharge_times", 1);
            //NlogUserSet(socalledUserId, "", addProerties);

            //ta.Close();
        }


        //package_code
        public void PackageCodeTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, string package_code, string package_channel, bool one_for_all, int package_id, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("package_code", package_code);
            properties.Add("package_channel", package_channel);
            properties.Add("one_for_all", one_for_all);
            properties.Add("package_id", package_id);

            properties.Add("uuid", sdkUuid);
            NlogTrack(socalledUserId, "", "package_code", properties);

            //ta.Close();
        }


        //welfare_account
        public void WelfareAccountTaLog(int userId, string channel, int serverId)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> userProerties = new Dictionary<string, object>();
            userProerties.Add("welfare_account", true);
            NlogUserSet(socalledUserId, "", userProerties);
            //ta.Close();
        }

        //theme_boss
        public void ThemeBossTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int id, int bossLevel, int degree, int rank = 0, int finalRank = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("themeboss_id", id);
            properties.Add("themeboss_lv", bossLevel);
            properties.Add("themeboss_pp", degree);
            properties.Add("themeboss_rank", rank);

            properties.Add("final_rank", finalRank);
            NlogTrack(socalledUserId, "", "theme_boss", properties);

            //ta.Close();
        }

        //camp_build
        public void CampBuildTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int buildLevel, int buildUp, int buildPoint, int rank = 0, int finalRank = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("campbuild_lv", buildLevel);
            properties.Add("campbuild_up", buildUp);
            properties.Add("campbuild_point", buildPoint);
            properties.Add("campbuild_rank", rank);

            properties.Add("final_rank", finalRank);
            NlogTrack(socalledUserId, "", "camp_build", properties);

            //ta.Close();
        }

        //garden
        public void GardenTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int id, int diamondNum, int gainType, int score, int totalScore, int rank = 0, int finalRank = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("garden_id", id);
            properties.Add("diamond_num", diamondNum);
            properties.Add("gain_type", gainType);
            properties.Add("gain_point", score);
            properties.Add("point", totalScore);
            properties.Add("garden_rank", rank);

            properties.Add("final_rank", finalRank);
            NlogTrack(socalledUserId, "", "Garden", properties);

            //ta.Close();
        }

        //rename
        public void RenameTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
        
            properties.Add("old_name", oldName);
            properties.Add("new_name", newName);
            properties.Add("#time", loggerTimestamp.Now());
           
            NlogTrack(socalledUserId, "", "rename", properties);

            //ta.Close();
        }

        //contribution
        public void ContributionTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int contribution, int phaseNum, int currentValue, int rank = 0, int finalRank = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("godtest_lv", phaseNum);
            properties.Add("godtest_up", contribution);
            properties.Add("godtest_point", currentValue);
            properties.Add("godtest_rank", rank);
            properties.Add("final_rank", finalRank);

            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "god_test", properties);

            //ta.Close();
        }

        //islandhigh
        public void IslandHighTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int stage, int itemId, int random, int curScore, int totalScore, int rank = 0, int finalRank = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("islandhigh_lv", stage);
            properties.Add("dice_id", itemId);
            properties.Add("dice_point", random);
            properties.Add("gain_point", curScore);
            properties.Add("point", totalScore);
            properties.Add("garden_rank", rank);
            properties.Add("final_rank", finalRank);

            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "IslandHigh", properties);

            //ta.Close();
        }

        //travel
        public void IsTravelTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int solt, int heroId, string eventType, string eventId)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            properties.Add("level", level);

            properties.Add("cell_id", solt);
            properties.Add("card_id", heroId.ToString());
            properties.Add("randomevent_type", eventType);
            properties.Add("randomevent_id", eventId);

            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "travel", properties);

            //ta.Close();
        }

        //rankActive
        public void RankActiveTaLog(int userId, string channel, int serverId, string[] serverIdArr, string rankType, int firstUid, int firstValue, int luckyUid)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);        
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            if (serverIdArr != null)
            {
                properties.Add("server_array", serverIdArr);
            }          
            properties.Add("rank_id", rankType);
            properties.Add("uid_best", firstUid);
            properties.Add("best_point", firstValue);
            properties.Add("uid_last", luckyUid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack(socalledUserId, "", "rank_active", properties);

            //ta.Close();
        }

        //rank
        public void RankTaLog(int serverId, string[] serverIdArr, string rankType, int stage, string[] rankInfoArr)
        {
            //if (string.IsNullOrWhiteSpace(channel))
            //{
            //    channel = "default";
            //}
            //string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            //properties.Add("user_id", socalledUserId);
            properties.Add("channel", "default");
            properties.Add("server_id", serverId);
            if (serverIdArr != null)
            {
                properties.Add("server_array", serverIdArr);
            }         
            properties.Add("rank_id", rankType);
            properties.Add("rank_lv", stage);
            properties.Add("rank", rankInfoArr);          
            properties.Add("#time", loggerTimestamp.Now());

            NlogTrack("1000000001", "", "rank", properties);

            //ta.Close();
        }

        //account_first_register  
        public void AccountFirstRegisterTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string registerIp, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string sdkUuid, string extendId)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("server_id", serverId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("register_ip", registerIp);

            properties.Add("idfa", idfa);
            properties.Add("caid", caid);
            properties.Add("idfv", idfv);
            properties.Add("imei", imei);
            properties.Add("oa_id", oa_id);
            properties.Add("imsi", imsi);
            properties.Add("AN_id", AN_id);

            properties.Add("#first_check_id", accountId);

            properties.Add("country_code", "CN");
            properties.Add("package_name", pachageName);
            properties.Add("uuid", sdkUuid);
            properties.Add("extend_id", extendId);
            properties.Add("#time", loggerTimestamp.Now());
            properties.Add("#ip", registerIp);
            NlogTrack(socalledUserId, "", "account_first_register", properties);

            Dictionary<string, object> userCreateProerties = new Dictionary<string, object>();
            userCreateProerties.Add("register_time", loggerTimestamp.Now());
            userCreateProerties.Add("user_name", "NoBody");
            userCreateProerties.Add("user_id", socalledUserId);
            userCreateProerties.Add("account_id", accountId);
            userCreateProerties.Add("device_id", deviceId);
            userCreateProerties.Add("channel", channel);
            userCreateProerties.Add("server_id", serverId);

            userCreateProerties.Add("idfa", idfa);
            userCreateProerties.Add("caid", caid);
            userCreateProerties.Add("idfv", idfv);
            userCreateProerties.Add("imei", imei);
            userCreateProerties.Add("oa_id", oa_id);
            userCreateProerties.Add("imsi", imsi);
            userCreateProerties.Add("AN_id", AN_id);

            userCreateProerties.Add("country_code", "CN");
            userCreateProerties.Add("package_name", pachageName);
            userCreateProerties.Add("uuid", sdkUuid);
            userCreateProerties.Add("extend_id", extendId);
            NlogUserSet(socalledUserId, "", userCreateProerties);

            //ta.Close();
        }

        //device_first_register  
        public void DeviceFirstRegisterTaLog(int userId, string accountId, string deviceId, string channel, int serverId, string registerIp, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string sdkUuid, string extendId)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("account_id", accountId);
            properties.Add("server_id", serverId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("register_ip", registerIp);

            properties.Add("idfa", idfa);
            properties.Add("caid", caid);
            properties.Add("idfv", idfv);
            properties.Add("imei", imei);
            properties.Add("oa_id", oa_id);
            properties.Add("imsi", imsi);
            properties.Add("AN_id", AN_id);

            properties.Add("#first_check_id", deviceId);

            properties.Add("country_code", "CN");
            properties.Add("package_name", pachageName);
            properties.Add("uuid", sdkUuid);
            properties.Add("extend_id", extendId);
            properties.Add("#time", loggerTimestamp.Now());
            properties.Add("#ip", registerIp);
            NlogTrack(socalledUserId, "", "device_first_register", properties);

            Dictionary<string, object> userCreateProerties = new Dictionary<string, object>();
            userCreateProerties.Add("register_time", loggerTimestamp.Now());
            userCreateProerties.Add("user_name", "NoBody");
            userCreateProerties.Add("user_id", socalledUserId);
            userCreateProerties.Add("account_id", accountId);
            userCreateProerties.Add("device_id", deviceId);
            userCreateProerties.Add("channel", channel);
            userCreateProerties.Add("server_id", serverId);

            userCreateProerties.Add("idfa", idfa);
            userCreateProerties.Add("caid", caid);
            userCreateProerties.Add("idfv", idfv);
            userCreateProerties.Add("imei", imei);
            userCreateProerties.Add("oa_id", oa_id);
            userCreateProerties.Add("imsi", imsi);
            userCreateProerties.Add("AN_id", AN_id);

            userCreateProerties.Add("country_code", "CN");
            userCreateProerties.Add("package_name", pachageName);
            userCreateProerties.Add("uuid", sdkUuid);
            userCreateProerties.Add("extend_id", extendId);
            NlogUserSet(socalledUserId, "", userCreateProerties);

            //ta.Close();
        }
        
        //inherit
        public void InheritTaLog(int userId, int level, string accountId, string deviceId, string channel, int serverId, int toId, int fromId, int[] toOldEquipId, int[] toOldEquipLevel, int[] fromOldEquipId, int[] fromOldEquipLevel, int[] toNewEquipId, int[] toNewEquipLevel, int[] fromNewEquipId, int[] fromNewEquipLevel, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("inheritor_id", fromId);
            properties.Add("inheritee_id", toId);
            properties.Add("inheritor_old_equip_id", fromOldEquipId);
            properties.Add("inheritor_old_equip_level", fromOldEquipLevel);
            properties.Add("inheritee_old_equip_id", toOldEquipId);
            properties.Add("inheritee_old_equip_level", toOldEquipLevel);

            properties.Add("inheritor_new_equip_id", fromNewEquipId);
            properties.Add("inheritor_new_equip_level", fromNewEquipLevel);
            properties.Add("inheritee_new_equip_id", toNewEquipId);
            properties.Add("inheritee_new_equip_level", toNewEquipLevel);
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogUserSet(socalledUserId, "", properties);

            //ta.Close();
        }

        //god_level_up
        public void GodLevelUpTaLog(int userId, int level, string accountId, string deviceId, string channel, int serverId, string heroId, int godLevel, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);

            properties.Add("card_id", heroId);
            properties.Add("god_level", godLevel);           
            properties.Add("uuid", sdkUuid);
            properties.Add("#time", loggerTimestamp.Now());

            NlogUserSet(socalledUserId, "", properties);

            //ta.Track(socalledUserId, "", "god_level_up", properties);

            //ta.Close();
        }

        //运营活动积分
        public void PointGameTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int score, int totalScore, string activityType, int activityNum)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            
            properties.Add("gain_point", score);
            properties.Add("point", totalScore);
            properties.Add("act_type", activityType);
            properties.Add("act_num", activityNum);

            NlogUserSet(socalledUserId, "", properties);

            //ta.Close();
        }
        
        //百兽时空塔
        public void PetTowerTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, int towerLevel, int pointState, int petId, int petLevel, int petAptitude, int petBreakLevel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            
            properties.Add("tower_floor", towerLevel);
            properties.Add("point_state", pointState);
            properties.Add("pet_id", petId);
            properties.Add("pet_level", petLevel);
            properties.Add("pet_talent", petAptitude);
            properties.Add("pet_class", petBreakLevel);

            NlogUserSet(socalledUserId, "", properties);
            
            //ta.Track(socalledUserId, "", "pet_tower", properties);

            //ta.Close();
        }
        
        //宠物养成
        public void PetDevelopTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, 
            string developType, int petId, int petLevel, int petAptitude, int petBreakLevel, 
            string petBeforelevel, string petAfterLevel, string consumeItemId, int consumeNum)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            
            properties.Add("develop_type", developType);
            properties.Add("pet_id", petId);
            properties.Add("pet_level", petLevel);
            properties.Add("pet_talent", petAptitude);
            properties.Add("pet_class", petBreakLevel);
            properties.Add("before_level", petBeforelevel);
            properties.Add("after_level", petAfterLevel);
            properties.Add("product_id", consumeItemId);
            properties.Add("quantity", consumeNum);

            NlogUserSet(socalledUserId, "", properties);
            
            //ta.Track(socalledUserId, "", "pet_develop", properties);

            //ta.Close();
        }
        
        //宠物技能洗练
        public void PetSkillTaLog(int userId, string accountId, string deviceId, string channel, int serverId, int level, 
            int petId, int petLevel, int petAptitude, int petBreakLevel, string beforeSkill, string afterSkill,
            string beforeRarity, string afterRarity, int protect, List<Dictionary<string, object>> skillList)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string socalledUserId = GetsocalledUserId(userId, channel, serverId);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("user_id", socalledUserId);
            properties.Add("level", level);
            properties.Add("account_id", accountId);
            properties.Add("device_id", deviceId);
            properties.Add("channel", channel);
            properties.Add("server_id", serverId);
            
            properties.Add("pet_id", petId);
            properties.Add("pet_level", petLevel);
            properties.Add("pet_talent", petAptitude);
            properties.Add("pet_class", petBreakLevel);
            properties.Add("before_skill", beforeSkill);
            properties.Add("after_skill", afterSkill);
            properties.Add("before_rarity", beforeRarity);
            properties.Add("after_rarity", afterRarity);
            properties.Add("protect", protect);
            properties.Add("skill_array", skillList);

            NlogUserSet(socalledUserId, "", properties);
            
            //ta.Track(socalledUserId, "", "pet_skill", properties);

            //ta.Close();
        }
        #endregion
    }
}

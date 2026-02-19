using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using ServerLogger;
using ServerLogger.KomoeLog;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {

        /* 
            b_log_id 每个日志的唯一ID   游戏名+事件名+加uid+加时间戳+随机数，例如 yjzspub999-3216049191039332853	必填 string
            b_udid  用户硬件设备号 Android和iOS都用的uuid，32位通用唯一识别码 必填  string
            b_sdk_udid  用户硬件设备号 b服SDK udid，客户端SDK登录事件接口获取，32位通用唯一识别码 必填，除实时在线事件 string
            b_sdk_uid   B站生成的uid b服SDK uid,客户端SDK登录事件接口获取，用户ID，一般为一款产品自增序列号，例如：156475929395	必填 string
            b_account_id    用户游戏内账号id 注册账号通过算法加密生成的账户ID，例如：1000004254	必填 string
            b_tour_indicator    是否是游客账号 枚举：0/1   （0:非游客，1：游客）	必填 int
            b_role_id   用户角色id 同一账户下的多角色识别id，没有该参数则时传相同的account_id 必填  string
            b_utc_timestamp 时间戳 例如：1603630292	必填 int
            b_datetime  日期格式 该动作实际发生的当地时间，格式 YYYY-MM-DD HH:MM:SS 必填  string
            b_game_base_id  游戏唯一id 每款游戏的唯一ID，通过运营SDK获得，每款游戏写死值即可 必填  string
            b_game_id   游戏id 一款游戏的ID 必填 int
            b_platform  平台名称 统一：ios|android|windows 必填  string
            b_zone_id   游戏自定义的区服id 针对分区分服的游戏填写分区id，用于区分区服。
            请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段 必填  int
            b_channel_id    游戏的渠道ID 游戏的渠道ID 必填 int
            b_version   游戏客户端版本 游戏的迭代版本，例如1.0.3	必填 string
            b_eventname 日志方法名/事件名 例如登录事件上报player_login    必填 string
            level   当前角色等级 当前角色等级，例如：2，如果最低等级不是1，给个默认值0 必填，玩家注册和实时在线事件无此字段 int
            role_name   玩家当前角色名 例如：黄昏蔷薇行者 必填  string 
        */

        private const string eventDateTimeString = "yyyy-MM-dd HH:mm:ss";
        private const string userDateTimeString = "yyyyMMdd";


        private Dictionary<string, object> commonInfDic = new Dictionary<string, object>();
        private int randomNum = 0;

        /*
         *  b_log_id	每个日志的唯一ID	游戏名+事件名+加uid+加时间戳+随机数，例如 yjzspub999-3216049191039332853	必填	string
            b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
            b_sdk_udid	用户硬件设备号	b服SDK udid，客户端SDK登录事件接口获取，32位通用唯一识别码	必填，除实时在线事件	string
            b_sdk_uid	B站生成的uid	b服SDK uid,客户端SDK登录事件接口获取，用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
            b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
            b_tour_indicator	是否是游客账号	枚举：0/1   （0:非游客，1：游客）	必填	int
            b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
            b_utc_timestamp	时间戳	例如：1603630292	必填	int
            b_datetime	日期格式	该动作实际发生的当地时间，格式 YYYY-MM-DD HH:MM:SS	必填	string
            b_game_base_id	游戏唯一id	每款游戏的唯一ID，通过运营SDK获得，每款游戏写死值即可	必填	string
            b_game_id	游戏id	一款游戏的ID	必填	int
            b_platform	平台名称	统一：ios|android|windows	必填	string
            b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。 请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
            b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
            b_version	游戏客户端版本	游戏的迭代版本，例如1.0.3	必填	string
            b_eventname	日志方法名/事件名	例如登录事件上报player_login	必填	string
            level	当前角色等级	当前角色等级，例如：2，如果最低等级不是1，给个默认值0	必填，玩家注册和实时在线事件无此字段	int
            role_name	玩家当前角色名	例如：黄昏蔷薇行者	必填	string
         */

        /// <summary>
        /// 公共字段
        /// </summary>
        public Dictionary<string, object> GetKomoeLogCommonInfo(KomoeLogEventType eventType)
        {
            Dictionary<string, object> returnInfDic = new Dictionary<string, object>();

            if (commonInfDic.Count > 0)
            {
                returnInfDic = new Dictionary<string, object>(commonInfDic);
            }
            else
            {
                commonInfDic.Add("b_udid", DeviceId);
                commonInfDic.Add("b_sdk_udid", SDKUuid);
                commonInfDic.Add("b_sdk_uid", AccountName);
                commonInfDic.Add("b_account_id", AccountName);
                commonInfDic.Add("b_tour_indicator", Tour);
                commonInfDic.Add("b_role_id", Uid);

                commonInfDic.Add("b_game_base_id", KomoeLogConfig.GameBaseId);
                //if (Platform == "ios")
                //{
                //    commonInfDic.Add("b_game_id", 6361);
                //}
                //else
                //{
                //    commonInfDic.Add("b_game_id", 6360);
                //}
                commonInfDic.Add("b_game_id", GameId);
                commonInfDic.Add("b_platform", Platform);
                commonInfDic.Add("b_zone_id", server.MainId);
                commonInfDic.Add("b_channel_id", ChannelName);
                commonInfDic.Add("b_version", ClientVersion);

                returnInfDic = new Dictionary<string, object>(commonInfDic);
            }
            string logId = GetLogId(eventType);
            returnInfDic.Add("b_log_id", logId);
            returnInfDic.Add("b_eventname", eventType.ToString());
            returnInfDic.Add("level", Level);
            returnInfDic.Add("role_name", Name);
            returnInfDic.Add("b_utc_timestamp", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            returnInfDic.Add("b_datetime", server.Now().ToString(eventDateTimeString));

            return returnInfDic;
        }
        private string GetLogId(KomoeLogEventType eventType)
        {
            if (randomNum >= 10000)
            {
                randomNum = 1;
            }
            else
            {
                randomNum++;
            }
            return $"{KomoeLogConfig.GameBaseId}-{eventType}-{Uid}-{Timestamp.GetUnixTimeStampSeconds(server.Now())}-{randomNum}";
        }

        #region Event

        /*
        * player_login	进入游戏/选择区服时触发(用户首次登录可在注册/创角时触发)（活跃玩家00:00:00上报一条日志）	
        role_name	string	玩家角色名	例如：黄昏蔷薇行者
		gender	int	角色性别	例如：0-男，1-女
		model	string	设备机型	设备的机型，例如Samsung GT-I9208
		os_version	string	操作系统版本	操作系统版本，例如13.0.2
		network	string	网络信息	4G/3G/WIFI/2G
		mac	string	mac 地址	局域网地址
		ip	string	玩家登录IP	玩家登录IP
		cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
         */
        public void KomoeEventLogPlayerLogin()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.player_login);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            properties.Add("model", DeviceModel);
            properties.Add("os_version", OsVersion);
            properties.Add("network", Network);
            properties.Add("mac", Mac);
            properties.Add("ip", ClientIp);
            //properties.Add("cp_param", string.Join("|", localSoftwares));

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        public void KomoeEventLogGetAppList()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.get_applist);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("applist", string.Join(",", localSoftwares));
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        /*
        * player_logout	无服务端心跳时上报（活跃玩家23:59:59上报一条日志）	
        online_time	int	在线时长（秒）	登出时间戳-登录时间戳，例如：2345
		role_name	string	玩家角色名	例如：黄昏蔷薇行者
		gender	int	角色性别	例如：0-男，1-女
		model	string	设备机型	设备的机型，例如Samsung GT-I9208
		os_version	string	操作系统版本	操作系统版本，例如13.0.2
		network	string	网络信息	4G/3G/WIFI/2G
		mac	string	mac 地址	局域网地址
		ip	string	玩家登录IP	玩家登录IP
		cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
         */
        public void KomoeEventLogPlayerLogout()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.player_logout);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            int onlineTime = (int)(server.Now() - LastLoginTime).TotalSeconds;
            properties.Add("online_time", onlineTime);
            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            properties.Add("model", DeviceModel);
            properties.Add("os_version", OsVersion);
            properties.Add("network", Network);
            properties.Add("mac", Mac);
            properties.Add("ip", ClientIp);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }


        /*
         * 新手引导步骤通过	guide_flow	每通过一个步骤上报一次	
            guide_id	int	引导节点ID	描述新手引导的埋点进度由递增的int值描述进度，比如：1,2,3,20,21,100,101...上传的序号需要可以进行从小至大排序，序号可以是离散的
			guide_subid	int	引导的次级ID，如guide_id为引导组，则需填此项	描述本引导组的进度由递增的int值描述进度，比如：1,2,3,20,21,100,101...上传的序号需要可以进行从小至大排序，序号可以是离散的
			guide_name	string	新手引导点的描述	例如：升级
			guide_type	string	新手引导点的类型	例如：剧情引导
			guide_time	int	步骤时间	结束时上传
			role_name	string	玩家角色名	例如：黄昏蔷薇行者
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogGuideFlow(int guide_id, int time)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.guide_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("guide_id", guide_id);
            properties.Add("guide_subid", guide_id);
            //properties.Add("guide_name", guide_id.ToString());
            //properties.Add("guide_type", guide_id.ToString());
            properties.Add("guide_time", time);
            properties.Add("role_name", Name);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * 人物等级流水表 人物等级变化触发	player_exp	人物等级变化/人物经验变化触发	
            change_type	string	变化类型，用户区分是等级变化还是经验变化。	枚举：level-等级，exp-经验。等级升级上报两条数据
			before_level	int	变化前等级	例如：2
			before_exp	int	变化前的经验	例如：20
			exp	int	变化后的经验	例如：30
			exp_change	int	经验变化数据	例如：10
			exp_time	int	经验变化所用时间(秒)	经验值每次增加的时长
			level_time	int	升级所用时间(秒)	经验值每次增加记录一个经验值时长，升级所用时间为升级期间所有经验值增加时间之和。
			reason	string	经验流动一级原因	例如：做任务
			subreason	string	经验流动二级原因	
			role_name	string	玩家角色名	例如：黄昏蔷薇行者
			gender	int	角色性别	举例：0-男|1-女
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogPlayereExp(string change_type, int before_level, int before_exp, int exp, int exp_change, int exp_time, int level_time, string reason)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.player_exp);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("change_type", change_type);
            properties.Add("before_level", before_level);
            properties.Add("before_exp", before_exp);
            properties.Add("exp", exp);
            properties.Add("exp_change", exp_change);
            properties.Add("exp_time", exp_time);
            properties.Add("level_time", level_time);
            properties.Add("reason", reason);
            properties.Add("subreason", reason);
            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * 充值流水表 (充值成功时触发)	recharge_flow	充值成功触发	
            role_name	string	玩家角色名	例如：黄昏蔷薇行者
			order_id	long	订单号	本条记录的订单号
			amount	int	订单金额（分）（人民币档位）	例如 51800
			amount_currency	int	订单金额（分）（实际充值的金额）	例如 韩币-51800分
			currency_type	string	币种代号-实际充值金额的币种	遵循ISO 4217规范
			product_id	int	订单商品ID（唯一标记id）	见'item'描述，如果是礼包，则使用礼包自身的id仅用于订单的信息补充，
			product_type	int	订单商品类型	自己方举例表 100 虚拟币，比如：金币，钻石等 200 月卡/季卡/年卡类递延收入 300 道具，活动道具，礼包道具 400 优惠券等运营产品
			product_name	string	订单商品名称	比如，“月卡”，“一大袋钻石”一般情况下此处的道具为直接购买的商品，即礼包本身，而非礼包内的钻石仅用于订单的信息补充
			product_number	int	订单商品数量	道具 被获得 或者 被消耗 的数量，如果是礼包则为1 注意，消耗道具也请传正数，比如消耗100个，请传'100'，而非'-100'
			total_pay	int	累计充值金额（分）（人民币）	例如 51800
			total_pay_currency	int	累计充值金额（分）（当地货币）	例如 韩币-51800分
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogRechargeFlow(long order_id, float amount, float amount_currency, string currency_type, int product_id, int product_type,
            string product_name, int product_number, float total_pay, float total_pay_currency, int is_sandbox)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.recharge_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("role_name", Name);
            properties.Add("order_id", order_id);
            properties.Add("amount", amount);
            properties.Add("amount_currency", amount_currency);
            properties.Add("currency_type", currency_type);
            properties.Add("product_id", product_id);
            properties.Add("product_type", product_type);
            properties.Add("product_name", product_name);
            properties.Add("product_number", product_number);
            properties.Add("total_pay", total_pay);
            properties.Add("total_pay_currency", total_pay_currency);
            properties.Add("is_sandbox", is_sandbox);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        /*
         * 礼包推送	gift_push	礼包推送时触发	
            gift_type	int	礼包类型	根据游戏定义，如 新手礼包，活动礼包等
			gift_id	int	礼包id，唯一标记id	如：100021
			gift_name	string	礼包名称	如：新手专属6元礼包
			gift_price	int	礼包价格 	如：6
			award	array	礼包包含内容	得的道具ID，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogGiftPush(int gift_type, int gift_id, string gift_name, float gift_price, int actionId, List<Dictionary<string, object>> award, ulong id, string dataBox)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.gift_push);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gift_type", gift_type);
            properties.Add("gift_id", gift_id);
            properties.Add("gift_name", gift_name);
            properties.Add("gift_price", gift_price);
            properties.Add("reason", actionId);
            properties.Add("award", award);
            properties.Add("data_box", dataBox);
            properties.Add("gift_push_uid", id);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        public void KomoeEventLogGiftPushBuy(int gift_type, int gift_id, string gift_name, float gift_price, int actionId, List<Dictionary<string, object>> award, ulong id, string dataBox)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.gift_push_buy);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gift_type", gift_type);
            properties.Add("gift_id", gift_id);
            properties.Add("gift_name", gift_name);
            properties.Add("gift_price", gift_price);
            properties.Add("reason", actionId);
            properties.Add("award", award);
            properties.Add("gift_push_uid", id);
            properties.Add("data_box", dataBox);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * 道具产销表	item_flow	道具变化时上报	
            act_type	string	是买入还是消耗	枚举：add-增加 |reduce-减少 必须记为add or reduce
			related_eventname	string	道具流动的关联事件	战斗消耗/商城购买
			related_event_logid	string	道具流动的关联事件的log_id	22:00:00商城消耗了5种货币，产生5条商城消耗日志，关联同一商城购买的log_id
			item_id	int	道具ID	道具ID
			item_name	string	道具名称	道具名称
			item_type	string	道具类型	道具类型
			item_num	int	变化数量	道具买入或者消耗的数量 （一定为正数）
			before_count	long	动作前的物品存量	动作前的物品存量
			after_count	long	动作后的物品存量	动作后的物品存量
			reason	int	道具流动一级原因	1-货币购买2-战斗获得3-完成任务获得
			subreason	int	道具流动二级原因	
			related_order_id	long	关联订单号	若充值获得，则记录关联订单号
			gold_type	int	货币类型(消耗货币的类型)	无货币类型传空1-金子2-绑金3-银子4-绑银5-魅力6-功勋7-贡献度8-boss积分9-活动积分10-英雄令封印积分11-试练塔积分
			gold_num	long	动作涉及的货币数	动作涉及的货币数，无货币消耗传0
			role_name	string	玩家角色名	例如：黄昏蔷薇行者
			gender	int	角色性别	举例：0-男|1-女
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogItemFlow(string act_type, string related_event_logid, int item_id, string item_type,
            int item_num, int before_count, int after_count, int reason, int subreason, long related_order_id, int gold_type, int gold_num)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.item_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("act_type", act_type);
            //properties.Add("related_eventname", related_eventname);
            properties.Add("related_event_logid", related_event_logid);
            properties.Add("item_id", item_id);
            //properties.Add("item_name", item_name);
            properties.Add("item_type", item_type);
            properties.Add("item_num", item_num);
            properties.Add("before_count", before_count);
            properties.Add("after_count", after_count);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            properties.Add("related_order_id", related_order_id);
            properties.Add("gold_type", gold_type);
            properties.Add("gold_num", gold_num);
            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
         * 货币产销表	gold_flow	货币变化时上报	
            act_type	string	货币是买入还是消耗	枚举：add-增加 |reduce-减少 必须记为add or reduce
			related_eventname	string	货币流动的关联事件	例如：战斗消耗|商城购买
			related_event_logid	string	货币流动的关联事件的log_id	22:00:00商城消耗了5种货币，产生5条商城消耗日志，关联同一商城购买的log_id
			gold_id	int	货币ID	货币ID
			gold_name	string	货币名称	货币名称
			gold_type	int	货币类型	举例-1-金子2-绑金3-银子4-绑银5-魅力6-功勋7-贡献度8-boss积分9-活动积分10-英雄令封印积分11-试练塔积分
			gold_num	long	动作涉及的货币数	动作涉及的货币数 （一定为正数）
			before_count	long	动作前货币数	动作前货币数
			after_count	long	动作后的货币数	动作后的货币数
			reason	int	货币流动一级原因	货币流动一级原因1-充值获得
			subreason	int	货币流动二级原因	货币流动二级原因
			related_order_id	long	关联订单号	若充值获得，则记录关联订单号
			role_name	string	玩家角色名	例如：黄昏蔷薇行者
			gender	int	角色性别	举例：0-男|1-女
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogGoldFlow(string act_type, string related_event_logid, int gold_id, string gold_name, int gold_type,
            int gold_num, int before_count, int after_count, int reason, int subreason, int related_order_id)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.gold_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("act_type", act_type);
            //properties.Add("related_eventname", related_eventname);
            properties.Add("related_event_logid", related_event_logid);
            properties.Add("gold_id", gold_id);
            properties.Add("gold_name", gold_name);
            properties.Add("gold_type", gold_type);
            properties.Add("gold_num", gold_num);
            properties.Add("before_count", before_count);
            properties.Add("after_count", after_count);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            properties.Add("related_order_id", related_order_id);
            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 商店购买行为	shop_purchase	商城/代币商城购买物品时上报	
            good_id	int	道具id	
			good_name	string	道具名称	道具名或礼包名
			good_num	int	购买数量	
			item_id	int	购买或兑换道具消耗的货币ID	如免费为空
			itme_name	string	购买或兑换道具消耗的货币名称	如免费为空
			itme_num	int	购买或兑换道具消耗的货币数量	如免费为空
			shop_id	string	商店ID	
			shop_name	string	商店名	例如: 银币商店
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogShopPurchase(int good_id, int good_num, int item_id, string itme_name, int itme_num, int shop_id, string shop_name)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.shop_purchase);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("good_id", good_id);
            //properties.Add("good_name", good_name);
            properties.Add("good_num", good_num);
            properties.Add("item_id", item_id);
            properties.Add("itme_name", itme_name);
            properties.Add("itme_num", itme_num);
            properties.Add("shop_id", shop_id);
            properties.Add("shop_name", shop_name);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 任务流水	mission_flow	接受/完成/领取/重做任务 时触发日志，比如达到50级、60级为任务，可以领取奖励等) 触发条件:任务接取、完成、领取奖励、重新做任务各一次	
            b_type	string	副本名称	魂师手札-阅历奖励;魂师手札-每日好礼;日常-每日任务;日常-每周任务;拟态训练-领取奖励;福利-战力值奖励;福利-每日签到;福利-成长基金
			mission_id	int	任务ID	任务ID
			mission_name	string	任务名	任务名
			mission_type	int	任务类型	例如：1-普通任务|2-关卡任务|3-日常任务
			mission_status	int	任务操作状态	例如：1-接受|2-完成|3-领取奖励
			exp	int	当前等级的经验	例如：2
			power	int	玩家战力	例如：2
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogMissionFlow(string b_type, int mission_id, int mission_type, int mission_status, int exp, int power)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.mission_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("b_type", b_type);
            properties.Add("mission_id", mission_id);
            //properties.Add("mission_name", mission_name);
            properties.Add("mission_type", mission_type);
            properties.Add("mission_status", mission_status);
            properties.Add("exp", exp);
            properties.Add("power", power);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 邮件流水	mail_flow	 邮件变化时触发	
            act_type	int	邮件的操作类型	1-发送邮件2- 接收邮件3-读取邮件4-删除邮件5- 清空邮箱6- 领取道具7-领取全部道具10-其他行为
			mail_type	int	邮件类型	1-系统邮件2-运营邮件3-玩家邮件
			from_id	int	发件人角色ID	
			from_name	string	发件人玩家角色名	
			mail_id	int	邮件ID	
			mail_title	string	邮件标题	
			mail_content	string	邮件内容	
			mail_createtime	datetime	邮件创建时间, 格式 YYYY-MM-DD HH:MM:SS	
			mail_deltime	datetime	邮件过期时间, 格式 YYYY-MM-DD HH:MM:SS	
			item_got	int	道具是否领取,0|1	
			bind_item_num	array	绑定（领取）邮件道具的数量	json上传{"item_id":5533,"item_type":10,"count":10}
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogMailFlow(int act_type, int mail_type, int mail_id, string mail_title, string mail_content,
            int mail_createtime, int mail_deltime, int item_got, List<Dictionary<string, object>> bind_item_num)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.mail_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("act_type", act_type);
            properties.Add("mail_type", mail_type);
            //properties.Add("from_id", from_id);
            //properties.Add("from_name", from_name);
            properties.Add("mail_id", mail_id);
            properties.Add("mail_title", mail_title);
            properties.Add("mail_content", mail_content);
            properties.Add("mail_createtime", mail_createtime);
            properties.Add("mail_deltime", mail_deltime);
            properties.Add("item_got", item_got);
            properties.Add("bind_item_num", bind_item_num);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 运营活动流水	operational_activity	活动完成触发日志，这里的活动是周期性的运营活动，比如签到、礼盒	
            activity_id	int	活动ID	例如：1000004254
			activity_second_id	int	子活动ID	例如：1000004254
			activity_name	string	活动名字	例如：弗兰德古董店；烈阳祝福；
			activity_second_name	string	子活动名字	例如：日礼包；烈阳祝福；探宝-普通;探宝-高级;
			activity_type	int	活动类型，如充值类的，登录类的，活动为运营过程中做的一些活动	activity_type 根据游戏情况自定义1-充值类2-登录类3-运营活动
			activity_status	int	活动操作状态	例如：1-接受|2-完成|3-领取奖励|4-探宝|5-幸运抽奖
			activity_days	int	任务/奖励 对应活动的第几天	任务/奖励 对应活动的第几天，如无天数区分则留空
			activity_progress	int	变动后完成任务进度	例如: 第一天有5个任务已经完成3个, 记3;累充三天,记3等
			b_api	string	服务端触发的api	触发本条行为记录的请求api，当觉得服务端api可以进一步协助描述该行为时，可以记录api名例如：b_eventname = completed(战斗结束)API=/hardMission/endBattlee 结束困难本战斗
                            /materialMission/endBattle 材料本结束战斗/recaptureMission/endBattle反夺回结束战斗/storyMission/endBattle 剧情本结束战斗
			
           mission_id	string	具体每个任务/奖励对应的id	完成的活跃任务对应任务id(如无可为空)
           mission_name	string	任务名称	例如：累计充值XX美元，完成三次魂骨淬炼
           award	array	奖励道具ID及数量	任务完成时触发，获得的道具ID，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
           role_name	string	玩家角色名	例如：黄昏蔷薇行者

           *累计充值活动        
           activity_name	string	活动名称	活动/魂师手札/日常/累充活动/累耗活动
           activity_second_name	string	活动名称	"活动:海神九考/登山庆典/凶兽森林/深海之森/快乐打工人/泰坦森林(完成进度奖励)/百炼神石
                                                魂师手札: 阅历奖励/每日好礼
                                                日常任务:每日任务/一周任务
           activity_type	int	活动类型，如充值类的，登录类的，活动为运营过程中做的一些活动	"activity_type 根据游戏情况自定义
                                                                                        1-累计充值类
                                                                                        2-活跃任务类
                                                                                        3-累计消耗类"
           activity_progress	string	完成任务进度	例如: 对应的累充档位，活跃任务的活跃度,累计登录第几天

            cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogOperationalActivity(int activity_id, string name, string activityName, int activityType, int activity_status, float activity_days, float activity_progress, string mission_id, string mission_name, List<Dictionary<string, object>> award, string role_name)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.operational_activity);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("activity_id", activity_id);
            properties.Add("activity_second_id", activity_id);
            properties.Add("activity_name", name);
            properties.Add("activity_second_name", activityName);
            properties.Add("activity_type", activityType);
            properties.Add("activity_status", activity_status);
            properties.Add("activity_days", activity_days);
            properties.Add("activity_progress", activity_progress);
            properties.Add("b_api", "");

            properties.Add("mission_id", mission_id);
            properties.Add("mission_name", mission_name);
            properties.Add("award", award);
            properties.Add("role_name", role_name);

            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        /*
        * 功能开启日志	function_open	玩家通过引导，开启某功能时上报	
            function_id	string	功能ID	功能开放配置表配置的相关内容
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogFunctionOpen(string function_id)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.function_open);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("function_id", function_id);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 玩家信息变更	user_info_change	玩家修改个人信息时触发	
            info_change_type	string	信息变化	1-姓名变更；2-性别变更；3-更改信息
			info_before	string	修改前信息	
			info_after	string	修改后信息	
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLoguUserInfoChange(string info_change_type, string info_before, string info_after)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.user_info_change);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("info_change_type", info_change_type);
            properties.Add("info_before", info_before);
            properties.Add("info_after", info_after);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 礼包码兑换	gift_code_exchange	礼包码兑换时触发	
            gift_code	string	礼包码	
			results	string	兑换结果	成功/失败
			reasons	string	错误原因	
			award	array	重置返还	返还的数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]`
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLoguGiftCodeExchange(string gift_code, string results, string reasons, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.gift_code_exchange);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gift_code", gift_code);
            properties.Add("results", results);
            properties.Add("reasons", reasons);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 角色称号变动表	player_title	角色称号变动表	
            gender	int	角色性别	举例：0-男|1-女
			change_type	int	操作类型	1-激活称号; 2-装备; 3-卸下;
			achievement_type	int	称号类型	例如：1-养成；2-活动；3-互动; 4-特殊
			achievement_id	string	称号ID	
			achievement_name	string	称号名称	
			power_before	string	变化前战力	
			power_after	string	变化后战力	
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLoguPlayerTitle(int change_type, int achievement_type, int achievement_id, int power_before, int power_after)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.player_title);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("change_type", change_type);
            properties.Add("achievement_type", achievement_type);
            properties.Add("achievement_id", achievement_id);
            //properties.Add("achievement_name", achievement_name);
            properties.Add("power_before", power_before);
            properties.Add("power_after", power_after);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 聊天流水	chat_flow	聊天相关日志(聊天频道发言的文字内容，发送时记录)	
            role_name	string	玩家角色名	例如：梅格科林
			gender	int	角色性别	举例：0-男|1-女
			chat_type	int	信息类型	信息类型，根据游戏实际聊天频道制定，如1-私聊;2-系统;3-世界;4-阵营;5-队伍;6-跨服传音;7-表情
			chat_contents	string	信息内容	例如：一起去打boss
			receive_id	int	接收者id	私聊为玩家id，社团为社团id，群聊为群聊id，世界频道填写0
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogChatFlow(int chat_type, string chat_contents, int receive_id)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.chat_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            properties.Add("chat_type", chat_type);
            properties.Add("chat_contents", chat_contents);
            properties.Add("receive_id", receive_id);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*yi
        * 好友操作	friend_flow	进行好友相关操作时记录	
            role_name	string	玩家角色名	例如：梅格科林
			gender	int	角色性别	举例：0-男|1-女
			operate_type	int	操作类型	举例：1-申请好友; 2-同意申请; 3-忽略申请;4-好友删除; 5-拉入黑名单;6-移出黑名单; 7-举报;8-义结金兰申请;  9-义结金兰同意; 
			operate_name	int	操作名称	例如：申请好友；忽略申请
			friend_id	string	对应好友id	
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogFriendFlow(int operate_type, string operate_name, int friend_id)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.friend_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("role_name", Name);
            properties.Add("gender", Sex);
            properties.Add("operate_type", operate_type);
            properties.Add("operate_name", operate_name);
            properties.Add("friend_id", friend_id);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 招募	draw_card	玩家进行抽卡时记录	
            draw_type	int	抽卡类型	0-限时招募，1-稀有招募，3-友情招募
			draw_id	string	抽卡卡池id	
			draw_sub_id	string	子卡池id	
			cost	array	抽卡消耗道具ID及数量	抽卡消耗的道具ID，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			draw_cnt	int	记录对应抽卡次数	例如：1，7
			draw_guarantee_base	string	还剩多少次有保底	例如: 47/49
			award	array	抽卡奖励道具ID及数量	抽卡获得的道具ID，数组上传[{"itemId":5533：,"count":10},{"itemId":1247,"count":100},...]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogDrawCard(int draw_type, List<Dictionary<string, object>> cost,
            int draw_cnt, string draw_guarantee_base, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.draw_card);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("draw_type", draw_type);
            properties.Add("draw_id", draw_type.ToString());
            //properties.Add("draw_sub_id", draw_type.ToString());
            properties.Add("cost", cost);
            properties.Add("draw_cnt", draw_cnt);
            properties.Add("draw_guarantee_base", draw_guarantee_base);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 排行榜	rank_flow	成就完成时触发	
            achieve_id	string	排行榜对应ID	
			achieve_type	int	排行榜类型	0-战力，1-猎杀魂兽，2-斗魂之路，3-秘境，4-大斗魂场，5-荣耀魂师大赛6-神祗贡献度
			rank_before	int	变化前排名	例如: 1，2，未上榜
			rank_after	int	变化后排名	例如: 1，2，未上榜
			award	array	如有，奖励道具ID及数量	获得的道具ID，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			achieve_level	string	对应的进度内容	例如: 战力6509k;难度系数 539; 【五怪登阶】19-23; 贡献值: 10270等
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogRankFlow(RankType achieve_type, int rank_before, int rank_after,
            string achieve_level, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.rank_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("achieve_id", (int)achieve_type);
            properties.Add("achieve_type", achieve_type.ToString());
            properties.Add("rank_before", rank_before);
            properties.Add("rank_after", rank_after);
            properties.Add("achieve_level", achieve_level);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 羁绊	tie_flow	羁绊变化时触发	
            achieve_id	string	对应的羁绊ID	
			achieve_name	string	羁绊名称	
			achieve_type	int	触发类型	1-羁绊完成; 2-羁绊激活
			power_before	int	变化前总战力	例如：3000
			power_after	int	变化后总战力	例如：3000
			power_change	int	总战力变化	例如：124
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogTieFlowstring(int achieve_id, string achieve_name, int achieve_type, int power_before, int power_after)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.tie_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("achieve_id", achieve_id);
            properties.Add("achieve_type", achieve_type);
            //properties.Add("achieve_name", achieve_type);
            properties.Add("power_before", power_before);
            properties.Add("power_after", power_after);
            properties.Add("power_change", power_after - power_before);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 阵营变化流水	camp_flow	阵营有操作时记录	
            gender	int	角色性别	举例：0-男|1-女
			camp_id	string	阵营ID	
			camp_name	string	阵营名称	例如:天斗;星罗;帝国
			operate_type	int	操作类型	1-加入阵营；2-转换阵营；
			camp_position	int	阵营职位	0-无职位; 1-国师；2-元帅；3-统领；
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogCampFlow(string camp_id, string camp_name, int operate_type, int camp_position)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.camp_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("camp_id", camp_id);
            properties.Add("camp_name", camp_name);
            properties.Add("operate_type", operate_type);
            properties.Add("camp_position", camp_position);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 阵营星宿变化流水	camp_constellation	阵营星宿变化时记录	
        * 
            gender	int	角色性别	举例：0-男|1-女
			camp_id	string	阵营ID	
			camp_name	string	阵营名称	例如:天斗;星罗;帝国
			camp_position	int	阵营职位	0-无职位; 1-国师；2-元帅；3-统领；
			change_type	int	星宿种类	1-白虎; 2-青龙; 3-朱雀; 4-玄武;
			skill_name	string	对应的技能名	例如: 太阿,龙渊等
			before_level	int	变化前等级	
			after_level	int	变化后等级	
			consume	array	消耗	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	 n'c'f'f
        */
        public void KomoeEventLogCampConstellation(string camp_id, string camp_name, int camp_position, int change_type, string skill_name,
            int before_level, int after_level, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.camp_constellation);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("camp_id", camp_id);
            properties.Add("camp_name", camp_name);
            properties.Add("camp_position", camp_position);
            properties.Add("change_type", change_type);
            properties.Add("skill_name", skill_name);
            properties.Add("before_level", before_level);
            properties.Add("after_level", after_level);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "n'c'f'f");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 阵营膜拜	camp_worship	阵营玩法操作时记录	
        * 
            gender	int	角色性别	举例：0-男|1-女
			camp_id	string	阵营ID	
			camp_name	string	阵营名称	例如:天斗;星罗;帝国
			camp_position	int	阵营职位	0-无职位; 1-国师；2-元帅；3-统领；
			target_role_id	string	觐见对象	
			consume	array	消耗的道具/货币	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogCampWorship(string camp_id, string camp_name, int camp_position, int target_role_id, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.camp_worship);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("camp_id", camp_id);
            properties.Add("camp_name", camp_name);
            properties.Add("camp_position", camp_position);
            properties.Add("target_role_id", target_role_id);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 阵营对决	camp_battle	阵营对决操作时记录	
        * 
            gender	int	角色性别	举例：0-男|1-女
			camp_id	string	阵营ID	
			camp_name	string	阵营名称	例如:天斗;星罗;帝国
			camp_position	int	阵营职位	0-无职位; 1-国师；2-元帅；3-统领；
			change_type	int	操作类型	1-属性增益; 2-采集; 3-挑战; 4-进攻;
			power	int	战力	
			cost_time	int	战斗耗费时间（秒）	
			result	int	结果	0-非战斗事件; 1-胜利；2-失败；3-主动退出
			enemy_camp_id	string	目标的阵营ID	
			enemy_camp	string	目标的阵营名称	例如:天斗;星罗;帝国。属性增益和采集行为计空
			enemy_id	string	目标的角色id	属性增益和采集行为计空
			event_type	int	触发事件的种类	例如: 采集事件会触发1-战斗; 2-非战斗;
			consume	array	消耗的行动点/货币	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogCampBattle(string camp_id, string camp_name, int camp_position, int change_type, int power, int cost_time, int result,
            string enemy_camp_id, string enemy_camp, string enemy_id, int event_type, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.camp_battle);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("camp_id", camp_id);
            properties.Add("camp_name", camp_name);
            properties.Add("camp_position", camp_position);
            properties.Add("change_type", change_type);
            properties.Add("power", power);
            properties.Add("cost_time", cost_time);
            properties.Add("result", result);
            properties.Add("enemy_camp_id", enemy_camp_id);
            properties.Add("enemy_camp", enemy_camp);
            properties.Add("enemy_id", enemy_id);
            properties.Add("event_type", event_type);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * 阵营建设	camp_build	阵营建设操作时记录	
        * 
            gender	int	角色性别	举例：0-男|1-女
			camp_id	string	阵营ID	
			camp_name	string	阵营名称	例如:天斗;星罗;帝国
			camp_position	int	阵营职位	0-无职位; 1-国师；2-元帅；3-统领；
			change_type	int	操作类型	1-掷骰子; 2-领取宝箱; 3-购买次数
			steps	int	走动的步数	
			before_count	int	变化前建设度	
			after_count	int	变化后建设度	
			change_amount	int	变化的建设度	
			flag_before_count	int	变化前夺旗数	
			flag_after_count	int	变化后夺旗数	
			steps_left	int	还剩几步获得双倍奖励	
			consume	array	消耗的建设次数/货币	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			award	long	结算奖励	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogCampBuild(string camp_id, string camp_name, int camp_position, int change_type, int steps, int before_count, int after_count,
            int change_amount, int flag_before_count, int flag_after_count, int steps_left, List<Dictionary<string, object>> consume, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.camp_build);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("gender", Sex);
            properties.Add("camp_id", camp_id);
            properties.Add("camp_name", camp_name);
            properties.Add("camp_position", camp_position);
            properties.Add("change_type", change_type);
            properties.Add("steps", steps);
            properties.Add("before_count", before_count);
            properties.Add("after_count", after_count);
            properties.Add("change_amount", change_amount);
            properties.Add("flag_before_count", flag_before_count);
            properties.Add("flag_after_count", flag_after_count);
            properties.Add("steps_left", steps_left);
            properties.Add("consume", consume);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * PVE/爬塔	pve_fight	对应玩法战斗结束时记录	
        * 
            stage_type	int	副本类型	0-秘境;1-兽袭；2-猎杀魂兽; 3-高原之乡; 4-冰火两仪; 5-异域迷岛; 6-大斗魂场~深渊战场；7-拟态训练-快速战斗;8-斗魂之路;9-泰坦森林对战;
			stage_id	string	副本id	
			stage_sub_id	string	副本子id	若无，可空 
			operation_type	int	操作种类	1-正常战斗; 2-扫荡; 3-购买次数; 4-领取奖励; 5-挑战统帅(深渊战场)
			battleteam	array	上阵阵容明细	数组上传[{"位置id":5,"hero_id":10,"hero_power":1000}...],位置上为空不记录。若未操作，为空 
			rest_times	int	今日剩余次数	
			player_power	int	玩家战力	
			cost_time	int	战斗耗费时间（秒）	
			if_buff	int	是否有buff	buff包内的buff,没有不记
			result	int	结果	1-胜利；2-失败；3-主动退出
			if_first	int	是否首通	0
			team_detail	array	挑战组队队伍明细	数组上传[{"uid":5533,"leve":10},{"uid":1247,"level":100}],若单人挑战，为空 
			award	long	结算奖励	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			consume	array	消耗次数/道具/货币	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogPveFight(int stage_type, string stage_id, string stage_sub_id, int operation_type, List<Dictionary<string, object>> battleteam, int rest_times, int player_power,
            int cost_time, int if_buff, int result, int if_first, List<Dictionary<string, object>> team_detail, List<Dictionary<string, object>> consume, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.pve_fight);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("stage_type", stage_type);
            properties.Add("stage_id", stage_id);
            properties.Add("stage_sub_id", stage_sub_id);
            properties.Add("operation_type", operation_type);
            properties.Add("battleteam", battleteam);
            properties.Add("rest_times", rest_times);
            properties.Add("player_power", player_power);
            properties.Add("cost_time", cost_time);
            properties.Add("if_buff", if_buff);
            properties.Add("result", result);
            properties.Add("if_first", if_first);
            properties.Add("team_detail", team_detail);
            properties.Add("consume", consume);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        * PVP	pvp_fight	对应玩法战斗结束时记录	
        * 
            stage_type	int	副本类型	1-大斗魂场~竞技场；2-荣耀魂师大赛; 3-好友切磋;
			stage_id	string	副本id	
			stage_sub_id	string	副本子id	若无，可空 
			operation_type	int	操作种类	1-正常战斗; 2-竞猜; 3-购买次数; 4-领取奖励; 5-换一批(刷新)
			battleteam	array	上阵阵容明细	数组上传[{"位置id":5,"hero_id":10,"hero_power":1000}...],位置上为空不记录。若未操作，为空 
			rest_times	int	今日剩余次数	
			cost_time	int	耗费时间（秒）	
			result	int	结果	1-胜利；2-失败
			before_rank	int	对战前排名	
			after_rank	int	对战后排名	
			before_honor	string	对战前徽章星数	例如: 银质-5星
			after_honor	string	对战后徽章星数	例如: 银质-5星
			enemy_id	string	竞技对象id	
			enemy_power	int	竞技对象战力	
			player_power	int	玩家战力	
			award	array	奖励	数组上传[{"buffid":123,"count":10},{"buffid":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogPvpFight(int stage_type, string stage_id, string stage_sub_id, int operation_type, List<Dictionary<string, object>> battleteam, int rest_times, int player_power,
            int cost_time, int result, int before_rank, int after_rank, string before_honor, string after_honor, int enemy_id, long enemy_power, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.pvp_fight);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("stage_type", stage_type);
            properties.Add("stage_id", stage_id);
            properties.Add("stage_sub_id", stage_sub_id);
            properties.Add("operation_type", operation_type);
            properties.Add("battleteam", battleteam);
            properties.Add("rest_times", rest_times);
            properties.Add("cost_time", cost_time);
            properties.Add("result", result);
            properties.Add("before_rank", before_rank);
            properties.Add("after_rank", after_rank);
            properties.Add("before_honor", before_honor);
            properties.Add("after_honor", after_honor);
            properties.Add("enemy_id", enemy_id);
            properties.Add("enemy_power", enemy_power);
            properties.Add("player_power", player_power);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *组队操作流水	team_flow	组队相关操作时记录	
        *
            operate_type	int	操作类型	1-创建队伍；2-申请加入; 3-加入队伍；4-邀请附近的人； 5-邀请好友; 6-退出队伍; 7-踢出队伍; 
			team_id	string	队伍id	
			team_detail	array	队伍明细	数组上传[{"uid":5533,"leve":10},{"uid":1247,"level":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogTeamFlow(int operate_type, string team_id, List<Dictionary<string, object>> team_detail)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.teamform_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("operate_type", operate_type);
            properties.Add("team_id", team_id);
            properties.Add("team_detail", team_detail);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *主线任务	main_task	进行主线任务时记录	
        *
            stage_type	string	副本类型	0-主线; 1-支线;
			mission_id	string	任务id	
			mission_name	string	任务名	任务名
			mission_type	int	任务类型	例如：1-交谈任务|2-关卡任务|3-战斗任务
			mission_status	int	任务操作状态	例如：1-接受|2-完成|3-领取奖励
			battleteam	array	上阵阵容明细	数组上传[{"位置id":5,"hero_id":10,"hero_power":1000}...],位置上为空不记录。若未操作，为空 
			cost_time	int	任务完成耗时（秒）	
			result	int	任务结果	1-胜利；2-失败
			award	array	任务奖励	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			exp	int	当前等级的经验	例如：2
			power	int	玩家战力	例如：200000
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogMainTask(string stage_type, string mission_id, int mission_type, int mission_status,
            List<Dictionary<string, object>> battleteam, List<Dictionary<string, object>> award, int exp, int power)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.main_task);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("stage_type", stage_type);
            properties.Add("mission_id", mission_id);
            //properties.Add("mission_name", mission_name);
            properties.Add("mission_type", mission_type);
            properties.Add("mission_status", mission_status);
            properties.Add("battleteam", battleteam);
            //properties.Add("cost_time", cost_time);
            properties.Add("result", 1);
            properties.Add("award", award);
            properties.Add("exp", exp);
            properties.Add("power", power);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *委托任务	delegate_tasks	进行委托任务时记录	
        *
            operate_type	int	操作类型	1-委派；2-立即完成; 3-领取奖励；4-任务升星； 
			task_id	string	任务id	
			task_name	string	任务名	如: 贵族的赌约
			battleteam	array	委派阵容明细	数组上传[{"位置id":5533,"角色卡itemid":10}...],位置上为空不记录。若未操作，为空 
			task_star_before	int	变化前任务星数	任务升星时触发，其他为空
			task_star_after	int	变化后任务星数	任务升星时触发，其他为空
			task_star	int	委派时的任务星数	
			cost_time	int	任务完成耗时（秒）	
			result	int	任务结果	1-胜利；2-失败
			consume	array	消耗货币	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			award	array	任务奖励	数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogDelegateTasks(int operate_type, string task_id, string task_name, List<Dictionary<string, object>> battleteam,
            int task_star_before, int task_star_after, int task_star, int cost_time, int result, List<Dictionary<string, object>> award, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.delegate_tasks);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("operate_type", operate_type);
            properties.Add("task_id", task_id);
            properties.Add("task_name", task_name);
            properties.Add("battleteam", battleteam);
            properties.Add("task_star_before", task_star_before);
            properties.Add("task_star_after", task_star_after);
            properties.Add("task_star", task_star);
            properties.Add("cost_time", cost_time);
            properties.Add("result", result);
            properties.Add("consume", consume);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师获取	hero_resource	魂师获取时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_quality	string	卡牌角色品质	0-N，1-R，2-SR，3-SSR
			hero_level	int	获取时卡牌角色等级	获取等级的初始等级
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			reason	string	一级原因	例如：抽卡，碎片合成，奖励领取等
			subreason	string	二级原因	例如：xxx具体卡池,xxx具体活动，礼包等
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroResource(string hero_id, string hero_name, string hero_quality, int hero_level,
            string hero_profession, string reason, string subreason)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_resource);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_quality", hero_quality);
            properties.Add("hero_level", hero_level);
            properties.Add("hero_profession", hero_profession);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师武魂升级	hero_levelup	魂师武魂等级变化触发	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_quality	string	卡牌角色品质	0-N，1-R，2-SR，3-SSR
			hero_skin	string	佩戴的神位id	如无，为空
			before_level	int	变化前等级	例如：2
			after_level	int	变化后等级	例如：5
			hero_power	int	武魂战力	例如：3000
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	武魂战力变化	例如：124
			reason	string	升级原因	例如：经验升级，共鸣等
			consume	array	等级变化消耗	加经验消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroLevelup(string hero_id, string hero_name, string hero_quality, string hero_skin,
            int hero_level, int before_level, int after_level, int hero_power, int hero_power_before, int hero_power_after, int hero_power_change,
            string reason, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_levelup);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_quality", hero_quality);
            properties.Add("hero_skin", hero_skin);
            properties.Add("hero_level", hero_level);
            properties.Add("before_level", before_level);
            properties.Add("after_level", after_level);
            properties.Add("hero_power", hero_power);
            properties.Add("hero_power_before", hero_power_before);
            //properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            properties.Add("reason", reason);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师升星	hero_starup	魂师升星变化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_quality	string	卡牌角色品质	0-N，1-R，2-SR，3-SSR
			before_star	int	变化前等级	例如：2
			after_star	int	变化后等级	例如：5
			star_quality	int	武魂星品	1-白色，2-蓝色，3-紫色，4-橙色，5-红色，6-金色
			consume	array	突破消耗材料	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroStarup(string hero_id, string hero_name, string hero_quality, int before_star,
            int after_star, int star_quality, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_starup);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_quality", hero_quality);
            properties.Add("before_star", before_star);
            properties.Add("after_star", after_star);
            //properties.Add("star_quality", star_quality);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师神位	hero_skin_resource	神位变化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			change_type	int	变化的种类	1-激活，2-更换
			hero_skin_before	string	变化前的神位id	
			hero_skin_after	string	变化后的神位id	
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	变化的武魂战力	例如：3000
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroSkinResource(string hero_id, string hero_name, string hero_profession, int change_type,
            string hero_skin_before, string hero_skin_after, int hero_power_before, int hero_power_after, int hero_power_change)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_skin_resource);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_profession", hero_profession);
            properties.Add("change_type", change_type);
            properties.Add("hero_skin_before", hero_skin_before);
            properties.Add("hero_skin_after", hero_skin_after);
            properties.Add("hero_power_before", hero_power_before);
            properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师天赋变化	modifytp_flow	天赋变化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			change_type	int	变化的种类	1-重置天赋，2-保存分配
			modifytp	array	天赋分配	分配的属性ID和数量，数组上传[{"id":5533,"count":10},{"id":1247,"count":100}]; 属性:力量,体质,敏捷,技巧
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	变化的武魂战力	例如：3000
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogModifytpFlow(string hero_id, string hero_name, string hero_profession, int change_type,
            List<Dictionary<string, object>> modifytp, int hero_power_before, int hero_power_after, int hero_power_change)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.modifytp_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_profession", hero_profession);
            properties.Add("change_type", change_type);
            properties.Add("modifytp", modifytp);
            properties.Add("hero_power_before", hero_power_before);
            properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师魂技升级	hero_skill_levelup	魂技升级变化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			hero_energy_before	int	变化前充能数	例如：3
			hero_energy_after	int	变化后充能数	例如：4
			hero_skill_before	int	变化前魂技等级	例如：2
			hero_skill_after	int	变化后魂技等级	例如：3
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	变化的武魂战力	例如：3000
			consume	array	突破消耗材料	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroSkillLevelup(string hero_id, string hero_name, string hero_profession, int hero_energy_before, int hero_energy_after,
         int hero_skill_before, int hero_skill_after, int hero_power_before, int hero_power_after, int hero_power_change, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_skill_levelup);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_profession", hero_profession);
            properties.Add("hero_energy_before", hero_energy_before);
            properties.Add("hero_energy_after", hero_energy_after);
            properties.Add("hero_skill_before", hero_skill_before);
            properties.Add("hero_skill_after", hero_skill_after);
            properties.Add("hero_power_before", hero_power_before);
            properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂师重置	hero_reset	魂师重置变化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			before_level	int	变化前等级	例如：2
			after_level	int	变化后等级	例如：5
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	变化的武魂战力	例如：3000
			consume	array	突破消耗材料	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			award	array	重置返还	返还的数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroReset(string hero_id, string hero_name, string hero_profession, int before_level, int after_level,
         int hero_power_before, int hero_power_after, int hero_power_change, List<Dictionary<string, object>> consume, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_reset);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_profession", hero_profession);
            properties.Add("before_level", before_level);
            properties.Add("after_level", after_level);
            properties.Add("hero_power_before", hero_power_before);
            properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            properties.Add("consume", consume);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂环获取	soullink_resource	魂环获取时记录	
        *
        *soullink_id	string	魂环ID	道具id和名称对应的模板ID
			soullink_unique_id	string	魂环唯一ID	可拥有多件相同道具，需要唯一id区分
			soullink_year	string	魂环年限	例如：500,2000
			soullink_attribute	json	魂环属性	
			reason	string	一级原因	例如：战斗，奖励领取等
			subreason	string	二级原因	例如：xxx具体活动，礼包等
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogSoullinkResource(string soullink_id, string soullink_unique_id, string soullink_year,
            Dictionary<string, object> soullink_attribute, string reason, string subreason)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.soullink_resource);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("soullink_id", soullink_id);
            properties.Add("soullink_unique_id", soullink_unique_id);
            properties.Add("soullink_year", soullink_year);
            properties.Add("soullink_attribute", soullink_attribute);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂骨获取	soulbone_resource	魂骨获取时记录	
        *
        *soulbone_id	string	魂骨ID	道具id和名称对应的模板ID
			soulbone_unique_id	string	魂骨唯一ID	可拥有多件相同道具，需要唯一id区分
			soulbone_quality1	string	魂骨品质1	例如: 良，优
			soulbone_quality2	string	魂骨品质2	例如: 蓝，紫
			soulbone_star	int	魂骨星级	例如：2
			soulbone_profession	string	魂骨适配职业	例如：辅助，坦克
			soulbone_position	string	魂骨装备部位	例如：头部、躯干、左手、右手、左腿、右腿
			soulbone_power	int	魂骨战力	例如：3000
			soulbone_attribute	json	魂骨属性	
			reason	string	一级原因	例如：战斗，奖励领取等
			subreason	string	二级原因	例如：xxx具体活动，礼包等
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogSoulboneResource(string soulbone_id, string soulbone_unique_id, string soulbone_quality1, string soulbone_quality2,
            int soulbone_star, string soulbone_profession, string soulbone_position, Dictionary<string, object> soulbone_attribute, string reason, string subreason)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.soulbone_resource);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("soulbone_id", soulbone_id);
            properties.Add("soulbone_unique_id", soulbone_unique_id);
            properties.Add("soulbone_quality1", soulbone_quality1);
            properties.Add("soulbone_quality2", soulbone_quality2);
            properties.Add("soulbone_star", soulbone_star);
            properties.Add("soulbone_profession", soulbone_profession);
            properties.Add("soulbone_position", soulbone_position);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *魂骨淬炼	soulbone_quenching	魂骨淬炼变化时记录	
        *
        *soulbone_id	string	魂骨唯一ID	
			soulbone_quality1	string	魂骨品质1	例如: 良，优
			soulbone_quality2	string	魂骨品质2	例如: 蓝，紫
			soulbone_star	int	魂骨星级	例如：2
			soulbone_profession	string	魂骨适配职业	例如：辅助，坦克
			soulbone_position	string	魂骨装备部位	例如：头部、躯干、左手、右手、左腿、右腿
			target_soulbone_id	string	目标魂骨唯一ID	
			soulbone_attribute	string	被替换的魂髓效果	例如:当自身满血时，伤害提高10%
			target_soulbone_attribute	string	被用作材料的魂骨的魂髓效果	例如:当自身满血时，伤害提高11%
			soulbone_power_before	int	变化前魂骨战力	例如：3000
			soulbone_power_after	int	变化后魂骨战力	例如：3000
			soulbone_power_change	int	变化的魂骨战力	例如：3000
			consume	array	突破消耗材料	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogSoulboneQuenching(string soulbone_id, string soulbone_unique_id, string soulbone_quality1, string soulbone_quality2,
            int soulbone_star, string soulbone_profession, string soulbone_position, string target_soulbone_id, string soulbone_attribute, string target_soulbone_attribute,
            int soulbone_power_before, int soulbone_power_after, int soulbone_power_change, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.soulbone_quenching);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("soulbone_id", soulbone_id);
            properties.Add("soulbone_unique_id", soulbone_unique_id);
            properties.Add("soulbone_quality1", soulbone_quality1);
            properties.Add("soulbone_quality2", soulbone_quality2);
            properties.Add("soulbone_star", soulbone_star);
            properties.Add("soulbone_profession", soulbone_profession);
            properties.Add("soulbone_position", soulbone_position);
            properties.Add("target_soulbone_id", target_soulbone_id);
            properties.Add("soulbone_attribute", soulbone_attribute);
            properties.Add("target_soulbone_attribute", target_soulbone_attribute);
            properties.Add("soulbone_power_before", soulbone_power_before);
            properties.Add("soulbone_power_after", soulbone_power_after);
            properties.Add("soulbone_power_change", soulbone_power_change);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
       *装备获取	equipment_resource	装备获取时记录	
       *
       *equipment_id	string	装备ID	装备id和名称对应的模板ID
			equipment_unique_id	string	装备唯一ID	可拥有多件相同道具，需要唯一id区分
			equipment_quality1	string	装备品质1	例如: 良，优
			equipment_quality2	string	装备品质2	例如: 蓝，紫
			equipment_star	int	装备星级	例如：2
			equipment_profession	string	装备适配职业	例如：辅助，坦克
			equipment_position	string	装备装备部位	例如：项链,戒指，护腕，鞋子
			equipment_power	int	装备战力	例如：3000
			reason	string	一级原因	例如：战斗，奖励领取等
			subreason	string	二级原因	例如：xxx具体活动，礼包等
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
       */
        public void KomoeEventLogEquipmentResource(string equipment_id, string equipment_unique_id, string equipment_quality1, string equipment_quality2,
            int equipment_star, string equipment_profession, string equipment_position, string equipment_power, string reason, string subreason)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.equipment_resource);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("equipment_id", equipment_id);
            properties.Add("equipment_unique_id", equipment_unique_id);
            properties.Add("equipment_quality1", equipment_quality1);
            properties.Add("equipment_quality2", equipment_quality2);
            properties.Add("equipment_star", equipment_star);
            properties.Add("equipment_profession", equipment_profession);
            properties.Add("equipment_position", equipment_position);
            properties.Add("equipment_power", equipment_power);
            properties.Add("reason", reason);
            properties.Add("subreason", subreason);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *装备强化	equipment_strengthen	装备强化时记录	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_quality	string	卡牌角色品质	0-N，1-R，2-SR，3-SSR
			hero_level	int	获取时卡牌角色等级	获取等级的初始等级
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			equipment_position	string	装备部位	例如：项链,戒指，护腕，鞋子
			change_before	int	变化前强化等级	例如：0
			change_after	int	变化后强化等级	例如：1
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogEquipmentStrengthen(string hero_id, string hero_name, string hero_quality, int hero_level,
            string hero_profession, string equipment_position, int change_before, int change_after)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.equipment_strengthen);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();

            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_quality", hero_quality);
            properties.Add("hero_level", hero_level);
            properties.Add("hero_profession", hero_profession);
            properties.Add("equipment_position", equipment_position);
            properties.Add("change_before", change_before);
            properties.Add("change_after", change_after);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *穿戴变化	equit_flow	穿戴变化	
        *
        *hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_profession	string	战场职业定位	例如: 辅助,单攻等
			change_type	int	更换的装备种类	1-魂环; 2-魂骨; 3-装备; 4-玄玉;
			operation_type	int	操作种类	1-装备; 2-卸下; 3-替换;
			itemid_before	string	变化前的道具唯一id	
			itemid_after	string	变化后的道具唯一id	
			hero_power_before	int	变化前武魂战力	例如：3000
			hero_power_after	int	变化后武魂战力	例如：3000
			hero_power_change	int	变化的武魂战力	例如：3000
			suit_count	int	套装效果	触发填对应套装数: 0-无套装; 1-3件套; 2-6件套
			suit_quality	int	套装品质	例如: 3-橙色; 2-蓝色;由小到大排列
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogEquitFlow(string hero_id, string hero_name, string hero_profession, int change_type, int operation_type, string itemid_before,
            string itemid_after, int hero_power_before, int hero_power_after, int hero_power_change, int suit_count, int suit_quality)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.equit_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_profession", hero_profession);
            properties.Add("change_type", change_type);
            properties.Add("operation_type", operation_type);
            properties.Add("itemid_before", itemid_before);
            properties.Add("itemid_after", itemid_after);
            properties.Add("hero_power_before", hero_power_before);
            properties.Add("hero_power_after", hero_power_after);
            properties.Add("hero_power_change", hero_power_change);
            //properties.Add("suit_count", suit_count);
            //properties.Add("suit_quality", suit_quality);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *阵容变化	battleteam_flow	阵容变化表	
        *
        *stage_type	string	阵容更换场景	例如: 秘境,狩猎魂兽等
			battleteam_before	array	变化前阵容明细	数组上传[{"位置id":5,"hero_id":10,"hero_power":1000}...],位置上为空不记录。若未操作，为空 
			battleteam_after	array	变化后阵容明细	数组上传[{"位置id":5,"hero_id":10,"hero_power":1000}...],位置上为空不记录。若未操作，为空 
			power_before	int	变化前总战力	例如：3000
			power_after	int	变化后总战力	例如：3000
			power_change	int	变化的总战力	例如：3000
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogBattleteamFlow(string stage_type, List<Dictionary<string, object>> battleteam_before,
            List<Dictionary<string, object>> battleteam_after, int power_before, int power_after, int power_change)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.battleteam_flow);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("stage_type", stage_type);
            properties.Add("battleteam_before", battleteam_before);
            properties.Add("battleteam_after", battleteam_after);
            properties.Add("power_before", power_before);
            properties.Add("power_after", power_after);
            properties.Add("power_change", power_change);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *武魂共鸣变化	hero_resonance	武魂共鸣时记录	
        *
        *stage_type	string	变化种类	1-购买共鸣位置; 2-上阵魂师; 3-卸下魂师; 
			hero_id	string	卡牌角色ID	
			hero_name	string	卡牌角色名	例如: 叶泠泠
			hero_quality	string	卡牌角色品质	0-N，1-R，2-SR，3-SSR
			hero_location	string	上阵/卸下的位置	例如: 1，2，3
			consume	array	突破消耗材料	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogHeroResonance(string stage_type, string hero_id,
            string hero_name, string hero_quality, string hero_location, List<Dictionary<string, object>> consume)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.hero_resonance);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("stage_type", stage_type);
            properties.Add("hero_id", hero_id);
            properties.Add("hero_name", hero_name);
            properties.Add("hero_quality", hero_quality);
            properties.Add("hero_location", hero_location);
            properties.Add("consume", consume);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }

        /*
        *藏宝图	treasure_map	藏宝图变化时记录	
        *
        *result	string	是否成功	
			if_reset	string	是否复活	例如: 叶泠泠
			consume	array	消耗的道具/货币	消耗的道具ID和数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			award	array	奖励	返还的数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogTreasureMap(string result, string if_reset, List<Dictionary<string, object>> consume, List<Dictionary<string, object>> award)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.treasure_map);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("result", result);
            properties.Add("if_reset", if_reset);
            properties.Add("consume", consume);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        /*
        *运营活动流水	intervention_activity	活动完成触发日志，这里的活动是流失干预活动	activity_id	int	活动ID	例如：1000004254
			activity_second_id	int	子活动ID(任务id)	例如：1000004
			mail_createtime	datetime	活动触发时间	格式 YYYY-MM-DD HH:MM:SS
			activity_name	string	活动名字	斗罗奇遇
			activity_second_name	string	子活动名字(任务名)	例如：完成N项日常任务；完成任意战斗N次；委派事件N次;
			activity_type	int	活动类型	activity_type 根据游戏情况自定义1-预流失玩家2-即将流失玩家3-流失回归玩家
			activity_status	int	活动操作状态	1-完成|2-领取奖励
			activity_days	int	任务/奖励 对应活动的第几天	任务/奖励 对应活动的第几天，如无天数区分则留空
			award	array	奖励	返还的数量，数组上传[{"itemId":5533,"count":10},{"itemId":1247,"count":100}]
			cp_param	json	事件自定义参数	没有自定义字段需求则不需要传
        */
        public void KomoeEventLogInterventionActivity(int activity_second_id, string activity_name, string activity_second_name, 
            int activity_type, int activity_status, int activity_days, List<Dictionary<string, object>> award, string dataBox)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = GetKomoeLogCommonInfo(KomoeLogEventType.intervention_activity);

            //事件属性
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("activity_second_id", activity_second_id);
            properties.Add("mail_createtime", ZoneServerApi.now.ToString(eventDateTimeString));
            properties.Add("activity_name", activity_name);
            properties.Add("activity_second_name", activity_second_name);
            properties.Add("activity_type", activity_type);
            properties.Add("activity_status", activity_status);
            properties.Add("activity_days", activity_days);
            properties.Add("data_box", dataBox);
            properties.Add("award", award);
            //properties.Add("cp_param", "");

            infDic.Add("properties", properties);
            KomoeLogManager.EventWrite(infDic);
        }
        #endregion


        #region user
        public void KomoeEventLogUserSnapshot()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            KomoeUserLogUserSnapshot();

            foreach (var item in HeroMng.GetHeroInfoList())
            {
                KomoeUserLogUserHeroSnapshot(item.Value);
            }

            foreach (var bag in bagManager.bagList)
            {
                Dictionary<ulong, BaseItem> items = bag.Value.GetAllItems();
                items.ForEach(it =>
                {
                    KomoeUserLogUserItemSnapshot(it.Value);
                });
            }

            KomoeUserLogUserTaskSnapshot(MainTaskId, "主线", MapType.NoCheckSingleDungeon.ToString());
            KomoeUserLogUserTaskSnapshot(SecretAreaManager.Id, "秘境", MapType.SecretArea.ToString());
            KomoeUserLogUserTaskSnapshot(HuntingManager.Research, "猎杀魂兽", MapType.Hunting.ToString());
            KomoeUserLogUserTaskSnapshot(TowerManager.NodeId, "异域迷岛", MapType.Tower.ToString());
            KomoeUserLogUserTaskSnapshot(ArenaMng.Rank, "竞技场", MapType.Arena.ToString());
            KomoeUserLogUserTaskSnapshot(CrossInfoMng.Info.Star, "大斗魂场", MapType.CrossBattle.ToString());
            KomoeUserLogUserTaskSnapshot(pushFigureManager.Id, "斗魂之路", MapType.PushFigure.ToString());


            KomoeUserLogUserCampSnapshot();
        }

        /*
         * 玩家快照	玩家每次登出上报，24:00上报一条当日仍在线的玩家快照	
         * 
         *  snapshot_name	快照名字	记：user_snapshot	必填	string
		    snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
		    snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
		    b_game_id	游戏id	一款游戏平台对应的游戏ID	必填	string
		    b_platform	平台名称	统一：ios|android|windows	必填	string
		    b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	string
		    b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。 请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	string
		    b_sdk_udid	用户硬件设备号	b服SDK udid，客户端SDK登录事件接口获取，32位通用唯一识别码	必填，除实时在线事件	string
		    b_udid	游戏的迭代版本，例如1.0.3	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
		    b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
		    b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
		    b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
		    role_name	角色名称	例如：黄昏蔷薇行者	必填	string
		    gender	角色性别	创角时角色的性别	选填	string
		    role_ctime	创角时间	创角的时间，role_id维度，上传后不可更改	必填	datetime
		    last_login_time	最后登录时间	最后登录时间，记录每次登录游戏时间，每次登录游戏更改	必填	datetime
		    level	当前角色等级	当前角色等级，role_id维度	必填	int
		    camp_id	所属阵营ID	所属阵营ID，role_id维度	必填	int
		    camp_name	所属阵营名	所属阵营名，role_id维度	必填	string
		    exp	当前经验值	当前经验值，role_id维度	必填	int
		    friend_cnt	好友数量	玩家当前拥有的好友的数量	必填	int
		    bfriend_cnt	金兰数量	玩家当前拥有的金兰的数量	必填	int
		    hero_cnt	武魂数量	拥有武魂数量	必填	int
		    fight	总战力	总战力，role_id维度，总战力变更时更新	必填	int
		    resonance_cnt	共鸣位	例如: 3,4	必填	int
		    last_mission_id	最后活跃任务ID	最后活跃任务mission_id，role_id维度，登出时活跃任务，每次登出时更新	必填	int
		    last_stage_id	最大主线关卡停留	如：第二章第3节，记为2-3	必填	string
		    first_order_time	首次充值时间	首次充值时间，role_id维度，不可更改	必填	datetime
		    first_order_id	首次充值订单ID	首次充值时间，role_id维度，不可更改	必填	long
		    achievement	获得称号	数组上传，[称号1id，称号2id，称号3id,….]	必填	array
		    balance_gold	钻石剩余量 	当前一级货币余额（例如钻石），role_id维度	必填	int
		    balance_silver	金币剩余量	当前二级货币余额（例如白钻），role_id维度	必填	int
		    cumulative_payment	累计付费金额	累计付费金额（人民币），role_id维度，充值时更新	必填	int
		    cumulative_order	累计付费次数	累计付费次数，role_id维度，充值时更新	必填	int
		    cumulative_days	累计活跃天数	累计活跃天数，role_id维度，登录游戏时更新	必填	int
		    cumulative_onlinetime	累计在线时长	累计活跃天数，role_id维度。登出游戏时更新	必填	int
		    user_param	自定义用户属性	自定义参数	选填	string
        */
        /// <summary>
        /// 玩家快照
        /// </summary>
        public void KomoeUserLogUserSnapshot()
        {
            // LOG 记录开关 
            //if (!GameConfig.TrackingLogSwitch)
            //{
            //    return;
            //}
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "user_snapshot");
            infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            //if (Platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", GameId);
            infDic.Add("b_platform", Platform);
            infDic.Add("b_channel_id", ChannelId);
            infDic.Add("b_zone_id", server.MainId);
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", SDKUuid);
            infDic.Add("b_sdk_uid", AccountName);
            infDic.Add("b_account_id", AccountName);
            infDic.Add("b_role_id", Uid);
            infDic.Add("role_name", Name);

            infDic.Add("gender", Sex);
            infDic.Add("role_ctime", TimeCreated.ToString(eventDateTimeString));
            infDic.Add("last_login_time", lastLoginTime.ToString(eventDateTimeString));
            infDic.Add("level", Level);
            infDic.Add("camp_id", (int)Camp);
            infDic.Add("camp_name", Camp);
            infDic.Add("exp", GetCoins(EnumerateUtility.CurrenciesType.exp));
            infDic.Add("friend_cnt", friendList.Count);
            infDic.Add("bfriend_cnt", brotherList.Count);
            infDic.Add("hero_cnt", HeroMng.GetHeroInfoList().Count);

            infDic.Add("fight", HeroMng.CalcBattlePower());
            infDic.Add("resonance_cnt", wuhunResonanceMng.GridCount);
            infDic.Add("last_mission_id", MainTaskId);
            infDic.Add("last_stage_id", MainTaskId);
            if (!string.IsNullOrEmpty(RechargeMng.FirstOrderInfo.Time))
            {
                infDic.Add("first_order_time", RechargeMng.FirstOrderInfo.Time);
            }
            infDic.Add("first_order_id", RechargeMng.FirstOrderInfo.OrderId);
            infDic.Add("achievement", TitleMng.TitleList.Keys.ToList());
            infDic.Add("balance_gold", GetCoins(EnumerateUtility.CurrenciesType.diamond));
            infDic.Add("balance_silver", GetCoins(EnumerateUtility.CurrenciesType.gold));
            infDic.Add("cumulative_payment", RechargeMng.AccumulateMoney);
            infDic.Add("cumulative_order", RechargeMng.PayCount);
            infDic.Add("cumulative_days", CumulateDays);
            infDic.Add("cumulative_onlinetime", CumulateOnlineTime + (server.Now()-lastLoginTime).TotalSeconds);

            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }


        /*
         * 武魂快照	玩家每次登出上报，23:59上报一条当日仍在线的玩家快照	
         * 
         *  snapshot_name	快照名字	记：user_hero_snapshot	必填	string
		    snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
		    snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
		    b_game_id	游戏id	一款游戏的ID	必填	int
		    b_platform	平台名称	统一：ios|android|windows	必填	string
		    b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
		    b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
		    b_sdk_udid	用户硬件设备号	b服SDK udid，客户端SDK登录事件接口获取，32位通用唯一识别码	必填，除实时在线事件	string
		    b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
		    b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
		    b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
		    b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
		    role_name	角色名称	例如：黄昏蔷薇行者	必填	string

		    hero_id	武魂ID		必填	string
		    hero_name	卡牌角色名	例如: 叶泠泠	必填	string
		    hero_quality	武魂品质	0-N，1-R，2-SR，3-SSR	必填	int
		    hero_profession	武魂战场职业定位	例如: 辅助,单攻等	必填	string
		    fight	战力	战力	必填	int
		    hero_level	武魂当前等级		必填	int
		    hero_star_quality	武魂升星品质	1-白色，2-蓝色，3-紫色，4-橙色，5-红色，6-金色	必填	int
		    hero_star_num	武魂升星数		必填	int
		    hero_skin	武魂神位id		必填	int
		    hero_modifytp	魂师天赋分配明细	数组上传[{"属性id":5533,"count":10},{"属性id":1247,"count":5}]; 属性:力量,体质,敏捷,技巧	必填	array
		    hero_energy_num	武魂充能等级	例如：充能3，3	必填	int
		    hero_skill_level	武魂技能等级	例如：技能2级，记为2	必填	int
		    hero_soullink	魂师装备魂环	数组上传[{"位置id":1,"魂环id":10010},{"位置id":2,"魂环id":10011}]; 	必填	array
		    hero_soulbone	魂师装备魂骨	数组上传[{"位置id":1,"魂骨id":10010},{"位置id":2,"魂骨id":10012}];	必填	array
		    hero_equipment	魂师装备	数组上传[{"位置id":1,"装备id":10010},{"位置id":2,"装备id":10013}];	必填	array
		    hero_jewel	魂师装备玄玉	数组上传[{"位置id":1,"玄玉id":10010},{"位置id":2,"玄玉id":10014}];	必填	array
		    hero_resonance	魂师共鸣	0-未上共鸣位; 1-上共鸣位	必填	array
		    user_param	自定义用户属性	自定义参数	选填	string
        */
        /// <summary>
        /// 武魂快照
        /// </summary>
        public void KomoeUserLogUserHeroSnapshot(HeroInfo hero)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "user_hero_snapshot");
            infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            //if (Platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", GameId);
            infDic.Add("b_platform", Platform);
            infDic.Add("b_channel_id", ChannelId);
            infDic.Add("b_zone_id", server.MainId);
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", SDKUuid);
            infDic.Add("b_sdk_uid", AccountName);
            infDic.Add("b_account_id", AccountName);
            infDic.Add("b_role_id", Uid);
            infDic.Add("role_name", Name);

            infDic.Add("hero_id", hero.Id);
            //infDic.Add("hero_name", hero_name);
            //infDic.Add("hero_quality", hero_quality);
            //infDic.Add("hero_profession", hero_profession);
            infDic.Add("fight", hero.GetBattlePower());
            infDic.Add("hero_level", hero.Level);
            //infDic.Add("hero_star_quality", hero.AwakenLevel);
            infDic.Add("hero_star_num", hero.StepsLevel);
            infDic.Add("hero_skin", hero.GodType);
            infDic.Add("hero_modifytp", hero.TalentMng);

            //infDic.Add("hero_energy_num", hero.SoulSkillLevel);
            infDic.Add("hero_skill_level", hero.SoulSkillLevel);

            Dictionary<int, SoulRingItem> soulRings = SoulRingManager.GetAllEquipedSoulRings(hero.Id);
            if (soulRings != null && soulRings.Count > 0)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> dic;
                foreach (var item in soulRings)
                {
                    dic = new Dictionary<string, object>();
                    dic.Add("位置id", item.Value.Position);
                    dic.Add("魂环id", item.Value.Id);
                    list.Add(dic);
                }
                infDic.Add("hero_soullink", list);
            }

            List<SoulBone> soulBoneList = SoulboneMng.GetEnhancedHeroBones(hero.Id);
            if (soulBoneList != null && soulBoneList.Count > 0)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                Dictionary<string, object> dic;
                foreach (var item in soulBoneList)
                {
                    dic = new Dictionary<string, object>();
                    dic.Add("位置id", item.PartType);
                    dic.Add("魂骨id", item.TypeId);
                    list.Add(dic);
                }
                infDic.Add("hero_soulbone", list);
            }


            Dictionary<int, Slot> slots = EquipmentManager.GetHeroPartSlot(hero.Id);
            if (slots != null)
            {
                List<Dictionary<string, object>> equipList = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> jewelList = new List<Dictionary<string, object>>();
                Dictionary<string, object> dic;
                foreach (var slot in slots)
                {
                    BaseBag bag = bagManager.GetBag(MainType.Equip);
                    if (bag != null)
                    {
                        EquipmentItem item = bag.GetItem(slot.Value.EquipmentUid) as EquipmentItem;
                        if (item != null)
                        {
                            dic = new Dictionary<string, object>();
                            dic.Add("位置id", slot.Key);
                            dic.Add("装备id", item.Id);
                            equipList.Add(dic);
                        }
                    }


                    if (slot.Value.JewelUid > 0)
                    {
                        BaseItem baseItem = BagManager.GetItem(slot.Value.JewelUid);
                        if (baseItem != null)
                        {
                            dic = new Dictionary<string, object>();
                            dic.Add("位置id", slot.Key);
                            dic.Add("玄玉id", baseItem.Id);
                            jewelList.Add(dic);
                        }
                    }
                }

                infDic.Add("hero_equipment", equipList);
                infDic.Add("hero_jewel", jewelList);
            }

            infDic.Add("hero_resonance", hero.ResonanceIndex);

            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }

        /*
         * 道具快照	 玩家每次登出上报，23:59上报一条当日仍在线的玩家快照	
         * 
         *  snapshot_name	快照名字	记：user_item_snapshot	必填	string
		    snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
		    snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
		    b_game_id	游戏id	一款游戏的ID	必填	int
		    b_platform	平台名称	统一：ios|android|windows	必填	string
		    b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
		    b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
		    b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
		    b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
		    b_sdk_udid	用户硬件设备号	b服sdk udid，客户端sdk登陆事件接口获取，32位通用唯一识别码	必填	string
		    b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
		    b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
		    role_name	角色名称	例如：黄昏蔷薇行者	必填	string

		    item_id	道具的ID		必填	int
		    item_unique_id	道具的唯一ID		必填	int
		    item_name	道具的名称		必填	string
		    item_type	道具类型	例如：货币，装备等	必填	int
		    item_cnt	玩家持有该物品的数量		必填	int
		    user_param	自定义用户属性	自定义参数	选填	string
        */
        /// <summary>
        /// 道具快照
        /// </summary>
        public void KomoeUserLogUserItemSnapshot(BaseItem item)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "user_item_snapshot");
            infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            //if (Platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", GameId);
            infDic.Add("b_platform", Platform);
            infDic.Add("b_channel_id", ChannelId);
            infDic.Add("b_zone_id", server.MainId);
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", SDKUuid);
            infDic.Add("b_sdk_uid", AccountName);
            infDic.Add("b_account_id", AccountName);
            infDic.Add("b_role_id", Uid);
            infDic.Add("role_name", Name);

            infDic.Add("item_id", item.Id);
            infDic.Add("item_unique_id", item.Uid);
            //infDic.Add("item_name", item_name);
            infDic.Add("item_type", item.MainType);
            infDic.Add("item_cnt", item.PileNum);

            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }

        /*
         * 副本进度快照	 玩家每次登出上报，23:59上报一条当日仍在线的玩家快照	
         * 
         *  snapshot_name	快照名字	记：user_task_snapshot	必填	string
		    snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
		    snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
		    b_game_id	游戏id	一款游戏的ID	必填	int
		    b_platform	平台名称	统一：ios|android|windows	必填	string
		    b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
		    b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
		    b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
		    b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
		    b_sdk_udid	用户硬件设备号	b服sdk udid，客户端sdk登陆事件接口获取，32位通用唯一识别码	必填	string
		    b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
		    b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
		    role_name	角色名称	例如：黄昏蔷薇行者	必填	string
		    copy_id	副本ID		必填	int
		    copy_name	副本名称	主线;    支线;    秘境;    猎杀魂兽;     兽袭;    高原之乡;     冰火两仪;     异域迷岛;    竞技场;    大斗魂场~深渊战场；    斗魂之路;	必填	string
		    copy_type	副本类型	例如：主线,支线，PVE，PVP,爬塔	必填	int
		    copy_progress	副本进度	停留的最后一个关卡或排名	必填	string
		    user_param	自定义用户属性	自定义参数	选填	string
        */
        /// <summary>
        /// 副本进度快照
        /// </summary>
        public void KomoeUserLogUserTaskSnapshot(int id, string name, string type)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "user_task_snapshot");
            infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            //if (Platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", GameId);
            infDic.Add("b_platform", Platform);
            infDic.Add("b_channel_id", ChannelId);
            infDic.Add("b_zone_id", server.MainId);
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", SDKUuid);
            infDic.Add("b_sdk_uid", AccountName);
            infDic.Add("b_account_id", AccountName);
            infDic.Add("b_role_id", Uid);
            infDic.Add("role_name", Name);

            infDic.Add("copy_id", id);
            infDic.Add("copy_name", name);
            infDic.Add("copy_type", type);
            infDic.Add("copy_progress", id);
            //infDic.Add("user_param", item.PileNum);

            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }


        //  /*
        //   * 功能快照	玩家每次登出上报，23:59上报一条当日仍在线的玩家快照	
        //   * 
        //   *  snapshot_name	快照名字	记：user_function_open_snapshot	必填	string
        //snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
        //snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
        //b_game_id	游戏id	一款游戏的ID	必填	int
        //b_platform	平台名称	统一：ios|android|windows	必填	string
        //b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
        //b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
        //b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
        //b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
        //b_sdk_udid	用户硬件设备号	b服sdk udid，客户端sdk登陆事件接口获取，32位通用唯一识别码	必填	string
        //b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
        //b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
        //role_name	角色名称	例如：黄昏蔷薇行者	必填	string
        //function_list	开启功能列表	功能开放配置表的功能id，记录格式[111,222,333]	必填	array
        //user_param	自定义用户属性	自定义参数	必填	string
        //  */
        //  /// <summary>
        //  /// 登出
        //  /// </summary>
        //  public void KomoeUserLogUserFunctionOpenSnapshot()
        //  {
        //      // LOG 记录开关 
        //      if (!GameConfig.TrackingLogSwitch)
        //      {
        //          return;
        //      }
        //      //公告字段
        //      Dictionary<string, object> infDic = new Dictionary<string, object>();

        //      infDic.Add("snapshot_name", "user_function_open_snapshot");
        //      infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
        //      infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
        //      infDic.Add("b_game_id", KomoeLogConfig.GameId);
        //      infDic.Add("b_platform", KomoeLogConfig.Platform);
        //      infDic.Add("b_channel_id", ChannelId);
        //      infDic.Add("b_zone_id", server.MainId);
        //      infDic.Add("b_udid", DeviceId);
        //      infDic.Add("b_sdk_udid", SDKUuid);
        //      infDic.Add("b_sdk_uid", AccountName);
        //      infDic.Add("b_account_id", AccountName);
        //      infDic.Add("b_role_id", Uid);
        //      infDic.Add("role_name", Name);

        //      //infDic.Add("function_list", item.Id);
        //      //properties.Add("user_param", "");
        //      KomoeLogManager.UserWrite(infDic);
        //  }

        /*
         * 阵营玩法快照	玩家每次登出上报，23:59上报一条当日仍在线的玩家快照	
         * 
         *  snapshot_name	快照名字	记：user_camp_snapshot	必填	string
		    snapshot_date	快照日期	记录快照记录时的日期，格式：yyyymmmdd，如20201228	必填	string
		    snapshot_time	快照时间	记录快照记录时的时间戳	必填	int
		    b_game_id	游戏id	一款游戏的ID	必填	int
		    b_platform	平台名称	统一：ios|android|windows	必填	string
		    b_channel_id	游戏的渠道ID	游戏的渠道ID	必填	int
		    b_zone_id	游戏自定义的区服id	针对分区分服的游戏填写分区id，用于区分区服。    请务必将cb与ob期间的区服id进行区分，不然cb测试数据将会被继承至ob阶段	必填	int
		    b_udid	用户硬件设备号	Android和iOS都用的uuid，32位通用唯一识别码	必填	string
		    b_sdk_uid	B站生成的uid	用户ID，一般为一款产品自增序列号，例如：156475929395	必填	string
		    b_sdk_udid	用户硬件设备号	b服sdk udid，客户端sdk登陆事件接口获取，32位通用唯一识别码	必填	string
		    b_account_id	用户游戏内账号id	注册账号通过算法加密生成的账户ID，例如：1000004254	必填	string
		    b_role_id	用户角色id	同一账户下的多角色识别id，没有该参数则时传相同的account_id	必填	string
		    role_name	角色名称	例如：黄昏蔷薇行者	必填	string
		    function_list	开启功能列表	功能开放配置表的功能id，记录格式[111,222,333]	必填	array
		    camp_id	所属阵营ID	所属阵营ID，role_id维度	必填	int
		    camp_name	所属阵营名	所属阵营名，role_id维度	必填	string
		    Individual_points	个人积分	例如: 1000	必填	string
		    Individual_base_num	个人占据据点数	例如: 3	必填	string
		    camp_points	阵营总积分	例如: 1000	必填	string
		    camp__base_num	阵营占据据点数	例如: 3	必填	string
		    camp_provisions	阵营总粮草	例如: 10000	必填	string
		    user_param	自定义用户属性	自定义参数	必填	string
        */
        /// <summary>
        /// 阵营玩法快照
        /// </summary>
        public void KomoeUserLogUserCampSnapshot()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //公告字段
            Dictionary<string, object> infDic = new Dictionary<string, object>();

            infDic.Add("snapshot_name", "user_camp_snapshot");
            infDic.Add("snapshot_date", server.Now().ToString(userDateTimeString));
            infDic.Add("snapshot_time", Timestamp.GetUnixTimeStampSeconds(server.Now()));
            //if (Platform == "ios")
            //{
            //    infDic.Add("b_game_id", 6361);
            //}
            //else
            //{
            //    infDic.Add("b_game_id", 6360);
            //}
            infDic.Add("b_game_id", GameId);
            infDic.Add("b_platform", Platform);
            infDic.Add("b_channel_id", ChannelId);
            infDic.Add("b_zone_id", server.MainId);
            infDic.Add("b_udid", DeviceId);
            infDic.Add("b_sdk_udid", SDKUuid);
            infDic.Add("b_sdk_uid", AccountName);
            infDic.Add("b_account_id", AccountName);
            infDic.Add("b_role_id", Uid);
            infDic.Add("role_name", Name);

            //infDic.Add("function_list", item.Id);
            infDic.Add("camp_id", (int)Camp);
            infDic.Add("camp_name", Camp.ToString());
            infDic.Add("Individual_points", CampBattleMng.CampScore);
            //infDic.Add("Individual_base_num", item.Id);
            //infDic.Add("camp_points", item.Id);
            //infDic.Add("camp_base_num", item.Id);
            if (server.RelationServer.campCoins.ContainsKey(Camp))
            {
                infDic.Add("camp_provisions", server.RelationServer.campCoins[Camp]);
            }
            //properties.Add("user_param", "");
            KomoeLogManager.UserWrite(infDic);
        }


        #endregion

        public List<Dictionary<string, object>> ParseConsumeInfoToList(Dictionary<CurrenciesType, int> costCoins, int costItemId = 0, int costItemCount = 0)
        {
            List<Dictionary<string, object>> consume = new List<Dictionary<string, object>>();
            if (costCoins != null)
            {
                Dictionary<string, object> consumeDic;
                foreach (var item in costCoins)
                {
                    consumeDic = new Dictionary<string, object>();
                    consumeDic.Add("itemId", (int)item.Key);
                    consumeDic.Add("count", item.Value);
                    consume.Add(consumeDic);
                }
            }
            if (costItemId > 0)
            {
                Dictionary<string, object> consumeDic;
                consumeDic = new Dictionary<string, object>();
                consumeDic.Add("itemId", costItemId);
                consumeDic.Add("count", costItemCount);
                consume.Add(consumeDic);
            }
            return consume;
        }

        public List<Dictionary<string, object>> ParseRewardInfoToList(Dictionary<RewardType, Dictionary<int, int>> rewardList)
        {
            List<Dictionary<string, object>> award = new List<Dictionary<string, object>>();
            Dictionary<string, object> awardDic = null;
            foreach (var kv in rewardList.Values)
            {
                foreach (var item in kv)
                {
                    awardDic = new Dictionary<string, object>();
                    awardDic.Add("itemId", item.Key.ToString());
                    awardDic.Add("count", item.Value);
                    award.Add(awardDic);
                }
            }       
            return award;
        }     

        public List<Dictionary<string, object>> Parse4NatureList(int strength, int physical, int agility, int outburst)
        {
            List<Dictionary<string, object>> natrueList = new List<Dictionary<string, object>>();
            Dictionary<string, object> natureDic;

            natureDic = new Dictionary<string, object>();
            natureDic.Add("id", (int)NatureType.PRO_POW);
            natureDic.Add("count", strength);
            natrueList.Add(natureDic);

            natureDic = new Dictionary<string, object>();
            natureDic.Add("id", (int)NatureType.PRO_CON);
            natureDic.Add("count", physical);
            natrueList.Add(natureDic);

            natureDic = new Dictionary<string, object>();
            natureDic.Add("id", (int)NatureType.PRO_AGI);
            natureDic.Add("count", agility);
            natrueList.Add(natureDic);

            natureDic = new Dictionary<string, object>();
            natureDic.Add("id", (int)NatureType.PRO_EXP);
            natureDic.Add("count", outburst);
            natrueList.Add(natureDic);

            return natrueList;
        }

        public List<Dictionary<string, object>> ParseHeroPosList(List<int> heroPos)
        {
            List<Dictionary<string, object>> heroPosList = new List<Dictionary<string, object>>();
            Dictionary<string, object> heroPosDic;
            for (int i = 1; i <= heroPos.Count; i++)
            {
                heroPosDic = new Dictionary<string, object>();
                heroPosDic.Add("位置id", i);
                heroPosDic.Add("角色卡itemid", heroPos[i-1]);
                heroPosList.Add(heroPosDic);
            }          
            return heroPosList;
        }

        public List<Dictionary<string, object>> ParseMainHeroPosPowerList(SortedDictionary<int, int> heroPos, out long totalPower)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> dic;
            totalPower = 0;
            foreach (var hero in heroPos)
            {           
                dic = new Dictionary<string, object>();
                dic.Add("位置id", hero.Value);
                dic.Add("hero_id", hero.Key);
                HeroInfo info = HeroMng.GetHeroInfo(hero.Key);
                if (info != null)
                {
                    int battlePower = info.GetBattlePower();
                    dic.Add("hero_power", battlePower);
                    totalPower += battlePower;
                }
                list.Add(dic);
            }
            return list;
        }

        public List<Dictionary<string, object>> ParseTowerHeroPosPowerList(Dictionary<int, int> heroPos, out long totalPower)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> dic;
            totalPower = 0;
            foreach (var hero in heroPos)
            {
                dic = new Dictionary<string, object>();
                dic.Add("位置id", hero.Value);
                dic.Add("hero_id", hero.Key);
                HeroInfo info = HeroMng.GetHeroInfo(hero.Key);
                if (info != null)
                {
                    int battlePower = info.GetBattlePower();
                    dic.Add("hero_power", battlePower);
                    totalPower += battlePower;
                }
                list.Add(dic);
            }
            return list;
        }

        public List<Dictionary<string, object>> ParseMultiQueueHeroPosPowerList(MapType mapType, Dictionary<int, Dictionary<int, HeroInfo>> heroPos, out long totalPower)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> dic;
            totalPower = 0;
            Dictionary<int, HeroInfo> queueHeroPos = null;
            foreach (var kv in heroPos)
            {
                queueHeroPos = kv.Value;
                foreach (var hero in queueHeroPos)
                {
                    dic = new Dictionary<string, object>();
                    switch (mapType)
                    {                     
                        case MapType.CrossBoss:
                        case MapType.CrossBossSite:
                            dic.Add("位置id", hero.Value.CrossBossPositionNum);
                            break;
                        case MapType.ThemeBoss:
                            dic.Add("位置id", hero.Value.ThemeBossPositionNum);
                            break;
                        case MapType.CrossBattle:
                            dic.Add("位置id", hero.Value.CrossPositionNum);
                            break;
                        default:
                            break;
                    }                   
                    dic.Add("hero_id", hero.Value.Id);

                    long battlePower = hero.Value.GetBattlePower();
                    dic.Add("hero_power", battlePower);
                    totalPower += battlePower;
                    list.Add(dic);
                }
            }        
            return list;
        }

        public void KomoeLogRecordPveFight(int dungeonType, int operateType, string dungeonId, Dictionary<RewardType, Dictionary<int, int>> rewards, int result, int finishTime = 0, Dictionary<CurrenciesType, int> costCoin = null, int first= 0, int hasBuff = 0)
        {
            long totalPower;
            List<Dictionary<string, object>> heroPosPower = null;
            switch (dungeonType)
            {
                case 5:
                    heroPosPower = ParseTowerHeroPosPowerList(TowerManager.HeroPos, out totalPower);
                    break;
                case 6:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.CrossBoss, HeroMng.CrossBossQueue, out totalPower);
                    break;
                case 9:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.ThemeBoss, HeroMng.ThemeBossQueue, out totalPower);
                    break;
                default:
                    heroPosPower = ParseMainHeroPosPowerList(HeroMng.GetHeroPos(), out totalPower);
                    break;
            }
            List<Dictionary<string, object>> award = null;
            if (rewards != null)
            {
                award = ParseRewardInfoToList(rewards);
            }
            List<Dictionary<string, object>> consume = null;
            if (costCoin != null)
            {
                consume = ParseConsumeInfoToList(costCoin);
            }       
            int restCount = 0;
            switch (dungeonType)
            {
                case 0:
                    restCount = GetCounterRestCount(CounterType.SecretAreaSweepCount, CounterType.SecretAreaSweepCountBuy);
                    break;
                case 1:
                    restCount = GetCounterRestCount(CounterType.IntegralBoss, CounterType.IntegralBossBuy);
                    break;
                case 2:
                    restCount = GetCounterRestCount(CounterType.HuntingCount, CounterType.HuntingBuy);
                    break;
                case 3:
                    restCount = GetCounterRestCount(CounterType.BenefitsSoulBreath, CounterType.BenefitsSoulBreathBuy);
                    break;
                case 4:
                    restCount = GetCounterRestCount(CounterType.BenefitsSoulPower, CounterType.BenefitsSoulPowerBuy);
                    break;
                case 6:
                    restCount = GetCounterRestCount(CounterType.CrossBossActionCount, CounterType.CrossBossActionBuyCount);
                    break;
                case 7:
                    restCount = GetCounterRestCount(CounterType.OnhookCount, CounterType.OnhookBuyCount);
                    break;
                case 9:
                    restCount = GetCounterRestCount(CounterType.ThemeBossCount, CounterType.ThemeBossBuyCount);
                    break;
                default:
                    break;
            }
            KomoeEventLogPveFight(dungeonType, dungeonId, "", operateType, heroPosPower, restCount, HeroMng.CalcBattlePower(), finishTime, hasBuff, result, first, GetTeamDetail(), consume, award);
        }

        public void KomoeLogRecordPvpFight(int dungeonType, int operateType, Dictionary<RewardType, Dictionary<int, int>> rewards, int result, int beforeRank, int afterRank, string beforeHonor, string afterHonor, int enemyId, long enemyPower, int costTime = 0)
        {
            long totalPower;
            List<Dictionary<string, object>> heroPosPower = null;
            switch (dungeonType)
            {
                case 2:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.CrossBattle, HeroMng.CrossQueue, out totalPower);
                    break;
                default:
                    heroPosPower = ParseMainHeroPosPowerList(HeroMng.GetHeroPos(), out totalPower);
                    break;
            }
            List<Dictionary<string, object>> award = null;
            if (rewards != null)
            {
                award = ParseRewardInfoToList(rewards);
            }          
            int restCount = 0;
            switch (dungeonType)
            {
                case 1:
                    restCount = GetCounterRestCount(CounterType.ChallengeCount, CounterType.ChallengeCountBuy);
                    break;
                case 2:
                    restCount = GetCounterRestCount(CounterType.CrossBattleCount, CounterType.CrossBattleBuyCount);
                    break;
                default:
                    break;
            }
            KomoeEventLogPvpFight(dungeonType, "", "", operateType, heroPosPower, restCount, HeroMng.CalcBattlePower(), costTime, result, beforeRank, afterRank, beforeHonor.ToString(), afterHonor.ToString(), enemyId, enemyPower, award);
        }

        public int GetActivityStartDays(RechargeGiftModel activityModel, DateTime now)
        {
            DateTime startTime = activityModel.StartTime >= activityModel.StartWeekTime ? activityModel.StartTime : activityModel.StartWeekTime;
            int days = (now - startTime).Days + 1;
            return days;
        }
    }
}

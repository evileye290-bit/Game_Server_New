//using DataProperty;

namespace ServerShared
{
    // 常量定义
    public struct CONST
    {
        public static readonly int CLIENT_ENTER_EXPIRED_TIME = 90; // zone等待client连接时间 单位 秒
        public static readonly int CLIENT_ENTER_MAP_EXPIRED_TIME = 60; // manager 等待client进入map时间 单位 秒
        public static readonly int BARRACK_RESTART_TIME = 10; // barrack 断开后重启间隔 单位 秒
        public static readonly int BARRACK_RESTART_COUNT = 3; // barrack断开连接尝试重启次数
        public static readonly int RELATION_RESTART_TIME = 30; // relation 断开后重启间隔 单位 秒
        public static readonly int RELATION_RESTART_COUNT = 3; // relation 断开连接尝试重启次数
        public static readonly int ZONE_CONNECT_MANAGER_TIME = 1; //zone与manager断开连接后，定期主动连接manager 单位 秒
        public static readonly int ZONE_CONNECT_RELATION_TIME = 2; // zone与manager断开连接后，定期主动连接relation 单位 秒
        public static readonly int ZONE_RESTART_TIME = 30; // zone断开后重启间隔
        public static readonly int ZONE_RESTART_COUNT = 10; // zone断开后尝试重启次数

        public static readonly int ACCEPT_CPU_SLEEP_TIME = 500; // 每秒cpu睡眠时间大于该值 则可在当前zone创建副本 否则需要向manager请求其他zone来创建副本 单位 ms
        public static readonly int HEART_FASHION_COIN = 10; // 收心加FashionCoin
        public static readonly int JJC_ROBOT_BEGIN_UID = 9900000; // 大于该id 说明是竞技场机器人
        public static readonly int MARRIAGE_ROBOT_BEGIN_UID = 9900000; // 大于该id 说明是结婚机器人
        public const int SEARCH_LIMIT_COUNT = 5;
        public const int AttackDamageType = 0;
        public const int SkillDamageType = 1;
        public const int BuffDamageType = 2;
        public const int SYNC_CURRIENCIES_TIME = 600; // exp gold等同步db时间 单位 秒
        public const int SYNC_COUNTER_TIME = 600; // exp gold等同步db时间 单位 秒
        public const int DB_POOL_COUNT = 1; // DB连接池内连接个数
        public const int DB_LOGIN_POOL_COUNT = 4; // LoginServer DB连接池内连接个数
        public const int DB_BARRACK_POOL_COUNT = 8; // Barrack DB 连接池连接个数
        public const int UNLIMITED_REVIVE_COUNT = 9999;
        public const bool OPEN_ROBOT_PVP = false;
        public const int ROBOT_PVP_TIME = 30000; // 30s
        public const int FAMILY_COUNT_PER_PAGE = 20; // 家族列表每页个数
        public const int HERO_COMMENT_PER_PAGE = 10; // 英雄评论每页个数
        public const int FAMILY_MEMBER_COUNT_PER_PAGE = 20; // 家族成员列表每页个数
        public const int FAMILIY_APPLICATION_COUNT_PER_PAGE = 20; //一键加入家族个数
        public const int CAMP_HONOR_BATTLE_RANK_PER_PAGE = 20; // 阵营荣誉战排名每页个数
        public const int JOIN_FAMILIES_COUNT = 20; //一键加入家族个数
        public const string DEFAULT_SILENCE_REASON = "不文明发言";
        public const string SPEAK_SENSITIVE_WORD = "发布违规信息";
        public static bool ALARM_OPEN = false; // 报警开关
        public const int DB_EXCEPTION_PERIOD = 60; // DB异常报错容错时长
        public const int DB_EXCEPTION_COUNT = 40; // DB容错时间内容错上限　
        public const int PROTOBUF_EXCEPTION_COUNT  = 10; // protobuf解析异常上限 防止封包攻击
        public const bool BLACK_IP_CHECK = true; // 黑名单是否开启
        public const int SERVER_PER_PAGE = 4; // 每页显示server个数
        public const int FAKE_ANNOUNCEMENT_ONLINE_COUNT_1 = 1000;
        public const int FAKE_ANNOUNCEMENT_ONLINE_COUNT_2 = 500;
        public const int FAKE_ANNOUNCEMENT_ONLINE_COUNT_3 = 200;
        public const int FAKE_ANNOUNCEMENT_ONLINE_COUNT_4 = 100;
        public const int CAMP_HONOR_BATTLE_EAMIL_ID = 11;
        public static int ONLINE_COUNT_WAIT_COUNT = 1000000;
        public static int ONLINE_COUNT_FULL_COUNT = 1100000;
        public static int LOGIN_QUEUE_PERIOD = 3000; // WAIT状态下 单gate 3s放1人
        public const int BIND_LEDO_wNum_MAIL_ID = 6051;
        public const int NOTIFY_WAITING_TIME_PERIOD = 20; // 满员排队状态下 每20s通知一次排队信息
        public const string HKSDKKEY = "OOJ*#*FHJF$%$&*(_#$(^9828*&#U";
        public const string VIRTUAL_RECHARGE_PREFIX = "test";
        public const int DB_DELAY_RECONNECT_COUNT = 1;
        public const int GIFT_CODE_LENGTH = 12; // 礼包兑换码长度
        public const int VEDIO_PACKET_LEN = 2048; // 录像文件每包内容长度
        public const int HeroQueueMinIndex = 0;
        public const int HeroQueueMaxIndex = 4;
        public const int HeroBackBeginPosition = 6; // 0-5 正式阵容 6-11 替补阵容
        public const int SkillQueueMinIndex = 0;
        public const int SkillQueueMaxIndex = 4;
        public const int MaxCommentUidCount = 200;

        public const bool DELAY_MANA = true;
        public const int BACK_UP_HERO_COUNT = 6;
        public const long MAX_PHOTO_COUNT = 5;
        public const float MAX_VEHICLE_DELTA = 0.2f;

        public const int RECENT_ONLINE_MAX_COUNT = 30; //最近登录PlayerOnlineSort限制最大人数

        public const string DATETIME_TO_STRING = "yyyy-MM-dd HH:mm:ss";
        public const string DATETIME_TO_STRING_1 = "yyyy-MM-dd HH:mm";
        public const string DATETIME_TO_STRING_2 = "yyyy-MM-dd";

        public const string SEARCH_ID_PREFIX = "#";

        public const long RechargeOrderTempNum = 10000000000;
        /// <summary>
        /// 聊天信息每包条数
        /// </summary>
        public const int CHATINFO_PER_PKG_COUNT = 50;

        /// <summary>
        /// 临时地图ID号
        /// </summary>
        public const int TEMP_BATTLE_MAP_ID = 11;

        /// <summary>
        /// 主城地图ID
        /// </summary>
        public const int MAIN_MAP_ID = 1;
        /// <summary>
        /// 主城地图分线
        /// </summary>
        public const int MAIN_MAP_CHANNEL = 1;

        /// <summary>
        /// 附近的人聊天室配置ID
        /// </summary>
        public const int CHAT_NEARBY_CONFIG_ID = 1;
        /// <summary>
        /// 世界频道配置ID
        /// </summary>
        public const int CHAT_WORLD_CONFIG_ID = 2;
        /// <summary>
        /// 创建角色剧情步骤
        /// </summary>
        public const int MAX_SHIP_STEP = 3;

        /// <summary>
        /// 邮件发送最大数量
        /// </summary>
        public const int EMAIL_MAX_COUNT = 10;

        /// <summary>
        /// 聊天广播消息最大数量
        /// </summary>
        public const int CHAT_BROADCAST_MAX_COUNT = 5;

        /// <summary>
        /// 主角装备位置
        /// </summary>
        public const int MAIN_HERO_EQUIP_INDEX = 1;
        /// <summary>
        /// 第1个伙伴位置
        /// </summary>
        public const int HERO_EQUIP_INDEX_1 = 2;
        /// <summary>
        /// 第2个伙伴位置
        /// </summary>
        public const int HERO_EQUIP_INDEX_2 = 3;
        /// <summary>
        /// 第3个伙伴位置
        /// </summary>
        public const int HERO_EQUIP_INDEX_3 = 4;

        /// <summary>
        /// 背包物品更新发送最大数量
        /// </summary>
        public const int ITEM_UPDATE_MAX_COUNT = 50;

        /// <summary>
        /// 宠物（蛋）发送最大数量
        /// </summary>
        public const int PET_MSG_MAX_COUNT = 30;
    }
    public enum SysCtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
    public static class CampBattle
    {
        public static int None = 0;
        public static int Freedom = 1;
        public static int Royal = 2;
    }

    public enum ServerState
    {
        DisConnect = 0,
        Connect = 1,
        Starting,
        Started,
        Stopping,
        Stopped
    }


    // 某服的状态
    public enum MainServerState
    {
        Stopped = 0,
        AllStarted = 1, // 该服下所有的进程启动完毕
        ParticialStarted = 2 // 该服部分进程启动完毕，但有进程结束
    }

    public enum TransformStep
    { 
        INVALID = -1,
        DONE = 0,
        CHARACTER_INFO = 1,  // 第一步 传基础信息
        ITEMS = 2,           //物品
        ACTIVITY =3,       //活动
        COUNTER =4,          //计数器
        HERO =5,             //伙伴
        SHOP =6,             //商店
        REDISDATA =7,        //加载Redis数据，如生涯，黑名单等
        EMAIL =8,        //邮件相关
        RECHARGE =9,     //充值
        DungeonInfo = 10,//猎杀魂兽、整点Boss
        Draw = 11,//抽奖
        GodPath = 12,//成神之路
        WuhunResonance = 13, //武魂共鸣
        HeroGod = 14, //成神
        Gift = 15,//礼包
        Action = 16,//玩家行为记录
        ShovelTreasure = 17,//挖宝
        Theme = 18,//主题
        CultivateGift = 19,//养成礼包
        PettyGift = 20,//小额礼包
        DaysRewardHero = 21,//累积天数奖励角色
        Garden = 22,//幽香草园
        DivineLove = 23,//乾坤问情
        FlipCard = 24,//翻卡活动
        IslandHigh = 25,//海岛登高
        IslandHighGift = 26,//海岛登礼包
        Trident = 27,//三叉戟
        DragonBoat = 28,//端午活动
        StoneWall = 29,//昊天石壁
        IslandChallenge = 30,//海岛挑战
        Carnival = 31,//嘉年华
        HeroTravel = 32,//嘉年华
        ShrekInvitation = 33,//史莱克邀约
        Roulette = 34,//轮盘
        Canoe = 35,//皮划艇
        MidAutumn = 36,//中秋
        ThemeFirework = 37,//主题烟花
        NineTest = 38,//九考试炼
        Warehouse = 39,//仓库
        School = 40,//学院
        SchoolTask = 41,//学院任务
        AnswerQuestion = 42,//答题
        DiamondRebate = 43,//钻石返利
        Pet = 44,//宠物
        PetEgg = 45,//宠物蛋
        XuanBox = 46,//玄天宝箱
        WishLantern = 47,//九笼祈愿
        PetDungeonQueue = 48,//宠物副本阵容
        DaysRecharge = 49,//n日充值
        TreasureFlipCard = 50, //夺宝翻翻乐
        Shrekland = 51,//史莱克乐园
        SpaceTimeTower = 52,//时空塔
        DevilTraining = 53, //魔鬼训练
        DomainBenedition = 54,//神域赐福
        DriftExplore = 56,//漂流探宝
    }

    public enum MapState
    { 
        CLOSE = 0,
        OPEN = 1
    }


    public enum FamilyTitle
    { 
        Nobody = 9999,
        Chief = 1,          // 族长
        ViceChief = 2,      // 副族长
        Elite = 3,          // 精英
        Member = 4,     // 普通成员
    }

    public enum LogType
    { 
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        DEBUF = 4
    }

    public enum GMProtocolID
    {
        // begin with 10000 
        CharacterList = 10001, // 角色列表
        CharacterInfo = 10002, // 角色详细信息
        AccountId = 10003, // 查询账号id

        Freeze = 10004, // 冻结
        UnFreeze = 10005, // 解冻

        UnVoice = 10008, // 禁言
        Voice = 10009, // 解除禁言
        BadWorld = 10010, // 敏感词

        City = 10012, // 回主城
        Bag = 10013, // 背包物品查询
        DeleteBatItem = 10014, // 删除背包物品
        Announcement = 10015, // 公告

        OrderState = 10016, // 订单状态查询
        RepairOrder = 10017, // 补单
        SendItem = 10018,// 物品发放
        VirtualRecharge = 10019, // 虚拟充值
        SendMail = 10020, // 发送邮件

        BadWords = 10021, // 非法字符串 
        ArenaInfo = 10022, // 竞技场信息
        FamilyInfo = 10023, // 家族信息
        ServerInfo = 10024, // 服务器信息
        GiftCode = 10025, // 激活码
        GameCounter = 10026, // 游戏相关计数器
        ChangeFamilyName = 10027, // 更改家族名称
        RecommendServer = 10028, // 更改推荐服务器
        ItemTypeList = 10029, // 某一类型物品列表
        PetTypeList = 10030, // 某一类型宠物
        PetMountList = 10031, // 坐骑列表
        DeletePet = 10032, // 删除宠物或宠物碎片
        DeletePetMount = 10033, // 删除坐骑
        EquipList = 10034, // 身上装备列表
        PetList = 10035, // 出战宠物列表
        PetMountStrength = 10036, // 坐骑强化信息
        DeleteItem = 10037, // 删除物品
        DeleteChar = 10038, // 删除角色
        ServerList = 10039, // 服务器列表
        RecentLoginServers = 10040, // 最近登录服务器
        OrderList = 10041, // 订单列表
        SpecItem = 10042, // 指定物品uid查询

        SpecPet = 10043, // 指定宠物uid查询

        UpdateItemCount = 10044, // 改变物品个数
        SpecEmail = 10045, // 新加模板邮件
        UpdateCharData = 10046, // 更改角色数据
       
        // ProjectX

        HeroList = 11000, // 获取角色英雄列表
        AddHero = 11001, // 增加某种类型英雄
        RemoveHero = 11002, // 删除某种类型英雄
        UpHero = 11003, // 某个英雄上阵
        DownHero = 11004, // 某个英雄下阵
        SetHeroLevel = 11005, // 设置英雄等级
        SetHeroCount = 11006, // 设置英雄个数
        SetPlayerLevel = 11007, // 设置主角等级
        SetPlayerExp = 11008, // 设置主角鹰眼
        SkillList = 11009, // 获取技能列表
        AddSkill = 11010, // 增加某种类型技能
        RemoveSkill = 11011, // 删除某种类型技能
        UpSkill = 11012, // 某个技能上阵
        DownSkill = 11013, // 某个技能下阵
        SetSkillLevel = 11014, // 设置技能等级
        SetSkillCount = 11015, // 设置技能数量
        HeroComment = 11016, // 英雄评论列表
        UpdateHeroCommentLikes = 11017, // 更改评论赞数量
        RemoveHeroComment = 11018, // 删除英雄评论
        AddHeroComment = 11019, // 添加英雄评论
        AddRecommentVedio = 11020, // 添加推荐录像
        ZoneTransform = 11021, // 跨zone



        //福利系统
        WelfareStallAdd = 12001, // 福利档次新增
        WelfareStallDelete = 12002, // 福利档次删除
        WelfareStallModify = 12003, // 福利档次修改
        WelfareStallGet = 12004, // 福利档次查询

        WelfarePlayerAdd = 12005, // 福利玩家新增
        WelfarePlayerDelete = 12006, // 福利玩家删除
        //WelfarePlayerModify = 12007, // 福利玩家修改
        WelfarePlayerGet = 12008, // 福利玩家查询

        ItemProduce = 13001, // 道具产出查询
        ItemConsume = 13002, // 道具消耗查询
        CurrencyProduce = 13003, // 货币产出查询
        CurrencyConsume = 13004, // 货币消耗查询
        LoginOrLogOut = 13005, // 登入/登出查询

        //服务器状态
        GetServerState = 14001,//获取服务器状态
        SetServerState = 14002,//设置服务器状态

        TipOffInfo = 15001,//举报信息
        IgnoreTipOff = 15002,//忽略该举报信息

        SendPersonEmail = 15003,//发送个人邮件

        GetItemInfo = 15004,//获取道具信息
        ChangeItemNum = 15005,//调整道具数量
        DelActiveProgress = 15006,//调整任务进度
    }

    public enum GMUpdateCharDataType
    { 
        Diamond = 1, // 更新钻石
        AccumulateDaily = 2, // 当日充值额度
        AccumulateFreq = 3, // 活动周期充值额度
        RechargeMoney = 4, // 充值总额
    }

    public enum AlarmType
    {
        NETWORK = 1,
        DB = 2,
        REDIS = 3,
    }

    public enum OnlinePlayerState
    { 
        NORMAL = 1,
        WAIT = 2,
        FULL = 3
    }

    public enum ChannelTask
    {
        FirstLogin = 1200, // 首次登录并达到10级（8月24日当天有效）
        EverydayLogin = 1201, // 每日登录
        Login2Days = 1202, // 连续2天登录
        Login3Days = 1203, // 连续3天登录
        Login5Days = 1204, // 连续5天登录
        Login15Days = 1205, // 连续15天登录
        Login30Days = 1206, // 连续30天登录
        VIPLevel5 = 1207, // VIP5级
        VIPLevel7 = 1208, // VIP7级
        VIPLevel12 = 1209 // VIP12级
    }

    public enum ChannelTaskRewardEamil
    {
        EverydayLoginReward = 6321,     // 每日登陆礼包
        Login2DaysReward = 6322,         //连续登陆2日礼包
        Login3DaysReward = 6323,        //连续登陆3日礼包
        Login5DaysReward = 6324,        //连续登陆5日礼包
        Login15DaysReward = 6325,       //连续登陆15日礼包
        Login30DaysReward = 6326,       //连续登陆30日礼包
        VipLevel5Reward = 6327,            //VIP等级达到5级礼包
        VipLevel7Reward = 6328,            //VIP等级达到7级礼包
        VipLevel12Reward = 6329         //IP等级达到12级礼包
    }

    public enum CreatePlayerStep
    { 
        Character = 1,
        Recource =2,
        HeroQueue = 3,
        SkillQueue = 4,
        Chest = 5,
        Task = 6,
        Email = 7,
        Shop = 8,
        Items = 9,
        Counter = 10,
        Heros = 11,
        Skills = 12,
        Recharge = 13,
        GameLevel = 14,
        AccountUid = 9999,
        End = 10000,
    }

    public enum LoadPlayerStep
    { 
        BaseInfo = 1,
        Recource = 2,
        Counter = 3,
        Skill = 4,
        Hero = 5,
        SkillQueue = 6,
        HeroQueue = 7,
        Chest =8,
        TaskCur = 9,
        EmailItem = 10,
        Email = 11,
        Item =12,
        Shop = 13,
        Activity = 14,
        Recharge = 15,
        GameLevel = 16,
        End = 10000
    }
}

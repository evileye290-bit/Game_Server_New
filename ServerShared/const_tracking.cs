using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    //public enum RewardType
    //{
    //    FirstReward = 1,                         //首充
    //    VipReward = 2,                            //VIP奖励
    //    SantoFundReward = 3,                      //城主基金
    //    GrowthFundReward = 4,                     //成长基金
    //    DailyRecharge = 5,                      //每日充值
    //    AccumulateyReward = 6,                    //累计充值
    //    AccumulateyDiamondReward = 7,             //累计消费
    //    MonthCard = 8,                          //月卡
    //    WeekCard = 9,                            //周卡
    //    ForeverCard = 10                         //永久卡
    //}

    public enum WelfareRewardType
    {
        Signing = 1,                            //签到
        FristSevenDaily = 2,                      //七日礼包
        OnlineReward = 3,                         //在线奖励
        LevelReward = 4,                        //等级奖励
        ChrismasSevenDaily = 5,                 //圣诞七日礼包
        RedPacketWall = 6                        //红包墙
    }

    public enum OtherRewardType
    {
        ActivityBox = 1,                         //活跃度领取    
        CampCelebrity = 2,                        //阵营每日领取
        CampVote = 3,                            //阵营投票 
        Retained = 4                             //留存                       
    }

    public enum ActivityRewardType
    {
        CampHonorBattle = 1,                     //荣誉之战  
        FamliyDungeonPass = 2,                    //家族副本通关奖励
        FamliyDungeonContribution = 3,            //家族副本伤害排行
        FamliyDungeonFrist = 4                  //家族最快奖励
    }

    public enum MarriedRewardType
    {
        PerDayGiftBag = 1,           //婚礼每日礼包
        SelectGift = 2, //流程选择获得
        Lucky = 3,   //抽奖
        Firework = 4,   //烟花
    }

    public enum RaffleType
    {
        PetGoldOne = 1,
        PetGoldTen = 2,
        PetDiamondOne = 3,
        PetDiamondTen = 4,
        PetDiamondTenDiscount = 5
    }

    public enum LotteryType
    {
        SmallRouletteOne = 1,
        SmallRouletteTen = 2,
        BigRouletteOne = 3,
        BigRouletteTen = 4
    }

    public enum MarriedCostType
    {
        GetMarried = 1,       //结婚 
        Divorce = 2,         //离婚
        WeddingSpeaker = 3,    //婚礼喇叭
        SelectGift = 4,      //流程选择
        WeddingInvitation = 5, //结婚请柬
        Firework = 5, //结婚烟花
    }

    public enum ConsumeWay
    {
        /// <summary>
        /// 打开宝箱
        /// </summary>
        OpenChest = 1,
        /// <summary>
        /// 升级英雄
        /// </summary>
        HeroUpgrade = 2,
        /// <summary>
        /// 技能升级
        /// </summary>
        SkillUpgrade = 3,
        /// <summary>
        /// 物品出售，消耗物品
        /// </summary>
        ItemSell = 4,
        /// <summary>
        /// 物品使用，消耗物品
        /// </summary>
        ItemUse = 5,
        /// <summary>
        /// 商店购买英雄&技能
        /// </summary>
        ShopCard = 6,
        /// <summary>
        /// 商店购买宝箱
        /// </summary>
        ShopChest = 7,
        /// <summary>
        /// 商店购买金币
        /// </summary>
        ShopGold = 8,
        /// <summary>
        /// 商店初始值
        /// </summary>
        Shop = 9,
        /// <summary>
        /// 任务物品消耗
        /// </summary>
        TaskItemUse = 10,
        /// <summary>
        /// 购买次数
        /// </summary>
        BuyCount = 11,

        /// <summary>
        /// 战斗胜利
        /// </summary>
        BattleWin = 20,
        /// <summary>
        /// 战斗失败
        /// </summary>
        BattleFail = 21,
        /// <summary>
        /// 战斗平局
        /// </summary>
        BattleTie = 22,
        /// <summary>
        /// 名字更改
        /// </summary>
        ChangeName = 23,
        /// <summary>
        /// 物品购买
        /// </summary>
        ItemBuy = 24,
        /// <summary>
        /// 皮肤购买
        /// </summary>
        SkinBuy = 24,
        /// <summary>
        /// 聊天喇叭
        /// </summary>
        UseTrumpet = 25,
        /// <summary>
        /// 赛季奖励
        /// </summary>
        SeasonReward = 26,
        /// <summary>
        /// 赠送礼物
        /// </summary>
        PresentGift = 27,
        /// <summary>
        /// 活动
        /// </summary>
        Activity = 28,
        /// <summary>
        /// 战斗结果
        /// </summary>
        BattleResult = 29,
        /// <summary>
        /// 每日答题
        /// </summary>
        DailyQuestion = 30,
        /// <summary>
        /// 分解
        /// </summary>
        BreakUp = 31,
        /// <summary>
        /// 合成
        /// </summary>
        Compose = 32,
        /// <summary>
        /// 剧情
        /// </summary>
        StoryLine = 33,
        /// <summary>
        /// 宝图
        /// </summary>
        TreasureMap = 34,
        /// <summary>
        /// 物品关卡
        /// </summary>
        ItemGameLevel = 35,
        /// <summary>
        /// PVE升级
        /// </summary>
        PVEHeroUpgrade = 36,
        /// <summary>
        /// PVE主角升级
        /// </summary>
        PVEMainHeroUpgrade = 37,
        /// <summary>
        /// 钓鱼
        /// </summary>
        Fishing = 38,
        /// <summary>
        /// 兑换鱼币
        /// </summary>
        ConvertFishCoin = 41,
        /// <summary>
        /// 送礼物
        /// </summary>
        RadioGiveGift = 42,

        /// <summary>
        /// 武魂升级
        /// </summary>
        HeroLevelUp = 43,
        /// <summary>
        /// 武魂觉醒
        /// </summary>
        HeroAwaken = 44,
        /// <summary>
        /// 魂骨熔炼
        /// </summary>
        SmeltSoulBone = 45,
        /// <summary>
        /// 重置天赋
        /// </summary>
        HeroResetTalent = 46,
        /// <summary>
        /// 打造
        /// </summary>
        Forge = 47,
        /// <summary>
        /// 背包扩容
        /// </summary>
        BagSpaceInc = 48,
        /// <summary>
        /// 阵营养成升级
        /// </summary>
        CampStarLevelUp = 49,

        /// <summary>
        /// 装备升级
        /// </summary>
        EquipmentUpgrade = 50,
        /// <summary>
        /// 装备碎裂
        /// </summary>
        EquipmentCrack = 51,
        /// <summary>
        /// 装备升阶
        /// </summary>
        EquipmentAdvance = 52,
        /// <summary>
        /// 装备附魔
        /// </summary>
        EquipmentEnchant = 53,

        /// <summary>
        /// 猎杀魂兽
        /// </summary>
        Hunting = 54,

        /// <summary>
        /// 福利经验副本
        /// </summary>
        BenefitsExp = 55,
        /// <summary>
        /// 福利银币副本
        /// </summary>
        BenefitsGold = 56,
        /// <summary>
        /// 高原之乡
        /// </summary>
        BenefitsSoulPower = 57,
        /// <summary>
        /// 高原之乡副本
        /// </summary>
        BenefitsSoulBreath = 58,
        /// <summary>
        /// 秘境购买
        /// </summary>
        SecretAreaSweepCountBuy = 59,
        /// <summary>
        /// 购买爱心赠送次数
        /// </summary>
        BuyFriendHeartGiveCount = 60,
        /// <summary>
        /// 购买爱心获取次数
        /// </summary>
        BuyFriendHeartTakeCount = 61,

        /// <summary>
        /// 更改阵营
        /// </summary>
        ChangeCamp = 71,
        /// <summary>
        /// 阵营膜拜
        /// </summary>
        CampWorship = 72,

        /// <summary>
        /// 魂环强化
        /// </summary>
        EnhanceSoulRing = 73,
        /// <summary>
        /// 魂环突破
        /// </summary>
        //BreakSoulRing = 74,
        /// <summary>
        /// 拟态训练
        /// </summary>
        OnHook = 75,
        /// <summary>
        /// 整点boss
        /// </summary>
        IntegralBossBuy = 76,
        /// <summary>
        /// 加速完成委派事件
        /// </summary>
        CompleteDelegation = 80,
        /// <summary>
        /// 手动刷新委派事件
        /// </summary>
        RefreshDelegation = 81,
        /// <summary>
        /// 购买委派次数
        /// </summary>
        BuyDelegationCount = 82,

        /// <summary>
        /// 重置竞技场挑战时间
        /// </summary>
        ResetArenaFightTime = 83,
        /// <summary>
        /// 抽卡
        /// </summary>
        DrawHeroCard = 84,
        /// <summary>
        /// 伙伴进阶
        /// </summary>
        HeroUpSteps = 85,
        /// <summary>
        /// 章节
        /// </summary>
        Chapter = 86,
        /// <summary>
        /// 购买时空力
        /// </summary>
        BuyTimeSpacePower = 87,
        /// <summary>
        /// 购买成神之路体力
        /// </summary>
        BuyGodPathPower = 88,
        /// <summary>
        /// 成神之路，七斗罗战斗
        /// </summary>
        GodPathFight = 89,
        /// <summary>
        /// 购买海洋之心次数
        /// </summary>
        BuyGodPathHeart = 90,
        /// <summary>
        /// 购买三叉戟次数
        /// </summary>
        BuyGodPathTrident = 100,
        /// <summary>
        /// 三叉戟灵巧花费
        /// </summary>
        BuyGodPathTridentStrategy = 101,
        /// <summary>
        /// 三叉戟奖励翻倍
        /// </summary>
        BuyGodPathTridentRandom = 102,
        /// <summary>
        /// 点亮拼图
        /// </summary>
        GodPathLightPuzzle = 103,
        /// <summary>
        /// 成神之路登高
        /// </summary>
        GodPathHeight = 104,

        /// <summary>
        /// 许愿池
        /// </summary>
        WishPool = 110,

        /// <summary>
        /// 购买行动力
        /// </summary>
        BuyAction = 120,

        /// <summary>
        /// 阵营建设每日可购买筛子次数
        /// </summary>
        CampBuildBuyDiceCount = 121,

        /// <summary>
        /// 通行证周期刷新
        /// </summary>
        PassCardClear = 130,
        /// <summary>
        /// 阵营战据点属性道具
        /// </summary>
        CampFortAddNature = 131,
        /// <summary>
        /// 魂师传记
        /// </summary>
        Tower = 132,
        /// <summary>
        /// 武魂共鸣开启槽位
        /// </summary>
        BuyResonanceGrid = 133,
        /// <summary>
        /// 强化魂技
        /// </summary>
        EnhanceSoulSkill = 134,
        /// <summary>
        /// 魂环强化（突破）
        /// </summary>
        BreakSoulSkill = 135,
        /// <summary>
        /// 成神
        /// </summary>
        HeroGod = 136,
        /// <summary>
        /// 藏宝图拼图
        /// </summary>
        TreasurePuzzle = 137,
        /// <summary>
        /// 跨服战次数购买
        /// </summary>
        CrossBattleBuyCount = 138,
        /// <summary>
        /// 主题Boss次数购买
        /// </summary>
        ThemeBossBuyCount = 139,
        /// <summary>
        /// 下注
        /// </summary>
        CrossGuessing = 140,
        /// <summary>
        /// 返还装备升级
        /// </summary>
        ReturnEquipmentUpgrade = 141,
        /// <summary>
        /// 重置
        /// </summary>
        HeroRevert = 141,
        /// <summary>
        /// 购买养成礼包
        /// </summary>
        BuyCultivateGift = 142,
        /// <summary>
        /// 小游戏复活
        /// </summary>
        ShovelGameRevive = 143,
        /// <summary>
        /// 暗器奖励训练
        /// </summary>
        HidderWeapon = 144,
        /// <summary>
        /// 暗器奖励
        /// </summary>
        BuyHidderWeaponItem = 145,
        /// <summary>
        /// 跨服boss购买行动力次数
        /// </summary>
        CrossBossBuyActionCount = 146,
        /// <summary>
        /// 暗器奖励
        /// </summary>
        BuySeaTreasureItem = 147,
        SeaTreasure = 148,
        /// <summary>
        /// 幽香花园
        /// </summary>
        Garden = 149,
        /// <summary>
        /// 魂环淬炼
        /// </summary>
        SoulBoneQuenching = 150,
        /// <summary>
        /// 乾坤问情
        /// </summary>
        DivineLove = 151,
        /// <summary>
        /// 乾坤问情道具购买
        /// </summary>
        BuyDivineLoveItem = 152,
        /// <summary>
        /// 海岛登高
        /// </summary>
        IslandHigh = 153,
        /// <summary>
        /// 暗器奖励高级
        /// </summary>
        HidderWeaponHigh = 154,
        /// <summary>
        /// 海神探险高级
        /// </summary>
        SeaTreasureHigh = 155,
        /// <summary>
        /// 乾坤问情高级
        /// </summary>
        DivineLoveHigh = 156,
        /// <summary>
        /// 端午活动
        /// </summary>
        DragonBoat = 157,
        /// <summary>
        /// 端午活动买门票
        /// </summary>
        DragonBoatBuyTicket = 158,
        /// <summary>
        /// 昊天石壁
        /// </summary>
        StoneWall = 159,
        /// <summary>
        /// 昊天石壁高级
        /// </summary>
        StoneWallHigh = 160,
        /// <summary>
        /// 昊天石壁买道具
        /// </summary>
        BuyStoneWallItem = 161,
        /// <summary>
        /// 海岛挑战
        /// </summary>
        IslandChallenge = 162,
        /// <summary>
        /// 漫游激活
        /// </summary>
        TravelHeroActivate = 163,
        /// <summary>
        /// 嘉年华特卖场
        /// </summary>
        BuyCarnivalMallGift = 164,
        /// <summary>
        /// 购买漫游商店
        /// </summary>
        ButTravelShopItem = 165,
        /// <summary>
        /// 暗器升级
        /// </summary>
        HiddenWeaponUpgrade = 166,
        HiddenWeapon = 167,
        /// <summary>
        /// 史莱克邀约
        /// </summary>
        ShrekInvitation = 168,
        /// <summary>
        /// 轮盘
        /// </summary>
        Roulette = 169,
        /// <summary>
        /// 皮划艇训练
        /// </summary>
        CanoeTrain = 170,
        /// <summary>
        /// 皮划艇比赛
        /// </summary>
        CanoeMatch = 171,
        /// <summary>
        /// 主战阵容
        /// </summary>
        MainBattleQueue = 172,
        /// <summary>
        /// 传承
        /// </summary>
        HeroInherit = 173,
        /// <summary>
        /// 中秋
        /// </summary>
        MidAutumn = 174,
        /// <summary>
        /// 常规道具兑换
        /// </summary>
        ItemExchange = 175,
        /// <summary>
        /// 跨服挑战次数购买
        /// </summary>
        CrossChallengeBuyCount = 176,
        /// <summary>
        /// 玄玉养成
        /// </summary>
        JewelAdvance = 177,
        /// <summary>
        /// 九考试炼
        /// </summary>
        NineTest = 178,
        /// <summary>
        /// 仓库
        /// </summary>
        Warehouse = 179,
        /// <summary>
        /// 超出货币转化
        /// </summary>
        BeyondCurrencyConvert = 180,
        /// <summary>
        /// 学院
        /// </summary>
        School = 181,
        /// <summary>
        /// 宠物孵化
        /// </summary>
        PetHatch = 182,
        /// <summary>
        /// 宠物放生
        /// </summary>
        PetRelease = 183,
        /// <summary>
        /// 宠物升级
        /// </summary>
        PetLevelUp = 184,
        /// <summary>
        /// 宠物继承
        /// </summary>
        PetInherit = 185,
        /// <summary>
        /// 宠物技能洗炼
        /// </summary>
        PetSkillBaptize = 186,
        /// <summary>
        /// 宠物突破
        /// </summary>
        PetBreak = 187,
        /// <summary>
        /// 宠物融合
        /// </summary>
        PetBlend = 188,
        /// <summary>
        /// 宠物喂养
        /// </summary>
        PetFeed = 189,
        /// <summary>
        /// 玄天宝箱
        /// </summary>
        XuanBox = 190,
        /// <summary>
        /// 史莱克乐园
        /// </summary>
        Shrekland = 191,
        /// <summary>
        /// 魔鬼训练
        /// </summary>
        DevilTraining = 192,
        /// <summary>
        /// 魔鬼训练负重
        /// </summary>
        DevilTrainingHigh = 193,
        /// <summary>
        /// 九笼祈愿
        /// </summary>
        WishLantern = 200,
        /// <summary>
        /// 百兽爬塔购买
        /// </summary>
        SpaceTimeTowerBuy = 201,
        /// <summary>
        /// 百兽爬塔刷新卡池
        /// </summary>
        SpaceTimeRefreshCardPool = 202,
        /// <summary>
        /// 时空塔重置
        /// </summary>
        SpaceTimeReset = 203,
        /// <summary>
        /// 时空塔购买
        /// </summary>
        SpaceTimeShopBuy = 204,
        /// <summary>
        /// 神域赐福抽卡消耗
        /// </summary>
        DomainBenedictionDrawExpend = 205,
    }

    //FR20161126
    public enum ObtainWay
    {
        /// <summary>
        /// 无，当重复获取的时候
        /// </summary>
        None = 0,

        /// <summary>
        /// 打开宝箱
        /// </summary>
        OpenChest = 1,
        /// <summary>
        /// 升级英雄
        /// </summary>
        HeroUpgrade = 2,
        /// <summary>
        /// 技能升级
        /// </summary>
        SkillUpgrade = 3,
        /// <summary>
        /// 战斗结果
        /// </summary>
        BattleResult = 4,
        /// <summary>
        /// 任务奖励
        /// </summary>
        Task = 7,
        /// <summary>
        /// 邮件奖励
        /// </summary>
        Eamil = 8,
        /// <summary>
        /// 物品出售，返还货币
        /// </summary>
        ItemSell = 11,
        /// <summary>
        /// 物品使用，获得物品
        /// </summary>
        ItemUse = 12,
        /// <summary>
        /// 商店购买英雄&技能
        /// </summary>
        ShopCard = 13,
        /// <summary>
        /// 商店购买宝箱
        /// </summary>
        ShopChest = 14,
        /// <summary>
        /// 商店购买金币
        /// </summary>
        ShopGold = 15,
        /// <summary>
        /// 商店购买
        /// </summary>
        ShopBuy = 16,
        /// <summary>
        /// 魂骨商店
        /// </summary>
        ShopSoulBone = 17,

        /// <summary>
        /// 战斗胜利
        /// </summary>
        BattleWin = 20,
        /// <summary>
        /// 战斗失败
        /// </summary>
        BattleFail = 21,
        /// <summary>
        /// 战斗平局
        /// </summary>
        BattleTie = 22,
        /// <summary>
        /// 每天登陆获取匹配值
        /// </summary>
        DailyLogin = 23,
        /// <summary>
        /// 上升段位附加分
        /// </summary>
        UpBattleLevel = 24,
        /// <summary>
        /// 物品购买
        /// </summary>
        ItemBuy = 25,
        /// <summary>
        /// 赛季奖励
        /// </summary>
        SeasonReward = 25,
        /// <summary>
        /// 充值
        /// </summary>
        Recharge = 26,
        /// <summary>
        /// 活动
        /// </summary>
        Activity = 27,
        /// <summary>
        /// 每日答题
        /// </summary>
        DailyQuestion = 28,
        /// <summary>
        /// 分解
        /// </summary>
        BreakUp = 29,
        /// <summary>
        /// 合成
        /// </summary>
        Compose = 30,
        /// <summary>
        /// 主角升级
        /// </summary>
        LevelUp = 31,
        /// <summary>
        /// 刷新
        /// </summary>
        Refresh = 32,
        /// <summary>
        /// 剧情
        /// </summary>
        StoryLine = 33,
        /// <summary>
        /// 宝图
        /// </summary>
        TreasureMap = 34,
        /// <summary>
        /// 物品关卡
        /// </summary>
        ItemGameLevel = 35,
        /// <summary>
        /// 海图扫荡
        /// </summary>
        OnePieceSweep = 36,
        /// <summary>
        /// 海图通关
        /// </summary>
        OnePieceBattle = 37,
        /// <summary>
        /// 领取鱼饵
        /// </summary>
        GetBait = 38,
        /// <summary>
        /// 钓鱼
        /// </summary>
        Fishing = 39,
        /// <summary>
        /// 兑换
        /// </summary>
        ConvertFishCoin = 42,
        /// <summary>
        /// 送礼物
        /// </summary>
        RadioGiveGift = 43,
        /// <summary>
        /// 贡献奖励
        /// </summary>
        RadioContributionReward = 44,
        /// <summary>
        /// 剧情宝箱
        /// </summary>
        StoryLineChest = 45,
        /// <summary>
        /// 组队boss
        /// </summary>
        TeamBoss = 46,

        /// <summary>
        /// 魂骨熔炼
        /// </summary>
        SmeltSoulBone = 47,
        //打造
        Forge = 48,

        /// <summary>
        /// 装备升级
        /// </summary>
        EquipmentUpgrade = 50,
        /// <summary>
        /// 装备碎裂
        /// </summary>
        EquipmentCrack = 51,
        /// <summary>
        /// 装备升阶段
        /// </summary>
        EquipmentAdvance = 52,

        /// <summary>
        /// 委派事件奖励
        /// </summary>
        DelegationReward = 60,

        /// <summary>
        /// 好友送心
        /// </summary>
        FriendlyHeart = 101,

        /// <summary>
        /// 阵营排名
        /// </summary>
        CampRankReward = 201,
        /// <summary>
        /// 随机选择阵营
        /// </summary>
        RandomChooseCamp = 202,
        /// <summary>
        /// 阵营膜拜
        /// </summary>
        CampWorship = 203,

        /// <summary>
        /// 单人剧情副本
        /// </summary>
        StoryDungeon = 204,

        /// <summary>
        /// 猎杀魂兽
        /// </summary>
        Hunting = 205,

        /// <summary>
        /// 整点boss
        /// </summary>
        IntegralBoss = 206,

        /// <summary>
        /// 段位奖励
        /// </summary>
        RankLevelReward = 207,
        /// <summary>
        /// 竞技场
        /// </summary>
        ChallengeResult = 208,
        /// <summary>
        /// 秘境
        /// </summary>
        SecretAreaDungeon = 209,
        /// <summary>
        /// 秘境扫荡
        /// </summary>
        SecretAreaSweep = 210,

        /// <summary>
        /// 通行证每日奖励
        /// </summary>
        PassTaskDailyReward = 211,
        /// <summary>
        /// 通行证等级奖励
        /// </summary>
        PassTaskLevelReward = 212,
        /// <summary>
        /// 抽卡
        /// </summary>
        DrawHeroCard = 213,

        /// <summary>
        /// 魂师试炼
        /// </summary>
        Benefit = 214,
        /// <summary>
        /// 章节
        /// </summary>
        Chapter = 215,
        /// <summary>
        /// 购买时空力
        /// </summary>
        BuyTimeSpacePower = 216,
        /// <summary>
        /// 时空力按时间回复
        /// </summary>
        TimeSpacePowerRecory = 217,
        /// <summary>
        /// 购买成神之路体力
        /// </summary>
        BuyGodPathPower = 218,
        /// <summary>
        /// 成神之路每日刷新
        /// </summary>
        GodPathDaily = 218,
        /// <summary>
        /// 成神之路七斗罗
        /// </summary>
        GodPathSevenFight = 219,
        /// <summary>
        /// 成神之路任务
        /// </summary>
        GodPathTask = 220,
        /// <summary>
        /// 成神之路登高
        /// </summary>
        GodPathHeight = 221,

        /// <summary>
        /// 跨服活跃奖励
        /// </summary>
        CrossActivityReward = 222,
        /// <summary>
        /// 跨服海选奖励
        /// </summary>
        CrossPreliminaryReward = 223,
        /// <summary>
        /// 海选战斗奖励
        /// </summary>
        CrossPreliminaryResult = 224,
        /// <summary>
        /// 许愿池
        /// </summary>
        WishPool = 230,
        /// <summary>
        /// 阵营建设
        /// </summary>
        CampBuild = 240,
        /// <summary>
        /// 阵营采集
        /// </summary>
        CampGather = 250,
        /// <summary>
        /// 通行证任务
        /// </summary>
        PassCardTask = 251,
        /// <summary>
        /// 通行证买经验
        /// </summary>
        PassCardBuyExp = 252,
        /// <summary>
        /// 爬塔
        /// </summary>
        Tower = 253,
        /// <summary>
        /// 阵营据点战斗
        /// </summary>
        CampBattle = 254,
        /// <summary>
        /// 阵营组队战斗
        /// </summary>
        CampTeamBattle = 255,
        /// <summary>
        /// 阵营战中立
        /// </summary>
        CampBattleNeutral = 256,
        /// <summary>
        /// 阵营宝箱
        /// </summary>
        CampBox = 257,
        /// <summary>
        /// 伙伴重置
        /// </summary>
        HeroRevert = 258,
        /// <summary>
        /// 挂机
        /// </summary>
        Onhook = 259,
        /// <summary>
        /// 推图
        /// </summary>
        PushFigure = 260,
        /// <summary>
        /// 挖宝
        /// </summary>
        ShovelTreasure = 261,
        /// <summary>
        /// 贡献
        /// </summary>
        Contribution = 262,
        /// <summary>
        /// 主题通行证等级奖励
        /// </summary>
        ThemePassLevelReward = 263,
        /// <summary>
        /// 跨服全服奖励
        /// </summary>
        CrossServerReward = 264,
        /// <summary>
        /// 领取物品
        /// </summary>
        ReceiveItem = 265,
        /// <summary>
        /// 领取首充奖励
        /// </summary>
        GetFirstRecharge = 266,
        /// <summary>
        /// 排行榜冲榜奖励
        /// </summary>
        RankReward = 267,
        /// <summary>
        /// 返还
        /// </summary>
        ReturnEquipmentUpgrade = 268,
        /// <summary>
        /// 藏宝拼图
        /// </summary>
        TreasurePuzzle = 269,
        /// <summary>
        /// 购买养成礼包
        /// </summary>
        BuyCultivateGift = 270,
        /// <summary>
        /// 暗器奖励
        /// </summary>
        HidderWeapon = 271,
        /// <summary>
        /// 暗器奖励
        /// </summary>
        BuyHidderWeaponItem = 278,
        /// <summary>
        /// 领取免费小额礼包
        /// </summary>
        ReceiveFreePettyGift = 279,
        /// <summary>
        /// 跨服Boss通关奖励
        /// </summary>
        CrossBossPassReward = 280,
        /// <summary>
        /// 领取每日充值累计天数奖励
        /// </summary>
        GetDailyRechargeDaysReward = 281,
        /// <summary>
        /// 海洋寻宝
        /// </summary>
        BuySeaTreasureItem = 282,
        SeaTreasure = 283,
        /// <summary>
        /// 领取角色七日奖励
        /// </summary>
        GetHeroDaysReward = 284,
        /// <summary>
        /// 领取累积充值奖励
        /// </summary>
        GetAccumulateRechargeReward = 285,
        /// <summary>
        /// 领取累积充值奖励
        /// </summary>
        CrossBossDefense = 286,
        /// <summary>
        /// 副本结算
        /// </summary>
        DungeonFinalReward = 287,
        /// <summary>
        /// 跨服Boss通关奖励
        /// </summary>
        CrossBossRankReward = 288,
        /// <summary>
        /// 跨服Boss通关奖励
        /// </summary>
        CrossBossDungeonReward = 289,
        /// <summary>
        /// 新服促销累计天数奖励
        /// </summary>
        NewServerPromotionDaysReward = 290,
        /// <summary>
        /// 幽香花园
        /// </summary>
        Garden = 291,
        /// <summary>
        /// 乾坤问情
        /// </summary>
        DivineLove = 292,
        /// <summary>
        /// 乾坤问情道具购买
        /// </summary>
        BuyDivineLoveItem = 293,
        /// <summary>
        /// 幸运翻翻乐累计奖励
        /// </summary>
        LuckyFlipCardCumulateReward = 294,
        /// <summary>
        ///  幸运翻翻乐翻牌奖励
        /// </summary>
        LuckyFlipCardReward = 295,
        /// <summary>
        /// 海岛登高
        /// </summary>
        IslandHigh = 296,
        /// <summary>
        /// 暗器奖励高级
        /// </summary>
        HidderWeaponHigh = 297,
        /// <summary>
        /// 海神探宝高级
        /// </summary>
        SeaTreasureHigh = 298,
        /// <summary>
        /// 乾坤问情高级
        /// </summary>
        DivineLoveHigh = 299,
        /// <summary>
        /// 三叉戟
        /// </summary>
        Trident = 300,
        /// <summary>
        /// 端午活动
        /// </summary>
        DragonBoat = 301,
        /// <summary>
        /// 端午活动买门票
        /// </summary>
        DragonBoatBuyTicket = 302,
        /// <summary>
        /// 昊天石壁
        /// </summary>
        StoneWall = 303,
        /// <summary>
        /// 昊天石壁高级
        /// </summary>
        StoneWallHigh = 304,
        /// <summary>
        /// 昊天石壁买道具
        /// </summary>
        BuyStoneWallItem = 305,
        /// <summary>
        /// 海岛挑战
        /// </summary>
        IslandChallenge = 306,
        /// <summary>
        /// 嘉年华Boss
        /// </summary>
        CarnivalBoss = 307,
        /// <summary>
        /// 嘉年华累充
        /// </summary>
        CarnivalRecharge = 308,
        /// <summary>
        /// 漫游物品使用
        /// </summary>
        TravelItemAdd = 309,
        /// <summary>
        /// 嘉年华特卖场
        /// </summary>
        BuyCarnivalMallGift = 310,
        /// <summary>
        /// 漫游物品使用
        /// </summary>
        TravelFriendAdd = 311,
        /// <summary>
        /// 漫游宝箱奖励
        /// </summary>
        TravelBoxReward = 312,
        /// <summary>
        /// 主题Boss
        /// </summary>
        ThemeBoss = 313,
        /// <summary>
        /// 暗器
        /// </summary>
        HiddenWeapon = 314,
        /// <summary>
        /// 史莱克邀约
        /// </summary>
        ShrekInvitation = 315,
        /// <summary>
        /// 海岛挑战
        /// </summary>
        TravelShopBuy = 316,
        /// <summary>
        /// 轮盘
        /// </summary>
        Roulette = 317,
        /// <summary>
        /// 皮划艇训练
        /// </summary>
        CanoeTrain = 318,
        /// <summary>
        /// 皮划艇比赛
        /// </summary>
        CanoeMatch = 319,
        /// <summary>
        /// 中秋
        /// </summary>
        MidAutumn = 320,
        /// <summary>
        /// 常规道具兑换
        /// </summary>
        ItemExchange = 321,
        /// <summary>
        /// 主题烟花
        /// </summary>
        ThemeFirework = 322,
        /// <summary>
        /// 玄玉养成
        /// </summary>
        JewelAdvance = 323,
        /// <summary>
        /// 九考试炼
        /// </summary>
        NineTest = 324,
        /// <summary>
        /// 银币仓库
        /// </summary>
        Warehouse = 325,
        /// <summary>
        /// 超出货币转化
        /// </summary>
        BeyondCurrencyConvert = 326,
        /// <summary>
        /// 物品仓库
        /// </summary>
        ItemWarehouse = 327,
        /// <summary>
        /// 学院
        /// </summary>
        School = 328,
        /// <summary>
        /// 学院任务
        /// </summary>
        SchoolTask = 329,
        /// <summary>
        /// 答题
        /// </summary>
        AnswerQuestion = 330,
        /// <summary>
        /// 钻石返利
        /// </summary>
        DiamondRebate = 331,
        /// <summary>
        /// 宠物孵化
        /// </summary>
        PetHatch = 332,
        /// <summary>
        /// 放生宠物
        /// </summary>
        PetRelease = 333,
        /// <summary>
        /// 选择宝箱
        /// </summary>
        OpenChooseBox = 334,
        /// <summary>
        /// 玄天宝箱
        /// </summary>
        XuanBox = 335,
        /// <summary>
        /// 网页支付返利
        /// </summary>
        WebPayRebate = 326,
        /// <summary>
        /// 九笼祈愿
        /// </summary>
        WishLantern = 340,
        /// <summary>
        /// 领取累积充值奖励
        /// </summary>
        NewRechargeGiftRewards = 341,


        /// <summary>
        /// 夺宝翻翻乐累计奖励
        /// </summary>
        TreasureFlipCardCumulateReward = 342,
        /// <summary>
        ///  夺宝翻翻乐翻牌奖励
        /// </summary>
        TreasureFlipCardReward = 343,
        /// <summary>
        /// 史莱克乐园
        /// </summary>
        Shrekland = 344,
        /// <summary>
        /// 魔鬼训练
        /// </summary>
        DevilTraining = 345,
        /// <summary>
        /// 魔鬼训练
        /// </summary>
        DevilTrainingHigh = 346,
        /// <summary>
        /// 神域赐福 阶段奖励
        /// </summary>
        DomainBenedictionStageAward = 348,
        /// <summary>
        /// 神域赐福 抽取奖励 单次
        /// </summary>
        DomainBenedictionDrawAwardOnly = 349,
        /// <summary>
        /// 神域赐福 抽取奖励 十连
        /// </summary>
        DomainBenedictionDrawAwardMore = 350,
        /// <summary>
        /// 神域赐福 基础奖励领取
        /// </summary>
        DomainBenedictionBaseAward = 351,
        /// <summary>
        /// 时空塔
        /// </summary>
        SpaceTimeTower = 352,
        /// <summary>
        /// 时空塔重置
        /// </summary>
        SpaceTimeReset = 353,
        /// <summary>
        /// 时空塔阶段奖励
        /// </summary>
        SpaceTimeStageAward = 354,
        /// <summary>
        /// 时空塔商城购买
        /// </summary>
        SpaceTimeShopBuy = 355,
        /// <summary>
        /// 时空塔副本奖励
        /// </summary>
        SpaceTimeTowerDungeonReward = 358,
        /// <summary>
        /// 漂流探宝
        /// </summary>
        DriftExplore = 357,
        /// <summary>
        /// 后台添加
        /// </summary>
        GM = 999,
    }

}


using DataProperty;

namespace ServerShared
{
    static public class GameConfig
    {
        public static int RegionSize;
        public static int BroadcastLimit;

        public static int WaitStreamCountHalf;
        public static int WaitStreamCountQuarter;
        public static int WaitStreamCounDisconnect;
        public static bool CatchOfflinePlayer;
        public static int CatchOfflinePeriod;

        public static int InitialBagLimit;
        public static int SpeakerCost;
        public static int EmailItemTimeOut;

        public static string LogServerKey;

        public static int CreateFamilyCost;
        public static int CreateFamilyLevel;
        public static int JoinFamilyLevel;
        public static int FamilyNameLength;
        public static int FamilyDeclarationLength;
        public static int FamilyNoticeLength;
        public static string FamilyDefaultDeclartation;

        public static int OfflineRewardMaxMinutes;
        public static int OfflineRewardMinMinutes;
        public static int OfflineMaxRewardCount;
        public static int OfflineRewardDiamond;
        public static int OfflineRewardMinLevel;

        public static bool TrackingLogSwitch;

        public static int UnlockItemTimeCost;

        public static int FamilyChatSeconds;
        public static int MapChatSeconds;
        public static int ItemUnravelEmailId;

        public static int ACCEPT_DUNGEON_FRAME = 20; // 当前帧数大于该数 则可以在当前zone创建副本 否则需要向manager请求其他zone来创建副本
        public static int ADJUST_CHANNEL_FRAME = 25; // 当前帧数大于该数 则在该zone下进入某个map时，优先在当前zone分配某个channel，减少跨zone

        public static int DbTransactionCount;
        public static int DbQueueCount;

        public static int UseDbServerList;
        public static int JewelAdvanceMaxCount;

        public static void InitGameCongfig()
        {
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;

                TrackingLogSwitch = data.GetBoolean("TrackingLogSwitch");

                //GameConfig.OpenServerTime = DateTime.Parse(data.GetString("OpenServerTime"));
                GameConfig.RegionSize = data.GetInt("RegionSize");
                GameConfig.BroadcastLimit = data.GetInt("BroadcastLimit");
                GameConfig.WaitStreamCountHalf = data.GetInt("WaitStreamCountHalf");
                GameConfig.WaitStreamCountQuarter = data.GetInt("WaitStreamCountQuarter");
                GameConfig.WaitStreamCounDisconnect = data.GetInt("WaitStreamCounDisconnect");

                if (data.GetInt("CatchOfflinePlayer") == 1)
                {
                    GameConfig.CatchOfflinePlayer = true;
                }
                else
                {
                    GameConfig.CatchOfflinePlayer = false;
                }
                GameConfig.CatchOfflinePeriod = data.GetInt("CatchOfflinePeriod");

                GameConfig.InitialBagLimit = data.GetInt("InitialBagLimit");
                GameConfig.SpeakerCost = data.GetInt("SpeakerCost");
                GameConfig.EmailItemTimeOut = data.GetInt("EmailItemTimeOut");

                GameConfig.LogServerKey = data.GetString("LogServerKey");

                GameConfig.CreateFamilyCost = data.GetInt("CreateFamilyCost");
                GameConfig.CreateFamilyLevel = data.GetInt("CreateFamilyLevel");
                GameConfig.JoinFamilyLevel = data.GetInt("JoinFamilyLevel");
                GameConfig.FamilyNameLength = data.GetInt("FamilyNameLength");
                GameConfig.FamilyDeclarationLength = data.GetInt("FamilyDeclarationLength");
                GameConfig.FamilyNoticeLength = data.GetInt("FamilyNoticeLength");
                GameConfig.FamilyDefaultDeclartation = data.GetString("FamilyDefaultDeclartation");

                GameConfig.UnlockItemTimeCost = data.GetInt("UnlockItemTimeCost");
                GameConfig.FamilyChatSeconds = data.GetInt("FamilyChatSeconds");
                GameConfig.MapChatSeconds = data.GetInt("MapChatSeconds");
                GameConfig.ItemUnravelEmailId = data.GetInt("ItemUnravelEmailId");

                ACCEPT_DUNGEON_FRAME = data.GetInt("AcceptDungeonFrame");
                ADJUST_CHANNEL_FRAME = data.GetInt("AdJustChannelFrame");

                GameConfig.UseDbServerList = data.GetInt("UseDbServerList");
                GameConfig.JewelAdvanceMaxCount = data.GetInt("JewelAdvanceMaxCount");
                //GameConfig.DbTransactionCount = data.GetInt("DbTransactionCount");
            }
        }

    }

}

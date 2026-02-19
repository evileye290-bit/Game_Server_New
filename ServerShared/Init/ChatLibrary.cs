using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public static class ChatLibrary
    {
        private static Dictionary<int, ChatFrameModel> BubbleDataList = new Dictionary<int, ChatFrameModel>();
        private static Dictionary<int, int> ChatTrumpetList = new Dictionary<int, int>();
        //private static List<int> tipOffTypeList = new List<int>();
        private static Dictionary<string, int> ChatWhiteList = new Dictionary<string, int>();
        //key:level, value:second
        private static Dictionary<int, int> ChatIntervalTimes = new Dictionary<int, int>();

        //public static int WorldIntervalTime { get; set; }
        public static int PersonIntervalTime { get; set; }
        //public static int NearbyIntenvalTime { get; set; }
        public static int FamilyIntervalTime { get; set; }
        public static int CampIntervalTime { get; set; }
        public static int TeamIntervalTime { get; set; }
        public static int RecruitIntervalTime { get; set; }
        public static int SensitiveWordDuration { get; set; }
        public static int SensitiveWordSilenceTime { get; set; }
        public static int SensitiveWordCountLimit { get; set; }

        //public static int GoldTrumpet { get; set; }
        //public static int GoldTrumpetDiamond { get; set; }
        //public static int SilverTrumpet { get; set; }
        //public static int SilverTrumpetDiamond { get; set; }

        public static int ChatWordMaxCount { get; set; }
        public static int ChatParamMaxCount { get; set; }
        public static int ChatTrumpetMaxCount { get; set; }

        public static int TipOffChatRecordEntry { get; set; }
        public static int TipOffDescriptionLimit { get; set; }

        public static int GiftGivingCount { get; set; }
        public static int GiftGivingQuality { get; set; }
        public static string DefaultSilenceReason = string.Empty;
        public static string SensitiveWordSilenceReason = string.Empty;

        public static void BindData()
        {
            BindConfig();
            //BindRoomConfig();
            BindBubbleData();
            BindChatTrumpet();
            //BindAnnounceConfig();
            //BindTipOffType();
            BindChatWhiteContent();
        }

        private static void BindChatTrumpet()
        {
            Dictionary<int, int> ChatTrumpetList = new Dictionary<int, int>();
            DataList gameConfig = DataListManager.inst.GetDataList("ChatTrumpet");
            //ChatTrumpetList.Clear();
            foreach (var item in gameConfig)
            {
                //int itemId = item.Value.GetInt("itemId");
                int itemId = item.Value.ID;
                if (!ChatTrumpetList.ContainsKey(itemId))
                {
                    ChatTrumpetList.Add(itemId, item.Value.ID);
                }
                else
                {
                    Logger.Log.Warn("ChatLibrary BindChatTrumpet has same item id {0}", itemId);
                }
            }
            ChatLibrary.ChatTrumpetList = ChatTrumpetList;
        }

        private static void BindBubbleData()
        {
            Data data;
            ChatFrameModel model;
            DataList gameConfig = DataListManager.inst.GetDataList("ChatBubble");
            //BubbleDataList.Clear();
            Dictionary<int, ChatFrameModel> BubbleDataList = new Dictionary<int, ChatFrameModel>();
            foreach (var item in gameConfig)
            {
                data = item.Value;
                model = new ChatFrameModel()
                {
                    Id = data.ID,
                    //TypeId= data.GetInt("itemId"),
                    TypeId = data.ID,
                    MainType = (MainType)data.GetInt("MainType"),
                    Data = data
                };

                if (!BubbleDataList.ContainsKey(model.TypeId))
                {
                    BubbleDataList.Add(model.TypeId, model);
                }
                else
                {
                    Logger.Log.Warn("ChatLibrary BindChatBubble has same item id {0}", model.Id);//warn
                    //Logger.Log.Error("ChatLibrary BindChatBubble has same item id {0}", model.Id);
                }
            }
            ChatLibrary.BubbleDataList = BubbleDataList;
        }

        //private static void BindRoomConfig()
        //{
        //    ChatConfigInfo info = new ChatConfigInfo();
        //    DataList gameConfig = DataListManager.inst.GetDataList("ChatRoomConfig");
        //    ConfigInfoList.Clear();
        //    foreach (var item in gameConfig)
        //    {
        //        info = new ChatConfigInfo();
        //        Data data = item.Value;
        //        info.Hot = data.GetInt("hot");
        //        info.Full = data.GetInt("full");
        //        info.Stop = data.GetInt("stop");
        //        info.Max = data.GetInt("max");
        //        info.Timespan = data.GetInt("timespan");
        //        ConfigInfoList.Add(data.ID, info);
        //    }
        //}

        private static void BindConfig()
        {
            Dictionary<int, int> ChatIntervalTimes = new Dictionary<int, int>();

            DataList gameConfig = DataListManager.inst.GetDataList("ChatConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;              
                //WorldIntervalTime = data.GetInt("WorldIntervalTime");
                PersonIntervalTime = data.GetInt("PersonIntervalTime");
                //NearbyIntenvalTime = data.GetInt("NearbyIntenvalTime");
                FamilyIntervalTime = data.GetInt("FamilyIntervalTime");
                CampIntervalTime = data.GetInt("CampIntervalTime");
                TeamIntervalTime = data.GetInt("TeamIntervalTime");
                RecruitIntervalTime = data.GetInt("RecruitIntervalTime");
                SensitiveWordDuration = data.GetInt("SensitiveWordDuration");
                SensitiveWordSilenceTime = data.GetInt("SensitiveWordSilenceTime");
                SensitiveWordCountLimit = data.GetInt("SensitiveWordCountLimit");
                //GoldTrumpet = data.GetInt("GoldTrumpet");
                //GoldTrumpetDiamond = data.GetInt("GoldTrumpetDiamond");
                //SilverTrumpet = data.GetInt("SilverTrumpet");
                //SilverTrumpetDiamond = data.GetInt("SilverTrumpetDiamond");

                ChatWordMaxCount = data.GetInt("ChatWordMaxCount");
                ChatParamMaxCount = data.GetInt("ChatParamMaxCount");
                ChatTrumpetMaxCount = data.GetInt("ChatTrumpetMaxCount");

                TipOffChatRecordEntry = data.GetInt("TipOffChatRecordEntry");
                TipOffDescriptionLimit = data.GetInt("TipOffDescriptionLimit");

                string[] chatIntervalTimeArr = data.GetString("ChatIntervalTime").Split('|');
                foreach (var intervalTime in chatIntervalTimeArr)
                {
                    string[] tempArr = intervalTime.Split(':');
                    ChatIntervalTimes.Add(tempArr[0].ToInt(), tempArr[1].ToInt());
                }

                DefaultSilenceReason = data.GetString("DefaultSilenceReason");
                SensitiveWordSilenceReason = data.GetString("SensitiveWordSilenceReason");
            }
            ChatLibrary.ChatIntervalTimes = ChatIntervalTimes;
        }

        private static void BindChatWhiteContent()
        {
            Dictionary<string, int> ChatWhiteList = new Dictionary<string, int>();
            DataList chatWhiteData = DataListManager.inst.GetDataList("ChatWhiteContent");
            foreach (var item in chatWhiteData)
            {
                Data data = item.Value;
                string content = data.GetString("Content");
                ChatWhiteList[content] = 0;
            }
            ChatLibrary.ChatWhiteList = ChatWhiteList;
        }

        //private static void BindAnnounceConfig()
        //{
        //    DataList gameConfig = DataListManager.inst.GetDataList("AnnounceConfig");
        //    foreach (var item in gameConfig)
        //    {
        //        Data data = item.Value;
        //        GiftGivingCount = data.GetInt("GiftGivingCount");
        //        GiftGivingQuality = data.GetInt("GiftGivingQuality");
        //    }
        //}

        //private static void BindBubbleData()
        //{
        //    ChatBubbleData info = new ChatBubbleData();
        //    DataList gameConfig = DataListManager.inst.GetDataList("ChatBubble");
        //    BubbleDataList.Clear();
        //    foreach (var item in gameConfig)
        //    {
        //        info = new ChatBubbleData();
        //        Data data = item.Value;
        //        info.Id = data.ID;
        //        //info.Type = (ChatBubbleType)data.GetInt("type");
        //        info.CurrenciesType = data.GetInt("currency");
        //        info.CurrenciesNum = data.GetInt("price");
        //        info.DisCurrenciesNum = data.GetInt("disprice");
        //        info.Discount = data.GetInt("discount");
        //        info.StartDate = data.GetString("startDate");
        //        info.EndDate = data.GetString("endDate");
        //        info.PassLevel = data.GetInt("passLevel");
        //        BubbleDataList.Add(data.ID, info);
        //    }
        //}

        //public static ChatConfigInfo GetConfigInfo(int id)
        //{
        //    ChatConfigInfo info;
        //    ConfigInfoList.TryGetValue(id, out info);
        //    return info;
        //}

        //public static ChatBubbleData GetBubbleData(int id)
        //{
        //    ChatBubbleData info;
        //    BubbleDataList.TryGetValue(id, out info);
        //    return info;
        //}

        //private static void BindTipOffType()
        //{
        //    tipOffTypeList.Clear();

        //    DataList dataList = DataListManager.inst.GetDataList("TipOffType");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;
        //        tipOffTypeList.Add(data.ID);
        //    }      
        //}

        public static int GetBubbleId(int itemId)
        {
            ChatFrameModel model = GetChatFrameModel(itemId);
            if (model != null)
            {
                return model.Id;
            }
            return 0;
        }

        public static ChatFrameModel GetChatFrameModel(int itemId)
        {
            ChatFrameModel model;
            BubbleDataList.TryGetValue(itemId, out model);
            return model;
        }

        public static int GetTrumpetId(int itemId)
        {
            int trumpetId;
            ChatTrumpetList.TryGetValue(itemId, out trumpetId);
            return trumpetId;
        }

        public static bool CheckIsWhiteContent(string content)
        {
            if (ChatWhiteList.ContainsKey(content))
            {
                return true;
            }
            return false;
        }

        public static int GetChatIntervalTimeByLevel(int level)
        {
            int intervalTime;
            int key = 0;
            foreach (var item in ChatIntervalTimes.Keys)
            {
                if (level >= item && item > key)
                {
                    key = item;
                }
            }
            ChatIntervalTimes.TryGetValue(key, out intervalTime);
            return intervalTime;
        }
        //public static List<int> GetTipOffTypeList()
        //{
        //    return tipOffTypeList;
        //}
    }
}

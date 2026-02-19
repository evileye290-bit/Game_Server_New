using CommonUtility;
using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BarrackServerLib
{
    internal class RecommendReturn
    {
        internal class GiftInfo
        {
            public string gift_id;
            public string score;
            //public long generate_time;
        }

        internal class RecommendGiftInfo
        {
            public int task_type;
            public string scene_id = string.Empty;
            public GiftInfo gift;
            public string data_box = string.Empty;
        }

        public long code;
        public string message;
        public RecommendGiftInfo data;
    }

    internal class RecommendGiftInfo
    {
        public int GiftId { get; set; }
        public float Score { get; set; }
        public string Data_Box = string.Empty;
    }

    internal class GiftRecommendHelper
    {
        private static int game_id = 107162;
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        private static string secretKey = "3a62e204477f462faf40e69d1044e4cb";

        private const int TASK_TYPE_RUN_AWAY = 101;//流失干预
        private const int TASK_TYPE_RECOMMEND_GIFT = 102;//推荐礼包


        public static Task<RecommendGiftInfo> GetRecommendGift(string account, string serverId, string roleId, string sceneId)
        { 
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("task_type", TASK_TYPE_RECOMMEND_GIFT);
            data.Add("game_base_id", game_id);
            data.Add("cp_uid", account);
            data.Add("cp_server_id", serverId);
            data.Add("cp_role_id", roleId);
            data.Add("scene_id", sceneId);
            data.Add("timestamp", Timestamp.GetUnixTimeStampSeconds(DateTime.Now));

            string sign = HttpHelper.Sign_SEA(data, secretKey);
            data.Add("sign", sign);

            return GetRecommendGiftInfo(data);
        }


        private static async Task<RecommendGiftInfo> GetRecommendGiftInfo(Dictionary<string, object> data)
        {
            string answer = string.Empty;
            RecommendGiftInfo info = new RecommendGiftInfo();

            try
            {
                string url = DataListManager.inst.GetData("UrlConfig", 1).GetString("giftSdkUrl");
                answer = await HttpHelper.GetAsync(data, url);

                RecommendReturn msg = serializer.Deserialize<RecommendReturn>(answer);

                Log.Info((object)$"GetRecommendGiftInfo request {serializer.Serialize(data)} response {answer}");
                info.Data_Box = msg.data == null ? "" : msg.data.data_box;

                if (msg.code != 0)
                {
                    Log.Info((object)"got recommend gift fail : had not got code. info {answer}");
                    return info;
                }

                if (msg.data == null || msg.data.gift == null)
                {
                    Log.Info((object)$"got recommend gift fail : had not got gift info. info {answer}");
                    return info;
                }
                info.Score = float.Parse(msg.data.gift.score);
                info.GiftId = int.Parse(msg.data.gift.gift_id);
            }
            catch (Exception ex)
            {
                Log.Info(answer);
                Log.Error(ex);
            }

            return info;
        }


        internal class RunAwayReturnInfo
        {
            internal class InterveneInfo
            {
                public string intervene_id;
                public long generate_time;
            }

            internal class DataInfo
            {
                public int task_type;
                public InterveneInfo intervene;
                public string data_box = string.Empty;
            }

            public long code;
            public string message;
            public DataInfo data;
        }

        public class InterveneResultInfo
        {
                public int task_type;
                public string intervene_id;
                public string data_box = string.Empty;
        }

        public static async Task<InterveneResultInfo> GetRunAwayInfo(string account, string serverId, string roleId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {"task_type", TASK_TYPE_RECOMMEND_GIFT},
                {"game_base_id", game_id},
                {"game_id", game_id},
                {"cp_uid", account},
                {"cp_server_id", serverId},
                {"cp_role_id", roleId},
                {"timestamp", Timestamp.GetUnixTimeStampSeconds(DateTime.Now)}
            };

            string sign = HttpHelper.Sign_SEA(data, secretKey);
            data.Add("sign", sign);

            string answer = string.Empty;
            InterveneResultInfo info = new InterveneResultInfo();

            try
            {
                string url = DataListManager.inst.GetData("UrlConfig", 1).GetString("interveneUrl");
                answer = await HttpHelper.GetAsync(data, url);

                RunAwayReturnInfo msg = serializer.Deserialize<RunAwayReturnInfo>(answer);

                Log.Info((object)$"GetRunAwayInfo request {serializer.Serialize(data)} response {answer}");

                if (msg.code != 0 || msg.data == null)
                {
                    Log.Info((object)"got recommend gift fail : had not got code. info {answer}");
                    return null;
                }

                if (msg.data.intervene == null)
                {
                    Log.Info((object)$"GetRunAwayInfo fail : had not got gift info. info {answer}");
                    return null;
                }

                info.data_box = msg.data.data_box;
                info.task_type = TASK_TYPE_RECOMMEND_GIFT;
                info.intervene_id = msg.data.intervene.intervene_id;
                return info;
            }
            catch (Exception ex)
            {
                Log.Info(answer);
                Log.Error(ex);
                return null;
            }
        }

        public static async Task RunAwayOpened(string interveneId, int state, long openTime)
        {
            long time = Timestamp.GetUnixTimeStampSeconds(DateTime.Now);
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {"task_type", TASK_TYPE_RECOMMEND_GIFT},
                {"game_base_id", game_id},
                {"game_id", game_id},
                {"business_type", 1},
                {"intervene_id", interveneId},
                {"intervene_name", interveneId},
                {"state", state},
                {"create_time", openTime},
                {"update_time ", time},
                {"timestamp", Timestamp.GetUnixTimeStampSeconds(DateTime.Now)},
            };

            string sign = HttpHelper.Sign_SEA(data, secretKey);
            data.Add("sign", sign);

            string answer = string.Empty;

            try
            {
                string url = DataListManager.inst.GetData("UrlConfig", 1).GetString("interveneSync");
                answer = await HttpHelper.PostAsync(serializer.Serialize(data), url);
            }
            catch (Exception ex)
            {
                Log.Info(answer);
                Log.Error(ex);
            }
        }
    }
}

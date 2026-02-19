#define TEXTCHECKER

using CommonUtility;
using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ZoneServerLib
{
    public enum Context163Type
    {
        Chat = 1,
        NickName = 2,
        Other = 5,
    }

    public  class Http163CheckerHelper
    {
        private static readonly object Locker = new object();
        private static HttpClient textCheckClient = null;

        HttpResponseQueue responseQueue;
        public HttpResponseQueue ResponseQueue { get { return responseQueue; } }
        public int CheckOpen = 1;
        string secretId = "90c6e3dc9be84148875b05876cfa729b";
        static string secretKey = "491e5bdab69f9d897ce189952590d68a";
        string businessId = "b19f20f53d3c8233fde8e4a588a39132";
        string textCheckUrl = "https://as.dun.163.com/v4/text/check";

        private ZoneServerApi zoneServerApi;
        static TimeSpan timeout;

        public void InitConfigData()
        {
            Data config = DataListManager.inst.GetData("Http163Check", 1);

            secretId = config.GetString("secretId");
            secretKey = config.GetString("secretKey");
            businessId = config.GetString("businessId");
            textCheckUrl = config.GetString("textCheckUrl");
            CheckOpen = config.GetInt("checkOpen");
            timeout = TimeSpan.FromMilliseconds(config.GetInt("timeout"));
        }

        public void InitHttpClient(ZoneServerApi zoneServerApi)
        {
            this.zoneServerApi = zoneServerApi;

            responseQueue = new HttpResponseQueue();
            responseQueue.Init();

            HttpClientTextCheck.BaseAddress = new Uri(textCheckUrl);
            try
            {
#if TEXTCHECKER
                //帮HttpClient热身
                textCheckClient.SendAsync(new HttpRequestMessage
                {
                    Method = new HttpMethod("HEAD"),
                    RequestUri = new Uri(textCheckUrl + "/")
                })
                  .Result.EnsureSuccessStatusCode();
                //Testttt();
#endif
            }
            catch (Exception ex)
            {
                Log.Warn("Init 163 checker {0} has error: {1}", textCheckUrl, ex);
            }
        }


        //private Dictionary<String, String> GetTextCheckParameters(ContextType type, string context)
        //{
        //    Dictionary<String, String> data = CreateTextCheckParameters(context, type);

        //    string signature = Http163CheckerHelper.GenSignature(data);
        //    data.Add("signature", signature);
        //    return data;
        //}


        //private void Testttt()
        //{
        //    CheckQuery_Text check = new CheckQuery_Text();

        //    var paramsArr = GetTextCheckParameters(ContextType.Chat, "Testttt()");
        //    PostTextAsync(paramsArr, check, () =>
        //    {

        //        return 1;
        //    });
        //}


        /// <summary>
        /// 单例获取
        /// </summary>
        public static HttpClient HttpClientTextCheck
        {
            get
            {
                if (textCheckClient == null)
                {
                    lock (Locker)
                    {
                        if (textCheckClient == null)
                        {
                            textCheckClient = new HttpClient();
                            textCheckClient.Timeout = timeout;
                        }
                    }
                }
                return textCheckClient;
            }
        }


        public Dictionary<String, String> CreateTextCheckParameters(string context, Context163Type contextType)
        {
            Dictionary<String, String> publicParams = new Dictionary<String, String>();

            // 1.设置公共参数
            publicParams.Add("secretId", secretId);
            publicParams.Add("version", "v4");
            publicParams.Add("businessId", businessId);
            publicParams.Add("timestamp", Timestamp.GetUnixTimeStamp(zoneServerApi.Now()).ToString());
            publicParams.Add("nonce", new Random().Next().ToString());

            publicParams.Add("dataId", "test100100001");
            publicParams.Add("content", context);

            publicParams.Add("account", "player.AccountName");
            publicParams.Add("nickname", "player.Name");
            publicParams.Add("extStr1", "1001");
            publicParams.Add("extStr2", "player.ChannelName");
            publicParams.Add("extLon1", "100100001");
            publicParams.Add("extLon2", "1");
            publicParams.Add("ip", "192.168.30.123");
            return publicParams;
        }


        public Dictionary<String, String> CreateTextCheckParameters(PlayerChar player, string context, Context163Type contextType)
        {
            Dictionary<String, String> publicParams = new Dictionary<String, String>();

            publicParams.Add("secretId", secretId);
            publicParams.Add("businessId", businessId);
            publicParams.Add("timestamp", Timestamp.GetUnixTimeStamp(zoneServerApi.Now()).ToString());
            publicParams.Add("nonce", new Random().Next().ToString());

            string dataId = string.Format("{0}_{1}_{2}", player.Uid, zoneServerApi.Now(), context);
            publicParams.Add("dataId", dataId);
            publicParams.Add("content", context);
            publicParams.Add("version", "v4");

            publicParams.Add("account", player.AccountName);
            publicParams.Add("nickname", player.Name);
            publicParams.Add("extStr1", zoneServerApi.MainId.ToString());
            publicParams.Add("extStr2",player.ChannelName);
            publicParams.Add("extLon1", player.Uid.ToString());
            publicParams.Add("extLon2", ((int)contextType).ToString());
            publicParams.Add("ip", player.ClientIp);
            return publicParams;
        }

        // 根据secretKey和parameters生成签名
        public static String GenSignature(Dictionary<String, String> parameters)
        {
            parameters = parameters.OrderBy(o => o.Key, StringComparer.Ordinal).ToDictionary(o => o.Key, p => p.Value);

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<String, String> kv in parameters)
            {
                builder.Append(kv.Key).Append(kv.Value);
            }
            builder.Append(secretKey);
            String tmp = builder.ToString();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(tmp));
            builder.Clear();
            foreach (byte b in result)
            {
                builder.Append(b.ToString("x2").ToLower());
            }
            return builder.ToString();
        }


        public async Task<bool> PostTextAsync(Dictionary<String, String> data, ABoilHttpQuery query, Func<string> callBack)//post异步请求方法
        {
#if TEXTCHECKER
            try
            {
                HttpContent content = new TextFromUrlEncodeContent(data);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await HttpClientTextCheck.PostAsync(textCheckUrl, content);

                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string str = res.Content.ReadAsStringAsync().Result;
                    query.SetResponse(str);
                    responseQueue.SetCallBack(query, callBack);
                    return true;
                }
                else
                {
                    query.SetResponse(null);
                    responseQueue.AddExceptionLog(res.StatusCode.ToString());
                    responseQueue.SetCallBack(query, callBack);
                    return false;
                }
            }
            catch (Exception ex)
            {
                query.SetResponse(null);
                responseQueue.AddExceptionLog(ex.Message);
                responseQueue.SetCallBack(query, callBack);
                return false;
            }
#endif
            return false;
        }


        public bool Exit()
        {
            responseQueue.Exit();
            return true;
        }

        public void Update()
        {
            var queue = responseQueue.GetPostUpdateQueue();
            while (queue.Count != 0)
            {
                try
                {
                    var oprate = queue.Dequeue();
                    oprate.PostUpdate();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }


    }

}

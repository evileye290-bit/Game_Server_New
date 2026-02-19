#define TEXTCHECKER

using CommonUtility;
using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ZoneServerLib
{

    public  class HttpSensitiveCheckerHelper
    {
        private static readonly object Locker = new object();
        private static HttpClient textCheckClient = null;

        HttpResponseQueue responseQueue;
        public HttpResponseQueue ResponseQueue { get { return responseQueue; } }
        public int CheckOpen = 1;
        static string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MTI1NzI1ODU3OTksInBheWxvYWQiOiJ7XCJpZFwiOjAsXCJnYW1lTmFtZVwiOlwi5paX572X5aSn6ZmGLeaWl-elnuWGjeS4tFwiLFwicGFja2FnZVNlcmlhbFwiOlwiZGxkbGRzemwtMjAyMTA0MDhcIixcInNlbnNpdGl2ZUtleVwiOlwiZHN6bC10ZXN0XCIsXCJ2YWxpZERhdGVcIjoxNzEyNTcyNTg1Nzk5fSJ9.Ok92ynK47t7KzpO7Apj1xEn2MFprXillqk0NG8dVtrM";
        string textCheckUrl = "http://8.131.51.119:30901/sensitive/wholeTextCheck";

        string gamename = "游戏名";
        string key = "（密钥）随意文本";
        static string packageSerial = "dldldszl-20210408";
        static TimeSpan timeout;

        private ZoneServerApi zoneServerApi;

        public void InitConfigData()
        {
            Data config = DataListManager.inst.GetData("HttpSensitiveCheck", 1);

            token = config.GetString("token");
            packageSerial = config.GetString("packageSerial");
            textCheckUrl = config.GetString("textCheckUrl");
            CheckOpen = config.GetInt("checkOpen");
            gamename = config.GetString("gamename");
            key = config.GetString("key");
            timeout =TimeSpan.FromMilliseconds(config.GetInt("timeout"));
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
                Log.Warn("Init sensitive checker {0} has error: {1}", textCheckUrl, ex);
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
                            textCheckClient.DefaultRequestHeaders.Add("token",token);
                            textCheckClient.DefaultRequestHeaders.Add("packageSerial", packageSerial);
                            textCheckClient.Timeout = timeout;
                        }
                    }
                }
                return textCheckClient;
            }
        }
        public Dictionary<String, String> CreateTextCheckParameters(PlayerChar player,string context,string channelName,string channelType, string toUid = null)
        {
            Dictionary<String, String> publicParams = new Dictionary<String, String>();

            // 1.设置公共参数
            publicParams.Add("game_name", gamename);
            publicParams.Add("client_serial", player.DeviceId.ToString());
            publicParams.Add("client_time", Timestamp.GetUnixTimeStamp(zoneServerApi.Now()).ToString()); //聊天发生时间
            publicParams.Add("content", context);

            publicParams.Add("#distinct_id", player.Uid.ToString());

            publicParams.Add("channel", channelName);
            publicParams.Add("channel_type", channelType);

            publicParams.Add("user_name", player.AccountName);
            publicParams.Add("user_ip", player.ClientIp);
            publicParams.Add("private_user_id", toUid);
            return publicParams;
        }

        public async Task<bool> PostTextAsync(Dictionary<String, String> data, ABoilHttpQuery query, Func<string> callBack)//post异步请求方法
        {
#if TEXTCHECKER
            try
            {
                string infoStr = query.serializer.Serialize(data);
                HttpContent content = new StringContent(infoStr);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
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

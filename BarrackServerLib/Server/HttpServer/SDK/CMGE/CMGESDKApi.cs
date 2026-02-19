using Logger;
using Message.Barrack.Protocol.BM;
using ServerShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarrackServerLib
{
    // Android

    public class CMGESDKApi : BaseSDK
    {
        private static HttpClient client = new HttpClient();

        //正式
        public static string appId = "78";
        public static string appKey = "cea8f47677ddced8c5b6e1f31524ceb6";
        public static string appSecret = "f72dcf13dffb84b627c64b9f4d9cd9a0";
        public static string loginCheckUrl = "http://u8.cmge.com/user/verifyAccount";


        private static CMGESDKApi singleApi = new CMGESDKApi();
        internal static CMGESDKApi SingleApi => singleApi;

        static CMGESDKApi()
        {
            client.Timeout = new TimeSpan(30000000);
        }

        public static async Task<U8Info> Verify(Client client, string sdkUserId, string sdkToken)
        {
            //Console.Write(" begin verify");
            U8Info info = new U8Info();
            info.Client = client;
            info.SdkToken = sdkToken;
            info.SdkUserId = sdkUserId;
            info.GenerateInfo();
            info = await LoginU8Async(info);
            return info;
        }

        private static async Task<U8Info> LoginU8Async(U8Info info)
        {
            string answer = await PostLoginAsync(info);

#if DEBUG
            Console.WriteLine("login U8 Async with param " + info.Info["userID"] + " " + info.Info["token"] + " " + info.Info["sign"] + " \n" + " ans " + answer);
#endif
            Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(answer);

            bool state = false;
            foreach (var item in dicMsg)
            {
                if (item.Key.Equals("state"))
                {
                    if (item.Value.Equals(1))
                    {
                        state = true;
                    }
                }
            }

            //放入队列
            info.U8Checked = state;
            info.Ans = dicMsg;
            return info;
            //infoQueue.Enqueue(info);
        }

        public static async Task<string> PostLoginAsync(U8Info info)
        {
            HttpContent contents = new FormUrlEncodedContent(info.Info);

            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(loginCheckUrl, contents);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async Task<string> PostLoginAsync(Dictionary<string, string> info)
        {
            HttpContent contents = new FormUrlEncodedContent(info);

            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(loginCheckUrl, contents);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        protected override bool CheckSign(BaseInfo info)
        {
            Dictionary<string, string> dictionary = info.Info;
            if (!dictionary.ContainsKey("sign") || !dictionary.ContainsKey("signType"))
            {
                return false;
            }

            List<string> keys = new List<string>(dictionary.Keys);
            sortParamNames(keys);
            string paramValues = "";
            foreach (string param in keys)
            {//拼接参数值
                if (param == "sign" || param == "signType")
                {
                    continue;
                }
                string paramValue = dictionary[param];
                if (paramValue != null)
                {
                    paramValues = paramValues + param + "=" + paramValue + "&";
                }
            }
            paramValues += appSecret;

            //string md5 = MD5Encode(paramValues);
            //if (md5.Equals(dictionary["sign"]))
            //{
            //    return true;
            //}

            if (dictionary["signType"].Equals("md5") || dictionary["signType"].Equals("MD5"))
            {

                string md5 = EncryptHelper.MD5Encode(paramValues);
#if DEBUG
                Console.WriteLine("param Values " + paramValues + "\n sign " + dictionary["sign"] + " " + md5);
#endif
                if (md5.Equals(dictionary["sign"]))
                {
                    return true;
                }
                else
                {
#if DEBUG
                    Console.WriteLine("md5 error " + paramValues + "\n sign " + dictionary["sign"] + " " + md5);
#endif
                }
            }
            else if (dictionary["signType"].Equals("RSA") || dictionary["signType"].Equals("rsa"))//TODO :RSA 验证
            {
                return true;
                //                string rsa = GetRSAEncrypt2Hex(paramValues);
                //                if (rsa.Equals(dictionary["sign"]))
                //                {
                //                    return true;
                //                }
                //                else
                //                {
                //#if DEBUG
                //                    Console.WriteLine("rsa error " + paramValues + "\n sign " + dictionary["sign"] + " " + rsa);
                //#endif
                //                }
            }

            return false;

        }

        protected override bool CheckOrderInfo(Dictionary<string, string> dicMsg)
        {
            try
            {
                string info = string.Empty;
                foreach (var item in dicMsg)
                {
                    info += $"{item.Key}:{item.Value}|";
                }
                Log.Debug($"passing data {info} to manager");
                string orderIdObj;
                if (dicMsg.TryGetValue("orderID", out orderIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay orderID {info} get failed");
                    return false;
                }

                string moneyObj;
                if (dicMsg.TryGetValue("money", out moneyObj) == false)
                {
                    BasePayServer.LogWarn($"pay money {info} get failed");
                    return false;
                }

                string channelIdObj;
                if (dicMsg.TryGetValue("channelID", out channelIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay channelID {info} get failed");
                    return false;
                }

                //string channelUidObj;
                //if (dicMsg.TryGetValue("userID", out channelUidObj) == false)
                //{
                //    SDKPayServer.LogList[LogType.WARN].Enqueue("pay order " + orderIdObj + " failed: openId obj failed");
                //    return false;
                //}

                string serverIdObj;
                if (dicMsg.TryGetValue("serverID", out serverIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay serverID {info} get failed");
                    return false;
                }

                string uidObj;
                if (dicMsg.TryGetValue("userID", out uidObj) == false)
                {
                    BasePayServer.LogWarn($"pay userID {info} get failed");
                    return false;
                }


                string extrasParams;
                if (dicMsg.TryGetValue("extension", out extrasParams) == false)
                {
                    BasePayServer.LogWarn($"pay extension {info} get failed");
                    return false;
                }

                MSG_BM_RECHARGE_RESULT notify = new MSG_BM_RECHARGE_RESULT();
                notify.OrderInfo = orderIdObj.ToString();
                notify.Money = (float)Convert.ToDouble(moneyObj);
                notify.ChannelId = Convert.ToInt32(channelIdObj);
                notify.ServerId = Convert.ToInt32(serverIdObj); 
                notify.OrderId = Convert.ToInt32(extrasParams);
                notify.PayTime = DateTime.Now.ToString();
                notify.Status = 1;
                //notify.Uid = int.Parse(extrasParams.ToString().Split('_')[0]);
                //notify.ProductId = int.Parse(extrasParams.ToString().Split('_')[2]);
                //uint uidHigh = uint.Parse(extrasParams.ToString().Split('_')[3]);
                //uint uidLow = uint.Parse(extrasParams.ToString().Split('_')[4]);
                //notify.RechargeUid = ExtendClass.GetUInt64(uidHigh, uidLow);
                //notify.gameOrderId = gameOrder;
                AddPayInfo(notify);
                return true;

            }
            catch (Exception e)
            {
                BasePayServer.LogWarn("Pay function error with " + e.ToString());
                return false;
            }
        }

        public static string getQueryString(Dictionary<string, string> dic)
        {
            NameValueCollection req = new NameValueCollection();
            foreach (var pair in dic)
            {
                req.Add(pair.Key, pair.Value);
            }

            string args = "";
            foreach (string key in req.AllKeys)
            {
                args += key + "=" + req[key] + "&";
            }
            args = args.Substring(0, args.Length - 1);
            return args;
        }

        public static string EncodeBase64(string code)
        {
            string encode = "";
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

        static public string decode(string src, string key)
        {
            if (src == null || src.Length == 0)
            {
                return src;
            }

            string pattern = "\\d+";
            MatchCollection results = Regex.Matches(src, pattern);

            ArrayList list = new ArrayList();
            for (int i = 0; i < results.Count; i++)
            {
                try
                {
                    String group = results[i].ToString();
                    list.Add((Object)group);
                }
                catch (Exception e)
                {
                    return src;
                }
            }

            if (list.Count > 0)
            {
                try
                {
                    byte[] data = new byte[list.Count];
                    byte[] keys = System.Text.Encoding.Default.GetBytes(key);

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(Convert.ToInt32(list[i]) - (0xff & Convert.ToInt32(keys[i % keys.Length])));
                    }
                    return System.Text.Encoding.Default.GetString(data);
                }
                catch (Exception e)
                {
                    return src;
                }
            }
            else
            {
                return src;
            }
        }

        public static string encode(string src, string key)
        {
            try
            {
                byte[] data = System.Text.Encoding.Default.GetBytes(src);
                byte[] keys = System.Text.Encoding.Default.GetBytes(key);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    int n = (0xff & data[i]) + (0xff & keys[i % keys.Length]);
                    sb.Append("@" + n);
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return src;
            }
        }
    }
}

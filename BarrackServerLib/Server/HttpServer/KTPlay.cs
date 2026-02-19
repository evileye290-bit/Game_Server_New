using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BarrackServerLib.Server.HttpServer
{
    public class KTPlay
    {
        /// <summary>
        /// 用于连接KTplay的http客户端
        /// </summary>
        private static HttpClient client = new HttpClient();

        //登陆地址
        public static string verifyCheckUrl = "http://api.ktplay.cn/open/rewards/verify";

        public static string app_secret = "ca613508595658af9b1b0c4bb159b580af0dde43";

        public static string app_open_key = "GJly5pHhuncorebNNlaNqMlTsDzWYHJyyjXfKdhJT1BVR8FkJ_Eqa_mygo_Oj9VdvRTQsgAaFvBGgWwgMhLtKUX3hA3L7zNlew1iSp09xes";

        public static int typeId = 0;
        public static JavaScriptSerializer serializer = new JavaScriptSerializer();
        //连接超时
        int connectTimeOut = 3000;

        static KTPlay()
        {
            client.Timeout = new TimeSpan(30000000);
        }


        public async Task<string> PostVerifyAsync(VerifyInfo info)
        {
            //Logger.Log.Write("postLoginStart");
            Dictionary<string, object> tempInfo = info.Info;
            tempInfo.Add("type", typeId);
            tempInfo.Add("app_open_key", app_open_key);
            foreach (var item in tempInfo)
            {
                //Logger.Log.Write("tempInfo {0}:{1}", item.Key, item.Value);
            }
            object uid = null;
            tempInfo.TryGetValue("uid", out uid);
            string realUid =uid.ToString();
            tempInfo["uid"] = realUid;
            Dictionary<string, string> finalInfo = new Dictionary<string, string>();
            finalInfo = ConvertInfo(tempInfo);

            string sign = GetSignForKTValid(finalInfo);

            //Logger.Log.Write("sign is :" + sign);
            string KtSign = sign.Replace("-", "");
            //Logger.Log.Write("KT sign is:"+KtSign);

            string tempSign = null;
            finalInfo.TryGetValue("sign", out tempSign);
            if (tempSign != null)
            {
                finalInfo["sign"] = sign;
            }
            else
            {
                finalInfo.Add("sign", KtSign);
            }

            foreach (var item in finalInfo)
            {
                //Console.WriteLine("final info key {0}: value {1}", item.Key, item.Value);
            }
            HttpContent contents = new FormUrlEncodedContent(finalInfo);

            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(verifyCheckUrl, contents);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public static Dictionary<string, string> ConvertInfo(Dictionary<string, object> dic)
        {
            Dictionary<string, string> dicMsg = new Dictionary<string, string>();

            List<string> ps = new List<string>();
            foreach (var pair in dic)
            {
                ps.Add(pair.Key);
            }

            foreach (string param in ps)
            {//拼接参数值

                object paramValue = dic[param];
                if (paramValue != null)
                {
                    if (paramValue.GetType() == "".GetType())
                    {
                        Console.WriteLine("convertInfo string " + paramValue);
                        dicMsg.Add(param, paramValue.ToString());
                    }
                    else
                    {
                        Console.WriteLine("convertInfo not string " + serializer.Serialize(paramValue));
                        dicMsg.Add(param, serializer.Serialize(paramValue));
                    }

                }
            }

            return dicMsg;
        }

        public static string GetSignForKTValid(Dictionary<string, object> data)
        {
            List<string> ps = new List<string>();
            foreach (var pair in data)
            {
                ps.Add(pair.Key);
            }

            sortParamNames(ps);// 将参数名从小到大排序，结果如：adfd,bcdr,bff,zx

            string paramValues = "";
            foreach (string param in ps)
            {//拼接参数值
                if (param == "sign" || param == "app_open_key")
                {
                    continue;
                }
                object paramValue = data[param];
                if (paramValue != null)
                {
                    if (paramValue.GetType() != "".GetType())
                    {
                        Console.WriteLine("not string " + paramValue);
                        paramValues += param + "%26" + serializer.Serialize(paramValue);
                    }
                    else
                    {
                        Console.WriteLine("string " + paramValue);
                        paramValues += param + "%26" + paramValue;
                    }
                }
            }
            string md5Values = MD5Encode(paramValues + "%26" + app_secret);
            //md5Values = MD5Encode(md5Values.ToLower() + ConfigurationManager.AppSettings["AnySDK_Key"].ToString());
            return md5Values;
        }

        public static string GetSignForKTValid(Dictionary<string, string> data)
        {
            List<string> ps = new List<string>();
            foreach (var pair in data)
            {
                ps.Add(pair.Key);
            }

            sortParamNames(ps);// 将参数名从小到大排序，结果如：adfd,bcdr,bff,zx

            string paramValues = "";
            foreach (string param in ps)
            {//拼接参数值
                if (param == "sign" || param == "app_open_key")
                {
                    continue;
                }
                object paramValue = data[param];
                if (paramValue != null)
                {

                    //Console.WriteLine("GetSignForKTValid string,string     string " + paramValue);
                    paramValues += param + "%3D" + paramValue+"%26";

                }
            }
            string md5Values = MD5Encode(paramValues+ app_secret);
            //md5Values = MD5Encode(md5Values.ToLower() + ConfigurationManager.AppSettings["AnySDK_Key"].ToString());
            return md5Values;
        }
        //MD5编码
        public static string MD5Encode(string sourceStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src, 0, src.Length);
            return BitConverter.ToString(res);
        }
        //将参数名从小到大排序，结果如：adfd,bcdr,bff,zx
        public static void sortParamNames(List<string> paramNames)
        {
            paramNames.Sort((string str1, string str2) => { return str1.CompareTo(str2); });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BarrackServerLib.Server.HttpServer
{
    class AnySDK
    {
        private static HttpClient client = new HttpClient();
        private static HttpClient payClient = new HttpClient();

        //登陆地址
        string loginCheckUrl = "http://pay.51wanxin.cn/api/User/LoginOauth/";

        //连接超时
        int connectTimeOut = 3000;

        static AnySDK()
        {
            client.Timeout = new TimeSpan(30000000);
        }
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    string strMsg = "";
        //    try
        //    {
        //        strMsg = postLogin();

        //        JavaScriptSerializer serializer = new JavaScriptSerializer();
        //        Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(strMsg);
        //        if (dicMsg["status"].ToString() == "ok")
        //        {
        //            //这里可以做数据验证等其他操作
        //            //Dictionary<string, object> common = (Dictionary<string, object>)rets["common"];
        //            dicMsg["ext"] = "test";
        //            strMsg = serializer.Serialize(dicMsg);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        strMsg = ex.ToString();
        //    }
        //    Response.Write(strMsg);
        //}

        //public string postLogin(baseInfo info)
        //{
        //    HttpWebRequest requester = WebRequest.Create(new Uri(loginCheckUrl)) as HttpWebRequest;
        //    requester.Method = "POST";
        //    requester.Timeout = connectTimeOut;
        //    requester.ContentType = "application/x-www-form-urlencoded";
        //    byte[] bs = Encoding.UTF8.GetBytes(getQueryString(info.Info));
        //    requester.ContentLength = bs.Length;
        //    using (Stream reqStream = requester.GetRequestStream())
        //    {
        //        reqStream.Write(bs, 0, bs.Length);
        //    }

        //    HttpWebResponse responser = requester.GetResponse() as HttpWebResponse;
        //    using (StreamReader reader = new StreamReader(responser.GetResponseStream(), Encoding.UTF8))
        //    {
        //        string answer=reader.ReadToEnd();
        //        return answer;
        //    }
        //}
        public async Task<string> PostLoginAsync(baseInfo info)
        {

            HttpContent contents = new FormUrlEncodedContent(info.Info);
            
            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(loginCheckUrl, contents);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public string getQueryString(Dictionary<string, string> dic)
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

        //public static string Pay(baseInfo info)
        //{
        //    string strMsg = "ok";
        //    try
        //    {

        //        Dictionary<string, object> data = new Dictionary<string, object>();
        //        foreach (var pair in info.Info)
        //        {
        //            data.Add(pair.Key, pair.Value);
        //        }
        //        if (data["sign"].ToString() == getSignForAnyValid(info.Info) && data["pay_status"].ToString() == "1")
        //        {
        //            //支付成功的处理
        //            //注意判断金额，客户端可能被修改从而使用低金额购买高价值道具
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        strMsg = ex.ToString();
        //    }

        //    //Response.Write(strMsg);
        //    return strMsg;
        //}

        public static string GetSignForAnyValid(Dictionary<string, string> data)
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
                if (param == "sign")
                {
                    continue;
                }
                string paramValue = data[param];
                if (paramValue != null)
                {
                    paramValues += paramValue;
                }
            }
            string md5Values = MD5Encode(paramValues);
            md5Values = MD5Encode(md5Values.ToLower() + ConfigurationManager.AppSettings["AnySDK_Key"].ToString()).ToLower();
            return md5Values;
        }
        //MD5编码
        public static string MD5Encode(string sourceStr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src, 0, src.Length);
            return BitConverter.ToString(res).ToLower().Replace("-", "");
        }
        //将参数名从小到大排序，结果如：adfd,bcdr,bff,zx
        public static void sortParamNames(List<string> paramNames)
        {
            paramNames.Sort((string str1, string str2) => { return str1.CompareTo(str2); });
        }
    }
}

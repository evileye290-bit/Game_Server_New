using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GlobalServerLib
{
    public class SelectSession : AHttpSession
    {
        public SelectSession(SessionType sessionType) : base(sessionType)
        {
        }

        public static string ObjectToJson(object data)
        {
            //实例化JavaScriptSerializer类的新实例
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Serialize(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static T ConvertObjectByJson<T>(object asObject) where T : new()
        {
            var serializer = new JavaScriptSerializer();
            //将object对象转换为json字符
            var json = serializer.Serialize(asObject);
            //将json字符转换为实体对象
            var t = serializer.Deserialize<T>(json);
            return t;
        }

        public string MD5Encode(string dataStr,string key)
        {
            String sourceStr = string.Format("{0}{1}", dataStr,key);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] src = Encoding.UTF8.GetBytes(sourceStr);
            byte[] res = md5.ComputeHash(src);
            return BitConverter.ToString(res).Replace("-", "").ToLower();
        }

        internal override bool CheckToken(string key)
        {
            object dataObject;
            if (!Dic.TryGetValue("data",out dataObject))
            {
                //错误
                return false;
            }

            object signObject;
            if (!Dic.TryGetValue("sign", out signObject))
            {
                //错误
                return false;
            }

            string signStr = signObject.ToString();
            string dataStr = ObjectToJson(dataObject);

            string tempSign = MD5Encode(dataStr, HttpCommondHelper.selectKey);
            if (signStr != tempSign)
            {
                //错误
                return false;
            }
            return true;
        }

        internal override string GetSessionKey()
        {
            return HttpCommondHelper.selectKey;
        }

        internal void SetData(HttpListenerContext context, Dictionary<string, object> dic)
        {
            this.dic = dic;
            this.context = context;

            object apiName;
            if (dic.TryGetValue("apiName",out apiName))
            {
                cmd = apiName.ToString();
            }
            else
            {
                cmd = "unknownApi";
            }
        }
    }
}

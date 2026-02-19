using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using DataProperty;

namespace PayServerLib
{
    public class VMallHelper
    {
        public static float Version = 1.0f;
        public static string Key = "65987456123645987546213654987235";
        //毫秒
        public static int Timeout = 30000;

        public static void InitConfigData()
        {
            Data config = DataListManager.inst.GetData("HttpVMall", 1);

            Key = config.GetString("key");
            Version = config.GetFloat("version");
            Timeout = config.GetInt("timeout");
        }

        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        public static string JsonSerialize(object srcObject)
        {
            return serializer.Serialize(srcObject);
        }

        public static Dictionary<string, object> JsonToDictionary(string jsonData)
        {
            //实例化JavaScriptSerializer类的新实例
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                //将指定的 JSON 字符串转换为 Dictionary<string, object> 类型的对象
                return jss.Deserialize<Dictionary<string, object>>(jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public static void WriteResponse(HttpListenerContext context, object obj)
        {
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(JsonSerialize(obj));
            }
            context.Response.Close();
        }

    }
}
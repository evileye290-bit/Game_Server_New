using NLog;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ServerLogger
{
    public class KomoeLogManager
    {
        //实例化JavaScriptSerializer类的新实例
        private static JavaScriptSerializer jss = new JavaScriptSerializer();

        public static void SetServerId(int serverId)
        {
            KomoeLogEvent.SetServerId(serverId);
        }

        public static void EventWrite(Dictionary<string, object> dic)
        {
            string msgString = DictionaryToJson(dic);
            KomoeLogEvent.Trace(msgString);
        }

        public static void UserWrite(Dictionary<string, object> dic)
        {
            string msgString = DictionaryToJson(dic);
            KomoeLogUser.Trace(msgString);
        }

        public static void UserDimension(Dictionary<string, object> dic)
        {
            string msgString = DictionaryToJson(dic);
            KomoeLogDimension.Trace(msgString);
        }

        /// <summary>
        /// 将json数据反序列化为Dictionary
        /// </summary>
        /// <param name="jsonData">json数据</param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDictionary(string jsonData)
        {
            //实例化JavaScriptSerializer类的新实例
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
        /// <summary>
        /// 将Dictionary序列化为json数据
        /// </summary>
        /// <param name="jsonData">json数据</param>
        /// <returns></returns>
        public static string DictionaryToJson(Dictionary<string, object> dic)
        {
            try
            {
                //将指定的 JSON 字符串转换为 Dictionary<string, object> 类型的对象
                return jss.Serialize(dic);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }       
    }
    /// <summary>
    /// nLog使用帮助类
    /// </summary>
    public class KomoeLogEvent
    {
        /// <summary>
        /// 实例化nLog，即为获取配置文件相关信息(获取以当前正在初始化的类命名的记录器)
        /// </summary>
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private static KomoeLogEvent _obj;

        public static KomoeLogEvent _
        {
            get { return _obj ?? (new KomoeLogEvent()); }
            set { _obj = value; }
        }

        public static void SetServerId(int serverId)
        {
            _logger.Properties["ServerId"] = serverId.ToString();
        }

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }
    }
    /// <summary>
    /// nLog使用帮助类
    /// </summary>
    public class KomoeLogUser
    {
        /// <summary>
        /// 实例化nLog，即为获取配置文件相关信息(获取以当前正在初始化的类命名的记录器)
        /// </summary>
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private static KomoeLogUser _obj;

        public static KomoeLogUser _
        {
            get { return _obj ?? (new KomoeLogUser()); }
            set { _obj = value; }
        }

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }
    }

    /// <summary>
    /// nLog使用帮助类
    /// </summary>
    public class KomoeLogDimension
    {
        /// <summary>
        /// 实例化nLog，即为获取配置文件相关信息(获取以当前正在初始化的类命名的记录器)
        /// </summary>
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private static KomoeLogDimension _obj;

        public static KomoeLogDimension _
        {
            get { return _obj ?? (new KomoeLogDimension()); }
            set { _obj = value; }
        }

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }
    }
}

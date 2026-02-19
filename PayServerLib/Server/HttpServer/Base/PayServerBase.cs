using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using Logger;
using PayServerLib.Server.SDK;
using ServerShared;

namespace PayServerLib
{
    public abstract class PayServerBase
    {
        private string payAddress = "http://+:8889/";

        protected PayServerApi server;
        protected static HttpListener PaySocket = null;
        protected ServerState state = ServerState.Stopped;

        public static JavaScriptSerializer serializer = new JavaScriptSerializer();


        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        public PayServerBase(string port)
        {
            //此处需要初始化payAddress
            payAddress = "http://+:" + port + "/";
            Log.WriteLine("PayServer initialized with PayAddress=" + payAddress);

            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        public virtual void Init(PayServerApi server)
        {
            this.server = server;
        }

        public void NotifyInitDone()
        {
            PaySocket = new HttpListener();
            PaySocket.Prefixes.Add(payAddress);
            PaySocket.Start();
            PaySocket.BeginGetContext(GetPayCallBack, PaySocket);
            state = ServerState.Started;
        }


        protected abstract void GetPayCallBack(IAsyncResult ar);


        protected virtual bool TryParseData(string str, PayMessage payMessage)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            str = str.Replace("/?", "");

            string[] kv = str.Split('&');
            for (int i = 0; i < kv.Length; i++)
            {
                string[] strPrams = kv[i].Split('=');
                if (strPrams.Length == 2)
                {
                    dic.Add(strPrams[0], strPrams[1]);
                }
            }

            payMessage.Info = dic;

            return false;
        }

        protected bool IsPlatformIOS(string channelName)
        {
            return channelName.Equals(SEASDKApi.IOSChannelId);
        }


        #region  static

        public static void ResponseMessage(HttpListenerContext context, string message)
        {
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(message);
            }
        }

        #endregion

        #region Log

        public static void LogWarn(string info)
        {
            lock (LogList[LogType.WARN])
            {
                LogList[LogType.WARN].Enqueue(info);
            }
        }

        public static void LogError(string info)
        {
            lock (LogList[LogType.ERROR])
            {
                LogList[LogType.ERROR].Enqueue(info);
            }
        }

        public void Update()
        {
            lock (LogList)
            {
                while (LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.INFO].Dequeue();
                        Log.Info(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }

                while (LogList[LogType.WARN].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.WARN].Dequeue();
                        Log.Alert(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }

                while (LogList[LogType.ERROR].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.ERROR].Dequeue();
                        Log.Error(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }

        #endregion
    }
}

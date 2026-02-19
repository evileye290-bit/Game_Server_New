using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Logger;
using ServerShared;

namespace PayServerLib
{
    public class VMallServer
    {
        private static HttpListener vMallSocket = null;
        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        private PayServerApi server;
        private VMallApiHandler vMallApiHandler;
        private ServerState state = ServerState.Stopped;
        private readonly string listenAddress = "http://+:8890/";

        private VMSessionManager vmSessionManager;

        public DateTime Now
        {
            get { return server.Now(); }
        }

        public VMallServer(string port)
        {
            vmSessionManager = new VMSessionManager(this);
            listenAddress = "http://+:" + port + "/";
            Log.WriteLine("VMallServer initialized with ListenAddress=" + listenAddress);

            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        public void Init(PayServerApi server)
        {
            this.server = server;
            vMallApiHandler = new VMallApiHandler(server);
            SEASDKApi_Web.Instance.Init(server);
        }

        public void NotifyInitDone()
        {
            vMallSocket = new HttpListener();
            vMallSocket.Prefixes.Add(listenAddress);
            vMallSocket.Start();
            vMallSocket.BeginGetContext(GetPayCallBack, vMallSocket);
            state = ServerState.Started;
        }

        void GetPayCallBack(IAsyncResult ar)
        {
            try
            {
                vMallSocket = ar.AsyncState as HttpListener;
                HttpListenerContext context = vMallSocket.EndGetContext(ar);

                vMallSocket.BeginGetContext(GetPayCallBack, vMallSocket);

                string message = string.Empty;
                VMallSession session;
                VMallErrorCode errorCode = TryParse(context, out message, out session);

                if (errorCode == VMallErrorCode.Success)
                {
                    session.BindContext(context);
                    lock (LogList[LogType.INFO])
                    {
                        LogList[LogType.INFO].Enqueue($"got http request with  {context.Request.RawUrl}");
                    }
                }
                else
                {
                    object result = VMResponse.GetFail(errorCode, message);
                    VMallHelper.WriteResponse(context, result);
                }
            }
            catch (Exception e)
            {
                lock (LogList[LogType.WARN])
                {
                    LogList[LogType.WARN].Enqueue("VMallServer Exception " + e.ToString());
                }
            }
        }

        public VMallErrorCode TryParse(HttpListenerContext context, out string errorMessage, out VMallSession session)
        {
            errorMessage = "success";
            session = null;
            var rawUrl = string.Empty;
            try
            {
                rawUrl = context.Request.RawUrl;
                Dictionary<string, object> dic = new Dictionary<string, object>();


                string apiName = string.Empty;
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        apiName = "queryServerList";
                        string[] arr = rawUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        dic = TryParseData(arr[0]);
                        break;
                    case "POST":
                        apiName = "recharge";

                        StreamReader reader = new StreamReader(context.Request.InputStream);
                        string requestStr = reader.ReadToEnd();
                        reader.Close();
                        dic = TryParseData(requestStr);
                        break;
                    default:
                        errorMessage = $"TryParse error, type {context.Request.HttpMethod} not find";
                        return VMallErrorCode.OtherError;
                }

                if (!SEASDKApi_Web.CheckSign(dic))
                {
                    LogList[LogType.WARN].Enqueue($"TryParse error, type {context.Request.HttpMethod} info {rawUrl}");
                    errorMessage = $"GET sign is error: {rawUrl}";
                    return VMallErrorCode.NoAccount;
                }
                Dictionary<string, object> header= new Dictionary<string, object>();
                if (context.Request.Headers.HasKeys())
                {
                    foreach (var key in context.Request.Headers.AllKeys)
                    {
                        header.Add(key, context.Request.Headers.Get(key));
                    }
                }

                session = vmSessionManager.CreateSession(apiName, dic, header);
                return VMallErrorCode.Success;
            }
            catch (Exception ex)
            {
                LogList[LogType.WARN].Enqueue($"TryParse error, type {context.Request.HttpMethod} info {rawUrl} : {ex}");
                return VMallErrorCode.OtherError;
            }
        }

        protected Dictionary<string, object> TryParseData(string str)
        {
            str = str.Replace("?", "");

            string[] paramSplit = str.Split('&');
            Dictionary<string, object> originalInfo = new Dictionary<string, object>();
            foreach (var param in paramSplit)
            {
                string[] split = param.Split('=');
                if (split.Length != 2)
                {
                    LogList[LogType.WARN].Enqueue($"TryParseData 2 error, info {str}");
                    continue;
                }

                originalInfo.Add(split[0], split[1]);
            }

            //Dictionary<string, object> originalInfo = BaseSDK.serializer.Deserialize<Dictionary<string, object>>(str);
            //if (originalInfo == null)
            //{
            //    LogList[LogType.WARN].Enqueue($"TryParseData 4 error, info {str}");
            //    return new Dictionary<string, object>();
            //}

            //baseInfo.OriginalInfo = originalInfo;

            //Dictionary<string, string> signData = new Dictionary<string, string>();
            //foreach (var kv in baseInfo.OriginalInfo)
            //{
            //    signData.Add(kv.Key, kv.Value.ToString());
            //}

            //baseInfo.Info = signData;

            return originalInfo;
        }

        private bool CheckVersion(object version)
        {
            float realVersion;
            float.TryParse(version.ToString(), out realVersion);

            return realVersion == VMallHelper.Version;
            //return string.Equals(VMallHelper.Version.ToString().Trim(),version);
        }

        private bool CheckApiName(string apiName)
        {
            return VMallApiHandler.CheckApiName(apiName);
        }


        public string GetSign(string jsonData)
        {
            //string sign = String.Empty;
            //string dataStr = VMallHelper.JsonSerialize(jsonData);
            string md5 = EncryptHelper.MD5Encode($"{jsonData}{VMallHelper.Key}");
            return md5;
        }

        public void Update()
        {
            vmSessionManager.Update();

            LogWrite();
        }


        public void DistributeMessage(VMallSession session)
        {
            vMallApiHandler.DistributeMessage(session);
        }


        public VMallSession GetCacheHttpSession(int sessionUid)
        {
            return vmSessionManager.GetCacheHttpSession(sessionUid);
        }

        private void LogWrite()
        {
            lock (LogList)
            {
                while (LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.INFO].Dequeue();
                        Log.Info((object)log);
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
    }
}

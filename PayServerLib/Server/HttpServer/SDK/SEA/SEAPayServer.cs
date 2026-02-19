using Logger;
using PayServerLib.Server.SDK;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PayServerLib
{
    public class SEAPayServer : PayServerBase
    {
        private readonly Dictionary<int, Action<PayMessage>> sdkPayCalls = new Dictionary<int, Action<PayMessage>>()
        {
            { SEASDKApi_Web.Instance.GetAppId(), SEASDKApi_Web.Instance.Pay},
            { SEASDKApi_IOS.Instance.GetAppId(), SEASDKApi_IOS.Instance.Pay},
            { SEASDKApi_Android.Instance.GetAppId(), SEASDKApi_Android.Instance.Pay},
            { SEASDKApi_Huawei.Instance.GetAppId(), SEASDKApi_Huawei.Instance.Pay},
        };

        public SEAPayServer(string port) : base(port)
        {
        }

        public override void Init(PayServerApi server)
        {
            base.Init(server);
            SEASDKApi_IOS.Instance.Init(server);
            SEASDKApi_Android.Instance.Init(server);
            SEASDKApi_Huawei.Instance.Init(server);
        }


        protected  override void GetPayCallBack(IAsyncResult ar)
        {
            try
            {
                PaySocket = ar.AsyncState as HttpListener;
                HttpListenerContext context = PaySocket.EndGetContext(ar);

                PaySocket.BeginGetContext(GetPayCallBack, PaySocket);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                StreamReader reader = new StreamReader(context.Request.InputStream);
                string request = reader.ReadToEnd();
                reader.Close();

                PayMessage tempBaseInfo = new PayMessage();
                tempBaseInfo.Info = dic;
                tempBaseInfo.Data = request;
                tempBaseInfo.Context = context;

                string url = System.Web.HttpUtility.UrlDecode(context.Request.RawUrl, Encoding.UTF8);

#if DEBUG
                Log.Info((object)$"got pay with request url {context.Request.RawUrl} data {request}");
#endif

                string decodeStr = url;
                if (context.Request.HttpMethod == "POST")
                {
                    decodeStr = System.Web.HttpUtility.UrlDecode(request, Encoding.UTF8);
#if DEBUG
                    Log.Info((object)$"decode str ---------------------{decodeStr}");
#endif
                }

                if (!TryParseData(decodeStr, tempBaseInfo))
                {
#if DEBUG
                    Log.Info((object)$"parse data error---------------------{decodeStr}");
#endif
                    ResponseMessage(context, "fail");
                    return;
                }

                Log.Info((object)$"parse data success! {decodeStr}");

                string gameIdStr;
                tempBaseInfo.Info.TryGetValue("game_id", out gameIdStr);

                int gameId;
                int.TryParse(gameIdStr, out gameId);

                bool isIos;
                ParseExtendInfo(tempBaseInfo, out isIos);

                Action<PayMessage> action;
                if (sdkPayCalls.TryGetValue(gameId, out action))
                {
                    action.Invoke(tempBaseInfo);
                }
                else
                {
                    if (isIos)
                    {
                        SEASDKApi_IOS.Instance.Pay(tempBaseInfo);
                    }
                    else
                    {
                        SEASDKApi_Android.Instance.Pay(tempBaseInfo);
                    }
                }
            }
            catch (Exception e)
            {
                lock (LogList[LogType.WARN])
                {
                    LogList[LogType.WARN].Enqueue("pay with Exception " + e.ToString());
                }
            }
        }

        protected override bool TryParseData(string str, PayMessage payMessage)
        {
            str = str.Replace("?", "");

            string[] paramSplit = str.Split('&');
            if (paramSplit.Length != 1)
            {
                LogError($"TryParseData 1 error, info {str}");
                return false;
            }

            string[] split = paramSplit[0].Split('=');
            if (split.Length != 2)
            {
                LogError($"TryParseData 2 error, info {str}");
                return false;
            }

            if (!split[0].Equals("data"))
            {
                LogError($"TryParseData 3 error, info {str}");
                return false;
            }

            Dictionary<string, object> originalInfo = PaySDKBase.Serializer.Deserialize<Dictionary<string, object>>(split[1]);
            if (originalInfo == null)
            {
                LogError($"TryParseData 4 error, info {str}");
                return false;
            }

            payMessage.OriginalInfo = originalInfo;

            Dictionary<string, string> signData = new Dictionary<string, string>();
            foreach (var kv in payMessage.OriginalInfo)
            {
                signData.Add(kv.Key, kv.Value.ToString());
            }

            payMessage.Info = signData;

            return true;
        }

        private void ParseExtendInfo(PayMessage payMessage, out bool isIos)
        {
            isIos = false;
            string extendStr;
            if (payMessage.Info.TryGetValue("extension_info", out extendStr) && !string.IsNullOrEmpty(extendStr))
            {
                string[] extendParams = extendStr.Split('_');
                if (extendParams.Length >= 2)
                {
                    isIos = IsPlatformIOS(extendParams[1]);

                    if (extendParams.Length > 2)
                    {
                        string serverId = extendParams[2];
                        int id;
                        if (int.TryParse(serverId, out id))
                        {
                            payMessage.ServerId = id;
                        }
                    }
                }
            }
        }
    }
}

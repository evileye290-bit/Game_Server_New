using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BarrackServerLib
{
    public class CmgePayServer : BasePayServer
    {
        public CmgePayServer(string port):base(port)
        {
        }

        public override void Init(BarrackServerApi server)
        {
            base.Init(server);

            CMGESDKApi.SingleApi.Init(server);
            CMGESDKIOSApi.SingleApi.Init(server);
        }

        protected override void GetPayCallBack(IAsyncResult ar)
        {
            try
            {
                PaySocket = ar.AsyncState as HttpListener;
                HttpListenerContext context = PaySocket.EndGetContext(ar);

                PaySocket.BeginGetContext(new AsyncCallback(GetPayCallBack), PaySocket);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                StreamReader reader = new StreamReader(context.Request.InputStream);
                string request = reader.ReadToEnd();
                reader.Close();

                BaseInfo tempBaseInfo = new BaseInfo();
                tempBaseInfo.Info = dic;
                tempBaseInfo.Data = request;
                tempBaseInfo.Context = context;

                string url = System.Web.HttpUtility.UrlDecode(context.Request.RawUrl, Encoding.UTF8);

                Log.Debug("got pay with request url {0} data {1}", context.Request.RawUrl, context.Request.InputStream);

                Dictionary<string, string> queryDic = new Dictionary<string, string>();

                string decodeStr = url;
                if (context.Request.HttpMethod == "POST")
                {
                    decodeStr = System.Web.HttpUtility.UrlDecode(request, Encoding.UTF8);
                }

                TryParseData(decodeStr, tempBaseInfo);

                if (IsIosPayCallBack(tempBaseInfo))
                {
                    tempBaseInfo.Info = queryDic;
                    tempBaseInfo.Data = decodeStr;

                    CMGESDKIOSApi.SingleApi.Pay(tempBaseInfo);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(tempBaseInfo.Data))
                    {
                        TryParseData(tempBaseInfo.Data, tempBaseInfo);

                        CMGESDKApi.SingleApi.Pay(tempBaseInfo);
                    }
                    else
                    {
                        BaseSDK.ResponseMessage(context, "error");
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

        protected override bool TryParseData(string str, BaseInfo baseInfo)
        {
            Dictionary<string, object> dicMsg = BaseSDK.serializer.Deserialize<Dictionary<string, object>>(str);
            Dictionary<string, string> signData = new Dictionary<string, string>();
            foreach (var kv in dicMsg)
            {
                Console.WriteLine($"add kv {kv.Key} {kv.Value}");
                if (kv.Key == "data")
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)kv.Value;

                    foreach (var temp in dic)
                    {
                        signData.Add(temp.Key, temp.Value.ToString());
                        Console.WriteLine($"add kv {temp.Key} {temp.Value}");
                    }
                }
            }

            baseInfo.Info = signData;
            baseInfo.OriginalInfo = dicMsg;

            return true;
        }
    }
}

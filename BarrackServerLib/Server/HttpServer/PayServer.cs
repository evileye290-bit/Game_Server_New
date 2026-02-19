using DataProperty;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BarrackServerLib.Server.HttpServer
{
    public class PayServer
    {
        private BarrackServerApi server;

        private ServerState state = ServerState.Stopped;
        public ServerState State
        { get { return state; } }

        static HttpListener PaySocket = null;
        static AnySDKApi api = AnySDKApi.SingleApi;

        internal static AnySDKApi Api
        {
            get { return PayServer.api; }
            set { PayServer.api = value; }
        }

        private string payAddress = "http://+:8889/";

        public string PayAddress
        {
            get { return payAddress; }
            set { payAddress = value; }
        }

        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();
        public PayServer(string ip,string port)
        {
            //此处需要初始化payAddress
            PayAddress = "http://+:" + port + "/";
            Logger.Log.WriteLine("PayServer initialized with PayAddress=" + PayAddress);

            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        public void Init(BarrackServerApi server)
        {
            
            this.server = server;
        }
        public void NotifyInitDone()
        {
            //ThreadStart startDispose = new ThreadStart(AnySDKApi.RunPayAfterInfo);
            //Thread threadDispose = new Thread(startDispose);
            //threadDispose.Start();

            PaySocket = new HttpListener();
            PaySocket.Prefixes.Add(PayAddress);
            PaySocket.Start();
            PaySocket.BeginGetContext(new AsyncCallback(GetPayCallBack), PaySocket);
            state = ServerState.Started;
        }

        static void GetPayCallBack(IAsyncResult ar)
        {
            try
            {
                PaySocket = ar.AsyncState as HttpListener;
                HttpListenerContext context = PaySocket.EndGetContext(ar);

                PaySocket.BeginGetContext(new AsyncCallback(GetPayCallBack), PaySocket);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                if (context.Request.HttpMethod == "POST")
                {
                    //Console.WriteLine("post");
                    StreamReader reader = new StreamReader(context.Request.InputStream);
                    string request = reader.ReadToEnd();

                    string[] requests = request.Split('&');
                    foreach (string pair in requests)
                    {
                        string key = pair.Split('=')[0];
                        string value = pair.Split('=')[1];
                        dic.Add(key, value);
                    }
                    reader.Close();
                }
                else
                {
                    //Console.WriteLine("get");
                    NameValueCollection pairs = HttpUtility.ParseQueryString(context.Request.Url.Query, Encoding.UTF8);
                    int count = context.Request.QueryString.Count;

                    foreach (var Item in context.Request.QueryString)
                    {
                        dic.Add(Item.ToString(), pairs[Item.ToString()]);
                    }
                }

                baseInfo tempBaseInfo = new baseInfo();
                tempBaseInfo.Info = dic;
                tempBaseInfo.Context = context;
                //lock (api.InfoBefore)
                //{
                //    api.InfoBefore.Enqueue(tempBaseInfo);
                //}
                AnySDKApi.Pay(tempBaseInfo);
                //Console.WriteLine(context.Request.Url.PathAndQuery);
                //Console.WriteLine("pay 处理结束");
                //Logger.Log.Write("pay 处理结束");
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                lock (PayServer.LogList[LogType.WARN])
                {
                    PayServer.LogList[LogType.WARN].Enqueue("pay with Exception " + e.ToString());
                }
            }
            finally
            {
                
            }
        }

    }
}

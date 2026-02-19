using ServerShared;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace BarrackServerLib.Server.HttpServer
{
    public class KTPlayServer
    {
        private BarrackServerApi server;

        private ServerState state = ServerState.Stopped;
        public ServerState State
        { get { return state; } }

        /// <summary>
        /// 提供给客户端的http连接
        /// </summary>
        static HttpListener VerifySocket = null;

        static KTPlayApi api = KTPlayApi.SingleApi;
        internal static KTPlayApi Api
        {
            get { return KTPlayServer.api; }
            set { KTPlayServer.api = value; }
        }

        private string transferAddress = "http://+:8890/";

        public string TransferAddress
        {
            get { return transferAddress; }
            set { transferAddress = value; }
        }

        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        public KTPlayServer(string ip, string port)
        {
            //此处需要初始化loginAddress

            TransferAddress = "http://+:" + port + "/";
            Logger.Log.WriteLine("KTPlayServer initialized with TransferAddress=" + TransferAddress);

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
            //ThreadStart startDispose = new ThreadStart(AnySDKApi.RunLoginAfterInfo);
            //Thread threadDispose = new Thread(startDispose);
            //threadDispose.Start();

            VerifySocket = new HttpListener();
            VerifySocket.Prefixes.Add(TransferAddress);
            VerifySocket.Start();
            Logger.Log.Write("transferSocket started");
            VerifySocket.BeginGetContext(new AsyncCallback(GetTransferCallBack), VerifySocket);

        }

        static void GetTransferCallBack(IAsyncResult ar)
        {
            try
            {
                VerifySocket = ar.AsyncState as HttpListener;
                //Console.WriteLine("before endget");
                HttpListenerContext context = VerifySocket.EndGetContext(ar);

                VerifySocket.BeginGetContext(new AsyncCallback(GetTransferCallBack), VerifySocket);
                //Console.WriteLine("after endget");
                Dictionary<string, object> dic = new Dictionary<string, object>();

                if (context.Request.HttpMethod == "POST")
                {
                    //Console.WriteLine("post");
                    StreamReader reader = new StreamReader(context.Request.InputStream);
                    string request = reader.ReadToEnd();

                    //Logger.Log.Write("client verify with post {0}", request.Replace("{", "").Replace("}", ""));

                    //string[] requests = request.Split('&');
                    //foreach (string pair in requests)
                    //{
                    //    string key = pair.Split('=')[0];
                    //    string value = pair.Split('=')[1];
                    //    dic.Add(key, value);

                    //}
                    reader.Close();
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(request);
                    dic = dicMsg;
                }
                else
                {
                    //Console.WriteLine("get");
                    NameValueCollection pairs = HttpUtility.ParseQueryString(context.Request.Url.Query, Encoding.UTF8);
                    int count = context.Request.QueryString.Count;

                    foreach (var Item in context.Request.QueryString)
                    {
                        dic.Add(Item.ToString(), pairs[Item.ToString()]);
                        //Logger.Log.Write("client verify with get {0}: {1}", Item.ToString(), pairs[Item.ToString()]);
                    }
                }

                VerifyInfo tempBaseInfo = new VerifyInfo();
                tempBaseInfo.Info = dic;
                //foreach (var item in dic)
                //{
                //    Logger.Log.Write("dic {0}:{1}", item.Key, item.Value);
                //}
                tempBaseInfo.Context = context;
                //lock (api.InfoBefore)
                //{
                //    api.InfoBefore.Enqueue(tempBaseInfo);
                //}
                KTPlayApi.Verify(tempBaseInfo);
                //Console.WriteLine(context.Request.Url.PathAndQuery);
                //Console.WriteLine("login 处理结束");
            }
            catch (Exception e)
            {
                lock (LogList[LogType.WARN])
                {
                    LogList[LogType.WARN].Enqueue("GetTransferCallBack with Error " + e.ToString());
                }
            }
            finally
            {
                
            }
        }


    }
}

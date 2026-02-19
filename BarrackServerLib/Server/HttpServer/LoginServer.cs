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
    public class LoginServer
    {
        private BarrackServerApi server;

        private ServerState state = ServerState.Stopped;
        public ServerState State
        { get { return state; } }

        static HttpListener LoginSocket = null;
        static HttpListener PaySocket = null;
        static AnySDKApi api = AnySDKApi.SingleApi;

        internal static AnySDKApi Api
        {
            get { return LoginServer.api; }
            set { LoginServer.api = value; }
        }

        private string loginAddress = "http://+:8888/";

        public string LoginAddress
        {
            get { return loginAddress; }
            set { loginAddress = value; }
        }

        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();
        public LoginServer(string ip, string port)
        {
            //此处需要初始化loginAddress

            LoginAddress = "http://+:" + port + "/";
            Logger.Log.WriteLine("LoginServer initialized with LoginAddress=" + LoginAddress);

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

            LoginSocket = new HttpListener();
            LoginSocket.Prefixes.Add(LoginAddress);
            LoginSocket.Start();
            Logger.Log.Write("loginSocket started");
            LoginSocket.BeginGetContext(new AsyncCallback(GetLoginCallBack), LoginSocket);

        }

        static void GetLoginCallBack(IAsyncResult ar)
        {
            try
            {
                LoginSocket = ar.AsyncState as HttpListener;
                //Console.WriteLine("before endget");
                HttpListenerContext context = LoginSocket.EndGetContext(ar);

                LoginSocket.BeginGetContext(new AsyncCallback(GetLoginCallBack), LoginSocket);
                //Console.WriteLine("after endget");
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
                AnySDKApi.Login(tempBaseInfo);
                //Console.WriteLine(context.Request.Url.PathAndQuery);
                //Console.WriteLine("login 处理结束");
            }
            catch (Exception e)
            {
                lock (LoginServer.LogList[LogType.WARN])
                {
                    LogList[LogType.WARN].Enqueue("GetLoginCallBack with Error " + e.ToString());
                }
            }
            finally
            {
                
            }
        }
    }
}

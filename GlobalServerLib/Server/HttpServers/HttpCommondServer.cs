using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace GlobalServerLib
{
    public class HttpCommondHelper
    {
        public static DateTime tokenLifeEnd = DateTime.Now;

        public static string token = UpdateToken();
        public static string password = "11111lst";

        public static string Token
        {
            get { return UpdateToken(); }
            set { token = value; }
        }

        public static string UpdateToken()
        {
            string tokenString = Guid.NewGuid().ToString("N").Substring(0, 6);
            token = tokenString;
            tokenLifeEnd = DateTime.Now.Add(new TimeSpan(TimeSpan.TicksPerMinute * 10));
            return token;
        }

        public static string selectKey = "75d4hTB&^#Moap28sr";

    }

    public class HttpCommondServer
    {
        private GlobalServerApi server;

        private ServerState state = ServerState.Stopped;
        public ServerState State
        { get { return state; } }

        /// <summary>
        /// 提供给客户端的http连接
        /// </summary>
        static HttpListener VerifySocket = null;


        private string transferAddress = "http://+:8891/";

        private static List<string> ip = new List<string>();


        public string TransferAddress
        {
            get { return transferAddress; }
            set { transferAddress = value; }
        }

        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        public static Queue<AHttpSession> CmdQue = new Queue<AHttpSession>();
        public static Queue<AHttpSession> AfterCmdQue = new Queue<AHttpSession>();
        public static AHttpSession TempRequest = null;


        public HttpCommondServer(List<string> ip, string port, string password)
        {
            UpdateIpList(ip, password);
            TransferAddress = "http://+:" + port + "/";
            Logger.Log.WriteLine("HttpGMServer initialized with Address=" + TransferAddress);

            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        public void UpdateIpList(List<string> ip, string password)
        {
            HttpCommondHelper.password = password;
            HttpCommondServer.ip = ip;
        }

        public void Init(GlobalServerApi server)
        {
            this.server = server;
        }

        public void NotifyInitDone()
        {
            VerifySocket = new HttpListener();
            VerifySocket.Prefixes.Add(TransferAddress);
            VerifySocket.Start();
            Logger.Log.Write("transferSocket started");
            VerifySocket.BeginGetContext(new AsyncCallback(GetTransferCallBack), VerifySocket);

        }

        public static bool CheckIp(HttpListenerContext context)
        {
            if (context != null)
            {
                string ipAddress = null;
                if (context.Request.RemoteEndPoint != null)
                {
                    ipAddress = context.Request.RemoteEndPoint.Address.ToString();
                    //调试log
                    //Console.WriteLine("http request with ip {0}", ipAddress);

                    //判断ip在配置中
                    //TODO
                    if (ip.Contains(ipAddress))
                    {
                        return true;
                    }
                }
            }
            return false;
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
                var rawUrl = context.Request.RawUrl;

                var arr = rawUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 0)
                {
                    switch (arr[0])
                    {
                        case "gmselect":
                            if (!GmHttpSelect(context))
                            {
                                return;
                            }
                            break;
                        default:
                            if (!GMCmdCallBackFunc(context))
                            {
                                return;
                            }
                            break;
                    }
                }
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

        private static Dictionary<string, object> JsonToDictionary(string jsonData)
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



        private static bool GmHttpSelect(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST")
            {
                StreamReader reader = new StreamReader(context.Request.InputStream);
                string request = reader.ReadToEnd();
                
                Dictionary<string, object> dic = JsonToDictionary(request);
                SelectSession session = new SelectSession(SessionType.SelectRequest);
                session.SetData(context,dic);

                AddToCmdList(session);

                StringBuilder requestInfo = new StringBuilder("got http select request with ");
                foreach (var item in dic)
                {
                    requestInfo.Append(" " + item.Key + "=");
                    requestInfo.Append(item.Value + " ");
                }
                lock (LogList[LogType.INFO])
                {
                    LogList[LogType.INFO].Enqueue(requestInfo.ToString());
                }
            }
            return false;
        }

        private static bool GMCmdCallBackFunc(HttpListenerContext context)
        {
            string ip = context.Request.RemoteEndPoint.Address.ToString();
            if (!CheckIp(context))
            {
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("FALSE");
                    //Console.WriteLine("writing done3");
                }
                lock (LogList[LogType.WARN])
                {
                    LogList[LogType.WARN].Enqueue("someone try HttpCmd with wrong ip" + ip);
                }
                return false;
            }

            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("ip", ip);
            string cmdStr = "";
            string argsStr = "";
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
                    if (Item.ToString() == "cmd")
                    {
                        cmdStr = pairs[Item.ToString()];
                    }
                    if (Item.ToString() == "args")
                    {
                        argsStr = pairs[Item.ToString()];
                    }
                }
            }

            object password = "";
            bool gotToken = false;
            if (dic.TryGetValue("password", out password))
            {
                GmSession session = new GmSession(SessionType.GMRequest);
                session.SetData(context,dic,cmdStr,argsStr);
                
                AddToCmdList(session);
                gotToken = true;
            }
            if (!gotToken)
            {
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(HttpCommondHelper.Token);
                }
            }

            StringBuilder requestInfo = new StringBuilder("got httpGm request with ");
            foreach (var item in dic)
            {
                requestInfo.Append(" " + item.Key + "=");
                requestInfo.Append(item.Value + " ");
            }
            lock (LogList[LogType.INFO])
            {
                LogList[LogType.INFO].Enqueue(requestInfo.ToString());
            }

            return true;
        }

        public static void AddToCmdList(AHttpSession session)
        {
            lock (CmdQue)
            {
                CmdQue.Enqueue(session);
            }
        }

        public void UpdateCmdList()
        {

            lock (CmdQue)
            {
                while (CmdQue.Count > 0)
                {
                    AfterCmdQue.Enqueue(CmdQue.Dequeue());
                }
            }

            while (AfterCmdQue.Count > 0)
            {
                TempRequest = AfterCmdQue.Dequeue();
                ProcessCmdRequest(TempRequest);
            }
        }

        public void UpdateLogList()
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
                        Log.Warn(log);
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

        public void ProcessCmdRequest(AHttpSession session)
        {
            if (server == null || session.Dic == null || session.Dic.Count < 1)
            {
                return;
            }

            string password = session.GetSessionKey();

            switch (session.Type)
            {
                case SessionType.GMRequest:
                    try
                    {
                        if (session.CheckToken(password))//检查密码和token
                        {
                            if (session.Args != null)
                            {
                                server.GMExcuteCommand(session);
                            }
                            else
                            {
                                session.AnswerHttpCmd(HttpCommondHelper.Token);
                            }
                        }
                        else
                        {
                            Log.Info("got httpGmCode with wrong password " + password);
                            session.AnswerHttpCmd(HttpCommondHelper.Token);
                        }

                    }
                    catch (Exception e)
                    {
                        string answer = "FALSE";
                        session.AnswerHttpCmd(answer);
                        lock (LogList[LogType.WARN])
                        {
                            LogList[LogType.WARN].Enqueue("ProcessCmd with Exception " + e.ToString());
                        }
                    }

                    break;
                case SessionType.SelectRequest:
                    if (session.CheckToken(password))//检查key
                    {
                        server.SelectExcuteCommand(session);
                    }
                    else
                    {

                    }
                    break;
                default:
                    break;
            }
        }


    }
}

using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BarrackServerLib.Server.HttpServer
{
    public class AnySDKApi
    {
        private static AnySDKApi singleApi = new AnySDKApi();

        private Queue<baseInfo> loginInfoAfter = new Queue<baseInfo>();

        public Queue<baseInfo> LoginInfoAfter
        {
            get { return loginInfoAfter; }
            set { loginInfoAfter = value; }
        }

        private Queue<baseInfo> payInfoAfter = new Queue<baseInfo>();

        public Queue<baseInfo> PayInfoAfter
        {
            get { return payInfoAfter; }
            set { payInfoAfter = value; }
        }


        private static AnySDK anySDK = new AnySDK();

        internal static AnySDK AnySDK
        {
            get { return AnySDKApi.anySDK; }
            set { AnySDKApi.anySDK = value; }
        }

        //public static void RunLoginAfterInfo()
        //{
        //    Console.WriteLine("api.RunLoginAfterInfo is starting");
        //    while (true)
        //    {
        //        try
        //        {
        //            AnySDKApi api = AnySDKApi.SingleApi;
        //            Queue<baseInfo> tempQueue = new Queue<baseInfo>();
        //            lock (AnySDKApi.SingleApi.LoginInfoAfter)
        //            {
        //                while (api.LoginInfoAfter.Count > 0)
        //                {
        //                    tempQueue.Enqueue(api.LoginInfoAfter.Dequeue());
        //                }

        //            }
        //            while (tempQueue.Count > 0)
        //            {
        //                baseInfo info = tempQueue.Dequeue();
        //                AnySDKApi.DealWithLoginInfo(info);
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //}

        //public static void RunPayAfterInfo()
        //{
        //    Console.WriteLine("api.RunPayAfterInfo is starting");
        //    while (true)
        //    {
        //        try
        //        {
        //            AnySDKApi api = AnySDKApi.SingleApi;
        //            Queue<baseInfo> tempQueue = new Queue<baseInfo>();
        //            lock (AnySDKApi.SingleApi.PayInfoAfter)
        //            {
        //                while (api.PayInfoAfter.Count > 0)
        //                {
        //                    tempQueue.Enqueue(api.PayInfoAfter.Dequeue());
        //                }

        //            }
        //            while (tempQueue.Count > 0)
        //            {
        //                baseInfo info = tempQueue.Dequeue();
        //                AnySDKApi.DealWithPayInfo(info);
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //}

        //public static void DealWithLoginInfo(baseInfo info)
        //{
        //    try
        //    {
        //        //Console.WriteLine(info.Answer);
        //        //Console.WriteLine("info");
        //    }
        //    catch
        //    {
        //    }

        //}

        //public static void DealWithPayInfo(baseInfo info)
        //{
        //    try
        //    {
        //        //Console.WriteLine(info.Answer);
        //        //Console.WriteLine("info");
        //    }
        //    catch
        //    {
        //    }

        //}


        public static void Pay(baseInfo info)
        {
            string answer = info.Answer = "ok";
            try
            {
                HttpListenerContext context = info.Context;
                context.Response.StatusCode = 200;
                //NameValueCollection req =info.Info;

                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (var pair in info.Info)
                {
                    data.Add(pair.Key, pair.Value);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string payInfo=serializer.Serialize(info.Info);
                //Log.Write("pay with info {0}", payInfo.Replace("{","").Replace("}",""));
                if (data["sign"].ToString() == AnySDK.GetSignForAnyValid(info.Info) && data["pay_status"].ToString() == "1")
                {
                    //支付成功的处理
                    //注意判断金额，客户端可能被修改从而使用低金额购买高价值道具
                    lock (AnySDKApi.SingleApi.PayInfoAfter)
                    {
                        AnySDKApi.SingleApi.PayInfoAfter.Enqueue(info);
                    }
                }
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(info.Answer);
                }
            }
            catch (Exception ex)
            {
                lock (PayServer.LogList[LogType.INFO])
                {
                    PayServer.LogList[LogType.INFO].Enqueue("Pay function error with " + ex.ToString());
                }
            }
        }

        public static async Task Login(baseInfo info)
        {
            try
            {
                //Logger.Log.Write("login running");
                string answer = await AnySDK.PostLoginAsync(info);
                //Console.WriteLine(answer);
                HttpListenerContext context = info.Context;
                context.Response.StatusCode = 200;
                //NameValueCollection req =info.Info;

                //处理内容
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(answer);
                //Console.WriteLine("writing done 1");

                bool hasStatus = false;
                bool hasSn = false;
                if (dicMsg != null)
                {
                    object status = null;
                    dicMsg.TryGetValue("status", out status);

                    object sn = null;
                    dicMsg.TryGetValue("sn", out sn);
                    if (status != null)
                    {
                        hasStatus = true;
                    }
                    if (sn != null)
                    {
                        hasSn = true;
                    }
                }
                if (hasStatus && dicMsg["status"].ToString() == "ok")
                {
                    //dicMsg["ext"] = "test from wanxinGame";
                    //dicMsg.Add("test", "passed");
                    //GateManager
                    string tokenString=Guid.NewGuid().ToString("N").Substring(0,6);
                    int token = Convert.ToInt32(tokenString,16);
                    info.Token = token;
                    info.Error = false;
                    Dictionary<string, string> ext = new Dictionary<string, string>();
                    //string gateIp = GateServerManager.LoginServerIp;
                    //string gatePort = GateServerManager.LoginServerPort.ToString();
                    ext.Add("token", token.ToString());
                    //ext.Add("gateIp", gateIp);
                    //ext.Add("gatePort", gatePort);
                    dicMsg["ext"] = ext;

                    info.Answer = serializer.Serialize(dicMsg);
                    info.AnswerDic = dicMsg;

                    //Console.WriteLine("writing done 2");
                    //Logger.Log.Write("someone login with token {0}, gateIp {1},gatePort {2},answer {3}", token, gateIp, gatePort, answer.Replace("{","").Replace("}",""));
                    lock (AnySDKApi.SingleApi.LoginInfoAfter)
                    {
                        AnySDKApi.SingleApi.LoginInfoAfter.Enqueue(info);
                    }
                }
                else
                {
                    if (hasSn)
                    {
                        string error = "error_sn=" + dicMsg["sn"].ToString();
                        lock (LoginServer.LogList[LogType.WARN])
                        {
                            LoginServer.LogList[LogType.WARN].Enqueue("someone login with error " + error + "info : " + answer.Replace("[", "_[").Replace("]", "_]").Replace("{", "[").Replace("}", "]"));
                        }
                    }
                    else
                    {
                        lock (LoginServer.LogList[LogType.WARN])
                        {
                            LoginServer.LogList[LogType.WARN].Enqueue("someone login with error and sn is null and info is " + answer.Replace("[", "_[").Replace("]", "_]").Replace("{", "[").Replace("}", "]"));
                        }
                    }
                    info.Error = true;
                    info.Answer = "error";
                    lock (AnySDKApi.SingleApi.LoginInfoAfter)
                    {
                        AnySDKApi.SingleApi.LoginInfoAfter.Enqueue(info);
                    }
                }

                

                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteAsync(info.Answer);
                    //Console.WriteLine("writing done3");
                }
            }
            catch(Exception ex)
            {
                lock (LoginServer.LogList[LogType.WARN])
                {
                    LoginServer.LogList[LogType.WARN].Enqueue("Login function error with ex " + ex.ToString());
                }
                return;
            }
            finally
            {
                //Logger.Log.Write("someone login with UserId={0}", info.UserId);
            }
        }


        internal static AnySDKApi SingleApi
        {
            get { return AnySDKApi.singleApi; }
            set { AnySDKApi.singleApi = value; }
        }




    }

    public struct baseInfo
    {
        private bool error;

        public bool Error
        {
            get { return error; }
            set { error = value; }
        }

        private int token;

        public int Token
        {
            get { return token; }
            set { token = value; }
        }


        private Dictionary<string, string> dic;

        public Dictionary<string, string> Info
        {
            get { return dic; }
            set { dic = value; }
        }

        private HttpListenerContext _context;

        public HttpListenerContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        private string userId;

        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private string answer;

        public string Answer
        {
            get { return answer; }
            set { answer = value; }
        }

        public Dictionary<string, object> AnswerDic;

    }
}

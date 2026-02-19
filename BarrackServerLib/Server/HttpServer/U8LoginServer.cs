using EnumerateUtility;
using Logger;
using Message.Barrack.Protocol.BarrackC;
using ServerShared;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BarrackServerLib.Server.HttpServer
{
    //使用单例
    public class U8LoginServer
    {
        private static HttpClient client = new HttpClient();

        public static string loginCheckUrl = "localhost:8080/user/verifyAccount";

        public static string AppSecret = "xxx";

        public static JavaScriptSerializer serializer = new JavaScriptSerializer();

        private static U8LoginServer u8 = new U8LoginServer();

        public static U8LoginServer U8
        {
            get { return U8LoginServer.u8; }
        }

        //消息队列
        private static ConcurrentQueue<U8Info> infoQueue = new ConcurrentQueue<U8Info>();

        public static ConcurrentQueue<U8Info> InfoQueue
        {
            get { return infoQueue; }
        }

        public static Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        static U8LoginServer()
        {
            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }

        //登录过程异步，post访问后，把消息塞入队列
        public static async Task LoginU8Async(U8Info info)
        {
            string answer = await U8LoginServer.U8.PostLoginAsync(info);
            
            Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(answer);

            bool state=false;
            foreach (var item in dicMsg)
            {
                if (item.Key.Equals("state"))
                {
                    if (item.Value.Equals(1))
                    {
                        state = true;
                    }
                }
            }

            //放入队列
            info.U8Checked = state;
            infoQueue.Enqueue(info);
        }

        public async Task<string> PostLoginAsync(U8Info info)
        {

            HttpContent contents = new FormUrlEncodedContent(info.Info);

            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync(loginCheckUrl, contents);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        //队列处理
        public static void ProcessInfoQueue()
        {
            //先获取队列的长度，然后只循环固定次数
            int length = infoQueue.Count;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    U8Info info = null;
                    if (infoQueue.TryDequeue(out info))
                    {
                        if (info.U8Checked)
                        {
                            //调用client之后的处理
                            info.Client.Login();
                        }
                        else
                        {
                            //假如U8没有过
                            MSG_BC_USER_LOGIN_ERROR response = new MSG_BC_USER_LOGIN_ERROR();
                            response.Result = (int)ErrorCode.SDKCheckFailed;
#if DEBUG
                            Log.Debug("login response " + info.Client.Account + " " + response.Result + " ");
#endif
                            info.Client.Write(response);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log.Warn("try process U8Info with Exception " + e);
                }
                
            }
        }

        public static void Verify(Client client, string sdkUserId, string sdkToken)
        {
            U8Info info = new U8Info();
            info.Client = client;
            info.SdkToken = sdkToken;
            info.SdkUserId = sdkUserId;
            info.GenerateInfo();

            LoginU8Async(info);
        }
    }

    public class U8Info
    {
        private bool u8Checked;

        public bool U8Checked
        {
            get { return u8Checked; }
            set { u8Checked = value; }
        }

        private Client client;

        public Client Client
        {
            get { return client; }
            set { client = value; }
        }

        private string sdkUserId;

        public string SdkUserId
        {
            get { return sdkUserId; }
            set { sdkUserId = value; }
        }

        private string sdkToken;

        public string SdkToken
        {
            get { return sdkToken; }
            set { sdkToken = value; }
        }

        private Dictionary<string, string> info;

        public Dictionary<string, string> Info
        {
            get { return info; }
            set { info = value; }
        }

        public void GenerateInfo()
        {
            info.Add("userID", sdkUserId);
            info.Add("token", sdkToken);
            string md5 = HttpUtils.MD5Encode("userID=" + sdkUserId + "token=" + sdkToken + U8LoginServer.AppSecret);
            info.Add("sign", md5);
        }

    }
}

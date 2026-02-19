using Logger;
using ServerShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BarrackServerLib.Server.HttpServer
{
    public class KTPlayApi
    {
        private static KTPlayApi singleApi = new KTPlayApi();

        internal static KTPlayApi SingleApi
        {
            get { return KTPlayApi.singleApi; }
            set { KTPlayApi.singleApi = value; }
        }

        private Queue<VerifyInfo> verifiedInfo = new Queue<VerifyInfo>();

        public Queue<VerifyInfo> VerifiedInfo
        {
            get { return verifiedInfo; }
            set { verifiedInfo = value; }
        }

        private static KTPlay ktSDK = new KTPlay();

        internal static KTPlay KTSDK
        {
            get { return KTPlayApi.ktSDK; }
            set { KTPlayApi.ktSDK = value; }
        }

        public static async Task Verify(VerifyInfo info)
        {
            try
            {
                //Logger.Log.Write("Verify start");
                info.Info = GetInsideInfo(info.Info);
                string answer = await KTSDK.PostVerifyAsync(info);
                //Console.WriteLine(answer);
                HttpListenerContext context = info.Context;
                context.Response.StatusCode = 200;
                //NameValueCollection req =info.Info;

                //处理内容
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                //Logger.Log.Write("someone verify with {0}", answer.Replace("{", "").Replace("}", ""));
                Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(answer);

                if (dicMsg != null)
                {
                    foreach (var item in dicMsg)
                    {
                        //Log.Write("ktplayVerified info {0} : {1}", item.Key, item.Value);
                        if (item.Key == "data")
                        {
                            //Log.Write("extract data");
                            Dictionary<string, object> values = (Dictionary<string, object>)item.Value;
                            foreach (var item2 in values)
                            {
                                if (item2.Key == "rewards")
                                {
                                    //Log.Write("extract rewards");
                                    ArrayList list = (ArrayList)item2.Value;
                                    foreach (var tempinfo in list)
                                    {
                                        Dictionary<string, object> dic = (Dictionary<string, object>)tempinfo;

                                        object rewardId;
                                        dic.TryGetValue("extern_id", out rewardId);

                                        object value;
                                        dic.TryGetValue("value", out value);
                                        //Log.Write("extract reward id:{0},value:{1}",rewardId,value);
                                        info.Items.Add(int.Parse((string)rewardId), (int)value);

                                    }
                                }

                                if (item2.Key == "uid")
                                {
                                    string uid = null;
                                    uid = (string)item2.Value;
                                    info.UserId = uid.Split('_')[1];
                                    //Log.Write("extract uid:{0}",info.UserId);
                                }
                            }
                        }
                    }

                    info.Answer = serializer.Serialize(dicMsg);

                    //Console.WriteLine("writing done 2");
                    //Logger.Log.Write("someone verify with {0}", answer.Replace("{", "").Replace("}", ""));
                    lock (KTPlayApi.SingleApi.VerifiedInfo)
                    {
                        KTPlayApi.SingleApi.VerifiedInfo.Enqueue(info);
                    }
                }
                else
                {
                    lock (KTPlayApi.SingleApi.VerifiedInfo)
                    {
                        info.Error = true;
                        KTPlayApi.SingleApi.VerifiedInfo.Enqueue(info);
                    }
                }



                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteAsync(info.Answer);
                    //Console.WriteLine("writing done3");
                }
            }
            catch (Exception ex)
            {
                lock (KTPlayServer.LogList[LogType.WARN])
                {
                    KTPlayServer.LogList[LogType.WARN].Enqueue("Verify function error with ex " + ex.ToString());
                }
                return;
            }
            finally
            {
                lock (KTPlayServer.LogList[LogType.INFO])
                {
                    KTPlayServer.LogList[LogType.INFO].Enqueue("someone Verify with UserId=  " + info.UserId);
                }
            }
        }

        private static Dictionary<string, object> GetInsideInfo(Dictionary<string, object> dic)
        {
            Dictionary<string, object> dicMsg = new Dictionary<string, object>();

            foreach (var item in dic)
            {
                //Log.Write("ktplayVerified info {0} : {1}", item.Key, item.Value);
                if (item.Key == "data")
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)item.Value;
                    foreach (var item2 in values)
                    {
                        if (item2.Key != "rewards")
                        {
                            dicMsg.Add(item2.Key, item2.Value);
                            //if (item2.Key == "rewards")
                            //{
                            //}

                            //if (item2.Key == "uid")
                            //{
                            //}
                        }
                    }
                }
            }
            return dicMsg;
        }
    }

    public class VerifyInfo
    {
        private bool error;

        public bool Error
        {
            get { return error; }
            set { error = value; }
        }

        private Dictionary<int, int> items = new Dictionary<int, int>();

        public Dictionary<int, int> Items
        {
            get { return items; }
            set { items = value; }
        }


        private Dictionary<string, object> dic;

        public Dictionary<string, object> Info
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

    }
}

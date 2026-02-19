using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Logger;
using Message.Pay.Protocol.PM;
using PayServerLib.Server.SDK;
using ServerFrame;

namespace PayServerLib
{
    public abstract class PaySDKBase
    {
        protected PayServerApi server;

        public static JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public void Init(PayServerApi serverApi)
        {
            this.server = serverApi;
        }

        public abstract void Pay(PayMessage message);

        protected virtual bool CheckSign(PayMessage message)
        {
            return false;
        }

        protected virtual bool CheckOrderInfo(Dictionary<string, string> dicMsg)
        {
            return false;
        }

        protected void AddPayInfo(MSG_PM_RECHARGE_RESULT pks)
        {
            PayInfo info = new PayInfo(pks, this);
            server.PayInfoMng.Add(info);
        }

        public void DoPay(MSG_PM_RECHARGE_RESULT pks)
        {
            //找到实际的mainId
            int serverId = server.GetDestServerMainId(pks.ServerId);
            pks.ServerId = serverId;
            FrontendServer mServer = server.ManagerServerManager.GetServer(serverId, 0);
            if (mServer == null)
            {
                Log.Warn("pay order {0} failed: server {1} not find", pks.OrderInfo, serverId);
            }
            else
            {
                mServer.Write(pks);
            }
        }


        protected async Task<string> PostAsync(HttpClient client, Dictionary<string, string> info, string url)
        {
            try
            {
                HttpContent contents = new FormUrlEncodedContent(info);
                contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, contents);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected async Task<string> PayPostAsync(HttpClient client, Dictionary<string, string> info, string url)
        {
            try
            {
                HttpContent contents = new FormUrlEncodedContent(info);
                contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, contents);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (AggregateException ex)
            {
                Console.Write("PayPostLoginAsync error");
                foreach (var item in ex.InnerExceptions)
                {
                    Console.WriteLine("异常类型：{0}{1}来自：  {2} {3} 异常内容：{4} ", item.GetType(), Environment.NewLine, item.Source, Environment.NewLine, item.Message);
                }
                Console.Write(ex.Message);
                throw;
            }
        }

        protected static void ResponseMessage(HttpListenerContext context, string message)
        {
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(message);
            }
        }

    }
}

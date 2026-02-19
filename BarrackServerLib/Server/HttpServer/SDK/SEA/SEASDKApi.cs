using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using CommonUtility;
using Logger;
using  ServerShared;

namespace BarrackServerLib
{
    /*
     *errorcode 
     *
     *-400 参数错误，例如参数缺少、order_no未找到对应订单
     *-1 app_id不存在或已被封禁
     *-2 access key错误
     *-3 API校验密匙错误
     *-4 请求头部的user-agent不匹配，参考：2.2.1数据格式
     *-101 帐号未登陆
     * -102 帐号被封停
     *-400 请求参数错误
     *-500 服务器内部错误
     */


    public class SEASDKApi
    {
        //private static readonly HttpClient HttpClient;

        protected int appId = 6361;
        protected int merchantId = 1650;//商户Id
        protected int serverId = 5518;
        protected string serverName = "seaserver";
        protected string appKey = "69942325302e43dba43cfd9fad334f61";
        protected string secretKey = "9c5d3b14282f432f82a9862e64ea6da7";
        protected string loginCheckUrl = "http://line3-sdk-adapter.komoejoy.com/api/server/session.verify";
        protected string payCheckUrl = "http://line3-sdk-adapter.komoejoy.com/gapi/server/query.order";
        public static string ChannelId = "1";


        static SEASDKApi()
        {
            //HttpClient = new HttpClient();
            //HttpClient.Timeout = new TimeSpan(30000000);
            //HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 GameServer");
        }

        public int GameId => appId;

        public SEAVerifyInfo Verify(Client client, string sdkUserId, string sdkToken, int gameServerId)
        {
            SEAVerifyInfo info = new SEAVerifyInfo
            {
                Client = client,
                Version = "2",
                Uid = sdkUserId,
                AccessKey = sdkToken,
                MerchantId = merchantId,
                GameId = appId,
                TimeStemp = Timestamp.GetUnixTimeStampSeconds(DateTime.Now)
            };

            Dictionary<string, string> paramList = info.GetLoginInfo();
            string sign = HttpHelper.Sign_SEA(paramList, secretKey);

            paramList.Add("sign", sign);

            info = LoginSDKVerify(info, paramList);
            return info;
        }

        private SEAVerifyInfo LoginSDKVerify(SEAVerifyInfo info, Dictionary<string, string> paramList)
        {
            string answer = HttpHelper.PostAsyncWithNoResultWait(paramList, loginCheckUrl);

            //#if DEBUG
            Log.Info((object)$"login SEA Async with param  {info.Uid} accessKey {info.AccessKey} sing {paramList["sign"]}, answer {answer}");
            //#endif
            //Dictionary<string, object> dicMsg = serializer.Deserialize<Dictionary<string, object>>(answer);

            //bool state = dicMsg.ContainsKey("code") && dicMsg["code"].Equals(0);

            ////放入队列
            //info.Checked = state;
            //info.VerifyResponse = dicMsg;
            info.Checked = true;
            info.VerifyResponse = new Dictionary<string, object>();
            return info;
        }
    }
}

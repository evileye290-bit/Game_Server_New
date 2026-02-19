using CommonUtility;
using Logger;
using Message.Barrack.Protocol.BM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Message.Pay.Protocol.PM;
using PayServerLib.Server.SDK;

namespace PayServerLib
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


    public class SEASDKApi : PaySDKBase
    {
        private static HttpClient client = null;
        public static readonly string IOSChannelId = "1";
        private static readonly object LockObj = new object();

        protected int appId = 6361;
        protected int merchantId = 1650;//商户Id
        protected int serverId = 5518;
        protected string serverName = "seaserver";
        protected string appKey = "69942325302e43dba43cfd9fad334f61";
        protected string secretKey = "9c5d3b14282f432f82a9862e64ea6da7";
        protected string payCheckUrl = "http://line3-sdk-adapter.komoejoy.com/gapi/server/query.order";

        public static HttpClient GetClientInstance()
        {
            if (client == null)
            {
                lock (LockObj)
                {
                    if (client == null)
                    {
                        client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 GameServer");
                        client.Timeout = new TimeSpan(30000000);
                    }
                }
            }
            return client;
        }


        static SEASDKApi()
        {
            GetClientInstance();
        }

        public int GetAppId()
        {
            return appId;
        }


        public void Init(PayServerApi serverApi)
        {
            base.Init(serverApi);
        }

        public override void Pay(PayMessage info)
        {
            bool responsed = false;
            try
            {
                if (!CheckSign(info))
                {
                    responsed = true;
                    ResponseMessage(info.Context, "sign error");
                    return;
                }

                SEAOrderInfo orderInfo = new SEAOrderInfo();
                if (!CheckOrderInfo(info.Info, orderInfo))
                {
                    responsed = true;
                    ResponseMessage(info.Context, "decode infos error");
                    return;
                }

                CheckSdkOrder(info, orderInfo);
            }
            catch (Exception ex)
            {
                if (responsed == false)
                {
                    ResponseMessage(info.Context, "got exception");
                }
                PayServerBase.LogError("pay got ex" + ex);
            }
        }

        private bool CheckOrderInfo(Dictionary<string, string> dicMsg, SEAOrderInfo orderInfo)
        {
            try
            {
                string info = string.Empty;
                foreach (var item in dicMsg)
                {
                    info += $"{item.Key}:{item.Value}|";
                }
                Log.Debug($"passing data {info} to manager");

                string orderId;
                if (!dicMsg.TryGetValue("order_no", out orderId))
                {
                    PayServerBase.LogWarn($"pay orderID {info} get failed");
                    return false;
                }

                string serverIdObj;
                if (!dicMsg.TryGetValue("server_id", out serverIdObj))
                {
                    PayServerBase.LogWarn($"pay server_id {info} get failed");
                    return false;
                }

                string uidObj;
                if (!dicMsg.TryGetValue("uid", out uidObj))
                {
                    PayServerBase.LogWarn($"pay uid {info} get failed");
                    return false;
                }

                string extrasParams;
                if (!dicMsg.TryGetValue("extension_info", out extrasParams))
                {
                    PayServerBase.LogWarn($"pay extension_info {info} get failed");
                    return false;
                }

                string status;
                if (!dicMsg.TryGetValue("order_status", out status) || status.Equals("0"))
                {
                    PayServerBase.LogWarn($"pay order_status {status} get failed");
                    return false;
                }

                string productId;
                if (!dicMsg.TryGetValue("product_id", out productId))
                {
                    PayServerBase.LogWarn($"pay product_id {info} get failed");
                    return false;
                }

                string payMoney;
                if (!dicMsg.TryGetValue("pay_money", out payMoney))
                {
                    PayServerBase.LogWarn($"pay pay_money {info} get failed");
                    return false;
                }

                string payCurrency;
                if (!dicMsg.TryGetValue("pay_currency", out payCurrency))
                {
                    PayServerBase.LogWarn($"pay pay_currency {info} get failed");
                    return false;
                }

                string isSandbox;
                if (!dicMsg.TryGetValue("is_sandbox", out isSandbox))
                {
                    PayServerBase.LogWarn($"pay is_sandbox {info} get failed");
                    return false;
                }

                string payMode;
                if (!dicMsg.TryGetValue("pay_mode", out payMode))
                {
                    PayServerBase.LogWarn($"pay pay_mode {info} get failed");
                    return false;
                }

                orderInfo.orderId = orderId;
                orderInfo.userId = uidObj;
                orderInfo.serverId = serverIdObj;
                orderInfo.productId = productId;
                orderInfo.payMoney = payMoney;
                orderInfo.payCurrency = payCurrency;
                orderInfo.isSandbox = isSandbox;
                orderInfo.payMode = payMode;
                return true;
            }
            catch (Exception e)
            {
                PayServerBase.LogWarn("Pay function error with " + e.ToString());
                return false;
            }
        }

        private async void CheckSdkOrder(PayMessage baseInfo, SEAOrderInfo orderInfo)
        {
            try
            {
                SEAVerifyPayResult result = await GetPayOrderInfo(orderInfo.userId, orderInfo.orderId, serverId.ToString());

                if (result == null || result.code != 0 || result.data == null)
                {
                    PayServerBase.LogWarn($"CheckSdkOrder fail, got order from sdk fail uid {orderInfo.userId} order {orderInfo.orderId} serverId {orderInfo.serverId} channel serverId {serverId}");
                    return;
                }

                if (result.data.product_id != orderInfo.productId)
                {
                    PayServerBase.LogWarn($"CheckSdkOrder check productId fail, got order from sdk fail uid {orderInfo.userId} order {orderInfo.orderId} serverId {orderInfo.serverId} product id {result.data.product_id} call back productId {orderInfo.productId}");
                    return;
                }

                if (result.data.order_status != 1)
                {
                    PayServerBase.LogWarn($"CheckSdkOrder fail, got order from sdk status error {result.data.order_status}");
                    return;
                }

                int orderId = 0;
                string extInfo;
                if (!baseInfo.Info.TryGetValue("extension_info", out extInfo))
                {
                    PayServerBase.LogWarn($"pay extension_info {extInfo} get failed");
                    return;
                }

                string[] exStrings = extInfo.Split('_');
                if (exStrings.Length <= 0)
                {
                    PayServerBase.LogWarn($"pay extension_info {extInfo} get failed");
                    return;
                }

                orderId = int.Parse(exStrings[0]);

                MSG_PM_RECHARGE_RESULT notify = new MSG_PM_RECHARGE_RESULT
                {
                    OrderInfo = orderInfo.orderId,
                    ServerId = baseInfo.ServerId,
                    OrderId = orderId,
                    PayTime = DateTime.Now.ToString(),
                    Status = 1,
                    Money = float.Parse(orderInfo.payMoney),
                    PayCurrency = orderInfo.payCurrency,
                    IsSandbox = orderInfo.isSandbox,
                    PayMode = orderInfo.payMode
                };

                Log.Info($"check order success order id {orderInfo.orderId} server id {baseInfo.ServerId} money {orderInfo.payCurrency} currency {orderInfo.payCurrency} isSandbox {orderInfo.isSandbox} payModel {orderInfo.payMode}");

                AddPayInfo(notify);

                ResponseMessage(baseInfo.Context, "success");
            }
            catch (Exception e)
            {
                PayServerBase.LogError($"CheckSdkOrder got error {e}");
            }
        }

        private async Task<SEAVerifyPayResult> GetPayOrderInfo(string sdkUserId, string orderId, string serverId)
        {
            SEAVerifyInfo info = new SEAVerifyInfo
            {
                Version = "1",
                Uid = sdkUserId,
                MerchantId = merchantId,
                GameId = appId,
                TimeStemp = Timestamp.GetUnixTimeStampSeconds(DateTime.Now)
            };

            Dictionary<string, string> paramList = info.GetCommonInfo();
            paramList.Add("order_no", orderId);
            paramList.Add("server_id", serverId);

            string sign = Sign_SEA(paramList, secretKey);
            paramList.Add("sign", sign);

            string result = await PayPostAsync(GetClientInstance(), paramList, payCheckUrl);

            SEAVerifyPayResult verifyPayResult = Serializer.Deserialize<SEAVerifyPayResult>(result);
            return verifyPayResult;
        }

        protected override bool CheckSign(PayMessage info)
        {
            string sign;
            Dictionary<string, string> dictionary = new Dictionary<string, string>(info.Info);
            if (!dictionary.TryGetValue("sign", out sign))
            {
                return false;
            }

            dictionary.Remove("sign");
            string md5 = Sign_SEA(dictionary, secretKey);
            if (md5.Equals(sign))
            {
                return true;
            }

            return false;
        }

        private static string Sign_SEA(Dictionary<string, string> paramList, string secretKey)
        {
            StringBuilder builder = new StringBuilder();
            paramList.OrderBy(x => x.Key).ForEach(x => builder.Append(x.Value));
            builder.Append(secretKey);

            string tempStr = builder.ToString();
            return EncryptHelper.MD5Encode(tempStr);
        }
    }
}

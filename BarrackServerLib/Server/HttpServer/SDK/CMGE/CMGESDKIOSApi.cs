using CommonUtility;
using Logger;
using Message.Barrack.Protocol.BM;
using Message.Client.Protocol.CBarrack;
using ServerShared;
using System;
using System.Collections.Generic;

namespace BarrackServerLib
{
    public class CMGESDKIOSApi : BaseSDK
    {
        //正式
        //public static string appId = "78";
        public static readonly string ChannelId = "1";
        private static string appKey = "136b4ae51a3461bf8fdcaf682e29baeb";
        //public static string appSecret = "f72dcf13dffb84b627c64b9f4d9cd9a0";

        private static CMGESDKIOSApi singleApi = new CMGESDKIOSApi();
        internal static CMGESDKIOSApi SingleApi => singleApi;

        static CMGESDKIOSApi()
        {
        }

        public bool Verify(MSG_CB_USER_LOGIN msg)
        {
            //超过10分钟，算登录失败
            ulong loginTime = ulong.Parse(msg.Timestemp);
            if (Timestamp.TimeStampToDateTime(loginTime) < DateTime.Now.AddMinutes(10))
            {
                return false;
            }

            string verifyStr = $"{msg.SdkId}&{msg.Timestemp}&{appKey}";

            string md5Str = EncryptHelper.MD5Encode(verifyStr);

            Log.Debug($"IOS login info {msg}");

            return md5Str.Equals(msg.Sign);
        }

        //IOS SDK中只支持MD5
        protected override bool CheckSign(BaseInfo info)
        {
            Dictionary<string, string> dictionary = info.Info;
            if (!dictionary.ContainsKey("sign"))
            {
                return false;
            }

            /*List<string> keys = new List<string>(dictionary.Keys);
            sortParamNames(keys);

            List<string> paramList = new List<string>();
            foreach (string param in keys)
            {
                //拼接参数值
                if (param == "sign")
                {
                    continue;
                }

                string paramValue = dictionary[param];
                if (paramValue != null && !string.IsNullOrEmpty(paramValue))
                {
                    paramList.Add($"{param}={paramValue}");
                }
            }

            paramList.Add($"key={appKey}");
            string paramValues = string.Join("&", paramList);*/

            int index = info.Data.LastIndexOf('&');
            string signValues = info.Data.Substring(0,  index);
            signValues += $"&key={appKey}";


            string md5 = EncryptHelper.MD5Encode(signValues);

#if DEBUG
            Log.Debug("param Values " + signValues + "\n sign " + dictionary["sign"] + " " + md5);
#endif

            if (md5.Equals(dictionary["sign"]))
            {
                return true;
            }
            else
            {
#if DEBUG
                Log.Debug("md5 error " + signValues + "\n sign " + dictionary["sign"] + " " + md5);
#endif
            }

            return false;
        }

        protected override bool CheckOrderInfo(Dictionary<string, string> dicMsg)
        {
            try
            {
                string info = dicMsg.ToString("|", ":");

                Log.Debug($"passing data {info} to manager");

                string orderStatus;
                if (!dicMsg.TryGetValue("orderStatus", out orderStatus) || !orderStatus.Equals("SUCCESS"))
                {
                    BasePayServer.LogWarn($"pay orderStatus {info} get failed");
                    return false;
                }

                string orderIdObj;
                if (dicMsg.TryGetValue("orderId", out orderIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay orderId {info} get failed");
                    return false;
                }

                string moneyObj;
                if (dicMsg.TryGetValue("amount", out moneyObj) == false)
                {
                    BasePayServer.LogWarn($"pay money {info} get failed");
                    return false;
                }

                string channelIdObj;
                if (dicMsg.TryGetValue("payId", out channelIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay channelId {info} get failed");
                    return false;
                }

                string serverIdObj;
                if (dicMsg.TryGetValue("serverId", out serverIdObj) == false)
                {
                    BasePayServer.LogWarn($"pay serverId {info} get failed");
                    return false;
                }

                string uidObj;
                if (dicMsg.TryGetValue("roleId", out uidObj) == false)
                {
                    BasePayServer.LogWarn($"pay roleId {info} get failed");
                    return false;
                }

                string extrasParams;
                string extendOrderId;
                if (dicMsg.TryGetValue("callBackInfo", out extrasParams) == false)
                {
                    BasePayServer.LogWarn($"pay extension {info} get failed");
                    return false;
                }
                else
                {
                    string[] extendParams = extrasParams.Split('_');
                    if (extendParams.Length >= 2)
                    {
                        extendOrderId = extendParams[0];
                    }
                    else
                    {
                        BasePayServer.LogWarn($"pay extension {info} get failed");
                        return false;
                    }
                }


                MSG_BM_RECHARGE_RESULT notify = new MSG_BM_RECHARGE_RESULT();
                notify.OrderInfo = orderIdObj.ToString();
                notify.Money = (float)Convert.ToDouble(moneyObj);
                notify.ChannelId = Convert.ToInt32(channelIdObj);
                notify.ServerId = Convert.ToInt32(serverIdObj); 
                notify.OrderId = Convert.ToInt32(extendOrderId);
                notify.PayTime = DateTime.Now.ToString();
                notify.Status = 1;

                //notify.Uid = int.Parse(extrasParams.ToString().Split('_')[0]);
                //notify.ProductId = int.Parse(extrasParams.ToString().Split('_')[2]);
                //uint uidHigh = uint.Parse(extrasParams.ToString().Split('_')[3]);
                //uint uidLow = uint.Parse(extrasParams.ToString().Split('_')[4]);
                //notify.RechargeUid = ExtendClass.GetUInt64(uidHigh, uidLow);
                //notify.gameOrderId = gameOrder;

                AddPayInfo(notify);

                return true;
            }
            catch (Exception e)
            {
                BasePayServer.LogWarn("Pay function error with " + e.ToString());
                return false;
            }
        }
    }
}

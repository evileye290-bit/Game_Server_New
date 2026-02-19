using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayServerLib
{
    public class SEASDKApi_Web : SEASDKApi
    {
        private static SEASDKApi_Web instance = new SEASDKApi_Web();
        internal static SEASDKApi_Web Instance => instance;

        public SEASDKApi_Web() : base()
        {
            appId = 6362;
            serverId = 5519;
            merchantId = 1650;
            serverName = "seaserver";
            appKey = "de161d83654941ce85cb14000d9d5c4f";
            secretKey = "6eac59b6a5aa45c1879ec871a78435c2";
        }

        public static bool CheckSign(Dictionary<string, object> info)
        {
            try
            {
                object sign;
                Dictionary<string, object> dictionary = new Dictionary<string, object>(info);
                if (!dictionary.TryGetValue("sign", out sign))
                {
                    return false;
                }

                dictionary.Remove("sign");

                StringBuilder builder = new StringBuilder();
                foreach (var item in dictionary.OrderBy(x=>x.Key))
                {
                    builder.Append(item.Value.ToString());
                }
                builder.Append(Instance.secretKey);

                string tempStr = builder.ToString();
                string md5 = EncryptHelper.MD5Encode(tempStr);
                if (md5.Equals(sign))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                PayServerBase.LogError("pay got ex" + ex);
            }
            return false;
        }      
    }
}

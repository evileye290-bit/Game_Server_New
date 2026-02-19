namespace BarrackServerLib
{
    // Android
    public class SEASDKApi_Android : SEASDKApi
    {
        private static SEASDKApi_Android instance = new SEASDKApi_Android();
        internal static SEASDKApi_Android Instance => instance;

        public SEASDKApi_Android() : base()
        {
            appId = 6360;
            serverId = 5517;
            merchantId = 1650;
            serverName = "seaserver";
            appKey = "f87684be1bf34f05a15dc9e9cc4604fa";
            secretKey = "9f0f15cd14c7442b89cdc68a5d3b6e61";
        }
    }
}

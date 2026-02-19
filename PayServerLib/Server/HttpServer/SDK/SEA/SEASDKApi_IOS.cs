namespace PayServerLib
{
    public class SEASDKApi_IOS : SEASDKApi
    {
        private static SEASDKApi_IOS instance = new SEASDKApi_IOS();
        internal static SEASDKApi_IOS Instance => instance;

        public SEASDKApi_IOS() : base()
        {
            appId = 6361;
            serverId = 5518;
            merchantId = 1650;
            serverName = "seaserver";
            appKey = "69942325302e43dba43cfd9fad334f61";
            secretKey = "9c5d3b14282f432f82a9862e64ea6da7";
        }
    }
}

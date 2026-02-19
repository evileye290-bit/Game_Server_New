namespace BarrackServerLib
{
    // Android
    public class SEASDKApi_Huawei : SEASDKApi
    {
        private static SEASDKApi_Huawei instance = new SEASDKApi_Huawei();
        internal static SEASDKApi_Huawei Instance => instance;

        public SEASDKApi_Huawei() : base()
        {
            appId = 7307;
            serverId = 6472;
            merchantId = 1650;
            serverName = "seaserver";
            appKey = "b68df3648273468cb11d3f587cb4af83";
            secretKey = "dbafb47c18d44fb9bc75ae6383f105b0";
        }
    }
}

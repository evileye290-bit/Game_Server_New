using System.Collections.Generic;

namespace BarrackServerLib
{
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

        public Dictionary<string, object> Ans = null;

        public void GenerateInfo()
        {
            //info = new Dictionary<string, string>();
            //info.Add("userID", sdkUserId);
            //info.Add("token", sdkToken);

            //string md5 = EncryptHelper.MD5Encode("userID=" + sdkUserId + "token=" + sdkToken + CMGESDKApi.appSecret);
            //info.Add("sign", md5);
        }
    }
}

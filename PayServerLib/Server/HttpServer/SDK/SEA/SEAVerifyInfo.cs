using System.Collections.Generic;

namespace PayServerLib
{
    public class SEAVerifyInfo
    {
        //公共参数
        private int game_id;
        private int merchant_id;
        private string version = "1";
        private int timestamp;

        private string uid;
        private string access_key;
        private int region = 7;

        public int GameId
        {
            get { return game_id; }
            set { game_id = value; }
        }

        public int MerchantId
        {
            get { return merchant_id; }
            set { merchant_id = value; }

        }

        public string Version
        {
            get { return version; }
            set { version = value; }

        }

        public int TimeStemp
        {
            get { return timestamp; }
            set { timestamp = value; }

        }

        public string Uid
        {
            get { return uid; }
            set { uid = value; }

        }

        public string AccessKey
        {
            get { return access_key; }
            set { access_key = value; }

        }

        public bool Checked { get; set; }
        public Dictionary<string, object> VerifyResponse { get; set; }


        public Dictionary<string, string> GetCommonInfo()
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("game_id", game_id.ToString());
            info.Add("merchant_id", merchant_id.ToString());
            info.Add("version", version);
            info.Add("timestamp", timestamp.ToString());
            return info;
        }

        public Dictionary<string, string> GetLoginInfo()
        {
            Dictionary<string, string> info = GetCommonInfo();
            info.Add("uid", uid);
            info.Add("region", region.ToString());

            return info;
        }
    }
}

using CommonUtility;
using DBUtility;
using System.Collections.Generic;
using ServerModels;

namespace BarrackServerLib
{
    partial class Client
    {
        private string account = string.Empty;
        public string Account
        { get { return account; } }

        private string deviceId = string.Empty;
        public string DeviceId
        { get { return deviceId; } }

        private string password = string.Empty;
        public string Password
        { get { return password; } }

        private string channelName = "default";
        public string ChannelName
        { get { return channelName; } }

        private string sdkId = string.Empty;
        public string SdkId
        { get { return sdkId; } }

        private string sdkUuid = "default";

        private BarrackServerApi server;
        public BarrackServerApi Server
        { get { return server; } }

        //public int MainId;
        //public int IsGm;
        private string accountRealName = string.Empty;
        public string AccountRealName
        { get { return accountRealName; } }

        private string sdkToken = string.Empty;
        public string SdkToken
        { get { return sdkToken; } }

        private bool underAge = false;
        public bool UnderAge
        { get { return underAge; } }

        private SortedDictionary<int, SimpleCharacterInfo> loginServers = new SortedDictionary<int, SimpleCharacterInfo>();

        private int destMainId;

        // 创建角色需要客户端阻塞，防止多次点击覆盖
        private bool isVerifying = false;

        public bool IsIos { get; private set; }
        public bool IsTestAccount { get; private set; }
        public bool IsRebated { get; private set; }

        public string channelId = string.Empty;
        public string idfa = string.Empty;      //苹果设备创建角色时使用
        public string idfv = string.Empty;     //苹果设备创建角色时使用
        public string imei = string.Empty;   //安卓设备创建角色时使用
        public string imsi = string.Empty;    //安卓设备创建角色时使用
        public string anid = string.Empty;    //安卓设备创建角色时使用
        public string oaid = string.Empty;    //安卓设备创建角色时使用
        public string packageName = string.Empty;//包名
        public string extendId = string.Empty;  //广告Id，暂时不使用
        public string caid = string.Empty;		//暂时不使用

        public int tour;                         //是否是游客账号（0:非游客，1：游客）
        public string platform = string.Empty;   //平台名称	统一：ios|android|windows
        public string clientVersion = string.Empty;   //游戏的迭代版本，例如1.0.3
        public string deviceModel = string.Empty;     //设备的机型，例如Samsung GT-I9208
        public string osVersion = string.Empty;  //操作系统版本，例如13.0.2	
        public string network = string.Empty;    //网络信息	4G/3G/WIFI/2G
        public string mac = string.Empty;        //局域网地址
        public int gameId = 6360;

        public Client(BarrackServerApi server)
        {
            this.server = server;
            InitTcp();
            BindResponser();
        }

        public void Init(string account, string channel, string sdk_id, string device_id, string token, 
            int main_id, bool underAge, string sdkUuid, string channelId, string idfa, string idfv, string imei
            , string imsi, string anid, string oaid, string packageName, string extendId, string caid
            , int tour, string platform, string clientVersion, string deviceModel, string osVersion, string network, string mac)
        {
            this.account = account;
            this.channelName = channel;
            this.accountRealName = string.Format("{0}${1}", account, channel);
            this.sdkId = sdk_id;
            this.deviceId = device_id;
            this.sdkToken = token;
            this.destMainId = main_id;
            this.underAge = underAge;
            this.sdkUuid = sdkUuid;

            this.channelId = channelId;
            this.idfa = idfa;       //苹果设备创建角色时使用
            this.idfv = idfv;       //苹果设备创建角色时使用
            this.imei = imei;       //安卓设备创建角色时使用
            this.imsi = imsi;       //安卓设备创建角色时使用
            this.anid = anid;       //安卓设备创建角色时使用
            this.oaid = oaid;       //安卓设备创建角色时使用
            this.packageName = packageName;//包名
            this.extendId = extendId;   //广告Id，暂时不使用
            this.caid = caid;       //暂时不使用

            this.tour = tour;                   //是否是游客账号（0:非游客，1：游客）
            this.platform = platform;           //平台名称	统一：ios|android|windows
            this.clientVersion = clientVersion; //游戏的迭代版本，例如1.0.3
            this.deviceModel = deviceModel;     //设备的机型，例如Samsung GT-I9208
            this.osVersion = osVersion;         //操作系统版本，例如13.0.2
            this.network = network;             //网络信息	4G/3G/WIFI/2G
            this.mac = mac;                     //局域网地址          

            Logger.Log.Info($"Account init client {account} charList ChannelId {channelId} Idfa {idfa} Idfv { idfv} Imei { imei} Imsi { imsi} Anid{anid} Oaid {oaid} PackageName {packageName} ExtendId {extendId} Caid{oaid}");
        }

        public void InitLoginServers(string record, bool isTestAccount, bool isRebated)
        {
            IsRebated = isRebated;
            IsTestAccount = isTestAccount;
            loginServers = SimpleCharacterInfo.GetSimpleCharacterInfos(record);
        }

        public void Update()
        {
            if (CheckHeartbeat())
            {
                return;
            }
            OnProcessProtocol();
        }


        public void RecordLoginTimeAndServer()
        {
            string loginServerStr = string.Empty;
            if (!loginServers.ContainsKey(destMainId))
            {
                // 首次登录该server 需要记录
                loginServers.Add(destMainId, new SimpleCharacterInfo() { ServerId = destMainId, Time = Timestamp.GetUnixTimeStampSeconds(server.Now()) });

                loginServerStr = SimpleCharacterInfo.LoginServerCharacterInfosToString(loginServers);
            }
            server.AccountDBPool.Call(new QueryUpdateAccountLoginTimeAndServer(account, channelName, BarrackServerApi.now.ToString(), loginServerStr));
        }
    }
}

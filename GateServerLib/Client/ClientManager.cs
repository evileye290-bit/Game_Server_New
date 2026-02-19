using Logger;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GateServerLib
{
    public class LoginClient
    {
        public static int ExpriedTime = 90;
        public string AccountId;
        public string ChannelName;
        public string DeviceId;
        public int Token;
        public DateTime ReadyTime;
        public string sdkUuid;
        public bool IsRebated;

        public string channelId { get; private set; }
        public string idfa { get; private set; }      //苹果设备创建角色时使用
        public string idfv { get; private set; }      //苹果设备创建角色时使用
        public string imei { get; private set; }      //安卓设备创建角色时使用
        public string imsi { get; private set; }      //安卓设备创建角色时使用
        public string anid { get; private set; }     //安卓设备创建角色时使用
        public string oaid { get; private set; }      //安卓设备创建角色时使用
        public string packageName { get; private set; }//包名
        public string extendId { get; private set; }   //广告Id，暂时不使用
        public string caid { get; private set; }        //暂时不使用
        public int MainId { get; private set; }		//登录MainId

        public int tour { get; private set; }            //是否是游客账号（0:非游客，1：游客）
        public string platform { get; private set; }     //平台名称	统一：ios|android|windows
        public string clientVersion { get; private set; }  //游戏的迭代版本，例如1.0.3
        public string deviceModel { get; private set; }    //设备的机型，例如Samsung GT-I9208
        public string osVersion { get; private set; }  //操作系统版本，例如13.0.2
        public string network { get; private set; }    //网络信息	4G/3G/WIFI/2G
        public string mac { get; private set; }        //局域网地址    
        public int gameId { get; private set; }		//gameId

        public LoginClient(string account_id, string channel_name, string device_id,int token,string sdkUuid, bool isRebated, string channelId,
             string idfa, string idfv, string imei, string imsi, string anid, string oaid, string packageName, string extendId, string caid, int mainId,
             int tour, string platform, string clientVersion, string deviceModel, string osVersion, string network, string mac, int gameId)
        {
            AccountId = account_id;
            ChannelName = channel_name;
            DeviceId = device_id;
            Token = token;
            ReadyTime = GateServerApi.now;
            this.sdkUuid = sdkUuid;
            IsRebated = isRebated;

            this.channelId = channelId;
            this.idfa = idfa;       //苹果设备创建角色时使用
            this.idfv = idfv;       //苹果设备创建角色时使用
            this.imei = imei;       //安卓设备创建角色时使用
            this.imsi = imsi;       //安卓设备创建角色时使用
            this.anid = anid;       //安卓设备创建角色时使用
            this.oaid = oaid;       //安卓设备创建角色时使用
            this.packageName = packageName;//包名
            this.extendId = extendId;   //广告Id，暂时不使用
            this.caid = caid;		//暂时不使用
            MainId = mainId;

            this.tour = tour;                   //是否是游客账号（0:非游客，1：游客）
            this.platform = platform;           //平台名称	统一：ios|android|windows
            this.clientVersion = clientVersion; //游戏的迭代版本，例如1.0.3
            this.deviceModel = deviceModel;     //设备的机型，例如Samsung GT-I9208
            this.osVersion = osVersion;         //操作系统版本，例如13.0.2
            this.network = network;             //网络信息	4G/3G/WIFI/2G
            this.mac = mac;                     //局域网地址          
            this.gameId = gameId;
        }
    }

    public class OfflineClient
    {
        public static int OfflinePeriod = 60;
        public int Uid;
        public string AccountName;
        public int MainId;
        public int SubId;
        public DateTime OfflineTime;

        public OfflineClient(int uid, string account_name, int main_id, int sub_id, DateTime offline_time)
        {
            Uid = uid;
            AccountName = account_name;
            MainId = main_id;
            SubId = sub_id;
            OfflineTime = offline_time;
        }
    }

    public partial class ClientManager
    {
        private static readonly object connectingLock1 = new object();
        private static readonly object connectingLock2 = new object();
        private static readonly object connectingLock3 = new object();
        private static readonly object connectingLock4 = new object();
        private static readonly object connectingLock5 = new object();
        private static readonly object connectingLock6 = new object();
        private static readonly object connectingLock7 = new object();
        private static readonly object connectingLock8 = new object();
        private static readonly object connectingLock9 = new object();
        private static readonly object connectingLock0 = new object();

        List<Client> connectingList1 = new List<Client>();
        List<Client> connectingList2 = new List<Client>();
        List<Client> connectingList3 = new List<Client>();
        List<Client> connectingList4 = new List<Client>();
        List<Client> connectingList5 = new List<Client>();
        List<Client> connectingList6 = new List<Client>();
        List<Client> connectingList7 = new List<Client>();
        List<Client> connectingList8 = new List<Client>();
        List<Client> connectingList9 = new List<Client>();
        List<Client> connectingList0 = new List<Client>();


        private GateServerApi server;
        private DateTime dbDelayTime = DateTime.MinValue;

        private List<Client> clientList;
        public List<Client> ClientList
        { get { return clientList; } }
        private Dictionary<int, Client> clientUidList;

        private static readonly object removeClientLock = new object();
        private List<Client> removeClientList;

        private static readonly object offlineClientLock = new object();
        private Dictionary<int, OfflineClient> offlineClientList = new Dictionary<int, OfflineClient>();
        private List<int> removeOfflineList = new List<int>();

        private Dictionary<string, LoginClient> loginClientList = new Dictionary<string, LoginClient>();

        private List<string> removeLoginList = new List<string>();


        public int CurCount { get { return clientList.Count; } }
        public int InGameCount { get; set; }
        public int RegistedCharacterCount { get; set; }


        public void Init(GateServerApi server)
        {
            this.server = server;
            clientList = new List<Client>();
            clientUidList = new Dictionary<int, Client>();
            removeClientList = new List<Client>();
        }

        public void BindAcceptedClient(Client client)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            //Console.WriteLine("int current thread is {0} ", id);
            //lock (clientListLock)
            //{
            //    client_list.Add(client);
            //}
            switch (id % 10)
            {
                case 0:
                    lock (connectingLock0)
                    {
                        connectingList0.Add(client);
                    }
                    break;
                case 1:
                    lock (connectingLock1)
                    {
                        connectingList1.Add(client);
                    }
                    break;
                case 2:
                    lock (connectingLock2)
                    {
                        connectingList2.Add(client);
                    }
                    break;
                case 3:
                    lock (connectingLock3)
                    {
                        connectingList3.Add(client);
                    }
                    break;
                case 4:
                    lock (connectingLock4)
                    {
                        connectingList4.Add(client);
                    }
                    break;
                case 5:
                    lock (connectingLock5)
                    {
                        connectingList5.Add(client);
                    }
                    break;
                case 6:
                    lock (connectingLock6)
                    {
                        connectingList6.Add(client);
                    }
                    break;
                case 7:
                    lock (connectingLock7)
                    {
                        connectingList7.Add(client);
                    }
                    break;
                case 8:
                    lock (connectingLock8)
                    {
                        connectingList8.Add(client);
                    }
                    break;
                case 9:
                    lock (connectingLock9)
                    {
                        connectingList9.Add(client);
                    }
                    break;
            }
        }

        public LoginClient GetLoginClient(string accountName, string channelName)
        {
            LoginClient client = null;
            string accountRealName = Client.MakeAccountName(accountName, channelName);
            loginClientList.TryGetValue(accountRealName, out client);
            return client;
        }
        public void RemoveLoginClient(string accountName, string channelName)
        {
            string accountRealName = Client.MakeAccountName(accountName, channelName);
            loginClientList.Remove(accountRealName);
        }

        public void RemoveClient(Client client)
        {
            if (client != null)
            {
                lock (removeClientLock)
                {
                    removeClientList.Add(client);
                }
            }
        }
        public OfflineClient GetOfflineClient(int uid)
        {
            OfflineClient offlineClient = null;
            lock (offlineClientLock)
            {
                if (offlineClientList.TryGetValue(uid, out offlineClient))
                {
                    offlineClientList.Remove(uid);
                    Log.Write("remove offline player {0} because get offline client", uid);
                }
            }
            return offlineClient;
        }

        public void AddOfflineClient(Client client)
        {
            if (client != null && client.CurZone != null)
            {
                lock (offlineClientLock)
                {
                    OfflineClient offlineClient = null;
                    if (offlineClientList.TryGetValue(client.Uid, out offlineClient))
                    {
                        offlineClient.Uid = client.Uid;
                        offlineClient.MainId = client.CurZone.MainId;
                        offlineClient.SubId = client.CurZone.SubId;
                        offlineClient.OfflineTime = GateServerApi.now;
                        Log.Write("update offline player {0}", client.Uid);
                    }
                    else
                    {
                        offlineClient = new OfflineClient(client.Uid, client.AccountName, client.CurZone.MainId, client.CurZone.SubId, GateServerApi.now);
                        offlineClientList.Add(client.Uid, offlineClient);
                        Log.Write("add offline player {0}", client.Uid);
                    }
                }
            }
        }

        private void DestroyClient(Client client)
        {
            if (client != null)
            {
                client.Disconnect();
                client.LeaveWorld();
                clientList.Remove(client);
                clientUidList.Remove(client.Uid);
            }
        }

        public void DestroyAllClients()
        {
            lock (removeClientLock)
            {
                removeClientList.AddRange(clientList);
            }
        }

        public void AddConnectingClients()
        {
            lock (connectingLock0)
            {
                if (connectingList0.Count > 0)
                {
                    clientList.AddRange(connectingList0);
                    connectingList0.Clear();
                }
            }
            lock (connectingLock1)
            {
                if (connectingList1.Count > 0)
                {
                    clientList.AddRange(connectingList1);
                    connectingList1.Clear();
                }
            }
            lock (connectingLock2)
            {
                if (connectingList2.Count > 0)
                {
                    clientList.AddRange(connectingList2);
                    connectingList2.Clear();
                }
            }
            lock (connectingLock3)
            {
                if (connectingList3.Count > 0)
                {
                    clientList.AddRange(connectingList3);
                    connectingList3.Clear();
                }
            }
            lock (connectingLock4)
            {
                if (connectingList4.Count > 0)
                {
                    clientList.AddRange(connectingList4);
                    connectingList4.Clear();
                }
            }
            lock (connectingLock5)
            {
                if (connectingList5.Count > 0)
                {
                    clientList.AddRange(connectingList5);
                    connectingList5.Clear();
                }
            }
            lock (connectingLock6)
            {
                if (connectingList6.Count > 0)
                {
                    clientList.AddRange(connectingList6);
                    connectingList6.Clear();
                }
            }
            lock (connectingLock7)
            {
                if (connectingList7.Count > 0)
                {
                    clientList.AddRange(connectingList7);
                    connectingList7.Clear();
                }
            }
            lock (connectingLock8)
            {
                if (connectingList8.Count > 0)
                {
                    clientList.AddRange(connectingList8);
                    connectingList8.Clear();
                }
            }
            lock (connectingLock9)
            {
                if (connectingList9.Count > 0)
                {
                    clientList.AddRange(connectingList9);
                    connectingList9.Clear();
                }
            }
        }

        public void Update(double dt)
        {
            AddConnectingClients();
            lock (removeClientLock)
            {
                foreach (var client in removeClientList)
                {
                    try
                    {
                        DestroyClient(client);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                removeClientList.Clear();
            }
            lock (offlineClientLock)
            {
                foreach (var item in offlineClientList)
                {
                    if ((GateServerApi.now - item.Value.OfflineTime).TotalSeconds >= OfflineClient.OfflinePeriod)
                    {
                        removeOfflineList.Add(item.Key);
                    }
                }
                foreach (var item in removeOfflineList)
                {
                    offlineClientList.Remove(item);
                    Log.Write("remove offline player {0} because offline period", item);
                }
                removeOfflineList.Clear();
            }
            foreach (var item in loginClientList)
            {
                if ((GateServerApi.now - item.Value.ReadyTime).TotalSeconds >= LoginClient.ExpriedTime)
                {
                    removeLoginList.Add(item.Key);
                }
            }
            foreach (var item in removeLoginList)
            {
                loginClientList.Remove(item);
            }
            removeLoginList.Clear();

            foreach (var client in clientList)
            {
                try
                {
                    client.Update();
                    //client.SendChatList();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            //CheckRefresh(dt);
        }

        public Client FindClientByUid(int uid)
        {
            Client client = null;
            clientUidList.TryGetValue(uid, out client);
            return client;
        }

        public Client FindClient(string account_name, string channel_name)
        {
            foreach (var client in clientList)
            {
                if (client.AccountName == account_name && client.ChannelName == channel_name)
                {
                    return client;
                }
            }
            return null;
        }

        public Client FindClient(string account_name, string channel_name, int token)
        {
            foreach (var client in clientList)
            {
                if (client.AccountName == account_name && client.Token == token && client.ChannelName == channel_name)
                {
                    return client;
                }
            }
            return null;
        }

        public void AddClientUid(Client client)
        {
            if (client == null)
            {
                return;
            }
            if (clientUidList.ContainsKey(client.Uid))
            {
                clientUidList[client.Uid] = client;
            }
            else
            {
                clientUidList.Add(client.Uid, client);
            }
        }

        public void AddLoginClient(string account_id, string device_id, string channel_name, int token, string sdkUuid, bool isRebated,
            string channelId, string idfa, string idfv, string imei, string imsi, string anid, string oaid, string packageName,
            string extendId, string caid, int mainId, int tour, string platform, string clientVersion, string deviceModel, string osVersion, string network, string mac, int gameId)
        {
            LoginClient loginClient;
            string accountName = Client.MakeAccountName(account_id, channel_name);
            if (loginClientList.TryGetValue(accountName, out loginClient))
            {
                Log.Warn("account {0} device {1} channel {2} add login client error: has in list, old token {2} new token {3}", account_id, device_id, channel_name, loginClient.Token, token);
                loginClientList.Remove(accountName);
            }
            loginClient = new LoginClient(account_id, channel_name, device_id, token, sdkUuid, isRebated, channelId,
                                            idfa, idfv, imei, imsi, anid, oaid, packageName, extendId, caid, mainId,
                                            tour, platform, clientVersion, deviceModel, osVersion, network, mac, gameId);

            loginClientList.Add(accountName, loginClient);

            Log.Info($"Account add client {loginClient.AccountId} charList ChannelId {loginClient.channelId} Idfa {loginClient.idfa} Idfv { loginClient.idfv} Imei { loginClient.imei} Imsi { loginClient.imsi} Anid{loginClient.anid} Oaid {loginClient.oaid} PackageName {loginClient.packageName} ExtendId {loginClient.extendId} Caid{loginClient.oaid} Tour {loginClient.tour} Platform {loginClient.platform} ClientVersion {loginClient.clientVersion} DeviceModel { loginClient.deviceModel} OsVersion {loginClient.deviceModel} Network {loginClient.network} Mac {loginClient.mac}");
        }
    }
}

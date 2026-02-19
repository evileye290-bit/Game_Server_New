using Message.IdGenerator;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateCM;
using Message.Gate.Protocol.GateZ;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerModels;

namespace GateServerLib
{
    public  partial class Client
    {
        public string AccountName = string.Empty;
        public DateTime AccountCreatedTime;
        public int Uid;
        public string DeviceId = string.Empty;
        public string Password = string.Empty;
        public string RegisterId=string.Empty;
        public string ChannelName = "default";
        public string SDKUuid = "default";
        public GateServerApi server;
        public int MainId;
        public int IsGm;
        public int Token;
        public bool IsRebated;

        public string channelId = string.Empty;
        public string idfa = string.Empty;   //苹果设备创建角色时使用
        public string idfv = string.Empty;     //苹果设备创建角色时使用
        public string imei = string.Empty;    //安卓设备创建角色时使用
        public string imsi = string.Empty;      //安卓设备创建角色时使用
        public string anid = string.Empty;     //安卓设备创建角色时使用
        public string oaid = string.Empty;     //安卓设备创建角色时使用
        public string packageName = string.Empty;//包名
        public string extendId = string.Empty;   //广告Id，暂时不使用
        public string caid = string.Empty;      //暂时不使用

        public int tour;                         //是否是游客账号（0:非游客，1：游客）
        public string platform = string.Empty;   //平台名称	统一：ios|android|windows
        public string clientVersion = string.Empty;   //游戏的迭代版本，例如1.0.3
        public string deviceModel = string.Empty;     //设备的机型，例如Samsung GT-I9208
        public string osVersion = string.Empty;  //操作系统版本，例如13.0.2	
        public string network = string.Empty;    //网络信息	4G/3G/WIFI/2G
        public string mac = string.Empty;        //局域网地址     
        public int gameId;

        public List<CharacterEnterInfo> CharacterList;

        // 创建角色需要客户端阻塞，防止多次点击覆盖
        public MSG_CG_CREATE_CHARACTER ReqCreateMsg; 
        // = new MSG_CG_CREATE_CHARACTER()
        //{
        //    Name = "",
        //    Sex = 1,
        //    Job = 4
        //};
        public bool GM = false;

        private BackendServer curZone;
        public BackendServer CurZone
        { get { return curZone; } }
        public bool CatchOffline = true;
        public bool InWorld = false;
        public bool RadioOpen = false;
        public bool SpeakSensitiveWord = false;
        public Client(GateServerApi server)
        {
            this.server = server;
            InitTcp();
            BindResponser();
        }
 
        public void Update()
        {
            // 发送缓存的广播信息
            SendCatchedBroadcastPacket();
            if (CheckWaitStreams())
            {
                return;
            }
            if (CheckHeartbeat())
            {
                return;
            }
            OnProcessProtocol();
        }

        public void EnterWorld(int uid, BackendServer zone)
        {
            Uid = uid;
            curZone = zone;
            InWorld = true;
            if (MainId != zone.MainId)
            {
                MainId = zone.MainId;
            }
            server.ClientMng.AddClientUid(this);
            if (CharacterList != null)
            {
                CharacterList.Clear();
            }
            server.ClientMng.InGameCount++;
        }

        public void LeaveWorld()
        {
            if (InWorld)
            { 
                server.ClientMng.InGameCount--;
            }
            InWorld = false;
            RadioOpen = false;
            if (Uid != 0 && curZone != null)
            {
                MSG_GateZ_LeaveWorld notify = new MSG_GateZ_LeaveWorld();
                notify.Uid = Uid;
                WriteToZone(notify);

                //MSG_GateCM_LEAVE_ROOM msg = new MSG_GateCM_LEAVE_ROOM();
                //msg.PcUid = Uid;
                //msg.WorldId = Rooms.WorldId;
                //msg.NearbyEncode = Rooms.NearbyEcode;
                //msg.NearbyId = Rooms.NearbyId;
                //WriteToChatManager(msg);

                //server.ChatMng.LeaveAllRooms(this);
            }
        }

        // 跨zone处理
        public void ChangeZone(BackendServer zone, int map_id, int channel, bool sync_data)
        {
            if (zone == null) return;
            curZone = zone;
            MSG_GateZ_EnterWorld request = new MSG_GateZ_EnterWorld();
            request.AccountName = AccountName;
            request.CharacterUid = Uid;
            request.SyncData = sync_data;
            request.ClientIp = Ip;
            request.DeviceId = DeviceId;
            request.SdkUuid = SDKUuid;
            request.MapId = map_id;
            request.Channel = channel;
            request.RegisterId = RegisterId;

            request.ChannelId = channelId;
            request.Idfa = idfa;       //苹果设备创建角色时使用
            request.Idfv = idfv;       //苹果设备创建角色时使用
            request.Imei = imei;       //安卓设备创建角色时使用
            request.Imsi = imsi;       //安卓设备创建角色时使用
            request.Anid = anid;       //安卓设备创建角色时使用
            request.Oaid = oaid;       //安卓设备创建角色时使用
            request.PackageName = packageName;//包名
            request.ExtendId = extendId;   //广告Id，暂时不使用
            request.Caid = caid;        //暂时不使用

            request.Tour = tour;              //是否是游客账号（0:非游客，1：游客）
            request.Platform = platform;           //平台名称	统一：ios|android|windows
            request.ClientVersion = clientVersion; //游戏的迭代版本，例如1.0.3
            request.DeviceModel = deviceModel;     //设备的机型，例如Samsung GT-I9208
            request.OsVersion = osVersion;         //操作系统版本，例如13.0.2
            request.Network = network;             //网络信息	4G/3G/WIFI/2G
            request.Mac = mac;                     //局域网地址
            request.GameId = gameId;

            WriteToZone(request);
        }

        public bool WriteToZone<T>(T msg)
         where T : Google.Protobuf.IMessage
        {
            if (curZone == null) return false;
            if (msg == null)
            {
                return false;
            }
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            header.Write(BitConverter.GetBytes(Uid), 0, 4);

            return curZone.Write(header, body);
        }

        //public bool WriteToChatManager<T>(T msg)
        // where T : Google.Protobuf.IMessage
        //{
        //    if (server.ChatManagerServer == null) return false;
        //    if (msg == null)
        //    {
        //        return false;
        //    }
        //    MemoryStream body = new MemoryStream();
        //    MessagePacker.ProtobufHelper.Serialize(body, msg);

        //    MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
        //    ushort len = (ushort)body.Length;
        //    header.Write(BitConverter.GetBytes(len), 0, 2);
        //    header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);

        //    return server.ChatManagerServer.Write(header, body);
        //}

        public static string MakeAccountName(string channel_uid, string channel_name)
        {
            if (string.IsNullOrWhiteSpace(channel_name))
            {
                return String.Format("{0}${1}", channel_uid, "default");
            }
            return String.Format("{0}${1}", channel_uid, channel_name);
        }
        public void EnableCatch(bool enable)
        {
            CatchOffline = enable;
        }
    }
}

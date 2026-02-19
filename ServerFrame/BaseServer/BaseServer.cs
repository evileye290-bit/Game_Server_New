using DataProperty;
using Engine;
using Logger;
using Message.IdGenerator;
using Message.Shared.Protocol.Shared;
using ServerShared;
using SocketShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ServerFrame
{
    public class BaseServer
    {
        protected BaseApi api;
        protected BaseServerInfo info;
        public BaseServerInfo Info
        { get { return info; } }

        public int ServerUid { get; private set; }

        public int MainId
        { get { return info.MainId; } }

        public int SubId
        { get { return info.SubId; } }

        public bool WatchDog
        { get { return info.WatchDog; } }

        public string ServerIp
        { get { return info.ServerIp; } }

        public string ClientIp
        { get { return info.ClientIp; } }

        public int Port
        { get { return info.Port; } }

        public Dictionary<string, int> ServerPort
        { get { return info.ServerPort; } }

        public string Key
        { get { return info.Key; } }

        public ServerType ServerType
        { get { return info.ServerType; } }

        public string ServerName
        { get { return info.ServerName; } }

        protected Tcp serverTcp;
        public Tcp ServerTcp
        {
            get { return serverTcp; }
        }

        private bool inited = false;
        public bool Inited
        { get { return inited; } }

        protected ServerState state = ServerState.Starting;
        public ServerState State
        { get { return state; } }

        public delegate void Responser(MemoryStream stream, int uid = 0);
        protected Dictionary<uint, Responser> responsers = new Dictionary<uint, Responser>();

        public Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();
        Queue<ServerPacket> recvMsgQueue = new Queue<ServerPacket>();
        Queue<ServerPacket> processMsgQueue = new Queue<ServerPacket>();

        private DateTime lastHeartbeatTime = DateTime.Now;

        protected KeepaliveManager keepalive;

        protected bool tcpAlive;

        // 初始化相关
        public BaseServer(BaseApi api)
        {
            this.api = api;
            this.info = new BaseServerInfo();
            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
            BindResponser();
            keepalive = new KeepaliveManager(this);
            tcpAlive = false;
        }

        public void InitUid(int serverUid)
        {
            ServerUid = serverUid;
        }

        public void InitBaseInfo(BaseServerInfo info)
        {
            this.info = info;
        }

        public virtual void InitDone()
        {
        }

        public virtual void InitNetwork(string ip, ushort port)
        {
        }
        protected int OnRead(MemoryStream transferred)
        {
            int offset = 0;
            byte[] buffer = transferred.GetBuffer();
            {
                while ((transferred.Length - offset) > sizeof(UInt16))
                {
                    UInt16 size = BitConverter.ToUInt16(buffer, offset);
                    if (size + SocketHeader.ServerHeaderSize > transferred.Length - offset)
                    {
                        break;
                    }

                    UInt32 msg_id = BitConverter.ToUInt32(buffer, offset + sizeof(UInt16));
                    int uid = BitConverter.ToInt32(buffer, offset + sizeof(UInt16) + sizeof(UInt32));
                    ServerPacket packet = new ServerPacket(msg_id, uid);

                    byte[] content = new byte[size];
                    Array.Copy(buffer, offset + SocketHeader.ServerHeaderSize, content, 0, size);
                    packet.Msg = new MemoryStream(content, 0, size, true, true);

                    lock (recvMsgQueue)
                    {
                        recvMsgQueue.Enqueue(packet);
                    }
                    offset += (size + SocketHeader.ServerHeaderSize);
                }
            }

            transferred.Seek(offset, SeekOrigin.Begin);
            return 0;
        }

        public bool Write<T>(T msg, int uid = 0) where T : Google.Protobuf.IMessage
        {
            if (state != ServerState.Started || serverTcp == null)
            {
                return false;
            }
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(SocketHeader.ServerHeaderSize);
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);
            return serverTcp.Write(header, body);
        }

        public bool Write<T>(T msg, int uid, uint id) where T : Google.Protobuf.IMessage
        {
            if (state != ServerState.Started || serverTcp == null)
            {
                return false;
            }
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(SocketHeader.ServerHeaderSize);
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(id), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);
            return serverTcp.Write(header, body);
        }

        public bool Write(MemoryStream header, MemoryStream body)
        {
            if (state != ServerState.Started || serverTcp == null)
            {
                return false;
            }
            return serverTcp.Write(header, body);
        }

        public bool Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            if (state != ServerState.Started || serverTcp == null)
            {
                return false;
            }
            return serverTcp.Write(first, second);
        }

        // 直接转发未反序列化的MemoryStream
        public bool Write(uint msgId, int uid, MemoryStream body)
        {
            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(msgId), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);
            return Write(header, body);
        }

        public static void BroadCastMsgMemoryMaker<T>(T msg, out ArraySegment<byte> first, out ArraySegment<byte> second) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();

            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            // 广播情况下 uid不应赋值
            header.Write(BitConverter.GetBytes(0), 0, 4);
            Tcp.MakeArray(header, body, out first, out second);
        }

        // 逻辑相关
        protected void OnProcessProtocol()
        {
            lock (recvMsgQueue)
            {
                while (recvMsgQueue.Count > 0)
                {
                    ServerPacket msg = recvMsgQueue.Dequeue();
                    processMsgQueue.Enqueue(msg);
                }
            }
            if (processMsgQueue.Count > 0)
            {
                keepalive.Alive();
            }
            while (processMsgQueue.Count > 0)
            {
                ServerPacket msg = processMsgQueue.Dequeue();
                OnResponse(msg.MsgId, msg.Msg, msg.Uid);
            }
        }

        protected void UpdateHeartbeat()
        {
            if (this is BackendServer)
            {
                if ((BaseApi.now - lastHeartbeatTime).TotalMinutes >= 10)
                {
                    HeartbeatPing();
                    lastHeartbeatTime = BaseApi.now;
                }
            }
        }

        protected void CheckAlive()
        {
            if (!tcpAlive)
            {
                return;
            }
            if (!keepalive.CheckAlive())
            {
                Log.Error($"{ToString()} check alive failed! disconnect!");
                SetTcpAlive(false);
                serverTcp.Disconnect();
            }
        }

        protected void AddResponser(uint id, Responser responser)
        {
            responsers.Add(id, responser);
        }

        public virtual void OnResponse(uint id, MemoryStream stream, int uid)
        {
            Responser responser = null;
            if (responsers.TryGetValue(id, out responser))
            {
                responser(stream, uid);
            }
            else
            {
                Log.Warn("{0} got {1} unsupported package id {2}",
                    api.ServerName, ServerName, id);
            }
        }

        protected virtual void BindResponser()
        {
            AddResponser(Id<MSG_REGIST_SERVER>.Value, OnResponse_RegistServer);
            AddResponser(Id<MSG_NOTIFY_SERVER>.Value, OnResponse_NotifyServer);
            AddResponser(Id<MSG_HEARTBEAT_PING>.Value, OnResponse_HeartbeatPing);
            AddResponser(Id<MSG_HEARTBEAT_PONG>.Value, OnResponse_HeartbeatPong);
            AddResponser(Id<MSG_REGIST_SUCCESS>.Value, OnResponse_RegistSuccess);
            AddResponser(Id<MSG_KEEPALIVE_PING>.Value, OnResponse_KeepalivePing);
            AddResponser(Id<MSG_KEEPALIVE_PONG>.Value, OnResponse_KeepalivePong);
        }

        public virtual void Update(double dt)
        {
            try
            {
                OnProcessProtocol();
                UpdateHeartbeat();
                CheckAlive();
                UpdateLogs();
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        public virtual void UpdateLogs()
        {
            lock (LogList)
            {
                while (LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.INFO].Dequeue();
                        Log.Info(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (LogList[LogType.WARN].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.WARN].Dequeue();
                        Log.Warn(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (LogList[LogType.ERROR].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.ERROR].Dequeue();
                        Log.Error(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }

        public void NotifyBackendServer(BaseServer newServer)
        {
            MSG_SERVER_BASE_INFO info = new MSG_SERVER_BASE_INFO();
            info.ServerType = (int)newServer.ServerType;
            info.MainId = newServer.MainId;
            info.SubId = newServer.SubId;
            info.WatchDog = newServer.WatchDog;
            info.ServerIp = newServer.ServerIp;
            info.ClientIp = newServer.ClientIp;
            info.Port = newServer.Port;
            foreach (var item in newServer.ServerPort)
            {
                info.ServerPort[item.Key] = item.Value;
            }

            //AttachPortInfo(info, newServer);

            MSG_NOTIFY_SERVER msg = new MSG_NOTIFY_SERVER();
            msg.ServerInfo = info;
            Write(msg);
        }

        //protected void AttachPortInfo(MSG_SERVER_BASE_INFO info, BaseServer newServer)
        //{
        //    Data backendServerData = null;
        //    DataList serverDataList = DataListManager.inst.GetDataList("ServerConfig");
        //    List<Data> serverGroupData = serverDataList.GetByGroup(newServer.ServerType.ToString());
        //    foreach (var item in serverGroupData)
        //    {
        //        if (item.Get("mainId").GetInt() == newServer.MainId && item.GetInt("subId") == newServer.SubId)
        //        {
        //            backendServerData = item;
        //            break;
        //        }
        //    }
        //    if (backendServerData == null)
        //    {
        //        Log.Warn($"{api.ServerName} got backend type {(ServerType)info.ServerType} main {info.MainId} sub {info.SubId} ip {info.ServerIp} failed: no such data");
        //        return;
        //    }

        //    string serverTypeStr = ServerType.ToString();
        //    string serverPort = serverTypeStr.Substring(0, 1).ToLower() + serverTypeStr.Substring(1) + "Port";
        //    info.Port = backendServerData.GetInt(serverPort);
        //}

        protected void HeartbeatPing()
        {
            MSG_HEARTBEAT_PING msg = new MSG_HEARTBEAT_PING();
            Write(msg);
        }

        protected void HeartbeatPong()
        {
            MSG_HEARTBEAT_PONG msg = new MSG_HEARTBEAT_PONG();
            Write(msg);
        }

        public virtual void OnResponse_RegistServer(MemoryStream stream, int uid = 0)
        {
        }

        public virtual void OnResponse_NotifyServer(MemoryStream stream, int uid = 0)
        {
        }

        public virtual void OnResponse_HeartbeatPing(MemoryStream stream, int uid = 0)
        {
            Log.Write("got {0} heart beat ping", ServerName);
            HeartbeatPong();
        }

        public virtual void OnResponse_RegistSuccess(MemoryStream stream, int uid = 0)
        {

        }

        public virtual void OnResponse_HeartbeatPong(MemoryStream stream, int uid = 0)
        {
            Log.Write("got {0} heart beat pong", ServerName);
        }

        public virtual void OnResponse_KeepalivePing(MemoryStream stream, int uid = 0)
        {
            Log.Write($"{ToString()} got keep alive ping");
            MSG_KEEPALIVE_PONG response = new MSG_KEEPALIVE_PONG();
            Write(response);
        }

        public virtual void OnResponse_KeepalivePong(MemoryStream stream, int uid = 0)
        {
            Log.Write($"{ToString()} got keep alive pong");
        }

        static public string MakeKey(int main_id, int sub_id)
        {
            return string.Format("{0}_{1}", main_id, sub_id);
        }

        public static Boolean IsIP(String str)
        {
            return Regex.IsMatch(str, @"^\d*[.]\d*[.]\d*[.]\d*$");
        }

        public void Stop()
        {
            state = ServerState.Stopped;
        }

        public override string ToString()
        {
            return string.Format($"server type {info.ServerType} main {info.MainId} subId {info.SubId}");
        }

        protected void SetTcpAlive(bool alive)
        {
            tcpAlive = alive;
            if (alive)
            {
                keepalive.Alive();
            }
        }

    }
}

using Engine;
using Message.IdGenerator;
using Logger;
using SocketShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Client.Protocol.CGate;
using Message.Barrack.Protocol.BarrackC;
using Google.Protobuf;

namespace BarrackServerLib
{
    public partial class Client
    {
        Tcp tcp = new Tcp();
        public DateTime LastHeartbeatTime = DateTime.MaxValue;
        public DateTime LastRecvTime = BarrackServerApi.now;
        public string Ip
        { get { return tcp.IP; } }
        public Tcp ClientTcp
        { get { return tcp; } }

        void InitTcp()
        {
            tcp.OnRead = OnRead;
            tcp.OnDisconnect = OnDisconnect;
            tcp.OnAccept = OnAccept;
        }

        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> deal_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        private int OnRead(MemoryStream transferred)
        {
            int offset = 0;
            byte[] buffer = transferred.GetBuffer();

            while ((transferred.Length - offset) > sizeof(UInt16))
            {
                UInt16 size = BitConverter.ToUInt16(buffer, offset);

                if (size + SocketHeader.ClientSize > transferred.Length - offset)
                {
                    break;
                }
                UInt16 protobutLen = BitConverter.ToUInt16(buffer, offset + sizeof(UInt16));
                uint msg_id = BitConverter.ToUInt32(buffer, offset + 4);
                byte[] content = new byte[size];
                Array.Copy(buffer, offset + SocketHeader.ClientSize, content, 0, size);
                MemoryStream msg = new MemoryStream(content, 0, size, true, true);
                lock (m_msgQueue)
                {
                    m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, msg));
                }
                offset += (size + SocketHeader.ClientSize);
            }

            transferred.Seek(offset, SeekOrigin.Begin);
            return 0;
        }

        private void OnAccept(bool ret)
        {
            if (ret == true)
            {
                LastRecvTime = BarrackServerApi.now;
                Server.ClientMng.BindAcceptedClient(this);
            }
        }

        public void OnDisconnect()
        {
            Server.ClientMng.RemoveClient(this);
        }

        public bool IsConnected()
        {
            if (tcp != null)
            {
                return !tcp.IsClosed();
            }

            return false;
        }

        public bool CheckHeartbeat()
        {
            if (LastHeartbeatTime != DateTime.MaxValue && (BarrackServerApi.now - LastHeartbeatTime).TotalSeconds > 120 && IsConnected())
            {
                Log.Warn("player account {0} heartbeat time out, will be disconnected", AccountRealName);
                Server.ClientMng.RemoveClient(this);
                return true;
            }
            if (IsConnected() && (BarrackServerApi.now - LastRecvTime).TotalSeconds > 120 && LastHeartbeatTime == DateTime.MaxValue)
            {
                LastHeartbeatTime = BarrackServerApi.now;
                HeartBeat();
            }
            return false;
        }

        private void HeartBeat()
        {
            // send heartbeat
            MSG_BC_HEARTBEAT heart = new MSG_BC_HEARTBEAT();
            Write(heart);
        }

        public void Listen(ushort port)
        {
            tcp.Accept(port);
        }

        public void Disconnect()
        {
            //lock (this)
            {
                if (tcp != null && IsConnected())
                {
                    tcp.Disconnect();
                }
            }
        }

        public void OnProcessProtocol()
        {
            lock (m_msgQueue)
            {
                while (m_msgQueue.Count > 0)
                {
                    var msg = m_msgQueue.Dequeue();
                    deal_msgQueue.Enqueue(msg);
                }
            }
            while (deal_msgQueue.Count > 0)
            {
                var msg = deal_msgQueue.Dequeue();
                try
                {
                    OnResponse(msg.Key, msg.Value);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex.ToString());
                }
            }
        }

        public bool Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
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

            return Write(header, body);
        }

        public bool Write(MemoryStream header, MemoryStream body)
        {
            return tcp.Write(header, body);
        }
    }
}

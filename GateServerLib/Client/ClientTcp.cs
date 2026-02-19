using Logger;
using System.IO;
using Engine;
using Message.IdGenerator;
using System;
using SocketShared;
using System.Collections.Generic;
using ServerShared;
using Message.Gate.Protocol.GateC;
using CryptoUtility;
using Message.Client.Protocol.CGate;
using EnumerateUtility;
#if DEBUG
using ProtocolObjectParserLib;
#endif
namespace GateServerLib
{
    public partial class Client
    {
        Tcp tcp = new Tcp();
        public DateTime LastHeartbeatTime = DateTime.MaxValue;
        public DateTime LastRecvTime = GateServerApi.now;
        public string Ip
        { get { return tcp.IP; } }
        public Tcp ClientTcp
        { get { return tcp; } }

        public bool CheckWaitStreams()
        {
            if (tcp.WaitStreamsCount >= 4000 && IsConnected())
            {
                Logger.Log.Warn("player {0} wait streams too many, will disconnect", Uid);
                server.ClientMng.RemoveClient(this);
                return true;
            }
            return false;
        }

        public bool CheckHeartbeat()
        {
            if (LastHeartbeatTime != DateTime.MaxValue && (GateServerApi.now - LastHeartbeatTime).TotalSeconds > 120 && IsConnected())
            {
                Log.Warn("player {0} heartbeat time out, will be disconnected", Uid);
                server.ClientMng.RemoveClient(this);
                return true;
            }
            if (IsConnected() && (GateServerApi.now - LastRecvTime).TotalSeconds > 120 && LastHeartbeatTime == DateTime.MaxValue)
            {
                LastHeartbeatTime = GateServerApi.now;
                HeartBeat();
            }
            return false;
        }

        private void HeartBeat()
        {
            // send heartbeat
            MSG_GC_HEARTBEAT heart = new MSG_GC_HEARTBEAT();
            Write(heart);
        }

        // NOTE : 当前通过端口连接
        public void Listen(ushort port)
        {
            tcp.Accept(port);
        }

        // NOTE : 调用对象
        void InitTcp()
        {
            tcp.OnRead = OnRead;
            tcp.OnDisconnect = OnDisconnect;
            tcp.OnAccept = OnAccept;
        }

        public bool IsConnected()
        {
            if (tcp != null)
            {
                return !tcp.IsClosed();
            }

            return false;
        }

        // NOTE : tcp连接你的端口
        private void OnAccept(bool ret)
        {
            if (ret == true)
            {
                LastRecvTime = GateServerApi.now;
                server.ClientMng.BindAcceptedClient(this);
            }
        }

        // NOTE : 访问结束时，通过已调用IOCP
        // IOCP Threading
        /// <summary>
        /// 从ClientMng 移除放在DestroyClient中；
        /// </summary>
        public void OnDisconnect()
        {
            if (CatchOffline)
            {
                server.ClientMng.AddOfflineClient(this);
            }
            else
            {
                Log.Write("player {0} account {1} disconnect and not catch offline", Uid, AccountName);
            }
            server.ClientMng.RemoveClient(this);
        }

        // NOTE : 客户端对象删除前直接处理终止连接
        public void Disconnect()
        {
            this.BlowFishKey = "0000000000000000";
            this.MyBlowfish = null;


            lock (this)
            {
                if (tcp != null && IsConnected())
                {
                    tcp.Disconnect();
                }
            }
        }

        /// <summary>
        ///The following code was written by Kumo.If you encountered any problem, please contact him by QQ479813005.
        ///2015年4月14日09:17:47 修改
        /// </summary>
        Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> deal_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        private int OnRead(MemoryStream transferred)
        {
            int offset = 0;
            byte[] buffer = transferred.GetBuffer();

            //lock (this)
            {
                if (cryptoOpen)
                {
                    while ((transferred.Length - offset) > sizeof(UInt16))
                    {
                        UInt16 size = BitConverter.ToUInt16(buffer, offset);
                        if (size + SocketHeader.ClientSize > transferred.Length - offset)
                        {
                            break;
                        }

                        UInt16 protobutLen = BitConverter.ToUInt16(buffer, offset + sizeof(UInt16));
                        uint msg_id = BitConverter.ToUInt32(buffer, offset + 4);
                        MemoryStream msg = new MemoryStream(buffer, offset + SocketHeader.ClientSize, size, true, true);
                        MemoryStream trueStream = null;
                     
                        if (msg_id != Id<MSG_CG_GET_BLOWFISHKEY>.Value)
                        {
                            if (size > 0)
                            {
                                if (MyBlowfish == null)
                                {
                                    return -1;
                                }
                                trueStream = MyBlowfish.Decrypt_CBC(msg,offset,size);
                                trueStream.SetLength(protobutLen);
                            }
                            else
                            {
                                trueStream = msg;
                            }
                        }
                        else
                        {
                            trueStream = msg;
                        }
                        //m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, msg));
                        lock (m_msgQueue)
                        {
                            m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, trueStream));
                        }
                        offset += (size + SocketHeader.ClientSize);
                    }
                }
                else
                {
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
                        //m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, msg));
                        lock (m_msgQueue)
                        {
                            m_msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msg_id, msg));
                        }
                        offset += (size + SocketHeader.ClientSize);
                    }
                }
            }

            transferred.Seek(offset, SeekOrigin.Begin);
            return 0;
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
                //deal_msgQueue = m_msgQueue;
                //m_msgQueue = m_msgQueue2;
            }
            while (deal_msgQueue.Count > 0)
            {
                var msg = deal_msgQueue.Dequeue();
                try
                {
#if DEBUG
                    OnResponse_Parser(msg.Key, msg.Value);
#endif
                    OnResponse(msg.Key, msg.Value);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex.ToString());
                }
            }
            //m_msgQueue2 = deal_msgQueue;
        }

#if DEBUG
        public void OnResponse_Parser( uint msgId, MemoryStream stream)
        {
            try
            {
                Parser.Responser_Parser(msgId, stream, GateServerApi.now, Uid);
                stream.Position = 0;
            }
            catch (Exception e)
            {
                Log.Debug("parse msg error "+e);
            }
        }
#endif
        // 直接转发未反序列化的MemoryStream
        public bool Write(uint pid, MemoryStream body)
        {
            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(pid), 0, 4);

            return Write(header, body);
        }

        public bool Write<T>(T msg)
            where T : Google.Protobuf.IMessage
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

        public bool Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            return tcp.Write(first, second);
        }
    }
}
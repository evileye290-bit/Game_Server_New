using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Engine
{
    public class Tcp
    {
        public bool needListenHeartbeat = true;
        public bool NeedListenHeartbeat
        {
            get { return needListenHeartbeat; }
            set { needListenHeartbeat = value; }
        }

        private string ip;
        public string IP { get { return ip; } }
        private ushort port = 0;
        public ushort Port { get { return port; } }
        int offset = 0;
        private byte[] recvstream = new byte[4096];

        public delegate int AsyncReadCallback(MemoryStream stream);
        public delegate void AsyncConnectCallback(bool ret);
        public delegate void AsyncAcceptCallback(bool ret);
        public delegate void AsyncDisconnectCallback();

        private AsyncReadCallback onRead = DefaultOnRead;
        private AsyncConnectCallback onConnect = DefaultOnConnect;
        private AsyncAcceptCallback onAccept = DefaultOnAccept;
        private AsyncDisconnectCallback onDistonnect = DefaultOnDisconncect;

        public Tcp()
        {
        }
        public Tcp(string ip, ushort port)
        {
            this.ip = ip;
            this.port = port;
        }
        static private void DefaultOnDisconncect()
        {
            Console.WriteLine("default on disconnect function called, check it");
        }
        static private void DefaultOnConnect(bool ret)
        {
            Console.WriteLine("default on connect function called, check it");
        }
        static private void DefaultOnAccept(bool ret)
        {
            Console.WriteLine("default on accept called, check it");
        }

        static private int DefaultOnRead(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.End);
            return 0;
        }

        public AsyncReadCallback OnRead
        {
            set { onRead = value; }
            get { return onRead; }
        }

        public AsyncConnectCallback OnConnect
        {
            set { onConnect = value; }
            get { return onConnect; }
        }

        public AsyncAcceptCallback OnAccept
        {
            set { onAccept = value; }
            get { return onAccept; }
        }

        IList<ArraySegment<byte>> sendStreams = new List<ArraySegment<byte>>();
        IList<ArraySegment<byte>> waitStreams = new List<ArraySegment<byte>>();
        public int WaitStreamsCount = 0;

        public AsyncDisconnectCallback OnDisconnect
        {
            set { onDistonnect = value; }
            get { return onDistonnect; }
        }

        enum State
        {
            IDLE = 0,
            WAIT,
            RUN,
            CLOSE,
        }

        private int state = (int)State.IDLE;

        public bool Accept()
        {
            return Accept(port);
        }
        public bool Accept(ushort port)
        {
            if (socket != null) return false;
            if (Engine.System.IsAvailable == false) return false;

            this.port = port;
            if (Interlocked.CompareExchange(ref state, (int)State.WAIT, (int)State.IDLE) != (int)State.IDLE)
            {
                return false;
            }

            Socket listen = Engine.System.Acceptor(port);
            if (listen == null) return false;

            try
            {
                listen.BeginAccept(new AsyncCallback(ListenComplete), listen);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp Accept error: " + ex);
            }
            return false;
        }

        public bool Connect()
        {
            return Connect(ip, port);
        }
        public bool Connect(string ip, ushort port)
        {
            if (socket != null) return false;
            if (Engine.System.IsAvailable == false) return false;

            if (Interlocked.CompareExchange(ref state, (int)State.WAIT, (int)State.IDLE) != (int)State.IDLE)
            {
                return false;
            }

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(ip, port, ConnectComplete, null);
                //socket.NoDelay = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp Connect error: " + ex);
            }
            return false;
        }

        public bool IsClosed()
        {
            lock (this)
            {
                if (socket == null) { return true; }
                if (state != (int)State.RUN) { return true; }
                return false;
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (socket == null) { return; }
                if (state == (int)State.IDLE) { return; }

                state = (int)State.IDLE;
                socket.Close(0);
                //Console.WriteLine("=======================socket disconnect");
                socket = null;
                waitStreams.Clear();
                sendStreams.Clear();
                offset = 0;

                if (OnDisconnect != null)
                {
                    Engine.System.OnDisconnectCallbacks.Enqueue(OnDisconnect);
                }
            }
        }

        public bool Write(MemoryStream stream)
        {
            if (stream.Length == 0) return true;

            stream.Seek(0, SeekOrigin.Begin);

            lock (this)
            {
                if (state != (int)State.RUN)
                {
                    return false;
                }

                ArraySegment<byte> segment = new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(segment);

                    try
                    {
                        socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("tcp write stream error: " + ex);
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(segment);
                    WaitStreamsCount = waitStreams.Count;
                }
            }

            return true;
        }
        public bool Write(MemoryStream head, MemoryStream body)
        {
            head.Seek(0, SeekOrigin.Begin);
            body.Seek(0, SeekOrigin.Begin);

            if (body.Length == 0)
            {
                return Write(head);
            }

            lock (this)
            {
                if (state != (int)State.RUN)
                {
                    return false;
                }

                ArraySegment<byte> first = new ArraySegment<byte>(head.GetBuffer(), 0, (int)head.Length);
                ArraySegment<byte> second = new ArraySegment<byte>(body.GetBuffer(), 0, (int)body.Length);
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(first);
                    sendStreams.Add(second);
                    try
                    {
                        socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("tcp write head body error: " + ex);
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(first);
                    waitStreams.Add(second);
                    WaitStreamsCount = waitStreams.Count;
                }
            }
            return true;
        }

        /// <summary>
        /// 直接发送字节数组
        /// </summary>
        /// <param name="first">报头</param>
        /// <param name="second">报文</param>
        /// <returns></returns>
        public bool Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            lock (this)
            {
                if (state != (int)State.RUN)
                {
                    return false;
                }
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(first);
                    sendStreams.Add(second);
                    try
                    {
                        socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("tcp write first second error: " + ex);
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(first);
                    waitStreams.Add(second);
                    WaitStreamsCount = waitStreams.Count;
                }
            }
            return true;
        }

        /// <summary>
        /// 用于制作广播资源
        /// </summary>
        /// <param name="head">报头</param>
        /// <param name="body">报文</param>
        /// <param name="first">生成的报头数组</param>
        /// <param name="second">生成的报文数组</param>
        public static void MakeArray(MemoryStream head, MemoryStream body, out ArraySegment<byte> first, out ArraySegment<byte> second)
        {
            head.Seek(0, SeekOrigin.Begin);
            body.Seek(0, SeekOrigin.Begin);

            first = new ArraySegment<byte>(head.GetBuffer(), 0, (int)head.Length);
            second = new ArraySegment<byte>(body.GetBuffer(), 0, (int)body.Length);
        }

        public static void MakeArray(MemoryStream body, out ArraySegment<byte> first)
        {
            body.Seek(0, SeekOrigin.Begin);
            first = new ArraySegment<byte>(body.GetBuffer(), 0, (int)body.Length);
        }

        //virtual private 
        private void ListenComplete(IAsyncResult ar)
        {
            Socket listen = (Socket)ar.AsyncState;
            if (needListenHeartbeat)
            {
                try
                {
                    Engine.System.Listen(port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("tcp ListenComplete Listen port error: " + ex);
                }
            }

            try
            {
                socket = listen.EndAccept(ar);
                socket.NoDelay = true;
                if (Interlocked.CompareExchange(ref state, (int)State.RUN, (int)State.WAIT) != (int)State.WAIT)
                {
                    socket.Close();
                    Console.WriteLine("socket disconect ---------------------2");
                    socket = null;
                    OnAccept(false);
                    return;
                }
                //Console.WriteLine("++++++++++++++++++++accept socket");
                socket.BeginReceive(recvstream, 0, 2048, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                OnAccept(true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp ListenComplete BeginReceive error: " + ex);
            }

            socket = null;
            OnAccept(false);
        }

        private void ConnectComplete(IAsyncResult ar)
        {
            if (socket == null)
            {
                OnConnect(false);
                return;
            }

            try
            {
                socket.EndConnect(ar);

                if (Interlocked.CompareExchange(ref state, (int)State.RUN, (int)State.WAIT) != (int)State.WAIT)
                {
                    socket.Close();
                    socket = null;
                    OnConnect(false);
                    return;
                }

                state = (int)State.RUN;
                socket.BeginReceive(recvstream, 0, 2048, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                OnConnect(true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp ConnectComplete error: " + ex);
            }

            socket = null;
            state = (int)State.IDLE;
            OnConnect(false);
        }

        private void RecvComplete(IAsyncResult ar)
        {
            SocketError error;
            if (socket == null) return;

            try
            {
                int len = (int)socket.EndReceive(ar, out error);
                if (len <= 0)
                {
                    Disconnect();
                    return;
                }

                len = offset + len;
                MemoryStream transferred = new MemoryStream(recvstream, 0, len, true, true);
                if (OnRead != null)
                {
                    OnRead(transferred);
                }

                offset = (int)len - (int)transferred.Position;
                if (offset < 0)
                {
                    Disconnect();
                    return;
                }

                int size = 16384;
                if (transferred.Position == 0)
                {
                    size = (int)(transferred.Length * 2);
                }

                if (size > 65535)
                {
                    Disconnect();
                    return;
                }

                byte[] buffer = new byte[size];
                Array.Copy(recvstream, transferred.Position, buffer, 0, offset);
                recvstream = buffer;
                socket.BeginReceive(recvstream, offset, size - offset, SocketFlags.None, new AsyncCallback(RecvComplete), null);

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp RecvComplete error: " + ex);
            }

            Disconnect();
        }

        private void SendComplete(IAsyncResult ar)
        {
            try
            {
                int len = socket.EndSend(ar);
                if (len == 0)
                {
                    return;
                }

                lock (this)
                {
                    sendStreams.Clear();
                    if (waitStreams.Count > 0)
                    {
                        IList<ArraySegment<byte>> temp = sendStreams;
                        sendStreams = waitStreams;
                        waitStreams = temp;
                        socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("tcp SendComplete error: " + ex);
            }
        }

        private Socket socket = null;
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                try
                {
                    return socket.RemoteEndPoint as IPEndPoint;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("tcp RemoteEndPoint error: " + ex);
                    return null;
                }
            }
        }

        public int SendBufferSize
        {
            get { return socket.SendBufferSize; }
            set { socket.SendBufferSize = value; }
        }

        public bool SocketIsNull()
        {
            return socket == null;
        }

    }

}

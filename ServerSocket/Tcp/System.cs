using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections;

using SocketShared;
using Logger;
using System.Collections.Concurrent;

namespace Engine
{
    static public class System
    {
        static bool m_is_available = false;
        static public bool IsAvailable { get { return m_is_available; } }

        static public void Begin()
        {
            m_is_available = true;
        }

        static public void End()
        {
            m_is_available = false;
        }

        static Dictionary<ushort, Socket> listeners = new Dictionary<ushort, Socket>();
        static Dictionary<ushort, Heartbeat> heartbeats = new Dictionary<ushort, Heartbeat>();
        public delegate void Heartbeat(ushort port);

        public delegate void OnDisconnectCallback();
        static internal ConcurrentQueue<Engine.Tcp.AsyncDisconnectCallback> OnDisconnectCallbacks = new ConcurrentQueue<Engine.Tcp.AsyncDisconnectCallback>();

        static private void DefaultHeartbeat(ushort port)
        {
            Console.WriteLine("============use default heart beat, check it =======================!!!!");
            new Tcp().Accept(port);
        }

        static internal Socket Acceptor(ushort port)
        {
            if (IsAvailable == false) return null;

            Socket socket;
            if (listeners.TryGetValue(port, out socket) == true)
            {
                return socket;
            }
            socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            listeners.Add(port, socket);
            //IPAddress hostIP = Dns.Resolve(IPAddress.Any.ToString()).AddressList[0];
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ep);
            socket.Listen(128);
            socket.SendBufferSize = 16384;

            return socket;
        }

        static public void Listen(ushort port)
        {
            if (IsAvailable == false) return;

            Heartbeat heartbeat = null;
            if (heartbeats.TryGetValue(port, out heartbeat) == true)
            {
                heartbeat(port);
            }
            else
            {
                heartbeats.Add(port, DefaultHeartbeat);
                DefaultHeartbeat(port);
            }
        }

        static public bool Listen(ushort port, Heartbeat heartbeat)
        {
            return Listen(port, 128, heartbeat);
        }

        static public bool Listen(ushort port, ushort backlog, Heartbeat heartbeat)
        {
            if (IsAvailable == false) return false;

            heartbeats.Add(port, heartbeat);

            for (int i = 0; i < backlog; ++i)
            {
                heartbeat(port);
            }
            return true;
        }

        static public void Update()
        {
            int count = OnDisconnectCallbacks.Count;

            Engine.Tcp.AsyncDisconnectCallback callback;
            for (int i = 0; i < count; ++i)
            {
                if (OnDisconnectCallbacks.TryDequeue(out callback))
                {
                    if (callback != null)
                    {
                        try
                        {
                            callback();
                        }
                        catch (Exception e)
                        {
                            Log.Alert(e.ToString());
                        }
                    }
                }
            }
        }
    }
}

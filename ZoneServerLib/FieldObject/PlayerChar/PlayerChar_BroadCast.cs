using Engine;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using ServerShared;
using SocketShared;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        //广播
        private GateServer gate = null;
        public GateServer Gate
        {
            get { return gate; }
        }

        private MSG_GC_BROADCAST_LIST broadcastList = new MSG_GC_BROADCAST_LIST();
        public MSG_GC_BROADCAST_LIST BroadcastList
        {
            get { return broadcastList; }
        }

        private WaitStreamState waitStreamState = WaitStreamState.Normal;

        private int waitStreamIndex = 0;

        public void BindGate(GateServer gate)
        {
            this.gate = gate;
        }

        public void SendCatchedBroadcastPacket()
        {
            if (Gate == null) return;

            bool needBroadcast = false;
            int waitCount = gate.ServerTcp.WaitStreamsCount;
            if (waitCount < GameConfig.WaitStreamCountHalf)
            {
                waitStreamState = WaitStreamState.Normal;
            }
            else if (waitCount < GameConfig.WaitStreamCountQuarter)
            {
                waitStreamState = WaitStreamState.Half;
            }
            else if (waitCount >= GameConfig.WaitStreamCountQuarter && waitCount < GameConfig.WaitStreamCounDisconnect)
            {
                waitStreamState = WaitStreamState.Quarter;
            }
            else
            {
                // 待发送数据包过多 丢包处理
                waitStreamState = WaitStreamState.IGNORE;
                return;
            }
            switch (waitStreamState)
            {
                case WaitStreamState.Normal:
                    waitStreamIndex = 0;
                    needBroadcast = true;
                    break;
                case WaitStreamState.Half:
                    if (waitStreamIndex % 2 != 1)
                    {
                        waitStreamIndex++;
                        needBroadcast = false;
                    }
                    else
                    {
                        waitStreamIndex = 0;
                        needBroadcast = true;
                    }
                    break;
                case WaitStreamState.Quarter:
                    if (waitStreamIndex % 4 != 3)
                    {
                        waitStreamIndex++;
                        needBroadcast = false;
                    }
                    else
                    {
                        waitStreamIndex = 0;
                        needBroadcast = true;
                    }
                    break;
            }

            if (needBroadcast == false)
            {
                return;
            }
            if (BroadcastList.List.Count <= 0) return;


            if (BroadcastList.List.Count > GameConfig.BroadcastLimit)
            {
                Logger.Log.Debug($"player {Uid} BroadcastList count is {BroadcastList.List.Count} need break.");
                int count = 0;
                MSG_GC_BROADCAST_LIST itemMsg = new MSG_GC_BROADCAST_LIST();
                foreach (var item in broadcastList.List)
                {
                    if (count == 0)
                    {
                        itemMsg = new MSG_GC_BROADCAST_LIST();
                    }
                    itemMsg.List.Add(item);
                    count++;
                    if (count == GameConfig.BroadcastLimit)
                    {
                        Write(itemMsg);
                        count = 0;
                    }
                }
                if (count > 0)
                {
                    Write(itemMsg);
                }
            }
            else
            {
                if (IsNeedCacheMessage())
                {
                    Write(new MSG_GC_BROADCAST_LIST(broadcastList));
                }
                else
                { 
                    Write(broadcastList);
                }
            }

            broadcastList.List.Clear();
        }

        // 直接转发未反序列化的MemoryStream
        public bool Write(uint pid, MemoryStream body)
        {
            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(pid), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);
            return gate.Write(header, body);
        }

        private static List<Type> notNeedCacheMessage = new List<Type>
        {
            typeof(MSG_ZGC_CHAT),
            typeof(MSG_ZGC_FRIEND_HEART_TAKE_COUNT),
            typeof(MSG_ZGC_FRIEND_HEART_GIVE),
            typeof(MSG_ZGC_REPAY_FRIENDS_HEART),
            typeof(MSG_ZGC_FRIEND_HEART_GIVE_COUNT),
        };

        private static bool IsNeedCacheMessage(Type type)
        {
            return !notNeedCacheMessage.Contains(type);
        }

        public void Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (gate == null) return;

            //当前地图是加速后的，需要缓存消息
            if (IsNeedCacheMessage(msg.GetType()) && IsNeedCacheMessage())
            {
                CurDungeon?.CachePlayerMessage(msg, this);
            }
            else
            {
                gate.Write(msg, Uid);
            }
        }

        public void Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            if (gate == null) return;

            // 当前地图是加速后的，需要缓存消息
            if (IsNeedCacheMessage())
            {
                CurDungeon?.CachePlayerMessage(this, first, second);
            }
            else
            {
                gate.Write(first, second);
            }
        }

        public static void BroadCastMsgBodyMaker<T>(T msg, out ArraySegment<byte> first, out ushort len) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);
            len = (ushort)body.Length;
            Tcp.MakeArray(body, out first);
        }

        public void BroadCastMsgHeaderMaker<T>(T msg, ushort len, out ArraySegment<byte> first) where T : Google.Protobuf.IMessage
        {
            MemoryStream header = new MemoryStream(SocketHeader.ZGateSize);
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            header.Write(BitConverter.GetBytes(uid), 0, 4);
            Tcp.MakeArray(header, out first);
        }

        public override void BroadCast<T>(T msg)
        {
            if (IsObserver)
            {
                Write(msg);
            }
            else
            {
                base.BroadCast(msg);
            }
        }

        public override void BroadCastRegionList<T>(T msg, Dictionary<int, Region> region_list)
        {
            if (IsObserver)
            {
                Write(msg);
            }
            else
            {
                base.BroadCastRegionList(msg, region_list);
            }
        }

    }
}

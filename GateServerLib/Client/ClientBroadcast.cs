using Engine;
using Message.IdGenerator;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{

    enum WaitStreamState
    {
        Normal = 0,
        Half = 1,
        Quarter = 2,
        IGNORE = 3
    }

    public partial class Client
    {
        /// <summary>
        /// 为广播Msg获取字节数组
        /// </summary>
        /// <typeparam name="T">泛型Msg类型</typeparam>
        /// <param name="msg">Msg实体</param>
        /// <param name="first">out 报头数组</param>
        /// <param name="second">out 报文数组</param>
        public static void BroadCastMsgMemoryMaker<T>(T msg, out ArraySegment<byte> first, out ArraySegment<byte> second) where T : Google.Protobuf.IMessage
        {
            MemoryStream body = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            Tcp.MakeArray(header, body, out first, out second);
        }

        //private PKS_ZC_BROADCAST_LIST broadcastList = new PKS_ZC_BROADCAST_LIST();
        //public PKS_ZC_BROADCAST_LIST BroadcastList
        //{ get { return broadcastList; } }
        private WaitStreamState waitStreamState = WaitStreamState.Normal;
        private int waitStreamIndex = 0;


        // 收到zone打包的广播信息
        //public void CacheBroadcastPacket(PKS_ZC_BROADCAST_INFO msg)
        //{
        //    BroadcastList.list.Add(msg);
        //}

        public void SendCatchedBroadcastPacket()
        {
            bool needBroadcast = false;
            int waitCount = tcp.WaitStreamsCount;
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
            //if (broadcastList.list.Count != 0)
            //{
            //    Write(broadcastList);
            //    broadcastList.list.Clear();
            //}
        }
    }
}

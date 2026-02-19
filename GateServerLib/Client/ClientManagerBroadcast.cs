using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ClientManager
    {
        public void Broadcast<T>(T msg, int mainId = 0) where T : Google.Protobuf.IMessage
        {
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            Client.BroadCastMsgMemoryMaker(msg, out head, out body);
            Broadcast(head, body);
        }

        public void Broadcast<T>(T msg, bool speakSensitiveWord, int mainId = 0) where T : Google.Protobuf.IMessage
        {
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            Client.BroadCastMsgMemoryMaker(msg, out head, out body);
            BroadcastByGroup(head, body, speakSensitiveWord);
        }

        public void Broadcast(ArraySegment<byte> head, ArraySegment<byte> body)
        {
            foreach (var client in clientList)
            {
                if (client.InWorld)
                {
                    client.Write(head, body);
                }
                //else
                //{
                //    Log.WarnLine("Broadcast not find client {0}", client.Uid);
                //}
            }
        }

        public void BroadcastByGroup(ArraySegment<byte> head, ArraySegment<byte> body, bool speakSensitiveWord)
        {
            if (speakSensitiveWord)
            {
                foreach (var client in clientList)
                {
                    if (client.InWorld && client.SpeakSensitiveWord)
                    {
                        client.Write(head, body);
                    }
                    //else
                    //{
                    //    Log.WarnLine("Broadcast not find client {0}", client.Uid);
                    //}
                }
            }
            else
            {
                foreach (var client in clientList)
                {
                    if (client.InWorld)
                    {
                        client.Write(head, body);
                    }
                    //else
                    //{
                    //    Log.WarnLine("Broadcast not find client {0}", client.Uid);
                    //}
                }
            }
        }
    }
}

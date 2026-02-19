using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleServerLib.Client
{
    public class BattleClientList
    {
        public int Key { get; set; }

        private Dictionary<int, int> pcList = new Dictionary<int, int>();
        /// <summary>
        /// 玩家列表
        /// </summary>
        public Dictionary<int, int> PcList
        {
            get { return pcList; }
        }

        private List<BattleClient> clientList = new List<BattleClient>();

        public List<BattleClient> ClientList
        {
            get { return clientList; }
        }

        /// <summary>
        /// 添加等待队列
        /// </summary>
        /// <param name="client">玩家信息</param>
        public void AddClient(BattleClient client)
        {
            int zoneId;
            if (PcList.TryGetValue(client.Uid, out zoneId))
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    BattleClient tempClient = clientList[i];
                    if (tempClient.Uid.Equals(client.Uid))
                    {
                        clientList.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                PcList.Add(client.Uid, zoneId);
            }
            clientList.Add(client);
        }

        /// <summary>
        /// 移除等待队列
        /// </summary>
        /// <param name="clientUid">PC UID</param>
        public BattleClient RemoveClient(int clientUid)
        {
            int zoneId;
            if (PcList.TryGetValue(clientUid, out zoneId))
            {
                PcList.Remove(clientUid);
            }

            for (int i = 0; i < clientList.Count; i++)
            {
                BattleClient tempClient = clientList[i];
                if (tempClient.Uid.Equals(clientUid))
                {
                    clientList.RemoveAt(i);
                    return tempClient;
                }
            }
            return null;
        }
    }
}

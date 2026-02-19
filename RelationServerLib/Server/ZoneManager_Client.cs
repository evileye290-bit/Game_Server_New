using Logger;
using System.Collections.Generic;

namespace RelationServerLib
{
    public partial class ZoneServerManager
    {
        // key character_uid value Client 该main id下的所有角色
        private Dictionary<int, Client> clientList = new Dictionary<int, Client>();
        public Dictionary<int, Client> ClientList { get { return clientList; } }

        //private Dictionary<int, Client> offlineList = new Dictionary<int, Client>();

        public Client GetClient(int uid)
        {
            Client client = null;
            clientList.TryGetValue(uid, out client);    
            return client;
        }

        //public Client GetOfflineClient(int uid)
        //{
        //    Client client = null;
        //    offlineList.TryGetValue(uid, out client);
        //    return client;
        //}

        public void AddClient(Client client)
        {
            if (client == null)
            {
                Log.Warn("add client failed: client is null");
                return;
            }
            clientList[client.Uid] = client;

            //Client uffline = GetOfflineClient(client.Uid);
            //if (uffline != null)
            //{
            //    RemoveOfflineClient(uffline.Uid);
            //}
        }


        public bool RemoveClient(int Uid)
        {
            return clientList.Remove(Uid);
        }

        //public bool RemoveOfflineClient(int Uid)
        //{
        //    return offlineList.Remove(Uid);
        //}

        public void RemoveClients(ZoneServer zone)
        {
            if (zone == null) return;
            List<Client> clientList = new List<Client>();
            foreach (var client in this.clientList)
            {
                if (client.Value.CurZone == zone)
                {
                    clientList.Add(client.Value);
                }
            }

            foreach (var client in clientList)
            {
                RemoveClient(client.Uid);
            }
        }

    }
}

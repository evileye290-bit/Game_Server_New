using System.Collections.Generic;
using Logger;
using System;

namespace GlobalServerLib
{
    public class ClientManager
    {
        public int MaxUid = 0;
        private List<Client> clientList;
        List<Client> remove_client_list;
        private GlobalServerApi server;
        public ClientManager(GlobalServerApi server)
        {
            this.server = server;
            clientList = new List<Client>();
            remove_client_list = new List<Client>();
        }

        public void BindClient(Client client)
        {
            if (MaxUid >= 1000000000)
            {
                MaxUid = 1;
            }
            lock (clientList)
            {
                client.Uid = ++MaxUid;
                clientList.Add(client);
            }
        }

        public void RemoveClient(Client client)
        {
            if (client != null)
            {
                if (clientList.Contains(client) == true)
                {
                    lock (remove_client_list)
                    {
                        remove_client_list.Add(client);
                    }
                }
            }
        }

        public void RemoveServer(Client client)
        {
            lock (clientList)
            {
                clientList.Remove(client);
            }
        }

        public void DestroyClient(Client client)
        {
            if (client != null)
            {
                client.Disconnect();

                lock (clientList)
                {
                    clientList.Remove(client);
                }
            }
        }

        public Client FindClient(int uid)
        {
            lock (clientList)
            {
                foreach (var item in clientList)
                {
                    if (item.Uid == uid)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public void Update(double dt)
        {
            lock (remove_client_list)
            {
                for (int i = 0; i < remove_client_list.Count; ++i)
                {
                    try
                    {
                        Client remove_client = remove_client_list[i];
                        DestroyClient(remove_client);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                remove_client_list.Clear();
            }

            lock (clientList)
            {
                foreach (var client in clientList)
                {
                    try
                    {
                        client.OnProcessProtocol();
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
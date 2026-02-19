using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarrackServerLib
{
    public class ClientManager
    {
        private BarrackServerApi server;
        private List<Client> clientList;
        public List<Client> ClientList
        { get { return clientList; } }
        // key accountRealName, value Client
        private Dictionary<string, Client> clientAccountList;

        private static readonly object removeClientLock = new object();
        private List<Client> removeClientList;

        private static readonly object connectingLock1 = new object();
        private static readonly object connectingLock2 = new object();
        private static readonly object connectingLock3 = new object();
        private static readonly object connectingLock4 = new object();
        private static readonly object connectingLock5 = new object();
        private static readonly object connectingLock6 = new object();
        private static readonly object connectingLock7 = new object();
        private static readonly object connectingLock8 = new object();
        private static readonly object connectingLock9 = new object();
        private static readonly object connectingLock0 = new object();

        List<Client> connectingList1 = new List<Client>();
        List<Client> connectingList2 = new List<Client>();
        List<Client> connectingList3 = new List<Client>();
        List<Client> connectingList4 = new List<Client>();
        List<Client> connectingList5 = new List<Client>();
        List<Client> connectingList6 = new List<Client>();
        List<Client> connectingList7 = new List<Client>();
        List<Client> connectingList8 = new List<Client>();
        List<Client> connectingList9 = new List<Client>();
        List<Client> connectingList0 = new List<Client>();

        public void Init(BarrackServerApi server)
        {
            this.server = server;
            clientList = new List<Client>();
            clientAccountList = new Dictionary<string, Client>();
            removeClientList = new List<Client>();
        }

        public void BindAcceptedClient(Client client)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            //Console.WriteLine("int current thread is {0} ", id);
            //lock (clientListLock)
            //{
            //    client_list.Add(client);
            //}
            switch (id % 10)
            {
                case 0:
                    lock (connectingLock0)
                    {
                        connectingList0.Add(client);
                    }
                    break;
                case 1:
                    lock (connectingLock1)
                    {
                        connectingList1.Add(client);
                    }
                    break;
                case 2:
                    lock (connectingLock2)
                    {
                        connectingList2.Add(client);
                    }
                    break;
                case 3:
                    lock (connectingLock3)
                    {
                        connectingList3.Add(client);
                    }
                    break;
                case 4:
                    lock (connectingLock4)
                    {
                        connectingList4.Add(client);
                    }
                    break;
                case 5:
                    lock (connectingLock5)
                    {
                        connectingList5.Add(client);
                    }
                    break;
                case 6:
                    lock (connectingLock6)
                    {
                        connectingList6.Add(client);
                    }
                    break;
                case 7:
                    lock (connectingLock7)
                    {
                        connectingList7.Add(client);
                    }
                    break;
                case 8:
                    lock (connectingLock8)
                    {
                        connectingList8.Add(client);
                    }
                    break;
                case 9:
                    lock (connectingLock9)
                    {
                        connectingList9.Add(client);
                    }
                    break;
            }
        }

        public void AddConnectingClients()
        {
            lock (connectingLock0)
            {
                if (connectingList0.Count > 0)
                {
                    clientList.AddRange(connectingList0);
                    connectingList0.Clear();
                }
            }
            lock (connectingLock1)
            {
                if (connectingList1.Count > 0)
                {
                    clientList.AddRange(connectingList1);
                    connectingList1.Clear();
                }
            }
            lock (connectingLock2)
            {
                if (connectingList2.Count > 0)
                {
                    clientList.AddRange(connectingList2);
                    connectingList2.Clear();
                }
            }
            lock (connectingLock3)
            {
                if (connectingList3.Count > 0)
                {
                    clientList.AddRange(connectingList3);
                    connectingList3.Clear();
                }
            }
            lock (connectingLock4)
            {
                if (connectingList4.Count > 0)
                {
                    clientList.AddRange(connectingList4);
                    connectingList4.Clear();
                }
            }
            lock (connectingLock5)
            {
                if (connectingList5.Count > 0)
                {
                    clientList.AddRange(connectingList5);
                    connectingList5.Clear();
                }
            }
            lock (connectingLock6)
            {
                if (connectingList6.Count > 0)
                {
                    clientList.AddRange(connectingList6);
                    connectingList6.Clear();
                }
            }
            lock (connectingLock7)
            {
                if (connectingList7.Count > 0)
                {
                    clientList.AddRange(connectingList7);
                    connectingList7.Clear();
                }
            }
            lock (connectingLock8)
            {
                if (connectingList8.Count > 0)
                {
                    clientList.AddRange(connectingList8);
                    connectingList8.Clear();
                }
            }
            lock (connectingLock9)
            {
                if (connectingList9.Count > 0)
                {
                    clientList.AddRange(connectingList9);
                    connectingList9.Clear();
                }
            }
        }

        // 加入remove队列，等待Update中调用DestroyClient真正删除
        public void RemoveClient(Client client)
        {
            if (client != null)
            {
                lock (removeClientLock)
                {
                    removeClientList.Add(client);
                }
            }
        }

        private void DestroyClient(Client client)
        {
            if (client != null)
            {
                client.Disconnect();
                clientList.Remove(client);
                clientAccountList.Remove(client.AccountRealName);
            }
        }

        public void DestroyAllClients()
        {
            lock (removeClientLock)
            {
                removeClientList.AddRange(clientList);
            }
        }

        public void Update(double dt)
        {
            AddConnectingClients();
            lock (removeClientLock)
            {
                foreach (var client in removeClientList)
                {
                    try
                    {
                        DestroyClient(client);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                removeClientList.Clear();
            }

            foreach (var client in clientList)
            {
                try
                {
                    client.Update();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public Client FindClientByAccount(string account_real_name)
        {
            Client client = null;
            clientAccountList.TryGetValue(account_real_name, out client); 
            return client;
        }

        public void AddClientByAccount(string account_real_name, Client client)
        {
            clientAccountList[account_real_name] = client;
        }
    }
}

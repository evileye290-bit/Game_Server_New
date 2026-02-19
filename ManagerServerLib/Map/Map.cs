using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using DataProperty;
using ServerShared;
using ServerModels;

// ZServer挂载的地图
namespace ManagerServerLib
{
    public class Map
    {
        // 所属zone 的信息
        private int mainId = 0;
        public int MainId
        { get { return mainId; } }

        private int subId = 0;
        public int SubId
        { get { return subId; } }

        private int channel = 0;
        public int Channel
        { get { return channel; } }

        private int mapId = 0;
        public int MapId
        { get { return mapId; } }

        MapModel model;
        public int UniformNum
        { get { return model.UniformNum; } }

        public int MaxNum
        { get { return model.MaxNum; } }


        public MapState State
        { get; set; }
        // 已分配zone的client
        private Dictionary<int, Client> clientListMap = new Dictionary<int, Client>();
        public Dictionary<int, Client> ClientListMap
        { get { return clientListMap; } }

        // 即将要进入地图的client: key uid, value 分配时的时间
        private Dictionary<int, DateTime> clientEnterList = new Dictionary<int, DateTime>();
        public Dictionary<int, DateTime> ClientEnterList
        { get { return clientEnterList; } }

        // 当前地图人数
        public int ClientCount
        { get { return clientListMap.Count + clientEnterList.Count; } }


        public Map(int main_id, int sub_id, int map_id, int channel)
        {
            mainId = main_id;
            subId = sub_id;
            this.channel = channel;
            mapId = map_id;
            model = MapLibrary.GetMap(map_id);
            State = MapState.OPEN;
        }

        public void AddClient(Client newClient)
        {
            if (newClient == null)
            {
                return;
            }
            try
            {
                clientListMap.Remove(newClient.CharacterUid);
                clientListMap.Add(newClient.CharacterUid, newClient);
            }
            catch (Exception e)
            {
                Logger.Log.Alert("mainId {0} subId {1} channel {2} map {3} add client {4} failed: {5}",
                    mainId, subId, channel, mapId, newClient.CharacterUid, e.ToString());
            }
        }

        public void RemoveClient(int character_uid)
        {
            try
            {
                clientListMap.Remove(character_uid);
            }
            catch (Exception e)
            {
                Logger.Log.Alert("mainId {0} subId {1} channel {2} map {3} remove client {4} failed: {5}",
                    mainId, subId, channel, mapId, character_uid, e.ToString());
            }
        }

        public void ClearClientList()
        {
            clientListMap.Clear();
            clientEnterList.Clear();
        }

        // 进入副本 不会调用此函数
        public void WillEnter(int uid)
        {
            DateTime time;
            if (clientEnterList.TryGetValue(uid, out time))
            {
                // 可能切图过程中掉线 重新尝试进入该地图
                clientEnterList[uid] = ManagerServerApi.now;
            }
            else
            {
                clientEnterList.Add(uid, ManagerServerApi.now);
            }
        }

        public void ClientEnter(int uid)
        {
            clientEnterList.Remove(uid);
        }

        List<int> removeWillEnterList = new List<int>();
        public void UpdateWillEnterList()
        {
            foreach (var item in clientEnterList)
            {
                if ((ManagerServerApi.now - item.Value).TotalSeconds > 60)
                {
                    removeWillEnterList.Add(item.Key);
                }
            }
            foreach (var item in removeWillEnterList)
            {
                clientEnterList.Remove(item);
            }
            removeWillEnterList.Clear();
        }

        public string GetKey()
        {
            return string.Format("{0}_{1}", mapId.ToString(), channel.ToString());
        }
    }
}

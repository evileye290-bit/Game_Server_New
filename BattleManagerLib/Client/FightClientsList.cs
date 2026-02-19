using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleServerLib.Client
{
    public class FightClientsList
    {
        public int Key { get; set; }

        //private Dictionary<int, int> pcList = new Dictionary<int, int>();
        ///// <summary>
        ///// 玩家列表
        ///// </summary>
        //public Dictionary<int, int> PcList
        //{
        //    get { return pcList; }
        //}

        private List<FightClients> fightList = new List<FightClients>();

        public List<FightClients> FightList
        {
            get { return fightList; }
        }

        /// <summary>
        /// 添加等待队列
        /// </summary>
        /// <param name="client">玩家信息</param>
        public void AddFight(FightClients fight)
        {
            //int zoneId;
            //if (PcList.TryGetValue(fight.Client1.Uid, out zoneId))
            //{
            //    for (int i = 0; i < clientList.Count; i++)
            //    {
            //        BattleClient tempClient = clientList[i];
            //        if (tempClient.Uid.Equals(client.Uid))
            //        {
            //            clientList.RemoveAt(i);
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    PcList.Add(fight.Client1.Uid, zoneId);
            //}
            fightList.Add(fight);
        }

        /// <summary>
        /// 移除等待队列
        /// </summary>
        /// <param name="clientUid">PC UID</param>
        public void RemoveFight(int clientUid)
        {
            //int zoneId;
            //if (PcList.TryGetValue(clientUid, out zoneId))
            //{
            //    PcList.Remove(clientUid);
            //}

            for (int i = 0; i < fightList.Count; i++)
            {
                FightClients tempClient = fightList[i];
                if (tempClient.Client1.Uid.Equals(clientUid))
                {
                    fightList.RemoveAt(i);
                    break;
                }
            }
        }

        public FightClients GetFightBattleClientAndRemoveFight(int clientUid)
        {
            for (int i = 0; i < fightList.Count; i++)
            {
                FightClients tempClient = fightList[i];
                if (tempClient.Client1!= null && tempClient.Client1.Uid.Equals(clientUid))
                {
                    fightList.RemoveAt(i);
                    return tempClient;
                }
                else if (tempClient.Client2 != null && tempClient.Client2.Uid.Equals(clientUid))
                {
                    fightList.RemoveAt(i);
                    return tempClient;
                }
            }
            Log.Warn("player {0} GetFightBattleClientAndRemoveFight not find fight", clientUid);
            return null;
        }

        //public BattleClient GetFightBattleClientAndRemoveFight(int clientUid)
        //{
        //    for (int i = 0; i < fightList.Count; i++)
        //    {
        //        FightClients tempClient = fightList[i];
        //        if (tempClient.Client1.Uid.Equals(clientUid))
        //        {
        //            fightList.RemoveAt(i);
        //            return tempClient.Client2;
        //        }
        //        else if (tempClient.Client2.Uid.Equals(clientUid))
        //        {
        //            fightList.RemoveAt(i);
        //            return tempClient.Client1;
        //        }
        //    }
        //    return null;
        //}
    }
}

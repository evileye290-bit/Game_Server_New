using ServerFrame;
using ServerShared;
using System.Collections.Generic;

namespace BarrackServerLib
{
    public class GateServerManager : FrontendServerManager
    {
        // key mainId， value 每个逻辑服下面的所有gate, 并依据在线人数从低到高排序
        private Dictionary<int, List<GateServer>> mainIdGateList = new Dictionary<int, List<GateServer>>();

        public GateServerManager(BaseApi api, ServerType serverType ):base(api, serverType)
        {
        }

        public override void DestroyServer(FrontendServer server)
        {
            base.DestroyServer(server);
            CalcLoginGate(server.MainId);
        }

        // 对该main_id下所有服的gate排序
        public void CalcLoginGate(int main_id)
        {
            List<GateServer> gates = null;
            if (!mainIdGateList.TryGetValue(main_id, out gates))
            {
                gates = new List<GateServer>();
                mainIdGateList.Add(main_id, gates);
            }
            gates.Clear();

            // 找到该main id 下所有gate，根据负载排序
            string prefixKey = string.Format("{0}_", main_id);
            foreach (var item in serverList)
            {
                if (item.Value.State == ServerState.Started && item.Key.StartsWith(prefixKey))
                {
                    gates.Add((GateServer)item.Value);
                }
            }

            gates.Sort((left, right) =>
            {
                if (left.ClientCount < right.ClientCount)
                {
                    return -1;
                }
                return 1;
            });
        }

        public GateServer GetLoginGate(int main_id)
        {
            List<GateServer> gates = null;
            mainIdGateList.TryGetValue(main_id, out gates);
            if (gates == null || gates.Count == 0)
            {
                return null;
            }

            // 随机进入人数最少的前1/4的gate
            int count = gates.Count;
            int quater = count / 4;
            int index = BaseApi.Random.Next(0, quater + 1);
            if (index < count)
            {
                int i = 0;
                foreach (var item in gates)
                {
                    if (i == index)
                    {
                        return item;
                    }
                    i++;
                }
            }
            else
            {
                return gates[0];
            }
            return null;
        }

        public int GetOnlineCount()
        {
            int count = 0;
            mainIdGateList.ForEach(kv => kv.Value.ForEach(x => count += x.InGameCount));
            return count;
        }
    }
}

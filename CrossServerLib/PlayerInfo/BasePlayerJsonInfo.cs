using Logger;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib.PlayerInfo
{
    public class BasePlayerJsonInfo
    {
        private Dictionary<int, JsonPlayerInfo> playerList = new Dictionary<int, JsonPlayerInfo>();

        protected int group;
        private CrossServerApi server { get; set; }
        public BasePlayerJsonInfo(CrossServerApi server, int group)
        {
            this.server = server;
            this.group = group;
            LoadInfoFromRedis();
        }

        public void LoadInfoFromRedis()
        {
            OperateGetCrossPlayerJsonInfo op = new OperateGetCrossPlayerJsonInfo(group);
            server.CrossRedis.Call(op, ret =>
            {
                playerList = op.Players;
            });
        }

        public void AddPlayerInfo(int uid, JsonPlayerInfo info)
        {
            if (info != null)
            {
                //info.UpdateTime = server.Now();
                playerList[uid] = info;
                server.CrossRedis.Call(new OperateAddCrossBossPlayerJsonInfo(group, uid, info));
            }
        }

        public JsonPlayerInfo GetPlayerInfo(int uid)
        {
            JsonPlayerInfo info;
            playerList.TryGetValue(uid, out info);
            return info;
        }

    }
}

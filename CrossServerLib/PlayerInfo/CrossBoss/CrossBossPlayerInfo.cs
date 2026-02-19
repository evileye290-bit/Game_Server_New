//using RedisUtility;
//using ServerModels;
//using System.Collections.Generic;

//namespace CrossServerLib.PlayerInfo.HiddenWeapon
//{
//    public class CrossBossPlayerInfo : BasePlayerJsonInfo
//    {
//        //private Dictionary<int, JsonPlayerInfo> playerList = new Dictionary<int, JsonPlayerInfo>();

//        //private CrossServerApi server { get; set; }
//        //public CrossBossPlayerInfo(CrossServerApi server) : base(server)
//        //{
//        //    //this.server = server;

//        //    //LoadInfoFromRedis();
//        //}

//        //public void LoadInfoFromRedis()
//        //{
//        //    OperateGetCrossPlayerJsonInfo op = new OperateGetCrossPlayerJsonInfo();
//        //    server.CrossRedis.Call(op, ret =>
//        //    {
//        //        playerList = op.Players;
//        //    });
//        //}

//        //public void AddPlayerInfo(int uid, JsonPlayerInfo info)
//        //{
//        //    if (info != null)
//        //    {
//        //        //info.UpdateTime = server.Now();
//        //        playerList[uid] = info;
//        //        server.CrossRedis.Call(new OperateAddCrossBossPlayerJsonInfo(uid, info));
//        //    }
//        //}

//        //public JsonPlayerInfo GetPlayerInfo(int uid)
//        //{
//        //    JsonPlayerInfo info;
//        //    playerList.TryGetValue(uid, out info);
//        //    return info;
//        //}

//    }
//}

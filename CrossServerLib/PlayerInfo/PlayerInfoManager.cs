using CrossServerLib.PlayerInfo;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace CrossServerLib
{
    public class PlayerInfoManager
    {
        //private CrossServerApi server { get; set; }

        //public HiddenWeaponPlayerInfo HiddenWeaponMng { get; set; }
        //public SeaTreasurePlayerInfo SeaTreasureMng { get; set; }
        //public  CrossBossPlayerInfo CrossBossMng { get; set; }

        private Dictionary<int, BasePlayerJsonInfo> groupPlayerInfos = new Dictionary<int, BasePlayerJsonInfo>();
        public PlayerInfoManager(CrossServerApi server)
        {
            //this.server = server;
            //SeaTreasureMng = new SeaTreasurePlayerInfo(server);
            //HiddenWeaponMng = new HiddenWeaponPlayerInfo(server);
            //CrossBossMng = new CrossBossPlayerInfo(server);

            foreach (var group in CrossBattleLibrary.GroupList)
            {
                BasePlayerJsonInfo jsonInfo = new BasePlayerJsonInfo(server, group.Key);
                groupPlayerInfos.Add(group.Key, jsonInfo);
            }
        }

        public void AddPlayerInfo(int group, int uid, JsonPlayerInfo info)
        {
            BasePlayerJsonInfo infoList;
            if (groupPlayerInfos.TryGetValue(group, out infoList))
            {
                infoList.AddPlayerInfo(uid, info);
            }
        }

        public JsonPlayerInfo GetJsonPlayerInfo(int group, int uid)
        {
            JsonPlayerInfo info = null;
            BasePlayerJsonInfo infoList;
            if (groupPlayerInfos.TryGetValue(group, out infoList))
            {
                info = infoList.GetPlayerInfo(uid);
            }
            return info;
        }

        //public void AddPlayerInfo(RankType type, int uid, RedisPlayerInfo info)
        //{
        //    if (info != null)
        //    {
        //        switch (type)
        //        {
        //            case RankType.SeaTreasure:
        //                SeaTreasureMng.AddPlayerInfo(uid, info);
        //                break;
        //            case RankType.HidderWeapon:
        //                HiddenWeaponMng.AddPlayerInfo(uid, info);
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

        //public RedisPlayerInfo GetPlayerInfo(RankType type, int uid)
        //{
        //    RedisPlayerInfo info = null;
        //    switch (type)
        //    {
        //        case RankType.SeaTreasure:
        //            info = SeaTreasureMng.GetPlayerInfo(uid);
        //            break;
        //        case RankType.HidderWeapon:
        //            info = HiddenWeaponMng.GetPlayerInfo(uid);
        //            break;
        //        default:
        //            break;
        //    }
        //    return info;
        //}

        //public JsonPlayerInfo GetJsonPlayerInfo(RankType type, int uid)
        //{
        //    JsonPlayerInfo info = null;
        //    switch (type)
        //    {
        //        case RankType.CrossBoss:
        //        case RankType.CrossBossSite:
        //            info = CrossBossMng.GetPlayerInfo(uid);
        //            break;
        //        case RankType.SeaTreasure:
        //            info = SeaTreasureMng.GetPlayerInfo(uid);
        //            break;
        //        case RankType.HidderWeapon:
        //            info = HiddenWeaponMng.GetPlayerInfo(uid);
        //            break;
        //        default:
        //            break;
        //    }
        //    return info;
        //}
    }
}

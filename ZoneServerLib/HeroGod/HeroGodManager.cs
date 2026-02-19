using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;
using RedisUtility;

namespace ZoneServerLib
{
    public class HeroGodManager
    {
        private PlayerChar owner;
        //当前装备的神位
        private Dictionary<int, int> equipedGodList = new Dictionary<int, int>();
        private Dictionary<int, HeroGodInfo> heroGodInfos = new Dictionary<int, HeroGodInfo>();

        public Dictionary<int, int> EquipedGodList => equipedGodList;

        public HeroGodManager(PlayerChar player)
        {
            owner = player;
        }

        public void BindHeroGodInfo(List<HeroGodInfo> heroInfos)
        {
            heroInfos.ForEach(x=>
            {
                heroGodInfos.Add(x.HeroId, x);

                int equipedGod = 0;
                if (x.EquipGodType > 0)
                {
                    equipedGod = x.EquipGodType;
                }
                else
                {
                    equipedGod = x.GodType.Count>0 ? x.GodType.First() : 0;
                }

                equipedGodList.Add(x.HeroId, equipedGod);
            });
        }

        public HeroGodInfo GetHeroGodInfo(int heroId)
        {
            HeroGodInfo info;
            heroGodInfos.TryGetValue(heroId, out info);
            return info;
        }

        public int GetHeroGodType(int heroId)
        {
            HeroGodInfo info = GetHeroGodInfo(heroId);
            return info != null ? info.EquipGodType : 0;
        }

        public void AddHeroGodInfo(int heroId, int godType)
        {
            if (heroGodInfos.ContainsKey(heroId)) return;

            HeroGodModel model = GodHeroLibrary.GetHeroGodModel(heroId);
            if(model == null || !model.HaveTheGodType(godType))
            {
                return;
            }

            HeroGodInfo info = new HeroGodInfo()
            {
                HeroId = heroId,
                GodType = new List<int>() { godType },
            };

            AddHeroGodInfo(info, godType);
        }

        public void AddHeroGodInfo(HeroGodInfo info, int godType)
        {
            if (!heroGodInfos.ContainsKey(info.HeroId))
            {
                heroGodInfos.Add(info.HeroId, info);
                SetEquipedGod(info, godType);

                SyncHeroGodInfo2Client(info);
                SyncInsertHeroGodInfoIntoDB(info);
            }
        }

        public void UnlockNewGodType(HeroGodInfo info, int type)
        {
            if (!info.GodType.Contains(type))
            {
                info.GodType.Add(type);

                SetEquipedGod(info, type);

                SyncHeroGodInfo2Client(info);
            }
        }

        public bool SetEquipedGod(HeroGodInfo info, int type)
        {
            if (!info.GodType.Contains(type) || info.EquipGodType == type) return false;

            info.EquipGodType = type;
            equipedGodList[info.HeroId] = type;

            owner.UpdateHeroGodType(info.HeroId, type);

            owner.HeroMng.UpdateBattlePower(info.HeroId);
            owner.HeroMng.NotifyClientBattlePowerFrom(info.HeroId);

            SyncUpdateHeroGodInfoIntoDB(info);
            owner.server.GameRedis.Call(new OperateSetHeroGod(owner.Uid, equipedGodList.ToString("|", ":")));

            return true;
        }

        public MSG_ZGC_HERO_GOD_INFO_LIST GenerateHeroGodInfoList()
        {
            MSG_ZGC_HERO_GOD_INFO_LIST msg = new MSG_ZGC_HERO_GOD_INFO_LIST();

            heroGodInfos.ForEach(x =>msg.GodHeroList.Add(GenerateHeroGodInfo(x.Value)));

            return msg;
        }

        public MSG_ZGC_HERO_GOD_INFO GenerateHeroGodInfo(HeroGodInfo info)
        {
            MSG_ZGC_HERO_GOD_INFO msg = new MSG_ZGC_HERO_GOD_INFO() { HeroId = info.HeroId, EquipedGodType = info.EquipGodType };
            msg.GodType.AddRange(info.GodType);
            return msg;
        }

        public MSG_ZMZ_HERO_GOD_INFO_LIST GenerateTransformMsg()
        {
            MSG_ZMZ_HERO_GOD_INFO_LIST msg = new MSG_ZMZ_HERO_GOD_INFO_LIST();

            heroGodInfos.ForEach(x => msg.GodHeroList.Add(GenerateHeroGodTransformInfo(x.Value)));

            return msg;
        }

        public MSG_ZMZ_HERO_GOD_INFO GenerateHeroGodTransformInfo(HeroGodInfo info)
        {
            MSG_ZMZ_HERO_GOD_INFO msg = new MSG_ZMZ_HERO_GOD_INFO() { HeroId = info.HeroId, EquipedGodType = info.EquipGodType };
            msg.GodType.AddRange(info.GodType);
            return msg;
        }

        public void SyncHeroGodInfo2Client(HeroGodInfo info)
        {
            owner.Write(GenerateHeroGodInfo(info));
        }

        public void SyncInsertHeroGodInfoIntoDB(HeroGodInfo info)
        {
            owner.server.GameDBPool.Call(new QueryInsertHeroGod(owner.Uid, info));
        }

        public void SyncUpdateHeroGodInfoIntoDB(HeroGodInfo info)
        {
            owner.server.GameDBPool.Call(new QueryUpdateHeroGod(owner.Uid, info));
        }
    }
}

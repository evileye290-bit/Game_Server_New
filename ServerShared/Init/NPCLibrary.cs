using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class NPCLibrary
    {
        private static Dictionary<int, NPCModel> npcList = new Dictionary<int, NPCModel>();
        private static Dictionary<int, ZoneNPCModel> zoneNPCList = new Dictionary<int, ZoneNPCModel>();

        public static Dictionary<int, ZoneNPCModel> ZoneNPCList
        { get { return zoneNPCList; } }

        public static void Init()
        {
            Dictionary<int, NPCModel> npcList = new Dictionary<int, NPCModel>();
            Dictionary<int, ZoneNPCModel> zoneNPCList = new Dictionary<int, ZoneNPCModel>();
            //npcList.Clear();
            //zoneNPCList.Clear();

            Data data;
            NPCModel model = null;
            DataList dataList = DataListManager.inst.GetDataList("NPC");
            foreach (var item in dataList)
            {
                data = item.Value;
                model = new NPCModel(data);
                npcList.Add(item.Key, model);
            }

            ZoneNPCModel zoneNPCModel = null;
            dataList = DataListManager.inst.GetDataList("ZoneNPC");
            foreach (var item in dataList)
            {
                data = item.Value;
                zoneNPCModel = new ZoneNPCModel(data);
                zoneNPCList.Add(item.Key, zoneNPCModel);
                if (zoneNPCModel.Params.ContainsKey(NpcParamType.FLY_MAP_ID))
                {
                    CheckPosInMap(zoneNPCModel);
                }

            }
            NPCLibrary.npcList = npcList;
            NPCLibrary.zoneNPCList = zoneNPCList;
        }

        public static void CheckPosInMap(ZoneNPCModel npc)
        {
            int mapId = npc.Params[NpcParamType.FLY_MAP_ID].ToInt();
            float x = npc.Params[NpcParamType.POS_X].ToFloat();
            float y = npc.Params[NpcParamType.POS_Y].ToFloat();

            MapModel model = MapLibrary.GetMap(mapId);
            Vec2 targetPos = new Vec2(x, y);
            if (!model.CheckStrictPosInMap(new Vec2(x, y)))
            {
                Log.Warn($"npc {npc.Id} pos {targetPos} is not strict in map {mapId}");
            }
            if (!model.CheckNoneStrictPosInMap(new Vec2(x, y)))
            {
                Log.Warn($"npc {npc.Id} pos {targetPos} is not in map {mapId}");
            }
        }

        public static NPCModel GetNPCModel(int id)
        {
            NPCModel model = null;
            npcList.TryGetValue(id, out model);
            return model;
        }

        public static ZoneNPCModel GetZoneNPCModel(int id)
        {
            ZoneNPCModel model = null;
            zoneNPCList.TryGetValue(id, out model);
            return model;
        }
    }
}

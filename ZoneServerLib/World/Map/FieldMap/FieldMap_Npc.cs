using System.Collections.Generic;
using CommonUtility;
using DataProperty;
using Message.Manager.Protocol.MZ;
using System;
using System.Linq;
using Logger;
using EpPathFinding;
using System.IO;
using Message.Relation.Protocol.RZ;
using DBUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZM;
using ServerShared.Map;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {
        private Dictionary<int, NPC> npcList = new Dictionary<int, NPC>();
        /// <summary>
        /// NPC
        /// </summary>
        public IReadOnlyDictionary<int, NPC> NpcList
        {
            get { return npcList; }
        }


        private Dictionary<int, int> zoneNpcIdList = new Dictionary<int, int>();

        private void UpdateNpc(float dt)
        {
            foreach (var npc in npcList)
            {
                try
                {
                    npc.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }


        #region 初始化
        private void InitNPC()
        {
            ZoneNPCModel model = null;
            foreach (var zoneNpc in NPCLibrary.ZoneNPCList)
            {
                model = zoneNpc.Value;
                if (model.ZoneId != MapId)
                {
                    //不是当前地图NPC
                    continue;
                }

                int npcId = model.NpcId;
                ////获取NPC信息
                //Data npcData = npcDataList.Get(npcId);
                //if (npcData == null)
                //{
                //    Log.Warn("Map {0} Init Npc error： not find Npc {1}", MapId, npcId);
                //    continue;
                //}
                //创建NPC
                NPC npc = NpcFactory.CreateNpc(server, npcId);
                npc.Init(this, model);
                AddNpc(npc);


                if (npc.CanRearchMapId > 0)
                {
                    AddCanRearchMaps(npc.CanRearchMapId, npc.InstanceId);
                    if (!neighborMap.Contains(npc.CanRearchMapId))
                    {
                        neighborMap.Add(npc.CanRearchMapId);
                    }
                }
            }
        }

        private void AddCanRearchMaps(int mapId, int instanceId)
        {
            List<int> list;
            if (canRearchMaps.TryGetValue(mapId, out list))
            {
                list.Add(instanceId);
            }
            else
            {
                list = new List<int>();
                list.Add(instanceId);
                canRearchMaps.Add(mapId, list);
            }
        }

        private void AddNpc(NPC npc)
        {
            npc.SetInstanceId(TokenId);

            npcList.Add(npc.InstanceId, npc);
            zoneNpcIdList.Add(npc.ZoneNpcId, npc.InstanceId);
            AddObjectSimpleInfo(npc.InstanceId, TYPE.NPC);
        }

        public void DisappearIntegralBossNpc()
        {
            MSG_ZGC_NPC_DISAPPEAR notify = new MSG_ZGC_NPC_DISAPPEAR();
            foreach (var item in npcList)
            {
                NPC npc = item.Value;

                if (npc.IsIntegralBossNPC && npc.IsVisable)
                {
                    npc.IsVisable = false;
                    notify.NPCList.Add(npc.ZoneNpcId);
                }
            }

            if (notify.NPCList.Count > 0)
            {
                BroadCast(notify);
            }
        }

        public void AppearIntegralBossNpc()
        {
            MSG_ZGC_NPC_APPEAR notify = new MSG_ZGC_NPC_APPEAR();
            foreach (var item in NpcList)
            {
                NPC npc = item.Value;
                if (npc.IsIntegralBossNPC && !npc.IsVisable)
                {
                    npc.IsVisable = true;
                    notify.NPCList.Add(npc.GetNpcPacketInfo());
                }
            }
            if (notify.NPCList.Count != 0)
            {
                BroadCast(notify);
            }
        }

        public void DisappearThemeBossNpc(int period)
        {
            MSG_ZGC_NPC_DISAPPEAR notify = new MSG_ZGC_NPC_DISAPPEAR();
            foreach (var item in npcList)
            {
                NPC npc = item.Value;

                if (npc.IsThemeBossNPC && npc.IsVisable)
                {
                    int npcId = ThemeBossLibrary.GetThemeBossNpcByPeriod(period);
                    if (npc.ZoneNpcId == npcId)
                    {
                        npc.IsVisable = false;
                        notify.NPCList.Add(npc.ZoneNpcId);
                    }
                }
            }

            if (notify.NPCList.Count > 0)
            {
                BroadCast(notify);
            }
        }

        public void AppearThemeBossNpc(int period)
        {
            MSG_ZGC_NPC_APPEAR notify = new MSG_ZGC_NPC_APPEAR();
            foreach (var item in NpcList)
            {
                NPC npc = item.Value;
                if (npc.IsThemeBossNPC && !npc.IsVisable)
                {
                    int npcId = ThemeBossLibrary.GetThemeBossNpcByPeriod(period);
                    if (npc.ZoneNpcId == npcId)
                    {
                        npc.IsVisable = true;
                        notify.NPCList.Add(npc.GetNpcPacketInfo());
                    }
                }
            }
            if (notify.NPCList.Count != 0)
            {
                BroadCast(notify);
            }
        }

        public void DisappearCarnivalBossNpc()
        {
            MSG_ZGC_NPC_DISAPPEAR notify = new MSG_ZGC_NPC_DISAPPEAR();
            foreach (var item in npcList)
            {
                NPC npc = item.Value;

                if (npc.IsCarnivalBossNPC && npc.IsVisable)
                {
                    if (npc.ZoneNpcId == CarnivalBossLibrary.BossNpcId)
                    {
                        npc.IsVisable = false;
                        notify.NPCList.Add(npc.ZoneNpcId);
                    }
                }
            }

            if (notify.NPCList.Count > 0)
            {
                BroadCast(notify);
            }
        }

        public void AppearCarnivalBossNpc()
        {
            MSG_ZGC_NPC_APPEAR notify = new MSG_ZGC_NPC_APPEAR();
            foreach (var item in NpcList)
            {
                NPC npc = item.Value;
                if (npc.IsCarnivalBossNPC && !npc.IsVisable)
                {                   
                    if (npc.ZoneNpcId == CarnivalBossLibrary.BossNpcId)
                    {
                        npc.IsVisable = true;
                        notify.NPCList.Add(npc.GetNpcPacketInfo());
                    }
                }
            }
            if (notify.NPCList.Count != 0)
            {
                BroadCast(notify);
            }
        }
        #endregion
    }
}
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

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {
        private Dictionary<int, ObjectSimpleInfo> allObjectList = new Dictionary<int, ObjectSimpleInfo>();

        #region PlayerChar
        public PlayerChar GetPlayer(int instance_id)
        {
            PlayerChar pc;
            pcList.TryGetValue(instance_id, out pc);
            return pc;
        }

        public void GetPlayerInRange(Vec2 center, float range, List<FieldObject> list, int instance_id = -1, int limit = CONST.SEARCH_LIMIT_COUNT, bool alive = true)
        {
            if (list.Count >= limit) return;
            // 全图同步 不需要遍历周围格子
            if(AoiType == AOIType.All)
            {
                foreach( var player in pcList)
                {
                    if (player.Value.IsObserver)
                    {
                        continue;
                    }
                    if (player.Value.InstanceId != instance_id && MATH.IsInRange(center, player.Value.Position, range + player.Value.Radius))
                    {
                        if (alive == true && player.Value.IsDead == false)
                        {
                            list.Add(player.Value);
                        }
                        else if (alive == false && player.Value.IsDead)
                        {
                            list.Add(player.Value);
                        }
                        if (list.Count >= limit)
                        {
                            return;
                        }
                    }
                }
                return;
            }
            Region region = regionMgr.GetRegion(center);
            if (region != null)
            {
                // 当前格子
                foreach (var player in region.PlayerList)
                {
                    //如果是观战者，不会被选中
                    if (player.Value.IsObserver)
                    {
                        continue;
                    }
                    if (player.Value.InstanceId != instance_id && MATH.IsInRange(center, player.Value.Position, range + player.Value.Radius))
                    {
                        if (alive == true && player.Value.IsDead == false)
                        {
                            list.Add(player.Value);
                        }
                        else if (alive == false && player.Value.IsDead)
                        {
                            list.Add(player.Value);
                        }
                        if (list.Count >= limit)
                        {
                            return;
                        }
                    }
                }
                // 周围格子
                for (int i = 0; i < 8; i++)
                {
                    if (region.NeighborList[i] != null)
                    {
                        foreach (var player in region.NeighborList[i].PlayerList)
                        {
                            if (player.Value.InstanceId != instance_id && MATH.IsInRange(center, player.Value.Position, range + player.Value.Radius))
                            {
                                //如果是观战者，不会被选中
                                if (player.Value.IsObserver)
                                {
                                    continue;
                                }
                                if (alive == true && player.Value.IsDead == false)
                                {
                                    list.Add(player.Value);
                                }
                                else if (alive == false && player.Value.IsDead)
                                {
                                    list.Add(player.Value);
                                }
                                if (list.Count >= limit)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region NPC
        public NPC GetNpcByCanReachMapId(int mapId)
        {
            List<int> list;
            if (canRearchMaps.TryGetValue(mapId, out list))
            {
                if (list.Count > 0)
                {
                    return GetNpc(list[0]);
                }
            }

            return null;
        }

        public NPC GetNpc(int instanceId)
        {
            NPC npc;
            npcList.TryGetValue(instanceId, out npc);
            return npc;
        }

        public NPC GetNpcById(int zoneNpcId)
        {
            int instanceId;
            if (zoneNpcIdList.TryGetValue(zoneNpcId, out instanceId))
            {
                return GetNpc(instanceId);
            }
            else
            {
                return null;
            }
        }

        public void GetNPCInRange(Vec2 center, float range, List<FieldObject> list, int instance_id = -1, int limit = ServerShared.CONST.SEARCH_LIMIT_COUNT)
        {
            if (list.Count >= limit) return;
            foreach (var pair in npcList)
            {
                if (pair.Value.InstanceId != instance_id && MATH.IsInRange(center, pair.Value.Position, range + pair.Value.Radius))
                {
                    TargetAdd(list, pair.Value, limit);
                }
            }
        }

        #endregion

        #region Goods
        public Goods GetGoods(int instanceId)
        {
            Goods goods;
            goodsList.TryGetValue(instanceId, out goods);
            return goods;
        }

        public Goods GetGoodsById(int goodsId)
        {
            int instanceId;
            if (goodsNameList.TryGetValue(goodsId, out instanceId))
            {
                return GetGoods(instanceId);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region PropBook
        public PropBook GetProp(int instanceId)
        {
            PropBook prop;
            propBookList.TryGetValue(instanceId, out prop);
            return prop;
        }

        public PropBook GetPropById(int propId)
        {
            int instanceId;
            if (propBookNameList.TryGetValue(propId, out instanceId))
            {
                return GetProp(instanceId);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Pet
        public Pet GetPet(int instanceId)
        {
            Pet pet;
            petList.TryGetValue(instanceId, out pet);
            return pet;
        }
        #endregion

        #region Hero
        public Hero GetHero(int instanceId)
        {
            Hero hero;
            heroList.TryGetValue(instanceId, out hero);
            return hero;
        }
        #endregion

        #region FieldObject

        public void AddObjectSimpleInfo(int instanceId, TYPE type)
        {
            ObjectSimpleInfo info = new ObjectSimpleInfo();
            info.InstanceId = instanceId;
            info.FieldObjectType = type;
            allObjectList[instanceId] = info;
        }

        public void RemoveObjectSimpleInfo(int instanceId)
        {
            allObjectList.Remove(instanceId);
        }

        public ObjectSimpleInfo GetObjectSimpleInfo(int instanceId)
        {
            ObjectSimpleInfo info;
            allObjectList.TryGetValue(instanceId, out info);
            return info;
        }

        // 慎重调用此接口 绝大多数情况下可以调用FieldObject.GetNearbyFieldObject替代
        public FieldObject GetFieldObject(int instanceId)
        {
            if (instanceId > 0)
            {
                ObjectSimpleInfo info = GetObjectSimpleInfo(instanceId);
                if (info != null)
                {
                    return GetFieldObject(info.FieldObjectType, info.InstanceId);
                }
            }
            return null;
        }

        public FieldObject GetFieldObject(TYPE type, int instanceId)
        {
            switch (type)
            {
                case TYPE.PC:
                    PlayerChar player = null;
                    PcList.TryGetValue(instanceId, out player);
                    return player;
                case TYPE.ROBOT:
                    Robot robot = null;
                    RobotList.TryGetValue(instanceId, out robot);
                    return robot;
                case TYPE.NPC:
                    NPC npc = null;
                    NpcList.TryGetValue(instanceId, out npc);
                    return npc;
                case TYPE.PET:
                    Pet pet = null;
                    PetList.TryGetValue(instanceId, out pet);
                    return pet;
                case TYPE.HERO:
                    Hero hero = null;
                    HeroList.TryGetValue(instanceId, out hero);
                    return hero;
                case TYPE.GOODS:
                    Goods goods = null;
                    GoodsList.TryGetValue(instanceId, out goods);
                    return goods;
                case TYPE.MONSTER:
                    Monster monster = null;
                    monsterList.TryGetValue(instanceId, out monster);
                    return monster;
                case TYPE.FLAG:
                default:
                    return null;
            }
        }

        //public List<FieldObject> GetFieldObjectInRange(Vec2 center, float range, TYPE type, int instance_id = -1, int limit = ServerShared.CONST.SEARCH_LIMIT_COUNT)
        //{
        //    List<FieldObject> allobj = new List<FieldObject>();
        //    switch (type)
        //    {
        //        case TYPE.PC:
        //            GetPlayerInRange(center, range, allobj, instance_id, limit);
        //            break;
        //        case TYPE.NPC:
        //            GetNPCInRange(center, range, allobj, instance_id, limit);
        //            break;
        //        case TYPE.ALL:
        //            GetPlayerInRange(center, range, allobj, instance_id, limit);
        //            break;
        //        default:
        //            break;
        //    }
        //    return allobj;
        //}

        //public FieldObject GetNearestFieldObjectInRange(Vec2 center, float range, TYPE type)
        //{
        //    if (range == 0.0f)
        //    {
        //        return null;
        //    }

        //    List<FieldObject> foList = new List<FieldObject>();
        //    switch (type)
        //    {
        //        case TYPE.PC:
        //            GetPlayerInRange(center, range, foList);
        //            break;
        //        case TYPE.NPC:
        //            GetNPCInRange(center, range, foList);
        //            break;
        //        default:
        //            break;
        //    }

        //    if (foList != null && foList.Count > 0)
        //    {
        //        return foList.OrderBy(pos => (center - pos.Position).magnitudePower).FirstOrDefault();
        //    }
        //    return null;
        //}

        #endregion

        #region Treasure
        public Treasure GetTreasure(int instanceId)
        {
            Treasure treasure;
            treasureList.TryGetValue(instanceId, out treasure);
            return treasure;
        }

        public Treasure GetTreasureById(int treasureId)
        {
            int instanceId;
            if (treasureNameList.TryGetValue(treasureId, out instanceId))
            {
                return GetTreasure(instanceId);
            }
            else
            {
                return null;
            }
        }
        #endregion

        private void TargetAdd(List<FieldObject> list, FieldObject target, int limit)
        {
            if (list.Count >= limit) return;
            list.Add(target);
        }

    }
    
}
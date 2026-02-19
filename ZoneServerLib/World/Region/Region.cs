using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using CommonUtility;
using System.IO;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    // Region相邻8个格子方向 顺时针递增
    public enum RegionDirection
    {
        REGION_N = 0,
        REGION_NE = 1,
        REGION_E = 2,
        REGION_SE = 3,
        REGION_S = 4,
        REGION_SW = 5,
        REGION_W = 6,
        REGION_NW = 7
    }

    public partial class Region : IFieldObjectContainer
    {
        Region[] neighborList = new Region[8];
        public Region[] NeighborList
        { get { return neighborList; } }

        public int index;
        public int x;
        public int y;
        public int width;
        public int height;
        private FieldMap map;

        Dictionary<int, PlayerChar> playerList = new Dictionary<int, PlayerChar>();
        public Dictionary<int, PlayerChar> PlayerList
        { get { return playerList; } }

        Dictionary<int, Robot> robotList = new Dictionary<int, Robot>();
        public Dictionary<int, Robot> RobotList
        { get { return robotList; } }

        Dictionary<int, Pet> petList = new Dictionary<int, Pet>();
        Dictionary<int, Pet> PetList
        { get { return petList; } }

        Dictionary<int, Hero> heroList = new Dictionary<int, Hero>();
        Dictionary<int, Hero> HeroList
        { get { return heroList; } }

        private Dictionary<int, NPC> npcList;
        public Dictionary<int, NPC> NpcList
        { get { return npcList; } }

        private Dictionary<int, PropBook> propBookList;
        public Dictionary<int, PropBook> PropBookList
        { get { return propBookList; } }

        private Dictionary<int, Goods> goodsList;
        public Dictionary<int, Goods> GoodsList
        { get { return goodsList; } }

        private Dictionary<int, Monster> monsterList=new Dictionary<int,Monster>();

        public Dictionary<int, Monster> MonsterList
        {
            get { return monsterList; }
        }


        public void Init(int index, int x, int y, int width, int height, FieldMap map)
        {
            this.index = index;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.map = map;
            npcList = new Dictionary<int, NPC>();
            propBookList = new Dictionary<int, PropBook>();
            goodsList = new Dictionary<int, Goods>();
            //flagList = (Dictionary<int, CampBattleFlag>)map.FlagList;
        }

        public void AddGameObject(FieldObject obj, Vec2 pos)
        {
            //Log.Write("map {0} region index {1} add obj type {2} instance id {3}", map.MapID, index, obj.type.ToString(), obj.Instance_id);
            switch (obj.FieldObjectType)
            {
                case TYPE.PC:
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add player {1} ====11 region index {2}", index, obj.Instance_id, index);
                    playerList[obj.InstanceId] = (PlayerChar)obj;
                    break;
                case TYPE.ROBOT:
                    NotifySurroundFieldObjectIn(obj, true);
                    robotList[obj.InstanceId] = (Robot)obj;
                    break;
                case TYPE.PET:
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add pet {1} ====200 region index {2}", index, obj.Instance_id, index);
                    petList[obj.InstanceId] = (Pet)obj;
                    break;
                case TYPE.HERO :
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add pet {1} ====200 region index {2}", index, obj.Instance_id, index);
                    heroList[obj.InstanceId] = (Hero)obj;
                    break;
                case TYPE.NPC:
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add pet {1} ====200 region index {2}", index, obj.Instance_id, index);
                    npcList[obj.InstanceId] = (NPC)obj;
                    break;
                case TYPE.PROPBOOK:
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add prop {1} ====200 region index {2}", index, obj.Instance_id, index);
                    propBookList[obj.InstanceId] = (PropBook)obj;
                    break;
                case TYPE.MONSTER:
                    NotifySurroundFieldObjectIn(obj, true);
                    //Log.Warn("region index {0} add pet {1} ====200 region index {2}", index, obj.Instance_id, index);
                    monsterList[obj.InstanceId] = (Monster)obj;
                    break;
                default:
                    break;
            }
        }

        public void RemoveGameObject(FieldObject obj)
        {
            //Log.Write("map {0} region index {1} remove obj type {2} instance id {3}", map.MapID, index, obj.type.ToString(), obj.Instance_id);
            switch (obj.FieldObjectType)
            {
                case TYPE.PC:
                    NotifySurroundFieldObjectOut(obj);
                    //Log.Warn("region index {0} remove player {1}=====8 region index {2}", index, obj.Instance_id, index);
                    playerList.Remove(obj.InstanceId);
                    break;
                case TYPE.ROBOT:
                    NotifySurroundFieldObjectOut(obj);
                    robotList.Remove(obj.InstanceId);
                    break;
                case TYPE.PET:
                    NotifySurroundFieldObjectOut(obj);
                    //Log.Warn("region index {0} remove player {1}=====8 region index {2}", index, obj.Instance_id, index);
                    petList.Remove(obj.InstanceId);
                    break;
                case TYPE.HERO:
                    NotifySurroundFieldObjectOut(obj);
                    //Log.Warn("region index {0} remove player {1}=====8 region index {2}", index, obj.Instance_id, index);
                    heroList.Remove(obj.InstanceId);
                    break;
                case TYPE.NPC:
                    NotifySurroundFieldObjectOut(obj);
                    //Log.Warn("region index {0} remove player {1}=====8 region index {2}", index, obj.Instance_id, index);
                    npcList.Remove(obj.InstanceId);
                    break;
                case TYPE.PROPBOOK:
                    NotifySurroundFieldObjectOut(obj);
                    //Log.Warn("region index {0} remove player {1}=====8 region index {2}", index, obj.Instance_id, index);
                    propBookList.Remove(obj.InstanceId);
                    break;
                case TYPE.MONSTER:
                    NotifySurroundFieldObjectOut(obj);
                    monsterList.Remove(obj.InstanceId);
                    break;
                default:
                    break;
            }
        }

        public void LinkNeighbor(RegionDirection diretion, Region neighbor_region)
        {
            neighborList[(int)diretion] = neighbor_region;
        }

        public bool InMyRegions(Region region)
        {
            if (region == null)
            {
                return false;
            }
            if (region.index == index)
            {
                return true;
            }
            for (int i = 0; i < 8; i++)
            {
                if (neighborList[i] != null && neighborList[i].index == region.index)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsNeighbor(Region region)
        {
            if (region == null)
            {
                return false;
            }
            for (int i = 0; i < 8; i++)
            {
                if (neighborList[i] != null && neighborList[i].index == region.index)
                {
                    return true;
                }
            }
            return false;
        }

        private void NotifySurroundFieldObjectIn(FieldObject obj, bool isBorn = false)
        {
            obj.NotifyContainerFieldObjectIn(this, true);
            for (int i = 0; i < 8; i++)
            {
                if (neighborList[i] != null)
                {
                    obj.NotifyContainerFieldObjectIn(neighborList[i], true);
                }
            }
        }

        public void NotifyCurRegionFieldObjectIn(FieldObject obj)
        {
            obj.NotifyContainerFieldObjectIn(this, true);
        }

        private void NotifySurroundFieldObjectOut(FieldObject obj)
        {
            obj.NotifyCoutainerFieldObjectOut(this);
            for (int i = 0; i < 8; i++)
            {
                if (neighborList[i] != null)
                {
                    obj.NotifyCoutainerFieldObjectOut(neighborList[i]);
                }
            }
        }

        public void NotifyCurRegionFieldObjectOut(FieldObject obj)
        {
            obj.NotifyCoutainerFieldObjectOut(this);
        }


        public void EnterRegion(FieldObject obj)
        {
            switch (obj.FieldObjectType)
            {
                case TYPE.PC:
                    PlayerChar player = (PlayerChar)obj;
                    if (player.IsMapLoadingDone == false)
                    {
                        Log.Warn("player {0} enter region before map loading done", player.Uid);
                    }
                    //Log.Warn("region index {0} add pc {1}", index, obj.Instance_id);
                    playerList.Add(obj.InstanceId, (PlayerChar)obj);
                    break;
                case TYPE.ROBOT:
                    robotList.Add(obj.InstanceId, (Robot)obj);
                    break;
                case TYPE.PET:
                    petList.Add(obj.InstanceId, (Pet)obj);
                    break;
                case TYPE.HERO:
                    heroList.Add(obj.InstanceId, (Hero)obj);
                    break;
                case TYPE.NPC:
                    NpcList.Add(obj.InstanceId, (NPC)obj);
                    break;
                case TYPE.PROPBOOK:
                    propBookList.Add(obj.InstanceId, (PropBook)obj);
                    break;
                case TYPE.MONSTER:
                    MonsterList.Add(obj.InstanceId, (Monster)obj);
                    break;
                default:
                    break;
            }
        }

        public void LeaveRegion(FieldObject obj)
        {
            switch (obj.FieldObjectType)
            {
                case TYPE.PC:
                    //Log.Warn("region index {0} remove pc {1}", index, obj.Instance_id);
                    playerList.Remove(obj.InstanceId);
                    break;
                case TYPE.ROBOT:
                    robotList.Remove(obj.InstanceId);
                    break;
                case TYPE.PET:
                    petList.Remove(obj.InstanceId);
                    break;
                case TYPE.HERO:
                    heroList.Remove(obj.InstanceId);
                    break;
                case TYPE.NPC:
                    NpcList.Remove(obj.InstanceId);
                    break;
                case TYPE.PROPBOOK:
                    propBookList.Remove(obj.InstanceId);
                    break;
                case TYPE.MONSTER:
                    MonsterList.Remove(obj.InstanceId);
                    break;
                default:
                    break;
            }
        }

        public FieldObject GetFieldObject(TYPE type, int instance_id)
        {
            if (instance_id == 0)
            {
                return null;
            }
            switch (type)
            {
                case TYPE.PC:
                    PlayerChar player = null;
                    playerList.TryGetValue(instance_id, out player);
                    return player;
                case TYPE.ROBOT:
                    Robot robot = null;
                    robotList.TryGetValue(instance_id, out robot);
                    return robot;
                case TYPE.NPC:
                    NPC npc = null;
                    npcList.TryGetValue(instance_id, out npc);
                    return npc;
                case TYPE.PROPBOOK:
                    PropBook prop = null;
                    propBookList.TryGetValue(instance_id, out prop);
                    return prop;
                case TYPE.GOODS:
                    Goods goods = null;
                    goodsList.TryGetValue(instance_id, out goods);
                    return goods;
                case TYPE.PET:
                    Pet pet = null;
                    petList.TryGetValue(instance_id, out pet);
                    return pet;
                case TYPE.HERO:
                    Hero hero = null;
                    heroList.TryGetValue(instance_id, out hero);
                    return hero;
                case TYPE.MONSTER:
                    Monster monster = null;
                    MonsterList.TryGetValue(instance_id, out monster);
                    return monster;
                //case TYPE.FLAG:
                //    CampBattleFlag flag = null;
                //    flagList.TryGetValue(instance_id, out flag);
                //return flag;
                default:
                    return null;
            }
        }
        // debug
        public void PrintNeigbor()
        {
            for (int i = 0; i < 8; i++)
            {
                if (neighborList[i] != null)
                {
                    //Log.Warn("region index {0} neighbor index {1} direction {2}", index, neighborList[i].index, ((RegionDirection)i).ToString());
                }
                else
                {
                    //Log.Warn("region index {0} neighbor is null direction {1}", index,  ((RegionDirection)i).ToString());
                }
            }
        }

    }

}
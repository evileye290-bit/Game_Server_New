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
        public ZoneServerApi server;

        private RegionManager regionMgr = new RegionManager();
        /// <summary>
        /// 格子管理
        /// </summary>
        public RegionManager RegionMgr
        {
            get { return regionMgr; }
        }

        #region 相邻关系
        private Dictionary<int, List<int>> canRearchMaps = new Dictionary<int, List<int>>();

        private List<int> neighborMap = new List<int>();
        /// <summary>
        /// 邻居地图
        /// </summary>
        public List<int> NeighborMap
        {
            get { return neighborMap; }
        }
        #endregion

        public bool IsDungeon { get { return Model.IsDungeon(); } }

        public bool UseDynamicGrid { get; protected set; }

        public MapType GetMapType()
        {
            return Model.MapType;
        }

        public FieldMap(ZoneServerApi server, int mapId, int channel):base(mapId, channel)
        {
            Reset();

            this.server = server;
            InitRegionManager();
            InitNPC();
//            InitPropBook();
            InitGoods();
            InitMonsterGens();
            InitDynamicGrid();
            InitDomainManager();
            InitTreasure();
        }

        public virtual void Update(float dt)
        {
            try
            {
                //检测移除放于Update开始，以免被移除对象会多更新一次
                UpdateRemove();
                //真正执行
                UpdateFieldObjects(dt);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        void UpdateFieldObjects(float dt)
        {
            UpdatePc(dt);

            UpdateNpc(dt);

            UpdateGoods(dt);

            UpdatePet(dt);

            UpdateHero(dt);

            if((int)((this as DungeonMap)?.State??0) <= 2)
            {
                UpdateMonster(dt);
                UpdateRobot(dt);
            }
        }


        /// <summary>
        /// 移除
        /// </summary>
        void UpdateRemove()
        {
            RemovePc();

            RemoveRobot();

            RemovePet();

            RemoveHero();

            RemoveMonster();
        }


        /// <summary>
        /// 初始化清理
        /// </summary>
        public void Reset()
        {
            playerRemoveList.Clear();
            pcList.Clear();
            npcList.Clear();
            zoneNpcIdList.Clear();
            goodsList.Clear();
            goodsNameList.Clear();
            canRearchMaps.Clear();
            treasureList.Clear();
            treasureNameList.Clear();
        }

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void InitRegionManager()
        {
            regionMgr.Init(this, MaxX - MinX, MaxY - MinY, MinX, MinY);
        }

        public void BroadCast<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (pcList.Count > 0)
            {
                ushort bodyLen = 0;
                ArraySegment<byte> body;
                PlayerChar.BroadCastMsgBodyMaker(msg, out body, out bodyLen);

                foreach (var player in pcList)
                {
                    try
                    {
                        ArraySegment<byte> header;
                        player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                        player.Value.Write(header, body);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("field map broadcast error: {0}", ex);
                    }
                }
            }

            (this as DungeonMap)?.BattleFpsManager.WriteBroadcastMsg(msg);
        }

        public void BroadCastNotPc<T>(T msg, int uid) where T : Google.Protobuf.IMessage
        {
            if (pcList.Count == 0)
                return;
            ArraySegment<byte> body;
            ushort bodyLen = 0;
            PlayerChar.BroadCastMsgBodyMaker(msg, out body, out bodyLen);

            foreach (var player in pcList)
            {
                try
                {
                    if (player.Value.Uid != uid)
                    {
                        ArraySegment<byte> header;
                        player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                        player.Value.Write(header, body);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("field map broadcast not myself error: {0}", ex);
                }
            }
        }

        public virtual MessageDispatcher GetMessageDispatcher()
        {
            return null;
        }

        /// <summary>
        /// 是否需要后端演算战斗过程
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedCheck()
        {
            return true;
        }
    }
}
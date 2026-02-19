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

        private Dictionary<int, Goods> goodsList = new Dictionary<int, Goods>();
        /// <summary>
        /// 采集物
        /// </summary>
        public IReadOnlyDictionary<int, Goods> GoodsList
        {
            get { return goodsList; }
        }

        private Dictionary<int, int> goodsNameList = new Dictionary<int, int>();


        private Dictionary<int, PropBook> propBookList = new Dictionary<int, PropBook>();
        /// <summary>
        /// 采集物
        /// </summary>
        public IReadOnlyDictionary<int, PropBook> PropBookList
        {
            get { return propBookList; }
        }

        private Dictionary<int, int> propBookNameList = new Dictionary<int, int>();


        private Dictionary<int, Treasure> treasureList = new Dictionary<int, Treasure>();
        /// <summary>
        /// 藏宝点
        /// </summary>
        public IReadOnlyDictionary<int, Treasure> TreasureList
        {
            get { return treasureList; }
        }

        private Dictionary<int, int> treasureNameList = new Dictionary<int, int>();

        private void UpdateGoods(float dt)
        {
            foreach (var goods in goodsList)
            {
                try
                {
                    goods.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }



        #region 初始化

        private void InitGoods()
        {

            //var goodsDataList = DataListManager.inst.GetDataList("Goods");
            var zoneGoodsDataList = DataListManager.inst.GetDataList("ZoneGoods");

            foreach (var zoneGoods in zoneGoodsDataList)
            {
                Data zoneGoodsData = zoneGoods.Value;
                if (zoneGoodsData.GetInt("ZoneId") != MapId)
                {
                    //不是当前地图NPC
                    continue;
                }

                //int goodsId = zoneGoodsData.GetInt("goodsId");
                //Data goodsData = goodsDataList.Get(goodsId);
                //if (goodsData == null)
                //{
                //    Log.Warn("Map {0} Init goods error： not find goods {1}", MapId, goodsId);
                //    continue;
                //}

                Goods goods = new Goods(server);
                goods.Init(this, zoneGoodsData);
                AddGoods(goods);

            }
        }

        private void AddGoods(Goods goods)
        {
            goods.SetInstanceId(TokenId);

            goodsList.Add(goods.InstanceId, goods);
            goodsNameList.Add(goods.ZoneGoodsId, goods.InstanceId);
            AddObjectSimpleInfo(goods.InstanceId, TYPE.GOODS);
        }



        private void InitPropBook()
        {
            var propDataList = DataListManager.inst.GetDataList("ZoneProp");
            foreach (var zoneProp  in propDataList)
            {
                Data zonePropData = zoneProp.Value;
                if (zonePropData.GetInt("ZoneId") != MapId)
                {
                    //不是当前地图NPC
                    continue;
                }

                PropBook prop = new PropBook(server);
                prop.Init(this, zonePropData);
                AddProp(prop);
            }
        }

        private void AddProp(PropBook prop)
        {
            prop.SetInstanceId(TokenId);

            propBookList.Add(prop.InstanceId, prop);
            propBookNameList.Add(prop.ZonePropId, prop.InstanceId);
            AddObjectSimpleInfo(prop.InstanceId, TYPE.PROPBOOK);
        }


        private void InitTreasure()
        {
            var zoneTreasureDataList = DataListManager.inst.GetDataList("ZoneShovelTreasure");

            foreach (var zoneTreasure in zoneTreasureDataList)
            {
                Data zoneTreasureData = zoneTreasure.Value;
                if (zoneTreasureData.GetInt("ZoneId") != MapId)
                {
                    //不是当前地图NPC
                    continue;
                }

                Treasure treasure = new Treasure(server);
                treasure.Init(this, zoneTreasureData);
                AddTreasure(treasure);
            }
        }

        private void AddTreasure(Treasure treasure)
        {
            treasure.SetInstanceId(TokenId);

            treasureList.Add(treasure.InstanceId, treasure);
            treasureNameList.Add(treasure.ZoneTreasureId, treasure.InstanceId);//
            AddObjectSimpleInfo(treasure.InstanceId, TYPE.TREASURE);
        }
        #endregion

    }
}
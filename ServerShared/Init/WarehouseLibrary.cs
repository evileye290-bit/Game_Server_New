using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class WarehouseLibrary
    {
        private static Dictionary<int, WareHouseModel> warehouseDic = new Dictionary<int, WareHouseModel>();

        public static void Init()
        {
            InitConfig();
        }

        private static void InitConfig()
        {
            Dictionary<int, WareHouseModel> warehouseDic = new Dictionary<int, WareHouseModel>();

            DataList dataList = DataListManager.inst.GetDataList("Warehouse");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                WareHouseModel configModel = new WareHouseModel(data);
                warehouseDic.Add(configModel.Id, configModel);
            }

            WarehouseLibrary.warehouseDic = warehouseDic;
        }

        public static WareHouseModel GetConfig(int id)
        {
            WareHouseModel model;
            warehouseDic.TryGetValue(id, out model);
            return model;
        }
    }
}

using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class TransferMapLibrary
    {
        private static Dictionary<int, TransferMapModel> mapDic = new Dictionary<int, TransferMapModel>();

        public static void Init()
        {
            InitTransferMap();
        }

        private static void InitTransferMap()
        {
            Dictionary<int, TransferMapModel> mapDic = new Dictionary<int, TransferMapModel>();
            //mapDic.Clear();

            DataList dataList = DataListManager.inst.GetDataList("TransferMap");

            foreach (var item in dataList)
            {
                Data data = item.Value;
                TransferMapModel model = new TransferMapModel(data);
                mapDic.Add(model.MapId, model);
            }
            TransferMapLibrary.mapDic = mapDic;
        }

        public static Vec2 GetBeginPos(int mapId)
        {
            TransferMapModel model;
            mapDic.TryGetValue(mapId, out model);
            if (model != null)
            {
                return model.BeginPos;
            }
            return null;
        }
    }
}

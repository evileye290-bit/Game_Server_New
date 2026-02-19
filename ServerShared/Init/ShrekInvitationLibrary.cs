using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class ShrekInvitationLibrary
    {
        private static Dictionary<int, ShrekInvitationModel> shrekInvitationDic = new Dictionary<int, ShrekInvitationModel>();
        private static DoubleDepthMap<int, int, DiamondRebateModel> diamondRebateDic = new DoubleDepthMap<int, int, DiamondRebateModel>();
        private static Dictionary<int, List<int>> diamondRebateIgnoreWays = new Dictionary<int, List<int>>();

        public static void Init()
        {
            InitShrekInvitationData();

            InitDiamondRebateData();

            InitDiamondRebateConfig();
        }

        private static void InitShrekInvitationData()
        {
            Dictionary<int, ShrekInvitationModel> shrekInvitationDic = new Dictionary<int, ShrekInvitationModel>();

            DataList dataList = DataListManager.inst.GetDataList("ShrekInvitation");
            foreach (var item in dataList)
            {
                ShrekInvitationModel model = new ShrekInvitationModel(item.Value);
                shrekInvitationDic.Add(model.Id, model);
            }

            ShrekInvitationLibrary.shrekInvitationDic = shrekInvitationDic;
        }

        public static ShrekInvitationModel GetShrekInvitationModel(int id)
        {
            ShrekInvitationModel model;
            shrekInvitationDic.TryGetValue(id, out model);
            return model;
        }

        private static void InitDiamondRebateData()
        {
            DoubleDepthMap<int, int, DiamondRebateModel> diamondRebateDic = new DoubleDepthMap<int, int, DiamondRebateModel>();

            DataList dataList = DataListManager.inst.GetDataList("DiamondRebate");
            foreach (var item in dataList)
            {
                DiamondRebateModel model = new DiamondRebateModel(item.Value);
                diamondRebateDic.Add(model.Period, model.Id, model);
            }

            ShrekInvitationLibrary.diamondRebateDic = diamondRebateDic;
        }

        public static DiamondRebateModel GetDiamondRebateModel(int period, int id)
        {
            DiamondRebateModel model;
            diamondRebateDic.TryGetValue(period, id, out model);
            return model;
        }

        private static void InitDiamondRebateConfig()
        {
            Dictionary<int, List<int>> diamondRebateIgnoreWays = new Dictionary<int, List<int>>();

            DataList dataList = DataListManager.inst.GetDataList("DiamondRebateConfig");
            List<int> list;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                list = data.GetString("IgnoreConsumeWays").ToList('|');
                diamondRebateIgnoreWays.Add(data.ID, list);
            }

            ShrekInvitationLibrary.diamondRebateIgnoreWays = diamondRebateIgnoreWays;
        }

        public static List<int> GetDiamondRebateIgnoreWays(int period)
        {
            List<int> ignoreWays;
            diamondRebateIgnoreWays.TryGetValue(period, out ignoreWays);
            return ignoreWays;
        }
    }
}

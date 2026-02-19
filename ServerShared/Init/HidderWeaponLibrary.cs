using DataProperty;
using EnumerateUtility;
using ServerModels;
using ServerModels.HidderWeapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class HidderWeaponLibrary
    {
        private static Dictionary<int, HidderWeaponConfig> hidderWeaponConfigs = new Dictionary<int, HidderWeaponConfig>();
        public static Dictionary<int, NotesConfig> NotesConfigs = new Dictionary<int, NotesConfig>();
        private static Dictionary<int, SeaTreasureConfig> seaTreasureConfigs = new Dictionary<int, SeaTreasureConfig>();
        private static Dictionary<int, SeaTreasureRewardModel> seaTreasureRewards = new Dictionary<int, SeaTreasureRewardModel>();
        private static Dictionary<int, HiddenWeaponRewardModel> hiddenWeaponRewards = new Dictionary<int, HiddenWeaponRewardModel>();
        public static void Init()
        {
            InitNotesConfig();
            InitHidderWeaponConfig();
            InitHiddenWeaponRewards();
            InitSeaTreasureConfig();
            InitSeaTreasureRewards();
        }

        private static void InitHidderWeaponConfig()
        {
            Dictionary<int, HidderWeaponConfig> hidderWeaponConfigs = new Dictionary<int, HidderWeaponConfig>();
            //hidderWeaponConfigs.Clear();

            DataList dataList = DataListManager.inst.GetDataList("HidderWeapon");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                HidderWeaponConfig info = new HidderWeaponConfig(data);
                hidderWeaponConfigs.Add(info.Type, info);
            }
            HidderWeaponLibrary.hidderWeaponConfigs = hidderWeaponConfigs;
        }
        private static void InitHiddenWeaponRewards()
        {
            Dictionary<int, HiddenWeaponRewardModel> hiddenWeaponRewards = new Dictionary<int, HiddenWeaponRewardModel>();
            //hiddenWeaponRewards.Clear();

            DataList dataList = DataListManager.inst.GetDataList("HidderWeaponReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                HiddenWeaponRewardModel info = new HiddenWeaponRewardModel(data);
                hiddenWeaponRewards.Add(info.Id, info);
            }
            HidderWeaponLibrary.hiddenWeaponRewards = hiddenWeaponRewards;
        }


        private static void InitSeaTreasureConfig()
        {
            Dictionary<int, SeaTreasureConfig> seaTreasureConfigs = new Dictionary<int, SeaTreasureConfig>();
            //seaTreasureConfigs.Clear();

            DataList dataList = DataListManager.inst.GetDataList("SeaTreasure");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                SeaTreasureConfig info = new SeaTreasureConfig(data);
                seaTreasureConfigs.Add(info.Type, info);
            }
            HidderWeaponLibrary.seaTreasureConfigs = seaTreasureConfigs;
        }

        private static void InitSeaTreasureRewards()
        {
            Dictionary<int, SeaTreasureRewardModel> seaTreasureRewards = new Dictionary<int, SeaTreasureRewardModel>();
            //seaTreasureRewards.Clear();

            DataList dataList = DataListManager.inst.GetDataList("SeaTreasureReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                SeaTreasureRewardModel info = new SeaTreasureRewardModel(data);
                seaTreasureRewards.Add(info.Id, info);
            }
            HidderWeaponLibrary.seaTreasureRewards = seaTreasureRewards;
        }
        public static SeaTreasureRewardModel GetSeaTreasureReward(int Id)
        {
            SeaTreasureRewardModel info;
            seaTreasureRewards.TryGetValue(Id, out info);
            return info;
        }

        public static HiddenWeaponRewardModel GetHiddenWeaponReward(int Id)
        {
            HiddenWeaponRewardModel info;
            hiddenWeaponRewards.TryGetValue(Id, out info);
            return info;
        }

        public static HidderWeaponConfig GetHidderWeaponConfig(int type)
        {
            HidderWeaponConfig info;
            hidderWeaponConfigs.TryGetValue(type, out info);
            return info;
        }
        public static SeaTreasureConfig GetSeaTreasureConfig(int type)
        {
            SeaTreasureConfig info;
            seaTreasureConfigs.TryGetValue(type, out info);
            return info;
        }

        private static void InitNotesConfig()
        {
            Dictionary<int, NotesConfig> NotesConfigs = new Dictionary<int, NotesConfig>();
            //NotesConfigs.Clear();

            DataList dataList = DataListManager.inst.GetDataList("Notes.xml");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                NotesConfig info = new NotesConfig(data);
                NotesConfigs.Add((int)info.Type, info);
            }
            HidderWeaponLibrary.NotesConfigs = NotesConfigs;
        }
    }
}

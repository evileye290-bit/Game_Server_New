using System.Collections.Generic;
using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ScriptFunctions;
using ScriptInterfaces;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class HiddenWeaponItem : BaseItem
    {
        private HiddenWeaponModel model;
        private HiddenWeaponDbInfo hiddenWeaponInfo;

        public HiddenWeaponModel Model => model;
        public HiddenWeaponDbInfo Info => hiddenWeaponInfo;

        public bool Deleted;

        public HiddenWeaponItem(HiddenWeaponDbInfo info) : base(info)
        {
            this.hiddenWeaponInfo = info;
            this.MainType = MainType.HiddenWeapon;
            this.BindData(info.TypeId);
        }

        public HiddenWeaponItem(HiddenWeaponDbInfo info, bool delete) : this(info)
        {
            this.hiddenWeaponInfo = info.Clone();
            Deleted = true;
            this.hiddenWeaponInfo.EquipHeroId = -1;
            PileNum = 0;
        }

        public override bool BindData(int id)
        {
            this.model = HiddenWeaponLibrary.GetHiddenWeaponModel(id);
            if (this.model != null)
            {
                //TODO 绑定差异数据
                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this hidden weapon model id {id}");
                return false;
            }
        }

        public MSG_ZGC_ITEM_EQUIPMENT GenerateSyncMessage()
        {
            MSG_ZGC_ITEM_EQUIPMENT syncMsg = new MSG_ZGC_ITEM_EQUIPMENT()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = 0,
                GenerateTime = this.GenerateTime,
                EquipedHeroId = this.hiddenWeaponInfo.EquipHeroId,
                Deleted = this.Deleted,
                PartType = model.Part,
                Level = this.hiddenWeaponInfo.Level,
                HiddenWeapon = new ZGC_HIDDEN_WEAPON_INFO()
                {
                    Star = this.hiddenWeaponInfo.Star,
                    NeedStar = this.hiddenWeaponInfo.NeedStar
                }
            };
            syncMsg.HiddenWeapon.WashList.AddRange(this.hiddenWeaponInfo.WashList);

            HiddenWeaponScoreMsg(syncMsg, hiddenWeaponInfo.WashList);
            return syncMsg;
        }

        public MSG_ZGC_ITEM_EQUIPMENT GenerateSyncShowMessage()
        {
            MSG_ZGC_ITEM_EQUIPMENT syncMsg = new MSG_ZGC_ITEM_EQUIPMENT()
            {
                Id = this.Id,
                EquipedHeroId = this.hiddenWeaponInfo.EquipHeroId,
                PartType = model.Part,
                Level = this.hiddenWeaponInfo.Level,
                HiddenWeapon = new ZGC_HIDDEN_WEAPON_INFO()
                {
                    Star = this.hiddenWeaponInfo.Star,
                    NeedStar = this.hiddenWeaponInfo.NeedStar
                }
            };
            syncMsg.HiddenWeapon.WashList.AddRange(this.hiddenWeaponInfo.WashList);
            return syncMsg;
        }

        public ZMZ_HIDDEN_WEAPON GenerateTransformMessage()
        {
            ZMZ_HIDDEN_WEAPON syncMsg = new ZMZ_HIDDEN_WEAPON()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                EquipHeroId = this.hiddenWeaponInfo.EquipHeroId,
                Level = this.hiddenWeaponInfo.Level,
                Star = this.hiddenWeaponInfo.Star,
                NeedStar = this.hiddenWeaponInfo.NeedStar
            };
            syncMsg.WashList.AddRange(this.hiddenWeaponInfo.WashList);
            return syncMsg;
        }

        public static MSG_ZGC_ITEM_EQUIPMENT HiddenWeaponScoreMsg(MSG_ZGC_ITEM_EQUIPMENT msg, List<int> washList)
        {
            msg.Score = HiddenWeaponScore(msg.Id, msg.Level, washList, msg.HiddenWeapon.Star);
            return msg;
        }

        public static int HiddenWeaponScore(int id, int level, List<int> washList, int star)
        {
            int score = 0;
            //评分
            Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            var model = HiddenWeaponLibrary.GetHiddenWeaponModel(id);
            if (model != null)
            {
                foreach (var item in model.BaseNatureDic)
                {
                    dic.Add(item.Key, item.Value);
                }

                if (level > 0)
                {
                    var upModel = HiddenWeaponLibrary.GetHiddenWeaponUpgradeModel(model.UpgradePool, level);
                    if (upModel != null)
                    {
                        dic.AddValue(upModel.UpgradeAddNature);
                    }
                }

                HiddenWeaponStarModel starModel = HiddenWeaponLibrary.GetHiddenWeaponStarModel(model.Quality, star);
                if (starModel != null)
                {
                    score += starModel.BattlePower;
                }

                foreach (var item in model.SpecList)
                {
                    HiddenWeaponSpecialModel specialModel = HiddenWeaponLibrary.GetHiddenWeaponSpecialModel(item);
                    if (specialModel != null)
                    {
                        score += specialModel.BattlePower;
                    }
                }
            }

            dic = HeroManager.Nature4To9(1, dic);

            score += ScriptManager.BattlePower.CaculateItemScore2(dic);

            if (washList?.Count > 0)
            {
                foreach (var washId in washList)
                {
                    HiddenWeaponWashModel washModel = HiddenWeaponLibrary.GetHiddenWeaponWashModel(washId);
                    if (washModel != null)
                    {
                        score += washModel.BattlePower;
                    }
                }
            }

            return score;
        }

        public HiddenWeaponItem GenerateDeleteInfo(HiddenWeaponDbInfo info)
        {
            HiddenWeaponItem item = new HiddenWeaponItem(info, true);
            return item;
        }
    }
}

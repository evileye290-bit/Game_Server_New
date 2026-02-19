using System.Collections.Generic;
using System.Linq;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public HeroGodManager HeroGodManager { get; private set; }

        private void InitHeroGodManager ()
        {
            HeroGodManager = new HeroGodManager (this);
        }

        public int GetHeroGod (int heroId)
        {
            HeroGodInfo godInfo = HeroGodManager.GetHeroGodInfo (heroId);
            return godInfo == null ? 0 : godInfo.EquipGodType;
        }

        public void InitHeroGodInfo (List<HeroGodInfo> godInfos)
        {
            HeroGodManager.BindHeroGodInfo (godInfos);
            InitHeroGodNature();
        }

        private void InitHeroGodNature ()
        {
            foreach (var kv in HeroGodManager.EquipedGodList)
            {
                HeroInfo heroInfo = HeroMng.GetHeroInfo (kv.Key);
                HeroGodDetailModel model = GodHeroLibrary.GetHeroGodDetailModel (kv.Value);
                if (heroInfo == null || model == null) continue;

                heroInfo.GodType = kv.Value;
                //GodHeroLibrary.NatureTypes.ForEach (x => heroInfo.AddNatureRatio (x, model.NatureRatio));
            }
        }

        public void UpdateHeroGodType (int heroId, int godType)
        {
            HeroInfo heroInfo = HeroMng.GetHeroInfo (heroId);
            HeroGodDetailModel model = GodHeroLibrary.GetHeroGodDetailModel (godType);
            if (heroInfo == null || model == null) return;
            heroInfo.GodType = godType;

            SyncHeroChangeMessage(new List<HeroInfo>() { heroInfo });
        }

        //public void SetHeroGodNature (int heroId, int beforeGodType, int afterGodType)
        //{
        //    HeroInfo heroInfo = HeroMng.GetHeroInfo (heroId);
        //    HeroGodDetailModel before = GodHeroLibrary.GetHeroGodDetailModel (beforeGodType);
        //    HeroGodDetailModel after = GodHeroLibrary.GetHeroGodDetailModel (afterGodType);

        //    if (heroInfo == null || after == null) return;

        //    int beforeV = before == null ? 0 : before.NatureRatio;
        //    int afterV = after.NatureRatio;

        //    int addV = afterV - beforeV;
        //    GodHeroLibrary.NatureTypes.ForEach (x => heroInfo.AddNatureRatio (x, addV));
        //}

        public void SendHeroGodInfo ()
        {
            MSG_ZGC_HERO_GOD_INFO_LIST msg = HeroGodManager.GenerateHeroGodInfoList ();
            Write (msg);
        }

        public void HeroGodUnlock (int heroId, int godType)
        {
            MSG_ZGC_HERO_GOD_UNLOCK msg = new MSG_ZGC_HERO_GOD_UNLOCK ();
            msg.GodType = godType;

            var heroInfo = HeroMng.GetHeroInfo (heroId);
            HeroGodModel model = GodHeroLibrary.GetHeroGodModel (heroId);
            HeroGodDetailModel costModel = GodHeroLibrary.GetHeroGodDetailModel (godType);
            if (heroInfo == null || model == null || costModel == null)
            {
                Logger.Log.Warn($"player {Uid} unlock hero god failed: param error heroId {heroId} godType {godType}");
                msg.Result = (int) ErrorCode.Fail;
                Write (msg);
                return;
            }

            if (!model.HaveTheGodType (godType))
            {
                Logger.Log.Warn($"player {Uid} unlock hero god failed:godType {godType} error");
                msg.Result = (int) ErrorCode.HaveNotThisGodType;
                Write (msg);
                return;
            }

            HeroGodInfo godInfo = HeroGodManager.GetHeroGodInfo (heroId);

            if (godInfo != null && godInfo.GodType.Contains (godType))
            {
                Logger.Log.Warn($"player {Uid} unlock hero god failed:godType {godType} unlocked");
                msg.Result = (int) ErrorCode.ThisGodTypeUnlocked;
                Write (msg);
                return;
            }

            if (!CheckGodUnlockLimit (heroInfo, godInfo, costModel))
            {
                Logger.Log.Warn($"player {Uid} unlock hero god failed: hero {HeroId} godType {godType} unlock limit");
                msg.Result = (int) ErrorCode.ThisGodTypeLimited;
                Write (msg);
                return;
            }

            bool haveEnoughItem = true;
            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int> ();
            foreach (var kv in costModel.CostItems)
            {
                BaseItem item = BagManager.GetItem (MainType.Consumable, kv.Id);
                if (item?.PileNum < kv.Num)
                {
                    haveEnoughItem = false;
                    break;
                }

                costItems.Add (item, kv.Num);
            }

            if (!haveEnoughItem)
            {
                Logger.Log.Warn($"player {Uid} unlock hero {heroId} god failed: item not enough");
                msg.Result = (int) ErrorCode.ItemNotEnough;
                Write (msg);
                return;
            }

            List<BaseItem> baseItems = DelItem2Bag (costItems, RewardType.NormalItem, ConsumeWay.HeroGod);
            SyncClientItemsInfo (baseItems);

            //komoelog before
            int godBefore = heroInfo.GodType;
            int powerBefore = heroInfo.GetBattlePower();

            if (godInfo == null)
            {
                HeroGodManager.AddHeroGodInfo (heroId, godType);
            }
            else
            {
                HeroGodManager.UnlockNewGodType (godInfo, godType);
            }

            msg.Result = (int) ErrorCode.Success;
            Write (msg);

            //养成
            BIRecordDevelopLog (DevelopType.HeroGodUnlock, heroInfo.Id, 0, godType, heroId, heroInfo.Level);

            SyncGodType (heroId, godType);

            BroadCastUnlockGodType(heroId, godType);

            //BI日志
            BIRecordGodLog(heroId, heroInfo.Level, godType);

            //解锁指定数量的准神发称号卡
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.AmateurGodCount);

            //komoelog
            int godAfter = heroInfo.GodType;
            int powerAfter = heroInfo.GetBattlePower();
            KomoeEventLogHeroSkinResource(heroInfo.Id.ToString(), "", heroInfo.GetData().GetString("Job"), 1, godBefore.ToString(), godBefore.ToString(), powerBefore, powerAfter, powerAfter - powerBefore);
            if (godAfter != godBefore)
            {
                KomoeEventLogHeroSkinResource(heroInfo.Id.ToString(), "", heroInfo.GetData().GetString("Job"), 2, godBefore.ToString(), godAfter.ToString(), powerBefore, powerAfter, powerAfter - powerBefore);
            }
        }

        public void HeroGodEquip (int heroId, int godType)
        {
            MSG_ZGC_HERO_GOD_EQUIP msg = new MSG_ZGC_HERO_GOD_EQUIP ();

            var heroInfo = HeroMng.GetHeroInfo (heroId);
            HeroGodModel model = GodHeroLibrary.GetHeroGodModel (heroId);
            if (heroInfo == null || model == null)
            {
                Logger.Log.Warn($"player {Uid} equip hero god failed: param error heroId {heroId} godType {godType}");
                msg.Result = (int) ErrorCode.Fail;
                Write (msg);
                return;
            }

            if (!model.HaveTheGodType (godType))
            {
                Logger.Log.Warn($"player {Uid} equip hero god failed:godType {godType} error");
                msg.Result = (int) ErrorCode.HaveNotThisGodType;
                Write (msg);
                return;
            }

            HeroGodInfo godInfo = HeroGodManager.GetHeroGodInfo (heroId);
            if (godInfo == null || !godInfo.GodType.Contains (godType))
            {
                Logger.Log.Warn($"player {Uid} equip hero god failed:godType {godType} unlocked");
                msg.Result = (int) ErrorCode.ThisGodTypeUnlocked;
                Write (msg);
                return;
            }
            int oldType = godInfo.EquipGodType;
            int oldBattlePower = heroInfo.GetBattlePower();
            if (HeroGodManager.SetEquipedGod (godInfo, godType))
            {
                HeroGodManager.SyncHeroGodInfo2Client (godInfo);
            }

            msg.Result = (int) ErrorCode.Success;
            Write (msg);

            //养成
            BIRecordDevelopLog (DevelopType.HeroGodEquip, heroInfo.Id, oldType, godType, heroId, heroInfo.Level);

            SyncGodType (heroId, godType);

            //BI日志
            //BIRecordGodReplaceLog(heroId, heroInfo.Level, oldType, oldBattlePower, godType, heroInfo.GetBattlePower());

            //komoelog
            int newBattlePower = heroInfo.GetBattlePower();
            KomoeEventLogHeroSkinResource(heroInfo.Id.ToString(), "", heroInfo.GetData().GetString("Job"), 2, oldType.ToString(), godType.ToString(), oldBattlePower, newBattlePower, newBattlePower - oldBattlePower);
        }

        public bool CheckGodUnlockLimit (HeroInfo heroInfo, HeroGodInfo info, HeroGodDetailModel model)
        {
            if (heroInfo == null) return false;

            if (heroInfo.Level < model.LevelLimit) return false;

            if (info == null)
            {
                //必须先解锁准神位置
                if (!model.IsPrimaryGod) return false;

                //必须有神位
                if (model.GodLimit > 0) return false;
            }
            else
            {
                // HeroGodDetailModel primaryModel = info.GodType.Select(x =>
                // {
                //     HeroGodDetailModel temp = GodHeroLibrary.GetHeroGodCostModel(x);
                //     if (temp?.IsPrimaryGod == true) return temp;
                //     return null;
                // }).FirstOrDefault(x => x != null);

                //准神的基础上才能解锁
                // if (primaryModel == null) return false;

                if (model.GodLimit <= 0 || !info.GodType.Contains (model.GodLimit))
                {
                    return false;
                }

                //开启时间限制
                if (model.OpenHeroGodTime > BaseApi.now)
                {
                    return false;
                }
            }

            return true;
        }

        private void SyncGodType (int heroId, int godType)
        {
            UpdateHeroGodType (heroId, godType);

            //当前主hero的神位变动，同步gamedb和redis
            if (HeroId == heroId && godType != GodType)
            {
                GodType = godType;
                server.GameDBPool.Call (new QueryUpdateMainHeroGodType (uid, godType));
                server.GameRedis.Call (new OperateSetGodType (uid, godType));

                //同步视野信息
                RemoveFromAoi();
                AddToAoi();

                Sync2ClientChangeMainHero();
            }
        }

        public void LoadTransform(MSG_ZMZ_HERO_GOD_INFO_LIST info)
        {
            List<HeroGodInfo> heroInfos = new List<HeroGodInfo>();
            info.GodHeroList.ForEach(x =>
            {
                HeroGodInfo godInfo = new HeroGodInfo() { HeroId = x.HeroId, EquipGodType = x.EquipedGodType, GodType = new List<int>() };
                godInfo.GodType.AddRange(x.GodType);
                heroInfos.Add(godInfo);
            });

            InitHeroGodInfo(heroInfos);
        }
    }
}

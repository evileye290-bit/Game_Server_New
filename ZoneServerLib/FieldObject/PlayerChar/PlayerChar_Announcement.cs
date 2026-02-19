using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Zone.Protocol.ZGate;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public void BroadcastAnnouncement(ANNOUNCEMENT_TYPE type, List<string> list)
        {
            MSG_ZGate_BROADCAST_ANNOUNCEMENT msg = new MSG_ZGate_BROADCAST_ANNOUNCEMENT();
            msg.Type = (int)type;
            foreach (var item in list)
            {
                msg.List.Add(item);
            }
            server.GateManager.Broadcast(msg);

            Log.Write("player {0} BroadcastAnnouncement type {1} list count {2}", Uid, type, list.Count);
        }

        public void CrossBroadcastAnnouncement(ANNOUNCEMENT_TYPE type, List<string> list)
        {
            MSG_ZR_BROADCAST_ANNOUNCEMENT msg = new MSG_ZR_BROADCAST_ANNOUNCEMENT();
            msg.Type = (int)type;
            foreach (var item in list)
            {
                msg.List.Add(item);
            }
            server.SendToRelation(msg);
            Log.Write("player {0} Cross BroadcastAnnouncement type {1} list count {2}", Uid, type, list.Count);
        }

        public void GetNotesListByType(int type)
        {
            //获取第一名信息
            MSG_ZR_NOTES_LIST_BY_TYPE msg = new MSG_ZR_NOTES_LIST_BY_TYPE();
            msg.Type = type;
            server.SendToRelation(msg, Uid);
        }


        public ZR_CROSS_NOTES GetCrossNotes(NotesType type, params object[] list)
        {
            ZR_CROSS_NOTES msg = new ZR_CROSS_NOTES();
            msg.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            foreach (var item in list)
            {
                msg.List.Add(item.ToString());
            }
            Log.Write("player {0} AddCrossNotes {1} list: " + list, Uid, type);
            return msg;
        }

        public void SendCrossNotes(MSG_ZR_CROSS_NOTES_LIST msg)
        {
            if (msg != null && msg.List.Count > 0)
            {
                server.SendToRelation(msg);
            }
        }

        //公告
        //广播掉落最高年份魂环
        private void BroadcastDropMaxYearSoulRing(SoulRingItem item, ActivityType type)
        {
            //SoulRingModel model = SoulRingLibrary.GetSoulRingMode(item.modelId);
            //if (model.MaxYear == item.Year)
            //{
            //    List<string> list = new List<string>();
            //    list.Add(Name);
            //    switch (type)
            //    {
            //        case ActivityType.Hunting:
            //            list.Add(((int)ActivityType.Hunting).ToString());
            //            break;
            //        case ActivityType.HourBoss:
            //            list.Add(((int)ActivityType.HourBoss).ToString());
            //            break;
            //        default:
            //            break;
            //    }
            //    list.Add(item.Year.ToString());
            //    list.Add(item.modelId.ToString() + "|" + item.Year.ToString());
            //    BroadcastAnnouncement(ANNOUNCEMENT_TYPE.MAX_YEAR_SOULRING, list);
            //}
        }

        //广播装备升到满级
        private void BroadcastEquipmentRaiseMaxLevel(int heroId, int slotId)
        {
            var slotEquipment = EquipmentManager.GetSlotEquipment(heroId, slotId);
            if (slotEquipment.Item1)
            {
                List<string> list = new List<string>();
                list.Add(Name);
                list.Add(slotEquipment.Item2);
                BroadcastAnnouncement(ANNOUNCEMENT_TYPE.MAX_EQUIPMENT_LEVEL, list);
            }
        }

        //广播熔炼出最高品质且带词缀的魂骨
        private void BroadcastSmeltBestSoulBone(int quality, int prefixId, int id)
        {
            int best = SoulBoneLibrary.GetSoulBoneBestQuality();
            if (best == quality && prefixId > 0)
            {
                List<string> list = new List<string>();
                list.Add(Name);
                list.Add(id.ToString() + "|" + quality);
                BroadcastAnnouncement(ANNOUNCEMENT_TYPE.SMELT_BEST_SOULBONE, list);
            }
        }

        //在狩猎魂兽中通关全部魂兽的所有难度

/*     private void BroadcastPassAllDifficultyHunting(ActivityType type)
        {
            if (PassAllDifficultyHunting())
            {
                List<string> list = new List<string>();
                list.Add(Name);
                switch (type)
                {
                    case ActivityType.Hunting:
                        list.Add(((int)ActivityType.Hunting).ToString());
                        break;
                    case ActivityType.HourBoss:
                        list.Add(((int)ActivityType.HourBoss).ToString());
                        break;
                    default:
                        break;
                }           
                BroadcastAnnouncement(ANNOUNCEMENT_TYPE.PASS_ALL_DIFFICULTY_HUNTING, list);
            }         
        }
        */
        private bool PassAllDifficultyHunting()
        {
            return HuntingManager.HuntingList.Count == HuntingLibrary.MaxHuntingCount;
        }

        //广播达到封号斗罗
        private void BroadCastRaiseMaxTitleLevel(HeroInfo info)
        {
            if (info.TitleLevel != HeroLibrary.GetMaxTitleLevel()) return;
            List<string> list = new List<string>();
            list.Add(Name);
            list.Add(info.Id.ToString());
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.MAX_HERO_TITLE_LEVEL, list);
        }

        //公告
        //广播掉落最高年份魂环
        private void BroadcastCampBattleHold( CampActivityType type)
        {
                //BroadcastAnnouncement(ANNOUNCEMENT_TYPE.MAX_YEAR_SOULRING, list);
        }

        //公告
        //广播攻克boss
        public void BroadcastCampBattleHoldBoss(int monsterId, int fortId)
        {
            List<string> list = new List<string>();
            list.Add(((int)Camp).ToString());
            list.Add(Name);
            list.Add(fortId.ToString());
            list.Add(monsterId.ToString());
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.HOLD_BOSS, list);
        }

        //广播攻克守将
        public void BroadcastCampBattleHoldDefender(string defenderName, int fortId)
        {
            List<string> list = new List<string>();
            list.Add(((int)Camp).ToString());
            list.Add(Name);
            list.Add(fortId.ToString());
            list.Add(defenderName);
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.HOLD_FORT, list);
        }

        //广播粮草
        public void BroadcastCampBattleGrain()
        {
            List<string> list = new List<string>();
            list.Add(((int)Camp).ToString());
            list.Add(Name);
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.GET_EXTRA_GRAIN, list);
        }

        //广播抽到SSR
        private void CheckBroadcastDrawSSRCard(Dictionary<int, int> announces, List<int> heroList)
        {
            List<int> list = new List<int>();
            //if (rewardHeroId > 0)
            //{
            //    heroSet.Add(rewardHeroId);
            //}
            foreach (var hero in heroList)
            {
                //抽到SSR 跑马灯
                HeroModel model = HeroLibrary.GetHeroModel(hero);
                if (model != null && model.Quality == 2)
                {
                    list.Add(hero);
                }
            }
            BroadcastDrawSSRCard(announces, list);
        }

        private void BroadcastDrawSSRCard(Dictionary<int, int> announces, List<int> heroList)
        {
            List<string> list = new List<string>();
            list.Add(Name);
            foreach (var hero in heroList)
            {
                list.Add(hero.ToString());
            }
            if (heroList.Count > 0)
            {
                int announce;
                if (announces.TryGetValue(heroList.Count, out announce))
                {
                    BroadcastAnnouncement((ANNOUNCEMENT_TYPE)announce, list);
                }
            }
            
            //switch (heroList.Count)
            //{
            //    case 0:
            //        break;
            //    case 1:
            //        BroadcastAnnouncement((ANNOUNCEMENT_TYPE)announce, list);
            //        if (drawType == 3)
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_UP_ONE_SSR, list);
            //        }
            //        else
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_ONE_SSR_CARD, list);
            //        }
            //        break;
            //    case 2:
            //        if (drawType == 3)
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_UP_TWO_SSR, list);
            //        }
            //        else
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_TWO_SSR_CARD, list);
            //        }                  
            //        break;
            //    default:
            //        if (drawType == 3)
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_UP_THREE_SSR, list);
            //        }
            //        else
            //        {
            //            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DRAW_THREE_ORMORE_SSR_CARD, list);
            //        }                  
            //        break;
            //}
        }

        //广播竞技场第一名上线
        private void BroadCastArenaFirstLogin()
        {
            List<string> list = new List<string>();
            list.Add(Name);
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.ARENA_FIRST_LOGIN, list);
        }

        //广播装备升到指定级别
        private void BroadcastEquipmentUpgradeToLevel(int heroId, int slotId)
        {
            EquipmentItem item = EquipmentManager.GetEquipedItem(heroId, slotId);
            if (item == null)
            {
                return;
            }
            int equipmentLevel = EquipmentManager.GetSlotEquipmentLevel(heroId, slotId);
            if (EquipLibrary.UpgradeToLevel > 0 && equipmentLevel >= EquipLibrary.UpgradeToLevel)
            {
                List<string> list = new List<string>();
                list.Add(Name);
                list.Add(item.Model.ID.ToString());
                list.Add(equipmentLevel.ToString());
                BroadcastAnnouncement(ANNOUNCEMENT_TYPE.EQUIPMENT_UPTO_LEVEL, list);
            }
        }

        //广播镶嵌指定品质的玄玉
        private void BroadCastInlayXuanyuLevel(int itemId, int level)
        {
            if (EquipLibrary.InlayXuanyuLevel > 0 && level >= EquipLibrary.InlayXuanyuLevel)
            {
                List<string> list = new List<string>();
                list.Add(Name);
                list.Add(itemId.ToString());
                BroadcastAnnouncement(ANNOUNCEMENT_TYPE.INLAY_XUANYU_LEVEL, list);
            }
        }

        //广播首充
        private void BroadCastFirstRecharge()
        {
            List<string> list = new List<string>();
            list.Add(Name);
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.FIRST_RECHARGE, list);
        }

        //广播竞技场第一名上线
        private void BroadCastUnlockGodType(int heroId, int godType)
        {
            List<string> list = new List<string>() { Name, godType.ToString(), heroId.ToString() };
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.UNLOCK_GOD_TYPE, list);
        }

        /// <summary>
        /// 广播神域赐福
        /// </summary>
        /// <param name="iNum">成功次数</param>
        private void BroadCastDomainBenediction(int iNum)
        {
            List<string> list = new List<string>() { Name, iNum.ToString() };
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.DOMAIN_BENEDICTION, list);
        }
    }
}

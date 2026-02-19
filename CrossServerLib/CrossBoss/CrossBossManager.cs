using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using RedisUtility;
using ServerModels;
using ServerModels.HidderWeapon;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{
    public class PlayerInfoMsgModel
    {
        public DateTime Time { get; set; }
        public object Item { get; set; }
    }
    public class CrossBossManager
    {
        private CrossServerApi server { get; set; }

        private Dictionary<int, CrossBossGroupItem> bossGroups = new Dictionary<int, CrossBossGroupItem>();

        private Dictionary<int, PlayerInfoMsgModel> playerInfoMsgList = new Dictionary<int, PlayerInfoMsgModel>();

        public CrossBossManager(CrossServerApi server)
        {
            this.server = server;

            InitGroupList();

            LoadCrossBossSiteInfoFromRedis();
        }

        private void InitGroupList()
        {
            foreach (var groupList in CrossBattleLibrary.GroupList)
            {
                int groupId = groupList.Key;
                if (!bossGroups.ContainsKey(groupId))
                {
                    AddCrossBossGroupItem(groupId);
                }
            }
        }

        private CrossBossGroupItem AddCrossBossGroupItem(int groupId)
        {
            CrossBossGroupItem group = new CrossBossGroupItem();
            bossGroups[groupId] = group;
            return group;
        }

        //加载据点信息
        protected void LoadCrossBossSiteInfoFromRedis()
        {
            OperateLoadCrossBossSiteInfos op = new OperateLoadCrossBossSiteInfos(bossGroups.Keys.ToList());
            server.CrossRedis.Call(op, ret =>
            {
                foreach (var group in bossGroups)
                {
                    CrossBossRankManager crossMng = server.RankMng.GetCrossBossRankManager(group.Key);
                    if (crossMng == null)
                    {
                        server.RankMng.AddCrossBossRankManager(group.Key);
                        crossMng = server.RankMng.GetCrossBossRankManager(group.Key);
                    }
                    Dictionary<int, CurrentBossSiteInfo> dic;
                    if (op.GroupList.TryGetValue(group.Key, out dic))
                    {
                        group.Value.SetSiteList(dic);

                        foreach (var kv in dic)
                        {
                            CrossBossSiteRank siteRank = crossMng.GetSiteRank(kv.Key);
                            if (siteRank == null)
                            {
                                crossMng.AddSiteRank(group.Key, kv.Key);
                                siteRank = crossMng.GetSiteRank(kv.Key);
                                siteRank.LoadInitRankFromRedis();
                            }
                        }
                    }
                    foreach (var chapter in CrossBossLibrary.chapterList)
                    {
                        CrossBossChapterRank chapterRank = crossMng.GetChapterRank(chapter.Key);
                        if (chapterRank == null)
                        {
                            crossMng.AddChapterRank(group.Key, chapter.Key);
                            chapterRank = crossMng.GetChapterRank(chapter.Key);
                            chapterRank.LoadInitRankFromRedis();
                        }
                    }
                }

                //初始化当前站点信息
                InitCurrentDungeon();
                //初始化防守信息
                LoadCrossBossDefenseFromRedis();
            });
        }

        protected void LoadCrossBossDefenseFromRedis()
        {
            OperateLoadCrossBossDefenses op = new OperateLoadCrossBossDefenses(bossGroups.Keys.ToList());
            server.CrossRedis.Call(op, ret =>
            {
                foreach (var group in bossGroups)
                {
                    Dictionary<int, int> dic;
                    if (op.GroupList.TryGetValue(group.Key, out dic))
                    {
                        group.Value.SetDefenseList(dic);
                    }
                }
            });
        }

        private void InitCurrentDungeon()
        {
            foreach (var group in bossGroups)
            {
                List<int> chapterList = new List<int>();
                //初始化当前副本
                foreach (var serverDungeon in CrossBossLibrary.ServerDungeonList)
                {
                    int serverId = serverDungeon.Key;
                    //初始化当前组副本
                    foreach (var dungeonId in serverDungeon.Value)
                    {
                        //查看副本信息
                        CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(dungeonId);
                        if (model != null)
                        {
                            if (!chapterList.Contains(model.Chapter))
                            {
                                chapterList.Add(model.Chapter);
                            }
                            //查看副本信息
                            CurrentBossSiteInfo site = group.Value.GetSiteInfo(dungeonId);
                            if (site != null)
                            {
                                group.Value.SetCurrentSite(serverId, dungeonId);
                                if (site.Hp > 0)
                                {
                                    //这是当前点
                                    break;
                                }
                                else
                                {
                                    //BOSS 已经击杀
                                    continue;
                                }
                            }
                            else
                            {
                                //没有这个，进行初始化
                                AddAddSiteInfo(group.Value, serverId, model);
                                break;
                            }
                        }
                    }
                }
                CrossBossRankManager crossMng = server.RankMng.GetCrossBossRankManager(group.Key);
                if (crossMng == null)
                {
                    server.RankMng.AddCrossBossRankManager(group.Key);
                    crossMng = server.RankMng.GetCrossBossRankManager(group.Key);
                }
                foreach (var chapterId in chapterList)
                {
                    CrossBossChapterRank chapterRank = crossMng.GetChapterRank(chapterId);
                    if (chapterRank == null)
                    {
                        crossMng.AddChapterRank(group.Key, chapterId);
                    }
                }
            }
        }

        public void AddAddSiteInfo(CrossBossGroupItem group, int serverId, CrossBossDungeonModel model)
        {
            CurrentBossSiteInfo site = new CurrentBossSiteInfo(model.Id, model.MaxHp, model.MaxHp);
            group.AddSiteInfo(site);
            //这组初始化了副本退出循环
            group.SetCurrentSite(serverId, model.Id);
        }

        public CrossBossGroupItem GetGroup(int groupId)
        {
            CrossBossGroupItem value;
            if (!bossGroups.TryGetValue(groupId, out value))
            {
                value = AddCrossBossGroupItem(groupId);
            }
            return value;
        }

        /// <summary>
        /// 新增玩家基本信息
        /// </summary>
        public void AddPlayerInfoMsg(int uid, PlayerInfoMsgModel info)
        {
            playerInfoMsgList[uid] = info;
        }

        public PlayerInfoMsgModel GetPlayerInfoMsg(int uid)
        {
            PlayerInfoMsgModel info;
            if (playerInfoMsgList.TryGetValue(uid, out info))
            {
                //if (info.Time < server.Now())
                //{
                //    return null;
                //}
            }
            return info;
        }

        
    }
}

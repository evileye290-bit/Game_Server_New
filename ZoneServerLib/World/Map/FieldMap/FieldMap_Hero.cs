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
        private Dictionary<int, Hero> heroList = new Dictionary<int, Hero>();
        /// <summary>
        /// 宠物
        /// </summary>
        public Dictionary<int, Hero> HeroList
        {
            get { return heroList; }
        }

        private List<int> heroRemoveList = new List<int>();

        private void UpdateHero(float dt)
        {
            foreach (var hero in heroList)
            {
                try
                {
                    hero.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        private void RemoveHero()
        {
            if (heroRemoveList.Count > 0)
            {
                foreach (var instanceId in heroRemoveList)
                {
                    try
                    {
                        Hero hero = GetHero(instanceId);
                        if (hero != null)
                        {
                            //hero.SetCurrentMap(null);
                            hero.RemoveFromAoi();
                            hero.SetInstanceId(0);//先通知离开然后在设为0
                            hero.SetCurrentMap(null);
                            heroList.Remove(instanceId);
                        }
                        RemoveObjectSimpleInfo(instanceId);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                heroRemoveList.Clear();
            }
        }

        #region 添加
        public virtual void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            // 加到地图里
            AddHero(hero);
            if (IsDungeon)
            {
                DungeonMap map = this as DungeonMap;
                DungeonModel model = map.DungeonModel;

                int PosIndex = 0;
                if (hero.IsAttacker)
                {
                    PosIndex = map.AttackerPosIndex;
                }
                else
                {
                    PosIndex = map.DefenderPosIndex;
                }

                Vec2 tempPosition;

                //设置位置，一定在aoi前
                if (hero.OwnerIsRobot)
                {
                    Robot ow = hero.Owner as Robot;

                    tempPosition = ow.GetHeroPosPosition(hero.HeroId);
   
                    int heroPos = ow.GetHeroPos(hero.HeroId);
                    hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(heroPos);
                }
                else
                {
                    PlayerChar owner = hero.Owner as PlayerChar;

                    Tuple<int, int, Vec2> posInfo = owner.HeroMng.GetHeroPosInfo(hero.HeroId);
                    hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(posInfo.Item2);

                    tempPosition = owner.HeroMng.GetHeroPos(hero.HeroId);
              
                    if (map.heroList.Count >= owner.HeroMng.CallHeroCount())
                    {
                        map.OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
                    }
                }

                if (tempPosition != null)
                {
                    tempPosition = model.GetPosition4Count(PosIndex, tempPosition);
                }

                hero.SetPosition(tempPosition ?? hero.Position);
                hero.InitBaseBattleInfo();
            }

            if (add2Aoi)
            {
                hero.AddToAoi();
                hero.BroadCastHp();
            }
        }    

        public static HeroInfo InitFromRobotInfo(MonsterHeroModel robot)
        {
            HeroInfo info = new HeroInfo();
            info.Id = robot.HeroId;
            info.Level = robot.Level;
            info.AwakenLevel = robot.AwakenLevel;
            foreach (var kv in robot.NatureList)
            {
                info.SetNatureBaseValue(kv.Key, kv.Value);
            }
            info.SetNatureBaseValue(NatureType.PRO_HP,  info.GetNatureValue(NatureType.PRO_MAX_HP));//有些地方需要外部传入血量，阵营战
            info.TalentMng = new TalentManager(0, 0, 0, 0, 0);
            info.IsRobotHero = true;
            info.BindData();
            return info;
        }

        public void AddHero(Hero hero)
        {
            hero.SetCurrentMap(this);
            hero.SetInstanceId(TokenId);
            
            heroList.Add(hero.InstanceId, hero);
            AddObjectSimpleInfo(hero.InstanceId, TYPE.HERO);
        }

        #endregion

        #region 删除

        public void RemoveHero(int instance_id)
        {
            heroRemoveList.Add(instance_id);
        }

        #endregion
    }
}
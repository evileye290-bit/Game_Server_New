using CommonUtility;
using DBUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class HeroManager
    {
        private SortedDictionary<int, int> heroPos = new SortedDictionary<int, int>();

        public SortedDictionary<int, int> GetHeroPos()
       {  return heroPos; } 

        public void InitHeroPos(List<Tuple<int, int, int>> list)
        {
            foreach (var temp in list)
            {
                heroPos.Add(temp.Item2,temp.Item3);
            }
        }

        public void InitHeroPosFromTransform(int heroId,int pos)
        {
            heroPos.Add(heroId, pos);
        }

        public bool UpdateHeroPos(int hero,int pos)
        {
            if (heroPos.ContainsKey(hero) && heroPos[hero] != pos)
            {
                heroPos[hero] = pos;
                UpdateHeroPos2DB(hero);
                return true;
            }
            else if (!heroPos.ContainsKey(hero) && heroPos.Count() < HeroLibrary.HeroPosCount)
            {
                heroPos[hero] = pos;
                InsertHeroPos2DB(hero);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetHeroPosMessage(RepeatedField<MSG_ZGC_HERO_POS> list)
        {
            foreach(var kv in heroPos)
            {
                MSG_ZGC_HERO_POS temp = new MSG_ZGC_HERO_POS();
                temp.HeroId = kv.Key;
                temp.PosId = kv.Value;
                list.Add(temp);
            }
        }

        public Vec2 GetHeroPos(int heroId)
        {
            if (heroPos.ContainsKey(heroId))
            {
                return HeroLibrary.GetHeroPos(heroPos[heroId]);
            }
            return null;
        }

        public List<Tuple<int,int,Vec2>> GetAllHeroPos()
        {
            List<Tuple<int, int, Vec2>> list = new List<Tuple<int, int, Vec2>>();
            foreach(var kv in heroPos)
            {
                list.Add(Tuple.Create(kv.Key, kv.Value, HeroLibrary.GetHeroPos(kv.Value)));
            }

            return list;
        }

        public Tuple<int, int, Vec2> GetHeroPosInfo(int heroId)
        {
            if (heroPos.ContainsKey(heroId))
            {
                return Tuple.Create(heroId, heroPos[heroId], HeroLibrary.GetHeroPos(heroPos[heroId]));
            }

            return null;
        }

        public List<int> GetAllHeroPosHeroId()
        {
            return heroPos.Keys.ToList();
        }

        public void DeleteHeroPos(int hero)
        {
            if (heroPos.ContainsKey(hero))
            {
                heroPos.Remove(hero);
                DeleteHeroPos2DB(hero);
            }
        }


        public void UpdateHeroPos2DB(int hero)
        {
            QueryUpdateHeroPosIndex query = new QueryUpdateHeroPosIndex(owner.Uid, hero, heroPos[hero]);
            SyncDBEquipHero(hero, heroPos[hero] + 1);
            owner.server.GameDBPool.Call(query);
        }

        public void InsertHeroPos2DB(int hero)
        {
            QueryInsertHeroPos query = new QueryInsertHeroPos(owner.Uid,hero, heroPos[hero]);
            SyncDBEquipHero(hero, heroPos[hero] + 1);
            owner.server.GameDBPool.Call(query);
        }

        public void DeleteHeroPos2DB(int hero)
        {
            QueryDeleteHeroPos query = new QueryDeleteHeroPos(owner.Uid, hero);
            SyncDBEquipHero(hero, -1);
            owner.server.GameDBPool.Call(query);
        }


    }
}

using CommonUtility;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DrawManager
    {
        //抽奖系统
        private PlayerChar owner { get; set; }
        public DrawManager(PlayerChar owner)
        {
            this.owner = owner;
        }
        //抽奖类型和对应抽取次数
        private Dictionary<int, DrawCounterItem> drawCounterList = new Dictionary<int, DrawCounterItem>();

        public void Init(string heroDraw, string constellation)
        {
            string[] heroDrawArray = StringSplit.GetArray("|", heroDraw);
            foreach (var heroDrawItem in heroDrawArray)
            {
                string[] item = StringSplit.GetArray(":", heroDrawItem);
                DrawCounterItem info = new DrawCounterItem();
                if (item.Length > 0)
                {
                    info.Id = int.Parse(item[0]);
                }
                if (item.Length > 1)
                {
                    info.Single = int.Parse(item[1]);
                }
                if (item.Length > 2)
                {
                    info.Continuous = int.Parse(item[2]);
                }
                if (item.Length > 3)
                {
                    info.Blessing = int.Parse(item[3]);
                }
                AddHeroDraw(info);
            }

            string[] constellationArray = StringSplit.GetArray("|", constellation);
            foreach (var constellationItem in constellationArray)
            {
                string[] item = StringSplit.GetArray(":", constellationItem);
                int type = int.Parse(item[0]);
                int constellationId = int.Parse(item[1]);
                Dictionary<int, int> star = new Dictionary<int, int>();
                if (item.Length > 2)
                {
                    string[] starLsit = StringSplit.GetArray("-", item[2]);
                    foreach (var starItem in starLsit)
                    {
                        if (starItem.Contains("_"))
                        {
                            string[] specialItem = StringSplit.GetArray("_", starItem);
                            int specialStar = int.Parse(specialItem[0]);
                            if (!star.ContainsKey(specialStar))
                            {
                                star.Add(specialStar, int.Parse(specialItem[1]));
                            }
                            else
                            {
                                Log.Warn("player {0} Init draw manager error: add same special star {1}", owner.Uid, specialStar);
                            }
                        }
                        else
                        {
                            Log.Warn("player {0} Init draw manager error: not has '_' in {1}", owner.Uid, starItem);
                        }
                    }
                }
                AddConstellation(type, constellationId, star);
            }
        }

        public Dictionary<int, DrawCounterItem> GetDrawCounterList()
        {
            return drawCounterList;
        }

        public void SetHeroDraw(DrawCounterItem info)
        {
            drawCounterList[info.Id] = info;
        }
        private void AddHeroDraw(DrawCounterItem info)
        {
            if (!drawCounterList.ContainsKey(info.Id))
            {
                drawCounterList.Add(info.Id, info);
            }
            else
            {
                Logger.Log.Warn("player {0} AddHeroDraw in draw manager add same combo id {1}", owner.Uid, info.Id);
            }
        }

        private void AddConstellation(int type, int constellation, Dictionary<int, int> star)
        {
            DrawCounterItem info = new DrawCounterItem();
            if (drawCounterList.TryGetValue(type, out info))
            {
                info.Constellation = constellation;
                info.SpecialStar = star;
            }
            else
            {
                Logger.Log.Warn("player {0} AddConstellation in draw manager add same combo id {1}", owner.Uid, info.Id);
                info = new DrawCounterItem();
                info.Id = type;
                info.Constellation = constellation;
                info.SpecialStar = star;
                AddHeroDraw(info);
            }
        }

        public DrawCounterItem Get(int type)
        {
            DrawCounterItem info;
            drawCounterList.TryGetValue(type, out info);
            return info;
        }

        public string GetHeroDraw()
        {
            string heros = string.Empty;

            foreach (var drawCounter in drawCounterList)
            {
                heros += string.Format("{0}:{1}:{2}:{3}|" , drawCounter.Value.Id, drawCounter.Value.Single, drawCounter.Value.Continuous, drawCounter.Value.Blessing);
            }

            return heros;
        }

        public string GetDrawConstellation()
        {
            string heros = string.Empty;

            foreach (var drawCounter in drawCounterList)
            {
                string stars = string.Empty;
                foreach (var star in drawCounter.Value.SpecialStar)
                {
                    stars += string.Format("{0}_{1}-", star.Key, star.Value);
                }
                heros += string.Format("{0}:{1}:{2}|", drawCounter.Value.Id, drawCounter.Value.Constellation, stars);
            }

            return heros;
        }
    }
}

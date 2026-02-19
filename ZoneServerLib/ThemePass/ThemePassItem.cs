using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ThemePassItem
    {
        public int ThemeType { get; private set; }
        public int PassLevel { get; private set; }
        public int Exp { get; private set; }
        public bool Bought { get; private set; }

        private SortedSet<int> basicRewardLevelSet;
        public SortedSet<int> BasicRewardLevelSet { get { return basicRewardLevelSet; } }

        private SortedSet<int> superRewardLevelSet;
        public SortedSet<int> SuperRewardLevelSet { get { return superRewardLevelSet; } }

        public ThemePassItem(DbThemePassItem dbItem)
        {
            ThemeType = dbItem.ThemeType;
            PassLevel = dbItem.PassLevel;
            Exp = dbItem.Exp;
            Bought = dbItem.Bought == 1 ? true : false;

            basicRewardLevelSet = new SortedSet<int>();
            string[] rewards = dbItem.BasicRewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in rewards)
            {
                basicRewardLevelSet.Add(temp.ToInt());
            }

            superRewardLevelSet = new SortedSet<int>();
            rewards = dbItem.SuperRewardedLevels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var temp in rewards)
            {
                superRewardLevelSet.Add(temp.ToInt());
            }
        }

        public ThemePassItem(int themeType)
        {
            ThemeType = themeType;
            //PassLevel = 0;
            //Exp = 0;
            basicRewardLevelSet = new SortedSet<int>();
            superRewardLevelSet = new SortedSet<int>();
        }

        public void UpdateBasicRewardLevelSet(SortedSet<int> basicRewardLevelSet)
        {
            this.basicRewardLevelSet = basicRewardLevelSet;
        }

        public void UpdateSuperRewardLevelSet(SortedSet<int> superRewardLevelSet)
        {
            this.superRewardLevelSet = superRewardLevelSet;
        }

        public void AddSuperRewardLevel(int rewardLevel)
        {
            if (!superRewardLevelSet.Contains(rewardLevel))
            {
                superRewardLevelSet.Add(rewardLevel);
            }
        }

        public void AddBasicRewardLevel(int rewardLevel)
        {
            if (!basicRewardLevelSet.Contains(rewardLevel))
            {
                basicRewardLevelSet.Add(rewardLevel);
            }
        }

        public string GetBasicRewardedLevelsStr()
        {
            string rewards = "";
            foreach (var level in basicRewardLevelSet)
            {
                rewards += level + "|";
            }
            return rewards;
        }

        public string GetSuperRewardedLevelsStr()
        {
            string rewards = "";
            foreach (var level in superRewardLevelSet)
            {
                rewards += level + "|";
            }
            return rewards;
        }

        public void ChangeBuyState(bool bought)
        {
            Bought = bought;
        }

        public int GetBoughtState()
        {
            if (Bought)
            {
                return 1;
            }
            return 0;
        }

        public void AddExp(int exp)
        {          
            Exp += exp;        
        }

        public void PassLevelUp()
        {
            PassLevel++;
        }

        public void SetExp(int exp)
        {
            Exp = exp;
        }

        public void SetPassLevel(int passLevel)
        {
            PassLevel = passLevel;
        }

        public void SetBoughtState(bool bought)
        {
            Bought = bought;
        }

        public void SetBasicRewardLevelSet(string levels)
        {
            string[] levelsArr = levels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in levelsArr)
            {
                basicRewardLevelSet.Add(item.ToInt());
            }
        }

        public void SetSuperRewardLevelSet(string levels)
        {
            string[] levelsArr = levels.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in levelsArr)
            {
                superRewardLevelSet.Add(item.ToInt());
            }
        }
    }
}

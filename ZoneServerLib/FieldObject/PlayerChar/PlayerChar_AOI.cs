using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public void GetDungeonAoi()
        {
            //因为dungeon只有一种情况
            if (currentMap.AoiType == AOIType.All)
            {
                NotifyMyselfFieldObjectIn(currentMap, true);
            }
            else
            {
                curRegion = currentMap.RegionMgr.GetRegion(Position);
                if (CurRegion != null)
                {
                    CurRegion.AddGameObject(this, Position);
                }
            }
        }

        public void NotifyMyselfFieldObjectIn(IFieldObjectContainer container, bool isBorn = false)
        {
            IReadOnlyDictionary<int, PlayerChar> playerList = container.GetPlayers();
            IReadOnlyDictionary<int, Pet> petList = container.GetPets();
            IReadOnlyDictionary<int, Hero> heroList = container.GetHeros();
            IReadOnlyDictionary<int, Monster> monsterList = container.GetMonsters();
            IReadOnlyDictionary<int, Robot> robotList = container.GetRobots();

            // 需要通知player 当前格子的 其他player monster pet 等等
            PlayerChar pc = this as PlayerChar;
            if (playerList.Count > 0)
            {
                int count = 0;
                MSG_GC_CHARACTER_ENTER_LIST msg = new MSG_GC_CHARACTER_ENTER_LIST();
                MSG_GC_CHAR_SIMPLE_INFO pcSimpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
                pc.GetSimpleInfo(pcSimpleInfo);

                foreach (var player in playerList)
                {
                    // 通知我 AOI里其他player的信息
                    if (player.Value.IsObserver)
                    {
                        continue;
                    }
                    if (count > 30)
                    {
                        pc.Write(msg);
                        count = 0;
                        msg = new MSG_GC_CHARACTER_ENTER_LIST();
                        MSG_GC_CHAR_SIMPLE_INFO playerSimpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
                        player.Value.GetSimpleInfo(playerSimpleInfo);
                        count++;
                        msg.CharacterList.Add(playerSimpleInfo);
                    }
                    else
                    {
                        MSG_GC_CHAR_SIMPLE_INFO playerSimpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
                        player.Value.GetSimpleInfo(playerSimpleInfo);
                        count++;
                        msg.CharacterList.Add(playerSimpleInfo);
                    }
                }
                if (msg.CharacterList.Count > 0)
                {
                    pc.Write(msg);
                }
            }
            // 通知pc AOI里的robot
            if (robotList.Count > 0)
            {
                MSG_GC_CHARACTER_ENTER_LIST msg = new MSG_GC_CHARACTER_ENTER_LIST();
                foreach (var robot in robotList)
                {
                    MSG_GC_CHAR_SIMPLE_INFO playerSimpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
                    robot.Value.GetSimpleInfo(playerSimpleInfo);
                    msg.CharacterList.Add(playerSimpleInfo);
                }
                pc.Write(msg);
            }
            // 通知pc AOI里的hero
            if (heroList.Count > 0)
            {
                MSG_GC_HERO_ENTER_LIST msgHeros = new MSG_GC_HERO_ENTER_LIST();
                int heroCount = 0;
                foreach (var item in heroList)
                {
                    if (heroCount > 30)
                    {
                        pc.Write(msgHeros);
                        heroCount = 0;
                        if (item.Value.Owner != null)
                        {
                            msgHeros = new MSG_GC_HERO_ENTER_LIST();
                            MSG_ZGC_HERO_SIMPLE_INFO heroInfo = new MSG_ZGC_HERO_SIMPLE_INFO();
                            item.Value.GetSimpleInfo(heroInfo);
                            heroCount++;
                            msgHeros.HeroList.Add(heroInfo);
                        }
                    }
                    else
                    {
                        if (item.Value.Owner != null)
                        {
                            MSG_ZGC_HERO_SIMPLE_INFO heroInfo = new MSG_ZGC_HERO_SIMPLE_INFO();
                            item.Value.GetSimpleInfo(heroInfo);
                            heroCount++;
                            msgHeros.HeroList.Add(heroInfo);
                        }
                    }
                }
                if (msgHeros.HeroList.Count > 0)
                {
                    pc.Write(msgHeros);
                }
            }
            // 通知pc AOI的monster
            if (monsterList.Count > 0)
            {
                MSG_GC_MONSTER_ENTER_LIST msgMons = new MSG_GC_MONSTER_ENTER_LIST();
                int monsterCount = 0;
                foreach (var item in monsterList)
                {
                    // 通知pc 格子里的pet
                    if (monsterCount > 40)
                    {
                        pc.Write(msgMons);
                        monsterCount = 0;
                        msgMons = new MSG_GC_MONSTER_ENTER_LIST();
                        MSG_ZGC_MONSTER_SIMPLE_INFO monInfo = new MSG_ZGC_MONSTER_SIMPLE_INFO();
                        item.Value.GetSimpleInfo(monInfo);
                        monsterCount++;
                        msgMons.MonList.Add(monInfo);
                    }
                    else
                    {
                        MSG_ZGC_MONSTER_SIMPLE_INFO monInfo = new MSG_ZGC_MONSTER_SIMPLE_INFO();
                        item.Value.GetSimpleInfo(monInfo);
                        monsterCount++;
                        msgMons.MonList.Add(monInfo);
                    }
                }
                if (msgMons.MonList.Count > 0)
                {
                    pc.Write(msgMons);
                }
            }
        }

        public void SendHiddenWeaponInfo()
        {
            MSG_ZGC_HERO_HIDDEN_WEAPON_INFO msg = new MSG_ZGC_HERO_HIDDEN_WEAPON_INFO();
            currentMap.HeroList.Values.ForEach(x =>
            {
                MSG_ZGC_HIDDEN_WEAPON_INFO info = x.GetHiddenWeaponInfo();
                if (info != null)
                {
                    msg.HiddenWeapons.Add(info);
                }
            });
            Write(msg);
        }
    }
}

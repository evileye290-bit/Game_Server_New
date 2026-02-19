using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap, IFieldObjectContainer
    {
        public IReadOnlyDictionary<int, PlayerChar> GetPlayers()
        { return pcList; }
        public IReadOnlyDictionary<int, Monster> GetMonsters()
        { return monsterList; }
        public IReadOnlyDictionary<int, Hero> GetHeros()
        { return heroList; }
        public IReadOnlyDictionary<int, Pet> GetPets()
        { return petList; }

        public IReadOnlyDictionary<int, Robot> GetRobots()
        { return robotList; }
    }
}

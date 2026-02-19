using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public interface IFieldObjectContainer
    {
        IReadOnlyDictionary<int, PlayerChar> GetPlayers();
        IReadOnlyDictionary<int, Monster> GetMonsters();
        IReadOnlyDictionary<int, Hero> GetHeros();
        IReadOnlyDictionary<int, Pet> GetPets();

        IReadOnlyDictionary<int, Robot> GetRobots();

    }
}

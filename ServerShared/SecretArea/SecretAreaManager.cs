using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class SecretAreaManager
    {
        private static int RankInitScore = 6000;
        public static int BuildSecretAreaJobRankScore(int id, int seconds)
        {
            return (id << 16) | ((RankInitScore - seconds) & 0x0000ffff);
        }

        public static void GetIdAndTimeByRankScore(int score, out int id, out int time)
        {
            id = score >> 16;
            time = RankInitScore - (score & 0x0000ffff);
        }
    }
}

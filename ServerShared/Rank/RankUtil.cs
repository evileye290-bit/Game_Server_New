using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class RankUtil
    {
        public static double BuildRankScore(int score, DateTime time)
        {
            string strScore = string.Format("{0}.{1}", score, int.MaxValue - Timestamp.GetUnixTimeStampSeconds(time));
            return double.Parse(strScore);
        }

        public static void GetRankScore(double dscore, out int score, out double time)
        {
            score = (int)dscore;
            time = Math.Floor(dscore);
        }
    }
}

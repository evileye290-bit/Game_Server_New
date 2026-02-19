using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class UidManager
    {
        /// <summary>
        /// 物品
        /// </summary>
        private int iIndex = 0;
        /// <summary>
        /// 邮件
        /// </summary>
        private int eIndex = 0;
        /// <summary>
        /// 仓库信息
        /// </summary>
        private int wIndex = 0;
        /// <summary>
        /// 宠物
        /// </summary>
        private int pIndex = 0;

        private uint timestamp = 0;
        private System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

        public ulong NewIuid(int mainId, int subId)
        {
            iIndex++;
            ulong itemId = ((ulong)mainId << 54) + ((ulong)subId << 48) + ((ulong)timestamp << 16) + (ulong)iIndex;
            return itemId;
        }

        public ulong NewEuid(int mainId, int subId)
        {
            eIndex++;
            ulong itemId = ((ulong)mainId << 54) + ((ulong)subId << 48) + ((ulong)timestamp << 16) + (ulong)eIndex;
            return itemId;
        }

        public ulong NewWuid(int mainId, int subId)
        {
            wIndex++;
            ulong itemId = ((ulong)mainId << 54) + ((ulong)subId << 48) + ((ulong)timestamp << 16) + (ulong)wIndex;
            return itemId;
        }

        public ulong NewPuid(int mainId, int subId)
        {
            pIndex++;
            ulong itemId = ((ulong)mainId << 54) + ((ulong)subId << 48) + ((ulong)timestamp << 16) + (ulong)pIndex;
            return itemId;
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public void ConvertTimestamp()
        {
            //startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            uint timestampTemp = (uint)(DateTime.Now - startTime).TotalSeconds;
            if (timestamp != timestampTemp)
            {
                timestamp = timestampTemp;
                iIndex = 0;
                eIndex = 0;
            }
        }
    }
}

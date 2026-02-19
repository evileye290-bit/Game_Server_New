using Logger;
using System.Collections.Generic;

namespace ServerFrame
{
    //跨zone管理
    public class ZoneTransformManager
    {
        private static ZoneTransformManager instance;

        private bool isForce;
        private List<int> fromZones = new List<int>();
        private List<int> toZones = new List<int>();

        public bool IsForce => isForce;
        public List<int> FromZones => fromZones;
        public List<int> ToZones => toZones;

        public static ZoneTransformManager Instance
        {
            get 
            {
                return instance != null ? instance : instance = new ZoneTransformManager();
            }
        }

        public void UpdateZonesInfo(bool isForce, List<int> fromZones, List<int> toZones)
        {
            ResetInfo();

            this.isForce = isForce;
            this.fromZones.AddRange(fromZones);
            this.toZones.AddRange(toZones);
        }

        /// <summary>
        /// 是否拦截该zone，不允许进入
        /// </summary>
        /// <param name="subId"></param>
        /// <returns></returns>
        public bool IsForbided(int subId)
        {
            return fromZones.Contains(subId);
        }

        private void ResetInfo()
        {
            isForce = false;
            fromZones.Clear();
            toZones.Clear();
        }
    }
}

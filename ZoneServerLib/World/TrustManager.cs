using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using ServerShared;

namespace ZoneServerLib
{
    // 托管系统
    public class TrustManager
    {
        public static Dictionary<int, List<Vector>> RobotMapPosList = new Dictionary<int, List<Vector>>();
        public TrustManager()
        {
            if (CONST.OPEN_ROBOT_PVP)
            {
                // 远古遗迹 501地图巡逻点
                List<Vector> RobotPosList_501 = new List<Vector>();
                RobotPosList_501.Add(new Vector(17.3f, 5.0f));
                RobotPosList_501.Add(new Vector(6.2f, -8.8f));
                RobotPosList_501.Add(new Vector(-7.7f, -12.0f));
                RobotPosList_501.Add(new Vector(-3.6f, 1.0f));
                RobotMapPosList.Add(501, RobotPosList_501);

                // 组队副本压测临时地图 90005
                List<Vector> RobotPosList_90005 = new List<Vector>();
                RobotPosList_90005.Add(new Vector(22f, -14f));
                RobotPosList_90005.Add(new Vector(32.5f, -0.6f));
                RobotPosList_90005.Add(new Vector(22f, -14f));
                RobotMapPosList.Add(90005, RobotPosList_90005);

                // 地下城 阵营战 502 巡逻点
                List<Vector> RobotPosList_502 = new List<Vector>();
                RobotPosList_502.Add(new Vector(9.36f, 3.5f));
                RobotPosList_502.Add(new Vector(8.36f, 22.4f));
                RobotPosList_502.Add(new Vector(-11.5f, 21.9f));
                RobotPosList_502.Add(new Vector(-10.0f, 0.76f));
                RobotMapPosList.Add(502, RobotPosList_502);
            }
        }

        public void Update(double dt)
        {
            if (CONST.OPEN_ROBOT_PVP == true)
            {
                foreach (var item in TrustList)
                {
                    item.Value.RobotTrustUpdate(dt);
                }
            }

        }
    }

}

using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class FieldMap
    {

        private Dictionary<int, Robot> robotList = new Dictionary<int, Robot>();
        /// <summary>
        /// 玩家
        /// </summary>
        public IReadOnlyDictionary<int, Robot> RobotList
        {
            get { return robotList; }
        }

        private List<int> robotRemoveList = new List<int>();

        private void UpdateRobot(float dt)
        {
            foreach (var robot in RobotList)
            {
                try
                {
                    //不用发送任何消息
                    robot.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }
        private void RemoveRobot()
        {
            if (robotRemoveList.Count > 0)
            {
                foreach (var instanceId in robotRemoveList)
                {
                    try
                    {
                        RemoveObjectSimpleInfo(instanceId);
                        robotList.Remove(instanceId);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                robotRemoveList.Clear();
            }
        }

        #region 添加
        protected void AddRobot(Robot robot)
        {
            Log.Write("Robot {0} enter map {1} channel {2} ", robot.Uid, MapId, Channel);

            robotList.Add(robot.InstanceId, robot);

            AddObjectSimpleInfo(robot.InstanceId, TYPE.ROBOT);
        }







        #endregion

        #region 删除

        private void RemoveRobot(Robot Robot)
        {
            if (Robot == null) return;
            //Robot.HeroMng.TakeBackHeroFromMap();
            // 通知已离开地图
            Log.Write("Robot {0} leave map {1} channel {2}", Robot.Uid, MapId, Channel);

            robotRemoveList.Add(Robot.InstanceId);
        }

        #endregion
    }
}

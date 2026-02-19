using DBUtility;
using Logger;
using ServerShared;

namespace RelationServerLib
{
    public class SpaceTimeTowerManager
    {
        private RelationServerApi server;

        /// <summary>
        /// 怪物期数
        /// </summary>
        private int period;

        /// <summary>
        /// 当期是否有玩家通关
        /// </summary>
        private bool passed;

        private bool initNotified;
        private double startServerDt = 0.0;
        
        public SpaceTimeTowerManager(RelationServerApi server)
        {
            this.server = server;
            LoadMonsterDifficultyFromDb();
        }
        
        private void LoadMonsterDifficultyFromDb()
        {
            QueryLoadSpaceTimeMonster query = new QueryLoadSpaceTimeMonster();
            server.GameDBPool.Call(query, ret =>
            {
                period = query.Period;
                passed = query.Passed;
                //NotifyZoneSpaceTimeMonsterInfo();
            });
        }

        public void Update(double dt)
        {
            if (initNotified) return;
            startServerDt += dt;
            if (startServerDt >= 15000.0 && server.ZoneManager.ServerList.Count > 0)
            {
                NotifyZoneSpaceTimeMonsterInfo(true);
                initNotified = true;
            }
        }
        
        private void NotifyZoneSpaceTimeMonsterInfo(bool notifyPc = false)
        {
            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).NotifySpaceTimeMonsterInfo(period, passed, notifyPc);
            }
        }

        public void NotifyUpdateMonsterInfo(int period, bool passed)
        {
            this.period = period;
            this.passed = passed;
            NotifyZoneSpaceTimeMonsterInfo();
        }

        public void Refresh()
        {
            RefreshMonsterPeriod();
            NotifyZoneSpaceTimeMonsterInfo(true);
        }

        private void RefreshMonsterPeriod()
        {
            if (period >= SpaceTimeTowerLibrary.MonsterDifficultyLimit) return;
            if (passed)
            {
                period++;
                passed = false;
                server.GameDBPool.Call(new QueryUpdateSpaceTimeMonster(period, passed));
            }
        }
    }
}
namespace BattleServerLib.Client
{
    public class BossTeamBattleClients
    {
        public BattleClient Attacker1 { get; set; }
        public BattleClient Attacker2 { get; set; }
        public BattleClient Boss { get; set; }
        public int GameLevelId { get; set; }

        public bool AllReady()
        {
            return (Attacker1.Ready && Attacker2.Ready);
        }
    }
}

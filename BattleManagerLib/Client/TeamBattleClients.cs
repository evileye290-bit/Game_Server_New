namespace BattleServerLib.Client
{
    public class TeamBattleClients
    {
        public BattleClient Attacker1 { get; set; }
        public BattleClient Attacker2 { get; set; }
        public BattleClient Defender1 { get; set; }
        public BattleClient Defender2 { get; set; }
        public int GameLevelId { get; set; }

        public bool AllReady()
        {
            return (Attacker1.Ready && Attacker2.Ready && Defender1.Ready && Defender2.Ready);
        }
    }
}

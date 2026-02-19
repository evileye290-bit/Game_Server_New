namespace ZoneServerLib
{
    public class HighGoForward : BaseHighEvent
    {
        public HighGoForward(IslandHighManager islandHighManager) : base(islandHighManager)
        {
        }

        public override void Invoke(int param)
        {
            IslandHighManager.AddGrid(param);
        }
    }
}

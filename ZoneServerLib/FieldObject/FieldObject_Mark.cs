namespace ZoneServerLib
{
    partial class FieldObject
    {
        protected MarkManager markManager ;
        public MarkManager MarkManager
        { get { return markManager; } }

        public void InitMarkManager()
        {
            markManager = new MarkManager(this);
        }

        public void AddMark(FieldObject field, int markId, int count)
        {
            if (IsDead || markManager == null)
            {
                return;
            }
            markManager.AddMark(field, markId, count);
        }

        public void RemoveMark(int markId)
        {
            if (markManager == null)
            {
                return;
            }
            markManager.RemoveMark(markId);
        }

    }
}


namespace ManagerServerLib
{
    public class OfflineClient
    {
        public int Uid;
        public int SubId;
        public int Token;
        public int MapId;
        public int Channel;
        public int MainId;

        public OfflineClient(int uid, int main_id, int sub_id, int token, int map_id, int channel)
        {
            Uid = uid;
            SubId = sub_id;
            Token = token;
            MapId = map_id;
            Channel = channel;
            MainId = main_id;
        }
    }
}

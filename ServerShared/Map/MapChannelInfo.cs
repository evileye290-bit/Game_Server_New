using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared.Map
{
    public class MapChannelInfo
    {
        public int MapId;
        public int MinChannel;
        public int MaxChannel;
        public MapChannelInfo(int map_id, int min_channel, int max_channel)
        {
            MapId = map_id;
            MinChannel = min_channel;
            MaxChannel = max_channel;
        }
    }
}

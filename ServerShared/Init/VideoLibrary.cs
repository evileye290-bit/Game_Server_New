using DataProperty;

namespace ServerShared
{
    public static class VideoLibrary
    {
        public static string URL;
        public static bool Upload;
        public static string VedioFileSuffix;
        public static string VedioInfoSuffix;
        public static int VedioDungeonId;

        public static void Init()
        {
            Data data = DataListManager.inst.GetData("VideoConfig", 1);
            Upload = data.GetBoolean("upload");
            VedioDungeonId = data.GetInt("VedioDungeonId");
            VedioFileSuffix = data.GetString("vedioFileSuffix");
            VedioInfoSuffix = data.GetString("vedioInfoSuffix");

            data = DataListManager.inst.GetData("UrlConfig", 1);
            URL = data.GetString("videoUrlServer");
        }
    }
}


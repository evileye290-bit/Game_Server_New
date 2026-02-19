using CommonUtility;
using Message.IdGenerator;
using ServerShared;
using System;
using System.IO;

public class VideoManager
{
    private static string LocalDir = "C:/Video/";

    private bool Closed = false;
    private MapType mapType;
    private MemoryStream Content = new MemoryStream(4096);
    private int attackerUid, defenderUid;

    public bool VedioStart;
    public string FilePath = string.Empty, fileName = string.Empty;

//#if DEBUG
//    public bool Valid => mapType == MapType.CrossFinals || mapType == MapType.CrossBattle;
//#else
    public bool Valid => mapType == MapType.CrossFinals || mapType == MapType.CrossChallengeFinals;
//#endif

    public VideoManager(MapType type)
    {
        mapType = type;
    }

    public void SetBattleUid(DateTime now, int attacker, int defencer)
    {
        this.attackerUid = attacker;
        this.defenderUid = defencer;

        fileName = $"{now.ToString("yyyy-MM-dd-HH-mm-ss")}_{attacker}_{defencer}.log";

        string info = $"{now.ToString("yyyy-MM-dd")}/{mapType}";

        FilePath = $"{info}/{fileName}";

        LocalDir = $"{LocalDir}/{info}";

        fileName = $"{LocalDir}/{fileName}";
    }

    public void Start()
    {
        VedioStart = true;
    }

    public void Write<T>(T msg) where T : Google.Protobuf.IMessage
    {
        if (!Valid || Closed)
        {
            return;
        }

        MemoryStream body = new MemoryStream();
        MessagePacker.ProtobufHelper.Serialize(body, msg);
        ushort len = (ushort)body.Length;
        Content.Write(BitConverter.GetBytes(len), 0, 2);
        Content.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
        Content.Write(body.GetBuffer(), 0, (int)len);
    }

    public void Close()
    {
        if (!Valid) return;
        
        Closed = true;

        // 记录完毕，上传 
        Upload(FilePath, Content);

#if DEBUG
        //WriteToFile();

        Logger.Log.Warn(FilePath);
#endif
    }

    private void Upload(string file_name, MemoryStream content)
    {
        if (VideoLibrary.Upload && Valid)
        {
            UpFileInfo vedio = new UpFileInfo(file_name, content);

            HttpUploader.UploadVedio(vedio);
        }
    }

    private void WriteToFile()
    {
        try
        {
            if (Directory.Exists(LocalDir) == false)
            {
                Directory.CreateDirectory(LocalDir);
            }

            using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew))
            {
                fileStream.Write(Content.GetBuffer(), 0, (int)Content.Position);
            }
        }
        catch (Exception ex)
        {
            Logger.Log.Warn(ex);
        }
    }
}

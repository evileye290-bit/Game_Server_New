using System.IO;

public class UpFileInfo
{
    public string FileName { get; private set; }
    public MemoryStream Content { get; private set; }

    public UpFileInfo(string file_name, MemoryStream content)
    {
        FileName = file_name;
        Content = content;
    }
}

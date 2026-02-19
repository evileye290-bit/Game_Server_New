using DataProperty;
using EnumerateUtility;
using Logger;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class UpFileInfo
    {
        public string FileName;
        public MemoryStream Content;
        public UpFileInfo(string file_name, MemoryStream content)
        {
            FileName = file_name;
            Content = content;
        }
    }

    public class RecommentVedioInfo
    {
        public RecommentVedioType RecommentType;
        public int LadderLevel;
        public VedioFilterType FilterType;
        public RecommentVedioInfo(RecommentVedioType recomment_type, VedioFilterType filter_type, int ladder_level)
        {
            RecommentType = recomment_type;
            FilterType = filter_type;
            LadderLevel = ladder_level;
        }
    }


    public class VedioConfig
    {
        public static string URL;
        public static bool Upload;
        public static int MaxVedioCount;
        public static int VedioPerPage;

        public static int SingleToughTime;
        public static int SingleToughMana;
        public static int SingleToughHpLeft;
        public static float SingleToughCardLevelDelta;

        public static int TeamToughTime;
        public static int TeamToughMana;
        public static int TeamToughHpLeft;
        public static float TeamToughCardLevelDelta;

        public static int SingleFastTime;
        public static int SingleFastMana;
        public static float SingleFastCardLevelDelta;
        public static int SingleFastLoserSkillCount;

        public static int TeamFastTime;
        public static int TeamFastMana;
        public static float TeamFastCardLevelDelta;
        public static int TeamFastLoserSkillCount;

        public static string VedioFileSuffix;
        public static string VedioInfoSuffix;

        public static void Init()
        {
            Data data = DataListManager.inst.GetData("VedioConfig", 1);
            Upload = data.GetBoolean("upload");
            MaxVedioCount = data.GetInt("maxVedioCount");
            VedioPerPage = data.GetInt("vedioPerPage");

            SingleToughTime = data.GetInt("singleToughTime");
            SingleToughMana = data.GetInt("singleToughMana");
            SingleToughHpLeft = data.GetInt("singleToughHpLeft");
            SingleToughCardLevelDelta = data.GetFloat("singleToughCardLevelDelta");

            TeamToughTime = data.GetInt("teamToughTime");
            TeamToughMana = data.GetInt("teamToughMana");
            TeamToughHpLeft = data.GetInt("teamToughHpLeft");
            TeamToughCardLevelDelta = data.GetFloat("teamToughCardLevelDelta");

            SingleFastTime = data.GetInt("singleFastTime");
            SingleFastMana = data.GetInt("singleFastMana");
            SingleFastCardLevelDelta = data.GetFloat("singleFastCardLevelDelta");
            SingleFastLoserSkillCount = data.GetInt("singleFastLoserSkillCount");

            TeamFastTime = data.GetInt("teamFastTime");
            TeamFastMana = data.GetInt("teamFastMana");
            TeamFastCardLevelDelta = data.GetFloat("teamFastCardLevelDelta");
            TeamFastLoserSkillCount = data.GetInt("teamFastLoserSkillCount");

            VedioFileSuffix = data.GetString("vedioFileSuffix");
            VedioInfoSuffix = data.GetString("vedioInfoSuffix");

            data = DataListManager.inst.GetData("UrlConfig", 1);
            URL = data.GetString("vedioUrl");
        }
    }

    public class HttpUploader
    {
        Queue<UpFileInfo> storeQueue = new Queue<UpFileInfo>();
        private object storeLock = new object();
        Queue<UpFileInfo> uploadQueue = new Queue<UpFileInfo>();
        public double lastTime;
        public void AddVedio(UpFileInfo stream)
        {
            lock (storeLock)
            {
                storeQueue.Enqueue(stream);
            }
        }

        public void Update()
        {
            lock (storeLock)
            {
                while (storeQueue.Count > 0)
                {
                    UpFileInfo vedio = storeQueue.Dequeue();
                    uploadQueue.Enqueue(vedio);
                }
            }
            while (uploadQueue.Count > 0)
            {
                UpFileInfo vedio = uploadQueue.Dequeue();
                UploadVedio(vedio);
            }
        }

        public void Run()
        {
            var time = new CommonUtility.Time();
            time.Init();
            while (true)
            {
                var dt = time.Update();
                lastTime = dt.TotalMilliseconds;

                if (lastTime > 10)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep(10);
                }
                Update();
            }
        }

        private void UploadVedio(UpFileInfo vedio)
        {
            vedio.Content.Seek(0, SeekOrigin.Begin);
            // 时间戳，用做boundary
            string boundaryBase = "ruafoo1";
            //根据uri创建HttpWebRequest对象
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(VedioConfig.URL));
            httpReq.Method = "POST";
            httpReq.AllowWriteStreamBuffering = false; //对发送的数据不使用缓存
            httpReq.Timeout = 30000;  //设置获得响应的超时时间（30秒）
            httpReq.ContentType = string.Format("multipart/form-data; boundary={0}", boundaryBase);

            //头信息
            string boundary = string.Format("--{0}", boundaryBase);
            //string dataFormat = boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\nContent-Type:application/octet-stream\r\n\r\n";
            string dataFormat = "{0}\r\nContent-Disposition: form-data; name=\"file\";filename=\"{1}\"\r\nContent-Type:application/octet-stream\r\n\r\n";
            string header = string.Format(dataFormat, boundary, vedio.FileName);
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(header);

            //结束边界
            byte[] boundaryBytes = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundaryBase));

            long length = vedio.Content.Length + postHeaderBytes.Length + boundaryBytes.Length;

            httpReq.ContentLength = length;//请求内容长度

            try
            {
                //每次上传4k
                int bufferLength = 4096;
                byte[] buffer = new byte[bufferLength];

                //已上传的字节数
                long offset = 0;
                int size = vedio.Content.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();

                //发送请求头部消息
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    size = vedio.Content.Read(buffer, 0, bufferLength);
                }

                //添加尾部边界
                postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                postStream.Close();

                //获取服务器端的响应
                using (HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse())
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    string returnValue = readStream.ReadToEnd();
                    //Console.WriteLine(returnValue);
                    response.Close();
                    readStream.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("upload vedio failed： {0}", ex.Message);
            }
            finally
            {
            }
        }
    }

    public class VedioManager
    {
        private void SaveCurFps()
        {

        }

        public bool VedioStart;
        public string VedioName = string.Empty;
        private bool Closed = false;
        public MemoryStream Content = new MemoryStream(4096);
        public MemoryStream InfoStream = new MemoryStream();
        public HttpUploader Uploader;

        public VedioManager(DateTime now,HttpUploader uploader)
        {
            this.Uploader = uploader;

            string fileName = string.Empty;

            string Dir = string.Format("{0}/{1}", now.ToString("yyyy-MM-dd"), now.Hour.ToString().PadLeft(2, '0'));
            VedioName = string.Format("{0}/{1}", Dir, fileName);
        }

        public VedioManager()
        {

        }


        public void Start()
        {
            VedioStart = true;
        }

        public void Close()
        {
            Closed = true;
            // 记录完毕，上传 
            Upload(VedioName, Content);
        }
        public void Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (Closed)
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

        //public bool Write(uint pid, MemoryStream body)
        //{
        //    MemoryStream header = new MemoryStream(sizeof(ushort) + sizeof(uint));
        //    ushort len = (ushort)body.Length;
        //    header.Write(BitConverter.GetBytes(len), 0, 2);
        //    header.Write(BitConverter.GetBytes(pid), 0, 4);

        //    return Write(header, body);
        //}

        public void Upload(string file_name, MemoryStream content)
        {
            if (VedioConfig.Upload)
            {
                UpFileInfo vedio = new UpFileInfo(file_name, content);
                Uploader.AddVedio(vedio);
            }
        }
    }

    
}

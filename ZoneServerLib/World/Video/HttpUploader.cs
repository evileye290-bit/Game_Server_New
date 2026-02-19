using Logger;
using ServerShared;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

public class HttpUploader
{
    private volatile bool stoped = false;
    private ConcurrentQueue<UpFileInfo> storeQueue = new ConcurrentQueue<UpFileInfo>();
    private ConcurrentQueue<UpFileInfo> uploadQueue = new ConcurrentQueue<UpFileInfo>();
    public double lastTime;

    public void Start()
    {
        //Thread thread = new Thread(Run);
        //thread.IsBackground = true;
        //thread.Start();
    }

    public void Stop()
    {
        stoped = true;
    }

    public void AddVedio(UpFileInfo stream)
    {
        storeQueue.Enqueue(stream);
    }

    public void Update()
    {
        while (storeQueue.Count > 0)
        {
            UpFileInfo video;
            if (storeQueue.TryDequeue(out video))
            {
                uploadQueue.Enqueue(video);
            }
        }

        while (uploadQueue.Count > 0)
        {
            UpFileInfo video;
            if (uploadQueue.TryDequeue(out video))
            {
                UploadVedio(video);
            }
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

            if (stoped)
            {
                break;
            }
        }
    }

    public static async void UploadVedio(UpFileInfo vedio)
    {

        try
        {
            vedio.Content.Seek(0, SeekOrigin.Begin);
            // 时间戳，用做boundary
            string boundaryBase = "ruafoo1";
            //根据uri创建HttpWebRequest对象
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(VideoLibrary.URL));
            httpReq.Method = "POST";
            httpReq.AllowWriteStreamBuffering = false; //对发送的数据不使用缓存
            httpReq.Timeout = 30000;  //设置获得响应的超时时间（30秒）
            httpReq.ContentType = string.Format("multipart/form-data; boundary={0}", boundaryBase);

            //头信息
            string boundary = string.Format("--{0}", boundaryBase);
            string header = $"{boundary}\r\nContent-Disposition: form-data; name=\"file\";filename=\"{vedio.FileName}\"\r\nContent-Type:application/octet-stream\r\n\r\n";
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(header);

            //结束边界
            byte[] boundaryBytes = Encoding.UTF8.GetBytes($"\r\n--{boundaryBase}--\r\n");

            long length = vedio.Content.Length + postHeaderBytes.Length + boundaryBytes.Length;

            httpReq.ContentLength = length;//请求内容长度

            int bufferLength = 1024 * 1024;
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
            using (HttpWebResponse response = (HttpWebResponse)await httpReq.GetResponseAsync())
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string returnValue = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }
        }
        catch (Exception ex)
        {
            Log.Error("upload vedio failed： {0}", ex.Message);
        }
    }
}

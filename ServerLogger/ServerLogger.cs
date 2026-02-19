using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection; // For Assembly name, debugging
using System.Collections.Concurrent;
using NLog;

namespace Logger
{
    public class ServerLogger : AbstractLogger
    {
        private bool infoConsolePrint = false;
        private bool warnConsolePrint = true;
        private bool errorConsolePrint = true;
        private bool alertConsolePrint = true;
        private bool doFilePrint = false;

        private string logFileName = "";
        private int writeLength = 0;
        private int flushLength = 0;
        private readonly int fileSize = 1048576; // 每个log文件大小为1M
        private readonly int flushLengthThreshold = 2048; // 超过2048字节则写入磁盘
        private readonly int flushTimeThreshold = 30;     // 超过30秒则写入磁盘
        private string prefix;
        //StreamWriter tw;
        //public string Logo = "";
        //private string baseDir = "C:/Log/GameLog/";
        //private DateTime lastLogTime = DateTime.MaxValue;

        //ILoggerTimestamp LoggerTimestamp;

        private bool _isinit;

        private bool _logTraceEnabled;
        private bool _logDebugEnable;
        private bool _logWarnEnable;

        private bool _logInfoEnable;
        private bool _logErrorEnable;
        private bool _logFatalEnabled;
        private readonly NLog.Logger _logger;
        private static ConcurrentDictionary<string, NLog.Logger> _customLoggers;
        public void Init(string prefix, bool infoConsolePrint, bool warnConsolePrint, bool errorConsolePrint, bool filePrint)
        {
            this.infoConsolePrint = infoConsolePrint;
            this.warnConsolePrint = warnConsolePrint;
            this.errorConsolePrint = errorConsolePrint;
            doFilePrint = filePrint;
            this.prefix = prefix;
            //We create a new log file every time we run the app.
            //logFileName = GetSaveFileName(prefix);

            //// create a writer and open the file
            //tw = new StreamWriter(logFileName);
            //tw.AutoFlush = false;
            SetProperty("SERVERNAME", prefix);
        }

        public void SetProperty(string key, string value)
        {
            _logger.SetProperty(key, value);
        }

        public ServerLogger()
        {
            _customLoggers = new ConcurrentDictionary<string, NLog.Logger>();
            //this._logger = LogManager.GetLogger(name);
            _logger = LogManager.GetCurrentClassLogger();
            _isinit = false;
            _logInfoEnable = false;
            _logErrorEnable = false;
            _logWarnEnable = false;
            _logTraceEnabled = false;
            _logDebugEnable = false;
            _logFatalEnabled = false;
            if (!_isinit)
            {
                _isinit = true;
                SetConfig();
            }
        }
        private void SetConfig()
        {
            _logTraceEnabled = _logger.IsTraceEnabled;
            _logDebugEnable = _logger.IsDebugEnabled;
            _logInfoEnable = _logger.IsInfoEnabled;
            _logWarnEnable = _logger.IsWarnEnabled;
            _logErrorEnable = _logger.IsErrorEnabled;
            _logFatalEnabled = _logger.IsFatalEnabled;
        }
        //public ServerLogger(ILoggerTimestamp loggerTimestamp)
        //{
        //    this.LoggerTimestamp = loggerTimestamp;
        //}

        //private string GetSaveFileName(string prefix)
        //{
        //    string path = baseDir + DateTime.Now.ToString("yyyy_MM_dd") + "/";
        //    try
        //    {
        //        if (Directory.Exists(path) == false)
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //    }
        //    catch
        //    {
        //        Log.Warn("Could not create save directory for log. See Logger.cs.");
        //    }

        //    //string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
        //    //Int32 index = assemblyFullName.IndexOf(',');
        //    string dt = "" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        //    //Save directory is created in ConfigFileHandler
        //    return path + prefix + "-" + dt + ".txt.now"; ;
        //}

        //private void CheckFileSize(int length, DateTime now)
        //{
        //    bool needCreate = false;
        //    writeLength += length;
        //    flushLength += length;
        //    if (flushLength >= flushLengthThreshold || (now.Date-lastLogTime).TotalSeconds > flushTimeThreshold)
        //    {
        //        tw.Flush();
        //        flushLength = 0;
        //    }
        //    if (writeLength > fileSize)
        //    {
        //        needCreate = true;
        //    }
        //    if (lastLogTime != DateTime.MaxValue && lastLogTime.Date != now.Date)
        //    {
        //        needCreate = true;
        //    }
        //    lastLogTime = now;
        //    if (needCreate)
        //    {
        //        tw.Close();
        //        // 日志输出完毕，删除 .now 后缀
        //        string closeFileName = logFileName.Replace(".now", "");
        //        try
        //        {
        //            System.IO.File.Move(logFileName, closeFileName);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Alert(e.ToString());
        //        }
        //        logFileName = GetSaveFileName(prefix);
        //        // 创建新的log file
        //        tw = new StreamWriter(logFileName);
        //        tw.AutoFlush = false;
        //        writeLength = 0;
        //        flushLength = 0;
        //    }
        //}

        public override void Write(object obj)
        {
            //            try
            //            {
            //                string nowStr = LoggerTimestamp.NowString();
            //                string info = string.Format("{0}{1}[INFO] {2}", nowStr, Logo, obj);
            //                tw.WriteLine(info);
            //                CheckFileSize(info.Length, LoggerTimestamp.Now());
            //#if DEBUG
            //                if (infoConsolePrint)
            //                {
            //                    Console.ForegroundColor = ConsoleColor.Green;
            //                    Console.WriteLine(info);
            //                }
            //#endif
            //            }
            //            catch (Exception e)
            //            {
            //                //Console.WriteLine(e.ToString());
            //            }
            _logger.Log(LogLevel.Trace, obj);
        }

        public override void WriteLine(object obj)
        {
            //try
            //{
            //    Write(obj);
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Trace, obj);
        }


        public override void Debug(object obj)
        {
#if DEBUG
            //try
            //{
            //    string nowStr = LoggerTimestamp.NowString();
            //    string info = string.Format("{0}{1}[DEBUG] {2}", nowStr, Logo, obj);
            //    tw.WriteLine(info);
            //    CheckFileSize(info.Length, LoggerTimestamp.Now());

            //    Console.ForegroundColor = ConsoleColor.Magenta;
            //    Console.WriteLine(info);
            //    Console.ResetColor();
            //}
            //catch (Exception e)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine(e.ToString());
            //    Console.ResetColor();
            //}
            _logger.Log(LogLevel.Debug, obj);
#endif

        }

        public override void DebugLine(object obj)
        {
#if DEBUG
            //Debug(obj);
            _logger.Log(LogLevel.Debug, obj);

#endif
        }

        public override void Warn(object obj)
        {
            //try
            //{
            //    string nowStr = LoggerTimestamp.NowString();
            //    string info = string.Format("{0}{1}[WARN] {2}", nowStr, Logo, obj);
            //    tw.WriteLine(info);
            //    CheckFileSize(info.Length, LoggerTimestamp.Now()); ;
            //    if (warnConsolePrint)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Yellow;
            //        Console.WriteLine(info);
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Warn, obj);
        }

        public override void WarnLine(object obj)
        {
            //try
            //{
            //    Warn(obj);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Warn, obj);
        }

        public override void Error(object obj)
        {
            //try
            //{
            //    string nowStr = LoggerTimestamp.NowString();
            //    string info = string.Format("{0}{1}[ERROR] {2}", nowStr, Logo, obj);
            //    tw.WriteLine(info);
            //    CheckFileSize(info.Length, LoggerTimestamp.Now());
            //    if (errorConsolePrint == true)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.WriteLine(info);
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Error, obj);
        }
        public override void ErrorLine(object obj)
        {

            //try
            //{
            //    Error(obj);
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Error, obj);
        }

        public override void Alert(object obj)
        {
            //try
            //{
            //    string nowStr = LoggerTimestamp.NowString();
            //    string info = string.Format("{0}{1}[ALERT] {2}", nowStr, Logo, obj);
            //    tw.WriteLine(info);
            //    CheckFileSize(info.Length, LoggerTimestamp.Now());
            //    if (alertConsolePrint == true)
            //    {
            //        Console.ForegroundColor = ConsoleColor.DarkYellow;
            //        Console.WriteLine(info);
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Warn, obj);
        }

        public override void AlertLine(object obj)
        {

            //try
            //{
            //    Alert(obj);
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Warn, obj);
        }

        public override void Info(object obj)
        {
            //try
            //{
            //    string nowStr = LoggerTimestamp.NowString();
            //    string info = string.Format("{0}{1}[info] {2}", nowStr, Logo, obj);
            //    tw.WriteLine(info);
            //    CheckFileSize(info.Length, LoggerTimestamp.Now());
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.WriteLine(info);
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine(e.ToString());
            //}
            _logger.Log(LogLevel.Info, obj);
        }

        public override void Close()
        {
            //if (tw == null)
            //{
            //    return;
            //}
            //WriteLine("This session was logged to " + logFileName);
            //tw.Close();
            //// 日志输出完毕，删除 .now 后缀
            //string closeFileName = logFileName.Replace(".now", "");
            //try
            //{
            //    System.IO.File.Move(logFileName, closeFileName);
            //}
            //catch (Exception e)
            //{
            //    Log.Alert(e.ToString());
            //}
            //tw = null;
        }
    }
}
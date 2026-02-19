using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerShared
{
    public class FpsAndCpuInfo
    {
        public double fps = 0;
        public double sleepTime = 0;
        public long memorySize = 0;
    }

    public class FrameManager
    {
        double FPS = 40;
        const double ONESECOND = 1000.0;
        /// <summary>
        /// 每帧耗时 （单位 millisecond）
        /// </summary>
        private double _timePerFrame;
        private double TimePerFrame
        {
            get { return _timePerFrame; }
        }

        public DateTime Now
        {
            get
            {
                return _frameBeginTime;
            }
        }

        /// <summary>
        /// 标示时间起点（1秒）
        /// </summary>
        DateTime _flagStartTime;

        /// <summary>
        /// 1帧开始时间
        /// </summary>
        DateTime _frameBeginTime;
        /// <summary>
        /// 1帧结束时间
        /// </summary>
        DateTime _frameEndTime;

        /// <summary>
        /// 帧数
        /// </summary>
        private int _frames = 0;
        /// <summary>
        /// 睡眠数
        /// </summary>
        private double _sleepTimes = 0;

        /// <summary>
        /// 统计时间
        /// </summary>
        private int _statTime = 0;
        /// <summary>
        /// 统计帧数
        /// </summary>
        private double _statFrames = 0;
        /// <summary>
        /// 统计睡眠时间
        /// </summary>
        private double _statSleepTime = 0;

        /// <summary>
        /// fps 平均值
        /// </summary>
        private double _averageFramesPerSecond = 0;
        /// <summary>
        /// 每秒睡眠时间 平均值
        /// </summary>
        private double _averageSleepTimePerSecond = 0;

        /// <summary>
        /// 统计最近10秒内的 平均FPS和CPU睡眠时间
        /// </summary>
        FpsAndCpuInfo _msg = null;

        //public FrameManager()
        //{
        //    _now = DateTime.Now;
        //    _flagStartTime = _now;
        //    _lastUpdateTime = _now;
        //    _nextUpdateTime = _now;
        //    _curFrameBeginTime = _now;
        //    _curFrameEndTime = _now;
        //    _frames = 0;
        //    _sleepTimes = 0;
        //    _statTime = 0;
        //    _statFrames = 0;
        //    _statSleepTime = 0;
        //    _averageFramesPerSecond = 0;
        //    _averageSleepTimePerSecond = 0;
        //    _msg = null;
        //    _timePerFrame = ONESECOND / FPS;
        //}
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(DateTime now)
        {
            _flagStartTime = now;
            _frameBeginTime = now;
            _frameEndTime = now;
            _frames = 0;
            _sleepTimes = 0;
            _statTime = 0;
            _statFrames = 0;
            _statSleepTime = 0;
            _averageFramesPerSecond = 0;
            _averageSleepTimePerSecond = 0;

            _msg = new FpsAndCpuInfo();
            _msg.fps = _averageFramesPerSecond;
            _msg.sleepTime = _averageSleepTimePerSecond;
            _msg.memorySize = 0;

            _timePerFrame = ONESECOND / FPS;
        }
        /// <summary>
        /// 设置帧起点
        /// </summary>
        public void SetFrameBegin()
        {
            _frameBeginTime = DateTime.Now;

            _sleepTimes += (_frameBeginTime - _frameEndTime).TotalMilliseconds;

            if ((_frameBeginTime - _flagStartTime).TotalSeconds > 1)
            {
                //以1秒为周期进行记录
                RecordFPSAndCpuInfo(_frames, (int)_sleepTimes);
                //Log.Debug("fps {0}, sleepTime {1} ,memorySize {2}", _msg.fps, _msg.sleepTime, _msg.memorySize);
                _flagStartTime = _frameBeginTime;
                _sleepTimes = 0;
                _frames = 0;
            }
        }

        /// <summary>
        /// 设置帧结束点
        /// </summary>
        public void SetFrameEnd()
        {
            _frameEndTime = DateTime.Now;
            TimeSpan curFrameConsume = _frameEndTime - _frameBeginTime;
            double sleepMilliseconds = TimePerFrame - curFrameConsume.TotalMilliseconds;
            if (sleepMilliseconds > 0)
            {
                TimeSpan sleep = TimeSpan.FromMilliseconds(sleepMilliseconds);
                Thread.Sleep(sleep);
            }
            _frames++;
        }

        /// <summary>
        // 统计最近10秒内的 平均FPS和CPU睡眠时间，反映当前进程状态
        /// </summary>
        /// <param name="frameCount">1秒内的帧数</param>
        /// <param name="sleepTime">1秒内睡眠时间</param>
        private FpsAndCpuInfo RecordFPSAndCpuInfo(int frameCount, int sleepTime)
        {
            //10秒一个周期 记录平均值
            if (_statTime < 10)
            {
                _statFrames += frameCount;
                _statSleepTime += sleepTime;
                _statTime++;
            }
            else
            {
                _averageFramesPerSecond = (int)(_statFrames / _statTime);
                _averageSleepTimePerSecond = (int)(_statSleepTime / _statTime);

                _msg.fps = _averageFramesPerSecond;
                _msg.sleepTime = _averageSleepTimePerSecond;
                Process proc = Process.GetCurrentProcess();
                _msg.memorySize = (long)(proc.PrivateMemorySize64 / (1024 * 1024));

                _statTime = 0;
                _statFrames = 0;
                _statSleepTime = 0;
                return _msg;
            }
            return _msg;
        }
        /// <summary>
        /// 获取 平均值
        /// </summary>
        /// <returns></returns>
        public FpsAndCpuInfo GetFPSAndCpuInfo()
        {
            return _msg;
        }
        public void ClearFPSAndCpuInfo()
        {
            _msg = null;
        }
        public int GetFrame()
        {
            return (int)_averageFramesPerSecond;
        }
        public int GetSleep()
        {
            return (int)_averageSleepTimePerSecond;
        }
        /// <summary>
        /// 设置 fps
        /// </summary>
        /// <param name="fps"> fps值</param>
        /// <returns></returns>
        public void SetFPS(double fps)
        {
            FPS = fps;
            _timePerFrame = ONESECOND / FPS;
            Log.Info("Set a new FPS: {0}", FPS);
        }
    }
}

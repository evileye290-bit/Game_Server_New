using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    public class BaseTimer
	{
		protected object Param { get; private set; }
		protected Action<object> Callback { get; private set; }
		protected TimerManager TimerManager{get; private set;}

		public long Id { get; private set; }

		public BaseTimer(TimerManager manager, Action<object> action, object param)
		{
			this.Callback = action;
			this.Param = param;
			this.TimerManager = manager;
			Id = manager.GetId;
		}

		public virtual void Run() { }
	}

	public class OnceTimer : BaseTimer
	{
		public OnceTimer(TimerManager timerManager, Action<object> action, Object param) : base(timerManager, action, param)
		{
		}

		public override void Run()
		{
			try
			{
				this.Callback.Invoke(Param);
				TimerManager.Remove(Id);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}

	public class RepeatedTimer : BaseTimer
	{
		private ulong count;
		private ulong startTime;
		private ulong repeatedTime;

		// 下次一是第几次触发

		public RepeatedTimer(TimerManager manager, ulong repeateTime, Action<object> action, Object param) : base(manager, action, param)
		{
			repeatedTime = repeateTime;
		}

		public override void Run()
		{
			++this.count;
			ulong tillTime = this.startTime + this.repeatedTime * this.count;
			TimerManager.AddToTimeId(tillTime, this.Id);

			try
			{
				this.Callback?.Invoke(Param);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}

	public class TimerManager
	{
		private long id = 0;
		private static TimerManager instance;
		private readonly Dictionary<long, BaseTimer> timers = new Dictionary<long, BaseTimer>();

		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		public readonly MultiMap<ulong, long> TimeId = new MultiMap<ulong, long>();

		private readonly Queue<ulong> timeOutTime = new Queue<ulong>();

		private readonly Queue<long> timeOutTimerIds = new Queue<long>();

		// 记录最小时间，不用每次都去MultiMap取第一个值
		private ulong minTime;

		public static TimerManager Instance
		{
			get
			{
				if(instance == null)
				{
					instance = new TimerManager();
				}
				return instance;
			}
		}

		public long GetId => ++id;

		public void Update()
		{
			if (this.TimeId.Count == 0)
			{
				return;
			}

			ulong timeNow = Timestamp.GetUnixTimeStamp(DateTime.Now);

			if (timeNow < this.minTime)
			{
				return;
			}

			foreach (KeyValuePair<ulong, List<long>> kv in this.TimeId.GetDictionary())
			{
				ulong k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				this.timeOutTime.Enqueue(k);
			}

			while (this.timeOutTime.Count > 0)
			{
				ulong time = this.timeOutTime.Dequeue();
				foreach (long timerId in this.TimeId[time])
				{
					this.timeOutTimerIds.Enqueue(timerId);
				}
				this.TimeId.Remove(time);
			}

			while (this.timeOutTimerIds.Count > 0)
			{
				long timerId = this.timeOutTimerIds.Dequeue();
				BaseTimer timer;
				if (!this.timers.TryGetValue(timerId, out timer))
				{
					continue;
				}

				timer.Run();
			}
		}

		public long NewOnceTimer(ulong tillTime, Action<object> action, object param = null)
		{
			OnceTimer timer = new OnceTimer(this, action, param);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return timer.Id;
		}

		/// <summary>
		/// 创建一个RepeatedTimer
		/// </summary>
		/// <param name="time"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public long NewRepeatedTimer(ulong time, Action<object> action, object param = null)
		{
			if (time < 30)
			{
				throw new Exception($"repeated time < 30");
			}
			ulong tillTime = Timestamp.GetUnixTimeStamp(DateTime.Now) + time;
			RepeatedTimer timer = new RepeatedTimer(this, time, action, param);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return timer.Id;
		}

		public RepeatedTimer GetRepeatedTimer(long id)
		{
			BaseTimer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return null;
			}
			return timer as RepeatedTimer;
		}

		public void Remove(long id)
		{
			if (id == 0)
			{
				return;
			}
			BaseTimer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);

			(timer as IDisposable)?.Dispose();
		}

		public OnceTimer GetOnceTimer(long id)
		{
			BaseTimer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return null;
			}
			return timer as OnceTimer;
		}

		public void AddToTimeId(ulong tillTime, long id)
		{
			this.TimeId.Add(tillTime, id);
			if (tillTime < this.minTime)
			{
				this.minTime = tillTime;
			}
		}
	}
}
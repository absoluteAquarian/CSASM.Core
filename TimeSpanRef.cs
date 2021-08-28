using System;

namespace CSASM.Core{
	public class TimeSpanRef{
		public TimeSpan Time{ get; private set; }

		public TimeSpanRef() => Time = TimeSpan.Zero;

		public TimeSpanRef(long ticks) => Time = new TimeSpan(ticks);

		public TimeSpanRef(int hours, int minutes, int seconds) => Time = new TimeSpan(hours, minutes, seconds);

		public TimeSpanRef(int days, int hours, int minutes, int seconds) => Time = new TimeSpan(days, hours, minutes, seconds);

		public TimeSpanRef(int days, int hours, int minutes, int seconds, int milliseconds) => Time = new TimeSpan(days, hours, minutes, seconds, milliseconds);

		public TimeSpanRef(TimeSpan time) => Time = time;

		public void AddMilliseconds(double milliseconds)
			=> Time = Time.Add(TimeSpan.FromMilliseconds(milliseconds));

		public void AddSeconds(double seconds)
			=> Time = Time.Add(TimeSpan.FromSeconds(seconds));

		public void AddMinutes(double minutes)
			=> Time = Time.Add(TimeSpan.FromMinutes(minutes));

		public void AddHours(double hours)
			=> Time = Time.Add(TimeSpan.FromHours(hours));

		public void AddDays(double days)
			=> Time = Time.Add(TimeSpan.FromDays(days));

		public void AddTicks(long ticks)
			=> Time = Time.Add(TimeSpan.FromTicks(ticks));

		public override string ToString() => $"tm: {Time.Ticks} ticks";
	}
}

using System;

namespace CSASM.Core{
	public class DateTimeRef{
		public DateTime Date{ get; private set; }

		/// <summary>
		/// Initializes the date field to <seealso cref="DateTime.Now"/>
		/// </summary>
		public DateTimeRef() => Date = DateTime.Now;

		public DateTimeRef(int year, int month, int day) => Date = new DateTime(year, month, day);

		public DateTimeRef(int year, int month, int day, int hour, int minute, int second) => Date = new DateTime(year, month, day, hour, minute, second);

		public DateTimeRef(int year, int month, int day, int hour, int minute, int second, int millisecond) => Date = new DateTime(year, month, day, hour, minute, second, millisecond);

		public DateTimeRef(DateTime date) => Date = date;

		public DateTimeRef(long ticks) => Date = DateTime.UnixEpoch.AddTicks(ticks);

		public void AddDays(double days) => Date = Date.AddDays(days);

		public void AddHours(double hours) => Date = Date.AddHours(hours);

		public void AddMinutes(double minutes) => Date = Date.AddMinutes(minutes);

		public void AddMilliseconds(double milliseconds) => Date = Date.AddMilliseconds(milliseconds);

		public void AddMonths(int months) => Date = Date.AddMonths(months);

		public void AddTicks(long ticks) => Date = Date.AddTicks(ticks);

		public void AddSeconds(double seconds) => Date = Date.AddSeconds(seconds);

		public void AddYears(int years) => Date = Date.AddYears(years);

		public override string ToString() => $"{Date.Ticks} ticks";
	}
}

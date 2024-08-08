using Android.Util;
using System.Globalization;
using Java.Util;
using Calendar = Java.Util.Calendar;
using Java.Lang;

namespace MauiCatAlarm.Services
{
    public static class KvMannger
    {
        public static EventHandler<byte>? ShiftAlarmChanged;

        public static DateTime GetLatestAlarmDateTime()
        {
            return ConvertFromMillis(GetLatestAlarmTimeMs());
        }

        public static long GetLatestAlarmTimeMs()
        {
            long ret = -1;
            long last = -1;
            for (int i = 0; i < 7; i++)
            {
                long t = GetScheduledTimeShifti((byte)i);
                if (t > 0)
                {
                    ret = t;
                    if ((last > 0) && (ret > last))
                    {
                        ret = last;
                    }
                }
                last = ret;
            }
            return ret;
        }

        public static long ConvertToMillis(TimeSpan time)
        {
            var calendar = Calendar.Instance;
            calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            calendar.Set(CalendarField.HourOfDay, time.Hours);
            calendar.Set(CalendarField.Minute, time.Minutes);
            calendar.Set(CalendarField.Second, time.Seconds);
            calendar.Set(CalendarField.Millisecond, 0);

            if (calendar.TimeInMillis < Java.Lang.JavaSystem.CurrentTimeMillis())
                calendar.Add(CalendarField.DayOfYear, 1);

            return calendar.TimeInMillis;
        }

        public static DateTime ConvertFromMillis(long millis)
        {
            var calendar = Calendar.Instance;
            calendar.TimeInMillis = millis;
            return new DateTime(
                calendar.Get(CalendarField.Year),
                calendar.Get(CalendarField.Month) + 1,
                calendar.Get(CalendarField.DayOfMonth),
                calendar.Get(CalendarField.HourOfDay),
                calendar.Get(CalendarField.Minute),
                calendar.Get(CalendarField.Second));
        }

        public static long ConvertToMillisWeek(TimeSpan time, byte dow)
        {
            var calendar = Calendar.Instance;
            calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
            calendar.Set(CalendarField.DayOfWeek, dow + 1);
            calendar.Set(CalendarField.HourOfDay, time.Hours);
            calendar.Set(CalendarField.Minute, time.Minutes);
            calendar.Set(CalendarField.Second, time.Seconds);
            calendar.Set(CalendarField.Millisecond, 0);

            if (calendar.TimeInMillis < Java.Lang.JavaSystem.CurrentTimeMillis())
                calendar.Add(CalendarField.WeekOfYear, 1);

            return calendar.TimeInMillis;
        }

        public static long SetAlarmFromShift(byte dow, byte shift)
        {
            if ((dow >= 0) && (dow < 7) && (shift >= 0) && (shift < 3))
            {
                TimeSpan timeSpan = TimeSpan.Zero;
                switch (shift)
                {
                    case 0:
                        timeSpan = GetScheduledTimeDay();
                        break;
                    case 1:
                        timeSpan = GetScheduledTimeMid();
                        break;
                    case 2:
                        timeSpan = GetScheduledTimeNight();
                        break;
                    default:
                        break;
                }

                long startTimeInMillis = ConvertToMillisWeek(timeSpan, dow);
                SetScheduledTimeShifti(dow, startTimeInMillis);
                return startTimeInMillis;
            }

            return 0;
        }

        public static bool IsEnableShiftiTime(byte dow)
        {
            return (GetShifti(dow) >= 0) && (GetShifti(dow) < 3) && (GetScheduledTimeShifti(dow) > 0);
        }

        public static bool IsEnableShifti(byte dow)
        {
            return (GetShifti(dow) >= 0) && (GetShifti(dow) < 3);
        }

        public static long GetScheduledTimeShifti(byte dow)
        {
            return Preferences.Default.Get<long>($"shift{dow}time", -123);
        }

        public static void SetScheduledTimeShifti(byte dow, long startTimeInMillis)
        {
            Preferences.Default.Set($"shift{dow}time", startTimeInMillis);
            if(startTimeInMillis > 0)
            {
                ShiftAlarmChanged?.Invoke(null, dow);
            }
        }

        public static void RemoveScheduledTimeShifti(byte dow)
        {
            Preferences.Default.Remove($"shift{dow}time");
        }

        public static byte GetShifti(byte dow)
        {
            int ret = Preferences.Default.Get<int>($"shift{dow}", -1);
            if(ret == -1)
            {
                SetShifti(dow, 3);
                Log.Info("KvMannger ", $"set shift{dow} to default 3");
                ret = 3;
            }
            return (byte)ret;
        }

        public static void SetShifti(byte dow, byte shift)
        {
            Preferences.Default.Set<int>($"shift{dow}", (int)shift);
        }

        public static TimeSpan GetScheduledTimeDay()
        {
            var storedValue = Preferences.Default.Get<string?>("start_time_day", null);
            Log.Info("GetScheduledTimeDay ", storedValue ?? " is null");
            if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
                return timeSpan;

            return new TimeSpan(7, 14, 0);
        }

        public static void SetScheduledTimeDay(TimeSpan startTime)
        {
            Log.Warn("SetScheduledTimeDay ", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
            Preferences.Default.Set("start_time_day", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
        }

        public static TimeSpan GetScheduledTimeMid()
        {
            var storedValue = Preferences.Default.Get<string?>("start_time_mid", null);
            Log.Info("GetScheduledTimeMid ", storedValue ?? " is null");
            if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
                return timeSpan;

            return new TimeSpan(15, 20, 0);
        }

        public static void SetScheduledTimeMid(TimeSpan startTime)
        {
            Log.Warn("SetScheduledTimeMid ", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
            Preferences.Default.Set("start_time_mid", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
        }

        public static TimeSpan GetScheduledTimeNight()
        {
            var storedValue = Preferences.Default.Get<string?>("start_time_night", null);
            Log.Info("GetScheduledTimeNight ", storedValue ?? " is null");
            if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
                return timeSpan;

            return new TimeSpan(23, 45, 0);
        }

        public static void SetScheduledTimeNight(TimeSpan startTime)
        {
            Log.Warn("SetScheduledTimeNight ", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
            Preferences.Default.Set("start_time_night", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
        }

        public static void SetAlarmRingtone(string name, string filePath)
        {
            Preferences.Default.Set("alarm_ringtone", filePath);
            Preferences.Default.Set("alarm_ringtone_name", name);
        }

        public static void SetDefaultAlarmRingtone()
        {
            Preferences.Default.Remove("alarm_ringtone");
            Preferences.Default.Remove("alarm_ringtone_name");
        }

        public static string GetAlarmRingtoneName()
        {
            return Preferences.Default.Get("alarm_ringtone_name", "Default");
        }

        public static string? GetAlarmRingtone()
        {
            return Preferences.Default.Get<string?>("alarm_ringtone", null);
        }

    }
}

using System.Diagnostics;
using System.Globalization;

using Android.App;
using Android.Content;
//using Android.Media;
using Android.Runtime;
using Android.Util;

using Java.Util;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Platforms.Android.Receivers;

using Calendar = Java.Util.Calendar;

namespace MauiCatAlarm.Services;

public partial class AlarmService
{
    private readonly AlarmManager _alarmManager;

    public AlarmService()
    {
        _alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>()
            ?? throw new Exception("Failed to get AlarmManager");
    }

    public partial bool IsSet()
    {
        var pendingIntent = GetPendingAlarmIntent();
        return pendingIntent != null;
    }

    public partial bool IsEnabled()
    {
        return Preferences.Default.Get("is_enabled", false)
            && GetScheduledTime() != null;
    }

    private bool IsSetShifti(int dow)
    {
        var pendingIntent = GetPendingAlarmIntent(false, dow);
        return pendingIntent != null;
    }

    private bool IsEnableShifti(int dow)
    {
        return (GetShifti(dow) >= 0) && (GetShifti(dow) < 3) && (GetScheduledTimeShifti(dow) > 0);
    }

    public partial async Task EnsureAlarmIsSetIfEnabled()
    {
        var status = await Permissions.CheckStatusAsync<ScheduleExactAlarmPermission>();
        if (status != PermissionStatus.Granted)
        {
            Log.Info("AlarmService", $"CheckStatusAsync status is {status}");
            status = await Permissions.RequestAsync<ScheduleExactAlarmPermission>();
            if (status != PermissionStatus.Granted)
            {
                Log.Info("AlarmService", $"RequestAsync status is {status}");
                if (App.Current.MainPage != null)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "需要权限",
                        "一个没有权限设置闹钟的闹钟应用可太悲哀了.",
                        "好的");
                }
                return;
            }
        }

        if (IsEnabled() && !IsSet())
        {
            var scheduledTime = GetScheduledTime()
                ?? throw new UnreachableException("IsEnabled guarantees a scheduled time is set, but GetScheduledTime returned null.");

            Log.Info("AlarmService", $"Alarm is enabled and schedule for {scheduledTime} but no PendingIntent was found");
            SetAlarm(scheduledTime);
        }
        else
        {
            if (!IsEnabled())
            {
                Log.Info("AlarmService", "Alarm is disabled");
            }
            else if (IsSet())
            {
                Log.Info("AlarmService", "Alarm is already set");
            }
        }

        for (int i = 0; i < 7; i++)
        {
            if (IsEnableShifti(i))
            {
                if (IsSetShifti(i))
                {
                    Log.Info("AlarmService", $"shift{i} is already set");
                }
                else
                {
                    SetAlarmShift(GetShifti(i), i);
                }
            }
            else
            {
                Log.Info("AlarmService", $"shift{i} is disabled");
            }
        }
    }

    public partial TimeSpan? GetScheduledTime()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time", null);
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return null;
    }

    public partial long GetScheduledTimeShifti(int dow)
    {
        return Preferences.Default.Get<long>($"shift{dow}time", 0);
    }

    public partial void DeleteAlarm()
    {
        var pendingIntent = GetPendingAlarmIntent();
        if (pendingIntent == null)
            return;

        _alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
        Preferences.Default.Remove("is_enabled");
        OnIsEnabledChanged(this, EventArgs.Empty);
        Log.Info("AlarmService", "Alarm cancelled");
    }

    public partial void SetAlarm(TimeSpan startTime)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31) && !_alarmManager.CanScheduleExactAlarms())
        {
            throw new InvalidOperationException("Unable to schedule exact alarms");
        }

        var pendingIntent = GetPendingAlarmIntent(create: true)!;

        var startTimeInMillis = ConvertToMillis(startTime);
        _alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, startTimeInMillis, pendingIntent);
        Preferences.Default.Set("start_time", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
        Preferences.Default.Set("is_enabled", true);
        OnIsEnabledChanged(this, EventArgs.Empty);
        OnScheduledTimeChanged(this, EventArgs.Empty);
        Log.Info("AlarmService", $"Alarm set for {ConvertFromMillis(startTimeInMillis)}");
    }

    public partial void DismissAlarm()
    {
        var intent = new Intent(Platform.AppContext, typeof(ActiveAlarmService));
        Platform.AppContext.StopService(intent);
    }

    private static PendingIntent? GetPendingAlarmIntent(bool create = false, int request = 0)
    {
        var flags = PendingIntentFlags.OneShot | PendingIntentFlags.Immutable;
        if (!create)
            flags |= PendingIntentFlags.NoCreate;

        var intent = new Intent(Platform.AppContext, typeof(AlarmReceiver));
        if (create)
            intent.SetFlags(ActivityFlags.ReceiverForeground);

        return PendingIntent.GetBroadcast(Platform.AppContext, request, intent, flags);
    }

    private static long ConvertToMillis(TimeSpan time)
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

    private static long ConvertToMillisWeek(TimeSpan time, int dow)
    {
        var calendar = Calendar.Instance;
        calendar.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
        calendar.Set(CalendarField.DayOfWeek, dow+1);
        calendar.Set(CalendarField.HourOfDay, time.Hours);
        calendar.Set(CalendarField.Minute, time.Minutes);
        calendar.Set(CalendarField.Second, time.Seconds);
        calendar.Set(CalendarField.Millisecond, 0);

        if (calendar.TimeInMillis < Java.Lang.JavaSystem.CurrentTimeMillis())
            calendar.Add(CalendarField.WeekOfYear, 1);

        return calendar.TimeInMillis;
    }

    private static DateTime ConvertFromMillis(long millis)
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

    private static TimeSpan GetScheduledTimeDay()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_day", null);
        Log.Info("GetScheduledTimeDay ", storedValue ?? "");
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return new TimeSpan(7, 14, 0);
    }

    private static TimeSpan GetScheduledTimeMid()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_mid", null);
        Log.Info("GetScheduledTimeMid ", storedValue ?? "");
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return new TimeSpan(15, 20, 0);
    }

    private static TimeSpan GetScheduledTimeNight()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_night", null);
        Log.Info("GetScheduledTimeNight ", storedValue ?? "");
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return new TimeSpan(23, 45, 0);
    }

    public partial int GetShifti(int dow)
    {
        return Preferences.Default.Get<int>($"shift{dow}", 3);
    }

    private void SetAlarmShift(int shift, int dow)
    {
        if (shift < 0)
        {
            return;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(31) && !_alarmManager.CanScheduleExactAlarms())
        {
            throw new InvalidOperationException("Unable to schedule exact alarms");
        }

        var pendingIntent = GetPendingAlarmIntent(create: true, request: dow)!;

        TimeSpan timeSpan;
        if (shift == 0)
        {
            timeSpan = GetScheduledTimeDay();
        }
        else if (shift == 1)
        {
            timeSpan = GetScheduledTimeMid();
        }
        else
        {
            timeSpan = GetScheduledTimeNight();
        }

        long startTimeInMillis = ConvertToMillisWeek(timeSpan, dow);
        _alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, startTimeInMillis, pendingIntent);
        Preferences.Default.Set(string.Format("shift{0}time", dow), startTimeInMillis);
        Log.Info("AlarmService", $"shift{dow} set for {ConvertFromMillis(startTimeInMillis)}");

        IsEnabledChangedWeek?.Invoke(this, (byte)dow);
    }

    public void DeleteAlarmShift(int dow)
    {
        var pendingIntent = GetPendingAlarmIntent(false, dow);
        if (pendingIntent == null)
            return;

        _alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
        Preferences.Default.Remove(string.Format("shift{0}time", dow));
        IsEnabledChangedWeek?.Invoke(this, (byte)dow);
        Log.Info("AlarmService", $"shift{dow} cancelled");
    }

    public partial void SetShifti(int idx, int dow)
    {
        if(idx >= 3)
        {
            Preferences.Default.Set($"shift{dow}", 3);
            DeleteAlarmShift(dow);
        }
        else
        {
            Preferences.Default.Set($"shift{dow}", idx);
            SetAlarmShift(idx, dow);
        }
        ShiftChanged?.Invoke(this, (byte)dow);
    }
}

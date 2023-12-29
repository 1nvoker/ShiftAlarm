﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.OS;
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

    public partial void EnsureAlarmIsSetIfEnabled()
    {
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
    }

    public partial TimeSpan? GetScheduledTime()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time", null);
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return null;
    }

    public partial void DeleteAlarm()
    {
        var pendingIntent = GetPendingAlarmIntent();
        if (pendingIntent == null)
            return;

        _alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
        Preferences.Default.Remove("is_enabled");
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
        Log.Info("AlarmService", $"Alarm set for {ConvertFromMillis(startTimeInMillis)}");
    }

    public partial void DismissAlarm()
    {
        var intent = new Intent(Platform.AppContext, typeof(ActiveAlarmService));
        Platform.AppContext.StopService(intent);
    }

    private static PendingIntent? GetPendingAlarmIntent(bool create = false)
    {
        var flags = PendingIntentFlags.OneShot | PendingIntentFlags.Immutable;
        if (!create)
            flags |= PendingIntentFlags.NoCreate;

        var intent = new Intent(Platform.AppContext, typeof(AlarmReceiver));
        if (create)
            intent.SetFlags(ActivityFlags.ReceiverForeground);

        return PendingIntent.GetBroadcast(Platform.AppContext, 0, intent, flags);
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
}

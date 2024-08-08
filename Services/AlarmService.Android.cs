using System.Diagnostics;
using System.Globalization;

using Android.App;
using Android.Content;
//using Android.Media;
using Android.Runtime;
using Android.Util;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Platforms.Android.Receivers;

namespace MauiCatAlarm.Services;

public partial class AlarmService
{
    private readonly AlarmManager _alarmManager;

    public AlarmService()
    {
        _alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>()
            ?? throw new Exception("Failed to get AlarmManager");

        KvMannger.ShiftAlarmChanged += new EventHandler<byte>(OnShiftAlarmChanged);
    }

    //public partial bool IsSet()
    //{
    //    var pendingIntent = GetPendingAlarmIntent();
    //    return pendingIntent != null;
    //}

    //public partial bool IsEnabled()
    //{
    //    return Preferences.Default.Get("is_enabled", false)
    //        && GetScheduledTime() != null;
    //}

    public partial bool IsNoShiftAlarmSet()
    {
        return !IsSetShifti(0) && !IsSetShifti(1) && !IsSetShifti(2)
            && !IsSetShifti(3) && !IsSetShifti(4) && !IsSetShifti(5)
            && !IsSetShifti(6);
    }

    public partial string GetNextAlarmTime()
    {
        return KvMannger.GetLatestAlarmDateTime().ToString("f");
    }

    private bool IsSetShifti(byte dow)
    {
        var pendingIntent = GetPendingAlarmIntent(false, dow);
        return pendingIntent != null;
    }

    public partial void EnsureAlarmIsSetIfEnabled()
    {
        for (byte i = 0; i < 7; i++)
        {
            if (KvMannger.IsEnableShiftiTime(i))
            {
                if (IsSetShifti(i))
                {
                    Log.Info("AlarmService", $"shift{i} is already set");
                }
                else
                {
                    Log.Info("AlarmService", $"SetAlarmShift");
                    SetAlarmShift((byte)GetShifti(i), i);
                }
            }
            else
            {
                Log.Info("AlarmService", $"shift{i} is disabled");
            }
        }
    }

    //public partial TimeSpan? GetScheduledTime()
    //{
    //    var storedValue = Preferences.Default.Get<string?>("start_time", null);
    //    if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
    //        return timeSpan;

    //    return null;
    //}

    //public partial void DeleteAlarm()
    //{
    //    var pendingIntent = GetPendingAlarmIntent();
    //    if (pendingIntent == null)
    //        return;

    //    _alarmManager.Cancel(pendingIntent);
    //    pendingIntent.Cancel();
    //    Preferences.Default.Remove("is_enabled");
    //    OnIsEnabledChanged(this, EventArgs.Empty);
    //    Log.Info("AlarmService", "Alarm cancelled");
    //}

    //public partial void SetAlarm(TimeSpan startTime)
    //{
    //    if (OperatingSystem.IsAndroidVersionAtLeast(31) && !_alarmManager.CanScheduleExactAlarms())
    //    {
    //        throw new InvalidOperationException("Unable to schedule exact alarms");
    //    }

    //    var pendingIntent = GetPendingAlarmIntent(create: true)!;

    //    var startTimeInMillis = KvMannger.ConvertToMillis(startTime);
    //    _alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, startTimeInMillis, pendingIntent);
    //    Preferences.Default.Set("start_time", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
    //    Preferences.Default.Set("is_enabled", true);
    //    OnIsEnabledChanged(this, EventArgs.Empty);
    //    OnScheduledTimeChanged(this, EventArgs.Empty);
    //    Log.Info("AlarmService", $"Alarm set for {KvMannger.ConvertFromMillis(startTimeInMillis)}");
    //}

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

    public partial byte GetShifti(byte dow)
    {
        return KvMannger.GetShifti(dow);
    }

    private void SetAlarmShift(byte shift, byte dow)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31) && !_alarmManager.CanScheduleExactAlarms())
        {
            Log.Info("AlarmService", $"SetAlarmShift Unable to schedule exact alarms");
            throw new InvalidOperationException("Unable to schedule exact alarms");
        }

        var pendingIntent = GetPendingAlarmIntent(create: true, request: dow)!;

        long startTimeInMillis = KvMannger.SetAlarmFromShift(dow, shift, false);
        _alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, startTimeInMillis, pendingIntent);
        Log.Info("AlarmService", $"shift{dow} set for {KvMannger.ConvertFromMillis(startTimeInMillis)}");

        IsEnabledChangedWeek?.Invoke(this, dow);
    }

    public void DeleteAlarmShift(byte dow)
    {
        var pendingIntent = GetPendingAlarmIntent(false, dow);
        if (pendingIntent == null)
            return;

        _alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
        KvMannger.RemoveScheduledTimeShifti(dow);
        IsEnabledChangedWeek?.Invoke(this, dow);
        Log.Info("AlarmService", $"shift{dow} cancelled");
    }

    public partial void SetShifti(byte shift, byte dow)
    {
        if (shift >= 3)
        {
            KvMannger.SetShifti(dow, 3);
            DeleteAlarmShift(dow);
        }
        else
        {
            KvMannger.SetShifti(dow, shift);
            SetAlarmShift(shift, dow);
        }
        ShiftChanged?.Invoke(this, dow);
    }

    private void OnShiftAlarmChanged(object? sender, byte dow)
    {
        ShiftChanged?.Invoke(this, dow);
        Log.Info("AlarmService", $"OnShiftAlarmChanged{dow} cancelled");

        for (byte i = 0; i < 7; i++)
        {
            if (KvMannger.IsEnableShiftiTime(i))
            {
                Log.Info("AlarmService", $"OnShiftAlarmChanged shift{i} refresh");
                SetAlarmShift((byte)GetShifti(i), i);
            }
            else
            {
                Log.Info("AlarmService", $"OnShiftAlarmChanged shift{i} is disabled");
            }
        }
    }

    //public async Task RequestandCheckPermission31()
    //{
    //    var status = await Permissions.CheckStatusAsync<Permission31>();
    //    if (status != PermissionStatus.Granted)
    //    {
    //        //Debug.WriteLine($"Permission31 CheckStatusAsync is {status}");
    //        status = await Permissions.RequestAsync<Permission31>();
    //        if (status != PermissionStatus.Granted)
    //        {
    //            //Debug.WriteLine($"Permission31 RequestAsync is {status}");
    //            if (App.Current.MainPage != null)
    //            {
    //                await App.Current.MainPage.DisplayAlert(
    //                    "需要权限",
    //                    "一个没有权限显示闹钟的闹钟应用可太悲哀了.",
    //                    "好的");
    //            }
    //        }
    //        else
    //        {
    //            //Debug.WriteLine($"Permission31 RequestAsync is {status}");
    //        }
    //    }
    //    else
    //    {
    //        //Debug.WriteLine($"Permission31 CheckStatusAsync is {status}");
    //    }
    //}

    //public async Task RequestandCheckPermission33()
    //{
    //    var status = await Permissions.CheckStatusAsync<Permission33>();
    //    if (status != PermissionStatus.Granted)
    //    {
    //        //Debug.WriteLine($"Permission33 CheckStatusAsync is {status}");
    //        status = await Permissions.RequestAsync<Permission33>();
    //        if (status != PermissionStatus.Granted)
    //        {
    //            //Debug.WriteLine($"Permission33 RequestAsync is {status}");
    //            if (App.Current.MainPage != null)
    //            {
    //                await App.Current.MainPage.DisplayAlert(
    //                    "需要权限",
    //                    "一个没有权限显示闹钟的闹钟应用可太悲哀了.",
    //                    "好的");
    //            }
    //        }
    //        else
    //        {
    //            //Debug.WriteLine($"Permission33 RequestAsync is {status}");
    //        }
    //    }
    //    else
    //    {
    //        //Debug.WriteLine($"Permission33 CheckStatusAsync is {status}");
    //    }
    //}
}

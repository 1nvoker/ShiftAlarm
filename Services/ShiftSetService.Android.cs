//using System.Diagnostics;
//using System.Globalization;

//using Android.App;
using Android.Content;
using Android.Media;
//using Android.Runtime;
using Android.Util;
//using static Android.InputMethodServices.Keyboard;

//using Java.Util;

//using MauiCatAlarm.Platforms.Android;
//using MauiCatAlarm.Platforms.Android.Receivers;

namespace MauiCatAlarm.Services;

public partial class ShiftSetService
{
    public ShiftSetService()
    {

    }

    public partial TimeSpan GetScheduledTimeDay()
    {
        return KvMannger.GetScheduledTimeDay();
    }

    public partial void SetScheduledTimeDay(TimeSpan startTime)
    {
        KvMannger.SetScheduledTimeDay(startTime);

        for (byte i = 0; i < 7; i++)
        {
            if (KvMannger.GetShifti(i) == 0)
            {
                long st = KvMannger.SetAlarmFromShift(i, 0, true);
                Log.Info("ShiftSetService", $"day shift{i} update for {KvMannger.ConvertFromMillis(st)}");
            }
        }
    }

    public partial TimeSpan GetScheduledTimeMid()
    {
        return KvMannger.GetScheduledTimeMid();
    }

    public partial void SetScheduledTimeMid(TimeSpan startTime)
    {
        KvMannger.SetScheduledTimeMid(startTime);

        for (byte i = 0; i < 7; i++)
        {
            if (KvMannger.GetShifti(i) == 1)
            {
                long st = KvMannger.SetAlarmFromShift(i, 1, true);
                Log.Info("ShiftSetService", $"mid shift{i} update for {KvMannger.ConvertFromMillis(st)}");
            }
        }
    }

    public partial TimeSpan GetScheduledTimeNight()
    {
        return KvMannger.GetScheduledTimeNight();
    }

    public partial void SetScheduledTimeNight(TimeSpan startTime)
    {
        KvMannger.SetScheduledTimeNight(startTime);

        for (byte i = 0; i < 7; i++)
        {
            if (KvMannger.GetShifti(i) == 2)
            {
                long st = KvMannger.SetAlarmFromShift(i, 2, true);
                Log.Info("ShiftSetService", $"night shift{i} update for {KvMannger.ConvertFromMillis(st)}");
            }
        }
    }

    public partial void PickAlarmRingtone()
    {
        if (Platform.CurrentActivity is null) throw new InvalidOperationException("Can't pick a ringtone without a current activity.");

        var intent = new Intent(RingtoneManager.ActionRingtonePicker);
        intent.PutExtra(RingtoneManager.ExtraRingtoneTitle, "Select alarm ringtone");
        intent.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Alarm);
        intent.PutExtra(RingtoneManager.ExtraRingtoneShowSilent, false);
        intent.PutExtra(RingtoneManager.ExtraRingtoneShowDefault, true);

        try
        {
            var currentRingtoneUri = GetAlarmRingtone();
            if (!string.IsNullOrEmpty(currentRingtoneUri))
            {
                intent.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, Android.Net.Uri.Parse(currentRingtoneUri));
            }
        }
        catch (Exception ex)
        {
            Log.Warn("ShiftSetService", Java.Lang.Throwable.FromException(ex));
        }
        Platform.CurrentActivity.StartActivityForResult(intent, RingtonePickerRequestCode);
    }

    public partial void SetAlarmRingtone(string name, string filePath)
    {
        KvMannger.SetAlarmRingtone(name, filePath);
        OnRingtoneChanged(this, EventArgs.Empty);
    }

    public partial void SetDefaultAlarmRingtone()
    {
        KvMannger.SetDefaultAlarmRingtone();
        OnRingtoneChanged(this, EventArgs.Empty);
    }

    public partial string GetAlarmRingtoneName()
    {
        return KvMannger.GetAlarmRingtoneName();
    }

    public partial string? GetAlarmRingtone()
    {
        return KvMannger.GetAlarmRingtone();
    }
}

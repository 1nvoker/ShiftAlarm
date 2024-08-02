using System.Diagnostics;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Util;

using Java.Util;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Platforms.Android.Receivers;

namespace MauiCatAlarm.Services;

public partial class ShiftSetService
{
    public ShiftSetService()
    {

    }

    public partial TimeSpan? GetScheduledTimeDay()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_day", null);
        Log.Warn("GetScheduledTimeDay ", storedValue??"");
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return null;
    }
    public partial void SetScheduledTimeDay(TimeSpan startTime)
    {
        Log.Warn("GetScheduledTimeDay ", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
        Preferences.Default.Set("start_time_day", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
    }

    public partial TimeSpan? GetScheduledTimeMid()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_mid", null);
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return null;
    }
    public partial void SetScheduledTimeMid(TimeSpan startTime)
    {
        Preferences.Default.Set("start_time_mid", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
    }

    public partial TimeSpan? GetScheduledTimeNight()
    {
        var storedValue = Preferences.Default.Get<string?>("start_time_night", null);
        if (TimeSpan.TryParseExact(storedValue, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan))
            return timeSpan;

        return null;
    }
    public partial void SetScheduledTimeNight(TimeSpan startTime)
    {
        Preferences.Default.Set("start_time_night", startTime.ToString("hh\\:mm", CultureInfo.InvariantCulture));
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
        Preferences.Default.Set("alarm_ringtone", filePath);
        Preferences.Default.Set("alarm_ringtone_name", name);
        OnRingtoneChanged(this, EventArgs.Empty);
    }

    public partial void SetDefaultAlarmRingtone()
    {
        Preferences.Default.Remove("alarm_ringtone");
        Preferences.Default.Remove("alarm_ringtone_name");
        OnRingtoneChanged(this, EventArgs.Empty);
    }

    public partial string GetAlarmRingtoneName()
    {
        return Preferences.Default.Get("alarm_ringtone_name", "Default");
    }

    public partial string? GetAlarmRingtone()
    {
        return Preferences.Default.Get<string?>("alarm_ringtone", null);
    }
}

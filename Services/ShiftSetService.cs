namespace MauiCatAlarm.Services;

public partial class ShiftSetService
{
    public const int RingtonePickerRequestCode = 8008;

    public event EventHandler? ScheduledTimeChanged;

    public event EventHandler? RingtoneChanged;

    public partial TimeSpan? GetScheduledTimeDay();

    public partial void SetScheduledTimeDay(TimeSpan startTime);

    public partial TimeSpan? GetScheduledTimeMid();

    public partial void SetScheduledTimeMid(TimeSpan startTime);

    public partial TimeSpan? GetScheduledTimeNight();

    public partial void SetScheduledTimeNight(TimeSpan startTime);

    public partial void SetAlarmRingtone(string name, string filePath);

    public partial void PickAlarmRingtone();

    public partial void SetDefaultAlarmRingtone();
    
    public partial string GetAlarmRingtoneName();

    public partial string? GetAlarmRingtone();

    protected virtual void OnScheduledTimeChanged(object sender, EventArgs e)
    {
        ScheduledTimeChanged?.Invoke(sender, e);
    }

    protected virtual void OnRingtoneChanged(object sender, EventArgs e)
    {
        RingtoneChanged?.Invoke(sender, e);
    }
}

namespace MauiCatAlarm.Services;

public partial class AlarmService
{
    public const int RingtonePickerRequestCode = 8008;

    //public event EventHandler? ScheduledTimeChanged;

    //public event EventHandler? IsEnabledChanged;

    public event EventHandler<byte>? ShiftChanged;

    public event EventHandler<byte>? IsEnabledChangedWeek;

    //public partial void SetAlarm(TimeSpan startTime);

    //public partial bool IsSet();

    //public partial bool IsEnabled();
    public partial bool IsNoShiftAlarmSet();
    public partial string GetNextAlarmTime();

    //public partial void DeleteAlarm();

    public partial void DismissAlarm();

    public partial void EnsureAlarmIsSetIfEnabled();

    //public partial TimeSpan? GetScheduledTime();

    protected virtual void OnScheduledTimeChanged(object sender, EventArgs e)
    {
        //ScheduledTimeChanged?.Invoke(sender, e);
    }

    protected virtual void OnIsEnabledChanged(object sender, EventArgs e)
    {
        //IsEnabledChanged?.Invoke(sender, e);
    }

    public partial byte GetShifti(byte dow);

    public partial void SetShifti(byte shift, byte dow);
}

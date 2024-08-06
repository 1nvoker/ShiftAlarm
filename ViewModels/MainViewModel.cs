using System.ComponentModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;

namespace MauiCatAlarm.ViewModels;

public class MainViewModel : ObservableObject, IDisposable
{
    private readonly AlarmService _alarmService;
    private readonly Func<AlarmPage> _alarmPageFactory;
    private TimeSpan _alarmTime;

    public MainViewModel(AlarmService alarmService, Func<AlarmPage> alarmPageFactory)
    {
        _alarmService = alarmService;
        _alarmPageFactory = alarmPageFactory;

        _alarmService.IsEnabledChanged += AlarmService_IsEnabledChanged;
        _alarmService.ScheduledTimeChanged += AlarmService_ScheduledTimeChanged;
        _alarmService.Shift1Changed += AlarmService_Shift1Changed;
        _alarmService.IsEnabledChangedWeek += AlarmService_IsEnableChangedWeek;

        App.Current.PropertyChanged += App_PropertyChanged;

        AlarmTime = _alarmService.GetScheduledTime() ?? new TimeSpan(9, 0, 0);
        _idx1 = _alarmService.GetShifti(3);
        OnPropertyChanged(nameof(SelectShift1));
        BtnBackgroundColor1 = GetColorFromShift(_idx1);
        OnPropertyChanged(nameof(BtnBackgroundColor1));

        ToggleAlarmCommand = new AsyncRelayCommand(ToggleAlarmAsync);
        NavigateToAlarmCommand = new RelayCommand(NavigateToAlarm);
        ToggleShift1Command = new RelayCommand(ToggleShift1);
        //UpdateAlarmRingtoneCommand = new AsyncRelayCommand(UpdateAlarmRingtoneAsync);

        App.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(CurrentWeekday));
            OnPropertyChanged(nameof(CurrentMonth));
            OnPropertyChanged(nameof(CurrentDayNumber));
            OnPropertyChanged(nameof(EnabledAlarmLabel));
            return true;
        });
    }

    public string CurrentTime => DateTime.Now.ToString("T");

    public string CurrentWeekday => DateTime.Now.ToString("dddd");

    public string CurrentMonth => DateTime.Now.ToString("MMM");

    public string CurrentDayNumber => DateTime.Now.ToString("dd");

    public string ToggleAlarmText => IsAlarmSet ? "禁用闹钟" : "启用闹钟";

    public string EnabledAlarmLabel => FormatAlarmText();

    public bool IsAlarmSet => _alarmService.IsSet();

    public bool IsAlarmUnset => !IsAlarmSet;

    public bool IsAlarmOngoing { get; private set; }

    public ICommand ToggleAlarmCommand { get; }

    public ICommand NavigateToAlarmCommand { get; }

    //public ICommand UpdateAlarmRingtoneCommand { get; }
    public ICommand ToggleShift1Command { get; }

    public TimeSpan AlarmTime
    {
        get => _alarmTime;
        set => SetProperty(ref _alarmTime, value);
    }

    public List<string> Shifts => new() { "白班", "中班", "夜班", "休息" };
    public string[] ShiftArray => Shifts.ToArray();

    private int _idx1 = 3;
    public string SelectShift1 => ShiftArray[_idx1];
    public Color BtnBackgroundColor1 { get; set; } = Color.FromArgb("#FF92A1B0");

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _alarmService.IsEnabledChanged -= AlarmService_IsEnabledChanged;
            _alarmService.ScheduledTimeChanged -= AlarmService_ScheduledTimeChanged;
            _alarmService.Shift1Changed -= AlarmService_Shift1Changed;
            _alarmService.IsEnabledChangedWeek -= AlarmService_IsEnableChangedWeek;

            App.Current.PropertyChanged -= App_PropertyChanged;
        }
    }

    protected virtual async Task ToggleAlarmAsync()
    {
        var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<PostNotificationsPermission>();
            if (status != PermissionStatus.Granted)
            {
                if (App.Current.MainPage != null)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "需要权限",
                        "一个没有权限显示闹钟的闹钟应用可太悲哀了.",
                        "好的");
                }
                return;
            }
        }

        if (_alarmService.IsSet())
        {
            _alarmService.DeleteAlarm();
        }
        else
        {
            _alarmService.SetAlarm(AlarmTime);
        }
    }

    protected virtual void NavigateToAlarm()
    {
        var alarmPage = _alarmPageFactory();
        App.Current.MainPage = alarmPage;
    }

    private void AlarmService_ScheduledTimeChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(EnabledAlarmLabel));
    }

    private void AlarmService_IsEnabledChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsAlarmSet));
        OnPropertyChanged(nameof(IsAlarmUnset));
        OnPropertyChanged(nameof(ToggleAlarmText));
    }

    private void App_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.IsAlarmActive))
        {
            IsAlarmOngoing = App.Current.IsAlarmActive;
            OnPropertyChanged(nameof(IsAlarmOngoing));
        }
    }

    private string FormatAlarmText()
    {
        var nextOccurrence = NextAlarm();
        if (nextOccurrence == null)
        {
            return $"Zzzzzzzz…";
        }

        if (nextOccurrence.Value.Date > DateTime.Today)
        {
            return $"将于明天 {nextOccurrence.Value:t} 醒来.";
        }

        return $"将于 {nextOccurrence.Value:t} 醒来.";
    }

    private DateTime? NextAlarm()
    {
        var scheduledTime = _alarmService.GetScheduledTime();
        if (scheduledTime == null)
            return null;

        var nextOccurence = DateTime.Today
            .AddHours(scheduledTime.Value.Hours)
            .AddMinutes(scheduledTime.Value.Minutes);

        if (nextOccurence < DateTime.Now)
            nextOccurence = nextOccurence.AddDays(1);

        return nextOccurence;
    }

    private void ToggleShift1()
    {
        if(_idx1 < (ShiftArray.Length-1))
        {
            _idx1++;
        }
        else
        {
            _idx1 = 0;
        }
        _alarmService.SetShifti(_idx1, 3);
    }

    private void AlarmService_Shift1Changed(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(SelectShift1));
        BtnBackgroundColor1 = GetColorFromShift(_alarmService.GetShifti(3));
        OnPropertyChanged(nameof(BtnBackgroundColor1));
    }

    private Color GetColorFromShift(int shift)
    {
        Color ret;
        switch (shift)
        {
            case 0:
                ret = Color.FromArgb("#FFC1F7F6");
                break;
            case 1:
                ret = Color.FromArgb("#FFF8A313");
                break;
            case 2:
                ret = Color.FromArgb("#FF419EF8");
                break;
            default:
                ret = Color.FromArgb("#FF92A1B0");
                break;
        }
        return ret;
    }

    private void AlarmService_IsEnableChangedWeek(object? sender, byte e)
    {

    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;
using static Android.InputMethodServices.Keyboard;

namespace MauiCatAlarm.ViewModels;

public class MainViewModel : ObservableObject, IDisposable
{
    private readonly AlarmService _alarmService;
    //private readonly Func<AlarmPage> _alarmPageFactory;
    //private TimeSpan _alarmTime;

    public static string[] ShiftArray => ["白班", "中班", "夜班", "休息"];

    private readonly int[] _shift = [3, 3, 3, 3, 3, 3, 3];
    private readonly Color[] _btncolors =
    [
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0"),
        Color.FromArgb("#FF92A1B0")
    ];

    public string SelectShift0 => ShiftArray[_shift[0]];
    public Color BtnBackgroundColor0 => _btncolors[0];

    public string SelectShift1 => ShiftArray[_shift[1]];
    public Color BtnBackgroundColor1 => _btncolors[1];

    public string SelectShift2 => ShiftArray[_shift[2]];
    public Color BtnBackgroundColor2 => _btncolors[2];

    public string SelectShift3 => ShiftArray[_shift[3]];
    public Color BtnBackgroundColor3 => _btncolors[3];

    public string SelectShift4 => ShiftArray[_shift[4]];
    public Color BtnBackgroundColor4 => _btncolors[4];

    public string SelectShift5 => ShiftArray[_shift[5]];
    public Color BtnBackgroundColor5 => _btncolors[5];

    public string SelectShift6 => ShiftArray[_shift[6]];
    public Color BtnBackgroundColor6 => _btncolors[6];

    private string _nextAlarmTime = DateTime.MinValue.ToString("f");
    public string NextAlarmTime
    {
        get => _alarmService.GetNextAlarmTime();
        set => SetProperty(ref _nextAlarmTime, value);
    }

    private bool _isShiftAlarmSet = false;
    public bool IsShiftAlarmSet
    {
        get => !_alarmService.IsNoShiftAlarmSet();
        set => SetProperty(ref _isShiftAlarmSet, value);
    }

    public MainViewModel(AlarmService alarmService)
    {
        _alarmService = alarmService;
        //_alarmPageFactory = alarmPageFactory;

        //_alarmService.IsEnabledChanged += AlarmService_IsEnabledChanged;
        //_alarmService.ScheduledTimeChanged += AlarmService_ScheduledTimeChanged;
        _alarmService.ShiftChanged += AlarmService_ShiftChanged;
        _alarmService.IsEnabledChangedWeek += AlarmService_IsEnableChangedWeek;

        IsAlarmOngoing = App.Current.IsAlarmActive;

        App.Current.PropertyChanged += App_PropertyChanged;

        //AlarmTime = _alarmService.GetScheduledTime() ?? new TimeSpan(9, 0, 0);

        for (int i = 0; i < _shift.Length; i++)
        {
            _shift[i] = _alarmService.GetShifti((byte)i);
            //Debug.WriteLine($"MainViewModel shift {i} = {_shift[i]}");
            _btncolors[i] = GetColorFromShift(_shift[i]);
        }
        OnPropertyChanged(nameof(SelectShift0));
        OnPropertyChanged(nameof(BtnBackgroundColor0));
        OnPropertyChanged(nameof(SelectShift1));
        OnPropertyChanged(nameof(BtnBackgroundColor1));
        OnPropertyChanged(nameof(SelectShift2));
        OnPropertyChanged(nameof(BtnBackgroundColor2));
        OnPropertyChanged(nameof(SelectShift3));
        OnPropertyChanged(nameof(BtnBackgroundColor3));
        OnPropertyChanged(nameof(SelectShift4));
        OnPropertyChanged(nameof(BtnBackgroundColor4));
        OnPropertyChanged(nameof(SelectShift5));
        OnPropertyChanged(nameof(BtnBackgroundColor5));
        OnPropertyChanged(nameof(SelectShift6));
        OnPropertyChanged(nameof(BtnBackgroundColor6));

        NextAlarmTime = _alarmService.GetNextAlarmTime();
        //Debug.WriteLine($"NextAlarmTime is {NextAlarmTime}");
        OnPropertyChanged(nameof(NextAlarmTime));
        IsShiftAlarmSet = !_alarmService.IsNoShiftAlarmSet();
        OnPropertyChanged(nameof(IsShiftAlarmSet));

        ToggleShift0Command = new RelayCommand(ToggleShift0);
        ToggleShift1Command = new RelayCommand(ToggleShift1);
        ToggleShift2Command = new RelayCommand(ToggleShift2);
        ToggleShift3Command = new RelayCommand(ToggleShift3);
        ToggleShift4Command = new RelayCommand(ToggleShift4);
        ToggleShift5Command = new RelayCommand(ToggleShift5);
        ToggleShift6Command = new RelayCommand(ToggleShift6);

        DismissAlarmCommand = new RelayCommand(DismissAlarm);
        //TestCommand = new RelayCommand(Test);

        //NavigateToAlarmCommand = new RelayCommand(NavigateToAlarm);

        App.Current.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(CurrentWeekday));
            OnPropertyChanged(nameof(CurrentMonth));
            OnPropertyChanged(nameof(CurrentDayNumber));
            return true;
        });
    }

    public string CurrentTime => DateTime.Now.ToString("T");

    public string CurrentWeekday => DateTime.Now.ToString("dddd");

    public string CurrentMonth => DateTime.Now.ToString("MMM");

    public string CurrentDayNumber => DateTime.Now.ToString("dd");

    //public bool IsAlarmSet => _alarmService.IsSet();

    public bool IsAlarmOngoing { get; private set; }

    //public ICommand NavigateToAlarmCommand { get; }

    public ICommand ToggleShift0Command { get; }

    public ICommand ToggleShift1Command { get; }

    public ICommand ToggleShift2Command { get; }

    public ICommand ToggleShift3Command { get; }

    public ICommand ToggleShift4Command { get; }

    public ICommand ToggleShift5Command { get; }

    public ICommand ToggleShift6Command { get; }

    //public ICommand TestCommand { get; }

    public ICommand DismissAlarmCommand { get; }

    //public TimeSpan AlarmTime
    //{
    //    get => _alarmTime;
    //    set => SetProperty(ref _alarmTime, value);
    //}

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            //_alarmService.IsEnabledChanged -= AlarmService_IsEnabledChanged;
            //_alarmService.ScheduledTimeChanged -= AlarmService_ScheduledTimeChanged;
            _alarmService.ShiftChanged -= AlarmService_ShiftChanged;
            _alarmService.IsEnabledChangedWeek -= AlarmService_IsEnableChangedWeek;

            App.Current.PropertyChanged -= App_PropertyChanged;
        }
    }

    //protected virtual async Task ToggleAlarmAsync()
    //{
    //    var status = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
    //    if (status != PermissionStatus.Granted)
    //    {
    //        status = await Permissions.RequestAsync<PostNotificationsPermission>();
    //        if (status != PermissionStatus.Granted)
    //        {
    //            if (App.Current.MainPage != null)
    //            {
    //                await App.Current.MainPage.DisplayAlert(
    //                    "需要权限",
    //                    "一个没有权限显示闹钟的闹钟应用可太悲哀了.",
    //                    "好的");
    //            }
    //            return;
    //        }
    //    }

    //    if (_alarmService.IsSet())
    //    {
    //        _alarmService.DeleteAlarm();
    //    }
    //    else
    //    {
    //        _alarmService.SetAlarm(AlarmTime);
    //    }
    //}

    protected virtual void NavigateToAlarm()
    {
        //var alarmPage = _alarmPageFactory();
        //App.Current.MainPage = alarmPage;
    }

    private void AlarmService_ScheduledTimeChanged(object? sender, EventArgs e)
    {

    }

    private void AlarmService_IsEnabledChanged(object? sender, EventArgs e)
    {
        //OnPropertyChanged(nameof(IsAlarmSet));
    }

    private void App_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.IsAlarmActive))
        {
            IsAlarmOngoing = App.Current.IsAlarmActive;
            OnPropertyChanged(nameof(IsAlarmOngoing));
        }
    }

    //private string FormatAlarmText()
    //{
    //    var nextOccurrence = NextAlarm();
    //    if (nextOccurrence == null)
    //    {
    //        return $"Zzzzzzzz…";
    //    }

    //    if (nextOccurrence.Value.Date > DateTime.Today)
    //    {
    //        return $"将于明天 {nextOccurrence.Value:t} 醒来.";
    //    }

    //    return $"将于 {nextOccurrence.Value:t} 醒来.";
    //}

    //private DateTime? NextAlarm()
    //{
    //    var scheduledTime = _alarmService.GetScheduledTime();
    //    if (scheduledTime == null)
    //        return null;

    //    var nextOccurence = DateTime.Today
    //        .AddHours(scheduledTime.Value.Hours)
    //        .AddMinutes(scheduledTime.Value.Minutes);

    //    if (nextOccurence < DateTime.Now)
    //        nextOccurence = nextOccurence.AddDays(1);

    //    return nextOccurence;
    //}

    private void ToggleShifti(byte dow)
    {
        if ((dow >= 0) && (dow < _shift.Length))
        {
            if (_shift[dow] < (ShiftArray.Length - 1))
            {
                _shift[dow]++;
            }
            else
            {
                _shift[dow] = 0;
            }
            _alarmService.SetShifti((byte)_shift[(int)dow], dow);
        }
        else
        {
            //Debug.WriteLine($"ToggleShifti wrong dow : {dow}, out of range");
        }
    }

    private void ToggleShift0()
    {
        ToggleShifti(0);
    }

    private void ToggleShift1()
    {
        ToggleShifti(1);
    }

    private void ToggleShift2()
    {
        ToggleShifti(2);
    }

    private void ToggleShift3()
    {
        ToggleShifti(3);
    }

    private void ToggleShift4()
    {
        ToggleShifti(4);
    }

    private void ToggleShift5()
    {
        ToggleShifti(5);
    }

    private void ToggleShift6()
    {
        ToggleShifti(6);
    }

    private static Color GetColorFromShift(int shift)
    {
        Color ret = shift switch
        {
            0 => Color.FromArgb("#FFC1F7F6"),
            1 => Color.FromArgb("#FFF8A313"),
            2 => Color.FromArgb("#FF419EF8"),
            _ => Color.FromArgb("#FF92A1B0"),
        };
        return ret;
    }

    private void AlarmService_ShiftChanged(object? sender, byte e)
    {
        switch (e)
        {
            case 0:
                _btncolors[0] = GetColorFromShift(_alarmService.GetShifti(0));
                OnPropertyChanged(nameof(SelectShift0));
                OnPropertyChanged(nameof(BtnBackgroundColor0));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 1:
                _btncolors[1] = GetColorFromShift(_alarmService.GetShifti(1));
                OnPropertyChanged(nameof(SelectShift1));
                OnPropertyChanged(nameof(BtnBackgroundColor1));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 2:
                _btncolors[2] = GetColorFromShift(_alarmService.GetShifti(2));
                OnPropertyChanged(nameof(SelectShift2));
                OnPropertyChanged(nameof(BtnBackgroundColor2));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 3:
                _btncolors[3] = GetColorFromShift(_alarmService.GetShifti(3));
                OnPropertyChanged(nameof(SelectShift3));
                OnPropertyChanged(nameof(BtnBackgroundColor3));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 4:
                _btncolors[4] = GetColorFromShift(_alarmService.GetShifti(4));
                OnPropertyChanged(nameof(SelectShift4));
                OnPropertyChanged(nameof(BtnBackgroundColor4));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 5:
                _btncolors[5] = GetColorFromShift(_alarmService.GetShifti(5));
                OnPropertyChanged(nameof(SelectShift5));
                OnPropertyChanged(nameof(BtnBackgroundColor5));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            case 6:
                _btncolors[6] = GetColorFromShift(_alarmService.GetShifti(6));
                OnPropertyChanged(nameof(SelectShift6));
                OnPropertyChanged(nameof(BtnBackgroundColor6));
                OnPropertyChanged(nameof(NextAlarmTime));
                break;
            default:
                break;
        }
    }

    private void AlarmService_IsEnableChangedWeek(object? sender, byte e)
    {
        OnPropertyChanged(nameof(IsShiftAlarmSet));
        OnPropertyChanged(nameof(NextAlarmTime));
    }

    public string LoveTalk => GetLoveTalk();

    private readonly string[] _lovetalks = [
            "不管我本人多么平庸，我总觉得对你的爱很美。--王小波",
            "最好的爱情，就是可以快乐的做自己，还依然被爱着。--王小波",
            "只要有想见的人，就不是孤身一人。--《夏目友人帐》",
            "约着见一面，就能使见面的前后几天都沾着光,变成好日子。--钱钟书",
            "深情不及久伴，厚爱无需多言。",
            "我想成为一个温柔的人，因为曾被温柔的人那样对待过。——《夏目友人帐》",
            "一切重要的事我都想好好珍惜。——《夏目友人帐》",
            "正义一定会胜么？这是当然的吧，因为只有胜利的一方才是正义啊！--《海贼王》",
            "只要能成为你的力量 我就算变成真正的怪物也在所不惜.--《海贼王》",
            "只要有你在，我就会继续努力。--《侧耳倾听》",
            "有了喜欢的人世界都会变得多姿多彩起来。--《四月是你的谎言》",
            "我渴望自己能有保护你的力量。--《秒速五厘米》",
            "我喜欢跟你待在一起消磨时间。--《马男波杰克》",
            "去冒险吧，去看世界。只要记得回到我身边。",
            "我是最无害的人啦,我唯一能忍心伤害的人就是我自己。--《爱在黎明破晓前》"
            ];
    private string GetLoveTalk()
    {
        Random rd = new Random();
        int n = rd.Next(_lovetalks.Length - 1);
        return _lovetalks[n];
    }

    private void Test()
    {
        _alarmService?.SetAlarmTest();
    }

    private void DismissAlarm()
    {
        _alarmService.DismissAlarm();
        _alarmService.EnsureAlarmIsSetIfEnabled();
    }
}

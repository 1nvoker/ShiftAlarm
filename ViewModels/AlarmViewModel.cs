using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiCatAlarm.Services;

namespace MauiCatAlarm.ViewModels;

public class AlarmViewModel : ObservableObject
{
    private readonly AlarmService _alarmService;
    private readonly Func<MainPage> _mainPageFactory;

    public AlarmViewModel(
        AlarmService alarmService,
        Func<MainPage> mainPageFactory)
    {
        _alarmService = alarmService;
        _mainPageFactory = mainPageFactory;

        DismissAlarmCommand = new RelayCommand(DismissAlarm, CanDismissAlarm);
    }

    public Window? Window { get; set; }

    public ICommand DismissAlarmCommand { get; }

    private bool CanDismissAlarm()
    {
        return true;
    }

    private void DismissAlarm()
    {
        _alarmService.DismissAlarm();

        if (App.Current.Windows.Count > 1 && Window != null)
        {
            App.Current.CloseWindow(Window);
        }
        else
        {
            App.Current.MainPage = _mainPageFactory();
        }
    }
}

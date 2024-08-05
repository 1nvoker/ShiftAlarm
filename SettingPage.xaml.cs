namespace MauiCatAlarm;
using MauiCatAlarm.ViewModels;

public partial class SettingPage : ContentPage
{
	public SettingPage(SettingViewModel sv)
	{
		BindingContext = sv;

        InitializeComponent();
	}

    private void TimePickerDay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(TimePicker.Time))
        {
            var sv = (SettingViewModel)BindingContext;
            sv.SaveShiftSettingsDay();
        }
    }

    private void TimePickerMid_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var sv = (SettingViewModel)BindingContext;
            sv.SaveShiftSettingsMid();
        }
    }

    private void TimePickerNight_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var sv = (SettingViewModel)BindingContext;
            sv.SaveShiftSettingsNight();
        }
    }
}

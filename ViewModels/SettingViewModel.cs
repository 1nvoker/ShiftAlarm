using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

//using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;


namespace MauiCatAlarm.ViewModels
{
    public class SettingViewModel : ObservableObject, IDisposable
    {
        private readonly ShiftSetService _shiftSetService;

        private TimeSpan _alarmTimeDay;
        private TimeSpan _alarmTimeMid;
        private TimeSpan _alarmTimeNight;

        public TimeSpan AlarmTimeDay
        {
            get => _alarmTimeDay;
            set => SetProperty(ref _alarmTimeDay, value);
        }

        public TimeSpan AlarmTimeMid
        {
            get => _alarmTimeMid;
            set => SetProperty(ref _alarmTimeMid, value);
        }

        public TimeSpan AlarmTimeNight
        {
            get => _alarmTimeNight;
            set => SetProperty(ref _alarmTimeNight, value);
        }

        public string AlarmRingtoneName => _shiftSetService.GetAlarmRingtoneName();

        public ICommand UpdateAlarmRingtoneCommand { get; }

        public ICommand SaveSettings { get; }

        public SettingViewModel(ShiftSetService shiftsetService)
        {
            _shiftSetService = shiftsetService;

            App.Current.PropertyChanged += App_PropertyChanged;

            _shiftSetService.RingtoneChanged += AlarmService_RingtoneChanged;

            AlarmTimeDay = _shiftSetService.GetScheduledTimeDay() ?? new TimeSpan(7, 14, 0);
            AlarmTimeMid = _shiftSetService.GetScheduledTimeMid() ?? new TimeSpan(15, 20, 0);
            AlarmTimeNight = _shiftSetService.GetScheduledTimeNight() ?? new TimeSpan(23, 40, 0);

            UpdateAlarmRingtoneCommand = new AsyncRelayCommand(UpdateAlarmRingtoneAsync);
            SaveSettings = new Command(SaveShiftSettings);

            Debug.WriteLine(string.Format("SettingViewModel {0}", AlarmTimeDay.ToString("hh\\:mm", CultureInfo.InvariantCulture)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_shiftSetService.IsEnabledChanged -= AlarmService_IsEnabledChanged;
                //_shiftSetService.ScheduledTimeChanged -= AlarmService_ScheduledTimeChanged;
                App.Current.PropertyChanged -= App_PropertyChanged;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void App_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.IsAlarmActive))
            {
                //IsAlarmOngoing = App.Current.IsAlarmActive;
                //OnPropertyChanged(nameof(IsAlarmOngoing));
            }
        }

        private void AlarmService_RingtoneChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AlarmRingtoneName));
        }

        private Task UpdateAlarmRingtoneAsync()
        {
            _shiftSetService.PickAlarmRingtone();
            return Task.CompletedTask;

            //var fileTypes = new Dictionary<DevicePlatform, IEnumerable<string>>()
            //{
            //    [DevicePlatform.Android] = ["audio/*"]
            //};

            //var result = await FilePicker.Default.PickAsync(new PickOptions
            //{
            //    PickerTitle = "Select alarm ringtone",
            //    FileTypes = new(fileTypes)
            //});

            //if (result != null)
            //{
            //    // It seems MAUI copies the selected file to the cache dir, but
            //    // Android might clear that, so we should copy it to the app data
            //    // dir.
            //    using var selectedFile = await result.OpenReadAsync();
            //    var targetPath = Path.Combine(FileSystem.Current.AppDataDirectory, result.FileName);

            //    using var targetFile = File.Create(targetPath);
            //    await selectedFile.CopyToAsync(targetFile);

            //    _shiftSetService.SetAlarmRingtone(result.FileName, targetPath);
            //}
        }

        public void SaveShiftSettings()
        {
            _shiftSetService.SetScheduledTimeDay(AlarmTimeDay);
            _shiftSetService.SetScheduledTimeMid(AlarmTimeMid);
            _shiftSetService.SetScheduledTimeNight(AlarmTimeNight);
            Debug.WriteLine(string.Format("SaveShiftSettings {0}", AlarmTimeDay.ToString("hh\\:mm", CultureInfo.InvariantCulture)));
        }
    }
}

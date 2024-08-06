using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

//using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;
using static Android.Resource;


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

        public string LoveTalk => GetLoveTalk();

        public ICommand UpdateAlarmRingtoneCommand { get; }

        public SettingViewModel(ShiftSetService shiftsetService)
        {
            _shiftSetService = shiftsetService;

            App.Current.PropertyChanged += App_PropertyChanged;

            _shiftSetService.RingtoneChanged += AlarmService_RingtoneChanged;

            AlarmTimeDay = _shiftSetService.GetScheduledTimeDay() ?? new TimeSpan(7, 14, 0);
            AlarmTimeMid = _shiftSetService.GetScheduledTimeMid() ?? new TimeSpan(15, 20, 0);
            AlarmTimeNight = _shiftSetService.GetScheduledTimeNight() ?? new TimeSpan(23, 40, 0);

            UpdateAlarmRingtoneCommand = new AsyncRelayCommand(UpdateAlarmRingtoneAsync);

            Debug.WriteLine(string.Format("SettingViewModel {0}", AlarmTimeDay.ToString("hh\\:mm", CultureInfo.InvariantCulture)));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
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

        public void SaveShiftSettingsDay()
        {
            _shiftSetService.SetScheduledTimeDay(AlarmTimeDay);
        }

        public void SaveShiftSettingsMid()
        {
            _shiftSetService.SetScheduledTimeMid(AlarmTimeMid);
        }

        public void SaveShiftSettingsNight()
        {
            _shiftSetService.SetScheduledTimeNight(AlarmTimeNight);
        }

        private string[] _lovetalks = {
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
            };
        private string GetLoveTalk()
        {
            Random rd = new Random();
            int n = rd.Next(_lovetalks.Length - 1);
            return _lovetalks[n];
        }
    }
}

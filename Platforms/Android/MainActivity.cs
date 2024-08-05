using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Util;

using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;

namespace MauiCatAlarm;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (data != null && requestCode == Services.ShiftSetService.RingtonePickerRequestCode)
        {
            var sService = App.Current.ServiceProvider.GetRequiredService<ShiftSetService>();
            var uri = data.GetParcelableExtra<Android.Net.Uri>(RingtoneManager.ExtraRingtonePickedUri);
            if (uri == null)
            {
                sService.SetDefaultAlarmRingtone();
                return;
            }

            var ringtone = RingtoneManager.GetRingtone(ApplicationContext, uri);
            var title = ringtone?.GetTitle(ApplicationContext);

            var filePath = uri?.ToString();
            if (filePath == null)
            {
                Log.Error("MainActivity", "Picked ringtone URI ToString() is null (but URI itself is not null)");
                return;
            }

            sService.SetAlarmRingtone(title ?? "Unknown", filePath);
        }
    }
}

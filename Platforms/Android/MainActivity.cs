using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using MauiCatAlarm.Platforms.Android;
using MauiCatAlarm.Services;

namespace MauiCatAlarm;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly List<string> _permissions31 =
        [
            Manifest.Permission.ReceiveBootCompleted,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.Vibrate,
            Manifest.Permission.WakeLock,
            Manifest.Permission.SetAlarm,
            Manifest.Permission.ForegroundService,
            Manifest.Permission.UseFullScreenIntent,
            Manifest.Permission.ScheduleExactAlarm,
        ];

    private readonly List<string> _permissions33 =
        [
            Manifest.Permission.ReadMediaAudio,
            Manifest.Permission.UseExactAlarm,
            Manifest.Permission.PostNotifications,
        ];

    private readonly List<string> _permissions34 =
        [
            Manifest.Permission.ForegroundServiceMediaPlayback,
        ];
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        //if (App.Current is not App app)
        //    throw new InvalidOperationException("Could not find App instance.");

        //var alarmPage = app.ServiceProvider.GetRequiredService<AlarmPage>();
        //app.OpenWindow();
        //app.OpenWindow(new Microsoft.Maui.Controls.Window(alarmPage));
        //Log.Info("MainActivity", "Opened new window with AlarmPage");

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            ActivityCompat.RequestPermissions(this, _permissions31.ToArray(), 0);
        }
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            ActivityCompat.RequestPermissions(this, _permissions33.ToArray(), 0);
        }
        if (OperatingSystem.IsAndroidVersionAtLeast(34))
        {
            ActivityCompat.RequestPermissions(this, _permissions34.ToArray(), 0);
        }
    }

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

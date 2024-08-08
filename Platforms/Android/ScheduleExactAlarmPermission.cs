using Android;

namespace MauiCatAlarm.Platforms.Android;

public class ScheduleExactAlarmPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                return [(Manifest.Permission.ScheduleExactAlarm, true)];
            return [];
        }
    }
}

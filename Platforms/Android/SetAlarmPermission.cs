using Android;

namespace MauiCatAlarm.Platforms.Android;

public class SetAlarmPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            return [(Manifest.Permission.SetAlarm, true)];
        }
    }
}

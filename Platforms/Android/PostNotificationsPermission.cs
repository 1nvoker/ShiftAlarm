
using Android;

namespace MauiCatAlarm.Platforms.Android;

public class PostNotificationsPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                return [(Manifest.Permission.PostNotifications, true)];
            return [];
        }
    }
}

#if MACOS
using Foundation;
using System.Runtime.Versioning;
using static Foundation.NSUserNotification;
using static Foundation.NSUserNotificationCenter;

namespace NearShare.Platforms.MacOs;

[SupportedOSPlatform("macos")]
internal static class NotificationCompat
{
    public static void ShowNotification(string title, string content)
    {
        NSUserNotification notification = new()
        {
            Title = title,
            Subtitle = content,
            SoundName = NSUserNotificationDefaultSoundName
        };
        DefaultUserNotificationCenter.DeliverNotification(notification);
    }
}
#endif
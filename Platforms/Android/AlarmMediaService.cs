﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;

using AndroidX.Core.App;

namespace MauiCatAlarm.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
public class AlarmMediaService : Service
{
    public const int NotificationId = 69;

    private MediaPlayer? _player;

    public override void OnCreate()
    {
        Log.Info("AlarmMediaService", "OnCreate Thread=" + Java.Lang.Thread.CurrentThread().Id);
        base.OnCreate();

        var context = ApplicationContext ?? throw new InvalidOperationException("Context not available.");

        var audioManager = context.GetSystemService(AudioService).JavaCast<AudioManager>();
        var session = audioManager!.GenerateAudioSessionId();

        var uri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
        var attrib = new AudioAttributes.Builder()
            .SetContentType(AudioContentType.Music)
            !.SetUsage(AudioUsageKind.Alarm)
            !.Build();

        var callbacks = new PlayerCallbacks();
        _player = MediaPlayer.Create(context, uri, null, attrib, session);
        _player!.Looping = true;
        _player.SetOnPreparedListener(callbacks);
        _player.SetOnInfoListener(callbacks);
        _player.SetOnErrorListener(callbacks);
    }

    public override IBinder? OnBind(Intent? intent) => null;

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        Log.Info("AlarmMediaService", $"OnStartCommand {intent} {flags} {startId} Thread=" + Java.Lang.Thread.CurrentThread().Id);
        var context = ApplicationContext ?? throw new InvalidOperationException("Context not available.");

        // if (OperatingSystem.IsAndroidVersionAtLeast(33))
        // {
        //     var notificationPermission = context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications);
        //     if (notificationPermission == Permission.Denied)
        //     {
        //         StopSelf();
        //         return StartCommandResult.NotSticky;
        //     }
        // }

        var alarmIntent = new Intent(context, typeof(AlarmActivity));
        alarmIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
        var pendingIntent = PendingIntent.GetActivity(context, 0, alarmIntent, PendingIntentFlags.Immutable);

        // var notificationManager = context.GetSystemService(Context.NotificationService).JavaCast<NotificationManager>();
        var notification = new NotificationCompat.Builder(context, MainApplication.ChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("CAT ALARM!")
            .SetContentText("Mew mew mew mew mew mew!")
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetCategory(NotificationCompat.CategoryAlarm)
            .SetContentIntent(pendingIntent)
            .SetFullScreenIntent(pendingIntent, true)
            .SetOngoing(true)
            .Build();

        // notificationManager!.Notify(NotificationId, notification);

        StartForeground(NotificationId, notification);
        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _player?.Stop();
        _player?.Dispose();
    }

    public void Stop()
    {
        _player?.Stop();
        StopForeground(StopForegroundFlags.Remove);
        StopSelf();
    }

    private class PlayerCallbacks : Java.Lang.Object, MediaPlayer.IOnErrorListener, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnInfoListener
    {
        public bool OnError(MediaPlayer? mp, [GeneratedEnum] MediaError what, int extra)
        {
            Log.Error("AlarmMediaService", $"MediaPlayer {mp.AudioSessionId} Error: {what} {extra}");
            return true;
        }

        public bool OnInfo(MediaPlayer? mp, [GeneratedEnum] MediaInfo what, int extra)
        {
            Log.Info("AlarmMediaService", $"MediaPlayer {mp.AudioSessionId} {what} {extra}");
            return true;
        }

        public void OnPrepared(MediaPlayer? mp)
        {
            Log.Info("AlarmMediaService", $"MediaPlayer {mp.AudioSessionId} prepared Thread=" + Java.Lang.Thread.CurrentThread().Id);
            mp.Start();
        }
    }
}
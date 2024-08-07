﻿using MauiCatAlarm.Services;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MauiCatAlarm;

public partial class App : Application
{
    private readonly IServiceScope _serviceScope;
    private bool _isAlarmActive;

    public App(AlarmService alarmService, IServiceProvider serviceProvider, ILogger<App> logger)
    {
        _serviceScope = serviceProvider.CreateScope();

        InitializeComponent();

        //Debug.WriteLine("App start");
        alarmService.EnsureAlarmIsSetIfEnabled();

        MainPage = new AppShell();
    }

    public static new App Current => (App)Application.Current!;

    public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    public bool IsAlarmActive
    {
        get => _isAlarmActive;
        set
        {
            if (_isAlarmActive != value)
            {
                _isAlarmActive = value;
                OnPropertyChanged(nameof(IsAlarmActive));
            }
        }
    }

    protected override void CleanUp()
    {
        _serviceScope.Dispose();
        base.CleanUp();
    }
}

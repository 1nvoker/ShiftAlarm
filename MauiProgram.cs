using MauiCatAlarm.Services;
using MauiCatAlarm.ViewModels;

using Microsoft.Extensions.Logging;

namespace MauiCatAlarm;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PublicSans-Regular.ttf", "Public Sans");
                fonts.AddFont("SpaceGrotesk-Regular.ttf", "Space Grotesk");
                fonts.AddFont("SpaceGrotesk-Bold.ttf", "Space Grotesk Bold");
            });

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AlarmViewModel>();
        builder.Services.AddTransient<SettingViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Func<MainPage>>(provider =>
        {
            return () => provider.GetRequiredService<MainPage>();
        });
        builder.Services.AddTransient<AlarmPage>();
        builder.Services.AddTransient<Func<AlarmPage>>(provider =>
        {
            return () => provider.GetRequiredService<AlarmPage>();
        });
        builder.Services.AddTransient<SettingPage>();
        builder.Services.AddTransient<Func<SettingPage>>(provider =>
        {
            return () => provider.GetRequiredService<SettingPage>();
        });
        builder.Services.AddSingleton<AlarmService>();
        builder.Services.AddSingleton<ShiftSetService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using PUV_Route_Recommender.Interfaces;
using PUV_Route_Recommender.Services;
using PUV_Route_Recommender.Views;

namespace PUV_Route_Recommender
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .Services
                .AddHttpClient();

            //Services
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IOverpassApiServices, OverpassApiServices>();
            builder.Services.AddSingleton<IRouteService, RouteService>();

            //ViewModels
            builder.Services.AddSingleton<RoutesViewModel>();

            //Views
            builder.Services.AddSingleton<RoutesView>();
#if DEBUG
		builder.Logging.AddDebug();
#endif
            //builder.Services.AddSingleton<IConfiguration>(provider =>
            //{
            //    var assembly = typeof(App).GetTypeInfo().Assembly;
            //    using var stream = assembly.GetManifestResourceStream("PUV_Route_Recommender.appsettings.json");

            //    return new ConfigurationBuilder()
            //        .AddJsonStream(stream)
            //        .Build();
            //});
            return builder.Build();
        }
    }
}
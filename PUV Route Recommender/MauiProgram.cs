using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using CommuteMate.Interfaces;
using CommuteMate.Services;
using CommuteMate.Views;
using CommuteMate.Repositories;
using SQLite;

namespace CommuteMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true) /*for mapsui*/
                //.UseMauiMaps() /*uses google maps api key*/
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif
            // Database
            builder.Services.AddDbContext<CommuteMateDbContext>();

            SQLitePCL.Batteries_V2.Init();

            // Repositories
            builder.Services.AddSingleton<IStreetRepository>(provider =>
                new StreetRepository(provider.GetService<CommuteMateDbContext>()));
            builder.Services.AddSingleton<IRouteRepository>(provider =>
                new RouteRepository(provider.GetService<CommuteMateDbContext>()));
            builder.Services.AddSingleton<IRouteStreetRepository>(provider =>
                new RouteStreetRepository(provider.GetService<CommuteMateDbContext>()));

            //Services
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IOverpassApiServices, OverpassApiServices>();
            builder.Services.AddSingleton<IRouteService, RouteService>();
            builder.Services.AddSingleton<IStreetService, StreetService>();
            builder.Services.AddSingleton<IRouteStreetService, RouteStreetService>();
            builder.Services.AddSingleton<IMapServices, MapServices>();


            //ViewModels
            builder.Services.AddSingleton<RoutesViewModel>();
            builder.Services.AddTransient<RouteInfoViewModel>();
            builder.Services.AddTransient<NavigatingViewModel>();

            //Views
            builder.Services.AddSingleton<RoutesView>();
            builder.Services.AddTransient<RoutesInfoPage>();
            builder.Services.AddSingleton<NavigatingPage>();

            var dbContext = new CommuteMateDbContext();

//#if DEBUG
//            dbContext.Database.EnsureDeleted();
//#endif
            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

            return builder.Build();
        }
    }
}
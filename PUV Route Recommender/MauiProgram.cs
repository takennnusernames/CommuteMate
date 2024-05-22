using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommuteMate.Interfaces;
using CommuteMate.Services;
using CommuteMate.Views;
using CommuteMate.Repositories;
using The49.Maui.BottomSheet;
using CommuteMate.ApiClient.IoC;
using System.Text.Json.Serialization;
using CommuteMate.Utilities;

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
                .UseMauiMaps()
                .UseBottomSheet()
                //.UseMauiMaps() /*uses google maps api key*/
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5005" : "http://localhost/5005";
            builder.Services.AddCommuteMateApiClientService(x => x.ApiBaseAddress = baseUrl);
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

            //Services
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IOverpassApiServices, OverpassApiServices>();
            builder.Services.AddSingleton<IRouteService, RouteService>();
            builder.Services.AddSingleton<IStreetService, StreetService>();
            builder.Services.AddSingleton<IMapServices, MapServices>();
            builder.Services.AddSingleton<ICommuteMateApiService, CommuteMateApiService>();


            //ViewModels
            builder.Services.AddSingleton<RoutesViewModel>();
            builder.Services.AddSingleton<RouteInfoViewModel>();
            builder.Services.AddTransient<NavigatingViewModel>();

            //Views
            builder.Services.AddSingleton<RoutesView>();
            builder.Services.AddSingleton<RoutesInfoPage>();
            builder.Services.AddSingleton<NavigatingPage>();
            builder.Services.AddTransient<MethodTests>();

            //Sheet
            builder.Services.AddTransient<RoutePathSelection>();

//            var dbContext = new CommuteMateDbContext();

//#if DEBUG
//            dbContext.Database.EnsureDeleted();
//#endif
//            dbContext.Database.EnsureCreated();
//            dbContext.Dispose();

            return builder.Build();
        }
    }
}
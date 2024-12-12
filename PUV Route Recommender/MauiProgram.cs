using CommuteMate.Interfaces;
using CommuteMate.Repositories;
using CommuteMate.Services;
using CommuteMate.Views;
using CommuteMate.Views.SlideUpSheets;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using The49.Maui.BottomSheet;
#if ANDROID
using CommuteMate.Platforms.Android;
#elif IOS
using CommuteMate.Platforms.iOS;
#endif

namespace CommuteMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                //.UseSkiaSharp(true) /*for mapsui*/
                .UseMauiCommunityToolkit()
                .UseMauiMaps()
                .UseBottomSheet()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID || IOS 
                handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
#endif
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
            builder.Services.AddSingleton<IDownloadsRepository>(provider =>
                new DownloadsRepository(provider.GetService<CommuteMateDbContext>()));
            builder.Services.AddSingleton<IVehicleRepository, VehicleRepository>();

            //Services
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IRouteService, RouteService>();
            builder.Services.AddSingleton<IStreetService, StreetService>();
            builder.Services.AddSingleton<IRouteStreetService, RouteStreetService>();
            builder.Services.AddSingleton<IMapServices, MapServices>();
            builder.Services.AddSingleton<ICommuteMateApiService, CommuteMateApiService>();


            //ViewModels
            builder.Services.AddSingleton<RoutesViewModel>();
            builder.Services.AddSingleton<RouteDetailsViewModel>();
            builder.Services.AddTransient<NavigatingViewModel>();
            builder.Services.AddSingleton<VehicleInfoViewModel>();
            builder.Services.AddSingleton<MapViewModel>();

            //Views
            builder.Services.AddSingleton<RoutesView>();
            builder.Services.AddSingleton<RouteListView>();
            builder.Services.AddSingleton<NavigatingPage>();
            builder.Services.AddTransient<SurveyPage>();
            builder.Services.AddTransient<MapView>();
            builder.Services.AddTransient<OfflinePathMapView>();
            builder.Services.AddTransient<RouteDetailsView>();
            builder.Services.AddSingleton<DetailsView>();
            builder.Services.AddSingleton<VehicleInfoPage>();

            //Sheet
            builder.Services.AddSingleton<SlideUpCard>();
            builder.Services.AddSingleton<OfflineSlideUpCard>();

            //Database
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
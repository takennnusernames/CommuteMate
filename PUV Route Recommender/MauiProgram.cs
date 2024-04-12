using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using PUV_Route_Recommender.Interfaces;
using PUV_Route_Recommender.Services;
using PUV_Route_Recommender.Views;
using PUV_Route_Recommender.Repositories;
using SQLite;

namespace PUV_Route_Recommender
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
            builder.Services.AddSingleton<SQLiteAsyncConnection>(provider =>
            {
                var connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "Commute_Mate_Data.db3"));

                connection.ExecuteAsync("PRAGMA foreign_keys = ON;").Wait();

#if DEBUG
                connection.DropTableAsync<Route>().Wait();
                connection.DropTableAsync<Street>().Wait();
                connection.DropTableAsync<RouteStreet>().Wait();
#endif
                connection.CreateTableAsync<Route>().Wait();
                connection.CreateTableAsync<Street>().Wait();
                connection.CreateTableAsync<RouteStreet>().Wait();

                return connection;
            });

            // Repositories
            builder.Services.AddSingleton<IStreetRepository>(provider =>
                new StreetRepository(provider.GetService<SQLiteAsyncConnection>()));
            builder.Services.AddSingleton<IRouteRepository>(provider =>
                new RouteRepository(provider.GetService<SQLiteAsyncConnection>()));
            builder.Services.AddSingleton<IRouteStreetRepository>(provider =>
                new RouteStreetRepository(provider.GetService<SQLiteAsyncConnection>()));

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

            return builder.Build();
        }
    }
}
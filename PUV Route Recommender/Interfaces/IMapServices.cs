using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Map = Mapsui.Map;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using Microsoft.Maui.Controls.Maps;

namespace CommuteMate.Interfaces
{
    public interface IMapServices
    {
        Task CreateGoogleMapAsync(GoogleMap map);
        Task<Pin> AddGooglePin(Location location, GoogleMap map, string label);
        Task<Pin> AddGooglePin(LocationDetails location, GoogleMap map);
        Task<CustomPin> AddCustomPin(LocationDetails location, GoogleMap map);
        Task<Pin> AddGoogleMarker(Location location, GoogleMap map, string label, string action);
        Task RemoveGooglePin(Pin pin, GoogleMap map);
        Task<Polyline> AddGooglePolyline(Geometry geometry, GoogleMap map, string action);
        Task<Polyline> AddGooglePolyline(Geometry geometry, GoogleMap map, string action, Color color);
        Task<LocationDetails> GetCurrentLocationAsync();
        Task<List<LocationDetails>> SearchLocationAsync(string input);
        Task<List<LocationDetails>> GoogleSearchLocationAsync(string input);
    }
}

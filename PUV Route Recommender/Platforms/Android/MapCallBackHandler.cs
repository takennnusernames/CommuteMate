using Android.Gms.Maps;
using IMap = Microsoft.Maui.Controls.Maps.Map;

namespace CommuteMate.Platforms.Android
{
    class MapCallbackHandler(CustomMapHandler mapHandler) : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(GoogleMap googleMap)
        {
            mapHandler.UpdateValue(nameof(IMap.Pins));
            //mapHandler.Map?.SetOnMarkerClickListener(new CustomMarkerClickListener(mapHandler));
            //mapHandler.Map?.SetOnInfoWindowClickListener(new CustomInfoWindowClickListener(mapHandler));
        }
    }
}

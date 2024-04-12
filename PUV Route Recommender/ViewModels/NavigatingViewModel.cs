using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Microsoft.Maui.Networking;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PUV_Route_Recommender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.ViewModels
{
    public partial class NavigatingViewModel : BaseViewModel
    {
        //dependecy injections
        readonly IMapServices _mapServices;
        readonly IConnectivity _connectivity;
        public NavigatingViewModel(IMapServices mapServices, IConnectivity connectivity) 
        {
            Title = "Map";
            _mapServices = mapServices;
            _connectivity = connectivity;
            SearchResults = [];
        }

        //properties
        [ObservableProperty]
        string searchText;
        [ObservableProperty]
        ObservableCollection<string> searchResults;

        string origin;
        public string Origin
        {
            get => origin;
            set
            {
                origin = value;
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        async Task GetLocationAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (string.IsNullOrWhiteSpace(origin))
                {
                    return;
                }
                try
                {
                    await _mapServices.GetLocationAsync(origin);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"An error occurred while searching for the location: {ex.Message}", "OK");
                }

        }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SearchLocation(string input)
        {
            if (IsBusy)
                return;
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }

                IsBusy = true;

                var locations = await _mapServices.SearchLocationAsync(input);

                if(SearchResults.Count != 0)
                    SearchResults.Clear();

                foreach (var location in locations)
                    SearchResults.Add(location);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to search location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

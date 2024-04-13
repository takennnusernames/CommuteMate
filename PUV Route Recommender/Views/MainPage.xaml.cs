using System;
using OsmSharp.Streams;
using OsmSharp;
using SkiaSharp;

namespace CommuteMate
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            //FetchOSMData();
            
        }

        //public async void FetchOSMData()
        //{
        //    try
        //    {
        //        // Fetch OSM data from the backend endpoint
        //        HttpClient client = new HttpClient();
        //        var response = await client.GetAsync("https://your-backend-url/api/osmdata");

        //        // Check if the response is successful
        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Parse the received OSM data
        //            var osmData = await response.Content.ReadAsAsync<List<OSMData>>();

        //            // Update the frontend UI to display the OSM data
        //            UpdateUI(osmData);
        //        }
        //        else
        //        {
        //            // Handle error
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }
        //}

    }
}
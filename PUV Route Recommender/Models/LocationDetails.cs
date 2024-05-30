using Microsoft.Maui.Controls.Maps;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Models
{
    public class LocationDetails
    {
        public string Name { get; set; }
        public Coordinate Coordinate { get; set; }
    }
    public class CustomPin : Pin
    {
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CustomPin));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
    }
}

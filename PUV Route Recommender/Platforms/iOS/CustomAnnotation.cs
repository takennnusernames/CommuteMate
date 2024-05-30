using MapKit;
using Microsoft.Maui.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace CommuteMate.Platforms.iOS
{
    public class CustomAnnotation : MKPointAnnotation
    {
        public Guid Identifier { get; init; }
        public UIImage Image { get; init; }
        public required IMapPin Pin { get; init; }
    }
}

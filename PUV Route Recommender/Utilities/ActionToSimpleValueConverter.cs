using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Utilities
{
    public class ActionToSimpleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string action)
            {
                if (action.StartsWith("Walk", StringComparison.OrdinalIgnoreCase))
                {
                    return "Walk";
                }
                if (action.StartsWith("Ride", StringComparison.OrdinalIgnoreCase))
                {
                    return "Ride";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

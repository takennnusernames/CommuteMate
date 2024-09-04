using Microsoft.Maui.Maps.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Utilities
{
    public class PinTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultPinTemplate { get; set; }
        public DataTemplate CustomPinTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is CustomPin)
            {
                return CustomPinTemplate;
            }
            return DefaultPinTemplate;
        }
    }
}

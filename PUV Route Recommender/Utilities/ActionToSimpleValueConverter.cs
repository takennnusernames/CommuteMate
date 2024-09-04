using System;
using System.Collections;
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
                else if (action.StartsWith("Ride", StringComparison.OrdinalIgnoreCase))
                {
                    return "Ride";
                }
                else
                {
                    return "Intersection";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as ICollection;
            return collection?.Count ?? 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DisableParentScrollBehavior : Behavior<CollectionView>
    {
        protected override void OnAttachedTo(CollectionView bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Scrolled += OnParentScrolled;
        }

        protected override void OnDetachingFrom(CollectionView bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Scrolled -= OnParentScrolled;
        }

        private void OnParentScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            var parentCollectionView = sender as CollectionView;
            if (parentCollectionView != null && parentCollectionView.Parent is VisualElement parent)
            {
                // Stop parent scrolling if needed
                // For example, you can reset the scroll position
                parentCollectionView.ScrollTo(0, animate: false);
            }
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Galiev_Глазки_save
{
    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int priority)
            {
                if (priority == 1)
                    return Brushes.LightCoral;
                if (priority == 2)
                    return Brushes.LightGreen;
                if (priority == 3)
                    return Brushes.LightBlue;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
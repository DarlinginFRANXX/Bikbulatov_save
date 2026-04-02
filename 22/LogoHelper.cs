using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Galiev_Глазки_save
{
    public static class ImageHelper
    {
        public static BitmapImage DefaultLogo { get; } = LoadDefaultLogo();

        private static BitmapImage LoadDefaultLogo()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents", "picture.png");
            if (File.Exists(defaultPath))
                return new BitmapImage(new Uri(defaultPath));
            return null;
        }

        public static BitmapImage LoadLogo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return DefaultLogo;

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents", fileName);
            if (File.Exists(fullPath))
                return new BitmapImage(new Uri(fullPath));
            return DefaultLogo;
        }
    }

    public class LogoToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.LoadLogo(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
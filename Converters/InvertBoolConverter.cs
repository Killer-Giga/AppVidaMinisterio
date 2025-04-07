using System.Globalization;

namespace AppVidaMinisterio.Converters
{
    internal class InvertBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter != null && parameter.ToString() == "Invert")
                {
                    return !boolValue;
                }
                return boolValue;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

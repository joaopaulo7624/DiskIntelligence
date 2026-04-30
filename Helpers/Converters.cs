using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence.Helpers;

public class ColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && !string.IsNullOrEmpty(hex))
        {
            try { return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, byte.Parse(hex[1..3], System.Globalization.NumberStyles.HexNumber), byte.Parse(hex[3..5], System.Globalization.NumberStyles.HexNumber), byte.Parse(hex[5..7], System.Globalization.NumberStyles.HexNumber))); }
            catch { }
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BytesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ulong bytes) return ByteFormatter.Format(bytes);
        if (value is long l) return ByteFormatter.Format((ulong)l);
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class NumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            uint n => ByteFormatter.FormatNumber(n),
            int n => ByteFormatter.FormatNumber((uint)n),
            _ => "0"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
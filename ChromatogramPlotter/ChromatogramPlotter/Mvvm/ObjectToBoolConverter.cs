using System;
using System.Globalization;
using System.Windows.Data;

namespace ChromatogramPlotter.Mvvm
{
    /// <summary>
    /// オブジェクトがnullでない場合にtrueを返すコンバーター。
    /// </summary>
    class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // valueがnullでなければtrue、nullならfalseを返す
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 今回は使わないので実装不要
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CalKnights.Converter
{
    class BoolListConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((List<Boolean>)value)[ParseInt(parameter)];
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack BoolListConverter");
        }
        int ParseInt(object parameter)
        {
            if (parameter is double)
                return (int)parameter;

            else if (parameter is int)
                return (int)parameter;

            else if (parameter is string)
                return int.Parse((string)parameter);

            return 1;
        }
    }
}

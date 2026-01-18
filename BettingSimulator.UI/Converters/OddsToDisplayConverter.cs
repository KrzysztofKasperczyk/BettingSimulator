using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BettingSimulator.UI.Converters
{
    public sealed class OddsToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not decimal v)
                return "-";

            if (!OddsLimits.IsWithinRange(v))
                return "-";

            return v.ToString("0.00", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

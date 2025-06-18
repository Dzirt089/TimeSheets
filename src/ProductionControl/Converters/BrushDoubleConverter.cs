using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ProductionControl.Converters
{
	[ValueConversion(typeof(double), typeof(Brush))]
	public class BrushDoubleConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double doubleValue)
			{
				if (doubleValue < 0) return Brushes.OrangeRed;
				else if (doubleValue > 0) return Brushes.ForestGreen;

			}
			return Brushes.Transparent;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

		public override object ProvideValue(IServiceProvider serviceProvider) => this;
	}
}

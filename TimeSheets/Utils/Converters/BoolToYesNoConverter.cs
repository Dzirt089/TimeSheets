using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TimeSheets.Utils.Converters
{
	[ValueConversion(typeof(bool), typeof(string))]
	public class BoolToYesNoConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? "ДА" : "НЕТ";
			}
			return "НЕТ";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

		public override object ProvideValue(IServiceProvider serviceProvider) => this;
	}
}

using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace ProductionControl.Utils.Converters
{
	[ValueConversion(typeof(byte[]), typeof(BitmapImage))]
	public class ByteArrayToImageConvertor : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is byte[] bytes && bytes.Length > 0)
			{
				BitmapImage image = new BitmapImage();
				using var mem = new MemoryStream(bytes);
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();

				image.Freeze();

				return image;
			}
			else return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

		public override object ProvideValue(IServiceProvider serviceProvider) => this;
	}
}

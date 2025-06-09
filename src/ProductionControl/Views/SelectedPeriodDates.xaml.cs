using ProductionControl.ViewModel;

using System.Windows.Controls;

namespace ProductionControl.Views
{
	/// <summary>
	/// Логика взаимодействия для SelectedPeriodDates.xaml
	/// </summary>
	public partial class SelectedPeriodDates : UserControl
	{
		public SelectedPeriodDates(MainViewModel context)
		{
			InitializeComponent();
			DataContext = context;

		}
	}
}

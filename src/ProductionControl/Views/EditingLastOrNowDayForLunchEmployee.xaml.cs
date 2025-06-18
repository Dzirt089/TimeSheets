using ProductionControl.ViewModel;

using System.Windows.Controls;

namespace ProductionControl.Views
{
	/// <summary>
	/// Логика взаимодействия для EditingLastOrNowDayForLunchEmployee.xaml
	/// </summary>
	public partial class EditingLastOrNowDayForLunchEmployee : UserControl
	{
		public EditingLastOrNowDayForLunchEmployee(MainViewModel context)
		{
			InitializeComponent();
			DataContext = context;

		}
	}
}

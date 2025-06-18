using ProductionControl.ViewModel;

using System.Windows.Controls;

namespace ProductionControl.Views
{
	/// <summary>
	/// Логика взаимодействия для DismissalEmployee.xaml
	/// </summary>
	public partial class DismissalEmployee : UserControl
	{
		public DismissalEmployee(MainViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}

		public DismissalEmployee(StaffExternalOrgViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}

using TimeSheets.ViewModel;
using System.Windows.Controls;

namespace TimeSheets.Views
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

		public DismissalEmployee(StaffViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}

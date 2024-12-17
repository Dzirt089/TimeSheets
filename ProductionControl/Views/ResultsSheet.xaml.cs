using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ProductionControl.ViewModel;

namespace ProductionControl.Views
{
	/// <summary>
	/// Логика взаимодействия для ResultsSheet.xaml
	/// </summary>
	public partial class ResultsSheet : MetroWindow
	{
		public ResultsSheet(MainViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}
}

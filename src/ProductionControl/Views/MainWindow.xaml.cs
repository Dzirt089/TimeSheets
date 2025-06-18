namespace ProductionControl
{
	using MahApps.Metro.Controls;

	using ProductionControl.ViewModel;



	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			Closed += MainWindow_Closed;
		}

		private void MainWindow_Closed(object? sender, EventArgs e)
		{
			var vmRS = (MainViewModel)DataContext;
			if (vmRS != null)
			{
				vmRS?.ResultsSheet?.Close();
				vmRS?.StaffView?.Close();
				vmRS?.FAQ?.Close();
				vmRS?.StaffExOrgView?.Close();
			}
		}
	}
}
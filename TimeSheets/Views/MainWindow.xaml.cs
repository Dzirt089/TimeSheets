namespace TimeSheets
{
	using MahApps.Metro.Controls;
	using TimeSheets.ViewModel;

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
		//TODO: Перенести от сюда в метод закрытия в App.xaml.cs
		private void MainWindow_Closed(object? sender, EventArgs e)
		{
			var vmRS = (MainViewModel)DataContext;
			if (vmRS != null)
			{
				vmRS?.ResultsSheet?.Close();
				vmRS?.StaffView?.Close();
				vmRS?.FAQ?.Close();
				vmRS?.StaffView?.Close();
			}
		}
	}
}
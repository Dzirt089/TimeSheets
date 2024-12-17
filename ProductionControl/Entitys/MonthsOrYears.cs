using CommunityToolkit.Mvvm.ComponentModel;
namespace ProductionControl.Entitys
{

	public class MonthsOrYears : ObservableObject
	{
		public MonthsOrYears(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get => _id; set => SetProperty(ref _id, value); }
		private int _id;
		public string? Name { get => _name; set => SetProperty(ref _name, value); }
		private string? _name;
	}
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Model.EmployeesFactory
{
	/// <summary>
	/// Класс, который отображает краткие сведения о сотруднике в Табеле: ФИО, смена, переработка
	/// </summary>
	public class ShiftDataEmployee : ObservableObject
	{
		/// <summary>
		/// Короткое ФИО сотрудника
		/// </summary>
		public string ShortName
		{
			get => _shortName;
			set => SetProperty(ref _shortName, value);
		}
		private string _shortName;

		/// <summary>
		/// Смена
		/// </summary>
		public string NameShift
		{
			get => _nameShift;
			set => SetProperty(ref _nameShift, value);
		}
		private string _nameShift;

		/// <summary>
		/// Переработка
		/// </summary>
		public string NameOverday
		{
			get => _nameOverday;
			set => SetProperty(ref _nameOverday, value);
		}
		private string _nameOverday;
	}
}

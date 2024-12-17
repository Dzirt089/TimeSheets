using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Entitys.FAQ
{
	public class FaqModel : ObservableObject
	{
		/// <summary>
		/// Краткое обозначение
		/// </summary>
		public string? Symbol
		{
			get => _symbol;
			set => SetProperty(ref _symbol, value);
		}
		private string? _symbol;

		/// <summary>
		/// Описание
		/// </summary>
		public string? Description
		{
			get => _description;
			set => SetProperty(ref _description, value);
		}
		private string? _description;

		/// <summary>
		/// Дневные часы в смене
		/// </summary>
		public double DayHours
		{
			get => _dayHours;
			set => SetProperty(ref _dayHours, value);
		}
		private double _dayHours;

		/// <summary>
		/// Ночные часы в смене
		/// </summary>
		public double NightHours
		{
			get => _nightHours;
			set => SetProperty(ref _nightHours, value);
		}
		private double _nightHours;

		/// <summary>
		/// Кол-во часов в смене
		/// </summary>
		public string? HoursCount
		{
			get => _hoursCount;
			set => SetProperty(ref _hoursCount, value);
		}
		private string? _hoursCount;
	}
}

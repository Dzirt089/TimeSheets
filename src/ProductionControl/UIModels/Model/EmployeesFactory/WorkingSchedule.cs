using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.DataAccess.Classes.Models.Model;

namespace ProductionControl.Models.Entitys.EmployeesFactory
{
	/// <summary>
	/// Прокси класс (вспомогательный), через который преобразуется график с данными о сменах и часах из ИС-ПРО в понятную нам модель в приложении 
	/// </summary>
	public class WorkingSchedule : ObservableObject
	{
		/// <summary>Код графика</summary>
		public string? NumGraf
		{
			get => _numGraf;
			set
			{
				SetProperty(ref _numGraf, value);
				SetShift();
			}
		}
		private string? _numGraf;

		/// <summary>Период (показывает данные, за какой месяц и год)</summary>
		public DateTime PeriodDate { get; set; }

		/// <summary>Дата в которой есть смена</summary>
		public DateTime DateWithShift { get; set; }

		/// <summary>День со сменой</summary>
		public int DayInDateWithShift { get; set; }

		/// <summary>Кол-во часов ы в смене</summary>
		public double CountHoursWithShift { get; set; }

		/// <summary>Тип смены</summary>
		public short TypShift
		{
			get => _typShift;
			set
			{
				SetProperty(ref _typShift, value);
				SetShift();
			}
		}
		private short _typShift;

		/// <summary>Ночные часы в смене</summary>
		public double NightHoursWithShift { get; set; }

		/// <summary>Сопоставление смены из ИС-ПРО с табелем</summary>
		public ShiftType? Shift
		{
			get => _shift;
			set
			{
				SetProperty(ref _shift, value);
				HoursSchedule = Shift?.HoursCount ?? string.Empty;
				ShiftSchedule = Shift?.ShiftType ?? string.Empty;
			}
		}
		private ShiftType? _shift;

		/// <summary>
		/// Общее кол-во часов в смене
		/// </summary>
		public string HoursSchedule { get; private set; }

		/// <summary>
		/// Короткое обозначение смены ("н","1","Б", и т.д.)
		/// </summary>
		public string ShiftSchedule { get; private set; }

		/// <summary>
		/// Метод по преобразованию смены и номера графика из ИС-ПРО, к понятному нашему приложению формату.
		/// </summary>
		private void SetShift()
		{
			if (NumGraf != null && TypShift > 0)
			{
				Shift = NumGraf switch
				{
					_ when NumGraf == "1" => ShiftType.FirstShift,
					_ when NumGraf == "21" && TypShift == 1 => ShiftType.FirstShift,
					_ when NumGraf == "21" && TypShift == 2 => ShiftType.SecondShift,
					_ when NumGraf == "22" && TypShift == 1 => ShiftType.FirstShift,
					_ when NumGraf == "22" && TypShift == 2 => ShiftType.SecondShift,
					_ when (NumGraf == "3" || NumGraf == "4" ||
					NumGraf == "5" || NumGraf == "6") && TypShift == 1 => ShiftType.DayShift,
					_ when (NumGraf == "3" || NumGraf == "4" ||
					NumGraf == "5" || NumGraf == "6") && TypShift == 2 => ShiftType.NightShift,
					_ when NumGraf == "23" && TypShift == 1 => ShiftType.FirstShift,
					_ when NumGraf == "23" && TypShift == 2 => ShiftType.SecondShift,
					_ when NumGraf == "23" && TypShift == 3 => ShiftType.ThirdShift,
					_ when (NumGraf == "14" || NumGraf == "7" ||
					NumGraf == "8" || NumGraf == "9") && TypShift == 1 => ShiftType.Hours24,
					_ when NumGraf == "2" => ShiftType.Мoonlighting,
					_ when NumGraf == "25" => ShiftType.Hours7,
					_ when NumGraf == "26" => ShiftType.Hours5,
					_ => null
				};
			}
		}
	}
}

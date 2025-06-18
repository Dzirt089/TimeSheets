using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.DataAccess.Classes.ApiModels.Model;

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ProductionControl.UIModels.Dtos.EmployeesFactory
{
	/// <summary>
	/// Модель учёта времени отработанного на производстве. Включает в себя часы в смене, тип смены, время переработки\недоработки
	/// </summary>
	public class ShiftDataDto : ObservableObject
	{
		/// <summary>Табельный номер сотрудника</summary>
		public long EmployeeID
		{
			get => _employeeID;
			set => SetProperty(ref _employeeID, value);
		}
		private long _employeeID;

		/// <summary>Навигационное сво-во, сотрудник. Для связи с <see cref="EmployeeDto"/> один-ко-многим</summary>
		public EmployeeDto Employee
		{
			get => _employee;
			set => SetProperty(ref _employee, value);
		}
		private EmployeeDto _employee;

		/// <summary>
		/// Дата табеля
		/// </summary>
		public DateTime WorkDate
		{
			get => _workDate;
			set => SetProperty(ref _workDate, value);
		}
		private DateTime _workDate;

		/// <summary>
		/// Часы отработанные в смене, вкл переработки
		/// </summary>
		public string? Hours
		{
			get => _hours;
			set
			{
				if (Validation())
					SetProperty(ref _hours, value);
			}
		}
		private string? _hours;

		/// <summary>
		/// Тип смены на производстве
		/// </summary>
		public string? Shift
		{
			get => _shift;
			set
			{
				if (Validation())
				{
					SetProperty(ref _shift, value);

					//При пустом значении типа смены - значения Часов и Переработки обнуляем.
					if (string.IsNullOrEmpty(Shift))
					{
						Hours = string.Empty;
						return;
					}
					else if (Employee.IsDismissal)
						Overday = string.Empty;
					else
						SetOverdayAndShiftAndGetHours();

					Brush = Shift.GetBrush();
				}
			}
		}
		private string? _shift;

		/// <summary>
		/// Цвет для окраски ФИО сотрудника в приложении Табеля. Красный - если уволен в выбранном месяце. Во всех остальных случаях - черный
		/// </summary>
		[NotMapped]
		public Brush Brush { get => _brush; set => SetProperty(ref _brush, value); }
		private Brush _brush;

		/// <summary>
		/// Время переработки\недоработки
		/// </summary>
		public string? Overday
		{
			get => _overday;
			set
			{
				if (Validation())
				{
					if (!string.IsNullOrEmpty(Shift))
					{
						SetProperty(ref _overday, value);
						SetOverdayAndShiftAndGetHours();
					}
					else //При обнулении смены, обнуляем и переработку.				
						SetProperty(ref _overday, value);
				}
			}
		}
		private string? _overday;

		/// <summary>Флаг, который обозначает что сотрудник отобедал в этот день на производстве или нет.</summary>
		public bool IsHaveLunch
		{
			get => _isHaveLunch;
			set
			{
				SetProperty(ref _isHaveLunch, value);
			}
		}
		private bool _isHaveLunch;

		/// <summary>Флаг, который обозначает что этот день предпраздничный нет. Если да, то день на час короче. Инфа берется в апишке из ис-про</summary>
		public bool IsPreHoliday
		{
			get => _isPreHoliday;
			set
			{
				SetProperty(ref _isPreHoliday, value);
			}
		}
		private bool _isPreHoliday;

		/// <summary>
		/// Установка кол-ва отработанных часов, в зависимости от типа смены, наличия переработок или недоработок
		/// </summary>
		private void SetOverdayAndShiftAndGetHours()
		{

			var tempHoursString = CheckShiftAndGetHours(Shift);
			if (!string.IsNullOrEmpty(tempHoursString))
			{
				if (string.IsNullOrEmpty(Overday))
					Hours = tempHoursString;
				else
				{
					var tempHoursString2 = CheckOverdayAndGetHourse(Overday, tempHoursString);
					Hours = tempHoursString2;
				}

				if (IsPreHoliday && Hours.TryParseDouble(out double temp))
				{
					Hours = (temp - 1).ToString();
				}

			}
			else
				Hours = tempHoursString;
		}

		/// <summary>
		/// Проверка наличия и типа переработки или недоработки.
		/// Расчет кол-ва отработанных часов в зависимости от данных.
		/// </summary>
		/// <param name="overDay">Значение кол-ва часов для переработки\недоработки</param>
		/// <param name="hours">Значение кол-ва часов в смене</param>
		/// <returns>Отработанные часы</returns>
		private static string CheckOverdayAndGetHourse(string overDay, string hours)
		{
			var regex = @"(^[-]\d)";
			overDay = overDay.Replace(".", ",");
			var checking = Regex.IsMatch(overDay, regex);
			if (checking)
			{
				string minus = overDay.Replace("-", "");
				if (double.TryParse(minus, out double tempOverDouble) &&
						double.TryParse(hours, out double tempHoursDouble))
					return $"{tempHoursDouble - tempOverDouble}";
				else
					return hours;
			}
			else
			{
				if (double.TryParse(overDay, out double tempOverDouble) &&
						double.TryParse(hours, out double tempHoursDouble))
					return $"{tempHoursDouble + tempOverDouble}";
				else
					return hours;
			}
		}

		/// <summary>
		/// Проверяем и получаем кол-во часов в смене
		/// </summary>
		/// <param name="shiftValue">Тип смены. Если указали правильно, то получим часы в данной смене.</param>
		/// <returns>Пустая строка или кол-во часов в смене</returns>
		private static string CheckShiftAndGetHours(string shiftValue)
		{
			return shiftValue switch
			{
				_ when string.Equals(ShiftType.FirstShift.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.FirstShift.HoursCount,
				_ when string.Equals(ShiftType.SecondShift.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.SecondShift.HoursCount,
				_ when string.Equals(ShiftType.ThirdShift.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.ThirdShift.HoursCount,
				_ when string.Equals(ShiftType.NightShift.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.NightShift.HoursCount,
				_ when string.Equals(ShiftType.DayShift.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.DayShift.HoursCount,
				_ when string.Equals(ShiftType.BusinessTrip.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.BusinessTrip.HoursCount,
				_ when string.Equals(ShiftType.SickLeave.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.SickLeave.HoursCount,
				_ when string.Equals(ShiftType.Vacation.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Vacation.HoursCount,
				_ when string.Equals(ShiftType.NoShowUnknown.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.NoShowUnknown.HoursCount,
				_ when string.Equals(ShiftType.Hours24.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Hours24.HoursCount,
				_ when string.Equals(ShiftType.Мoonlighting.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Мoonlighting.HoursCount,
				_ when string.Equals(ShiftType.Hours7.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Hours7.HoursCount,
				_ when string.Equals(ShiftType.Hours5.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Hours5.HoursCount,
				_ when string.Equals(ShiftType.AdministrativeLeave.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.AdministrativeLeave.HoursCount,

				_ when string.Equals(ShiftType.AdministrativeLeavev2.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.AdministrativeLeavev2.HoursCount,
				_ when string.Equals(ShiftType.Demobilized.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.Demobilized.HoursCount,
				_ when string.Equals(ShiftType.ParentalLeave.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.ParentalLeave.HoursCount,
				_ when string.Equals(ShiftType.InvalidLeave.ShiftType, shiftValue, StringComparison.OrdinalIgnoreCase) => ShiftType.InvalidLeave.HoursCount,


				_ => string.Empty,
			};

		}

		/// <summary>
		/// метод валидации, где блокируется внесение данных в график сотрудника, если он уволен или ещё не принят на работу
		/// </summary>
		private bool Validation()
		{
			// Если Employee ещё не проинициализирован — считаем, что валидация не пройдена
			//if (Employee == null)
			//	return false;

			if (Employee.IsDismissal)
				return false;

			if (Employee.DateEmployment > WorkDate)
				return false;

			return true;
		}
	}
}

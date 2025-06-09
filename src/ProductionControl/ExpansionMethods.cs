using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.Models.Dtos.EmployeesFactory;
using ProductionControl.Models.Dtos.ExternalOrganization;

using System.Windows.Media;

namespace ProductionControl
{
	public static class ExpansionMethods
	{
		// <summary>
		/// Возвращает кисть (`Brush`), соответствующую указанной смене.
		/// </summary>
		/// <param name="shift">Строка, представляющая смену.</param>
		/// <returns>
		/// Возвращает `Brushes.DarkRed`, если смена содержит "3" или "н" (нечувствительно к регистру),
		/// `Brushes.DeepPink`, если смена содержит "2",
		/// и `Brushes.Black` для всех остальных значений или если строка пустая.
		/// </returns>
		public static Brush GetBrush(this string shift)
		{
			if (string.IsNullOrEmpty(shift)) return Brushes.Black;

			if (shift.Contains('3') || shift.Contains('н', StringComparison.OrdinalIgnoreCase))
				return Brushes.DarkRed;
			else if (shift.Contains('2'))
				return Brushes.DeepPink;
			else
				return Brushes.Black;
		}

		public static Brush GetBrush(this double hoursOverday)
		{
			if (hoursOverday > 0)
				return Brushes.Green;
			else if (hoursOverday < 0)
				return Brushes.Red;
			else
				return Brushes.Black;
		}

		public static Brush GetBrushARGB(this ShiftDataExOrgDto shiftData)
		{
			Brush brush = Brushes.Black;
			if (shiftData.CodeColor is null) return brush;
			else
			{
				return shiftData.CodeColor switch
				{
					1 => Brushes.Red,
					2 => Brushes.Black,
					_ => Brushes.Black
				};
			}
		}

		/// <summary>
		/// Валидация по сотрудникам. Пояснение:
		/// Уволенный в сентябре 2024 года сотрудник, 
		/// отображается на графике только до начала октября. 
		/// И трудоустроенный сотрудник не может отображаться в графике тогда, когда его не было
		/// </summary>
		/// <param name="employee">Данные сотрудника</param>
		/// <param name="months">Выбранный месяц в программе</param>
		/// <param name="years">Выбранный год в программе</param>
		/// <returns><see cref="True"/> если валидация прошла. Иначе - <see cref="False"/></returns>
		public static bool ValidateEmployee(
			this EmployeeDto employee,
			int months, int years)
		{
			if (employee.DateDismissal != DateTime.Parse("31.12.1876"))
			{
				if (employee.DateDismissal.Year < years)
					return false;
				else
				if (employee.DateDismissal.Month >= months
					&& employee.DateDismissal.Year <= years)
					return true;
				else
					return false;
			}
			else
			if (employee.DateEmployment.Month > months
				&& employee.DateEmployment.Year >= years)
				return false;
			else
				return true;
		}
		/// <summary>
		/// Валидация по сотрудникам. Пояснение:
		/// Уволенный в сентябре 2024 года сотрудник, 
		/// отображается на графике только до начала октября. 
		/// И трудоустроенный сотрудник не может отображаться в графике тогда, когда его не было
		/// </summary>
		/// <param name="employee">Данные сотрудника</param>
		/// <param name="months">Выбранный месяц в программе</param>
		/// <param name="years">Выбранный год в программе</param>
		/// <returns><see cref="True"/> если валидация прошла. Иначе - <see cref="False"/></returns>
		public static bool ValidateEmployee(
			this EmployeeExOrgDto employee,
			int months, int years)
		{
			if (employee.DateDismissal != DateTime.Parse("31.12.1876"))
			{
				if (employee.DateDismissal.Year < years)
					return false;
				else
				if (employee.DateDismissal.Month >= months
					&& employee.DateDismissal.Year <= years)
					return true;
				else
					return false;
			}
			else
			if (employee.DateEmployment.Month > months
				&& employee.DateEmployment.Year >= years)
				return false;
			else
				return true;
		}
		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataDto"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataDto"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDays(this ShiftDataDto shiftData)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataDto"/> допустимое значение сверхурочных часов (`Overday`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataDto"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Overday`,
		/// и значение `Overday` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationOverdayDays(this ShiftDataDto shiftData)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Overday) &&
				shiftData.Overday.Replace(".", ",").TryParseDouble(out _))
				return true;
			else
				return false;
		}
		/// <summary>
		/// Проверяет, соответствует ли смена требованиям для обеда.
		/// </summary>
		/// <param name="shiftsData">Коллекция данных смен.</param>
		/// <returns>Возвращает true, если смена соответствует требованиям для обеда, в противном случае - false.</returns>
		public static bool ValidationShiftForLunch(this IEnumerable<ShiftDataDto> shiftsData)
		{
			if (shiftsData.Count() == 1)
			{
				var itemShift = shiftsData.FirstOrDefault();
				if (itemShift is null) return false;

				if (itemShift.Hours.TryParseDouble(out _) &&
					!itemShift.Shift.Contains("К", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Проверяет, соответствует ли смена требованиям для ужина в столовой номер два.
		/// </summary>
		/// <param name="shiftsData">Коллекция данных смен.</param>
		/// <returns>Возвращает true, если смена соответствует требованиям для ужина, в противном случае - false.</returns>
		public static bool ValidationShiftDinnerForCafeteriaTwo(this IEnumerable<ShiftDataDto> shiftsData)
		{
			if (shiftsData.Count() == 1)
			{
				var itemShift = shiftsData.FirstOrDefault();
				if (itemShift is null) return false;

				if (itemShift.Shift.GetNightHoursBool() &&
					!itemShift.Shift.Contains("С", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Получает количество обедов в месяц.
		/// </summary>
		/// <param name="shiftsData">Коллекция данных смен.</param>
		/// <param name="isDismissal">Флаг, указывающий, если сотрудник уволен.</param>
		/// <returns>Возвращает количество обедов, если сотрудник уволен, возвращает отрицательное значение.</returns>
		public static int GetCountLunchInMonth(this IEnumerable<ShiftDataDto> shiftsData, bool isDismissal)
		{
			int count = shiftsData
				.Where(x => x.IsHaveLunch)
				.Count();
			return count > 0 ? isDismissal ? -count : count : 0;
		}

		/// <summary>
		/// Пробует преобразовать строку в число с плавающей точкой (double).
		/// </summary>
		/// <param name="str">Строка для преобразования.</param>
		/// <param name="value">Выходное значение типа double, если преобразование успешно.</param>
		/// <returns>Возвращает true, если преобразование прошло успешно; в противном случае - false.</returns>
		public static bool TryParseDouble(this string str, out double value)
		{
			return double.TryParse(str, out value);
		}

		/// <summary>
		/// Получаем часы отработанные в смене \ или в графе переработки если чел был выходной.
		/// </summary>
		/// <param name="hours">Строка с данными по кол-ву часов</param>
		/// <returns></returns>
		public static double GetCountHoursInShiftOrOverday(this string hours)
		{
			if (string.IsNullOrEmpty(hours)) return 0;
			else if (hours.TryParseDouble(out var value)) return value;
			else return 0;
		}

		/// <summary>
		/// Получаем из списка смен часы отработанные в смене \ 
		/// или в графе переработки если чел был выходной.
		/// </summary>
		public static double GetCountHoursInShiftOrOverday(this IEnumerable<ShiftDataDto> shiftsData)
		{
			if (shiftsData is null || shiftsData.Count() == 0) return 0;
			double count = 0;
			foreach (var item in shiftsData)
			{
				count += item.Hours.GetCountHoursInShiftOrOverday();
				count += item.Overday.GetCountHoursInShiftOrOverday();
			}
			return count;
		}

		/// <summary>
		/// Возвращает <see cref="True"/>, если стоит дата уволнения отличная от значения по-умолчанию
		/// </summary>
		/// <param name="employee"></param>
		/// <returns></returns>
		public static bool ValidateDateDismissal(this EmployeeDto employee)
		{
			if (employee.DateDismissal != DateTime.Parse("31.12.1876"))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Выполняет заданное действие для каждого элемента коллекции.
		/// </summary>
		/// <typeparam name="T">Тип элементов в коллекции.</typeparam>
		/// <param name="collection">Коллекция элементов.</param>
		/// <param name="Action">Действие, выполняемое для каждого элемента.</param>
		public static void Foreach<T>(this IEnumerable<T> collection, Action<T> Action)
		{
			if (collection is not T[] array)
			{
				if (collection is not List<T> list)
				{
					if (collection is IList<T> list2)
					{
						foreach (T item in list2)
						{
							Action(item);
						}
						return;
					}

					foreach (T item2 in collection)
					{
						Action(item2);
					}
					return;
				}

				foreach (T item3 in list)
				{
					Action(item3);
				}
				return;
			}

			T[] array2 = array;
			foreach (T obj in array2)
			{
				Action(obj);
			}
		}

		/// <summary>
		/// Добавляет элементы из одной коллекции в другую.
		/// </summary>
		/// <typeparam name="T">Тип элементов в коллекции.</typeparam>
		/// <param name="collection">Коллекция, в которую добавляются элементы.</param>
		/// <param name="items">Коллекция элементов для добавления.</param>
		public static void AddItems<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			ICollection<T> collection2 = collection;
			if (collection2 is not List<T> list)
			{
				items.Foreach(delegate (T item)
				{
					collection2.Add(item);
				});
			}
			else
			{
				list.AddRange(items);
			}
		}

		/// <summary>
		/// Проверяет, соответствует ли объект <see cref="ShiftDataDto"/> заданной дате и имеет ли он допустимое значение часов.
		/// </summary>
		/// <param name="shiftData">Объект `shiftData`, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Hours`, 
		/// значение `Hours` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDaysOnDate(this ShiftDataDto shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, соответствует ли объект <see cref="ShiftDataExOrgDto"/> заданной дате и имеет ли он допустимое значение часов.
		/// </summary>
		/// <param name="shiftData">Объект `ShiftDataExOrgDto`, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Hours`, 
		/// значение `Hours` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDaysOnDate(this ShiftDataExOrgDto shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, соответствует ли объект <see cref="ShiftDataDto"/> заданной дате и имеет ли он допустимое значение сверхурочных часов (`Overday`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataDto"/>, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Overday`, 
		/// значение `Overday` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationOverdayDaysOnDate(this ShiftDataDto shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Overday) &&
				shiftData.Overday.Replace(".", ",").TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataExOrgDto"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataExOrgDto"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDays(this ShiftDataExOrgDto shiftData)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataExOrgDto"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataExOrgDto"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static double GetWorkingDays(this ShiftDataExOrgDto shiftData)
		{
			if (shiftData is null) return 0;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _))
				return double.TryParse(shiftData.Hours, out double result) ? result : 0;
			else
				return 0;
		}

		/// <summary>
		/// Получает ID департамента для СО, у текущего пользователя, если есть права.
		/// </summary>
		/// <returns>ID департамента.</returns>
		public static string GetDepartmentAsync(this string machineName)
		{
			return machineName.ToLower() switch
			{
				"ceh11" => "048",
				"ceh10" => "049",
				"ceh20" => "049",
				"ceh06" => "044",
				"pdo05" => "045",
				"okad01" => "015",
				"ceh05" => "051",
				_ => string.Empty
			};
		}
	}
}

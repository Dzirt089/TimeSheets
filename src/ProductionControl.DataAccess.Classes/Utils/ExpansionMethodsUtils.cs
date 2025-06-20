﻿using ProductionControl.DataAccess.Classes.ApiModels.Model;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;


namespace ProductionControl.DataAccess.Classes.Utils
{
	public static class ExpansionMethodsUtils
	{
		/// <summary>
		/// Метод расширения для получения дневных часов из смен
		/// </summary>
		/// <param name="shift">тип смены</param>
		/// <returns>часы</returns>
		public static double GetDaysHours(this string shift)
		{
			if (string.IsNullOrEmpty(shift)) return 0;

			return shift switch
			{
				_ when string.Equals(ShiftType.FirstShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.FirstShift.DayHours,
				_ when string.Equals(ShiftType.SecondShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SecondShift.DayHours,
				_ when string.Equals(ShiftType.ThirdShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ThirdShift.DayHours,
				_ when string.Equals(ShiftType.DayShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.DayShift.DayHours,
				_ when string.Equals(ShiftType.NightShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NightShift.DayHours,
				_ when string.Equals(ShiftType.BusinessTrip.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.BusinessTrip.DayHours,
				_ when string.Equals(ShiftType.SickLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SickLeave.DayHours,
				_ when string.Equals(ShiftType.Vacation.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Vacation.DayHours,
				_ when string.Equals(ShiftType.NoShowUnknown.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NoShowUnknown.DayHours,
				_ when string.Equals(ShiftType.Hours24.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours24.DayHours,
				_ when string.Equals(ShiftType.Мoonlighting.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Мoonlighting.DayHours,
				_ when string.Equals(ShiftType.Hours7.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours7.DayHours,
				_ when string.Equals(ShiftType.Hours5.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours5.DayHours,
				_ when string.Equals(ShiftType.AdministrativeLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeave.DayHours,

				_ when string.Equals(ShiftType.AdministrativeLeavev2.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeavev2.DayHours,
				_ when string.Equals(ShiftType.Demobilized.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
					ShiftType.Demobilized.DayHours,
				_ when string.Equals(ShiftType.ParentalLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ParentalLeave.DayHours,
				_ when string.Equals(ShiftType.InvalidLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
					ShiftType.InvalidLeave.DayHours,

				_ => 0
			};
		}

		/// <summary>
		/// Метод расширения для получения ночных часов из смен
		/// </summary>
		/// <param name="shift">тип смены</param>
		/// <returns>часы</returns>
		public static double GetNightHours(this string shift)
		{
			if (string.IsNullOrEmpty(shift)) return 0;

			return shift switch
			{
				_ when string.Equals(ShiftType.FirstShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.FirstShift.NightHours,
				_ when string.Equals(ShiftType.SecondShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SecondShift.NightHours,
				_ when string.Equals(ShiftType.ThirdShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ThirdShift.NightHours,
				_ when string.Equals(ShiftType.DayShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.DayShift.NightHours,
				_ when string.Equals(ShiftType.NightShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NightShift.NightHours,
				_ when string.Equals(ShiftType.BusinessTrip.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.BusinessTrip.NightHours,
				_ when string.Equals(ShiftType.SickLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SickLeave.NightHours,
				_ when string.Equals(ShiftType.Vacation.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Vacation.NightHours,
				_ when string.Equals(ShiftType.NoShowUnknown.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NoShowUnknown.NightHours,
				_ when string.Equals(ShiftType.Hours24.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours24.NightHours,
				_ when string.Equals(ShiftType.Мoonlighting.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Мoonlighting.NightHours,
				_ when string.Equals(ShiftType.Hours7.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours7.NightHours,
				_ when string.Equals(ShiftType.Hours5.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours5.NightHours,
				_ when string.Equals(ShiftType.AdministrativeLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeave.NightHours,

				_ when string.Equals(ShiftType.AdministrativeLeavev2.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeavev2.NightHours,
				_ when string.Equals(ShiftType.Demobilized.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Demobilized.NightHours,
				_ when string.Equals(ShiftType.ParentalLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ParentalLeave.NightHours,
				_ when string.Equals(ShiftType.InvalidLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.InvalidLeave.NightHours,

				_ => 0
			};
		}

		/// <summary>
		/// Метод расширения для выяснения, есть ли ночные часы у смены
		/// </summary>
		/// <param name="shift">тип смены</param>
		/// <returns>часы</returns>
		public static bool GetNightHoursBool(this string shift)
		{
			if (string.IsNullOrEmpty(shift)) return false;

			var hours = shift switch
			{
				_ when string.Equals(ShiftType.FirstShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.FirstShift.NightHours,
				_ when string.Equals(ShiftType.SecondShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SecondShift.NightHours,
				_ when string.Equals(ShiftType.ThirdShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ThirdShift.NightHours,
				_ when string.Equals(ShiftType.DayShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.DayShift.NightHours,
				_ when string.Equals(ShiftType.NightShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NightShift.NightHours,
				_ when string.Equals(ShiftType.BusinessTrip.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.BusinessTrip.NightHours,
				_ when string.Equals(ShiftType.SickLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.SickLeave.NightHours,
				_ when string.Equals(ShiftType.Vacation.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Vacation.NightHours,
				_ when string.Equals(ShiftType.NoShowUnknown.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.NoShowUnknown.NightHours,
				_ when string.Equals(ShiftType.Hours24.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours24.NightHours,
				_ when string.Equals(ShiftType.Мoonlighting.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Мoonlighting.NightHours,
				_ when string.Equals(ShiftType.Hours7.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours7.NightHours,
				_ when string.Equals(ShiftType.Hours5.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Hours5.NightHours,
				_ when string.Equals(ShiftType.AdministrativeLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeave.NightHours,

				_ when string.Equals(ShiftType.AdministrativeLeavev2.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.AdministrativeLeavev2.NightHours,
				_ when string.Equals(ShiftType.Demobilized.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.Demobilized.NightHours,
				_ when string.Equals(ShiftType.ParentalLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.ParentalLeave.NightHours,
				_ when string.Equals(ShiftType.InvalidLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				ShiftType.InvalidLeave.NightHours,

				_ => 0
			};

			if (hours > 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Получаем кол-во часов в смене 
		/// </summary>
		/// <param name="shift">тип смены</param>
		/// <returns>часы</returns>
		public static double GetShiftHours(this string shift)
		{
			if (string.IsNullOrEmpty(shift)) return 0;

			return shift switch
			{
				_ when string.Equals(ShiftType.FirstShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.FirstShift.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.SecondShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.SecondShift.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.ThirdShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.ThirdShift.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.DayShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.DayShift.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.NightShift.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.NightShift.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.BusinessTrip.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.BusinessTrip.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.SickLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.SickLeave.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Vacation.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Vacation.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.NoShowUnknown.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.NoShowUnknown.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Hours24.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Hours24.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Мoonlighting.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Мoonlighting.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Hours7.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Hours7.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Hours5.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Hours5.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.AdministrativeLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.AdministrativeLeave.HoursCount, out double res) ? res : 0,

				_ when string.Equals(ShiftType.AdministrativeLeavev2.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.AdministrativeLeavev2.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.Demobilized.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.Demobilized.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.ParentalLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.ParentalLeave.HoursCount, out double res) ? res : 0,
				_ when string.Equals(ShiftType.InvalidLeave.ShiftType, shift, StringComparison.OrdinalIgnoreCase) =>
				double.TryParse(ShiftType.InvalidLeave.HoursCount, out double res) ? res : 0,

				_ => 0
			};
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
			this Employee employee,
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
			this EmployeeExOrg employee,
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
		/// Проверяет, имеет ли объект <see cref="ShiftsDatum"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftsDatum"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'ShiftsDatum' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDays(this ShiftData shiftData)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftData"/> допустимое значение сверхурочных часов (`Overday`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftData"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Overday`,
		/// и значение `Overday` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationOverdayDays(this ShiftData shiftData)
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
		public static bool ValidationShiftForLunch(this IEnumerable<ShiftData> shiftsData)
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
		public static bool ValidationShiftDinnerForCafeteriaTwo(this IEnumerable<ShiftData> shiftsData)
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
		public static int GetCountLunchInMonth(this IEnumerable<ShiftData> shiftsData, bool isDismissal)
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
		public static double GetCountHoursInShiftOrOverday(this IEnumerable<ShiftData> shiftsData)
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
		public static bool ValidateDateDismissal(this Employee employee)
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
		/// Проверяет, соответствует ли объект <see cref="ShiftData"/> заданной дате и имеет ли он допустимое значение часов.
		/// </summary>
		/// <param name="shiftData">Объект `ShiftData`, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Hours`, 
		/// значение `Hours` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDaysOnDate(this ShiftData shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, соответствует ли объект <see cref="ShiftDataExOrg"/> заданной дате и имеет ли он допустимое значение часов.
		/// </summary>
		/// <param name="shiftData">Объект `ShiftData`, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Hours`, 
		/// значение `Hours` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDaysOnDate(this ShiftDataExOrg shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, соответствует ли объект <see cref="ShiftData"/> заданной дате и имеет ли он допустимое значение сверхурочных часов (`Overday`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftData"/>, содержащий данные о рабочем дне.</param>
		/// <param name="date">Дата, которую необходимо проверить.</param>
		/// <returns>
		/// Возвращает `true`, если `shiftData` не является `null`, содержит не пустое значение `Overday`, 
		/// значение `Overday` можно преобразовать в `double`, и дата в `shiftData.WorkDate` совпадает с указанной датой `date`.
		/// В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationOverdayDaysOnDate(this ShiftData shiftData, DateTime date)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Overday) &&
				shiftData.Overday.Replace(".", ",").TryParseDouble(out _) &&
				shiftData.WorkDate == date) return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataExOrg"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataExOrg"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static bool ValidationWorkingDays(this ShiftDataExOrg shiftData)
		{
			if (shiftData is null) return false;

			if (!string.IsNullOrEmpty(shiftData.Hours) &&
				shiftData.Hours.TryParseDouble(out _))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Проверяет, имеет ли объект <see cref="ShiftDataExOrg"/> допустимое значение рабочих часов (`Hours`).
		/// </summary>
		/// <param name="shiftData">Объект <see cref="ShiftDataExOrg"/>, содержащий данные о рабочем дне.</param>
		/// <returns>
		/// Возвращает `true`, если 'shiftData' не является `null`, содержит не пустое значение `Hours`,
		/// и значение `Hours` можно преобразовать в `double`. В противном случае возвращает `false`.
		/// </returns>
		public static double GetWorkingDays(this ShiftDataExOrg shiftData)
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

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Models.Dtos;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	/// <summary>
	/// Сервис для получения данных по сотрудникам предприятия за месяц,
	/// формирования сводных данных и генерации отчёта.
	/// </summary>
	public class MonthlySummaryService(
		IEmployeesFactorysRepository contextServices,
		IErrorLogger errorLogger,
		IReportService report) : IMonthlySummaryService
	{
		/// <summary>
		/// Список всех сотрудников, загруженных для построения отчёта.
		/// </summary>
		public List<Employee> AllPeople { get; private set; }

		/// <summary>
		/// Загружает данные по сотрудникам за указанный месяц и год, 
		/// заполняет DTO со сводкой по дням и по итоговым часам, 
		/// генерирует файл отчёта и отправляет его по почте.
		/// </summary>
		/// <param name="month">Месяц отчёта (1–12).</param>
		/// <param name="year">Год отчёта.</param>
		/// <param name="token">Токен отмены операции.</param>
		/// <returns>Задача без результата. Исключения логируются внутри.</returns>
		public async Task GetDataForMonthlySummary(int month, int year, CancellationToken token)
		{
			try
			{
				// 1) Устанавливаем даты на начало и конец прогнозируемого месяца
				var startDate = new DateTime(year: year, month: month, day: 1);
				var endDate = new DateTime(year: year, month: month, day: DateTime.DaysInMonth(year: year, month: month));

				StartEndDateTime startEndDate = new StartEndDateTime { StartDate = startDate, EndDate = endDate };

				// 2) Загружаем список сотрудников за период
				AllPeople = await contextServices.GetEmployeesAsync(startEndDate, token);

				if (AllPeople == null || AllPeople.Count == 0)
					return; // если нет данных — выходим

				// 3) Проводим валидацию сотрудников на обстоятельства: уволнение и приёма на работу. 
				//Например: Уволенный в июне 2020 сотрудник не должен быть в табеле в июле 2024.
				//И нанятый новый человек в августе 2024 не должен быть в табеле 2023 года.
				AllPeople = AllPeople.Where(x => x.ValidateEmployee(month, year)).ToList();

				// 4) Подготовим список DTO для итогового отчёта
				List<MonthlySummaryDto> monthlySummaries = [];

				// 5) По каждому сотруднику формируем сводку
				foreach (var employee in AllPeople.OrderBy(x => x.ShortName))
				{
					// a) Словарь смен по дате для быстрого доступа
					var shiftDict = employee.Shifts?.ToDictionary(x => x.WorkDate) ??
						new Dictionary<DateTime, ShiftData>();

					// b) Новый пустой DTO на сотрудника
					var summary = new MonthlySummaryDto();

					// c) Проходим по каждому дню месяца
					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						// Если записи нет — создаём "пустую" смену
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftData
							{
								EmployeeID = employee.EmployeeID,
								WorkDate = date,
								Employee = employee,
								Hours = string.Empty,
							};
						}
						// d) Записываем часы в соответствующее свойство Day1…Day31
						SetDayHours(summary, date.Day, shiftDict[date].Hours);

					}

					// e) Сохраняем обратно список смен (на случай, если где-то изменили)
					employee.Shifts = shiftDict.Values.ToList();

					// f) Заполняем поля DTO по сотруднику и месяцу
					summary.Year = year;
					summary.MonthName = GetMonthName(month);
					summary.EmployeeID = employee.EmployeeID;
					summary.ShortName = employee.ShortName;
					summary.DepartmentID = int.TryParse(employee.DepartmentID, out int res) ? res : 0;
					summary.NameDepartment = employee.DepartmentProduction?.NameDepartment ?? string.Empty;

					// 6) Подсчет количества предпраздничных дней
					summary.CountPreholiday = employee.Shifts
						.Where(x => x.IsPreHoliday == true
						 && x.Shift != null && x.Shift.GetShiftHours() != 0)
						.Count();

					// 7) Определяем, дневная или ночная смена у работника
					bool daysShift = false, nightShift = false;
					var shift = employee.Shifts
						.Where(x => x.IsPreHoliday)
						.Select(s => s.Shift)
						.FirstOrDefault();

					if (int.TryParse(shift, out int numberShift))
					{
						switch (numberShift)
						{
							case 1: daysShift = true; break;
							case 2: nightShift = true; break;
							case 3: nightShift = true; break;
							case 4: daysShift = true; break;
							case 5: daysShift = true; break;
							case 7: daysShift = true; break;
						}
					}

					// Подсчет общего количества рабочих дней
					summary.TotalWorksDays = employee.Shifts.Where(x => x.ValidationWorkingDays()).Count();

					// Подсчет общего количества рабочих часов без сверхурочных
					var tempTotalWorksHoursWithoutOverday = employee.Shifts
						.AsParallel()
						.Sum(x => x.Shift?.GetShiftHours() ?? 0);
					var tempCheckTWHWO = Math.Round(tempTotalWorksHoursWithoutOverday, 1) - summary.CountPreholiday;
					summary.TotalWorksHoursWithoutOverday = tempCheckTWHWO < 0 ? 0 : tempCheckTWHWO;


					// Подсчет общего количества сверхурочных часов
					var tempTotalOverHours = employee.Shifts
						.AsParallel()
						.Where(e => e.ValidationOverdayDays())
						.Sum(r => double.TryParse(r.Overday?.Replace(".", ","), out double tempValue) ?
						tempValue : 0);
					summary.TotalOverdayHours = Math.Round(tempTotalOverHours, 1);

					// Подсчет общего количества рабочих часов с учетом сверхурочных
					summary.TotalWorksHoursWithOverday = Math.Round(summary.TotalWorksHoursWithoutOverday + summary.TotalOverdayHours, 1);

					// Подсчет общего количества ночных рабочих часов
					var tempTotalNightHours = employee.Shifts
						.AsParallel()
						.Where(x => x.ValidationWorkingDays())
						.Sum(y => y.Shift?.GetNightHours() ?? 0);

					if (nightShift)
						tempTotalNightHours -= summary.CountPreholiday;

					summary.TotalNightHours = Math.Round(tempTotalNightHours, 1);


					// Подсчет общего количества дневных рабочих часов
					var tempTotalDaysHours = employee.Shifts
						.AsParallel()
						.Where(x => x.ValidationWorkingDays())
						.Sum(x => x.Shift?.GetDaysHours() ?? 0);

					var tempCheckTDH = Math.Round(tempTotalDaysHours, 1);

					if (daysShift)
						tempCheckTDH -= summary.CountPreholiday;

					summary.TotalDaysHours = tempCheckTDH < 0 ? 0 : tempCheckTDH;

					monthlySummaries.Add(summary);
				}

				var path = await report.CreateReportForMonthlySummaryAsync(monthlySummaries, token);

				await errorLogger.SendMailReportMonthlySummaryAsync(path);
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
			}
		}

		/// <summary>
		/// Возвращает название месяца на русском языке по его номеру.
		/// </summary>
		private static string GetMonthName(int month) => month switch
		{
			1 => "Январь",
			2 => "Февраль",
			3 => "Март",
			4 => "Апрель",
			5 => "Май",
			6 => "Июнь",
			7 => "Июль",
			8 => "Август",
			9 => "Сентябрь",
			10 => "Октябрь",
			11 => "Ноябрь",
			12 => "Декабрь",
			_ => string.Empty
		};

		/// <summary>
		/// Устанавливает значение часов в нужное свойство Day1…Day31 DTO.
		/// </summary>
		private static void SetDayHours(MonthlySummaryDto dto, int day, string hours)
		{
			switch (day)
			{
				case 1: dto.Day1 = hours; break;
				case 2: dto.Day2 = hours; break;
				case 3: dto.Day3 = hours; break;
				case 4: dto.Day4 = hours; break;
				case 5: dto.Day5 = hours; break;
				case 6: dto.Day6 = hours; break;
				case 7: dto.Day7 = hours; break;
				case 8: dto.Day8 = hours; break;
				case 9: dto.Day9 = hours; break;
				case 10: dto.Day10 = hours; break;
				case 11: dto.Day11 = hours; break;
				case 12: dto.Day12 = hours; break;
				case 13: dto.Day13 = hours; break;
				case 14: dto.Day14 = hours; break;
				case 15: dto.Day15 = hours; break;
				case 16: dto.Day16 = hours; break;
				case 17: dto.Day17 = hours; break;
				case 18: dto.Day18 = hours; break;
				case 19: dto.Day19 = hours; break;
				case 20: dto.Day20 = hours; break;
				case 21: dto.Day21 = hours; break;
				case 22: dto.Day22 = hours; break;
				case 23: dto.Day23 = hours; break;
				case 24: dto.Day24 = hours; break;
				case 25: dto.Day25 = hours; break;
				case 26: dto.Day26 = hours; break;
				case 27: dto.Day27 = hours; break;
				case 28: dto.Day28 = hours; break;
				case 29: dto.Day29 = hours; break;
				case 30: dto.Day30 = hours; break;
				case 31: dto.Day31 = hours; break;
			}
		}
	}
}

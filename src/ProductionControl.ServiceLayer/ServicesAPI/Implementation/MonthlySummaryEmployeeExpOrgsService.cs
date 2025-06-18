using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

using System.Text;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	/// <summary>
	/// Сервис для формирования ежемесячной сводки по отработанным часам 
	/// сотрудников внешних организаций и отправки отчёта по электронной почте.
	/// </summary>
	public class MonthlySummaryEmployeeExpOrgsService(
		IEmployeesExternalOrganizationsRepository services,
		IErrorLogger logger,
		IReportService report) : IMonthlySummaryEmployeeExpOrgsService
	{
		private IEmployeesExternalOrganizationsRepository _services = services;
		private IErrorLogger _logger = logger;
		private IReportService _report = report;

		/// <summary>
		/// Генерирует отчёт по сотрудникам внешних организаций за указанный период,
		/// сохраняет его через IReportService и отправляет ссылку на файл по почте.
		/// </summary>
		/// <param name="_startPeriodString">Дата начала в формате строки (например, "2025-06-01").</param>
		/// <param name="_endPeriodString">Дата окончания в формате строки (например, "2025-06-30").</param>
		/// <param name="token">Токен отмены асинхронной операции.</param>
		/// <returns>
		/// true, если отчёт успешно создан и отправлен; 
		/// false в случае ошибок парсинга дат, отсутствия данных или внутренних исключений.
		/// </returns>
		public async Task<bool> CreateReportEmployeeExpOrgAsync(StartEndDateTime startEndDate, CancellationToken token)
		{
			try
			{
				// Парсим входные строки в DateTime. Если не получается — возвращаем false.
				var startPeriod = startEndDate.StartDate;
				var endPeriod = startEndDate.EndDate;

				// Определяем первые и последние дни месяца по дате начала
				int month = startPeriod.Month;
				int year = startPeriod.Year;
				DateTime startDate = new(year, month, 1);
				DateTime endDate = new(year, month, DateTime.DaysInMonth(year, month));

				// Собираем итоговые DTO для отчёта и накопительные часы по участкам
				List<EmployeesExOrgForReportDto> orgForReports = [];
				Dictionary<string, double> departmentAllHoursDict = [];

				StartEndDateTime startEndDateTime = new StartEndDateTime
				{
					StartDate = startDate,
					EndDate = endDate
				};

				// Загружаем данные по сменам сотрудников за период
				var employeeExOrgs = await _services.GetEmployeeExOrgsAsync(startEndDateTime, token);

				// Фильтруем по валидности данных (метод ValidateEmployee проверяет, что у сотрудника есть необходимые данные)
				employeeExOrgs = employeeExOrgs
					.Where(x => x.ValidateEmployee(startDate.Month, startDate.Year))
					.ToList();

				// Если нет валидных сотрудников — прерываем
				if (employeeExOrgs.Count == 0) return false;

				// Собираем список уникальных отделов (DepartmentID) среди всех сотрудников
				List<string> departmentsId = employeeExOrgs
					.SelectMany(x => x.EmployeeExOrgAddInRegions)
					.Where(x => !string.IsNullOrEmpty(x.DepartmentID))
					.Select(x => x.DepartmentID)
					.Distinct()
					.ToList();

				// Если ни одного отдела нет — нечего отчётывать
				if (departmentsId.Count == 0) return false;

				// Для каждого сотрудника и для каждого отдела считаем часы по дням
				foreach (var employee in employeeExOrgs)
				{
					// Преобразуем список смен в словарь для быстрого доступа по дате и отделу
					var shiftDict = employee.ShiftDataExOrgs?.ToDictionary(x => (x.WorkDate, x.DepartmentID)) ?? [];

					foreach (var departament in departmentsId)
					{
						double sumHours = 0;    // всего часов за месяц
						double sumHoursPeriod = 0;  // часов в рамках строго указанного периода

						// Создаём новый DTO для формирования отчёта
						EmployeesExOrgForReportDto? empOrgForRep = new()
						{
							EmployeeExOrgID = employee.EmployeeExOrgID
						};

						// Проходим по каждому дню месяца и заполняем DTO
						for (var date = startDate; date <= endDate; date = date.AddDays(1))
						{
							// Если для конкретного дня/отдела нет записи — создаём пустую
							if (!shiftDict.ContainsKey((date, departament)))
								shiftDict[(date, departament)] = new ShiftDataExOrg
								{
									EmployeeExOrgID = employee.EmployeeExOrgID,
									EmployeeExOrg = employee,
									DepartmentID = departament,
									WorkDate = date,
									Hours = string.Empty
								};

							var hours = shiftDict[(date, departament)].Hours;

							// Устанавливаем свойство DayN в DTO (где N — число дня)
							SetHoursByDay(empOrgForRep, date, hours);

							// Если удалось распарсить часы в число — учитываем суммы
							if (hours.TryParseDouble(out double res))
							{
								sumHours += res;
								if (date.Day >= startPeriod.Day && date.Day <= endPeriod.Day)
									sumHoursPeriod += res;
							}
						}

						// Накопление часов по отделу для всего отчёта
						if (!departmentAllHoursDict.ContainsKey(departament))
							departmentAllHoursDict[departament] = sumHoursPeriod;
						else
							departmentAllHoursDict[departament] += sumHoursPeriod;

						// Заполняем остальные поля DTO
						empOrgForRep.SumHours = sumHours.ToString();
						empOrgForRep.Year = startDate.Year;
						empOrgForRep.MonthName = GetMonthName(startDate.Month);
						empOrgForRep.NumCategory = employee.NumCategory;
						empOrgForRep.DepartmentID = departament;
						empOrgForRep.FullName = employee.FullName;

						orgForReports.Add(empOrgForRep);
					}
				}

				// Оставляем только тех сотрудников, у которых есть хоть какие-то часы
				List<EmployeesExOrgForReportDto> sortedOrgForReports = orgForReports
				.Where(x => !string.IsNullOrEmpty(x.SumHours) &&
						x.SumHours.TryParseDouble(out double re) && re > 0)
						.ToList();

				if (sortedOrgForReports.Count == 0) return false;

				// Генерируем файл отчёта через внешний сервис
				var path = await _report.CreateReportForMonthlySummaryEmployeeExpOrgAsync(sortedOrgForReports, startPeriod, endPeriod, token);

				// Формируем текст письма с резюме по участкам
				string message = ConfigTextMail(startPeriod, endPeriod, departmentAllHoursDict);

				if (!string.IsNullOrEmpty(path))
				{
					// Отправляем письмо с прикреплённым файлом
					await _logger.SendMailTestAsync([path], message);
					return true;
				}
				else
					return false;
			}
			catch (Exception ex)
			{
				// Логируем любую неожиданную ошибку и возвращаем false
				await _logger.ProcessingErrorLogAsync(ex);
				return false;
			}
		}

		/// <summary>
		/// Устанавливает свойство DayN в объекте DTO в зависимости от номера дня.
		/// </summary>
		/// <param name="empOrgForRep">DTO для заполнения.</param>
		/// <param name="date">Текущая дата.</param>
		/// <param name="hours">Строка с отработанными часами.</param>
		private static void SetHoursByDay(EmployeesExOrgForReportDto empOrgForRep, DateTime date, string? hours)
		{
			// Для каждого дня месяца присваиваем соответствующее свойство Day1, Day2, … Day31
			switch (date.Day)
			{
				case 1: empOrgForRep.Day1 = hours; break;
				case 2: empOrgForRep.Day2 = hours; break;
				case 3: empOrgForRep.Day3 = hours; break;
				case 4: empOrgForRep.Day4 = hours; break;
				case 5: empOrgForRep.Day5 = hours; break;
				case 6: empOrgForRep.Day6 = hours; break;
				case 7: empOrgForRep.Day7 = hours; break;
				case 8: empOrgForRep.Day8 = hours; break;
				case 9: empOrgForRep.Day9 = hours; break;
				case 10: empOrgForRep.Day10 = hours; break;
				case 11: empOrgForRep.Day11 = hours; break;
				case 12: empOrgForRep.Day12 = hours; break;
				case 13: empOrgForRep.Day13 = hours; break;
				case 14: empOrgForRep.Day14 = hours; break;
				case 15: empOrgForRep.Day15 = hours; break;
				case 16: empOrgForRep.Day16 = hours; break;
				case 17: empOrgForRep.Day17 = hours; break;
				case 18: empOrgForRep.Day18 = hours; break;
				case 19: empOrgForRep.Day19 = hours; break;
				case 20: empOrgForRep.Day20 = hours; break;
				case 21: empOrgForRep.Day21 = hours; break;
				case 22: empOrgForRep.Day22 = hours; break;
				case 23: empOrgForRep.Day23 = hours; break;
				case 24: empOrgForRep.Day24 = hours; break;
				case 25: empOrgForRep.Day25 = hours; break;
				case 26: empOrgForRep.Day26 = hours; break;
				case 27: empOrgForRep.Day27 = hours; break;
				case 28: empOrgForRep.Day28 = hours; break;
				case 29: empOrgForRep.Day29 = hours; break;
				case 30: empOrgForRep.Day30 = hours; break;
				case 31: empOrgForRep.Day31 = hours; break;

			}
		}

		/// <summary>
		/// Формирует HTML-сообщение для письма с таблицей итоговых часов и среднесписочного штата.
		/// </summary>
		/// <param name="startPeriod">Начало учётного периода.</param>
		/// <param name="endPeriod">Конец учётного периода.</param>
		/// <param name="departmentAllHoursDict">
		/// Словарь с суммарными часами для каждого отдела.
		/// </param>
		/// <returns>Строка с HTML-контентом письма.</returns>
		private static string ConfigTextMail(DateTime startPeriod, DateTime endPeriod, Dictionary<string, double> departmentAllHoursDict)
		{
			// Формирование HTML-сообщения с результатами
			int count = 0;
			double summa = 0;

			// Начинаем формировать сообщение с заголовка
			// Используем StringBuilder для эффективной конкатенации строк
			var message = new StringBuilder();
			message.Append($"За период с {startPeriod:d} по {endPeriod:d}\n");

			message.Append($"<table border='1' cols='{departmentAllHoursDict.Count + 1}' style='font-family:\"Courier New\", Courier, monospace'>");
			message.Append($"<tr>");
			message.Append($"<td style='padding:5px'>Участок</td>");
			message.Append($"<td style='padding:5px'>Часы</td>");
			message.Append($"<td style='padding:5px'>Кол-во человек(среднесписочно)</td>");
			message.Append($"<tr>");

			// Строки по каждому участку
			foreach (var item in departmentAllHoursDict)
			{
				summa += Math.Round(item.Value, 1);

				// Примерный расчёт среднесписочного: (часов / 11.8 / 5)
				double result = Math.Round(item.Value / 11.8 / 5);
				count += (int)result;

				message.Append($"<td style='padding:5px'>{item.Key}</td>");
				message.Append($"<td style='padding:5px'>{Math.Round(item.Value, 1)}</td>");
				message.Append($"<td style='padding:5px'>{(int)result}</td>");
				message.Append($"<tr>");
			}

			// Итоговая строка
			message.Append($"<td style='padding:5px'>Итого:</td>");
			message.Append($"<td style='padding:5px'>{summa}</td>");
			message.Append($"<td style='padding:5px'>{count}</td>");

			message.Append($"</table>");
			return message.ToString();
		}

		/// <summary>
		/// Возвращает название месяца на русском по его номеру.
		/// </summary>
		/// <param name="month">Номер месяца (1–12).</param>
		/// <returns>Строка с названием месяца.</returns>
		private static string GetMonthName(int month) =>
			month switch
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
	}
}

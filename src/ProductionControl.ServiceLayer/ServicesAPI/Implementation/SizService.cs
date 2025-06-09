using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.DataAccess.Sql.Interfaces;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	public class SizService(
		ISizsRepository contextServices,
		IReportService report,
		IMonthlyValuesService valuesService,
		IErrorLogger errorLogger,
		IDbServices dbServices)
		: ISizService
	{
		#region Property
		/// <summary>
		/// Фактически отработанное время сотрудника за определенный период
		/// </summary>
		public double FactHoursInShiftAndOverday { get; private set; }

		/// <summary>
		/// Список СИЗ-ов со всеми данными по ним (арт., имя, е.и., норма СИЗ и т.д.)
		/// </summary>
		public List<SizUsageRate>? ListAllSizs { get; private set; }

		/// <summary>
		/// Список выданных СИЗ-ов на сотрудника с первого числа месяца
		/// </summary>
		public List<DataSizForMonth>? ListSizsOutputWithOneDayMonth { get; private set; }

		/// <summary>
		/// Временный список СИЗ, положенных сотруднику
		/// </summary>
		public List<SizUsageRate>? TempListSiz { get; private set; }

		/// <summary>
		/// Норма рабочих часов в месяце
		/// </summary>
		public double HoursInMonth { get; private set; }

		/// <summary>
		/// Текущий месяц
		/// </summary>
		public int MonthCurrent { get; private set; }

		/// <summary>
		/// Текущий год
		/// </summary>
		public int YearCurrent { get; private set; }

		/// <summary>
		/// Начало периода расчета
		/// </summary>
		public DateTime StartDate { get; private set; }

		/// <summary>
		/// Конец периода расчета
		/// </summary>
		public DateTime EndDate { get; private set; }

		/// <summary>
		/// Список сотрудников, которым положены СИЗ
		/// </summary>
		public List<Employee>? AllPeople { get; private set; }

		/// <summary>
		/// Список сотрудников с данными для инициализации СИЗ
		/// </summary>
		public List<EmployeeForSizDto>? EmployeesForSizsInit { get; private set; }

		/// <summary>
		/// Фактически отработанные часы
		/// </summary>
		public double ActualTotalHoursLive { get; private set; }

		/// <summary>
		/// Минимум часов в процентном соотношении
		/// </summary>
		public double MinPercent { get; private set; }

		/// <summary>
		/// Минимум часов в числовом значении
		/// </summary>
		public int MinHourse { get; private set; }

		/// <summary>
		/// Остаток срока службы СИЗ при прогнозе, что сотрудник также отработает и вторую половину месяца
		/// </summary>
		public double Balance { get; private set; }

		/// <summary>
		/// Предыдущий месяц
		/// </summary>
		public int LastMonht { get; private set; }

		/// <summary>
		/// Предыдущий/текущий год
		/// </summary>
		public int LastYear { get; private set; }

		/// <summary>
		/// Аналитические данные (double)
		/// </summary>
		public double Analitics { get; private set; }

		/// <summary>
		/// Аналитические данные (int)
		/// </summary>
		public int AnaliticsCount { get; private set; }
		public List<EmployeeForSizDto> CompletedEmployeesSizs { get; private set; }

		/// <summary>Данные для номера ведомости: с порядковым номером, месяцем и годом.</summary>
		public OrderNumberOnDate MonthlyValueCurrent { get; private set; }
		#endregion

		/// <summary>
		/// Распределение логики выдачи СИЗ-ов
		/// </summary>
		/// <returns></returns>
		public async Task DivisionLogikalCalcSizAsync(CancellationToken token)
		{
			CompletedEmployeesSizs = [];

			MonthlyValueCurrent = await valuesService.GetValuesAsync(token);

			if (DateTime.Now.Day == 1 || DateTime.Now.Day == 2)
			{
				ListSizsOutputWithOneDayMonth = await contextServices.GetAllDataSizForMonthsAsync(token);

				if (ListSizsOutputWithOneDayMonth is null || ListSizsOutputWithOneDayMonth.Count == 0)
					CompletedEmployeesSizs = await CalculateSizForOneDayInMonthWithoutBalanceAsync(token);
				else
					CompletedEmployeesSizs = await CalculateSizForNewMonthWithBalanceAsync(token);
			}

			if (DateTime.Now.Day == 14 || DateTime.Now.Day == 15)
			{
				ListSizsOutputWithOneDayMonth = await contextServices.GetAllDataSizForMonthsAsync(token);

				if (ListSizsOutputWithOneDayMonth is null || ListSizsOutputWithOneDayMonth.Count == 0)
					CompletedEmployeesSizs = await CalculateSizForOneDayInMonthWithoutBalanceAsync(token);
				else
					CompletedEmployeesSizs = await CalculateSizForFifteenDayWithBalanceAsync(token);
			}
			if (CompletedEmployeesSizs is null || CompletedEmployeesSizs.Count == 0)
				return;

			string path = string.Empty;

			bool checkOne = await SetMonthlyValueBeforeReport(token);
			if (checkOne)
				path = await report.CreateReportForSIZAsync(CompletedEmployeesSizs, token);

			bool checkTwo = await RunTNOInBestAsync(CompletedEmployeesSizs, token);

			if (checkOne && checkTwo)
			{
				var mail = new MailerVKT.MailParameters();
				if (!string.IsNullOrEmpty(path))
				{
					mail = new MailerVKT.MailParameters
					{
						Recipients = ["teho19@vkt-vent.ru", "ok@vkt-vent.ru", "pdo03@vkt-vent.ru"],
						Subject = "Ведомости по выдаче СИЗ",
						SenderName = "Табель",
						Attachs = [path],
					};
				}
				else
				{
					mail = new MailerVKT.MailParameters
					{
						Recipients = ["teho19@vkt-vent.ru", "ok@vkt-vent.ru", "pdo03@vkt-vent.ru"],
						Subject = "Ведомости по выдаче СИЗ",
						SenderName = "Табель",
						Text = "Ошибка в формировании, нет файла ведомости. Сообщите разработчикам!"
					};
				}

				await errorLogger.SendMailReportAsync(mail);
			}
		}
		public async Task<bool> SetMonthlyValueBeforeReport(CancellationToken token)
		{
			var listDepartment = CompletedEmployeesSizs
				.Select(x => x.DepartmentID)
				.Distinct()
				.ToList() ?? [];

			foreach (var item in listDepartment)
			{
				if (string.IsNullOrEmpty(item)) continue;

				foreach (var item1 in CompletedEmployeesSizs.Where(x => x.DepartmentID == item))
				{
					if (item1 is null) continue;
					item1.MonthlyValue.YearValue = int.Parse(MonthlyValueCurrent.YearValue.ToString().Remove(0, 2));
					item1.MonthlyValue.MonthValue = MonthlyValueCurrent.MonthValue;
					item1.MonthlyValue.OrderNumber = MonthlyValueCurrent.OrderNumber;
				}

				MonthlyValueCurrent.OrderNumber++;
			}
			await valuesService.UpdateValuesAsync(MonthlyValueCurrent, token);
			return true;
		}
		/// <summary>
		/// Асинхронно выполняет обработку данных для списка сотрудников.
		/// </summary>
		/// <param name="completedEmployeesSizs">Список сотрудников с данными SIZ.</param>
		/// <returns>Возвращает true, если обработка прошла успешно, иначе false.</returns>
		public async Task<bool> RunTNOInBestAsync(List<EmployeeForSizDto> completedEmployeesSizs, CancellationToken token)
		{
			// Проверка на null или пустой список
			if (completedEmployeesSizs is null || completedEmployeesSizs.Count == 0) return false;

			// Получение значения rcdSkladOut из базы данных
			var rcdSkladOut = await contextServices.GetWarehouseIDAsync("06-05", token);
			if (rcdSkladOut <= 0) return false;

			// Получение уникальных идентификаторов отделов
			var listDepartment = completedEmployeesSizs
				.Where(x => !string.IsNullOrEmpty(x.DepartmentID))
				.Select(x => x.DepartmentID)
				.Distinct()
				.ToList();
			// Обработка данных для каждого отдела
			foreach (var item in listDepartment)
			{
				var currentListDepartment = completedEmployeesSizs
					.Where(x => x.DepartmentID == item)
					.ToList();

				if (string.IsNullOrEmpty(item)) continue;
				var rcdSkladIn = await contextServices.GetCodeIDRegionAsync(item, token);

				string nameDepartment = currentListDepartment.First().NameDepartnent ?? string.Empty;

				// Получение списка данных SIZ для текущего отдела
				var sizs = completedEmployeesSizs
					.Where(x => x.DepartmentID == item && x.DataSizsForSizs != null)
					.Select(x => new DataSizsForSizDto
					{
						Article = x.DataSizsForSizs?.Article ?? string.Empty,
						Count = x.DataSizsForSizs?.Count ?? 0,
					})
					.ToList();

				// Группировка данных SIZ по артикулу и суммирование количества
				var groupSizs = sizs
					.GroupBy(x => x.Article)
					.Select(y => new
					{
						Art = y.Key,
						SumCount = y.Sum(e => e.Count)
					})
					.Where(d => d.SumCount > 0)
					.ToList();

				if (groupSizs.Count == 0)
					continue;

				foreach (var group in groupSizs)
				{
					TnoDataDto tno = new()
					{
						Art = group.Art,
						Count = group.SumCount,
						RcdSkladIn = rcdSkladIn,
						RcdSkladOut = rcdSkladOut,
						Time = DateTime.Now.Date,
						Description = nameDepartment,
					};
					// Асинхронное обновление данных в базе данных
					await dbServices.UpdateU_PR_TNOAsync(tno, token);
				}

				await dbServices.ExecuteMakeTnoAsync(token);
			}
			return true;
		}

		/// <summary>
		/// Инициализация данных для расчета выдачи СИЗ на 1 число месяца, 
		/// сотрудникам (кому положено), без данных за прошлый месяц
		/// </summary>
		/// <returns>Список данных сотрудников с даныыми СИЗ</returns>
		public async Task<List<EmployeeForSizDto>> InitializeSizForOneDayWithoutBalanceAsync(CancellationToken token)
		{
			AllPeople = await contextServices.GetEmployeesForSizOneDayAsync(token);

			AllPeople = AllPeople.Where(x => x.ValidateEmployee(DateTime.Now.Month, DateTime.Now.Year)).ToList();


			List<EmployeeForSizDto> employeeForSizs = [];

			foreach (var employee in AllPeople)
			{
				if (employee is null || employee.NumGraf is null) continue;
				// Получаем список всех доступных СИЗ
				ListAllSizs = await contextServices.GetSizUsageRateAsync(token);
				// Получаем норму рабочих часов в месяце для сотрудника
				HoursInMonth = await dbServices.GetHoursPlanMonhtAsync(employee.NumGraf, DateTime.Now.Date, token);
				// Получаем список СИЗ, которые положены сотруднику по его норме
				TempListSiz = ListAllSizs.Where(x => x.UsageNormID == employee.UsageNormID).ToList();
				// Инициализируем список СИЗ для сотрудника
				List<DataSizsForSizDto> listSizForEmployee = InitializeSizsUSageFromEmployeeAsync(TempListSiz);

				foreach (var siz in listSizForEmployee)
				{
					//Собираем данные на сотрудника
					employeeForSizs.Add(new EmployeeForSizDto
					{
						EmployeeID = employee.EmployeeID,
						ShortName = employee.ShortName,
						DepartmentID = employee.DepartmentID,
						NameDepartnent = employee.DepartmentProduction.NameDepartment ?? string.Empty,
						//MonthlyValue = MonthlyValueCurrent,
						NumGraf = employee.NumGraf,
						HoursPlanMonht = HoursInMonth,
						HoursWorkinfFact = 0,
						DataSizsForSizs = siz,
					});
				}
			}
			return employeeForSizs;
		}
		/// <summary>
		/// Инициализация списка СИЗ для сотрудника на основании его нормы
		/// </summary>
		/// <param name="tempListSiz">Список норм СИЗ для сотрудника</param>
		/// <returns>Список СИЗ для сотрудника</returns>
		public List<DataSizsForSizDto> InitializeSizsUSageFromEmployeeAsync(List<SizUsageRate> tempListSiz)
		{
			List<DataSizsForSizDto> listSizForEmployee = [];

			//Собираем данные по СИЗ для сотрудника в первый раз
			foreach (var s in tempListSiz)
			{
				listSizForEmployee.Add(new DataSizsForSizDto
				{
					UsageNormID = s.UsageNormID,
					Article = s.Siz.Article,
					Name = s.Siz.Name,
					Unit = s.Siz.Unit,
					HoursPerUnit = s.HoursPerUnit,
					TotalHoursLive = 0,
					Count = 0,
					SizID = s.SizID,
				});
			}

			return listSizForEmployee;
		}

		/// <summary>
		/// Расчет на выдачу СИЗ, в начале месяца. С учётом остатка срока службы СИЗ, за прошлый месяц  
		/// </summary>
		public async Task<List<EmployeeForSizDto>> CalculateSizForNewMonthWithBalanceAsync(CancellationToken token)
		{
			// Выполняем расчет остатков сроков службы СИЗ за предыдущий месяц
			bool check = await CalculateSizForLastMonthEndWithBalanceAsync(token);
			if (!check) return [];

			// Инициализируем данные на 1 число текущего месяца без учета остатка
			EmployeesForSizsInit = await InitializeSizForOneDayWithoutBalanceAsync(token);

			foreach (var employee in EmployeesForSizsInit)
			{
				if (employee is null || employee.DataSizsForSizs is null) continue;
				// Получаем список выданных СИЗ на сотрудника с первого числа месяца
				ListSizsOutputWithOneDayMonth = await contextServices.GetDataSizForMonthsAsync(employee.EmployeeID, token);
				// Выполняем расчет количества СИЗ для сотрудника с учетом остатка срока службы за прошлый месяц


				var itemtSizsOutputWithOneDayMonth = ListSizsOutputWithOneDayMonth
					.FirstOrDefault(s => employee.DataSizsForSizs.SizID == s.SizID);

				if (itemtSizsOutputWithOneDayMonth is null)
				{
					Analitics = employee.HoursPlanMonht / employee.DataSizsForSizs.HoursPerUnit;
					AnaliticsCount = (int)Math.Ceiling(Analitics);

					employee.DataSizsForSizs.Count = AnaliticsCount;
					employee.DataSizsForSizs.TotalHoursLive = AnaliticsCount * employee.DataSizsForSizs.HoursPerUnit;
				}
				else
				{
					Analitics = (employee.HoursPlanMonht - itemtSizsOutputWithOneDayMonth.LifeTime) /
						employee.DataSizsForSizs.HoursPerUnit;
					AnaliticsCount = (int)Math.Ceiling(Analitics);

					employee.DataSizsForSizs.Count = AnaliticsCount;

					employee.DataSizsForSizs.TotalHoursLive = AnaliticsCount *
						employee.DataSizsForSizs.HoursPerUnit + itemtSizsOutputWithOneDayMonth.LifeTime;
				}

			}
			// Создаем данные о выдаче СИЗ на текущий месяц
			await CreateMonthlySizDataAsync(EmployeesForSizsInit, token);
			return EmployeesForSizsInit;
		}

		/// <summary>
		/// Корректировка отстатков срока службы у СИЗ в конце месяца
		/// </summary>
		/// /// <returns>Список сотрудников с данными СИЗ</returns>
		public async Task<bool> CalculateSizForLastMonthEndWithBalanceAsync(CancellationToken token)
		{
			// Устанавливаем текущую и последнюю дату
			SetCurrentAndLastDate();

			var maxDayLast = DateTime.DaysInMonth(LastYear, LastMonht);
			// Устанавливаем период расчета
			SetCalculationPeriod(15, maxDayLast);
			// Получаем список всех сотрудников
			AllPeople = await contextServices.GetEmployeesForSizFifteenDayAsync(StartDate, EndDate, token);

			AllPeople = AllPeople.Where(x => x.ValidateEmployee(DateTime.Now.Month, DateTime.Now.Year)).ToList();

			List<EmployeeForSizDto> employeeForSizs = [];

			foreach (var employee in AllPeople)
			{
				if (employee is null || employee.NumGraf is null) continue;

				// Получаем фактически отработанное время сотрудника за определенный период
				FactHoursInShiftAndOverday = employee.Shifts.GetCountHoursInShiftOrOverday();

				// Получаем список всех доступных СИЗ
				ListAllSizs = await contextServices.GetSizUsageRateAsync(token);

				ListSizsOutputWithOneDayMonth = await contextServices.GetDataSizForMonthsAsync(employee.EmployeeID, token);

				// Получаем список СИЗ, положенных сотруднику по его норме
				TempListSiz = ListAllSizs.Where(x => x.UsageNormID == employee.UsageNormID).ToList();

				// Инициализируем список СИЗ для сотрудника
				List<DataSizsForSizDto> listSizForEmployee = await CalculateSizsUSageFromEmployee(
					TempListSiz, ListSizsOutputWithOneDayMonth,
					FactHoursInShiftAndOverday, token);

				foreach (var siz in listSizForEmployee)
				{
					employeeForSizs.Add(new EmployeeForSizDto
					{
						EmployeeID = employee.EmployeeID,
						ShortName = employee.ShortName,
						DepartmentID = employee.DepartmentID,
						NameDepartnent = employee.DepartmentProduction.NameDepartment ?? string.Empty,
						//MonthlyValue = MonthlyValueCurrent,
						NumGraf = employee.NumGraf,
						HoursPlanMonht = HoursInMonth,
						HoursWorkinfFact = FactHoursInShiftAndOverday,
						DataSizsForSizs = siz,
					});
				}
			}
			await CreateMonthlySizDataAsync(employeeForSizs, token);
			return true;
		}

		/// <summary>
		/// Устанавливаем текущую и последнюю дату
		/// </summary>
		public void SetCurrentAndLastDate()
		{
			MonthCurrent = DateTime.Now.Month + 1;//test +1
			YearCurrent = DateTime.Now.Year;

			if (MonthCurrent != 1)
			{
				LastMonht = MonthCurrent - 1;
				LastYear = YearCurrent;
			}
			else
			{
				LastMonht = 12;
				LastYear = YearCurrent - 1;
			}
		}

		/// <summary>
		/// Устанавливаем период расчета
		/// </summary>
		public void SetCalculationPeriod(int start, int maxDayLast)
		{
			StartDate = new DateTime(day: start, month: LastMonht, year: LastYear);
			EndDate = new DateTime(day: maxDayLast, month: LastMonht, year: LastYear);
		}

		/// <summary>
		/// Рассчитывает использование СИЗ для сотрудника на основе переданных норм и фактически отработанного времени.
		/// </summary>
		/// <param name="tempListSiz">Список норм использования СИЗ для сотрудника.</param>
		/// <param name="listSizsOutputWithOneDayMonth">Список данных о СИЗ за один день в месяц.</param>
		/// <param name="factHoursInShiftAndOverday">Фактически отработанные часы за смену и сверхурочные.</param>
		/// <returns>Список данных о СИЗ для сотрудника.</returns>
		public async Task<List<DataSizsForSizDto>> CalculateSizsUSageFromEmployee(
			List<SizUsageRate> tempListSiz, List<DataSizForMonth> listSizsOutputWithOneDayMonth, double factHoursInShiftAndOverday, CancellationToken token)
		{
			List<DataSizsForSizDto> listSizForEmployee = [];

			foreach (var s in tempListSiz)
			{
				var itemtSizsOutputWithOneDayMonth = listSizsOutputWithOneDayMonth
					.FirstOrDefault(x => x.SizID == s.SizID);

				double actualTotalHoursLive = 0;

				if (itemtSizsOutputWithOneDayMonth != null)
					actualTotalHoursLive = itemtSizsOutputWithOneDayMonth.LifeTime - factHoursInShiftAndOverday;
				else
					continue;

				listSizForEmployee.Add(new DataSizsForSizDto
				{
					UsageNormID = s.UsageNormID,
					Article = s.Siz.Article,
					Name = s.Siz.Name,
					Unit = s.Siz.Unit,
					HoursPerUnit = s.HoursPerUnit,
					TotalHoursLive = actualTotalHoursLive,
					SizID = s.SizID,
				});
			}

			return listSizForEmployee;
		}

		/// <summary>
		/// Второй расчёт на выдачу СИЗ, в середине месяца. Делаем прогноз на основе проделанной работы с 1 по 14 вкл. число.
		/// </summary>
		public async Task<List<EmployeeForSizDto>> CalculateSizForFifteenDayWithBalanceAsync(CancellationToken token)
		{
			SetCurrentDate();
			SetCurrentMonthAndYear();

			AllPeople = await contextServices.GetEmployeesForSizFifteenDayAsync(StartDate, EndDate, token);

			AllPeople = AllPeople.Where(x => x.ValidateEmployee(DateTime.Now.Month, DateTime.Now.Year)).ToList();

			List<EmployeeForSizDto> employeeForSizs = [];
			foreach (var employee in AllPeople)
			{
				if (employee is null || employee.NumGraf is null) continue;

				FactHoursInShiftAndOverday = employee.Shifts.GetCountHoursInShiftOrOverday();
				HoursInMonth = await dbServices.GetHoursPlanMonhtAsync(employee.NumGraf, StartDate, token);

				ListAllSizs = await contextServices.GetSizUsageRateAsync(token);
				ListSizsOutputWithOneDayMonth = await contextServices.GetDataSizForMonthsAsync(employee.EmployeeID, token);
				TempListSiz = ListAllSizs.Where(x => x.UsageNormID == employee.UsageNormID).ToList();

				List<DataSizsForSizDto> listSizForEmployee = await CalculateSizsUSageFromEmployeeFifteenAsync(FactHoursInShiftAndOverday,
					HoursInMonth, ListSizsOutputWithOneDayMonth, TempListSiz, token);

				foreach (var siz in listSizForEmployee)
				{
					employeeForSizs.Add(new EmployeeForSizDto
					{
						EmployeeID = employee.EmployeeID,
						ShortName = employee.ShortName,
						DepartmentID = employee.DepartmentID,
						NameDepartnent = employee.DepartmentProduction.NameDepartment ?? string.Empty,
						NumGraf = employee.NumGraf,
						HoursPlanMonht = HoursInMonth,
						HoursWorkinfFact = FactHoursInShiftAndOverday,
						DataSizsForSizs = siz,
					});
				}
			}

			await CreateMonthlySizDataAsync(employeeForSizs, token);

			return employeeForSizs;
		}

		/// <summary>
		/// Устанавливает текущий месяц и год, и вычисляет начало и конец периода (с 1 по 14 число).
		/// </summary>
		public void SetCurrentMonthAndYear()
		{
			StartDate = new DateTime(day: 1, month: MonthCurrent, year: YearCurrent);
			EndDate = new DateTime(day: 14, month: MonthCurrent, year: YearCurrent);
		}

		/// <summary>
		/// Устанавливает текущие месяц и год на основе текущей даты.
		/// </summary>
		public void SetCurrentDate()
		{
			MonthCurrent = DateTime.Now.Month;
			YearCurrent = DateTime.Now.Year;
		}

		/// <summary>
		/// Рассчитывает использование СИЗ для сотрудника за первую половину месяца на основе переданных норм и фактически отработанного времени.
		/// </summary>
		/// <param name="factHoursInShiftAndOverday">Фактически отработанные часы за смену и сверхурочные.</param>
		/// <param name="hoursInMonth">Количество рабочих часов в месяце.</param>
		/// <param name="listSizsOutputWithOneDayMonth">Список данных о СИЗ за один день в месяц.</param>
		/// <param name="tempListSiz">Список норм использования СИЗ для сотрудника.</param>
		/// <returns>Список данных о СИЗ для сотрудника за первую половину месяца.</returns>
		public async Task<List<DataSizsForSizDto>> CalculateSizsUSageFromEmployeeFifteenAsync(double factHoursInShiftAndOverday, double hoursInMonth,
			List<DataSizForMonth> listSizsOutputWithOneDayMonth, List<SizUsageRate> tempListSiz, CancellationToken token)
		{
			List<DataSizsForSizDto> listSizForEmployee = [];

			foreach (var s in tempListSiz)
			{
				var itemtSizsOutputWithOneDayMonth = listSizsOutputWithOneDayMonth
					.FirstOrDefault(x => x.SizID == s.SizID);

				ActualTotalHoursLive = 0;
				Analitics = 0;
				AnaliticsCount = 0;
				bool higherMinimum = true;

				if (itemtSizsOutputWithOneDayMonth != null)
				{
					//Актуальный срок службы СИЗ, на момент расчетов
					ActualTotalHoursLive = itemtSizsOutputWithOneDayMonth.LifeTime - factHoursInShiftAndOverday;

					//Подготавливаем данные:
					//Минимум часов в % соотношении
					MinPercent = s.HoursPerUnit * 0.1;
					//Минимум часов, в числовом значении
					MinHourse = 20;
					//Предпологаемый остаток (в часах) срока службы СИЗ, при прогнозе, что сотрудник также отработает и вторую половину месяца. 
					Balance = ActualTotalHoursLive - factHoursInShiftAndOverday;

					//Если остаток <= 10%, но не больше 20 часов, то СИЗ не проходит по минимальному остатку СИЗ, и сотруднику надо выдать доп. СИЗ 
					if (Balance <= MinPercent && Balance <= MinHourse)
						higherMinimum = false; //минимальный порог не проходит - значит выдаём доп. СИЗ.

					//Прогноз на наличие СИЗ в штучном эквиваленте. Примеры:
					// 1.2 СИЗ --> означает, что в наличии у сотрудника одной целой пары СИЗ, а вторая израсходованна на 80%;
					// -1.6  --> означает, что при таком же стиле работы, сотрудник доп. израсходует все имеющиеся СИЗ и по прогнозу ему не хватит почти 2 пары СИЗ
					Analitics = Balance / s.HoursPerUnit;

					switch (Analitics)
					{
						case >= 0:
							if (!higherMinimum)
							{
								Analitics = Math.Ceiling(Analitics);
								AnaliticsCount = (int)Math.Ceiling(Analitics);
							}
							else
								Analitics = 0;
							break;
						default:
							Analitics = Analitics * -1;
							Analitics = Math.Ceiling(Analitics);
							AnaliticsCount = (int)Math.Ceiling(Analitics);
							break;
					}
				}
				else
				{
					Analitics = hoursInMonth / s.HoursPerUnit;
					AnaliticsCount = (int)Math.Ceiling(Analitics);
					ActualTotalHoursLive = AnaliticsCount * s.HoursPerUnit;
				}

				if (Analitics > 0 && itemtSizsOutputWithOneDayMonth != null)
					ActualTotalHoursLive += AnaliticsCount * s.HoursPerUnit;

				listSizForEmployee.Add(new DataSizsForSizDto
				{
					UsageNormID = s.UsageNormID,
					Article = s.Siz.Article,
					Name = s.Siz.Name,
					Unit = s.Siz.Unit,
					HoursPerUnit = s.HoursPerUnit,
					TotalHoursLive = ActualTotalHoursLive,
					Count = AnaliticsCount,
					SizID = s.SizID,
				});
			}

			return listSizForEmployee;
		}

		/// <summary>
		/// Расчет на выдачу СИЗ, в начале месяца. Расчет первичный, без учёта остатка за прошлый месяц  
		/// </summary>
		public async Task<List<EmployeeForSizDto>> CalculateSizForOneDayInMonthWithoutBalanceAsync(CancellationToken token)
		{
			//Получаем список сотрудников с подготовленными данными, для дальнейшего расчёта СИЗ
			EmployeesForSizsInit = await InitializeSizForOneDayWithoutBalanceAsync(token);

			foreach (var employee in EmployeesForSizsInit)
			{
				if (employee is null || employee.DataSizsForSizs is null) continue;

				//т.к. расчет без анализа остатков с прошлого месяца,
				//то всё просто: месячный план делим на срок службы СИЗ-а, округляем,
				//получаем приблизительное кол-во СИЗ-ов на выдачу на месяц (с запасом

				double analitics = employee.HoursPlanMonht / employee.DataSizsForSizs.HoursPerUnit;
				int analiticsCount = (int)Math.Ceiling(analitics);

				employee.DataSizsForSizs.Count = analiticsCount;
				employee.DataSizsForSizs.TotalHoursLive = analiticsCount * employee.DataSizsForSizs.HoursPerUnit;
			}

			await CreateMonthlySizDataAsync(EmployeesForSizsInit, token);

			return EmployeesForSizsInit;
		}

		/// <summary>
		/// Создаем данные о выдаче СИЗ на текущий месяц
		/// </summary>
		public async Task CreateMonthlySizDataAsync(List<EmployeeForSizDto> employeeForSizsInit, CancellationToken token)
		{
			List<DataSizForMonth> dataSizs = [];
			foreach (var employee in employeeForSizsInit)
			{
				if (employee is null || employee.DataSizsForSizs is null) continue;

				dataSizs.Add(new DataSizForMonth
				{
					EmployeeID = employee.EmployeeID,
					SizID = employee.DataSizsForSizs.SizID,
					CountExtradite = employee.DataSizsForSizs.Count,
					LifeTime = employee.DataSizsForSizs.TotalHoursLive,
				});
			}

			//Здесь происходит разбор, какие записи СИЗ-ов новые для добавления, а какие на обновление данных.

			//Общий список выданных СИЗ на сотрудников. 
			var AllDataSizForMonths = await contextServices.GetAllDataSizForMonthsAsync(token);

			var listForUpdate = dataSizs.Where(e =>
			AllDataSizForMonths.Any(x => x.EmployeeID == e.EmployeeID && x.SizID == e.SizID)).ToList();

			var listForNewAdd = dataSizs.Where(e =>
			!AllDataSizForMonths.Any(x => x.EmployeeID == e.EmployeeID && x.SizID == e.SizID)).ToList();

			if (listForNewAdd.Any()) await contextServices.AddDataSizForMonthAsync(listForNewAdd, token);
			if (listForUpdate.Any()) await contextServices.UpdateDataSizForMonthAsync(listForUpdate, token);
		}
	}
}

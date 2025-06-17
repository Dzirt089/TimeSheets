using Microsoft.EntityFrameworkCore;

using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.DataAccess.Sql.Interfaces;
using ProductionControl.Infrastructure.Repositories.Interfaces;

using System.Collections.ObjectModel;

namespace ProductionControl.Infrastructure.Repositories.Implementation
{
	public class EmployeesFactorysRepository(
		ProductionControlDbContext context, IDbServices dbServices) : IEmployeesFactorysRepository
	{
		private readonly ProductionControlDbContext _context = context;
		private readonly IDbServices _dbServices = dbServices;

		/// <summary>
		/// Пакетное обновление данных сотрудников
		/// </summary>
		/// <param name="allPeople">Список </param>
		/// <returns></returns>
		public async Task<int> UpdateEmployeesAsync(List<Employee> allPeople, CancellationToken token = default)
		{
			await using var transactions = await _context.Database.BeginTransactionAsync(token);
			_context.Employees.UpdateRange(allPeople);
			int row = await _context.SaveChangesAsync(token);
			await transactions.CommitAsync(token);

			return row;
		}

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		/// <param name="namesDepartmentItem">Выбранный отдел предприятия.</param>
		/// <param name="startDate">Дата начала периода.</param>
		/// <param name="endDate">Дата окончания периода.</param>
		/// <param name="itemMonthsTO">Выбранный месяц.</param>
		/// <param name="itemYearsTO">Выбранный год.</param>
		/// <param name="noWorkDaysTO">Список нерабочих дней.</param>
		/// <param name="checkingSeeOrWriteBool">Флаг проверки на запись или просмотр.</param>
		/// <returns>Коллекция элементов табеля.</returns>
		public async Task<List<TimeSheetItemDto>> SetDataForTimeSheetAsync(
			DataForTimeSheet dataForTimeSheet, CancellationToken token = default)
		{
			//Временная (на время расчетов) коллекция для сбора всех данных о сотрудниках для табеля
			var tempShifts = new List<TimeSheetItemDto>();

			int id = 1;

			//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
			var employeeWithSifts = await _context.Employees
				.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
				.Include(i => i.DepartmentProduction)
				.Where(w => w.DepartmentID == dataForTimeSheet.NamesDepartmentItem.DepartmentID)
				.Include(e => e.Shifts
					.Where(s => s.WorkDate >= dataForTimeSheet.StartDate && s.WorkDate <= dataForTimeSheet.EndDate))
				.OrderBy(o => o.FullName)
				.ToListAsync();

			//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
			employeeWithSifts = employeeWithSifts
				.Where(x => x.ValidateEmployee(
						dataForTimeSheet.ItemMonthsTO.Id, dataForTimeSheet.ItemYearsTO.Id))
				.ToList();

			//Циклы в которых,
			//Создаём и заполняем на каждого сотрудника "Пустой" график работ. Который будет заполняться в ручную и автоматически раз в месяц.
			foreach (var employee in employeeWithSifts.OrderBy(x => int.TryParse(x.NumGraf, out int res) ? res : 0))
			{
				var shiftDict = employee.Shifts?.ToDictionary(x => x.WorkDate) ?? [];

				for (var date = dataForTimeSheet.StartDate; date <= dataForTimeSheet.EndDate; date = date.AddDays(1))
				{
					if (!shiftDict.ContainsKey(date))
					{
						shiftDict[date] = new ShiftData
						{
							EmployeeID = employee.EmployeeID,
							WorkDate = date,
							Employee = employee,
							Hours = string.Empty,
							Shift = string.Empty,
							Overday = string.Empty,
							IsHaveLunch = false,
						};
					}
				}
				employee.Shifts = [.. shiftDict.Values];

				//Конфигурируем данные сотрудника для отображения в табеле
				var itemShift = new TimeSheetItemDto(
						id,
						new ShiftDataEmployeeDto
						{
							ShortName = employee.ShortName,
							NameShift = "Смена",
							NameOverday = "Переработка"
						},
						new ObservableCollection<ShiftData>(employee.Shifts),
						dataForTimeSheet.NoWorkDaysTO,
						dataForTimeSheet.CheckingSeeOrWriteBool,
						employee.IsLunch);

				tempShifts.Add(itemShift);
				id++;
			}

			if (_context.ChangeTracker.HasChanges())
			{
				await _context.SaveChangesAsync();
			}

			return tempShifts;

		}

		/// <summary>
		/// Получаем сотрудников для отчёта обедов, которое не уволенные
		/// </summary>
		/// <param name="startDate">Начало периода</param>
		/// <param name="endDate">Конец периода</param>
		/// <returns></returns>
		public async Task<List<Employee>> GetEmployeesForReportLunchAsync(
			StartEndDateTime startEndDate, CancellationToken token = default)
		{
			return await _context.Employees
					.Include(i => i.Shifts
						.Where(d => d.WorkDate >= startEndDate.StartDate && d.WorkDate <= startEndDate.EndDate))
					.Where(w => w.IsDismissal == false)
					.ToListAsync(token);
		}

		/// <summary>
		/// Получаем сотрудников для заказов каждодневного обеда, которые не уволенны и которые кушают
		/// </summary>
		/// <returns></returns>
		public async Task<List<Employee>> GetEmployeesForLunchAsync(CancellationToken token = default)
		{
			return await _context.Employees
					.AsNoTracking()
					.Include(i => i.Shifts
						.Where(w => w.WorkDate == DateTime.Now.Date))
					.Where(ww => ww.IsLunch == true &&
							ww.IsDismissal == false)
					.ToListAsync(token);
		}

		/// <summary>
		/// заполняет график сотрудника на месяц по его графику из ИС-ПРО
		/// </summary>
		/// <param name="startDate">дата на начало прогнозируемого месяца</param>
		/// <param name="endDate">дата на конец прогнозируемого месяца</param>
		public async Task<List<Employee>> GetEmployeesAsync(
			StartEndDateTime startEndDate, CancellationToken token = default)
		{
			return await _context.Employees
					.Include(i => i.DepartmentProduction)
					.Include(i => i.Shifts
						.Where(d => d.WorkDate >= startEndDate.StartDate && d.WorkDate <= startEndDate.EndDate))
					.ToListAsync(token);
		}

		/// <summary>
		/// Отменяет увольнение сотрудника.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="defaultDateDismissal">Дата увольнения по умолчанию.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Результат отмены увольнения.</returns>
		public async Task<bool> CancelDismissalEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.Employees
				.Where(w => w.EmployeeID == idEmployeeDateTime.IdEmployee)
				.SingleOrDefaultAsync(token);

			if (itemEmployee is null) return false;


			//Если уволен - отменяем увольнение. 
			if (itemEmployee.ValidateDateDismissal())
			{
				itemEmployee.IsDismissal = false;

				//Обновляем дату уволнения
				itemEmployee.DateDismissal = idEmployeeDateTime.Date;

				//Сохраняем
				await _context.SaveChangesAsync(token);

				return true;
			}
			return false;
		}

		/// <summary>
		/// Очищаем данные на сегодня, перед новыми заказами обедов
		/// </summary>
		public async Task CleareDataForFormulateReportForLunchEveryDayDbAsync(CancellationToken token = default)
		{
			var tempListForClear = await _context.Employees
				.Include(i => i.Shifts
					.Where(w => w.WorkDate == DateTime.Now.Date))
				.Where(ww => ww.IsDismissal == false)
				.ToListAsync(token);

			foreach (var item in tempListForClear)
				item.Shifts.Foreach(x => x.IsHaveLunch = false);

			if (_context.ChangeTracker.HasChanges())
				await _context.SaveChangesAsync(token);
		}

		/// <summary>
		/// Синхронный метод по очистке данных при завершении программы.
		/// </summary>
		/// <param name="lastSelectedDepartmentID">ID последнего выбранного отдела.</param>
		/// <param name="employeeAccesses">Список прав доступа сотрудников.</param>
		/// <returns>True, если данные успешно очищены, иначе false.</returns>
		public async Task<bool> ClearIdAccessRightFromDepartmentDb(DataClearIdAccessRight dataClearId, CancellationToken token = default)
		{
			var itemDepartmentLast = await _context.DepartmentProductions
					.Where(x => x.DepartmentID == dataClearId.LastSelectedDepartmentID)
					.SingleOrDefaultAsync(token);

			if (itemDepartmentLast.AccessRight != 0 &&
				itemDepartmentLast.AccessRight == dataClearId.EmployeeAccesses
				.First(x => x.DepartmentID == dataClearId.LastSelectedDepartmentID)
				.EmployeeAccessRightId)
			{
				itemDepartmentLast.AccessRight = 0;
				_context.DepartmentProductions.Update(itemDepartmentLast);
				await _context.SaveChangesAsync(token);
			}
			return true;
		}

		/// <summary>
		/// Проверяем при переходе на другой участок, были ли предыдущие в режиме редактирования.
		/// Если да, то проводим проверки и очищаем данные.
		/// </summary>
		public async Task ClearLastDeport(DataForClearLastDeport dataForClear, CancellationToken token = default)
		{
			var itemDepartmentLast = await _context.DepartmentProductions
						.SingleOrDefaultAsync(x => x.DepartmentID == dataForClear.LastSelectedDepartmentID, token);

			if (itemDepartmentLast.AccessRight != 0 && itemDepartmentLast.AccessRight != null)
			{
				var lastAccessRight = dataForClear.EmployeeAccesses
					.FirstOrDefault(x => x.DepartmentID == dataForClear.LastSelectedDepartmentID);

				if (lastAccessRight != null &&
					itemDepartmentLast.AccessRight == lastAccessRight.EmployeeAccessRightId)
				{
					await using var trans = await _context.Database.BeginTransactionAsync(token);
					itemDepartmentLast.AccessRight = 0;

					_context.DepartmentProductions.Update(itemDepartmentLast);
					await _context.SaveChangesAsync(token);
					await trans.CommitAsync(token);
				}
			}
		}

		/// <summary>
		/// Получает данные по доступам сотрудника.
		/// </summary>
		/// <param name="userName">Имя локального компьютера.</param>
		/// <returns>Список прав доступа сотрудника.</returns>
		public async Task<List<EmployeeAccessRight>> GetAccessRightsEmployeeAsync(string userName, CancellationToken token = default)
		{
			return await _context.EmployeeAccessRights
						.AsNoTracking()
						.Where(z => z.NameUsers.Contains(userName))
						.Include(x => x.DepartmentProduction)
						.ToListAsync(token);
		}

		/// <summary>
		/// Получает все данные по участкам для картотеки.
		/// </summary>
		/// <returns>Список участков.</returns>
		public async Task<List<DepartmentProduction>> GetAllDepartmentsAsync(CancellationToken token = default)
		{
			//Получаем список участков из БД приложения
			var departmentProductionsDb =
				await _context.DepartmentProductions
				.AsNoTracking()
				.ToListAsync(token);

			return departmentProductionsDb;
		}

		public async Task<DepartmentProduction> GetDepartmentProductionAsync(string depId, CancellationToken token = default)
		{
			return await _context.DepartmentProductions
					.AsNoTracking()
					.SingleOrDefaultAsync(x =>
					x.DepartmentID == depId, token);
		}

		public async Task<EmployeeAccessRight> GetEmployeeByIdAsync(DepartmentProduction itemDepartment, CancellationToken token = default)
		{
			return await _context.EmployeeAccessRights
					.AsNoTracking()
					.SingleOrDefaultAsync(x =>
					x.EmployeeAccessRightId == itemDepartment.AccessRight, token);
		}

		/// <summary>
		/// Получает данные по сотрудникам, которые не уволены.
		/// </summary>
		/// <param name="department">Выбранный участок предприятия.</param>
		/// <returns>Список сотрудников, работающих на выбранном участке.</returns>
		public async Task<List<Employee>> GetEmployeeForCartotecasAsync(DepartmentProduction department, CancellationToken token = default)
		{
			var employeesForCartoteca = await _context.Employees
					.AsNoTracking()
					.Where(x => x.DepartmentID == department.DepartmentID)
					.Include(i => i.DepartmentProduction)
					.Include(i => i.UsageNorm)
					.OrderBy(o => o.ShortName)
					.ToListAsync(token);

			return employeesForCartoteca;
		}

		/// <summary>
		/// Получает данные по сотруднику по его табельному номеру и дате.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="date">Дата.</param>
		/// <returns>Сотрудник.</returns>
		public async Task<Employee> GetEmployeeIdAndDateAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default)
		{
			return await _context.Employees
					.AsNoTracking()
					.Include(i => i.Shifts
						.Where(x => x.WorkDate == idEmployeeDateTime.Date))
					.SingleOrDefaultAsync(w =>
					w.EmployeeID == idEmployeeDateTime.IdEmployee, token);
		}

		/// <summary>
		/// Получает список сотрудников с переработками для указанных регионов за период.
		/// </summary>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<Employee>> GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(
			StartEndDateTime startEndDate, CancellationToken token = default)
		{
			var list = await _context.Employees
					.AsNoTracking()
					.Where(x => x.DepartmentID.Contains("043") || x.DepartmentID.Contains("044"))
					.Include(i => i.Shifts
						.Where(r => r.WorkDate >= startEndDate.StartDate && r.WorkDate <= startEndDate.EndDate))
					.ToListAsync(token);

			return list;
		}

		/// <summary>
		/// Асинхронно сохраняет данные сотрудника.
		/// </summary>
		/// <param name="employee">Данные сотрудника.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		public async Task SetDataEmployeeAsync(Employee employee, CancellationToken token = default)
		{
			_context.Employees.Update(employee);
			await _context.SaveChangesAsync(token);
		}

		/// <summary>
		/// Асинхронно устанавливает названия отделов из БД приложения.
		/// Если их нет, идёт получение данных по ним в ИС-ПРО, с последующим сохранением в нашу БД.
		/// </summary>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		public async Task SetNamesDepartmentAsync(CancellationToken token = default)
		{
			//Получаем список участков из БД приложения
			var departmentProductionsDb = await _context.DepartmentProductions
				.ToListAsync(token);

			//Получаем список участков  из ИС-ПРО, из всех доступных для нас сотрудников.
			var departmentProductionsBest = await _dbServices
					.GetDepartmentProductionsAsync(DateTime.Now, token)
					.ConfigureAwait(false);

			var departmentProductionsMissing = departmentProductionsBest
				.Where(x => !departmentProductionsDb.Any(a => a.DepartmentID == x.DepartmentID))
				.ToList();

			if (departmentProductionsMissing.Any())
			{
				//Добавляем новый список в таблицу участков БД и сохраняем данные
				await _context.DepartmentProductions.AddRangeAsync(departmentProductionsMissing, token);
				if (_context.ChangeTracker.HasChanges())
					await _context.SaveChangesAsync(token);
			}
		}

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		public async Task SetTotalWorksDaysAsync(ShiftData shiftData, CancellationToken token = default)
		{
			var shifts = await _context.ShiftsData
				   .Where(x => x.EmployeeID == shiftData.EmployeeID && x.WorkDate == shiftData.WorkDate)
				   .Include(x => x.Employee)
				   .FirstOrDefaultAsync(token);

			if (shifts != null)
			{
				shifts.Hours = shiftData.Hours;
				shifts.Shift = shiftData.Shift;
				shifts.Overday = shiftData.Overday;

				await _context.SaveChangesAsync(token);
			}
		}

		/// <summary>
		/// Обновляет таблицу данных, сравнивая и синхронизируя локальные и удаленные данные.
		/// </summary>
		/// <param name="periodDate">Период, за который необходимо обновить данные.</param>
		/// <returns>Строка с отчетом об обновлении данных.</returns>
		public async Task<string> UpdateDataTableNewEmployeeAsync(DateTime periodDate, CancellationToken token = default)
		{
			await SetNamesDepartmentAsync(token);

			var report = string.Empty;//#A			
			var timeShidsEmployees = await _context.Employees  //#B										
				.OrderBy(o => o.FullName)
				.ToListAsync(token);

			var bestEmployees = await _dbServices.GetEmployeesFromBestAsync(
				string.Empty, periodDate, token).ConfigureAwait(false) ?? []; //#C

			if (bestEmployees is null ||
				timeShidsEmployees is null ||
				bestEmployees.Count == 0) return string.Empty;

			if (bestEmployees.Any(x => !timeShidsEmployees.Any(y => y.EmployeeID == x.EmployeeID)))     //#D
			{
				var peopleMissingDB = bestEmployees
					.Where(x => !timeShidsEmployees
					.Any(y => y.EmployeeID == x.EmployeeID))
					.ToList();   //#D

				await _context.Employees.AddRangeAsync(peopleMissingDB, token);   //#D 

				if (_context.ChangeTracker.HasChanges())   //#E 
				{
					var row = await _context.SaveChangesAsync(token);   //#E 

					foreach (var item in peopleMissingDB)
						report += $"Добавлен {item.FullName} на участок {item.DepartmentID} с табельным № {item.EmployeeID}\n";
				}
			}
			//#A Конфигурируем периоды дат, из выбранных в приложении месяца и года

			//#B Выбираем по участкам людей из БД приложения
			//#C Выбираем по участкам людей из ИС-ПРО с последними изменениями на заданный период

			//#D Если В ИС-ПРО есть новые люди на переданный период, то записываем их в нашу БД
			//#D формируем новый список людей, которых добавим в БД приложения
			//#D добавление в БД
			//#E Если изменения в dbContext присутствуют
			//#E то сохраняем в БД. Если сотрудников ещё нет в БД,
			//#E то извлекаем из полученных данных на сотрудников, все участки с их номерами
			//#E рекурсивно вызываем свой же метод для дальнейшего расчета с полученными данными. 
			//#E после всего, запускаем главный расчет формирования табеля для программы.		


			//Проверяем, есть ли измененные данные (В ИС-ПРО) по уже имеющимся записям сотрудников, это принадлежность к участку, к графику работы, даты уволнения
			var listForUpdateData = bestEmployees
				.Where(x => timeShidsEmployees.Any(y => y.EmployeeID == x.EmployeeID &&
					  (y.DepartmentID != x.DepartmentID || y.NumGraf != x.NumGraf || y.FullName != x.FullName)));

			//Если есть хотябы одна запись, то производим обновление этих данных
			if (listForUpdateData.Any())
			{
				foreach (var bestEmployee in listForUpdateData)
				{
					foreach (var employee in timeShidsEmployees
						.Where(x => x.EmployeeID == bestEmployee.EmployeeID))
					{
						report += $"\nУ {employee.ShortName} обновлены: \n";
						if (employee.DepartmentID != bestEmployee.DepartmentID)
						{
							report += $"Номер участка с {employee.DepartmentID}, на {bestEmployee.DepartmentID} \n";
							employee.DepartmentID = bestEmployee.DepartmentID;
						}
						if (employee.NumGraf != bestEmployee.NumGraf)
						{
							report += $"Номер графика с {employee.NumGraf}, на {bestEmployee.NumGraf} \n";
							employee.NumGraf = bestEmployee.NumGraf;
						}
						if (employee.FullName != bestEmployee.FullName)
						{
							report += $"ФИО с {employee.FullName}, на {bestEmployee.FullName} \n";
							employee.FullName = bestEmployee.FullName;
						}
					}
				}
				//Если изменения есть, то сохраняем и запускаем перерасчёт
				if (_context.ChangeTracker.HasChanges())
					await _context.SaveChangesAsync(token);
			}
			//await SetNewRightDepartment();
			return report;
		}

		public async Task UpdateDepartamentAsync(DepartmentProduction itemDepartment, CancellationToken token = default)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync(token);
			_context.DepartmentProductions.Update(itemDepartment);
			await _context.SaveChangesAsync(token);
			await transaction.CommitAsync(token);
		}

		/// <summary>
		/// Обновляет дату увольнения у выбранного сотрудника.
		/// </summary>
		/// <param name="date">Дата увольнения.</param>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <returns>Результат обновления.</returns>
		public async Task<bool> UpdateDismissalDataEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.Employees
				.Include(i => i.Shifts)
				.SingleOrDefaultAsync(e => e.EmployeeID == idEmployeeDateTime.IdEmployee, token);

			if (itemEmployee is null) return false;

			//Обнуляем все проставленные наперёд данные по табелю
			itemEmployee?.Shifts?
				.Where(x => x.WorkDate > idEmployeeDateTime.Date)
				.ToList()
				.ForEach(f =>
				{
					f.Shift = string.Empty;
					f.Hours = string.Empty;
					f.Overday = string.Empty;
				});

			//Обновляем дату уволнения
			itemEmployee.DateDismissal = idEmployeeDateTime.Date;
			//Ставим флаг уволнения, после которого нельзя редактировать данные
			itemEmployee.IsDismissal = true;

			//Сохраняем
			await _context.SaveChangesAsync(token);
			return true;
		}

		/// <summary>
		/// Если сотрудник обедает, то в его данных отображается инфа, что он кушает, 
		/// и на него заказывается обед
		/// </summary>
		public async Task<bool> UpdateIsLunchingDbAsync(long idEmployee, CancellationToken token = default)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.Employees
				.Where(w => w.EmployeeID == idEmployee)
				.SingleOrDefaultAsync(token);

			if (itemEmployee is null) return false;

			itemEmployee.IsLunch = !itemEmployee.IsLunch;

			//Сохраняем
			await _context.SaveChangesAsync(token);

			return true;
		}

		/// <summary>
		/// Обновляет данные об обеде у сотрудника по указанной дате.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="manualLastDateLunch">Дата последнего обеда.</param>
		/// <returns>Результат обновления.</returns>
		public async Task<bool> UpdateLunchEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.Employees
				.Where(w => w.EmployeeID == idEmployeeDateTime.IdEmployee)
				.Include(i => i.Shifts
					.Where(x => x.WorkDate == idEmployeeDateTime.Date))
				.SingleOrDefaultAsync(token);

			if (itemEmployee == null || itemEmployee.IsDismissal) return false;

			var isEats = itemEmployee.Shifts
				.First(x => x.WorkDate == idEmployeeDateTime.Date).IsHaveLunch;

			itemEmployee.Shifts.Foreach(x => x.IsHaveLunch = !isEats);

			//Сохраняем
			await _context.SaveChangesAsync(token);
			return true;
		}

		public async Task SaveEmployeeCardNumsAsync(IEnumerable<EmployeeCardNumShortNameId> employeeCardNums, CancellationToken token = default)
		{
			var ids = employeeCardNums.Select(c => c.EmployeeID).ToList();
			var employeeCardNumsDict = employeeCardNums.ToDictionary(x => x.EmployeeID);

			var tempEmployees = await _context.Employees
				.Where(x => ids.Contains(x.EmployeeID))
				.ToListAsync(token);

			foreach (var employeeDb in tempEmployees)
			{
				var temp = employeeCardNumsDict[employeeDb.EmployeeID];
				employeeDb.CardNumber = temp.CardNumber;
			}

			var ad = await _context.SaveChangesAsync(token);
		}

		public async Task<IEnumerable<EmployeeCardNumShortNameId>> GetEmployeeEmptyCardNumsAsync(CancellationToken token = default)
		{
			var tempEmployees = await _context.Employees
				.AsNoTracking()
				.Where(x => string.IsNullOrEmpty(x.CardNumber))
				.ToListAsync(token);

			var result = tempEmployees.Select(x => new EmployeeCardNumShortNameId
			{
				EmployeeID = x.EmployeeID,
				ShortName = x.ShortName,
				CardNumber = x.CardNumber,
			});

			return result;
		}
	}
}

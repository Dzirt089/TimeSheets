using Microsoft.EntityFrameworkCore;

using TimeSheets.DAL;
using TimeSheets.Entitys;
using TimeSheets.Models;
using TimeSheets.Services.Interfaces;
using TimeSheets.Utils;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace TimeSheets.Services
{
	public class TimeSheetDbService(
		IDbContextFactory<ShiftTimesDbContext> context,
		IErrorLogger errorLogger) : ITimeSheetDbService
	{
		private readonly IDbContextFactory<ShiftTimesDbContext> _context = context;
		private readonly IErrorLogger _errorLogger = errorLogger;

		public List<Employee> TimeShidsEmployees { get; private set; }
		public List<Employee> BestEmployees { get; private set; }
		public List<Employee> EmployeeWithSifts { get; private set; }
		public List<DepartmentProduction> DepartmentProductionsDb { get; private set; }
		public List<DepartmentProduction> DepartmentProductionsBest { get; private set; }
		public List<DepartmentProduction> DepartmentProductionsMissing { get; private set; }

		/// <summary>
		/// Добавляет нового сотрудника.
		/// </summary>
		/// <param name="emp">Сотрудник для добавления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если сотрудник успешно добавлен, иначе False.</returns>
		public async Task<bool> AddEmployeeAsync(Employee emp, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{				
				await dbContext.Employees.AddAsync(emp);
				await dbContext.SaveChangesAsync();
				await trans.CommitAsync();
				return true;
			}
			catch (Exception ex)
			{
				await trans.RollbackAsync();
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return false;
			}
		}

		/// <summary>
		/// Обновляет данные сотрудника и его смен.
		/// </summary>
		/// <param name="employee">Сотрудник для обновления.</param>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <param name="valueDepartmentID">Идентификатор отдела.</param>
		/// <param name="addInTimeSheetEmployee">Флаг добавления в табель.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		public async Task<bool> UpdateEmployeeAndShiftDataAsync(Employee employee,
			DateTime startDate, DateTime endDate, string valueDepartmentID,
			bool addInTimeSheetEmployee, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{
				var emp = await dbContext.Employees
					.SingleOrDefaultAsync(e => e.EmployeeID == employee.EmployeeID);

				if (emp is null)
					return await AddEmployeeAsync(employee, userDataCurrent);

				var shiftDict = emp.Shifts?.ToDictionary(x => x.WorkDate) ?? [];

				for (var date = startDate; date <= endDate; date = date.AddDays(1))
				{
					if (!shiftDict.ContainsKey(date))
					{
						shiftDict[date] = new ShiftData
						{
							EmployeeID = emp.EmployeeID,
							WorkDate = date,
							Employee = emp,
							Overday = string.Empty,
							Hours = string.Empty,
						};
					}
				}
				emp.Shifts = [.. shiftDict.Values];
				await dbContext.SaveChangesAsync();
				await trans.CommitAsync();
				return true;
			}
			catch (Exception ex)
			{
				await trans.RollbackAsync();
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return false;
			}
		}

		/// <summary>
		/// Проверяем, существует ли табельный номер, при создании нового сотрудника
		/// </summary>
		/// <param name="employeeId">Табельный номер сотрудника</param>
		/// <param name="userDataCurrent">Данные текущего пользователя</param>
		/// <returns>True, если совпадение найдено, иначе False</returns>
		public async Task<bool> CheckingDoubleEmployeeAsync(long employeeId, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();

			try
			{
				return dbContext.Employees
			   .Any(x => x.EmployeeID == employeeId);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return false;
			}
		}


		/// <summary>
		/// Обновляет данные сотрудника.
		/// </summary>
		/// <param name="employee">Сотрудник для обновления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		public async Task<bool> UpdateEmployeeAsync(Employee employee, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{
				var emp = await dbContext.Employees
					.Where(x => x.EmployeeID == employee.EmployeeID)
					.FirstOrDefaultAsync();

				if (emp is null)
					return await AddEmployeeAsync(employee, userDataCurrent);
				else
				{
					emp.DateDismissal = employee.DateDismissal;
					emp.IsDismissal = employee.IsDismissal;
					emp.NumberPass = employee.NumberPass;
					emp.FullName = employee.FullName;
					emp.ShortName = employee.ShortName;
					emp.DateEmployment = employee.DateEmployment;
					emp.Photo = employee.Photo;
					emp.Descriptions = employee.Descriptions;
					emp.DepartmentID = employee.DepartmentID;
					emp.NumGraf = employee.NumGraf;
					emp.IsLunch = employee.IsLunch;
				}

				await dbContext.SaveChangesAsync();
				await trans.CommitAsync();
				return true;
			}
			catch (Exception ex)
			{
				await trans.RollbackAsync();
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return false;
			}
		}

		/// <summary>
		/// Получает данные по сотруднику по его табельному номеру и дате.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="date">Дата.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Сотрудник.</returns>
		public async Task<Employee?> GetEmployeeIdAndDateAsync(long idEmployee,
			DateTime date,
			LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				return await dbContext.Employees
					.AsNoTracking()
					.Include(i => i.Shifts
						.Where(x => x.WorkDate == date))
					.SingleOrDefaultAsync(w => w.EmployeeID == idEmployee);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return null;
			}
		}

		/// <summary>
		/// Обновляет дату увольнения у выбранного сотрудника.
		/// </summary>
		/// <param name="date">Дата увольнения.</param>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Результат обновления.</returns>
		public async Task<bool?> UpdateDismissalDataEmployeeAsync(
			DateTime date,
			long idEmployee,
			LocalUserData userDataCurrent
			)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Достаём данные по сотруднику по его табельному номеру
				var itemEmployee = await dbContext.Employees
					.Include(i => i.Shifts)
					.SingleOrDefaultAsync(e => e.EmployeeID == idEmployee);

				if (itemEmployee is null) return false;

				//Обнуляем все проставленные наперёд данные по табелю
				itemEmployee?.Shifts?
					.Where(x => x.WorkDate > date)
					.ToList()
					.ForEach(f =>
					{
						f.Shift = string.Empty;
						f.Hours = string.Empty;
						f.Overday = string.Empty;
					});

				//Обновляем дату уволнения
				itemEmployee.DateDismissal = date;
				//Ставим флаг уволнения, после которого нельзя редактировать данные
				itemEmployee.IsDismissal = true;

				//Сохраняем
				await dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return null;
			}
		}

		/// <summary>
		/// Отменяет увольнение сотрудника.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="defaultDateDismissal">Дата увольнения по умолчанию.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Результат отмены увольнения.</returns>
		public async Task<bool?> CancelDismissalEmployeeAsync(long idEmployee,
			DateTime defaultDateDismissal, LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Достаём данные по сотруднику по его табельному номеру
				var itemEmployee = await dbContext.Employees
					.Where(w => w.EmployeeID == idEmployee)
					.SingleOrDefaultAsync();

				if (itemEmployee is null) return null;


				//Если уволен - отменяем увольнение. Иначе - Увольняем
				if (itemEmployee.ValidateDateDismissal())
				{
					//Ставим флаг уволнения, если true - нельзя редактировать данные
					itemEmployee.IsDismissal = false;

					//Обновляем дату уволнения
					itemEmployee.DateDismissal = defaultDateDismissal;

					//Сохраняем
					await dbContext.SaveChangesAsync();

					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return null;
			}
		}


		/// <summary>
		/// Обновляет данные об обеде у сотрудника по указанной дате.
		/// </summary>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="manualLastDateLunch">Дата последнего обеда.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Результат обновления.</returns>
		public async Task<bool?> UpdateLunchEmployeeAsync(
			long idEmployee, DateTime manualLastDateLunch, LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Достаём данные по сотруднику по его табельному номеру
				var itemEmployee = await dbContext.Employees
					.Where(w => w.EmployeeID == idEmployee)
					.Include(i => i.Shifts
						.Where(x => x.WorkDate == manualLastDateLunch))
					.SingleOrDefaultAsync();

				if (itemEmployee == null || itemEmployee.IsDismissal) return false;

				var isEats = itemEmployee.Shifts
					.First(x => x.WorkDate == manualLastDateLunch).IsHaveLunch;

				itemEmployee.Shifts.Foreach(x => x.IsHaveLunch = !isEats);

				//Сохраняем
				await dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return null;
			}
		}


		/// <summary>
		/// Получает все данные по участкам для картотеки.
		/// </summary>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Список участков.</returns>
		public async Task<List<DepartmentProduction>>
			GetAllDepartmentsAsync(LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Получаем список участков из БД приложения
				DepartmentProductionsDb =
					await dbContext.DepartmentProductions.ToListAsync();

				//Для подробного отображения в табеле составного имени,
				//составляем его. Это св-во не мапится в таблицу. Т.к. не нужно.
				DepartmentProductionsDb?
					.ForEach(x => x.FullNameDepartment =
					$"{x.DepartmentID} : {x.NameDepartment}");

				return DepartmentProductionsDb;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}

		/// <summary>
		/// Получает экземпляр локальных данных на сотрудника.
		/// </summary>
		/// <returns>Локальные данные пользователя.</returns>
		public async Task<LocalUserData?> GetLocalUserAsync()
		{
			try
			{
				string localNameMachine = Environment.MachineName;
				return new LocalUserData()
				{
					MachineName = localNameMachine,
					UserName = string.Empty
				};
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				return null;
			}
		}

		/// <summary>
		/// Получает данные по сотрудникам, которые не уволены.
		/// </summary>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера.</param>
		/// <param name="department">Выбранный участок предприятия.</param>
		/// <returns>Список сотрудников, работающих на выбранном участке.</returns>
		public async Task<ObservableCollection<Employee>>
			GetEmployeeForCartotecasAsync(
			LocalUserData userDataCurrent,
			DepartmentProduction department)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				var employeesForCartoteca = await dbContext.Employees
					.Where(x => x.DepartmentID == department.DepartmentID)
					.Include(i => i.DepartmentProduction)
					.OrderBy(o => o.ShortName)
					.ToListAsync();

				//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
				employeesForCartoteca = employeesForCartoteca
					.Where(x => x.VolidateEmployee(DateTime.Now.Month, DateTime.Now.Year) && x.IsDismissal == false)
					.ToList();

				return new ObservableCollection<Employee>(employeesForCartoteca);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}

		/// <summary>
		/// Получает данные по сотрудникам, которые не уволены.
		/// </summary>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера.</param>
		/// <param name="department">Выбранный участок предприятия.</param>
		/// <returns>Список сотрудников, работающих на выбранном участке.</returns>
		public async Task<List<Employee>>
			GetEmployeeForCartotecasAsync(
			LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				var employeesForCartoteca = await dbContext.Employees
					.Include(i => i.DepartmentProduction)
					.OrderBy(o => o.ShortName)
					.ToListAsync();

				//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
				employeesForCartoteca = employeesForCartoteca
					.Where(x => x.VolidateEmployee(DateTime.Now.Month, DateTime.Now.Year) && x.IsDismissal == false)
					.ToList();

				return employeesForCartoteca;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
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
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Коллекция элементов табеля.</returns>
		public async Task<ObservableCollection<TimeSheetItem>> SetDataForTimeSheetAsync(
			DepartmentProduction namesDepartmentItem,
			DateTime startDate, DateTime endDate,
			MonthsOrYears itemMonthsTO, MonthsOrYears itemYearsTO,
			List<int> noWorkDaysTO,
			LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Временная (на время расчетов) коллекция для сбора всех данных о сотрудниках для табеля
				var tempShifts = new ObservableCollection<TimeSheetItem>();

				int id = 1;

				//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
				EmployeeWithSifts = await dbContext.Employees
					.Include(i => i.DepartmentProduction)
					.Where(w => w.DepartmentID == namesDepartmentItem.DepartmentID)
					.Include(e => e.Shifts
						.Where(s => s.WorkDate >= startDate && s.WorkDate <= endDate))
					.OrderBy(o => o.FullName)
					.ToListAsync();

				//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
				EmployeeWithSifts = EmployeeWithSifts
					.Where(x => x.VolidateEmployee(itemMonthsTO.Id, itemYearsTO.Id))
					.ToList();

				//Циклы в которых,
				//Создаём и заполняем на каждого сотрудника "Пустой" график работ. Который будет заполняться в ручную и автоматически раз в месяц.
				foreach (var employee in EmployeeWithSifts)
				{
					var shiftDict = employee.Shifts?.ToDictionary(x => x.WorkDate) ?? [];

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
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
					var itemShift = new TimeSheetItem(
							id,
							new ShiftDataEmployee
							{
								ShortName = employee.ShortName,
								NameShift = "Смена",
								NameOverday = "Переработка"
							},
							new ObservableCollection<ShiftData>(employee.Shifts),
							noWorkDaysTO,
							employee.IsLunch);

					//Если сотрудник уволен в выбранном месяце, то его ФИО красятся в красный. Все остальные случаи - в черный
					if (employee.DateDismissal.Month == itemMonthsTO.Id &&
						employee.DateDismissal.Year == itemYearsTO.Id)
						itemShift.Brush = Brushes.Red;
					else
						itemShift.Brush = Brushes.Black;

					employee.Shifts.Foreach(x =>
					{
						if (!string.IsNullOrEmpty(x.Shift))
							x.Brush = x.Shift.GetBrush();
					});

					tempShifts.Add(itemShift);
					id++;
				}

				if (dbContext.ChangeTracker.HasChanges())
				{
					await dbContext.SaveChangesAsync();
				}

				return tempShifts;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}

		/// <summary>
		/// Асинхронно сохраняет данные сотрудника.
		/// </summary>
		/// <param name="employee">Данные сотрудника.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		public async Task SetDataEmployeeAsync(Employee employee,
			LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();
				dbContext.Employees.Update(employee);
				await dbContext.SaveChangesAsync();

			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		public async Task SetTotalWorksDaysAsync(object? sender,
			LocalUserData userDataCurrent)
		{
			try
			{
				if (sender is ShiftData shiftData)
				{
					await using var dbContext = await _context.CreateDbContextAsync();
					dbContext.ShiftsData?.Update(shiftData);
					await dbContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Очищаем данные на сегодня, перед новыми заказами обедов
		/// </summary>
		public async Task CleareDataForFormulateReportForLunchEveryDayDbAsync(
			LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				var tempListForClear = await dbContext.Employees
				.Include(i => i.Shifts
					.Where(w => w.WorkDate == DateTime.Now.Date))
				.Where(ww => ww.IsDismissal == false)
				.ToListAsync();

				foreach (var item in tempListForClear)
					item.Shifts.Foreach(x => x.IsHaveLunch = false);

				if (dbContext.ChangeTracker.HasChanges())
					await dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Если сотрудник обедает, то в его данных отображается инфа, что он кушает, 
		/// и на него заказывается обед
		/// </summary>
		public async Task<bool> UpdateIsLunchingDbAsync(
			long idEmployee, LocalUserData userDataCurrent)
		{
			try
			{

				await using var dbContext = await _context.CreateDbContextAsync();

				//Достаём данные по сотруднику по его табельному номеру
				var itemEmployee = await dbContext.Employees
					.Where(w => w.EmployeeID == idEmployee)
					.SingleOrDefaultAsync();

				if (itemEmployee is null) return false;

				itemEmployee.IsLunch = !itemEmployee.IsLunch;

				//Сохраняем
				await dbContext.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return false;
			}
		}

	}
}

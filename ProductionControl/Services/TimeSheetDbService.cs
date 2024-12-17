using Microsoft.EntityFrameworkCore;

using ProductionControl.DAL;
using ProductionControl.Entitys;
using ProductionControl.Entitys.ExternalOrganization;
using ProductionControl.Models;
using ProductionControl.Models.ExternalOrganization;
using ProductionControl.Services.Interfaces;
using ProductionControl.Utils;

using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ProductionControl.Services
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
		/// Получает список сотрудников за указанный период.
		/// </summary>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <param name="valueDepartmentID">Идентификатор отдела.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(
			LocalUserData userDataCurrent, DateTime startDate, DateTime endDate, string valueDepartmentID)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			try
			{
				var listResult = await dbContext.EmployeeExOrgs
					.AsNoTracking()
					.Include(x => x.EmployeeExOrgAddInRegions)
					.Include(x => x.ShiftDataExOrgs
						.Where(r => r.WorkDate >= startDate && r.WorkDate <= endDate && r.DepartmentID == valueDepartmentID))
					.Where(s => s.IsDismissal == false)
					.ToListAsync();

				return listResult;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}

		/// <summary>
		/// Получает список сотрудников за указанный период.
		/// </summary>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <param name="valueDepartmentID">Идентификатор отдела.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsNoDismissalAsync(LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			try
			{
				var listResult = await dbContext.EmployeeExOrgs
					.AsNoTracking()
					.Include(x => x.EmployeeExOrgAddInRegions)
					.Where(s => s.IsDismissal == false)
					.ToListAsync();

				return listResult;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}


		/// <summary>
		/// Получает список всех сотрудников.
		/// </summary>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>Коллекция сотрудников.</returns>
		public async Task<List<EmployeeExOrg>> GetAllEmployeeExOrgsAsync(LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			try
			{
				var listResult = await dbContext.EmployeeExOrgs.ToListAsync();
				return listResult;
			}
			catch (Exception ex)
			{

				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
			}
		}

		/// <summary>
		/// Добавляет нового сотрудника.
		/// </summary>
		/// <param name="exOrg">Сотрудник для добавления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если сотрудник успешно добавлен, иначе False.</returns>
		public async Task<bool> AddEmployeeExOrgAsync(EmployeeExOrg exOrg, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{
				await dbContext.EmployeeExOrgs.AddAsync(exOrg);
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
		/// <param name="exOrg">Сотрудник для обновления.</param>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <param name="valueDepartmentID">Идентификатор отдела.</param>
		/// <param name="addInTimeSheetEmployeeExOrg">Флаг добавления в табель.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		public async Task<bool> UpdateEmployeeAndShiftDataExOrgAsync(EmployeeExOrg exOrg,
			DateTime startDate, DateTime endDate, string valueDepartmentID,
			bool addInTimeSheetEmployeeExOrg, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{
				var empExOrg = await dbContext.EmployeeExOrgs
					.Include(x => x.ShiftDataExOrgs
						.Where(e => e.DepartmentID == valueDepartmentID))
					.SingleOrDefaultAsync(e => e.EmployeeExOrgID == exOrg.EmployeeExOrgID);

				if (empExOrg is null)
					return await AddEmployeeExOrgAsync(exOrg, userDataCurrent);

				var shiftDict = empExOrg.ShiftDataExOrgs?.ToDictionary(x => x.WorkDate) ?? [];

				for (var date = startDate; date <= endDate; date = date.AddDays(1))
				{
					if (!shiftDict.ContainsKey(date))
					{
						shiftDict[date] = new ShiftDataExOrg
						{
							EmployeeExOrgID = empExOrg.EmployeeExOrgID,
							WorkDate = date,
							EmployeeExOrg = empExOrg,
							Hours = string.Empty,
							DepartmentID = valueDepartmentID,
						};
					}
				}
				empExOrg.ShiftDataExOrgs = [.. shiftDict.Values];
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
		/// Обновляет данные сотрудника.
		/// </summary>
		/// <param name="exOrg">Сотрудник для обновления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		public async Task<bool> UpdateEmployeeExOrgAsync(EmployeeExOrg exOrg, string valueDepId, bool addWorkInReg, LocalUserData userDataCurrent)
		{
			await using var dbContext = await _context.CreateDbContextAsync();
			await using var trans = await dbContext.Database.BeginTransactionAsync();
			try
			{
				var empExOrg = await dbContext.EmployeeExOrgs
					.Where(x => x.EmployeeExOrgID == exOrg.EmployeeExOrgID)
					.Include(x => x.EmployeeExOrgAddInRegions
						.Where(w => w.DepartmentID == valueDepId && w.EmployeeExOrgID == exOrg.EmployeeExOrgID))
					.FirstOrDefaultAsync();

				if (empExOrg is null)
					return await AddEmployeeExOrgAsync(exOrg, userDataCurrent);
				else
				{
					empExOrg.DateDismissal = exOrg.DateDismissal;
					empExOrg.IsDismissal = exOrg.IsDismissal;
					empExOrg.NumberPass = exOrg.NumberPass;
					empExOrg.FullName = exOrg.FullName;
					empExOrg.ShortName = exOrg.ShortName;
					empExOrg.DateEmployment = exOrg.DateEmployment;
					empExOrg.Photo = exOrg.Photo;
					empExOrg.Descriptions = exOrg.Descriptions;
				}

				if (empExOrg.EmployeeExOrgAddInRegions.Count() > 0)
				{
					empExOrg.EmployeeExOrgAddInRegions
						.Foreach(x =>
						{
							x.WorkingInTimeSheetEmployeeExOrg = addWorkInReg;
						});
				}
				else
				{
					var exOrgInRegDict = empExOrg.EmployeeExOrgAddInRegions.ToDictionary(x => x.EmployeeExOrgID) ?? [];
					if (!exOrgInRegDict.ContainsKey(exOrg.EmployeeExOrgID))
					{
						exOrgInRegDict[exOrg.EmployeeExOrgID] = new EmployeeExOrgAddInRegion
						{
							DepartmentID = valueDepId,
							EmployeeExOrgID = exOrg.EmployeeExOrgID,
							EmployeeExOrg = exOrg,
							WorkingInTimeSheetEmployeeExOrg = addWorkInReg
						};
					}
					empExOrg.EmployeeExOrgAddInRegions = [.. exOrgInRegDict.Values];
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
		/// Получает список сотрудников с переработками для указанных регионов за период.
		/// </summary>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<Employee>> GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(
			LocalUserData userDataCurrent, DateTime startDate, DateTime endDate)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				var list = await dbContext.Employees
					.Where(x => x.DepartmentID.Contains("043") || x.DepartmentID.Contains("044"))
					.Include(i => i.Shifts.Where(r => r.WorkDate >= startDate && r.WorkDate <= endDate))
					.ToListAsync();
				return list;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName).ConfigureAwait(false);
				return [];
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
		public async Task<ObservableCollection<DepartmentProduction>>
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

				return new ObservableCollection<DepartmentProduction>(
					DepartmentProductionsDb ?? []);
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
			List<int> noWorkDaysTO, bool checkingSeeOrWriteBool,
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
							checkingSeeOrWriteBool,
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
		/// Рассчитывает элементы табеля учета рабочего времени для сотрудников Сторонних Организаций.
		/// </summary>
		/// <param name="valueDepartmentID">ID СО отдела предприятия.</param>
		/// <param name="startDate">Дата начала периода.</param>
		/// <param name="endDate">Дата окончания периода.</param>
		/// <param name="itemMonthsTO">Выбранный месяц.</param>
		/// <param name="itemYearsTO">Выбранный год.</param>
		/// <param name="noWorkDaysTO">Список нерабочих дней.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Коллекция элементов табеля.</returns>
		public async Task<ObservableCollection<TimeSheetItemExOrg>> SetDataForTimeSheetExOrgAsync(
			string valueDepartmentID,
			DateTime startDate, DateTime endDate,
			MonthsOrYears itemMonthsTO, MonthsOrYears itemYearsTO,
			List<int> noWorkDaysTO, LocalUserData userDataCurrent)
		{
			try
			{
				await using var dbContext = await _context.CreateDbContextAsync();

				//Временная (на время расчетов) коллекция для сбора всех данных о сотрудниках для табеля
				var tempShifts = new ObservableCollection<TimeSheetItemExOrg>();

				int id = 1;

				//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
				var EmployeeExOrgWithSifts = await dbContext.EmployeeExOrgs
					.Include(i => i.ShiftDataExOrgs
						.Where(s => s.WorkDate >= startDate && s.WorkDate <= endDate && s.DepartmentID == valueDepartmentID))
					.Include(x => x.EmployeeExOrgAddInRegions
						.Where(x => x.DepartmentID == valueDepartmentID && x.WorkingInTimeSheetEmployeeExOrg == true))
					.OrderBy(o => o.FullName)
					.ToListAsync();

				//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
				EmployeeExOrgWithSifts = EmployeeExOrgWithSifts
					.Where(x => x.VolidateEmployee(itemMonthsTO.Id, itemYearsTO.Id))
					.ToList();

				//Циклы в которых,
				//Создаём и заполняем на каждого сотрудника "Пустой" график работ. Который будет заполняться в ручную и автоматически раз в месяц.
				foreach (var employee in EmployeeExOrgWithSifts)
				{
					var shiftDict = employee.ShiftDataExOrgs?.ToDictionary(x => x.WorkDate) ?? [];

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftDataExOrg
							{
								EmployeeExOrgID = employee.EmployeeExOrgID,
								WorkDate = date,
								EmployeeExOrg = employee,
								Hours = string.Empty,
								DepartmentID = valueDepartmentID,
							};
						}
					}
					employee.ShiftDataExOrgs = [.. shiftDict.Values];


					//Конфигурируем данные сотрудника для отображения в табеле
					var itemShift = new TimeSheetItemExOrg(
							id,
							new ShiftDataEmployee
							{
								ShortName = employee.ShortName
							},
							new ObservableCollection<ShiftDataExOrg>(employee.ShiftDataExOrgs),
							noWorkDaysTO);

					//Если сотрудник уволен в выбранном месяце, то его ФИО красятся в красный. Все остальные случаи - в черный
					if (employee.DateDismissal.Month == itemMonthsTO.Id &&
						employee.DateDismissal.Year == itemYearsTO.Id)
						itemShift.Brush = Brushes.Red;
					else
						itemShift.Brush = Brushes.Black;

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
		public async Task SetTotalWorksDaysExOrgAsync(object? sender,
			LocalUserData userDataCurrent)
		{
			try
			{
				if (sender is ShiftDataExOrg shiftData)
				{
					await using var dbContext = await _context.CreateDbContextAsync();
					dbContext.ShiftDataExOrgs?.Update(shiftData);
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

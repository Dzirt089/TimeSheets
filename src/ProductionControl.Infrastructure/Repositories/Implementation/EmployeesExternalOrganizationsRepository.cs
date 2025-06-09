using Microsoft.EntityFrameworkCore;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.Models.Dtos;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.Infrastructure.Repositories.Interfaces;

using System.Collections.ObjectModel;

namespace ProductionControl.Infrastructure.Repositories.Implementation
{
	public class EmployeesExternalOrganizationsRepository : IEmployeesExternalOrganizationsRepository
	{
		private readonly ProductionControlDbContext _context;

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для сотрудников Сторонних Организаций.
		/// </summary>
		/// <param name="valueDepartmentID">ID СО отдела предприятия.</param>
		/// <param name="startDate">Дата начала периода.</param>
		/// <param name="endDate">Дата окончания периода.</param>
		/// <param name="itemMonthsTO">Выбранный месяц.</param>
		/// <param name="itemYearsTO">Выбранный год.</param>
		/// <param name="noWorkDaysTO">Список нерабочих дней.</param>
		/// <returns>Коллекция элементов табеля.</returns>
		public async Task<ObservableCollection<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(
			string valueDepartmentID, DateTime startDate, DateTime endDate,
			MonthsOrYearsDto itemMonthsTO, MonthsOrYearsDto itemYearsTO, List<int> noWorkDaysTO, bool flagAllEmployeeExOrg)
		{
			//Временная (на время расчетов) коллекция для сбора всех данных о сотрудниках для табеля
			var tempShifts = new ObservableCollection<TimeSheetItemExOrgDto>();
			int id = 1;

			List<EmployeeExOrg> EmployeeExOrgWithSifts = [];
			if (flagAllEmployeeExOrg)
			{
				//Выбираем только тех сотрудников, которые принадлежат к датам
				EmployeeExOrgWithSifts = await _context.EmployeeExOrgs
					.Include(i => i.ShiftDataExOrgs
						.Where(s => s.WorkDate >= startDate && s.WorkDate <= endDate))
					.OrderBy(o => o.FullName)
					.ToListAsync();
			}
			else
			{
				//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
				EmployeeExOrgWithSifts = await _context.EmployeeExOrgs
					.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
					.Include(i => i.ShiftDataExOrgs
						.Where(s => s.WorkDate >= startDate &&
												s.WorkDate <= endDate && s.DepartmentID == valueDepartmentID))
					.Include(x => x.EmployeeExOrgAddInRegions
						.Where(x => x.DepartmentID == valueDepartmentID &&
												x.WorkingInTimeSheetEmployeeExOrg == true))
					.OrderBy(o => o.FullName)
					.ToListAsync();
			}

			if (EmployeeExOrgWithSifts.Count == 0) return [];

			//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
			EmployeeExOrgWithSifts = EmployeeExOrgWithSifts
				.Where(x => x.ValidateEmployee(itemMonthsTO.Id, itemYearsTO.Id))
				.ToList();

			//Циклы в которых,
			//Создаём и заполняем на каждого сотрудника "Пустой" график работ. Который будет заполняться в ручную и автоматически раз в месяц.
			foreach (var employee in EmployeeExOrgWithSifts)
			{
				if (flagAllEmployeeExOrg)
				{
					var shiftList = employee.ShiftDataExOrgs
						.GroupBy(x => x.WorkDate)
						.Select(s =>
						{
							var totalHours = s.Sum(g => double
									.TryParse(g.Hours?.Replace(".", ","),
									out double tempResult) ? tempResult : 0);

							return new ShiftDataExOrg()
							{
								EmployeeExOrg = employee,
								WorkDate = s.Key,
								Hours = totalHours == 0 ? string.Empty : totalHours.ToString()
							};
						})
						.ToList();

					if (shiftList is null || !shiftList.Any())
						continue;

					employee.ShiftDataExOrgs = shiftList.AsEnumerable();

					var shiftDict = employee.ShiftDataExOrgs?
						.ToDictionary(x => x.WorkDate) ?? [];

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftDataExOrg()
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
				}
				else
				{
					if (!employee.EmployeeExOrgAddInRegions.Any()) continue;

					var shiftDict = employee.ShiftDataExOrgs?.ToDictionary(x => x.WorkDate) ?? [];

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftDataExOrg()
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
				}

				//Конфигурируем данные сотрудника для отображения в табеле
				var itemShift = new TimeSheetItemExOrgDto(
						id: id,
						fioShiftOverday: new ShiftDataEmployeeDto
						{
							ShortName = employee.ShortName
						},
						workerHours: new ObservableCollection<ShiftDataExOrg>(employee.ShiftDataExOrgs),
						noWorksDay: noWorkDaysTO,
						seeOrWrite: flagAllEmployeeExOrg
						);

				tempShifts.Add(itemShift);
				id++;
			}

			if (!flagAllEmployeeExOrg)
			{
				if (_context.ChangeTracker.HasChanges())
				{
					await _context.SaveChangesAsync();
				}
			}

			return tempShifts;
		}

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(DateTime startDate, DateTime endDate, CancellationToken token)
		{

			var employeeExOrgs = await _context.EmployeeExOrgs
				.AsNoTracking()
				.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
				.Include(i => i.ShiftDataExOrgs
					.Where(w => w.WorkDate >= startDate && w.WorkDate <= endDate))
				.Include(i => i.EmployeeExOrgAddInRegions)
				.OrderBy(o => o.NumCategory)
				.ToListAsync(token);

			return employeeExOrgs;
		}

		public EmployeesExternalOrganizationsRepository(ProductionControlDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Добавляет нового сотрудника.
		/// </summary>
		/// <param name="exOrg">Сотрудник для добавления.</param>
		/// <returns>True, если сотрудник успешно добавлен, иначе False.</returns>
		public async Task AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token)
		{
			await _context.EmployeeExOrgs.AddAsync(exOrg, token);
			await _context.SaveChangesAsync();
		}

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token)
		{
			var listResult = await _context.EmployeeExOrgs
					.AsNoTracking()
					.Include(x => x.EmployeeExOrgAddInRegions)
					.Include(z => z.EmployeePhotos)
					.ToListAsync(token);

			return listResult;
		}

		/// <summary>
		/// Получает список сотрудников за указанный период.
		/// </summary>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <param name="valueDepartmentID">Идентификатор отдела.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(
			DateTime startDate, DateTime endDate, string valueDepartmentID, CancellationToken token)
		{
			var listResult = await _context.EmployeeExOrgs
					.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
					.AsNoTracking()
					.Include(x => x.EmployeeExOrgAddInRegions)
					.Include(x => x.ShiftDataExOrgs
						.Where(r => r.WorkDate >= startDate && r.WorkDate <= endDate && r.DepartmentID == valueDepartmentID))
					.Where(s => s.IsDismissal == false)
					.ToListAsync(token);

			return listResult;
		}

		/// <summary>
		/// Получает список сотрудников с переработками для указанных регионов за период.
		/// </summary>
		/// <param name="startDate">Начальная дата периода.</param>
		/// <param name="endDate">Конечная дата периода.</param>
		/// <returns>Список сотрудников.</returns>
		public async Task<List<EmployeeExOrg>> GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(
			DateTime startDate, DateTime endDate, CancellationToken token)
		{
			//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
			var list = await _context.EmployeeExOrgs
				.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
				.AsNoTracking()
				.Include(i => i.ShiftDataExOrgs
					.Where(s => s.WorkDate >= startDate && s.WorkDate <= endDate && s.DepartmentID.Contains("044")))
				.Include(x => x.EmployeeExOrgAddInRegions
					.Where(x => x.DepartmentID.Contains("044") && x.WorkingInTimeSheetEmployeeExOrg == true))
				.OrderBy(o => o.FullName)
				.ToListAsync(token);

			return list;
		}

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		public async Task SetTotalWorksDaysExOrgAsync(object? sender, CancellationToken token)
		{
			if (sender is ShiftDataExOrg shiftData)
			{
				_context.ShiftDataExOrgs?.Update(shiftData);
				await _context.SaveChangesAsync(token);
			}
		}

		/// <summary>
		/// Обновляет дату увольнения у выбранного сотрудника.
		/// </summary>
		/// <param name="date">Дата увольнения.</param>
		/// <param name="idEmployee">Табельный номер сотрудника.</param>
		/// <param name="userDataCurrent">Текущие данные пользователя.</param>
		/// <returns>Результат обновления.</returns>
		public async Task<bool?> UpdateDismissalDataEmployeeAsync(DateTime date, int idEmployee, CancellationToken token)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.EmployeeExOrgs
				.Include(i => i.ShiftDataExOrgs)
				.SingleOrDefaultAsync(e => e.EmployeeExOrgID == idEmployee, token);

			if (itemEmployee is null) return false;

			//Обнуляем все проставленные наперёд данные по табелю
			itemEmployee?.ShiftDataExOrgs?
				.Where(x => x.WorkDate > date)
				.ToList()
				.ForEach(f =>
				{
					f.Hours = string.Empty;
				});

			//Обновляем дату уволнения
			itemEmployee.DateDismissal = date;
			//Ставим флаг уволнения, после которого нельзя редактировать данные
			itemEmployee.IsDismissal = true;

			//Сохраняем
			await _context.SaveChangesAsync(token);

			return true;
		}

		/// <summary>
		/// Обновляет данные сотрудника.
		/// </summary>
		/// <param name="exOrg">Сотрудник для обновления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		public async Task UpdateEmployeeExOrgAsync(EmployeeExOrg exOrg, string valueDepId, bool addWorkInReg, CancellationToken token)
		{
			var empExOrg = await _context.EmployeeExOrgs
						.Where(x => x.EmployeeExOrgID == exOrg.EmployeeExOrgID)
						.Include(x => x.EmployeeExOrgAddInRegions
							.Where(w => w.DepartmentID == valueDepId && w.EmployeeExOrgID == exOrg.EmployeeExOrgID))
						.Include(z => z.EmployeePhotos)
						.FirstOrDefaultAsync(token);

			if (empExOrg is null)
			{
				await AddEmployeeExOrgAsync(exOrg, token);
			}
			else
			{
				empExOrg.DateDismissal = exOrg.DateDismissal;
				empExOrg.IsDismissal = exOrg.IsDismissal;
				empExOrg.NumberPass = exOrg.NumberPass;
				empExOrg.FullName = exOrg.FullName;
				empExOrg.ShortName = exOrg.ShortName;
				empExOrg.DateEmployment = exOrg.DateEmployment;
				empExOrg.EmployeePhotos = exOrg.EmployeePhotos;
				empExOrg.Descriptions = exOrg.Descriptions;
				empExOrg.NumCategory = exOrg.NumCategory;
			}

			if (empExOrg.EmployeeExOrgAddInRegions.Any())
			{
				empExOrg.EmployeeExOrgAddInRegions
					.Foreach(x =>
					{
						x.WorkingInTimeSheetEmployeeExOrg = addWorkInReg;
					});
			}
			else
			{
				if (addWorkInReg)
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
			}

			await _context.SaveChangesAsync(token);
		}
	}
}

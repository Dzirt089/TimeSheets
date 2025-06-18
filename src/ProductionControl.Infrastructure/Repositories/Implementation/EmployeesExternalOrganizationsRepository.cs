using Microsoft.EntityFrameworkCore;

using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.Infrastructure.Repositories.Interfaces;

using System.Collections.ObjectModel;

namespace ProductionControl.Infrastructure.Repositories.Implementation
{
	public class EmployeesExternalOrganizationsRepository : IEmployeesExternalOrganizationsRepository
	{
		private readonly ProductionControlDbContext _context;

		public EmployeesExternalOrganizationsRepository(ProductionControlDbContext context)
		{
			_context = context;
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
		/// <returns>Коллекция элементов табеля.</returns>
		public async Task<List<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(
			DataForTimeSheetExOrgs dataForTimeSheetEx, CancellationToken token = default)
		{
			//Временная (на время расчетов) коллекция для сбора всех данных о сотрудниках для табеля
			var tempShifts = new List<TimeSheetItemExOrgDto>();
			int id = 1;

			List<EmployeeExOrg> EmployeeExOrgWithSifts = [];
			if (dataForTimeSheetEx.FlagAllEmployeeExOrg)
			{
				//Выбираем только тех сотрудников, которые принадлежат к датам
				EmployeeExOrgWithSifts = await _context.EmployeeExOrgs
					.Include(i => i.ShiftDataExOrgs
						.Where(s => s.WorkDate >= dataForTimeSheetEx.StartDate && s.WorkDate <= dataForTimeSheetEx.EndDate))
					.OrderBy(o => o.FullName)
					.ToListAsync(cancellationToken: token);
			}
			else
			{
				//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
				EmployeeExOrgWithSifts = await _context.EmployeeExOrgs
					.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
					.Include(i => i.ShiftDataExOrgs
						.Where(s => s.WorkDate >= dataForTimeSheetEx.StartDate &&
												s.WorkDate <= dataForTimeSheetEx.EndDate && s.DepartmentID == dataForTimeSheetEx.ValueDepartmentID))
					.Include(x => x.EmployeeExOrgAddInRegions
						.Where(x => x.DepartmentID == dataForTimeSheetEx.ValueDepartmentID &&
												x.WorkingInTimeSheetEmployeeExOrg == true))
					.OrderBy(o => o.FullName)
					.ToListAsync(cancellationToken: token);
			}

			if (EmployeeExOrgWithSifts.Count == 0) return [];

			//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
			EmployeeExOrgWithSifts = EmployeeExOrgWithSifts
				.Where(x => x.ValidateEmployee(
					dataForTimeSheetEx.ItemMonthsTO.Id, dataForTimeSheetEx.ItemYearsTO.Id))
				.ToList();

			//Циклы в которых,
			//Создаём и заполняем на каждого сотрудника "Пустой" график работ. Который будет заполняться в ручную и автоматически раз в месяц.
			foreach (var employee in EmployeeExOrgWithSifts)
			{
				if (dataForTimeSheetEx.FlagAllEmployeeExOrg)
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

					for (var date = dataForTimeSheetEx.StartDate; date <= dataForTimeSheetEx.EndDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftDataExOrg()
							{
								EmployeeExOrgID = employee.EmployeeExOrgID,
								WorkDate = date,
								EmployeeExOrg = employee,
								Hours = string.Empty,
								DepartmentID = dataForTimeSheetEx.ValueDepartmentID,
							};
						}
					}
					employee.ShiftDataExOrgs = [.. shiftDict.Values];
				}
				else
				{
					if (!employee.EmployeeExOrgAddInRegions.Any())
						continue;

					var shiftDict = employee.ShiftDataExOrgs?.ToDictionary(x => x.WorkDate) ?? [];

					for (var date = dataForTimeSheetEx.StartDate; date <= dataForTimeSheetEx.EndDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey(date))
						{
							shiftDict[date] = new ShiftDataExOrg()
							{
								EmployeeExOrgID = employee.EmployeeExOrgID,
								WorkDate = date,
								EmployeeExOrg = employee,
								Hours = string.Empty,
								DepartmentID = dataForTimeSheetEx.ValueDepartmentID,
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
						noWorksDay: dataForTimeSheetEx.NoWorkDaysTO,
						seeOrWrite: dataForTimeSheetEx.FlagAllEmployeeExOrg
						);

				tempShifts.Add(itemShift);
				id++;
			}

			if (!dataForTimeSheetEx.FlagAllEmployeeExOrg)
			{
				if (_context.ChangeTracker.HasChanges())
				{
					await _context.SaveChangesAsync();
				}
			}

			return tempShifts;
		}

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(StartEndDateTime startEndDateTime, CancellationToken token = default)
		{
			var employeeExOrgs = await _context.EmployeeExOrgs
				.AsNoTracking()
				.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
				.Include(i => i.ShiftDataExOrgs
					.Where(w => w.WorkDate >= startEndDateTime.StartDate
										&& w.WorkDate <= startEndDateTime.EndDate))
				.Include(i => i.EmployeeExOrgAddInRegions)
				.OrderBy(o => o.NumCategory)
				.ToListAsync(token);

			return employeeExOrgs;
		}

		/// <summary>
		/// Добавляет нового сотрудника.
		/// </summary>
		/// <param name="exOrg">Сотрудник для добавления.</param>
		/// <returns>True, если сотрудник успешно добавлен, иначе False.</returns>
		public async Task AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token = default)
		{
			await _context.EmployeeExOrgs.AddAsync(exOrg, token);
			await _context.SaveChangesAsync();
		}

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token = default)
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
			StartEndDateTimeDepartmentID startEndDateTimeDepartmentID, CancellationToken token = default)
		{
			var listResult = await _context.EmployeeExOrgs
					.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
					.AsNoTracking()
					.Include(x => x.EmployeeExOrgAddInRegions)
					.Include(x => x.ShiftDataExOrgs
						.Where(r => r.WorkDate >= startEndDateTimeDepartmentID.StartDate
										&& r.WorkDate <= startEndDateTimeDepartmentID.EndDate
										&& r.DepartmentID == startEndDateTimeDepartmentID.ValueDepartmentID))
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
			StartEndDateTime startEndDateTime, CancellationToken token = default)
		{
			//Выбираем только тех сотрудников, которые принадлежат выбранному участку и датам
			var list = await _context.EmployeeExOrgs
				.AsSplitQuery()   //раздельная загрузка каждой включенной коллекции (Include)
				.AsNoTracking()
				.Include(i => i.ShiftDataExOrgs
					.Where(s => s.WorkDate >= startEndDateTime.StartDate
							&& s.WorkDate <= startEndDateTime.EndDate
							&& s.DepartmentID.Contains("044")))
				.Include(x => x.EmployeeExOrgAddInRegions
					.Where(x => x.DepartmentID.Contains("044")
							&& x.WorkingInTimeSheetEmployeeExOrg == true))
				.OrderBy(o => o.FullName)
				.ToListAsync(token);

			return list;
		}

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		public async Task SetTotalWorksDaysExOrgAsync(ShiftDataExOrg shiftDataExOrg, CancellationToken token = default)
		{
			var shifts = await _context.ShiftDataExOrgs
				.Where(x => x.EmployeeExOrgID == shiftDataExOrg.EmployeeExOrgID
					&& x.WorkDate == shiftDataExOrg.WorkDate
					&& x.DepartmentID == shiftDataExOrg.DepartmentID)
				.Include(x => x.EmployeeExOrg)
				.FirstOrDefaultAsync(token);

			if (shifts != null)
			{
				shifts.Hours = shiftDataExOrg.Hours;
				shifts.CodeColor = shiftDataExOrg.CodeColor;

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
		public async Task<bool?> UpdateDismissalDataEmployeeAsync(IdEmployeeExOrgDateTime idEmployeeExOrgDate, CancellationToken token = default)
		{
			//Достаём данные по сотруднику по его табельному номеру
			var itemEmployee = await _context.EmployeeExOrgs
				.Include(i => i.ShiftDataExOrgs)
				.SingleOrDefaultAsync(e => e.EmployeeExOrgID == idEmployeeExOrgDate.IdEmployee, token);

			if (itemEmployee is null) return false;

			//Обнуляем все проставленные наперёд данные по табелю
			itemEmployee?.ShiftDataExOrgs?
				.Where(x => x.WorkDate > idEmployeeExOrgDate.Date)
				.ToList()
				.ForEach(f =>
				{
					f.Hours = string.Empty;
				});

			//Обновляем дату уволнения
			itemEmployee.DateDismissal = idEmployeeExOrgDate.Date;
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
		public async Task UpdateEmployeeExOrgAsync(DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg, CancellationToken token = default)
		{
			var empExOrg = await _context.EmployeeExOrgs
						.Where(x => x.EmployeeExOrgID == dataForUpdateEmloyeeExOrg.ExOrg.EmployeeExOrgID)
						.Include(x => x.EmployeeExOrgAddInRegions
							.Where(w => w.DepartmentID == dataForUpdateEmloyeeExOrg.ValueDepId
												&& w.EmployeeExOrgID == dataForUpdateEmloyeeExOrg.ExOrg.EmployeeExOrgID))
						.Include(z => z.EmployeePhotos)
						.FirstOrDefaultAsync(token);

			if (empExOrg is null)
			{
				await AddEmployeeExOrgAsync(dataForUpdateEmloyeeExOrg.ExOrg, token);
			}
			else
			{
				empExOrg.DateDismissal = dataForUpdateEmloyeeExOrg.ExOrg.DateDismissal;
				empExOrg.IsDismissal = dataForUpdateEmloyeeExOrg.ExOrg.IsDismissal;
				empExOrg.NumberPass = dataForUpdateEmloyeeExOrg.ExOrg.NumberPass;
				empExOrg.FullName = dataForUpdateEmloyeeExOrg.ExOrg.FullName;
				empExOrg.ShortName = dataForUpdateEmloyeeExOrg.ExOrg.ShortName;
				empExOrg.DateEmployment = dataForUpdateEmloyeeExOrg.ExOrg.DateEmployment;
				empExOrg.EmployeePhotos = dataForUpdateEmloyeeExOrg.ExOrg.EmployeePhotos;
				empExOrg.Descriptions = dataForUpdateEmloyeeExOrg.ExOrg.Descriptions;
				empExOrg.NumCategory = dataForUpdateEmloyeeExOrg.ExOrg.NumCategory;
			}

			if (empExOrg.EmployeeExOrgAddInRegions.Any())
			{
				empExOrg.EmployeeExOrgAddInRegions
					.Foreach(x =>
					{
						x.WorkingInTimeSheetEmployeeExOrg = dataForUpdateEmloyeeExOrg.AddWorkInReg;
					});
			}
			else
			{
				if (dataForUpdateEmloyeeExOrg.AddWorkInReg)
				{
					var exOrgInRegDict = empExOrg.EmployeeExOrgAddInRegions.ToDictionary(x => x.EmployeeExOrgID) ?? [];
					if (!exOrgInRegDict.ContainsKey(dataForUpdateEmloyeeExOrg.ExOrg.EmployeeExOrgID))
					{
						exOrgInRegDict[dataForUpdateEmloyeeExOrg.ExOrg.EmployeeExOrgID] = new EmployeeExOrgAddInRegion
						{
							DepartmentID = dataForUpdateEmloyeeExOrg.ValueDepId,
							EmployeeExOrgID = dataForUpdateEmloyeeExOrg.ExOrg.EmployeeExOrgID,
							EmployeeExOrg = dataForUpdateEmloyeeExOrg.ExOrg,
							WorkingInTimeSheetEmployeeExOrg = dataForUpdateEmloyeeExOrg.AddWorkInReg
						};
					}
					empExOrg.EmployeeExOrgAddInRegions = [.. exOrgInRegDict.Values];
				}
			}

			await _context.SaveChangesAsync(token);
		}

		public async Task SaveEmployeeExOrgCardNumsAsync(IEnumerable<EmployeeExOrgCardNumShortNameId> employeeExOrgCards, CancellationToken token = default)
		{
			var ids = employeeExOrgCards
				.Select(x => x.EmployeeExOrgID)
				.ToList();

			var employeeExOrgCardsDict = employeeExOrgCards.ToDictionary(x => x.EmployeeExOrgID);

			var tempEmloyeeExOrgs = await _context.EmployeeExOrgs
				.Where(x => ids.Contains(x.EmployeeExOrgID))
				.ToListAsync(token);

			foreach (var empExOrgDb in tempEmloyeeExOrgs)
			{
				var temp = employeeExOrgCardsDict[empExOrgDb.EmployeeExOrgID];
				empExOrgDb.CardNumber = temp.CardNumber;
			}

			var df = await _context.SaveChangesAsync(token);
		}

		public async Task<IEnumerable<EmployeeExOrgCardNumShortNameId>> GetEmployeeExOrgEmptyCardNumsAsync(CancellationToken token = default)
		{
			var tenpEmployeeExOrgs = await _context.EmployeeExOrgs
				.AsNoTracking()
				.Where(x => string.IsNullOrEmpty(x.CardNumber))
				.ToListAsync(token);

			var result = tenpEmployeeExOrgs.Select(x => new EmployeeExOrgCardNumShortNameId
			{
				EmployeeExOrgID = x.EmployeeExOrgID,
				ShortName = x.ShortName,
				CardNumber = x.CardNumber,
			});

			return result;
		}
	}
}

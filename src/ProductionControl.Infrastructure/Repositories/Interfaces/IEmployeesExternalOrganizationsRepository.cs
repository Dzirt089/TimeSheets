using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.Models.Dtos;

using System.Collections.ObjectModel;

namespace ProductionControl.Infrastructure.Repositories.Interfaces
{
	public interface IEmployeesExternalOrganizationsRepository
	{
		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		Task<ObservableCollection<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(
			string valueDepartmentID, DateTime startDate, DateTime endDate,
			MonthsOrYearsDto itemMonthsTO, MonthsOrYearsDto itemYearsTO, List<int> noWorkDaysTO, bool flagAllEmployeeExOrg);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(DateTime startDate, DateTime endDate, string valueDepartmentID, CancellationToken token);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token);

		Task UpdateEmployeeExOrgAsync(EmployeeExOrg exOrg, string valueDepId, bool addWorkInReg, CancellationToken token);

		Task AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token);

		Task<List<EmployeeExOrg>> GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(
			DateTime startDate, DateTime endDate, CancellationToken token);

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		Task<bool?> UpdateDismissalDataEmployeeAsync(DateTime date, int idEmployee, CancellationToken token);

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		Task SetTotalWorksDaysExOrgAsync(object? sender, CancellationToken token);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(DateTime startDate, DateTime endDate, CancellationToken token);
	}
}

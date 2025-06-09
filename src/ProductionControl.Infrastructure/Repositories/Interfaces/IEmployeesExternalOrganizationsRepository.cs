using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.Infrastructure.Repositories.Interfaces
{
	public interface IEmployeesExternalOrganizationsRepository
	{
		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		Task<List<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(DataForTimeSheetExOrgs dataForTimeSheetEx, CancellationToken token = default);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(StartEndDateTimeDepartmentID startEndDateTimeDepartmentID, CancellationToken token);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token);

		Task UpdateEmployeeExOrgAsync(DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg, CancellationToken token);

		Task AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token);

		Task<List<EmployeeExOrg>> GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(
			StartEndDateTime startEndDateTime, CancellationToken token);

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		Task<bool?> UpdateDismissalDataEmployeeAsync(DateTime date, int idEmployee, CancellationToken token);

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		Task SetTotalWorksDaysExOrgAsync(object? sender, CancellationToken token);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(StartEndDateTime startEndDateTime, CancellationToken token);
	}
}

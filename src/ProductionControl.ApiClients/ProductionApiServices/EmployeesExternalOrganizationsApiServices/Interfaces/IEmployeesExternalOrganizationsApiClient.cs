using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Interfaces
{
	public interface IEmployeesExternalOrganizationsApiClient
	{

		/// <summary>
		/// метод, который сохранит в нашей БД изменения с номером пропуска
		/// </summary>
		/// <returns></returns>
		Task<bool> SaveEmployeeExOrgCardNumsAsync(IEnumerable<EmployeeExOrgCardNumShortNameId> employeeExOrgCards, CancellationToken token = default);

		/// <summary>
		/// метод, который получит из апи нашей БД список всех, у кого поле с номером пропуска пустое
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<EmployeeExOrgCardNumShortNameId>> GetEmployeeExOrgEmptyCardNumsAsync(CancellationToken token = default);

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		Task<List<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(DataForTimeSheetExOrgs dataForTimeSheetEx, CancellationToken token = default);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(StartEndDateTimeDepartmentID startEndDateTimeDepartmentID, CancellationToken token = default);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token = default);

		Task<bool> UpdateEmployeeExOrgAsync(DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg, CancellationToken token = default);

		Task<bool> AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token = default);

		Task<List<EmployeeExOrg>> GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(
			StartEndDateTime startEndDateTime, CancellationToken token = default);

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		Task<bool> UpdateDismissalDataEmployeeAsync(IdEmployeeExOrgDateTime idEmployeeExOrgDate, CancellationToken token = default);

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		Task<bool> SetTotalWorksDaysExOrgAsync(ShiftDataExOrg shiftDataExOrg, CancellationToken token = default);

		Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(StartEndDateTime startEndDateTime, CancellationToken token = default);
	}
}

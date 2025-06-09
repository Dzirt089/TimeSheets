using ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Implementation
{
	public class EmployeesExternalOrganizationsApiClient : BaseApiClient, IEmployeesExternalOrganizationsApiClient
	{
		public EmployeesExternalOrganizationsApiClient(IHttpClientFactory httpClientFactory)
			: base(httpClientFactory.CreateClient("ProductionApi"))
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "EmployeesExternalOrganizations/");
		}

		public async Task<bool> AddEmployeeExOrgAsync(EmployeeExOrg exOrg, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("AddEmployeeExOrg", exOrg, token);

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAllAsync(CancellationToken token = default) =>
			await GetTJsonTAsync<List<EmployeeExOrg>>("GetEmployeeExOrgsAll", token);

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsAsync(StartEndDateTime startEndDateTime, CancellationToken token = default) =>
			await PostTJsonTAsync<List<EmployeeExOrg>>("GetEmployeeExOrgs", startEndDateTime, token);

		public async Task<List<EmployeeExOrg>> GetEmployeeExOrgsOnDateAsync(StartEndDateTimeDepartmentID startEndDateTimeDepartmentID, CancellationToken token = default) =>
			await PostTJsonTAsync<List<EmployeeExOrg>>("GetEmployeeExOrgsOnDate", startEndDateTimeDepartmentID, token);

		public async Task<List<EmployeeExOrg>> GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(StartEndDateTime startEndDateTime, CancellationToken token = default) =>
			await PostTJsonTAsync<List<EmployeeExOrg>>("GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgs", startEndDateTime, token);

		public async Task<List<TimeSheetItemExOrgDto>> SetDataForTimeSheetExOrgAsync(DataForTimeSheetExOrgs dataForTimeSheetEx, CancellationToken token = default) =>
			await PostTJsonTAsync<List<TimeSheetItemExOrgDto>>("SetDataForTimeSheetExOrg", dataForTimeSheetEx, token);

		public async Task<bool> SetTotalWorksDaysExOrgAsync(ShiftDataExOrg shiftDataExOrg, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("SetTotalWorksDaysExOrg", shiftDataExOrg, token);

		public async Task<bool> UpdateDismissalDataEmployeeAsync(IdEmployeeExOrgDateTime idEmployeeExOrgDate, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateDismissalDataEmployee", idEmployeeExOrgDate, token);

		public async Task<bool> UpdateEmployeeExOrgAsync(DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateEmployeeExOrg", dataForUpdateEmloyeeExOrg, token);
	}
}

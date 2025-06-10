using ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;

using System.Text.Json;

namespace ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Implementation
{
	public class EmployeeSheetApiClient : BaseApiClient, IEmployeeSheetApiClient
	{
		public EmployeeSheetApiClient(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions)
			: base(httpClientFactory.CreateClient("ProductionApi"), jsonOptions)
		{
			// Установка базового адреса для клиента этого экземпляра, для работы с группой вызовов апи "EmployeeSheet"
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "EmployeeSheet/");
		}

		public async Task<bool> CancelDismissalEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default) =>
			 await PostTJsonTAsync<bool>("CancelDismissalEmployee", idEmployeeDateTime, token);

		public async Task<bool> CleareDataForFormulateReportForLunchEveryDayDbAsync(CancellationToken token = default) =>
			await DeleteTJsonTAsync<bool>("CleareDataForFormulateReportForLunchEveryDayDb", token);

		public async Task<bool> ClearIdAccessRightFromDepartmentDbAsync(DataClearIdAccessRight dataClearId, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("ClearIdAccessRightFromDepartmentDb", dataClearId, token);

		public bool ClearIdAccessRightFromDepartmentDbSync(DataClearIdAccessRight dataClearId) =>
			PostTJsonT<bool>("ClearIdAccessRightFromDepartmentDb", dataClearId);

		public async Task<bool> ClearLastDeport(DataForClearLastDeport dataForClear, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("ClearLastDeport", dataForClear, token);

		public async Task<List<EmployeeAccessRight>> GetAccessRightsEmployeeAsync(string localUserName, CancellationToken token = default) =>
			await PostTJsonTAsync<List<EmployeeAccessRight>>("GetAccessRightsEmployee", localUserName, token);

		public async Task<List<DepartmentProduction>> GetAllDepartmentsAsync(CancellationToken token = default) =>
			await GetTJsonTAsync<List<DepartmentProduction>>("GetAllDepartments", token);

		public async Task<DepartmentProduction> GetDepartmentProductionAsync(string depId, CancellationToken token = default) =>
			await PostTJsonTAsync<DepartmentProduction>("GetDepartmentProduction", depId, token);

		public async Task<EmployeeAccessRight> GetEmployeeByIdAsync(DepartmentProduction itemDepartment, CancellationToken token = default) =>
			await PostTJsonTAsync<EmployeeAccessRight>("GetEmployeeById", itemDepartment, token);

		public async Task<List<Employee>> GetEmployeeForCartotecasAsync(DepartmentProduction department, CancellationToken token = default) =>
			await PostTJsonTAsync<List<Employee>>("GetEmployeeForCartotecas", department, token);

		public async Task<Employee> GetEmployeeIdAndDateAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default) =>
			await PostTJsonTAsync<Employee>("GetEmployeeIdAndDate", idEmployeeDateTime, token);

		public async Task<List<Employee>> GetEmployeesAsync(StartEndDateTime startEndDate, CancellationToken token = default) =>
			await PostTJsonTAsync<List<Employee>>("GetEmployees", startEndDate, token);

		public async Task<List<Employee>> GetEmployeesForLunchAsync(CancellationToken token = default) =>
			await GetTJsonTAsync<List<Employee>>("GetEmployeesForLunch", token);

		public async Task<List<Employee>> GetEmployeesForReportLunchAsync(StartEndDateTime startEndDate, CancellationToken token = default) =>
			await PostTJsonTAsync<List<Employee>>("GetEmployeesForReportLunch", startEndDate, token);

		public async Task<List<Employee>> GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(StartEndDateTime startEndDate, CancellationToken token = default) =>
			await PostTJsonTAsync<List<Employee>>("GetTotalWorkingHoursWithOverdayHoursForRegions043and044", startEndDate, token);

		public async Task<bool> SetDataEmployeeAsync(Employee employee, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("SetDataEmployee", employee, token);

		public async Task<List<TimeSheetItemDto>> SetDataForTimeSheetAsync(DataForTimeSheet dataForTimeSheet, CancellationToken token = default) =>
			await PostTJsonTAsync<List<TimeSheetItemDto>>("SetDataForTimeSheet", dataForTimeSheet, token);

		public async Task<bool> SetNamesDepartmentAsync(CancellationToken token = default) =>
			await GetTJsonTAsync<bool>("SetNamesDepartment", token);

		public async Task<bool> SetTotalWorksDaysAsync(ShiftData shiftData, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("SetTotalWorksDays", shiftData, token);

		public async Task<string> UpdateDataTableNewEmployeeAsync(DateTime periodDate, CancellationToken token = default) =>
			await PostTJsonTAsync<string>("UpdateDataTableNewEmployee", periodDate, token);

		public async Task<bool> UpdateDepartamentAsync(DepartmentProduction itemDepartment, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateDepartament", itemDepartment, token);

		public async Task<bool> UpdateDismissalDataEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateDismissalDataEmployee", idEmployeeDateTime, token);

		public async Task<int> UpdateEmployeesAsync(List<Employee> allPeople, CancellationToken token = default) =>
			await PostTJsonTAsync<int>("UpdateEmployees", allPeople, token);

		public async Task<bool> UpdateIsLunchingDbAsync(long idEmployee, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateIsLunchingDb", idEmployee, token);

		public async Task<bool> UpdateLunchEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>("UpdateLunchEmployee", idEmployeeDateTime, token);
	}
}

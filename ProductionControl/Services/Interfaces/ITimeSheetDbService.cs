using ProductionControl.Entitys;
using ProductionControl.Models;

using System.Collections.ObjectModel;

namespace ProductionControl.Services.Interfaces
{
	public interface ITimeSheetDbService
	{
		Task<bool> UpdateEmployeeAndShiftDataExOrgAsync(Employee exOrg,
			DateTime startDate, DateTime endDate, string valueDepartmentID,
			bool addInTimeSheetEmployeeExOrg, LocalUserData userDataCurrent);


		Task<bool> UpdateEmployeeExOrgAsync(Employee exOrg, string valueDepId, LocalUserData userDataCurrent);
		Task<bool> AddEmployeeExOrgAsync(Employee exOrg, LocalUserData userDataCurrent);

		Task<List<Employee>> GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(
			LocalUserData userDataCurrent, DateTime startDate, DateTime endDate);
		/// <summary>
		/// Достаём данные по сотруднику по его табельному номеру и дате
		/// </summary>
		/// <returns></returns>
		Task<Employee>? GetEmployeeIdAndDateAsync(long idEmployee,
			DateTime date,
			LocalUserData userDataCurrent);

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		Task<bool?> UpdateDismissalDataEmployeeAsync(
			DateTime date,
			long idEmployee,
			LocalUserData userDataCurrent
			);

		/// <summary>
		/// Отменяем уволнение сотрудника
		/// </summary>
		Task<bool?> CancelDismissalEmployeeAsync(long idEmployee,
			DateTime defaultDateDismissal, LocalUserData userDataCurrent);


		/// <summary>
		/// Отменяем или наоборот, проставляем обед у сотрудника по указанной дате
		/// </summary>
		Task<bool?> UpdateLunchEmployeeAsync(
			long idEmployee, DateTime manualLastDateLunch,
			LocalUserData userDataCurrent);

		/// <summary>
		/// Получаем все данные по участкам, вне зависимости от прав.
		/// Предоставляет список для картотеки.
		/// </summary>
		/// <param name="userDataCurrent"></param>
		/// <returns></returns>
		Task<ObservableCollection<DepartmentProduction>>
			GetAllDepartmentsAsync(LocalUserData userDataCurrent);

		/// <summary>
		/// Получаем данные по сотрудникам, которые не уволенны
		/// </summary>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		/// <param name="department">Выбранный участок предприятия</param>
		Task<ObservableCollection<Employee>>
			GetEmployeeForCartotecasAsync(
			LocalUserData userDataCurrent,
			DepartmentProduction department);

		/// <summary>
		/// Получаем данные по сотрудникам, которые не уволенны
		/// </summary>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		Task<List<Employee>>
		GetEmployeeForCartotecasAsync(
		LocalUserData userDataCurrent);

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		Task<ObservableCollection<TimeSheetItem>> SetDataForTimeSheetAsync(
			DepartmentProduction namesDepartmentItem,
			DateTime startDate, DateTime endDate,
			MonthsOrYears itemMonthsTO, MonthsOrYears itemYearsTO,
			List<int> noWorkDaysTO, bool checkingSeeOrWriteBool,
			LocalUserData userDataCurrent);

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		Task SetTotalWorksDaysAsync(object? sender,
			LocalUserData userDataCurrent);

		/// <summary>
		/// Очищаем данные на сегодня, перед новыми заказами обедов
		/// </summary>
		Task CleareDataForFormulateReportForLunchEveryDayDbAsync(
			LocalUserData userDataCurrent);


		/// <summary>
		/// Если сотрудник обедает, то в его данных отображается инфа, что он кушает, 
		/// и на него заказывается обед
		/// </summary>
		Task<bool> UpdateIsLunchingDbAsync(
			long idEmployee, LocalUserData userDataCurrent);

		/// <summary>
		/// Получаем экземпляр локальных данных на сотрудника.
		/// Имя компьютера и ФИО сотрудника, за которым закреплён комп.
		/// </summary>
		Task<LocalUserData?> GetLocalUserAsync();

		Task SetDataEmployeeAsync(Employee employee, LocalUserData userDataCurrent);
	}
}

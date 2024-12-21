using ProductionControl.Entitys;
using ProductionControl.Models;

using System.Collections.ObjectModel;

namespace ProductionControl.Services.Interfaces
{
	public interface ITimeSheetDbService
	{
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
		Task<bool> UpdateEmployeeAndShiftDataExOrgAsync(Employee emp,
			DateTime startDate, DateTime endDate, string valueDepartmentID,
			bool addInTimeSheetEmployeeExOrg, LocalUserData userDataCurrent);

		/// <summary>
		/// Проверяем, существует ли табельный номер, при создании нового сотрудника
		/// </summary>
		/// <param name="employeeId">Табельный номер сотрудника</param>
		/// <param name="userDataCurrent">Данные текущего пользователя</param>
		/// <returns>True, если совпадение найдено, иначе False</returns>
		Task<bool> CheckingDoubleEmployeeAsync(long employeeId, LocalUserData userDataCurrent);

		/// <summary>
		/// Обновляет данные сотрудника.
		/// </summary>
		/// <param name="emp">Сотрудник для обновления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если данные успешно обновлены, иначе False.</returns>
		Task<bool> UpdateEmployeeAsync(Employee emp, LocalUserData userDataCurrent);

		/// <summary>
		/// Добавляет нового сотрудника.
		/// </summary>
		/// <param name="emp">Сотрудник для добавления.</param>
		/// <param name="userDataCurrent">Данные текущего пользователя.</param>
		/// <returns>True, если сотрудник успешно добавлен, иначе False.</returns>
		Task<bool> AddEmployeeAsync(Employee emp, LocalUserData userDataCurrent);


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
		Task<List<DepartmentProduction>>
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
			List<int> noWorkDaysTO,
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

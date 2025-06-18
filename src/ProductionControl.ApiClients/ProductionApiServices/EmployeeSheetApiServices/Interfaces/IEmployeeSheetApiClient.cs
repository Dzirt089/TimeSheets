using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Interfaces
{
	public interface IEmployeeSheetApiClient
	{

		/// <summary>
		/// метод, который сохранит в нашей БД изменения с номером пропуска
		/// </summary>
		/// <returns></returns>
		Task<bool> SaveEmployeeCardNumsAsync(IEnumerable<EmployeeCardNumShortNameId> employeeCardNums, CancellationToken token = default);

		/// <summary>
		/// метод, который получит из апи нашей БД список всех, у кого поле с номером пропуска пустое
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<EmployeeCardNumShortNameId>> GetEmployeeEmptyCardNumsAsync(CancellationToken token = default);

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		Task<List<Employee>> SetDataForTimeSheetAsync(
			DataForTimeSheet dataForTimeSheet, CancellationToken token = default);

		/// <summary>
		/// Пакетное обновление данных сотрудников
		/// </summary>
		/// <param name="allPeople">Список </param>
		/// <returns></returns>
		Task<int> UpdateEmployeesAsync(List<Employee> allPeople, CancellationToken token = default);

		/// <summary>
		/// Получаем сотрудников для отчёта обедов, которое не уволенные
		/// </summary>
		/// <param name="startDate">Начало периода</param>
		/// <param name="endDate">Конец периода</param>
		/// <returns></returns>
		Task<List<Employee>> GetEmployeesForReportLunchAsync(
		   StartEndDateTime startEndDate, CancellationToken token = default);

		/// <summary>
		/// Получаем сотрудников для заказов каждодневного обеда, которые не уволенны и которые кушают
		/// </summary>
		/// <returns></returns>
		Task<List<Employee>> GetEmployeesForLunchAsync(CancellationToken token = default);

		/// <summary>
		/// заполняет график сотрудника на месяц по его графику из ИС-ПРО
		/// </summary>
		/// <param name="startDate">дата на начало прогнозируемого месяца</param>
		/// <param name="endDate">дата на конец прогнозируемого месяца</param>
		Task<List<Employee>> GetEmployeesAsync(
		   StartEndDateTime startEndDate, CancellationToken token = default);

		Task<bool> GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(
			StartEndDateTime startEndDate, CancellationToken token = default);

		/// <summary>
		/// Достаём данные по сотруднику по его табельному номеру и дате
		/// </summary>
		/// <returns></returns>
		Task<Employee> GetEmployeeIdAndDateAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default);

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		Task<bool> UpdateDismissalDataEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default);

		/// <summary>
		/// Отменяем уволнение сотрудника
		/// </summary>
		Task<bool> CancelDismissalEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default);

		/// <summary>
		/// Отменяем или наоборот, проставляем обед у сотрудника по указанной дате
		/// </summary>
		Task<bool> UpdateLunchEmployeeAsync(IdEmployeeDateTime idEmployeeDateTime, CancellationToken token = default);

		/// <summary>
		/// запрашиваем все данные по доступам.
		/// Затем ищем в данных, есть ли имя локального компьютера в тех данных
		/// Если есть, то получаем список прав
		/// </summary>
		Task<List<EmployeeAccessRight>> GetAccessRightsEmployeeAsync(string localUserName, CancellationToken token = default);

		/// <summary>
		/// Получаем все данные по участкам, вне зависимости от прав.
		/// Предоставляет список для картотеки.
		/// </summary>
		/// <param name="userDataCurrent"></param>
		/// <returns></returns>
		Task<List<DepartmentProduction>> GetAllDepartmentsAsync(CancellationToken token = default);

		/// <summary>
		/// Получаем данные по сотрудникам, которые не уволенны
		/// </summary>
		/// <param name="department">Выбранный участок предприятия</param>
		Task<List<Employee>> GetEmployeeForCartotecasAsync(DepartmentProduction department, CancellationToken token = default);

		/// <summary>
		/// Асинхронно устанавливает названия отделов из БД приложения.
		/// Если их нет, идёт получение данных по ним в ИС-ПРО, с последующим сохранением в нашу БД.
		/// </summary>
		Task<bool> SetNamesDepartmentAsync(CancellationToken token = default);

		/// <summary>
		/// Обновляет таблицу данных, сравнивая и синхронизируя локальные и удаленные данные.
		/// </summary>
		Task<string> UpdateDataTableNewEmployeeAsync(DateTime periodDate, CancellationToken token = default);

		/// <summary>
		/// Синхронный метод по очистке данных при завершении программы
		/// </summary>
		/// <returns></returns>
		Task<bool> ClearIdAccessRightFromDepartmentDbAsync(DataClearIdAccessRight dataClearId, CancellationToken token = default);

		public bool ClearIdAccessRightFromDepartmentDbSync(DataClearIdAccessRight dataClearId);

		/// <summary>
		/// Обработчик вызываемого события, который обновляет данные о сменах в табеле, при непосредственном его изменении
		/// </summary>
		Task<bool> SetTotalWorksDaysAsync(ShiftData shiftData, CancellationToken token = default);

		/// <summary>
		/// Очищаем данные на сегодня, перед новыми заказами обедов
		/// </summary>
		Task<bool> CleareDataForFormulateReportForLunchEveryDayDbAsync(CancellationToken token = default);

		/// <summary>
		/// Если сотрудник обедает, то в его данных отображается инфа, что он кушает, 
		/// и на него заказывается обед
		/// </summary>
		Task<bool> UpdateIsLunchingDbAsync(long idEmployee, CancellationToken token = default);

		/// <summary>
		/// Проверяем при переходе на другой участок, были ли предыдущие в режиме редактирования.
		/// Если да, то проводим проверки и очищаем данные.
		/// </summary>
		Task<bool> ClearLastDeport(DataForClearLastDeport dataForClear, CancellationToken token = default);

		Task<bool> SetDataEmployeeAsync(Employee employee, CancellationToken token = default);

		Task<DepartmentProduction> GetDepartmentProductionAsync(string depId, CancellationToken token = default);

		Task<bool> UpdateDepartamentAsync(DepartmentProduction itemDepartment, CancellationToken token = default);

		Task<EmployeeAccessRight> GetEmployeeByIdAsync(DepartmentProduction itemDepartment, CancellationToken token = default);
	}
}

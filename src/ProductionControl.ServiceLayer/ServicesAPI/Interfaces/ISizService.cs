using ProductionControl.DataAccess.Classes.EFClasses.Sizs;
using ProductionControl.DataAccess.Classes.Models.Dtos;

namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface ISizService
	{
		/// <summary>
		/// Распределение логики выдачи СИЗ-ов
		/// </summary>
		/// <returns></returns>
		Task DivisionLogikalCalcSizAsync(CancellationToken token);

		Task<bool> SetMonthlyValueBeforeReport(CancellationToken token);

		/// <summary>
		/// Асинхронно выполняет обработку данных для списка сотрудников.
		/// </summary>
		/// <param name="completedEmployeesSizs">Список сотрудников с данными SIZ.</param>
		/// <returns>Возвращает true, если обработка прошла успешно, иначе false.</returns>
		Task<bool> RunTNOInBestAsync(List<EmployeeForSizDto> completedEmployeesSizs, CancellationToken token);

		/// <summary>
		/// Инициализация данных для расчета выдачи СИЗ на 1 число месяца, 
		/// сотрудникам (кому положено), без данных за прошлый месяц
		/// </summary>
		/// <returns>Список данных сотрудников с даныыми СИЗ</returns>
		Task<List<EmployeeForSizDto>> InitializeSizForOneDayWithoutBalanceAsync(CancellationToken token);

		/// <summary>
		/// Инициализация списка СИЗ для сотрудника на основании его нормы
		/// </summary>
		/// <param name="tempListSiz">Список норм СИЗ для сотрудника</param>
		/// <returns>Список СИЗ для сотрудника</returns>
		List<DataSizsForSizDto> InitializeSizsUSageFromEmployeeAsync(List<SizUsageRate> tempListSiz);

		/// <summary>
		/// Расчет на выдачу СИЗ, в начале месяца. С учётом остатка срока службы СИЗ, за прошлый месяц  
		/// </summary>
		Task<List<EmployeeForSizDto>> CalculateSizForNewMonthWithBalanceAsync(CancellationToken token);

		/// <summary>
		/// Корректировка отстатков срока службы у СИЗ в конце месяца
		/// </summary>
		/// /// <returns>Список сотрудников с данными СИЗ</returns>
		Task<bool> CalculateSizForLastMonthEndWithBalanceAsync(CancellationToken token);

		/// <summary>
		/// Устанавливаем текущую и последнюю дату
		/// </summary>
		void SetCurrentAndLastDate();

		/// <summary>
		/// Устанавливаем период расчета
		/// </summary>
		void SetCalculationPeriod(int start, int maxDayLast);

		/// <summary>
		/// Рассчитывает использование СИЗ для сотрудника на основе переданных норм и фактически отработанного времени.
		/// </summary>
		/// <param name="tempListSiz">Список норм использования СИЗ для сотрудника.</param>
		/// <param name="listSizsOutputWithOneDayMonth">Список данных о СИЗ за один день в месяц.</param>
		/// <param name="factHoursInShiftAndOverday">Фактически отработанные часы за смену и сверхурочные.</param>
		/// <returns>Список данных о СИЗ для сотрудника.</returns>
		Task<List<DataSizsForSizDto>> CalculateSizsUSageFromEmployee(
			List<SizUsageRate> tempListSiz, List<DataSizForMonth> listSizsOutputWithOneDayMonth, double factHoursInShiftAndOverday, CancellationToken token);

		/// <summary>
		/// Второй расчёт на выдачу СИЗ, в середине месяца. Делаем прогноз на основе проделанной работы с 1 по 14 вкл. число.
		/// </summary>
		Task<List<EmployeeForSizDto>> CalculateSizForFifteenDayWithBalanceAsync(CancellationToken token);

		/// <summary>
		/// Устанавливает текущий месяц и год, и вычисляет начало и конец периода (с 1 по 14 число).
		/// </summary>
		void SetCurrentMonthAndYear();

		/// <summary>
		/// Устанавливает текущие месяц и год на основе текущей даты.
		/// </summary>
		void SetCurrentDate();

		/// <summary>
		/// Рассчитывает использование СИЗ для сотрудника за первую половину месяца на основе переданных норм и фактически отработанного времени.
		/// </summary>
		/// <param name="factHoursInShiftAndOverday">Фактически отработанные часы за смену и сверхурочные.</param>
		/// <param name="hoursInMonth">Количество рабочих часов в месяце.</param>
		/// <param name="listSizsOutputWithOneDayMonth">Список данных о СИЗ за один день в месяц.</param>
		/// <param name="tempListSiz">Список норм использования СИЗ для сотрудника.</param>
		/// <returns>Список данных о СИЗ для сотрудника за первую половину месяца.</returns>
		Task<List<DataSizsForSizDto>> CalculateSizsUSageFromEmployeeFifteenAsync(double factHoursInShiftAndOverday, double hoursInMonth,
			List<DataSizForMonth> listSizsOutputWithOneDayMonth, List<SizUsageRate> tempListSiz, CancellationToken token);

		/// <summary>
		/// Расчет на выдачу СИЗ, в начале месяца. Расчет первичный, без учёта остатка за прошлый месяц  
		/// </summary>
		Task<List<EmployeeForSizDto>> CalculateSizForOneDayInMonthWithoutBalanceAsync(CancellationToken token);

		/// <summary>
		/// Создаем данные о выдаче СИЗ на текущий месяц
		/// </summary>
		Task CreateMonthlySizDataAsync(List<EmployeeForSizDto> employeeForSizsInit, CancellationToken token);
	}
}

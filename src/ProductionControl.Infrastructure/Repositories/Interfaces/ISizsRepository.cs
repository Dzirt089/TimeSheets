using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

namespace ProductionControl.Infrastructure.Repositories.Interfaces
{
	public interface ISizsRepository
	{
		/// <summary>
		/// Получаем полные данные по СИЗ
		/// </summary>
		Task<List<SizUsageRate>> GetSizUsageRateAsync(CancellationToken token);

		/// <summary>
		/// Получение id склада 06-05, для создания заявки в ТНО (ИС-ПРО) о выдачи СИЗ со склада на склад участка.
		/// </summary>
		/// <param name="code">строка шифра склада. например: 06-05</param>
		Task<int> GetWarehouseIDAsync(string code, CancellationToken token);

		/// <summary>
		/// Получение id склада УЧАСТКА, куда будут выдаваться СИЗ-ы со склада 06-05
		/// </summary>
		/// <param name="code">строка шифра участка. например: 03 (техотдел) </param>
		Task<int> GetCodeIDRegionAsync(string code, CancellationToken token);

		/// <summary>
		/// Получение списка выданных СИЗ-ов на сотрудников с первого числа месяца
		/// </summary>
		/// <returns></returns>
		Task<List<DataSizForMonth>> GetAllDataSizForMonthsAsync(CancellationToken token);

		/// <summary>
		/// Получение списка выданных СИЗ-ов для сотрудника
		/// </summary>
		Task<List<DataSizForMonth>> GetDataSizForMonthsAsync(long employeeID, CancellationToken token);

		/// <summary>
		/// Пакетное обновление 
		/// </summary>
		Task UpdateDataSizForMonthAsync(List<DataSizForMonth> datas, CancellationToken token);

		/// <summary>
		/// Пакетное добавление 
		/// </summary>
		Task AddDataSizForMonthAsync(List<DataSizForMonth> datas, CancellationToken token);

		Task<OrderNumberOnDate?> GetOrderNumberOnDateAsync(CancellationToken token);

		Task AddOrderNumberOnDateAsync(OrderNumberOnDate onDate, CancellationToken token);

		Task UpdateOrderNumberOnDateAsync(OrderNumberOnDate onDate, CancellationToken token);

		/// <summary>
		/// Выбор людей для расчёта СИЗ на первое число, у которых есть нормы СИЗ, они не уволенны.
		/// </summary>
		/// <returns></returns>
		Task<List<Employee>> GetEmployeesForSizOneDayAsync(CancellationToken token);


		/// <summary>
		/// Выбор людей для расчёта СИЗ за период дат, у которых есть нормы СИЗ, они не уволенны.
		/// </summary>
		Task<List<Employee>> GetEmployeesForSizFifteenDayAsync(
		   DateTime startDate, DateTime endDate, CancellationToken token);
	}
}

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.Models.Dtos;

namespace ProductionControl.DataAccess.Sql.Interfaces
{

	public interface IDbServices
	{
		/// <summary>
		/// Получаем код (id) склада для формирования заявки ТНО
		/// </summary>
		Task<int> GetrcdBestAsync(string skladBest, CancellationToken token);

		/// <summary>
		/// Получаем норму месяца часов работы на сотрудника, по его графику
		/// </summary>
		Task<double> GetHoursPlanMonhtAsync(string numgraf, DateTime dateTime, CancellationToken token);

		/// <summary>
		/// Записываем данные ТНО из программы САГА во временную таблицу ИС-ПРО
		/// </summary>
		Task UpdateU_PR_TNOAsync(TnoDataDto tno, CancellationToken token);

		/// <summary>
		/// Запуск хранимой процедуры для формирования ТНО в ИС-ПРО
		/// </summary>
		Task<bool> ExecuteMakeTnoAsync(CancellationToken token);

		Task<List<Employee>> GetEmployeesFromBestAsync(string codeRegion, DateTime dateTime, CancellationToken token);

		Task<List<WorkingScheduleDto>> GetGrafAsync(List<string> numGraf, DateTime dateTime, CancellationToken token);

		Task<List<DepartmentProduction>> GetDepartmentProductionsAsync(DateTime dateTime, CancellationToken token);
	}

}

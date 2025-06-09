namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IScheduleForEmployeeService
	{
		/// <summary>
		/// Метод, который заполняет график сотрудника на месяц по его графику из ИС-ПРО
		/// </summary>
		/// <returns></returns>
		Task SetScheduleForEmployee(CancellationToken token);
	}
}

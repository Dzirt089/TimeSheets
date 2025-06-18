using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IMonthlyValuesService
	{
		Task<OrderNumberOnDate> GetValuesAsync(CancellationToken token);

		Task ValidationOrderNumberOnDateAsync(OrderNumberOnDate result, CancellationToken token);

		Task UpdateValuesAsync(OrderNumberOnDate value, CancellationToken token);
	}
}

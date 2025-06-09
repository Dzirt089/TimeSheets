using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	public class MonthlyValuesService(
		IErrorLogger logger,
		ISizsRepository contextServices)
		: IMonthlyValuesService
	{
		public async Task<OrderNumberOnDate> GetValuesAsync(CancellationToken token)
		{
			try
			{
				var orderNumberOnDate = await contextServices.GetOrderNumberOnDateAsync(token);
				if (orderNumberOnDate is null)
				{
					orderNumberOnDate = new()
					{
						OrderNumber = 1,
						MonthValue = DateTime.Now.Month,
						YearValue = DateTime.Now.Year
					};
					await contextServices.AddOrderNumberOnDateAsync(orderNumberOnDate, token);
				}
				await ValidationOrderNumberOnDateAsync(orderNumberOnDate, token);

				return orderNumberOnDate;
			}
			catch (Exception ex)
			{
				await logger.ProcessingErrorLogAsync(ex);
				return new();
			}
		}

		public async Task ValidationOrderNumberOnDateAsync(OrderNumberOnDate result, CancellationToken token)
		{
			try
			{
				bool flagUpdate = false;

				if (result.MonthValue < DateTime.Now.Month)
				{
					result.MonthValue = DateTime.Now.Month;
					result.OrderNumber = 1;
					flagUpdate = true;
				}

				if (result.YearValue < DateTime.Now.Year)
				{
					result.YearValue = DateTime.Now.Year;
					result.OrderNumber = 1;
					flagUpdate = true;
				}

				if (flagUpdate)
					await contextServices.UpdateOrderNumberOnDateAsync(result, token);
			}
			catch (Exception ex)
			{
				await logger.ProcessingErrorLogAsync(ex);
			}
		}

		public async Task UpdateValuesAsync(OrderNumberOnDate value, CancellationToken token)
		{
			try
			{
				if (value != null)
					await contextServices.UpdateOrderNumberOnDateAsync(value, token);
			}
			catch (Exception ex)
			{
				await logger.ProcessingErrorLogAsync(ex);
			}
		}
	}
}

using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

namespace ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Interfaces
{
	public interface ISizEmployeeApiClient
	{
		/// <summary>
		/// Получаем полные данные по СИЗ
		/// </summary>
		Task<List<SizUsageRate>> GetSizUsageRateAsync(CancellationToken token = default);
	}
}

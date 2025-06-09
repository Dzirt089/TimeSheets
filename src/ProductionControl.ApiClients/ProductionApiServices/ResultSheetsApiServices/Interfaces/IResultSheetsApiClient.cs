using ProductionControl.DataAccess.Classes.ApiModels.Dtos;

namespace ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces
{
	public interface IResultSheetsApiClient
	{
		Task<ResultSheetResponseDto> GetDataResultSheetAsync(List<TimeSheetItemDto> copyTimeSheet, CancellationToken token = default);
	}
}

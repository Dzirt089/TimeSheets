using ProductionControl.DataAccess.Classes.Models.Dtos;

namespace ProductionControl.ApiClients.ApiServices.ResultSheetsApiServices.Interfaces
{
	public interface IResultSheetsApiClient
	{
		Task<ResultSheetResponseDto> GetDataResultSheetAsync(List<TimeSheetItemDto> copyTimeSheet, CancellationToken token = default);
	}
}

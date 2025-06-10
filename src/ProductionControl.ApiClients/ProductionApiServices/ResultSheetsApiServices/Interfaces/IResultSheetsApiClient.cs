using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces
{
	public interface IResultSheetsApiClient
	{
		Task<ResultSheetResponseDto> GetDataResultSheetAsync(DataForTimeSheet dataForTimeSheet, CancellationToken token = default);
	}
}

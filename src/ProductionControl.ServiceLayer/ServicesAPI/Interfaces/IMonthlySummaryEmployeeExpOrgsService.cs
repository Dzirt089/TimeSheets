using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IMonthlySummaryEmployeeExpOrgsService
	{
		/// <summary>
		/// Генерирует отчёт по сотрудникам внешних организаций за указанный период,
		/// сохраняет его через IReportService и отправляет ссылку на файл по почте.
		/// </summary>
		/// <param name="_startPeriodString">Дата начала в формате строки (например, "2025-06-01").</param>
		/// <param name="_endPeriodString">Дата окончания в формате строки (например, "2025-06-30").</param>
		/// <param name="token">Токен отмены асинхронной операции.</param>
		/// <returns>
		/// true, если отчёт успешно создан и отправлен; 
		/// false в случае ошибок парсинга дат, отсутствия данных или внутренних исключений.
		/// </returns>
		Task<bool> CreateReportEmployeeExpOrgAsync(StartEndDateTime startEndDate, CancellationToken token);
	}
}

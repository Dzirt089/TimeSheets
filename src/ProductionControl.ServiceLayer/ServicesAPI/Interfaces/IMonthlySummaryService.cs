namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IMonthlySummaryService
	{
		/// <summary>
		/// Загружает данные по сотрудникам за указанный месяц и год, 
		/// заполняет DTO со сводкой по дням и по итоговым часам, 
		/// генерирует файл отчёта и отправляет его по почте.
		/// </summary>
		/// <param name="month">Месяц отчёта (1–12).</param>
		/// <param name="year">Год отчёта.</param>
		/// <param name="token">Токен отмены операции.</param>
		/// <returns>Задача без результата. Исключения логируются внутри.</returns>
		Task GetDataForMonthlySummary(int month, int year, CancellationToken token);
	}
}

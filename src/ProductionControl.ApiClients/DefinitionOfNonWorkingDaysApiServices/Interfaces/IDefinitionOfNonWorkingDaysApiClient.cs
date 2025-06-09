namespace ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Interfaces
{
	public interface IDefinitionOfNonWorkingDaysApiClient
	{
		Task<bool> GetWeekendDayAsync(DateTime date, CancellationToken token = default);

		/// <summary> Получаем список вых. дней в месяце</summary>
		Task<List<int>> GetWeekendsInMonthAsync(int year, int month, CancellationToken token = default);
	}
}

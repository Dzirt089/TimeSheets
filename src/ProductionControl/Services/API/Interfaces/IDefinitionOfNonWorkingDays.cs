namespace ProductionControl.Services.API.Interfaces
{
	public interface IDefinitionOfNonWorkingDays
	{
		Task<bool> GetWeekendDayAsync(DateTime date);
		/// <summary> Получаем список вых. дней в месяце</summary>
		Task<List<int>> GetWeekendsInMonthAsync(int year, int month);
	}
}

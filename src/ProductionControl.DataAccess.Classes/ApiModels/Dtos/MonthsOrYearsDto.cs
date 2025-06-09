namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{

	public class MonthsOrYearsDto
	{
		public MonthsOrYearsDto(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get; set; }
		public string? Name { get; set; }
	}
}

namespace ProductionControl.UIModels.Dtos.Siz
{
	public class SizDto
	{
		public int SizID { get; set; }
		public string? Article { get; set; }
		public string? Name { get; set; }
		public string? Unit { get; set; }

		public IEnumerable<SizUsageRateDto> SizUsageRates { get; set; }

		public SizDto()
		{
			SizUsageRates = new HashSet<SizUsageRateDto>();
		}
	}
}

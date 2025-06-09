namespace ProductionControl.Models.Dtos.Siz
{
	public class UsageNormDto
	{
		public int UsageNormID { get; set; }

		public string? Descriptions { get; set; }

		public IEnumerable<SizUsageRateDto> SizUsageRates { get; set; }

		public UsageNormDto()
		{
			SizUsageRates = new HashSet<SizUsageRateDto>();
		}
	}
}

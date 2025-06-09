namespace ProductionControl.Models.Dtos.Siz
{
	public class SizUsageRateDto
	{
		public int SizUsageRateID { get; set; }
		public int SizID { get; set; }
		public int UsageNormID { get; set; }
		public double HoursPerUnit { get; set; }

		public SizDto? Siz { get; set; }
		public UsageNormDto? UsageNorm { get; set; }
	}
}

namespace ProductionControl.DataAccess.Classes.EFClasses.Sizs
{
	public class SizUsageRate
	{
		public int SizUsageRateID { get; set; }
		public int SizID { get; set; }
		public int UsageNormID { get; set; }
		public double HoursPerUnit { get; set; }

		public Siz? Siz { get; set; }
		public UsageNorm? UsageNorm { get; set; }
	}
}

namespace ProductionControl.DataAccess.Classes.EFClasses.Sizs
{
	public class Siz
	{
		public int SizID { get; set; }
		public string? Article { get; set; }
		public string? Name { get; set; }
		public string? Unit { get; set; }

		public IEnumerable<SizUsageRate> SizUsageRates { get; set; }
		public IEnumerable<DataSizForMonth> DataSizForMonths { get; set; }

		public Siz()
		{
			SizUsageRates = new HashSet<SizUsageRate>();
			DataSizForMonths = new HashSet<DataSizForMonth>();
		}
	}
}

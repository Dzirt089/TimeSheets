using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

namespace ProductionControl.DataAccess.Classes.EFClasses.Sizs
{
	public class UsageNorm
	{
		public int UsageNormID { get; set; }

		public string? Descriptions { get; set; }

		public IEnumerable<SizUsageRate> SizUsageRates { get; set; }
		public IEnumerable<Employee> Employees { get; set; }


		public UsageNorm()
		{
			SizUsageRates = new HashSet<SizUsageRate>();
			Employees = new HashSet<Employee>();
		}
	}
}

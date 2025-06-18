namespace ProductionControl.DataAccess.Classes.EFClasses.Sizs;

public class DataSizForMonth
{
	public int id { get; set; }

	public long EmployeeID { get; set; }

	public int SizID { get; set; }

	public int CountExtradite { get; set; }

	public double LifeTime { get; set; }

	public Siz Siz { get; set; } = null!;
}
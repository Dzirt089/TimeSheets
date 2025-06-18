using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ServiceLayer.PlannedLaborServicesAPI.Interfaces
{
	public interface IPlannedLaborServices
	{
		Task CalcPlannedLaborForRegions043and044EmployeesAndEmployeesExOrg(StartEndDateTime startEndDate, CancellationToken token = default);
	}
}

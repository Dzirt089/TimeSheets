using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys
{
	public class DepartmentProduction
	{

		public string DepartmentID { get; set; }

		/// <summary>Наименование участка, где работает(закреплен) сотрудник</summary>
		public string? NameDepartment { get; set; }

		[NotMapped]//Это св-во не вносим в БД, оно вспомогательное в коде
		public string? FullNameDepartment { get; set; }

		public IEnumerable<EmployeeExOrg> EmployeeExOrgs { get; set; }

		public EmployeeAccessRight EmployeeAccessRight { get; set; }

		public IEnumerable<Employee>? EmployeesList { get; set; }

		/// <summary>
		/// Обозначение индентификатора из <see cref="EmployeeAccessRight"/>
		/// </summary>
		public int AccessRight { get; set; }

		public DepartmentProduction()
		{
			// чтобы разработчики могли добавлять продукты в категорию,
			// мы должны инициализировать свойство навигации в пустую коллекцию
			EmployeesList = new HashSet<Employee>();
			EmployeeExOrgs = new HashSet<EmployeeExOrg>();
		}
	}
}

using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.Models.ExternalOrganization;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionControl.Models
{
	public class DepartmentProduction : ObservableObject
	{

		public string DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}
		private string _departmentID;

		/// <summary>Наименование участка, где работает(закреплен) сотрудник</summary>
		public string? NameDepartment
		{
			get => _nameDepartment;
			set
			{
				SetProperty(ref _nameDepartment, value);
				FullNameDepartment = DepartmentID + NameDepartment;
			}
		}
		private string? _nameDepartment;

		public IEnumerable<EmployeeExOrg> EmployeeExOrgs
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private IEnumerable<EmployeeExOrg> _employeeExOrg;

		/// <summary>Наименование участка, где работает(закреплен) сотрудник</summary>
		[NotMapped]//Это св-во не вносим в БД, оно вспомогательное в коде
		public string? FullNameDepartment
		{
			get => _fullNameDepartment;
			set => SetProperty(ref _fullNameDepartment, value);
		}
		private string? _fullNameDepartment;	  						 	

		public IEnumerable<Employee>? EmployeesList
		{
			get => _employeesList;
			set
			{
				SetProperty(ref _employeesList, value);
			}
		}
		private IEnumerable<Employee>? _employeesList;

		/// <summary>
		/// Обозначение индентификатора из <see cref="EmployeeAccessRight"/>
		/// </summary>
		public int AccessRight { get => _accessRight; set => SetProperty(ref _accessRight, value); }
		private int _accessRight;

		

		public DepartmentProduction()
		{
			// чтобы разработчики могли добавлять продукты в категорию,
			// мы должны инициализировать свойство навигации в пустую коллекцию
			EmployeesList = new HashSet<Employee>();
			EmployeeExOrgs = new HashSet<EmployeeExOrg>();
		}
	}
}

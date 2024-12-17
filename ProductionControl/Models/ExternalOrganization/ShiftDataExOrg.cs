using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace ProductionControl.Models.ExternalOrganization
{
	public class ShiftDataExOrg : ObservableObject
	{
		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		public EmployeeExOrg EmployeeExOrg
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private EmployeeExOrg _employeeExOrg;

		/// <summary>
		/// Дата табеля
		/// </summary>
		public DateTime WorkDate
		{
			get => _workDate;
			set => SetProperty(ref _workDate, value);
		}
		private DateTime _workDate;

		/// <summary>
		/// Часы отработанные в смене, вкл переработки
		/// </summary>
		public string? Hours
		{
			get => _hours;
			set
			{
				if (Validation())
					SetProperty(ref _hours, value);
			}
		}
		private string? _hours;

		public string DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}
		private string _departmentID;
		
		/// <summary>
		/// Цвет для окраски ФИО сотрудника в приложении Табеля. Красный - если уволен в выбранном месяце. Во всех остальных случаях - черный
		/// </summary>
		[NotMapped]
		public Brush Brush
		{
			get => _brush;
			set => SetProperty(ref _brush, value);
		}
		private Brush _brush;


		/// <summary>
		/// метод валидации, где блокируется внесение данных в график сотрудника, если он уволен или ещё не принят на работу
		/// </summary>
		private bool Validation()
		{
			if (EmployeeExOrg.IsDismissal) //Если уволенный сотрудник
			{
				return false;
			}
			else if (EmployeeExOrg.DateEmployment > WorkDate)
				return false;
			else
				return true;
		}
	}
}

using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.UIModels.Model.GlobalPropertys;

using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;



namespace ProductionControl.UIModels.Dtos.ExternalOrganization
{
	public class ShiftDataExOrgDto : ObservableObject
	{
		private GlobalSettingsProperty _globalProperty;
		public ShiftDataExOrgDto() { }
		public ShiftDataExOrgDto(GlobalSettingsProperty globalProperty) { _globalProperty = globalProperty; }


		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		public EmployeeExOrgDto EmployeeExOrg
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private EmployeeExOrgDto _employeeExOrg;

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
				bool temp = false;

				if (_globalProperty?.FlagAllEmployeeExOrg != null)
					temp = true;
				if (value != null)
					value = value.Replace('.', ',');
				if (Validation() || temp)
				{
					SetProperty(ref _hours, value);
				}

				Brush brush = this.GetBrushARGB();
				Brush = brush;
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
		/// Цвет для окраски часов сотрудника в приложении Табеля (вкладка СО). Красный - ночная смена. Дневная - черный.
		/// </summary>		
		[NotMapped]
		public Brush Brush
		{
			get => _brush;
			set
			{
				SetProperty(ref _brush, value);
				if (Brush != null)
				{
					CodeColor = Brush switch
					{
						_ when Brush == Brushes.Red => 1,
						_ when Brush == Brushes.Black => 2,
						_ => 2
					};
				}
			}
		}
		private Brush _brush;

		public byte? CodeColor
		{
			get => _codeColor;
			set => SetProperty(ref _codeColor, value);
		}
		private byte? _codeColor;


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

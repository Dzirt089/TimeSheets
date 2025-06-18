namespace ProductionControl.UIModels.Model.GlobalPropertys
{
	public class GlobalSettingsProperty
	{
		/// <summary>
		/// Флаг означает, если он в true - то загружаются для табеля все участки у сотруднико сторонних организаций, для вкладки все сотрудники СО (она выполняет операцию "сводной таблицы" у сторонников для Наливайко Н.Б.
		/// Если false, то выбираются данные для табеля только по указанному участку и датам.
		/// </summary>
		public bool FlagAllEmployeeExOrg { get; set; } = false;
	}
}

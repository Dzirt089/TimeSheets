namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	public class DataSizsForSizDto
	{
		/// <summary>
		/// Норма выдачи СИЗ (его ID)
		/// </summary>
		public int UsageNormID { get; set; }

		/// <summary>
		/// ID из табл Sizs
		/// </summary>
		public int SizID { get; set; }

		/// <summary>Артикул СИЗ-а</summary>
		public string? Article { get; set; }

		/// <summary>Наименование СИЗ-а</summary>
		public string? Name { get; set; }

		/// <summary>Ед. Изм. СИЗ-а</summary>
		public string? Unit { get; set; }

		/// <summary>Срок службы в часах, СИЗ</summary>
		public double HoursPerUnit { get; set; }

		/// <summary>Общий срок службы СИЗ-а (кол-во * HoursPerUnit)</summary>
		public double TotalHoursLive { get; set; }

		/// <summary>кол-во для примерного понимания остатка, не в часах, а в кол-ве (2шт по мере износа => 1.2 шт ит.д.)</summary>
		public int Count { get; set; }

		/// <summary>Прогноз кол-ва для выдачи СИЗ-а сотруднику</summary>
		//public double AnaliticsOutputSiz { get; set; }
	}
}

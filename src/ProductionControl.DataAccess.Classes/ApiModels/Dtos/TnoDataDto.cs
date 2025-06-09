namespace ProductionControl.DataAccess.Classes.Models.Dtos
{
	/// <summary>
	/// Модель ТНО
	/// </summary>
	public record TnoDataDto
	{
		/// <summary>
		/// Id склада отправителя
		/// </summary>
		public int RcdSkladOut { get; set; }
		/// <summary>
		/// Id склада\участка получателя
		/// </summary>
		public int RcdSkladIn { get; set; }
		/// <summary>
		/// Время записи в формате 'dd.mm.yyyy'
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// Артикул детали
		/// </summary>
		public string? Art { get; set; }
		/// <summary>
		/// Кол-во деталей
		/// </summary>
		public double Count { get; set; }
		/// <summary>
		/// Комментарий, для какого отдела идет ТНО
		/// </summary>
		public string? Description { get; set; }
	}
}

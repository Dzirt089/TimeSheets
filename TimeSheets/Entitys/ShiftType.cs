using TimeSheets.Models;

namespace TimeSheets.Entitys
{
	/// <summary>
	/// Шаблон - конструктор для смены на производстве. 
	/// Включает в себя кол-во часов и краткое обозначение смены в табеле
	/// </summary>
	public class ShiftType : EnumerationShift
	{

		/// !!!!!ВНИМАНИЕ!!!!!
		/// 1) При добавлении нового графика здесь <see cref="ShiftType"/>, 
		/// 2) обязательно нужно внести новые данные в <see cref="ShiftData.CheckShiftAndGetHours"/>
		/// 4) Потом в статическом классе <see cref="ExpansionMethodsUtils"/> 
		///		в методы расширения: 
		///		<see cref="GetDaysHours"/>,
		///		<see cref="GetNightHours"/>, 
		///		<see cref="GetShiftHours"/>, 
		///		<see cref="GetNightHoursBool"/>


		/// <summary>Первая Смена</summary>
		public static ShiftType FirstShift = new(1, "8", "1", 8, 0);
		/// <summary>Вторая Смена</summary>
		public static ShiftType SecondShift = new(2, "8", "2", 5, 3);
		/// <summary>Третья Смена</summary>
		public static ShiftType ThirdShift = new(3, "8", "3", 1, 7);
		/// <summary>Ночная Смена</summary>
		public static ShiftType NightShift = new(4, "10,5", "н", 4, 7);
		/// <summary>Дневная Смена</summary>
		public static ShiftType DayShift = new(5, "11,5", "д", 11.5, 0);
		/// <summary>Командировка</summary>
		public static ShiftType BusinessTrip = new(6, "8", "к", 8, 0);
		/// <summary>Больничный</summary>
		public static ShiftType SickLeave = new(7, "Б", "Б", 0, 0);
		/// <summary>Отпуск</summary>
		public static ShiftType Vacation = new(8, "ОТ", "ОТ", 0, 0);
		/// <summary>Не явка неизвестна</summary>
		public static ShiftType NoShowUnknown = new(9, "НН", "НН", 0, 0);
		/// <summary>Смена сутки через трое</summary>
		public static ShiftType Hours24 = new(10, "24", "С", 16, 8);
		/// <summary>Работа по совместительству на не полный рабочий день</summary>
		public static ShiftType Мoonlighting = new(11, "4", "4", 4, 0);
		/// <summary>График с семи-часовым рабочим днём </summary>
		public static ShiftType Hours7 = new(12, "7", "7", 7, 0);
		/// <summary>График с 5-часовым рабочим днём </summary>
		public static ShiftType Hours5 = new(13, "5", "5", 5, 0);
		/// <summary>административный отпуск</summary>
		public static ShiftType AdministrativeLeave = new(14, "АД", "АД", 0, 0);
		/// <summary>административный отпуск v2</summary>
		public static ShiftType AdministrativeLeavev2 = new(15, "ДО", "ДО", 0, 0);
		/// <summary>Демобилизованный на СВО</summary>
		public static ShiftType Demobilized = new(16, "ПД", "ПД", 0, 0);
		/// <summary>Отпуск по уходу за ребенком</summary>
		public static ShiftType ParentalLeave = new(17, "МО", "МО", 0, 0);
		/// <summary>Отпуск по уходу за инвалидом</summary>
		public static ShiftType InvalidLeave = new(18, "ОВ", "ОВ", 0, 0);

		/// <summary>
		/// Можем создавать и другие смены через открытый конструктор
		/// </summary>
		/// <param name="id">ID новой позиции</param>
		/// <param name="hoursCount">Кол-во рабочих часов в смене</param>
		/// <param name="shift">Короткое обозначение смены</param>
		/// <param name="dayHours">Дневные часы в смене</param>
		/// <param name="nightHours">Ночные часы в смене</param>
		public ShiftType(int id, string hoursCount, string shift, double dayHours, double nightHours) :
			base(id, hoursCount, shift, dayHours, nightHours)
		{
		}
	}
}

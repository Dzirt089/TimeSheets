using System.Reflection;

namespace ProductionControl.DataAccess.Classes.ApiModels.Model
{
	/// <summary>
	/// Класс перечесления вместо типов перечисления, скорректирован для учета рабочего времени (Табель ТО)
	/// Шаблон взят от сюда https://learn.microsoft.com/ru-ru/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
	/// </summary>
	public abstract class EnumerationShift : IComparable
	{
		/// <summary>
		/// Кол-во дневных часов в смене
		/// </summary>
		public double DayHours { get; private set; }
		/// <summary>
		/// Кол-во ночных часов в смене
		/// </summary>
		public double NightHours { get; private set; }
		/// <summary>
		/// Короткое обозначение смены
		/// </summary>
		public string ShiftType { get; private set; }
		/// <summary>
		/// Общее кол-во часов в смене
		/// </summary>
		public string HoursCount { get; private set; }
		public int Id { get; private set; }
		protected EnumerationShift(int id, string hoursCount, string shifttype, double dayHours, double nightHours) =>
			(Id, HoursCount, ShiftType, DayHours, NightHours) = (id, hoursCount, shifttype, dayHours, nightHours);
		//public override string ToString() => $"{HoursCount}";

		public static IEnumerable<T> GetAll<T>() where T : EnumerationShift =>
			typeof(T).GetFields(BindingFlags.Public |
								BindingFlags.Static |
								BindingFlags.DeclaredOnly)
			.Select(f => f.GetValue(null))
			.Cast<T>();

		public override bool Equals(object? obj)
		{
			if (obj is not EnumerationShift otherValue) return false;

			var typeMatches = GetType().Equals(obj.GetType());
			var valueMathes = Id.Equals(otherValue.Id);

			return typeMatches && valueMathes;
		}

		public int CompareTo(object other) => Id.CompareTo(((EnumerationShift)other).Id);
	}
}

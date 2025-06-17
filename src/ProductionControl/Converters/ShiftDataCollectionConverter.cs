using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;

using System.Collections.ObjectModel;

namespace ProductionControl.Converters
{
	// Конвертер для ObservableCollection<ShiftData> → ObservableCollection<ShiftDataDto>
	public class ShiftDataCollectionConverter : ITypeConverter<ObservableCollection<ShiftData>, ObservableCollection<ShiftDataDto>>
	{
		public ObservableCollection<ShiftDataDto> Convert(ObservableCollection<ShiftData> source, ObservableCollection<ShiftDataDto> destination, ResolutionContext context)
		{
			var result = new ObservableCollection<ShiftDataDto>();

			foreach (var item in source)
			{
				var dto = context.Mapper.Map<ShiftDataDto>(item);

				// Явно устанавливаем Employee из контекста
				if (item.Employee != null && dto.Employee == null)
				{
					dto.Employee = context.Mapper.Map<EmployeeDto>(item.Employee);
				}

				result.Add(dto);
			}

			return result;
		}
	}
}

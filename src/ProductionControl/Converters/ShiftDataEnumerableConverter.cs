using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;

namespace ProductionControl.Converters
{
	// Конвертер для IEnumerable<ShiftData> → IEnumerable<ShiftDataDto>
	public class ShiftDataEnumerableConverter : ITypeConverter<IEnumerable<ShiftData>, IEnumerable<ShiftDataDto>>
	{
		public IEnumerable<ShiftDataDto> Convert(IEnumerable<ShiftData> source, IEnumerable<ShiftDataDto> destination, ResolutionContext context)
		{
			if (source == null)
				return null;

			var result = new List<ShiftDataDto>();
			foreach (var item in source)
			{
				if (item == null)
				{
					// либо пропускаем, либо добавляем null
					// result.Add(null);
					continue;
				}

				var dto = context.Mapper.Map<ShiftDataDto>(item);

				// если Employee мапится в null, но исходный не null — заполним вручную
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

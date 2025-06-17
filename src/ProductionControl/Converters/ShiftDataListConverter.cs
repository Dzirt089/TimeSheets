using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;

namespace ProductionControl.Converters
{
	public class ShiftDataListConverter : ITypeConverter<IEnumerable<ShiftData>, IEnumerable<ShiftDataDto>>
	{
		public IEnumerable<ShiftDataDto> Convert(IEnumerable<ShiftData> source, IEnumerable<ShiftDataDto> destination, ResolutionContext context)
		{
			var result = new List<ShiftDataDto>();
			foreach (var item in source)
			{
				var dto = context.Mapper.Map<ShiftDataDto>(item);
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

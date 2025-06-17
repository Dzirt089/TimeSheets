using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;

namespace ProductionControl.Converters
{
	public class ShiftDataDtoEnumerableConverter : ITypeConverter<IEnumerable<ShiftDataDto>, IEnumerable<ShiftData>>
	{
		public IEnumerable<ShiftData> Convert(IEnumerable<ShiftDataDto> source, IEnumerable<ShiftData> destination, ResolutionContext context)
		{
			//return source.Select(item => context.Mapper.Map<ShiftData>(item)).ToList();
			var result = new List<ShiftData>();
			foreach (var item in source)
			{
				result.Add(context.Mapper.Map<ShiftData>(item));
			}
			return result;
		}
	}
}

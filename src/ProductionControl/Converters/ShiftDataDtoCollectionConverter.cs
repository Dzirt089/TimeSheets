using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;

using System.Collections.ObjectModel;

namespace ProductionControl.Converters
{
	// Аналогичные конвертеры для обратного маппинга
	public class ShiftDataDtoCollectionConverter : ITypeConverter<ObservableCollection<ShiftDataDto>, ObservableCollection<ShiftData>>
	{
		public ObservableCollection<ShiftData> Convert(ObservableCollection<ShiftDataDto> source, ObservableCollection<ShiftData> destination, ResolutionContext context)
		{
			var result = new ObservableCollection<ShiftData>();
			foreach (var item in source)
			{
				result.Add(context.Mapper.Map<ShiftData>(item));
			}
			return result;
		}
	}
}

using AutoMapper;

using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;
using ProductionControl.UIModels.Dtos.EmployeesFactory;
using ProductionControl.UIModels.Dtos.ExternalOrganization;
using ProductionControl.UIModels.Dtos.Siz;
using ProductionControl.UIModels.Model.EmployeesFactory;
using ProductionControl.UIModels.Model.ExternalOrganization;

using System.Collections.ObjectModel;

namespace ProductionControl.UIModels
{

	public class EmployeesFactoryProfile : Profile
	{
		public EmployeesFactoryProfile()
		{
			#region Основные маппинги
			CreateMap<MonthsOrYears, MonthsOrYearsDto>().ReverseMap();

			CreateMap<WorkingSchedule, WorkingScheduleDto>().ReverseMap();

			CreateMap<TimeSheetItemDto, TimeSheetItem>()
				.ForMember(dest => dest.WorkerHours, opt => opt.MapFrom((src, _, _, ctx) =>
					src.WorkerHours == null
					? null
					: ctx.Mapper.Map<ObservableCollection<ShiftData>>(src.WorkerHours)))
				.ForMember(dest => dest.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday))
				.ForMember(dest => dest.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday))
				.ForMember(dest => dest.Brush, opt => opt.Ignore())
				.AfterMap((src, dest) =>
				{
					dest.SetTotalWorksDays();
					dest.SetCalendarDayAndHours();
				})
				.ReverseMap()
				.ForMember(dest => dest.WorkerHours, opt => opt.MapFrom((src, _, _, ctx) =>
					src.WorkerHours == null
						? null
						: ctx.Mapper.Map<ObservableCollection<ShiftData>>(src.WorkerHours)))
				.ForMember(dest => dest.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday))
				.ForMember(dest => dest.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday));

			CreateMap<ShiftDataEmployeeDto, ShiftDataEmployee>()
				.ForMember(dest => dest.ShortName, opt => opt.MapFrom(src => src.ShortName))
				.ForMember(dest => dest.NameShift, opt => opt.MapFrom(src => src.NameShift))
				.ForMember(dest => dest.NameOverday, opt => opt.MapFrom(src => src.NameOverday))
				.ReverseMap();

			CreateMap<ShiftData, ShiftDataDto>()
				// Сначала маппим Employee
				.ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src.Employee))
				.ForMember(dest => dest.Brush, opt => opt.Ignore())
				// Затем остальные свойства
				.ForMember(dest => dest.WorkDate, opt => opt.MapFrom(src => src.WorkDate))
				.ForMember(dest => dest.IsPreHoliday, opt => opt.MapFrom(src => src.IsPreHoliday))
				.ForMember(dest => dest.IsHaveLunch, opt => opt.MapFrom(src => src.IsHaveLunch))
				.ForMember(dest => dest.Shift, opt => opt.Ignore())  // Используем AfterMap для сложных вычислений
				.ForMember(dest => dest.Hours, opt => opt.Ignore())  // Используем AfterMap для сложных вычислений
				.ForMember(dest => dest.Overday, opt => opt.Ignore())  // Используем AfterMap для сложных вычислений
				.AfterMap((src, dest) =>
				{
					// Убедиться, что Employee установлен до установки Shift
					if (dest.Employee != null)
					{
						dest.Shift = src.Shift;
						dest.Hours = src.Hours;  // Теперь безопасно
						dest.Overday = src.Overday;  // Теперь безопасно

						if (!string.IsNullOrEmpty(dest.Shift))
							dest.Brush = dest.Shift.GetBrush(); // Установка цвета на основе Shift
					}
				})
				.ReverseMap();


			#endregion

			#region Сложные маппинги с коллекциями

			// Employee Mapping
			CreateMap<Employee, EmployeeDto>()
				.ForMember(dest => dest.DepartmentProduction, opt => opt.MapFrom(src => src.DepartmentProduction))
				.ForMember(dest => dest.Shifts, opt => opt.MapFrom(src => src.Shifts))
				.AfterMap((src, dest, context) => // Добавьте третий параметр ResolutionContext
				{
					if (src.Shifts != null)
					{
						dest.Shifts = context.Mapper.Map<IEnumerable<ShiftDataDto>>(src.Shifts);
					}
				})
				.ReverseMap();

			// EmployeeAccessRight <-> EmployeeAccessRightDto
			CreateMap<EmployeeAccessRight, EmployeeAccessRightDto>()
				.ReverseMap();

			// DepartmentProduction <-> DepartmentProductionDto
			CreateMap<DepartmentProduction, DepartmentProductionDto>()
				.ForMember(dest => dest.EmployeesList, opt =>
					opt.MapFrom(src => src.EmployeesList))
				.ForMember(dest => dest.EmployeeAccessRight, opt =>
					opt.MapFrom(src => src.EmployeeAccessRight))
				.ReverseMap();



			#endregion

			#region Коллекции

			// ObservableCollection<ShiftData> → ObservableCollection<ShiftDataDto>
			CreateMap<ObservableCollection<ShiftData>, ObservableCollection<ShiftDataDto>>()
				.ConvertUsing<ShiftDataCollectionConverter>();

			// ObservableCollection<ShiftDataDto> → ObservableCollection<ShiftData>
			CreateMap<ObservableCollection<ShiftDataDto>, ObservableCollection<ShiftData>>()
				.ConvertUsing<ShiftDataDtoCollectionConverter>();

			// IEnumerable<ShiftData> → IEnumerable<ShiftDataDto>
			CreateMap<IEnumerable<ShiftData>, IEnumerable<ShiftDataDto>>()
				.ConvertUsing<ShiftDataEnumerableConverter>();

			// IEnumerable<ShiftDataDto> → IEnumerable<ShiftData>
			CreateMap<IEnumerable<ShiftDataDto>, IEnumerable<ShiftData>>()
				.ConvertUsing<ShiftDataDtoEnumerableConverter>();

			#endregion

			#region SIZ

			CreateMap<Siz, SizDto>().ReverseMap();
			CreateMap<SizUsageRate, SizUsageRateDto>().ReverseMap();
			CreateMap<UsageNorm, UsageNormDto>().ReverseMap();

			#endregion

			#region Внешние организации

			CreateMap<EmployeeExOrgAddInRegion, EmployeeExOrgAddInRegionDto>().ReverseMap();
			CreateMap<EmployeeExOrg, EmployeeExOrgDto>()
				.ForMember(dest => dest.ShiftDataExOrgs, opt => opt.Ignore())
				.ReverseMap()
				.ForMember(dest => dest.ShiftDataExOrgs, opt => opt.Ignore());
			CreateMap<EmployeePhoto, EmployeePhotoDto>().ReverseMap();

			CreateMap<ShiftDataExOrg, ShiftDataExOrgDto>()
				.ForMember(d => d.Brush, opt => opt.Ignore())
				.ForMember(d => d.Hours, opt => opt.Ignore())
				.ForMember(d => d.CodeColor, opt => opt.Ignore())
				.ForMember(d => d.EmployeeExOrg, opt => opt.MapFrom(s => s.EmployeeExOrg))
				.AfterMap((src, dst) =>
				{
					if (src.Hours != null)
					{
						dst.Hours = src.Hours;
					}
				})
				.ReverseMap()
				.ForMember(d => d.EmployeeExOrg, opt => opt.Ignore());

			CreateMap<TimeSheetItemExOrgDto, TimeSheetItemExOrg>()
				.ForMember(d => d.Brush, opt => opt.Ignore())
				.ForMember(d => d.WorkerHours, opt => opt.MapFrom((src, dest, _, ctx) =>
				{
					if (src.WorkerHours == null) return null;
					return new ObservableCollection<ShiftDataExOrgDto>(
						ctx.Mapper.Map<IEnumerable<ShiftDataExOrgDto>>(src.WorkerHours)
					);
				}))
				.AfterMap((src, dst) =>
				{
					dst.SetTotalWorksDays();
					dst.SetCalendarDayAndHours();
				})
				.ReverseMap()
				.ForMember(d => d.WorkerHours, opt => opt.MapFrom((src, dest, _, ctx) =>
				{
					if (src.WorkerHours == null) return null;
					return ctx.Mapper.Map<IEnumerable<ShiftDataExOrgDto>>(src.WorkerHours);
				}));

			#endregion
		}
	}

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

	// Конвертер для IEnumerable<ShiftData> → IEnumerable<ShiftDataDto>
	public class ShiftDataEnumerableConverter
	: ITypeConverter<IEnumerable<ShiftData>, IEnumerable<ShiftDataDto>>
	{
		public IEnumerable<ShiftDataDto> Convert(
			IEnumerable<ShiftData> source,
			IEnumerable<ShiftDataDto> destination,
			ResolutionContext context)
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

	public class ShiftDataDtoEnumerableConverter : ITypeConverter<IEnumerable<ShiftDataDto>, IEnumerable<ShiftData>>
	{
		public IEnumerable<ShiftData> Convert(IEnumerable<ShiftDataDto> source, IEnumerable<ShiftData> destination, ResolutionContext context)
		{
			return source.Select(item => context.Mapper.Map<ShiftData>(item)).ToList();
		}
	}
}

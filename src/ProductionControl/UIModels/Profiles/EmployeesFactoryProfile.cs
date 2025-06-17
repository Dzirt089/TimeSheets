using AutoMapper;

using ProductionControl.Converters;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.UIModels.Dtos.EmployeesFactory;
using ProductionControl.UIModels.Model.EmployeesFactory;

using System.Collections.ObjectModel;

namespace ProductionControl.UIModels.Profiles
{

	public class EmployeesFactoryProfile : Profile
	{
		public EmployeesFactoryProfile()
		{
			#region Сотрудники предприятия

			CreateMap<EmployeeCardNumShortNameId, EmployeeCardNumShortNameIdDto>().ReverseMap();
			CreateMap<MonthsOrYears, MonthsOrYearsDto>().ReverseMap();
			CreateMap<WorkingSchedule, WorkingScheduleDto>().ReverseMap();

			CreateMap<TimeSheetItemDto, TimeSheetItem>()
				.ForMember(dest => dest.WorkerHours, opt => opt.MapFrom((src, _, _, ctx) =>
					src.WorkerHours == null
					? null
					: ctx.Mapper.Map<ObservableCollection<ShiftData>>(src.WorkerHours)))
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

			#endregion
		}
	}
}

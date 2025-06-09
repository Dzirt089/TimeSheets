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

using System.Windows.Media;

namespace ProductionControl.UIModels
{
	public class EmployeesFactoryProfile : Profile
	{
		public EmployeesFactoryProfile()
		{
			#region Employees Factory
			CreateMap<DepartmentProduction, DepartmentProductionDto>().ReverseMap();
			CreateMap<Employee, EmployeeDto>().ReverseMap();
			CreateMap<EmployeeAccessRight, EmployeeAccessRightDto>().ReverseMap();
			CreateMap<ShiftData, ShiftDataDto>()
				// в прямом маппинге: из сущности в DTO заполняем Brush
				.ForMember(dest => dest.Brush,
						   opt => opt.MapFrom(src => src.Shift.GetBrush()))
				// двунаправленный маппинг
				.ReverseMap()
					// здесь DTO.Brush — «исходное» свойство, его надо просто проигнорировать
					.ForSourceMember(src => src.Brush, opt => opt.DoNotValidate());

			CreateMap<ShiftDataEmployee, ShiftDataEmployeeDto>().ReverseMap();
			CreateMap<MonthsOrYears, MonthsOrYearsDto>().ReverseMap();

			CreateMap<TimeSheetItemDto, TimeSheetItem>()
				.ForMember(dest => dest.Brush, opt => opt.Ignore())
				.ReverseMap()
				.ForSourceMember(dto => dto.Brush, opt => opt.DoNotValidate());

			////Если сотрудник уволен в выбранном месяце, то его ФИО красятся в красный. Все остальные случаи - в черный
			//if (employee.DateDismissal.Month == itemMonthsTO.Id &&
			//	employee.DateDismissal.Year == itemYearsTO.Id)
			//	itemShift.Brush = Brushes.Red;
			//else
			//	itemShift.Brush = Brushes.Black;

			//employee.Shifts.Foreach(x =>
			//{
			//	if (!string.IsNullOrEmpty(x.Shift))
			//		x.Brush = x.Shift.GetBrush();
			//});



			CreateMap<WorkingSchedule, WorkingScheduleDto>().ReverseMap();
			#endregion

			#region Employees External Organization
			CreateMap<EmployeeExOrgAddInRegion, EmployeeExOrgAddInRegionDto>().ReverseMap();
			CreateMap<EmployeeExOrg, EmployeeExOrgDto>().ReverseMap();
			CreateMap<EmployeePhoto, EmployeePhotoDto>().ReverseMap();
			CreateMap<ShiftDataExOrg, ShiftDataExOrgDto>()
				// 1) Простые свойства подхватит AutoMapper:
				//    EmployeeExOrgID, WorkDate, DepartmentID, Hours, CodeColor, EmployeeExOrg
				// 2) Brush в DTO зададим в AfterMap, чтобы сработал ваш setter с GetBrushARGB()
				.AfterMap((src, dst) =>
				{
					// если Hour’ы могут быть null, лучше проверить
					if (!string.IsNullOrEmpty(dst.Hours))
						dst.Brush = dst.GetBrushARGB();
					else
						dst.Brush = Brushes.Black;
				})
				// 3) при обратном маппинге (DTO→Entity) просто игнорируем Brush
				.ReverseMap()
					.ForSourceMember(dto => dto.Brush, opt => opt.DoNotValidate());

			CreateMap<TimeSheetItemExOrgDto, TimeSheetItemExOrg>()
				.ForMember(dest => dest.Brush, opt => opt.Ignore())
				.ReverseMap()
				.ForSourceMember(dto => dto.Brush, opt => opt.DoNotValidate());

			////Если сотрудник уволен в выбранном месяце, то его ФИО красятся в красный. Все остальные случаи - в черный
			//if (employee.DateDismissal.Month == itemMonthsTO.Id &&
			//	employee.DateDismissal.Year == itemYearsTO.Id)
			//	itemShift.Brush = Brushes.Red;
			//else
			//	itemShift.Brush = Brushes.Black;

			//employee.ShiftDataExOrgs.Foreach(x =>
			//{
			//	Brush brush = x.GetBrushARGB();
			//	x.Brush = brush;
			//});
			#endregion

			#region Siz's
			CreateMap<Siz, SizDto>().ReverseMap();
			CreateMap<SizUsageRate, SizUsageRateDto>().ReverseMap();
			CreateMap<UsageNorm, UsageNormDto>().ReverseMap();
			#endregion
		}
	}
}

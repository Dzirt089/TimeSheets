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
			#region Employees Factory
			CreateMap<DepartmentProduction, DepartmentProductionDto>().ReverseMap();
			CreateMap<Employee, EmployeeDto>().ReverseMap();
			CreateMap<EmployeeAccessRight, EmployeeAccessRightDto>().ReverseMap();

			// 1) Правильный маппинг ShiftData → ShiftDataDto
			CreateMap<ShiftData, ShiftDataDto>()
				.ForMember(d => d.Brush, opt => opt.Ignore())
				.ForMember(d => d.Shift, opt => opt.Ignore())
				.ForMember(d => d.Overday, opt => opt.MapFrom(s => s.Overday))
				.ForMember(d => d.WorkDate, opt => opt.MapFrom(s => s.WorkDate))
				.ForMember(d => d.IsHaveLunch, opt => opt.MapFrom(s => s.IsHaveLunch))
				.ForMember(d => d.IsPreHoliday, opt => opt.MapFrom(s => s.IsPreHoliday))
				.ForMember(d => d.Employee, opt => opt.MapFrom(s => s.Employee))
				.AfterMap((src, dst) =>
				{
					// Сначала Employee, потом Shift/Overday
					dst.Shift = src.Shift ?? string.Empty;
					dst.Overday = src.Overday ?? string.Empty;
					// DTO сам пересчитает Hours/Brush
				})
				.ReverseMap()
					.ForSourceMember(s => s.Brush, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.Shift, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.Overday, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.Hours, opt => opt.DoNotValidate());


			CreateMap<ShiftDataEmployee, ShiftDataEmployeeDto>().ReverseMap();
			CreateMap<MonthsOrYears, MonthsOrYearsDto>().ReverseMap();

			// 2) Маппинг TimeSheetItemDto → TimeSheetItem, включая WorkerHours
			CreateMap<TimeSheetItemDto, TimeSheetItem>()
					// Brush в UI-модели считается внутри, игнорируем
					.ForMember(d => d.Brush, opt => opt.Ignore())

					// Вот как говорим: WorkerHours (ObsCol<ShiftData>) → ObsCol<ShiftDataDto>
					.ForMember(d => d.WorkerHours, opt => opt.MapFrom((src, dest, _, ctx) =>
						new ObservableCollection<ShiftDataDto>(
							ctx.Mapper.Map<IEnumerable<ShiftDataDto>>(src.WorkerHours)
						)))

					// И после того, как все данные в WorkerHours прочитаны,
					// запускаем пересчёт итогов (он у вас в SetTotalWorksDays и SetCalendarDayAndHours)
					.AfterMap((src, dst) =>
					{
						dst.SetTotalWorksDays();
						dst.SetCalendarDayAndHours();
					})

					// Обратный маппинг, если нужен
					.ReverseMap()
						.ForSourceMember(s => s.Brush, opt => opt.DoNotValidate())
						.ForSourceMember(s => s.WorkerHours, opt => opt.DoNotValidate());

			#region 1
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
			#endregion

			CreateMap<WorkingSchedule, WorkingScheduleDto>().ReverseMap();
			#endregion

			#region Employees External Organization
			CreateMap<EmployeeExOrgAddInRegion, EmployeeExOrgAddInRegionDto>().ReverseMap();
			CreateMap<EmployeeExOrg, EmployeeExOrgDto>().ReverseMap();
			CreateMap<EmployeePhoto, EmployeePhotoDto>().ReverseMap();
			// 1) Маппинг одной записи смены СО
			CreateMap<ShiftDataExOrg, ShiftDataExOrgDto>()
				// Brush не мапим — DTO сам его рассчитывает
				.ForMember(d => d.Brush, opt => opt.Ignore())
				// Устанавливаем Hours в AfterMap, чтобы Validation() увидел EmployeeExOrg
				.ForMember(d => d.Hours, opt => opt.Ignore())
				.ForMember(d => d.EmployeeExOrg, opt => opt.MapFrom(s => s.EmployeeExOrg))
				.ForMember(d => d.WorkDate, opt => opt.MapFrom(s => s.WorkDate))
				.ForMember(d => d.DepartmentID, opt => opt.MapFrom(s => s.DepartmentID))
				.ForMember(d => d.CodeColor, opt => opt.Ignore())
				.AfterMap((src, dst) =>
				{
					// EmployeeExOrg уже установлен — можно присвоить Hours
					dst.Hours = src.Hours ?? string.Empty;
				})
				.ReverseMap()
					.ForSourceMember(s => s.Brush, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.Hours, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.CodeColor, opt => opt.DoNotValidate());

			// 2) Маппинг всего табеля СО
			CreateMap<TimeSheetItemExOrgDto, TimeSheetItemExOrg>()
				// Brush в VM рассчитывается кодом, AutoMapper его игнорит
				.ForMember(d => d.Brush, opt => opt.Ignore())

				// Явно говорим, как собрать коллекцию WorkerHours:
				.ForMember(d => d.WorkerHours, opt => opt.MapFrom((src, dest, _, ctx) =>
					new ObservableCollection<ShiftDataExOrgDto>(
						ctx.Mapper.Map<IEnumerable<ShiftDataExOrgDto>>(src.WorkerHours)
					)))

				// После того как WorkerHours готова, запускаем вашу логику:
				.AfterMap((src, dst) =>
				{
					dst.SetTotalWorksDays();
					dst.SetCalendarDayAndHours();
				})

				.ReverseMap()
					.ForSourceMember(s => s.Brush, opt => opt.DoNotValidate())
					.ForSourceMember(s => s.WorkerHours, opt => opt.DoNotValidate());
			#region 2
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

			#endregion

			#region Siz's
			CreateMap<Siz, SizDto>().ReverseMap();
			CreateMap<SizUsageRate, SizUsageRateDto>().ReverseMap();
			CreateMap<UsageNorm, UsageNormDto>().ReverseMap();
			#endregion
		}
	}
}

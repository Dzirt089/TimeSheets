using AutoMapper;

using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.UIModels.Dtos.ExternalOrganization;
using ProductionControl.UIModels.Model.ExternalOrganization;

using System.Collections.ObjectModel;

namespace ProductionControl.UIModels.Profiles
{
	public class ExternalOrganizationProfile : Profile
	{
		public ExternalOrganizationProfile()
		{
			#region Внешние организации

			CreateMap<EmployeeExOrgCardNumShortNameId, EmployeeExOrgCardNumShortNameIdDto>().ReverseMap();
			CreateMap<EmployeePhoto, EmployeePhotoDto>().ReverseMap();
			CreateMap<EmployeeExOrgAddInRegion, EmployeeExOrgAddInRegionDto>().ReverseMap();


			CreateMap<EmployeeExOrg, EmployeeExOrgDto>()
				.ForMember(dest => dest.EmployeeExOrgAddInRegions, opt => opt.MapFrom(src => src.EmployeeExOrgAddInRegions))
				.ForMember(dest => dest.ShiftDataExOrgs, opt => opt.MapFrom(src => src.ShiftDataExOrgs))
				.AfterMap((src, dest, context) =>
				{
					if (src.ShiftDataExOrgs != null)
					{
						dest.ShiftDataExOrgs = context.Mapper.Map<IEnumerable<ShiftDataExOrgDto>>(src.ShiftDataExOrgs);
					}
				})
				.ReverseMap();


			CreateMap<ShiftDataExOrg, ShiftDataExOrgDto>()
				.ForMember(d => d.EmployeeExOrg, opt => opt.MapFrom(src => src.EmployeeExOrg))
				.ForMember(d => d.Brush, opt => opt.Ignore())

				.ForMember(d => d.WorkDate, opt => opt.MapFrom(src => src.WorkDate))
				.ForMember(d => d.CodeColor, opt => opt.MapFrom(src => src.CodeColor))
				.ForMember(d => d.Hours, opt => opt.Ignore())
				.AfterMap((src, dst) =>
				{
					if (src.EmployeeExOrg != null)
					{
						dst.Hours = src.Hours;
					}
				})
				.ReverseMap();

			CreateMap<TimeSheetItemExOrgDto, TimeSheetItemExOrg>()
				.ForMember(d => d.Brush, opt => opt.Ignore())
				.ForMember(d => d.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday))
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
				}))
				.ForMember(d => d.FioShiftOverday, opt => opt.MapFrom(src => src.FioShiftOverday));

			#endregion
		}
	}
}

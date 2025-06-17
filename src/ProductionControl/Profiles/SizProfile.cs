using AutoMapper;

using ProductionControl.DataAccess.Classes.EFClasses.Sizs;
using ProductionControl.UIModels.Dtos.Siz;

namespace ProductionControl.Profiles
{
	public class SizProfile : Profile
	{
		public SizProfile()
		{
			#region SIZ

			CreateMap<Siz, SizDto>().ReverseMap();
			CreateMap<SizUsageRate, SizUsageRateDto>().ReverseMap();
			CreateMap<UsageNorm, UsageNormDto>().ReverseMap();

			#endregion
		}
	}
}

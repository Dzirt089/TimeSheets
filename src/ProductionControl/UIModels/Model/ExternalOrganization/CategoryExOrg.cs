using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Models.Entitys.ExternalOrganization
{
	public class CategoryExOrg : ObservableObject
	{
		public int Categoryes
		{
			get => _categoryes;
			set => SetProperty(ref _categoryes, value);
		}
		private int _categoryes;
	}
}

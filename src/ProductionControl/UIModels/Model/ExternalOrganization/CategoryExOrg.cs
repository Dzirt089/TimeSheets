using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Model.ExternalOrganization
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

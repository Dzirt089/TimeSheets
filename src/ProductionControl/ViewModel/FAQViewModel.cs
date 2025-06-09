using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.DataAccess.Classes.Models.Model;
using ProductionControl.Models.Entitys.FAQ;

using System.Collections.ObjectModel;

namespace ProductionControl.ViewModel
{
	public class FAQViewModel : ObservableObject
	{
		public FAQViewModel()
		{
			FaqModels = GetFaqModels();
		}

		public ObservableCollection<FaqModel> GetFaqModels()
		{
			ObservableCollection<FaqModel> temp = [];

			var item1 = new FaqModel
			{
				Symbol = "1",
				Description = "Первая смена",
				HoursCount = ShiftType.FirstShift.HoursCount,
				DayHours = ShiftType.FirstShift.DayHours,
				NightHours = ShiftType.FirstShift.NightHours
			};
			temp.Add(item1);

			var item2 = new FaqModel
			{
				Symbol = "2",
				Description = "Вторая смена",
				HoursCount = ShiftType.SecondShift.HoursCount,
				DayHours = ShiftType.SecondShift.DayHours,
				NightHours = ShiftType.SecondShift.NightHours
			};
			temp.Add(item2);

			var item3 = new FaqModel
			{
				Symbol = "3",
				Description = "Третья смена",
				HoursCount = ShiftType.ThirdShift.HoursCount,
				DayHours = ShiftType.ThirdShift.DayHours,
				NightHours = ShiftType.ThirdShift.NightHours
			};
			temp.Add(item3);

			var item4 = new FaqModel
			{
				Symbol = "4",
				Description = "Четырех часовая смена",
				HoursCount = ShiftType.Мoonlighting.HoursCount,
				DayHours = ShiftType.Мoonlighting.DayHours,
				NightHours = ShiftType.Мoonlighting.NightHours
			};
			temp.Add(item4);

			var item5 = new FaqModel
			{
				Symbol = "5",
				Description = "Пяти часовая смена;",
				HoursCount = ShiftType.Hours5.HoursCount,
				DayHours = ShiftType.Hours5.DayHours,
				NightHours = ShiftType.Hours5.NightHours
			};
			temp.Add(item5);

			var item7 = new FaqModel
			{
				Symbol = "7",
				Description = "Семи часовая смена",
				HoursCount = ShiftType.Hours7.HoursCount,
				DayHours = ShiftType.Hours7.DayHours,
				NightHours = ShiftType.Hours7.NightHours
			};
			temp.Add(item7);

			var itemN = new FaqModel
			{
				Symbol = "Н",
				Description = "Ночная смена (12-ти часовая)",
				HoursCount = ShiftType.NightShift.HoursCount,
				DayHours = ShiftType.NightShift.DayHours,
				NightHours = ShiftType.NightShift.NightHours
			};
			temp.Add(itemN);

			var itemD = new FaqModel
			{
				Symbol = "Д",
				Description = "Дневная смена (12-ти часовая)",
				HoursCount = ShiftType.DayShift.HoursCount,
				DayHours = ShiftType.DayShift.DayHours,
				NightHours = ShiftType.DayShift.NightHours
			};
			temp.Add(itemD);

			var itemC = new FaqModel
			{
				Symbol = "С",
				Description = "24 часовая рабочая смена",
				HoursCount = ShiftType.Hours24.HoursCount,
				DayHours = ShiftType.Hours24.DayHours,
				NightHours = ShiftType.Hours24.NightHours
			};
			temp.Add(itemC);

			var itemK = new FaqModel
			{
				Symbol = "К",
				Description = "Командировка",
				HoursCount = ShiftType.BusinessTrip.HoursCount,
				DayHours = ShiftType.BusinessTrip.DayHours,
				NightHours = ShiftType.BusinessTrip.NightHours
			};
			temp.Add(itemK);

			var itemB = new FaqModel
			{
				Symbol = "Б",
				Description = "Больничный лист",
				HoursCount = "0",
				DayHours = ShiftType.SickLeave.DayHours,
				NightHours = ShiftType.SickLeave.NightHours
			};
			temp.Add(itemB);



			var itemNN = new FaqModel
			{
				Symbol = "НН",
				Description = "Неявка по невыясненным причинам (прогул)",
				HoursCount = "0",
				DayHours = ShiftType.NoShowUnknown.DayHours,
				NightHours = ShiftType.NoShowUnknown.NightHours
			};
			temp.Add(itemNN);



			var itemV = new FaqModel
			{
				Symbol = "ОТ",
				Description = "Очередной оплачиваемый отпуск",
				HoursCount = "0",
				DayHours = ShiftType.Vacation.DayHours,
				NightHours = ShiftType.Vacation.NightHours
			};
			temp.Add(itemV);

			var itemDO = new FaqModel
			{
				Symbol = "ДО",
				Description = "Дополнительный отпуск без сохранения заработной платы",
				HoursCount = "0",
				DayHours = ShiftType.AdministrativeLeavev2.DayHours,
				NightHours = ShiftType.AdministrativeLeavev2.NightHours
			};
			temp.Add(itemDO);



			var itemMO = new FaqModel
			{
				Symbol = "МО",
				Description = "Отпуск по уходу за ребенком",
				HoursCount = "0",
				DayHours = ShiftType.ParentalLeave.DayHours,
				NightHours = ShiftType.ParentalLeave.NightHours
			};
			temp.Add(itemMO);

			var itemOB = new FaqModel
			{
				Symbol = "ОВ",
				Description = "Отпуск по уходу за инвалидом",
				HoursCount = "0",
				DayHours = ShiftType.InvalidLeave.DayHours,
				NightHours = ShiftType.InvalidLeave.NightHours
			};
			temp.Add(itemOB);

			var itemPD = new FaqModel
			{
				Symbol = "ПД",
				Description = "Демобилизованный на СВО",
				HoursCount = "0",
				DayHours = ShiftType.Demobilized.DayHours,
				NightHours = ShiftType.Demobilized.NightHours
			};
			temp.Add(itemPD);

			return temp;
		}

		public ObservableCollection<FaqModel>? FaqModels
		{
			get => _faqModels;
			set => SetProperty(ref _faqModels, value);
		}
		private ObservableCollection<FaqModel>? _faqModels;
	}
}

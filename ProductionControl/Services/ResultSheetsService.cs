using ProductionControl.Entitys;
using ProductionControl.Entitys.ResultTimeSheet;
using ProductionControl.Services.Interfaces;
using ProductionControl.Utils;

using System.Collections.ObjectModel;

namespace ProductionControl.Services
{
	public class ResultSheetsService(IErrorLogger _errorLogger) : IResultSheetsService
	{
		/// <summary>
		/// Подготавливаем данными первый показатель "Фактически отработанное время"
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		/// <param name="shadowid">Теневой "id" для удобства работы с коллекциями итогов табеля</param>
		public async Task<Indicator> GetDataForIndicatorOneAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent,
			int shadowid)
		{
			try
			{
				SumTotalWorksDays = 0;
				SumTotalWorksHoursWithOverday = 0;

				foreach (var item in TimeSheets)
				{
					if (item is null) continue;

					SumTotalWorksDays += item?.TotalWorksDays ?? 0;
					SumTotalWorksHoursWithOverday += item?.TotalWorksHoursWithOverday ?? 0;
				}

				var indicator = new Indicator
				{
					ShadowId = shadowid,
					DescriptionIndicator = "Фактически отработанное время",
					CountDays = SumTotalWorksDays,
					CountHours = SumTotalWorksHoursWithOverday,
				};
				return indicator;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);
				return new();
			}
		}


		public async Task<List<EmployeesInIndicator>> ProcessTimeSheetsOverdayOrUnderdayAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent,
			bool overday)
		{
			try
			{
				List<EmployeesInIndicator> underdayOrOverday = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					if (overday && item.TotalOverdayHours > 0 || !overday && item.TotalOverdayHours < 0)
					{
						EmployeesInIndicator oneEmployeeInIndicator = new();
						var itemOne = item.WorkerHours.FirstOrDefault();

						oneEmployeeInIndicator.EmployeeID = itemOne?.EmployeeID ?? 0;
						oneEmployeeInIndicator.FullName = itemOne?.Employee.FullName ?? item.FioShiftOverday.ShortName;
						oneEmployeeInIndicator.CountDays = 0;
						oneEmployeeInIndicator.CountHours = item.TotalOverdayHours;

						underdayOrOverday.Add(oneEmployeeInIndicator);
					}
				}

				return underdayOrOverday.Where(x => x.CountHours != 0).ToList();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return [];
			}
		}

		public async Task<List<EmployeesInIndicator>> ProcessTimeSheetNightHoursAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent)
		{
			try
			{
				List<EmployeesInIndicator> night = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					var nightList = item.WorkerHours
						.Where(x => !string.IsNullOrEmpty(x.Shift) && x.Shift.GetNightHoursBool())
						.ToList();

					if (nightList.Count > 0)
					{
						EmployeesInIndicator oneEmployeeInIndicator = new();
						foreach (var i in nightList)
						{
							oneEmployeeInIndicator.EmployeeID = i.EmployeeID;
							oneEmployeeInIndicator.FullName = i.Employee.FullName ?? item.FioShiftOverday.ShortName;
							oneEmployeeInIndicator.CountDays += 1;
							oneEmployeeInIndicator.CountHours += i.Shift?.GetNightHours() ?? 0;
						}
						night.Add(oneEmployeeInIndicator);
					}
				}

				return night;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return [];
			}
		}

		public async Task<Indicator?> CreateIndicatorAsync(string currentLongValue,
			int shadowid, List<EmployeesInIndicator> employeesIns, LocalUserData userDataCurrent)
		{
			try
			{
				Indicator? indicatiorOne = null;
				//if (employeesIns.Count > 0)
				//{
				indicatiorOne = new()
				{
					ShadowId = shadowid,
					DescriptionIndicator = currentLongValue,
					CountDays = employeesIns.Sum(x => x.CountDays),
					CountHours = employeesIns.Sum(x => x.CountHours)
				};
				//}

				return indicatiorOne;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return null;
			}
		}

		public async Task<List<EmployeesInIndicator>> ProcessTimeSheets(
			ObservableCollection<TimeSheetItem> timeSheets,
			string currentShortValue,
			LocalUserData userDataCurrent)
		{
			try
			{
				List<EmployeesInIndicator> employeesIns = [];

				foreach (var item in timeSheets.Where(x => x != null))
				{
					var datas = item.WorkerHours
						.Where(x => !string.IsNullOrEmpty(x.Shift) && x.Shift.Contains(currentShortValue, StringComparison.OrdinalIgnoreCase))
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicator oneEmployeeInIndicator = new();
						foreach (var i in datas)
						{
							oneEmployeeInIndicator.EmployeeID = i.EmployeeID;
							oneEmployeeInIndicator.FullName = i.Employee.FullName ?? item.FioShiftOverday.ShortName;
							oneEmployeeInIndicator.CountDays += 1;
							oneEmployeeInIndicator.CountHours = 0;
						}
						employeesIns.Add(oneEmployeeInIndicator);
					}
				}

				return employeesIns;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return [];
			}
		}



		public async Task<List<EmployeesInIndicator>> ProcessTimeSheetsDismissalAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent)
		{
			try
			{
				List<EmployeesInIndicator> employeesIns = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					var datas = item.WorkerHours
						.Where(x => x.Employee != null && x.Employee.IsDismissal == true)
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicator oneEmployeeInIndicator = new();
						var itemOne = item.WorkerHours.FirstOrDefault();

						oneEmployeeInIndicator.EmployeeID = itemOne?.EmployeeID ?? 0;
						oneEmployeeInIndicator.FullName = itemOne?.Employee.FullName ?? item.FioShiftOverday.ShortName;
						oneEmployeeInIndicator.CountDays = item.TotalWorksDays;
						oneEmployeeInIndicator.CountHours = item.TotalWorksHoursWithOverday;

						employeesIns.Add(oneEmployeeInIndicator);
					}
				}

				return employeesIns;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return [];
			}
		}

		public async Task<List<EmployeesInIndicator>> ProcessTimeSheetsLunchAsync(ObservableCollection<TimeSheetItem> TimeSheets, LocalUserData userDataCurrent)
		{
			try
			{
				List<EmployeesInIndicator> employeesIns = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					var datas = item.WorkerHours
						.Where(x => x.IsHaveLunch == true)
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicator oneEmployeeInIndicator = new();
						foreach (var i in datas)
						{
							oneEmployeeInIndicator.EmployeeID = i.EmployeeID;
							oneEmployeeInIndicator.FullName = i.Employee.FullName ?? item.FioShiftOverday.ShortName;
							oneEmployeeInIndicator.CountDays += 1;
							oneEmployeeInIndicator.CountHours = 0;
						}
						employeesIns.Add(oneEmployeeInIndicator);
					}
				}

				return employeesIns;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName,
					machine: userDataCurrent.MachineName)
					.ConfigureAwait(false);

				return [];
			}
		}

		/// <summary>
		/// Список показателей в итогах табеля
		/// </summary>
		public ObservableCollection<Indicator> Indicators { get; private set; }

		/// <summary>
		/// Общая сумма всех отработанных по факту дней на участке
		/// </summary>
		public int SumTotalWorksDays { get; private set; }

		/// <summary>
		/// Общая сумма всех отработанных по факту часов на участке
		/// </summary>
		public double SumTotalWorksHoursWithOverday { get; private set; }

		/// <summary>
		/// Список по неявкам
		/// </summary>
		public List<EmployeesInIndicator> NNList { get; private set; }

		/// <summary>
		/// Список по недоработкам
		/// </summary>
		public List<EmployeesInIndicator> Underday { get; private set; }

		/// <summary>
		/// Список по переработкам
		/// </summary>
		public List<EmployeesInIndicator> Overday { get; private set; }

		/// <summary>
		/// Список по ночным отработкам
		/// </summary>
		public List<EmployeesInIndicator> Night { get; private set; }

		/// <summary>
		/// Список по ежегодному отпуску
		/// </summary>
		public List<EmployeesInIndicator> Vacation { get; private set; }

		/// <summary>
		/// Список по административному отпуску
		/// </summary>
		public List<EmployeesInIndicator> ADVacation { get; private set; }

		/// <summary>
		/// Список по больничным
		/// </summary>
		public List<EmployeesInIndicator> SickLeave { get; private set; }
		public List<EmployeesInIndicator> Demobilized { get; private set; }
		public List<EmployeesInIndicator> ParentalLeave { get; private set; }
		public List<EmployeesInIndicator> InvalidLeave { get; private set; }
		public List<EmployeesInIndicator> Dismissal { get; private set; }
		public List<EmployeesInIndicator> Lunching { get; private set; }

		/// <summary>
		/// Метод, который рассчитывает данные показателей и списков сотрудников по ним.
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		public async Task<(
			ObservableCollection<Indicator> Indicators,
			List<EmployeesInIndicator> NNList,
			List<EmployeesInIndicator> Underday,
			List<EmployeesInIndicator> Overday,
			List<EmployeesInIndicator> Night,
			List<EmployeesInIndicator> Vacation,
			List<EmployeesInIndicator> ADVacation,
			List<EmployeesInIndicator> SickLeave,
			List<EmployeesInIndicator> Demobilized,
			List<EmployeesInIndicator> ParentalLeave,
			List<EmployeesInIndicator> InvalidLeave,
			List<EmployeesInIndicator> Dismissal,
			List<EmployeesInIndicator> Lunching)>
			ShowResultSheet(ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent)
		{
			try
			{
				var tempInd = new List<Indicator>();

				var indicatiorOne = await GetDataForIndicatorOneAsync(TimeSheets, userDataCurrent, 1);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				NNList = await ProcessTimeSheets(TimeSheets, "нн", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Неявки", 2, NNList, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Underday = await ProcessTimeSheetsOverdayOrUnderdayAsync(TimeSheets, userDataCurrent, false);
				indicatiorOne = await CreateIndicatorAsync("Недоработки", 3, Underday, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Overday = await ProcessTimeSheetsOverdayOrUnderdayAsync(TimeSheets, userDataCurrent, true);
				indicatiorOne = await CreateIndicatorAsync("Переработки", 4, Overday, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Night = await ProcessTimeSheetNightHoursAsync(TimeSheets, userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Ночные часы", 5, Night, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Vacation = await ProcessTimeSheets(TimeSheets, "от", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Отпуск ежегодный", 6, Vacation, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				ADVacation = [];
				var tempAd = await ProcessTimeSheets(TimeSheets, "ад", userDataCurrent);
				var tempDo = await ProcessTimeSheets(TimeSheets, "до", userDataCurrent);

				if (tempAd != null && tempAd.Count > 0)
					ADVacation.AddRange(tempAd);
				if (tempDo != null && tempDo.Count > 0)
					ADVacation.AddRange(tempDo);

				indicatiorOne = await CreateIndicatorAsync("Отпуск административный", 7, ADVacation, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				SickLeave = await ProcessTimeSheets(TimeSheets, "б", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Больничный", 8, SickLeave, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Demobilized = await ProcessTimeSheets(TimeSheets, "пд", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Демобилизованный на СВО", 9, Demobilized, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				ParentalLeave = await ProcessTimeSheets(TimeSheets, "мо", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Отпуск по уходу за ребенком", 10, ParentalLeave, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);


				InvalidLeave = await ProcessTimeSheets(TimeSheets, "ов", userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Отпуск по уходу за инвалидом", 11, InvalidLeave, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Dismissal = await ProcessTimeSheetsDismissalAsync(TimeSheets, userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Уволенные, с отработанными днями и часами", 12, Dismissal, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Lunching = await ProcessTimeSheetsLunchAsync(TimeSheets, userDataCurrent);
				indicatiorOne = await CreateIndicatorAsync("Обеды", 13, Lunching, userDataCurrent);
				await AddIndicatorAsync(tempInd, indicatiorOne, userDataCurrent);

				Indicator factWithoutOverday = await GetIndicatorFactWithoutOverdayAsync(tempInd, userDataCurrent);
				await AddIndicatorAsync(tempInd, factWithoutOverday, userDataCurrent);

				Indicators = new ObservableCollection<Indicator>(tempInd.OrderBy(x => x.ShadowId));

				return (Indicators, NNList, Underday, Overday, Night,
					Vacation, ADVacation, SickLeave, Demobilized, ParentalLeave, InvalidLeave, Dismissal, Lunching);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName, machine: userDataCurrent.MachineName);

				return ([], [], [], [], [], [], [], [], [], [], [], [], []);
			}
		}

		private async Task<Indicator> GetIndicatorFactWithoutOverdayAsync(
			List<Indicator> tempInd, LocalUserData userDataCurrent)
		{
			try
			{
				var fullFact = tempInd.Where(x => x.ShadowId == 1).First();
				var overday = tempInd.Where(x => x.ShadowId == 4).First();

				var factWithoutOverday = new Indicator
				{
					ShadowId = 0,
					DescriptionIndicator = "Отработанное время без переработок",
					CountDays = fullFact.CountDays,
					CountHours = fullFact.CountHours - overday.CountHours,
				};
				return factWithoutOverday;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName, machine: userDataCurrent.MachineName);

				return new();
			}
		}

		public async Task AddIndicatorAsync(List<Indicator> tempInd, Indicator? indicatiorOne, LocalUserData userDataCurrent)
		{
			try
			{
				if (indicatiorOne != null)
					tempInd.Add(indicatiorOne);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: userDataCurrent.UserName, machine: userDataCurrent.MachineName);
			}
		}
	}
}

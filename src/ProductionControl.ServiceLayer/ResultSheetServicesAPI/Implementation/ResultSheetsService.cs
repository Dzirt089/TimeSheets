using ProductionControl.DataAccess.Classes.Models.Dtos;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.ServiceLayer.ResultSheetServicesAPI.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ResultSheetServicesAPI.Implementation
{
	public class ResultSheetsService(IErrorLogger _errorLogger) : IResultSheetsService
	{
		/// <summary>
		/// Подготавливаем данными первый показатель "Фактически отработанное время"
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		/// <param name="shadowid">Теневой "id" для удобства работы с коллекциями итогов табеля</param>
		public async Task<IndicatorDto> GetDataForIndicatorOneAsync(List<TimeSheetItemDto> TimeSheets, int shadowid, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				SumTotalWorksDays = 0;
				SumTotalWorksHoursWithOverday = 0;

				foreach (var item in TimeSheets)
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					if (item is null) continue;

					SumTotalWorksDays += item?.TotalWorksDays ?? 0;
					SumTotalWorksHoursWithOverday += item?.TotalWorksHoursWithOverday ?? 0;
				}

				var IndicatorDto = new IndicatorDto
				{
					ShadowId = shadowid,
					DescriptionIndicator = "Фактически отработанное время",
					CountDays = SumTotalWorksDays,
					CountHours = SumTotalWorksHoursWithOverday,
				};
				return IndicatorDto;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return new();
				throw;
			}
		}


		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsOverdayOrUnderdayAsync(
			List<TimeSheetItemDto> TimeSheets, bool overday, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> underdayOrOverday = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					if (overday && item.TotalOverdayHours > 0 || !overday && item.TotalOverdayHours < 0)
					{
						EmployeesInIndicatorDto oneEmployeeInIndicator = new();
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetNightHoursAsync(List<TimeSheetItemDto> TimeSheets, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> night = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					var nightList = item.WorkerHours
						.Where(x => !string.IsNullOrEmpty(x.Shift) && x.Shift.GetNightHoursBool())
						.ToList();

					if (nightList.Count > 0)
					{
						EmployeesInIndicatorDto oneEmployeeInIndicator = new();
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		public async Task<IndicatorDto?> CreateIndicatorAsync(string currentLongValue,
			int shadowid, List<EmployeesInIndicatorDto> employeesIns, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				IndicatorDto? indicatiorOne = null;
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return null;
				throw;
			}
		}

		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheets(List<TimeSheetItemDto> timeSheets, string currentShortValue, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> employeesIns = [];

				foreach (var item in timeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					var datas = item.WorkerHours
						.Where(x => !string.IsNullOrEmpty(x.Shift) && x.Shift.Contains(currentShortValue, StringComparison.OrdinalIgnoreCase))
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicatorDto oneEmployeeInIndicator = new();
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsAD(List<TimeSheetItemDto> timeSheets, string currentShortValue, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> employeesIns = [];

				foreach (var item in timeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					var datas = item.WorkerHours
						.Where(x => !string.IsNullOrEmpty(x.Shift) && x.Shift.Contains(currentShortValue, StringComparison.OrdinalIgnoreCase))
						.ToList();

					if (datas.Count > 0)
					{

						foreach (var i in datas)
						{
							EmployeesInIndicatorDto oneEmployeeInIndicator = new();

							oneEmployeeInIndicator.EmployeeID = i.EmployeeID;
							oneEmployeeInIndicator.FullName = i.Employee.FullName ?? item.FioShiftOverday.ShortName;
							oneEmployeeInIndicator.Date = i.WorkDate.ToString("d");
							oneEmployeeInIndicator.CountHours = 0;

							employeesIns.Add(oneEmployeeInIndicator);
						}

						Dictionary<long, int> valuePairs = employeesIns
							.GroupBy(g => g.EmployeeID)
							.ToDictionary(x => x.Key, x => x.Select(s => s.Date).Distinct().Count());

						employeesIns.ForEach(x =>
						{
							x.CountDays = valuePairs[x.EmployeeID];
						});
					}
				}

				return employeesIns;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsDismissalAsync(
			List<TimeSheetItemDto> TimeSheets, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> employeesIns = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					var datas = item.WorkerHours
						.Where(x => x.Employee != null && x.Employee.IsDismissal == true)
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicatorDto oneEmployeeInIndicator = new();
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		public async Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsLunchAsync(List<TimeSheetItemDto> TimeSheets, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				List<EmployeesInIndicatorDto> employeesIns = [];

				foreach (var item in TimeSheets.Where(x => x != null))
				{
					token.ThrowIfCancellationRequested(); // Проверка на входе

					var datas = item.WorkerHours
						.Where(x => x.IsHaveLunch == true)
						.ToList();

					if (datas.Count > 0)
					{
						EmployeesInIndicatorDto oneEmployeeInIndicator = new();
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}

		/// <summary>
		/// Список показателей в итогах табеля
		/// </summary>
		public List<IndicatorDto> Indicators { get; private set; }

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
		public List<EmployeesInIndicatorDto> NNList { get; private set; }

		/// <summary>
		/// Список по недоработкам
		/// </summary>
		public List<EmployeesInIndicatorDto> Underday { get; private set; }

		/// <summary>
		/// Список по переработкам
		/// </summary>
		public List<EmployeesInIndicatorDto> Overday { get; private set; }

		/// <summary>
		/// Список по ночным отработкам
		/// </summary>
		public List<EmployeesInIndicatorDto> Night { get; private set; }

		/// <summary>
		/// Список по ежегодному отпуску
		/// </summary>
		public List<EmployeesInIndicatorDto> Vacation { get; private set; }

		/// <summary>
		/// Список по административному отпуску
		/// </summary>
		public List<EmployeesInIndicatorDto> ADVacation { get; private set; }

		/// <summary>
		/// Список по больничным
		/// </summary>
		public List<EmployeesInIndicatorDto> SickLeave { get; private set; }
		public List<EmployeesInIndicatorDto> Demobilized { get; private set; }
		public List<EmployeesInIndicatorDto> ParentalLeave { get; private set; }
		public List<EmployeesInIndicatorDto> InvalidLeave { get; private set; }
		public List<EmployeesInIndicatorDto> Dismissal { get; private set; }
		public List<EmployeesInIndicatorDto> Lunching { get; private set; }

		/// <summary>
		/// Метод, который рассчитывает данные показателей и списков сотрудников по ним.
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		public async Task<ResultSheetResponseDto> ShowResultSheet(List<TimeSheetItemDto> TimeSheets, CancellationToken token)
		{
			try
			{
				token.ThrowIfCancellationRequested(); // Проверка на входе

				var tempInd = new List<IndicatorDto>();

				var indicatiorOne = await GetDataForIndicatorOneAsync(TimeSheets, 1, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				NNList = await ProcessTimeSheets(TimeSheets, "нн", token);
				indicatiorOne = await CreateIndicatorAsync("Неявки", 2, NNList, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Underday = await ProcessTimeSheetsOverdayOrUnderdayAsync(TimeSheets, false, token);
				indicatiorOne = await CreateIndicatorAsync("Недоработки", 3, Underday, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Overday = await ProcessTimeSheetsOverdayOrUnderdayAsync(TimeSheets, true, token);
				indicatiorOne = await CreateIndicatorAsync("Переработки", 4, Overday, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Night = await ProcessTimeSheetNightHoursAsync(TimeSheets, token);
				indicatiorOne = await CreateIndicatorAsync("Ночные часы", 5, Night, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Vacation = await ProcessTimeSheets(TimeSheets, "от", token);
				indicatiorOne = await CreateIndicatorAsync("Отпуск ежегодный", 6, Vacation, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				ADVacation = [];
				var tempAd = await ProcessTimeSheetsAD(TimeSheets, "ад", token);
				var tempDo = await ProcessTimeSheetsAD(TimeSheets, "до", token);

				if (tempAd != null && tempAd.Count > 0)
					ADVacation.AddRange(tempAd);
				if (tempDo != null && tempDo.Count > 0)
					ADVacation.AddRange(tempDo);

				indicatiorOne = await CreateIndicatorAsync("Отпуск административный", 7, ADVacation, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				SickLeave = await ProcessTimeSheets(TimeSheets, "б", token);
				indicatiorOne = await CreateIndicatorAsync("Больничный", 8, SickLeave, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Demobilized = await ProcessTimeSheets(TimeSheets, "пд", token);
				indicatiorOne = await CreateIndicatorAsync("Демобилизованный на СВО", 9, Demobilized, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				ParentalLeave = await ProcessTimeSheets(TimeSheets, "мо", token);
				indicatiorOne = await CreateIndicatorAsync("Отпуск по уходу за ребенком", 10, ParentalLeave, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);


				InvalidLeave = await ProcessTimeSheets(TimeSheets, "ов", token);
				indicatiorOne = await CreateIndicatorAsync("Отпуск по уходу за инвалидом", 11, InvalidLeave, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Dismissal = await ProcessTimeSheetsDismissalAsync(TimeSheets, token);
				indicatiorOne = await CreateIndicatorAsync("Уволенные, с отработанными днями и часами", 12, Dismissal, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				Lunching = await ProcessTimeSheetsLunchAsync(TimeSheets, token);
				indicatiorOne = await CreateIndicatorAsync("Обеды", 13, Lunching, token);
				await AddIndicatorAsync(tempInd, indicatiorOne);

				IndicatorDto factWithoutOverday = await GetIndicatorFactWithoutOverdayAsync(tempInd);
				await AddIndicatorAsync(tempInd, factWithoutOverday);

				Indicators = new List<IndicatorDto>(tempInd.OrderBy(x => x.ShadowId));

				var results = new ResultSheetResponseDto
				{
					Indicators = Indicators,
					NNList = NNList,
					Underday = Underday,
					Overday = Overday,
					Night = Night,
					Vacation = Vacation,
					ADVacation = ADVacation,
					SickLeave = SickLeave,
					Demobilized = Demobilized,
					ParentalLeave = ParentalLeave,
					InvalidLeave = InvalidLeave,
					Dismissal = Dismissal,
					Lunching = Lunching
				};
				return results;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return new();
			}
		}

		private async Task<IndicatorDto> GetIndicatorFactWithoutOverdayAsync(List<IndicatorDto> tempInd)
		{
			try
			{
				var fullFact = tempInd.Where(x => x.ShadowId == 1).First();
				var overday = tempInd.Where(x => x.ShadowId == 4).First();

				var factWithoutOverday = new IndicatorDto
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return new();
				throw;
			}
		}

		public async Task AddIndicatorAsync(List<IndicatorDto> tempInd, IndicatorDto? indicatiorOne)
		{
			try
			{
				if (indicatiorOne != null)
					tempInd.Add(indicatiorOne);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}
	}
}

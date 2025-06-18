using FastReport;
using FastReport.Export.OoXML;

using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

using System.Drawing;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	public class ReportService(
		IEmployeesFactorysRepository contextServices,
		IErrorLogger errorLogger)
		: IReportService
	{
		#region Property
		/// <summary>Все сотрудники, которые обедают</summary>
		internal List<Employee>? AllEmployeeWithLuch { get; private set; }

		/// <summary>Общее кол-во блюд в заказе</summary>
		internal int TotalNumberLunch { get; private set; }

		/// <summary>Общее кол-во блюд для столовой №1</summary>
		public int TotalForCafeteriaOne { get; private set; }

		/// <summary>Общее кол-во блюд для столовой №2</summary>
		public int TotalForCafeteriaTwo { get; private set; }

		/// <summary>Общее кол-во ужинов для столовой №2</summary>
		public int TotalDinnerForCafeteriaTwo { get; private set; }

		/// <summary>Общее кол-во обедов для столовой №2</summary>
		public int TotalLunchTimeForCafeteriaTwo { get; private set; }

		/// <summary>Номер прошлого месяца</summary>
		public int LastMonht { get; private set; }

		/// <summary>Прошлая дата</summary>
		public DateTime LastDate { get; private set; }

		/// <summary>Все сотрудники</summary>
		public List<Employee>? AllPeople { get; private set; }
		#endregion


		/// <summary>
		/// Excel отчёт по выбранному итогу табеля
		/// </summary>
		public async Task<string> CreateReportForMonthlySummaryEmployeeExpOrgAsync(
			List<EmployeesExOrgForReportDto> summaries, DateTime startDate, DateTime endDate, CancellationToken token)
		{
			try
			{
				if (summaries is null || summaries.Count == 0) return string.Empty;

				var item = summaries.FirstOrDefault();

				string fileName = await GetPathDiskMAsync(
					$"Сводная на сотрудников СО, за {item.MonthName} {item.Year}.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportMonthlyForEmployeeExpOrg.frx");
				report.RegisterData(summaries, "RequestData");
				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;
				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;

				for (var date = startDate; date <= endDate; date = date.AddDays(1))
				{
					var m = report.FindObject($"Day{date.Day}") as TextObject;
					if (m != null) m.FillColor = Color.Green;
				}

				report.Prepare();

				using Excel2007Export excel = new();
				excel.PageBreaks = false;
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Excel отчёт по выбранному итогу табеля
		/// </summary>
		public async Task<string> CreateReportForMonthlySummaryAsync(
			List<MonthlySummaryDto> summaries, CancellationToken token)
		{
			try
			{
				if (summaries is null || summaries.Count == 0) return string.Empty;

				var item = summaries.FirstOrDefault();

				string fileName = await GetPathDiskMAsync(
					$"Сводная за {item.MonthName} {item.Year}.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportMonthlySummary.frx");
				report.RegisterData(summaries, "RequestData");
				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;
				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;

				report.Prepare();

				using Excel2007Export excel = new();
				excel.PageBreaks = false;
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}


		/// <summary>
		/// Формируем Excel отчёт по переданным данным
		/// </summary>
		/// <param name="dataForReports">Список с данными</param>
		/// <param name="date">Прошлая дата</param>
		/// <returns>строка пути, где храниться отчёт</returns>
		public async Task<string> CreateReportExcelForLunchLastMonhtAsync(
			List<DataForReportLunchLastMonthDto> dataForReports,
			DateTime date, CancellationToken token)
		{
			try
			{
				string monthName = date.Month switch
				{
					1 => "Январь",
					2 => "Февраль",
					3 => "Март",
					4 => "Апрель",
					5 => "Май",
					6 => "Июнь",
					7 => "Июль",
					8 => "Август",
					9 => "Сентябрь",
					10 => "Октябрь",
					11 => "Ноябрь",
					12 => "Декабрь",
					_ => string.Empty
				};

				string fileName = await GetPathDiskServerAsync(
					$"Обеды Поляны на {monthName} {date.Year}.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportLunchForLastMonth.frx");
				report.RegisterData(dataForReports, "RequestData");
				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;
				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;

				report.Prepare();

				using Excel2007Export excel = new();
				excel.PageBreaks = false;
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}


		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		public async Task ProcessingDataReportForLUnchEveryDayAsync(CancellationToken token)
		{
			try
			{
				List<string> departmentForCafeteriaOne = ["012", "013", "014", "015", "016", "03"];
				List<string> paths = [];

				AllEmployeeWithLuch = await contextServices.GetEmployeesForLunchAsync(token);
				AllEmployeeWithLuch = AllEmployeeWithLuch
					.Where(x => x.ValidateEmployee(DateTime.Now.Month, DateTime.Now.Year)).ToList();
				AllEmployeeWithLuch = AllEmployeeWithLuch
					.Where(s => s.Shifts.ValidationShiftForLunch())
					.ToList();

				TotalNumberLunch = AllEmployeeWithLuch.Count();

				TotalForCafeteriaOne = AllEmployeeWithLuch
					.Where(x => departmentForCafeteriaOne.Contains(x.DepartmentID))
					.Count();

				//список людей, которые обедают в столовой №1
				var TotalForCafeteriaOneList = AllEmployeeWithLuch
					.Where(x => departmentForCafeteriaOne.Contains(x.DepartmentID))
					.ToList();

				//создание отчёта 1
				var pathLunch1 = await CreateReportForResultSheetAsync(TotalForCafeteriaOneList, "Сотрудники, обедающие в столовой №1", token);

				if (!string.IsNullOrEmpty(pathLunch1))
					paths.Add(pathLunch1);

				TotalForCafeteriaTwo = TotalNumberLunch - TotalForCafeteriaOne;

				TotalDinnerForCafeteriaTwo = AllEmployeeWithLuch
					.Where(x => x.Shifts.ValidationShiftDinnerForCafeteriaTwo())
					.Count();

				//список людей, которые ужинают в столовой №2
				var TotalDinnerForCafeteriaTwoList = AllEmployeeWithLuch
					.Where(x => x.Shifts.ValidationShiftDinnerForCafeteriaTwo())
					.ToList();

				//создание отчёта 2
				var pathLunch2 = await CreateReportForResultSheetAsync(TotalDinnerForCafeteriaTwoList, "Сотрудники, ужинающие в столовой №2", token);

				if (!string.IsNullOrEmpty(pathLunch2))
					paths.Add(pathLunch2);

				TotalLunchTimeForCafeteriaTwo = TotalForCafeteriaTwo - TotalDinnerForCafeteriaTwo;
				//список людей, которые обедают в столовой №2
				var list = AllEmployeeWithLuch
					.Where(e => !TotalForCafeteriaOneList.Any(s => s.EmployeeID == e.EmployeeID))
					.Where(e => !TotalDinnerForCafeteriaTwoList.Any(s => s.EmployeeID == e.EmployeeID))
					.ToList();

				//создание отчёта 3
				var pathLunch3 = await CreateReportForResultSheetAsync(list, "Сотрудники, обедающие в столовой №2", token);

				if (!string.IsNullOrEmpty(pathLunch3))
					paths.Add(pathLunch3);

				string text = $@"<pre>
Заказ на {DateTime.Now.Date}.
РВ+ {TotalNumberLunch}

Из них:
Столовая офис - {TotalForCafeteriaOne}

Обедов - {TotalLunchTimeForCafeteriaTwo}
Ужинов - {TotalDinnerForCafeteriaTwo}
</pre>";


				if (paths != null && paths.Count > 0)
					await errorLogger.SendMailReportNowAsync(paths, text);
				else
					await errorLogger.SendMailWithOrderLunchEveryDayAsync(text);

				foreach (var item in AllEmployeeWithLuch)
				{
					var lanch = item.Shifts.FirstOrDefault();
					if (lanch != null)
						lanch.IsHaveLunch = true;
				}

				int row = await contextServices.UpdateEmployeesAsync(AllEmployeeWithLuch, token);
				if (row > 0)
				{
					await errorLogger.ProcessingLogAsync($@"<pre>
Успешно обновлен график табеля в API, 
проставлены обеды на {DateTime.Now.Day} {DateTime.Now.Month} {DateTime.Now.Year}.
Кол-во затронутых строк: {row}. 
</pre>");
				}
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
			}
		}

		/// <summary>
		/// Excel отчёт по выбранному итогу табеля
		/// </summary>
		/// <param name="indicators"></param>
		/// <returns></returns>
		public async Task<string> CreateReportForResultSheetAsync(
			List<Employee> employees, string lunchDinner, CancellationToken token)
		{
			try
			{
				if (employees is null || employees.Count == 0) return string.Empty;

				string fileName = await GetPathDiskMAsync(
					$"Показатели для {lunchDinner}.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportListEmployeeWithLunch.frx");
				report.RegisterData(employees, "RequestData");
				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;
				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;


				var m = report.FindObject("Text14") as TextObject;
				if (m != null) m.Text = @$"{lunchDinner}.";

				report.Prepare();

				using Excel2007Export excel = new();
				excel.PageBreaks = false;
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		public async Task ProcessingDataForReportLunchLastMonth(string totalSum, CancellationToken token)
		{
			try
			{
				if (!decimal.TryParse(totalSum, out decimal totalSumDecimal)) return;
				//Программа должна должна стабильно за прошлый месяц составлять отчет по обедам.
				//Подготавливаем месяц и дату
				if (DateTime.Now.Month != 1)
				{
					LastMonht = DateTime.Now.Month - 1;
					LastDate = DateTime.Now.AddMonths(-1);
				}
				else
				{
					LastMonht = 12;
					LastDate = new DateTime(DateTime.Now.AddYears(-1).Year, LastMonht, 1);
				}

				var startDate = new DateTime(LastDate.Year, LastMonht, 1);
				var endDate = new DateTime(LastDate.Year, LastMonht,
				DateTime.DaysInMonth(LastDate.Year, LastMonht));

				//Тестовые даты:
				//var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				//var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
				//DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

				StartEndDateTime startEndDate = new StartEndDateTime { StartDate = startDate, EndDate = endDate };


				//Выбираем всех людей из БД , кроме уволенных. В рамках начала и конца выбранного месяца в сменах
				AllPeople = await contextServices.GetEmployeesForReportLunchAsync(startEndDate, token);

				AllPeople = AllPeople
					.Where(x => x.ValidateEmployee(LastMonht, LastDate.Year))
					.ToList();

				if (AllPeople == null || AllPeople.Count == 0) return; //Проверка

				List<DataForReportLunchLastMonthDto> dataForReports = [];
				foreach (var item in AllPeople)
				{
					int count = item.Shifts.GetCountLunchInMonth(item.IsDismissal);
					if (count == 0) continue;

					var temp = new DataForReportLunchLastMonthDto
					{
						EmployeeId = item.EmployeeID,
						ShortName = item.ShortName,
						CountLunch = count,
						TotalSum = totalSumDecimal,
						StartDate = startDate,
					};
					dataForReports.Add(temp);
				}

				if (dataForReports.Count() == 0 || dataForReports is null) return;
				else
				{
					var total = dataForReports.Sum(x => x.CountLunch);
					dataForReports.ForEach(x =>
					{
						x.TotalCountLunch = total;
						x.AverageAmount = Math.Round(x.TotalSum / total, 2);
					});

					string path = await CreateReportExcelForLunchLastMonhtAsync(dataForReports, LastDate, token);
					await errorLogger.SendMailReportAsync(path);
				}
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
			}
		}

		/// <summary>
		/// Получаем путь для сохранения файлов 
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		public async Task<string> GetPathDiskServerAsync(string fileName, CancellationToken token)
		{
			try
			{
				//var folder = @"D:\Отчеты по Обедам\"; //папка на сервере
				var folder = @"C:\Users\teho19\Desktop\Новая папка\";

				var exportPath = $@"{folder}\{DateTime.Now:yyMMdd}";
				if (!Directory.Exists(exportPath))
					Directory.CreateDirectory(exportPath);

				return $@"{exportPath}\{fileName}";
			}
			catch (Exception ex)
			{
				await errorLogger.LogErrorAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Получаем путь для сохранения расчетных файлов
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		public async Task<string> GetPathDiskMAsync(string fileName, CancellationToken token)
		{
			try
			{
				var folder = @"Share\Общая\Файлы отчётов табеля";
				var M = $@"\\192.168.168.153";
				var exportPath = $@"{M}\{folder}\{DateTime.Now:yyMMdd}";
				if (!Directory.Exists(exportPath)) Directory.CreateDirectory(exportPath);
				return $@"{exportPath}\{fileName}";
			}
			catch (Exception ex)
			{
				await errorLogger.LogErrorAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Получаем путь для сохранения расчетных файлов
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		public async Task<string> GetPathDiskMSizAsync(string fileName, CancellationToken token)
		{
			try
			{
				var folder = @"Share\Общая\Файлы отчётов табеля\СИЗ";
				var M = $@"\\192.168.168.153";
				var exportPath = $@"{M}\{folder}\{DateTime.Now:yyMMdd}";
				if (!Directory.Exists(exportPath)) Directory.CreateDirectory(exportPath);
				return $@"{exportPath}\{fileName}";
			}
			catch (Exception ex)
			{
				await errorLogger.LogErrorAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Excel отчёт по выбранному итогу табеля
		/// </summary>
		/// <param name="indicators"></param>
		/// <returns></returns>
		public async Task<string> CreateReportForResultSheetAsync(
			List<EmployeesInIndicatorDto> indicators, CancellationToken token)
		{
			try
			{
				var empInIndItem = indicators.FirstOrDefault();
				if (empInIndItem is null) return string.Empty;

				string fileName = await GetPathDiskMAsync(
					$"Показатели для {empInIndItem.IndicatorItem.DescriptionIndicator}.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportResultSheet.frx");
				report.RegisterData(indicators, "RequestData");
				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;
				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;


				var m = report.FindObject("Text3") as TextObject;
				if (m != null) m.Text = @$"Работники показателя: {empInIndItem.IndicatorItem.DescriptionIndicator}.";

				m = report.FindObject("Text13") as TextObject;
				if (m != null) m.Text = @$"Участок: {empInIndItem.NameDepartmentForApi}.";

				report.Prepare();

				using Excel2007Export excel = new();
				excel.PageBreaks = false;
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}

		/// <summary>
		/// Excel отчёт для Ведомости
		/// </summary>
		/// <param name="indicators"></param>
		/// <returns></returns>   
		public async Task<string> CreateReportForSIZAsync(
			List<EmployeeForSizDto> forSizs, CancellationToken token)
		{
			try
			{
				string fileName = await GetPathDiskMSizAsync(
					$"Ведомость.xlsx", token);

				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				using Report report = new();
				report.Load(@"Report/ReportSIZ.frx");
				report.RegisterData(forSizs, "RequestData", 2);

				var ds = report.GetDataSource("RequestData");
				ds.Alias = "list";
				ds.Enabled = true;

				var band = (DataBand)report.FindObject("DataList");
				band.DataSource = ds;

				report.Prepare();

				using Excel2007Export excel = new();
				excel.Export(report, fileName);

				return fileName;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
				return string.Empty;
			}
		}
	}
}

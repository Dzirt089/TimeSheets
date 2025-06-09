using API.ProductionControl.Data;
using API.ProductionControl.Entityes;
using API.ProductionControl.Models;
using API.ProductionControl.Services;
using API.ProductionControl.Services.Interfaces;
using API.ProductionControl.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Moq;


namespace Tests
{
	[TestClass]
	public sealed class TimeSheetContextServicesIntegrationTests
	{
		private TimeSheetContext _context;
		private ITimeSheetContextServices _services;
		private IErrorLogger _logger;
		private IReportService _report;
		private IMonthlySummaryEmployeeExpOrgsService _expOrgsService;
		private ISizService _sizService;

		[TestInitialize]
		public void Setup()
		{
			var conf = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			var con = conf.GetConnectionString("TestTimeSheet");

			var options = new DbContextOptionsBuilder<TimeSheetContext>()
				.UseSqlServer(con)
				.Options;

			_context = new TimeSheetContext(options);

			_logger = new Mock<IErrorLogger>().Object;

			//var mail = new API.ProductionControl.Services.Mail.MailService(new MailerVKT.Sender());
			//_logger = new ErrorLogger(mail, _context);

			_services = new TimeSheetContextServices(_context, _logger);
			_report = new ReportService(_services, _logger);
			_expOrgsService = new MonthlySummaryEmployeeExpOrgsService(_services, _logger, _report);
			//_sizService = new SizService(_services,_report,)
		}

		

		[TestMethod]
		public async Task TestReportEmployeeExpOrg()
		{
			int month = DateTime.Now.Month;
			int year = DateTime.Now.Year;
			DateTime startPeriod = new DateTime(year: year, month: month, day: 5);
			DateTime endPeriod = new DateTime(year: year, month: month, day: 14);


			DateTime startDate = new DateTime(2025, 1, 1);
			DateTime endDate = new DateTime(2025, 1, 31);

			List<EmployeeExOrg>? employeeExOrgs = await _services.GetEmployeeExOrgsAsync(startDate, endDate);

			var oneEmployee = employeeExOrgs.Where(x => x.EmployeeExOrgAddInRegions.Count != 0).FirstOrDefault();

			List<string> departmentsId = employeeExOrgs
				.SelectMany(x => x.EmployeeExOrgAddInRegions)
				.Where(x => !string.IsNullOrEmpty(x.DepartmentID))
				.Select(x => x.DepartmentID)
				.Distinct()
				.ToList();

			List<EmployeesExOrgForReport> orgForReports = [];
			Dictionary<string, double> departmentAllHoursDict = [];

			foreach (var employee in employeeExOrgs)
			{
				var shiftDict = employee.ShiftDataExOrgs?.ToDictionary(x => (x.WorkDate, x.DepartmentID)) ?? [];

				foreach (var departament in departmentsId)
				{
					double sumHours = 0;
					double sumHoursPeriod = 0;

					EmployeesExOrgForReport? empOrgForRep = new()
					{
						EmployeeExOrgID = employee.EmployeeExOrgID
					};

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						if (!shiftDict.ContainsKey((date, departament)))
						{
							shiftDict[(date, departament)] = new ShiftDataExOrg
							{
								EmployeeExOrgID = employee.EmployeeExOrgID,
								EmployeeExOrg = employee,
								DepartmentID = departament,
								WorkDate = date,
								Hours = string.Empty
							};
						}

						var hours = shiftDict[(date, departament)].Hours;
						switch (date.Day)
						{
							case 1: empOrgForRep.Day1 = hours; break;
							case 2: empOrgForRep.Day2 = hours; break;
							case 3: empOrgForRep.Day3 = hours; break;
							case 4: empOrgForRep.Day4 = hours; break;
							case 5: empOrgForRep.Day5 = hours; break;
							case 6: empOrgForRep.Day6 = hours; break;
							case 7: empOrgForRep.Day7 = hours; break;
							case 8: empOrgForRep.Day8 = hours; break;
							case 9: empOrgForRep.Day9 = hours; break;
							case 10: empOrgForRep.Day10 = hours; break;
							case 11: empOrgForRep.Day11 = hours; break;
							case 12: empOrgForRep.Day12 = hours; break;
							case 13: empOrgForRep.Day13 = hours; break;
							case 14: empOrgForRep.Day14 = hours; break;
							case 15: empOrgForRep.Day15 = hours; break;
							case 16: empOrgForRep.Day16 = hours; break;
							case 17: empOrgForRep.Day17 = hours; break;
							case 18: empOrgForRep.Day18 = hours; break;
							case 19: empOrgForRep.Day19 = hours; break;
							case 20: empOrgForRep.Day20 = hours; break;
							case 21: empOrgForRep.Day21 = hours; break;
							case 22: empOrgForRep.Day22 = hours; break;
							case 23: empOrgForRep.Day23 = hours; break;
							case 24: empOrgForRep.Day24 = hours; break;
							case 25: empOrgForRep.Day25 = hours; break;
							case 26: empOrgForRep.Day26 = hours; break;
							case 27: empOrgForRep.Day27 = hours; break;
							case 28: empOrgForRep.Day28 = hours; break;
							case 29: empOrgForRep.Day29 = hours; break;
							case 30: empOrgForRep.Day30 = hours; break;
							case 31: empOrgForRep.Day31 = hours; break;

						}
						if (hours.TryParseDouble(out double res))
						{
							sumHours += res;

							if (date.Day >= startPeriod.Day && date.Day <= endPeriod.Day)
								sumHoursPeriod += res;
						}
					}
					if (!departmentAllHoursDict.ContainsKey(departament))
					{
						departmentAllHoursDict[departament] = sumHoursPeriod;
					}
					else
					{
						departmentAllHoursDict[departament] += sumHoursPeriod;
					}

					empOrgForRep.SumHours = sumHours.ToString();

					empOrgForRep.Year = startDate.Year;
					empOrgForRep.MonthName = startDate.Month switch
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

					empOrgForRep.NumCategory = employee.NumCategory;
					empOrgForRep.DepartmentID = departament;
					empOrgForRep.FullName = employee.FullName;

					orgForReports.Add(empOrgForRep);
				}
			}

			var sortedOrgForReports = orgForReports
				.Where(x => x.SumHours.TryParseDouble(out double re) && (re > 0))
				.ToList();

			var path = await _report.CreateReportForMonthlySummaryEmployeeExpOrgAsync(sortedOrgForReports, startPeriod, endPeriod);

			// Формирование HTML-сообщения с результатами

			int count = 0;
			double summa = 0;
			string message = string.Empty;
			message += $"За период с {startPeriod:d} по {endPeriod:d}\n";

			message += $"<table border='1' cols='{departmentAllHoursDict.Count + 1}' style='font-family:\"Courier New\", Courier, monospace'>";
			message += $"<tr>";
			message += $"<td style='padding:5px'>Участок</td>";
			message += $"<td style='padding:5px'>Часы</td>";
			message += $"<td style='padding:5px'>Кол-во человек(среднесписочно)</td>";
			message += $"<tr>";
			foreach (var item in departmentAllHoursDict)
			{
				summa += Math.Round(item.Value, 1);
				double result = Math.Round(item.Value / 11.8 / 5);
				count += (int)result;

				message += $"<td style='padding:5px'>{item.Key}</td>";
				message += $"<td style='padding:5px'>{Math.Round(item.Value, 1)}</td>";
				message += $"<td style='padding:5px'>{(int)result}</td>";
				message += $"<tr>";
			}
			message += $"<td style='padding:5px'>Итого:</td>";
			message += $"<td style='padding:5px'>{summa}</td>";
			message += $"<td style='padding:5px'>{count}</td>";

			message += $"</table>";

			await _logger.SendMailTestAsync([path], message);

			Assert.IsNotNull(employeeExOrgs);
			Assert.IsTrue(employeeExOrgs.Count > 0);

			Assert.IsNotNull(orgForReports);
			Assert.IsTrue(orgForReports.Count > 0);

			Assert.IsNotNull(path);
			Assert.IsTrue(!string.IsNullOrEmpty(path));
		}

		[TestMethod]
		public async Task TestMonthlySummaryEmployeeExpOrgsService()
		{
			var startPeriod = new DateTime(year: 2025, month: 1, day: 5).ToString();
			var endPeriod = new DateTime(year: 2025, month: 1, day: 9).ToString();
			bool checking = await _expOrgsService.CreateReportEmployeeExpOrgAsync(startPeriod, endPeriod);

			Assert.IsTrue(checking);
		}
	}
}

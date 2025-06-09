using MailerVKT;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProductionControl.API.Middlewares;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Models.Dtos;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.DataAccess.Sql.Implementation;
using ProductionControl.DataAccess.Sql.Interfaces;
using ProductionControl.Infrastructure.Repositories.Implementation;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.Mail;
using ProductionControl.ServiceLayer.ResultSheetServicesAPI.Implementation;
using ProductionControl.ServiceLayer.ResultSheetServicesAPI.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Implementation;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			#region Настраиваем dbContext в DI

			var config = builder.Configuration;
			builder.Services.AddHttpClient("ProductionApi", client =>
			{
				client.BaseAddress = new Uri(config["ApiSettings:TestUrl"]);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.Timeout = TimeSpan.FromSeconds(30);
			});

			var connString = config.GetConnectionString("TestTimeSheet");
			//var connString = config.GetConnectionString("ConTimeSheet");

			builder.Services.AddDbContext<ProductionControlDbContext>(opt => opt.UseSqlServer(connString));
			builder.Services.AddScoped<IMonthlySummaryEmployeeExpOrgsService, MonthlySummaryEmployeeExpOrgsService>();
			builder.Services.AddScoped<IMonthlyValuesService, MonthlyValuesService>();
			builder.Services.AddScoped<ISizService, SizService>();
			builder.Services.AddScoped<IErrorLogger, ErrorLogger>();
			builder.Services.AddScoped<IDbServices, DbServices>();
			builder.Services.AddScoped<IScheduleForEmployeeService, ScheduleForEmployeeService>();
			builder.Services.AddScoped<IReportService, ReportService>();
			builder.Services.AddScoped<IMonthlySummaryService, MonthlySummaryService>();
			builder.Services.AddScoped<IEmployeesExternalOrganizationsRepository, EmployeesExternalOrganizationsRepository>();
			builder.Services.AddScoped<IEmployeesFactorysRepository, EmployeesFactorysRepository>();
			builder.Services.AddScoped<ISizsRepository, SizsRepository>();
			builder.Services.AddScoped<IResultSheetsService, ResultSheetsService>();

			builder.Services.AddScoped<MailService>();
			builder.Services.AddScoped<Sender>();

			#endregion

			builder.Services.AddAuthorization();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();
			app.UseMiddleware<ExceptionHandlingMiddleware>();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseAuthorization();

			app.MapGet("/", () => "Hello, World!");

			#region ResultSheets
			app.MapPost("/GetDataResultSheet",
				async (IResultSheetsService service, [FromBody] List<TimeSheetItemDto> copyTimeSheet, CancellationToken token) =>
				{
					return await service.ShowResultSheet(copyTimeSheet, token);
				});
			#endregion

			#region Report's
			app.MapGet("/SetScheduleForEmployee",
				async (IScheduleForEmployeeService schedule,
					CancellationToken token) =>
				{
					await schedule.SetScheduleForEmployee(token);
				});
			app.MapGet("/GetOrderForLunch",
				async (IReportService service,
					CancellationToken token) =>
				{
					await service.ProcessingDataReportForLUnchEveryDayAsync(token);
				});
			app.MapGet("/CreateOrderLunchLastMonth/{totalSum}",
				async (IReportService service, string totalSum,
					CancellationToken token) =>
				{
					await service.ProcessingDataForReportLunchLastMonth(totalSum, token);
				});
			app.MapPost("/CreateReportForResultSheet",
				async (IReportService service, [FromBody] List<EmployeesInIndicatorDto> indica,
					CancellationToken token) =>
				{
					return await service.CreateReportForResultSheetAsync(indica, token);
				});
			app.MapGet("/GetStatementSiz",
				async (ISizService service, CancellationToken token) =>
				{
					await service.DivisionLogikalCalcSizAsync(token);
				});
			app.MapGet("/CreateReportForMonthlySummary/{month}/{year}",
				async (IMonthlySummaryService service, int month, int year,
					CancellationToken token) =>
				{
					await service.GetDataForMonthlySummary(month, year, token);
				});
			app.MapGet("/CreateReportForMonthlySummaryEmployeeExpOrg/{startPeriod}/{endPeriod}",
				async (IMonthlySummaryEmployeeExpOrgsService service, string startPeriod, string endPeriod,
					CancellationToken token) =>
				{
					await service.CreateReportEmployeeExpOrgAsync(startPeriod, endPeriod, token);
				});
			#endregion

			#region EmployeeSheet
			var employeeSheets = app.MapGroup("EmployeeSheet");

			employeeSheets.MapPost("/UpdateEmployees",
			async (IEmployeesFactorysRepository service, [FromBody] List<Employee> allPeople, CancellationToken token) =>
				{
					return await service.UpdateEmployeesAsync(allPeople, token);
				});

			employeeSheets.MapPost("/SetDataForTimeSheet",
			async (IEmployeesFactorysRepository service, [FromBody] DataForTimeSheet dataForTimeSheet, CancellationToken token) =>
				{
					return await service.SetDataForTimeSheetAsync(dataForTimeSheet, token);
				});

			employeeSheets.MapPost("/GetEmployeesForReportLunch",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					return await service.GetEmployeesForReportLunchAsync(startEndDate, token);
				});

			employeeSheets.MapPost("/GetEmployeesForLunch",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					return await service.GetEmployeesForLunchAsync(token);
				});

			employeeSheets.MapPost("/GetEmployees",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					return await service.GetEmployeesAsync(startEndDate, token);
				});

			employeeSheets.MapPost("/CancelDismissalEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					return await service.CancelDismissalEmployeeAsync(idEmployeeDateTime, token);
				});

			employeeSheets.MapPost("/CleareDataForFormulateReportForLunchEveryDayDb",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					await service.CleareDataForFormulateReportForLunchEveryDayDbAsync(token);
				});

			employeeSheets.MapPost("/ClearIdAccessRightFromDepartmentDb",
				async (IEmployeesFactorysRepository service, [FromBody] DataClearIdAccessRight dataClearId, CancellationToken token) =>
				{
					return await service.ClearIdAccessRightFromDepartmentDb(dataClearId, token);
				});

			employeeSheets.MapPost("/ClearLastDeport",
				async (IEmployeesFactorysRepository service, [FromBody] DataForClearLastDeport dataForClear, CancellationToken token) =>
				{
					await service.ClearLastDeport(dataForClear, token);
				});

			employeeSheets.MapPost("/GetAccessRightsEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] string userName, CancellationToken token) =>
				{
					return await service.GetAccessRightsEmployeeAsync(userName, token);
				});

			employeeSheets.MapPost("/GetAllDepartments",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					return await service.GetAllDepartmentsAsync(token);
				});

			employeeSheets.MapPost("/GetDepartmentProduction",
				async (IEmployeesFactorysRepository service, [FromBody] string depId, CancellationToken token) =>
				{
					return await service.GetDepartmentProductionAsync(depId, token);
				});

			employeeSheets.MapPost("/GetEmployeeById",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction? itemDepartment, CancellationToken token) =>
				{
					return await service.GetEmployeeByIdAsync(itemDepartment, token);
				});

			employeeSheets.MapPost("/GetEmployeeForCartotecas",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction department, CancellationToken token) =>
				{
					return await service.GetEmployeeForCartotecasAsync(department, token);
				});

			employeeSheets.MapPost("/GetEmployeeIdAndDate",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					return await service.GetEmployeeIdAndDateAsync(idEmployeeDateTime, token);
				});

			employeeSheets.MapPost("/GetTotalWorkingHoursWithOverdayHoursForRegions043and044",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					return await service.GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(startEndDate, token);
				});

			employeeSheets.MapPost("/SetDataEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] Employee employee, CancellationToken token) =>
				{
					await service.SetDataEmployeeAsync(employee, token);
				});

			employeeSheets.MapPost("/SetNamesDepartment",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					await service.SetNamesDepartmentAsync(token);
				});

			employeeSheets.MapPost("/SetTotalWorksDays",
				async (IEmployeesFactorysRepository service, [FromBody] ShiftData shiftData, CancellationToken token) =>
				{
					await service.SetTotalWorksDaysAsync(shiftData, token);
				});

			employeeSheets.MapPost("/UpdateDataTableNewEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] DateTime periodDate, CancellationToken token) =>
				{
					return await service.UpdateDataTableNewEmployeeAsync(periodDate, token);
				});

			employeeSheets.MapPost("/UpdateDepartament",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction? itemDepartment, CancellationToken token) =>
				{
					await service.UpdateDepartamentAsync(itemDepartment, token);
				});

			employeeSheets.MapPost("/UpdateDismissalDataEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					return await service.UpdateDismissalDataEmployeeAsync(idEmployeeDateTime, token);
				});

			employeeSheets.MapPost("/UpdateIsLunchingDb",
				async (IEmployeesFactorysRepository service, [FromBody] long idEmployee, CancellationToken token) =>
				{
					return await service.UpdateIsLunchingDbAsync(idEmployee, token);
				});

			employeeSheets.MapPost("/UpdateLunchEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					return await service.UpdateLunchEmployeeAsync(idEmployeeDateTime, token);
				});

			//employeeSheets.MapPost("/",
			//	async (IEmployeesFactorysRepository service, [FromBody] , CancellationToken token) =>
			//	{
			//		return await service.();
			//	});

			//employeeSheets.MapPost("/",
			//	async (IEmployeesFactorysRepository service, [FromBody] , CancellationToken token) =>
			//	{
			//		return await service.();
			//	});
			#endregion

			app.Run();
		}
	}
}

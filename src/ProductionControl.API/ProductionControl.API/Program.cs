using MailerVKT;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProductionControl.API.Middlewares;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
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

			#region Report's для службы VKT, вызываются по рассписанию.

			//Вызывается из службы VKT, которая формирует Excel ведомость по СИЗ-ам
			app.MapGet("GetStatementSiz",
				async (ISizService service, CancellationToken token) =>
				{
					await service.DivisionLogikalCalcSizAsync(token);
					return Results.Ok(true);
				});

			//Вызывается из службы VKT, которая заполняет график сотрудника на месяц по его графику из ИС-ПРО
			app.MapGet("SetScheduleForEmployee",
				async (IScheduleForEmployeeService schedule,
					CancellationToken token) =>
				{
					await schedule.SetScheduleForEmployee(token);
					return Results.Ok(true);
				});

			#endregion

			#region ResultSheets

			var resultSheets = app.MapGroup("ResultSheets");
			resultSheets.MapPost("GetDataResultSheet",
				async (IResultSheetsService service, [FromBody] List<TimeSheetItemDto> copyTimeSheet, CancellationToken token) =>
				{
					var result = await service.ShowResultSheet(copyTimeSheet, token);
					return Results.Ok(result);
				});

			#endregion

			#region Report's
			var reports = app.MapGroup("Reports");

			reports.MapGet("GetOrderForLunch",
				async (IReportService service,
					CancellationToken token) =>
				{
					await service.ProcessingDataReportForLUnchEveryDayAsync(token);
					return Results.Ok(true);
				});

			reports.MapPost("CreateOrderLunchLastMonth",
				async (IReportService service, [FromBody] string totalSum,
					CancellationToken token) =>
				{
					await service.ProcessingDataForReportLunchLastMonth(totalSum, token);
					return Results.Ok(true);
				});

			reports.MapPost("CreateReportForResultSheet",
				async (IReportService service, [FromBody] List<EmployeesInIndicatorDto> indica,
					CancellationToken token) =>
				{
					var result = await service.CreateReportForResultSheetAsync(indica, token);
					return Results.Ok(result);
				});

			reports.MapGet("CreateReportForMonthlySummary",
				async (IMonthlySummaryService service, [FromBody] DateTime date,
					CancellationToken token) =>
				{
					await service.GetDataForMonthlySummary(date, token);
					return Results.Ok(true);
				});

			reports.MapPost("CreateReportForMonthlySummaryEmployeeExpOrg",
				async (IMonthlySummaryEmployeeExpOrgsService service, [FromBody] StartEndDateTime startEndDate,
					CancellationToken token) =>
				{
					await service.CreateReportEmployeeExpOrgAsync(startEndDate, token);
					return Results.Ok(true);
				});

			#endregion

			#region EmployeeSheet
			var employeeSheets = app.MapGroup("EmployeeSheet");

			employeeSheets.MapPost("UpdateEmployees",
			async (IEmployeesFactorysRepository service, [FromBody] List<Employee> allPeople, CancellationToken token) =>
				{
					var result = await service.UpdateEmployeesAsync(allPeople, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("SetDataForTimeSheet",
			async (IEmployeesFactorysRepository service, [FromBody] DataForTimeSheet dataForTimeSheet, CancellationToken token) =>
				{
					var result = await service.SetDataForTimeSheetAsync(dataForTimeSheet, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetEmployeesForReportLunch",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					var result = await service.GetEmployeesForReportLunchAsync(startEndDate, token);
					return Results.Ok(result);
				});

			employeeSheets.MapGet("GetEmployeesForLunch",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					var result = await service.GetEmployeesForLunchAsync(token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetEmployees",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					var result = await service.GetEmployeesAsync(startEndDate, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("CancelDismissalEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					var result = await service.CancelDismissalEmployeeAsync(idEmployeeDateTime, token);
					return Results.Ok(result);
				});

			employeeSheets.MapDelete("CleareDataForFormulateReportForLunchEveryDayDb",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					await service.CleareDataForFormulateReportForLunchEveryDayDbAsync(token);
					return Results.Ok(true);
				});

			employeeSheets.MapPost("ClearIdAccessRightFromDepartmentDb",
				async (IEmployeesFactorysRepository service, [FromBody] DataClearIdAccessRight dataClearId, CancellationToken token) =>
				{
					var result = await service.ClearIdAccessRightFromDepartmentDb(dataClearId, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("ClearLastDeport",
				async (IEmployeesFactorysRepository service, [FromBody] DataForClearLastDeport dataForClear, CancellationToken token) =>
				{
					await service.ClearLastDeport(dataForClear, token);
					return Results.Ok(true);
				});

			employeeSheets.MapPost("GetAccessRightsEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] string userName, CancellationToken token) =>
				{
					var result = await service.GetAccessRightsEmployeeAsync(userName, token);
					return Results.Ok(result);
				});

			employeeSheets.MapGet("GetAllDepartments",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					var result = await service.GetAllDepartmentsAsync(token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetDepartmentProduction",
				async (IEmployeesFactorysRepository service, [FromBody] string depId, CancellationToken token) =>
				{
					var result = await service.GetDepartmentProductionAsync(depId, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetEmployeeById",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction itemDepartment, CancellationToken token) =>
				{
					var result = await service.GetEmployeeByIdAsync(itemDepartment, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetEmployeeForCartotecas",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction department, CancellationToken token) =>
				{
					var result = await service.GetEmployeeForCartotecasAsync(department, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetEmployeeIdAndDate",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					var result = await service.GetEmployeeIdAndDateAsync(idEmployeeDateTime, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("GetTotalWorkingHoursWithOverdayHoursForRegions043and044",
				async (IEmployeesFactorysRepository service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					var result = await service.GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(startEndDate, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("SetDataEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] Employee employee, CancellationToken token) =>
				{
					await service.SetDataEmployeeAsync(employee, token);
					return Results.Ok(true);
				});

			employeeSheets.MapGet("SetNamesDepartment",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					await service.SetNamesDepartmentAsync(token);
					return Results.Ok(true);
				});

			employeeSheets.MapPost("SetTotalWorksDays",
				async (IEmployeesFactorysRepository service, [FromBody] ShiftData shiftData, CancellationToken token) =>
				{
					await service.SetTotalWorksDaysAsync(shiftData, token);
					return Results.Ok(true);
				});

			employeeSheets.MapPost("UpdateDataTableNewEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] DateTime periodDate, CancellationToken token) =>
				{
					var result = await service.UpdateDataTableNewEmployeeAsync(periodDate, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("UpdateDepartament",
				async (IEmployeesFactorysRepository service, [FromBody] DepartmentProduction itemDepartment, CancellationToken token) =>
				{
					await service.UpdateDepartamentAsync(itemDepartment, token);
					return Results.Ok(true);
				});

			employeeSheets.MapPost("UpdateDismissalDataEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					var result = await service.UpdateDismissalDataEmployeeAsync(idEmployeeDateTime, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("UpdateIsLunchingDb",
				async (IEmployeesFactorysRepository service, [FromBody] long idEmployee, CancellationToken token) =>
				{
					var result = await service.UpdateIsLunchingDbAsync(idEmployee, token);
					return Results.Ok(result);
				});

			employeeSheets.MapPost("UpdateLunchEmployee",
				async (IEmployeesFactorysRepository service, [FromBody] IdEmployeeDateTime idEmployeeDateTime, CancellationToken token) =>
				{
					var result = await service.UpdateLunchEmployeeAsync(idEmployeeDateTime, token);
					return Results.Ok(result);
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

			#region Employees External Organizations
			var employeesExternalOrganizations = app.MapGroup("EmployeesExternalOrganizations");

			#endregion

			app.Run();
		}
	}
}

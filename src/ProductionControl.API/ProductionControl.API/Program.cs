using MailerVKT;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using ProductionControl.API.Middlewares;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.DataAccess.Sql.Implementation;
using ProductionControl.DataAccess.Sql.Interfaces;
using ProductionControl.Infrastructure.Repositories.Implementation;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.Mail;
using ProductionControl.ServiceLayer.PlannedLaborServicesAPI.Implementation;
using ProductionControl.ServiceLayer.PlannedLaborServicesAPI.Interfaces;
using ProductionControl.ServiceLayer.ResultSheetServicesAPI.Implementation;
using ProductionControl.ServiceLayer.ResultSheetServicesAPI.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Implementation;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

using System.Reflection;
using System.Text.Json.Serialization;

namespace ProductionControl.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.ConfigureHttpJsonOptions(options =>
			{
				options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				options.SerializerOptions.MaxDepth = 2048;
			});
			builder.Services.Configure<KestrelServerOptions>(options =>
			{
				options.Limits.MaxRequestBodySize = 500_000_000;
			});

			#region Настраиваем DI

			var config = builder.Configuration;
			builder.Services.AddHttpClient("ProductionApi", client =>
			{
				client.BaseAddress = new Uri(config["ApiSettings:TestUrl"]);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.Timeout = TimeSpan.FromSeconds(30);
			});

			var connString = config.GetConnectionString("TestTimeSheet");
			//var connString = config.GetConnectionString("ConTimeSheet");
			builder.Services.AddDbContext<ProductionControlDbContext>(opt =>
				opt.UseSqlServer(connString, sqlOptions =>
					sqlOptions.UseCompatibilityLevel(110))); //Задаем в коде максимальный уровень совместимости (говорим EF Core, что он работает с SQL Server 2012)


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
			builder.Services.AddScoped<IPlannedLaborServices, PlannedLaborServices>();

			builder.Services.AddScoped<MailService>();
			builder.Services.AddScoped<Sender>();

			#endregion

			builder.Services.AddAuthorization();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductionControl.API", Version = "v1" });
				options.CustomSchemaIds(x => x.FullName);
				var xmlFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
				var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
				options.IncludeXmlComments(xmlFilePath);
			}); ;

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
					await schedule.SetScheduleForEmployee(CancellationToken.None);
					return Results.Ok(true);
				});

			#endregion

			#region ResultSheets

			var resultSheets = app.MapGroup("ResultSheets");
			resultSheets.MapPost("GetDataResultSheet",
				async (IResultSheetsService serviceResult, IEmployeesFactorysRepository service, [FromBody] DataForTimeSheet dataForTimeSheet, CancellationToken token) =>
				{
					var copyTimeSheet = await service.SetDataForTimeSheetAsync(dataForTimeSheet, token);
					var result = await serviceResult.ShowResultSheet(copyTimeSheet, token);
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

			reports.MapPost("CreateReportForMonthlySummary",
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

			employeeSheets.MapPost("SaveEmployeeCardNums",
				async (IEmployeesFactorysRepository service, [FromBody] List<EmployeeCardNumShortNameId> employeeCardNums, CancellationToken token) =>
				{
					await service.SaveEmployeeCardNumsAsync(employeeCardNums, token);
					return Results.Ok(true);
				});

			employeeSheets.MapGet("GetEmployeeEmptyCardNums",
				async (IEmployeesFactorysRepository service, CancellationToken token) =>
				{
					var result = await service.GetEmployeeEmptyCardNumsAsync(token);
					return Results.Ok(result);
				});

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
				async (IPlannedLaborServices service, [FromBody] StartEndDateTime startEndDate, CancellationToken token) =>
				{
					await service.CalcPlannedLaborForRegions043and044EmployeesAndEmployeesExOrg(startEndDate, token);
					return Results.Ok(true);
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

			#endregion

			#region Employees External Organizations

			var employeesExternalOrganizations = app.MapGroup("EmployeesExternalOrganizations");

			employeesExternalOrganizations.MapPost("SaveEmployeeExOrgCardNums",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] List<EmployeeExOrgCardNumShortNameId> employeeExOrgCards,
				CancellationToken token) =>
				{
					await service.SaveEmployeeExOrgCardNumsAsync(employeeExOrgCards, token);
					return Results.Ok(true);
				});

			employeesExternalOrganizations.MapGet("GetEmployeeExOrgEmptyCardNums",
				async (IEmployeesExternalOrganizationsRepository service, CancellationToken token) =>
				{
					var result = await service.GetEmployeeExOrgEmptyCardNumsAsync(token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapPost("SetDataForTimeSheetExOrg",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] DataForTimeSheetExOrgs dataForTimeSheetEx,
				CancellationToken token) =>
				{
					var result = await service.SetDataForTimeSheetExOrgAsync(dataForTimeSheetEx, token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapPost("GetEmployeeExOrgsOnDate",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] StartEndDateTimeDepartmentID startEndDateTimeDepartmentID,
				CancellationToken token) =>
				{
					var result = await service.GetEmployeeExOrgsOnDateAsync(startEndDateTimeDepartmentID, token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapGet("GetEmployeeExOrgsAll",
				async (IEmployeesExternalOrganizationsRepository service, CancellationToken token) =>
				{
					var result = await service.GetEmployeeExOrgsAllAsync(token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapPost("UpdateEmployeeExOrg",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg,
				CancellationToken token) =>
				{
					await service.UpdateEmployeeExOrgAsync(dataForUpdateEmloyeeExOrg, token);
					return Results.Ok(true);
				});

			employeesExternalOrganizations.MapPost("AddEmployeeExOrg",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] EmployeeExOrg exOrg,
				CancellationToken token) =>
				{
					await service.AddEmployeeExOrgAsync(exOrg, token);
					return Results.Ok(true);
				});

			employeesExternalOrganizations.MapPost("GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgs",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] StartEndDateTime startEndDateTime,
				CancellationToken token) =>
				{
					var result = await service
					.GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(startEndDateTime, token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapPost("UpdateDismissalDataEmployee",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] IdEmployeeExOrgDateTime idEmployeeExOrgDate,
				CancellationToken token) =>
				{
					var result = await service.UpdateDismissalDataEmployeeAsync(idEmployeeExOrgDate, token);
					return Results.Ok(result);
				});

			employeesExternalOrganizations.MapPost("SetTotalWorksDaysExOrg",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] ShiftDataExOrg shiftDataExOrg,
				CancellationToken token) =>
				{
					await service.SetTotalWorksDaysExOrgAsync(shiftDataExOrg, token);
					return Results.Ok(true);
				});

			employeesExternalOrganizations.MapPost("GetEmployeeExOrgs",
				async (IEmployeesExternalOrganizationsRepository service,
				[FromBody] StartEndDateTime startEndDateTime,
				CancellationToken token) =>
				{
					var result = await service.GetEmployeeExOrgsAsync(startEndDateTime, token);
					return Results.Ok(result);
				});

			#endregion

			#region Siz Employee

			var sizs = app.MapGroup("SizEmployee");
			sizs.MapGet("GetSizUsageRate",
				async (ISizsRepository service, CancellationToken token) =>
				{
					var result = await service.GetSizUsageRateAsync(token);
					return Results.Ok(result);
				});

			#endregion

			app.Run();
		}
	}
}

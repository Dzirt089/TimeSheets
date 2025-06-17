using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.PlannedLaborServicesAPI.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

using System.Text;

namespace ProductionControl.ServiceLayer.PlannedLaborServicesAPI.Implementation
{
	public class PlannedLaborServices(
		IEmployeesFactorysRepository employeesFactorys,
		IEmployeesExternalOrganizationsRepository employeesExternal,
		IErrorLogger errorLogger) : IPlannedLaborServices
	{
		private readonly IEmployeesFactorysRepository _employeesFactorys = employeesFactorys;
		private readonly IEmployeesExternalOrganizationsRepository _employeesExternal = employeesExternal;
		private readonly IErrorLogger _errorLogger = errorLogger;

		public async Task CalcPlannedLaborForRegions043and044EmployeesAndEmployeesExOrg(StartEndDateTime startEndDate, CancellationToken token = default)
		{
			// Получение данных о рабочих часах и сверхурочных часах за указанный период для текущего пользователя
			var list = await _employeesFactorys
				.GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(startEndDate, token);

			list = list.Where(x =>
				x.ValidateEmployee(startEndDate.StartDate.Month,
				startEndDate.StartDate.Year))
				.ToList();

			var listExpOrgs = await _employeesExternal
				.GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(startEndDate, token);



			listExpOrgs = listExpOrgs.Where(x =>
			x.ValidateEmployee(startEndDate.StartDate.Month,
				startEndDate.StartDate.Year))
				.ToList();

			listExpOrgs = listExpOrgs.Where(x => x.EmployeeExOrgAddInRegions != null && x.EmployeeExOrgAddInRegions.Any()).ToList();

			// Списки для хранения общих рабочих часов и сверхурочных часов
			List<double> totalHourse = [];
			List<double> totalOverday = [];
			List<int> days = [];
			// Итерация по каждому дню в указанном периоде
			for (var date = startEndDate.StartDate; date <= startEndDate.EndDate; date = date.AddDays(1))
			{
				days.Add(date.Day);
				double summaForDay = 0;
				double summaOverday = 0;

				bool isNotWeekend = false;
				bool isPreHoliday = false;

				// Обработка данных для каждого элемента в списке
				foreach (var item in list)
				{
					// Подсчет общего количества рабочих часов для указанного дня
					summaForDay += item.Shifts
						.Where(x => x.ValidationWorkingDaysOnDate(date))
						.Select(s => double.TryParse(s.Hours, out double tempValue) ? tempValue : 0)
						.SingleOrDefault();

					// Подсчет общего количества сверхурочных часов для указанного дня
					summaOverday += item.Shifts
						.Where(x => x.ValidationOverdayDaysOnDate(date))
						.Select(s => double.TryParse(s.Overday?.Replace(".", ","), out double tempValue)
									 ? tempValue : 0)
						.SingleOrDefault();
				}

				// Проверка, является ли день не выходным
				isNotWeekend = list.Any(z => z.Shifts.Any(x => x.ValidationWorkingDaysOnDate(date)));

				// Проверка, является ли день предпраздничным
				isPreHoliday = list
					.SelectMany(x => x.Shifts)
					.Where(z => z.ValidationWorkingDaysOnDate(date))
					.Select(s => s.IsPreHoliday)
					.FirstOrDefault();

				// Корректировка общего количества рабочих часов в зависимости от типа дня
				if (isNotWeekend)
				{
					if (isPreHoliday)
						summaForDay -= 14;
					else
						summaForDay -= 16;
				}

				double sumHoursInday = 0;

				foreach (var item in listExpOrgs)
				{
					sumHoursInday += item.ShiftDataExOrgs
						.Where(x => x.ValidationWorkingDaysOnDate(date))
						.Select(x => x.Hours.TryParseDouble(out double res) ? res : 0)
						.SingleOrDefault();
				}

				// Добавление подсчитанных значений в соответствующие списки
				totalHourse.Add(summaForDay + sumHoursInday);
				totalOverday.Add(summaOverday);
			}

			// Формирование HTML-сообщения с результатами
			var message = new StringBuilder();
			message.Append($"<table border='1' cols='{totalHourse.Count}' style='font-family:\"Courier New\", Courier, monospace'>");
			message.Append($"<tr>");

			foreach (var item in days)
				message.Append($"<td style='padding:5px'>{item}</td>");

			message.Append($"<tr>");
			foreach (var item in totalHourse)
				message.Append($"<td style='padding:5px'>{Math.Round(item, 1)}</td>");

			message.Append($"<tr>");
			foreach (var item in totalOverday)
				message.Append($"<td style='padding:5px'>{item}</td>");

			message.Append($"</table>");


			// Отправка сформированного сообщения по электронной почте
			await _errorLogger.SendMailPlanLaborAsync(message.ToString());
		}
	}
}

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.DataAccess.Classes.Utils;
using ProductionControl.DataAccess.Sql.Interfaces;
using ProductionControl.Infrastructure.Repositories.Interfaces;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	/// <summary>
	/// Класс-сервис, который заполняет график сотрудников на месяц вперед по его графику из ИС-ПРО
	/// </summary>
	/// <param name="dbContext">контекст EF Core</param>
	/// <param name="dbServices">класс для работы с БД ИС-ПРО</param>
	/// <param name="errorLogger">класс, по ведению ошибок и логгов</param>
	public class ScheduleForEmployeeService(
		IEmployeesFactorysRepository contextServices,
		IErrorLogger errorLogger,
		IDbServices dbServices)
		: IScheduleForEmployeeService
	{

		/// <summary>
		/// Все сотрудники
		/// </summary>
		internal List<Employee>? AllPeople { get; private set; }

		/// <summary>
		/// Будущий месяц
		/// </summary>
		internal int FutureMonht { get; private set; }

		/// <summary>
		/// Будущая дата
		/// </summary>
		internal DateTime FutureDate { get; private set; }

		/// <summary>
		/// Метод, который заполняет график сотрудника на месяц по его графику из ИС-ПРО
		/// </summary>
		/// <returns></returns>
		public async Task SetScheduleForEmployee(CancellationToken token)
		{
			try
			{
				//Программа должна должна стабильно на месяц вперед составлять график.
				//Подготавливаем месяц и дату
				if (DateTime.Now.Month != 12)
				{
					FutureMonht = DateTime.Now.Month + 1;
					FutureDate = DateTime.Now.AddMonths(1);
				}
				else
				{
					FutureMonht = 1;
					FutureDate = new DateTime(DateTime.Now.AddYears(1).Year, FutureMonht, 1);
				}

				//Устанавливаем даты на начало и конец прогнозируемого месяца
				var startDate = new DateTime(FutureDate.Year, FutureMonht, 1);
				var endDate = new DateTime(FutureDate.Year, FutureMonht,
				DateTime.DaysInMonth(FutureDate.Year, FutureMonht));

				StartEndDateTime startEndDate = new StartEndDateTime { StartDate = startDate, EndDate = endDate };

				//Выбираем всех людей из БД, в рамках начала и конца выбранного месяца в сменах
				AllPeople = await contextServices.GetEmployeesAsync(startEndDate, token);

				if (AllPeople == null || AllPeople.Count == 0) return; //Проверка

				//Проводим валидацию сотрудников на обстоятельства: уволнение и приёма на работу. 
				//Например: Уволенный в июне 2020 сотрудник не должен быть в табеле в июле 2024.
				//И нанятый новый человек в августе 2024 не должен быть в табеле 2023 года.
				AllPeople = AllPeople.Where(x => x.ValidateEmployee(FutureMonht, FutureDate.Year)).ToList();

				//Список участков, на которых работают люди.
				var listWithNumSchedule = AllPeople
					.AsParallel()
					.Where(w => !string.IsNullOrEmpty(w.NumGraf))
					.Select(s => s.NumGraf)
					.Distinct()
					.ToList();
				//Проверка
				if (listWithNumSchedule == null || listWithNumSchedule.Count() == 0) return;

				//Получаем все графики работ по участкам и дате, уже подготовленные для работы с ними
				var listWithSchedule = await dbServices.GetGrafAsync(listWithNumSchedule, FutureDate, token);

				//Проверка
				if (listWithSchedule == null || listWithSchedule.Count() == 0) return;

				//Цикл, в котором перебираем все данные из БД приложения, и сравниваем с данными из ИС_ПРО. Если данные разнятся - то переписываем их.
				foreach (var employee in AllPeople)
				{
					var shiftDict = employee.Shifts?.ToDictionary(x => x.WorkDate) ??
						new Dictionary<DateTime, ShiftData>();

					for (var date = startDate; date <= endDate; date = date.AddDays(1))
					{
						var dataScheduleByDate = listWithSchedule
							.FirstOrDefault(x => x.NumGraf == employee.NumGraf
							&& x.DateWithShift == date);

						var hourTempStandartSheetString = dataScheduleByDate?.HoursSchedule ?? string.Empty;
						var hourTempFromBEST = dataScheduleByDate?.CountHoursWithShift.ToString() ?? string.Empty;
						var shiftTemp = dataScheduleByDate?.ShiftSchedule ?? string.Empty;
						bool preholiday = false;

						if (hourTempStandartSheetString.TryParseDouble(out double hourTempStandartDouble))
						{
							if (hourTempStandartDouble - dataScheduleByDate?.CountHoursWithShift == 1)
								preholiday = true;
						}


						if (!shiftDict.ContainsKey(date) ||
							shiftDict[date].Hours != hourTempFromBEST ||
							shiftDict[date].Shift != shiftTemp ||
							shiftDict[date].IsPreHoliday != preholiday)
						{
							shiftDict[date] = new ShiftData
							{
								EmployeeID = employee.EmployeeID,
								WorkDate = date,
								Employee = employee,
								Shift = shiftTemp,
								Hours = hourTempFromBEST,
								Overday = string.Empty,
								IsPreHoliday = preholiday
							};
						}
					}
					employee.Shifts = shiftDict.Values.ToList();
				}

				int row = await contextServices.UpdateEmployeesAsync(AllPeople, token);
				if (row > 0)
				{
					await errorLogger.ProcessingLogAsync($@"<pre>
Успешно обновлен график табеля в API, методом 'SetScheduleForEmployee'.
С {startDate} по {endDate}. 
Кол-во затронутых строк: {row}. 
</pre>");
				}
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex);
			}
		}
	}
}

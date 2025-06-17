using Microsoft.EntityFrameworkCore;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;
using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.Infrastructure.Repositories.Interfaces;

namespace ProductionControl.Infrastructure.Repositories.Implementation
{
	public class SizsRepository(ProductionControlDbContext context) : ISizsRepository
	{
		private readonly ProductionControlDbContext _context = context;

		public async Task<OrderNumberOnDate?> GetOrderNumberOnDateAsync(CancellationToken token)
		{
			var orderNumberOnDate = await _context.OrderNumberOnDates.SingleOrDefaultAsync(token);
			return orderNumberOnDate;
		}

		/// <summary>
		/// Получение id склада УЧАСТКА, куда будут выдаваться СИЗ-ы со склада 06-05
		/// </summary>
		/// <param name="code">строка шифра участка. например: 03 (техотдел) </param>
		public async Task<int> GetCodeIDRegionAsync(string code, CancellationToken token)
		{
			var result = await _context.IssueSizs
					.Where(x => x.code != null
						&& x.codeBest != "06-05" && x.code.Contains(code))
					.Select(s => s.rcdBest)
					.FirstOrDefaultAsync(token) ?? 0;

			return result;
		}

		/// <summary>
		/// Получение id склада 06-05, для создания заявки в ТНО (ИС-ПРО) о выдачи СИЗ со склада на склад участка.
		/// </summary>
		/// <param name="code">строка шифра склада. например: 06-05</param>
		public async Task<int> GetWarehouseIDAsync(string code, CancellationToken token)
		{
			var result = await _context.IssueSizs
					.Where(x => x.codeBest != null && x.codeBest.Contains(code))
					.Select(e => e.rcdBest)
					.FirstOrDefaultAsync(token) ?? 0;

			return result;
		}

		/// <summary>
		/// Получает полные данные по СИЗ.
		/// </summary>
		/// <returns>Список данных по СИЗ.</returns>
		public async Task<List<SizUsageRate>> GetSizUsageRateAsync(CancellationToken token)
		{
			return await _context.SizUsageRates
					.Include(i => i.Siz)
					.Include(i => i.UsageNorm)
					.OrderBy(o => o.UsageNorm.UsageNormID)
					.ToListAsync(token);
		}

		/// <summary>
		/// Получение списка выданных СИЗ-ов на сотрудников с первого числа месяца
		/// </summary>
		/// <returns></returns>
		public async Task<List<DataSizForMonth>> GetAllDataSizForMonthsAsync(CancellationToken token)
		{
			return await _context.DataSizForMonths
				.ToListAsync(token);
		}

		/// <summary>
		/// Получение списка выданных СИЗ-ов для сотрудника
		/// </summary>
		public async Task<List<DataSizForMonth>> GetDataSizForMonthsAsync(long employeeID, CancellationToken token)
		{
			return await _context.DataSizForMonths
					.Where(x => x.EmployeeID == employeeID)
					.ToListAsync(token);
		}

		/// <summary>
		/// Пакетное обновление 
		/// </summary>
		public async Task UpdateDataSizForMonthAsync(List<DataSizForMonth> datas, CancellationToken token)
		{
			await using var transactions = await _context.Database.BeginTransactionAsync(token);

			var allList = await _context.DataSizForMonths.ToListAsync(token);

			foreach (var item in allList)
			{
				var one = datas.FirstOrDefault(x => x.EmployeeID == item.EmployeeID &&
				x.SizID == item.SizID);

				if (one != null)
				{
					item.CountExtradite = one.CountExtradite;
					item.LifeTime = one.LifeTime;
				}
			}

			await _context.SaveChangesAsync(token);
			await transactions.CommitAsync(token);
		}

		/// <summary>
		/// Пакетное добавление 
		/// </summary>
		public async Task AddDataSizForMonthAsync(List<DataSizForMonth> datas, CancellationToken token)
		{
			await using var transactions = await _context.Database.BeginTransactionAsync(token);
			_context.DataSizForMonths?.AddRangeAsync(datas, token);

			await _context.SaveChangesAsync(token);
			await transactions.CommitAsync(token);
		}

		public async Task AddOrderNumberOnDateAsync(OrderNumberOnDate onDate, CancellationToken token)
		{
			await using var transactions = await _context.Database.BeginTransactionAsync(token);
			await _context.OrderNumberOnDates.AddAsync(onDate, token);
			await _context.SaveChangesAsync(token);
			await transactions.CommitAsync(token);
		}

		public async Task UpdateOrderNumberOnDateAsync(OrderNumberOnDate onDate, CancellationToken token)
		{
			await using var transactions = await _context.Database.BeginTransactionAsync(token);
			try
			{
				_context.OrderNumberOnDates.Update(onDate);
				await _context.SaveChangesAsync(token);
				await transactions.CommitAsync(token);
			}
			catch (Exception ex)
			{
				await transactions?.RollbackAsync(token);
			}
		}

		/// <summary>
		/// Выбор людей для расчёта СИЗ на первое число, у которых есть нормы СИЗ, они не уволенны.
		/// </summary>
		/// <returns></returns>
		public async Task<List<Employee>> GetEmployeesForSizOneDayAsync(CancellationToken token)
		{
			var result = await _context.Employees
					.Include(i => i.DepartmentProduction)
					.Include(i => i.UsageNorm)
					.Where(x => x.IsDismissal == false && x.UsageNormID != null)
					.ToListAsync(token);

			return result;
		}

		/// <summary>
		/// Выбор людей для расчёта СИЗ за период дат, у которых есть нормы СИЗ, они не уволенны.
		/// </summary>
		public async Task<List<Employee>> GetEmployeesForSizFifteenDayAsync(
			DateTime startDate, DateTime endDate, CancellationToken token)
		{
			var result = await _context.Employees
					.Include(i => i.DepartmentProduction)
					.Include(i => i.Shifts
						.Where(d => d.WorkDate >= startDate && d.WorkDate <= endDate))
					.Include(i => i.UsageNorm)
					.Where(x => x.IsDismissal == false &&
					x.UsageNormID != null)
					.ToListAsync(token);

			return result;
		}
	}
}

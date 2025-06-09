using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.Models.Dtos;
using ProductionControl.DataAccess.Sql.Interfaces;

namespace ProductionControl.DataAccess.Sql.Implementation
{
	public sealed class DbServices : IDbServices
	{
		private string ConBest;
		private string ConVKT;

		public DbServices(IConfiguration configuration)
		{
			ConBest = configuration.GetConnectionString("ConBest") ?? string.Empty;
			ConVKT = configuration.GetConnectionString("VKT") ?? string.Empty;
		}

		/// <summary>
		/// Получаем код (id) склада для формирования заявки ТНО
		/// </summary>
		/// <param name="skladBest">строковое обозначение склада</param>
		/// <returns>номер склада в ИС-ПРО</returns>
		public async Task<int> GetrcdBestAsync(string skladBest,
					CancellationToken token)
		{
			await using SqlConnection sqlVKT = new SqlConnection(ConVKT);
			await sqlVKT.OpenAsync(token);
			await using var cmdVKT = sqlVKT.CreateCommand();
			cmdVKT.CommandText = @"SELECT rcdBest FROM TNO_SkladList where codeBest=@_sklad";
			cmdVKT.Parameters.Clear();
			cmdVKT.Parameters.AddWithValue("@_sklad", skladBest);

			int rcdSkladBest = 0;
			try
			{
				await using var readerVKT = await cmdVKT.ExecuteReaderAsync(token);
				if (!readerVKT.HasRows) return rcdSkladBest;
				await readerVKT.ReadAsync(token);
				rcdSkladBest = readerVKT.GetInt32(0);
				return rcdSkladBest;
			}
			catch (Exception ex)
			{
				return -1;
			}
		}


		/// <summary>
		/// Получаем норму месяца часов работы на сотрудника, по его графику
		/// </summary>
		/// <param name="numgraf">Номер графика</param>
		/// <returns>Часы</returns>
		public async Task<double> GetHoursPlanMonhtAsync(string numgraf, DateTime dateTime,
					CancellationToken token)
		{
			try
			{
				if (string.IsNullOrEmpty(numgraf)) return 0;

				string sqlExpression = "Select Sum(gr_Hrs) From[rvp002].[dbo].[U_f_KPUGRAF] (@idGraf, @inDt) group by gr_Cd";
				await using SqlConnection sql = new(ConBest);
				await sql.OpenAsync(token);
				var cmd = new SqlCommand(sqlExpression, sql);
				cmd.CommandType = System.Data.CommandType.Text;

				cmd.Parameters.Clear();

				SqlParameter parameter = new SqlParameter
				{
					ParameterName = "@idGraf",
					Value = numgraf
				};
				cmd.Parameters.Add(parameter);

				SqlParameter parameter1 = new SqlParameter
				{
					ParameterName = "@inDt",
					Value = dateTime.ToString("d")
				};
				cmd.Parameters.Add(parameter1);

				await using var reader = await cmd.ExecuteReaderAsync(token);

				if (!reader.HasRows) return 0;
				await reader.ReadAsync(token);
				return (double)reader.GetDecimal(0);
			}
			catch (Exception ex)
			{
				return 0;
			}
		}

		/// <summary>
		/// Записываем данные во временную таблицу ИС-ПРО
		/// </summary>
		/// <param name="tno">Сформированные данные ТНО</param>
		/// <returns>Флаг о выполнении: true при успехе, false при неудаче</returns>
		public async Task UpdateU_PR_TNOAsync(TnoDataDto tno,
					CancellationToken token)
		{
			await using SqlConnection sql = new(ConBest);
			await sql.OpenAsync(token);
			await using var cmd = sql.CreateCommand();
			cmd.CommandText = @"INSERT INTO dbo.U_PR_TNO(U_TNO_SKLPF,U_TNO_SKLPRD,U_TNO_DAT,U_TNO_ART,U_TNO_KRT,U_TNO_MOD,U_TNO_MOV,U_TNO_QT,U_TNO_CMT2) 
			VALUES(@_rcdSkOut, @_rcdSkIn, @_dateTNO, @_art, @_rcdKrt, @_TNO_MOD, @_TNO_MOV, @_kol, @_desk)";
			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@_rcdSkOut", tno.RcdSkladOut);
			cmd.Parameters.AddWithValue("@_rcdSkIn", tno.RcdSkladIn);
			cmd.Parameters.AddWithValue("@_dateTNO", tno.Time);
			cmd.Parameters.AddWithValue("@_art", tno.Art);
			cmd.Parameters.AddWithValue("@_rcdKrt", 0);
			cmd.Parameters.AddWithValue("@_TNO_MOD", "340");
			cmd.Parameters.AddWithValue("@_TNO_MOV", 1);
			cmd.Parameters.AddWithValue("@_kol", tno.Count);
			cmd.Parameters.AddWithValue("@_desk", tno.Description ?? string.Empty);

			//Начало Транзакции. 
			SqlTransaction? transaction = null;
			try
			{
				//Инициализируем новый объект SqlTransaction и связываем его с экземпляром SqlConnection
				transaction = sql.BeginTransaction();

				//Присваиваем транзакцию экземпляру SqlCommand, чтобы обеспечить выполнение команды в контексте транзакции
				cmd.Transaction = transaction;
				await cmd.ExecuteNonQueryAsync(token);

				//Фиксация транзакции, если нет ошибок в cmd.ExecuteNonQuery();
				await transaction.CommitAsync(token);
			}
			catch (Exception ex)
			{
				//Откат транзакции, в случае исключения
				transaction?.RollbackAsync(token);
			}
			finally
			{
				//Утилизация объекта Transaction
				transaction?.DisposeAsync();
			}
		}

		/// <summary>
		/// Запуск хранимой процедуры для формирования ТНО в ИС-ПРО
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ExecuteMakeTnoAsync(CancellationToken token)
		{
			await using SqlConnection sqlConn = new(ConBest);
			await sqlConn.OpenAsync(token);
			await using var cmd = sqlConn.CreateCommand();
			cmd.CommandText = "EXECUTE [rvp002].[dbo].[U_PR_MakeTno]";
			try
			{
				await cmd.ExecuteNonQueryAsync(token);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Получаем список участков с их номерами, из всех доступных сотрудников для табеля
		/// </summary>
		/// <param name="dateTime">Дата, обозначающая в запросе функции временной период (месяц, год)</param>
		public async Task<List<DepartmentProduction>> GetDepartmentProductionsAsync(DateTime dateTime, CancellationToken token)
		{
			string sqlExpression = "Select Distinct tn_PdCd, tn_PdNm From dbo.U_f_KPUTN(@inPdr, @inDt)";

			await using SqlConnection sql = new(ConBest);
			await sql.OpenAsync(token);
			var cmd = new SqlCommand(sqlExpression, sql);
			cmd.CommandType = System.Data.CommandType.Text;

			SqlParameter parameter = new SqlParameter
			{
				ParameterName = "@inPdr",
				Value = string.Empty
			};
			cmd.Parameters.Add(parameter);

			SqlParameter parameter1 = new SqlParameter
			{
				ParameterName = "@inDt",
				Value = dateTime.ToString("d")
			};
			cmd.Parameters.Add(parameter1);

			List<DepartmentProduction> departments = new List<DepartmentProduction>();
			try
			{
				await using var reader = await cmd.ExecuteReaderAsync(token);
				if (reader.HasRows)
				{
					while (await reader.ReadAsync(token))
					{
						departments.Add(new DepartmentProduction()
						{
							DepartmentID = reader.GetString(0).Trim(),
							NameDepartment = reader.GetString(1).Trim(),
						});
					}
				}
				return departments;
			}
			catch (Exception ex)
			{
				return new List<DepartmentProduction>();
				throw;
			}
		}

		/// <summary>
		/// Вызов функции Sql Server в C#
		/// </summary>
		/// <param name="codeRegion">Номер участка строкой, пример: 051 или 03. Если передать <see cref="string.Empty"/> то получим все участки</param>
		/// <param name="dateTime">Дата для запроса. Связан на показ уволенных сотрудников в заданный период</param>
		/// <returns></returns>
		public async Task<List<Employee>> GetEmployeesFromBestAsync(
			string codeRegion,
			DateTime dateTime, CancellationToken token)
		{
			string sqlExpression = "Select * From dbo.U_f_KPUTN(@inPdr, @inDt)";

			await using SqlConnection sql = new(ConBest);
			await sql.OpenAsync(token);
			var cmd = new SqlCommand(sqlExpression, sql);
			cmd.CommandType = System.Data.CommandType.Text;

			SqlParameter parameter = new SqlParameter
			{
				ParameterName = "@inPdr",
				Value = codeRegion
			};
			cmd.Parameters.Add(parameter);

			SqlParameter parameter1 = new SqlParameter
			{
				ParameterName = "@inDt",
				Value = dateTime.ToString("d")
			};
			cmd.Parameters.Add(parameter1);

			List<Employee> employees = new List<Employee>();
			try
			{
				await using var reader = await cmd.ExecuteReaderAsync(token);
				if (reader.HasRows)
				{
					while (await reader.ReadAsync(token))
					{
						employees.Add(new Employee()
						{
							EmployeeID = reader.GetInt64(1),
							DepartmentID = reader.GetString(2).Trim(),
							FullName = reader.GetString(4).Trim(),
							DateEmployment = reader.GetDateTime(5),
							DateDismissal = reader.GetDateTime(6),
							NumGraf = reader.GetString(8).Trim(),
						});
					}
				}
				return employees;
			}
			catch (Exception ex)
			{
				return new List<Employee>();
				throw;
			}
		}

		/// <summary>
		/// Получаем график работы из ИС-ПРО, построенный для определенной смены 
		/// </summary>
		/// <param name="numGraf">Список Номеров графика</param>
		/// <param name="dateTime">Дата периода(например, сентябрь 2024 года)</param>
		/// <returns>Данные графика</returns>
		public async Task<List<WorkingScheduleDto>> GetGrafAsync(
			List<string> numGraf,
			DateTime dateTime, CancellationToken token)
		{
			string sqlExpression = "Select * From dbo.U_f_KPUGRAF(@idGraf, @inDt)";

			await using SqlConnection sql = new(ConBest);
			await sql.OpenAsync(token);
			var cmd = new SqlCommand(sqlExpression, sql);
			cmd.CommandType = System.Data.CommandType.Text;

			List<WorkingScheduleDto> workingSchedules = new();

			try
			{
				foreach (var num in numGraf)
				{
					cmd.Parameters.Clear();

					SqlParameter parameter = new SqlParameter
					{
						ParameterName = "@idGraf",
						Value = num
					};
					cmd.Parameters.Add(parameter);

					SqlParameter parameter1 = new SqlParameter
					{
						ParameterName = "@inDt",
						Value = dateTime.ToString("d")
					};
					cmd.Parameters.Add(parameter1);

					await using var reader = await cmd.ExecuteReaderAsync(token);
					if (reader.HasRows)
					{
						while (await reader.ReadAsync(token))
						{
							workingSchedules.Add(new WorkingScheduleDto
							{
								NumGraf = reader.GetString(1).Trim(),
								PeriodDate = reader.GetDateTime(2),
								DateWithShift = reader.GetDateTime(3),
								DayInDateWithShift = reader.GetInt32(4),
								CountHoursWithShift = (double)reader.GetDecimal(5),
								TypShift = reader.GetByte(6),
								NightHoursWithShift = (double)reader.GetDecimal(7),
							});
						}
					}
				}
				return workingSchedules;
			}
			catch (Exception ex)
			{
				return new List<WorkingScheduleDto>();
				throw;
			}
		}
	}
}

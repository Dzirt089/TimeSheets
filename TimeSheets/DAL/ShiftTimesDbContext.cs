using Microsoft.EntityFrameworkCore;

using TimeSheets.Models;

namespace TimeSheets.DAL
{
	public class ShiftTimesDbContext : DbContext
	{
		public ShiftTimesDbContext(
			DbContextOptions<ShiftTimesDbContext> options)
			: base(options)
		{
		}

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	base.OnConfiguring(optionsBuilder);

		//	// Подавляем предупреждение о незавершенных изменениях модели
		//	optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
		//}

		public DbSet<Employee>? Employees { get; set; }
		public DbSet<ShiftData>? ShiftsData { get; set; }
		public DbSet<ErrorLog>? ErrorLogs { get; set; }
		public DbSet<DepartmentProduction>? DepartmentProductions { get; set; }
		public DbSet<Employee>? ShiftDatas { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<ShiftData>(entity =>
			{
				entity.HasKey(k => new { k.EmployeeID, k.WorkDate });   //Устанавливаем составной первичный ключ
				entity.Property(e => e.EmployeeID)                      //указывает Entity Framework Core, что значение для данного свойства
				.ValueGeneratedNever();                                 //не должно генерироваться автоматически базой данных.
																		//Это полезно, когда значение свойства устанавливается приложением, а не базой данных.

				entity.HasIndex(i => new { i.EmployeeID, i.WorkDate }).IsUnique(); //Устанавливаем индексы
				entity.HasOne(s => s.Employee)                          //устанавливаем связь "один-
						.WithMany(e => e.Shifts)                        //ко-многим"
						.HasForeignKey(i => i.EmployeeID)               //указываем ВК (HasForeignKey)
						.OnDelete(DeleteBehavior.Restrict)             // ограничивает удаление сущности, имеющей зависимые сущности.
						.HasConstraintName("FK_Employees");
			});

			modelBuilder.Entity<Employee>(entity =>
			{
				entity.HasIndex(i => new { i.EmployeeID, i.DepartmentID, i.NumGraf }).IsUnique();   //Устанавливаем индексы
				entity.HasKey(e => e.EmployeeID);                                       //Устанавливаем PK
				entity.Property(x => x.EmployeeID).ValueGeneratedNever();               //ID не должен генерироваться автоматически базой данных.
																						//entity.Property(x => x.DepartmentID).HasDefaultValue("");				//На тот случай, если забудем прописать ПК для DepartmentProduction
				entity.Property(e => e.Photo).HasColumnType("varbinary(max)");

				entity.HasOne(d => d.DepartmentProduction)                              //Устанавливаем связь через навигационную сущность участков, связь "один - ко -
						.WithMany(r => r.EmployeesList)                                 // - многим, указывая на коллекцию сущности сотрудников
						.HasForeignKey(d => d.DepartmentID)                             // Определяем внешний ключ
						.OnDelete(DeleteBehavior.Restrict)                              // Запрещаем каскадное удаление
						.HasConstraintName("FK_Departments");
			});

			modelBuilder.Entity<DepartmentProduction>(entity =>
			{
				entity.HasKey(h => h.DepartmentID);                             //Устанавливаем PK
				entity.Property(x => x.DepartmentID).ValueGeneratedNever();     //ID не должен генерироваться автоматически базой данных.
				entity.HasIndex(i => i.DepartmentID).IsUnique();                //Устанавливаем индексы	
																				//
				entity.HasData(new DepartmentProduction { DepartmentID = "01", NameDepartment = "Группа разработки" }
				, new DepartmentProduction { DepartmentID = "02", NameDepartment = "Группа аналитики" }
				, new DepartmentProduction { DepartmentID = "03", NameDepartment = "Группа тестирования" }
				, new DepartmentProduction { DepartmentID = "04", NameDepartment = "Группа исследования" });                                                               //			
			});
		}
	}
}

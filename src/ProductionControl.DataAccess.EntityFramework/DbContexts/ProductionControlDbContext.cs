using Microsoft.EntityFrameworkCore;

using ProductionControl.DataAccess.Classes.ApiModels.Model;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

namespace ProductionControl.DataAccess.EntityFramework.DbContexts
{
	public class ProductionControlDbContext : DbContext
	{
		#region ContextWPF
		public ProductionControlDbContext(
			DbContextOptions<ProductionControlDbContext> options)
			: base(options)
		{
		}
		public DbSet<DataSizForMonth> DataSizForMonth { get; set; }
		public DbSet<IssueSiz> IssueSizs { get; set; }
		public DbSet<Employee>? Employees { get; set; }
		public DbSet<ShiftData>? ShiftsData { get; set; }
		public DbSet<ErrorLog>? ErrorLogs { get; set; }
		public DbSet<DepartmentProduction>? DepartmentProductions { get; set; }
		public DbSet<EmployeeAccessRight>? EmployeeAccessRights { get; set; }
		public DbSet<Siz>? Sizs { get; set; }
		public DbSet<UsageNorm>? UsageNorms { get; set; }
		public DbSet<SizUsageRate>? SizUsageRates { get; set; }
		public DbSet<EmployeeExOrg>? EmployeeExOrgs { get; set; }
		public DbSet<ShiftDataExOrg>? ShiftDataExOrgs { get; set; }
		public DbSet<EmployeeExOrgAddInRegion>? EmployeeExOrgAddInRegions { get; set; }
		public DbSet<EmployeePhoto>? EmployeePhotos { get; set; }
		public DbSet<OrderNumberOnDate> OrderNumberOnDate { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<IssueSiz>(entity =>
			{
				entity.HasKey(e => e.id);
				//.HasName("PK__IssueSiz__3213E83FB8BC5DBB");
			});

			modelBuilder.Entity<DataSizForMonth>(entity =>
			{
				entity.HasKey(e => e.id);
				//.HasName("PK__DataSizF__3213E83F09214833");

				//entity.ToTable("DataSizForMonth");

				entity.HasOne(d => d.Siz).WithMany(p => p.DataSizForMonths)
					.HasForeignKey(d => d.SizID)
					.OnDelete(DeleteBehavior.ClientSetNull);
				//.HasConstraintName("FK__DataSizFo__SizID__0C85DE4D");
			});

			modelBuilder.Entity<OrderNumberOnDate>(entity =>
			{
				entity.HasKey(e => e.Id);
				//entity.ToTable("OrderNumberOnDate");
			});

			modelBuilder.Entity<EmployeeExOrgAddInRegion>(entity =>
			{
				entity.HasKey(k => new { k.EmployeeExOrgID, k.DepartmentID });

				entity.HasOne(h => h.EmployeeExOrg)
						.WithMany(m => m.EmployeeExOrgAddInRegions)
						.HasForeignKey(h => h.EmployeeExOrgID)
						.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<EmployeeExOrg>(entity =>
			{
				entity.HasKey(x => x.EmployeeExOrgID);
				entity.HasOne(x => x.EmployeePhotos)
						.WithOne(z => z.EmployeeExOrg)
						.HasForeignKey<EmployeePhoto>(c => c.EmployeeExOrgID);
			});

			modelBuilder.Entity<EmployeePhoto>(entity =>
			{
				entity.HasKey(x => x.EmployeeExOrgID);
				entity.Property(e => e.Photo).HasColumnType("varbinary(max)");
			});

			modelBuilder.Entity<ShiftDataExOrg>(entity =>
			{
				entity.HasKey(x => new { x.EmployeeExOrgID, x.WorkDate, x.DepartmentID });
				entity.HasIndex(i => new { i.EmployeeExOrgID, i.WorkDate, i.DepartmentID }).IsUnique();

				entity.HasOne(x => x.EmployeeExOrg)
						.WithMany(w => w.ShiftDataExOrgs)
						.HasForeignKey(s => s.EmployeeExOrgID)
						.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<ShiftData>(entity =>
			{//Устанавливаем составной первичный ключ
				entity.HasKey(k => new { k.EmployeeID, k.WorkDate });
				//указывает Entity Framework Core, что значение для данного свойства
				entity.Property(e => e.EmployeeID)
				//не должно генерироваться автоматически базой данных.
				//Это полезно, когда значение свойства устанавливается приложением, а не базой данных.
				.ValueGeneratedNever();

				//Устанавливаем индексы
				entity.HasIndex(i => new { i.EmployeeID, i.WorkDate }).IsUnique();
				//устанавливаем связь "один-
				entity.HasOne(s => s.Employee)
						//ко-многим"
						.WithMany(e => e.Shifts)
						//указываем ВК (HasForeignKey)
						.HasForeignKey(i => i.EmployeeID)
						// ограничивает удаление сущности, имеющей зависимые сущности.
						.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("FK_Employees");
			});

			modelBuilder.Entity<Employee>(entity =>
			{
				//Устанавливаем индексы
				entity.HasIndex(i => new { i.EmployeeID, i.DepartmentID, i.NumGraf }).IsUnique();
				entity.HasIndex(x => x.UsageNormID).IsUnique(false);
				//Устанавливаем PK
				entity.HasKey(e => e.EmployeeID);
				//ID не должен генерироваться автоматически базой данных.
				entity.Property(x => x.EmployeeID).ValueGeneratedNever();
				//Устанавливаем связь через навигационную сущность участков, связь "один - ко -
				entity.HasOne(d => d.DepartmentProduction)
						// - многим, указывая на коллекцию сущности сотрудников
						.WithMany(r => r.EmployeesList)
						// Определяем внешний ключ
						.HasForeignKey(d => d.DepartmentID)
						// Запрещаем каскадное удаление
						.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("FK_Departments");
			});

			modelBuilder.Entity<DepartmentProduction>(entity =>
			{
				//Устанавливаем PK
				entity.HasKey(h => h.DepartmentID);
				//ID не должен генерироваться автоматически базой данных.
				entity.Property(x => x.DepartmentID).ValueGeneratedNever();
				//Устанавливаем индексы		
				entity.HasIndex(i => i.DepartmentID).IsUnique();
			});


			modelBuilder.Entity<EmployeeAccessRight>(entity =>
			{
				entity.HasIndex(d => new { d.EmployeeAccessRightId, d.DepartmentID }).IsUnique();
				entity.HasKey(h => h.EmployeeAccessRightId);

				//Связь один-к-одному
				entity.HasOne(x => x.DepartmentProduction)
						.WithOne(r => r.EmployeeAccessRight)
						.HasForeignKey<EmployeeAccessRight>(x => x.DepartmentID)
						.OnDelete(DeleteBehavior.Restrict);
			});

			//Установка связей между Siz and SizUsageRate
			modelBuilder.Entity<Siz>(entity =>
			{
				entity.HasMany(x => x.SizUsageRates)
						.WithOne(w => w.Siz)
						.HasForeignKey(f => f.SizID);
			});

			//Установка связей между UsageNorm and SizUsageRate
			modelBuilder.Entity<UsageNorm>(entity =>
			{
				entity.HasMany(x => x.SizUsageRates)
						.WithOne(w => w.UsageNorm)
						.HasForeignKey(f => f.UsageNormID);
			});
		}
		#endregion

		#region API
		//public ProductionControlDbContext(DbContextOptions<ProductionControlDbContext> options)
		//  : base(options)
		//{
		//}

		//public virtual DbSet<DataSizForMonth> DataSizForMonths { get; set; }

		//public virtual DbSet<DepartmentProduction> DepartmentProductions { get; set; }

		//public virtual DbSet<Employee> Employees { get; set; }

		//public virtual DbSet<EmployeeAccessRight> EmployeeAccessRights { get; set; }

		//public virtual DbSet<EmployeeExOrg> EmployeeExOrgs { get; set; }

		//public virtual DbSet<EmployeeExOrgAddInRegion> EmployeeExOrgAddInRegions { get; set; }

		//public virtual DbSet<EmployeePhoto> EmployeePhotos { get; set; }

		//public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

		//public virtual DbSet<IssueSiz> IssueSizs { get; set; }

		//public virtual DbSet<OrderNumberOnDate> OrderNumberOnDates { get; set; }

		//public virtual DbSet<ShiftDataExOrg> ShiftDataExOrgs { get; set; }

		//public virtual DbSet<ShiftsDatum> ShiftsData { get; set; }

		//public virtual DbSet<Siz> Sizs { get; set; }

		//public virtual DbSet<SizUsageRate> SizUsageRates { get; set; }

		//public virtual DbSet<UsageNorm> UsageNorms { get; set; }

		//protected override void OnModelCreating(ModelBuilder modelBuilder)
		//{
		//	modelBuilder.Entity<DataSizForMonth>(entity =>
		//	{
		//		entity.HasKey(e => e.id).HasName("PK__DataSizF__3213E83F09214833");

		//		entity.ToTable("DataSizForMonth");

		//		entity.HasOne(d => d.Siz).WithMany(p => p.DataSizForMonths)
		//			.HasForeignKey(d => d.SizID)
		//			.OnDelete(DeleteBehavior.ClientSetNull)
		//			.HasConstraintName("FK__DataSizFo__SizID__0C85DE4D");
		//	});

		//	modelBuilder.Entity<DepartmentProduction>(entity =>
		//	{
		//		entity.HasKey(e => e.DepartmentID);

		//		entity.HasIndex(e => e.DepartmentID, "IX_DepartmentProductions_DepartmentID").IsUnique();
		//	});

		//	modelBuilder.Entity<Employee>(entity =>
		//	{
		//		entity.HasIndex(e => e.DepartmentID, "IX_Employees_DepartmentID");

		//		entity.HasIndex(e => new { e.EmployeeID, e.DepartmentID, e.NumGraf }, "IX_Employees_EmployeeID_DepartmentID_NumGraf")
		//			.IsUnique()
		//			.HasFilter("([NumGraf] IS NOT NULL)");

		//		entity.HasIndex(e => e.UsageNormID, "IX_Employees_UsageNormID");

		//		entity.Property(e => e.EmployeeID).ValueGeneratedNever();
		//		entity.Property(e => e.DepartmentID).HasDefaultValue("");

		//		entity.HasOne(d => d.Department).WithMany(p => p.Employees)
		//			.HasForeignKey(d => d.DepartmentID)
		//			.OnDelete(DeleteBehavior.ClientSetNull)
		//			.HasConstraintName("FK_Departments");

		//		entity.HasOne(d => d.UsageNorm).WithMany(p => p.Employees).HasForeignKey(d => d.UsageNormID);
		//	});

		//	modelBuilder.Entity<EmployeeAccessRight>(entity =>
		//	{
		//		entity.HasIndex(e => e.DepartmentID, "IX_EmployeeAccessRights_DepartmentID");

		//		entity.HasIndex(e => new { e.EmployeeAccessRightId, e.DepartmentID }, "IX_EmployeeAccessRights_EmployeeAccessRightId_DepartmentID").IsUnique();

		//		entity.Property(e => e.DepartmentID).HasDefaultValue("");

		//		entity.HasOne(d => d.Department).WithMany(p => p.EmployeeAccessRights)
		//			.HasForeignKey(d => d.DepartmentID)
		//			.OnDelete(DeleteBehavior.ClientSetNull);
		//	});

		//	modelBuilder.Entity<EmployeeExOrg>(entity =>
		//	{
		//		entity.HasIndex(e => e.DepartmentProductionDepartmentID, "IX_EmployeeExOrgs_DepartmentProductionDepartmentID");

		//		entity.HasOne(d => d.DepartmentProductionDepartment).WithMany(p => p.EmployeeExOrgs).HasForeignKey(d => d.DepartmentProductionDepartmentID);
		//	});

		//	modelBuilder.Entity<EmployeeExOrgAddInRegion>(entity =>
		//	{
		//		entity.HasKey(e => new { e.EmployeeExOrgID, e.DepartmentID });

		//		entity.HasOne(d => d.EmployeeExOrg).WithMany(p => p.EmployeeExOrgAddInRegions)
		//			.HasForeignKey(d => d.EmployeeExOrgID)
		//			.OnDelete(DeleteBehavior.ClientSetNull);
		//	});

		//	modelBuilder.Entity<EmployeePhoto>(entity =>
		//	{
		//		entity.HasKey(e => e.EmployeeExOrgID);

		//		entity.Property(e => e.EmployeeExOrgID).ValueGeneratedNever();

		//		entity.HasOne(d => d.EmployeeExOrg).WithOne(p => p.EmployeePhoto).HasForeignKey<EmployeePhoto>(d => d.EmployeeExOrgID);
		//	});

		//	modelBuilder.Entity<IssueSiz>(entity =>
		//	{
		//		entity.HasKey(e => e.id).HasName("PK__IssueSiz__3213E83FB8BC5DBB");
		//	});

		//	modelBuilder.Entity<OrderNumberOnDate>(entity =>
		//	{
		//		entity.HasKey(e => e.Id).HasName("PK__OrderNum__3214EC0755119831");

		//		entity.ToTable("OrderNumberOnDate");
		//	});

		//	modelBuilder.Entity<ShiftDataExOrg>(entity =>
		//	{
		//		entity.HasKey(e => new { e.EmployeeExOrgID, e.WorkDate, e.DepartmentID });

		//		entity.HasIndex(e => new { e.EmployeeExOrgID, e.WorkDate, e.DepartmentID }, "IX_ShiftDataExOrgs_EmployeeExOrgID_WorkDate_DepartmentID").IsUnique();

		//		entity.HasOne(d => d.EmployeeExOrg).WithMany(p => p.ShiftDataExOrgs)
		//			.HasForeignKey(d => d.EmployeeExOrgID)
		//			.OnDelete(DeleteBehavior.ClientSetNull);
		//	});

		//	modelBuilder.Entity<ShiftsDatum>(entity =>
		//	{
		//		entity.HasKey(e => new { e.EmployeeID, e.WorkDate });

		//		entity.HasIndex(e => new { e.EmployeeID, e.WorkDate }, "IX_ShiftsData_EmployeeID_WorkDate").IsUnique();

		//		entity.HasOne(d => d.Employee).WithMany(p => p.ShiftsData)
		//			.HasForeignKey(d => d.EmployeeID)
		//			.OnDelete(DeleteBehavior.ClientSetNull)
		//			.HasConstraintName("FK_Employees");
		//	});

		//	modelBuilder.Entity<SizUsageRate>(entity =>
		//	{
		//		entity.HasIndex(e => e.SizID, "IX_SizUsageRates_SizID");

		//		entity.HasIndex(e => e.UsageNormID, "IX_SizUsageRates_UsageNormID");

		//		entity.HasOne(d => d.Siz).WithMany(p => p.SizUsageRates).HasForeignKey(d => d.SizID);

		//		entity.HasOne(d => d.UsageNorm).WithMany(p => p.SizUsageRates).HasForeignKey(d => d.UsageNormID);
		//	});

		//	OnModelCreatingPartial(modelBuilder);
		//}

		//partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
		#endregion
	}
}

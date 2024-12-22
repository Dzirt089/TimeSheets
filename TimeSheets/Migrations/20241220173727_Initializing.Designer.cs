﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeSheets.DAL;

#nullable disable

namespace TimeSheets.Migrations
{
    [DbContext(typeof(ShiftTimesDbContext))]
    [Migration("20241220173727_Initializing")]
    partial class Initializing
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ProductionControl.Models.DepartmentProduction", b =>
                {
                    b.Property<string>("DepartmentID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessRight")
                        .HasColumnType("int");

                    b.Property<string>("NameDepartment")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DepartmentID");

                    b.HasIndex("DepartmentID")
                        .IsUnique();

                    b.ToTable("DepartmentProductions");
                });

            modelBuilder.Entity("ProductionControl.Models.Employee", b =>
                {
                    b.Property<long>("EmployeeID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateDismissal")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateEmployment")
                        .HasColumnType("datetime2");

                    b.Property<string>("DepartmentID")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Descriptions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDismissal")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLunch")
                        .HasColumnType("bit");

                    b.Property<string>("NumGraf")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("NumberPass")
                        .HasColumnType("int");

                    b.Property<byte[]>("Photo")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EmployeeID");

                    b.HasIndex("DepartmentID");

                    b.HasIndex("EmployeeID", "DepartmentID", "NumGraf")
                        .IsUnique()
                        .HasFilter("[NumGraf] IS NOT NULL");

                    b.ToTable("Employee");
                });

            modelBuilder.Entity("ProductionControl.Models.ErrorLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Application")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InnerException")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Machine")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StackTrace")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("User")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ErrorLogs");
                });

            modelBuilder.Entity("ProductionControl.Models.ShiftData", b =>
                {
                    b.Property<long>("EmployeeID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("WorkDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Hours")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsHaveLunch")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPreHoliday")
                        .HasColumnType("bit");

                    b.Property<string>("Overday")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Shift")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EmployeeID", "WorkDate");

                    b.HasIndex("EmployeeID", "WorkDate")
                        .IsUnique();

                    b.ToTable("ShiftsData");
                });

            modelBuilder.Entity("ProductionControl.Models.Employee", b =>
                {
                    b.HasOne("ProductionControl.Models.DepartmentProduction", "DepartmentProduction")
                        .WithMany("EmployeesList")
                        .HasForeignKey("DepartmentID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Departments");

                    b.Navigation("DepartmentProduction");
                });

            modelBuilder.Entity("ProductionControl.Models.ShiftData", b =>
                {
                    b.HasOne("ProductionControl.Models.Employee", "Employee")
                        .WithMany("Shifts")
                        .HasForeignKey("EmployeeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Employees");

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("ProductionControl.Models.DepartmentProduction", b =>
                {
                    b.Navigation("EmployeesList");
                });

            modelBuilder.Entity("ProductionControl.Models.Employee", b =>
                {
                    b.Navigation("Shifts");
                });
#pragma warning restore 612, 618
        }
    }
}
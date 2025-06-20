﻿namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	public class MonthlySummaryDto
	{
		public long EmployeeID { get; set; }
		public string? ShortName { get; set; }
		public int DepartmentID { get; set; }
		public string? NameDepartment { get; set; }
		public int Year { get; set; }
		public string? MonthName { get; set; }
		public string? Day1 { get; set; }
		public string? Day2 { get; set; }
		public string? Day3 { get; set; }
		public string? Day4 { get; set; }
		public string? Day5 { get; set; }
		public string? Day6 { get; set; }
		public string? Day7 { get; set; }
		public string? Day8 { get; set; }
		public string? Day9 { get; set; }
		public string? Day10 { get; set; }
		public string? Day11 { get; set; }
		public string? Day12 { get; set; }
		public string? Day13 { get; set; }
		public string? Day14 { get; set; }
		public string? Day15 { get; set; }
		public string? Day16 { get; set; }
		public string? Day17 { get; set; }
		public string? Day18 { get; set; }
		public string? Day19 { get; set; }
		public string? Day20 { get; set; }
		public string? Day21 { get; set; }
		public string? Day22 { get; set; }
		public string? Day23 { get; set; }
		public string? Day24 { get; set; }
		public string? Day25 { get; set; }
		public string? Day26 { get; set; }
		public string? Day27 { get; set; }
		public string? Day28 { get; set; }
		public string? Day29 { get; set; }
		public string? Day30 { get; set; }
		public string? Day31 { get; set; }

		/// <summary>
		/// Общее кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		public int TotalWorksDays { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов, без учёта переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithoutOverday { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов c учётом переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithOverday { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов в ночную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalNightHours { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов в дневную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalDaysHours { get; set; }

		/// <summary>
		/// Общее кол-во часов переработок\недоработок
		/// </summary>
		public double TotalOverdayHours { get; set; }
		public int CountPreholiday { get; set; }
	}
}

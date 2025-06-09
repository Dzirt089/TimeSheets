Используем расширение EF Core Power Tools, для быстрого обновления\реконструкции данных.

 Нажимаем ПКМ на проекте, выбираем EF Core Power Tools.
1. Если надо добавить данные таблицы, то нажимаем на реконструкцию.
2. Выбираем БД ([TimeSheet] для основной БД) из выпадающего списка.
3. Нажимаем ОК
4. Смотрим, все ли нужные нам таблицы помечены галлочкой. Если нет, то ставим\убираем.
   По-умолчанию галлочки стоят которые выставили с прошлого раза .
5. Потом нажимаем ок на следующем окне и ждем окончания обратных миграций 

--- Если нам надо подтянуть изменения в таблицах, которые УЖЕ есть в проекте - то:
	Нажимаем ПКМ на проекте, выбираем EF Core Power Tools.
1. Если надо обновить данные таблиц, то нажимаем на обновление. 


Старый запуск, ручками  приписывать всё. В крайних случаях. Описание ниже.


//Первый запуск обратного проектирования для EF Core, когда нет данных по таблицам и самому DbContext-у

dotnet ef dbcontext scaffold "Data Source = SERVER-TO1; Initial Catalog = TimeSheet; Persist Security Info = True; 
User ID = Apps; Password = 793148625; Application Name = ProductionControl; TrustServerCertificate=True" 
Microsoft.EntityFrameworkCore.SqlServer   
--project Api.ProductionControl  
--context-dir Data 
--output-dir Models  

//Повторный запуск, обновляем\перезаписываем новые данные из БД. Помним, что строка подключения находится в DI

 dotnet ef dbcontext scaffold 
 "Data Source = SERVER-TO1; Initial Catalog = TimeSheet; Persist Security Info = True; User ID = Apps; Password = 793148625; 
 Application Name = ProductionControl; TrustServerCertificate=True"    
 Microsoft.EntityFrameworkCore.SqlServer 
 --project Api.ProductionControl 
 --context-dir Data 
 --output-dir Models  
 --no-onconfiguring 
 --force


 ВАЖНО!
 После обновления, необходимо пройтись по сформированным\обновленным моделям, и поменять значения 
С:	
	ICollection<T> {get;set;} = new List<T> 
На:
1) IEnumerable<T> {get;set;}

2) Добавить конструктор класса, на примере Employee:
Было : 
	ICollection<ShiftsDatum> {get;set;} = new List<ShiftsDatum>();

Должно стать :
	public Employee()
	{
		ShiftsData = new HashSet<ShiftsDatum>();
	}
	public virtual IEnumerable<ShiftsDatum> ShiftsData { get; set; }
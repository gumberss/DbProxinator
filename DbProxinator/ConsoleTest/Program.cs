using Application.Parsers;
using Domain.Entities;
using Domain.Interfaces.Parsers;
using Infraestructure.Manager;
using System;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = @"data source=localhost; initial catalog=ProxinatorTest;persist security info=True; Integrated Security=SSPI";

            var query = @"SELECT sch.name as TableSchema,
						obj.object_id as tableId,
	                    obj.name as tableName,
						cols.column_id as ColumnId,
		                cols.name as ColumnName, 
		                cols.system_type_id SystemTypeId, 
		                sysType.name TypeName, 
		                cols.max_length MaxLength, 
		                cols.precision Precision, 
		                is_nullable IsNullable, 
		                cols.is_identity IsIdentity
                from sys.objects as obj
                inner join sys.columns as cols  on obj.object_id = cols.object_id
                inner join sys.systypes sysType on sysType.xtype = cols.system_type_id
                inner join sys.schemas sch on sch.schema_id = obj.schema_id
                where obj.type='U'
                ";
            //var sqlManager = new SqlManager(connString);

            //var list = sqlManager.ExecuteReader<TableSchemaEntity>(query);

            //ITableSchemaParser tableParser = new TableSchemaParser();

            //var businessData = tableParser.Parse(list);


            var connString2 = @"Server=localhost;Database=Expenses;User Id=sa;Password=HereWeG0Again!;";

            var query2 = @"SELECT  
	e.[Id] 'Expense.Id',
	e.[Title] 'Expense.Title',
	ei.[Id] 'Expense.ExpenseItem.Id',
	ei.[Cost] 'Expense.ExpenseItem.Cost',
	ei.[ExpenseId] 'Expense.ExpenseItem.ExpenseId'
  FROM [Expenses].[expenses].[Expenses] e
  inner join [Expenses].[expenses].[ExpenseItems] ei on e.id = ei.expenseid";

            Nested.AddEntities<Expense>();
            Nested.AddEntities<ExpenseItem>();

            var sql = new SqlManagerNested(connString2);

            var expenses = sql.ExecuteReader<Expense>(query2);

        }
    }
}

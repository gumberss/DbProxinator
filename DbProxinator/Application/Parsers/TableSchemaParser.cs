using Domain.Entities;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Parsers
{
    public class TableSchemaParser
    {
        //public IEnumerable<Table> Parse(IEnumerable<TableSchemaEntity> entities)
        //{
        //    List<Table> tables = new List<Table>();

        //    var groupedTables = entities.GroupBy(x => new { x.TableSchema, x.TableName });

        //    foreach (var current in groupedTables)
        //    {
        //        Table table = new Table(current.Key.TableSchema, current.Key.TableName);

        //        List<Column> columns = new List<Column>();

        //        foreach (var currentColumn in current.ToList())
        //        {
        //            columns.Add(new Column(currentColumn.ColumnName)
        //            {
                        
        //            });
        //        }


        //        table.Fill();
        //    }
        //}
    }
}

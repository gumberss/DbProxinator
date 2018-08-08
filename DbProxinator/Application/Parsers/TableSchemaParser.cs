using Domain.Entities;
using Domain.Interfaces.Parsers;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Parsers
{
    public class TableSchemaParser : ITableSchemaParser
    {
        public IEnumerable<Table> Parse(IEnumerable<TableSchemaEntity> entities)
        {
            var groupedTables = entities.GroupBy(x => new { x.TableId, x.TableSchema, x.TableName });

            foreach (var current in groupedTables)
            {
                Table table = new Table(current.Key.TableId, current.Key.TableSchema, current.Key.TableName);

                List<Column> columns = new List<Column>();

                foreach (var currentColumn in current)
                {
                    columns.Add(new Column(currentColumn.ColumnId, currentColumn.ColumnName)
                    {
                        IsIdentity = currentColumn.IsIdentity,
                        IsNullable = currentColumn.IsNullable,
                        MaxLength = currentColumn.MaxLength,
                        Precision = currentColumn.Precision,
                        SystemTypeId = currentColumn.SystemTypeId,
                        TypeName = currentColumn.TypeName
                    });
                }

                table.Fill(columns);

                yield return table;
            }
        }
    }
}

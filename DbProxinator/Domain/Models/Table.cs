using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Table
    {
        public Table(int id, string schema, string name)
        {
            Id = id;
            Schema = schema;
            Name = name;
        }

        public long Id { get; set; }

        public String Schema { get; set; }

        public String Name { get; set; }

        public IEnumerable<Column> Columns { get; private set; }

        public void Fill(IEnumerable<Column> columns)
        {
            Columns = columns;
        }
    }
}

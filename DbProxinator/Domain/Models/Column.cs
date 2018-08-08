using System;

namespace Domain.Models
{
    public class Column
    {
        public Column(int id, String columnName)
        {
            Id = id;
            ColumnName = columnName;
        }

        public int Id { get; set; }

        public String ColumnName { get; set; }

        public byte SystemTypeId { get; set; }

        public String TypeName { get; set; }

        public short MaxLength { get; set; }

        public byte Precision { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIdentity { get; set; }
    }
}

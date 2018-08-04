using System;

namespace Domain.Models
{
    public class Column
    {
        public Column(String columnName)
        {
            ColumnName = columnName;
        }

        public String ColumnName { get; }

        public byte SystemTypeId { get; }

        public String TypeName { get; }

        public short MaxLength { get; }

        public byte Precision { get; }

        public bool IsNullable { get; }

        public bool IsIdentity { get; }

    }
}

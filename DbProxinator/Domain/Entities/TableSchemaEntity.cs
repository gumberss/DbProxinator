using System;

namespace Domain.Entities
{
    public class TableSchemaEntity
    {
        public int TableId { get; set; }

        public int ColumnId { get; set; }

        public String TableSchema { get; set; }

        public String TableName { get; set; }

        public String ColumnName { get; set; }

        public byte SystemTypeId { get; set; }

        public String TypeName { get; set; }

        public short MaxLength { get; set; }

        public byte Precision { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIdentity { get; set; }
    }
}

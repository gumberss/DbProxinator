using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Expense
    {
        public Guid Id { get; set; }

        public String Title { get; set; }

        public ExpenseItem ExpenseItem { get; set; }
    }

    public class ExpenseItem
    {
        public Guid Id { get; set; }

        public Guid ExpenseId { get; set; }

        public decimal Cost { get; set; }
    }
}

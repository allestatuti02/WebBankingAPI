using System;
using System.Collections.Generic;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class AccountMovement
    {
        public AccountMovement()
        {

        }
        public AccountMovement(DateTime date, int fkBankAccount, double? in2, double? out2, string description)
        {
            Date = date;
            FkBankAccount = fkBankAccount;
            In = in2;
            Out = out2;
            Description = description;
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int FkBankAccount { get; set; }
        public double? In { get; set; }
        public double? Out { get; set; }
        public string Description { get; set; }

        public virtual BankAccount FkBankAccountNavigation { get; set; }
    }
}

using System;
using ProiectMTP.ViewModels;

namespace ProiectMTP.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Transaction : ViewModelBase
    {
        private Guid _id = Guid.NewGuid();
        private decimal _amount;
        private TransactionType _type;
        private string _category = string.Empty;
        private DateTime _date = DateTime.Now;
        private string _description = string.Empty;

        public Guid Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public TransactionType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
    }
}
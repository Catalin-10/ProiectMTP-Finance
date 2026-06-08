using ProiectMTP.ViewModels;

namespace ProiectMTP.Models
{
    public class StockPosition : ViewModelBase
    {
        private string _ticker = string.Empty;
        private string _companyName = string.Empty;
        private decimal _quantity;
        private decimal _averagePurchasePrice;
        private decimal _currentPrice;

        public string Ticker
        {
            get => _ticker;
            set { _ticker = value; OnPropertyChanged(); }
        }

        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(); }
        }

        public decimal Quantity
        {
            get => _quantity;
            set 
            { 
                _quantity = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(ProfitLoss));
            }
        }

        public decimal AveragePurchasePrice
        {
            get => _averagePurchasePrice;
            set 
            { 
                _averagePurchasePrice = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(ProfitLoss));
            }
        }

        public decimal CurrentPrice
        {
            get => _currentPrice;
            set 
            { 
                _currentPrice = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(ProfitLoss));
            }
        }

        public decimal TotalValue => Quantity * CurrentPrice;
        public decimal ProfitLoss => (CurrentPrice - AveragePurchasePrice) * Quantity;
    }
}
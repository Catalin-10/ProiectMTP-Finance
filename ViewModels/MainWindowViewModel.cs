using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ProiectMTP.Models;
using ProiectMTP.Services;
using ProiectMTP.Views;

namespace ProiectMTP.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly StorageService _storageService = new();
        private readonly StockApiService _stockApiService = new();

        private decimal _totalBalance;
        private decimal _portfolioValue;
        private string _newTicker = string.Empty;
        private decimal _newQuantity;
        private decimal _selectedStockPrice;
        private const decimal UsdToRonExchangeRate = 4.65m;
        private string _graphStatusText = "Introduceți un simbol (ex: AAPL) și apăsați Caută Preț Acțiune";

        private string _newTransDescription = string.Empty;
        private decimal _newTransAmount;
        private string _newTransCategory = string.Empty;
        private int _newTransTypeIndex;
        private StockPosition? _selectedStock;

        // Câmpuri private noi pentru gestionarea bugetului din BudgetWindow
        private decimal _incomeInput;
        private decimal _ruleNeeds;
        private decimal _ruleWants;
        private decimal _ruleSavings;
        private decimal _budgetFood;
        private decimal _budgetBills;
        private decimal _budgetInvestments;

        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<StockPosition> Stocks { get; set; } = new();

        public MainWindowViewModel()
        {
            _ = LoadDataAsync();
        }

        public decimal TotalBalance
{
    get => _totalBalance;
    set
    {
        if (_totalBalance != value)
        {
            _totalBalance = value;
            OnPropertyChanged();
        }
    }
}

        public decimal PortfolioValue
        {
            get => _portfolioValue;
            set { _portfolioValue = value; OnPropertyChanged(); }
        }

        public string NewTicker
        {
            get => _newTicker;
            set { _newTicker = value; OnPropertyChanged(); }
        }

        public decimal NewQuantity
        {
            get => _newQuantity;
            set { _newQuantity = value; OnPropertyChanged(); }
        }

        public decimal SelectedStockPrice
        {
            get => _selectedStockPrice;
            set { _selectedStockPrice = value; OnPropertyChanged(); }
        }

        public string GraphStatusText
        {
            get => _graphStatusText;
            set { _graphStatusText = value; OnPropertyChanged(); }
        }

        public string NewTransDescription
        {
            get => _newTransDescription;
            set { _newTransDescription = value; OnPropertyChanged(); }
        }

        public decimal NewTransAmount
        {
            get => _newTransAmount;
            set { _newTransAmount = value; OnPropertyChanged(); }
        }

        public string NewTransCategory
        {
            get => _newTransCategory;
            set { _newTransCategory = value; OnPropertyChanged(); }
        }

        public int NewTransTypeIndex
        {
            get => _newTransTypeIndex;
            set { _newTransTypeIndex = value; OnPropertyChanged(); }
        }

        public StockPosition? SelectedStock
        {
            get => _selectedStock;
            set { _selectedStock = value; OnPropertyChanged(); }
        }

        public decimal IncomeInput
        {
            get => _incomeInput;
            set { _incomeInput = value; OnPropertyChanged(); }
        }

        public decimal RuleNeeds
        {
            get => _ruleNeeds;
            set { _ruleNeeds = value; OnPropertyChanged(); }
        }

        public decimal RuleWants
        {
            get => _ruleWants;
            set { _ruleWants = value; OnPropertyChanged(); }
        }

        public decimal RuleSavings
        {
            get => _ruleSavings;
            set { _ruleSavings = value; OnPropertyChanged(); }
        }

        public decimal BudgetFood
        {
            get => _budgetFood;
            set 
            { 
                _budgetFood = value; 
                OnPropertyChanged(); 
                _ = SaveDataAsync(); 
            }
        }

        public decimal BudgetBills
        {
            get => _budgetBills;
            set 
            { 
                _budgetBills = value; 
                OnPropertyChanged(); 
                _ = SaveDataAsync(); 
            }
        }

        public decimal BudgetInvestments
        {
            get => _budgetInvestments;
            set 
            { 
                _budgetInvestments = value; 
                OnPropertyChanged(); 
                _ = SaveDataAsync(); 
            }
        }

        public void ApplyBudgetRule()
        {
            RuleNeeds = IncomeInput * 0.50m;
            RuleWants = IncomeInput * 0.30m;
            RuleSavings = IncomeInput * 0.20m;
        }

        public async Task AddTransactionAsync()
       {
    if (NewTransAmount <= 0) return;

    var newTx = new Transaction
    {
        Description = NewTransDescription,
        Amount = NewTransAmount,
        Category = NewTransCategory,
        Type = NewTransTypeIndex == 0 ? TransactionType.Income : TransactionType.Expense,
        Date = DateTime.Now
    };

    Transactions.Add(newTx);

    NewTransDescription = string.Empty;
    NewTransAmount = 0;
    NewTransCategory = string.Empty;

    RecalculateFinances();

    _ = SaveDataAsync();
}

        public async Task AddStockAsync()
{
    if (string.IsNullOrWhiteSpace(NewTicker) || NewQuantity <= 0 || SelectedStockPrice <= 0) 
        return;

    decimal costInUsd = SelectedStockPrice * NewQuantity;

    decimal costInRon = costInUsd * UsdToRonExchangeRate;

    if (TotalBalance < costInRon)
    {
        GraphStatusText = "Fonduri insuficiente în RON pentru a finaliza achiziția!";
        return;
    }

    TotalBalance -= costInRon;

    Transactions.Add(new Transaction
    {
        Description = $"Cumpărat {NewQuantity} x {NewTicker.ToUpper()}",
        Amount = costInRon,
        Category = "Investiții",
        Type = TransactionType.Expense,
        Date = DateTime.Now
    });

    var symbol = NewTicker.ToUpper();
    var existingStock = Stocks.FirstOrDefault(s => s.Ticker.Equals(symbol, StringComparison.OrdinalIgnoreCase));
    
    if (existingStock != null)
    {
        decimal totalCostInUsd = (existingStock.AveragePurchasePrice * existingStock.Quantity) + costInUsd;
        existingStock.Quantity += NewQuantity;
        existingStock.AveragePurchasePrice = totalCostInUsd / existingStock.Quantity;
    }
    else
    {
        Stocks.Add(new StockPosition
        {
            Ticker = symbol,
            Quantity = NewQuantity,
            AveragePurchasePrice = SelectedStockPrice,
            CurrentPrice = SelectedStockPrice
        });
    }

    RecalculateFinances();
    await SaveDataAsync();

    NewTicker = string.Empty;
    NewQuantity = 0;
    SelectedStockPrice = 0;
    GraphStatusText = "Achiziție efectuată! Portofoliul și contul au fost actualizate.";
}

        public async Task SearchAndPreviewStockAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTicker)) return;

            string symbol = NewTicker.ToUpper();
            GraphStatusText = $"Se caută datele pentru {symbol}...";

            decimal price = await _stockApiService.GetLatestPriceAsync(symbol);

            SelectedStockPrice = price;
            GraphStatusText = $"Compania: {symbol} | Preț API: {price:N2} USD";
        }

        public async Task RefreshStockPricesAsync()
        {
            foreach (var stock in Stocks)
            {
                decimal price = await _stockApiService.GetLatestPriceAsync(stock.Ticker);
                stock.CurrentPrice = price;
            }
            RecalculateFinances();
            await SaveDataAsync();
        }

       private void RecalculateFinances()
{
   decimal balance = 0;
    foreach (var t in Transactions)
    {
        if (t.Type == TransactionType.Income)
        {
            balance += t.Amount;
        }
        else if (t.Type == TransactionType.Expense)
        {
            balance -= t.Amount;
        }
    }
    TotalBalance = balance;

    decimal totalStocks = 0;
    foreach (var stock in Stocks)
    {
        totalStocks += stock.TotalValue;
    }
    PortfolioValue = totalStocks;
}
        public async Task LoadDataAsync()
        {
            var data = await _storageService.LoadDataAsync();

            Transactions.Clear();
            foreach (var t in data.Transactions) Transactions.Add(t);

            Stocks.Clear();
            foreach (var s in data.Stocks) Stocks.Add(s);

            _budgetFood = data.BudgetFood;
            _budgetBills = data.BudgetBills;
            _budgetInvestments = data.BudgetInvestments;

            OnPropertyChanged(nameof(BudgetFood));
            OnPropertyChanged(nameof(BudgetBills));
            OnPropertyChanged(nameof(BudgetInvestments));

            RecalculateFinances();
        }

        private async Task SaveDataAsync()
        {
            var data = new AppData
            {
                Transactions = Transactions.ToList(),
                Stocks = Stocks.ToList(),
                BudgetFood = BudgetFood,
                BudgetBills = BudgetBills,
                BudgetInvestments = BudgetInvestments
            };
            await _storageService.SaveDataAsync(data);
        }

        public void OpenBudgetWindow(Window parentWindow)
        {
            var budgetWindow = new BudgetWindow();
            budgetWindow.DataContext = this;
            budgetWindow.ShowDialog(parentWindow);
        }
    }
}
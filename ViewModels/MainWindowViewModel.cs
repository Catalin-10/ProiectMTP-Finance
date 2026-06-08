using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ProiectMTP.Models;
using ProiectMTP.Services;

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
        private string _graphStatusText = "Introduceti un simbol (ex: AAPL) si apasati Afiseaza Trend Grafic";

        private string _newTransDescription = string.Empty;
        private decimal _newTransAmount;
        private string _newTransCategory = string.Empty;
        private int _newTransTypeIndex;
        private StockPosition? _selectedStock;
        private Points _chartPoints = new();

        private decimal _incomeInput;
        private decimal _ruleNeeds;
        private decimal _ruleWants;
        private decimal _ruleSavings;
        private decimal _budgetFood;
        private decimal _budgetBills;
        private decimal _budgetInvestments;

        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<StockPosition> Stocks { get; set; } = new();

        public decimal TotalBalance
        {
            get => _totalBalance;
            set { _totalBalance = value; OnPropertyChanged(); }
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

        public Points ChartPoints
        {
            get => _chartPoints;
            set { _chartPoints = value; OnPropertyChanged(); }
        }

        public StockPosition? SelectedStock
        {
            get => _selectedStock;
            set
            {
                _selectedStock = value;
                OnPropertyChanged();
                if (_selectedStock != null)
                {
                    NewTicker = _selectedStock.Ticker;
                    SelectedStockPrice = _selectedStock.CurrentPrice;
                    GraphStatusText = $"Portofoliu: {_selectedStock.Ticker} | Pret curent: {_selectedStock.CurrentPrice:N2} USD";
                    _ = LoadHistoricalTrendForSelectedAsync(_selectedStock.Ticker, _selectedStock.CurrentPrice);
                }
            }
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
            set { _budgetFood = value; OnPropertyChanged(); _ = SaveDataAsync(); }
        }
        public decimal BudgetBills
        {
            get => _budgetBills;
            set { _budgetBills = value; OnPropertyChanged(); _ = SaveDataAsync(); }
        }
        public decimal BudgetInvestments
        {
            get => _budgetInvestments;
            set { _budgetInvestments = value; OnPropertyChanged(); _ = SaveDataAsync(); }
        }

        public MainWindowViewModel()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadDataAsync();
            _ = StartStockUpdateLoopAsync();
        }

        public async Task LoadDataAsync()
        {
            var data = await _storageService.LoadDataAsync();
            
            BudgetFood = data.BudgetFood;
            BudgetBills = data.BudgetBills;
            BudgetInvestments = data.BudgetInvestments;
            
            Dispatcher.UIThread.Post(() =>
            {
                Transactions.Clear();
                if (data.Transactions != null)
                {
                    foreach (var t in data.Transactions)
                    {
                        Transactions.Add(t);
                    }
                }

                Stocks.Clear();
                if (data.Stocks != null)
                {
                    foreach (var s in data.Stocks)
                    {
                        Stocks.Add(s);
                    }
                }

                RecalculateFinances();
                OnPropertyChanged(nameof(Transactions));
                OnPropertyChanged(nameof(Stocks));
            });
        }

        public async Task SaveDataAsync()
        {
            var data = new AppData
            {
                Transactions = Transactions.ToList(),
                Stocks = Stocks.ToList(),
                BudgetFood = this.BudgetFood,
                BudgetBills = this.BudgetBills,
                BudgetInvestments = this.BudgetInvestments
            };
            await _storageService.SaveDataAsync(data);
        }

        public void RecalculateFinances()
        {
            decimal balance = 0;
            foreach (var t in Transactions)
            {
                if (t.Type == TransactionType.Income)
                {
                    balance += t.Amount;
                }
                else
                {
                    balance -= t.Amount;
                }
            }
            TotalBalance = balance;
            PortfolioValue = Stocks.Sum(s => s.TotalValue);
        }

        public async Task AddTransactionAsync()
        {
            if (NewTransAmount <= 0 || string.IsNullOrWhiteSpace(NewTransCategory)) return;

            var type = NewTransTypeIndex == 0 ? TransactionType.Income : TransactionType.Expense;
            var transaction = new Transaction
            {
                Amount = NewTransAmount,
                Type = type,
                Category = NewTransCategory,
                Description = NewTransDescription,
                Date = DateTime.Now
            };

            Dispatcher.UIThread.Post(() =>
            {
                Transactions.Insert(0, transaction);
                RecalculateFinances();
            });

            await Task.Delay(50);
            await SaveDataAsync();

            NewTransAmount = 0;
            NewTransCategory = string.Empty;
            NewTransDescription = string.Empty;
        }

        public async Task AddStockAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTicker) || NewQuantity <= 0 || SelectedStockPrice <= 0) return;

            string symbol = NewTicker.ToUpper();
            decimal totalCostInUsd = NewQuantity * SelectedStockPrice;
            decimal totalCostInRon = totalCostInUsd * 4.5m;

            var stockExpense = new Transaction
            {
                Amount = totalCostInRon,
                Type = TransactionType.Expense,
                Category = "Investitii",
                Description = $"Cumparat {NewQuantity} actiuni {symbol} @ {SelectedStockPrice:N2} USD",
                Date = DateTime.Now
            };

            Dispatcher.UIThread.Post(() =>
            {
                Transactions.Insert(0, stockExpense);

                var existing = Stocks.FirstOrDefault(s => s.Ticker.Equals(symbol, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    decimal totalQty = existing.Quantity + NewQuantity;
                    decimal totalCost = (existing.Quantity * existing.AveragePurchasePrice) + (NewQuantity * SelectedStockPrice);
                    existing.Quantity = totalQty;
                    existing.AveragePurchasePrice = totalCost / totalQty;
                    existing.CurrentPrice = SelectedStockPrice;
                }
                else
                {
                    var newStock = new StockPosition
                    {
                        Ticker = symbol,
                        Quantity = NewQuantity,
                        AveragePurchasePrice = SelectedStockPrice,
                        CurrentPrice = SelectedStockPrice
                    };
                    Stocks.Add(newStock);
                }

                RecalculateFinances();
            });

            await Task.Delay(50);
            await SaveDataAsync();

            NewTicker = string.Empty;
            NewQuantity = 0;
            SelectedStockPrice = 0;
            GraphStatusText = "Achizitie efectuata! Portofoliul si contul au fost actualizate.";
            ChartPoints = new Points();
        }

        public async Task SearchAndPreviewStockAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTicker)) return;
        
            string symbol = NewTicker.ToUpper();
            GraphStatusText = $"Se cauta datele pentru {symbol}...";

            decimal price = await _stockApiService.GetLatestPriceAsync(symbol);
            var historicalPrices = await _stockApiService.GetHistoricalPricesAsync(symbol);

            SelectedStockPrice = price;
            GraphStatusText = $"Compania: {symbol} | Pret actiune: {price:N2} USD";
            
            GenerateChartPoints(price, historicalPrices);
        }

        private async Task LoadHistoricalTrendForSelectedAsync(string ticker, decimal currentPrice)
        {
            var historicalPrices = await _stockApiService.GetHistoricalPricesAsync(ticker);
            GenerateChartPoints(currentPrice, historicalPrices);
        }

        private void GenerateChartPoints(decimal activePrice, List<decimal> historicalPrices)
        {
            var pts = new Points();
            var doublePrices = new List<double>();

            if (historicalPrices != null && historicalPrices.Count >= 2)
            {
                foreach (var p in historicalPrices)
                {
                    doublePrices.Add((double)p);
                }
            }
            else
            {
                double basePrice = (double)activePrice;
                int seed = 0;
                string symbol = string.IsNullOrWhiteSpace(NewTicker) ? "STOCK" : NewTicker.ToUpper();
                foreach (char c in symbol) seed += c;
                var random = new Random(seed);

                doublePrices.Add(basePrice * (0.93 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice * (0.91 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice * (0.95 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice * (0.89 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice * (0.96 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice * (0.92 + random.NextDouble() * 0.1));
                doublePrices.Add(basePrice);
            }

            double maxVal = doublePrices.Max();
            double minVal = doublePrices.Min();
            double range = maxVal - minVal;
            if (range == 0) range = 1.0;

            Func<double, double> scaleY = (val) => 130 - ((val - minVal) / range * 100);

            int count = doublePrices.Count;
            for (int i = 0; i < count; i++)
            {
                double x = 10 + (i * (480.0 / (count - 1)));
                pts.Add(new Point(x, scaleY(doublePrices[i])));
            }

            ChartPoints = pts;
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

        private async Task StartStockUpdateLoopAsync()
        {
            using var timer = new System.Threading.PeriodicTimer(TimeSpan.FromMinutes(5));
            while (await timer.WaitForNextTickAsync())
            {
                await RefreshStockPricesAsync();
            }
        }

        public void ApplyBudgetRule()
        {
            if (IncomeInput <= 0) return;
            RuleNeeds = IncomeInput * 0.50m;
            RuleWants = IncomeInput * 0.30m;
            RuleSavings = IncomeInput * 0.20m;
            
            BudgetFood = RuleNeeds * 0.4m; 
            BudgetBills = RuleNeeds * 0.6m; 
            BudgetInvestments = RuleSavings;
        }

        public void OpenBudgetWindow(Avalonia.Controls.Window owner)
        {
            var window = new ProiectMTP.Views.BudgetWindow
            {
                DataContext = this
            };
            window.ShowDialog(owner);
        }
    }
}